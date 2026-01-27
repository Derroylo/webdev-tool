using WebDev.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Helper.Php
{
    public class PhpPackagesHelper(IDebugOutputHelper _debugOutputHelper, IConfigHelper _configHelper, ExecCommand _execCommand, PhpConfig _phpConfig) : IPhpPackagesHelper
    {
        public void InstallPackages(string[] newPackages, string phpVersion)
        {
            // Update the pecl channel to the latest protocol
            _execCommand.Exec("sudo pecl channel-update pecl.php.net");

            var updateRes = _execCommand.Exec("sudo apt-get update");
            AnsiConsole.MarkupLine("Updating package manager list...[green1]Done[/]");

            _debugOutputHelper.WriteInfoOutput("Update result: " + updateRes, this);

            string packages = string.Join(" ", newPackages).Replace("php-", "php" + phpVersion + "-");

            var installRes = _execCommand.Exec("sudo apt-get install -y " + packages);
            AnsiConsole.MarkupLine("Installing packages...[green1]Done[/]");
            
            _debugOutputHelper.WriteInfoOutput("Install result: " + installRes, this);

            _execCommand.Exec("apachectl stop");
            _execCommand.Exec("apachectl start");
            AnsiConsole.MarkupLine("Restarting apache...[green1]Success[/]");
            
            SavePackagesInConfig(newPackages);
        }

        private void SavePackagesInConfig(string[] packages)
        {
            foreach (string package in packages) {
                if (!_phpConfig.Packages.Contains(package)) {
                    _phpConfig.Packages.Add(package);
                    
                    _configHelper.ConfigUpdated = true;
                }
            }    
        }
    }
}
using WebDev.Tool.Helper.Internal.Config.Sections;
using WebDev.Tool.Helper.NodeJs;
using WebDev.Tool.Helper.Php;
using Spectre.Console;

namespace WebDev.Tool.Helper
{
    public class RestoreHelper(IPhpHelper _phpHelper, IPhpIniHelper _phpIniHelper, INodeJsVersionHelper _nodeJsVersionHelper, PhpConfig _phpConfig, NodeJsConfig _nodeJsConfig) : IRestoreHelper
    {
        public void RestorePhpVersion()
        {
            AnsiConsole.Write("Checking if php version has been set via config....");

            if (_phpConfig.PhpVersion == string.Empty) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            _phpHelper.SetNewPhpVersion(_phpConfig.PhpVersion);
        }

        public void RestorePhpIni()
        {
            AnsiConsole.Write("Checking if php settings has been set via config....");

            if (_phpConfig.Config.Count == 0 && _phpConfig.ConfigCli.Count == 0 && _phpConfig.ConfigWeb.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            _phpIniHelper.UpdatePhpIniFiles();
        }

        public void RestoreNodeJsVersion()
        {
            AnsiConsole.Write("Checking if NodeJS version has been set via config....");

            if (_nodeJsConfig.NodeJsVersion == string.Empty) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            _nodeJsVersionHelper.SetNewNodeJSVersion(_nodeJsConfig.NodeJsVersion);
        }

        public void RestoreEnvVariables()
        {
            // Check if there has been something set via config file
            AnsiConsole.Write("Checking if Env variables has been set via config....");

            // Not implemented yet, will come with the next major release
            AnsiConsole.MarkupLine("[cyan3]Not found[/]");
        }
    }
}
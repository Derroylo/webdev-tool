using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace WebDev.Tool.Helper.NodeJs
{
    public partial class NodeJsPackageHelper(ExecCommand _execCommand) : INodeJsPackageHelper
    {  
        [GeneratedRegex(@"([a-z0-9\-]+)@([0-9.]+)")]
        private static partial Regex NodeJsPackageMatchRegex();

        public List<string> GetCurrentInstalledNodeJSPackages(string packageListOutput = null)
        {
            packageListOutput ??= _execCommand.Exec("npm list -g --depth=0");

            List<string> packages = packageListOutput.Split("\n").ToList();

            List<string> filteredPackages = new();

            foreach (string package in packages) {
                Match match = NodeJsPackageMatchRegex().Match(package);

                if (match.Success && match.Groups[1].Value != "npm" && match.Groups[1].Value != "corepack") {
                    filteredPackages.Add(match.Groups[1].Value);
                }
            }

            return filteredPackages;
        }

        public void InstallPackages(string[] newPackages, bool debug = false)
        {
            string packages = string.Join(" ", newPackages);

            var installRes = _execCommand.Exec("npm install -g " + packages);
            AnsiConsole.MarkupLine("Installing packages...[green1]Done[/]");
            
            if (debug) {
                AnsiConsole.WriteLine(installRes);
            }
        }
    }
}
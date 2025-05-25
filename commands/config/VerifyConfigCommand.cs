using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Docker;

namespace WebDev.Tool.Commands.Config
{
    class VerifyConfigCommand : Command<VerifyConfigCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-p|--php")]
            [Description("Verify php settings")]
            [DefaultValue(false)]
            public bool VerifyPhp { get; set; }

            [CommandOption("-n|--nodejs")]
            [Description("Verify NodeJS settings")]
            [DefaultValue(false)]
            public bool VerifyNodeJs { get; set; }

            [CommandOption("-s|--services")]
            [Description("Verify services settings")]
            [DefaultValue(false)]
            public bool VerifyServices { get; set; }

            [CommandOption("-S|--shell")]
            [Description("Verify shell script settings")]
            [DefaultValue(false)]
            public bool VerifyShellScripts { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (!ConfigHelper.ConfigFileExists) {
                AnsiConsole.MarkupLine("[red]Config file not found. Make sure the file .devcontainer/devcontainer.json exists.[/]");

                return 0;
            }

            if (!ConfigHelper.IsConfigFileValid) {
                AnsiConsole.MarkupLine("[red]The config file is invalid. Correct the syntax errors and try again.[/]");

                return 0;
            }

            bool showSingleOutput = settings.VerifyPhp || settings.VerifyServices || settings.VerifyShellScripts || settings.VerifyNodeJs;

            if (!showSingleOutput || settings.VerifyPhp) {
                OutputPhpSettings();

                if (!showSingleOutput)
                {
                    AnsiConsole.Write(new Rule() {
                        Style = new Style(Color.White, null, Decoration.Dim) 
                    });
                }
            }
            
            if (!showSingleOutput || settings.VerifyNodeJs) {
                OutputNodeJsSettings();
                
                if (!showSingleOutput)
                {
                    AnsiConsole.Write(new Rule() {
                        Style = new Style(Color.White, null, Decoration.Dim) 
                    });
                }
            }
            
            if (!showSingleOutput || settings.VerifyServices) {
                OutputServiceServices();
                
                if (!showSingleOutput)
                {
                    AnsiConsole.Write(new Rule() {
                        Style = new Style(Color.White, null, Decoration.Dim) 
                    });
                }
            }

            if (!showSingleOutput || settings.VerifyShellScripts) {
                OutputShellScriptSettings();
                
                if (!showSingleOutput)
                {
                    AnsiConsole.Write(new Rule() {
                        Style = new Style(Color.White, null, Decoration.Dim) 
                    });
                }
            }

            return 0;
        }

        private void OutputPhpSettings()
        {
            AnsiConsole.MarkupLine($"[bold yellow]PHP Settings[/]");

            if (PhpConfig.PhpVersion != string.Empty) {
                AnsiConsole.Markup($"[bold]Version[/]".PadRight(30));
                AnsiConsole.Markup($"[green]{PhpConfig.PhpVersion}[/]\n");
            }
            
            if (PhpConfig.Config.Count > 0) {
                AnsiConsole.MarkupLine($"\n[bold yellow]Overrides CLI and Web[/]");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in PhpConfig.Config) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (PhpConfig.ConfigCli.Count > 0) {
                AnsiConsole.MarkupLine($"\n[bold yellow]Overrides CLI[/]");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in PhpConfig.ConfigCli) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (PhpConfig.ConfigWeb.Count > 0) {
                AnsiConsole.MarkupLine($"\n[bold yellow]Overrides Web[/]");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in PhpConfig.ConfigWeb) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (PhpConfig.Packages.Count > 0) {
                AnsiConsole.MarkupLine($"\n[bold yellow]Packages[/]");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");

                foreach(string item in PhpConfig.Packages) {
                    settingsTable.AddRow(item);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }
        }

        private void OutputNodeJsSettings()
        {
            AnsiConsole.MarkupLine($"[bold yellow]NodeJS Settings[/]");

            if (NodeJsConfig.NodeJsVersion != string.Empty) {
                AnsiConsole.Markup($"[bold]Version[/]".PadRight(30));
                AnsiConsole.Markup($"[green]{NodeJsConfig.NodeJsVersion}[/]\n");
            }
        }

        private void OutputServiceServices()
        {
            AnsiConsole.MarkupLine($"[bold yellow]Service Settings[/]");

            var services = DockerComposeHelper.GetServices(DockerComposeHelper.GetFile());

            var servicesTable = new Table();

            servicesTable.AddColumn("[bold yellow]Name[/]");
            servicesTable.AddColumn("[bold yellow]Description[/]");
            servicesTable.AddColumn("[bold yellow]Active per default[/]");

            foreach(KeyValuePair<string, Dictionary<string, string>> item in services) {
                if (item.Key == "devcontainer")
                {
                    continue;
                }
                
                var serviceName = item.Value.ContainsKey("name") ? item.Value["name"] : item.Key;
                var serviceDescription = item.Value.ContainsKey("description") ? item.Value["description"] : "-";

                var isActive = ServicesConfig.ActiveServices.Contains(item.Key);

                servicesTable.AddRow(serviceName, serviceDescription, isActive ? "[green1]Active[/]" : "[red]Inactive[/]");
            }
            
            AnsiConsole.Write(servicesTable);
        }

        private void OutputShellScriptSettings()
        {
            AnsiConsole.MarkupLine($"[bold yellow]Shell scripts[/]");

            if (ShellScriptConfig.AdditionalDirectories.Count > 0) {
                AnsiConsole.WriteLine("Additional directories:");

                // Create a table
                var directoriesTable = new Table();

                // Add columns
                directoriesTable.AddColumn("[bold yellow]Directory[/]");
                directoriesTable.AddColumn("[bold yellow]Exists[/]");
                directoriesTable.AddColumn("[bold yellow]Scripts found[/]");

                var currentDir = Directory.GetCurrentDirectory() + "/";

                foreach(string item in ShellScriptConfig.AdditionalDirectories) {
                    bool dirExists = Directory.Exists(currentDir + item);
                    int scriptsFound = 0;

                    if (dirExists) {
                        scriptsFound = Directory.GetFiles(currentDir + item, "*.sh", SearchOption.AllDirectories).Length;
                    }

                    directoriesTable.AddRow(item, dirExists ? "[green1]Yes[/]" : "[red]No[/]", scriptsFound.ToString());
                }
                
                // Render the table to the console
                AnsiConsole.Write(directoriesTable);
            }
        }
    }   
}

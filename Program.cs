using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Classes;
using WebDev.Tool.Commands.Shell;
using WebDev.Tool.Commands.Apache;
using WebDev.Tool.Commands.Services;
using WebDev.Tool.Commands;
using WebDev.Tool.Commands.Config;
using WebDev.Tool.Commands.ModeJS;
using WebDev.Tool.Commands.NodeJS;
using WebDev.Tool.Commands.Php;

namespace WebDev.Tool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app     = new CommandApp();
            var version = UpdateHelper.CurrentVersion;

            // Output the program name, version and info if the config file could not be read
            OutputProgramHeader(version, args.Contains("--debug"));

            // Load additional commands that are defined within shell scripts
            var additionalCommands = new Dictionary<string, CustomBranch>();

            try {
                additionalCommands = CustomCommandsLoader.Load();
            } catch (Exception e) {
                AnsiConsole.MarkupLine("[red]Unable to load the custom commands[/] - [orange3]Append '--debug' to show more details[/]");

                // With he debug arg, output the exception and exit
                if (args.Contains("--debug")) {
                    AnsiConsole.WriteException(e);

                    return;
                }
            }

            app.Configure(config =>
            {
                config.SetApplicationName("webdev");
                config.SetApplicationVersion(version);

                // Add Branches and their commands
                config.AddBranch("apache", branch => AddApacheCommandBranch(branch, additionalCommands));
                config.AddBranch("services", branch => AddServicesCommandBranch(branch, additionalCommands));
                config.AddBranch("config", branch => AddConfigCommandBranch(branch, additionalCommands));
                config.AddBranch("mysql", branch => AddMysqlCommandBranch(branch, additionalCommands));
                config.AddBranch("nodejs", branch => AddNodeJsCommandBranch(branch, additionalCommands));
                config.AddBranch("php", branch => AddPhpCommandBranch(branch, additionalCommands));
                config.AddBranch("restore", branch => AddRestoreCommandBranch(branch, additionalCommands));
                config.AddCommand<NotYetImplementedCommand>("update").WithDescription("Update this tool to the latest version");

                List<string> reservedBranches = new() { "default", "config", "php", "nodejs", "apache", "mysql", "services", "restore" };

                // Add branches that haven´t been added yet via custom commands
                foreach (KeyValuePair<string, CustomBranch> entry in additionalCommands.Where(x => !reservedBranches.Contains(x.Key))) {
                    config.AddBranch(entry.Value.Name, branch => 
                    {
                        branch.SetDescription(entry.Value.Description);

                        foreach (CustomCommand cmd in entry.Value.Commands) {
                            branch.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    });
                }

                // Add all commands without branches
                if (additionalCommands.TryGetValue("default", out CustomBranch branch)) {
                    foreach (CustomCommand cmd in branch.Commands) {
                        config.AddCommand<ShellFileCommand>(cmd.Command)
                            .WithData(cmd)
                            .WithDescription(cmd.Description);
                    }
                }
            });

            app.Run(args);
        }

        private static void OutputProgramHeader(string programVersion, bool showException = false)
        {
            AnsiConsole.Write(new FigletText("WebDev"));
            AnsiConsole.Markup("[deepskyblue3]WebDev Tool[/] - Version [green]" + programVersion + "[/]");

            // Try to load the config file
            ConfigHelper.ReadConfigFile(true);
        }

        private static void AddConfigCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Creates or verify the configuration file");
                    
            branch.AddCommand<VerifyConfigCommand>("verify")
                .WithAlias("v")
                .WithDescription(@"Tries to read the config file and shows it`s content");                    

            if (additionalCommands.TryGetValue("config", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddPhpCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Different commands to change active php version, ini settings etc.");
                    
            branch.AddCommand<PhpVersionCommand>("version")
                .WithAlias("v")
                .WithDescription("Shows or sets the currently used PHP Version");
            branch.AddCommand<PhpIniCommand>("ini")
                .WithAlias("i")
                .WithDescription("Change the value of a PHP setting.");
            branch.AddCommand<PhpRestoreCommand>("restore")
                .WithAlias("r")
                .WithDescription("Restores a previously set PHP version and their settings");
            branch.AddCommand<PhpDebugCommand>("xdebug")
                .WithAlias("d")
                .WithDescription("Shows or sets the current xdebug mode");
            branch.AddCommand<PhpPackageCommand>("packages")
                .WithAlias("p")
                .WithDescription("Shows installed php packages or install new ones");

            if (additionalCommands.TryGetValue("php", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddNodeJsCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Different commands to change active nodejs version, etc.");
                    
            branch.AddCommand<NodeJSVersionCommand>("version")
                .WithAlias("v")
                .WithDescription("Shows or sets the currently used NodeJS Version");
            branch.AddCommand<NodeJSRestoreCommand>("restore")
                .WithAlias("r")
                .WithDescription("Restores a previously set NodeJS version");

            if (additionalCommands.TryGetValue("nodejs", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddApacheCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Some simple commands to start/stop/restart the apache webserver");

            branch.AddCommand<ApacheStatusCommand>("status")
                .WithDescription("Shows the current status of apache");
            branch.AddCommand<ApacheStartCommand>("start")
                .WithDescription("Starts apache");
            branch.AddCommand<ApacheStopCommand>("stop")
                .WithDescription("Stops apache");
            branch.AddCommand<ApacheRestartCommand>("restart")
                .WithDescription("Restarts apache");

            if (additionalCommands.TryGetValue("apache", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddMysqlCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Import or Export Databases or create snapshots [red]Not implemented yet[/]");

            branch.AddCommand<NotYetImplementedCommand>("export")
                .WithDescription("Exports the content of the database to a file [red]Not implemented yet[/]");
            branch.AddCommand<NotYetImplementedCommand>("import")
                .WithDescription("Imports database content from a file [red]Not implemented yet[/]");
            branch.AddCommand<NotYetImplementedCommand>("snapshot")
                .WithDescription("Create/Restore a snapshot of the database. Useful to make a backup before you test something and want to restore the old state fast if anything goes wrong [red]Not implemented yet[/]");

            if (additionalCommands.TryGetValue("mysql", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddServicesCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("List, status of services and define which should be started");

            branch.AddCommand<ListServicesCommand>("list")
                .WithDescription("List available the services");
            branch.AddCommand<StartServicesCommand>("start")
                .WithDescription("Start the services that are marked as active");
            branch.AddCommand<StopServicesCommand>("stop")
                .WithDescription("Stops running services");
            branch.AddCommand<SelectServicesCommand>("select")
                .WithDescription("Select which services should be active");

            if (additionalCommands.TryGetValue("services", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddRestoreCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Restore settings separate for nodejs or php, or for all at once ");
                    
            branch.AddCommand<NotYetImplementedCommand>("all")
                .WithAlias("a")
                .WithDescription("Restore all settings");
            branch.AddCommand<NotYetImplementedCommand>("php")
                .WithAlias("p")
                .WithDescription("Restore settings for php");
            branch.AddCommand<NotYetImplementedCommand>("nodejs")
                .WithAlias("n")
                .WithDescription("Restore settings for NodeJS");
            branch.AddCommand<NotYetImplementedCommand>("env")
                .WithAlias("e")
                .WithDescription("Restore environment variables");

            if (additionalCommands.TryGetValue("restore", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }
    }
}

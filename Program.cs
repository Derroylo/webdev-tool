using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Classes;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Commands.Shell;
using WebDev.Tool.Commands.Apache;
using WebDev.Tool.Commands.Services;
using WebDev.Tool.Commands;
using WebDev.Tool.Commands.Config;
using WebDev.Tool.Commands.Info;
using WebDev.Tool.Commands.ModeJS;
using WebDev.Tool.Commands.NodeJS;
using WebDev.Tool.Commands.Php;
using WebDev.Tool.Commands.Project;
using WebDev.Tool.Commands.Restore;
using WebDev.Tool.Commands.Secrets;
using WebDev.Tool.Commands.tasks;
using WebDev.Tool.Commands.Terminal;
using WebDev.Tool.Commands.Workspaces;
using WebDev.Tool.Helper;
using WebDev.Tool.Helper.Internal.Config.Sections;
using WebDev.Tool.Commands.Mysql;
using WebDev.Tool.Commands.Admin;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using NetCore.AutoRegisterDi;
using WebDev.Tool.Helper.Secrets;

namespace WebDev.Tool
{
    internal class Program
    {
        public static List<string> ProgramArgs { get; private set; } = [];

        public static string ApplicationName { get; private set; } = "webdev";

        static void Main(string[] args)
        {
            // Auto register all classes that end with Helper for DI
            var services = new ServiceCollection();

            services.RegisterAssemblyPublicNonGenericClasses()
                .Where(c => c.Name.EndsWith("Helper"))
                .AsPublicImplementedInterfaces();

            // Register config section classes
            services.AddSingleton<GeneralConfig>();
            services.AddSingleton<TasksConfig>();
            services.AddSingleton<TestsConfig>();
            services.AddSingleton<PhpConfig>();
            services.AddSingleton<NodeJsConfig>();
            services.AddSingleton<SecretsConfig>();
            services.AddSingleton<ServicesConfig>();
            services.AddSingleton<ShellScriptConfig>();
            services.AddSingleton<WorkspacesConfig>();

            services.AddSingleton<CustomCommandsLoader>();
            services.AddSingleton<ExecCommand>();
            services.AddSingleton<ISecretsLoader, SecretsLoader>();

            var serviceProvider = services.BuildServiceProvider();
            var app     = new CommandApp(new TypeRegistrar(services));
            
            // Get helpers from DI for use in Program.cs
            var updateHelper = serviceProvider.GetRequiredService<IUpdateHelper>();
            var pathHelper = serviceProvider.GetRequiredService<IPathHelper>();
            var environmentHelper = serviceProvider.GetRequiredService<IEnvironmentHelper>();
            var appSettingsHelper = serviceProvider.GetRequiredService<IAppSettingsHelper>();
            var customCommandsLoader = serviceProvider.GetRequiredService<CustomCommandsLoader>();
            var debugOutputHelper = serviceProvider.GetRequiredService<IDebugOutputHelper>();
            var tasksConfig = serviceProvider.GetRequiredService<TasksConfig>();
            var testsConfig = serviceProvider.GetRequiredService<TestsConfig>();
            var configHelper = serviceProvider.GetRequiredService<IConfigHelper>();
            
            var version = updateHelper.CurrentVersion;

            // Save the program args for later use
            ProgramArgs = [.. args];

            if (args.Contains("--not-main") || args.Contains("-n"))
            {
                PathHelper.IsMainWorkspace = false;
            }

            if (args.Contains("--no-header"))
            {
                environmentHelper.DisableProgramHeader();
            }

            var debugOutputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".debug_enabled");
            bool debugModeEnabled = File.Exists(debugOutputFile) || args.Contains("--debug");

            // Set the application name
            // This is usually webdev, but when it is put into a prerelease folder, it is webdev-prerelease
            if (Environment.GetCommandLineArgs()[0].Contains("prerelease"))
            {
                ApplicationName = "webdev-prerelease";
            }

            // Output the program name, version and info if the config file could not be read
            OutputProgramHeader(version, environmentHelper, appSettingsHelper, updateHelper, configHelper, debugModeEnabled);
          
            // Load additional commands that are defined within shell scripts
            var additionalCommands = new Dictionary<string, CustomBranch>();

            try {
                additionalCommands = customCommandsLoader.Load();
            } catch (Exception e) {
                AnsiConsole.MarkupLine("[red]Unable to load the custom commands[/] - [orange3]Append '--debug' to show more details[/]");

                // With he debug arg, output the exception and exit
                if (debugModeEnabled) {
                    AnsiConsole.WriteException(e);

                    return;
                }
            }

            app.Configure(config =>
            {
                config.SetApplicationName("webdev");
                config.SetApplicationVersion(version);
                
                // Set up the interceptor to configure logging before command execution
                config.SetInterceptor(new CommandInterceptor(debugOutputHelper, environmentHelper));

                // Register custom help provider for a better help output
                config.SetHelpProvider(new CustomHelpProvider(environmentHelper, config.Settings));

                // Add Branches and their commands
                config.AddBranch("admin", branch => AddAdminCommandBranch(branch, additionalCommands))
                    .RunOnlyOnHost()
                    .ShowAllCommandsDirectly()
                    .ShowInCommonCommands();

                config.AddBranch("apache", branch => AddApacheCommandBranch(branch, additionalCommands))
                    .RunOnlyInDevcontainer();
                
                config.AddBranch("services", branch => AddServicesCommandBranch(branch, additionalCommands))
                    .RunOnlyOnHost()
                    .ShowAllCommandsDirectly()
                    .ShowInCommonCommands();

                config.AddBranch("config", branch => AddConfigCommandBranch(branch, additionalCommands));
                
                config.AddBranch("project", branch => AddProjectCommandBranch(branch, additionalCommands))
                    .RunOnlyOnHost()
                    .ShowAllCommandsDirectly()
                    .ShowInCommonCommands();

                config.AddBranch("secrets", branch => AddSecretsCommandBranch(branch, additionalCommands))
                    .RunOnlyOnHost();
                
                config.AddBranch("mysql", branch => AddMysqlCommandBranch(branch, additionalCommands))
                    .RunOnlyOnHost();

                config.AddBranch("nodejs", branch => AddNodeJsCommandBranch(branch, additionalCommands))
                    .RunOnlyInDevcontainer()
                    .ShowInCommonCommands()
                    .ShowAllCommandsDirectly();

                config.AddBranch("php", branch => AddPhpCommandBranch(branch, additionalCommands))
                    .RunOnlyInDevcontainer()
                    .ShowInCommonCommands()
                    .ShowAllCommandsDirectly();

                config.AddBranch("restore", branch => AddRestoreCommandBranch(branch, additionalCommands))
                    .RunOnlyInDevcontainer();
                
                if (tasksConfig.Tasks.Count > 0)
                {
                    config.AddBranch("task", branch => AddTaskCommandBranch(branch, tasksConfig.Tasks));
                }
                
                config.AddBranch("tasks", branch => AddTasksCommandBranch(branch, additionalCommands));

                config.AddCommand<OpenDevcontainerTerminalCommand>("terminal")
                    .RunOnlyOnHost()
                    .ShowInCommonCommands()
                    .WithDescription("Open a terminal to the running devcontainer");

                config.AddCommand<SelfUpdateCommand>("update")
                    .WithDescription("Update this tool to the latest version");

                config.AddCommand<ShowProjectInfoCommand>("info")
                    .ShowInCommonCommands()
                    .WithDescription("Shows information about the current project");
                
                // Prepare and run workspaces
                config.AddCommand<OnInitWorkspacesCommand>("workspaces-on-init").IsHidden();
                config.AddCommand<PostStartWorkspacesCommand>("workspaces-post-start").IsHidden();

                config.AddCommand<DebugCommand>("debug")
                    .WithDescription("Debug command")
                    .IsHidden();

                // Add Tools branch
                config.AddBranch("tools", branch => AddToolsCommandBranch(branch, additionalCommands));

                List<string> reservedBranches = new() { "default", "config", "php", "nodejs", "apache", "mysql", "services", "restore", "secrets", "tasks", "task", "tests", "admin", "project" };

                // Add Tests branch
                if (testsConfig.Tests.Count > 0)
                {
                    config.AddBranch("tests", branch => 
                    {
                        branch.SetDescription("Run tests");

                        foreach (KeyValuePair<string, TestEntryConfiguration> entry in testsConfig.Tests) {
                            branch.AddCommand<TestsCommand>(entry.Key)
                                .WithData(entry.Value)
                                .WithDescription(entry.Value.Name)
                                .ShowInCommonCommands();
                        }
                    })
                    .ShowInCommonCommands();
                }

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
            
            if (configHelper.ConfigFileExists && configHelper.IsConfigFileValid && configHelper.ConfigUpdated) {
                try {
                    // Save config file
                    configHelper.SaveConfigFile();
                } catch (Exception e) {
                    AnsiConsole.WriteLine("[red]Saving the config file failed[/] - [orange3]Append '--debug' to show more details[/]");

                    if (debugModeEnabled) {
                        AnsiConsole.WriteException(e);
                    }
                }
            }
        }
        
        private static void OutputProgramHeader(string programVersion, IEnvironmentHelper environmentHelper, IAppSettingsHelper appSettingsHelper, IUpdateHelper updateHelper, IConfigHelper configHelper, bool showException = false)
        {           
            if (!environmentHelper.IsProgramHeaderDisabled())
            {
                AnsiConsole.Write(new FigletText("WebDev"));
                AnsiConsole.Markup("[deepskyblue3]WebDev[/] - Version [green]" + programVersion + "[/]");

                AnsiConsole.Write(environmentHelper.IsRunningInDevContainer() ? " - DevContainer Mode" : " - Local Mode");
            }

            // Try to load the config file
            configHelper.ReadConfigFile();

            // Try to load the app settings
            appSettingsHelper.LoadAppSettings(showException);
            
            if (environmentHelper.IsProgramHeaderDisabled())
            {
                return;
            }
            
            try {
                // Check for updates
                var latestVersion = updateHelper.GetLatestVersion().Result;
                var isUpdateAvailable = updateHelper.IsUpdateAvailable();

                if (isUpdateAvailable) {
                    AnsiConsole.MarkupLine(" - [orange3]Latest Version is " + latestVersion + ". Use 'webdev update' to update.[/]");
                } else {
                    AnsiConsole.MarkupLine("");
                }
            } catch (Exception e) {
                AnsiConsole.MarkupLine(" - [red]Check for update failed[/]");

                if (showException) {
                    AnsiConsole.WriteException(e);
                }
            }
            
            if (!configHelper.ConfigFileExists) {
                AnsiConsole.MarkupLine("[orange3]No config file found - falling back to default settings[/]");
            } else if(!configHelper.IsConfigFileValid) {
                AnsiConsole.MarkupLine("[red]Config file is invalid - falling back to default settings[/] - [orange3]Append '--debug' to show more details[/]");

                if (showException) {
                    try {
                        configHelper.ReadConfigFile(true);
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);
                    }
                }
            }

            AnsiConsole.MarkupLine("WebDev CLI is a tool to manage your local web development environment. Visit [link]https://derroylo.github.io/[/] for more information.");
        }

        private static void AddAdminCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Commands for the admin interface");

            branch.AddCommand<AdminStartCommand>("start")
                .WithDescription("Starts the admin interface");
            branch.AddCommand<AdminStopCommand>("stop")
                .WithDescription("Stops the admin interface");

            if (additionalCommands.TryGetValue("admin", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddToolsCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Various commands, like updating the traefik config file");

            branch.AddCommand<UpdateTraefikConfigCommand>("traefik-update")
                .WithDescription("Updates the traefik config file")
                .WithAlias("t");

            branch.AddCommand<NotYetImplementedCommand>("domains")
                .WithDescription("List all domains configured in the traefik config file")
                .WithAlias("d");  

            branch.AddCommand<NotYetImplementedCommand>("update-certs")
                .WithDescription("Updates the certificates")
                .WithAlias("u");  

            branch.AddCommand<NotYetImplementedCommand>("install-certs")
                .WithDescription("Installs the root CA certificates")
                .WithAlias("i");
            
            branch.AddCommand<NotYetImplementedCommand>("autocompletion")
                .WithDescription("Generates the autocompletion script")
                .WithAlias("a");

            if (additionalCommands.TryGetValue("tools", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
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

        private static void AddTaskCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, TaskEntryConfiguration> Tasks)
        {
            branch.SetDescription("Run a specific task defined in the config file");

            foreach (KeyValuePair<string, TaskEntryConfiguration> entry in Tasks) {
                branch.AddCommand<RunTaskCommand>(entry.Key)
                    .WithData(entry.Key)
                    .WithDescription(entry.Value.Name);
            }
        }
        
        private static void AddTasksCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Run a specific section of all tasks defined in the config file");         

            branch.AddCommand<RunTasksCommand>("create")
                .RunOnlyInDevcontainer("tasks")
                .WithData("create")
                .WithAlias("c")
                .WithDescription(@"Runs create sections of all tasks");
            
            branch.AddCommand<RunTasksCommand>("start")
                .RunOnlyInDevcontainer("tasks")
                .WithData("start")
                .WithAlias("s")
                .WithDescription(@"Runs start sections of all tasks");
            
            branch.AddCommand<RunTasksCommand>("init")
                .RunOnlyOnHost("tasks")
                .WithData("init")
                .WithAlias("i")
                .WithDescription(@"Runs init sections of all tasks");
            
            if (additionalCommands.TryGetValue("tasks", out CustomBranch customBranch)) {
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
            branch.SetDescription("Various commands to interact with the mysql database");

            branch.AddCommand<MysqlPullCommand>("update")
                .WithDescription("Pulls the latest version of the custom database image");

            /* branch.AddCommand<NotYetImplementedCommand>("export")
                .WithDescription("Exports the content of the database to a file [red]Not implemented yet[/]");
            branch.AddCommand<NotYetImplementedCommand>("import")
                .WithDescription("Imports database content from a file [red]Not implemented yet[/]");
            branch.AddCommand<NotYetImplementedCommand>("snapshot")
                .WithDescription("Create/Restore a snapshot of the database. Useful to make a backup before you test something and want to restore the old state fast if anything goes wrong [red]Not implemented yet[/]"); */

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
                .WithDescription("Start the services that are marked as active")
                .ShowInCommonCommands();
            branch.AddCommand<StopServicesCommand>("stop")
                .WithDescription("Stops running services")
                .ShowInCommonCommands();
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
                    
            branch.AddCommand<RestoreAllCommand>("all")
                .WithAlias("a")
                .WithDescription("Restore all settings");
            branch.AddCommand<RestorePhpCommand>("php")
                .WithAlias("p")
                .WithDescription("Restore settings for php");
            branch.AddCommand<RestoreNodeJsCommand>("nodejs")
                .WithAlias("n")
                .WithDescription("Restore settings for NodeJS");
            branch.AddCommand<RestoreEnvCommand>("env")
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
        
        private static void AddSecretsCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Loads external secrets");
                    
            branch.AddCommand<LoadSecretsCommand>("load")
                .WithAlias("l")
                .WithDescription(@"Reads the config file and loads the defined secrets");                    

            branch.AddCommand<ExportSecretsCommand>("export")
                .WithAlias("e")
                .WithDescription(@"Exports the secrets to the console");

            if (additionalCommands.TryGetValue("secrets", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }
        
        private static void AddProjectCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Project commands");
                    
            /* branch.AddCommand<InitProjectCommand>("init")
                .WithAlias("i")
                .WithDescription(@"Creates a new project from a given Repo and sets up the necessary devcontainer");                     */

            branch.AddCommand<StartProjectCommand>("start")
                .WithAlias("s")
                .WithDescription(@"Checks if the current folder contains a devcontainer spec and starts it");

            branch.AddCommand<StopProjectCommand>("stop")
                .WithAlias("st")
                .WithDescription(@"Checks if the current folder contains a devcontainer spec and stops it");                
            
            if (additionalCommands.TryGetValue("project", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }
    }
}

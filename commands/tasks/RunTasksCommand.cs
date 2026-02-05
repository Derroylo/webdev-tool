using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Commands.tasks;

internal class RunTasksCommand(
    IDebugOutputHelper _debugOutputHelper, 
    IEnvironmentHelper _environmentHelper, 
    IPathHelper _pathHelper, 
    TasksConfig _tasksConfig, 
    GeneralConfig _generalConfig, 
    WorkspacesConfig _workspacesConfig,
    ExecCommand _execCommand
    ) : Command<RunTasksCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
        [CommandOption("-n|--not-main")]
        [Description("Execute commands only with the flag IsMain set to false")]
        [DefaultValue(false)]
        public bool IsNotMain { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing RunTasksCommand", this);
        
        string sectionName = context.Data?.ToString();
        
        var workspacePath = Environment.GetEnvironmentVariable("WEBDEV_WORKSPACE_FOLDER");
        
        if (string.IsNullOrEmpty(sectionName))
        {
            AnsiConsole.MarkupLine("[red]No section specified.[/]");

            return 0;
        }
        
        foreach (KeyValuePair<string, TaskEntryConfiguration> entry in _tasksConfig.Tasks)
        {
            if (settings.IsNotMain && entry.Value.OnlyMain)
            {
                continue;
            }

            var shownRunningCommands = false;
            
            if (sectionName == "init" && entry.Value.Init.Count > 0)
            {
                AnsiConsole.MarkupLine("[green]Running commands for task: " + entry.Value.Name + "[/]");
                shownRunningCommands = true;
                
                AnsiConsole.MarkupLine("[green]Running init commands[/]");
                
                foreach (string cmd in entry.Value.Init)
                {
                    _debugOutputHelper.WriteInfoOutput("Running init command: " + cmd, this);
                    _execCommand.ExecWithDirectOutput(cmd, false, true);
                }
            }
            
            if (sectionName == "create" && entry.Value.Create.Count > 0)
            {
                if (!shownRunningCommands)
                {
                    AnsiConsole.MarkupLine("[green]Running commands for task: " + entry.Value.Name + "[/]");
                    shownRunningCommands = true;
                }
                
                if (!File.Exists(_pathHelper.GetWorkspacePath(_environmentHelper.IsRunningInDevContainer()) + "/.devcontainer/.createDoneLock"))
                {
                    AnsiConsole.MarkupLine("[green]Running create commands[/]");
                
                    foreach (string cmd in entry.Value.Create)
                    {
                        _debugOutputHelper.WriteInfoOutput("Running create command: " + cmd, this);
                        
                        _execCommand.ExecWithDirectOutput(cmd, false, true);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Create commands already executed. Skipping...[/]");
                }
                
            }
                      
            if (sectionName == "start" && entry.Value.Start.Count > 0)
            {
                if (!shownRunningCommands)
                {
                    AnsiConsole.MarkupLine("[green]Running commands for task: " + entry.Value.Name + "[/]");
                }
                
                AnsiConsole.MarkupLine("[green]Running start commands[/]");

                foreach (string cmd in entry.Value.Start)
                {
                    _debugOutputHelper.WriteInfoOutput("Running start command: " + cmd, this);
                    
                    _execCommand.ExecWithDirectOutput(cmd, false, true);
                }
            }
        }

        if (sectionName == "create")
        {
            File.Create(_pathHelper.GetWorkspacePath(_environmentHelper.IsRunningInDevContainer()) + "/.devcontainer/.createDoneLock");
        }

        if (_workspacesConfig.Workspaces.Count <= 1) return 1;

        if (settings.IsNotMain) return 1;
        
        var commands = new List<string>();

        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in _workspacesConfig.Workspaces)
        {
            if (workspace.Key == "main") continue;

            var workspaceCommands = new List<string>
            {
                "cd " + Path.Combine("./", _generalConfig.WorkspaceFolder, workspace.Value.Folder)
            };

            if (sectionName == "init")
            {
                // Make sure we process the secrets for each workspace (the argument --not-main is needed to avoid recursion in the webdev.sh script)
                workspaceCommands.Add(Program.ApplicationName + " secrets load --no-header --not-main");
            }
            
            // Run the tasks for the workspace
            workspaceCommands.Add(Program.ApplicationName + " tasks " + sectionName + " --not-main --no-header");

            commands.Add(string.Join(" && ", workspaceCommands));
        }
            
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        File.WriteAllLines(applicationDir + ".workspaces_tasks", commands);

        return 1;
    }
}
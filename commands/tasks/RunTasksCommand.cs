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

internal class RunTasksCommand: Command<RunTasksCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-d|--debug")]
        [Description("Outputs debug information")]
        [DefaultValue(false)]
        public bool Debug { get; set; }
        
        [CommandOption("-n|--not-main")]
        [Description("Execute commands only with the flag IsMain set to false")]
        [DefaultValue(false)]
        public bool IsNotMain { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        string sectionName = context.Data?.ToString();
        
        var workspacePath = Environment.GetEnvironmentVariable("WEBDEV_WORKSPACE_FOLDER");
        
        if (string.IsNullOrEmpty(sectionName))
        {
            AnsiConsole.MarkupLine("[red]No section specified.[/]");

            return 0;
        }
        
        foreach (KeyValuePair<string, TaskEntryConfiguration> entry in TasksConfig.Tasks)
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
                    if (settings.Debug)
                    {
                        AnsiConsole.MarkupLine("[green]Running command:[/] " + cmd);
                    }
                    
                    ExecCommand.ExecWithDirectOutput(cmd, false, true);
                }
            }
            
            if (sectionName == "create" && entry.Value.Create.Count > 0)
            {
                if (!shownRunningCommands)
                {
                    AnsiConsole.MarkupLine("[green]Running commands for task: " + entry.Value.Name + "[/]");
                    shownRunningCommands = true;
                }
                
                if (!File.Exists(PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer()) + "/.devcontainer/.createDoneLock"))
                {
                    AnsiConsole.MarkupLine("[green]Running create commands[/]");
                
                    foreach (string cmd in entry.Value.Create)
                    {
                        if (settings.Debug)
                        {
                            AnsiConsole.MarkupLine("[green]Running command:[/] " + cmd);
                        }
                        
                        ExecCommand.ExecWithDirectOutput(cmd, false, true);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Create commands already executed. Skipping...[/]");
                }
                
            }
            
            if (sectionName == "prebuild" && entry.Value.Prebuild.Count > 0)
            {
                if (!shownRunningCommands)
                {
                    AnsiConsole.MarkupLine("[green]Running commands for task: " + entry.Value.Name + "[/]");
                    shownRunningCommands = true;
                }
                
                foreach (string cmd in entry.Value.Prebuild)
                {
                    if (settings.Debug)
                    {
                        AnsiConsole.MarkupLine("[green]Running command:[/] " + cmd);
                    }
                    
                    ExecCommand.ExecWithDirectOutput(cmd, false, true);
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
                    if (settings.Debug)
                    {
                        AnsiConsole.MarkupLine("[green]Running command:[/] " + cmd);
                    }
                    
                    ExecCommand.ExecWithDirectOutput(cmd, false, true);
                }
            }
        }

        if (sectionName == "create")
        {
            File.Create(PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer()) + "/.devcontainer/.createDoneLock");
        }

        if (WorkspacesConfig.Workspaces.Count <= 0) return 1;

        if (settings.IsNotMain) return 1;
        
        var commands = new List<string>();
            
        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
        {
            commands.Add("cd " + Path.Combine("./", GeneralConfig.WorkspaceFolder, workspace.Value.Folder) + " && WEBDEV_DISABLE_HEADER=1 webdev tasks " + sectionName + " --not-main");
        }
            
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            
        File.WriteAllLines(applicationDir + ".workspaces_tasks", commands);

        return 1;
    }
}
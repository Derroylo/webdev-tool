using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper;
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
            AnsiConsole.MarkupLine("[green]Running commands for task: " + entry.Value.Name + "[/]");
            
            if (sectionName == "init" && entry.Value.Init.Count > 0)
            {
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
                if (!File.Exists(workspacePath + "/.devcontainer/.createDoneLock"))
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
                AnsiConsole.WriteLine("[green]Running start commands[/]");

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
            File.Create(workspacePath + "/.devcontainer/.createDoneLock");
        }
        
        return 1;
    }
}
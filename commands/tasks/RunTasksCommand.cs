using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Commands.tasks;

internal class RunTasksCommand: Command
{
    public override int Execute(CommandContext context)
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
            AnsiConsole.WriteLine("[green]Running commands for task: " + entry.Value.Name + "[/]");
            
            if (sectionName == "init" && entry.Value.Init.Count > 0)
            {
                AnsiConsole.WriteLine("[green]Running init commands[/]");
                
                foreach (string cmd in entry.Value.Init)
                {
                    ExecCommand.ExecWithDirectOutput(cmd, false, true);
                }
            }
            
            if (sectionName == "create" && entry.Value.Create.Count > 0)
            {
                if (!File.Exists(workspacePath + "/.devcontainer/.createDoneLock"))
                {
                    AnsiConsole.WriteLine("[green]Running create commands[/]");
                
                    foreach (string cmd in entry.Value.Create)
                    {
                        ExecCommand.ExecWithDirectOutput(cmd, false, true);
                    }

                    File.Create(workspacePath + "/.devcontainer/.createDoneLock");
                }
                else
                {
                    AnsiConsole.WriteLine("[red]Create commands already executed. Skipping...[/]");
                }
                
            }
            
            if (sectionName == "prebuild" && entry.Value.Prebuild.Count > 0)
            {
                foreach (string cmd in entry.Value.Prebuild)
                {
                    ExecCommand.ExecWithDirectOutput(cmd, false, true);
                }
            }
            
            if (sectionName == "start" && entry.Value.Start.Count > 0)
            {
                AnsiConsole.WriteLine("[green]Running start commands[/]");

                foreach (string cmd in entry.Value.Start)
                {
                    ExecCommand.ExecWithDirectOutput(cmd, false, true);
                }
            }
        }
        
        return 1;
    }
}
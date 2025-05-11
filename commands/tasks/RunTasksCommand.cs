using System.Collections.Generic;
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
        
        if (string.IsNullOrEmpty(sectionName))
        {
            AnsiConsole.MarkupLine("[red]No section specified.[/]");

            return 0;
        }

        foreach (KeyValuePair<string, TaskEntryConfiguration> entry in TasksConfig.Tasks)
        {
            if (sectionName == "create" && entry.Value.Create.Count > 0)
            {
                foreach (string cmd in entry.Value.Create)
                {
                    ExecCommand.ExecWithDirectOutput(cmd, true, true);
                }
            }
            
            if (sectionName == "prebuild" && entry.Value.Prebuild.Count > 0)
            {
                foreach (string cmd in entry.Value.Prebuild)
                {
                    ExecCommand.ExecWithDirectOutput(cmd, true, true);
                }
            }
            
            if (sectionName == "start" && entry.Value.Start.Count > 0)
            {
                foreach (string cmd in entry.Value.Start)
                {
                    ExecCommand.ExecWithDirectOutput(cmd, true, true);
                }
            }
        }
        
        return 1;
    }
}
using System.Collections.Generic;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Commands.tasks;

internal class RunTaskCommand: Command<RunTaskCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-c|--create")]
        [Description("Run commands defined under \"create\"")]
        [DefaultValue(false)]
        public bool RunCreateCommands { get; set; }
        
        [CommandOption("-s|--start")]
        [Description("Run commands defined under \"start\"")]
        [DefaultValue(true)]
        public bool RunStartCommands { get; set; }
        
        [CommandOption("-p|--prebuild")]
        [Description("Run commands defined under \"prebuild\"")]
        [DefaultValue(false)]
        public bool RunPreBuildCommands { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        string taskName = context.Data?.ToString();
        
        if (string.IsNullOrEmpty(taskName))
        {
            AnsiConsole.MarkupLine("[red]No task specified.[/]");

            return 0;
        }
        
        if (!TasksConfig.Tasks.TryGetValue(taskName, out TaskEntryConfiguration task))
        {
            AnsiConsole.MarkupLine("[red]Unable to find the task \"" + taskName + "\" in the config file.[/]");

            return 0;
        }

        if (task.Mode != TaskMode.All && ((EnvironmentHelper.IsRunningInDevContainer() && task.Mode != TaskMode.DevContainer) || (!EnvironmentHelper.IsRunningInDevContainer() && task.Mode != TaskMode.Local)))
        {
            AnsiConsole.MarkupLine("[red]This task is not available in the current environment.[/]");

            return 0;
        }

        AnsiConsole.WriteLine("-------------------");
        AnsiConsole.MarkupLine("[deepskyblue3]Executing commands for task[/] " + task.Name);
        
        if (settings.RunCreateCommands && task.Create.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Create[/]");
            
            foreach (string cmd in task.Create)
            {
                ExecCommand.ExecWithDirectOutput(cmd, true, true);
            }
        }
        
        if (settings.RunPreBuildCommands && task.Prebuild.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]PreBuild[/]");
            
            foreach (string cmd in task.Prebuild)
            {
                ExecCommand.ExecWithDirectOutput(cmd, true, true);
            }
        }

        if (settings.RunStartCommands && task.Start.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Start[/]");
            
            foreach (string cmd in task.Start)
            {
                ExecCommand.ExecWithDirectOutput(cmd, true, true);
            }
        }
        
        AnsiConsole.WriteLine("-------------------");
        
        return 0;
    }
}
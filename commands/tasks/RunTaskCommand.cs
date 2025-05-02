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
        [CommandArgument(0, "[Task]")]
        [Description("Execute the commands that are defined in this task")]
        public string Task { get; set; }
        
        [CommandOption("-i|--init")]
        [Description("Run commands defined under \"init\"")]
        [DefaultValue(false)]
        public bool RunInitCommands { get; set; }
        
        [CommandOption("-c|--commands")]
        [Description("Run commands defined under \"commands\"")]
        [DefaultValue(true)]
        public bool RunNormalCommands { get; set; }
        
        [CommandOption("-p|--prebuild")]
        [Description("Run commands defined under \"prebuild\"")]
        [DefaultValue(true)]
        public bool RunPreBuildCommands { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        if (string.Empty == settings.Task)
        {
            AnsiConsole.MarkupLine("[red]No task specified.[/");

            return 0;
        }

        if (!TasksConfig.Tasks.TryGetValue(settings.Task, out TaskEntryConfiguration task))
        {
            AnsiConsole.MarkupLine("[red]Unable to find the task \"" + settings.Task + "\" in the config file.[/]");

            return 0;
        }

        if (EnvironmentHelper.IsRunningInDevContainer() && task.Mode != TaskMode.All && task.Mode != TaskMode.DevContainer || !EnvironmentHelper.IsRunningInDevContainer() && task.Mode != TaskMode.All && task.Mode != TaskMode.Local)
        {
            AnsiConsole.MarkupLine("[red]This task is not available in the current environment.[/]");

            return 0;
        }

        AnsiConsole.WriteLine("-------------------");
        AnsiConsole.MarkupLine("[deepskyblue3]Executing commands for task[/] " + task.Name);
        
        if (settings.RunInitCommands && task.Init.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Init[/]");
            
            foreach (string cmd in task.Init)
            {
                ExecCommand.ExecWithDirectOutput(cmd);
            }
        }
        
        if (settings.RunPreBuildCommands && task.Prebuild.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]PreBuild[/]");
            
            foreach (string cmd in task.Prebuild)
            {
                ExecCommand.ExecWithDirectOutput(cmd);
            }
        }

        if (settings.RunNormalCommands && task.Command.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Commands[/]");
            
            foreach (string cmd in task.Command)
            {
                ExecCommand.ExecWithDirectOutput(cmd);
            }
        }
        
        AnsiConsole.WriteLine("-------------------");
        
        return 0;
    }
}
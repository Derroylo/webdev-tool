using System.Collections.Generic;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Commands.tasks;

internal class RunTaskCommand(IDebugOutputHelper _debugOutputHelper, IEnvironmentHelper _environmentHelper, TasksConfig _tasksConfig, ExecCommand _execCommand) : Command<RunTaskCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
        [CommandOption("-i|--init")]
        [Description("Run commands defined under \"init\"")]
        [DefaultValue(false)]
        public bool RunInitCommands { get; set; }
        
        [CommandOption("-c|--create")]
        [Description("Run commands defined under \"create\"")]
        [DefaultValue(false)]
        public bool RunCreateCommands { get; set; }
        
        [CommandOption("-s|--start")]
        [Description("Run commands defined under \"start\"")]
        [DefaultValue(false)]
        public bool RunStartCommands { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing RunTaskCommand", this);
        
        string taskName = context.Data?.ToString();
        
        if (string.IsNullOrEmpty(taskName))
        {
            AnsiConsole.MarkupLine("[red]No task specified.[/]");

            return 0;
        }
        
        if (!_tasksConfig.Tasks.TryGetValue(taskName, out TaskEntryConfiguration task))
        {
            AnsiConsole.MarkupLine("[red]Unable to find the task \"" + taskName + "\" in the config file.[/]");

            return 0;
        }

        if (task.Mode != TaskMode.All && ((_environmentHelper.IsRunningInDevContainer() && task.Mode != TaskMode.DevContainer) || (!_environmentHelper.IsRunningInDevContainer() && task.Mode != TaskMode.Local)))
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
                _execCommand.ExecWithDirectOutput(cmd, true, true);
            }
        }
        
        if (settings.RunCreateCommands && task.Create.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Create[/]");
            
            foreach (string cmd in task.Create)
            {
                _execCommand.ExecWithDirectOutput(cmd, true, true);
            }
        }

        if (settings.RunStartCommands && task.Start.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Start[/]");
            
            foreach (string cmd in task.Start)
            {
                _execCommand.ExecWithDirectOutput(cmd, true, true);
            }
        }
        
        AnsiConsole.WriteLine("-------------------");
        
        return 0;
    }
}
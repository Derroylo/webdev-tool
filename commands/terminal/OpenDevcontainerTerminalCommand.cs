using System;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.DevContainer;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Terminal;

internal class OpenDevcontainerTerminalCommand(IDebugOutputHelper _debugOutputHelper, IDevContainerHelper _devContainerHelper) : Command<OpenDevcontainerTerminalCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing OpenDevcontainerTerminalCommand", this);
        
        // Get the container id of the running devcontainer
        var containerId = _devContainerHelper.GetDevContainerId();
        
        if (string.IsNullOrEmpty(containerId))
        {
            AnsiConsole.MarkupLine("[red]No running devcontainer found.[/]");

            return 1;
        }
        
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        
        File.WriteAllText(applicationDir + ".terminal", containerId);
        
        return 0;
    }
}
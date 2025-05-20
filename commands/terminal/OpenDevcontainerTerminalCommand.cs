using System;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.devcontainer;

namespace WebDev.Tool.Commands.terminal;

internal class OpenDevcontainerTerminalCommand: Command
{
    public override int Execute(CommandContext context)
    {
        // Get the container id of the running devcontainer
        var containerId = DevContainerHelper.GetDevContainerId();
        
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
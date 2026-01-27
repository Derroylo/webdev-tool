using System;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper;
using WebDev.Tool.Helper.DevContainer;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Project;

public class StartProjectCommand(
    IDebugOutputHelper _debugOutputHelper,
    IDevContainerHelper _devContainerHelper,
    IDockerHelper _dockerHelper,
    IPathHelper _pathHelper
) : Command<StartProjectCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing StartProjectCommand", this);
        
        // Check if the devcontainer CLI is installed
        if (!_devContainerHelper.IsDevContainerCliInstalled())
        {
            AnsiConsole.MarkupLine("[red]Error:[/] devcontainer CLI is not installed.");
            
            var installConfirm = AnsiConsole.Confirm("To start the project, you need to install the devcontainer CLI. Do you want to install it now?");
            if (installConfirm)
            {
                if (!_devContainerHelper.IsNpmInstalled())
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] npm is not installed. Please install it first.");
                    
                    return 1;
                }
                
                if (!_devContainerHelper.InstallDevContainerCli())
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] Installing the devcontainer cli failed. Please install it manually.");

                    return 1;
                }
                
                AnsiConsole.MarkupLine("[green]devcontainer CLI installed successfully.[/]");
            }
            else
            {
                return 1;
            }
        }
        
        // Check if the folder already contains a devcontainer.json file
        if (!File.Exists(_pathHelper.GetWorkspacePath(false) + "/.devcontainer/devcontainer.json"))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] The current folder doesn´t contain a devcontainer.json file. Make sure you are in the right folder or use the \"webdev project init\" command to create a new project.");
            
            return 1;
        }
        
        _debugOutputHelper.WriteInfoOutput("Checking for running devcontainers", this);
        
        // Check for running devcontainers
        var runningContainers = _dockerHelper.GetRunningContainers("_devcontainer");
        if (runningContainers.Count > 0)
        {
            _debugOutputHelper.WriteWarningOutput("Found running devcontainers: " + string.Join(", ", runningContainers), this);
            
            AnsiConsole.MarkupLine("[yellow]Warning:[/] Found running devcontainers:");
            foreach (var container in runningContainers)
            {
                AnsiConsole.MarkupLine($"  - {container}");
            }
            
            var stopConfirm = AnsiConsole.Confirm("Do you want to stop these containers before continuing?");
            if (stopConfirm)
            {
                foreach (var container in runningContainers)
                {
                    _dockerHelper.StopContainer(container);
                }
                AnsiConsole.MarkupLine("[green]Successfully stopped running devcontainers.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Cannot start new devcontainer while others are running.[/]");
                return 1;
            }
        }

        // Check if the folder already contains a .devcontainer/vhost directory
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        
        _debugOutputHelper.WriteInfoOutput("Writing devcontainer up command to file: " + applicationDir + ".devcontainer_up", this);

        File.WriteAllText(applicationDir + ".devcontainer_up", "devcontainer up");
        
        return 0;
    }
}
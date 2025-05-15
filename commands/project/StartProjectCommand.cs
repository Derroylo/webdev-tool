using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.devcontainer;

namespace WebDev.Tool.Commands.Project;

internal class StartProjectCommand: Command
{
    public override int Execute(CommandContext context)
    {
        // Check if the devcontainer CLI is installed
        if (!DevContainerHelper.IsDevContainerCliInstalled())
        {
            AnsiConsole.MarkupLine("[red]Error:[/] devcontainer CLI is not installed.");
            
            var installConfirm = AnsiConsole.Confirm("To apply the template, you need to install the devcontainer CLI. Do you want to install it now?");
            if (installConfirm)
            {
                if (!DevContainerHelper.IsNpmInstalled())
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] npm is not installed. Please install it first.");
                    
                    return 1;
                }
                
                if (!DevContainerHelper.InstallDevContainerCli())
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
        if (!File.Exists("./.devcontainer/devcontainer.json"))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] The current folder doesn´t contain a devcontainer.json file. Make sure you are in the right folder or use the \"webdev project init\" command to create a new project.");
            
            return 1;
        }
        
        DevContainerHelper.WriteEnvFile();
        
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        
        File.WriteAllText(applicationDir + ".devcontainer_up", "devcontainer up");
        
        return 0;
    }
}
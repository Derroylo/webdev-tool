using System;
using Spectre.Console;
using Spectre.Console.Cli;
using LibGit2Sharp;
using System.IO;

namespace WebDev.Tool.Commands.Project;

internal class InitProjectCommand : Command
{
    public override int Execute(CommandContext context)
    {
        // Prompt for the Git repository URL
        var repoUrl = AnsiConsole.Ask<string>("Enter the [green]URL[/] of the Git repository:");

        // Prompt for the target folder
        var targetFolder = AnsiConsole.Ask<string>("Enter the [green]target folder[/] to clone the repository into:");

        // Check if the folder exists
        if (Directory.Exists(targetFolder))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] The target folder already exists.");
            
            return 1;
        }

        // Clone the repository
        if (!CloneRepository(repoUrl, targetFolder))
        {
            return 1;
        }

        // Ask which devcontainer template to use
        var template = this.SelectDecontainerTemplate();
        
        // Prompt for devcontainer basic settings like name
        
        // Finish setup with info that auth.json should be created manually
        
        // Prompt for any additional setup steps (second devcontainer like a linked shop system etc.)
        
        // Provide a summary of the setup steps
        return 0;
    }

    private bool CloneRepository(string repoUrl, string targetFolder)
    {
        try
        {
            AnsiConsole.Status()
                .Start("Cloning repository...", ctx =>
                {
                    var cloneOptions = new CloneOptions();

                    // Prompt for username/password if authentication is required
                    cloneOptions.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                    {
                        var username = AnsiConsole.Ask<string>("Enter [green]username[/]:");
                        var password = AnsiConsole.Prompt(
                            new TextPrompt<string>("Enter [green]password[/]:")
                                .PromptStyle("red")
                                .Secret());
                        return new UsernamePasswordCredentials
                        {
                            Username = username,
                            Password = password
                        };
                    };

                    Repository.Clone(repoUrl, targetFolder, cloneOptions);
                });

            AnsiConsole.MarkupLine("[green]Repository cloned successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            
            return false;
        }

        return true;
    }

    private string SelectDecontainerTemplate()
    {
        var remoteRepo = "https://github.com/devcontainers/templates/tree/main/src";
        
        
        return "test";
    }
}

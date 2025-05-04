using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using LibGit2Sharp;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using Octokit;
using WebDev.Tool.Helper.devcontainer;
using Repository = LibGit2Sharp.Repository;

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
        var template = SelectDecontainerTemplate();

        if (string.IsNullOrEmpty(template))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] No template selected.");
            
            return 1;
        }
        
        // Prompt for devcontainer basic settings like name
        var devContainerName = AnsiConsole.Ask<string>("Enter the [green]name[/] of the devcontainer:");
        var devContainerDescription = AnsiConsole.Ask<string>("Enter the [green]description[/] of the devcontainer:");
        
        // Create a summary of all selected options
        AnsiConsole.MarkupLine("[green]Summary:[/]");
        AnsiConsole.MarkupLine($"[green]Template:[/] {template}");
        AnsiConsole.MarkupLine($"[green]Name:[/] {devContainerName}");
        AnsiConsole.MarkupLine($"[green]Description:[/] {devContainerDescription}");
        
        // Ask for confirmation to proceed
        var proceed = AnsiConsole.Confirm("Do you want to proceed with the setup?");
        if (!proceed)
        {
            AnsiConsole.MarkupLine("[red]Setup cancelled.[/]");
            
            return 0;
        }
        
        // Apply the devcontainer template
        AnsiConsole.WriteLine(DevContainerHelper.ApplyTemplate(targetFolder, "ghcr.io/devcontainers/templates/cpp:latest"));
        
        // Update the devcontainer.json with the name and description
        DevContainerHelper.UpdateNameAndDescription(targetFolder, devContainerName, devContainerDescription);
        
        AnsiConsole.MarkupLine("[green]All done.[/]");
        
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
        var repoUrl = "https://github.com/devcontainers/templates.git";
        var localRepoPath = Path.Combine(Directory.GetCurrentDirectory(), "devContainerTemplates");
        var srcPath = Path.Combine(localRepoPath, "src");
        var templates = new Dictionary<string, string>();

        try
        {
            if (!Directory.Exists(localRepoPath))
            {
                AnsiConsole.Status().Start("Cloning templates repository...", ctx =>
                {
                    Repository.Clone(repoUrl, localRepoPath);
                });
                AnsiConsole.MarkupLine("[green]Templates repository cloned.[/]");
            }

            if (!Directory.Exists(srcPath))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] src folder not found in cloned repository.");
                return string.Empty;
            }

            foreach (var folder in Directory.GetDirectories(srcPath))
            {
                var jsonFile = Path.Combine(folder, "devcontainer-template.json");
                if (!File.Exists(jsonFile)) continue;

                try
                {
                    var json = File.ReadAllText(jsonFile);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("name", out var nameProp))
                    {
                        var name = nameProp.GetString();
                        if (!string.IsNullOrEmpty(name))
                            templates[name] = folder;
                    }
                }
                catch (JsonException) { /* ignore invalid json */ }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return string.Empty;
        }
        
        templates = templates.OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.Value);
        
        if (templates.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No templates found.[/]");
            return string.Empty;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [green]DevContainer Template[/]:")
                .AddChoices(templates.Keys)
        );

        return templates[selected];
    }
}

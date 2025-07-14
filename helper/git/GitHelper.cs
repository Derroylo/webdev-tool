using System;
using LibGit2Sharp;
using Spectre.Console;

namespace WebDev.Tool.Helper.git;

internal class GitHelper
{
    public static bool CloneRepository(string repoUrl, string targetFolder)
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
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            
            return false;
        }

        return true;
    }

    public static bool CheckoutBranch(string repoPath, string branchName)
    {
        try
        {
            using (var repo = new Repository(repoPath))
            {
                var branch = repo.Branches[branchName] ?? repo.Branches[$"origin/{branchName}"];
                if (branch == null)
                {
                    return false;
                }

                LibGit2Sharp.Commands.Checkout(repo, branch);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");

            return false;
        }

        return true;
    }
}
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Spectre.Console;
using WebDev.Tool.Helper;

namespace WebDev.Tool.Helper.git;

public class GitHelper : IGitHelper
{
    private (string username, string password) GetCredentialsFromGitHelper(string repoUrl)
    {
        try
        {
            // Parse the URL to extract protocol, host, and path
            var uri = new Uri(repoUrl);
            var protocol = uri.Scheme;
            var host = uri.Host;
            var path = uri.PathAndQuery.TrimStart('/');

            // Build the credential request according to git credential protocol
            var credentialRequest = new StringBuilder();
            credentialRequest.AppendLine($"protocol={protocol}");
            credentialRequest.AppendLine($"host={host}");
            if (!string.IsNullOrEmpty(path))
            {
                credentialRequest.AppendLine($"path={path}");
            }
            credentialRequest.AppendLine(); // Empty line required by protocol

            // Query git credential helper
            using (var process = new Process())
            {
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = "credential fill";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                // Send the credential request
                process.StandardInput.Write(credentialRequest.ToString());
                process.StandardInput.Close();

                // Read the response
                var response = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(response))
                {
                    // Parse the response
                    var usernameMatch = Regex.Match(response, @"^username=(.+)$", RegexOptions.Multiline);
                    var passwordMatch = Regex.Match(response, @"^password=(.+)$", RegexOptions.Multiline);

                    if (usernameMatch.Success && passwordMatch.Success)
                    {
                        var username = usernameMatch.Groups[1].Value.Trim();
                        var password = passwordMatch.Groups[1].Value.Trim();
                        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        {
                            return (username, password);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // If anything fails, return empty strings to fall back to prompting
        }

        return (string.Empty, string.Empty);
    }

    public bool CloneRepository(string repoUrl, string targetFolder)
    {
        try
        {
            // First, try to get credentials from git credential helper
            var (username, password) = GetCredentialsFromGitHelper(repoUrl);

            var cloneOptions = new CloneOptions();
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                // Use credentials from git credential helper
                cloneOptions.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                {
                    return new UsernamePasswordCredentials
                    {
                        Username = username,
                        Password = password
                    };
                };
            }
            else
            {
                // Try DefaultCredentials first (for systems where git credential helper might not be configured)
                cloneOptions.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                {
                    return new DefaultCredentials();
                };
            }

            Repository.Clone(repoUrl, targetFolder, cloneOptions);
        }
        catch (LibGit2SharpException ex) when (ex.Message.Contains("could not find appropriate mechanism for credentials") || 
                                               ex.Message.Contains("authentication required"))
        {
            // Credentials didn't work, retry with user-provided credentials
            try
            {
                AnsiConsole.MarkupLine("[yellow]No stored credentials found. Please provide authentication.[/]");
                
                var cloneOptions = new CloneOptions();
                cloneOptions.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                {
                    var promptUsername = AnsiConsole.Ask<string>("Enter [green]username[/]:");
                    var promptPassword = AnsiConsole.Prompt(
                        new TextPrompt<string>("Enter [green]password[/]:")
                            .PromptStyle("red")
                            .Secret());
                    return new UsernamePasswordCredentials
                    {
                        Username = promptUsername,
                        Password = promptPassword
                    };
                };

                Repository.Clone(repoUrl, targetFolder, cloneOptions);
            }
            catch (Exception retryEx)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {retryEx.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            
            return false;
        }

        return true;
    }

    public bool CheckoutBranch(string repoPath, string branchName)
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
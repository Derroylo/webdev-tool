using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Commands.secrets;

internal class LoadSecretsCommand: Command
{
    public override int Execute(CommandContext context)
    {
        if (SecretsConfig.Secrets.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No secrets found in the config file[/]");

            return 0;
        }
        
        AnsiConsole.WriteLine("Found " +  SecretsConfig.Secrets.Count + " secrets");

        foreach (KeyValuePair<string, SecretConfiguration> secret in SecretsConfig.Secrets)
        {
            AnsiConsole.WriteLine(secret.Key);
            AnsiConsole.WriteLine(secret.Value.SourceName);
            AnsiConsole.WriteLine(secret.Value.SourceType);
            AnsiConsole.WriteLine(secret.Value.TargetType);
            AnsiConsole.WriteLine(secret.Value.ProjectId);
        }
        
        return 0;
    }
}
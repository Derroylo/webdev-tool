using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;
using WebDev.Tool.Helper.Secrets.Handler;

namespace WebDev.Tool.Helper.Secrets;

internal class SecretsLoader
{
    private static Dictionary<string, string> _envVarSecrets = new Dictionary<string, string>();

    public static IReadOnlyDictionary<string, string> EnvVarSecrets => _envVarSecrets;

    private static bool _loadedEnvVarSecrets = false;
    
    public static bool LoadFileSecrets(bool showMessages = true)
    {
        if (SecretsConfig.Secrets.Count == 0)
        {
            return true;
        }
        
        var secretsHandlers = GetSecretsHandler();
        
        if (secretsHandlers.Count == 0)
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[red]No secrets handlers configured[/]");
            }
            
            return false;
        }

        var fileSecrets = SecretsConfig.Secrets
                .Where(s => !string.IsNullOrEmpty(s.Value.Target.File))
                .ToDictionary(s => s.Key, s => s.Value);

        if (fileSecrets.Count == 0)
        {
            return true;
        }

        foreach (KeyValuePair<string, SecretConfiguration> secret in fileSecrets)
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[green]Loading file secret: {secret.Key}[/]");
            }

            var secretContent = secretsHandlers.FirstOrDefault(h => h.SupportsFileLoad())?.HandleLoadSecret(secret.Key, secret.Value, showMessages);

            if (secretContent == null)
            {
                return false;
            }

            secretsHandlers.FirstOrDefault(h => h.SupportsFileWrite())?.HandleWriteSecret(secret.Key, secretContent, secret.Value, showMessages);
        }

        return true;
    }

    public static bool LoadEnvVarSecrets(bool showMessages = true)
    {
        if (_loadedEnvVarSecrets)
        {
            return true;
        }

        _loadedEnvVarSecrets = true;

        if (SecretsConfig.Secrets.Count == 0)
        {
            return true;
        }
        
        var secretsHandlers = GetSecretsHandler();
        
        if (secretsHandlers.Count == 0)
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[red]No secrets handlers configured[/]");
            }
            
            return false;
        }

        var envVarSecrets = SecretsConfig.Secrets
                .Where(s => !string.IsNullOrEmpty(s.Value.Target.EnvVar))
                .ToDictionary(s => s.Key, s => s.Value);

        if (envVarSecrets.Count == 0)
        {
            return true;
        }

        foreach (KeyValuePair<string, SecretConfiguration> secret in envVarSecrets)
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[green]Loading envvar secret: {secret.Key}[/]");
            }

            var secretContent = secretsHandlers.FirstOrDefault(h => h.SupportsFileLoad())?.HandleLoadSecret(secret.Key, secret.Value, showMessages);

            if (secretContent == null)
            {
                return false;
            }

            var lines = secretContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var secretDict = lines
                .Where(line => line.Contains('='))
                .ToDictionary(
                    line => line.Substring(0, line.IndexOf('=')).Trim(),
                    line => line.Substring(line.IndexOf('=') + 1).Trim()
                );

            foreach (var expectedVar in secret.Value.Target.ExpectedVars)
            {
                if (!secretDict.ContainsKey(expectedVar))
                {
                    if (showMessages)
                    {
                        AnsiConsole.MarkupLine($"[red]Expected environment variable {expectedVar} not found in secret {secret.Key}[/]");
                    }

                    return false;
                }
            }

            foreach (var kvp in secretDict)
            {
                _envVarSecrets[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in secretDict)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);

                if (showMessages)
                {
                    AnsiConsole.MarkupLine($"[green]Set environment variable: {kvp.Key}[/]");
                }
            }
        }

        return true;
    }
    
    private static List<SecretsHandlerInterface> GetSecretsHandler()
    {
        var secretsHandler = new List<SecretsHandlerInterface>
        {
            new FileHandler()
        };
        
        return secretsHandler;
    }
}
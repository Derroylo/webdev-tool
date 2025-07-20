using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;
using WebDev.Tool.Helper.Secrets.Loader;

namespace WebDev.Tool.Helper.Secrets;

internal class SecretsLoader
{
    public static void LoadSecret(bool showMessages = true, bool loadFileSecrets = true, bool loadEnvVarSecrets = true)
    {
        if (SecretsConfig.Secrets.Count == 0)
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[red]No secrets found in the config file[/]");
            }

            return;
        }
        
        var secretsLoader = GetSecretsLoader();
        
        if (secretsLoader == null)
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[red]No secrets loader configured[/]");
            }
            
            return;
        }

        if (!secretsLoader.IsCorrectlyConfigured())
        {
            return;
        }
        
        if (loadFileSecrets)
        {
            var fileSecrets = SecretsConfig.Secrets
                .Where(s => !string.IsNullOrEmpty(s.Value.Target.File))
                .ToDictionary(s => s.Key, s => s.Value);

            if (fileSecrets.Count > 0)
            {
                foreach (KeyValuePair<string, SecretConfiguration> secret in fileSecrets)
                {
                    if (showMessages)
                    {
                        AnsiConsole.MarkupLine($"[green]Loading file secret: {secret.Key}[/]");
                    }

                    secretsLoader.HandleFileSecrets(secret.Key, secret.Value, showMessages);
                }
            }
        }
        
        if (loadEnvVarSecrets)
        {
            var envVarSecrets = SecretsConfig.Secrets
                .Where(s => !string.IsNullOrEmpty(s.Value.Target.EnvVar))
                .ToDictionary(s => s.Key, s => s.Value);

            if (envVarSecrets.Count > 0)
            {
                foreach (KeyValuePair<string, SecretConfiguration> secret in envVarSecrets)
                {
                    if (showMessages)
                    {
                        AnsiConsole.MarkupLine($"[green]Loading envvar secret: {secret.Key}[/]");
                    }

                    secretsLoader.HandleEnvVarSecrets(secret.Key, secret.Value, showMessages);
                }
            }
        }
    }
    
    private static SecretsLoaderInterface GetSecretsLoader()
    {
        var secretsLoader = AppSettingsHelper.AppSettings.SecretsLoader.Type;

        if (string.IsNullOrEmpty(secretsLoader))
        {
            return null;
        }
        
        return secretsLoader.ToLower() switch
        {
            "file" => new FileLoader(),
            _ => null
        };
    }
}
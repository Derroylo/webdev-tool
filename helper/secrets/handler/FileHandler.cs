using System;
using System.IO;
using System.Linq;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Helper.Secrets.Handler;

internal class FileHandler(IPathHelper _pathHelper, ITranslationHelper _translationHelper): SecretsHandlerInterface
{
    public bool IsCorrectlyConfigured()
    {
        var secretDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets");

        if (!Directory.Exists(secretDir))
        {
            Directory.CreateDirectory(secretDir);
            
            return false;
        }
        
        return true;
    }
    
    public bool SupportsFileLoad() => true;

    public bool SupportsFileWrite() => true;

    public string HandleLoadSecret(string secretName, SecretConfiguration secret, bool showMessages)
    {
        var secretDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets");
        var sourcePath = Path.Combine(secretDir, secret.Source.Group);

        if (!Directory.Exists(sourcePath))
        {
            if (showMessages) 
            {
                AnsiConsole.MarkupLine($"{GetMissingMessage(secretName, sourcePath, secret)}");
            }

            return null;
        }

        var files = Directory.GetFiles(sourcePath, $"{secret.Source.Key}.*")
            .Where(f => !f.EndsWith(":Zone.Identifier", StringComparison.OrdinalIgnoreCase)
                     && !f.EndsWith(".Zone.Identifier", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (files.Length == 0)
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"{GetMissingMessage(secretName, sourcePath, secret)}");
            }

            return null;
        }

        return File.ReadAllText(files.First());
    }

    public bool HandleWriteSecret(string secretName, string secretContent, SecretConfiguration secret, bool showMessages)
    {
        var targetPath = Path.Combine(_pathHelper.GetWorkspacePath(false), secret.Target.File);
        
        try
        {
            File.WriteAllText(targetPath, secretContent);
            
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[green]Secret {secretName} written to file: {targetPath}[/]");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[red]Failed to write secret {secretName} to file: {ex.Message}[/]");
            }
            
            return false;
        }
    }

    private string GetMissingMessage(string secretName, string sourcePath, SecretConfiguration secret)
    {
        var missingMessage = secret.MissingMessage;

        if (string.IsNullOrEmpty(missingMessage))
        {
            return $"[red]File not found for secret {secretName}: {secret.Target.File}/{secret.Source.Key}[/]";;
        }

        missingMessage = _translationHelper.GetString(missingMessage);

        missingMessage = missingMessage.Replace("#SECRET_FILE_NAME#", secret.Source.Key);
        missingMessage = missingMessage.Replace("#SECRET_FILE_DIRECTORY#", sourcePath);

        if (secret.Target.ExpectedVars.Count > 1)
        {
            missingMessage = missingMessage.Replace("#DOCKER_USERNAME#", secret.Target.ExpectedVars[0]);
            missingMessage = missingMessage.Replace("#DOCKER_PASSWORD#", secret.Target.ExpectedVars[1]);
        }
        else 
        {
            missingMessage = missingMessage.Replace("#DOCKER_USERNAME#", "DOCKER_USERNAME");
            missingMessage = missingMessage.Replace("#DOCKER_PASSWORD#", "DOCKER_PASSWORD");
        }
        
        if (secret.Target.ExpectedSecrets.Count > 0)
        {
            missingMessage = missingMessage.Replace("#COMPOSER_AUTH_SECRETS#", "(" + string.Join(", ", secret.Target.ExpectedSecrets) + ")");
        }
        else
        {
            missingMessage = missingMessage.Replace("#COMPOSER_AUTH_SECRETS#", "");
        }

        return missingMessage;
    }
}
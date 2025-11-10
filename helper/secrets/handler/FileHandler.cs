using System;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Helper.Secrets.Handler;

internal class FileHandler: SecretsHandlerInterface
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
                var missingMessage = secret.MissingMessage;

                if (!string.IsNullOrEmpty(missingMessage))
                {
                    missingMessage = TranslationHelper.GetString(missingMessage);
                }
                else
                {
                    missingMessage = $"[red]Directory not found for secret {secretName}: {sourcePath}[/]";
                }

                AnsiConsole.MarkupLine($"{missingMessage}");
            }

            return null;
        }

        var files = Directory.GetFiles(sourcePath, $"{secret.Source.Key}.*");

        if (files.Length == 0)
        {
            var missingMessage = secret.MissingMessage;

            if (!string.IsNullOrEmpty(missingMessage))
            {
                missingMessage = TranslationHelper.GetString(missingMessage);
            }
            else
            {
                missingMessage = $"[red]File not found for secret {secretName}: {sourcePath}[/]";
            }

            if (showMessages)
            {
                AnsiConsole.MarkupLine($"{missingMessage}");
            }

            return null;
        }

        return File.ReadAllText(files[0]);
    }

    public bool HandleWriteSecret(string secretName, string secretContent, SecretConfiguration secret, bool showMessages)
    {
        var targetPath = Path.Combine(PathHelper.GetWorkspacePath(false), secret.Target.File);
        
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
}
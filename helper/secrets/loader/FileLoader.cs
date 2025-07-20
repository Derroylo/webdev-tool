using System;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Helper.Secrets.Loader;

internal class FileLoader: SecretsLoaderInterface
{
    public bool IsCorrectlyConfigured()
    {
        var settings = AppSettingsHelper.AppSettings.SecretsLoader.Settings;

        if (settings is null || settings.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]FileLoader for secrets is missing its settings[/]");

            return false;
        }
        
        if (settings.ContainsKey("filePath") == false)
        {
            AnsiConsole.MarkupLine($"[red]FileLoader for secrets is missing the 'filePath' setting[/]");

            return false;
        }
        
        var filePath = settings["filePath"];

        if (!Directory.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]FileLoader for secrets has an invalid 'filePath': {filePath} does not exist[/]");
            
            return false;
        }
        
        return true;
    }
    
    public bool HandleFileSecrets(string secretName, SecretConfiguration secret, bool showMessages)
    {
        var settings = AppSettingsHelper.AppSettings.SecretsLoader.Settings;
        var filePath = settings["filePath"];
        
        if (string.IsNullOrEmpty(secret.Target.File))
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[red]Secret {secretName} has no file target[/]");
            }

            return false;
        }

        var sourcePath = Path.Combine(filePath, secret.Source.Group, secret.Target.File);

        if (!File.Exists(sourcePath))
        {
            if (showMessages)
            {
                AnsiConsole.MarkupLine($"[red]File not found for secret {secretName}: {sourcePath}[/]");
            }

            return false;
        }
        
        var targetPath = Path.Combine(PathHelper.GetWorkspacePath(false), secret.Target.File);
        
        try
        {
            File.WriteAllText(File.ReadAllText(sourcePath), targetPath);
            
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
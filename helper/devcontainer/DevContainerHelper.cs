using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace WebDev.Tool.Helper.devcontainer;

public class DevContainerHelper
{
    public static bool IsDevContainerCliInstalled()
    {
        // Check if the devcontainer CLI is installed
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "devcontainer",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return !string.IsNullOrWhiteSpace(output);
    }
    
    public static bool IsNpmInstalled()
    {
        // Check if npm is installed
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "npm",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return !string.IsNullOrWhiteSpace(output);
    }
    
    public static bool InstallDevContainerCli()
    {
        // Install the devcontainer cli via npm
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "install -g @devcontainers/cli",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        return process.ExitCode == 0;
    }
    
    public static string ApplyTemplate(string workspacePath, string templateId, string templateArgs = null, string features = null)
    {
        var cmd = "devcontainer templates apply";
        
        var args = " -w " + workspacePath;
        args += " -t " + templateId;

        if (templateArgs != null)
        {
            args += " -a '" + templateArgs + "'";
        }
        
        if (features != null)
        {
            args += " -f '" + features + "'";
        }

        return ExecCommand.ExecWithDirectOutput(cmd + args);
    }
    
    public static void UpdateNameAndDescription(string workspacePath, string name)
    {
        var filePath = Path.Combine(workspacePath, ".devcontainer", "devcontainer.json");

        try
        {
            // Read the file content
            var jsonContent = File.ReadAllText(filePath);

            // Replace the "name" property value
            var updatedContent = Regex.Replace(
                jsonContent,
                @"""name""\s*:\s*"".*?""",
                $@"""name"": ""{name}""",
                RegexOptions.Singleline
            );

            // Write the updated content back to the file
            File.WriteAllText(filePath, updatedContent);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error updating devcontainer.json: {ex.Message}[/]");
        }
    }
}
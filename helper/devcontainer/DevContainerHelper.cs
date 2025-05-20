using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Helper.devcontainer;

public class DevContainerHelper
{
    public static JsonDocument ReadDevContainerConfig(string workspacePath = null)
    {
        // determine path
        var baseDir = workspacePath ?? Directory.GetCurrentDirectory();
        var filePath = Path.Combine(baseDir, ".devcontainer", "devcontainer.json");
        
        // read and strip comments (// and /* */)
        var raw = File.ReadAllText(filePath);
        var cleaned = Regex.Replace(raw, @"(\/\/.*?$)|(/\*.*?\*/)", "", RegexOptions.Singleline | RegexOptions.Multiline);
        
        return JsonDocument.Parse(cleaned);
    }

    public static void WriteEnvFile()
    {
        var workspacePath = Environment.GetEnvironmentVariable("WEBDEV_WORKSPACE_FOLDER");
        
        if (string.IsNullOrEmpty(workspacePath)) {
            workspacePath = Directory.GetCurrentDirectory();
        }
        
        var filePath = Path.Combine(workspacePath, ".devcontainer", ".env");

        var envVars = new List<string>();
        envVars.Add("WEBDEV_PROXY_DOMAIN=" + GeneralConfig.Proxy.Domain);
        envVars.Add("WEBDEV_PROXY_SUBDOMAIN=" + GeneralConfig.Proxy.Subdomain);
            
        File.WriteAllLines(filePath, envVars);
    }
    
    public static string GetDevContainerId()
    {
        var output = ExecCommand.Exec("docker ps --filter \"name=devcontainer-app\" --format \"{{.ID}}\"");
        
        return string.IsNullOrEmpty(output) ? null : output.Trim();
    }
    
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
    
    public static bool InstallDevContainerCli(bool useSudo = false)
    {
        // Install the devcontainer cli via npm
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = (useSudo ? "sudo " : "") + "npm",
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
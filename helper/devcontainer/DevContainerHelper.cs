using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebDev.Tool.Helper.devcontainer;

public class DevContainerHelper
{
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
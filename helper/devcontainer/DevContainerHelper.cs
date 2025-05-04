using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Spectre.Console;

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
    
    public static void UpdateNameAndDescription(string workspacePath, string name, string description)
    {
        var jsonContent = File.ReadAllText(workspacePath + "/.devcontainer/devcontainer.json");

        var jsonObject = JsonNode.Parse(jsonContent) as JsonObject;
        if (jsonObject == null)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Failed to parse devcontainer.json.");
            return;
        }

        // Add or update the name and description properties
        jsonObject["name"] = name;
        jsonObject["description"] = description;

        // Serialize and save the updated JSON
        var updatedJson = JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(workspacePath + "/.devcontainer/devcontainer.json", updatedJson);
    }
}
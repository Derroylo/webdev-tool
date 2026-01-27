using System;
using System.IO;

namespace WebDev.Tool.Helper;

public class PathHelper : IPathHelper
{
    public bool IsMainWorkspace { get; set; } = true;
    
    public string GetWorkspacePath(bool insideContainer = true)
    {
        return insideContainer ? GetWorkspacePathInsideContainer() : GetWorkspacePathHost();
    }
    
    private string GetWorkspacePathInsideContainer()
    {
        var workspacePath = Environment.GetEnvironmentVariable("WEBDEV_WORKSPACE_FOLDER");
        
        if (string.IsNullOrEmpty(workspacePath)) {
            workspacePath = "/var/www/html";
        }

        // TODO This needs to be better implemented later, just a quick fix when using in other workspace folders
        if (!IsMainWorkspace)
        {
            workspacePath = Directory.GetCurrentDirectory();
        }
        
        return workspacePath;
    }
    
    private string GetWorkspacePathHost()
    {
        return Directory.GetCurrentDirectory();
    }
}
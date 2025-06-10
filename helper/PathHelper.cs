using System;
using System.IO;

namespace WebDev.Tool.Helper;

internal class PathHelper
{
    public static bool IsMainWorkspace = true;
    
    public static string GetWorkspacePath(bool insideContainer = true)
    {
        return insideContainer ? GetWorkspacePathInsideContainer() : GetWorkspacePathHost();
    }
    
    private static string GetWorkspacePathInsideContainer()
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
    
    private static string GetWorkspacePathHost()
    {
        return Directory.GetCurrentDirectory();
    }
}
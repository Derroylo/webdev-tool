using System;
using System.IO;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Helper;

internal class PathHelper
{
    public static bool IsMainWorkspace = true;
    
    public static string GetWorkspacePath(bool insideContainer = true)
    {
        return insideContainer ? GetWorkspacePathInternal() : GetWorkspacePathExternal();
    }
    
    private static string GetWorkspacePathInternal()
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
    
    private static string GetWorkspacePathExternal()
    {
        var workspacePath = Directory.GetCurrentDirectory();

        return workspacePath;
    }
}
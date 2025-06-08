using System;
using System.IO;

namespace WebDev.Tool.Helper;

internal class PathHelper
{
    public static string GetWorkspacePath(bool insideContainer = true)
    {
        return insideContainer ? GetWorkspacePathInternal() : GetWorkspacePathExternal();
    }
    
    private static string GetWorkspacePathInternal()
    {
        var workspacePath = Environment.GetEnvironmentVariable("WEBDEV_WORKSPACE_FOLDER");
        
        if (string.IsNullOrEmpty(workspacePath)) {
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
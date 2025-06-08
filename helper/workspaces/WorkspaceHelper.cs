using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.git;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Helper.workspaces;

internal class WorkspaceHelper
{
    public static bool ValidateWorkspaces(bool debug = false)
    {
        if (WorkspacesConfig.Workspaces.Count == 0)
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]No workspaces defined in the config.[/]");
            }

            return false;
        }

        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
        {
            var workspaceFolder = "";

            if (workspace.Value.Folder != "" && (workspace.Value.Folder.StartsWith("./") || workspace.Value.Folder.StartsWith("../")))
            {
                workspaceFolder = workspace.Value.Folder + "/";
            }
            else
            {
                workspaceFolder = GeneralConfig.WorkspaceFolder + "/" + workspace.Value.Folder + "/";
            }

            if (!Directory.Exists(workspaceFolder) && workspace.Value.Repository == "")
            {
                AnsiConsole.MarkupLine("[red]Workspace: [bold yellow]{0}[/] is [red]invalid[/][/]", workspace.Key);
                AnsiConsole.MarkupLine("Reason: Workspace folder does not exist and no Repository was given: {0}", workspaceFolder);
                
                return false;
            }
            
            if (!Directory.Exists(workspaceFolder) && workspace.Value.Repository != "")
            {
                AnsiConsole.MarkupLine("Cloning repository {0} into {1}", workspace.Value.Repository, workspaceFolder);
                
                if (!GitHelper.CloneRepository(workspace.Value.Repository, workspaceFolder))
                {
                    return false;
                }
            
                AnsiConsole.MarkupLine("[green]Repository cloned successfully![/]");
            }

            // This mode is not supported yet
            if (workspace.Value.Mode == WorkspaceMode.DevContainer)
            {
                throw new Exception("DevContainer workspaces are not supported yet.");
            }
            
            if (workspace.Value.Mode == WorkspaceMode.DevContainer && !File.Exists(workspaceFolder + ".devcontainer/devcontainer.json"))
            {
                AnsiConsole.MarkupLine("[red]Workspace: [bold yellow]{0}[/] is [red]invalid[/][/]", workspace.Key);
                AnsiConsole.MarkupLine("Reason: .devcontainer/devcontainer.json not found in the workspace folder {0}", workspaceFolder);
                
                return false;
            }
            
            if (debug)
            {
                AnsiConsole.MarkupLine("Workspace: [bold yellow]{0}[/] is [green]valid[/]", workspace.Key);
            }
        }

        return true;
    }
    
    public static bool PrepareWorkspaces(bool debug = false)
    {
        if (WorkspacesConfig.Workspaces.Count == 0)
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]No workspaces defined in the config.[/]");
            }

            return false;
        }
        
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        var workspaces = new List<string>();

        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
        {
            if (workspace.Value.Folder != "" &&
                (workspace.Value.Folder.StartsWith("./") || workspace.Value.Folder.StartsWith("../")))
            {
                // TODO: Create a symlink from the given folder to the workspace folder, otherwise the source code will not be available in the container
            }
            
            var workspaceFolder = GeneralConfig.WorkspaceFolder + "/" + workspace.Value.Folder + "/";

            if (workspace.Value.Mode == WorkspaceMode.DevContainer)
            {
                workspaces.Add(workspaceFolder);
            }
            else
            {
                CreateVhostWorkspace(workspace.Value);
            }
        }

        if (workspaces.Count > 0)
        {
            File.WriteAllLines(applicationDir + ".workspaces_start", workspaces);            
        }
        
        return true;
    }

    private static void CreateVhostWorkspace(WorkspaceEntryConfiguration workspace)
    {
        var vHostConfig = """
            <VirtualHost *:8080>
              ServerName #SUDOMAIN#.dev.localhost

              ServerAdmin webmaster@localhost
              DocumentRoot #DOCROOT#

              <Directory "#DOCROOT#">
                  AllowOverride all
                  Require all granted
              </Directory>

              ErrorLog ${APACHE_LOG_DIR}/error.log
              CustomLog ${APACHE_LOG_DIR}/access.log combined
            </VirtualHost>
            """;
        
        vHostConfig = vHostConfig.Replace("#SUDOMAIN#", workspace.SubDomain);
        vHostConfig = vHostConfig.Replace("#DOCROOT#", Path.Combine("/var/www/html/", GeneralConfig.WorkspaceFolder, workspace.Folder, workspace.DocRoot));

        var workspacePath = PathHelper.GetWorkspacePath();

        if (!Directory.Exists(Path.Combine(workspacePath, ".devcontainer", "vhost")))
        {
            Directory.CreateDirectory(Path.Combine(workspacePath, ".devcontainer", "vhost"));
        }
        
        File.WriteAllText(Path.Combine(workspacePath, ".devcontainer", "vhost") + "/" + workspace.SubDomain + ".conf", vHostConfig);
    }
}
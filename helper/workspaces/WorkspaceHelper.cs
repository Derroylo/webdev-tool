using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.apache;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.git;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Helper.workspaces;

internal class WorkspaceHelper
{
    public static bool ValidateWorkspaces(bool debug = false)
    {
        if (WorkspacesConfig.Workspaces.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]At least one workspace needs to be defined in the config.[/]");

            return false;
        }

        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
        {
            if (workspace.Key == "main")
            {
                continue;
            }
            
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
            
            if (!Directory.Exists(workspaceFolder) && !string.IsNullOrEmpty(workspace.Value.Repository))
            {
                AnsiConsole.MarkupLine("Cloning repository {0} into {1}", workspace.Value.Repository, workspaceFolder);
                
                if (!GitHelper.CloneRepository(workspace.Value.Repository, workspaceFolder))
                {
                    AnsiConsole.MarkupLine($"[red]Failed to clone repository:[/] {workspace.Value.Repository}");
                    
                    return false;
                }
            
                AnsiConsole.MarkupLine("[green]Repository cloned successfully![/]");

                if (!string.IsNullOrEmpty(workspace.Value.Branch))
                {
                    if (!GitHelper.CheckoutBranch(workspaceFolder, workspace.Value.Branch))
                    {
                        AnsiConsole.MarkupLine($"[red]Failed to check out branch:[/] {workspace.Value.Branch}");
                        
                        return false;
                    }
                    
                    AnsiConsole.MarkupLine($"[green]Checked out branch:[/] {workspace.Value.Branch}");
                }
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
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        var workspaces = new List<string>();
       
        if (WorkspacesConfig.Workspaces.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No workspaces defined in the config.[/]");

            return false;
        }
        
        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
        {
            if (workspace.Value.DisableWeb)
            {
                continue;
            }

            if (workspace.Key == "main")
            {
                // Add www subdomain to the main workspace, otherwise the vhost config will not be generated
                if (!workspace.Value.SubDomains.Contains("www"))
                {
                    workspace.Value.SubDomains.Add("www");
                }

                CreateVhostWorkspace(workspace.Value, true);
                
                continue;
            }
            
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

    private static void CreateVhostWorkspace(WorkspaceEntryConfiguration workspace, bool isMainWorkspace = false, bool isWwwSubdomain = false)
    {
        var vHostConfig = """
            <VirtualHost *:8080>
              ServerName #WSSUDOMAIN##SUBDOMAIN#.#DOMAIN#

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
        
        vHostConfig = vHostConfig.Replace("#SUBDOMAIN#", GeneralConfig.Proxy.SubDomain);
        vHostConfig = vHostConfig.Replace("#DOMAIN#", GeneralConfig.Proxy.Domain);

        foreach (var subDomain in workspace.SubDomains)
        {
            var configFileName = subDomain + ".conf";

            if (!isMainWorkspace)
            {
                vHostConfig = vHostConfig.Replace("#WSSUDOMAIN#", subDomain + ".");
                vHostConfig = vHostConfig.Replace("#DOCROOT#", Path.Combine("/var/www/html/", GeneralConfig.WorkspaceFolder, workspace.Folder, workspace.DocRoot));
            }
            else
            {
                if (isWwwSubdomain)
                {
                    vHostConfig    = vHostConfig.Replace("#WSSUDOMAIN#", "www.");
                    configFileName = "www_main.conf";
                }
                else
                {
                    vHostConfig    = vHostConfig.Replace("#WSSUDOMAIN#", "");
                    configFileName = "main.conf";
                }
                
                vHostConfig = vHostConfig.Replace("#DOCROOT#", Path.Combine("/var/www/html/", workspace.DocRoot));
            }

            var workspacePath = PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer());

            if (!Directory.Exists(Path.Combine(workspacePath, ".devcontainer", "vhost")))
            {
                Directory.CreateDirectory(Path.Combine(workspacePath, ".devcontainer", "vhost"));
            }
            
            File.WriteAllText(Path.Combine(workspacePath, ".devcontainer", "vhost") + "/" + configFileName, vHostConfig);
        }
    }

    public static void EnableVhostConfigurations(bool debug = false)
    {
        // Create symlinks for each entry in the vhost directory(.devcontainer/vhost) to the Apache configuration directory
        var workspacePath = PathHelper.GetWorkspacePath();
        var vhostDir = Path.Combine(workspacePath, ".devcontainer", "vhost");
        
        if (!Directory.Exists(vhostDir))
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]Vhost directory does not exist: {vhostDir}[/]");
            }

            return;
        }
        
        var apacheVhostDir = "/etc/apache2/sites-available/";
        foreach (var file in Directory.GetFiles(vhostDir, "*.conf"))
        {
            var fileName = Path.GetFileName(file);
            var symlinkPath = Path.Combine(apacheVhostDir, fileName);

            if (File.Exists(symlinkPath))
            {
                if (debug)
                {
                    AnsiConsole.MarkupLine($"[yellow]Symlink {fileName} already exists. Skipping...[/]");
                }

                continue;
            }

            try
            {
                File.CreateSymbolicLink(symlinkPath, file);

                if (debug)
                {
                    AnsiConsole.MarkupLine($"[yellow]Creating symlink: {symlinkPath} -> {file}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to create symlink for {fileName}: {ex.Message}[/]");
            }

            // Enable the site configuration
            ApacheHelper.EnableVhost(Path.GetFileNameWithoutExtension(file), debug);
        }

        // Make sure the following line is in the Apache configuration file:
        var apacheConfigFile = "/etc/apache2/apache2.conf";
        if (!File.Exists(apacheConfigFile))
        {
            AnsiConsole.MarkupLine($"[red]Apache configuration file does not exist: {apacheConfigFile}[/]");
            return;
        }
        
        var configContent = File.ReadAllText(apacheConfigFile);
        if (!configContent.Contains("IncludeOptional /etc/apache2/sites-enabled/*.conf"))
        {
            configContent += "\nIncludeOptional /etc/apache2/sites-enabled/*.conf\n";
            File.WriteAllText(apacheConfigFile, configContent);

            if (debug)
            {
                AnsiConsole.MarkupLine($"[green]Added IncludeOptional directive to {apacheConfigFile}[/]");
            }
        }
        else if (debug)
        {
            AnsiConsole.MarkupLine($"[yellow]IncludeOptional directive already exists in {apacheConfigFile}[/]");
        }
        
        // Make sure that the default config is disabled
        ApacheHelper.DisableVhost("000-default", debug);
    }
}
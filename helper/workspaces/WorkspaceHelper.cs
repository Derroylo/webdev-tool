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
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]No workspaces defined in the config.[/]");
            }

            return true;
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
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        var workspaces = new List<string>();

        // Add the main project as vhost too
        var composeFile = Path.Combine(PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer()), ".devcontainer", "docker-compose.yml");
        var services = DockerComposeHelper.GetServices(composeFile);
        var devContainerService = services.ContainsKey("devcontainer") ? services["devcontainer"] : null;

        if (null != devContainerService)
        {
            var proxyDomain = devContainerService.ContainsKey("proxy.subdomain") ? devContainerService["proxy.subdomain"] : "";

            var mainWorkspace = new WorkspaceEntryConfiguration()
            {
                SubDomain = proxyDomain,
                DocRoot = "public"
            };
            
            CreateVhostWorkspace(mainWorkspace, true);
        }
        
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
            if (workspace.Value.DisableWeb)
            {
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

    private static void CreateVhostWorkspace(WorkspaceEntryConfiguration workspace, bool isMainWorkspace = false)
    {
        var vHostConfig = """
            <VirtualHost *:8080>
              ServerName #WSSUDOMAIN#.#SUBDOMAIN#.#DOMAIN#

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
        
        vHostConfig = vHostConfig.Replace("#WSSUDOMAIN#", workspace.SubDomain);
        vHostConfig = vHostConfig.Replace("#SUBDOMAIN#", GeneralConfig.Proxy.Subdomain);
        vHostConfig = vHostConfig.Replace("#DOMAIN#", GeneralConfig.Proxy.Domain);

        if (!isMainWorkspace)
        {
            vHostConfig = vHostConfig.Replace("#DOCROOT#", Path.Combine("/var/www/html/", GeneralConfig.WorkspaceFolder, workspace.Folder, workspace.DocRoot));
        }
        else
        {
            vHostConfig = vHostConfig.Replace("#DOCROOT#", Path.Combine("/var/www/html/", workspace.DocRoot));
        }

        var workspacePath = PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer());

        if (!Directory.Exists(Path.Combine(workspacePath, ".devcontainer", "vhost")))
        {
            Directory.CreateDirectory(Path.Combine(workspacePath, ".devcontainer", "vhost"));
        }
        
        File.WriteAllText(Path.Combine(workspacePath, ".devcontainer", "vhost") + "/" + workspace.SubDomain + ".conf", vHostConfig);
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
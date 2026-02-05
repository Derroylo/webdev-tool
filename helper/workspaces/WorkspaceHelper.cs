using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.apache;
using WebDev.Tool.Helper.git;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Helper.Workspaces;

public class WorkspaceHelper(
    WorkspacesConfig _workspacesConfig,
    GeneralConfig _generalConfig,
    IDebugOutputHelper _debugOutputHelper,
    IGitHelper _gitHelper,
    IApacheHelper _apacheHelper,
    IPathHelper _pathHelper,
    IEnvironmentHelper _environmentHelper
) : IWorkspaceHelper
{
    public bool ValidateWorkspaces()
    {
        if (_workspacesConfig.Workspaces.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]At least one workspace needs to be defined in the config.[/]");

            return false;
        }

        _debugOutputHelper.WriteInfoOutput("Validating workspaces", this);

        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in _workspacesConfig.Workspaces)
        {
            if (workspace.Key == "main")
            {
                _debugOutputHelper.WriteInfoOutput("Skipping main workspace", this);
                continue;
            }
            
            _debugOutputHelper.WriteInfoOutput("Validating workspace: " + workspace.Key, this);
            
            var workspaceFolder = "";

            if (workspace.Value.Folder != "" && (workspace.Value.Folder.StartsWith("./") || workspace.Value.Folder.StartsWith("../")))
            {
                workspaceFolder = workspace.Value.Folder + "/";
            }
            else
            {
                if (workspace.Value.Folder == "") {
                    workspace.Value.Folder = workspace.Key;
                }

                workspaceFolder = _generalConfig.WorkspaceFolder + "/" + workspace.Value.Folder + "/";
            }

            _debugOutputHelper.WriteInfoOutput("Workspace folder: " + workspaceFolder, this);

            if (!Directory.Exists(workspaceFolder) && workspace.Value.Repository == "")
            {
                AnsiConsole.MarkupLine("[red]Workspace: [bold yellow]{0}[/] is [red]invalid[/][/]", workspace.Key);
                AnsiConsole.MarkupLine("Reason: Workspace folder does not exist and no Repository was given: {0}", workspaceFolder);
                
                return false;
            }
            
            if (!Directory.Exists(workspaceFolder) && !string.IsNullOrEmpty(workspace.Value.Repository))
            {
                AnsiConsole.MarkupLine("Cloning repository {0} into {1}", workspace.Value.Repository, workspaceFolder);
                
                if (!_gitHelper.CloneRepository(workspace.Value.Repository, workspaceFolder))
                {
                    AnsiConsole.MarkupLine($"[red]Failed to clone repository:[/] {workspace.Value.Repository}");
                    
                    return false;
                }
            
                AnsiConsole.MarkupLine("[green]Repository cloned successfully![/]");

                if (!string.IsNullOrEmpty(workspace.Value.Branch))
                {
                    if (!_gitHelper.CheckoutBranch(workspaceFolder, workspace.Value.Branch))
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
            
            _debugOutputHelper.WriteInfoOutput("Workspace: " + workspace.Key + " is valid", this);
        }

        return true;
    }
    
    public bool PrepareWorkspaces()
    {
        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        var workspaces = new List<string>();
       
        if (_workspacesConfig.Workspaces.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No workspaces defined in the config.[/]");

            return false;
        }
        
        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in _workspacesConfig.Workspaces)
        {
            _debugOutputHelper.WriteInfoOutput("Preparing workspace: " + workspace.Key, this);
            
            if (workspace.Value.DisableWeb)
            {
                _debugOutputHelper.WriteInfoOutput("Workspace: " + workspace.Key + " is disabled", this);
                
                continue;
            }

            if (workspace.Key == "main")
            {
                // Add www subdomain to the main workspace, otherwise the vhost config will not be generated
                if (!workspace.Value.SubDomains.Contains("www"))
                {
                    workspace.Value.SubDomains.Add("www");
                }

                _debugOutputHelper.WriteInfoOutput("Creating vhost workspace for main workspace", this);

                CreateVhostWorkspace(workspace.Value, true);
                
                continue;
            }
            
            if (workspace.Value.Folder != "" &&
                (workspace.Value.Folder.StartsWith("./") || workspace.Value.Folder.StartsWith("../")))
            {
                // Not supported yet
                _debugOutputHelper.WriteWarningOutput("Using relative folder path for workspace: " + workspace.Key + " is not supported yet", this);
                // TODO: Create a symlink from the given folder to the workspace folder, otherwise the source code will not be available in the container
            }
            
            var workspaceFolder = _generalConfig.WorkspaceFolder + "/" + workspace.Value.Folder + "/";
           
            if (workspace.Value.Mode == WorkspaceMode.DevContainer)
            {
                _debugOutputHelper.WriteInfoOutput("Adding workspace: " + workspace.Key + " to the list of workspaces to start", this);
                
                workspaces.Add(workspaceFolder);
            }
            else
            {
                _debugOutputHelper.WriteInfoOutput("Creating vhost workspace for workspace: " + workspace.Key, this);
                
                CreateVhostWorkspace(workspace.Value);
            }
        }

        if (workspaces.Count > 0)
        {
            _debugOutputHelper.WriteInfoOutput("Writing workspaces to start to file: " + applicationDir + ".workspaces_start: " + string.Join(", ", workspaces), this);
            
            File.WriteAllLines(applicationDir + ".workspaces_start", workspaces);
        }
        
        return true;
    }

    private void CreateVhostWorkspace(WorkspaceEntryConfiguration workspace, bool isMainWorkspace = false, bool isWwwSubdomain = false)
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
        
        vHostConfig = vHostConfig.Replace("#SUBDOMAIN#", _generalConfig.Proxy.SubDomain);
        vHostConfig = vHostConfig.Replace("#DOMAIN#", _generalConfig.Proxy.Domain);

        foreach (var subDomain in workspace.SubDomains)
        {
            var configFileName = subDomain + ".conf";

            if (!isMainWorkspace)
            {
                vHostConfig = vHostConfig.Replace("#WSSUDOMAIN#", subDomain + ".");
                vHostConfig = vHostConfig.Replace("#DOCROOT#", Path.Combine("/var/www/html/", _generalConfig.WorkspaceFolder, workspace.Folder, workspace.DocRoot));
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

            var workspacePath = _pathHelper.GetWorkspacePath(_environmentHelper.IsRunningInDevContainer());

            if (!Directory.Exists(Path.Combine(workspacePath, ".devcontainer", "vhost")))
            {
                _debugOutputHelper.WriteInfoOutput("Creating vhost directory: " + Path.Combine(workspacePath, ".devcontainer", "vhost"), this);
                
                Directory.CreateDirectory(Path.Combine(workspacePath, ".devcontainer", "vhost"));
            }
            
            _debugOutputHelper.WriteInfoOutput("Writing vhost config file: " + Path.Combine(workspacePath, ".devcontainer", "vhost") + "/" + configFileName + ": " + vHostConfig, this);
            
            File.WriteAllText(Path.Combine(workspacePath, ".devcontainer", "vhost") + "/" + configFileName, vHostConfig);
        }
    }

    public void EnableVhostConfigurations()
    {
        // Create symlinks for each entry in the vhost directory(.devcontainer/vhost) to the Apache configuration directory
        var workspacePath = _pathHelper.GetWorkspacePath();
        var vhostDir = Path.Combine(workspacePath, ".devcontainer", "vhost");
        
        if (!Directory.Exists(vhostDir))
        {
            _debugOutputHelper.WriteErrorOutput("Vhost directory does not exist: " + vhostDir, this);

            return;
        }
        
        var apacheVhostDir = "/etc/apache2/sites-available/";
        foreach (var file in Directory.GetFiles(vhostDir, "*.conf"))
        {
            var fileName = Path.GetFileName(file);
            var symlinkPath = Path.Combine(apacheVhostDir, fileName);

            if (File.Exists(symlinkPath))
            {
                _debugOutputHelper.WriteWarningOutput("Symlink " + fileName + " already exists. Skipping...", this);

                continue;
            }

            try
            {
                File.CreateSymbolicLink(symlinkPath, file);

                _debugOutputHelper.WriteInfoOutput("Creating symlink: " + symlinkPath + " -> " + file, this);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to create symlink for {fileName}: {ex.Message}[/]");
            }

            // Enable the site configuration
            _apacheHelper.EnableVhost(Path.GetFileNameWithoutExtension(file));
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

            _debugOutputHelper.WriteInfoOutput("Added IncludeOptional directive to " + apacheConfigFile, this);
        }
        else
        {
            _debugOutputHelper.WriteWarningOutput("IncludeOptional directive already exists in " + apacheConfigFile, this);
        }
        
        // Make sure that the default config is disabled
        _apacheHelper.DisableVhost("000-default");
    }
}
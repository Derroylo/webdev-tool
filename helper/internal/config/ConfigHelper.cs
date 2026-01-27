using System;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Helper.Internal.Config
{
    public class ConfigHelper(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): IConfigHelper
    {
        protected static Configuration appConfig;

        private bool configFileExists = false;

        public bool ConfigFileExists => configFileExists;

        public bool ConfigUpdated { get; set; }

        private bool configFileValid = false;

        public bool IsConfigFileValid => configFileValid;

        public bool IsConfigFileLoaded => appConfig != null && configFileValid && configFileExists;

        public void ReadConfigFile(bool rethrowParseException = false)
        {
            // Reset everything
            configFileExists = false;
            configFileValid = false;
            appConfig = null;

            var configFileWithPath = GetConfigFileWithPath();

            if (!File.Exists(configFileWithPath)) {
                configFileExists = false;
                configFileValid = false;

                // Init the app config with default data if there is no config file present
                appConfig = new Configuration();

                // Add the default workspace
                appConfig.Workspaces.Add("main", new WorkspaceEntryConfiguration {
                    Name = "Main Workspace",
                    Description = "This is the default workspace."
                });
                
                return;
            }

            configFileExists = true;

            try {
                appConfig = ConfigReader.ReadConfigFile(configFileWithPath);
            } catch {
                configFileValid = false;

                if (rethrowParseException) {
                    // Init the app config with default data if the existing config file is invalid
                    appConfig = new Configuration();

                    // Add the default workspace
                    appConfig.Workspaces.Add("main", new WorkspaceEntryConfiguration {
                        Name = "Main Workspace",
                        Description = "This is the default workspace."
                    });
                    
                    throw;
                }
            }

            if (appConfig == null) {
                configFileValid = false;

                // Init the app config with default data if the existing config file is invalid
                appConfig = new Configuration();
                
                // Add the default workspace
                appConfig.Workspaces.Add("main", new WorkspaceEntryConfiguration {
                    Name = "Main Workspace",
                    Description = "This is the default workspace."
                });

                return;
            }
            
            configFileValid = true;
            
            // Make sure the default workspace is always present
            if (!appConfig.Workspaces.ContainsKey("main")) {
                appConfig.Workspaces.Add("main", new WorkspaceEntryConfiguration {
                    Name = "Main Workspace",
                    Description = "This is the default workspace."
                });
            }
        }

        public void SaveConfigFile()
        {
            var configFileWithPath = GetConfigFileWithPath();

            try {
                ConfigWriter.WriteConfigFile(configFileWithPath, appConfig);
            } catch {
                throw;
            }
        }
        
        private string GetConfigFileWithPath()
        {
            return _pathHelper.GetWorkspacePath(_environmentHelper.IsRunningInDevContainer()) + "/.devcontainer/webdev.yml";
        }
    }
}
using System;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config
{
    internal class ConfigHelper
    {
        protected static Configuration appConfig;

        private static bool configFileExists = false;

        public static bool ConfigFileExists => configFileExists;

        public static bool ConfigUpdated { get; set; } = false;

        private static bool configFileValid = false;

        public static bool IsConfigFileValid => configFileValid;

        public static bool IsConfigFileLoaded => appConfig != null && configFileValid && configFileExists;

        public static void ReadConfigFile(bool rethrowParseException = false)
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

                    throw;
                }
            }

            if (appConfig == null) {
                configFileValid = false;

                // Init the app config with default data if the existing config file is invalid
                appConfig = new Configuration();
            } else {
                configFileValid = true;
            }
        }

        public static string GetConfigFileWithPath()
        {
            var workspacePath = Environment.GetEnvironmentVariable("WEBDEV_WORKSPACE_FOLDER");

            if (string.IsNullOrEmpty(workspacePath)) {
                workspacePath = Directory.GetCurrentDirectory();
            }

            return workspacePath + "/.devcontainer/webdev.yml";
        }
    }
}
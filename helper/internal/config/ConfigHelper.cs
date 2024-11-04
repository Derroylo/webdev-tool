using System;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config
{
    class ConfigHelper
    {
        protected static Configuration appConfig;

        private static bool configFileExists = false;

        public static bool ConfigFileExists { get { return configFileExists; } }

        private static bool configUpdated = false;

        public static bool ConfigUpdated { get { return configUpdated; } set { configUpdated = value; }}

        private static bool configFileValid = false;

        public static bool IsConfigFileValid { get { return configFileValid; } }

        public static bool IsConfigFileLoaded { get { return appConfig != null && configFileValid && configFileExists; } }

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

            if (workspacePath == null || workspacePath == string.Empty) {
                workspacePath = Directory.GetCurrentDirectory();
            }

            return workspacePath + "/.devcontainer/devcontainer.json";
        }
    }
}
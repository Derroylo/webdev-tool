using System;
using System.Collections.Generic;
using System.IO;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Helper.Php
{
    public class PhpIniHelper(IPhpVersionHelper _phpVersionHelper, PhpConfig _phpConfig, IDebugOutputHelper _debugOutputHelper, ExecCommand _execCommand) : IPhpIniHelper
    {
        public string GetPhpIniPath()
        {
            return _execCommand.Exec("php -i | grep 'Configuration File'");
        }

        public void UpdatePhpIniFiles()
        {
            string currentPhpVersion = _phpVersionHelper.GetCurrentPhpVersion();

            _debugOutputHelper.WriteInfoOutput("Active PHP Version " + currentPhpVersion, this);

            _debugOutputHelper.WriteInfoOutput("Generating custom.ini....", this);

            var customIniFiles = GenerateCustomIniFilesFromConfig();

            if (customIniFiles.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]No custom settings found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Done[/]");
           
            if (customIniFiles.TryGetValue("cli", out string customIniFileCli)) {
                AnsiConsole.Write("Copy the custom_cli.ini to the target directory....");

                string targetFile = "/etc/php/" + currentPhpVersion + "/cli/conf.d/custom.ini";
                
                _debugOutputHelper.WriteInfoOutput("Copying from \"" + customIniFileCli + "\" to \"" + targetFile + "\"", this);
                
                _execCommand.Exec("sudo cp " + customIniFileCli + " " + targetFile);

                AnsiConsole.MarkupLine("[green1]Done[/]");
            }

            if (customIniFiles.TryGetValue("web", out string customIniFileWeb)) {
                AnsiConsole.Write("Copy the custom_web.ini to the target directory....");

                string targetFile = "/etc/php/" + currentPhpVersion + "/apache2/conf.d/custom.ini";
                
                _debugOutputHelper.WriteInfoOutput("Copying from \"" + customIniFileWeb + "\" to \"" + targetFile + "\"", this);
                
                _execCommand.Exec("sudo cp " + customIniFileWeb + " " + targetFile);

                targetFile = "/etc/php/" + currentPhpVersion + "/apache2/conf.d/custom.ini";
                
                _debugOutputHelper.WriteInfoOutput("Copying from \"" + customIniFileWeb + "\" to \"" + targetFile + "\"", this);
                
                _execCommand.Exec("sudo cp " + customIniFileWeb + " " + targetFile);

                AnsiConsole.MarkupLine("[green1]Done[/]");

                AnsiConsole.Write("Restart apache....");

                // Restart apache when changes has been made to the web php.ini files (doing this here as this one is used be restore php command)
                _execCommand.Exec("apachectl stop");
                _execCommand.Exec("apachectl start");

                AnsiConsole.MarkupLine("[green1]Done[/]");
            }
        }

        public void AddSettingToPhpIni(string name, string value, bool setForWeb = false, bool setForCLI = false)
        {
            if (name == null || name.Length <= 3 || value == null || value.Length < 1) {
                AnsiConsole.MarkupLine("[red]Invalid name and/or value[/]");

                return;
            }

            if (!setForWeb && !setForCLI) {
                if (_phpConfig.Config.ContainsKey(name)) {
                    _phpConfig.Config[name] = value;
                } else {
                    _phpConfig.Config.Add(name, value);
                }
            }

            if (setForWeb) {
                if (_phpConfig.ConfigWeb.ContainsKey(name)) {
                    _phpConfig.ConfigWeb[name] = value;
                } else {
                    _phpConfig.ConfigWeb.Add(name, value);
                }
            }

            if (setForCLI) {
                if (_phpConfig.ConfigCli.ContainsKey(name)) {
                    _phpConfig.ConfigCli[name] = value;
                } else {
                    _phpConfig.ConfigCli.Add(name, value);
                }
            }

            // Manually set that the config has been updated
            _phpConfig.ConfigUpdated = true;

            // Update the ini files that are being used by apache and cli
            UpdatePhpIniFiles();
        }

        private Dictionary<string, string> GenerateCustomIniFilesFromConfig()
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            var iniSettingsFolder = applicationDir + "/php";
            var customFilesWithPath = new Dictionary<string, string>();

            if (!Directory.Exists(iniSettingsFolder)) {
                Directory.CreateDirectory(iniSettingsFolder);
            }

            if (_phpConfig.Config.Count == 0 && _phpConfig.ConfigWeb.Count == 0 && _phpConfig.ConfigCli.Count == 0) {
                return customFilesWithPath;
            }

            // Combine config and configWeb to a custom.ini for web
            var configWeb = new Dictionary<string, string>(_phpConfig.ConfigWeb);
            var configGlobal = new Dictionary<string, string>(_phpConfig.Config);
            var combinedConfig = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> item in configGlobal) {
                if (configWeb.TryGetValue(item.Key, out string configWebValue)) {
                    combinedConfig.Add(item.Key, configWebValue);
                    configWeb.Remove(item.Key);
                } else {
                    combinedConfig.Add(item.Key, item.Value);
                }
            }

            if (configWeb.Count > 0) {
                foreach (KeyValuePair<string, string> item in configWeb) {
                    combinedConfig.Add(item.Key, item.Value);
                }
            }

            string customIniContent = string.Empty;

            foreach (KeyValuePair<string, string> item in combinedConfig) {
                customIniContent += item.Key + " = " + item.Value + "\n";
            }

            File.WriteAllText(iniSettingsFolder + "/custom_web.ini", customIniContent);
            customFilesWithPath.Add("web", iniSettingsFolder + "/custom_web.ini");

            // Combine config and configCLI to a custom.ini for CLI
            var configCLI = new Dictionary<string, string>(_phpConfig.ConfigCli);
            configGlobal = new Dictionary<string, string>(_phpConfig.Config);
            combinedConfig = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> item in configGlobal) {
                if (configCLI.TryGetValue(item.Key, out string configCLIValue)) {
                    combinedConfig.Add(item.Key, configCLIValue);
                    configCLI.Remove(item.Key);
                } else {
                    combinedConfig.Add(item.Key, item.Value);
                }
            }

            if (configCLI.Count > 0) {
                foreach (KeyValuePair<string, string> item in configCLI) {
                    combinedConfig.Add(item.Key, item.Value);
                }
            }

            customIniContent = string.Empty;

            foreach (KeyValuePair<string, string> item in combinedConfig) {
                customIniContent += item.Key + " = " + item.Value + "\n";
            }

            File.WriteAllText(iniSettingsFolder + "/custom_cli.ini", customIniContent);
            customFilesWithPath.Add("cli", iniSettingsFolder + "/custom_cli.ini");

            return customFilesWithPath;
        }
    }
}

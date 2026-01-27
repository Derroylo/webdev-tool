// helper/internal/TranslationHelper.cs
using System;
using System.Collections.Generic;
using System.IO;
using WebDev.Tool.Helper;
using YamlDotNet.Serialization;

namespace WebDev.Tool.Helper.Internal
{
    public class TranslationHelper : ITranslationHelper
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IPathHelper _pathHelper;
        private Dictionary<string, string> _translations = new();
        private string _currentLanguage = "en";
        private bool _loaded = false;

        public TranslationHelper(IAppSettingsHelper appSettingsHelper, IPathHelper pathHelper)
        {
            _appSettingsHelper = appSettingsHelper;
            _pathHelper = pathHelper;
            LoadTranslations();
        }

        public void LoadTranslations()
        {
            _translations.Clear();
            _loaded = false;

            // Try to get language from config or environment
            _currentLanguage = _appSettingsHelper.AppSettings.Language ?? "en";

            // Look in multiple locations:
            // 1. App directory (for bundled translations)
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var appTranslationsDir = Path.Combine(appDir, "translations");
            
            // 2. User home directory (for user-provided translations)
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var userTranslationsDir = Path.Combine(userHome, ".webdev", "translations");
            
            // 3. Workspace directory (for project-specific translations)
            var workspaceTranslationsDir = Path.Combine(_pathHelper.GetWorkspacePath(false), ".webdev", "translations");

            // Load translations in order: workspace > user > app (workspace overrides user, user overrides app)
            LoadFromDirectory(appTranslationsDir);
            LoadFromDirectory(userTranslationsDir);
            LoadFromDirectory(workspaceTranslationsDir);

            _loaded = true;
        }

        private void LoadFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            // Load default language file first
            var defaultFile = Path.Combine(directory, "en.yml");
            if (File.Exists(defaultFile))
            {
                LoadTranslationFile(defaultFile);
            }

            // Then load language-specific file (overrides defaults)
            if (_currentLanguage != "en")
            {
                var langFile = Path.Combine(directory, $"{_currentLanguage}.yml");
                if (File.Exists(langFile))
                {
                    LoadTranslationFile(langFile);
                }
            }
        }

        private void LoadTranslationFile(string filePath)
        {
            try
            {
                var yaml = File.ReadAllText(filePath);
                var deserializer = new DeserializerBuilder()
                    .Build();
                
                var yamlObject = deserializer.Deserialize<Dictionary<object, object>>(yaml);
                
                // Flatten nested YAML structure to dot-notation keys
                FlattenYamlObject(yamlObject, _translations);
            }
            catch
            {
                // Silently fail - fall back to default/English
            }
        }

        private void FlattenYamlObject(Dictionary<object, object> yamlObject, Dictionary<string, string> target, string prefix = "")
        {
            foreach (var kvp in yamlObject)
            {
                var keyName = kvp.Key?.ToString() ?? "";
                var key = string.IsNullOrEmpty(prefix) ? keyName : $"{prefix}.{keyName}";
                
                if (kvp.Value is Dictionary<object, object> nestedDict)
                {
                    // Recursively flatten nested dictionaries
                    FlattenYamlObject(nestedDict, target, key);
                }
                else if (kvp.Value is string stringValue)
                {
                    // Store string values with dot-notation key
                    target[key] = stringValue;
                }
                else if (kvp.Value != null)
                {
                    // Convert other types to string (handles numbers, booleans, etc.)
                    target[key] = kvp.Value.ToString();
                }
            }
        }

        public string GetString(string key, params object[] args)
        {
            if (!_loaded)
                LoadTranslations();

            if (_translations.TryGetValue(key, out var value))
            {
                if (args.Length > 0)
                    return string.Format(value, args);
                return value;
            }

            // Fallback to key if translation not found
            return key;
        }

        public void SetLanguage(string languageCode)
        {
            _currentLanguage = languageCode.ToLower();
            LoadTranslations();
        }

        public string CurrentLanguage => _currentLanguage;
    }
}
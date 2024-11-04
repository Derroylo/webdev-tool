using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using System.Text.Json;
using System;

namespace WebDev.Tool.Helper.Internal.Config
{
    class ConfigReader
    {
        public static Configuration ReadConfigFile(string configFile)
        {
            // Read the JSON file
            string jsonFilePath = configFile;
            string jsonString = File.ReadAllText(jsonFilePath);

            // Parse the JSON string with comments allowed
            var options = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip
            };

            using JsonDocument doc = JsonDocument.Parse(jsonString, options);

            // Navigate to the specific key in the "customizations" section
            JsonElement customizationsElement = doc.RootElement.GetProperty("customizations");
            bool elementFound = customizationsElement.TryGetProperty("webdev", out JsonElement webDevElement);

            if (!elementFound) {
                return new Configuration();
            }

            // Deserialize the JSON element into a C# object
            return JsonSerializer.Deserialize<Configuration>(webDevElement.GetRawText(), new JsonSerializerOptions{PropertyNameCaseInsensitive = true});
        }
    }
}
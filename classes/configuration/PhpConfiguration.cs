using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration
{
    internal class PhpConfiguration
    {
        public string Version { get; set; } = "8.2";

        public Dictionary<string, string> Config { get; set; } = new();

        public Dictionary<string, string> ConfigWeb { get; set; } = new();

        public Dictionary<string, string> ConfigCLI { get; set; } = new();

        public List<string> Packages { get; set; } = new();
    }
}
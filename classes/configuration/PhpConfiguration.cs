using System.Collections.Generic;
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration
{
    public class PhpConfiguration
    {
        [DefaultValue("8.3")]
        public string Version { get; set; } = "8.3";

        public Dictionary<string, string> Config { get; set; } = new();

        public Dictionary<string, string> ConfigWeb { get; set; } = new();

        public Dictionary<string, string> ConfigCLI { get; set; } = new();

        public List<string> Packages { get; set; } = new();
    }
}
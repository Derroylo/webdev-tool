using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration
{
    internal class EnvironmentConfiguration
    {
        public string Default { get; set; } = "";
        
        public Dictionary<string, EnvironmentSettingConfiguration> Settings { get; set; } = new();
    }
}
using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration
{
    public class EnvironmentSettingConfiguration
    {
        public string Name { get; set; } = "";
        
        public string Description { get; set; } = "";
        
        public List<string> SetUp { get; set; } = new();
        
        public List<string> TearDown { get; set; } = new();
    }
}
using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration
{
    internal class Configuration
    {
        public ConfigConfiguration Config { get; set; } = new();

        public PhpConfiguration Php { get; set; } = new();

        public EnvironmentConfiguration Environment { get; set; } = new();

        public Dictionary<string, SecretConfiguration> Secrets { get; set; } = new();
        
        public Dictionary<string, TaskEntryConfiguration> Tasks { get; set; } = new();
        
        public NodeJsConfiguration Nodejs { get; set; } = new();

        public ServiceConfiguration Services { get; set; } = new();

        public ShellScriptsConfiguration ShellScripts { get; set; } = new();
    }
}
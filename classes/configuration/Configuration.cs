using System.Collections.Generic;
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration
{
    internal class Configuration
    {
        [DefaultValue(3)]
        public int SchemaVersion { get; set; } = 3;
        
        public ConfigConfiguration Config { get; set; } = new();

        public ShellScriptsConfiguration ShellScripts { get; set; } = new();
        
        public PhpConfiguration Php { get; set; } = new();

        public NodeJsConfiguration Nodejs { get; set; } = new();
        
        public Dictionary<string, ServiceEntryConfiguration> Services { get; set; } = new();
        
        public EnvironmentConfiguration Environment { get; set; } = new();

        public Dictionary<string, SecretConfiguration> Secrets { get; set; } = new();
        
        public Dictionary<string, TaskEntryConfiguration> Tasks { get; set; } = new();
        
        public Dictionary<string, WorkspaceEntryConfiguration> Workspaces { get; set; } = new();

        public Dictionary<string, TestEntryConfiguration> Tests { get; set; } = new();
    }
}
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration
{
    internal class ConfigConfiguration
    {
        [DefaultValue(false)]
        public bool AllowPreReleases { get; set; } = false;
        
        public ProxyConfiguration Proxy { get; set; } = new();
        
        [DefaultValue("workspaces")]
        public string WorkspaceFolder { get; set; } = "workspaces";
    }
}
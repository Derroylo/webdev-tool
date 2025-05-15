namespace WebDev.Tool.Classes.Configuration
{
    internal class ConfigConfiguration
    {
        public bool AllowPreReleases { get; set; } = false;
        
        public ProxyConfiguration Proxy { get; set; } = new();
    }
}
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration;

public class ProxyConfiguration
{
    [DefaultValue("dev.localhost")]
    public string Domain { get; set; } = "dev.localhost";
    
    [DefaultValue("devcontainer")]
    public string SubDomain { get; set; } = "devcontainer";
}
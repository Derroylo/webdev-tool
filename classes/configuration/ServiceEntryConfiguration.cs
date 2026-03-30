using System.Collections.Generic;
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration;

public class ServiceEntryConfiguration
{
    public string Name { get; set; } = "";
    
    public string Description { get; set; } = "";

    public string Category { get; set; } = "";
    
    [DefaultValue(false)]
    public bool Active { get; set; } = false;
    
    public int Port { get; set; } = 8080;
    
    public string SubDomain { get; set; } = "";

    public List<ServiceLinkEntryConfiguration> Links { get; set; } = new();
}
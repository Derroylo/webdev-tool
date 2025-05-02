using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration;

internal class SecretConfiguration
{
    public string SourceType { get; set; } = "";
        
    public string SourceName { get; set; } = "";
    
    public string TargetType { get; set; } = "";
    
    public string ProjectId { get; set; } = "";
}
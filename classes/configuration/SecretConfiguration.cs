using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration;

internal class SecretConfiguration
{
    public string Group { get; set; } = "";
        
    public SecretSourceConfiguration Source { get; set; } = new();
    
    public SecretTargetConfiguration Target { get; set; } = new();
}
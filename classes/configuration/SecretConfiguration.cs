using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration;

internal class SecretConfiguration
{
    public SecretSourceConfiguration Source { get; set; } = new();
    
    public SecretTargetConfiguration Target { get; set; } = new();

    public string MissingMessage { get; set; } = "";
}
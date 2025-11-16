using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration;

internal class SecretTargetConfiguration
{
    public string File { get; set; } = "";
    
    public string EnvVar { get; set; } = "";

    public List<string> ExpectedVars { get; set; } = new();

    public List<string> ExpectedSecrets { get; set; } = new();
}
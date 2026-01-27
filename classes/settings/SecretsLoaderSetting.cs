using System.Collections.Generic;

namespace WebDev.Tool.Classes.Settings;

public class SecretsLoaderSetting
{
    public string Type { get; set; } = null;
    
    public Dictionary<string, string> Settings { get; set; } = new ();
}
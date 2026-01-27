namespace WebDev.Tool.Classes.Settings;

public class AppSettings
{
    public ProxySetting Proxy { get; set; } = new();
    
    public SecretsLoaderSetting SecretsLoader { get; set; } = new();

    public string Language { get; set; } = "en";
}
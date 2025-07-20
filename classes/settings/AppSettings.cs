namespace WebDev.Tool.Classes.Settings;

internal class AppSettings
{
    public ProxySetting Proxy { get; set; } = new();
    
    public SecretsLoaderSetting SecretsLoader { get; set; } = new();
}
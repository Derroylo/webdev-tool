using System;
using System.IO;
using Spectre.Console;
using WebDev.Tool.Classes.Settings;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WebDev.Tool.Helper.Internal;

public class AppSettingsHelper : IAppSettingsHelper
{
    public AppSettings AppSettings { get; private set; } = new AppSettings();
    
    public void LoadAppSettings(bool rethrowParseException = false)
    {
        var appDir= AppDomain.CurrentDomain.BaseDirectory;

        if (!File.Exists(Path.Combine(appDir, "appsettings.yml")))
        {
            return;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            AppSettings = deserializer.Deserialize<AppSettings>(File.ReadAllText(Path.Combine(appDir, "appsettings.yml")));
        }
        catch
        {
            if (rethrowParseException)
            {
                throw;
            }
        }
    }

    public void SaveAppSettings()
    {
        var appDir= AppDomain.CurrentDomain.BaseDirectory;
        
        var serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitNull)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var stringResult = serializer.Serialize(AppSettings);

        File.WriteAllText(Path.Combine(appDir, "appsettings.yml"), stringResult);
    }
}
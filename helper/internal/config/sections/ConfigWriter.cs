using System.IO;
using WebDev.Tool.Classes.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

internal class ConfigWriter
{
    public static void WriteConfigFile(string configFile, Configuration configuration)
    {
        var serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var tempConfig = new Configuration
        {
            Config = configuration.Config,
            ShellScripts = configuration.ShellScripts,
            Php = configuration.Php,
            Nodejs = configuration.Nodejs,
            Services = configuration.Services,
            Environment = configuration.Environment,
            Secrets = configuration.Secrets,
            Tasks = configuration.Tasks,
            Workspaces = configuration.Workspaces,
            Tests = configuration.Tests
        };
        
        if (tempConfig.Workspaces.Count == 1 && tempConfig.Workspaces.ContainsKey("main") && tempConfig.Workspaces["main"].DocRoot == "public") 
        {
            tempConfig.Workspaces = null;
        }
        
        if (tempConfig.ShellScripts.AdditionalDirectories.Count == 0)
        {
            tempConfig.ShellScripts = null;
        }
        
        if (tempConfig.Environment.Settings.Count == 0)
        {
            tempConfig.Environment = null;
        }
        
        if (tempConfig.Tasks.Count == 0)
        {
            tempConfig.Tasks = null;
        }
        
        if (tempConfig.Secrets.Count == 0)
        {
            tempConfig.Secrets = null;
        }
        
        if (tempConfig.Tests.Count == 0)
        {
            tempConfig.Tests = null;
        }
        
        var stringResult = serializer.Serialize(tempConfig);

        File.WriteAllText(configFile, stringResult);
    }
}
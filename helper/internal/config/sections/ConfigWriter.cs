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
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitNull)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var stringResult = serializer.Serialize(configuration);

        File.WriteAllText(configFile, stringResult);
    }
}
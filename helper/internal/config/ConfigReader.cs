using System.IO;
using WebDev.Tool.Classes.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WebDev.Tool.Helper.Internal.Config
{
    internal class ConfigReader
    {
        public static Configuration ReadConfigFile(string configFile)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<Configuration>(File.ReadAllText(configFile));
        }
    }
}
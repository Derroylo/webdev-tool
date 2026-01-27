using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

public class SecretsConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
{
    public Dictionary<string, SecretConfiguration> Secrets
    {
        get => appConfig.Secrets;

        set
        {
            ConfigUpdated = true;
            
            appConfig.Secrets = value;
        }
    }
}
using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

internal class SecretsConfig: ConfigHelper
{
    public static Dictionary<string, SecretConfiguration> Secrets
    {
        get => appConfig.Secrets;

        set
        {
            ConfigUpdated = true;
            
            appConfig.Secrets = value;
        }
    }
}
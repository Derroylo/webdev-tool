using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    public class ServicesConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
    {
        public Dictionary<string, ServiceEntryConfiguration> Services
        {
            get => appConfig.Services;

            set {
                ConfigUpdated = true;

                appConfig.Services = value;
            }
        }
    }
}
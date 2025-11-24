using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    internal class ServicesConfig: ConfigHelper
    {
        public static Dictionary<string, ServiceEntryConfiguration> Services
        {
            get => appConfig.Services;

            set {
                ConfigUpdated = true;

                appConfig.Services = value;
            }
        }
    }
}
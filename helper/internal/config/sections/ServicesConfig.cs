using System.Collections.Generic;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    internal class ServicesConfig: ConfigHelper
    {
        public static string DockerComposeFile => appConfig.Services.File;

        public static List<string> ActiveServices
        {
            get => appConfig.Services.Active;

            set {
                ConfigUpdated = true;

                appConfig.Services.Active = value;
            }
        }
    }
}
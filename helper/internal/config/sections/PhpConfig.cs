using System.Collections.Generic;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    internal class PhpConfig: ConfigHelper
    {
        public static string PhpVersion
        {
            get => appConfig.Php.Version;

            set {
                if (appConfig.Php.Version != value) {
                    ConfigUpdated = true;
                }

                appConfig.Php.Version = value;
            }
        }

        public static Dictionary<string, string> Config => appConfig.Php.Config;

        public static Dictionary<string, string> ConfigWeb => appConfig.Php.ConfigWeb;

        public static Dictionary<string, string> ConfigCli => appConfig.Php.ConfigCLI;

        public static List<string> Packages => appConfig.Php.Packages;
    }
}
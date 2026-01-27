using System.Collections.Generic;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    public class PhpConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
    {
        public string PhpVersion
        {
            get => appConfig.Php.Version;

            set {
                if (appConfig.Php.Version != value) {
                    ConfigUpdated = true;
                }

                appConfig.Php.Version = value;
            }
        }

        public Dictionary<string, string> Config => appConfig.Php.Config;

        public Dictionary<string, string> ConfigWeb => appConfig.Php.ConfigWeb;

        public Dictionary<string, string> ConfigCli => appConfig.Php.ConfigCLI;

        public List<string> Packages => appConfig.Php.Packages;
    }
}
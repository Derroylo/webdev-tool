using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    internal class GeneralConfig: ConfigHelper
    {
        public static bool AllowPreReleases => appConfig.Config.AllowPreReleases;
        
        public static ProxyConfiguration Proxy => appConfig.Config.Proxy;

        public static string WorkspaceFolder
        {
            get => appConfig.Config.WorkspaceFolder;
            set
            {
                ConfigUpdated = true;
                appConfig.Config.WorkspaceFolder = value;
            }
        }
    }
}
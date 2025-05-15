using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    internal class GeneralConfig: ConfigHelper
    {
        public static bool AllowPreReleases => appConfig.Config.AllowPreReleases;
        
        public static ProxyConfiguration Proxy => appConfig.Config.Proxy;
    }
}
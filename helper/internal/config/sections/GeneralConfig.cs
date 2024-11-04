namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    class GeneralConfig: ConfigHelper
    {
        public static bool AllowPreReleases
        {
            get {
                return appConfig.Config.AllowPreReleases;
            }
        }
    }
}
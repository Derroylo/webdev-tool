namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    internal class GeneralConfig: ConfigHelper
    {
        public static bool AllowPreReleases => appConfig.Config.AllowPreReleases;
    }
}
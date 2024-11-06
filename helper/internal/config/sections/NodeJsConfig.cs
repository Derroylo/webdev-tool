namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    internal class NodeJsConfig: ConfigHelper
    {
        public static string NodeJsVersion
        {
            get => appConfig.Nodejs.Version;

            set {
                if (appConfig.Nodejs.Version != value) {
                    ConfigUpdated = true;
                }

                appConfig.Nodejs.Version = value;
            }
        }
    }
}
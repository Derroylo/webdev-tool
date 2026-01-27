namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    public class NodeJsConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
    {
        public string NodeJsVersion
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
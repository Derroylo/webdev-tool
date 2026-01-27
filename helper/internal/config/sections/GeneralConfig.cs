using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    public class GeneralConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
    {
        public bool AllowPreReleases => appConfig.Config.AllowPreReleases;
        
        public ProxyConfiguration Proxy => appConfig.Config.Proxy;

        public string WorkspaceFolder
        {
            get => appConfig.Config.WorkspaceFolder;
            set
            {
                ConfigUpdated = true;
                appConfig.Config.WorkspaceFolder = value;
            }
        }

        public string ComposeFileName
        {
            get => appConfig.Config.ComposeFileName;
            set
            {
                ConfigUpdated = true;
                appConfig.Config.ComposeFileName = value;
            }
        }
    }
}
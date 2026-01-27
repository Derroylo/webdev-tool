using System.Collections.Generic;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    public class ShellScriptConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
    {
        public List<string> AdditionalDirectories => appConfig.ShellScripts.AdditionalDirectories;
    }
}
using System.Collections.Generic;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    internal class ShellScriptConfig: ConfigHelper
    {
        public static List<string> AdditionalDirectories => appConfig.ShellScripts.AdditionalDirectories;
    }
}
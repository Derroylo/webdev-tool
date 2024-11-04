using System.Collections.Generic;

namespace WebDev.Tool.Helper.Internal.Config.Sections
{
    class ShellScriptConfig: ConfigHelper
    {
        public static List<string> AdditionalDirectories
        {
            get {
                return appConfig.ShellScripts.AdditionalDirectories;
            }
        }
    }
}
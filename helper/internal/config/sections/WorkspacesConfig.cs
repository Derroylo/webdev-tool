using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

internal class WorkspacesConfig: ConfigHelper
{
    public static Dictionary<string, WorkspaceEntryConfiguration> Workspaces
    {
        get => appConfig.Workspaces;

        set {
            ConfigUpdated = true;

            appConfig.Workspaces = value;
        }
    }
}
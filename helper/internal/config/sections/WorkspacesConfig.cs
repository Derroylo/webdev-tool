using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

public class WorkspacesConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
{
    public Dictionary<string, WorkspaceEntryConfiguration> Workspaces
    {
        get => appConfig.Workspaces;

        set {
            ConfigUpdated = true;

            appConfig.Workspaces = value;
        }
    }
}
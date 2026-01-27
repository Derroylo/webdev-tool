using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

public class TasksConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
{
    public Dictionary<string, TaskEntryConfiguration> Tasks
    {
        get => appConfig.Tasks;

        set {
            ConfigUpdated = true;

            appConfig.Tasks = value;
        }
    }
}
using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

internal class TasksConfig: ConfigHelper
{
    public static Dictionary<string, TaskEntryConfiguration> Tasks
    {
        get => appConfig.Tasks;

        set {
            ConfigUpdated = true;

            appConfig.Tasks = value;
        }
    }
}
using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

public class TestsConfig(IPathHelper _pathHelper, IEnvironmentHelper _environmentHelper): ConfigHelper(_pathHelper, _environmentHelper)
{
    public Dictionary<string, TestEntryConfiguration> Tests
    {
        get => appConfig.Tests;

        set {
            ConfigUpdated = true;

            appConfig.Tests = value;
        }
    }
}
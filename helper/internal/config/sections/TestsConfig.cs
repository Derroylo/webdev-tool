using System.Collections.Generic;
using WebDev.Tool.Classes.Configuration;

namespace WebDev.Tool.Helper.Internal.Config.Sections;

internal class TestsConfig: ConfigHelper
{
    public static Dictionary<string, TestEntryConfiguration> Tests
    {
        get => appConfig.Tests;

        set {
            ConfigUpdated = true;

            appConfig.Tests = value;
        }
    }
}
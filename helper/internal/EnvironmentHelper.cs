using System;

namespace WebDev.Tool.Helper.Internal;

internal static class EnvironmentHelper
{
    public static bool IsRunningInDevContainer()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEVCONTAINER"));
    }

    public static bool DisableProgramHeader()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBDEV_DISABLE_HEADER"));
    }
}
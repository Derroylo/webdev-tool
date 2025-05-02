using System;

namespace WebDev.Tool.Helper.Internal;

internal static class EnvironmentHelper
{
    public static bool IsRunningInDevContainer()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEVCONTAINER"));
    }
}
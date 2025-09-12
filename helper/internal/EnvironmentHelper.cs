using System;
using System.Dynamic;

namespace WebDev.Tool.Helper.Internal;

internal static class EnvironmentHelper
{
    public static bool IsRunningInDevContainer()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEVCONTAINER"));
    }

    private static bool ProgramHeaderDisabled = false;
    
    public static void DisableProgramHeader()
    {
        ProgramHeaderDisabled = true;
    }

    public static bool IsProgramHeaderDisabled()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBDEV_DISABLE_HEADER")))
        {
            return true;
        }

        return ProgramHeaderDisabled;
    }
}
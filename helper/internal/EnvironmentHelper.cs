using System;
using System.Dynamic;

namespace WebDev.Tool.Helper.Internal;

public class EnvironmentHelper : IEnvironmentHelper
{
    private bool ProgramHeaderDisabled = false;
    
    public bool IsRunningInDevContainer()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEVCONTAINER"));
    }
    
    public void DisableProgramHeader()
    {
        ProgramHeaderDisabled = true;
    }

    public bool IsProgramHeaderDisabled()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBDEV_DISABLE_HEADER")))
        {
            return true;
        }

        return ProgramHeaderDisabled;
    }
}
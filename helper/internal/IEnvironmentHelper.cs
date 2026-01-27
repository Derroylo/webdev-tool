namespace WebDev.Tool.Helper.Internal;

public interface IEnvironmentHelper
{
    bool IsRunningInDevContainer();
    void DisableProgramHeader();
    bool IsProgramHeaderDisabled();
}

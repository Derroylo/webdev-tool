namespace WebDev.Tool.Helper.Internal;

public interface IDebugOutputHelper
{
    void EnableDebugOutput();
    void DisableDebugOutput();
    bool IsDebugOutputEnabled();
    void WriteInfoOutput(string message, object sourceClass);
    void WriteWarningOutput(string message, object sourceClass);
    void WriteErrorOutput(string message, object sourceClass);
}

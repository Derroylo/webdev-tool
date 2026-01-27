namespace WebDev.Tool.Helper;

public interface IPtyHelper
{
    void ExecWithPty(string command, bool isInteractive = false, bool disableJobControl = false, string workingDirectory = "", System.Collections.Generic.Dictionary<string, string> additionalEnvVars = null);
}

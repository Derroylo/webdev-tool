namespace WebDev.Tool.Helper;

public interface IRestoreHelper
{
    void RestorePhpVersion();
    void RestorePhpIni();
    void RestoreNodeJsVersion();
    void RestoreEnvVariables();
}

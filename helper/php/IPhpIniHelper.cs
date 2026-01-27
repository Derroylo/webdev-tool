namespace WebDev.Tool.Helper.Php;

public interface IPhpIniHelper
{
    string GetPhpIniPath();
    void UpdatePhpIniFiles();
    void AddSettingToPhpIni(string name, string value, bool setForWeb = false, bool setForCLI = false);
}

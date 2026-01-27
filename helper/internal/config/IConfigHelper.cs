namespace WebDev.Tool.Helper.Internal.Config;

public interface IConfigHelper
{
    bool ConfigUpdated { get; set; }
    bool ConfigFileExists { get; }
    bool IsConfigFileValid { get; }
    bool IsConfigFileLoaded { get; }
    void ReadConfigFile(bool rethrowParseException = false);
    void SaveConfigFile();
}
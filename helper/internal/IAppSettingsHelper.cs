using WebDev.Tool.Classes.Settings;

namespace WebDev.Tool.Helper.Internal;

public interface IAppSettingsHelper
{
    AppSettings AppSettings { get; }
    void LoadAppSettings(bool rethrowParseException = false);
    void SaveAppSettings();
}

using System.Threading.Tasks;

namespace WebDev.Tool.Helper.Internal;

public interface IUpdateHelper
{
    string CurrentVersion { get; }
    Task<string> GetLatestVersion(bool forceUpdate = false);
    bool IsUpdateAvailable();
    Task<bool> UpdateToLatestRelease();
}

using System.Threading.Tasks;

namespace WebDev.Tool.Helper.FrankenPHP;

public interface IFrankenPHPHelper
{
    /// <summary>
    /// Returns the path to the FrankenPHP binary, downloading it if necessary.
    /// </summary>
    /// <returns>Full path to the binary, or null if platform is unsupported or download failed.</returns>
    Task<string> GetOrDownloadFrankenPHPAsync();

    /// <summary>
    /// Starts FrankenPHP as a background process serving the given document root on the given port.
    /// Writes the process ID to pidFile.
    /// </summary>
    void StartFrankenPHP(string documentRoot, int port, string pidFile);

    /// <summary>
    /// Stops the FrankenPHP process whose PID is stored in pidFile.
    /// </summary>
    /// <returns>True if the process was found and stopped, false otherwise.</returns>
    bool StopFrankenPHP(string pidFile);
}

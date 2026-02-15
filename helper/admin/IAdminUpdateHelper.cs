using System.Threading.Tasks;

namespace WebDev.Tool.Helper.Admin;

public interface IAdminUpdateHelper
{
    /// <summary>
    /// Ensures the admin backend is present: downloads from GitHub if missing,
    /// or checks for a newer release and asks the user to update if available.
    /// </summary>
    /// <returns>True if the backend is ready (existing or downloaded/updated), false on error.</returns>
    Task<bool> InstallOrUpdateIfNeededAsync();
}

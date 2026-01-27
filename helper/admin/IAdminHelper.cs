namespace WebDev.Tool.Helper.Admin;

public interface IAdminHelper
{
    bool StartAdmin(int port = 8000, bool runAsDaemon = true);
    bool StopAdmin(bool debug = false);
    bool IsSymfonyCliInstalled(bool debug = false);
}

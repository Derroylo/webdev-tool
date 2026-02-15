namespace WebDev.Tool.Helper.Admin;

public interface IAdminHelper
{
    bool StartAdmin(int port = 8000);
    bool StopAdmin(bool debug = false);
}

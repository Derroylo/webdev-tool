namespace WebDev.Tool.Helper.apache;

public interface IApacheHelper
{
    bool DisableVhost(string siteName, bool debug = false);
    bool EnableVhost(string siteName, bool debug = false);
    bool ReloadApache(bool debug = false);
}

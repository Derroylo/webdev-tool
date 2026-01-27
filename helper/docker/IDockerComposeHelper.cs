using System.Collections.Generic;

namespace WebDev.Tool.Helper.Docker;

public interface IDockerComposeHelper
{
    string GetFile();
    string GetProxyFile();
    Dictionary<string, Dictionary<string, string>> GetServices(string filename);
    bool IsServiceStarted(string name);
}

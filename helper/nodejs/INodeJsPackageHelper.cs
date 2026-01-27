using System.Collections.Generic;

namespace WebDev.Tool.Helper.NodeJs;

public interface INodeJsPackageHelper
{
    List<string> GetCurrentInstalledNodeJSPackages(string packageListOutput = null);
    void InstallPackages(string[] newPackages, bool debug = false);
}

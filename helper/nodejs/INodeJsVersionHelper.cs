using System.Collections.Generic;

namespace WebDev.Tool.Helper.NodeJs;

public interface INodeJsVersionHelper
{
    string GetCurrentNodeJSVersionOutput();
    string GetCurrentNodeJSVersion();
    List<string> GetAvailableNodeJSVersions();
    void SetNewNodeJSVersion(string newVersion);
}

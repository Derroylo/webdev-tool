using System.Collections.Generic;

namespace WebDev.Tool.Helper.Php;

public interface IPhpVersionHelper
{
    List<string> GetAvailablePhpVersions();
    string GetCurrentPhpVersion();
    string GetCurrentPhpVersionOutput();
}

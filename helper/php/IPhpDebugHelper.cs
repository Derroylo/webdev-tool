using System.Collections.Generic;

namespace WebDev.Tool.Helper.Php;

public interface IPhpDebugHelper
{
    Dictionary<string, string> GetCurrentSettings();
}

namespace WebDev.Tool.Helper;

public interface IPathHelper
{
    static bool IsMainWorkspace { get; set; }
    string GetWorkspacePath(bool insideContainer = true);
}

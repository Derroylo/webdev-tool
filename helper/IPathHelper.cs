namespace WebDev.Tool.Helper;

public interface IPathHelper
{
    bool IsMainWorkspace { get; set; }
    string GetWorkspacePath(bool insideContainer = true);
}

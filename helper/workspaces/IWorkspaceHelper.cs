namespace WebDev.Tool.Helper.Workspaces;

public interface IWorkspaceHelper
{
    bool ValidateWorkspaces();
    bool PrepareWorkspaces();

    void EnableVhostConfigurations();
}
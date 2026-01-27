using Spectre.Console.Cli;
using WebDev.Tool.Helper.Workspaces;

namespace WebDev.Tool.Commands.Workspaces;

internal class PostStartWorkspacesCommand(IWorkspaceHelper _workspaceHelper): Command<PostStartWorkspacesCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        if (!_workspaceHelper.ValidateWorkspaces())
        {
            return 1;
        }
        
        _workspaceHelper.EnableVhostConfigurations();
        
        return 0;
    }
}
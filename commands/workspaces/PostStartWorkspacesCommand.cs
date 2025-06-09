using System.ComponentModel;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.workspaces;

namespace WebDev.Tool.Commands.workspaces;

public class PostStartWorkspacesCommand: Command<PostStartWorkspacesCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--debug")]
        [Description("Outputs debug information")]
        [DefaultValue(false)]
        public bool Debug { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        if (!WorkspaceHelper.ValidateWorkspaces(settings.Debug))
        {
            return 1;
        }
        
        WorkspaceHelper.EnableVhostConfigurations(settings.Debug);
        
        return 0;
    }
}
using System.ComponentModel;
using Spectre.Console.Cli;

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
        return 0;
    }
}
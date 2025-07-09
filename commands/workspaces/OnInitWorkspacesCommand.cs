using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Proxy;
using WebDev.Tool.Helper.workspaces;

namespace WebDev.Tool.Commands.workspaces;

internal class OnInitWorkspacesCommand: Command<OnInitWorkspacesCommand.Settings>
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
        if (settings.Debug)
        {
            AnsiConsole.WriteLine("Executing OnInitWorkspacesCommand with debug mode enabled.");
        }
        
        if (!WorkspaceHelper.ValidateWorkspaces(settings.Debug))
        {
            return 1;
        }

        if (!TraefikHelper.CreateServiceLabels(settings.Debug))
        {
            return 1;
        }
        
        if (!TraefikHelper.CreateTraefikCertificates())
        {
            return 1;
        }
        
        return !WorkspaceHelper.PrepareWorkspaces(settings.Debug) ? 1 : 0;
    }
}
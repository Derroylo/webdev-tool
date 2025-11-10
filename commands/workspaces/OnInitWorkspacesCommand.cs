using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Proxy;
using WebDev.Tool.Helper.Secrets;
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
        
        // Load secrets
        if (!SecretsLoader.LoadEnvVarSecrets())
        {
            return 1;
        }

        if (!SecretsLoader.LoadFileSecrets())
        {
            return 1;
        }

        // Stop other devcontainers
        var runningContainers = DockerHelper.GetRunningContainers("_devcontainer");
        if (runningContainers.Count > 0)
        {
            foreach (var container in runningContainers)
            {
                DockerHelper.StopContainer(container);
            }
        }
        
        return !WorkspaceHelper.PrepareWorkspaces(settings.Debug) ? 1 : 0;
    }
}
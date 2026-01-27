using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Proxy;
using WebDev.Tool.Helper.Secrets;
using WebDev.Tool.Helper.Workspaces;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Workspaces;

internal class OnInitWorkspacesCommand(
    IDebugOutputHelper _debugOutputHelper,
    ITraefikHelper _traefikHelper,
    IDockerHelper _dockerHelper,
    IWorkspaceHelper _workspaceHelper,
    ISecretsLoader _secretsLoader
): Command<OnInitWorkspacesCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing OnInitWorkspacesCommand", this);
        
        if (!_workspaceHelper.ValidateWorkspaces())
        {
            return 1;
        }
       
        if (!_traefikHelper.CreateTraefikConfig(settings.Debug))
        {
            return 1;
        }
        
        // Load secrets
        if (!_secretsLoader.LoadEnvVarSecrets())
        {
            return 1;
        }

        if (!_secretsLoader.LoadFileSecrets())
        {
            return 1;
        }

        // Stop other devcontainers
        var runningContainers = _dockerHelper.GetRunningContainers("_devcontainer");
        if (runningContainers.Count > 0)
        {
            foreach (var container in runningContainers)
            {
                _dockerHelper.StopContainer(container);
            }
        }
        
        return !_workspaceHelper.PrepareWorkspaces() ? 1 : 0;
    }
}
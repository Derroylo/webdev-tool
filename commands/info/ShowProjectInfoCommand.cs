using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Commands.Info;

internal class ShowProjectInfoCommand(IDebugOutputHelper _debugOutputHelper, IDockerComposeHelper _dockerComposeHelper, IEnvironmentHelper _environmentHelper, ServicesConfig _servicesConfig, GeneralConfig _generalConfig, PhpConfig _phpConfig, NodeJsConfig _nodeJsConfig, WorkspacesConfig _workspacesConfig): Command<ShowProjectInfoCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing ShowProjectInfoCommand", this);
        
        AnsiConsole.WriteLine("\n");
        
        var services = _dockerComposeHelper.GetServices(_dockerComposeHelper.GetFile());
        
        var panelContent = $"[bold yellow]Services:[/]\n";
        
        if (_servicesConfig.Services != null && _servicesConfig.Services.Any())
        {
            foreach (var service in _servicesConfig.Services)
            {
                var serviceDescription = service.Value.Name != "" ? service.Value.Name : service.Key;
                serviceDescription += service.Value.Description != "" ? " - " + service.Value.Description : "";
                
                if (serviceDescription.Length > 60)
                {
                    serviceDescription = serviceDescription.Substring(0, 57) + "...";
                }

                panelContent += $"[bold]{serviceDescription}[/]".PadRight(65);

                if (IsProxyActive() && service.Value.SubDomain != "")
                {
                    panelContent += $"[green]https://" + service.Value.SubDomain + "." + _generalConfig.Proxy.SubDomain + "." + _generalConfig.Proxy.Domain + "[/]";
                }
                
                panelContent += "\n";
            }
        }
        else
        {
            panelContent += "[red]None[/]\n";
        }
        
        AnsiConsole.MarkupLine(panelContent);

        var workspacePanelContent = "";
        
        // Show Workspaces
        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in _workspacesConfig.Workspaces)
        {
            // Skip main workspace
            if (workspace.Key == "main")
            {
                continue;
            }

            if (!string.IsNullOrEmpty(workspace.Value.Name))
            {
                workspacePanelContent += $"[bold]{workspace.Value.Name}[/]".PadRight(30);
            }
            else
            {
                workspacePanelContent += $"[bold]{workspace.Key} Workspace[/]".PadRight(30);
            }

            if (workspace.Value.DisableWeb)
            {
                workspacePanelContent += "\n";

                continue;
            }

            foreach (var subDomain in workspace.Value.SubDomains)
            {
                workspacePanelContent += $"[green]https://" + subDomain + "." + _generalConfig.Proxy.SubDomain + "." + _generalConfig.Proxy.Domain + "[/] ";
            }

            workspacePanelContent += "\n";
        }

        if (workspacePanelContent != "")
        {
            AnsiConsole.MarkupLine($"[bold yellow]Workspaces:[/]\n" + workspacePanelContent);
        }
        
        AnsiConsole.MarkupLine($"[bold yellow]Versions[/]");
        AnsiConsole.Markup($"[bold]PHP[/]".PadRight(30));
        AnsiConsole.Markup($"[green]{_phpConfig.PhpVersion}[/]\n");
        AnsiConsole.Markup($"[bold]Node.js[/]".PadRight(30));
        AnsiConsole.Markup($"[green]{_nodeJsConfig.NodeJsVersion}[/]");
        
        AnsiConsole.MarkupLine($"\n\n[bold yellow]Need help?[/]");
        AnsiConsole.MarkupLine($"Visit [green]https://derroylo.github.io/[/] to checkout the docs.");
        
        AnsiConsole.MarkupLine($"\n[bold yellow]What´s next?[/]");
        AnsiConsole.MarkupLine($"Open one of the following URLs in your browser to access your application:");
        AnsiConsole.MarkupLine($"- [green]https://" + _generalConfig.Proxy.SubDomain + "." + _generalConfig.Proxy.Domain + "[/]");
        foreach (var subDomain in _workspacesConfig.Workspaces["main"].SubDomains)
        {
            AnsiConsole.MarkupLine($"- [green]https://" + subDomain + "." + _generalConfig.Proxy.SubDomain + "." + _generalConfig.Proxy.Domain + "[/]");
        }

        if (!_environmentHelper.IsRunningInDevContainer())
        {
            AnsiConsole.MarkupLine($"\nOpen the project with your favorite IDE to start coding: [green]code .[/] or [green]phpstorm .[/]");
        }
        
        AnsiConsole.MarkupLine("\n[bold green]Happy coding! :rocket:[/]");

        return 0;
    }
    
    private bool IsProxyActive()
    {
        return _servicesConfig.Services !=null && _servicesConfig.Services.ContainsKey("traefik") && _servicesConfig.Services["traefik"].Active;
    }
}
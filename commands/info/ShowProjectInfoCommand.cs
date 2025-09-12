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

internal class ShowProjectInfoCommand: Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.WriteLine("\n");
        
        var services = DockerComposeHelper.GetServices(DockerComposeHelper.GetFile());
        
        var panelContent = $"[bold yellow]Services:[/]\n";
        
        if (ServicesConfig.ActiveServices != null && ServicesConfig.ActiveServices.Any())
        {
            foreach (var service in ServicesConfig.ActiveServices)
            {
                services.TryGetValue(service, out var serviceConfig);
                
                var serviceDescription = serviceConfig != null && serviceConfig.ContainsKey("name") ? serviceConfig["name"] : service;
                serviceDescription += serviceConfig != null && serviceConfig.ContainsKey("description") ? " - " + serviceConfig["description"] : "";
                
                if (serviceDescription.Length > 60)
                {
                    serviceDescription = serviceDescription.Substring(0, 57) + "...";
                }

                panelContent += $"[bold]{serviceDescription}[/]".PadRight(65);

                if (IsProxyActive() && serviceConfig != null && serviceConfig.ContainsKey("proxy.subdomain"))
                {
                    panelContent += $"[green]https://" + serviceConfig["proxy.subdomain"] + "." + GeneralConfig.Proxy.SubDomain + "." + GeneralConfig.Proxy.Domain + "[/]";
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
        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
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
                workspacePanelContent += $"[green]https://" + subDomain + "." + GeneralConfig.Proxy.SubDomain + "." + GeneralConfig.Proxy.Domain + "[/] ";
            }

            workspacePanelContent += "\n";
        }

        if (workspacePanelContent != "")
        {
            AnsiConsole.MarkupLine($"[bold yellow]Workspaces:[/]\n" + workspacePanelContent);
        }
        
        AnsiConsole.MarkupLine($"[bold yellow]Versions[/]");
        AnsiConsole.Markup($"[bold]PHP[/]".PadRight(30));
        AnsiConsole.Markup($"[green]{PhpConfig.PhpVersion}[/]\n");
        AnsiConsole.Markup($"[bold]Node.js[/]".PadRight(30));
        AnsiConsole.Markup($"[green]{NodeJsConfig.NodeJsVersion}[/]");
        
        AnsiConsole.MarkupLine($"\n\n[bold yellow]Need help?[/]");
        AnsiConsole.MarkupLine($"Visit [green]https://derroylo.github.io/[/] to checkout the docs.");
        
        AnsiConsole.MarkupLine($"\n[bold yellow]What´s next?[/]");
        AnsiConsole.MarkupLine($"Open one of the following URLs in your browser to access your application:");
        AnsiConsole.MarkupLine($"- [green]https://" + GeneralConfig.Proxy.SubDomain + "." + GeneralConfig.Proxy.Domain + "[/]");
        foreach (var subDomain in WorkspacesConfig.Workspaces["main"].SubDomains)
        {
            AnsiConsole.MarkupLine($"- [green]https://" + subDomain + "." + GeneralConfig.Proxy.SubDomain + "." + GeneralConfig.Proxy.Domain + "[/]");
        }

        if (!EnvironmentHelper.IsRunningInDevContainer())
        {
            AnsiConsole.MarkupLine($"\nOpen the project with your favorite IDE to start coding: [green]code .[/] or [green]phpstorm .[/]");
        }
        
        AnsiConsole.MarkupLine("\n[bold green]Happy coding! :rocket:[/]");

        return 0;
    }
    
    private static bool IsProxyActive()
    {
        return ServicesConfig.ActiveServices !=null && ServicesConfig.ActiveServices.Contains("traefik");
    }
}
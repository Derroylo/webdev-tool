using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Commands.Info;

internal class ShowProjectStartSummaryCommand: Command
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
                    panelContent += $"[green]https://" + serviceConfig["proxy.subdomain"] + "." + GeneralConfig.Proxy.Subdomain + "." + GeneralConfig.Proxy.Domain + "[/]";
                }
                
                panelContent += "\n";
            }
        }
        else
        {
            panelContent += "[red]None[/]\n";
        }
        
        AnsiConsole.MarkupLine(panelContent);

        var workspacePanelContent = $"[bold yellow]Workspaces:[/]\n";
        
        // Add main workspace
        var mainService = services["devcontainer"];
        
        workspacePanelContent += $"[bold]Main[/]".PadRight(30);
        workspacePanelContent += $"[green]https://" + mainService["proxy.subdomain"] + "." + GeneralConfig.Proxy.Subdomain + "." + GeneralConfig.Proxy.Domain + "[/]";
        workspacePanelContent += "\n";
        
        // Add additional workspaces
        foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
        {
            workspacePanelContent += $"[bold]{workspace.Value.Name}[/]".PadRight(30);
            workspacePanelContent += $"[green]https://" + workspace.Value.SubDomain + "." + GeneralConfig.Proxy.Subdomain + "." + GeneralConfig.Proxy.Domain + "[/]";
            workspacePanelContent += "\n";
        }

        AnsiConsole.MarkupLine(workspacePanelContent);
        
        AnsiConsole.MarkupLine($"[bold yellow]Versions[/]");
        AnsiConsole.Markup($"[bold]PHP[/]".PadRight(30));
        AnsiConsole.Markup($"[green]{PhpConfig.PhpVersion}[/]\n");
        AnsiConsole.Markup($"[bold]Node.js[/]".PadRight(30));
        AnsiConsole.Markup($"[green]{NodeJsConfig.NodeJsVersion}[/]");
        
        AnsiConsole.MarkupLine($"\n\n[bold yellow]Need help?[/]");
        AnsiConsole.MarkupLine($"Visit [green]https://derroylo.github.io/[/] to checkout the docs.");
        
        AnsiConsole.MarkupLine($"\n[bold yellow]What´s next?[/]");
        AnsiConsole.MarkupLine($"Open [green]https://{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}[/] in your browser to access your application.");
        AnsiConsole.MarkupLine($"Open the project with your favorite IDE to start coding: [green]code .[/] or [green]phpstorm .[/]");
        
        AnsiConsole.MarkupLine("\n[bold green]Happy coding! :rocket:[/]");

        return 0;
    }
    
    private static bool IsProxyActive()
    {
        return ServicesConfig.ActiveServices !=null && ServicesConfig.ActiveServices.Contains("traefik");
    }
}
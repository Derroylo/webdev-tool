using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Commands.Info;

internal class ShowProjectStartSummaryCommand: Command
{
    public override int Execute(CommandContext context)
    {
        var panelContent = $"[bold yellow]Services:[/]\n";
        
        if (ServicesConfig.ActiveServices != null && ServicesConfig.ActiveServices.Any())
        {
            var services = DockerComposeHelper.GetServices(DockerComposeHelper.GetFile());
            
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

                if (IsProxyActive() && serviceConfig != null && serviceConfig.ContainsKey("url"))
                {
                    panelContent += $"[green]http://" + serviceConfig["url"].Replace("${WEBDEV_PROXY_SUBDOMAIN:-devcontainer}", GeneralConfig.Proxy.Subdomain).Replace("${WEBDEV_PROXY_DOMAIN:-dev.localhost}", GeneralConfig.Proxy.Domain) + "[/]";
                }
                
                panelContent += "\n";
            }
        }
        else
        {
            panelContent += "[red]None[/]\n";
        }
        
        AnsiConsole.MarkupLine(panelContent);

        AnsiConsole.MarkupLine($"[bold yellow]Versions[/]");
        AnsiConsole.Markup($"[bold]PHP[/]".PadRight(30));
        AnsiConsole.Markup($"[green]{PhpConfig.PhpVersion}[/]\n");
        AnsiConsole.Markup($"[bold]Node.js[/]".PadRight(30));
        AnsiConsole.Markup($"[green]{NodeJsConfig.NodeJsVersion}[/]");
        
        AnsiConsole.MarkupLine($"\n\n[bold yellow]Need help?[/]");
        AnsiConsole.MarkupLine($"Visit [green]https://derroylo.github.io/[/] to checkout the docs.");
        
        AnsiConsole.MarkupLine($"\n[bold yellow]What´s next?[/]");
        AnsiConsole.MarkupLine($"Open [green]http://{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}[/] in your browser to access your application.");
        AnsiConsole.MarkupLine($"Open the project with your favorite IDE to start coding: [green]code .[/] or [green]phpstorm .[/]");
        
        AnsiConsole.MarkupLine("\n[bold green]Happy coding! :rocket:[/]");

        return 0;
    }
    
    private static bool IsProxyActive()
    {
        return ServicesConfig.ActiveServices !=null && ServicesConfig.ActiveServices.Contains("proxy");
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectre.Console;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WebDev.Tool.Helper.proxy;

internal class TraefikHelper
{
    public static bool CreateServiceLabels(bool debug = false)
    {
        if (WorkspacesConfig.Workspaces.Count == 0)
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]No workspaces defined in the config.[/]");
            }
            return false;
        }

        var workspacePath = PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer());
        var composeFile = Path.Combine(workspacePath, ".devcontainer", "docker-compose.yml");
        var proxyFile = Path.Combine(workspacePath, ".devcontainer", "docker-compose.proxy.yml");

        var domain = GeneralConfig.Proxy.Domain;
        var globalSubDomain = GeneralConfig.Proxy.Subdomain;

        if (!File.Exists(composeFile))
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]docker-compose.yml not found at {composeFile}[/]");
            }

            return false;
        }

        var services = DockerComposeHelper.GetServices(composeFile);
        var newServices = new Dictionary<string, object>();
        
        if (services.Count == 0)
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]No services found in {composeFile}[/]");
            }
            return false;
        }

        foreach (KeyValuePair<string, Dictionary<string, string>> service in services)
        {
            var traefikLabels = new Dictionary<string, string>();
            
            var serviceName = service.Key;
            var proxyDomain = service.Value.ContainsKey("proxy.subdomain") ? service.Value["proxy.subdomain"] : "";
            var proxyPort = service.Value.ContainsKey("proxy.port") ? service.Value["proxy.port"] : "";

            if (proxyDomain == "" || proxyPort == "")
            {
                traefikLabels.Add($"traefik.enable", "false");
                
                newServices[serviceName] = new Dictionary<string, object> { { "labels", traefikLabels } };
                
                continue;
            }
            
            traefikLabels = new Dictionary<string, string>
            {
                { $"traefik.enable", "true" },
                { $"traefik.http.routers.{serviceName}.rule", $"Host(`{proxyDomain}.{globalSubDomain}.{domain}`)" },
                { $"traefik.http.routers.{serviceName}.entrypoints", "https" },
                { $"traefik.http.routers.{serviceName}.tls", "true" },
                { $"traefik.http.routers.{serviceName}.service", $"{serviceName}@docker" },
                { $"traefik.http.services.{serviceName}.loadbalancer.server.port", proxyPort }
            };

            if (serviceName == "devcontainer" && WorkspacesConfig.Workspaces.Count > 0)
            {
                foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
                {
                    if (workspace.Value.Mode != WorkspaceMode.Vhost) continue;

                    traefikLabels.Add($"traefik.http.routers.{serviceName}-{workspace.Value.SubDomain}.rule", $"Host(`{workspace.Value.SubDomain}.{globalSubDomain}.{domain}`)");
                    traefikLabels.Add($"traefik.http.routers.{serviceName}-{workspace.Value.SubDomain}.entrypoints", "https" );
                    traefikLabels.Add($"traefik.http.routers.{serviceName}-{workspace.Value.SubDomain}.tls", "true");
                    traefikLabels.Add($"traefik.http.routers.{serviceName}-{workspace.Value.SubDomain}.service", $"{serviceName}@docker");
                    traefikLabels.Add($"traefik.http.services.{serviceName}-{workspace.Value.SubDomain}.loadbalancer.server.port", proxyPort);
                }
            }
            
            newServices[serviceName] = new Dictionary<string, object> { { "labels", traefikLabels } };
        }

        if (newServices.Count == 0)
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[yellow]No services with com.webdev.subdomain found.[/]");
            }

            return false;
        }

        // Write new docker-compose.proxy.yml
        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var proxyCompose = new Dictionary<string, object> { { "services", newServices } };
            var outputYaml = serializer.Serialize(proxyCompose);
            File.WriteAllText(proxyFile, outputYaml);
            
            if (debug)
            {
                AnsiConsole.MarkupLine($"[green]docker-compose.proxy.yml created with {newServices.Count} services.[/]");
            }
        } 
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error writing docker-compose.proxy.yml: {ex.Message}[/]");
            
            if (debug) 
            {
                AnsiConsole.WriteException(ex);
            }
            
            return false;
        }
        
        return true;
    }
}


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

namespace WebDev.Tool.Helper.Proxy;

internal class TraefikHelper
{
    public static bool CreateServiceLabels(bool debug = false)
    {
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
            
            newServices[serviceName] = new Dictionary<string, object> { { "labels", traefikLabels } };
        }

        if (WorkspacesConfig.Workspaces.Count > 0)
        {
            var traefikLabels = new Dictionary<string, string>();
            var hosts = new List<string>();
            
            foreach (KeyValuePair<string, WorkspaceEntryConfiguration> workspace in WorkspacesConfig.Workspaces)
            {
                if (workspace.Value.Mode != WorkspaceMode.Vhost || workspace.Value.DisableWeb) continue;

                traefikLabels.Add($"traefik.enable", "true");
                
                if (workspace.Key == "main")
                {
                    hosts.Add($"Host(`{globalSubDomain}.{domain}`)");
                    hosts.Add($"Host(`www.{globalSubDomain}.{domain}`)");
                }
                else
                {
                    hosts.Add($"Host(`{workspace.Value.SubDomain}.{globalSubDomain}.{domain}`)");
                }
            }
            
            if (hosts.Count > 0)
            {
                traefikLabels.Add($"traefik.http.routers.devcontainer.rule", string.Join(" || ", hosts));
                traefikLabels.Add($"traefik.http.routers.devcontainer.entrypoints", "https" );
                traefikLabels.Add($"traefik.http.routers.devcontainer.tls", "true");
                traefikLabels.Add($"traefik.http.routers.devcontainer.service", $"devcontainer@docker");
                traefikLabels.Add($"traefik.http.services.devcontainer.loadbalancer.server.port", "8080");
            }
            
            newServices["devcontainer"] = new Dictionary<string, object> { { "labels", traefikLabels } };
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

    public static bool CreateTraefikCertificates()
    {
        var workspacePath = PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer());

        var certificateDir = Path.Combine(workspacePath, ".devcontainer", "traefik", "certs");

        var rootCaDir = AppSettingsHelper.AppSettings.Proxy.CaRootDirectory;

        if (!Directory.Exists(rootCaDir))
        {
            return false;
        }
        
        // Make sure the proxy settings exist in the config
        if (GeneralConfig.Proxy.Domain == "" || GeneralConfig.Proxy.Subdomain == "")
        {
            return false;
        }
        
        if (!Directory.Exists(certificateDir))
        {
            Directory.CreateDirectory(certificateDir);
        }
        
        // Check if the certificates already exist
        if (File.Exists(Path.Combine(certificateDir, $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}.pem")) &&
            File.Exists(Path.Combine(certificateDir, $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}-key.pem")))
        {
            return true;
        }
        
        // Run mkcert in a container to generate certs
        var dockerCmd = $@"
            docker run --rm -v {rootCaDir}:/root/.local/share/mkcert -v {certificateDir}:/certs -w /certs alpine/mkcert ""*.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}""
        ";
        
        ExecCommand.Exec(dockerCmd);

        // Update the traefik configuration to use the new certificates
        var traefikConfig = new Dictionary<string, object>();
        traefikConfig["stores"] = new Dictionary<string, object>
        {
            { "default", new Dictionary<string, object> { { "defaultCertificate", new Dictionary<string, string> { { "certFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}.pem") }, { "keyFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}-key.pem") } } } } }
        };
        
        traefikConfig["certificates"] = new Dictionary<string, string>
        {
            { "certFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}.pem") }, 
            { "keyFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}-key.pem") }
        };
        
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var traefikDynamicConfig = new Dictionary<string, object> { { "tls", traefikConfig } };
        var outputYaml = serializer.Serialize(traefikDynamicConfig);
        
        File.WriteAllText(Path.Combine(workspacePath, ".devcontainer", "traefik", "config", "dynamic.yml"), outputYaml);

        return true;
    }
}

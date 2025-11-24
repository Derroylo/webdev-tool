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
    public static bool CreateTraefikConfig(bool debug = false)
    {
        var workspacePath = PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer());

        // Create the certificates and the devcontainer route config
        var certConfig = CreateTraefikCertificates(debug);
        var routerConfig = CreateContainerRouteConfig(debug);

        // Serialize the traefik config
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var traefikDynamicConfig = new Dictionary<string, object> { { "tls", certConfig }, { "http", routerConfig } };
        var outputYaml = serializer.Serialize(traefikDynamicConfig);
        
        File.WriteAllText(Path.Combine(workspacePath, ".devcontainer", "traefik", "config", "dynamic.yml"), outputYaml);

        return true;
    }

    private static Dictionary<string, object> CreateContainerRouteConfig(bool debug = false)
    {
        var services = ServicesConfig.Services;
        var routerConfig = new Dictionary<string, object> { { "routers", new Dictionary<string, object>() }, { "services", new Dictionary<string, object>() } };

        foreach (KeyValuePair<string, ServiceEntryConfiguration> service in services)
        {
            if (!service.Value.Active)
            {
                if (debug)
                {
                    AnsiConsole.MarkupLine($"[yellow]Service {service.Value.Name} is not active[/]");
                }

                continue;
            }

            CreateServiceRouteConfig(ref routerConfig, service, debug);
        }

        return routerConfig;
    }

    private static bool CreateServiceRouteConfig(ref Dictionary<string, object> routerConfig, KeyValuePair<string, ServiceEntryConfiguration> service, bool debug = false)
    {
        var domain = GeneralConfig.Proxy.Domain;
        var globalSubDomain = GeneralConfig.Proxy.SubDomain;
        var serviceName = service.Key;
        var subDomain = service.Value.SubDomain;
        var port = service.Value.Port;
        
        if (subDomain == "" || port == 0)
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]No subdomain or port defined for {serviceName}[/]");
            }

            return true;
        }
        
        if (debug)
        {
            AnsiConsole.MarkupLine($"[green]Adding traefik config for {serviceName}[/]");
        }

        routerConfig["routers"][serviceName] = new Dictionary<string, object>
        {
            { "rule", $"Host(`{subDomain}.{globalSubDomain}.{domain}`) || Host(`{subDomain}.{domain}`)" },
            { "entrypoints", "https" },
            { "tls", true },
            { "service", $"{serviceName}@docker" }
        };

        routerConfig["services"][serviceName] = new Dictionary<string, object>
        {
            { "rule", $"Host(`{subDomain}.{globalSubDomain}.{domain}`) || Host(`{subDomain}.{domain}`)" },
            { "entrypoints", "https" },
            { "tls", true },
            { "service", $"{serviceName}@docker" }
        };

        return true;
    }

    private static Dictionary<string, object> CreateTraefikCertificates(bool debug = false)
    {
        var workspacePath = PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer());

        var certificateDir = Path.Combine(workspacePath, ".devcontainer", "traefik", "certs");

        var rootCaDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certs");

        if (!Directory.Exists(rootCaDir))
        {
            Directory.CreateDirectory(rootCaDir);
        }
        
        // Make sure the proxy settings exist in the config
        if (GeneralConfig.Proxy.Domain == "" || GeneralConfig.Proxy.SubDomain == "")
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[red]No domain or subdomain defined in the config[/]");
            }

            return new Dictionary<string, object>();
        }
        
        if (!Directory.Exists(certificateDir))
        {
            Directory.CreateDirectory(certificateDir);
        }
        
        // Check if the certificates already exist
        if (!File.Exists(Path.Combine(certificateDir, $"_wildcard.{GeneralConfig.Proxy.SubDomain}.{GeneralConfig.Proxy.Domain}.pem")) ||
            !File.Exists(Path.Combine(certificateDir, $"_wildcard.{GeneralConfig.Proxy.SubDomain}.{GeneralConfig.Proxy.Domain}-key.pem")))
        {
            // Run mkcert in a container to generate certs
            var dockerCmd = $@"
                docker run --rm --user 1000:1000 -v {rootCaDir}:/root/.local/share/mkcert -v {certificateDir}:/certs -w /certs alpine/mkcert ""*.{GeneralConfig.Proxy.SubDomain}.{GeneralConfig.Proxy.Domain}""
            ";
            
            ExecCommand.Exec(dockerCmd);

            if (debug)
            {
                AnsiConsole.MarkupLine($"[green]Certificates created[/]");
            }
        }
        else
        {
            if (debug)
            {
                AnsiConsole.MarkupLine($"[yellow]Certificates already exist[/]");
            }
        }      

        var traefikConfig = new Dictionary<string, object>();

        // Update the traefik configuration to use the new certificates
        traefikConfig["stores"] = new Dictionary<string, object>
        {
            { "default", new Dictionary<string, object> { { "defaultCertificate", new Dictionary<string, string> { { "certFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.SubDomain}.{GeneralConfig.Proxy.Domain}.pem") }, { "keyFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.SubDomain}.{GeneralConfig.Proxy.Domain}-key.pem") } } } } }
        };
        
        traefikConfig["certificates"] = new Dictionary<string, string>
        {
            { "certFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.SubDomain}.{GeneralConfig.Proxy.Domain}.pem") }, 
            { "keyFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.SubDomain}.{GeneralConfig.Proxy.Domain}-key.pem") }
        };      

        if (debug)
        {
            AnsiConsole.MarkupLine($"[green]Traefik config for certificates updated[/]");
        }

        return traefikConfig;
    }
}

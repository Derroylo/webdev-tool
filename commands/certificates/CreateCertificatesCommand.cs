using System.Collections.Generic;
using Spectre.Console.Cli;
using WebDev.Tool.Helper;
using System.IO;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WebDev.Tool.Commands.Certificates;

internal class CreateCertificatesCommand: Command
{
    public override int Execute(CommandContext context)
    {
        var workspacePath = PathHelper.GetWorkspacePath(EnvironmentHelper.IsRunningInDevContainer());

        var certificateDir = Path.Combine(workspacePath, "traefik", "certs");

        var rootCaDir = AppSettingsHelper.AppSettings.Proxy.CaRootDirectory;

        if (!Directory.Exists(rootCaDir))
        {
            return 0;
        }
        
        // Make sure the proxy settings exist in the config
        if (GeneralConfig.Proxy.Domain == "" || GeneralConfig.Proxy.Subdomain == "")
        {
            return 0;
        }
        
        if (!Directory.Exists(certificateDir))
        {
            Directory.CreateDirectory(certificateDir);
        }
        
        // Check if the certificates already exist
        if (File.Exists(Path.Combine(certificateDir, $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}.pem")) &&
            File.Exists(Path.Combine(certificateDir, $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}-key.pem")))
        {
            return 0;
        }
        
        // Run mkcert in a container to generate certs
        var dockerCmd = $@"
            docker run -ti --rm -v {rootCaDir}:/root/.local/share/mkcert -v {certificateDir}:/certs -w /certs alpine/mkcert ""*.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}""
        ";
        
        ExecCommand.Exec(dockerCmd);

        // Update the traefik configuration to use the new certificates
        var traefikConfig = new Dictionary<string, object>();
        traefikConfig["stores"] = new Dictionary<string, object>
        {
            { "default", new Dictionary<string, object> { { "defaultCertificate", new Dictionary<string, string> { { "certFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}.pem") }, { "keyFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}-key.pem") } } } } }
        };
        
        traefikConfig["certificates:"] = new Dictionary<string, string>
        {
            { "certFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}.pem") }, 
            { "keyFile", Path.Combine("/etc/certs/", $"_wildcard.{GeneralConfig.Proxy.Subdomain}.{GeneralConfig.Proxy.Domain}-key.pem") }
        };
        
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var traefikDynamicConfig = new Dictionary<string, object> { { "tls", traefikConfig } };
        var outputYaml = serializer.Serialize(traefikDynamicConfig);
        File.WriteAllText(Path.Combine(workspacePath, "traefik", "config", "dynamic.yml"), outputYaml);
        
        return 0;
    }
}
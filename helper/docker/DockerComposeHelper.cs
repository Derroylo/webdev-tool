using System;
using System.Collections.Generic;
using System.IO;
using WebDev.Tool.Helper.Internal.Config.Sections;
using WebDev.Tool.Helper.Internal;
using YamlDotNet.Serialization.NamingConventions;

namespace WebDev.Tool.Helper.Docker
{
    public class DockerComposeHelper(GeneralConfig _generalConfig, ExecCommand _execCommand, IDebugOutputHelper _debugOutputHelper) : IDockerComposeHelper
    {
        public string GetFile()
        {
            var filename = _generalConfig.ComposeFileName;

            var workspacePath = Environment.GetEnvironmentVariable("WEBDEV_WORKSPACE_FOLDER");

            if (string.IsNullOrEmpty(workspacePath)) {
                workspacePath = Directory.GetCurrentDirectory();
            }

            return workspacePath + "/.devcontainer/" + filename;
        }
        
        public string GetProxyFile()
        {
            var filename = _generalConfig.ComposeFileName.Replace(".yml", ".proxy.yml");

            var workspacePath = Environment.GetEnvironmentVariable("WEBDEV_WORKSPACE_FOLDER");

            if (string.IsNullOrEmpty(workspacePath)) {
                workspacePath = Directory.GetCurrentDirectory();
            }

            return workspacePath + "/.devcontainer/" + filename;
        }

        public Dictionary<string, Dictionary<string, string>> GetServices(string filename)
        {
            var services = new Dictionary<string, Dictionary<string, string>>();

            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .Build();
            dynamic dockerCompose = deserializer.Deserialize<dynamic>(File.ReadAllText(filename));

            foreach (KeyValuePair<object, object> item in dockerCompose["services"]) {
                var serviceInfos = new Dictionary<string, string>();

                string alias = item.Key.ToString();               
                string serviceName = item.Key.ToString();

                if (((Dictionary<object, object>) dockerCompose["services"][alias]).ContainsKey("container_name")) {
                    alias = dockerCompose["services"][alias]["container_name"].ToString();
                }

                serviceInfos.Add("alias", alias);

                try
                {
                    if (dockerCompose["services"][serviceName].ContainsKey("image")) {
                        serviceInfos.Add("image", dockerCompose["services"][serviceName]["image"].ToString());
                    }

                    if (((Dictionary<object, object>) dockerCompose["services"][serviceName]).ContainsKey("environment")) {
                        if (dockerCompose["services"][serviceName]["environment"].ContainsKey("VIRTUAL_HOST")) {
                            serviceInfos.Add("url", dockerCompose["services"][serviceName]["environment"]["VIRTUAL_HOST"].ToString());
                        }
                    }
                } catch (Exception ex)
                {
                    // Ignore if keys are not found
                    _debugOutputHelper.WriteErrorOutput("Error getting service information", ex);
                }
                
                services.Add(item.Key.ToString(), serviceInfos);
            }

            return services;
        }

        public bool IsServiceStarted(string name)
        {
            var result = _execCommand.Exec("docker ps -q -f status=running -f name=^/" + name);

            if (result.Trim().Length == 0) {
                return false;
            }

            return true;
        }
    }
}

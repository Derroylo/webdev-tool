using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectre.Console;
using WebDev.Tool.Helper.Internal.Config.Sections;
using YamlDotNet.Serialization.NamingConventions;

namespace WebDev.Tool.Helper.Docker
{
    public class DockerHelper(ExecCommand _execCommand) : IDockerHelper
    {
        public List<string> GetRunningContainers(string name)
        {
            var containers = new List<string>();

            var result = _execCommand.Exec("docker ps -q -f status=running -f name=" + name);

            if (result.Trim().Length > 0) {
                containers = result.Trim().Split('\n').ToList();
            }

            return containers;
        }

        public void StopContainer(string name)
        {
            _execCommand.ExecWithDirectOutput("docker stop " + name);
        }

        public string GetProjectName()
        {
            var projectName = Path.GetFileName(Directory.GetCurrentDirectory()) + "_devcontainer";

            // Replace invalid characters with an underscore
            projectName = System.Text.RegularExpressions.Regex.Replace(projectName, @"[^A-Za-z0-9\-_]", "");
            
            return projectName;
        }
    }
}
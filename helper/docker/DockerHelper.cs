using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectre.Console;
using WebDev.Tool.Helper.Internal.Config.Sections;
using YamlDotNet.Serialization.NamingConventions;

namespace WebDev.Tool.Helper.Docker
{
    internal class DockerHelper
    {
        public static List<string> GetRunningContainers(string name)
        {
            var containers = new List<string>();

            var result = ExecCommand.Exec("docker ps -q -f status=running -f name=" + name);

            if (result.Trim().Length > 0) {
                containers = result.Trim().Split('\n').ToList();
            }

            return containers;
        }

        public static void StopContainer(string name)
        {
            ExecCommand.ExecWithDirectOutput("docker stop " + name);
        }
    }
}
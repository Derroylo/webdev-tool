using System.Collections.Generic;
using System.ComponentModel;

namespace WebDev.Tool.Classes.Configuration
{
    internal class ServiceConfiguration
    {
        public List<string> Active { get; set; } = new();

        [DefaultValue("docker-compose.yml")]
        public string File { get; set; } = "docker-compose.yml";
    }
}
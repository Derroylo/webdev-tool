using System.Collections.Generic;

namespace WebDev.Tool.Classes.Configuration
{
    internal class ServiceConfiguration
    {
        public List<string> Active { get; set; } = new();

        public string File { get; set; } = "docker-compose.yml";
    }
}
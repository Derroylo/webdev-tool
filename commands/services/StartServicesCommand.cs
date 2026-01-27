using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.DevContainer;
using System.Linq;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Services
{
    internal class StartServicesCommand(IDebugOutputHelper _debugOutputHelper, IDockerComposeHelper _dockerComposeHelper, IDockerHelper _dockerHelper, ServicesConfig _servicesConfig) : Command<StartServicesCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
            [CommandOption("-d|--detached")]
            [Description("Start the services in detached mode")]
            [DefaultValue(false)]
            public bool Detached { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing StartServicesCommand", this);
            
            if (!File.Exists(_dockerComposeHelper.GetFile())) {
                _debugOutputHelper.WriteErrorOutput(_dockerComposeHelper.GetFile() + " not found", this);

                return 0;
            }

            var services = _dockerComposeHelper.GetServices(_dockerComposeHelper.GetFile());

            if (_servicesConfig.Services == null || _servicesConfig.Services.Count == 0 || _servicesConfig.Services.All(s => !s.Value.Active)) {
                AnsiConsole.MarkupLine("[red]No active services selected[/]");

                AnsiConsole.MarkupLine($"\n[bold yellow]How to select active services?[/]");
                AnsiConsole.MarkupLine($"Execute [green]webdev services select[/] to select which services should be started or");
                AnsiConsole.MarkupLine($"edit the file [green].devcontainer/webdev.yml[/] manually to add services to the [green]services.active[/] section.");
                
                AnsiConsole.MarkupLine($"\n[bold yellow]Need more information?[/]");
                AnsiConsole.MarkupLine($"Visit [green]https://derroylo.github.io/[/] to checkout the docs.");
                
                return 0;
            }

            var activeServices = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, string>> item in services) {
                if (!_servicesConfig.Services.ContainsKey(item.Key) || !_servicesConfig.Services[item.Key].Active) {
                    continue;
                }
                 
                activeServices.Add(item.Key);
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectName = _dockerHelper.GetProjectName();

            if (File.Exists(_dockerComposeHelper.GetProxyFile()))
            {
                File.WriteAllText(applicationDir + ".services_start", "-f " + _dockerComposeHelper.GetFile() + " -f" + _dockerComposeHelper.GetProxyFile() + " -p " + projectName + " up " + (settings.Detached ? "-d " : "") +  string.Join(' ', activeServices));
            }
            else
            {
                File.WriteAllText(applicationDir + ".services_start", "-f " + _dockerComposeHelper.GetFile() + " -p " + projectName + " up " + (settings.Detached ? "-d " : "") +  string.Join(' ', activeServices));
            }

            return 0;
        }
    }   
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.devcontainer;

namespace WebDev.Tool.Commands.Services
{
    internal class StartServicesCommand : Command<StartServicesCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--detached")]
            [Description("Start the services in detached mode")]
            [DefaultValue(false)]
            public bool Detached { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (!File.Exists(DockerComposeHelper.GetFile())) {
                AnsiConsole.MarkupLine($"[red]{DockerComposeHelper.GetFile()} not found[/]");

                return 0;
            }

            var services = DockerComposeHelper.GetServices(DockerComposeHelper.GetFile());

            if (ServicesConfig.ActiveServices.Count == 0) {
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
                if (!ServicesConfig.ActiveServices.Contains(item.Key)) {
                    continue;
                }
                 
                activeServices.Add(item.Key);
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectName = Path.GetFileName(Directory.GetCurrentDirectory()) + "_devcontainer";
            
            File.WriteAllText(applicationDir + ".services_start", "-f " + DockerComposeHelper.GetFile() + " -p " + projectName + " up " + (settings.Detached ? "-d " : "") +  string.Join(' ', activeServices));

            return 0;
        }
    }   
}

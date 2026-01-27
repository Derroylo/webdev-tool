using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal.Config.Sections;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Services
{
    internal class ListServicesCommand(IDebugOutputHelper _debugOutputHelper, IDockerComposeHelper _dockerComposeHelper, ServicesConfig _servicesConfig) : Command<ListServicesCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
            
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing ListServicesCommand", this);
            
            if (!File.Exists(_dockerComposeHelper.GetFile())) {
                AnsiConsole.MarkupLine($"[red]{_dockerComposeHelper.GetFile()} not found[/]");

                return 0;
            }

            var services = _dockerComposeHelper.GetServices(_dockerComposeHelper.GetFile());

            var servicesTable = new Table();

            servicesTable.AddColumn("[bold yellow]Name[/]");
            servicesTable.AddColumn("[bold yellow]Description[/]");
            servicesTable.AddColumn("[bold yellow]Status[/]");
            servicesTable.AddColumn("[bold yellow]Active per default[/]");

            foreach(KeyValuePair<string, Dictionary<string, string>> item in services) {
                if (item.Key == "devcontainer")
                {
                    continue;
                }
                
                var serviceAlias = item.Value["alias"];
                var serviceName = item.Value.ContainsKey("name") ? item.Value["name"] : item.Key;
                var serviceDescription = item.Value.ContainsKey("description") ? item.Value["description"] : "-";

                bool isActive = _servicesConfig.Services.ContainsKey(item.Key) && _servicesConfig.Services[item.Key].Active;
                bool isRunning = _dockerComposeHelper.IsServiceStarted(serviceAlias);

                servicesTable.AddRow(serviceName, serviceDescription, isRunning ? "[green1]Running[/]" : "[red]Not started[/]", isActive ? "[green1]Active[/]" : "[red]Inactive[/]");
            }
            
            AnsiConsole.Write(servicesTable);

            return 0;
        }
    }   
}

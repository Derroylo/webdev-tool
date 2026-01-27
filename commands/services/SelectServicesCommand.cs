using System.Collections.Generic;
using System.IO;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper.Internal.Config;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Services
{
    internal class SelectServicesCommand(IDebugOutputHelper _debugOutputHelper, IDockerComposeHelper _dockerComposeHelper, ServicesConfig _servicesConfig) : Command<SelectServicesCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing SelectServicesCommand", this);
            
            if (!File.Exists(_dockerComposeHelper.GetFile())) {
                AnsiConsole.MarkupLine($"[red]{_dockerComposeHelper.GetFile()} not found[/]");

                return 0;
            }

            var services = _dockerComposeHelper.GetServices(_dockerComposeHelper.GetFile());
            Dictionary<string, List<string>> serviceCategories = new() {{"unknown", new List<string>()}};
        
            foreach (KeyValuePair<string, Dictionary<string, string>> item in services) {      
                var serviceEntry = _servicesConfig.Services.FirstOrDefault(s => s.Key == item.Key);

                if (serviceEntry.Value == null) {
                    continue;
                }

                if (serviceEntry.Value.Category == "") {
                    serviceCategories["unknown"].Add(item.Key);
                } else {
                    if (!serviceCategories.ContainsKey(serviceEntry.Value.Category)) {
                        serviceCategories.Add(serviceEntry.Value.Category, new List<string>());
                    }

                    serviceCategories[serviceEntry.Value.Category].Add(item.Key);
                }
            }

            var multiSelectPrompt = new MultiSelectionPrompt<string>()
                    .PageSize(20)
                    .Title("[bold yellow]Which service(s) should be started with your workspace?[/]")
                    .InstructionsText("[grey](Press [blue]space[/] to toggle a service, [green]enter[/] to accept)[/]");

            if (serviceCategories.Count > 1) {
                foreach (KeyValuePair<string, List<string>> item in serviceCategories) {
                    if (item.Key == "unknown") {
                        continue;
                    }

                    if (item.Value.Count == 0) {
                        continue;
                    }

                    multiSelectPrompt.AddChoiceGroup(item.Key, item.Value.ToArray());
                }
            }

            if (serviceCategories["unknown"].Count > 0) {
                multiSelectPrompt.AddChoices(serviceCategories["unknown"].ToArray());
            }

            if (_servicesConfig.Services != null && _servicesConfig.Services.Count > 0 && _servicesConfig.Services.Any(s => s.Value.Active)) {
                foreach (string item in _servicesConfig.Services.Where(s => s.Value.Active).Select(s => s.Key)) {
                    multiSelectPrompt.Select(item);
                }
            }

            var selectedServices = AnsiConsole.Prompt(multiSelectPrompt);

            foreach (KeyValuePair<string, Dictionary<string, string>> item in services) {
                var serviceEntry = _servicesConfig.Services.FirstOrDefault(s => s.Key == item.Key);

                if (serviceEntry.Value == null) {
                    continue;
                }

                serviceEntry.Value.Active = selectedServices.Contains(item.Key);
            }

            _servicesConfig.ConfigUpdated = true;

            AnsiConsole.WriteLine("The following services have been marked as active and will start with the workspace");

            foreach (string item in selectedServices) {
                AnsiConsole.WriteLine("- " + item);
            }

            return 0;
        }
    }   
}

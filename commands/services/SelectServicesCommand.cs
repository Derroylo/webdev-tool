using System.Collections.Generic;
using System.IO;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;

namespace WebDev.Tool.Commands.Services
{
    internal class SelectServicesCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            if (!File.Exists(DockerComposeHelper.GetFile())) {
                AnsiConsole.MarkupLine($"[red]{DockerComposeHelper.GetFile()} not found[/]");

                return 0;
            }

            var services = DockerComposeHelper.GetServices(DockerComposeHelper.GetFile());
            Dictionary<string, List<string>> serviceCategories = new() {{"unknown", new List<string>()}};

            foreach (KeyValuePair<string, Dictionary<string, string>> item in services) {               
                if (!item.Value.ContainsKey("category")) {
                    serviceCategories["unknown"].Add(item.Key);
                } else {
                    if (!serviceCategories.ContainsKey(item.Value["category"])) {
                        serviceCategories.Add(item.Value["category"], new List<string>());
                    }

                    serviceCategories[item.Value["category"]].Add(item.Key);
                }
            }

            var multiSelectPrompt = new MultiSelectionPrompt<string>()
                    .PageSize(10)
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

            if (ServicesConfig.Services != null && ServicesConfig.Services.Count > 0 && ServicesConfig.Services.Any(s => s.Value.Active)) {
                foreach (string item in ServicesConfig.Services.Where(s => s.Value.Active).Select(s => s.Key)) {
                    multiSelectPrompt.Select(item);
                }
            }

            var selectedServices = AnsiConsole.Prompt(multiSelectPrompt);

            foreach (string item in selectedServices) {
                if (ServicesConfig.Services.ContainsKey(item)) {
                    ServicesConfig.Services[item].Active = true;
                }
            }

            AnsiConsole.WriteLine("The following services have been marked as active and will start with the workspace");

            foreach (string item in selectedServices) {
                AnsiConsole.WriteLine("- " + item);
            }

            return 0;
        }
    }   
}

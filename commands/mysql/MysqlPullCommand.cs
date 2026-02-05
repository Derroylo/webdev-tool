using System.ComponentModel;
using Spectre.Console;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;
using System;
using System.Linq;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Mysql
{
    internal class MysqlPullCommand(IDebugOutputHelper _debugOutputHelper, IDockerComposeHelper _dockerComposeHelper, IDockerHelper _dockerHelper, ExecCommand _execCommand) : Command<MysqlPullCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
            [CommandArgument(0, "[Service]")]
            [Description("The name of the service to pull")]
            [DefaultValue("")]
            public string Service { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing MysqlPullCommand", this);
            
            // Check if mysql service exists in docker-compose
            var services = _dockerComposeHelper.GetServices(_dockerComposeHelper.GetFile());
            var mysqlServices = services.Where(s => s.Value.ContainsKey("image") && s.Key.StartsWith("mysql")).Select(s => s.Key).ToArray<string>();

            if (mysqlServices.Length == 0) {
                AnsiConsole.MarkupLine("[red]No MySQL service found in docker-compose.yml[/]");
                return 1;
            }

            AnsiConsole.MarkupLine("Updating MySQL service: " + settings.Service);
            if (string.IsNullOrEmpty(settings.Service) && mysqlServices.Length == 1) {
                settings.Service = mysqlServices[0];
            } else if (string.IsNullOrEmpty(settings.Service)) {
                settings.Service = SelectServicesToUpdate(mysqlServices);
            }

            var selectedServices = settings.Service.Split(',');
            if (selectedServices.Length == 0) {
                AnsiConsole.MarkupLine("[red]No MySQL service selected[/]");
                return 1;
            }

            if (!AnsiConsole.Confirm("Do you really want to update the selected database image(s)? All changes to the database will be lost.", false)) {
                return 0;
            }

            var composeFile = _dockerComposeHelper.GetFile();

            // Determine whether "docker-compose" or "docker compose" command should be used
            string dockerComposeCmd;

            // Try to find out which docker compose command is available
            try
            {
                var result = _execCommand.Exec("which docker-compose");
                if (result.Contains("docker-compose"))
                {
                    dockerComposeCmd = "docker-compose";
                }
                else
                {
                    // Fallback: check `docker compose` by checking if `docker` is present and `docker compose version` works
                    var dockerResult = _execCommand.Exec("docker compose version");
                    if (dockerResult.Contains("docker compose"))
                    {
                        dockerComposeCmd = "docker compose";
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Neither 'docker-compose' nor 'docker compose' found on this system![/]");
                        return 1;
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error checking docker compose command: {ex.Message}[/]");
                return 1;
            }

            foreach (var service in selectedServices) {
                if (!mysqlServices.Contains(service)) {
                    AnsiConsole.MarkupLine("[red]Service " + service + " not found in docker-compose.yml[/]");
                    return 1;
                }

                if (!services[service].ContainsKey("image")) {
                    AnsiConsole.MarkupLine("[red]No image found for service " + service + " in docker-compose.yml[/]");
                    return 1;
                }

                var image = services[service]["image"];

                if (string.IsNullOrEmpty(image)) {
                    AnsiConsole.MarkupLine("[red]No image found for MySQL service in docker-compose.yml[/]");
                    return 1;
                }

                if (image.StartsWith("mysql:")) {
                    AnsiConsole.MarkupLine("[red]Only custom database images can be updated.[/]");
                    return 1;
                }

                AnsiConsole.MarkupLine("Stopping " + service + " container...");
                _execCommand.ExecWithDirectOutput(dockerComposeCmd + " -f " + composeFile + " stop " + service);

                AnsiConsole.MarkupLine("Removing " + service + " container...");
                _execCommand.ExecWithDirectOutput(dockerComposeCmd + " -f " + composeFile + " rm -f " + service);

                AnsiConsole.MarkupLine("Pulling latest " + service + " image...");
                _execCommand.ExecWithDirectOutput(dockerComposeCmd + " -f " + composeFile + " pull " + service);

                AnsiConsole.MarkupLine("Starting " + service + " container...");
                _execCommand.ExecWithDirectOutput(dockerComposeCmd + " -f " + composeFile + " -p " + _dockerHelper.GetProjectName() + " up -d " + service);
            }

            AnsiConsole.MarkupLine("[green]" + string.Join(", ", selectedServices) + " container(s) have been updated successfully![/]");

            return 0;
        }

        private string SelectServicesToUpdate(string[] services)
        {
            var selected = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                .Title("Which [green]services[/] do you want to update?")
                .AddChoices(services));

            return string.Join(",", selected.ToArray<string>());
        }
    }
}
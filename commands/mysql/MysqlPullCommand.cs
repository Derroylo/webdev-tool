using System.ComponentModel;
using Spectre.Console;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;
using System;
using System.Linq;
using WebDev.Tool.Helper.Docker;
using System.IO;

namespace WebDev.Tool.Commands.Mysql
{
    internal class MysqlPullCommand : Command<MysqlPullCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            // Check if mysql service exists in docker-compose
            var services = DockerComposeHelper.GetServices(DockerComposeHelper.GetFile());
            if (!services.ContainsKey("mysql")) {
                AnsiConsole.MarkupLine("[red]No MySQL service found in docker-compose.yml[/]");
                return 1;
            }

            var image = services["mysql"]["image"];

            if (string.IsNullOrEmpty(image)) {
                AnsiConsole.MarkupLine("[red]No image found for MySQL service in docker-compose.yml[/]");
                return 1;
            }

            if (image.StartsWith("mysql:")) {
                AnsiConsole.MarkupLine("[red]Only custom database images can be updated.[/]");
                return 1;
            }

            if (!AnsiConsole.Confirm("Do you really want to update the database image? All changes to the current database will be lost.", false))
            {
                return 0;
            }

            var composeFile = DockerComposeHelper.GetFile();
            var projectName = Path.GetFileName(Directory.GetCurrentDirectory()) + "_devcontainer";
            
            AnsiConsole.MarkupLine("Stopping MySQL container...");
            ExecCommand.ExecWithDirectOutput("docker-compose -f " + composeFile + " stop mysql", settings.Debug);

            AnsiConsole.MarkupLine("Removing MySQL container...");
            ExecCommand.ExecWithDirectOutput("docker-compose -f " + composeFile + " rm -f mysql", settings.Debug);

            AnsiConsole.MarkupLine("Pulling latest MySQL image...");
            ExecCommand.ExecWithDirectOutput("docker-compose -f " + composeFile + " pull mysql", settings.Debug);

            AnsiConsole.MarkupLine("Starting MySQL container...");
            ExecCommand.ExecWithDirectOutput("docker-compose -f " + composeFile + " -p " + projectName + " up -d mysql", settings.Debug);

            AnsiConsole.MarkupLine("[green]MySQL container has been updated successfully![/]");

            return 0;
        }
    }
}
using System.ComponentModel;
using Spectre.Console;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;
using System;
using System.Linq;
using WebDev.Tool.Helper.Docker;
using System.IO;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Mysql
{
    internal class MysqlPullCommand(IDebugOutputHelper _debugOutputHelper, IDockerComposeHelper _dockerComposeHelper, IDockerHelper _dockerHelper, ExecCommand _execCommand) : Command<MysqlPullCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing MysqlPullCommand", this);
            
            // Check if mysql service exists in docker-compose
            var services = _dockerComposeHelper.GetServices(_dockerComposeHelper.GetFile());
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

            var composeFile = _dockerComposeHelper.GetFile();
            
            AnsiConsole.MarkupLine("Stopping MySQL container...");
            _execCommand.ExecWithDirectOutput("docker-compose -f " + composeFile + " stop mysql");

            AnsiConsole.MarkupLine("Removing MySQL container...");
            _execCommand.ExecWithDirectOutput("docker-compose -f " + composeFile + " rm -f mysql");

            AnsiConsole.MarkupLine("Pulling latest MySQL image...");
            _execCommand.ExecWithDirectOutput("docker-compose -f " + composeFile + " pull mysql");

            AnsiConsole.MarkupLine("Starting MySQL container...");
            _execCommand.ExecWithDirectOutput("docker-compose -f " + composeFile + " -p " + _dockerHelper.GetProjectName() + " up -d mysql");

            AnsiConsole.MarkupLine("[green]MySQL container has been updated successfully![/]");

            return 0;
        }
    }
}
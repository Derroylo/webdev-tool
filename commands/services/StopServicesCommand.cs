using System;
using System.IO;
using WebDev.Tool.Helper.Docker;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Services
{
    internal class StopServicesCommand(IDebugOutputHelper _debugOutputHelper, IDockerComposeHelper _dockerComposeHelper, IDockerHelper _dockerHelper) : Command<StopServicesCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing StopServicesCommand", this);
            
            if (!File.Exists(_dockerComposeHelper.GetFile())) {
                AnsiConsole.MarkupLine($"[red]{_dockerComposeHelper.GetFile()} not found[/]");

                return 0;
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            File.WriteAllText(applicationDir + ".services_stop", "-f " + _dockerComposeHelper.GetFile() + " -p " + _dockerHelper.GetProjectName() + " stop");

            return 0;
        }
    }   
}

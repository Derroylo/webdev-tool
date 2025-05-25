using System;
using System.IO;
using WebDev.Tool.Helper.Docker;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands.Services
{
    internal class StopServicesCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            if (!File.Exists(DockerComposeHelper.GetFile())) {
                AnsiConsole.MarkupLine($"[red]{DockerComposeHelper.GetFile()} not found[/]");

                return 0;
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            File.WriteAllText(applicationDir + ".services_stop", "-f " + DockerComposeHelper.GetFile() + " stop");

            return 0;
        }
    }   
}

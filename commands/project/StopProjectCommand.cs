using System;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Docker;

namespace WebDev.Tool.Commands.Project;

internal class StopProjectCommand: Command
{
    public override int Execute(CommandContext context)
    {
        if (!File.Exists(DockerComposeHelper.GetFile())) {
            AnsiConsole.MarkupLine($"[red]{DockerComposeHelper.GetFile()} not found[/]");

            return 0;
        }

        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        
        File.WriteAllText(applicationDir + ".services_stop", "-f " + DockerComposeHelper.GetFile() + " -p " + DockerHelper.GetProjectName() + " stop");
        
        return 0;
    }
}
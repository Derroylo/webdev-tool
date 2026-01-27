using System;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Docker;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Project;

internal class StopProjectCommand(IDebugOutputHelper _debugOutputHelper, IDockerComposeHelper _dockerComposeHelper, IDockerHelper _dockerHelper) : Command<StopProjectCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing StopProjectCommand", this);
        
        if (!File.Exists(_dockerComposeHelper.GetFile())) {
            AnsiConsole.MarkupLine($"[red]{_dockerComposeHelper.GetFile()} not found[/]");

            return 0;
        }

        var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
        
        File.WriteAllText(applicationDir + ".services_stop", "-f " + _dockerComposeHelper.GetFile() + " -p " + _dockerHelper.GetProjectName() + " stop");
        
        return 0;
    }
}
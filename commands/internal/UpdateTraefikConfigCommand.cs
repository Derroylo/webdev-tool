using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Proxy;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Info;

internal class UpdateTraefikConfigCommand(
    ITraefikHelper _traefikHelper,
    IDebugOutputHelper _debugOutputHelper
): Command<UpdateTraefikConfigCommand.Settings>
{
    public class Settings : LogCommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _debugOutputHelper.WriteInfoOutput("Executing UpdateTraefikConfigCommand", this);
        
        if (!_traefikHelper.CreateTraefikConfig())
        {
            AnsiConsole.MarkupLine("[red]Failed to update the traefik config file[/]");
            return 1;
        }

        AnsiConsole.MarkupLine("[green]The traefik config file has been successfully updated[/]");

        return 0;
    }
}
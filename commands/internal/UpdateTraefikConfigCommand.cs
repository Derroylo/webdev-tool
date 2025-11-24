using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Proxy;

namespace WebDev.Tool.Commands.Info;

internal class UpdateTraefikConfigCommand: Command<UpdateTraefikConfigCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--debug")]
        [Description("Outputs debug information")]
        [DefaultValue(false)]
        public bool Debug { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!TraefikHelper.CreateTraefikConfig(settings.Debug))
        {
            AnsiConsole.MarkupLine("[red]Failed to update the traefik config file[/]");
            return 1;
        }

        AnsiConsole.MarkupLine("[green]The traefik config file has been successfully updated[/]");

        return 0;
    }
}
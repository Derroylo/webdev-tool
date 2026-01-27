using WebDev.Tool.Helper.Internal;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands
{
    class SelfUpdateCommand(IDebugOutputHelper _debugOutputHelper, IUpdateHelper _updateHelper) : Command<SelfUpdateCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
            
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing SelfUpdateCommand", this);
            
            // Force an update check
            var tmp = _updateHelper.GetLatestVersion(true);

            if (!_updateHelper.IsUpdateAvailable()) {
                AnsiConsole.MarkupLine("[red]You already have the latest version[/].");

                return 0;
            }

            AnsiConsole.WriteLine("Downloading the new release...");

            var res = _updateHelper.UpdateToLatestRelease();

            if (!res.Result) {
                AnsiConsole.MarkupLine("[red]Failed to update the application.[/]");
            } else {
                AnsiConsole.MarkupLine("[green1]The application has been successfully updated to the latest version.[/]");
            }
            
            return 0;
        }
    }   
}
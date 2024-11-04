using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands.Apache
{
    class ApacheStatusCommand : Command<ApacheStatusCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.WriteLine(ExecCommand.Exec("apachectl status"));

            return 0;
        }
    }   
}

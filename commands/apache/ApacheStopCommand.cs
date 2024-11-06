using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands.Apache
{
    internal class ApacheStopCommand : Command<ApacheStopCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.WriteLine(ExecCommand.Exec("apachectl stop"));

            return 0;
        }
    }   
}

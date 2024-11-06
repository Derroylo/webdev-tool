using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands.Apache
{
    internal class ApacheRestartCommand : Command<ApacheRestartCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.WriteLine(ExecCommand.Exec("apachectl restart"));

            return 0;
        }
    }   
}

using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Apache
{
    internal class ApacheStopCommand(IDebugOutputHelper _debugOutputHelper, ExecCommand _execCommand) : Command<ApacheStopCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing ApacheStopCommand", this);
            
            AnsiConsole.WriteLine(_execCommand.Exec("apachectl stop"));

            return 0;
        }
    }   
}

using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Apache
{
    internal class ApacheStatusCommand(
        IDebugOutputHelper _debugOutputHelper,
        ExecCommand _execCommand
    ) : Command<ApacheStatusCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing ApacheStatusCommand", this);
            
            AnsiConsole.WriteLine(_execCommand.Exec("apachectl status"));

            return 0;
        }
    }   
}

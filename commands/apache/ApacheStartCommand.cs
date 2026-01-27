using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Apache
{
    internal class ApacheStartCommand(IDebugOutputHelper _debugOutputHelper, ExecCommand _execCommand) : Command<ApacheStartCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing ApacheStartCommand", this);
            
            AnsiConsole.WriteLine(_execCommand.Exec("apachectl start"));

            return 0;
        }
    }   
}

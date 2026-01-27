using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Apache
{
    internal class ApacheRestartCommand(IDebugOutputHelper _debugOutputHelper, ExecCommand _execCommand) : Command<ApacheRestartCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing ApacheRestartCommand", this);
            
            AnsiConsole.WriteLine(_execCommand.Exec("apachectl restart"));

            return 0;
        }
    }   
}

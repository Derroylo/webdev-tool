using Spectre.Console;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands
{
    internal class NotYetImplementedCommand : Command<NotYetImplementedCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.WriteLine("This command has not been implemented yet. Sorry.");

            return 0;
        }
    }   
}

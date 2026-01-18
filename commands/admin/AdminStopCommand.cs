using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Admin;

namespace WebDev.Tool.Commands.Admin
{
    internal class AdminStopCommand : Command<AdminStopCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AdminHelper.StopAdmin();

            return 0;
        }
    }   
}

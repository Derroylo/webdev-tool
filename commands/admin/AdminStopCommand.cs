using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Admin;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Admin
{
    internal class AdminStopCommand(IDebugOutputHelper _debugOutputHelper, IAdminHelper _adminHelper) : Command<AdminStopCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing AdminStopCommand", this);
            
            _adminHelper.StopAdmin();

            return 0;
        }
    }   
}

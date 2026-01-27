using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Admin;
using System.ComponentModel;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Admin
{
    internal class AdminStartCommand(IDebugOutputHelper _debugOutputHelper, IAdminHelper _adminHelper) : Command<AdminStartCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
            [CommandOption("--port")]
            [Description("The port to use for the admin interface")]
            [DefaultValue(8000)]
            public int Port { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing AdminStartCommand", this);
            
            _adminHelper.StartAdmin(settings.Port);

            return 0;
        }
    }   
}

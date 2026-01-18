using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Admin;
using System.ComponentModel;

namespace WebDev.Tool.Commands.Admin
{
    internal class AdminStartCommand : Command<AdminStartCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("--port")]
            [Description("The port to use for the admin interface")]
            [DefaultValue(8000)]
            public int Port { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AdminHelper.StartAdmin(settings.Port);

            return 0;
        }
    }   
}

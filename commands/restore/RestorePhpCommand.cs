using System.ComponentModel;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands.Restore
{
    internal class RestorePhpCommand : Command<RestorePhpCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            RestoreHelper.RestorePhpVersion(settings.Debug);
            RestoreHelper.RestorePhpIni(settings.Debug);
            
            return 0;
        }
    }
}
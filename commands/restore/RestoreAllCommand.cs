using System.ComponentModel;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands.Restore
{
    internal class RestoreAllCommand : Command<RestoreAllCommand.Settings>
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
            RestoreHelper.RestoreEnvVariables(settings.Debug);

            RestoreHelper.RestorePhpVersion(settings.Debug);
            RestoreHelper.RestorePhpIni(settings.Debug);
            
            RestoreHelper.RestoreNodeJsVersion(settings.Debug);

            return 0;
        }
    }
}
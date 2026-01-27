using System.ComponentModel;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Restore
{
    internal class RestoreAllCommand(IDebugOutputHelper _debugOutputHelper, IRestoreHelper _restoreHelper) : Command<RestoreAllCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing RestoreAllCommand", this);
            
            _restoreHelper.RestoreEnvVariables();

            _restoreHelper.RestorePhpVersion();
            _restoreHelper.RestorePhpIni();
            
            _restoreHelper.RestoreNodeJsVersion();

            return 0;
        }
    }
}
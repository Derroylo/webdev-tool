using System.ComponentModel;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Restore
{
    internal class RestorePhpCommand(IDebugOutputHelper _debugOutputHelper, IRestoreHelper _restoreHelper) : Command<RestorePhpCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing RestorePhpCommand", this);
            
            _restoreHelper.RestorePhpVersion();
            _restoreHelper.RestorePhpIni();
            
            return 0;
        }
    }
}
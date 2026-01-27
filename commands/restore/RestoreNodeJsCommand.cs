using System.ComponentModel;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Restore
{
    internal class RestoreNodeJsCommand(IDebugOutputHelper _debugOutputHelper, IRestoreHelper _restoreHelper) : Command<RestoreNodeJsCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing RestoreNodeJsCommand", this);
            
            _restoreHelper.RestoreNodeJsVersion();
            
            return 0;
        }
    }
}
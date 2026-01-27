using System.ComponentModel;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Restore
{
    internal class RestoreEnvCommand(IDebugOutputHelper _debugOutputHelper, IRestoreHelper _restoreHelper) : Command<RestoreEnvCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing RestoreEnvCommand", this);
            
            _restoreHelper.RestoreEnvVariables();
            
            return 0;
        }
    }
}
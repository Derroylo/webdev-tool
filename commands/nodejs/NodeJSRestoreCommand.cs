using System.ComponentModel;
using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.NodeJS
{
    internal class NodeJSRestoreCommand(IDebugOutputHelper _debugOutputHelper, IRestoreHelper _restoreHelper) : Command<NodeJSRestoreCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing NodeJSRestoreCommand", this);
            
            _restoreHelper.RestoreNodeJsVersion();
            
            return 0;
        }
    }
}
using System.ComponentModel;
using WebDev.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace WebDev.Tool.Commands.NodeJS
{
    internal class NodeJSRestoreCommand : Command<NodeJSRestoreCommand.Settings>
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
            //RestoreHelper.RestoreNodeJsVersion(settings.Debug);
            AnsiConsole.WriteLine("Not implemented yet");
            
            return 0;
        }
    }
}
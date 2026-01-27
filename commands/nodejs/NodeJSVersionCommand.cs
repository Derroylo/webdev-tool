using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;
using System.ComponentModel;
using WebDev.Tool.Helper.NodeJs;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.ModeJS
{
    internal class NodeJSVersionCommand(IDebugOutputHelper _debugOutputHelper, INodeJsVersionHelper _nodeJsVersionHelper) : Command<NodeJSVersionCommand.Settings>
    {
        private Settings settings;

        public class Settings : LogCommandSettings
        {
            [CommandArgument(0, "[Version]")]
            [Description("Set this parameter to change the active nodejs version. Leave this parameter empty to show the current version.")]
            public string Version { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing NodeJSVersionCommand", this);
            
            this.settings = settings;

            if (this.settings.Version != null) {
                _nodeJsVersionHelper.SetNewNodeJSVersion(this.settings.Version);

                return 0;
            }

            string result = _nodeJsVersionHelper.GetCurrentNodeJSVersion();
            AnsiConsole.WriteLine(result);

            if (!AnsiConsole.Confirm("Do you want to change the active nodejs version?", false)) {
                return 0;
            }

            var availableNodeJSVersions = _nodeJsVersionHelper.GetAvailableNodeJSVersions();

            var newVersion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the new active NodeJS version")
                    .PageSize(10)
                    .AddChoices(availableNodeJSVersions.ToArray<string>())
            );

            _nodeJsVersionHelper.SetNewNodeJSVersion(newVersion);

            return 0;
        }        
    }
}
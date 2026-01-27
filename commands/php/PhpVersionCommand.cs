using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;
using System.ComponentModel;
using WebDev.Tool.Helper.Php;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Php
{
    internal class PhpVersionCommand(IDebugOutputHelper _debugOutputHelper, IPhpHelper _phpHelper, IPhpVersionHelper _phpVersionHelper) : Command<PhpVersionCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
            [CommandArgument(0, "[Version]")]
            [Description("Set this parameter to change the active PHP version. Leave this parameter empty to show the current version.")]
            public string Version { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing PhpVersionCommand", this);
            
            if (settings.Version != null) {
                _phpHelper.SetNewPhpVersion(settings.Version);

                return 0;
            }

            string result = _phpVersionHelper.GetCurrentPhpVersionOutput();
            AnsiConsole.WriteLine(result);

            if (!AnsiConsole.Confirm("Do you want to change the active php version?", false)) {
                return 0;
            }

            var availablePhpVersions = _phpVersionHelper.GetAvailablePhpVersions();

            var newVersion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the new active php version")
                    .PageSize(5)
                    .AddChoices(availablePhpVersions.ToArray<string>())
            );

            _phpHelper.SetNewPhpVersion(newVersion);

            return 0;
        }        
    }
}
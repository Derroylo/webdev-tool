using System;
using System.ComponentModel;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands
{
    internal class DebugCommand(IDebugOutputHelper _debugOutputHelper) : Command<DebugCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing DebugCommand", this);
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            var debugOutputFile = Path.Combine(applicationDir, ".debug_enabled");
            bool debugModeEnabled = File.Exists(debugOutputFile);

            AnsiConsole.WriteLine("");
            AnsiConsole.MarkupLine("To find out in detail what the tool is doing or to find the source of a problem, you can either append '--debug' to the command or enable debug mode globally with this command. This means it will be enabled for all executions of this tool, via create/start commands in your devcontainer.json or from a task in your webdev.yml, until you disable it again.");
            AnsiConsole.WriteLine("");
            AnsiConsole.WriteLine("Debug mode is currently " + (debugModeEnabled ? "enabled" : "disabled"));

            if (debugModeEnabled) 
            {
                bool disableDebugOutput = AnsiConsole.Confirm("Do you want to disable debug output?", true);

                if (disableDebugOutput)
                {
                    File.Delete(debugOutputFile);
                    AnsiConsole.WriteLine("Debug output disabled");
                }
            }
            else
            {
                bool enableDebugOutput = AnsiConsole.Confirm("Do you want to enable debug output?", true);

                if (enableDebugOutput)
                {
                    File.Create(debugOutputFile);
                    AnsiConsole.WriteLine("Debug output enabled");
                }
            }

            AnsiConsole.WriteLine("Debug mode is now " + (debugModeEnabled ? "enabled" : "disabled"));

            return 0;
        }
    }   
}

using System.ComponentModel;
using Spectre.Console;
using WebDev.Tool.Helper;
using Spectre.Console.Cli;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Commands.Php
{
    internal class PhpRestoreCommand(IDebugOutputHelper _debugOutputHelper, IRestoreHelper _restoreHelper) : Command<PhpRestoreCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing PhpRestoreCommand", this);
            
            _restoreHelper.RestorePhpVersion();
            _restoreHelper.RestorePhpIni();

            return 0;
        }
    }
}
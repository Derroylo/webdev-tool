using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using WebDev.Tool.Classes;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Helper;

namespace WebDev.Tool.Commands.Shell
{
    internal class ShellFileCommand : Command<ShellFileCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[args]")]
            public string[] Arguments { get; set; }

            [CommandOption("-a|--args")]
            [Description("Show available arguments")]
            [DefaultValue(false)]
            public bool ShowArguments { get; set; }
        }

        private void ShowShellFileArguments(CustomCommand cmd)
        {
            if (cmd.Arguments.Count == 0) {
                AnsiConsole.WriteLine("The Script has no defined Arguments");

                return;
            }

            AnsiConsole.WriteLine("The Script has the following Arguments:");

            foreach (string arg in cmd.Arguments) {
                AnsiConsole.WriteLine(arg);
            }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            CustomCommand cmd = (CustomCommand) context.Data;

            if (settings.ShowArguments) {
                ShowShellFileArguments(cmd);

                return 0;
            }

            try
            {
                // Make sure the file is executable
                var proc = new Process();
                var procStartInfo = new ProcessStartInfo()
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = $"/bin/bash",
                    WorkingDirectory = "./",
                    Arguments = $"-c \" chmod +x " + cmd.File + "\""
                };

                proc.StartInfo = procStartInfo;
                proc.Start();
                proc.WaitForExit();

                var args = string.Empty;

                if (settings.Arguments != null && settings.Arguments.Length > 0) {
                    for (int i = 0; i < settings.Arguments.Length; i++) {
                        settings.Arguments[i] = "\\\"" + settings.Arguments[i] + "\\\"";
                    }

                    args = string.Join(' ', settings.Arguments);
                }

                var workingDirectory = "./";

                if (cmd.WorkspaceFolder != null)
                {
                    workingDirectory = cmd.WorkspaceFolder;
                }

                ExecCommand.ExecWithDirectOutput(cmd.File + (args != String.Empty ? " " + args : ""), false, false, workingDirectory);
            }
            catch(Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }

            return 0;
        }
    }   
}

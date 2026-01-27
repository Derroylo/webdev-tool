using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using WebDev.Tool.Classes;
using Spectre.Console;
using Spectre.Console.Cli;
using WebDev.Tool.Classes.Configuration;
using WebDev.Tool.Helper;
using WebDev.Tool.Helper.Internal;
using WebDev.Tool.Helper.Internal.Config.Sections;
using System.Linq;

namespace WebDev.Tool.Commands.Shell
{
    public class TestsCommand(IDebugOutputHelper _debugOutputHelper, IEnvironmentHelper _environmentHelper, PhpConfig _phpConfig, TestsConfig _testsConfig, ExecCommand _execCommand) : Command<TestsCommand.Settings>
    {
        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            _debugOutputHelper.WriteInfoOutput("Executing TestsCommand", this);
            
            TestEntryConfiguration test = (TestEntryConfiguration) context.Data;
            
            _debugOutputHelper.WriteInfoOutput("Running test: " + test.Name, this);

            if (test.Commands.Count == 0 && test.Tests.Count == 0) {
                AnsiConsole.MarkupLine($"[red]The Test \"{test.Name}\" has no defined commands[/]");

                return 0;
            }

            if (_environmentHelper.IsRunningInDevContainer()) {
                RunTestInsideDevContainer(test);
            } else {
                RunTestOutsideDevContainer(test);
            }

            return 0;
        }

        private void RunTestOutsideDevContainer(TestEntryConfiguration test)
        {
            // Run the test commands inside a php:8.2-cli-alpine container and show their output

            // Build the docker run command to execute the test commands sequentially via 'sh -c'
            // Mount the current directory to /app for file access if needed by the test commands
            // Use interactive tty for better output rendering
            // Compose all test commands into a single shell script line: "cmd1 && cmd2 && ..."
            string workDir = Directory.GetCurrentDirectory().Replace("\\", "/");
            string dockerImage = test.Image;

            if (string.IsNullOrEmpty(dockerImage)) {
                dockerImage = "ghcr.io/derroylo/docker-images/php-alpine:" + _phpConfig.PhpVersion;
            }

            // Join the commands for sh -c execution
            string allCommands = GetTestCommands(test);

            AnsiConsole.MarkupLine($"[green]Running test \"{test.Name}\" with commands:[/]");
            AnsiConsole.MarkupLine($"[green]{allCommands}[/]");

            var dockerCommand = $"docker run --rm --user 1000:1000 -v \"{workDir}:/app\" -w /app {dockerImage} sh -c \"{allCommands.Replace("\"", "\\\"")}\"";
            
            _execCommand.ExecWithDirectOutput(dockerCommand, false, true, "", useStreaming: true);
        }

        private void RunTestInsideDevContainer(TestEntryConfiguration test)
        {
            string allCommands = GetTestCommands(test);

            _execCommand.ExecWithDirectOutput(allCommands, true, true);
        }

        private string GetTestCommands(TestEntryConfiguration test)
        {
            string commands = "";
            string arguments = "";

            if (Program.ProgramArgs.Count > 2) {
                arguments = string.Join(" ", Program.ProgramArgs.Skip(2));
            }

            foreach (string testName in test.Tests) {
                if (_testsConfig.Tests.TryGetValue(testName, out TestEntryConfiguration testEntry)) {
                    commands += GetTestCommands(testEntry) + " && ";
                }
            }

            foreach (string cmd in test.Commands) {
                commands += cmd + (arguments != "" ? " " + arguments : "") + " && ";
            }

            return commands.TrimEnd(" &".ToCharArray()).Replace("\"", "\\\"");
        }
    }
}
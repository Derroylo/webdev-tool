using System.IO;
using System.Collections.Generic;
using WebDev.Tool.Classes;
using System.Text.RegularExpressions;

namespace WebDev.Tool.Helper.Internal
{
    internal class CustomCommandsLoader
    {
        public static Dictionary<string, CustomBranch> Load()
        {           
            var scripts = SearchForShellScripts(".devcontainer/scripts");

            /*if (ShellScriptConfig.AdditionalDirectories.Count > 0) {
                foreach (string folder in ShellScriptConfig.AdditionalDirectories) {
                    scripts = SearchForShellScripts(folder, scripts);
                }
            }*/

            return scripts;
        }

        private static Dictionary<string, CustomBranch> SearchForShellScripts(string folder, Dictionary<string, CustomBranch> commands = null)
        {
            if (commands == null) {
                var defaultBranch = new CustomBranch("default");
                commands = new()
                {
                    // Add the default branch (commands that have no specified branch)
                    { defaultBranch.Name, defaultBranch }
                };
            }

            if (!Directory.Exists(folder)) {
                return commands;
            }

            string[] files = Directory.GetFiles(folder, "*.sh");
            foreach (string file in files) {
                var shellScriptSettings = ProcessShellScript(file);
                
                if (shellScriptSettings == null) {
                    continue;
                }

                var newCustomCommand = new CustomCommand(shellScriptSettings.Command, file, shellScriptSettings.Description, shellScriptSettings.Arguments);

                if (shellScriptSettings.Branch != string.Empty) {
                    if (commands.TryGetValue(shellScriptSettings.Branch, out CustomBranch customBranch)) {
                        customBranch.Commands.Add(newCustomCommand);
                    } else {
                        var newBranch = new CustomBranch(shellScriptSettings.Branch, shellScriptSettings.BranchDescription);
                        newBranch.Commands.Add(newCustomCommand);

                        commands.Add(newBranch.Name, newBranch);
                    }
                } else {
                    commands["default"].Commands.Add(newCustomCommand);
                }
            }

            // Check for subdirectories
            string [] subDirectories = Directory.GetDirectories(folder);
            foreach(string subdirectory in subDirectories) {
                commands = SearchForShellScripts(subdirectory, commands);
            }
            
            return commands;
        }

        private static ShellScriptSettings ProcessShellScript(string fileWithPath)
        {
            string[] lines = File.ReadAllLines(fileWithPath);

            string command = string.Empty;
            string description = string.Empty;
            string branch = string.Empty;
            string branchDescription = string.Empty;
            List<string> args = new();

            string commandPattern =  @"\# webDevCommand: ([a-zA-Z0-9-_]+)";
            string descriptionPattern =  @"\# webDevDescription: ([a-zA-Z0-9-_ ,]+)";
            string branchPattern =  @"\# webDevBranch: ([a-zA-Z0-9-_]+)";
            string branchDescriptionPattern =  @"\# webDevBranchDescription: ([a-zA-Z0-9-_ ,]+)";
            string argsPattern =  @"\# webDevArgument: ([a-zA-Z0-9-_ ,]+)";

            Regex commandRegex = new(commandPattern);
            Regex descriptionRegex = new(descriptionPattern);
            Regex branchRegex = new(branchPattern);
            Regex branchDescriptionRegex = new(branchDescriptionPattern);
            Regex argsRegex = new(argsPattern);

            foreach (string line in lines) {
                if (commandRegex.IsMatch(line)) {
                    Match match = commandRegex.Match(line);

                    command = match.Groups[1].Value;
                }

                if (descriptionRegex.IsMatch(line)) {
                    Match match = descriptionRegex.Match(line);

                    description = match.Groups[1].Value;
                }

                if (branchRegex.IsMatch(line)) {
                    Match match = branchRegex.Match(line);

                    branch = match.Groups[1].Value;
                }

                if (branchDescriptionRegex.IsMatch(line)) {
                    Match match = branchDescriptionRegex.Match(line);

                    branchDescription = match.Groups[1].Value;
                }

                if (argsRegex.IsMatch(line)) {
                    Match match = argsRegex.Match(line);
                    args.Add(match.Groups[1].Value);
                }
            }

            return command != string.Empty ? new ShellScriptSettings(command, description, branch, branchDescription, args) : null;
        }
    }
}

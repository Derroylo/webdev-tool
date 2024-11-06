using System;
using System.Collections.Generic;

namespace WebDev.Tool.Classes
{
    internal class ShellScriptSettings
    {
        public string Branch { get; } = string.Empty;

        public string BranchDescription { get; } = string.Empty;

        public string Command { get; } = string.Empty;

        public string Description { get; } = string.Empty;

        public List<string> Arguments { get; } = new();

        public ShellScriptSettings(string command, string description = null, string branch = null, string branchDescription = null, List<string> arguments = null) {
            if (string.IsNullOrEmpty(command) || command.Length < 1) {
                throw new Exception("Missing command");
            }

            this.Command = command;

            if (description != null && description.Length > 0) {
                this.Description = description;
            }

            if (branch != null && branch.Length > 0) {
                this.Branch = branch;
            }

            if (branchDescription != null && branchDescription.Length > 0) {
                this.BranchDescription = branchDescription;
            }

            if (arguments != null && arguments.Count > 0) {
                this.Arguments = arguments;
            }
        }
    }
}
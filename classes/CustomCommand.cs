using System;
using System.Collections.Generic;

namespace WebDev.Tool.Classes
{
    internal class CustomCommand
    {
        public string Command { get; } = string.Empty;

        public string File { get; } = string.Empty;

        public string Description { get; } = string.Empty;

        public List<string> Arguments { get; } = new();

        public string WorkspaceFolder { get; } = null;
        
        public CustomCommand(string command, string file, string description = null, List<string> arguments = null, string workspaceFolder = null) {
            if (string.IsNullOrEmpty(command) || command.Length < 1) {
                throw new Exception("Missing command for custom command");
            }

            if (string.IsNullOrEmpty(file) || file.Length < 4) {
                throw new Exception("Missing file for custom command");
            }

            Command = command;
            File = file;
            WorkspaceFolder = workspaceFolder;

            if (description != null && description.Length > 0) {
                Description = description;
            }

            if (arguments != null && arguments.Count > 0) {
                Arguments = arguments;
            }
        }
    }
}
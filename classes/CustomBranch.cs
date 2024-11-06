using System;
using System.Collections.Generic;

namespace WebDev.Tool.Classes
{
    internal class CustomBranch
    {
        public string Name { get; } = string.Empty;

        public string Description { get; } = string.Empty;

        public List<CustomCommand> Commands { get; } = new();

        public CustomBranch(string name, string description = null) {
            if (string.IsNullOrEmpty(name) || name.Length < 1) {
                throw new Exception("Missing name for custom branch");
            }

            this.Name = name;

            if (description != null && description.Length > 0) {
                this.Description = description;
            }
        }
    }
}
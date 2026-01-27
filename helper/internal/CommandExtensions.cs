using System;
using System.Collections.Generic;
using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;

namespace WebDev.Tool.Helper.Internal
{
    internal static class CommandExtensions
    {
        private static readonly HashSet<string> _runOnlyInDevcontainerCommands = new();
        private static readonly HashSet<string> _runOnlyInDevcontainerBranches = new();

        private static readonly HashSet<string> _runOnlyOnHostCommands = new();
        private static readonly HashSet<string> _runOnlyOnHostBranches = new();

        private static readonly HashSet<string> _commonCommands = new();
        private static readonly HashSet<string> _commonBranches = new();

        private static readonly HashSet<string> _showBranchCommands = new();

#region Branches Setter
        public static IBranchConfigurator RunOnlyInDevcontainer(this IBranchConfigurator configurator)
        {
            var branchName = GetBranchName(configurator);

            if (!string.IsNullOrEmpty(branchName))
            {
                _runOnlyInDevcontainerBranches.Add(branchName);
            } else {
                AnsiConsole.MarkupLine($"[red]Failed to get the branch name[/]");
            }
            
            return configurator;
        }

        public static IBranchConfigurator RunOnlyOnHost(this IBranchConfigurator configurator)
        {
            var branchName = GetBranchName(configurator);

            if (!string.IsNullOrEmpty(branchName))
            {
                _runOnlyOnHostBranches.Add(branchName);
            } else {
                AnsiConsole.MarkupLine($"[red]Failed to get the branch name[/]");
            }
            
            return configurator;
        }

        public static IBranchConfigurator ShowInCommonCommands(this IBranchConfigurator configurator)
        {
            var branchName = GetBranchName(configurator);

            if (!string.IsNullOrEmpty(branchName))
            {
                _commonBranches.Add(branchName);
            } else {
                AnsiConsole.MarkupLine($"[red]Failed to get the branch name[/]");
            }

            return configurator;
        }

        public static IBranchConfigurator ShowAllCommandsDirectly(this IBranchConfigurator configurator)
        {
            var branchName = GetBranchName(configurator);

            if (!string.IsNullOrEmpty(branchName))
            {
                _showBranchCommands.Add(branchName);
            } else {
                AnsiConsole.MarkupLine($"[red]Failed to get the branch name[/]");
            }

            return configurator;
        }
#endregion

#region Commands Setter
        public static ICommandConfigurator RunOnlyInDevcontainer(this ICommandConfigurator configurator, string parentBranchName = null)
        {
            var commandName = GetCommandName(configurator);

            if (!string.IsNullOrEmpty(commandName))
            {
                _runOnlyInDevcontainerCommands.Add(parentBranchName != null ? parentBranchName + ":" + commandName : commandName);
            } else {
                AnsiConsole.MarkupLine($"[red]Failed to get the command name[/]");
            }

            return configurator;
        }

        public static ICommandConfigurator RunOnlyOnHost(this ICommandConfigurator configurator, string parentBranchName = null)
        {
            var commandName = GetCommandName(configurator);

            if (!string.IsNullOrEmpty(commandName))
            {
                _runOnlyOnHostCommands.Add(parentBranchName != null ? parentBranchName + ":" + commandName : commandName);
            } else {
                AnsiConsole.MarkupLine($"[red]Failed to get the command name[/]");
            }

            return configurator;
        }

        public static ICommandConfigurator ShowInCommonCommands(this ICommandConfigurator configurator, string parentBranchName = null)
        {
            var commandName = GetCommandName(configurator);

            if (!string.IsNullOrEmpty(commandName))
            {
                _commonCommands.Add(parentBranchName != null ? parentBranchName + ":" + commandName : commandName);
            }
            else {
                AnsiConsole.MarkupLine($"[red]Failed to get the command name[/]");
            }

            return configurator;
        }

        public static bool ShowAllSubCommandsDirectly(string branchName)
        {
            return _showBranchCommands.Contains(branchName);
        }
#endregion

#region Getter
        public static bool IsRunOnlyInDevcontainer(string commandOrBranchName, bool isCommand = true)
        {
            if (isCommand)
            {
                return _runOnlyInDevcontainerCommands.Contains(commandOrBranchName);
            }
            else
            {
                return _runOnlyInDevcontainerBranches.Contains(commandOrBranchName);
            }
        }

        public static bool IsCommon(string commandOrBranchName, bool isCommand = true)
        {
            if (isCommand)
            {
                return _commonCommands.Contains(commandOrBranchName);
            }
            else
            {
                return _commonBranches.Contains(commandOrBranchName);
            }
        }

        public static bool IsRunOnlyOnHost(string commandOrBranchName, bool isCommand = true)
        {
            if (isCommand)
            {
                return _runOnlyOnHostCommands.Contains(commandOrBranchName);
            }
            else
            {
                return _runOnlyOnHostBranches.Contains(commandOrBranchName);
            }
        }
#endregion

        private static string GetCommandName(ICommandConfigurator configurator)
        {
            string commandName = null;

            if (configurator is ICommandInfo commandInfo)
            {
                return commandInfo.Name;
            }

            // Option 2: Prüfe ob die Command-Property den Branch-Namen enthält
            // Der BranchConfigurator hat eine Command-Property vom Typ ConfiguredCommand
            // ConfiguredCommand hat eine Name-Property die den Branch-Namen enthält
            var configuratorType = configurator.GetType();
            var commandProperty = configuratorType.GetProperty("Command",
                BindingFlags.Public | BindingFlags.Instance);
            
            if (commandProperty != null)
            {
                var commandValue = commandProperty.GetValue(configurator);
                
                // Versuche zuerst über ICommandInfo Interface
                if (commandValue is ICommandInfo commandInfoFromProperty)
                {
                    commandName = commandInfoFromProperty.Name;
                }
                // Fallback: Direkter Zugriff auf Name-Property via Reflection
                else
                {
                    var commandNameProperty = commandValue?.GetType()?.GetProperty("Name",
                        BindingFlags.Public | BindingFlags.Instance);
                    
                    if (commandNameProperty != null)
                    {
                        commandName = commandNameProperty.GetValue(commandValue) as string;
                    }
                }
            }

            return commandName;
        }

        private static string GetBranchName(IBranchConfigurator configurator)
        {
            string branchName = null;
    
            // Option 1: Prüfe ob IBranchConfigurator auch ICommandInfo ist
            if (configurator is ICommandInfo commandInfo)
            {
                return commandInfo.Name;
            }

            // Option 2: Prüfe ob die Command-Property den Branch-Namen enthält
            // Der BranchConfigurator hat eine Command-Property vom Typ ConfiguredCommand
            // ConfiguredCommand hat eine Name-Property die den Branch-Namen enthält
            var configuratorType = configurator.GetType();
            var commandProperty = configuratorType.GetProperty("Command",
                BindingFlags.Public | BindingFlags.Instance);
            
            if (commandProperty != null)
            {
                var commandValue = commandProperty.GetValue(configurator);
                
                // Versuche zuerst über ICommandInfo Interface
                if (commandValue is ICommandInfo commandInfoFromProperty)
                {
                    branchName = commandInfoFromProperty.Name;
                }
                // Fallback: Direkter Zugriff auf Name-Property via Reflection
                else
                {
                    var commandNameProperty = commandValue?.GetType()?.GetProperty("Name",
                        BindingFlags.Public | BindingFlags.Instance);
                    
                    if (commandNameProperty != null)
                    {
                        branchName = commandNameProperty.GetValue(commandValue) as string;
                    }
                }
            }

            return branchName;
        }
    }
}

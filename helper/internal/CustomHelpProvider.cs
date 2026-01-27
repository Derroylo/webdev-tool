using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace WebDev.Tool.Helper.Internal
{
    internal class CustomHelpProvider(IEnvironmentHelper _environmentHelper, ICommandAppSettings _settings) : IHelpProvider
    {
        public IEnumerable<IRenderable> Write(ICommandModel model, ICommandInfo commandInfo = null)
        {
            var args = Program.ProgramArgs;
            var showAll = args.Contains("--help") || args.Contains("-h");

            yield return new Markup("\n");
            yield return new Markup("USAGE:\n");
            yield return new Markup($"  {_settings.ApplicationName} [[OPTIONS]] [[COMMAND]]\n");
            yield return new Markup("\n");

            yield return new Markup("OPTIONS:\n");
            yield return new Markup("  [bold]-h, --help[/]           Show all commands\n");
            yield return new Markup("  [bold]--version[/]            Show version information\n");
            yield return new Markup("  [bold]--debug[/]              Output debug information\n");
            yield return new Markup("  [bold]--no-header[/]          Disable the program header\n");
            yield return new Markup("\n");
            
            foreach (var renderable in WriteGroupedCommands(model, showAll))
            {
                yield return renderable;
            }
        }

        private IEnumerable<IRenderable> WriteGroupedCommands(ICommandModel model, bool showAll)
        {
            var allCommands = model.Commands.Where(cmd => !cmd.IsHidden).ToList();

            yield return new Markup("COMMANDS:\n");
            
            // Filter and sort all commands (branches and standalone) together alphabetically
            var filteredCommands = allCommands
                .Where(cmd =>
                {
                    // Check if it's a branch or standalone command
                    bool isBranch = cmd is ICommandContainer container && container.Commands.Any();
                    
                    // Apply common filter and availability check
                    bool isCommon = isBranch 
                        ? (CommandExtensions.IsCommon(cmd.Name, false) || showAll)
                        : (CommandExtensions.IsCommon(cmd.Name) || showAll);
                    
                    bool isAvailable = isBranch
                        ? IsAvailableInCurrentEnv(CommandExtensions.IsRunOnlyInDevcontainer(cmd.Name, false), CommandExtensions.IsRunOnlyOnHost(cmd.Name, false))
                        : IsAvailableInCurrentEnv(CommandExtensions.IsRunOnlyInDevcontainer(cmd.Name), CommandExtensions.IsRunOnlyOnHost(cmd.Name));
                    
                    return isCommon && isAvailable;
                })
                .OrderBy(c => c.Name)
                .ToList();

            // Output all commands and branches together, sorted alphabetically
            foreach (var cmd in filteredCommands)
            {
                // Check if it's a branch (has children) or standalone command
                if (cmd is ICommandContainer container && container.Commands.Any())
                {
                    // It's a branch - use WriteBranch method
                    foreach (var renderable in WriteBranch(cmd, CommandExtensions.ShowAllSubCommandsDirectly(cmd.Name), showAll, true))
                    {
                        yield return renderable;
                    }
                }
                else
                {
                    // It's a standalone command
                    yield return new Markup($"  [bold]{cmd.Name.PadRight(20)}[/] {cmd.Description ?? "No description available"}\n");
                }
            }

            yield return new Markup("\n");

            // Filter and sort all commands (branches and standalone) together alphabetically
            var notAvailableCommands = allCommands
                .Where(cmd =>
                {
                    // Check if it's a branch or standalone command
                    bool isBranch = cmd is ICommandContainer container && container.Commands.Any();
                    
                    // Apply common filter and availability check
                    bool isCommon = isBranch 
                        ? (CommandExtensions.IsCommon(cmd.Name, false) || showAll)
                        : (CommandExtensions.IsCommon(cmd.Name) || showAll);
                    
                    bool isAvailable = isBranch
                        ? IsAvailableInCurrentEnv(CommandExtensions.IsRunOnlyInDevcontainer(cmd.Name, false), CommandExtensions.IsRunOnlyOnHost(cmd.Name, false))
                        : IsAvailableInCurrentEnv(CommandExtensions.IsRunOnlyInDevcontainer(cmd.Name), CommandExtensions.IsRunOnlyOnHost(cmd.Name));
                    
                    return isCommon && !isAvailable;
                })
                .OrderBy(c => c.Name)
                .ToList();

            if (notAvailableCommands.Count > 0)
            {
                // Show entries that are currently not available in the current environment
                yield return new Markup($"[orange3]These commands are currently not available because they can only run {(!_environmentHelper.IsRunningInDevContainer() ? "in the devcontainer" : "on the host")}[/]:\n");

                // Output all commands and branches together, sorted alphabetically
                foreach (var cmd in notAvailableCommands)
                {
                    // Check if it's a branch (has children) or standalone command
                    if (cmd is ICommandContainer container && container.Commands.Any())
                    {
                        // It's a branch - use WriteBranch method
                        foreach (var renderable in WriteBranch(cmd, false))
                        {
                            yield return renderable;
                        }
                    }
                    else
                    {
                        // It's a standalone command
                        yield return new Markup($"  [bold]{cmd.Name.PadRight(20)}[/] {cmd.Description ?? "No description available"}\n");
                    }
                }
            }
        }

        private IEnumerable<IRenderable> WriteBranch(ICommandInfo branchInfo, bool showSubcommands, bool forceShowAll = false, bool showSubcommandsHint = false)
        {
            var branchName = branchInfo.Name.PadRight(20);
            var description = branchInfo.Description ?? "No description available";
            
            yield return new Markup($"  [bold]{branchName}[/] {description}\n");

            if ((forceShowAll || showSubcommands) && branchInfo is ICommandContainer container)
            {
                var subcommands = container.Commands
                    .Where(cmd => !cmd.IsHidden && (CommandExtensions.IsCommon(cmd.Name) || forceShowAll))
                    .OrderBy(c => c.Name);

                foreach (var subcmd in subcommands)
                {
                    var subName = $"    {subcmd.Name}".PadRight(20);
                    var subDesc = subcmd.Description ?? "No description available";
                    yield return new Markup($"  [dim]{subName}[/] {subDesc}\n");
                }
            }
            else if (showSubcommandsHint)
            {
                yield return new Markup($"  [dim]    Use '{_settings.ApplicationName} {branchInfo.Name}' to see subcommands[/]\n");
            }
        }

        private bool IsAvailableInCurrentEnv(bool isAvailableInDevContainer, bool isAvailableOnHost)
        {
            if (!isAvailableInDevContainer && !isAvailableOnHost)
            {
                return true;
            }

            return _environmentHelper.IsRunningInDevContainer() ? isAvailableInDevContainer : isAvailableOnHost;
        }
    }
}

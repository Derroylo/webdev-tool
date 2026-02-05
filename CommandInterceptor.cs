namespace WebDev.Tool;

using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebDev.Tool.Helper.Internal;

public class CommandInterceptor(IDebugOutputHelper _debugOutputHelper, IEnvironmentHelper _environmentHelper) : ICommandInterceptor
{
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        var debugOutputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".debug_enabled");
        bool debugModeEnabled = File.Exists(debugOutputFile);
        
        // If the command has a log level setting, set the log level
        if (debugModeEnabled || (settings is LogCommandSettings logSettings && logSettings.Debug))
        {
            _debugOutputHelper.EnableDebugOutput();
        }
        else 
        {
            _debugOutputHelper.DisableDebugOutput();
        }

        _debugOutputHelper.WriteInfoOutput("Intercepting command: " + context.Name, this);

        var commandTokens = GetInvokedCommandTokens(Program.ProgramArgs);

        _debugOutputHelper.WriteInfoOutput("Command tokens: " + string.Join(" ", commandTokens), this);

        if (commandTokens.Count == 2) {
            // If the command can run in the current env, return (it is either allowed to run via property or not explicitly set)
            EnsureCommandCanRunInCurrentEnv(commandTokens[0] + ":" + context.Name);
        } else {
            EnsureCommandCanRunInCurrentEnv(context.Name);
        }
    }

    private List<string> GetInvokedCommandTokens(IReadOnlyList<string> args)
    {
        // Global args start with '-' (e.g. --debug, --no-header, etc.). Commands are the first non-option tokens.
        // We only need the first two tokens: <branch> [subcommand] OR <command>.
        var tokens = args
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Where(a => !a.StartsWith("-", StringComparison.Ordinal))
            .Take(2)
            .ToList();

        return tokens;
    }
    
    private bool BranchCanRunInCurrentEnv(string name)
    {
        var runOnlyInDevcontainer = CommandExtensions.IsRunOnlyInDevcontainer(name, false);
        var runOnlyOnHost = CommandExtensions.IsRunOnlyOnHost(name, false);

        _debugOutputHelper.WriteInfoOutput("Ensuring branch can run in current env: " + name + ", runOnlyInDevcontainer: " + runOnlyInDevcontainer + ", runOnlyOnHost: " + runOnlyOnHost, this);

        if (IsAvailableInCurrentEnv(runOnlyInDevcontainer, runOnlyOnHost))
        {
            _debugOutputHelper.WriteInfoOutput("Branch can run in current env: " + name, this);
            return true;
        }

        var expectedEnv = _environmentHelper.IsRunningInDevContainer() ? "on the host" : "in the devcontainer";

        throw new InvalidOperationException($"The branch {name} cannot be executed here. It can only run {expectedEnv}.");
    }

    private void EnsureCommandCanRunInCurrentEnv(string name)
    {
        var runOnlyInDevcontainer = CommandExtensions.IsRunOnlyInDevcontainer(name);
        var runOnlyOnHost = CommandExtensions.IsRunOnlyOnHost(name);

        _debugOutputHelper.WriteInfoOutput("Ensuring command can run in current env: " + name + ", runOnlyInDevcontainer: " + runOnlyInDevcontainer + ", runOnlyOnHost: " + runOnlyOnHost, this);

        if (IsAvailableInCurrentEnv(runOnlyInDevcontainer, runOnlyOnHost))
        {
            _debugOutputHelper.WriteInfoOutput("Command can run in current env: " + name, this);
            return;
        }

        var expectedEnv = _environmentHelper.IsRunningInDevContainer() ? "on the host" : "in the devcontainer";

        throw new InvalidOperationException($"The command {name} cannot be executed here. It can only run {expectedEnv}.");
    }

    private bool IsAvailableInCurrentEnv(bool runOnlyInDevcontainer, bool runOnlyOnHost)
    {
        // If no restriction is set, it's available everywhere.
        if (!runOnlyInDevcontainer && !runOnlyOnHost)
        {
            return true;
        }

        return _environmentHelper.IsRunningInDevContainer() ? runOnlyInDevcontainer : runOnlyOnHost;
    }
}
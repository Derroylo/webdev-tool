using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace WebDev.Tool.Tests.Helpers;

/// <summary>
/// Executes commands within a devcontainer and captures their output
/// </summary>
public class DevContainerCommandExecutor
{
    private readonly ILogger _logger;
    private readonly string _workspaceFolder;
    private readonly int _commandTimeoutSeconds;

    public DevContainerCommandExecutor(
        ILogger logger,
        string workspaceFolder = "/workspaces/webdev-tool/devcontainer-testenv",
        int commandTimeoutSeconds = 30)
    {
        _logger = logger;
        _workspaceFolder = workspaceFolder;
        _commandTimeoutSeconds = commandTimeoutSeconds;
    }

    /// <summary>
    /// Executes a command within the devcontainer
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="arguments">Optional arguments for the command</param>
    /// <returns>Command execution result</returns>
    public async Task<CommandExecutionResult> ExecuteCommandAsync(string command, params string[] arguments)
    {
        var fullCommand = arguments.Length > 0 
            ? $"{command} {string.Join(" ", arguments.Select(arg => $"\"{arg}\""))}"
            : command;

        _logger.LogInformation("Executing command in devcontainer: {Command}", fullCommand);

        var startInfo = new ProcessStartInfo
        {
            FileName = "devcontainer",
            Arguments = $"exec --workspace-folder {_workspaceFolder} {fullCommand}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new System.Text.StringBuilder();
        var error = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
                _logger.LogDebug("STDOUT: {Output}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
                _logger.LogDebug("STDERR: {Error}", e.Data);
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_commandTimeoutSeconds));
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Command timed out after {Timeout} seconds", _commandTimeoutSeconds);
                process.Kill();
                return new CommandExecutionResult
                {
                    ExitCode = -1,
                    StandardOutput = output.ToString(),
                    StandardError = error.ToString(),
                    TimedOut = true
                };
            }

            return new CommandExecutionResult
            {
                ExitCode = process.ExitCode,
                StandardOutput = output.ToString().TrimEnd('\r', '\n'),
                StandardError = error.ToString().TrimEnd('\r', '\n'),
                TimedOut = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Command}", fullCommand);
            return new CommandExecutionResult
            {
                ExitCode = -1,
                StandardOutput = output.ToString(),
                StandardError = error.ToString() + $"\nException: {ex.Message}",
                TimedOut = false
            };
        }
    }

    /// <summary>
    /// Executes a webdev tool command within the devcontainer
    /// </summary>
    /// <param name="arguments">Arguments to pass to the webdev command</param>
    /// <returns>Command execution result</returns>
    public async Task<CommandExecutionResult> ExecuteWebDevCommandAsync(params string[] arguments)
    {
        return await ExecuteCommandAsync("webdev", arguments);
    }

    /// <summary>
    /// Checks if the devcontainer is running and accessible
    /// </summary>
    /// <returns>True if the devcontainer is accessible</returns>
    public async Task<bool> IsDevContainerAccessibleAsync()
    {
        var result = await ExecuteCommandAsync("echo", "test");
        return result.ExitCode == 0;
    }
}

/// <summary>
/// Represents the result of a command execution
/// </summary>
public class CommandExecutionResult
{
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public bool TimedOut { get; set; }
    public bool IsSuccess => ExitCode == 0 && !TimedOut;
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using WebDev.Tool.Tests.Helpers;

namespace WebDev.Tool.Tests.Base;

/// <summary>
/// Base class for devcontainer-based tests
/// Assumes the devcontainer is already running
/// </summary>
public abstract class DevContainerTestBase : IDisposable
{
    protected readonly DevContainerCommandExecutor CommandExecutor;
    protected readonly ILogger Logger;

    protected DevContainerTestBase()
    {
        // Use the shared logger factory and configuration
        Logger = TestUtilities.CreateLogger<DevContainerTestBase>();
        
        var workspaceFolder = TestUtilities.GetTestSetting("DevContainer:WorkspaceFolder", "/workspaces/webdev-tool/devcontainer-testenv");
        var commandTimeout = TestUtilities.GetTestSettingInt("DevContainer:CommandTimeoutSeconds", 30);
        
        CommandExecutor = new DevContainerCommandExecutor(Logger, workspaceFolder, commandTimeout);
    }

    /// <summary>
    /// Asserts that a command execution was successful
    /// </summary>
    /// <param name="result">The command execution result</param>
    /// <param name="message">Optional message for the assertion</param>
    protected void AssertCommandSuccess(CommandExecutionResult result, string? message = null)
    {
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(message ?? "Command should have executed successfully");
        result.TimedOut.Should().BeFalse("Command should not have timed out");
    }

    /// <summary>
    /// Asserts that a command execution failed with a specific exit code
    /// </summary>
    /// <param name="result">The command execution result</param>
    /// <param name="expectedExitCode">The expected exit code</param>
    /// <param name="message">Optional message for the assertion</param>
    protected void AssertCommandFailure(CommandExecutionResult result, int expectedExitCode, string? message = null)
    {
        result.Should().NotBeNull();
        result.ExitCode.Should().Be(expectedExitCode, message ?? $"Command should have failed with exit code {expectedExitCode}");
        result.TimedOut.Should().BeFalse("Command should not have timed out");
    }

    /// <summary>
    /// Asserts that the output contains specific text
    /// </summary>
    /// <param name="result">The command execution result</param>
    /// <param name="expectedText">The text that should be present in the output</param>
    /// <param name="message">Optional message for the assertion</param>
    protected void AssertOutputContains(CommandExecutionResult result, string expectedText, string? message = null)
    {
        result.Should().NotBeNull();
        result.StandardOutput.Should().Contain(expectedText, message ?? $"Output should contain '{expectedText}'");
    }

    /// <summary>
    /// Asserts that the error output contains specific text
    /// </summary>
    /// <param name="result">The command execution result</param>
    /// <param name="expectedText">The text that should be present in the error output</param>
    /// <param name="message">Optional message for the assertion</param>
    protected void AssertErrorContains(CommandExecutionResult result, string expectedText, string? message = null)
    {
        result.Should().NotBeNull();
        result.StandardError.Should().Contain(expectedText, message ?? $"Error output should contain '{expectedText}'");
    }

    /// <summary>
    /// Asserts that the output matches a specific pattern
    /// </summary>
    /// <param name="result">The command execution result</param>
    /// <param name="pattern">The regex pattern to match</param>
    /// <param name="message">Optional message for the assertion</param>
    protected void AssertOutputMatches(CommandExecutionResult result, string pattern, string? message = null)
    {
        result.Should().NotBeNull();
        result.StandardOutput.Should().MatchRegex(pattern, message ?? $"Output should match pattern '{pattern}'");
    }

    /// <summary>
    /// Asserts that the output is empty
    /// </summary>
    /// <param name="result">The command execution result</param>
    /// <param name="message">Optional message for the assertion</param>
    protected void AssertOutputEmpty(CommandExecutionResult result, string? message = null)
    {
        result.Should().NotBeNull();
        result.StandardOutput.Should().BeEmpty(message ?? "Output should be empty");
    }

    /// <summary>
    /// Asserts that the error output is empty
    /// </summary>
    /// <param name="result">The command execution result</param>
    /// <param name="message">Optional message for the assertion</param>
    protected void AssertErrorEmpty(CommandExecutionResult result, string? message = null)
    {
        result.Should().NotBeNull();
        result.StandardError.Should().BeEmpty(message ?? "Error output should be empty");
    }

    /// <summary>
    /// Executes a webdev command and asserts it was successful
    /// </summary>
    /// <param name="arguments">Arguments for the webdev command</param>
    /// <returns>The command execution result</returns>
    protected async Task<CommandExecutionResult> ExecuteWebDevCommandSuccessfullyAsync(params string[] arguments)
    {
        var result = await CommandExecutor.ExecuteWebDevCommandAsync(arguments);
        AssertCommandSuccess(result, $"webdev {string.Join(" ", arguments)} should execute successfully. Output: {result.StandardOutput}. Error: {result.StandardError}");
        return result;
    }

    /// <summary>
    /// Executes a webdev command and asserts it failed with a specific exit code
    /// </summary>
    /// <param name="expectedExitCode">The expected exit code</param>
    /// <param name="arguments">Arguments for the webdev command</param>
    /// <returns>The command execution result</returns>
    protected async Task<CommandExecutionResult> ExecuteWebDevCommandWithFailureAsync(int expectedExitCode, params string[] arguments)
    {
        var result = await CommandExecutor.ExecuteWebDevCommandAsync(arguments);
        AssertCommandFailure(result, expectedExitCode, $"webdev {string.Join(" ", arguments)} should fail with exit code {expectedExitCode}");
        return result;
    }

    /// <summary>
    /// Verifies that the devcontainer is accessible before running tests
    /// </summary>
    /// <returns>True if the devcontainer is accessible</returns>
    protected async Task<bool> VerifyDevContainerAccessibleAsync()
    {
        var isAccessible = await CommandExecutor.IsDevContainerAccessibleAsync();
        isAccessible.Should().BeTrue("DevContainer should be accessible for tests to run");
        return isAccessible;
    }

    public virtual void Dispose()
    {
        // Cleanup if needed
    }
}

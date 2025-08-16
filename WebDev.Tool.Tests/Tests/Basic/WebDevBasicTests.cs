using FluentAssertions;
using WebDev.Tool.Tests.Base;

namespace WebDev.Tool.Tests.Tests.Basic;

/// <summary>
/// Basic functionality tests for webdev tool (version, help, availability)
/// </summary>
public class WebDevBasicTests : DevContainerTestBase
{
    [Fact]
    public async Task WebDev_Version_Command_Should_Output_Version()
    {
        // Arrange & Act
        var result = await ExecuteWebDevCommandSuccessfullyAsync("--version");

        // Assert
        AssertOutputContains(result, "WebDev Tool");
        AssertErrorEmpty(result);
    }

    [Fact]
    public async Task WebDev_Help_Command_Should_Show_Usage_Information()
    {
        // Arrange & Act
        var result = await ExecuteWebDevCommandSuccessfullyAsync("--help");

        // Assert
        AssertOutputContains(result, "USAGE:");
        AssertOutputContains(result, "webdev");
        AssertErrorEmpty(result);
    }

    [Fact]
    public async Task WebDev_Invalid_Command_Should_Fail_With_Error()
    {
        // Arrange & Act
        var result = await ExecuteWebDevCommandWithFailureAsync(0, "invalid-command");

        // Assert
        // The webdev tool shows an error message for invalid commands
        AssertOutputContains(result, "Error: Unknown command");
        // Note: The exact behavior may vary depending on the webdev tool implementation
    }

    [Fact]
    public async Task WebDev_Command_Should_Be_Available_In_Container()
    {
        // Arrange & Act
        var result = await CommandExecutor.ExecuteCommandAsync("which", "webdev");

        // Assert
        AssertCommandSuccess(result, "webdev command should be available in the container");
        AssertOutputContains(result, "webdev");
        AssertErrorEmpty(result);
    }

    [Fact]
    public async Task WebDev_Command_Should_Execute_Without_Arguments()
    {
        // Arrange & Act
        var result = await CommandExecutor.ExecuteWebDevCommandAsync();

        // Assert
        // This test verifies that the webdev command can be executed without arguments
        // The exact behavior depends on the webdev tool implementation
        result.Should().NotBeNull();
        result.TimedOut.Should().BeFalse("Command should not have timed out");
    }
}

using FluentAssertions;

namespace WebDev.Tool.Tests;

/// <summary>
/// Tests for webdev tool commands executed within a devcontainer
/// </summary>
public class DevContainerWebDevTests : DevContainerTestBase
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

    [Fact]
    public async Task DevContainer_Should_Be_Accessible()
    {
        // Arrange & Act
        var isAccessible = await VerifyDevContainerAccessibleAsync();

        // Assert
        isAccessible.Should().BeTrue("DevContainer should be accessible for all tests");
    }

    [Fact]
    public async Task WebDev_Should_Change_PHP_Version_Successfully()
    {
        // Arrange - Check initial PHP version
        var initialPhpResult = await CommandExecutor.ExecuteCommandAsync("php", "-v");
        AssertCommandSuccess(initialPhpResult, "Initial PHP version check should succeed");
        AssertOutputContains(initialPhpResult, "8.1", "Initial PHP version should be 8.1");

        // Check initial webdev.yml configuration
        var initialConfigResult = await CommandExecutor.ExecuteCommandAsync("cat", ".devcontainer/webdev.yml");
        AssertCommandSuccess(initialConfigResult, "Reading initial webdev.yml should succeed");
        AssertOutputContains(initialConfigResult, "php:\n  version: 8.1", "Initial webdev.yml should have PHP version 8.1");

        // Act - Change PHP version to 8.2
        var changeVersionResult = await ExecuteWebDevCommandSuccessfullyAsync("php", "version", "8.2", "--debug");
        AssertOutputContains(changeVersionResult, "PHP Version has been set to 8.2", "Should show PHP version change message");
        AssertOutputContains(changeVersionResult, "Saving the new active version so it can be restored", "Should show the new version being saved in the config");

        // Assert - Verify PHP version changed
        var newPhpResult = await CommandExecutor.ExecuteCommandAsync("php", "-v");
        AssertCommandSuccess(newPhpResult, "New PHP version check should succeed");
        AssertOutputContains(newPhpResult, "8.2", "PHP version should now be 8.2");

        // Assert - Verify webdev.yml configuration updated
        var updatedConfigResult = await CommandExecutor.ExecuteCommandAsync("cat", ".devcontainer/webdev.yml");
        AssertCommandSuccess(updatedConfigResult, "Reading updated webdev.yml should succeed");
        AssertOutputContains(updatedConfigResult, "php:\n  version: 8.2", "Updated webdev.yml should have PHP version 8.2");
    }
}

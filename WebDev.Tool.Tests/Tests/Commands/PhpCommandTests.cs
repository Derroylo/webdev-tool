using FluentAssertions;
using WebDev.Tool.Tests.Base;

namespace WebDev.Tool.Tests.Tests.Commands;

/// <summary>
/// Tests for PHP-related webdev commands
/// </summary>
public class PhpCommandTests : DevContainerTestBase
{
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

using FluentAssertions;
using WebDev.Tool.Tests.Base;
using WebDev.Tool.Tests.Helpers;

namespace WebDev.Tool.Tests.Tests.Commands;

/// <summary>
/// Tests for PHP-related webdev commands
/// </summary>
[Trait("Category", "Commands")]
public class PhpCommandTests : DevContainerTestBase
{
    [Fact]
    [TestOrder(1)]
    public async Task WebDev_Should_Change_PHP_Version_Successfully()
    {
        // Arrange - Check initial PHP version and ensure it's 8.1
        var initialPhpResult = await CommandExecutor.ExecuteCommandAsync("php", "-v");
        AssertCommandSuccess(initialPhpResult, "Initial PHP version check should succeed");
        
        // If current version is not 8.1, set it to 8.1 first
        if (!initialPhpResult.StandardOutput.Contains("8.1"))
        {
            var setTo81Result = await ExecuteWebDevCommandSuccessfullyAsync("php", "version", "8.1");
            AssertOutputContains(setTo81Result, "PHP Version has been set to 8.1", "Should set PHP version to 8.1");
            
            // Verify the change
            var verify81Result = await CommandExecutor.ExecuteCommandAsync("php", "-v");
            AssertCommandSuccess(verify81Result, "PHP version 8.1 verification should succeed");
            AssertOutputContains(verify81Result, "8.1", "PHP version should now be 8.1");
        }

        // Check initial webdev.yml configuration
        var initialConfigResult = await CommandExecutor.ExecuteCommandAsync("cat", ".devcontainer/webdev.yml");
        AssertCommandSuccess(initialConfigResult, "Reading initial webdev.yml should succeed");

        // Act - Change PHP version to 8.2
        var changeVersionResult = await ExecuteWebDevCommandSuccessfullyAsync("php", "version", "8.2");
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

        // Act - Change PHP version back to 8.1
        var resetVersionResult = await ExecuteWebDevCommandSuccessfullyAsync("php", "version", "8.1");
        AssertOutputContains(resetVersionResult, "PHP Version has been set to 8.1", "Should show PHP version change message");
        AssertOutputContains(resetVersionResult, "Saving the new active version so it can be restored", "Should show the new version being saved in the config");
    }

    [Fact]
    [TestOrder(2)]
    public async Task WebDev_Should_Restore_PHP_Version_Successfully()
    {
        // Arrange - Check initial PHP version and ensure it's 8.1
        var initialPhpResult = await CommandExecutor.ExecuteCommandAsync("php", "-v");
        AssertCommandSuccess(initialPhpResult, "Initial PHP version check should succeed");
        
        // If current version is not 8.1, set it to 8.1 first
        if (!initialPhpResult.StandardOutput.Contains("8.1"))
        {
            var setTo81Result = await ExecuteWebDevCommandSuccessfullyAsync("php", "version", "8.1");
            AssertOutputContains(setTo81Result, "PHP Version has been set to 8.1", "Should set PHP version to 8.1");
            
            // Verify the change
            var verify81Result = await CommandExecutor.ExecuteCommandAsync("php", "-v");
            AssertCommandSuccess(verify81Result, "PHP version 8.1 verification should succeed");
            AssertOutputContains(verify81Result, "8.1", "PHP version should now be 8.1");
        }

        // Act - Change PHP version in webdev.yml file to 8.2
        // First, let's check what the current version is in the file
        var currentConfigResult = await CommandExecutor.ExecuteCommandAsync("cat", ".devcontainer/webdev.yml");
        AssertCommandSuccess(currentConfigResult, "Reading current webdev.yml should succeed");
        
        // Update the file to version 8.2 (replace only php.version)
        var updateConfigResult = await CommandExecutor.ExecuteCommandAsync("sed", "-i", "/^php:/,/^[^ ]/ s/version: [0-9.]\\+/version: 8.2/", ".devcontainer/webdev.yml");
        AssertCommandSuccess(updateConfigResult, "Updating webdev.yml should succeed");

        // Verify the file was updated
        var verifyConfigResult = await CommandExecutor.ExecuteCommandAsync("cat", ".devcontainer/webdev.yml");
        AssertCommandSuccess(verifyConfigResult, "Reading updated webdev.yml should succeed");
        AssertOutputContains(verifyConfigResult, "php:\n  version: 8.2", "webdev.yml should now have PHP version 8.2");

        // Act - Execute restore command
        var restoreResult = await ExecuteWebDevCommandSuccessfullyAsync("restore", "php");
        AssertOutputContains(restoreResult, "Checking if php version has been set via config", "Should show PHP version check message");
        AssertOutputContains(restoreResult, "Found", "Should show PHP version was found in config");

        // Assert - Verify PHP version has been restored to 8.2
        var finalPhpResult = await CommandExecutor.ExecuteCommandAsync("php", "-v");
        AssertCommandSuccess(finalPhpResult, "Final PHP version check should succeed");
        AssertOutputContains(finalPhpResult, "8.2", "PHP version should be restored to 8.2");

         // Act - Execute restore command
        var restoreResult2 = await ExecuteWebDevCommandSuccessfullyAsync("restore", "php");
        AssertOutputContains(restoreResult2, "Checking if php settings has been set via config", "Should show PHP version 8.2 is already active");

        // Cleanup - Reset webdev.yml back to 8.1 for other tests
         var resetConfigResult = await CommandExecutor.ExecuteCommandAsync("sed", "-i", "s/version: 8.2/version: 8.1/", ".devcontainer/webdev.yml");
        AssertCommandSuccess(resetConfigResult, "Resetting webdev.yml should succeed");
        
        // Verify cleanup
        var finalConfigResult = await CommandExecutor.ExecuteCommandAsync("cat", ".devcontainer/webdev.yml");
        AssertCommandSuccess(finalConfigResult, "Reading final webdev.yml should succeed");
        AssertOutputContains(finalConfigResult, "php:\n  version: 8.1", "webdev.yml should be reset to PHP version 8.1");
    }
}

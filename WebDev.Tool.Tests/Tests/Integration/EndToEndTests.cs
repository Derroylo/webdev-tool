using FluentAssertions;
using WebDev.Tool.Tests.Base;
using WebDev.Tool.Tests.Helpers;

namespace WebDev.Tool.Tests.Tests.Integration;

/// <summary>
/// End-to-end workflow tests for webdev tool
/// </summary>
[Trait("Category", "Integration")]
public class EndToEndTests : DevContainerTestBase
{
    [Fact]
    [TestOrder(1)]
    public async Task Complete_WebDev_Workflow_Should_Succeed()
    {
        // Arrange & Act - Test complete workflow from basic to command execution
        var versionResult = await ExecuteWebDevCommandSuccessfullyAsync("--version");
        var helpResult = await ExecuteWebDevCommandSuccessfullyAsync("--help");
        
        // Assert
        AssertOutputContains(versionResult, "WebDev Tool");
        AssertOutputContains(helpResult, "USAGE:");
        AssertErrorEmpty(versionResult);
        AssertErrorEmpty(helpResult);
    }

    [Fact]
    [TestOrder(2)]
    public async Task WebDev_Environment_Should_Be_Fully_Functional()
    {
        // Arrange & Act - Verify complete environment setup
        var isAccessible = await VerifyDevContainerAccessibleAsync();
        var webdevAvailable = await CommandExecutor.ExecuteCommandAsync("which", "webdev");
        
        // Assert
        isAccessible.Should().BeTrue("DevContainer should be accessible");
        AssertCommandSuccess(webdevAvailable, "webdev command should be available");
        AssertOutputContains(webdevAvailable, "webdev");
    }
}

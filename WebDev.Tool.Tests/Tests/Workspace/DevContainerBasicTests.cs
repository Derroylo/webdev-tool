using FluentAssertions;
using WebDev.Tool.Tests.Base;

namespace WebDev.Tool.Tests.Tests.Workspace;

/// <summary>
/// Tests for devcontainer environment and workspace setup
/// </summary>
public class DevContainerBasicTests : DevContainerTestBase
{
    [Fact]
    public async Task DevContainer_Should_Be_Accessible()
    {
        // Arrange & Act
        var isAccessible = await VerifyDevContainerAccessibleAsync();

        // Assert
        isAccessible.Should().BeTrue("DevContainer should be accessible for all tests");
    }
}

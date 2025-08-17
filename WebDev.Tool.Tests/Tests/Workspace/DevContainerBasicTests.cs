using FluentAssertions;
using WebDev.Tool.Tests.Base;
using WebDev.Tool.Tests.Helpers;

namespace WebDev.Tool.Tests.Tests.Workspace;

/// <summary>
/// Tests for devcontainer environment and workspace setup
/// </summary>
[Trait("Category", "Workspace")]
public class DevContainerBasicTests : DevContainerTestBase
{
    [Fact]
    [TestOrder(1)]
    public async Task DevContainer_Should_Be_Accessible()
    {
        // Arrange & Act
        var isAccessible = await VerifyDevContainerAccessibleAsync();

        // Assert
        isAccessible.Should().BeTrue("DevContainer should be accessible for all tests");
    }
}

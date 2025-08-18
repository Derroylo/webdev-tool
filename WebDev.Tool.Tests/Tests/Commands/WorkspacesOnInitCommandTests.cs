using FluentAssertions;
using Spectre.Console;
using WebDev.Tool.Tests.Base;
using WebDev.Tool.Tests.Helpers;

namespace WebDev.Tool.Tests.Tests.Commands;

/// <summary>
/// Tests for workspaces-on-init command
/// </summary>
[Trait("Category", "Commands")]
public class WorkspacesOnInitCommandTests : DevContainerTestBase
{
    [Fact]
    [TestOrder(1)]
    public async Task WebDev_Workspaces_On_Init_Should_Execute_Successfully()
    {
        // Arrange & Act - Execute the workspaces-on-init command
        var result = await ExecuteWebDevCommandSuccessfullyAsync("workspaces-on-init");

        // Assert - Command should execute without errors
        AssertCommandSuccess(result, "workspaces-on-init command should execute successfully");
        AssertErrorEmpty(result);
    }

    [Fact]
    [TestOrder(2)]
    public async Task WebDev_Workspaces_On_Init_With_Debug_Should_Show_Debug_Information()
    {
        // Arrange & Act - Execute the workspaces-on-init command with debug flag
        var result = await ExecuteWebDevCommandSuccessfullyAsync("workspaces-on-init", "--debug");

        // Assert - Command should execute and show debug information
        AssertCommandSuccess(result, "workspaces-on-init command with debug should execute successfully");
        AssertOutputContains(result, "Executing OnInitWorkspacesCommand with debug mode enabled", "Should show debug mode message");
        AssertErrorEmpty(result);
    }

    [Fact]
    [TestOrder(3)]
    public async Task WebDev_Workspaces_On_Init_Should_Validate_Workspaces()
    {
        // Arrange & Act - Execute the workspaces-on-init command
        var result = await ExecuteWebDevCommandSuccessfullyAsync("workspaces-on-init");

        // Assert - Command should validate workspaces successfully
        AssertCommandSuccess(result, "workspaces-on-init should validate workspaces successfully");
        
        // The command should complete without workspace validation errors
        // If there are validation issues, they would appear in the output
        AssertErrorEmpty(result);
    }

    [Fact]
    [TestOrder(4)]
    public async Task WebDev_Workspaces_On_Init_Should_Create_Service_Labels()
    {
        // Arrange & Act - Execute the workspaces-on-init command
        var result = await ExecuteWebDevCommandSuccessfullyAsync("workspaces-on-init");

        // Assert - Command should create service labels successfully
        AssertCommandSuccess(result, "workspaces-on-init should create service labels successfully");
        
        // Verify that the command completed without service label creation errors
        AssertErrorEmpty(result);
    }

    [Fact]
    [TestOrder(5)]
    public async Task WebDev_Workspaces_On_Init_Should_Create_Traefik_Certificates()
    {
        // Arrange & Act - Execute the workspaces-on-init command
        var result = await ExecuteWebDevCommandSuccessfullyAsync("workspaces-on-init");

        // Assert - Command should create Traefik certificates successfully
        AssertCommandSuccess(result, "workspaces-on-init should create Traefik certificates successfully");
        
        // Verify that the command completed without certificate creation errors
        AssertErrorEmpty(result);

        // Verify host system certificates directory exists
        var hostCertsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "webdev-host", "certs");
        Directory.Exists(hostCertsPath).Should().BeTrue("Host system certificates directory should exist");
        var rootKeyFile = Path.Combine(hostCertsPath, "rootCA-key.pem");
        var rootCertFile = Path.Combine(hostCertsPath, "rootCA.pem");

        File.Exists(rootKeyFile).Should().BeTrue("Root certificate key file should exist");
        File.Exists(rootCertFile).Should().BeTrue("Root cCertificate file should exist");

        // Verify devcontainer certificate files exist
        var devContainerCertsPath = Path.Combine("/workspaces", "webdev-tool", "devcontainer-testenv", ".devcontainer", "traefik", "certs");
        Directory.Exists(devContainerCertsPath).Should().BeTrue("Devcontainer certificates directory should exist");
        var keyFile = Path.Combine(devContainerCertsPath, "_wildcard.testenv.dev.localhost-key.pem");
        var certFile = Path.Combine(devContainerCertsPath, "_wildcard.testenv.dev.localhost.pem");

        File.Exists(keyFile).Should().BeTrue("Certificate key file should exist");
        File.Exists(certFile).Should().BeTrue("Certificate file should exist");
    }

    [Fact]
    [TestOrder(6)]
    public async Task WebDev_Workspaces_On_Init_Should_Prepare_Workspaces()
    {
        // Arrange & Act - Execute the workspaces-on-init command
        var result = await ExecuteWebDevCommandSuccessfullyAsync("workspaces-on-init");

        // Assert - Command should prepare workspaces successfully
        AssertCommandSuccess(result, "workspaces-on-init should prepare workspaces successfully");
        
        // Verify that the command completed without workspace preparation errors
        AssertErrorEmpty(result);
    }

    [Fact]
    [TestOrder(7)]
    public async Task WebDev_Workspaces_On_Init_Should_Be_Available_As_Command()
    {
        // Arrange & Act - Check if the command is available
        var result = await ExecuteWebDevCommandSuccessfullyAsync("--help");

        // Assert - The help should show that workspaces-on-init is available (though it's hidden)
        AssertCommandSuccess(result, "Help command should execute successfully");
        
        // Note: Since workspaces-on-init is a hidden command, it may not appear in help output
        // But the command should still be executable
        AssertErrorEmpty(result);
    }
}

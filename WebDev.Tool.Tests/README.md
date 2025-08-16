# WebDev Tool DevContainer Testing Framework

This directory contains the automated testing system for the webdev tool that executes commands within a devcontainer environment.

## Overview

The testing framework consists of:

- **DevContainerCommandExecutor**: Executes commands within a devcontainer and captures output
- **DevContainerTestBase**: Base class providing common test functionality and assertions
- **TestUtilities**: Utility methods for configuration, logging, and common operations
- **Sample Tests**: Example tests demonstrating the framework usage

## Prerequisites

1. **DevContainer CLI**: Must be installed and available in PATH
2. **Running DevContainer**: The devcontainer must be running (start with `run_devcontainer_tests.sh`)
3. **WebDev Tool**: Must be installed within the devcontainer

## Configuration

The testing framework uses `appsettings.json` for configuration:

```json
{
  "TestSettings": {
    "DevContainer": {
      "WorkspaceFolder": "./devcontainer-testenv",
      "CommandTimeoutSeconds": 30,
      "RetryAttempts": 3,
      "RetryDelaySeconds": 2
    },
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "WebDev.Tool.Tests": "Debug"
      }
    }
  }
}
```

## Writing Tests

### Basic Test Structure

```csharp
public class MyWebDevTests : DevContainerTestBase
{
    [Fact]
    public async Task My_Test_Method()
    {
        // Arrange & Act
        var result = await ExecuteWebDevCommandSuccessfullyAsync("--version");

        // Assert
        AssertOutputContains(result, "expected text");
        AssertErrorEmpty(result);
    }
}
```

### Available Assertion Methods

- `AssertCommandSuccess(result)`: Verifies command executed successfully
- `AssertCommandFailure(result, expectedExitCode)`: Verifies command failed with specific exit code
- `AssertOutputContains(result, expectedText)`: Verifies output contains specific text
- `AssertErrorContains(result, expectedText)`: Verifies error output contains specific text
- `AssertOutputMatches(result, pattern)`: Verifies output matches regex pattern
- `AssertOutputEmpty(result)`: Verifies output is empty
- `AssertErrorEmpty(result)`: Verifies error output is empty

### Helper Methods

- `ExecuteWebDevCommandSuccessfullyAsync(args)`: Executes webdev command and asserts success
- `ExecuteWebDevCommandWithFailureAsync(expectedExitCode, args)`: Executes webdev command and asserts failure
- `VerifyDevContainerAccessibleAsync()`: Verifies devcontainer is accessible

## Running Tests

### Prerequisites

1. Start the devcontainer:
   ```bash
   ./run_devcontainer_tests.sh
   ```

2. Build the test project:
   ```bash
   dotnet build WebDev.Tool.Tests
   ```

### Execute Tests

```bash
# Run all tests
dotnet test WebDev.Tool.Tests

# Run specific test class
dotnet test WebDev.Tool.Tests --filter "FullyQualifiedName~DevContainerWebDevTests"

# Run with verbose output
dotnet test WebDev.Tool.Tests --verbosity normal

# Run with specific logger
dotnet test WebDev.Tool.Tests --logger "console;verbosity=detailed"
```

## Test Categories

### 1. Command Tests
Test individual webdev commands and their output:
- Version command
- Help command
- Invalid command handling

### 2. Integration Tests
Test complete workflows and command chaining:
- Command sequences
- State persistence
- Error recovery

### 3. Environment Tests
Test behavior in different scenarios:
- Devcontainer accessibility
- Tool availability
- Configuration loading

## Best Practices

1. **Test Isolation**: Each test should be independent and not rely on other tests
2. **Clear Assertions**: Use descriptive assertion messages
3. **Timeout Handling**: Use appropriate timeouts for long-running commands
4. **Error Testing**: Test both success and failure scenarios
5. **Logging**: Use the built-in logging for debugging test issues

## Troubleshooting

### Common Issues

1. **DevContainer Not Accessible**
   - Ensure devcontainer is running: `docker ps`
   - Check devcontainer CLI: `devcontainer --version`
   - Verify workspace folder path in configuration

2. **Command Timeouts**
   - Increase `CommandTimeoutSeconds` in configuration
   - Check if devcontainer is responsive
   - Verify command syntax

3. **Test Failures**
   - Check test logs for detailed error information
   - Verify expected output matches actual output
   - Ensure webdev tool is properly installed in container

### Debug Mode

Enable debug logging in `appsettings.json`:

```json
{
  "TestSettings": {
    "Logging": {
      "LogLevel": {
        "WebDev.Tool.Tests": "Debug"
      }
    }
  }
}
```

## Extending the Framework

### Adding New Assertion Methods

Extend `DevContainerTestBase` with new assertion methods:

```csharp
protected void AssertOutputJson(CommandExecutionResult result, string expectedJson)
{
    result.Should().NotBeNull();
    // Add JSON validation logic
}
```

### Adding New Command Executors

Create specialized command executors for different tools:

```csharp
public class DockerCommandExecutor : DevContainerCommandExecutor
{
    public async Task<CommandExecutionResult> ExecuteDockerCommandAsync(params string[] arguments)
    {
        return await ExecuteCommandAsync("docker", arguments);
    }
}
```

## Contributing

When adding new tests:

1. Follow the existing naming conventions
2. Add appropriate documentation
3. Include both positive and negative test cases
4. Use descriptive test names
5. Add configuration options if needed

# WebDev.Tool.Tests

This project contains comprehensive tests for the WebDev.Tool, organized by functionality and test type.

## Folder Structure

```
WebDev.Tool.Tests/
├── Base/                          # Base classes and test infrastructure
│   └── DevContainerTestBase.cs    # Base class for devcontainer tests
│
├── Helpers/                       # Helper classes and utilities
│   ├── DevContainerCommandExecutor.cs
│   ├── TestUtilities.cs
│   └── TestData/                  # Test data and fixtures
│
├── Tests/                         # All test classes organized by category
│   ├── Basic/                     # Basic functionality tests
│   │   └── WebDevBasicTests.cs    # Version, help, availability tests
│   │
│   ├── Commands/                  # Command-specific tests
│   │   └── PhpCommandTests.cs     # PHP-related command tests
│   │
│   ├── Workspace/                 # Workspace and environment tests
│   │   └── DevContainerBasicTests.cs
│   │
│   └── Integration/               # End-to-end and integration tests
│
├── Configuration/                 # Test configuration files
│   ├── appsettings.json
│   └── xunit.runner.json
│
└── Resources/                     # Test resources and assets
    ├── TestProjects/
    ├── ExpectedOutputs/
    └── TestScripts/
```

## Test Categories

### Basic Tests (`Tests/Basic/`)
Fundamental functionality tests including:
- Version command (`--version`)
- Help command (`--help`)
- Command availability (`which webdev`)
- Basic command execution without arguments
- Invalid command handling

### Command Tests (`Tests/Commands/`)
Specific command functionality tests:
- **PHP Commands**: Version management, configuration updates
- **Node Commands**: Node.js related functionality (to be added)
- **Database Commands**: Database setup and configuration (to be added)

### Workspace Tests (`Tests/Workspace/`)
Environment and workspace setup tests:
- DevContainer accessibility
- Environment configuration
- Workspace setup validation

### Integration Tests (`Tests/Integration/`)
End-to-end workflow and performance tests:
- Complete workflow testing
- Performance benchmarks
- Cross-component integration

## Running Tests

### Prerequisites
- .NET 9.0 SDK
- Access to a running devcontainer environment

### Test Categories and Execution Order

Tests are organized into categories that execute in a specific order to ensure proper test dependencies:

1. **Basic Tests** (`Category=Basic`) - Fundamental functionality
2. **Workspace Tests** (`Category=Workspace`) - Environment setup
3. **Command Tests** (`Category=Commands`) - Command functionality
4. **Integration Tests** (`Category=Integration`) - End-to-end workflows

### Test Execution

#### Run All Tests in Order (Recommended)
```bash
# Run all tests in the correct order
dotnet test

# Or use the automated script
./run_devcontainer_tests.sh
```

#### Run Specific Categories
```bash
# Run Basic Tests only
dotnet test --filter "Category=Basic"

# Run Workspace Tests only
dotnet test --filter "Category=Workspace"

# Run Command Tests only
dotnet test --filter "Category=Commands"

# Run Integration Tests only
dotnet test --filter "Category=Integration"
```

#### Run Tests in Order Manually
```bash
# Step 1: Basic Tests
dotnet test --filter "Category=Basic"

# Step 2: Workspace Tests
dotnet test --filter "Category=Workspace"

# Step 3: Command Tests
dotnet test --filter "Category=Commands"

# Step 4: Integration Tests
dotnet test --filter "Category=Integration"
```

#### Run Specific Test Classes
```bash
# Run specific test class
dotnet test --filter "ClassName=WebDev.Tool.Tests.Tests.Basic.WebDevBasicTests"
```

### Configuration
Test configuration is managed through:
- `Configuration/appsettings.json` - General test settings
- `Configuration/xunit.runner.json` - xUnit runner configuration

## Adding New Tests

When adding new tests, follow these guidelines:

1. **Place tests in the appropriate category folder**:
   - Basic functionality → `Tests/Basic/`
   - Command-specific → `Tests/Commands/`
   - Environment/workspace → `Tests/Workspace/`
   - End-to-end → `Tests/Integration/`

2. **Use test categories**:
   - Add `[Trait("Category", "{Category}")]` to test classes
   - Use `[TestOrder(n)]` attribute to control execution order within categories
   - Categories execute in order: Basic → Workspace → Commands → Integration

3. **Follow naming conventions**:
   - Test classes: `{Category}{Feature}Tests.cs`
   - Test methods: `{Feature}_{Scenario}_Should_{ExpectedResult}`

4. **Use existing base classes**:
   - Inherit from `DevContainerTestBase` for devcontainer tests
   - Use helper utilities from `Helpers/` folder

5. **Group related tests**:
   - Keep related tests in the same file
   - Use descriptive test method names
   - Follow AAA pattern (Arrange, Act, Assert)

### Example Test Structure

```csharp
[Trait("Category", "Commands")]
public class PhpCommandTests : DevContainerTestBase
{
    [Fact]
    [TestOrder(1)]
    public async Task Setup_Initial_PHP_Environment()
    {
        // Setup test environment
    }

    [Fact]
    [TestOrder(2)]
    public async Task WebDev_Should_Change_PHP_Version_Successfully()
    {
        // Main test that depends on setup
    }

    [Fact]
    [TestOrder(3)]
    public async Task Cleanup_PHP_Environment()
    {
        // Cleanup after tests
    }
}
```

## Test Infrastructure

### Base Classes
- `DevContainerTestBase`: Base class for all devcontainer-based tests
  - Provides command execution utilities
  - Includes common assertion methods
  - Handles test setup and teardown

### Helper Classes
- `DevContainerCommandExecutor`: Executes commands in devcontainer environment
- `TestUtilities`: Common utility methods for configuration and logging

### Test Data
- `Helpers/TestData/`: Contains test fixtures, sample projects, and configuration files
- `Resources/`: Test assets, expected outputs, and test scripts

## Contributing

When contributing new tests:

1. Follow the established folder structure
2. Use appropriate base classes and helpers
3. Write descriptive test names and comments
4. Ensure tests are independent and repeatable
5. Add appropriate error handling and assertions
6. Update this README if adding new test categories or patterns

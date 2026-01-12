# MTM Receiving Application - Test Project

This test project provides comprehensive testing for the MTM Receiving Application using xUnit, FluentAssertions, and Moq.

## 📋 Table of Contents

- [Project Structure](#-project-structure)
- [Testing Framework](#-testing-framework)
- [Getting Started](#-getting-started)
- [Creating Tests](#-creating-tests)
- [Running Tests](#-running-tests)
- [Test Organization](#-test-organization)
- [Testing Patterns](#-testing-patterns)
- [Dependencies & Services](#-dependencies--services)

---

## 📁 Project Structure

```bash
MTM_Receiving_Application.Tests/
├── build.ps1                          # Build script for test project
├── README.md                          # This file
├── GlobalUsings.cs                    # Global using statements
├── Fixtures/                          # Test fixtures for reusable setup
│   ├── DatabaseFixture.cs             # Database test setup/teardown
│   └── ServiceCollectionFixture.cs    # DI container for testing
├── Helpers/                           # Test helper utilities
│   └── TestHelper.cs                  # Common test utilities
├── Unit/                              # Unit tests (isolated component tests)
│   ├── Module_Core/                   # Core module tests
│   │   ├── Converters/                # Converter tests
│   │   ├── Helpers/                   # Helper class tests
│   │   ├── Models/                    # Model tests
│   │   └── Services/                  # Core service tests
│   ├── Module_Dunnage/                # Dunnage module tests
│   │   ├── Data/                      # DAO tests
│   │   ├── Services/                  # Service tests
│   │   └── ViewModels/                # ViewModel tests
│   ├── Module_Receiving/              # Receiving module tests
│   │   ├── Data/                      # DAO tests
│   │   ├── Services/                  # Service tests
│   │   └── ViewModels/                # ViewModel tests
│   ├── Module_Reporting/              # Reporting module tests
│   ├── Module_Routing/                # Routing module tests
│   ├── Module_Settings/               # Settings module tests
│   ├── Module_Shared/                 # Shared module tests
│   └── Module_Volvo/                  # Volvo module tests
└── Integration/                       # Integration tests (multi-component)
    ├── Module_Core/                   # Core integration tests
    ├── Module_Dunnage/                # Dunnage workflow tests
    ├── Module_Receiving/              # Receiving workflow tests
    ├── Module_Routing/                # Routing workflow tests
    └── Module_Volvo/                  # Volvo integration tests
```

---

## 🧪 Testing Framework

### Core Technologies

- **xUnit** - Test framework (per project standards)
- **FluentAssertions** - Readable assertion library
- **Moq** - Mocking framework for dependencies
- **Microsoft.Data.Sqlite** - In-memory database for testing
- **MySql.Data** - MySQL testing support

### Test Categories

1. **Unit Tests** (`Unit/`) - Test individual components in isolation
2. **Integration Tests** (`Integration/`) - Test multiple components working together

---

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Main MTM Receiving Application compiled (x64 platform)
- MySQL database (for integration tests)

### Building the Test Project

```powershell
# Option 1: Using the build script (recommended)
.\build.ps1                    # Builds main app + tests
.\build.ps1 -Clean             # Clean + full rebuild
.\build.ps1 -Configuration Release  # Release build

# Option 2: Using dotnet CLI directly
cd MTM_Receiving_Application.Tests
dotnet restore
dotnet build --no-dependencies
```

---

## 📝 Creating Tests

### Naming Conventions

**File Naming:**

```text
{ClassName}_Tests.cs
```

**Examples:**

- `Service_Authentication_Tests.cs`
- `ViewModel_Receiving_Workflow_Tests.cs`
- `Dao_ReceivingLine_Tests.cs`

**Class Naming:**

```csharp
public class {ClassName}_Tests
```

### Test Method Naming

Use descriptive names that explain what is being tested:

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()

// Examples:
[Fact]
public void InsertReceivingLine_ValidData_ReturnsSuccess()

[Fact]
public void LoadData_EmptyDatabase_ReturnsEmptyList()

[Fact]
public void ValidateInput_NullValue_ThrowsArgumentNullException()
```

### Unit Test Template

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services
{
    public class Service_MySQL_ReceivingLine_Tests
    {
        private readonly Mock<Dao_ReceivingLine> _mockDao;
        private readonly Service_MySQL_ReceivingLine _service;

        public Service_MySQL_ReceivingLine_Tests()
        {
            // Arrange: Setup mocks
            _mockDao = new Mock<Dao_ReceivingLine>("fake-connection-string");
            _service = new Service_MySQL_ReceivingLine(
                _mockDao.Object,
                Mock.Of<IService_LoggingUtility>()
            );
        }

        [Fact]
        public async Task InsertLineAsync_ValidLine_ReturnsSuccess()
        {
            // Arrange
            var testLine = new Model_ReceivingLine
            {
                PONumber = "PO-12345",
                Quantity = 100
            };

            var expectedResult = new Model_Dao_Result
            {
                Success = true,
                AffectedRows = 1
            };

            _mockDao
                .Setup(dao => dao.InsertReceivingLineAsync(testLine))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.InsertLineAsync(testLine);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AffectedRows.Should().Be(1);

            _mockDao.Verify(
                dao => dao.InsertReceivingLineAsync(testLine),
                Times.Once
            );
        }

        [Fact]
        public async Task InsertLineAsync_NullLine_ReturnsFail()
        {
            // Arrange
            Model_ReceivingLine nullLine = null;

            // Act
            var result = await _service.InsertLineAsync(nullLine);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeEmpty();
        }
    }
}
```

### ViewModel Test Template

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels
{
    public class ViewModel_Receiving_Workflow_Tests
    {
        private readonly Mock<IService_ReceivingWorkflow> _mockWorkflowService;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Mock<IService_LoggingUtility> _mockLogger;
        private readonly ViewModel_Receiving_Workflow _viewModel;

        public ViewModel_Receiving_Workflow_Tests()
        {
            _mockWorkflowService = new Mock<IService_ReceivingWorkflow>();
            _mockErrorHandler = new Mock<IService_ErrorHandler>();
            _mockLogger = new Mock<IService_LoggingUtility>();

            _viewModel = new ViewModel_Receiving_Workflow(
                _mockWorkflowService.Object,
                _mockErrorHandler.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task LoadDataCommand_Success_UpdatesProperties()
        {
            // Arrange
            var testData = new List<Model_ReceivingLine>
            {
                new Model_ReceivingLine { PONumber = "PO-001" },
                new Model_ReceivingLine { PONumber = "PO-002" }
            };

            _mockWorkflowService
                .Setup(s => s.GetReceivingLinesAsync())
                .ReturnsAsync(new Model_Dao_Result<List<Model_ReceivingLine>>
                {
                    Success = true,
                    Data = testData
                });

            // Act
            await _viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Items.Should().HaveCount(2);
            _viewModel.IsBusy.Should().BeFalse();
            _viewModel.StatusMessage.Should().Contain("2 items");
        }

        [Fact]
        public void Property_IsBusy_RaisesPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.IsBusy))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.IsBusy = true;

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }
    }
}
```

### Integration Test Template

```csharp
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Tests.Fixtures;

namespace MTM_Receiving_Application.Tests.Integration.Module_Receiving
{
    [Collection("Database Collection")]
    public class ReceivingWorkflow_Integration_Tests : IClassFixture<ServiceCollectionFixture>
    {
        private readonly ServiceCollectionFixture _fixture;
        private readonly IService_ReceivingWorkflow _workflowService;

        public ReceivingWorkflow_Integration_Tests(
            ServiceCollectionFixture fixture,
            DatabaseFixture dbFixture)
        {
            _fixture = fixture;
            _workflowService = _fixture.ServiceProvider
                .GetRequiredService<IService_ReceivingWorkflow>();
        }

        [Fact]
        public async Task CompleteWorkflow_ValidData_Success()
        {
            // Arrange
            var poNumber = "PO-TEST-001";

            // Act
            var result = await _workflowService.StartWorkflowAsync(poNumber);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }
    }
}
```

---

## 🏃 Running Tests

### Command Line

```powershell
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~Service_Authentication_Tests"

# Run tests by category
dotnet test --filter "FullyQualifiedName~Unit"           # Unit tests only
dotnet test --filter "FullyQualifiedName~Integration"    # Integration tests only
dotnet test --filter "FullyQualifiedName~Module_Receiving"  # Module-specific

# Run with detailed output
dotnet test -v:detailed

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Visual Studio / VS Code

- **Test Explorer**: View and run tests from the test explorer panel
- **Run from editor**: Click the green arrow next to `[Fact]` attributes

---

## 📂 Test Organization

### File Placement Rules

**Unit Tests** - Mirror the main project structure:

```bash
Main Project:                          Test Project:
Module_Receiving/                      Unit/Module_Receiving/
├── Data/                              ├── Data/
│   └── Dao_ReceivingLine.cs          │   └── Dao_ReceivingLine_Tests.cs
├── Services/                          ├── Services/
│   └── Service_MySQL_ReceivingLine.cs│   └── Service_MySQL_ReceivingLine_Tests.cs
└── ViewModels/                        └── ViewModels/
    └── ViewModel_Receiving_Workflow.cs    └── ViewModel_Receiving_Workflow_Tests.cs
```

**Integration Tests** - Grouped by feature/workflow:

```bash
Integration/Module_Receiving/
├── ReceivingWorkflow_Integration_Tests.cs
├── POValidation_Integration_Tests.cs
└── LabelGeneration_Integration_Tests.cs
```

---

## 🎯 Testing Patterns

### 1. Testing DAOs (Data Access Objects)

**Key Points:**

- DAOs must NEVER throw exceptions (return `Model_Dao_Result` with failure)
- Use in-memory SQLite or mock database connections
- Test both success and failure paths

```csharp
[Fact]
public async Task InsertAsync_ValidData_ReturnsSuccess()
{
    // Arrange
    var dao = new Dao_ReceivingLine("connection-string");
    var testLine = new Model_ReceivingLine { /* ... */ };

    // Act
    var result = await dao.InsertReceivingLineAsync(testLine);

    // Assert
    result.Success.Should().BeTrue();
    result.ErrorMessage.Should().BeNullOrEmpty();
}

[Fact]
public async Task InsertAsync_NullParameter_ReturnsFailure()
{
    // Arrange
    var dao = new Dao_ReceivingLine("connection-string");

    // Act
    var result = await dao.InsertReceivingLineAsync(null);

    // Assert
    result.Success.Should().BeFalse();
    result.ErrorMessage.Should().Contain("null");
}
```

### 2. Testing Services

**Key Points:**

- Mock DAO dependencies
- Test business logic validation
- Verify error handling and logging

```csharp
[Fact]
public async Task ProcessData_InvalidInput_LogsError()
{
    // Arrange
    var mockLogger = new Mock<IService_LoggingUtility>();
    var service = new Service_MyService(mockLogger.Object);

    // Act
    await service.ProcessDataAsync(null);

    // Assert
    mockLogger.Verify(
        l => l.LogError(It.IsAny<string>(), It.IsAny<string>()),
        Times.Once
    );
}
```

### 3. Testing ViewModels

**Key Points:**

- Must be `partial` classes (CommunityToolkit.Mvvm)
- Test `[ObservableProperty]` property changes
- Test `[RelayCommand]` command execution
- Verify `IsBusy` state management

```csharp
[Fact]
public async Task Command_SetsIsBusyDuringExecution()
{
    // Arrange
    var mockService = new Mock<IService_Data>();
    mockService
        .Setup(s => s.GetDataAsync())
        .Returns(async () =>
        {
            await Task.Delay(100); // Simulate work
            return new Model_Dao_Result { Success = true };
        });

    var viewModel = new ViewModel_Test(mockService.Object);

    // Act
    var commandTask = viewModel.LoadDataCommand.ExecuteAsync(null);

    // Assert - During execution
    viewModel.IsBusy.Should().BeTrue();

    await commandTask;

    // Assert - After execution
    viewModel.IsBusy.Should().BeFalse();
}
```

### 4. Testing Converters

```csharp
[Theory]
[InlineData(true, Visibility.Visible)]
[InlineData(false, Visibility.Collapsed)]
public void Convert_BoolToVisibility_ReturnsCorrectValue(bool input, Visibility expected)
{
    // Arrange
    var converter = new Converter_BooleanToVisibility();

    // Act
    var result = converter.Convert(input, typeof(Visibility), null, null);

    // Assert
    result.Should().Be(expected);
}
```

---

## 🔧 Dependencies & Services

### Accessing Services via DI

Use `ServiceCollectionFixture` to access configured services:

```csharp
public class MyTest : IClassFixture<ServiceCollectionFixture>
{
    private readonly IService_MySQL_Receiving _receivingService;

    public MyTest(ServiceCollectionFixture fixture)
    {
        _receivingService = fixture.ServiceProvider
            .GetRequiredService<IService_MySQL_Receiving>();
    }
}
```

### Database Testing

Use `DatabaseFixture` for tests requiring database access:

```csharp
[Collection("Database Collection")]
public class DatabaseTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _dbFixture;

    public DatabaseTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }
}
```

---

## 📚 Additional Resources

### Project Architecture

- **Constitution**: `../.specify/memory/constitution.md`
- **MVVM Guide**: `../.github/instructions/mvvm-pattern.instructions.md`
- **DAO Guide**: `../.github/instructions/dao-pattern.instructions.md`
- **Main README**: `../README.md`

### Testing Best Practices

1. **AAA Pattern**: Arrange, Act, Assert
2. **One assertion per test** (or logically related assertions)
3. **Descriptive test names** that explain intent
4. **Test isolation**: Each test should be independent
5. **Mock external dependencies**: Database, file system, network
6. **Clean up**: Fixtures handle setup/teardown

### FluentAssertions Examples

```csharp
// Collections
result.Should().NotBeNull();
result.Should().HaveCount(5);
result.Should().Contain(x => x.Id == 123);
result.Should().BeEmpty();

// Strings
name.Should().Be("Expected");
name.Should().StartWith("Exp");
name.Should().Contain("pec");
name.Should().NotBeNullOrWhiteSpace();

// Numbers
count.Should().BeGreaterThan(0);
count.Should().BeLessThanOrEqualTo(100);
count.Should().BeInRange(1, 10);

// Booleans
result.Success.Should().BeTrue();
result.HasErrors.Should().BeFalse();

// Exceptions
Action act = () => method.ThrowException();
act.Should().Throw<ArgumentNullException>()
   .WithMessage("*parameter*");

// Async
Func<Task> act = async () => await method.DoWorkAsync();
await act.Should().ThrowAsync<InvalidOperationException>();
```

---

## 🐛 Troubleshooting

### Common Issues

**Issue**: `Metadata file could not be found`

```powershell
# Solution: Build main app first
cd ..
.\build.ps1 -Platform x64
cd MTM_Receiving_Application.Tests
```

**Issue**: `Type or namespace could not be found`

```powershell
# Solution: Restore packages
dotnet restore
```

**Issue**: Test runner can't find tests

```powershell
# Solution: Rebuild
dotnet clean
dotnet build
```

---

## 📝 Quick Reference

```powershell
# Create new test file for Service
New-Item "Unit/Module_Receiving/Services/Service_MyService_Tests.cs"

# Create new test file for ViewModel
New-Item "Unit/Module_Receiving/ViewModels/ViewModel_MyFeature_Tests.cs"

# Create new test file for DAO
New-Item "Unit/Module_Receiving/Data/Dao_MyEntity_Tests.cs"

# Run specific module tests
dotnet test --filter "FullyQualifiedName~Module_Receiving"

# Run and watch for changes
dotnet watch test
```

---

**Last Updated**: January 11, 2026
**Framework**: xUnit 2.9.0
**Target**: .NET 8 (net8.0-windows10.0.22621.0)

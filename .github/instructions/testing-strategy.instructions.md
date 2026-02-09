---
applyTo: "**"
---

# Testing Strategy Instructions

> **Last Updated:** 2026-01-16  
> **Purpose:** Guidelines for creating tests in the MTM Receiving Application

---

## 🎯 Core Principle: Test What You Can Mock

**Golden Rule**: If a class has concrete dependencies (DAOs, non-virtual methods), it requires **integration testing**. If it has pure logic or interface dependencies, it can be **unit tested**.

---

## 📊 Testing Decision Tree

```
┌─────────────────────────────┐
│  What are you testing?      │
└──────────┬──────────────────┘
           │
           ├─► Validator (FluentValidation)
           │   └─► ✅ UNIT TEST (no dependencies)
           │
           ├─► ViewModel
           │   ├─► Injects IMediator? ✅ UNIT TEST (mock IMediator)
           │   └─► Injects concrete services? ⚠️ INTEGRATION TEST
           │
           ├─► Handler (CQRS)
           │   ├─► Injects concrete DAOs? ⚠️ INTEGRATION TEST
           │   ├─► Injects interfaces only? ✅ UNIT TEST
           │   └─► Pure logic handler? ✅ UNIT TEST
           │
           ├─► Service
           │   ├─► Injects concrete DAOs? ⚠️ INTEGRATION TEST
           │   ├─► Injects interfaces? ✅ UNIT TEST
           │   └─► Pure logic? ✅ UNIT TEST
           │
           └─► DAO
               └─► ⚠️ INTEGRATION TEST (requires database)
```

---

## ✅ Unit Testing Patterns

### 1. Validator Tests (FluentValidation)

**When to Use**: Testing validation rules in AbstractValidator<T> classes

**Pattern**:
```csharp
using FluentAssertions;
using Xunit;

namespace MTM_Receiving_Application.Tests.{Module}.Validators;

/// <summary>
/// Unit tests for {ValidatorName}.
/// Tests validation rules without any external dependencies.
/// </summary>
public class {ValidatorName}Tests
{
    private readonly {ValidatorName} _validator;

    public {ValidatorName}Tests()
    {
        _validator = new {ValidatorName}();
    }

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        // Arrange
        var command = new {CommandName}
        {
            Property1 = "valid value",
            Property2 = 10
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPropertyIsInvalid()
    {
        // Arrange
        var command = new {CommandName}
        {
            Property1 = "", // Invalid!
            Property2 = 10
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Property1));
    }
}
```

**Key Points**:
- No mocking needed (pure validation logic)
- Test both valid and invalid scenarios
- Use descriptive test names: `Validate_Should{Result}_When{Condition}`
- Use FluentAssertions for readable assertions

### 2. ViewModel Tests (with IMediator)

**When to Use**: ViewModels that inject IMediator or other interfaces

**Pattern**:
```csharp
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace MTM_Receiving_Application.Tests.{Module}.ViewModels;

public class {ViewModelName}Tests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly {ViewModelName} _viewModel;

    public {ViewModelName}Tests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();
        
        _viewModel = new {ViewModelName}(
            _mockMediator.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CommandMethod_ShouldSendQuery_WhenCalled()
    {
        // Arrange
        var expectedResult = new Model_Dao_Result<DataType> { IsSuccess = true };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<QueryType>(), default))
            .ReturnsAsync(expectedResult);

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        _mockMediator.Verify(
            m => m.Send(It.IsAny<QueryType>(), default),
            Times.Once);
    }
}
```

**Key Points**:
- Mock IMediator, not concrete services
- Verify MediatR queries/commands are sent
- Test error handling paths

---

## ⚠️ Integration Testing Patterns

### 1. Handler Integration Tests

**When to Use**: Handlers that inject concrete DAO classes

**Why**: Concrete DAOs cannot be mocked (architectural constraint)

**Pattern**:
```csharp
using FluentAssertions;
using Xunit;

namespace MTM_Receiving_Application.Tests.{Module}.Integration;

/// <summary>
/// Integration tests for {HandlerName}.
/// Tests handler with real database access.
/// </summary>
[Collection("Database")]
public class {HandlerName}IntegrationTests : IAsyncLifetime
{
    private readonly Dao_{Entity} _dao;
    private readonly {HandlerName} _handler;
    private int _testEntityId;

    public {HandlerName}IntegrationTests()
    {
        var connectionString = Helper_Database_Variables.GetConnectionString();
        _dao = new Dao_{Entity}(connectionString);
        _handler = new {HandlerName}(_dao);
    }

    public async Task InitializeAsync()
    {
        // Setup: Create test data
        var testEntity = new Model_{Entity}
        {
            Property = "TEST-VALUE-001",
            // ... test data
        };
        
        var result = await _dao.InsertAsync(testEntity);
        _testEntityId = result.Data;
    }

    public async Task DisposeAsync()
    {
        // Cleanup: Delete test data
        await _dao.DeleteAsync(_testEntityId);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityExists()
    {
        // Arrange
        var query = new {QueryName} { Id = _testEntityId };

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Property.Should().Be("TEST-VALUE-001");
    }
}
```

**Key Points**:
- Use real database connection
- Implement `IAsyncLifetime` for setup/cleanup
- Use TEST-prefixed data for easy identification
- Always clean up test data in `DisposeAsync`
- Use `[Collection("Database")]` to prevent parallel execution

### 2. Golden File Tests (CSV/Output Validation)

**When to Use**: Testing output files (labels, reports, exports)

**Pattern**:
```csharp
using VerifyXunit;
using Xunit;

namespace MTM_Receiving_Application.Tests.{Module}.GoldenFiles;

[UsesVerify]
public class {FeatureName}GoldenFileTests
{
    [Fact]
    public async Task GenerateOutput_ShouldMatchGoldenFile()
    {
        // Arrange
        var handler = new {HandlerName}(/* dependencies */);
        var query = new {QueryName} { /* test data */ };

        // Act
        var result = await handler.Handle(query, default);
        var output = result.Data; // CSV content, HTML, etc.

        // Assert
        await Verify(output)
            .UseFileName("expected_output")
            .UseDirectory("GoldenFiles");
    }
}
```

**Key Points**:
- Use Verify.Xunit for approval testing
- Store golden files in `GoldenFiles/` directory
- First run creates `.received.txt`, review and rename to `.verified.txt`
- Subsequent runs compare output to `.verified.txt`

### 3. End-to-End Workflow Tests

**When to Use**: Testing complete user workflows across layers

**Pattern**:
```csharp
namespace MTM_Receiving_Application.Tests.{Module}.Integration;

[Collection("Database")]
public class {Workflow}IntegrationTests : IAsyncLifetime
{
    // Setup similar to handler tests

    [Fact]
    public async Task CompleteWorkflow_ShouldSucceed_WhenDataIsValid()
    {
        // Arrange: Setup initial state
        
        // Act: Execute workflow steps
        // 1. Initialize
        var initResult = await _mediator.Send(new InitializeQuery());
        
        // 2. Add items
        var addResult = await _mediator.Send(new AddItemCommand { /* data */ });
        
        // 3. Complete
        var completeResult = await _mediator.Send(new CompleteCommand { /* data */ });

        // Assert: Verify final state
        completeResult.IsSuccess.Should().BeTrue();
        
        // Verify database state
        var dbRecord = await _dao.GetByIdAsync(completeResult.Data);
        dbRecord.Status.Should().Be("Completed");
    }
}
```

---

## 🚫 Anti-Patterns (Never Do This)

### ❌ Trying to Mock Concrete DAOs

```csharp
// ❌ WRONG - Will fail with "Non-overridable members" error
var mockDao = new Mock<Dao_Entity>();
mockDao.Setup(x => x.GetByIdAsync(123)).ReturnsAsync(result);
```

**Why**: DAOs are concrete classes with non-virtual methods. Moq requires virtual/interface members.

**Fix**: Use integration tests with real DAOs or refactor to use interfaces.

### ❌ Static Test Data

```csharp
// ❌ WRONG - Data can conflict between parallel tests
var testEntity = new Model_Entity { Id = 1, Name = "Test" };
```

**Fix**: Use dynamic test data with unique identifiers:
```csharp
// ✅ CORRECT
var testEntity = new Model_Entity
{
    Id = 0, // Auto-generated
    Name = $"TEST-{Guid.NewGuid()}"
};
```

### ❌ Forgetting Cleanup

```csharp
// ❌ WRONG - Leaves test data in database
[Fact]
public async Task MyTest()
{
    var id = await _dao.InsertAsync(testData);
    // ... test code
    // Missing: await _dao.DeleteAsync(id);
}
```

**Fix**: Use `IAsyncLifetime` for guaranteed cleanup.

---

## 📁 Test Organization

### Folder Structure

```
MTM_Receiving_Application.Tests/
├── Module_{Name}/
│   ├── Validators/
│   │   └── {Validator}Tests.cs          # Unit tests
│   ├── ViewModels/
│   │   └── {ViewModel}Tests.cs          # Unit tests (if mockable)
│   ├── Integration/
│   │   ├── {Handler}IntegrationTests.cs # Integration tests
│   │   └── {Workflow}IntegrationTests.cs
│   ├── GoldenFiles/
│   │   ├── {Feature}GoldenFileTests.cs
│   │   └── expected_{output}.verified.csv
│   └── TESTING_STRATEGY.md              # Module-specific notes
```

### Naming Conventions

- **Unit Tests**: `{ClassName}Tests.cs`
- **Integration Tests**: `{Feature}IntegrationTests.cs`
- **Golden File Tests**: `{Feature}GoldenFileTests.cs`
- **Test Methods**: `{Method}_Should{Result}_When{Condition}`

---

## 🛠️ Testing Tools

### Required NuGet Packages

```xml
<ItemGroup>
  <!-- Core Testing -->
  <PackageReference Include="xUnit" Version="2.8.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  
  <!-- Assertions -->
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  
  <!-- Mocking (interfaces only) -->
  <PackageReference Include="Moq" Version="4.20.70" />
  
  <!-- Golden File Testing -->
  <PackageReference Include="Verify.Xunit" Version="25.0.0" />
  
  <!-- Test Data Generation -->
  <PackageReference Include="Bogus" Version="35.5.0" />
  
  <!-- Property-Based Testing -->
  <PackageReference Include="FsCheck.Xunit" Version="2.16.6" />
</ItemGroup>
```

### When to Use Each Tool

- **xUnit**: All tests (standard framework)
- **FluentAssertions**: All assertions (readable syntax)
- **Moq**: Unit tests with interface dependencies only
- **Verify.Xunit**: Golden file tests (CSV, HTML, JSON output)
- **Bogus**: Generate realistic test data
- **FsCheck**: Property-based testing (advanced scenarios)

---

## 📋 Test Checklist

Before creating tests, ask:

- [ ] Does this class have concrete dependencies? → Integration test
- [ ] Does this class only use interfaces? → Unit test
- [ ] Is this a validator? → Unit test (no dependencies)
- [ ] Does this produce file output? → Golden file test
- [ ] Does this involve database operations? → Integration test
- [ ] Can I mock all dependencies? → Unit test
- [ ] Does this test a complete workflow? → Integration test

---

## 🎓 Best Practices

### 1. AAA Pattern (Arrange-Act-Assert)

Always structure tests clearly:
```csharp
[Fact]
public async Task MyTest()
{
    // Arrange: Setup test data and dependencies
    var input = new TestInput();
    
    // Act: Execute the code under test
    var result = await _handler.Handle(input, default);
    
    // Assert: Verify the outcome
    result.Should().NotBeNull();
}
```

### 2. One Assertion Per Test

```csharp
// ✅ GOOD - Tests one specific scenario
[Fact]
public void Validate_ShouldFail_WhenNameIsEmpty() { }

[Fact]
public void Validate_ShouldFail_WhenNameIsTooLong() { }

// ❌ AVOID - Tests multiple scenarios
[Fact]
public void Validate_ShouldHandleAllNameErrors() { }
```

### 3. Descriptive Test Names

```csharp
// ✅ GOOD - Clear what is being tested
[Fact]
public void Handle_ShouldReturnFailure_WhenEntityNotFound() { }

// ❌ AVOID - Vague test name
[Fact]
public void TestHandler() { }
```

### 4. Test Data Independence

```csharp
// ✅ GOOD - Each test creates its own data
[Fact]
public async Task Test1()
{
    var data = CreateTestData();
    // ... test code
}

// ❌ AVOID - Shared data between tests
private static TestData _sharedData = new TestData();
```

---

## 🔍 Debugging Failed Tests

### Common Issues

1. **"Non-overridable members" error**
   - **Cause**: Trying to mock concrete class
   - **Fix**: Use integration test or refactor to interface

2. **Database connection errors**
   - **Cause**: Wrong connection string or database not available
   - **Fix**: Check `appsettings.json`, ensure test database exists

3. **Test data conflicts**
   - **Cause**: Parallel tests using same data
   - **Fix**: Use `[Collection("Database")]` or unique identifiers

4. **Golden file mismatches**
   - **Cause**: Output format changed
   - **Fix**: Review `.received.txt`, update `.verified.txt` if intentional

---

## 📚 Further Reading

- **Project Constitution**: `.specify/memory/constitution.md`
- **MVVM Pattern**: `.github/instructions/mvvm-pattern.instructions.md`
- **DAO Pattern**: `.github/instructions/dao-pattern.instructions.md`
- **FluentValidation Docs**: https://docs.fluentvalidation.net/
- **Verify Docs**: https://github.com/VerifyTests/Verify

---

**Remember**: When in doubt, prefer integration tests over forcing unit tests on unmockable dependencies. It's better to have reliable integration tests than flaky unit tests with workarounds.

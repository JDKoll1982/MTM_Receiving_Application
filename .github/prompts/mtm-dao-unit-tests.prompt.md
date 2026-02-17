---
name: Generate DAO (Data Access Object) Tests
description: Creates comprehensive xUnit tests for C# DAO classes with MySQL stored procedures
argument-hint: "Select or specify the DAO file to test"
agent: agent
---

# Generate Unit Tests for DAO (Data Access Object) Classes

You are an expert C# unit test developer specializing in database integration testing with .NET 8 and xUnit.

## Context

- **Framework:** xUnit with FluentAssertions
- **Target:** DAO classes that call MySQL stored procedures
- **Pattern:** Instance-based DAOs with connection string injection
- **Database:** MySQL 8.0 (use test database or mocking)
- **Namespace Convention:** `MTM_Receiving_Application.Tests.[Module].DAOs`
- **Base Class:** Tests should inherit from appropriate base class if available

## Requirements

### Test Strategy

**Option A - Integration Tests (Preferred):**

- Use a dedicated test MySQL database with known seed data
- Test actual stored procedure execution against real database
- Verify returned `Model_Dao_Result` correctness with real data
- Use database transactions that rollback after each test
- Benefits: Catches SQL errors, validates parameter mapping, tests real database behavior

**Option B - Unit Tests with Mocking:**

- Mock `Helper_Database_StoredProcedure.ExecuteAsync()` method
- Verify parameter construction and types
- Test error handling paths without database dependency
- Benefits: Fast execution, no database setup required, isolates DAO logic

### Test Coverage Requirements

Generate comprehensive tests for:

1. **Successful Operations:**
   - Valid parameters return `Success = true`
   - `Data` property contains expected models
   - `NewId` populated for INSERT operations
   - Correct stored procedure name called
   - All required fields properly mapped

2. **Parameter Validation:**
   - Required parameters cannot be null/empty
   - MySqlParameter types match stored procedure expectations
   - Parameter names use correct prefix (e.g., `@p_ParameterName`)
   - Date/time parameters use correct format
   - Numeric parameters within valid ranges

3. **Error Handling:**
   - Database exceptions return `Success = false`
   - `ErrorMessage` contains meaningful diagnostic information
   - `Severity` level appropriately set
   - No unhandled exceptions propagate to caller
   - Connection failures handled gracefully

4. **Edge Cases:**
   - Empty result sets (returns empty collection, not null)
   - NULL parameter values handled correctly
   - Large data sets (pagination if applicable)
   - Special characters in string parameters (SQL injection prevention)
   - Unicode characters properly encoded
   - Maximum length string values

### Test Structure (Integration Tests)

```csharp
public class Dao_[DaoName]Tests :  IDisposable
{
    private readonly string _testConnectionString;
    private readonly Dao_[DaoName] _dao;

    public Dao_[DaoName]Tests()
    {
        _testConnectionString = "Server=172.16.1.104;Database=MTM_Test;... ";
        _dao = new Dao_[DaoName](_testConnectionString);
    }

    [Fact]
    public async Task InsertAsync_ValidModel_ReturnsSuccessWithNewId()
    {
        // Arrange
        var model = new Model_[Name]
        {
            Property1 = "TestValue",
            Property2 = 123
        };

        // Act
        var result = await _dao.InsertAsync(model);

        // Assert
        result.Success.Should().BeTrue();
        result.NewId.Should().BeGreaterThan(0);
        result.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsModel()
    {
        // Arrange
        var knownId = 1; // From seed data

        // Act
        var result = await _dao.GetByIdAsync(knownId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(knownId);
    }

    [Fact]
    public async Task InsertAsync_DatabaseError_ReturnsFailureResult()
    {
        // Arrange
        var invalidModel = new Model_[Name]
        {
            // Missing required fields
        };

        // Act
        var result = await _dao. InsertAsync(invalidModel);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        // Clean up test data if needed
    }
}
```

### Test Structure (Unit Tests with Mocking)

```csharp
public class Dao_[DaoName]Tests
{
    private readonly Mock<IHelper_Database_StoredProcedure> _spHelperMock;
    private readonly Dao_[DaoName] _dao;

    public Dao_[DaoName]Tests()
    {
        _spHelperMock = new Mock<IHelper_Database_StoredProcedure>();
        _dao = new Dao_[DaoName](_spHelperMock.Object);
    }

    [Fact]
    public async Task InsertAsync_ValidModel_CallsExecuteAsyncWithCorrectParameters()
    {
        // Arrange
        var model = new Model_[Name]
        {
            Property1 = "TestValue",
            Property2 = 123
        };

        // Act
        await _dao.InsertAsync(model);

        // Assert
        _spHelperMock.Verify(sp => sp.ExecuteAsync(It.IsAny<string>(), It.IsAny<MySqlParameter[]>(), null), Times.Once);
    }

    [Fact]
    public async Task InsertAsync_ValidModel_ReturnsSuccess()
    {
        // Arrange
        var model = new Model_[Name]
        {
            Property1 = "TestValue",
            Property2 = 123
        };
        _spHelperMock.Setup(sp => sp.ExecuteAsync(It.IsAny<string>(), It.IsAny<MySqlParameter[]>(), null))
                     .ReturnsAsync(new DbResult { Success = true, NewId = 1 });

        // Act
        var result = await _dao.InsertAsync(model);

        // Assert
        result.Success.Should().BeTrue();
        result.NewId.Should().Be(1);
    }

    [Fact]
    public async Task InsertAsync_DatabaseError_ReturnsFailureResult()
    {
        // Arrange
        var invalidModel = new Model_[Name]
        {
            // Missing required fields
        };
        _spHelperMock.Setup(sp => sp.ExecuteAsync(It.IsAny<string>(), It.IsAny<MySqlParameter[]>(), null))
                     .ThrowsAsync(new MySqlException("Database error"));

        // Act
        var result = await _dao.InsertAsync(invalidModel);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Database error");
    }
}
```

### Best Practices

**Database Setup:**

- Use dedicated test database separate from development/production
- Seed test database with known baseline data
- Use database transactions with rollback for test isolation
- Consider using database snapshots for faster test execution

**Test Organization:**

- Group related tests using `[Trait("Category", "Integration")]`
- Use descriptive test method names: `MethodName_Scenario_ExpectedBehavior`
- Arrange-Act-Assert pattern for all tests
- One logical assertion per test (use `Should().And` for related assertions)

**Data Management:**

- Never test against production database
- Use test data factories for consistent model creation
- Clean up test data in `Dispose()` method
- Consider using `[Theory]` with `[InlineData]` for parameterized tests

**Error Scenarios:**

- Test database connection failures
- Test timeout scenarios
- Test constraint violations
- Test concurrent access scenarios (if applicable)

**Performance Considerations:**

- Keep integration tests fast (<100ms per test)
- Use connection pooling in test database
- Minimize data volume in test scenarios
- Consider parallel test execution settings

### Common Patterns to Test

## Output Requirements

Generate a complete xUnit test class with:

- **6-12 test cases** covering all CRUD operations
- Integration tests using test database connection
- Proper setup/teardown with `IDisposable` and transactions
- XML documentation for complex test scenarios
- `[Trait]` attributes for test categorization
- FluentAssertions for all assertions with descriptive messages
- Clear Arrange-Act-Assert sections in each test
- Edge case coverage (nulls, empty strings, special characters)
- Error handling verification
- Parameter validation tests

**File Naming Convention:** `Dao_[DaoName]Tests.cs`
**Namespace:** `MTM_Receiving_Application.Tests.[Module].DAOs`

**Additional Files to Generate (if needed):**

- Test data seed scripts (`TestData/[DaoName]_SeedData.sql`)
- Test configuration file (`appsettings.Test.json`)
- Helper classes for test data generation

---

**Note:** If the DAO class uses static methods or hardcoded connection strings, recommend refactoring to instance-based pattern before generating tests.

**Constitutional Compliance:** All generated tests must follow MVVM architecture principles and never directly instantiate DAOs in ViewModels.

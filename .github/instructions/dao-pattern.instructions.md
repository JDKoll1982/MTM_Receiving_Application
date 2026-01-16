# DAO Pattern Guidelines

**Category**: Data Access Architecture  
**Last Updated**: December 27, 2025  
**Applies To**: All DAO classes in Data/ folder

## DAO Pattern Overview

The Data Access Object (DAO) pattern provides an abstract interface to the database, encapsulating all data access logic and hiding implementation details from the business layer.

**CONSTITUTIONAL MANDATE**: All DAOs must be **Instance-Based** and registered in the Dependency Injection container. Static DAOs are strictly prohibited.

### Deprecated: Static DAO Pattern

The legacy pattern of using `public static class Dao_Name` is **DEPRECATED**.

- Do NOT create new static DAOs.
- Refactor existing static DAOs to instance-based when modifying them.
- Static DAOs cannot be injected and make unit testing difficult.

### Benefits

- **Separation of Concerns**: Business logic separate from data access
- **Testability**: Easy to mock for unit testing (Moq compatible)
- **Consistency**: Standardized database access patterns
- **Error Handling**: Centralized error management
- **Performance Monitoring**: Built-in metrics collection

## File Structure

```
Data/
├── Receiving/
│   ├── Dao_ReceivingLine.cs
│   ├── Dao_ReceivingLoad.cs
│   └── Dao_PackageTypePreference.cs
├── InforVisual/
│   ├── Dao_InforVisualPO.cs
│   └── Dao_InforVisualPart.cs
└── Dunnage/
    └── Dao_DunnageLoad.cs
```

## Naming Conventions

### File Names

- Format: `Dao_<EntityName>.cs`
- Examples: `Dao_ReceivingLine.cs`, `Dao_User.cs`

### Class Names

- Format: `Dao_<EntityName>`
- Must be **public class** (NOT static)
- Example: `public class Dao_ReceivingLine`

### Method Names

- Format: `<Action><Entity>Async` or generic `<Action>Async`
- Actions: Insert, Update, Delete, Get, GetAll, GetBy<Criteria>
- Always suffix with `Async`
- Examples:
  - `InsertAsync`
  - `UpdateAsync`
  - `GetByIdAsync`
  - `GetByPoNumberAsync`

## Standard DAO Template

```csharp
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Core;

namespace MTM_Receiving_Application.Data.Receiving;

/// <summary>
/// Data Access Object for <table_name> table
/// Provides CRUD operations using stored procedures
/// </summary>
public class Dao_<EntityName>
{
    private readonly string _connectionString;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    public Dao_<EntityName>(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts a new record into the database
    /// </summary>
    /// <param name="entity">Entity model to insert</param>
    /// <returns>Model_Dao_Result with new ID or failure</returns>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_<EntityName> entity)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_Field1", entity.Field1 },
                { "p_Field2", entity.Field2 }
            };

            // Use Helper_Database_StoredProcedure for MySQL operations
            return await Helper_Database_StoredProcedure.ExecuteScalarAsync<int>(
                _connectionString,
                "sp_<table_name>_insert",
                parameters
            );
        }
        catch (Exception ex)
        {
            return DaoResultFactory.Failure<int>(
                $"Error inserting <entity>: {ex.Message}",
                ex);
        }
    }
    
    /// <summary>
    /// Retrieves all records
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_<EntityName>>>> GetAllAsync()
    {
        try
        {
            var parameters = new Dictionary<string, object>();
            
            return await Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync<Model_<EntityName>>(
                _connectionString,
                "sp_<table_name>_get_all",
                parameters);
        }
        catch (Exception ex)
        {
            return DaoResultFactory.Failure<List<Model_<EntityName>>>(
                $"Error retrieving <entity> list: {ex.Message}",
                ex);
        }
    }
}
```

## Dependency Injection Registration

All DAOs must be registered in `App.xaml.cs` as Singletons (stateless):

```csharp
// App.xaml.cs
private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    // Get connection strings
    var mySqlConnectionString = Helper_Database_Variables.GetConnectionString();
    var inforVisualConnectionString = Helper_Database_Variables.GetInforVisualConnectionString();
    
    // Register MySQL DAOs
    services.AddSingleton(sp => new Dao_ReceivingLine(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_User(mySqlConnectionString));
    
    // Register Infor Visual DAOs (READ-ONLY)
    services.AddSingleton(sp => new Dao_InforVisualPO(inforVisualConnectionString));
}
```

## Method Signatures

### Return Type

All DAO methods MUST return `Task<Model_Dao_Result<T>>` or `Task<Model_Dao_Result>`:

```csharp
public async Task<Model_Dao_Result<int>> InsertAsync(Model_ReceivingLine line)
public async Task<Model_Dao_Result<List<Model_ReceivingLine>>> GetAllAsync()
```

### Parameters

- Accept strongly-typed model objects
- Use nullable types for optional fields (`int?`, `string?`)
- For queries, accept specific criteria parameters

### Async/Await

- All methods must be async
- Use `await` for all database operations
- Don't use `.Result` or `.Wait()` (causes deadlocks)

## Infor Visual DAOs (READ-ONLY)

For Infor Visual integration, strict READ-ONLY policy applies:

```csharp
public class Dao_InforVisualPO
{
    public Dao_InforVisualPO(string connectionString)
    {
        // Validate READ-ONLY intent
        if (!connectionString.Contains("ApplicationIntent=ReadOnly"))
            throw new InvalidOperationException("Infor Visual connection must be ReadOnly");
            
        _connectionString = connectionString;
    }
    
    // SELECT methods only - NO Insert/Update/Delete
    public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetByPoAsync(string poNumber) { ... }
}
```

## Error Handling

### DAO Level

Catches exceptions, logs errors (optional), and returns `Model_Dao_Result.Failure`.
**NEVER throw exceptions from a DAO.**

```csharp
catch (Exception ex)
{
    return DaoResultFactory.Failure<T>(
        $"Error message: {ex.Message}", 
        ex, 
        Enum_ErrorSeverity.High);
}
```

## Testing

### Unit Tests

Mock the DAO when testing Services:

```csharp
// Service Test
[Fact]
public async Task Insert_ValidatesInput()
{
    // Arrange
    var mockDao = new Mock<Dao_ReceivingLine>("fake_connection");
    // Note: Since Dao is a class, methods must be virtual to mock, 
    // OR better: Mock the interface if one exists, or use integration tests for DAOs.
    // Ideally, Services inject DAOs, so we test Services by mocking DAOs.
    // For DAO unit tests, we use integration tests with a test database.
}
```

### Integration Tests

Test against real database with test connection string:

```csharp
[Fact]
public async Task Insert_ReturnsNewId()
{
    // Arrange
    var dao = new Dao_ReceivingLine(TestHelper.GetTestConnectionString());
    var line = new Model_ReceivingLine { ... };
    
    // Act
    var result = await dao.InsertAsync(line);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Data > 0);
}
```

## Performance Guidelines

### Connection Management

- Helper_Database_StoredProcedure manages connections automatically
- For Infor Visual (direct SQL), use `using` statements for `SqlConnection`

### Query Optimization

- Use indexes on frequently-queried columns
- Avoid SELECT * (specify columns)
- Use stored procedures for MySQL
- Use parameterized queries for Infor Visual

### Monitoring

Check `Model_Dao_Result.ExecutionTimeMs` in Service layer logging.

## Examples

See existing implementations:

- `Data/Receiving/Dao_ReceivingLoad.cs` - Instance-based MySQL DAO
- `Data/InforVisual/Dao_InforVisualPO.cs` - Instance-based READ-ONLY SQL Server DAO

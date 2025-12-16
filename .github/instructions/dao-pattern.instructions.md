# DAO Pattern Guidelines

**Category**: Data Access Architecture  
**Last Updated**: December 15, 2025  
**Applies To**: All DAO classes in Data/ folder

## DAO Pattern Overview

The Data Access Object (DAO) pattern provides an abstract interface to the database, encapsulating all data access logic and hiding implementation details from the business layer.

### Benefits

- **Separation of Concerns**: Business logic separate from data access
- **Testability**: Easy to mock for unit testing
- **Consistency**: Standardized database access patterns
- **Error Handling**: Centralized error management
- **Performance Monitoring**: Built-in metrics collection

## File Structure

```
Data/
├── Receiving/
│   ├── Dao_ReceivingLine.cs
│   ├── Dao_DunnageLine.cs
│   └── Dao_RoutingLabel.cs
├── Labels/
│   └── (Future DAOs)
└── Lookup/
    └── (Future DAOs)
```

## Naming Conventions

### File Names
- Format: `Dao_<EntityName>.cs`
- Examples: `Dao_ReceivingLine.cs`, `Dao_Employee.cs`, `Dao_Part.cs`

### Class Names
- Format: `Dao_<EntityName>`
- Must be static classes (no instances needed)
- Example: `public static class Dao_ReceivingLine`

### Method Names
- Format: `<Action><Entity>Async`
- Actions: Insert, Update, Delete, Get, GetAll, GetBy<Criteria>
- Always suffix with `Async`
- Examples:
  - `InsertReceivingLineAsync`
  - `UpdateReceivingLineAsync`
  - `GetReceivingLineByIdAsync`
  - `GetReceivingLinesByPONumberAsync`
  - `DeleteReceivingLineAsync`

## Standard DAO Template

```csharp
using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Data.Receiving;

/// <summary>
/// Data Access Object for <table_name> table
/// Provides CRUD operations using stored procedures
/// </summary>
public static class Dao_<EntityName>
{
    private static IService_ErrorHandler? _errorHandler;

    /// <summary>
    /// Sets the error handler service (dependency injection)
    /// </summary>
    public static void SetErrorHandler(IService_ErrorHandler errorHandler)
    {
        _errorHandler = errorHandler;
    }

    /// <summary>
    /// Inserts a new record into the database
    /// </summary>
    /// <param name="entity">Entity model to insert</param>
    /// <returns>Model_Dao_Result with success status and affected rows</returns>
    public static async Task<Model_Dao_Result> Insert<EntityName>Async(Model_<EntityName> entity)
    {
        try
        {
            string connectionString = Helper_Database_Variables.GetConnectionString(useProduction: true);

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_Field1", entity.Field1),
                new MySqlParameter("@p_Field2", entity.Field2),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Models.Enums.Enum_ErrorSeverity.Warning
                };
            }

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "<table_name>_Insert",
                parameters,
                connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            var errorResult = new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error inserting <entity>: {ex.Message}",
                Severity = Models.Enums.Enum_ErrorSeverity.Error
            };

            if (_errorHandler != null)
            {
                await _errorHandler.HandleErrorAsync(
                    errorResult.ErrorMessage,
                    errorResult.Severity,
                    ex,
                    showDialog: false
                );
            }

            return errorResult;
        }
    }
}
```

## Method Signatures

### Return Type
All DAO methods MUST return `Task<Model_Dao_Result>`:
```csharp
public static async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
```

### Parameters
- Accept strongly-typed model objects
- Use nullable types for optional fields (`int?`, `string?`)
- For queries, accept specific criteria parameters

### Async/Await
- All methods must be async
- Use `await` for all database operations
- Don't use `.Result` or `.Wait()` (causes deadlocks)

## Parameter Handling

### Required Parameters
```csharp
new MySqlParameter("@p_PartID", line.PartID ?? string.Empty)
```

### Optional Parameters
```csharp
new MySqlParameter("@p_CoilsOnSkid", (object?)line.CoilsOnSkid ?? DBNull.Value)
```

### OUT Parameters
```csharp
new MySqlParameter("@p_Status", MySqlDbType.Int32) 
{ 
    Direction = System.Data.ParameterDirection.Output 
},
new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) 
{ 
    Direction = System.Data.ParameterDirection.Output 
}
```

## Transaction Handling

### Single Operation
Transactions are handled in stored procedures:
```sql
START TRANSACTION;
-- Operation
COMMIT; -- or ROLLBACK on error
```

### Multiple Operations (Future)
For operations spanning multiple procedures:
```csharp
using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();
using var transaction = await connection.BeginTransactionAsync();

try
{
    // Multiple operations
    await Operation1(connection, transaction);
    await Operation2(connection, transaction);
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Error Handling

### Three-Tier Error Handling

1. **Stored Procedure Level**: Validates and returns status
2. **Helper Level**: Retries transient errors, measures performance
3. **DAO Level**: Catches exceptions, logs errors, returns Model_Dao_Result

### Exception Types

```csharp
catch (MySqlException ex) when (ex.Number == 1062)
{
    // Duplicate key error
    return new Model_Dao_Result
    {
        Success = false,
        ErrorMessage = "Record already exists",
        Severity = Enum_ErrorSeverity.Warning
    };
}
catch (MySqlException ex)
{
    // Database-specific errors
    // Log and return error result
}
catch (Exception ex)
{
    // Unexpected errors
    // Log and return error result
}
```

## Retry Logic

Retry logic is automatic in `Helper_Database_StoredProcedure`:
- **Max Attempts**: 3
- **Delays**: 100ms, 200ms, 400ms
- **Transient Errors**: MySQL error codes 1205, 1213, 2006, 2013

DAOs don't need explicit retry logic.

## Testing

### Unit Tests
Mock the database connection:
```csharp
[Test]
public async Task InsertReceivingLine_ValidData_ReturnsSuccess()
{
    // Arrange
    var line = new Model_ReceivingLine { /* test data */ };
    
    // Act
    var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);
    
    // Assert
    Assert.IsTrue(result.Success);
    Assert.AreEqual(1, result.AffectedRows);
}
```

### Integration Tests
Test against real database:
```csharp
[Test]
public async Task InsertReceivingLine_DatabaseUnavailable_ReturnsError()
{
    // Stop MySQL before this test
    var result = await Dao_ReceivingLine.InsertReceivingLineAsync(testLine);
    
    Assert.IsFalse(result.Success);
    Assert.IsNotEmpty(result.ErrorMessage);
}
```

## Performance Guidelines

### Connection Management
- Use `using` statements for connections
- Don't keep connections open longer than necessary
- Let Helper manage connection pooling

### Query Optimization
- Use indexes on frequently-queried columns
- Avoid SELECT * (specify columns)
- Use stored procedures (compiled and cached)

### Monitoring
Check `Model_Dao_Result.ExecutionTimeMs`:
```csharp
var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);

if (result.ExecutionTimeMs > 500) // Threshold: 500ms
{
    _logger.LogWarning($"Slow database operation: {result.ExecutionTimeMs}ms");
}
```

## Common Patterns

### Bulk Operations
```csharp
public static async Task<Model_Dao_Result> InsertMultipleAsync(List<Model_ReceivingLine> lines)
{
    int successCount = 0;
    var errors = new List<string>();

    foreach (var line in lines)
    {
        var result = await InsertReceivingLineAsync(line);
        if (result.Success)
            successCount++;
        else
            errors.Add(result.ErrorMessage);
    }

    return new Model_Dao_Result
    {
        Success = errors.Count == 0,
        AffectedRows = successCount,
        ErrorMessage = string.Join("; ", errors)
    };
}
```

### Conditional Updates
```csharp
public static async Task<Model_Dao_Result> UpdateReceivingLineIfExistsAsync(Model_ReceivingLine line)
{
    // First check if exists
    var existsResult = await GetReceivingLineByIdAsync(line.Id);
    if (!existsResult.Success)
        return existsResult;

    // Then update
    return await UpdateReceivingLineAsync(line);
}
```

## Examples

See existing implementations:
- `Data/Receiving/Dao_ReceivingLine.cs` - Complete CRUD example
- `Data/Receiving/Dao_DunnageLine.cs` - Simplified example
- `Data/Receiving/Dao_RoutingLabel.cs` - Optional field handling
- `Helpers/Database/Helper_Database_StoredProcedure.cs` - Helper utility
- `Database/StoredProcedures/Receiving/*.sql` - Stored procedure examples

# MySQL Service Pattern

**Category**: Service Layer
**Last Updated**: December 26, 2025
**Applies To**: `Service_MySQL_Receiving`, `Service_MySQL_Dunnage`, `Service_MySQL_PackagePreferences`

## Overview

MySQL Services act as a facade over the Data Access Objects (DAOs). They provide a higher-level API for ViewModels, handling business logic, validation, and error logging before calling the DAOs.

## Responsibilities

1.  **Validation**: Check inputs before calling DAO (Fail Fast).
2.  **DAO Orchestration**: Call one or more DAO methods to complete a business operation.
3.  **Error Handling**: Catch exceptions, log them using `ILoggingService`, and return user-friendly `Model_Dao_Result`.
4.  **Data Transformation**: Convert DAO results if necessary (though usually they return Models directly).

## Pattern

```csharp
public async Task<Model_Dao_Result> DeleteItemAsync(int id)
{
    try
    {
        // 1. Validation
        if (id <= 0) return Model_Dao_Result.Failure("Invalid ID");

        // 2. Business Logic (e.g., check dependencies)
        var count = await Dao_Dependency.GetCountAsync(id);
        if (count > 0) return Model_Dao_Result.Failure("Cannot delete: Item is in use.");

        // 3. DAO Call
        var result = await Dao_Item.DeleteAsync(id);
        
        // 4. Logging (on failure)
        if (!result.IsSuccess)
        {
            _logger.LogError($"Failed to delete item {id}: {result.ErrorMessage}", "Service_MySQL_Example");
        }

        return result;
    }
    catch (Exception ex)
    {
        // 5. Exception Handling
        _logger.LogError("Exception in DeleteItemAsync", ex, "Service_MySQL_Example");
        return Model_Dao_Result.Failure($"System error: {ex.Message}");
    }
}
```

## Dependency Injection
- Register as **Transient** (usually stateless) or **Singleton** (if caching is involved, though rare for these services).
- Inject `ILoggingService` and connection strings.

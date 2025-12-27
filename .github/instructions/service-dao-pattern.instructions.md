# Service-DAO Pattern Instructions

## Purpose
This document defines the standard pattern for interaction between the Service Layer and the Data Access Layer (DAOs).

## The Pattern

The application follows a strict layering:
`ViewModel` -> `Service` -> `DAO` -> `Database`

### Rules

1.  **Services Own Business Logic**: Services are responsible for orchestration, validation, logging, and error handling.
2.  **DAOs Own Data Access**: DAOs are responsible for executing database queries and mapping results to Models.
3.  **No Direct DB Access in Services**: Services must NOT use `MySqlConnection`, `SqlCommand`, or `Helper_Database_StoredProcedure` directly. They must delegate to a DAO.
4.  **Dependency Injection**: Services must receive DAOs via constructor injection.

## Implementation Example

### 1. The DAO (Data Access)

```csharp
public class Dao_Example
{
    private readonly string _connectionString;

    public Dao_Example(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result<List<Model_Example>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Example>(
            _connectionString,
            "sp_example_get_all",
            MapFromReader
        );
    }
}
```

### 2. The Service (Business Logic)

```csharp
public class Service_Example : IService_Example
{
    private readonly Dao_Example _daoExample;
    private readonly IService_ErrorHandler _errorHandler;

    public Service_Example(Dao_Example daoExample, IService_ErrorHandler errorHandler)
    {
        _daoExample = daoExample;
        _errorHandler = errorHandler;
    }

    public async Task<Model_Dao_Result<List<Model_Example>>> GetExamplesAsync()
    {
        try
        {
            // Business logic (e.g. validation, caching) can go here
            return await _daoExample.GetAllAsync();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleErrorAsync("Error getting examples", Enum_ErrorSeverity.Error, ex);
            return DaoResultFactory.Failure<List<Model_Example>>(ex.Message);
        }
    }
}
```

## Benefits
-   **Testability**: Services can be unit tested by mocking the DAO.
-   **Separation of Concerns**: Database logic is isolated from business logic.
-   **Maintainability**: Changes to database schema only affect the DAO.

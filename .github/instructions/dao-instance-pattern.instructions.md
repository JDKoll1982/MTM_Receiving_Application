# DAO Instance Pattern Instructions

## Purpose

This document defines the standard for creating Data Access Objects (DAOs) as instance-based classes, replacing the legacy static pattern.

## Why Instance-Based?

1. **Dependency Injection**: Allows DAOs to be injected into Services, enabling loose coupling.
2. **Configuration**: Connection strings and other settings can be injected via constructor.
3. **Testability**: Instance methods can be mocked in unit tests (via interfaces or virtual methods), whereas static methods cannot be easily mocked.
4. **Thread Safety**: Avoids shared static state issues (though DAOs should generally be stateless).

## The Pattern

### 1. Class Definition

- `public class` (NOT `static`)
- Constructor accepts `connectionString` (or configuration object).
- Store connection string in `private readonly` field.

```csharp
public class Dao_Example
{
    private readonly string _connectionString;

    public Dao_Example(string connectionString)
    {
        _connectionString = connectionString;
    }
    // ...
}
```

### 2. Methods

- `public async Task<Model_Dao_Result<T>>` (Standard return type)
- Use `_connectionString` field instead of static helper property.

```csharp
public async Task<Model_Dao_Result<List<Model_Example>>> GetAllAsync()
{
    return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Example>(
        _connectionString, // Use instance field
        "sp_example_get_all",
        MapFromReader
    );
}
```

### 3. Registration

Register in `App.xaml.cs` as Singleton (since they are stateless).

```csharp
services.AddSingleton(sp => new Dao_Example(Helper_Database_Variables.GetConnectionString()));
```

## Migration Guide (Static -> Instance)

1. Remove `static` from class definition.
2. Add constructor with `connectionString` parameter.
3. Replace `static string ConnectionString => ...` with `private readonly string _connectionString`.
4. Remove `static` from all methods.
5. Update all usages to use Dependency Injection instead of `Dao_Example.Method()`.

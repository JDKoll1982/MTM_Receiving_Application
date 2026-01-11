# Architectural Patterns

## MVVM Architecture (NON-NEGOTIABLE)

### Layer Separation Rules
**ViewModels SHALL NOT:**
- Directly instantiate or call DAO classes (`Dao_*`)
- Access database helpers (`Helper_Database_*`)
- Use connection strings or database configuration
- Contain any database access logic

**Correct Flow:** View → ViewModel → Service → DAO → Database

### Example of Violations
```csharp
// ❌ FORBIDDEN - ViewModel calling DAO
var result = await Dao_ReceivingLine.InsertAsync(line);

// ❌ FORBIDDEN - ViewModel instantiating DAO
var dao = new Dao_User(connectionString);

// ✅ CORRECT - ViewModel calling Service
var result = await _receivingService.AddLineAsync(line);
```

## Service → DAO Delegation Pattern (MANDATORY)

Services MUST delegate to DAOs - NO direct database access in service classes.

```csharp
// ✅ CORRECT Pattern
public class Service_MySQL_Receiving : IService_MySQL_Receiving
{
    private readonly Dao_ReceivingLoad _dao;

    public Service_MySQL_Receiving(Dao_ReceivingLoad dao)
    {
        _dao = dao;
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync()
    {
        return await _dao.GetAllAsync();
    }
}

// ❌ FORBIDDEN - Service with direct database access
public class Service_MySQL_Receiving
{
    public async Task<int> SaveAsync(Model_ReceivingLoad load)
    {
        using var connection = new MySqlConnection(_connectionString); // ❌ NO!
    }
}
```

## Instance-Based DAO Pattern (MANDATORY)

ALL DAOs must be instance-based classes (NOT static).

```csharp
// ✅ CORRECT - Instance-based DAO
public class Dao_ReceivingLoad
{
    private readonly string _connectionString;

    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_ReceivingLoad>(
            _connectionString,
            "sp_receiving_history_get_all",
            MapFromReader
        );
    }
}

// ❌ FORBIDDEN - Static DAO (legacy pattern)
public static class Dao_DunnageLoad
{
    private static string ConnectionString => Helper_Database_Variables.GetConnectionString();
    public static async Task<Model_Dao_Result> GetAllAsync() { }
}
```

## Dependency Injection Registration

All services and DAOs must be registered in `App.xaml.cs`:

```csharp
// Get connection strings
var connectionString = Helper_Database_Variables.GetConnectionString();

// Register DAOs as Singletons (stateless)
services.AddSingleton(sp => new Dao_ReceivingLine(connectionString));
services.AddSingleton(sp => new Dao_User(connectionString));

// Register Services (inject DAOs)
services.AddSingleton<IService_MySQL_Receiving>(sp => new Service_MySQL_Receiving(
    sp.GetRequiredService<Dao_ReceivingLine>(),
    sp.GetRequiredService<ILoggingService>()
));

// Register ViewModels as Transient
services.AddTransient<ReceivingViewModel>();
```

## Model_Dao_Result Pattern

All DAO methods MUST return `Model_Dao_Result<T>` or `Model_Dao_Result`.

```csharp
public async Task<Model_Dao_Result<int>> InsertAsync(Model_ReceivingLine line)
{
    try
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_part_id", line.PartID },
            { "p_quantity", line.Quantity }
        };

        return await Helper_Database_StoredProcedure.ExecuteScalarAsync<int>(
            _connectionString,
            "sp_receiving_line_insert",
            parameters
        );
    }
    catch (Exception ex)
    {
        return DaoResultFactory.Failure<int>($"Error inserting line: {ex.Message}", ex);
    }
}
```

## Pre-Commit Validation

ALWAYS verify before committing:
- ✅ No ViewModel→DAO dependencies
- ✅ No Service→Database direct access
- ✅ All DAOs are instance-based and registered in DI
- ✅ No static DAOs created
- ✅ Dependency graph shows no cycles

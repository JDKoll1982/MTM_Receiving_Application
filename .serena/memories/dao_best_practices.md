# DAO Best Practices

## Instance-Based Pattern (MANDATORY)

### Class Structure
```csharp
public class Dao_EntityName
{
    private readonly string _connectionString;
    
    public Dao_EntityName(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    // Methods use _connectionString instance field
}
```

### ❌ Legacy Static Pattern (Deprecated)
```csharp
// DO NOT USE - Static DAOs are prohibited
public static class Dao_EntityName
{
    private static string ConnectionString => Helper_Database_Variables.GetConnectionString();
    public static async Task<Model_Dao_Result> GetAllAsync() { }
}
```

## Method Signatures

### Return Type
ALL methods must return `Task<Model_Dao_Result<T>>` or `Task<Model_Dao_Result>`

```csharp
// ✅ CORRECT
public async Task<Model_Dao_Result<int>> InsertAsync(Model_Entity entity)
public async Task<Model_Dao_Result<List<Model_Entity>>> GetAllAsync()
public async Task<Model_Dao_Result> UpdateAsync(Model_Entity entity)

// ❌ WRONG
public async Task<int> InsertAsync(Model_Entity entity) // Missing Model_Dao_Result
public List<Model_Entity> GetAll() // Not async
```

### Naming Convention
- Format: `<Action><Entity>Async` or `<Action>Async`
- Actions: Insert, Update, Delete, Get, GetAll, GetBy<Criteria>
- Always suffix with `Async`

## Using Helper_Database_StoredProcedure

### For MySQL Operations
```csharp
public async Task<Model_Dao_Result<int>> InsertAsync(Model_ReceivingLine line)
{
    try
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_part_id", line.PartID },
            { "p_quantity", line.Quantity },
            { "p_po_number", line.PONumber }
        };
        
        return await Helper_Database_StoredProcedure.ExecuteScalarAsync<int>(
            _connectionString,
            "sp_sp_Receiving_Line_Insert",
            parameters
        );
    }
    catch (Exception ex)
    {
        return DaoResultFactory.Failure<int>(
            $"Error inserting receiving line: {ex.Message}",
            ex);
    }
}
```

### Parameter Naming
- C# parameter names match stored procedure parameters
- NO `p_` prefix in C# (added automatically by helper)
- Example: C# `"part_id"` → SQL `@p_part_id`

## Error Handling in DAOs

### NEVER Throw Exceptions
```csharp
// ❌ WRONG - Throwing exception
public async Task<Model_Dao_Result<int>> InsertAsync(Model_Entity entity)
{
    if (entity == null)
        throw new ArgumentNullException(nameof(entity)); // NO!
}

// ✅ CORRECT - Return failure result
public async Task<Model_Dao_Result<int>> InsertAsync(Model_Entity entity)
{
    if (entity == null)
        return DaoResultFactory.Failure<int>("Entity cannot be null");
    
    try
    {
        // Database operation
    }
    catch (Exception ex)
    {
        return DaoResultFactory.Failure<int>($"Error: {ex.Message}", ex);
    }
}
```

### Using DaoResultFactory
```csharp
// Success
return DaoResultFactory.Success(data, affectedRows: 1);

// Failure
return DaoResultFactory.Failure<T>("Error message", exception);
```

## DI Registration Pattern

### In App.xaml.cs
```csharp
private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    // Get connection string
    var connectionString = Helper_Database_Variables.GetConnectionString();
    
    // Register DAOs as Singletons (stateless)
    services.AddSingleton(sp => new Dao_User(connectionString));
    services.AddSingleton(sp => new Dao_ReceivingLine(connectionString));
    services.AddSingleton(sp => new Dao_ReceivingLoad(connectionString));
    
    // For Infor Visual (READ-ONLY)
    var inforConnectionString = Helper_Database_Variables.GetInforVisualConnectionString();
    services.AddSingleton(sp => new Dao_InforVisualPO(inforConnectionString));
}
```

## MySQL vs Infor Visual

### MySQL Pattern (Stored Procedures)
```csharp
public async Task<Model_Dao_Result<List<Model_ReceivingLine>>> GetAllAsync()
{
    return await Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync<Model_ReceivingLine>(
        _connectionString,
        "sp_receiving_line_get_all",
        new Dictionary<string, object>()
    );
}
```

### Infor Visual Pattern (Direct SQL - READ ONLY)
```csharp
public class Dao_InforVisualPO
{
    public Dao_InforVisualPO(string connectionString)
    {
        // VALIDATE READ-ONLY
        if (!connectionString.Contains("ApplicationIntent=ReadOnly"))
            throw new InvalidOperationException("Infor Visual must be READ-ONLY");
        
        _connectionString = connectionString;
    }
    
    public async Task<Model_Dao_Result<Model_InforVisualPO>> GetPOAsync(string poNumber)
    {
        try
        {
            string query = "SELECT * FROM PURCHASE_ORDER WHERE ID = @PoNumber";
            
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);
            
            await connection.OpenAsync();
            // Execute and map results
        }
        catch (Exception ex)
        {
            return DaoResultFactory.Failure<Model_InforVisualPO>($"Error: {ex.Message}", ex);
        }
    }
}
```

## Testing DAOs

### Integration Tests (Preferred)
```csharp
[Fact]
public async Task InsertAsync_ValidEntity_ReturnsNewId()
{
    // Arrange
    var dao = new Dao_ReceivingLine(TestHelper.GetTestConnectionString());
    var line = new Model_ReceivingLine
    {
        PartID = "TEST-PART",
        Quantity = 100,
        PONumber = "TEST-PO"
    };
    
    // Act
    var result = await dao.InsertAsync(line);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Data > 0);
}
```

## File Organization

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

## Checklist for New DAOs

- [ ] Instance-based class (NOT static)
- [ ] Constructor accepts `connectionString` parameter
- [ ] All methods return `Model_Dao_Result<T>`
- [ ] All methods are async
- [ ] Use `Helper_Database_StoredProcedure` for MySQL
- [ ] No exceptions thrown (return failure results)
- [ ] Registered in DI container (`App.xaml.cs`)
- [ ] Integration tests written
- [ ] XML documentation comments added

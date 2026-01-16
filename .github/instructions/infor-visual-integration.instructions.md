# Infor Visual Integration Standards

**Category**: Database Integration
**Last Updated**: December 27, 2025
**Applies To**: `IService_InforVisual`, `Service_InforVisual`, `Dao_InforVisual*`

## Critical Constraints

⚠️ **STRICT READ-ONLY POLICY** ⚠️

The application interacts with the Infor Visual ERP database (SQL Server). Under **NO CIRCUMSTANCES** should the application attempt to write, update, or delete data in this database.

1. **Connection String**: Must include `ApplicationIntent=ReadOnly`.
2. **Operations**: `SELECT` statements only.
3. **Stored Procedures**: Do not call stored procedures that might modify data.
4. **Architecture**: Services must delegate to **DAOs**. Services must NOT use `SqlConnection` directly.

## Architecture Pattern

**Correct Flow**:
`ViewModel` → `IService_InforVisual` → `Dao_InforVisualPO` / `Dao_InforVisualPart` → `SQL Server`

**Prohibited**:
`ViewModel` → `Dao_InforVisualPO` (Direct DAO access)
`Service_InforVisual` → `SqlConnection` (Direct SQL access in Service)

## Service Contract (`IService_InforVisual`)

The service provides methods to retrieve:

- Purchase Order details (`GetPOWithPartsAsync`)
- Part details (`GetPartByIDAsync`)
- Receiving history for validation (`GetSameDayReceivingQuantityAsync`)
- Remaining quantity calculations (`GetRemainingQuantityAsync`)

## Implementation Guidelines

### 1. Service Implementation

Services should inject DAOs and delegate calls.

```csharp
public class Service_InforVisual : IService_InforVisual
{
    private readonly Dao_InforVisualPO _daoPO;
    private readonly Dao_InforVisualPart _daoPart;

    public Service_InforVisual(Dao_InforVisualPO daoPO, Dao_InforVisualPart daoPart)
    {
        _daoPO = daoPO;
        _daoPart = daoPart;
    }

    public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetPOWithPartsAsync(string poNumber)
    {
        // Business logic (e.g. validation, logging)
        return await _daoPO.GetByPoAsync(poNumber);
    }
}
```

### 2. DAO Implementation (Read-Only)

DAOs handle the `SqlConnection` and SQL queries.

```csharp
public class Dao_InforVisualPO
{
    private readonly string _connectionString;

    public Dao_InforVisualPO(string connectionString)
    {
        if (!connectionString.Contains("ApplicationIntent=ReadOnly"))
            throw new InvalidOperationException("Connection must be ReadOnly");
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetByPoAsync(string poNumber)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = "SELECT ID, PART_ID FROM PO WHERE PO_ID = @PoID";
            // ... Execute query ...
        }
    }
}
```

### 3. Query Pattern

Use parameterized queries to prevent SQL injection, even for internal tools.

```csharp
string query = "SELECT ID, PART_ID FROM PART WHERE PART_ID = @PartID";
using (var command = new SqlCommand(query, connection))
{
    command.Parameters.AddWithValue("@PartID", partId);
    // ...
}
```

### 4. Error Handling

Wrap all SQL Server operations in try-catch blocks in the DAO. Return `Model_Dao_Result.Failure` on exception. Do not throw exceptions to the caller.

### 5. Data Mapping

Map SQL `SqlDataReader` results to `Model_InforVisualPO` or `Model_InforVisualPart` manually or using a lightweight mapper. Handle `DBNull` values gracefully.

## Testing

- **Mocking**: Unit tests for Services should mock `Dao_InforVisual*`.
- **Integration Tests**: Read-only integration tests against a dev/test instance of Infor Visual are permitted if configured.

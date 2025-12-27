# Infor Visual Integration Standards

**Category**: Database Integration
**Last Updated**: December 26, 2025
**Applies To**: `IService_InforVisual`, `Service_InforVisual`

## Critical Constraints

⚠️ **STRICT READ-ONLY POLICY** ⚠️

The application interacts with the Infor Visual ERP database (SQL Server). Under **NO CIRCUMSTANCES** should the application attempt to write, update, or delete data in this database.

1.  **Connection String**: Must include `ApplicationIntent=ReadOnly`.
2.  **Operations**: `SELECT` statements only.
3.  **Stored Procedures**: Do not call stored procedures that might modify data.

## Service Contract (`IService_InforVisual`)

The service provides methods to retrieve:
- Purchase Order details (`GetPOWithPartsAsync`)
- Part details (`GetPartByIDAsync`)
- Receiving history for validation (`GetSameDayReceivingQuantityAsync`)
- Remaining quantity calculations (`GetRemainingQuantityAsync`)

## Implementation Guidelines

### 1. Connection Management
Use `Microsoft.Data.SqlClient`. Ensure connections are disposed properly using `using` statements.

```csharp
using (var connection = new SqlConnection(_connectionString))
{
    await connection.OpenAsync();
    // ...
}
```

### 2. Query Pattern
Use parameterized queries to prevent SQL injection, even for internal tools.

```csharp
string query = "SELECT ID, PART_ID FROM PART WHERE PART_ID = @PartID";
using (var command = new SqlCommand(query, connection))
{
    command.Parameters.AddWithValue("@PartID", partId);
    // ...
}
```

### 3. Error Handling
Wrap all SQL Server operations in try-catch blocks. Return `Model_Dao_Result.Failure` on exception. Do not throw exceptions to the caller.

### 4. Data Mapping
Map SQL `SqlDataReader` results to `Model_InforVisualPO` or `Model_InforVisualPart` manually or using a lightweight mapper. Handle `DBNull` values gracefully.

## Testing
- **Mocking**: Unit tests should mock `IService_InforVisual`. Do not connect to the real ERP database in unit tests.
- **Integration Tests**: Read-only integration tests against a dev/test instance of Infor Visual are permitted if configured.

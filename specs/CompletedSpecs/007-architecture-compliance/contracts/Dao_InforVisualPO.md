# Dao_InforVisualPO Contract

**Type**: Data Access Object (READ-ONLY)  
**Namespace**: MTM_Receiving_Application.Data.InforVisual  
**Purpose**: Instance-based READ-ONLY DAO for Infor Visual ERP purchase order queries (SQL Server)

---

## Class Definition

```csharp
namespace MTM_Receiving_Application.Data.InforVisual;

public class Dao_InforVisualPO
{
    private readonly string _connectionString;
    
    public Dao_InforVisualPO(string inforVisualConnectionString)
    {
        ValidateReadOnlyConnection(inforVisualConnectionString);
        _connectionString = inforVisualConnectionString;
    }
    
    // Methods documented below
}
```

---

## Constructor

```csharp
public Dao_InforVisualPO(string inforVisualConnectionString)
```

**Parameters**:
- `inforVisualConnectionString` - SQL Server connection string with ApplicationIntent=ReadOnly

**Validation**:
- Throws ArgumentNullException if connection string is null
- Throws InvalidOperationException if ApplicationIntent=ReadOnly is missing

**Validation Logic**:
```csharp
private static void ValidateReadOnlyConnection(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new ArgumentNullException(nameof(connectionString));
    
    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        
        if (builder.ApplicationIntent != ApplicationIntent.ReadOnly)
        {
            throw new InvalidOperationException(
                $"CONSTITUTIONAL VIOLATION: Infor Visual DAO requires ApplicationIntent=ReadOnly. " +
                $"Current ApplicationIntent: {builder.ApplicationIntent}. " +
                $"Writing to Infor Visual ERP database (Server={builder.DataSource}, " +
                $"Database={builder.InitialCatalog}) is STRICTLY PROHIBITED. " +
                "See Constitution Principle X: Infor Visual DAO Architecture.");
        }
    }
    catch (ArgumentException ex)
    {
        throw new InvalidOperationException(
            $"Invalid Infor Visual connection string format: {ex.Message}", ex);
    }
}
```

**DI Registration** (App.xaml.cs):
```csharp
var inforVisualConnectionString = Helper_Database_Variables.GetInforVisualConnectionString();
// ☝️ Must include ApplicationIntent=ReadOnly

services.AddSingleton(sp => new Dao_InforVisualPO(inforVisualConnectionString));
```

---

## Methods

### GetByPoNumberAsync

**Purpose**: Retrieve all line items for a specific purchase order number.

**Signature**:
```csharp
public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetByPoNumberAsync(string poNumber)
```

**Query Type**: Direct SQL SELECT (not stored procedure - per constitutional allowance)

**Parameters**:
- `poNumber` (string) - Purchase order number (e.g., "PO-2025-001234")

**Returns**: `Model_Dao_Result<List<Model_InforVisualPO>>`
- Success: List of PO line items (may be empty if PO not found)
- Failure: Database error or connection error

**SQL Query**:
```sql
SELECT 
    po.po_num AS PoNumber,
    pol.po_line AS PoLine,
    pol.part AS PartNumber,
    p.description AS PartDescription,
    pol.qty_ordered AS OrderedQty,
    pol.qty_received AS ReceivedQty,
    (pol.qty_ordered - pol.qty_received) AS RemainingQty,
    pol.u_m AS UnitOfMeasure,
    pol.due_date AS DueDate,
    po.vend_id AS VendorCode,
    v.name AS VendorName,
    po.stat AS PoStatus,
    po.site_id AS SiteId
FROM po
INNER JOIN po_line pol ON po.po_num = pol.po_num
INNER JOIN part p ON pol.part = p.part_id
LEFT JOIN vendor v ON po.vend_id = v.vend_id
WHERE po.po_num = @PoNumber
  AND po.site_id = '002'
ORDER BY pol.po_line;
```

**Implementation Pattern**:
```csharp
public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetByPoNumberAsync(string poNumber)
{
    try
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            var command = new SqlCommand(SQL_GET_PO_LINES, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);
            
            var poList = new List<Model_InforVisualPO>();
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    poList.Add(MapFromReader(reader));
                }
            }
            
            return DaoResultFactory.Success(poList);
        }
    }
    catch (SqlException ex)
    {
        return DaoResultFactory.Failure<List<Model_InforVisualPO>>(
            $"Error querying Infor Visual PO {poNumber}: {ex.Message}",
            ex);
    }
}

private static Model_InforVisualPO MapFromReader(SqlDataReader reader)
{
    return new Model_InforVisualPO
    {
        PoNumber = reader.GetString(reader.GetOrdinal("PoNumber")),
        PoLine = reader.GetInt32(reader.GetOrdinal("PoLine")),
        PartNumber = reader.GetString(reader.GetOrdinal("PartNumber")),
        PartDescription = reader.IsDBNull(reader.GetOrdinal("PartDescription")) 
            ? string.Empty 
            : reader.GetString(reader.GetOrdinal("PartDescription")),
        OrderedQty = reader.GetDecimal(reader.GetOrdinal("OrderedQty")),
        ReceivedQty = reader.GetDecimal(reader.GetOrdinal("ReceivedQty")),
        RemainingQty = reader.GetDecimal(reader.GetOrdinal("RemainingQty")),
        UnitOfMeasure = reader.GetString(reader.GetOrdinal("UnitOfMeasure")),
        DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) 
            ? (DateTime?)null 
            : reader.GetDateTime(reader.GetOrdinal("DueDate")),
        VendorCode = reader.GetString(reader.GetOrdinal("VendorCode")),
        VendorName = reader.IsDBNull(reader.GetOrdinal("VendorName")) 
            ? string.Empty 
            : reader.GetString(reader.GetOrdinal("VendorName")),
        PoStatus = reader.GetString(reader.GetOrdinal("PoStatus")),
        SiteId = reader.GetString(reader.GetOrdinal("SiteId"))
    };
}
```

---

### ValidatePoNumberAsync

**Purpose**: Check if a PO number exists in Infor Visual.

**Signature**:
```csharp
public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
```

**Query Type**: Direct SQL SELECT

**Parameters**:
- `poNumber` (string)

**Returns**: `Model_Dao_Result<bool>`
- Success: Data = true if PO exists, false if not found
- Failure: Database error

**SQL Query**:
```sql
SELECT COUNT(*) AS PoCount
FROM po
WHERE po_num = @PoNumber
  AND site_id = '002';
```

**Implementation**:
```csharp
public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
{
    try
    {
        const string sql = "SELECT COUNT(*) FROM po WHERE po_num = @PoNumber AND site_id = '002'";
        
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);
            
            var count = (int)await command.ExecuteScalarAsync();
            
            return DaoResultFactory.Success(count > 0);
        }
    }
    catch (SqlException ex)
    {
        return DaoResultFactory.Failure<bool>(
            $"Error validating PO {poNumber}: {ex.Message}",
            ex);
    }
}
```

---

## Prohibited Operations

**Constitutional Constraint**: This DAO is READ-ONLY. The following operations are **FORBIDDEN**:

```csharp
// ❌ FORBIDDEN - NO INSERT methods
public async Task<Model_Dao_Result> InsertPoAsync(Model_InforVisualPO po) { }

// ❌ FORBIDDEN - NO UPDATE methods
public async Task<Model_Dao_Result> UpdatePoStatusAsync(string poNumber, string status) { }

// ❌ FORBIDDEN - NO DELETE methods
public async Task<Model_Dao_Result> DeletePoAsync(string poNumber) { }

// ❌ FORBIDDEN - NO DDL operations
public async Task<Model_Dao_Result> CreateTempTableAsync() { }
```

**Rationale**: Infor Visual is the production ERP system. Any write operations could corrupt critical business data. Constitutional Principle X mandates READ-ONLY access with ApplicationIntent enforcement.

---

## Error Handling

All methods use standard DAO pattern:

```csharp
try
{
    // Database operation
    return DaoResultFactory.Success(data);
}
catch (SqlException ex)
{
    // Never throw - return failure result
    return DaoResultFactory.Failure<T>($"Error message: {ex.Message}", ex);
}
```

---

## Connection String Requirements

**Valid Connection String** (from Helper_Database_Variables.GetInforVisualConnectionString()):
```
Server=VISUAL;Database=MTMFG;User ID=SHOP2;Password=SHOP;ApplicationIntent=ReadOnly;Connection Timeout=30;
```

**Invalid Connection Strings** (will throw InvalidOperationException):
```
❌ Server=VISUAL;Database=MTMFG;User ID=SHOP2;Password=SHOP;
   (Missing ApplicationIntent=ReadOnly)

❌ Server=VISUAL;Database=MTMFG;User ID=SHOP2;Password=SHOP;ApplicationIntent=ReadWrite;
   (Wrong ApplicationIntent value)
```

---

## Testing Approach

### Unit Tests (Constructor Validation)

```csharp
[Fact]
public void Constructor_ThrowsIfConnectionStringMissingReadOnlyIntent()
{
    // Arrange
    var invalidConnectionString = "Server=VISUAL;Database=MTMFG;User ID=test;";
    
    // Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() => 
        new Dao_InforVisualPO(invalidConnectionString));
    
    Assert.Contains("ApplicationIntent=ReadOnly", exception.Message);
    Assert.Contains("CONSTITUTIONAL VIOLATION", exception.Message);
}

[Fact]
public void Constructor_SucceedsWithValidReadOnlyConnectionString()
{
    // Arrange
    var validConnectionString = "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly;";
    
    // Act
    var dao = new Dao_InforVisualPO(validConnectionString);
    
    // Assert
    Assert.NotNull(dao);
}
```

### Integration Tests (Real Database)

```csharp
[Fact]
public async Task GetByPoNumberAsync_ReturnsPoLines()
{
    // Arrange
    var dao = new Dao_InforVisualPO(TestHelper.GetInforVisualConnectionString());
    var testPoNumber = "PO-TEST-001"; // Known test PO in Infor Visual
    
    // Act
    var result = await dao.GetByPoNumberAsync(testPoNumber);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.All(result.Data, po => Assert.Equal(testPoNumber, po.PoNumber));
}
```

---

## Related Documentation

- **Constitution**: Principle X (Infor Visual DAO Architecture - READ-ONLY enforcement)
- **Service Usage**: See `IService_InforVisual` for service delegation pattern
- **Data Model**: See `Model_InforVisualPO` for entity definition
- **Integration Guide**: `specs/003-database-foundation/INFOR_VISUAL_INTEGRATION.md`

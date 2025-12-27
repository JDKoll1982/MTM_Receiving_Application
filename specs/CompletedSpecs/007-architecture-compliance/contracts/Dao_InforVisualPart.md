# Dao_InforVisualPart Contract

**Type**: Data Access Object (READ-ONLY)  
**Namespace**: MTM_Receiving_Application.Data.InforVisual  
**Purpose**: Instance-based READ-ONLY DAO for Infor Visual ERP part master queries (SQL Server)

---

## Class Definition

```csharp
namespace MTM_Receiving_Application.Data.InforVisual;

public class Dao_InforVisualPart
{
    private readonly string _connectionString;
    
    public Dao_InforVisualPart(string inforVisualConnectionString)
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
public Dao_InforVisualPart(string inforVisualConnectionString)
```

**Parameters**:
- `inforVisualConnectionString` - SQL Server connection string with ApplicationIntent=ReadOnly

**Validation**:
- Throws ArgumentNullException if connection string is null
- Throws InvalidOperationException if ApplicationIntent=ReadOnly is missing

**Validation Logic**: Same as Dao_InforVisualPO (see Dao_InforVisualPO.md for ValidateReadOnlyConnection implementation)

**DI Registration** (App.xaml.cs):
```csharp
var inforVisualConnectionString = Helper_Database_Variables.GetInforVisualConnectionString();
services.AddSingleton(sp => new Dao_InforVisualPart(inforVisualConnectionString));
```

---

## Methods

### GetByPartNumberAsync

**Purpose**: Retrieve part master details for a specific part number.

**Signature**:
```csharp
public async Task<Model_Dao_Result<Model_InforVisualPart>> GetByPartNumberAsync(string partNumber)
```

**Query Type**: Direct SQL SELECT (not stored procedure - per constitutional allowance)

**Parameters**:
- `partNumber` (string) - Part number (e.g., "PART-12345")

**Returns**: `Model_Dao_Result<Model_InforVisualPart>`
- Success with Data: Part found
- Success with Data = null: Part not found (not an error)
- Failure: Database error

**SQL Query**:
```sql
SELECT 
    p.part_id AS PartNumber,
    p.description AS Description,
    p.part_type AS PartType,
    p.unit_cost AS UnitCost,
    p.u_m AS PrimaryUom,
    COALESCE(inv.on_hand, 0) AS OnHandQty,
    COALESCE(inv.allocated, 0) AS AllocatedQty,
    (COALESCE(inv.on_hand, 0) - COALESCE(inv.allocated, 0)) AS AvailableQty,
    p.site_id AS DefaultSite,
    p.stat AS PartStatus,
    p.prod_line AS ProductLine
FROM part p
LEFT JOIN inventory inv ON p.part_id = inv.part_id AND inv.site_id = '002'
WHERE p.part_id = @PartNumber;
```

**Implementation Pattern**:
```csharp
public async Task<Model_Dao_Result<Model_InforVisualPart>> GetByPartNumberAsync(string partNumber)
{
    try
    {
        const string sql = @"
            SELECT 
                p.part_id AS PartNumber,
                p.description AS Description,
                p.part_type AS PartType,
                p.unit_cost AS UnitCost,
                p.u_m AS PrimaryUom,
                COALESCE(inv.on_hand, 0) AS OnHandQty,
                COALESCE(inv.allocated, 0) AS AllocatedQty,
                (COALESCE(inv.on_hand, 0) - COALESCE(inv.allocated, 0)) AS AvailableQty,
                p.site_id AS DefaultSite,
                p.stat AS PartStatus,
                p.prod_line AS ProductLine
            FROM part p
            LEFT JOIN inventory inv ON p.part_id = inv.part_id AND inv.site_id = '002'
            WHERE p.part_id = @PartNumber";
        
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PartNumber", partNumber);
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var part = MapFromReader(reader);
                    return DaoResultFactory.Success(part);
                }
                
                // Part not found - return success with null data
                return DaoResultFactory.Success<Model_InforVisualPart>(null);
            }
        }
    }
    catch (SqlException ex)
    {
        return DaoResultFactory.Failure<Model_InforVisualPart>(
            $"Error querying part {partNumber}: {ex.Message}",
            ex);
    }
}

private static Model_InforVisualPart MapFromReader(SqlDataReader reader)
{
    return new Model_InforVisualPart
    {
        PartNumber = reader.GetString(reader.GetOrdinal("PartNumber")),
        Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
            ? string.Empty 
            : reader.GetString(reader.GetOrdinal("Description")),
        PartType = reader.IsDBNull(reader.GetOrdinal("PartType")) 
            ? string.Empty 
            : reader.GetString(reader.GetOrdinal("PartType")),
        UnitCost = reader.GetDecimal(reader.GetOrdinal("UnitCost")),
        PrimaryUom = reader.GetString(reader.GetOrdinal("PrimaryUom")),
        OnHandQty = reader.GetDecimal(reader.GetOrdinal("OnHandQty")),
        AllocatedQty = reader.GetDecimal(reader.GetOrdinal("AllocatedQty")),
        AvailableQty = reader.GetDecimal(reader.GetOrdinal("AvailableQty")),
        DefaultSite = reader.GetString(reader.GetOrdinal("DefaultSite")),
        PartStatus = reader.GetString(reader.GetOrdinal("PartStatus")),
        ProductLine = reader.IsDBNull(reader.GetOrdinal("ProductLine")) 
            ? string.Empty 
            : reader.GetString(reader.GetOrdinal("ProductLine"))
    };
}
```

---

### SearchPartsByDescriptionAsync

**Purpose**: Search for parts by description (fuzzy search).

**Signature**:
```csharp
public async Task<Model_Dao_Result<List<Model_InforVisualPart>>> SearchPartsByDescriptionAsync(
    string searchTerm, 
    int maxResults = 50)
```

**Query Type**: Direct SQL SELECT with LIKE clause

**Parameters**:
- `searchTerm` (string) - Search term (e.g., "bearing")
- `maxResults` (int) - Maximum number of results to return (default 50)

**Returns**: `Model_Dao_Result<List<Model_InforVisualPart>>`
- Success: List of parts matching search (may be empty)
- Failure: Database error

**SQL Query**:
```sql
SELECT TOP @MaxResults
    p.part_id AS PartNumber,
    p.description AS Description,
    p.part_type AS PartType,
    p.unit_cost AS UnitCost,
    p.u_m AS PrimaryUom,
    COALESCE(inv.on_hand, 0) AS OnHandQty,
    COALESCE(inv.allocated, 0) AS AllocatedQty,
    (COALESCE(inv.on_hand, 0) - COALESCE(inv.allocated, 0)) AS AvailableQty,
    p.site_id AS DefaultSite,
    p.stat AS PartStatus,
    p.prod_line AS ProductLine
FROM part p
LEFT JOIN inventory inv ON p.part_id = inv.part_id AND inv.site_id = '002'
WHERE p.description LIKE @SearchTerm + '%'
  AND p.stat = 'ACTIVE'
ORDER BY p.part_id;
```

**Implementation**:
```csharp
public async Task<Model_Dao_Result<List<Model_InforVisualPart>>> SearchPartsByDescriptionAsync(
    string searchTerm, 
    int maxResults = 50)
{
    try
    {
        const string sql = @"
            SELECT TOP (@MaxResults)
                p.part_id AS PartNumber,
                p.description AS Description,
                p.part_type AS PartType,
                p.unit_cost AS UnitCost,
                p.u_m AS PrimaryUom,
                COALESCE(inv.on_hand, 0) AS OnHandQty,
                COALESCE(inv.allocated, 0) AS AllocatedQty,
                (COALESCE(inv.on_hand, 0) - COALESCE(inv.allocated, 0)) AS AvailableQty,
                p.site_id AS DefaultSite,
                p.stat AS PartStatus,
                p.prod_line AS ProductLine
            FROM part p
            LEFT JOIN inventory inv ON p.part_id = inv.part_id AND inv.site_id = '002'
            WHERE p.description LIKE @SearchTerm + '%'
              AND p.stat = 'ACTIVE'
            ORDER BY p.part_id";
        
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@SearchTerm", searchTerm);
            command.Parameters.AddWithValue("@MaxResults", maxResults);
            
            var partList = new List<Model_InforVisualPart>();
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    partList.Add(MapFromReader(reader));
                }
            }
            
            return DaoResultFactory.Success(partList);
        }
    }
    catch (SqlException ex)
    {
        return DaoResultFactory.Failure<List<Model_InforVisualPart>>(
            $"Error searching parts by description '{searchTerm}': {ex.Message}",
            ex);
    }
}
```

---

## Prohibited Operations

**Constitutional Constraint**: This DAO is READ-ONLY. The following operations are **FORBIDDEN**:

```csharp
// ❌ FORBIDDEN - NO INSERT methods
public async Task<Model_Dao_Result> InsertPartAsync(Model_InforVisualPart part) { }

// ❌ FORBIDDEN - NO UPDATE methods
public async Task<Model_Dao_Result> UpdatePartCostAsync(string partNumber, decimal newCost) { }

// ❌ FORBIDDEN - NO DELETE methods
public async Task<Model_Dao_Result> DeletePartAsync(string partNumber) { }

// ❌ FORBIDDEN - NO inventory adjustments
public async Task<Model_Dao_Result> AdjustInventoryAsync(string partNumber, decimal qty) { }
```

**Rationale**: Infor Visual is the production ERP system. Part master and inventory data must only be modified through Infor Visual's official interfaces to maintain data integrity.

---

## Error Handling

Same pattern as Dao_InforVisualPO:

```csharp
try
{
    // Database operation
    return DaoResultFactory.Success(data);
}
catch (SqlException ex)
{
    return DaoResultFactory.Failure<T>($"Error message: {ex.Message}", ex);
}
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
        new Dao_InforVisualPart(invalidConnectionString));
    
    Assert.Contains("ApplicationIntent=ReadOnly", exception.Message);
}
```

### Integration Tests

```csharp
[Fact]
public async Task GetByPartNumberAsync_ReturnsPartIfExists()
{
    // Arrange
    var dao = new Dao_InforVisualPart(TestHelper.GetInforVisualConnectionString());
    var knownPartNumber = "TEST-PART-001";
    
    // Act
    var result = await dao.GetByPartNumberAsync(knownPartNumber);
    
    // Assert
    Assert.True(result.IsSuccess);
    if (result.Data != null)
    {
        Assert.Equal(knownPartNumber, result.Data.PartNumber);
        Assert.NotEmpty(result.Data.Description);
    }
}

[Fact]
public async Task SearchPartsByDescriptionAsync_ReturnsMatchingParts()
{
    // Arrange
    var dao = new Dao_InforVisualPart(TestHelper.GetInforVisualConnectionString());
    
    // Act
    var result = await dao.SearchPartsByDescriptionAsync("bearing", maxResults: 10);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.True(result.Data.Count <= 10);
}
```

---

## Related Documentation

- **Constitution**: Principle X (Infor Visual DAO Architecture - READ-ONLY enforcement)
- **Service Usage**: See `IService_InforVisual` for service delegation pattern
- **Data Model**: See `Model_InforVisualPart` for entity definition
- **Integration Guide**: `specs/003-database-foundation/INFOR_VISUAL_INTEGRATION.md`

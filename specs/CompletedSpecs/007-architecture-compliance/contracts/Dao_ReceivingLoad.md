# Dao_ReceivingLoad Contract

**Type**: Data Access Object  
**Namespace**: MTM_Receiving_Application.Data.Receiving  
**Purpose**: Instance-based DAO for MySQL `receiving_loads` table operations

---

## Class Definition

```csharp
namespace MTM_Receiving_Application.Data.Receiving;

public class Dao_ReceivingLoad
{
    private readonly string _connectionString;
    
    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    // Methods documented below
}
```

---

## Constructor

```csharp
public Dao_ReceivingLoad(string connectionString)
```

**Parameters**:
- `connectionString` - MySQL connection string from Helper_Database_Variables.GetConnectionString()

**Validation**:
- Throws ArgumentNullException if connectionString is null

**DI Registration** (App.xaml.cs):
```csharp
var mySqlConnectionString = Helper_Database_Variables.GetConnectionString();
services.AddSingleton(sp => new Dao_ReceivingLoad(mySqlConnectionString));
```

---

## Methods

### GetAllAsync

**Purpose**: Retrieve all receiving loads from database, ordered by date descending.

**Signature**:
```csharp
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync()
```

**Stored Procedure**: `sp_receiving_loads_get_all`

**Parameters**: None

**Returns**: `Model_Dao_Result<List<Model_ReceivingLoad>>`
- Success: List of all receiving loads (may be empty)
- Failure: Error message from stored procedure execution

**SQL Logic**:
```sql
SELECT 
    load_id,
    po_number,
    vendor,
    received_date,
    user_id,
    status,
    created_at
FROM receiving_loads
ORDER BY received_date DESC, load_id DESC;
```

---

### GetByDateRangeAsync

**Purpose**: Retrieve receiving loads within a specific date range.

**Signature**:
```csharp
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetByDateRangeAsync(
    DateTime startDate, 
    DateTime endDate)
```

**Stored Procedure**: `sp_receiving_loads_get_by_date_range`

**Parameters**:
- `start_date` (DateTime) - Inclusive start date
- `end_date` (DateTime) - Inclusive end date

**Returns**: `Model_Dao_Result<List<Model_ReceivingLoad>>`
- Success: List of loads within date range (may be empty)
- Failure: Error message

**Parameter Mapping**:
```csharp
var parameters = new Dictionary<string, object>
{
    { "start_date", startDate.Date },
    { "end_date", endDate.Date.AddDays(1).AddSeconds(-1) } // Include full end date
};
```

**SQL Logic**:
```sql
SELECT * FROM receiving_loads
WHERE received_date >= @start_date 
  AND received_date <= @end_date
ORDER BY received_date DESC;
```

---

### GetByIdAsync

**Purpose**: Retrieve a single receiving load by its primary key.

**Signature**:
```csharp
public async Task<Model_Dao_Result<Model_ReceivingLoad>> GetByIdAsync(int loadId)
```

**Stored Procedure**: `sp_receiving_loads_get_by_id`

**Parameters**:
- `load_id` (int) - Primary key

**Returns**: `Model_Dao_Result<Model_ReceivingLoad>`
- Success with Data: Load found
- Success with Data = null: Load not found (not an error)
- Failure: Database error

**Parameter Mapping**:
```csharp
var parameters = new Dictionary<string, object>
{
    { "load_id", loadId }
};
```

---

### InsertAsync

**Purpose**: Insert a new receiving load and return auto-increment ID.

**Signature**:
```csharp
public async Task<Model_Dao_Result<int>> InsertAsync(Model_ReceivingLoad load)
```

**Stored Procedure**: `sp_receiving_loads_insert`

**Parameters**:
- `po_number` (string)
- `vendor` (string)
- `received_date` (DateTime)
- `user_id` (int) - Foreign key to users table
- `status` (string) - Default "ACTIVE"

**Returns**: `Model_Dao_Result<int>`
- Success: Data contains new load_id (auto-increment)
- Failure: Error message (e.g., foreign key constraint violation)

**Parameter Mapping**:
```csharp
var parameters = new Dictionary<string, object>
{
    { "po_number", load.PoNumber },
    { "vendor", load.Vendor ?? string.Empty },
    { "received_date", load.ReceivedDate },
    { "user_id", load.UserId },
    { "status", load.Status ?? "ACTIVE" }
};

return await Helper_Database_StoredProcedure.ExecuteScalarAsync<int>(
    _connectionString,
    "sp_receiving_loads_insert",
    parameters);
```

**Example SP Logic**:
```sql
INSERT INTO receiving_loads (po_number, vendor, received_date, user_id, status)
VALUES (@po_number, @vendor, @received_date, @user_id, @status);

SELECT LAST_INSERT_ID();
```

---

### UpdateAsync

**Purpose**: Update an existing receiving load.

**Signature**:
```csharp
public async Task<Model_Dao_Result> UpdateAsync(Model_ReceivingLoad load)
```

**Stored Procedure**: `sp_receiving_loads_update`

**Parameters**:
- `load_id` (int) - Primary key
- `po_number` (string)
- `vendor` (string)
- `status` (string)

**Returns**: `Model_Dao_Result`
- Success: AffectedRows = 1 if load updated, 0 if load_id not found
- Failure: Error message

**Parameter Mapping**:
```csharp
var parameters = new Dictionary<string, object>
{
    { "load_id", load.LoadId },
    { "po_number", load.PoNumber },
    { "vendor", load.Vendor ?? string.Empty },
    { "status", load.Status ?? "ACTIVE" }
};

return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
    _connectionString,
    "sp_receiving_loads_update",
    parameters);
```

---

## Error Handling

All methods follow the DAO error handling pattern:

```csharp
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync()
{
    try
    {
        var parameters = new Dictionary<string, object>();
        
        var result = await Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync<Model_ReceivingLoad>(
            _connectionString,
            "sp_receiving_loads_get_all",
            parameters);
        
        return result.IsSuccess
            ? DaoResultFactory.Success(result.Data)
            : DaoResultFactory.Failure<List<Model_ReceivingLoad>>(result.ErrorMessage);
    }
    catch (Exception ex)
    {
        return DaoResultFactory.Failure<List<Model_ReceivingLoad>>(
            $"Error retrieving receiving loads: {ex.Message}",
            ex);
    }
}
```

**Key Points**:
- ✅ Never throw exceptions - return Model_Dao_Result.Failure() instead
- ✅ Wrap database calls in try-catch for unexpected errors
- ✅ Use DaoResultFactory for all result creation (no static methods in Model_Dao_Result)

---

## Stored Procedures Required

The following stored procedures must exist in `Database/StoredProcedures/`:

### sp_receiving_loads_get_all.sql
```sql
DROP PROCEDURE IF EXISTS sp_receiving_loads_get_all;

DELIMITER //
CREATE PROCEDURE sp_receiving_loads_get_all()
BEGIN
    SELECT 
        load_id AS LoadId,
        po_number AS PoNumber,
        vendor AS Vendor,
        received_date AS ReceivedDate,
        user_id AS UserId,
        status AS Status,
        created_at AS CreatedAt
    FROM receiving_loads
    ORDER BY received_date DESC, load_id DESC;
END //
DELIMITER ;
```

### sp_receiving_loads_get_by_date_range.sql
```sql
DROP PROCEDURE IF EXISTS sp_receiving_loads_get_by_date_range;

DELIMITER //
CREATE PROCEDURE sp_receiving_loads_get_by_date_range(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT 
        load_id AS LoadId,
        po_number AS PoNumber,
        vendor AS Vendor,
        received_date AS ReceivedDate,
        user_id AS UserId,
        status AS Status,
        created_at AS CreatedAt
    FROM receiving_loads
    WHERE received_date >= p_start_date 
      AND received_date <= p_end_date
    ORDER BY received_date DESC;
END //
DELIMITER ;
```

### sp_receiving_loads_get_by_id.sql
```sql
DROP PROCEDURE IF EXISTS sp_receiving_loads_get_by_id;

DELIMITER //
CREATE PROCEDURE sp_receiving_loads_get_by_id(IN p_load_id INT)
BEGIN
    SELECT 
        load_id AS LoadId,
        po_number AS PoNumber,
        vendor AS Vendor,
        received_date AS ReceivedDate,
        user_id AS UserId,
        status AS Status,
        created_at AS CreatedAt
    FROM receiving_loads
    WHERE load_id = p_load_id;
END //
DELIMITER ;
```

### sp_receiving_loads_insert.sql
```sql
DROP PROCEDURE IF EXISTS sp_receiving_loads_insert;

DELIMITER //
CREATE PROCEDURE sp_receiving_loads_insert(
    IN p_po_number VARCHAR(50),
    IN p_vendor VARCHAR(100),
    IN p_received_date DATETIME,
    IN p_user_id INT,
    IN p_status VARCHAR(20)
)
BEGIN
    INSERT INTO receiving_loads (po_number, vendor, received_date, user_id, status)
    VALUES (p_po_number, p_vendor, p_received_date, p_user_id, p_status);
    
    SELECT LAST_INSERT_ID() AS LoadId;
END //
DELIMITER ;
```

### sp_receiving_loads_update.sql
```sql
DROP PROCEDURE IF EXISTS sp_receiving_loads_update;

DELIMITER //
CREATE PROCEDURE sp_receiving_loads_update(
    IN p_load_id INT,
    IN p_po_number VARCHAR(50),
    IN p_vendor VARCHAR(100),
    IN p_status VARCHAR(20)
)
BEGIN
    UPDATE receiving_loads
    SET po_number = p_po_number,
        vendor = p_vendor,
        status = p_status
    WHERE load_id = p_load_id;
    
    SELECT ROW_COUNT() AS AffectedRows;
END //
DELIMITER ;
```

---

## Testing Approach

### Unit Tests (with Mock Database)
```csharp
[Fact]
public async Task GetAllAsync_ReturnsAllLoads()
{
    // Arrange
    var testConnectionString = TestHelper.GetTestConnectionString();
    var dao = new Dao_ReceivingLoad(testConnectionString);
    
    // Act
    var result = await dao.GetAllAsync();
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.IsType<List<Model_ReceivingLoad>>(result.Data);
}

[Fact]
public async Task InsertAsync_ReturnsNewLoadId()
{
    // Arrange
    var dao = new Dao_ReceivingLoad(TestHelper.GetTestConnectionString());
    var newLoad = new Model_ReceivingLoad
    {
        PoNumber = "TEST-PO-001",
        Vendor = "Test Vendor",
        ReceivedDate = DateTime.Now,
        UserId = 1,
        Status = "ACTIVE"
    };
    
    // Act
    var result = await dao.InsertAsync(newLoad);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Data > 0);
}
```

---

## Related Documentation

- **Constitution**: Principle II (Database Layer Consistency - instance-based DAOs)
- **Service Usage**: See `IService_MySQL_Receiving` for service that delegates to this DAO
- **Migration Guide**: `.github/instructions/architecture-refactoring-guide.instructions.md`

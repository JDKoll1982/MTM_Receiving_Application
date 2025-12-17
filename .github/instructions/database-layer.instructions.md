# Database Layer Instructions

This file defines patterns and standards for all database access in the MTM Receiving Application.

**Applies To**: `Data/**/*.cs`, `Database/**/*.sql`, `Helpers/Database/**/*.cs`

---

## Core Principles

1. **Model_Dao_Result Pattern**: ALL DAO methods MUST return `Model_Dao_Result<T>` or `Model_Dao_Result`
2. **Async-Only**: ALL database operations MUST be async (`Task<T>`)
3. **Stored Procedures Only**: NO direct SQL in C# code
4. **Helper_Database_StoredProcedure**: ONLY way to execute stored procedures
5. **No Exception Throwing**: Return `Model_Dao_Result.Failure()` instead

---

## DAO Method Template

```csharp
/// <summary>
/// Inserts a new receiving line entry.
/// </summary>
/// <param name="line">The receiving line data</param>
/// <returns>
/// Model_Dao_Result indicating success/failure.
/// Check IsSuccess before accessing Data.
/// </returns>
public static async Task<Model_Dao_Result<int>> InsertReceivingLineAsync(Model_ReceivingLine line)
{
    try
    {
        // 1. Build parameter dictionary (NO p_ prefix in C# - added automatically)
        var parameters = new Dictionary<string, object>
        {
            ["Quantity"] = line.Quantity,
            ["PartID"] = line.PartID,
            ["PONumber"] = line.PONumber,
            ["EmployeeNumber"] = line.EmployeeNumber,
            ["Heat"] = line.Heat,
            ["Date"] = line.Date,
            ["InitialLocation"] = line.InitialLocation,
            ["CoilsOnSkid"] = line.CoilsOnSkid ?? (object)DBNull.Value
        };

        // 2. Execute stored procedure via Helper
        var result = await Helper_Database_StoredProcedure.ExecuteNonQueryWithStatusAsync(
            Model_Application_Variables.ConnectionString,
            "receiving_line_Insert",  // Stored procedure name
            parameters
        );

        // 3. Check stored procedure status
        if (!result.IsSuccess)
            return Model_Dao_Result<int>.Failure(result.StatusMessage);

        // 4. Return success with rows affected
        return Model_Dao_Result<int>.Success(result.RowsAffected, "Receiving line inserted successfully", result.RowsAffected);
    }
    catch (MySqlException ex)
    {
        // 5. Return failure with exception (never throw)
        return Model_Dao_Result<int>.Failure($"Database error inserting receiving line: {ex.Message}", ex);
    }
    catch (Exception ex)
    {
        return Model_Dao_Result<int>.Failure($"Unexpected error: {ex.Message}", ex);
    }
}
```

---

## Stored Procedure Template

```sql
DELIMITER $$

DROP PROCEDURE IF EXISTS `receiving_line_Insert` $$

CREATE PROCEDURE `receiving_line_Insert`(
    IN p_Quantity INT,
    IN p_PartID VARCHAR(50),
    IN p_PONumber INT,
    IN p_EmployeeNumber INT,
    IN p_Heat VARCHAR(100),
    IN p_Date DATE,
    IN p_InitialLocation VARCHAR(50),
    IN p_CoilsOnSkid INT,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    -- Error handler
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = -1;
        ROLLBACK;
    END;

    -- Start transaction
    START TRANSACTION;

    -- Validation
    IF p_PartID IS NULL OR p_PartID = '' THEN
        SET p_Status = -1;
        SET p_ErrorMsg = 'Part ID is required';
        ROLLBACK;
        LEAVE;
    END IF;

    -- Insert operation
    INSERT INTO label_table_receiving (
        quantity, part_id, po_number, employee_number, heat,
        transaction_date, initial_location, coils_on_skid
    ) VALUES (
        p_Quantity, p_PartID, p_PONumber, p_EmployeeNumber, p_Heat,
        p_Date, p_InitialLocation, p_CoilsOnSkid
    );

    -- Success
    SET p_Status = 1;
    SET p_ErrorMsg = 'Receiving line inserted successfully';

    COMMIT;
END $$

DELIMITER ;
```

---

## MySQL 5.7.24 Constraints

**FORBIDDEN** (MySQL 8.0+ features not available):
- ❌ JSON functions (`JSON_EXTRACT`, `JSON_TABLE`)
- ❌ Common Table Expressions (CTEs / `WITH` clause)
- ❌ Window functions (`ROW_NUMBER()`, `RANK()`)
- ❌ `CHECK` constraints
- ❌ Recursive queries

**USE INSTEAD**:
- Temporary tables for complex queries
- Stored procedure variables
- Subqueries
- Application-level validation

---

## Parameter Naming Convention

**C# (Dictionary Key)**: `"PartID"` (NO prefix)  
**SQL (Stored Procedure)**: `IN p_PartID` (WITH p_ prefix)  
**Automatic**: Helper_Database_StoredProcedure adds `p_` prefix automatically

**Example**:
```csharp
// C# Code
var parameters = new Dictionary<string, object>
{
    ["Quantity"] = 100,  // NO p_ prefix
    ["PartID"] = "MMC0000848"  // NO p_ prefix
};

// SQL Stored Procedure
CREATE PROCEDURE `example_SP`(
    IN p_Quantity INT,  -- WITH p_ prefix
    IN p_PartID VARCHAR(50),  -- WITH p_ prefix
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
```

---

## Error Handling Pattern

### DAO Layer (Returns Model_Dao_Result)
```csharp
catch (MySqlException ex)
{
    return Model_Dao_Result.Failure($"Database error: {ex.Message}", ex);
}
```

### ViewModel Layer (Handles Model_Dao_Result)
```csharp
var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);

if (result.IsSuccess)
{
    // Success path
    StatusMessage = "Line added successfully";
}
else
{
    // Error path - use Service_ErrorHandler
    _errorHandler.ShowUserError(result.ErrorMessage, "Database Error", nameof(AddLineAsync));
    _logger.LogApplicationError(result.Exception);
}
```

---

## Testing Pattern

```csharp
[Fact]
public async Task InsertReceivingLineAsync_ValidData_ReturnsSuccess()
{
    // Arrange
    var line = new Model_ReceivingLine
    {
        Quantity = 100,
        PartID = "MMC0000848",
        PONumber = 66754,
        EmployeeNumber = 6229,
        Heat = "TEST-HEAT",
        Date = DateTime.Now,
        InitialLocation = "EXPO"
    };

    // Act
    var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.RowsAffected > 0);
}
```

---

## Common Mistakes to Avoid

❌ **Throwing exceptions from DAO methods**
```csharp
// WRONG
public static async Task<int> InsertAsync(Model_ReceivingLine line)
{
    // ...
    throw new Exception("Database error"); // NEVER THROW!
}
```

✅ **Return Model_Dao_Result instead**
```csharp
// CORRECT
public static async Task<Model_Dao_Result<int>> InsertAsync(Model_ReceivingLine line)
{
    // ...
    return Model_Dao_Result<int>.Failure("Database error");
}
```

❌ **Direct MySqlConnection usage**
```csharp
// WRONG
using (var conn = new MySqlConnection(connectionString))
{
    // Never create connections directly!
}
```

✅ **Use Helper_Database_StoredProcedure**
```csharp
// CORRECT
await Helper_Database_StoredProcedure.ExecuteNonQueryWithStatusAsync(...);
```

---

## Quick Reference

| Operation | Method | Returns |
|-----------|--------|---------|
| SELECT (single table) | `ExecuteDataTableWithStatusAsync` | `Model_Dao_Result<DataTable>` |
| SELECT (multiple tables) | `ExecuteDataSetWithStatusAsync` | `Model_Dao_Result<DataSet>` |
| INSERT/UPDATE/DELETE | `ExecuteNonQueryWithStatusAsync` | `Model_Dao_Result` |
| SELECT (scalar) | `ExecuteScalarWithStatusAsync` | `Model_Dao_Result<object>` |

**Always**:
- Check `result.IsSuccess` before accessing `result.Data`
- Log errors with `_logger.LogApplicationError(result.Exception)`
- Use `Service_ErrorHandler` to display user-friendly errors

---

**Last Updated**: December 15, 2025  
**Pattern Source**: MTM WIP Application Constitution

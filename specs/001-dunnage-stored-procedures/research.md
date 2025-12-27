# Research: Dunnage Stored Procedures

**Feature**: 001-dunnage-stored-procedures  
**Date**: 2025-12-26  
**Purpose**: Document technology decisions, patterns, and best practices for implementing dunnage stored procedures

## Technology Decisions

### MySQL 5.7.24 Compatibility

**Decision**: Use MySQL 5.7.24 native features only (no CTEs, window functions, limited JSON)

**Rationale**:
- Production environment locked to MySQL 5.7.24
- JSON data type IS supported (added in MySQL 5.7.8)
- JSON functions available: JSON_EXTRACT, JSON_VALID, JSON_OBJECT, JSON_ARRAY
- Common Table Expressions (CTEs) NOT available (added in MySQL 8.0)
- Window functions NOT available (added in MySQL 8.0)
- CHECK constraints NOT enforced until MySQL 8.0.16 (parsed but ignored in 5.7)

**Alternatives Considered**:
- **Upgrade to MySQL 8.0**: Rejected - requires infrastructure changes and testing
- **Avoid JSON entirely**: Rejected - dynamic spec schema requires flexible storage
- **Use TEXT with manual validation**: Rejected - JSON type provides native validation

**Implementation Impact**:
- Use JSON data type for `spec_values` column in dunnage_parts table
- Use JSON data type for `spec_value` column in dunnage_specs table
- Validate JSON in stored procedures using JSON_VALID() function
- Replace CHECK constraints with conditional logic in stored procedures
- Avoid CTEs - use subqueries or temporary tables if needed

---

### Stored Procedure Naming Convention

**Decision**: Use `sp_{table_name}_{operation}` pattern consistently

**Rationale**:
- Aligns with existing codebase conventions
- Clear, predictable naming enables easier DAO generation
- Operation suffixes: get_all, get_by_id, get_by_{field}, insert, update, delete, count_{relation}

**Examples**:
- `sp_dunnage_types_get_all()`
- `sp_dunnage_types_get_by_id(p_id)`
- `sp_dunnage_types_count_parts(p_type_id)`
- `sp_dunnage_parts_search(p_search_text, p_type_id)`

**Alternatives Considered**:
- **usp_ prefix**: Rejected - existing codebase uses sp_ prefix
- **No prefix**: Rejected - namespace collisions with built-in functions
- **Table prefix first**: Rejected - less readable, harder to group by operation

---

### Parameter Naming Convention

**Decision**: Use `p_{parameter_name}` prefix for all stored procedure parameters

**Rationale**:
- Distinguishes parameters from column names in SQL
- Prevents ambiguity in WHERE clauses
- Aligns with existing stored procedure patterns in codebase

**Implementation Notes**:
- C# DAO code passes parameters WITHOUT `p_` prefix
- `Helper_Database_StoredProcedure` automatically adds `p_` prefix
- Parameter names in spec should NOT include `p_` prefix (documentation clarity)

**Examples**:
```sql
CREATE PROCEDURE sp_dunnage_parts_get_by_type(
    IN p_type_id INT
)
-- p_ prefix distinguishes from column `type_id`
SELECT * FROM dunnage_parts WHERE type_id = p_type_id;
```

---

### Error Handling Strategy

**Decision**: Return error codes and messages via OUT parameters, not SIGNAL

**Rationale**:
- DAO layer expects `Model_Dao_Result` pattern (IsSuccess, ErrorMessage)
- SIGNAL/RESIGNAL exceptions complicate C# error handling
- OUT parameters enable structured error responses

**Pattern**:
```sql
CREATE PROCEDURE sp_example(
    IN p_id INT,
    OUT p_error_code INT,
    OUT p_error_message VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET p_error_code = 500;
        SET p_error_message = 'Database error occurred';
    END;
    
    SET p_error_code = 0;  -- Success
    -- Procedure logic here
    
    IF some_validation_fails THEN
        SET p_error_code = 400;
        SET p_error_message = 'Validation failed: {details}';
    END IF;
END;
```

**Alternatives Considered**:
- **SIGNAL exceptions**: Rejected - harder to parse in C#, less structured
- **Return result sets**: Rejected - ambiguous with data results
- **No error handling**: Rejected - violates constitution error handling principles

---

### Batch Insert Strategy

**Decision**: Accept JSON array parameter for batch inserts, iterate with cursor

**Rationale**:
- MySQL 5.7.24 does not support JSON_TABLE (added in MySQL 8.0.4)
- Cursor-based iteration is the only viable approach
- Maintains transactional integrity (all-or-nothing)

**Pattern**:
```sql
CREATE PROCEDURE sp_dunnage_loads_insert_batch(
    IN p_load_data JSON,
    IN p_user VARCHAR(50),
    OUT p_error_code INT,
    OUT p_error_message VARCHAR(500)
)
BEGIN
    DECLARE v_index INT DEFAULT 0;
    DECLARE v_count INT;
    DECLARE v_part_id VARCHAR(50);
    DECLARE v_quantity DECIMAL(10,2);
    
    START TRANSACTION;
    
    SET v_count = JSON_LENGTH(p_load_data);
    WHILE v_index < v_count DO
        SET v_part_id = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', v_index, '].part_id')));
        SET v_quantity = JSON_EXTRACT(p_load_data, CONCAT('$[', v_index, '].quantity'));
        
        INSERT INTO dunnage_loads (...) VALUES (...);
        
        SET v_index = v_index + 1;
    END WHILE;
    
    COMMIT;
END;
```

**Alternatives Considered**:
- **Multiple single inserts from C#**: Rejected - no transactional atomicity
- **Temporary tables**: Considered - more complex, similar performance
- **Wait for MySQL 8.0**: Rejected - not viable timeline

---

### Foreign Key Constraint Enforcement

**Decision**: Use RESTRICT behavior for all foreign keys, provide count_* procedures for impact analysis

**Rationale**:
- RESTRICT prevents accidental deletion of referenced data
- Count procedures enable UI to show impact before deletion
- Aligns with data integrity principles in constitution

**Implementation**:
- `sp_dunnage_types_count_parts(p_type_id)` - Returns count of parts using this type
- `sp_dunnage_types_count_transactions(p_type_id)` - Returns count of loads using parts of this type
- `sp_dunnage_parts_count_transactions(p_part_id)` - Returns count of loads using this part
- `sp_dunnage_specs_count_parts_using_spec(p_type_id, p_spec_key)` - Impact of removing a spec

**UI Integration**:
- Before delete operation, call count_* procedure
- Display confirmation dialog with impact count
- Proceed only on user confirmation
- Handle foreign key violation gracefully with user-friendly message

---

### Search Functionality Pattern

**Decision**: Use LIKE matching on PartID and JSON_EXTRACT on spec values

**Rationale**:
- No full-text indexing available in MySQL 5.7.24 for JSON columns
- LIKE matching sufficient for PartID searches
- JSON_EXTRACT enables searching within spec values
- Performance acceptable for expected data volumes (<10K parts per type)

**Pattern**:
```sql
CREATE PROCEDURE sp_dunnage_parts_search(
    IN p_search_text VARCHAR(100),
    IN p_type_id INT,  -- NULL for all types
    OUT p_error_code INT,
    OUT p_error_message VARCHAR(500)
)
BEGIN
    SELECT * FROM dunnage_parts
    WHERE (p_type_id IS NULL OR type_id = p_type_id)
      AND (part_id LIKE CONCAT('%', p_search_text, '%')
           OR JSON_SEARCH(spec_values, 'one', CONCAT('%', p_search_text, '%')) IS NOT NULL);
END;
```

**Performance Considerations**:
- Create index on `part_id` column for LIKE prefix matching
- JSON_SEARCH may be slow on large datasets - acceptable for current scale
- Future optimization: Add computed columns for frequently searched specs

---

### Audit Trail Strategy

**Decision**: Use CreatedBy/CreatedDate and ModifiedBy/ModifiedDate columns, updated by stored procedures

**Rationale**:
- Simple, proven approach used throughout codebase
- No separate audit table complexity
- Sufficient for compliance and troubleshooting needs

**Implementation**:
- All INSERT procedures set CreatedBy = p_user, CreatedDate = NOW()
- All UPDATE procedures set ModifiedBy = p_user, ModifiedDate = NOW()
- Created fields never modified after insertion
- No soft deletes - hard delete policy (documented in spec out-of-scope)

**Alternatives Considered**:
- **Separate audit trail table**: Rejected - out of scope, adds complexity
- **Trigger-based auditing**: Rejected - harder to test and debug
- **No audit trail**: Rejected - violates security principles

---

## Best Practices & Patterns

### DAO Implementation Pattern

Based on existing codebase analysis (`Dao_User.cs`, `Dao_ReceivingLine.cs`):

```csharp
public static class Dao_DunnageType
{
    public static async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllAsync()
    {
        try
        {
            var result = await Helper_Database_StoredProcedure
                .ExecuteStoredProcedureAsync<Model_DunnageType>(
                    "sp_dunnage_types_get_all",
                    new Dictionary<string, object>());
            
            return result.IsSuccess
                ? Model_Dao_Result<List<Model_DunnageType>>.Success(result.Data)
                : Model_Dao_Result<List<Model_DunnageType>>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<List<Model_DunnageType>>.Failure(
                $"Error retrieving dunnage types: {ex.Message}");
        }
    }
    
    public static async Task<Model_Dao_Result<Model_DunnageType>> GetByIdAsync(int id)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "id", id }  // No p_ prefix - added automatically
            };
            
            var result = await Helper_Database_StoredProcedure
                .ExecuteStoredProcedureAsync<Model_DunnageType>(
                    "sp_dunnage_types_get_by_id",
                    parameters);
            
            return result.IsSuccess && result.Data.Any()
                ? Model_Dao_Result<Model_DunnageType>.Success(result.Data.First())
                : Model_Dao_Result<Model_DunnageType>.Failure("Dunnage type not found");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<Model_DunnageType>.Failure(
                $"Error retrieving dunnage type: {ex.Message}");
        }
    }
}
```

**Key Points**:
- Static class (no instantiation)
- All methods async (Task<Model_Dao_Result<T>>)
- Never throw exceptions - return Failure results
- Use Helper_Database_StoredProcedure for all database operations
- Parameters passed without `p_` prefix
- Check result.IsSuccess before accessing result.Data

---

### Model Class Pattern

Based on existing models (`Model_User.cs`, `Model_ReceivingLine.cs`):

```csharp
public class Model_DunnageType : INotifyPropertyChanged
{
    private int _id;
    private string _typeName = string.Empty;
    private string _createdBy = string.Empty;
    private DateTime _createdDate;
    private string _modifiedBy = string.Empty;
    private DateTime? _modifiedDate;
    
    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }
    
    public string TypeName
    {
        get => _typeName;
        set { _typeName = value; OnPropertyChanged(); }
    }
    
    // ... other properties
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

**Key Points**:
- Implement INotifyPropertyChanged
- Private backing fields with public properties
- OnPropertyChanged called in setters
- Property names match database column names (PascalCase in C#, snake_case in DB)
- Nullable types for optional database columns

---

### Integration Test Pattern

```csharp
[Fact]
public async Task GetAllAsync_ReturnsAllDunnageTypes()
{
    // Arrange - use test database with known data
    
    // Act
    var result = await Dao_DunnageType.GetAllAsync();
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.NotEmpty(result.Data);
}

[Fact]
public async Task InsertAsync_WithValidData_CreatesNewType()
{
    // Arrange
    var typeName = "Test Type " + Guid.NewGuid();
    var user = "TestUser";
    
    // Act
    var result = await Dao_DunnageType.InsertAsync(typeName, user);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Data > 0); // New ID returned
    
    // Cleanup
    await Dao_DunnageType.DeleteAsync(result.Data);
}

[Fact]
public async Task DeleteAsync_WithDependentParts_ReturnsFalse()
{
    // Arrange - create type with associated parts
    var typeId = await CreateTypeWithParts();
    
    // Act
    var result = await Dao_DunnageType.DeleteAsync(typeId);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("foreign key", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    
    // Cleanup
    await CleanupTestData(typeId);
}
```

---

## Open Questions

**None identified.** All technical decisions resolved based on:
- MySQL 5.7.24 feature set documentation
- Existing codebase patterns and conventions
- Constitutional requirements
- Performance requirements from specification

## References

- [MySQL 5.7 Reference Manual - JSON Functions](https://dev.mysql.com/doc/refman/5.7/en/json-functions.html)
- [MySQL 5.7 Reference Manual - Stored Procedures](https://dev.mysql.com/doc/refman/5.7/en/stored-programs.html)
- Existing Codebase:
  - `Data/Authentication/Dao_User.cs`
  - `Data/Receiving/Dao_ReceivingLine.cs`
  - `Helpers/Database/Helper_Database_StoredProcedure.cs`
  - `Models/Systems/Model_Dao_Result.cs`
- Constitution: `.specify/memory/constitution.md` v1.1.0

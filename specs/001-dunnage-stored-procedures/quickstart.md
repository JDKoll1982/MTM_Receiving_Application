# Developer Quickstart: Dunnage Stored Procedures

**Feature**: 001-dunnage-stored-procedures  
**Purpose**: Get developers up to speed quickly on implementing dunnage stored procedures  
**Estimated Setup Time**: 30 minutes

---

## Overview

This feature implements 33 stored procedures across 5 database tables to support dunnage receiving functionality. The procedures follow the DAO pattern with `Model_Dao_Result` wrappers, ensuring consistent error handling and async operations.

**What you'll build**:
- 33 MySQL stored procedures (Database/StoredProcedures/Dunnage/)
- 5 DAO static classes (Data/Dunnage/)
- 5 Model classes (Models/Dunnage/)
- Integration tests (MTM_Receiving_Application.Tests/Integration/Dunnage/)

---

## Prerequisites

### Required Knowledge
- MySQL 5.7.24 stored procedure syntax
- C# async/await patterns
- JSON data type and MySQL JSON functions
- xUnit testing framework

### Required Tools
- MySQL Workbench or similar (for testing stored procedures directly)
- Visual Studio 2022 with .NET 8.0 SDK
- Access to `mtm_receiving_application` MySQL database

### Required Reading
1. [Constitution](.specify/memory/constitution.md) - Database Layer Consistency section
2. [research.md](research.md) - Technology decisions and patterns
3. [data-model.md](data-model.md) - Entity relationships and schema

---

## 5-Minute Quick Start

### 1. Create Database Schema (if not exists)

```sql
-- Run in MySQL Workbench against mtm_receiving_application database
SOURCE Database/Schemas/04_create_dunnage_tables.sql;
```

### 2. Create Your First Stored Procedure

**File**: `Database/StoredProcedures/Dunnage/sp_dunnage_types_get_all.sql`

```sql
DELIMITER $$

CREATE PROCEDURE sp_dunnage_types_get_all()
BEGIN
    SELECT 
        id,
        type_name,
        created_by,
        created_date,
        modified_by,
        modified_date
    FROM dunnage_types
    ORDER BY type_name;
END$$

DELIMITER ;
```

**Deploy**:
```bash
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Database/StoredProcedures/Dunnage/sp_dunnage_types_get_all.sql
```

### 3. Test Stored Procedure Directly

```sql
-- Insert test data
INSERT INTO dunnage_types (type_name, created_by, created_date) 
VALUES ('Pallet', 'TestUser', NOW());

-- Test procedure
CALL sp_dunnage_types_get_all();
-- Should return 1 row with Pallet type
```

### 4. Create Model Class

**File**: `Models/Dunnage/Model_DunnageType.cs`

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTM_Receiving_Application.Models.Dunnage;

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

    public string CreatedBy
    {
        get => _createdBy;
        set { _createdBy = value; OnPropertyChanged(); }
    }

    public DateTime CreatedDate
    {
        get => _createdDate;
        set { _createdDate = value; OnPropertyChanged(); }
    }

    public string ModifiedBy
    {
        get => _modifiedBy;
        set { _modifiedBy = value; OnPropertyChanged(); }
    }

    public DateTime? ModifiedDate
    {
        get => _modifiedDate;
        set { _modifiedDate = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### 5. Create DAO Wrapper

**File**: `Data/Dunnage/Dao_DunnageType.cs`

```csharp
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.Data.Dunnage;

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
}
```

### 6. Test DAO Integration

**File**: `MTM_Receiving_Application.Tests/Integration/Dunnage/Dao_DunnageType_Tests.cs`

```csharp
using MTM_Receiving_Application.Data.Dunnage;
using Xunit;

namespace MTM_Receiving_Application.Tests.Integration.Dunnage;

public class Dao_DunnageType_Tests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllDunnageTypes()
    {
        // Act
        var result = await Dao_DunnageType.GetAllAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        // Should have at least the test Pallet type
        Assert.NotEmpty(result.Data);
    }
}
```

**Run Test**:
```bash
dotnet test --filter "FullyQualifiedName~Dao_DunnageType_Tests.GetAllAsync_ReturnsAllDunnageTypes"
```

---

## Development Workflow

### Phase 1: Stored Procedures (Database-First)

For each table (types, specs, parts, loads, inventoried_dunnage):

1. **Create procedures** in `Database/StoredProcedures/Dunnage/`
   - Follow naming: `sp_{table}_{operation}.sql`
   - Use `p_` prefix for parameters
   - Include error handling
   - Test directly in MySQL Workbench

2. **Deploy procedures**:
   ```bash
   mysql -h localhost -P 3306 -u root -p mtm_receiving_application < sp_filename.sql
   ```

3. **Verify deployment**:
   ```sql
   SHOW PROCEDURE STATUS WHERE Db = 'mtm_receiving_application' AND Name LIKE 'sp_dunnage_%';
   ```

### Phase 2: Models (Data Classes)

1. **Create model class** in `Models/Dunnage/`
   - Implement `INotifyPropertyChanged`
   - Private backing fields with public properties
   - Property names match DB columns (PascalCase)
   - Use nullable types where appropriate

2. **Verify compilation**:
   ```bash
   dotnet build
   ```

### Phase 3: DAOs (Data Access Objects)

1. **Create static DAO class** in `Data/Dunnage/`
   - All methods async returning `Task<Model_Dao_Result<T>>`
   - Use `Helper_Database_StoredProcedure`
   - Never throw exceptions - return `Failure()` results
   - Parameters without `p_` prefix (added automatically)

2. **Build and fix errors**:
   ```bash
   dotnet build
   ```

### Phase 4: Integration Tests

1. **Create test class** in `MTM_Receiving_Application.Tests/Integration/Dunnage/`
   - One test class per DAO
   - Test success paths and error conditions
   - Clean up test data in finally blocks

2. **Run tests**:
   ```bash
   dotnet test --filter "FullyQualifiedName~Dunnage"
   ```

---

## Common Patterns

### Stored Procedure with OUT Parameter

```sql
CREATE PROCEDURE sp_dunnage_types_insert(
    IN p_type_name VARCHAR(100),
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
    INSERT INTO dunnage_types (type_name, created_by, created_date)
    VALUES (p_type_name, p_user, NOW());
    
    SET p_new_id = LAST_INSERT_ID();
END$$
```

**DAO Wrapper**:
```csharp
public static async Task<Model_Dao_Result<int>> InsertAsync(string typeName, string user)
{
    try
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_name", typeName },
            { "user", user }
        };

        var result = await Helper_Database_StoredProcedure
            .ExecuteStoredProcedureWithOutParameterAsync<int>(
                "sp_dunnage_types_insert",
                parameters,
                "new_id");  // OUT parameter name

        return result.IsSuccess
            ? Model_Dao_Result<int>.Success(result.Data)
            : Model_Dao_Result<int>.Failure(result.ErrorMessage);
    }
    catch (Exception ex)
    {
        return Model_Dao_Result<int>.Failure($"Error inserting type: {ex.Message}");
    }
}
```

### JSON Parameter Handling

```sql
-- Stored procedure accepts JSON
CREATE PROCEDURE sp_dunnage_parts_insert(
    IN p_part_id VARCHAR(50),
    IN p_type_id INT,
    IN p_spec_values JSON,  -- JSON parameter
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
    -- Validate JSON
    IF NOT JSON_VALID(p_spec_values) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid JSON in spec_values';
    END IF;
    
    INSERT INTO dunnage_parts (part_id, type_id, spec_values, created_by, created_date)
    VALUES (p_part_id, p_type_id, p_spec_values, p_user, NOW());
    
    SET p_new_id = LAST_INSERT_ID();
END$$
```

**DAO Wrapper**:
```csharp
public static async Task<Model_Dao_Result<int>> InsertAsync(
    string partId, 
    int typeId, 
    string specValuesJson,  // Pass JSON as string
    string user)
{
    var parameters = new Dictionary<string, object>
    {
        { "part_id", partId },
        { "type_id", typeId },
        { "spec_values", specValuesJson },  // String will be interpreted as JSON by MySQL
        { "user", user }
    };

    var result = await Helper_Database_StoredProcedure
        .ExecuteStoredProcedureWithOutParameterAsync<int>(
            "sp_dunnage_parts_insert",
            parameters,
            "new_id");

    return result.IsSuccess
        ? Model_Dao_Result<int>.Success(result.Data)
        : Model_Dao_Result<int>.Failure(result.ErrorMessage);
}
```

---

## Troubleshooting

### Procedure Not Found Error

**Symptom**: "Procedure 'sp_dunnage_types_get_all' does not exist"

**Solution**:
```sql
-- Check if procedure exists
SHOW PROCEDURE STATUS WHERE Name = 'sp_dunnage_types_get_all';

-- If not, deploy it
SOURCE Database/StoredProcedures/Dunnage/sp_dunnage_types_get_all.sql;
```

### JSON Validation Errors

**Symptom**: "Invalid JSON in spec_values"

**Solution**: Validate JSON before passing to stored procedure:
```csharp
// In DAO method
if (string.IsNullOrWhiteSpace(specValuesJson))
{
    return Model_Dao_Result<int>.Failure("Spec values cannot be empty");
}

try
{
    System.Text.Json.JsonDocument.Parse(specValuesJson);
}
catch (System.Text.Json.JsonException)
{
    return Model_Dao_Result<int>.Failure("Invalid JSON format in spec values");
}
```

### Foreign Key Constraint Violations

**Symptom**: "Cannot add or update a child row: a foreign key constraint fails"

**Solution**: Use count procedures to check dependencies first:
```csharp
// Before deleting type
var countResult = await Dao_DunnageType.CountPartsAsync(typeId);
if (countResult.IsSuccess && countResult.Data > 0)
{
    return Model_Dao_Result.Failure(
        $"Cannot delete type: {countResult.Data} parts are using this type");
}
```

---

## Next Steps

1. Review [contracts/](contracts/) directory for all 33 procedure signatures
2. Implement procedures in order: types → specs → parts → loads → inventoried
3. Follow test-driven approach: write test, implement procedure, verify DAO
4. Update [tasks.md](tasks.md) to track implementation progress (use `/speckit.tasks`)

---

## Resources

- **Constitution**: `.specify/memory/constitution.md` - Database Layer principles
- **Research**: `research.md` - Technology decisions and best practices
- **Data Model**: `data-model.md` - Entity relationships and schema
- **Contracts**: `contracts/` - Detailed procedure specifications
- **Existing Examples**:
  - `Data/Authentication/Dao_User.cs` - DAO pattern reference
  - `Database/StoredProcedures/sp_users_*.sql` - Procedure examples
  - `Models/Systems/Model_Dao_Result.cs` - Result type definition

---

## Checklist

Before considering this feature complete:

- [ ] All 33 stored procedures deployed to database
- [ ] All 5 Model classes created with INotifyPropertyChanged
- [ ] All 5 DAO static classes created with async methods
- [ ] Integration tests pass for all DAOs
- [ ] Build succeeds with no warnings
- [ ] Code follows constitution principles (no direct SQL, async operations, error handling)
- [ ] Documentation updated (if applicable)
- [ ] Peer review completed

# Stored Procedure Contracts: Dunnage Specs

**Table**: `dunnage_specs`  
**Purpose**: Dynamic schema definitions per dunnage type  
**Procedures**: 7 total

---

## sp_dunnage_specs_get_by_type

**Purpose**: Retrieve all spec definitions for a specific type

**Parameters**:
- `p_type_id` (INT, IN): Type ID to get specs for

**Returns**: Result set with all dunnage_specs records for this type

**Result Columns**:
- `id` (INT)
- `type_id` (INT)
- `spec_key` (VARCHAR)
- `spec_value` (JSON)
- `created_by` (VARCHAR)
- `created_date` (DATETIME)
- `modified_by` (VARCHAR)
- `modified_date` (DATETIME)

**Error Conditions**: None (empty set if type has no specs)

**Example Usage**:
```sql
CALL sp_dunnage_specs_get_by_type(1);
```

---

## sp_dunnage_specs_get_all

**Purpose**: Retrieve all spec definitions across all types (for CSV column union)

**Parameters**: None

**Returns**: Result set with all dunnage_specs records

**Result Columns**: Same as sp_dunnage_specs_get_by_type

**Error Conditions**: None (empty set if no specs exist)

**Example Usage**:
```sql
CALL sp_dunnage_specs_get_all();
```

**Note**: Used to discover all possible spec keys across types for dynamic column generation.

---

## sp_dunnage_specs_get_by_id

**Purpose**: Retrieve a specific spec definition by ID

**Parameters**:
- `p_id` (INT, IN): Spec ID to retrieve

**Returns**: Result set with single dunnage_specs record or empty set

**Result Columns**: Same as sp_dunnage_specs_get_by_type

**Error Conditions**: None (empty set if spec ID does not exist)

**Example Usage**:
```sql
CALL sp_dunnage_specs_get_by_id(5);
```

---

## sp_dunnage_specs_insert

**Purpose**: Create a new spec definition for a type

**Parameters**:
- `p_type_id` (INT, IN): Type ID this spec belongs to
- `p_spec_key` (VARCHAR(100), IN): Specification field name (e.g., "length", "material")
- `p_spec_value` (JSON, IN): JSON schema definition for this spec
- `p_user` (VARCHAR(50), IN): Employee number creating the record
- `p_new_id` (INT, OUT): ID of the newly created spec

**Returns**: OUT parameter `p_new_id` with new spec ID

**Error Conditions**:
- Type ID does not exist (foreign key violation)
- Duplicate (type_id, spec_key) combination
- Invalid JSON in p_spec_value
- NULL parameters

**Example Usage**:
```sql
CALL sp_dunnage_specs_insert(
    1, 
    'length', 
    '{"data_type":"decimal","unit":"inches","required":true,"min_value":0,"max_value":999.99}',
    'EMP001',
    @new_id
);
SELECT @new_id;
```

---

## sp_dunnage_specs_update

**Purpose**: Modify an existing spec definition

**Parameters**:
- `p_id` (INT, IN): Spec ID to update
- `p_spec_value` (JSON, IN): New JSON schema definition
- `p_user` (VARCHAR(50), IN): Employee number modifying the record

**Returns**: Affected rows count (1 if successful, 0 if not found)

**Error Conditions**:
- Spec ID does not exist
- Invalid JSON in p_spec_value
- NULL parameters

**Example Usage**:
```sql
CALL sp_dunnage_specs_update(
    5,
    '{"data_type":"decimal","unit":"inches","required":false,"min_value":0,"max_value":1200.0}',
    'EMP001'
);
```

**Note**: Cannot change spec_key or type_id - must delete and recreate to change key.

---

## sp_dunnage_specs_delete_by_id

**Purpose**: Remove a specific spec definition

**Parameters**:
- `p_id` (INT, IN): Spec ID to delete

**Returns**: Affected rows count (1 if successful, 0 if not found)

**Error Conditions**:
- Spec ID does not exist

**Example Usage**:
```sql
CALL sp_dunnage_specs_delete_by_id(5);
```

**Note**: Use `sp_dunnage_specs_count_parts_using_spec` to check if parts use this spec before deletion.

---

## sp_dunnage_specs_delete_by_type

**Purpose**: Remove all spec definitions for a specific type (CASCADE behavior)

**Parameters**:
- `p_type_id` (INT, IN): Type ID to delete all specs for

**Returns**: Affected rows count (number of specs deleted)

**Error Conditions**: None (returns 0 if type has no specs)

**Example Usage**:
```sql
CALL sp_dunnage_specs_delete_by_type(1);
```

**Use Case**: Called when deleting a type, or when resetting a type's schema.

---

## sp_dunnage_specs_count_parts_using_spec

**Purpose**: Count parts that have values for a specific spec key (impact analysis)

**Parameters**:
- `p_type_id` (INT, IN): Type ID to check
- `p_spec_key` (VARCHAR(100), IN): Spec key to search for in parts' spec_values JSON

**Returns**: Result set with single row containing count

**Result Columns**:
- `parts_with_spec_count` (INT): Number of parts with this spec key in their spec_values JSON

**Error Conditions**: None (returns 0 if no parts use this spec)

**Example Usage**:
```sql
CALL sp_dunnage_specs_count_parts_using_spec(1, 'length');
```

**Implementation**: Uses JSON_SEARCH or JSON_EXTRACT to check if spec_key exists in dunnage_parts.spec_values.

---

## DAO Mapping

```csharp
// Static class: Dao_DunnageSpec
public static async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetByTypeAsync(int typeId)
public static async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetAllAsync()
public static async Task<Model_Dao_Result<Model_DunnageSpec>> GetByIdAsync(int id)
public static async Task<Model_Dao_Result<int>> InsertAsync(int typeId, string specKey, string specValueJson, string user)
public static async Task<Model_Dao_Result> UpdateAsync(int id, string specValueJson, string user)
public static async Task<Model_Dao_Result> DeleteByIdAsync(int id)
public static async Task<Model_Dao_Result> DeleteByTypeAsync(int typeId)
public static async Task<Model_Dao_Result<int>> CountPartsUsingSpecAsync(int typeId, string specKey)
```

# Stored Procedure Contracts: Dunnage Types

**Table**: `dunnage_types`  
**Purpose**: Type categorization for dunnage items  
**Procedures**: 7 total

---

## sp_dunnage_types_get_all

**Purpose**: Retrieve all dunnage types

**Parameters**: None

**Returns**: Result set with all dunnage_types records

**Result Columns**:
- `id` (INT)
- `type_name` (VARCHAR)
- `created_by` (VARCHAR)
- `created_date` (DATETIME)
- `modified_by` (VARCHAR)
- `modified_date` (DATETIME)

**Error Conditions**: None (empty set if no types exist)

**Example Usage**:
```sql
CALL sp_dunnage_types_get_all();
```

---

## sp_dunnage_types_get_by_id

**Purpose**: Retrieve a specific dunnage type by ID

**Parameters**:
- `p_id` (INT, IN): Type ID to retrieve

**Returns**: Result set with single dunnage_types record or empty set

**Result Columns**: Same as sp_dunnage_types_get_all

**Error Conditions**:
- Empty result set if type ID does not exist

**Example Usage**:
```sql
CALL sp_dunnage_types_get_by_id(1);
```

---

## sp_dunnage_types_insert

**Purpose**: Create a new dunnage type

**Parameters**:
- `p_type_name` (VARCHAR(100), IN): Name of the new type
- `p_user` (VARCHAR(50), IN): Employee number creating the record
- `p_new_id` (INT, OUT): ID of the newly created type

**Returns**: OUT parameter `p_new_id` with new type ID

**Error Conditions**:
- Duplicate type_name (unique constraint violation)
- NULL or empty type_name
- NULL user

**Example Usage**:
```sql
CALL sp_dunnage_types_insert('Pallet', 'EMP001', @new_id);
SELECT @new_id;
```

---

## sp_dunnage_types_update

**Purpose**: Modify an existing dunnage type

**Parameters**:
- `p_id` (INT, IN): Type ID to update
- `p_type_name` (VARCHAR(100), IN): New type name
- `p_user` (VARCHAR(50), IN): Employee number modifying the record

**Returns**: Affected rows count (1 if successful, 0 if not found)

**Error Conditions**:
- Type ID does not exist
- Duplicate type_name (if changing to existing name)
- NULL type_name or user

**Example Usage**:
```sql
CALL sp_dunnage_types_update(1, 'Updated Pallet Name', 'EMP001');
```

---

## sp_dunnage_types_delete

**Purpose**: Remove a dunnage type (only if no dependent records)

**Parameters**:
- `p_id` (INT, IN): Type ID to delete

**Returns**: Affected rows count (1 if successful, 0 if not found or restricted)

**Error Conditions**:
- Type ID does not exist
- Foreign key constraint violation (type has associated parts or specs)

**Example Usage**:
```sql
CALL sp_dunnage_types_delete(1);
```

**Note**: Use `sp_dunnage_types_count_parts` and `sp_dunnage_types_count_transactions` to check dependencies before deletion.

---

## sp_dunnage_types_count_parts

**Purpose**: Count number of parts associated with a type (impact analysis)

**Parameters**:
- `p_type_id` (INT, IN): Type ID to check

**Returns**: Result set with single row containing count

**Result Columns**:
- `part_count` (INT): Number of parts using this type

**Error Conditions**: None (returns 0 if type has no parts or doesn't exist)

**Example Usage**:
```sql
CALL sp_dunnage_types_count_parts(1);
```

---

## sp_dunnage_types_count_transactions

**Purpose**: Count number of load transactions involving parts of this type (deep impact analysis)

**Parameters**:
- `p_type_id` (INT, IN): Type ID to check

**Returns**: Result set with single row containing count

**Result Columns**:
- `transaction_count` (INT): Number of dunnage_loads records for parts of this type

**Error Conditions**: None (returns 0 if no transactions exist)

**Example Usage**:
```sql
CALL sp_dunnage_types_count_transactions(1);
```

**Note**: This performs a JOIN between dunnage_parts and dunnage_loads to count all historical receiving transactions for this type.

---

## DAO Mapping

```csharp
// Static class: Dao_DunnageType
public static async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllAsync()
public static async Task<Model_Dao_Result<Model_DunnageType>> GetByIdAsync(int id)
public static async Task<Model_Dao_Result<int>> InsertAsync(string typeName, string user)
public static async Task<Model_Dao_Result> UpdateAsync(int id, string typeName, string user)
public static async Task<Model_Dao_Result> DeleteAsync(int id)
public static async Task<Model_Dao_Result<int>> CountPartsAsync(int typeId)
public static async Task<Model_Dao_Result<int>> CountTransactionsAsync(int typeId)
```

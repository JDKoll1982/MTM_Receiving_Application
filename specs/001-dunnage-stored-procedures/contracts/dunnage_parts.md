# Stored Procedure Contracts: Dunnage Parts

**Table**: `dunnage_parts`  
**Purpose**: Master data for individual dunnage parts  
**Procedures**: 8 total

---

## sp_dunnage_parts_get_all

**Purpose**: Retrieve all dunnage parts with type information

**Parameters**: None

**Returns**: Result set with all dunnage_parts records joined with type info

**Result Columns**:
- `id` (INT) - Part PK
- `part_id` (VARCHAR) - Business key
- `type_id` (INT)
- `type_name` (VARCHAR) - From dunnage_types JOIN
- `spec_values` (JSON)
- `created_by` (VARCHAR)
- `created_date` (DATETIME)
- `modified_by` (VARCHAR)
- `modified_date` (DATETIME)

---

## sp_dunnage_parts_get_by_type

**Purpose**: Retrieve all parts of a specific type

**Parameters**:
- `p_type_id` (INT, IN): Type ID to filter by

**Returns**: Result set with dunnage_parts for this type (with type info)

**Result Columns**: Same as sp_dunnage_parts_get_all

---

## sp_dunnage_parts_get_by_id

**Purpose**: Retrieve specific part by business key (part_id)

**Parameters**:
- `p_part_id` (VARCHAR(50), IN): Business key to search for

**Returns**: Result set with single part record or empty set

**Result Columns**: Same as sp_dunnage_parts_get_all

---

## sp_dunnage_parts_insert

**Purpose**: Create a new dunnage part

**Parameters**:
- `p_part_id` (VARCHAR(50), IN): Business key (unique)
- `p_type_id` (INT, IN): Type ID this part belongs to
- `p_spec_values` (JSON, IN): Part attribute values conforming to type's spec schema
- `p_user` (VARCHAR(50), IN): Employee number
- `p_new_id` (INT, OUT): PK of newly created part

**Error Conditions**:
- Duplicate part_id (unique constraint)
- Type ID does not exist (FK violation)
- Invalid JSON in p_spec_values
- Spec values don't match type's schema (validation in procedure)

---

## sp_dunnage_parts_update

**Purpose**: Modify part's spec values

**Parameters**:
- `p_id` (INT, IN): Part PK to update
- `p_spec_values` (JSON, IN): New spec values JSON
- `p_user` (VARCHAR(50), IN): Employee number

**Returns**: Affected rows count

**Note**: Cannot change part_id or type_id after creation.

---

## sp_dunnage_parts_delete

**Purpose**: Remove a part (only if no transaction history)

**Parameters**:
- `p_id` (INT, IN): Part PK to delete

**Error Conditions**:
- Part has associated dunnage_loads records (FK RESTRICT)

**Note**: Use `sp_dunnage_parts_count_transactions` before deletion.

---

## sp_dunnage_parts_count_transactions

**Purpose**: Count load transactions for this part

**Parameters**:
- `p_part_id` (VARCHAR(50), IN): Part business key

**Returns**: Single row with `transaction_count` (INT)

---

## sp_dunnage_parts_search

**Purpose**: Search parts by text (PartID or spec values)

**Parameters**:
- `p_search_text` (VARCHAR(100), IN): Search term
- `p_type_id` (INT, IN, NULLABLE): Optional type filter (NULL = all types)

**Returns**: Result set with matching parts

**Implementation**: Uses LIKE on part_id and JSON_SEARCH on spec_values

---

## DAO Mapping

```csharp
// Static class: Dao_DunnagePart
public static async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllAsync()
public static async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetByTypeAsync(int typeId)
public static async Task<Model_Dao_Result<Model_DunnagePart>> GetByIdAsync(string partId)
public static async Task<Model_Dao_Result<int>> InsertAsync(string partId, int typeId, string specValuesJson, string user)
public static async Task<Model_Dao_Result> UpdateAsync(int id, string specValuesJson, string user)
public static async Task<Model_Dao_Result> DeleteAsync(int id)
public static async Task<Model_Dao_Result<int>> CountTransactionsAsync(string partId)
public static async Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchAsync(string searchText, int? typeId)
```

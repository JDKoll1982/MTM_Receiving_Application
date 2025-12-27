# Stored Procedure Contracts: Inventoried Dunnage

**Table**: `inventoried_dunnage`  
**Purpose**: Parts requiring Visual ERP inventory notifications  
**Procedures**: 6 total

---

## sp_inventoried_dunnage_get_all

**Purpose**: Retrieve all parts requiring inventory notification

**Parameters**: None

**Returns**: Result set with all inventoried_dunnage records with part info

**Result Columns**:
- `id` (INT) - PK
- `part_id` (VARCHAR) - FK to parts
- `part_name` (VARCHAR) - From dunnage_parts.part_id
- `type_name` (VARCHAR) - From dunnage_types.type_name
- `inventory_method` (VARCHAR)
- `notes` (TEXT)
- `created_by` (VARCHAR)
- `created_date` (DATETIME)
- `modified_by` (VARCHAR)
- `modified_date` (DATETIME)

**Use Case**: Admin view for managing inventory notification list.

---

## sp_inventoried_dunnage_check

**Purpose**: Check if a specific part requires inventory notification

**Parameters**:
- `p_part_id` (VARCHAR(50), IN): Part ID to check

**Returns**: Single row with boolean-like result

**Result Columns**:
- `requires_inventory` (TINYINT): 1 if part is in inventoried_dunnage table, 0 otherwise

**Use Case**: Called after receiving to determine if inventory notification dialog should appear.

**Example Usage**:
```sql
CALL sp_inventoried_dunnage_check('PALLET-48X40-WOOD');
-- Returns: requires_inventory = 1 or 0
```

---

## sp_inventoried_dunnage_get_by_part

**Purpose**: Get inventory details for a specific part

**Parameters**:
- `p_part_id` (VARCHAR(50), IN): Part ID to get details for

**Returns**: Single record or empty set

**Result Columns**: Same as sp_inventoried_dunnage_get_all

**Use Case**: Display inventory method and notes in notification UI.

---

## sp_inventoried_dunnage_insert

**Purpose**: Add a part to the inventory notification list

**Parameters**:
- `p_part_id` (VARCHAR(50), IN): Part ID to add
- `p_inventory_method` (VARCHAR(100), IN): Notification method (e.g., "Manual Entry", "API Call")
- `p_notes` (TEXT, IN, NULLABLE): Additional instructions
- `p_user` (VARCHAR(50), IN): Employee number
- `p_new_id` (INT, OUT): PK of new record

**Error Conditions**:
- Part ID does not exist (FK violation)
- Part already in inventoried_dunnage table (unique constraint on part_id)
- NULL inventory_method

**Example Usage**:
```sql
CALL sp_inventoried_dunnage_insert(
    'PALLET-48X40-WOOD',
    'Manual Entry',
    'Enter into Visual ERP within 24 hours of receiving',
    'EMP001',
    @new_id
);
```

---

## sp_inventoried_dunnage_update

**Purpose**: Modify inventory settings for a part

**Parameters**:
- `p_id` (INT, IN): Record PK to update
- `p_inventory_method` (VARCHAR(100), IN): New method
- `p_notes` (TEXT, IN, NULLABLE): New notes
- `p_user` (VARCHAR(50), IN): Employee number

**Returns**: Affected rows count

**Note**: Cannot change part_id - must delete and recreate to reassign.

---

## sp_inventoried_dunnage_delete

**Purpose**: Remove a part from inventory notification list

**Parameters**:
- `p_part_id` (VARCHAR(50), IN): Part ID to remove

**Returns**: Affected rows count

**Use Case**: Part no longer requires inventory tracking in Visual ERP.

---

## DAO Mapping

```csharp
// Static class: Dao_InventoriedDunnage
public static async Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllAsync()
public static async Task<Model_Dao_Result<bool>> CheckAsync(string partId)
public static async Task<Model_Dao_Result<Model_InventoriedDunnage>> GetByPartAsync(string partId)
public static async Task<Model_Dao_Result<int>> InsertAsync(string partId, string inventoryMethod, string notes, string user)
public static async Task<Model_Dao_Result> UpdateAsync(int id, string inventoryMethod, string notes, string user)
public static async Task<Model_Dao_Result> DeleteAsync(string partId)
```

## Workflow Integration

**Receiving Workflow**:
1. User receives dunnage load → `sp_dunnage_loads_insert`
2. Application calls → `sp_inventoried_dunnage_check(part_id)`
3. If `requires_inventory = 1` → Show notification dialog
4. Dialog displays data from → `sp_inventoried_dunnage_get_by_part(part_id)`
5. User completes inventory notification per method/notes
6. UI confirms notification sent

**Admin Management**:
- Add part to list → `sp_inventoried_dunnage_insert`
- View all → `sp_inventoried_dunnage_get_all`
- Update method/notes → `sp_inventoried_dunnage_update`
- Remove from list → `sp_inventoried_dunnage_delete`

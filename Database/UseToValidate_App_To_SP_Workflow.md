# Database Validation Workflow

**Purpose**: Comprehensive validation of SQL stored procedures, DAOs, and database schema alignment to ensure data integrity and prevent runtime errors.

**Last Updated**: January 5, 2026

---

## Overview

This workflow validates the complete data pipeline:
```
Application Code (DAO) → Stored Procedure → Database Schema → Response → Application Model
```

## Validation Layers

### 1. **Static Schema Validation** (Audit Script)
### 2. **Parameter Alignment Validation** (Manual)
### 3. **Model Mapping Validation** (Manual)
### 4. **Runtime Testing** (Optional)

---

## Layer 1: Static Schema Validation

### Tool: `Scripts/Audit-StoredProcedures.ps1`

**What It Does:**
- Parses all database schema files to extract table definitions and column names
- Analyzes all stored procedures to identify SELECT/INSERT/UPDATE statements
- Cross-references column usage against schema definitions
- Generates `StoredProcedure_Audit.md` report with findings

**Limitations:**
- Cannot fully validate JOINs (flags columns from joined tables as errors)
- Cannot validate function calls like `COALESCE()`, `CAST()`, etc.
- Does not validate parameter types or counts

**How to Run:**
```powershell
.\Scripts\Audit-StoredProcedures.ps1
```

**Output:**
- `StoredProcedure_Audit.md` - Lists schema mismatches and validation results

**Key Features:**
- ✅ Detects missing columns in SELECT/INSERT/UPDATE statements
- ✅ Ignores SQL keywords like `INTO`, `AS`, `COUNT`, `SUM`, etc.
- ✅ Handles table aliases (`l.id` → `id`)
- ✅ Skips function calls (columns containing `(`)
- ✅ Uses smart comma-splitting (respects parentheses in `COALESCE(a,b)`)

**Interpreting Results:**
- **Critical Errors**: Columns that don't exist in the base table (excluding JOINs)
- **False Positives**: Columns from JOINed tables or complex expressions

---

## Layer 2: Parameter Alignment Validation

**Manual Process - Required for Each Module**

### Step 1: Identify All Stored Procedure Calls

**Search DAOs for SP calls:**
```powershell
# Find all stored procedure invocations
Get-ChildItem -Path "Module_[ModuleName]\Data" -Filter "*.cs" -Recurse |
    Select-String -Pattern "ExecuteStoredProcedureAsync|ExecuteNonQueryAsync" -Context 0,5
```

**Example Output:**
```csharp
var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
    _connectionString,
    "sp_volvo_shipment_line_insert",  // ← SP Name
    parameters);
```

### Step 2: Extract Parameter Lists from DAOs

**For each SP call, document:**
1. **SP Name**: `sp_volvo_shipment_line_insert`
2. **Parameters Passed**: `shipment_id`, `part_number`, `received_skid_count`, etc.
3. **Parameter Types**: `int`, `string`, `DateTime`, etc.

**Example from DAO:**
```csharp
new Dictionary<string, object>
{
    { "shipment_id", line.ShipmentId },              // int
    { "part_number", line.PartNumber },              // string
    { "received_skid_count", line.ReceivedSkidCount } // int
}
```

### Step 3: Compare Against Stored Procedure Signature

**Read SP file:**
```sql
CREATE PROCEDURE sp_volvo_shipment_line_insert(
  IN p_shipment_id INT,
  IN p_part_number VARCHAR(20),
  IN p_received_skid_count INT
)
```

**Validation Checklist:**
- [ ] All parameters from DAO exist in SP signature
- [ ] Parameter types match (int → INT, string → VARCHAR, etc.)
- [ ] **Boolean values converted to 0/1 for TINYINT parameters**
- [ ] No extra parameters in SP that aren't provided by DAO
- [ ] OUT parameters are handled correctly in DAO (if applicable)

**Common Issues Found:**
- ❌ **Missing Parameter**: DAO passes `quantity_per_skid`, SP doesn't define it (MySQL error 1318)
- ❌ **Extra Parameter**: SP defines `p_description`, but table doesn't have `description` column
- ❌ **Type Mismatch**: DAO passes `string`, SP expects `INT`
- ❌ **Boolean Not Converted**: DAO passes `true/false`, SP expects TINYINT (0/1)

---

## Layer 3: Model Mapping Validation

**Validates that returned data can be mapped to application Models**

### Step 1: Find MapFromReader Methods

**Search pattern:**
```powershell
Get-ChildItem -Path "Module_[ModuleName]\Data" -Filter "*.cs" -Recurse |
    Select-String -Pattern "MapFromReader"
```

### Step 2: Extract Column Reads from Mapper

**Example MapFromReader:**
```csharp
private static Model_VolvoPart MapFromReader(IDataReader reader)
{
    return new Model_VolvoPart
    {
        PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
        QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
        IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
    };
}
```

**Columns Required:**
- `part_number` (string)
- `quantity_per_skid` (int)
- `is_active` (bool)

### Step 3: Compare Against SP SELECT Statement

**Read SP SELECT clause:**
```sql
SELECT part_number, quantity_per_skid, is_active
FROM volvo_masterdata
WHERE part_number = p_part_number;
```

**Validation Checklist:**
- [ ] All columns read by `MapFromReader` are returned by SP
- [ ] Column names match exactly (case-sensitive in MySQL)
- [ ] Data types are compatible (VARCHAR → string, INT → int, TINYINT → bool)

**Common Issues Found:**
- ❌ **Missing Column in SELECT**: SP doesn't return `description`, but mapper tries to read it
- ❌ **Column Name Mismatch**: SP returns `PartNumber`, mapper reads `part_number`
- ❌ **Type Incompatibility**: SP returns `VARCHAR`, mapper uses `GetInt32()`

---

## Layer 4: Runtime Testing (Optional)

**Execute actual stored procedures with test data to catch runtime errors**

### Manual SQL Testing

**Connect to database:**
```sql
USE mtm_receiving_application;
```

**Test SP with dummy data:**
```sql
-- Test INSERT
CALL sp_volvo_shipment_line_insert(
    1,              -- shipment_id
    'V-EMB-500',    -- part_number
    5,              -- received_skid_count
    440,            -- calculated_piece_count
    0,              -- has_discrepancy
    NULL,           -- expected_skid_count
    NULL            -- discrepancy_note
);

-- Verify result
SELECT * FROM volvo_line_data WHERE shipment_id = 1;

-- Cleanup
DELETE FROM volvo_line_data WHERE shipment_id = 1;
```

**Common Errors:**
- `1054: Unknown column 'description'` → Schema mismatch
- `1318: Incorrect number of arguments` → Parameter count mismatch
- `1048: Column cannot be null` → Missing NOT NULL constraint
- `1452: Foreign key constraint fails` → Invalid reference data

---

## Complete Module Validation Workflow

### Example: Volvo Module Validation

**Step 1: Inventory All Stored Procedures**
```powershell
Get-ChildItem -Path "Database\StoredProcedures\Volvo" -Filter "*.sql" |
    Select-Object Name
```

**Output:**
- `sp_volvo_shipment_insert.sql`
- `sp_volvo_shipment_update.sql`
- `sp_volvo_part_master_get_all.sql`
- ... (18 total)

**Step 2: Inventory All DAO SP Calls**
```powershell
Get-ChildItem -Path "Module_Volvo\Data" -Filter "*.cs" -Recurse |
    Select-String -Pattern '"sp_volvo' |
    ForEach-Object { $_ -replace '.*"(sp_\w+)".*', '$1' } |
    Sort-Object -Unique
```

**Step 3: Cross-Reference (Find Unused SPs)**
```
Compare-Object -ReferenceObject $allSpFiles -DifferenceObject $usedSps
```

**Step 4: Validate Each Used SP**

For each SP found in DAOs:

1. **Open SP file**: `Database/StoredProcedures/Volvo/sp_volvo_part_master_get_all.sql`
2. **Read DAO call**: `Module_Volvo/Data/Dao_VolvoPart.cs`
3. **Read Model mapper**: `Dao_VolvoPart.MapFromReader()`
4. **Check schema**: `Database/Schemas/11_schema_volvo.sql`

**Validation Table:**

| SP Name | Parameters Match | Columns Match | Model Maps | Status |
|---------|------------------|---------------|------------|--------|
| `sp_volvo_part_master_get_all` | ✅ | ❌ (description) | ✅ | **FIXED** |
| `sp_volvo_shipment_line_insert` | ❌ (quantity_per_skid) | ✅ | ✅ | **FIXED** |

**Step 5: Fix Issues**

**Example Fix 1: Remove Non-Existent Column**
```sql
-- BEFORE (BROKEN)
SELECT part_number, description, quantity_per_skid
FROM volvo_masterdata;

-- AFTER (FIXED)
SELECT part_number, quantity_per_skid
FROM volvo_masterdata;
```

**Example Fix 2: Remove Invalid Parameter**
```csharp
// BEFORE (BROKEN)
new Dictionary<string, object>
{
    { "shipment_id", line.ShipmentId },
    { "quantity_per_skid", line.QuantityPerSkid }, // ← NOT IN SP
}

// AFTER (FIXED)
new Dictionary<string, object>
{
    { "shipment_id", line.ShipmentId }
}
```

**Step 6: Archive Unused SPs**

If any SPs are found that are NOT called by any DAO:
```powershell
# Create archive folder if it doesn't exist
New-Item -Path "Database\StoredProcedures\Archived" -ItemType Directory -Force

# Move unused SP
Move-Item -Path "Database\StoredProcedures\Volvo\sp_unused_procedure.sql" `
          -Destination "Database\StoredProcedures\Archived\"
```

---

## Validation Checklist Template

**Use this for each module:**

### Module: `[Module_Name]`

**Date**: _________

#### 1. Schema Validation
- [ ] Run `Scripts\Audit-StoredProcedures.ps1`
- [ ] Review `StoredProcedure_Audit.md` for `[Module]` errors
- [ ] Verify schema files exist: `Database\Schemas\[##]_schema_[module].sql`

#### 2. Stored Procedure Inventory
- [ ] Count SP files: `Database\StoredProcedures\[Module]\*.sql`
- [ ] Count DAO SP calls in `Module_[Module]\Data\*.cs`
- [ ] Identify unused SPs (if any)

#### 3. Parameter Validation (Per SP)
- [ ] SP Name: `_________________________`
- [ ] DAO File: `_________________________`
- [ ] Parameters in DAO: `_________________________`
- [ ] Parameters in SP: `_________________________`
- [ ] Match: ✅ / ❌

#### 4. Column Validation (Per SP)
- [ ] SP Name: `_________________________`
- [ ] SELECT columns: `_________________________`
- [ ] Table schema columns: `_________________________`
- [ ] MapFromReader columns: `_________________________`
- [ ] Match: ✅ / ❌

#### 5. Issues Found
| Issue | SP/DAO | Type | Fix Applied | Status |
|-------|--------|------|-------------|--------|
| | | | | |

#### 6. Deployment
- [ ] Deploy fixed SPs: `Database\Deploy\Deploy-Database-GUI-Fixed.ps1`
- [ ] Verify SP deployment with query: `SHOW CREATE PROCEDURE sp_name`
- [ ] Build application: `dotnet build`
- [ ] Test in UI (if applicable)

---

## Critical Deployment Note

**IMPORTANT**: After fixing stored procedures in `.sql` files, you MUST redeploy them to the database!

### Quick Deployment Steps:
```powershell
# Run the deployment GUI
.\Database\Deploy\Deploy-Database-GUI-Fixed.ps1

# OR manually deploy single SP
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Database\StoredProcedures\Volvo\sp_volvo_part_master_get_by_id.sql
```

### Verify Deployment:
```sql
USE mtm_receiving_application;
SHOW CREATE PROCEDURE sp_volvo_part_master_get_by_id;
-- Check that SELECT statement does NOT include 'description' column
```

---

## Tools Reference

### Audit Script Improvements Made

**Version History:**

**v1.0 - Initial Static Schema Parser**
- Parsed CREATE TABLE statements
- Extracted column names from schemas
- Matched SELECT statements against tables
- **Limitation**: False positives on JOINs, aliases, functions

**v2.0 - Enhanced Parsing (2026-01-05)**
- ✅ Added `SELECT ... INTO` filtering
- ✅ Improved alias handling (`l.id AS label_id` → `id`)
- ✅ Table alias extraction (`l.column` → `column`)
- ✅ Parenthesis-aware comma splitting (handles `COALESCE(a,b)`)
- ✅ Function call detection (skips validation for columns with `(`)

**v3.0 - Recommended Future Enhancements**
- 🔄 Parse JOIN clauses to validate cross-table columns
- 🔄 Extract table schemas from `information_schema` instead of parsing SQL
- 🔄 Validate parameter types (VARCHAR → string, INT → int)
- 🔄 Automated DAO parameter extraction via AST parsing

---

## Quick Reference Commands

### Run Static Audit
```powershell
.\Scripts\Audit-StoredProcedures.ps1
```

### Find All SP Calls in Module
```powershell
Get-ChildItem -Path "Module_Volvo\Data" -Filter "*.cs" -Recurse |
    Select-String -Pattern '"sp_' -Context 0,3
```

### List All SPs for Module
```powershell
Get-ChildItem -Path "Database\StoredProcedures\Volvo" -Filter "*.sql" |
    Select-Object Name
```

### Deploy Database Changes
```powershell
.\Database\Deploy\Deploy-Database-GUI-Fixed.ps1
```

### Build Application
```powershell
dotnet build
```

---

## Lessons Learned

### Issue 1: `description` Column in Volvo Module
**Problem**: 4 stored procedures referenced a `description` column that didn't exist in `volvo_masterdata` table.

**Root Cause**: SPs were created before final schema was locked in.

**Fix**: Removed `description` from all SELECT/INSERT/UPDATE statements.

**Prevention**: Always validate SP against current schema before deployment.

---

### Issue 2: `quantity_per_skid` Parameter Mismatch
**Problem**: `Service_Volvo.cs` passed `quantity_per_skid` to `sp_volvo_shipment_line_insert`, causing MySQL error 1318.

**Root Cause**: `QuantityPerSkid` is a cached UI property in `Model_VolvoShipmentLine`, but NOT a database column in `volvo_line_data`.

**Fix**: Removed parameter from dictionary in Service call.

**Prevention**: Cross-reference DAO parameters with SP signatures before committing.

---

### Issue 3: Collation Mismatch (MySQL Error 1267)
**Problem**: "Illegal mix of collations" error when comparing VARCHAR columns.

**Root Cause**: Volvo tables were created with `utf8mb4_general_ci` collation, but other database tables use `utf8mb4_unicode_ci`, causing comparison failures in WHERE clauses.

**Fix**: Updated all Volvo table schemas to explicitly use `COLLATE=utf8mb4_unicode_ci` to match database standard.

**Prevention**: Always specify `COLLATE=utf8mb4_unicode_ci` in table definitions to ensure consistency.

**Rule**: All tables must use the same collation as the database default.

---

### Issue 4: Boolean to TINYINT Conversion
**Problem**: `Service_Volvo.cs` passed boolean `true/false` directly to MySQL TINYINT(1) parameter, causing type mismatch error.

**Root Cause**: C# bool values are not automatically converted to MySQL TINYINT by the parameter handler.

**Fix**: Explicitly convert boolean to int: `line.HasDiscrepancy ? 1 : 0`

**Prevention**: Always convert C# bool to int (0/1) when passing to MySQL TINYINT parameters.

**Rule**: For TINYINT(1) parameters, use: `boolValue ? 1 : 0`

---

### Issue 4: Audit Script False Positives
**Problem**: Audit script flagged columns like `l.id`, `name AS deliver_to`, `COALESCE(...)` as errors.

**Root Cause**: Simple regex parsing couldn't handle complex SQL expressions.

**Fix**:
- Handle table aliases by extracting last segment after `.`
- Skip validation for any column containing `(` (function calls)
- Smart comma-splitting that respects parentheses

**Remaining Limitation**: Still can't validate JOINed table columns (acceptable trade-off).

---

## Success Metrics

After completing this workflow:
- ✅ **Zero runtime SQL errors** from schema mismatches
- ✅ **All SPs documented** with usage in DAOs
- ✅ **No orphaned SPs** (all used or archived)
- ✅ **Model mapping validated** (no missing columns)
- ✅ **Parameter alignment confirmed** (no MySQL 1318 errors)

---

## Appendix: Common MySQL Error Codes

| Code | Error | Meaning | Fix |
|------|-------|---------|-----|
| 1054 | Unknown column 'X' | Column doesn't exist in table | Update SP to use correct column name |
| 1146 | Table doesn't exist | Missing table | Check schema deployment |
| 1267 | Illegal mix of collations | Table/column collation mismatch | Add COLLATE=utf8mb4_unicode_ci to table schema |
| 1318 | Incorrect number of arguments | Parameter count mismatch | Align DAO parameters with SP signature |
| 1366 | Incorrect integer value | Type conversion failed | Convert bool to int (0/1) for TINYINT |
| 1048 | Column cannot be null | NOT NULL constraint violated | Provide value or make column nullable |
| 1062 | Duplicate entry | UNIQUE constraint violated | Check for existing data |
| 1452 | Foreign key constraint fails | Invalid FK reference | Verify referenced record exists |

---

**Document Maintained By**: Development Team
**Contact**: See project README for team contacts
**Related Files**:
- `Scripts/Audit-StoredProcedures.ps1`
- `StoredProcedure_Audit.md` (generated)
- `Database/Deploy/Deploy-Database-GUI-Fixed.ps1`

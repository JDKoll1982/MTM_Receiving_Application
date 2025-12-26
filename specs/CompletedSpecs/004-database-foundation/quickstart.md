# Quickstart Guide: Dunnage Database Foundation

**Feature**: Dunnage Database Foundation  
**Branch**: `004-database-foundation`  
**Date**: 2025-12-26

## Overview

This guide covers deploying the dunnage database schema, seeding default data, and verifying the installation. The migration creates 5 new tables and removes the legacy `label_table_dunnage` table.

---

## Prerequisites

- MySQL Server 5.7.24+ installed and running
- Database `mtm_receiving_application` exists
- MySQL user with the following privileges:
  - CREATE, DROP, ALTER (for schema changes)
  - INSERT (for seed data)
  - SELECT (for verification)
- Access to repository `Database/Schemas/` directory
- MySQL command-line client or compatible tool (MySQL Workbench, DBeaver, etc.)

---

## Installation Steps

### Step 1: Backup Existing Data (Optional)

If you need to preserve legacy `label_table_dunnage` data before removal:

```powershell
# Export legacy table to CSV
mysql -h localhost -P 3306 -u root -p mtm_receiving_application -e "SELECT * FROM label_table_dunnage" > legacy_dunnage_backup.csv
```

### Step 2: Navigate to Schema Directory

```powershell
cd Database\Schemas
```

### Step 3: Run Migration Script

Execute the dunnage table creation script:

```powershell
# Option 1: Using PowerShell
Get-Content .\06_create_dunnage_tables.sql | mysql -h localhost -P 3306 -u root -p mtm_receiving_application

# Option 2: Using mysql client directly
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < .\06_create_dunnage_tables.sql
```

**Expected Output**: Script completes without errors. All 5 tables are created.

### Step 4: Seed Default Data

Execute the seed data script:

```powershell
# Option 1: Using PowerShell
Get-Content .\06_seed_dunnage_data.sql | mysql -h localhost -P 3306 -u root -p mtm_receiving_application

# Option 2: Using mysql client directly
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < .\06_seed_dunnage_data.sql
```

**Expected Output**: 11 dunnage types created with default specification schemas.

### Step 5: Remove Legacy Table Reference

Update the legacy schema file to remove `label_table_dunnage`:

```powershell
# Open and edit manually (remove CREATE TABLE label_table_dunnage section)
notepad .\01_create_receiving_tables.sql
```

**OR** use the automated deployment script (if available):

```powershell
cd ..\Deploy
.\Deploy-Database.ps1
```

---

## Verification

### Verify Table Creation

Connect to MySQL and verify all 5 tables exist:

```sql
USE mtm_receiving_application;

-- List all dunnage tables
SHOW TABLES LIKE 'dunnage%';
```

**Expected Result**: 5 tables listed:
- `dunnage_types`
- `dunnage_specs`
- `dunnage_part_numbers`
- `dunnage_loads`
- `inventoried_dunnage_list`

### Verify Seed Data

Check that 11 default dunnage types were created:

```sql
-- Verify dunnage types count
SELECT COUNT(*) AS type_count FROM dunnage_types;
-- Expected: 11

-- List all dunnage types
SELECT ID, DunnageType, EntryDate, EntryUser 
FROM dunnage_types 
ORDER BY ID;
```

**Expected Types**:
1. Pallet
2. Crate
3. Box
4. Skid
5. Foam
6. Shrink Wrap
7. Bubble Wrap
8. Gaylord
9. Foldable Crate
10. Wooden Crate
11. Plastic Totes

### Verify Specification Schemas

Check that each type has a default spec schema:

```sql
-- Verify spec count matches type count
SELECT COUNT(*) AS spec_count FROM dunnage_specs;
-- Expected: 11

-- View spec schema for Pallet type
SELECT 
    dt.DunnageType,
    ds.DunnageSpecs,
    ds.SpecAlterDate,
    ds.SpecAlterUser
FROM dunnage_specs ds
JOIN dunnage_types dt ON ds.DunnageTypeID = dt.ID
WHERE dt.DunnageType = 'Pallet';
```

**Expected JSON Schema**:
```json
{
  "Width": { "type": "number", "unit": "inches", "required": true },
  "Height": { "type": "number", "unit": "inches", "required": true },
  "Depth": { "type": "number", "unit": "inches", "required": true },
  "IsInventoriedToVisual": { "type": "boolean", "required": true }
}
```

### Verify Foreign Key Constraints

Test foreign key enforcement:

```sql
-- Test FK-002: Cannot delete type with existing parts
-- (Should fail once parts are created in future features)

-- Test FK-001: Deleting type cascades to specs
DELETE FROM dunnage_types WHERE DunnageType = 'Test Type';
-- Should also delete corresponding spec record
```

### Verify Indexes

Check that all indexes are created:

```sql
-- Show indexes for dunnage_types
SHOW INDEX FROM dunnage_types;
-- Expected: PRIMARY KEY + UNIQUE INDEX on DunnageType

-- Show indexes for dunnage_part_numbers
SHOW INDEX FROM dunnage_part_numbers;
-- Expected: PRIMARY KEY + UNIQUE INDEX on PartID + INDEX on DunnageTypeID

-- Show indexes for dunnage_loads
SHOW INDEX FROM dunnage_loads;
-- Expected: PRIMARY KEY + indexes on PartID, ReceivedDate, PONumber, UserId
```

### Verify Legacy Table Removal

Confirm `label_table_dunnage` no longer exists:

```sql
-- Should return empty result set
SHOW TABLES LIKE 'label_table_dunnage';
```

---

## Usage Examples

### Create a New Part

```sql
-- Create a new pallet part
INSERT INTO dunnage_part_numbers (PartID, DunnageTypeID, DunnageSpecValues, EntryDate, EntryUser)
VALUES (
    'PALLET-48X40-WOOD',
    (SELECT ID FROM dunnage_types WHERE DunnageType = 'Pallet'),
    JSON_OBJECT(
        'Width', 48,
        'Height', 40,
        'Depth', 6,
        'IsInventoriedToVisual', true
    ),
    NOW(),
    'admin'
);
```

### Record a Dunnage Load

```sql
-- Record receiving a dunnage load
INSERT INTO dunnage_loads (
    ID, PartID, DunnageTypeID, Quantity, PONumber, 
    ReceivedDate, UserId, Location, LabelNumber
)
VALUES (
    UUID(),
    'PALLET-48X40-WOOD',
    (SELECT ID FROM dunnage_types WHERE DunnageType = 'Pallet'),
    10.00,
    'PO-123456',
    NOW(),
    'jdoe',
    'DOCK-A',
    'DN-2025-001'
);
```

### Mark Part for Inventory Tracking

```sql
-- Add part to inventory tracking list
INSERT INTO inventoried_dunnage_list (PartID, RequiresInventory, InventoryMethod, DateAdded, AddedBy)
VALUES (
    'PALLET-48X40-WOOD',
    true,
    'Cycle Count',
    NOW(),
    'admin'
);
```

### Query Transaction History

```sql
-- Get all loads for a specific part
SELECT 
    dl.ID,
    dl.Quantity,
    dl.PONumber,
    dl.ReceivedDate,
    dl.UserId,
    dl.Location,
    dl.LabelNumber
FROM dunnage_loads dl
WHERE dl.PartID = 'PALLET-48X40-WOOD'
ORDER BY dl.ReceivedDate DESC;
```

---

## Troubleshooting

### Issue: Script fails with "Table already exists"

**Solution**: Migration script should be idempotent. Check if `DROP TABLE IF EXISTS` statements are present. If not, manually drop tables:

```sql
DROP TABLE IF EXISTS dunnage_loads;
DROP TABLE IF EXISTS inventoried_dunnage_list;
DROP TABLE IF EXISTS dunnage_part_numbers;
DROP TABLE IF EXISTS dunnage_specs;
DROP TABLE IF EXISTS dunnage_types;
```

Then re-run the migration script.

### Issue: Foreign key constraint fails

**Solution**: Ensure tables are created in the correct order:
1. `dunnage_types` (parent)
2. `dunnage_specs` (child of types)
3. `dunnage_part_numbers` (child of types)
4. `inventoried_dunnage_list` (child of parts)
5. `dunnage_loads` (child of parts and types)

### Issue: JSON validation error when inserting data

**Solution**: MySQL 5.7.24 validates JSON syntax only. Ensure JSON is valid:

```sql
-- Test JSON validity
SELECT JSON_VALID('{"Width": 48, "Height": 40}');
-- Should return 1 (true)
```

Application layer will validate JSON schema compliance (data types, required fields).

### Issue: Cannot insert negative quantity

**Solution**: MySQL 5.7.24 does not support CHECK constraints. Application layer must validate `Quantity > 0` before INSERT/UPDATE operations.

### Issue: Character encoding problems

**Solution**: Ensure database and tables use `utf8mb4` character set:

```sql
-- Verify table character set
SHOW CREATE TABLE dunnage_types;
-- Should show CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci
```

---

## Rollback Procedure

If you need to undo the migration:

```sql
-- Drop all dunnage tables (in reverse dependency order)
DROP TABLE IF EXISTS dunnage_loads;
DROP TABLE IF EXISTS inventoried_dunnage_list;
DROP TABLE IF EXISTS dunnage_part_numbers;
DROP TABLE IF EXISTS dunnage_specs;
DROP TABLE IF EXISTS dunnage_types;
```

**Warning**: This will permanently delete all dunnage data. Ensure you have backups if needed.

To restore legacy table, re-run the original schema script:

```powershell
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < .\01_create_receiving_tables.sql
```

---

## Performance Tuning

### Analyze Table Statistics

After loading significant data, update table statistics:

```sql
ANALYZE TABLE dunnage_types;
ANALYZE TABLE dunnage_specs;
ANALYZE TABLE dunnage_part_numbers;
ANALYZE TABLE dunnage_loads;
ANALYZE TABLE inventoried_dunnage_list;
```

### Monitor Query Performance

Use EXPLAIN to verify index usage:

```sql
-- Verify index usage for common query
EXPLAIN SELECT * FROM dunnage_loads 
WHERE PartID = 'PALLET-48X40-WOOD' 
ORDER BY ReceivedDate DESC;
-- Should show "Using index" for PartID
```

### Table Maintenance

For tables with frequent INSERT/DELETE operations:

```sql
-- Optimize tables monthly
OPTIMIZE TABLE dunnage_loads;
```

---

## Next Steps

After successful installation:

1. **Future Features**: 
   - Create stored procedures for CRUD operations (not in this scope)
   - Implement DAOs in C# application layer
   - Build ViewModels and Views for dunnage receiving UI

2. **Data Population**:
   - Add company-specific dunnage types beyond the 11 defaults
   - Import existing part numbers (if available)
   - Configure inventory tracking for specific parts

3. **Integration**:
   - Connect to Infor Visual for PO validation (future feature)
   - Implement label printing integration (future feature)
   - Build reporting and analytics views (future feature)

---

## Support

For issues or questions:

- Review [data-model.md](data-model.md) for detailed schema documentation
- Check [spec.md](spec.md) for functional requirements
- Consult constitution at `.specify/memory/constitution.md` for architectural principles
- Verify MySQL version compatibility (`SELECT VERSION();`)

---

## Changelog

**v1.0 - 2025-12-26**
- Initial database schema creation
- 11 default dunnage types seeded
- Legacy table removal
- Complete foreign key and index implementation

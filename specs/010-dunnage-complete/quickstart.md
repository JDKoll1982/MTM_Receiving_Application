# Quickstart Guide: Dunnage Receiving System - Complete Implementation

**Feature**: Dunnage Complete Implementation  
**Date**: 2025-12-29  
**Version**: 1.0

## Overview

This guide walks you through setting up, testing, and verifying the complete Dunnage Receiving System implementation. Use this after all code is committed to ensure everything works end-to-end.

---

## Prerequisites

1. ✅ Visual Studio 2022 or higher with WinUI 3 workload
2. ✅ .NET 8 SDK installed
3. ✅ MySQL 5.7.24+ server running locally or accessible
4. ✅ Database `mtm_receiving_application` exists with spec 004 foundation schema
5. ✅ User account in `users` table for testing (Windows username or PIN login)
6. ✅ Network share `\\MTMDC\DunnageData\` accessible (optional - for CSV dual-write testing)

---

## Step 1: Database Schema Migration

### 1.1 Run Schema Extension Script

```powershell
# Navigate to Database folder
cd MTM_Receiving_Application/Database

# Execute migration script
& "C:\MAMP\bin\mysql\bin\mysql.exe" -h 172.16.1.104 -P 3306 -u root -proot mtm_receiving_application < Migrations/010-dunnage-complete-schema.sql
```

### 1.2 Verify New Tables

```sql
USE mtm_receiving_application;

-- Check custom_field_definitions table
SHOW CREATE TABLE custom_field_definitions;

-- Check user_preferences table
SHOW CREATE TABLE user_preferences;

-- Verify new columns on existing tables
DESCRIBE dunnage_types; -- Should show 'Icon' column
DESCRIBE inventoried_dunnage_list; -- Should show 'InventoryMethod' and 'Notes' columns

-- Check new indexes
SHOW INDEX FROM dunnage_loads WHERE Key_name LIKE 'IDX_LOADS%';
```

**Expected Output**:
- custom_field_definitions table exists with 9 columns
- user_preferences table exists with 5 columns
- dunnage_types has Icon column (VARCHAR(10), default '&#xE7B8;')
- inventoried_dunnage_list has InventoryMethod and Notes columns
- dunnage_loads has IDX_LOADS_DATE and IDX_LOADS_USER indexes

---

### 1.3 Load Test Data (Optional)

```sql
-- Insert sample custom fields for "Pallet" type (assuming ID=1)
INSERT INTO custom_field_definitions (DunnageTypeID, FieldName, DatabaseColumnName, FieldType, DisplayOrder, IsRequired, ValidationRules, CreatedBy)
VALUES 
(1, 'Weight (lbs)', 'weight_lbs', 'Number', 1, TRUE, '{"min": 1, "max": 9999, "decimals": 2}', 'ADMIN'),
(1, 'Material', 'material', 'Text', 2, FALSE, '{"maxLength": 50}', 'ADMIN'),
(1, 'Condition', 'condition', 'Text', 3, FALSE, '{"maxLength": 30}', 'ADMIN');

-- Insert sample user preferences
INSERT INTO user_preferences (UserId, PreferenceKey, PreferenceValue)
VALUES 
('TESTUSER', 'icon_usage_history', '[{"glyph":"&#xE7B8;","count":5,"lastUsed":"2025-12-20T14:32:00Z"}]'),
('TESTUSER', 'pagination_size_admin', '20'),
('TESTUSER', 'pagination_size_edit', '50');

-- Insert sample inventoried dunnage entries
INSERT INTO inventoried_dunnage_list (PartID, RequiresInventory, InventoryMethod, Notes, AddedBy)
VALUES 
('PALLET-48X40', TRUE, 'Both', 'Standard 48x40 pallet requires inventory tracking', 'ADMIN'),
('CRATE-WOOD-LARGE', TRUE, 'Receive In', 'Large wooden crate for engine shipments', 'ADMIN');
```

---

## Step 2: Build and Run Application

### 2.1 Clean and Build

```powershell
# Clean solution
dotnet clean MTM_Receiving_Application.slnx

# Restore NuGet packages
dotnet restore MTM_Receiving_Application.slnx

# Build in Debug mode
dotnet build MTM_Receiving_Application.slnx --configuration Debug
```

**Expected Output**: Build succeeds with 0 errors

---

### 2.2 Launch Application

```powershell
# Run application
dotnet run --project MTM_Receiving_Application.csproj
```

**Expected**:
1. Splash screen appears (optional)
2. Login screen appears (shared terminal) OR auto-login (personal workstation)
3. Main window displays with Dunnage label button

---

## Step 3: Feature Testing

### 3.1 Manual Entry Mode (F-006)

**Test Case**: Batch entry of 3 dunnage loads

1. Click **Dunnage Label** button on main window
2. Select **Manual Entry (Grid)** mode
3. Verify:
   - DataGrid displays with columns: PartID, Type, Quantity, PO, Location, [Spec columns if type selected]
   - Toolbar shows: Add Row, Delete Row, Save, Export, Sort buttons
4. Click **Add Row** button 3 times
5. Fill first row:
   - PartID: PALLET-48X40 (autocomplete should trigger)
   - Type: Pallet (auto-populated from part)
   - Quantity: 100
   - PO: 66754
   - Location: RECV
   - Spec columns: Width=48, Height=40, Depth=6
6. Fill rows 2-3 with different data
7. Click **Save** button
8. Verify:
   - Success message appears
   - Status bar shows "3 loads saved"
   - Grid clears or remains (per spec requirement - clarify)

**Expected Result**: ✅ All 3 loads saved to `dunnage_loads` table

---

### 3.2 Edit Mode (F-007)

**Test Case**: Load and edit historical data

1. Navigate to **Edit Mode** view (if separate screen)
2. Set date filter: **This Week** preset button
3. Verify:
   - DataGrid populates with loads from current week
   - Pagination shows current page / total pages
   - Date range displayed in header
4. Click on a load row to edit
5. Change **Quantity** from 100 to 95
6. Click **Save Changes** button
7. Verify:
   - Success message appears
   - Updated quantity reflected in grid
   - Database record updated (verify with SQL query)

**Expected Result**: ✅ Load updated successfully

---

### 3.3 Admin Interface - Types Section (F-008)

**Test Case**: Add new dunnage type via Add Type Dialog

1. Navigate to **Admin** → **Dunnage Types**
2. Click **Add New Type** button
3. Verify Add Type Dialog appears with 4 sections:
   - Basic Information
   - Icon Selection
   - Custom Specifications
   - Preview
4. Fill Basic Information:
   - Type Name: "Test Container"
5. Select icon:
   - Click **Search** tab
   - Enter "box" in search field
   - Click a box icon
   - Verify icon appears in preview
6. Add custom fields:
   - Click **Add Field** button
   - Field Name: "Volume (cu ft)"
   - Type: Number
   - Required: Yes
   - Min: 1, Max: 1000, Decimals: 2
   - Click **Add to List**
7. Verify field appears in preview list
8. Drag field to reorder (if >1 field)
9. Click **Create Type** button
10. Verify:
    - Dialog closes
    - New type appears in Types grid
    - Database has new record in `dunnage_types` and `custom_field_definitions`

**Expected Result**: ✅ New type "Test Container" created with custom field

---

### 3.4 Admin Interface - Parts Section (F-008)

**Test Case**: Manage parts

1. Navigate to **Admin** → **Parts**
2. Click **Add New Part** button
3. Fill form:
   - Part ID: TEST-PART-001
   - Dunnage Type: Test Container (dropdown)
   - Spec values: Volume = 50
4. Click **Save**
5. Verify part appears in grid
6. Click **Delete** button on test part
7. Confirm deletion dialog
8. Verify:
   - Part removed from grid
   - Transaction count shown (should be 0)

**Expected Result**: ✅ Part created and deleted successfully

---

### 3.5 Admin Interface - Inventoried List (F-008)

**Test Case**: Manage inventoried parts

1. Navigate to **Admin** → **Inventoried List**
2. Click **Add Part** button
3. Fill form:
   - Part ID: PALLET-48X40 (dropdown or autocomplete)
   - Requires Inventory: Yes
   - Inventory Method: Both (dropdown)
   - Notes: "Used for heavy equipment shipments"
4. Click **Save**
5. Verify entry appears in grid
6. Edit entry:
   - Change Inventory Method to "Receive In"
   - Update Notes
   - Click **Save**
7. Verify changes reflected

**Expected Result**: ✅ Inventoried part added and updated

---

### 3.6 CSV Export - Dynamic Columns (F-009)

**Test Case**: Export loads with multiple types

1. Navigate to **Manual Entry** or **Edit Mode**
2. Ensure grid has loads with different dunnage types:
   - Pallet (specs: Width, Height, Depth)
   - Crate (specs: Length, Width, Height, Material)
   - Box (specs: Dimensions, Weight)
3. Click **Export to CSV** button
4. Verify:
   - Success message appears
   - CSV file created in %APPDATA%\MTM_Receiving_Application\DunnageData_{timestamp}.csv
5. Open CSV file in Excel or text editor
6. Verify columns:
   - Fixed: ID, PartID, DunnageType, Quantity, PONumber, ReceivedDate, UserId, Location, LabelNumber
   - Dynamic: Width, Height, Depth, Length, Material, Dimensions, Weight
   - Blank cells for specs not applicable to a type (e.g., Pallet row has no "Material" value)
7. Verify RFC 4180 compliance:
   - Commas in notes field properly quoted
   - Newlines in notes field properly escaped

**Expected Result**: ✅ CSV exported with dynamic columns, proper escaping

---

### 3.7 Network Path Dual-Write (F-009)

**Test Case**: Verify network write (if network share available)

1. Ensure `\\MTMDC\DunnageData\{username}\` folder exists
2. Export CSV from Manual Entry or Edit Mode
3. Verify:
   - Local CSV created: %APPDATA%\MTM_Receiving_Application\DunnageData_{timestamp}.csv
   - Network CSV created: \\MTMDC\DunnageData\{username}\DunnageData_{timestamp}.csv
   - Status message shows both paths

**Test Case**: Network unavailable fallback

1. Disconnect from network or rename network share
2. Export CSV
3. Verify:
   - Local CSV created successfully
   - Network CSV write fails gracefully
   - Error message shows "Network path unavailable - saved locally only"
   - Application does NOT crash

**Expected Result**: ✅ Local-first strategy works, network failure graceful

---

## Step 4: Validation Testing

### 4.1 Real-Time Validation (Add Type Dialog)

**Test Case**: Duplicate type name validation

1. Open Add Type Dialog
2. Enter existing type name (e.g., "Pallet")
3. Wait 300ms (debounce timer)
4. Verify:
   - Warning InfoBar appears: "A type with this name already exists"
   - Create button remains enabled (warning, not error)

**Test Case**: Required field validation

1. Leave Type Name blank
2. Try to click Create button
3. Verify:
   - Button is disabled
   - Red border on Type Name field
   - Error message: "Type name is required"

**Test Case**: Custom field validation

1. Add custom field with Name: "Weight"
2. Type: Number
3. Min: 10, Max: 5 (invalid range)
4. Verify:
   - Error message: "Maximum must be greater than minimum"
   - Add to List button disabled

**Expected Result**: ✅ All validation works with 300ms debounce

---

### 4.2 Pagination Testing

**Test Case**: Edit Mode pagination

1. Insert 100+ dunnage loads for current month (via SQL or manual entry)
2. Navigate to Edit Mode
3. Filter: This Month
4. Verify:
   - Grid shows 50 items (default page size)
   - Pagination controls show: Page 1 of 3
   - Next/Previous buttons enabled/disabled correctly
5. Click **Next Page**
6. Verify:
   - Grid shows items 51-100
   - Page indicator updates: Page 2 of 3
7. Change items per page to 20 (if UI allows)
8. Verify:
   - Pagination recalculates: Page 1 of 5 (100 items / 20 per page)

**Expected Result**: ✅ Pagination works correctly with dynamic page sizes

---

### 4.3 Performance Testing

**Test Case**: DataGrid with dynamic columns

1. Create dunnage type with 15 custom spec fields
2. Create 10 parts with this type
3. Navigate to Manual Entry
4. Add 100 rows with this type
5. Verify:
   - DataGrid renders without lag (<1 second)
   - Scrolling is smooth (60fps)
   - Editing cells is responsive

**Test Case**: CSV export with 1,000 loads

1. Insert 1,000 dunnage loads via SQL
2. Navigate to Edit Mode
3. Filter: All Time (to show all 1,000)
4. Click **Export to CSV**
5. Measure time from button click to success message
6. Verify:
   - Export completes in <5 seconds
   - CSV file size reasonable (~200KB for 1,000 rows)
   - File opens correctly in Excel

**Expected Result**: ✅ Performance meets requirements

---

## Step 5: Database Verification

### 5.1 Verify Data Integrity

```sql
-- Check dunnage_loads table for manual entry saves
SELECT * FROM dunnage_loads 
WHERE ReceivedDate >= CURDATE() 
ORDER BY CreatedAt DESC 
LIMIT 10;

-- Verify custom field definitions
SELECT t.DunnageType, cf.FieldName, cf.DatabaseColumnName, cf.DisplayOrder
FROM custom_field_definitions cf
JOIN dunnage_types t ON cf.DunnageTypeID = t.ID
ORDER BY t.DunnageType, cf.DisplayOrder;

-- Check user preferences (icon usage tracking)
SELECT * FROM user_preferences 
WHERE PreferenceKey = 'icon_usage_history';

-- Verify inventoried dunnage list
SELECT p.PartID, p.DunnageSpecValues, i.RequiresInventory, i.InventoryMethod, i.Notes
FROM inventoried_dunnage_list i
JOIN dunnage_part_numbers p ON i.PartID = p.PartID;
```

---

## Step 6: Error Handling Testing

### 6.1 Database Connection Failure

**Test Case**: Simulate MySQL server down

1. Stop MySQL server
2. Try to load Edit Mode or Admin grid
3. Verify:
   - User-friendly error dialog: "Unable to connect to database. Please check your connection and try again."
   - Application does NOT crash
   - Log file contains detailed error with stack trace

**Test Case**: Network timeout during CSV export

1. Export to network path
2. Unplug network cable during export
3. Verify:
   - Local CSV write completes
   - Network write fails gracefully
   - Error message shown: "Network write failed - data saved locally"

**Expected Result**: ✅ Graceful error handling, no crashes

---

### 6.2 Concurrent Edit Conflicts

**Test Case**: Two users editing same load

1. User A opens Edit Mode, edits Load ID "ABC-123"
2. User B opens Edit Mode, edits same Load ID "ABC-123"
3. User A saves first
4. User B tries to save
5. Verify:
   - User B gets warning: "This load was modified by another user. Refresh to see latest data."
   - Option to Overwrite or Cancel

**Expected Result**: ✅ Conflict detection works (if implemented - check spec)

---

## Step 7: Acceptance Criteria Validation

### Manual Entry (F-006)

- [x] SC-001: DataGrid displays with correct columns
- [x] SC-002: Toolbar with Add, Delete, Save, Export, Sort buttons
- [x] SC-003: Part ID autocomplete triggers after 3 characters
- [x] SC-004: Type auto-populated when part selected
- [x] SC-005: Spec columns dynamically added based on type
- [x] SC-006: Save writes to dunnage_loads table
- [x] SC-007: Export generates CSV with correct data

### Edit Mode (F-007)

- [x] SC-001: Date filter presets work (This Week, This Month, This Quarter)
- [x] SC-002: Custom date range selection works
- [x] SC-003: DataGrid shows historical loads
- [x] SC-004: Inline editing enabled
- [x] SC-005: Save updates database
- [x] SC-006: Delete confirmation dialog appears
- [x] SC-007: Pagination works (50 items per page default)

### Admin Interface (F-008)

- [x] SC-001: 4-section navigation hub displays
- [x] SC-002: Types CRUD operations work
- [x] SC-003: Parts CRUD operations work
- [x] SC-004: Specs CRUD operations work
- [x] SC-005: Inventoried List CRUD operations work
- [x] SC-006: Impact analysis prevents orphan deletions

### CSV Export (F-009)

- [x] SC-001: Dynamic columns generated from spec keys
- [x] SC-002: RFC 4180 escaping (commas, quotes, newlines)
- [x] SC-003: Dual-path write (local always, network best-effort)
- [x] SC-004: Local write success even if network fails

### Add New Type Dialog (F-010)

- [x] SC-001: Icon picker with 500+ glyphs
- [x] SC-002: Search and category filtering works
- [x] SC-003: Recently used icons section displays
- [x] SC-004: Custom field definitions saved
- [x] SC-005: Drag-drop field reordering works
- [x] SC-006: Real-time validation with 300ms debounce
- [x] SC-007: MaxHeight=750 - no scrolling with ≤5 fields at 1920x1080

---

## Troubleshooting

### Issue: DataGrid columns not displaying

**Cause**: Type selection not triggering column generation  
**Fix**: Check ViewModel subscription to SelectedPart.PropertyChanged event. Verify GetSpecsByTypeAsync() called after type change.

### Issue: CSV export shows "Access Denied"

**Cause**: %APPDATA% folder permissions or network share permissions  
**Fix**: Check folder exists and user has write access. Verify network share path `\\MTMDC\DunnageData\` is correct.

### Issue: Add Type Dialog too tall for 1080p screen

**Cause**: MaxHeight not set or too large  
**Fix**: Verify ContentDialog has `MaxHeight="750"` in XAML. Check custom field count ≤5.

### Issue: Icon picker shows blank icons

**Cause**: Segoe Fluent Icons font not loaded  
**Fix**: Verify font reference in App.xaml or use fallback FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets"

---

## Cleanup After Testing

```sql
-- Delete test data
DELETE FROM dunnage_loads WHERE PartID LIKE 'TEST-%';
DELETE FROM dunnage_part_numbers WHERE PartID LIKE 'TEST-%';
DELETE FROM custom_field_definitions WHERE CreatedBy = 'TESTUSER';
DELETE FROM user_preferences WHERE UserId = 'TESTUSER';
DELETE FROM inventoried_dunnage_list WHERE AddedBy = 'TESTUSER';

-- Reset auto-increment if needed
ALTER TABLE custom_field_definitions AUTO_INCREMENT = 1;
ALTER TABLE user_preferences AUTO_INCREMENT = 1;
```

---

## Next Steps

After successful testing:

1. ✅ Commit all code changes to branch `010-dunnage-complete`
2. ✅ Create pull request for code review
3. ✅ Update CHANGELOG.md with new features
4. ✅ Update user documentation (if exists)
5. ✅ Deploy to staging environment for UAT
6. ✅ Plan production deployment after UAT approval

---

**Quickstart Guide Complete** - Feature ready for testing and validation.

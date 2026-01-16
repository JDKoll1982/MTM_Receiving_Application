# Infor Visual SQL Query Testing

This directory contains SQL query files for **testing and validating** Infor Visual database queries **before implementing them into the application**. Use these files in SQL Server Management Studio (SSMS) to verify query logic, test with real data, and troubleshoot SQL-related failures.

## Purpose

These query files serve as a **testing sandbox** where you can:

- ✅ Verify query syntax and logic against live Infor Visual data
- ✅ Test with actual PO numbers and part IDs before writing C# code
- ✅ Troubleshoot SQL errors without rebuilding the application
- ✅ Validate expected results match actual database structure
- ✅ Document working queries that will be loaded by the application via `Helper_SqlQueryLoader`

**Important**: Once tested and validated here, these queries are loaded by the application's DAOs using the SQL query loader helper. The application does NOT hardcode SQL - it reads these files at runtime.

## File Format Standard

All SQL files in this directory follow a consistent format:

```sql
-- ========================================
-- Query: [Query Name]
-- Description: [What the query does]
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================
-- Test Passed/Failed: [Status and notes]

USE [MTMFG];
GO
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
GO

-- USAGE: [Parameter instructions]
DECLARE @ParameterName DATATYPE = 'TestValue';  -- TEST VALUE - Replace with actual value

-- NOTE: [Important implementation notes about table names, joins, etc.]
SELECT 
    [columns]
FROM [tables]
WHERE [conditions]
ORDER BY [sort];

-- Expected Results:
-- - [Column descriptions and expected outputs]
```

### Format Guidelines

1. **Header Block**: Query name, description, connection details (lines 1-8)
2. **Test Status**: Document whether query passed testing (line 9)
3. **Database Context**: `USE [MTMFG]` and `SET TRANSACTION ISOLATION LEVEL` (lines 10-13)
4. **Parameter DECLAREs**: Test values that can be easily modified (lines 15-16)
5. **Implementation Notes**: Document any table name mappings or special considerations (lines 18-19)
6. **Main Query**: The actual SELECT statement (lines 20-25)
7. **Expected Results**: Documentation of what each column represents (lines 27+)

## Connection Details

- **Server**: VISUAL
- **Database**: MTMFG
- **Site/Warehouse**: 002
- **Authentication**: SQL Server Authentication
  - Username: SHOP2
  - Password: SHOP
- **Important**: All queries are **READ-ONLY**. Never execute INSERT, UPDATE, or DELETE statements.

## Query Files

### 01_GetPOWithParts.sql

Retrieves purchase order details with all associated line items and parts.

**Use Case**: Validating PO data during receiving workflow

**Parameters**:

- `@PoNumber` - 6-digit purchase order number

**Returns**: All lines from the PO with part details, quantities, vendor info

---

### 02_ValidatePONumber.sql

Checks if a purchase order number exists in the system.

**Use Case**: Quick validation before attempting to retrieve PO details

**Parameters**:

- `@PoNumber` - Purchase order number to validate

**Returns**: Count (1 if exists, 0 if not found)

---

### 03_GetPartByNumber.sql

Retrieves detailed information for a specific part including inventory levels.

**Use Case**: Non-PO item receiving, inventory lookup

**Parameters**:

- `@PartNumber` - Part ID to lookup

**Returns**: Part details including on-hand, allocated, and available quantities

---

### 04_SearchPartsByDescription.sql

Searches for parts by description pattern (prefix match).

**Use Case**: Finding parts when part number is unknown

**Parameters**:

- `@SearchTerm` - Text to search for (matches start of description)
- `@MaxResults` - Maximum number of results (default: 50)

**Returns**: List of matching parts with full details

---

## Testing Workflow

### Step 1: Test Query in SSMS

1. Open SQL Server Management Studio (SSMS)
2. Connect to server **VISUAL** using credentials above
3. Open desired `.sql` file from this directory
4. Replace DECLARE parameter values with real test data (e.g., actual PO numbers)
5. Execute query (F5)
6. Verify results match expected output
7. Update test status comment at top of file

### Step 2: Document Results

- Add `-- Test Passed` or `-- Test Failed: [reason]` comment after header
- Document any discovered table name differences (e.g., `PURCHASE_ORDER` vs `po`)
- Note any unexpected behaviors or edge cases

### Step 3: Implement in Application

- Once query is validated, it's automatically loaded by the application
- The `Helper_SqlQueryLoader` class reads these files at runtime
- DAOs call `Helper_SqlQueryLoader.LoadAndPrepareQuery("01_GetPOWithParts.sql")`
- DECLARE statements are automatically stripped out before execution

### Step 4: Troubleshoot Failures

If a query fails in the application:

1. Copy failing query parameters from application logs
2. Open corresponding `.sql` file in SSMS
3. Update DECLARE statements with failing parameter values
4. Execute in SSMS to see actual SQL Server error
5. Fix query in this file
6. Rebuild application (no code changes needed - files are embedded resources)

## Important Notes

⚠️ **READ-ONLY ACCESS ONLY**

- These queries are for testing and validation purposes only
- Never modify data in the Infor Visual database
- All production data writes happen through Infor Visual's native interface

🔍 **Site Filter**

- All queries include `site_id = '002'` filter
- This restricts results to warehouse 002
- Do not remove this filter unless specifically needed

📊 **Performance**

- Queries are optimized with appropriate indexes
- Search queries use TOP clause to limit results
- Add additional WHERE clauses as needed for performance

## Troubleshooting

### Query Execution Issues

**Connection Issues**:

- Verify server name is exactly **VISUAL**
- Ensure SQL Server Authentication is selected
- Check that you can ping the VISUAL server
- Verify SHOP2 account has read permissions

**No Results**:

- Verify parameter values are correct (check actual PO numbers in Infor Visual)
- Check that PO/Part exists for site 002
- Note: Site filter commented out in test files for flexibility
- Try `SELECT TOP 10 * FROM PURCHASE_ORDER` to see available data

**Table/Column Not Found**:

- Infor Visual may use different table names (PURCHASE_ORDER vs po view)
- Check actual table names: `SELECT * FROM INFORMATION_SCHEMA.TABLES`
- Update query file with correct table names
- **Application Code**: See `Data/InforVisual/` for C# DAO implementations that load these queries
- **Service Layer**: See `Services/Database/Service_InforVisual.cs` for business logic using DAOs
- **Query Loader**: See `Helpers/Database/Helper_SqlQueryLoader.cs` for SQL file loading logic
- **Constitution**: See `.specify/memory/constitution.md` for data access principles
- **DAO Pattern**: See `.github/instructions/dao-pattern.instructions.md` for DAO implementation standards

## Adding New Queries

To add a new query for testing:

1. **Create File**: `Database/InforVisualTest/05_YourQueryName.sql`
2. **Follow Format**: Use standard header format (see existing files)
3. **Add DECLAREs**: Include test parameter declarations
4. **Document**: Add expected results section
5. **Test in SSMS**: Validate query works before implementing
6. **Update DAO**: Modify appropriate DAO to call `Helper_SqlQueryLoader.LoadAndPrepareQuery("05_YourQueryName.sql")`
7. **Rebuild**: Embedded resources update automatically

**Example New Query**:

```sql
-- ========================================
-- Query: Get Open PO Count by Vendor
-- Description: Counts open purchase orders grouped by vendor
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- Site: 002
-- ========================================

USE [MTMFG];
GO
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
GO

SELECT 
    po.VENDOR_ID,
    v.NAME AS VendorName,
    COUNT(*) AS OpenPOCount
FROM dbo.PURCHASE_ORDER po
LEFT JOIN dbo.VENDOR v ON po.VENDOR_ID = v.ID
WHERE po.STATUS = 'O'
AND po.SITE_ID = '002'
GROUP BY po.VENDOR_ID, v.NAME
ORDER BY OpenPOCount DESC;

-- Expected Results:
-- - VENDOR_ID: Vendor code
-- - VendorName: Vendor name
-- - OpenPOCount: Number of open POs for this vendor
```

- Ensure SHOP2 account has SELECT permissions on all referenced tables
- Contact DBA if permissions need to be granted
- Never request INSERT/UPDATE/DELETE permissions (READ-ONLY only)

### Application Integration Issues

**Query Loads but Returns Wrong Data**:

- Verify DECLARE parameters match actual DAO parameter names
- Check that `Helper_SqlQueryLoader.ExtractQueryFromFile()` strips DECLAREs correctly
- Ensure DAO passes correct parameter values to `SqlCommand.Parameters.AddWithValue()`

**Embedded Resource Not Found**:

- Verify file is in `Database/InforVisualTest/` directory
- Check `.csproj` includes: `<EmbeddedResource Include="Database\InforVisualTest\*.sql" />`
- Rebuild solution to update embedded resources
- Check resource name: `MTM_Receiving_Application.Database.InforVisualTest.[filename]`

## Related Documentation

- See `Data/InforVisual/` for C# DAO implementations
- See `Services/Database/Service_InforVisual.cs` for service layer
- See `.specify/memory/constitution.md` for data access principles

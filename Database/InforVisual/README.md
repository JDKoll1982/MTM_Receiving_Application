# Infor Visual SQL Queries

This directory contains SQL query files for testing Infor Visual database access using SQL Server Management Studio (SSMS).

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

## Testing Instructions

1. Open SQL Server Management Studio (SSMS)
2. Connect to server **VISUAL** using credentials above
3. Select database **MTMFG**
4. Open desired query file
5. Replace parameter values with test data
6. Execute query (F5)
7. Verify results match expected output

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

**Connection Issues**:
- Verify server name is exactly **VISUAL**
- Ensure SQL Server Authentication is selected
- Check that you can ping the VISUAL server
- Verify SHOP2 account has read permissions

**No Results**:
- Verify parameter values are correct
- Check that PO/Part exists for site 002
- Try removing site filter temporarily to see if data exists for other sites

**Permission Errors**:
- Ensure SHOP2 account has SELECT permissions
- Contact DBA if permissions need to be granted
- Never request INSERT/UPDATE/DELETE permissions

## Related Documentation

- See `Data/InforVisual/` for C# DAO implementations
- See `Services/Database/Service_InforVisual.cs` for service layer
- See `.specify/memory/constitution.md` for data access principles

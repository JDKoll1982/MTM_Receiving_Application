# Infor Visual Database Integration Guide

**Feature**: 003-database-foundation  
**Date**: December 17, 2025  
**Status**: Active

## ⚠️ CRITICAL: READ ONLY ACCESS

**INFOR VISUAL DATABASE IS STRICTLY READ ONLY - NO WRITES ALLOWED AT ANY TIME**

All application writes must go to the MySQL database (`MTM_Receiving_Database`). Infor Visual is queried only for reference data (POs, parts, receiving history).

---

## Connection Configuration

### Server Details
- **Server Name**: VISUAL
- **Database Name**: MTMFG
- **Warehouse ID**: 002 (fixed, always used)
- **Default Username**: SHOP2
- **Default Password**: SHOP
- **Protocol**: SQL Server / T-SQL

### Connection String Format
```csharp
Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;
```

**Key Parameters**:
- `ApplicationIntent=ReadOnly` - Enforces read-only access at connection level
- `TrustServerCertificate=True` - Required for internal servers without valid SSL certs

---

## Database Schema (Actual MTMFG Tables)

### PURCHASE_ORDER (Purchase Order Header)
```sql
CREATE TABLE PURCHASE_ORDER (
    ROWID INT PRIMARY KEY,
    ID NVARCHAR(15) NOT NULL,              -- PO Number
    VENDOR_ID NVARCHAR(15) NOT NULL,
    ORDER_DATE DATETIME NOT NULL,
    DESIRED_RECV_DATE DATETIME,
    STATUS NCHAR(1) NOT NULL,              -- O=Open, P=Partial, C=Closed
    SITE_ID NVARCHAR(8) NOT NULL,
    WAREHOUSE_ID NVARCHAR(8),
    LAST_RECEIVED_DATE DATETIME,
    TOTAL_AMT_ORDERED DECIMAL(20,8),
    TOTAL_AMT_RECVD DECIMAL(20,8),
    -- ... additional columns
);
```

### PURC_ORDER_LINE (Purchase Order Line Items)
```sql
CREATE TABLE PURC_ORDER_LINE (
    ROWID INT PRIMARY KEY,
    PURC_ORDER_ID NVARCHAR(15) NOT NULL,   -- FK to PURCHASE_ORDER.ID
    LINE_NO SMALLINT NOT NULL,             -- Line number (not sequential)
    PART_ID NVARCHAR(35),                  -- FK to PART.ID
    ORDER_QTY DECIMAL(20,8) NOT NULL,      -- Quantity ordered
    TOTAL_RECEIVED_QTY DECIMAL(20,8),      -- Quantity already received
    PURCHASE_UM NVARCHAR(4),               -- Unit of measure
    UNIT_PRICE DECIMAL(20,8),
    LINE_STATUS NCHAR(1) NOT NULL,         -- O=Open, C=Closed, X=Cancelled
    LAST_RECEIVED_DATE DATETIME,
    DESIRED_RECV_DATE DATETIME,
    -- ... additional columns
);
```

### PART (Part Master)
```sql
CREATE TABLE PART (
    ROWID INT PRIMARY KEY,
    ID NVARCHAR(35) NOT NULL,              -- Part ID (primary key)
    DESCRIPTION NVARCHAR(60),
    STOCK_UM NVARCHAR(4) NOT NULL,         -- Unit of measure
    PRODUCT_CODE NVARCHAR(8),              -- Part type/category
    PURCHASED NCHAR(1) NOT NULL,           -- Y/N
    STOCKED NCHAR(1) NOT NULL,             -- Y/N
    INVENTORY_LOCKED NCHAR(1),             -- Y = cannot receive
    -- ... additional columns
);
```

### INVENTORY_TRANS (Receiving Transactions)
```sql
CREATE TABLE INVENTORY_TRANS (
    ROWID INT PRIMARY KEY,
    TRANSACTION_ID INT NOT NULL,
    PURC_ORDER_ID NVARCHAR(15),            -- FK to PURCHASE_ORDER.ID
    PURC_ORDER_LINE_NO SMALLINT,           -- FK to PURC_ORDER_LINE.LINE_NO
    PART_ID NVARCHAR(35),                  -- FK to PART.ID
    TYPE NCHAR(1) NOT NULL,                -- R = Receipt, I = Issue, A = Adjustment
    CLASS NCHAR(1) NOT NULL,               -- 1 = PO Receipt, 2 = Work Order, etc.
    QTY DECIMAL(20,8) NOT NULL,            -- Quantity transacted
    TRANSACTION_DATE DATETIME NOT NULL,
    WAREHOUSE_ID NVARCHAR(8),
    SITE_ID NVARCHAR(8) NOT NULL,
    -- ... additional columns
);
```

**Key for Receiving Queries**:
- `TYPE = 'R'` → Receipt transaction
- `CLASS = '1'` → Purchase Order receipt (as opposed to work order, transfer, etc.)

---

## Stored Procedures

### sp_GetPOWithParts
**Purpose**: Retrieve PO header and all line items with part details

**Parameters**:
- `@PONumber NVARCHAR(15)` - Purchase order number

**Returns**:
- PartID, POLineNumber, PartType, QtyOrdered, Description, VendorID, TotalReceivedQty

**Usage**:
```csharp
var po = await inforVisualService.GetPOWithPartsAsync("123456");
```

---

### sp_GetPartByID
**Purpose**: Retrieve part information for non-PO items (customer-supplied materials)

**Parameters**:
- `@PartID NVARCHAR(35)` - Part identifier

**Returns**:
- PartID, PartType, Description, UnitOfMeasure, Status

**Usage**:
```csharp
var part = await inforVisualService.GetPartByIDAsync("PART-123");
```

---

### sp_GetReceivingByPOPartDate
**Purpose**: Check for same-day receipts to warn of potential duplicate entries

**Parameters**:
- `@PONumber NVARCHAR(15)` - Purchase order number
- `@PartID NVARCHAR(35)` - Part identifier
- `@Date DATE` - Date to query (typically TODAY)

**Returns**:
- TotalQtyReceived, ReceiptCount, LastReceiptTime

**Usage**:
```csharp
var qty = await inforVisualService.GetSameDayReceivingQuantityAsync("123456", "PART-123", DateTime.Today);
```

---

## Service Implementation Pattern

```csharp
public class Service_InforVisual : IService_InforVisual
{
    private const string DefaultServer = "VISUAL";
    private const string DefaultDatabase = "MTMFG";
    private const string DefaultUsername = "SHOP2";
    private const string DefaultPassword = "SHOP";
    
    public async Task<Model_InforVisualPO?> GetPOWithPartsAsync(string poNumber)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("sp_GetPOWithParts", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };
        
        command.Parameters.AddWithValue("@PONumber", poNumber);
        
        await connection.OpenAsync();
        
        // Always use READ UNCOMMITTED for performance (no locking)
        using var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
        command.Transaction = transaction;
        
        using var reader = await command.ExecuteReaderAsync();
        
        // Parse results...
    }
}
```

---

## Best Practices

### DO
✅ Use `SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED` in stored procedures  
✅ Use `ApplicationIntent=ReadOnly` in connection string  
✅ Handle connection failures gracefully (Infor Visual may be offline)  
✅ Use parameterized queries to prevent SQL injection  
✅ Set reasonable command timeouts (30 seconds)  
✅ Cache frequently accessed reference data when appropriate  
✅ Log all queries for debugging and performance monitoring  

### DON'T
❌ Execute INSERT, UPDATE, DELETE, or MERGE statements  
❌ Create, alter, or drop any database objects  
❌ Use transactions that lock tables (e.g., SERIALIZABLE, REPEATABLE READ)  
❌ Assume write access in any code path  
❌ Store connection passwords in plain text (use secure configuration)  
❌ Query without WHERE clauses (table scans on large tables)  
❌ Use `SELECT *` - always specify columns explicitly  

---

## Error Handling

```csharp
try
{
    var po = await _inforVisualService.GetPOWithPartsAsync(poNumber);
    
    if (po == null)
    {
        // PO not found - user may have entered wrong number
        ShowError("PO not found in Infor Visual");
        return;
    }
    
    // Process PO...
}
catch (SqlException ex) when (ex.Number == -2) // Timeout
{
    ShowError("Infor Visual connection timeout - please try again");
}
catch (SqlException ex) when (ex.Number == 53) // Connection failed
{
    ShowError("Cannot connect to Infor Visual - check network connection");
}
catch (SqlException ex)
{
    LogError($"Infor Visual query failed: {ex.Message}");
    ShowError("Database error - contact IT support");
}
```

---

## Testing Checklist

- [ ] Verify connection string uses `ApplicationIntent=ReadOnly`
- [ ] Confirm stored procedures only contain SELECT statements
- [ ] Test graceful handling of Infor Visual offline scenarios
- [ ] Validate PO number formats match PURCHASE_ORDER.ID structure
- [ ] Test part ID validation against PART.ID
- [ ] Verify warehouse filter uses '002'
- [ ] Confirm no writes are attempted anywhere in codebase
- [ ] Test same-day receiving detection with INVENTORY_TRANS
- [ ] Validate performance with typical PO sizes (1-50 lines)
- [ ] Test with locked/inactive parts (INVENTORY_LOCKED = 'Y')

---

## Related Documentation

- [Database Schema CSV Files](../../../Documentation/InforVisual/DatabaseReferenceFiles/)
  - MTMFG_Schema_Tables.csv - Complete table/column listing
  - MTMFG_Schema_FKs.csv - Foreign key relationships
  - MTMFG_Schema_PKs.csv - Primary key definitions
- [Copilot Instructions](../../../.github/agents/copilot-instructions.md) - READ ONLY constraints
- [Service Contracts](./contracts/IService_InforVisual.md) - Interface specification
- [Research Document](./research.md) - Technical decisions

---

**Last Updated**: December 17, 2025  
**Maintained By**: Development Team  
**Review Required**: Before any Infor Visual-related code changes

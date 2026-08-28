<!-- 
[DOC-META-START]
- File Name: infor_visual_constraints.md
- Description: Infor Visual SQL Server read-only rules, connection requirements, and DAO patterns.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 20-23: # Infor Visual Integration Constraints
  - Line 24-27: ## CRITICAL: READ-ONLY DATABASE
  - Line 28-42: ## Connection Requirements
  - Line 43-81: ## DAO Pattern for Infor Visual
  - Line 82-113: ## Allowed Operations
  - Line 114-124: ## Forbidden Operations
  - Line 125-147: ## Error Handling
  - Line 148-157: ## Registration in DI
  - Line 158-167: ## Pre-Commit Validation
- Critical Notes: ApplicationIntent=ReadOnly; never INSERT/UPDATE/DELETE against Visual.
[DOC-META-END]
-->

# Infor Visual Integration Constraints

Last Updated: 2026-03-21

## CRITICAL: READ-ONLY DATABASE

⚠️ **INFOR VISUAL DATABASE IS STRICTLY READ ONLY - NO WRITES ALLOWED AT ANY TIME** ⚠️

## Connection Requirements

### Connection String MUST Include

```csharp
ApplicationIntent=ReadOnly
```

### Connection Details

- Server: VISUAL
- Database: MTMFG
- Warehouse ID: 002 (fixed, always)
- Default Credentials: Username=SHOP2, Password=SHOP

## DAO Pattern for Infor Visual

```csharp
public class Dao_InforVisualPO
{
    private readonly string _connectionString;

    public Dao_InforVisualPO(string connectionString)
    {
        // VALIDATE READ-ONLY INTENT
        if (!connectionString.Contains("ApplicationIntent=ReadOnly"))
            throw new InvalidOperationException("Infor Visual connection MUST be read-only");

        _connectionString = connectionString;
    }

    // ✅ ALLOWED - SELECT query
    public async Task<Model_Dao_Result<Model_InforVisualPO>> GetPOByNumberAsync(string poNumber)
    {
        string query = @"
            SELECT po.ID, po.VENDOR_ID, po.STATUS, po.ORDER_DATE
            FROM PURCHASE_ORDER po
            WHERE po.ID = @PoNumber";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PoNumber", poNumber);

        // Execute and return results
    }

    // ❌ FORBIDDEN - Any write operation
    public async Task UpdatePOStatusAsync(string poNumber, string status)
    {
        throw new InvalidOperationException("Writes to Infor Visual are STRICTLY FORBIDDEN");
    }
}
```

## Allowed Operations

### ✅ SELECT Queries Only

- Query PURCHASE_ORDER, PURC_ORDER_LINE, PART, INVENTORY_TRANS tables
- SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED (performance)
- Graceful handling when Infor Visual is unavailable

### Common Queries

```sql
-- PO Lookup
SELECT po.ID, po.VENDOR_ID, po.STATUS, po.ORDER_DATE
FROM PURCHASE_ORDER po
WHERE po.ID = @PoNumber

-- PO Line Details
SELECT pol.PURC_ORDER_ID, pol.LINE_NO, pol.PART_ID, pol.ORDER_QTY
FROM PURC_ORDER_LINE pol
WHERE pol.PURC_ORDER_ID = @PoNumber

-- Part Information
SELECT p.ID, p.DESCRIPTION, p.PRODUCT_CODE, p.STOCK_UM
FROM PART p
WHERE p.ID = @PartId

-- Transaction History (Receipts)
SELECT it.*
FROM INVENTORY_TRANS it
WHERE it.TYPE = 'R' AND it.CLASS = '1'
```

## Forbidden Operations

### ❌ NEVER ALLOWED

- Any INSERT statements
- Any UPDATE statements
- Any DELETE statements
- Any MERGE statements
- CREATE, ALTER, DROP (DDL)
- Transactions that lock tables (SERIALIZABLE, REPEATABLE READ)

## Error Handling

### Graceful Offline Handling

```csharp
public async Task<Model_Dao_Result<Model_InforVisualPO>> GetPOAsync(string poNumber)
{
    try
    {
        // Attempt connection
        return await QueryInforVisual(poNumber);
    }
    catch (SqlException ex) when (IsConnectionError(ex))
    {
        // Infor Visual may be offline - this is acceptable
        return DaoResultFactory.Failure<Model_InforVisualPO>(
            "Infor Visual database is currently unavailable. Please try again later.",
            ex,
            Enum_ErrorSeverity.Warning);
    }
}
```

## Registration in DI

```csharp
// Infrastructure/DependencyInjection/ extension methods
var inforConnectionString = Helper_Database_Variables.GetInforVisualConnectionString();

services.AddSingleton(sp => new Dao_InforVisualPO(inforConnectionString));
services.AddSingleton(sp => new Dao_InforVisualPart(inforConnectionString));
```

## Pre-Commit Validation

Before committing Infor Visual code:

- [ ] Connection string includes `ApplicationIntent=ReadOnly`
- [ ] Constructor validates read-only intent
- [ ] Only SELECT queries used
- [ ] No INSERT/UPDATE/DELETE statements
- [ ] Graceful handling of connection failures
- [ ] Proper error messages for unavailable database

---
applyTo: "Database/InforVisualScripts/**/*.sql,Module_*/Data/**/*Visual*.cs,Module_*/Data/**/*InforVisual*.cs,Module_*/Data/**/*SqlServer*.cs"
description: >
  Step-by-step guide for planning and writing new SQL SELECT queries against the MTMFG
  Infor Visual (SQL Server) database. Covers the required research workflow using the CSV
  reference files, mandatory constraints, query patterns, and the C# DAO wiring that consumes
  the query. Includes annotated examples drawn from Database/InforVisualScripts/Queries/.
---

# Infor Visual Query Authoring Guide

## Absolute Constraints — Read Before Writing a Single Line of SQL

1. **READ ONLY.** No `INSERT`, `UPDATE`, `DELETE`, `MERGE`, `TRUNCATE`, or DDL of any kind.
   The connection string must include `ApplicationIntent=ReadOnly`.
2. **No stored procedures exist.** MTMFG has zero user-defined stored procedures or functions
   in SQL Server (verified 2026-03-06). Every database operation is a plain `SELECT` query.
3. **V_ prefix tables are base tables, not views.** `V_ACCOUNT`, `V_CONTRACT`, and similar are
   regular SQL Server tables accessed the same way as any other table. The only actual SQL
   Server views are `CR_PART_LOCATION` and `SYSUSERAUTH` (see `MTMFG_Schema_Views.csv`).
4. **Always use the `dbo.` schema prefix** — all MTMFG tables are in the `dbo` schema.
5. **Always use `TOP (@MaxResults)`** when the result set is potentially unbounded. Never
   return all rows from a large table without a filter or row limit.
6. **Parameterize every filter.** No string concatenation. Use `@ParameterName` placeholders
   throughout; the C# DAO passes them as `SqlParameter` objects.
7. **Never expose connection strings, passwords, or server names** in query files or C# code.
   Connection strings live in `appsettings.json` / `Helper_Database_Variables`.

---

## Step-by-Step Authoring Workflow

### Step 1 — Determine what tables you need

Open `docs/InforVisual/DatabaseReferenceFiles/MTMFG_Schema_Tables.csv` (columns:
`TABLE_SCHEMA`, `TABLE_NAME`, `COLUMN_NAME`, `DATA_TYPE`, `IS_NULLABLE`) and identify the
candidate tables. Then confirm the columns exist and note their exact spelling — Infor Visual
column names must be spelled exactly.

For precision/scale of `decimal` columns and string lengths, look up the same table/column in
`MTMFG_Schema_ColumnDetails.csv` (no header row; column order:
`TABLE_NAME, COLUMN_ORDER, COLUMN_NAME, DATA_TYPE, MAX_CHAR_LENGTH, NUMERIC_PRECISION,
NUMERIC_SCALE, IS_NULLABLE, IS_IDENTITY, IS_COMPUTED`).

### Step 2 — Discover join paths

Open `MTMFG_Schema_FKs.csv` (columns: `FK_Name`, `Table`, `Column`, `Referenced_Table`,
`Referenced_Column`) and trace the FK chain between the tables you need. For composite FKs
all listed columns in the same `FK_Name` group must appear in the JOIN condition.

### Step 3 — Check indexes on large tables

For tables expected to have many rows, consult `MTMFG_Schema_TableRowCounts.csv` (no header row;
columns: `TABLE_NAME, APPROX_ROW_COUNT`). If a table has > 100,000 rows, also check
`MTMFG_Schema_Indexes.csv` (no header row; columns: `TABLE_NAME, INDEX_NAME, INDEX_TYPE,
IS_UNIQUE, IS_PRIMARY_KEY, COLUMN_NAME, KEY_ORDINAL, IS_INCLUDED`) and ensure your `WHERE`
clause filters on indexed columns.

**Largest tables (avoid full scans):**
- `CUST_BOOK_DEL` (~4.8M rows)
- `WIP_ISSUE_DETAIL` (~3.9M rows)
- `INVENTORY_TRANS` (~3.5M rows)
- `LABOR_TICKET`, `CUSTOMER_ORDER`, `PURCHASE_ORDER`, `RECEIVER_LINE`

### Step 4 — Write the query following the style rules below

See the **Query Style Rules** section and the **Annotated Examples** section.

### Step 5 — Save in the correct folder

All Infor Visual SQL files belong in `Database/InforVisualScripts/Queries/`.
Name files using the sequential prefix pattern: `NN_PurposeName.sql`.

### Step 6 — Wire the C# DAO

The query is consumed by a DAO class in the relevant module's `Data/` folder. See the
**C# DAO Wiring** section below.

---

## Query Style Rules

### Header Comment Block (Required on every file)

```sql
-- ========================================
-- Query: <Short human-readable name>
-- Description: <What this query returns and why>
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
```

For shorter, single-purpose files the block may be condensed to four leading comment lines
(see examples 06 and 07 below).

### SELECT clause

- Always name every column with an `AS` alias using PascalCase.
- Never use `SELECT *`.
- Prefix every column reference with the table alias when joining multiple tables.

### FROM / JOIN clause

- Always use `dbo.TableName alias` form.
- Use `INNER JOIN` when the related row must exist; `LEFT JOIN` when it may be absent.
- Join on the FK columns found in `MTMFG_Schema_FKs.csv`.

### WHERE clause

- All filter values must be `@ParameterName` — never literals embedded in the query.
- Filter on indexed columns first (consult `MTMFG_Schema_Indexes.csv`).
- Never apply a function to an indexed column on the left side of a comparison
  (e.g., avoid `UPPER(p.ID) = @id`; compare lowercase parameters instead).

### Fuzzy / LIKE searches

- The C# caller wraps the term with `%` wildcards before passing it (e.g., `'%term%'`).
- Use `p.COLUMN LIKE @Term`; do not append wildcards inside the SQL.
- Always pair a `LIKE` search with `TOP (@MaxResults)` and an `ORDER BY`.

### DECLARE blocks for standalone test execution

Files in `Database/InforVisualScripts/Queries/` include `DECLARE` blocks at the top so they
can be run directly in SSMS for testing. The C# DAO strips these and passes the values as
`SqlParameter` objects instead.

```sql
DECLARE @PartNumber nvarchar(30) = 'TEST-PART';
DECLARE @MaxResults int          = 50;
```

### Comments

- Add a `-- NOTE:` line for any non-obvious table choice, missing FK, or ERP quirk.
- Add `-- READ-ONLY query against Infor Visual (MTMFG) - no writes.` at the top of
  condensed-header files.
- Add an `-- Expected Results:` block listing each output column and what it represents
  (see examples 01–04).

---

## Annotated Examples

### Example 01 — Exact PK lookup with JOINs (`01_GetPOWithParts.sql`)

Returns all lines for a specific purchase order, joined to PART and VENDOR.

```sql
-- ========================================
-- Query: Get PO with Parts
-- Description: Retrieves purchase order details with all associated parts
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
SELECT
    po.ID                                       AS PoNumber,
    pol.LINE_NO                                 AS PoLine,
    pol.PART_ID                                 AS PartNumber,
    p.DESCRIPTION                               AS PartDescription,
    pol.ORDER_QTY                               AS OrderedQty,
    pol.TOTAL_RECEIVED_QTY                      AS ReceivedQty,
    (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY)    AS RemainingQty,
    pol.PURCHASE_UM                             AS UnitOfMeasure,
    pol.PROMISE_DATE                            AS DueDate,
    po.VENDOR_ID                                AS VendorCode,
    v.NAME                                      AS VendorName,
    po.STATUS                                   AS PoStatus,
    po.SITE_ID                                  AS SiteId
FROM dbo.PURCHASE_ORDER po
INNER JOIN dbo.PURC_ORDER_LINE pol ON po.ID         = pol.PURC_ORDER_ID
LEFT  JOIN dbo.PART            p   ON pol.PART_ID   = p.ID
LEFT  JOIN dbo.VENDOR          v   ON po.VENDOR_ID  = v.ID
WHERE po.ID = @PoNumber
ORDER BY pol.LINE_NO;
```

**Research path used:**
- `MTMFG_Schema_FKs.csv`: `PURC_ORDER_LINE.PURC_ORDER_ID → PURCHASE_ORDER.ID`
- `MTMFG_Schema_Tables.csv`: confirmed `TOTAL_RECEIVED_QTY`, `PROMISE_DATE`, `SITE_ID` columns
- `MTMFG_Schema_PKs.csv`: `PURCHASE_ORDER` PK = `ID`

---

### Example 02 — Existence check (`02_ValidatePONumber.sql`)

```sql
-- ========================================
-- Query: Validate PO Number
-- Description: Checks if a PO number exists in the system
-- Database: MTMFG (Infor Visual)
-- ========================================
SELECT COUNT(*) AS POExists
FROM dbo.PURCHASE_ORDER
WHERE ID = @PoNumber;
-- Returns 1 if exists, 0 if not
```

**Pattern:** Use `COUNT(*)` for boolean existence checks. The C# DAO maps the result to `bool`.

---

### Example 03 — Single-entity lookup with calculated columns (`03_GetPartByNumber.sql`)

```sql
SELECT
    p.ID                                                        AS PartNumber,
    p.DESCRIPTION                                               AS Description,
    ps.UNIT_MATERIAL_COST                                       AS UnitCost,
    p.STOCK_UM                                                  AS PrimaryUom,
    COALESCE(ps.QTY_ON_HAND, 0)                                 AS OnHandQty,
    COALESCE(ps.QTY_COMMITTED, 0)                               AS AllocatedQty,
    (COALESCE(ps.QTY_ON_HAND, 0) - COALESCE(ps.QTY_COMMITTED, 0)) AS AvailableQty,
    ps.SITE_ID                                                  AS DefaultSite,
    ps.STATUS                                                   AS PartStatus,
    p.PRODUCT_CODE                                              AS ProductLine
FROM dbo.PART p
LEFT JOIN dbo.PART_SITE ps ON p.ID = ps.PART_ID
WHERE p.ID = @PartNumber;
```

**Key patterns:**
- `COALESCE(column, 0)` for nullable numeric columns — `PART_SITE` rows may be absent.
- `LEFT JOIN` used because a part may not have a `PART_SITE` row.

---

### Example 04 — Prefix search with row limit (`04_SearchPartsByDescription.sql`)

```sql
SELECT TOP (@MaxResults)
    p.ID            AS PartNumber,
    p.DESCRIPTION   AS Description,
    p.STOCK_UM      AS PrimaryUom,
    ps.STATUS       AS PartStatus
FROM dbo.PART p
LEFT JOIN dbo.PART_SITE ps ON p.ID = ps.PART_ID
WHERE p.DESCRIPTION LIKE @SearchTerm + '%'
ORDER BY p.ID;
```

**Pattern:** Suffix wildcard (`LIKE @term + '%'`) for prefix searches.
`@SearchTerm` is passed without wildcards; the SQL appends the trailing `%`.

---

### Example 05 — Fuzzy (contains) search (`06_FuzzySearchPartsByID.sql`)

```sql
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
-- Parameters:
--   @Term        nvarchar  Wildcard-wrapped search term, e.g. '%21-288%'
--   @MaxResults  int       Maximum rows to return

DECLARE @Term       nvarchar(60) = '%21-288%';
DECLARE @MaxResults int          = 50;

SELECT TOP (@MaxResults)
    p.ID          AS PartNumber,
    p.DESCRIPTION AS Description,
    p.STOCK_UM    AS PrimaryUom
FROM dbo.PART p
WHERE p.ID LIKE @Term
ORDER BY p.ID;
```

**Pattern:** The C# caller wraps the search term with `%` before passing it as a parameter.
Do not concatenate wildcards inside the SQL itself.

---

### Example 06 — Multi-join history query (`05_GetOutsideServiceHistoryByPart.sql`)

```sql
SELECT DISTINCT
    v.ID                AS VendorID,
    v.NAME              AS VendorName,
    sd.ID               AS DispatchID,
    sd.CREATE_DATE      AS DispatchDate,
    sdl.SERVICE_PART_ID AS PartNumber,
    sdl.DISPATCH_QTY    AS QuantitySent,
    CASE
        WHEN sdl.RECEIVED_QTY >= sdl.DISPATCH_QTY THEN 'Closed'
        ELSE 'Open'
    END                 AS DispatchStatus
FROM dbo.SERVICE_DISP_LINE sdl
INNER JOIN dbo.SERVICE_DISPATCH sd ON sdl.DISPATCH_ID = sd.ID
INNER JOIN dbo.VENDOR           v  ON sd.VENDOR_ID    = v.ID
WHERE sdl.SERVICE_PART_ID = @PartNumber
ORDER BY sd.CREATE_DATE DESC;
```

**Pattern:** Use `DISTINCT` when the join cardinality can produce duplicate dispatch rows.
Use `CASE` for derived status columns rather than joining a lookup table.

---

### Example 07 — Aggregate with GROUP BY (`09_GetDistinctPartsByVendor.sql`)

```sql
SELECT
    sdl.SERVICE_PART_ID  AS PartNumber,
    COUNT(sd.ID)         AS DispatchCount,
    MAX(sd.CREATE_DATE)  AS LastDispatchDate
FROM dbo.SERVICE_DISP_LINE sdl
INNER JOIN dbo.SERVICE_DISPATCH sd ON sdl.DISPATCH_ID = sd.ID
WHERE sd.VENDOR_ID = @VendorID
GROUP BY sdl.SERVICE_PART_ID
ORDER BY sdl.SERVICE_PART_ID;
```

**Pattern:** Use `COUNT` / `MAX` aggregates with `GROUP BY` to summarize history.

---

## C# DAO Wiring

Every query written here is consumed by a DAO class. The DAO follows these rules:

- DAO class is **instance-based** (not static), constructed with a connection string.
- Connection string comes from `Helper_Database_Variables` — never hardcoded.
- SQL Server connection must include `ApplicationIntent=ReadOnly`.
- All parameters are passed as `SqlParameter` objects — never string-interpolated.
- The DAO method returns `Model_Dao_Result<T>` — it **never throws**, returning an error
  result instead.

### Canonical DAO pattern for a Visual query

```csharp
public async Task<Model_Dao_Result<List<Model_PurchaseOrderLine>>> GetPOLinesAsync(string poNumber)
{
    const string sql = @"
        SELECT
            pol.LINE_NO          AS PoLine,
            pol.PART_ID          AS PartNumber,
            pol.ORDER_QTY        AS OrderedQty,
            pol.TOTAL_RECEIVED_QTY AS ReceivedQty,
            pol.PROMISE_DATE     AS DueDate
        FROM dbo.PURCHASE_ORDER     po
        INNER JOIN dbo.PURC_ORDER_LINE pol ON po.ID = pol.PURC_ORDER_ID
        WHERE po.ID = @PoNumber
        ORDER BY pol.LINE_NO;";

    try
    {
        await using var connection = new SqlConnection(_connectionString);
        var rows = await connection.QueryAsync<Model_PurchaseOrderLine>(
            sql,
            new { PoNumber = poNumber });

        return Model_Dao_Result<List<Model_PurchaseOrderLine>>.Success(rows.ToList());
    }
    catch (Exception ex)
    {
        return Model_Dao_Result<List<Model_PurchaseOrderLine>>.Failure(
            $"Error retrieving PO lines for {poNumber}: {ex.Message}");
    }
}
```

**Important:**
- The `DECLARE` blocks from the `.sql` file are removed; the C# method passes `@PoNumber`
  as a Dapper / SqlParameter argument instead.
- Map each query alias (`PoLine`, `PartNumber`, etc.) to the corresponding C# model property.
- The C# model property names should match the SQL `AS` aliases exactly.

---

## Common Mistakes to Avoid

| Mistake | Correct Approach |
|---|---|
| Using a `V_` table name expecting it to be a view | `V_` tables are base tables — query them like any other table |
| Calling a stored procedure that doesn't exist | Use a plain `SELECT` — MTMFG has no SPs |
| Including `CHECK_DEFINITION` or `DEFAULT_EXPRESSION` from the CSVs as valid values | These are all NULL in MTMFG (encrypted) — do not rely on them for enum mapping |
| Running unsupported `STRING_AGG` | SQL Server 2016 — use `STUFF`/`FOR XML PATH` instead |
| Filtering on a non-indexed column on a large table | Check `MTMFG_Schema_Indexes.csv` first |
| Returning all rows from `INVENTORY_TRANS` or `CUST_BOOK_DEL` | Always add `TOP (@MaxResults)` and a tight `WHERE` clause |
| Writing to the Visual database from C# | READ ONLY — `ApplicationIntent=ReadOnly` enforced |

---

## Reference Files Checklist

Before writing a new query, confirm you have consulted:

- [ ] `MTMFG_Schema_Tables.csv` — column names and data types
- [ ] `MTMFG_Schema_FKs.csv` — JOIN paths between tables
- [ ] `MTMFG_Schema_PKs.csv` — single-row lookup keys
- [ ] `MTMFG_Schema_ColumnDetails.csv` — string lengths and decimal precision
- [ ] `MTMFG_Schema_Indexes.csv` — ensure WHERE filters use indexed columns
- [ ] `MTMFG_Schema_TableRowCounts.csv` — assess whether TOP and tight WHERE are needed

All files are in `docs/InforVisual/DatabaseReferenceFiles/`.

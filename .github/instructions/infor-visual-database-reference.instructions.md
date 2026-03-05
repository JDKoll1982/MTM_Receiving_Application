---
applyTo: "**"
description: >
  Infor Visual (MTMFG) SQL Server database reference files — what each CSV file contains,
  how to use them when generating SQL queries or C# models, and the complete file inventory
  including recommended files not yet generated.
---

# Infor Visual Database Reference Files

The `docs/InforVisual/DatabaseReferenceFiles/` folder contains CSV exports of the MTMFG SQL
Server schema used by the Infor Visual ERP. **This database is READ ONLY** — the application
never issues INSERT, UPDATE, or DELETE against it. All write operations go to MySQL via stored
procedures.

Reference these files whenever you need to write a SQL SELECT query, a C# model that maps to
an Infor Visual table, or a DAO that joins multiple Visual tables.

---

## Critical Rules Before Writing Any SQL

- `ApplicationIntent=ReadOnly` must appear in the SQL Server connection string.
- No INSERT, UPDATE, or DELETE against any Visual table — ever.
- Always verify column names against the CSV files before generating code; Infor Visual
  column names are case-insensitive but must be spelled exactly.
- Prefer filtering on **indexed** columns (see `MTMFG_Schema_Indexes.csv` when available).
- Tables prefixed `V_` are SQL Server **views**, not base tables (see Views section below).

---

## Existing Reference Files

### `MTMFG_Schema_Tables.csv`

**Columns:** `TABLE_SCHEMA`, `TABLE_NAME`, `COLUMN_NAME`, `DATA_TYPE`, `IS_NULLABLE`

**Use for:**
- Confirming a column exists on a table before referencing it in SQL.
- Determining the SQL data type to map to a C# property.
- Checking nullability to decide whether a C# property should be nullable (`?`).

**Data-type → C# mapping guide:**

| SQL Type | C# Type | Notes |
|---|---|---|
| `int` | `int` | Nullable if `IS_NULLABLE = YES` → `int?` |
| `smallint` | `short` | Nullable → `short?` |
| `bigint` | `long` | Nullable → `long?` |
| `decimal` | `decimal` | Nullable → `decimal?` — check precision/scale in ColumnDetails CSV |
| `nvarchar` | `string` | Always reference-type; use `string?` when nullable |
| `nchar` | `string` | Single-char flags (`'Y'`/`'N'`) — map to `bool` with a converter |
| `datetime` | `DateTime` | Nullable → `DateTime?` |
| `image` | `byte[]` | Binary blobs — rarely needed in this application |
| `bit` | `bool` | Nullable → `bool?` |

**Limitation:** Does not include string max-length, decimal precision/scale, or identity flags.
See `MTMFG_Schema_ColumnDetails.csv` (recommended) for those details.

---

### `MTMFG_Schema_FKs.csv`

**Columns:** `FK_Name`, `Table`, `Column`, `Referenced_Table`, `Referenced_Column`

**Use for:**
- Constructing JOIN clauses between tables.
- Understanding navigation paths (e.g., to get from `RECEIVER_LINE` to `PURCHASE_ORDER`).
- Validating that a proposed JOIN is based on an actual relationship, not an assumption.

**Example usage — finding how to join RECEIVER_LINE to PURCHASE_ORDER:**

```
FK lookup: Table=RECEIVER_LINE, Column=PURC_ORDER_ID → References PURCHASE_ORDER.ID
FK lookup: Table=RECEIVER_LINE, Column=PURC_ORDER_LINE_NO → References PURC_ORDER_LINE.LINE_NO
```

This tells you:
```sql
SELECT r.*, po.*
FROM   RECEIVER_LINE r
JOIN   PURCHASE_ORDER po ON r.PURC_ORDER_ID = po.ID
```

**Tip:** When a table has a composite FK (multiple rows with the same `FK_Name`), all listed
columns must appear in the JOIN condition.

---

### `MTMFG_Schema_PKs.csv`

**Columns:** `Table`, `PrimaryKeyColumn`

**Use for:**
- Building WHERE clauses that uniquely identify a record.
- Understanding composite PKs — if multiple rows list the same table, all listed columns
  together form the PK.
- Generating correct C# model `[Key]` annotations or DAO lookup methods.

**Example — OPERATION has a 5-column composite PK:**

```
OPERATION | WORKORDER_TYPE
OPERATION | WORKORDER_BASE_ID
OPERATION | WORKORDER_LOT_ID
OPERATION | WORKORDER_SPLIT_ID
OPERATION | WORKORDER_SUB_ID
OPERATION | SEQUENCE_NO
```

So any DAO method that fetches a single operation must supply all six columns.

---

## Recommended Additional Files (Not Yet Generated)

See `ADDITIONAL_CSV_RECOMMENDATIONS.md` in this folder for the full list with SSMS SQL queries.
Summary of what each will provide once generated:

### `MTMFG_Schema_ColumnDetails.csv` ★★★ HIGH PRIORITY

Adds `MAX_CHAR_LENGTH`, `NUMERIC_PRECISION`, `NUMERIC_SCALE`, `IS_IDENTITY`, `IS_COMPUTED`
to every column. **Required before generating C# model properties for decimal or string columns.**

Use when:
- Generating `decimal(18,4)` vs `decimal(10,2)` stored procedure parameters.
- Setting `[MaxLength]` annotations on C# string properties.
- Skipping identity columns from INSERT parameter lists (MySQL DAOs).

---

### `MTMFG_Schema_Views.csv` ★★★ HIGH PRIORITY

All `V_` prefix objects in the Tables CSV are views. Their column definitions differ from base
tables and the Tables CSV includes them mixed in with real tables.

Use when:
- Writing SELECT queries against any `V_` object (V_ACCOUNT, V_CONTRACT, V_SRVC_ORDER, etc.).
- Verifying that a column available on a view actually exists on that view.

---

### `MTMFG_Schema_Indexes.csv` ★★

Use when:
- Writing WHERE clauses on large tables — filter on indexed columns first.
- Choosing between equivalent join paths — prefer the one using an indexed FK column.
- Tables known to be large (see RowCounts CSV): `INVENTORY_TRANS`, `LABOR_TICKET`,
  `CUSTOMER_ORDER`, `PURCHASE_ORDER`, `RECEIVER_LINE`.

---

### `MTMFG_Schema_TableRowCounts.csv` ★★

Use when:
- Deciding whether a full-table query is safe or whether a TOP/WHERE is mandatory.
- Documenting expected query performance in DAO method XML comments.

---

### `MTMFG_Schema_Triggers.csv` ★★

Use when:
- Explaining to the user why a Visual record appears to change fields automatically after an
  ERP action (trigger side-effects).
- Documenting that certain tables should not be queried mid-transaction from the ERP side.

---

### `MTMFG_Schema_StoredProcedures.csv` ★

Use when:
- Checking whether Infor Visual already exposes a data operation as a stored procedure before
  building raw SELECT logic.
- Generating parameter lists for any Visual SP that may be called in the future.

---

### `MTMFG_Schema_UniqueConstraints.csv` ★

Use when:
- Writing lookup queries by business key (e.g., `PART.ID`, `VENDOR.ID`, `CUSTOMER.ID`) —
  unique constraints confirm these are safe single-row lookups.

---

### `MTMFG_Schema_DefaultConstraints.csv` ★

Use when:
- Building test fixture data for integration tests — columns with defaults can be omitted.
- Documenting expected default values in model XML comments.

---

### `MTMFG_Schema_CheckConstraints.csv` ★

Use when:
- Generating enum types for `nchar` flag columns — check constraints reveal the allowed values.
- Example: a check constraint `IN ('Y','N')` on `BACKORDER_FLAG` → map to `bool` with
  `'Y' = true`.

---

### `MTMFG_Schema_ExtendedProperties.csv` ★

Use when:
- The column name is ambiguous (e.g., `ACT_LABOR_COST` vs `EST_LABOR_COST`) — the
  MS_Description will clarify the business meaning.
- Writing XML `<summary>` doc comments on C# model properties.

---

## Common Query Patterns

### Pattern: Look up a purchase order with its lines and vendor

```sql
-- Tables: PURCHASE_ORDER, PURC_ORDER_LINE, VENDOR
-- FK path from PKs/FKs CSVs:
--   PURC_ORDER_LINE.PURC_ORDER_ID → PURCHASE_ORDER.ID
--   PURCHASE_ORDER.VENDOR_ID      → VENDOR.ID
SELECT
    po.ID           AS PurchaseOrderId,
    po.ORDER_DATE,
    v.NAME          AS VendorName,
    pol.LINE_NO,
    pol.PART_ID,
    pol.USER_ORDER_QTY
FROM   PURCHASE_ORDER   po
JOIN   VENDOR           v   ON po.VENDOR_ID     = v.ID
JOIN   PURC_ORDER_LINE  pol ON pol.PURC_ORDER_ID = po.ID
WHERE  po.ID = @purchaseOrderId;
```

### Pattern: Look up receiver lines for a PO

```sql
-- Tables: RECEIVER, RECEIVER_LINE, PURC_ORDER_LINE
-- FK: RECEIVER_LINE.RECEIVER_ID       → RECEIVER.ID
-- FK: RECEIVER_LINE.PURC_ORDER_ID     → PURC_ORDER_LINE.PURC_ORDER_ID
-- FK: RECEIVER_LINE.PURC_ORDER_LINE_NO→ PURC_ORDER_LINE.LINE_NO
SELECT
    r.ID            AS ReceiverId,
    r.RECEIVED_DATE,
    rl.LINE_NO,
    rl.PART_ID,
    rl.RECEIVED_QTY
FROM   RECEIVER         r
JOIN   RECEIVER_LINE    rl  ON rl.RECEIVER_ID            = r.ID
JOIN   PURC_ORDER_LINE  pol ON pol.PURC_ORDER_ID         = rl.PURC_ORDER_ID
                           AND pol.LINE_NO               = rl.PURC_ORDER_LINE_NO
WHERE  rl.PURC_ORDER_ID = @purchaseOrderId;
```

### Pattern: Composite PK lookup (OPERATION example)

```sql
SELECT *
FROM  OPERATION
WHERE WORKORDER_TYPE      = @type
  AND WORKORDER_BASE_ID   = @baseId
  AND WORKORDER_LOT_ID    = @lotId
  AND WORKORDER_SPLIT_ID  = @splitId
  AND WORKORDER_SUB_ID    = @subId
  AND SEQUENCE_NO         = @seqNo;
```

---

## File Inventory

| File | Status | Priority |
|---|---|---|
| `MTMFG_Schema_Tables.csv` | ✅ Generated | Core |
| `MTMFG_Schema_FKs.csv` | ✅ Generated | Core |
| `MTMFG_Schema_PKs.csv` | ✅ Generated | Core |
| `MTMFG_Schema_ColumnDetails.csv` | ⬜ Not yet generated | ★★★ |
| `MTMFG_Schema_Views.csv` | ⬜ Not yet generated | ★★★ |
| `MTMFG_Schema_Indexes.csv` | ⬜ Not yet generated | ★★ |
| `MTMFG_Schema_TableRowCounts.csv` | ⬜ Not yet generated | ★★ |
| `MTMFG_Schema_Triggers.csv` | ⬜ Not yet generated | ★★ |
| `MTMFG_Schema_StoredProcedures.csv` | ⬜ Not yet generated | ★ |
| `MTMFG_Schema_UniqueConstraints.csv` | ⬜ Not yet generated | ★ |
| `MTMFG_Schema_DefaultConstraints.csv` | ⬜ Not yet generated | ★ |
| `MTMFG_Schema_CheckConstraints.csv` | ⬜ Not yet generated | ★ |
| `MTMFG_Schema_ExtendedProperties.csv` | ⬜ Not yet generated | ★ |
| `ADDITIONAL_CSV_RECOMMENDATIONS.md` | ✅ Created | Reference |

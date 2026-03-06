---
applyTo: "**"
description: >
  Infor Visual (MTMFG) SQL Server database reference files — what each CSV file contains,
  how to use them when generating SQL queries or C# models, and the complete verified file
  inventory with known limitations (encrypted definitions, missing headers, V_ naming).
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
- Tables prefixed `V_` are SQL Server **base tables**, not views. The `V_` prefix is an Infor Visual domain naming convention (verified 2026-03-06: `V_ACCOUNT` and similar appear in `sys.tables`, not `sys.views`). The only actual SQL Server views in MTMFG are `CR_PART_LOCATION` and `SYSUSERAUTH` — see `MTMFG_Schema_Views.csv`.

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

## Additional Reference Files

All files below have been generated (2026-03-06). See `ADDITIONAL_CSV_RECOMMENDATIONS.md` for
the SSMS queries used to produce each one.

> **Note:** These files were exported using SSMS "Results to File" and do **not** include a
> header row. Column order is documented under each file below.

### `MTMFG_Schema_ColumnDetails.csv` ★★★

**Columns (no header row):** `TABLE_NAME`, `COLUMN_ORDER`, `COLUMN_NAME`, `DATA_TYPE`,
`MAX_CHAR_LENGTH`, `NUMERIC_PRECISION`, `NUMERIC_SCALE`, `IS_NULLABLE`, `IS_IDENTITY`,
`IS_COMPUTED` — ~14,775 rows

Use when:
- Generating `decimal(18,4)` vs `decimal(10,2)` stored procedure parameters.
- Setting `[MaxLength]` annotations on C# string properties.
- Skipping identity columns (`IS_IDENTITY=YES`) from INSERT parameter lists (MySQL DAOs).

---

### `MTMFG_Schema_Views.csv` ★★★

**Verified 2026-03-06:** MTMFG has only **2 actual SQL Server views**: `CR_PART_LOCATION` and
`SYSUSERAUTH`.

> ⚠️ **CORRECTION — `V_` tables are base tables, not views.** Objects such as `V_ACCOUNT`,
> `V_CONTRACT`, and `V_SRVC_ORDER` appear in `sys.tables` (not `sys.views`). They are standard
> SQL Server base tables; the `V_` prefix is an Infor Visual domain naming convention, not a
> view indicator. Use `MTMFG_Schema_Tables.csv` and `MTMFG_Schema_ColumnDetails.csv` for all
> `V_` prefixed tables.

**Columns (no header row):** `VIEW_SCHEMA`, `VIEW_NAME`, `COLUMN_ORDER`, `COLUMN_NAME`,
`DATA_TYPE`, `MAX_CHAR_LENGTH`, `NUMERIC_PRECISION`, `NUMERIC_SCALE`, `IS_NULLABLE`

Use when: Querying `CR_PART_LOCATION` (part/warehouse location data) or `SYSUSERAUTH`.

---

### `MTMFG_Schema_Indexes.csv` ★★

**Columns (no header row):** `TABLE_NAME`, `INDEX_NAME`, `INDEX_TYPE`, `IS_UNIQUE`,
`IS_PRIMARY_KEY`, `COLUMN_NAME`, `KEY_ORDINAL`, `IS_INCLUDED`

Use when:
- Writing WHERE clauses on large tables — filter on indexed columns first.
- Choosing between equivalent join paths — prefer the one using an indexed FK column.
- Tables known to be large (see RowCounts CSV): `CUST_BOOK_DEL` (~4.8M rows),
  `WIP_ISSUE_DETAIL` (~3.9M), `INVENTORY_TRANS` (~3.5M), `LABOR_TICKET`, `CUSTOMER_ORDER`,
  `PURCHASE_ORDER`, `RECEIVER_LINE`.

---

### `MTMFG_Schema_TableRowCounts.csv` ★★

**Columns (no header row):** `TABLE_NAME`, `APPROX_ROW_COUNT` — ordered largest-first.

Top tables: `CUST_BOOK_DEL` (~4.8M), `WIP_ISSUE_DETAIL` (~3.9M), `INVENTORY_TRANS` (~3.5M).

Use when:
- Deciding whether a full-table query is safe or whether a TOP/WHERE clause is mandatory.
- Documenting expected query performance in DAO method XML comments.

---

### `MTMFG_Schema_Triggers.csv` ★★

**Columns (no header row):** `TABLE_NAME`, `TRIGGER_NAME`, `TRIGGER_TYPE`, `IS_DISABLED`,
`IS_INSTEAD_OF`, `TRIGGER_EVENTS` — 331 rows

Use when:
- Explaining why a Visual record changes automatically after an ERP action (trigger side-effects).
- Identifying tables with heavy trigger activity that should not be queried mid-transaction.

---

### `MTMFG_Schema_StoredProcedures.csv` — ⛔ Not applicable

**Verified 2026-03-06:** MTMFG has no stored procedures and no user-defined functions in SQL
Server (`sys.procedures` and all UDF object types return 0 rows). Infor Visual implements all
business logic in the application tier. This file will never have data — skip it entirely.

---

### `MTMFG_Schema_UniqueConstraints.csv` ★

**Columns (no header row):** `TABLE_NAME`, `CONSTRAINT_NAME`, `COLUMN_NAME`, `KEY_ORDINAL`

Use when:
- Writing lookup queries by business key (e.g., `PART.ID`, `VENDOR.ID`, `CUSTOMER.ID`) —
  unique constraints confirm these are safe single-row lookups.

---

### `MTMFG_Schema_DefaultConstraints.csv` ★

**Columns (no header row):** `TABLE_NAME`, `COLUMN_NAME`, `CONSTRAINT_NAME`,
`DEFAULT_EXPRESSION` — 1,412 rows

> ⚠️ **Limitation:** `DEFAULT_EXPRESSION` is NULL for all rows. Infor Visual stores default
> constraint definitions with encryption, making them inaccessible via
> `sys.default_constraints.definition`. The file confirms *which* columns have defaults but
> not *what* the default values are.

Use when: Knowing which columns have defaults (safe to omit from INSERT test fixture data).

---

### `MTMFG_Schema_CheckConstraints.csv` ★

**Columns (no header row):** `TABLE_NAME`, `CONSTRAINT_NAME`, `CHECK_DEFINITION` — 117 rows

> ⚠️ **Limitation:** `CHECK_DEFINITION` is NULL for all rows. Infor Visual stores check
> constraint definitions with encryption, making them inaccessible via
> `sys.check_constraints.definition`. The file confirms *which* tables have check constraints
> but cannot reveal allowed values for enum mapping.

Use when: Knowing which tables enforce check constraints — but do not rely on this file to
derive enum values or flag domains.

---

### `MTMFG_Schema_ExtendedProperties.csv` — ⛔ Not applicable

**Verified 2026-03-06:** MTMFG has no `MS_Description` extended properties on any tables or
columns (`sys.extended_properties` returns 0 rows for `ep.class = 1`). This file will never
have data — skip it entirely.

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
| `MTMFG_Schema_ColumnDetails.csv` | ✅ Generated — no header row, ~14,775 rows | ★★★ |
| `MTMFG_Schema_Views.csv` | ✅ Generated — 2 views only; V_ tables are base tables, not views | ★★★ |
| `MTMFG_Schema_Indexes.csv` | ✅ Generated — no header row | ★★ |
| `MTMFG_Schema_TableRowCounts.csv` | ✅ Generated — no header row | ★★ |
| `MTMFG_Schema_Triggers.csv` | ✅ Generated — no header row, 331 rows | ★★ |
| `MTMFG_Schema_StoredProcedures.csv` | ⛔ Not applicable — MTMFG has no SPs or UDFs | N/A |
| `MTMFG_Schema_UniqueConstraints.csv` | ✅ Generated — no header row | ★ |
| `MTMFG_Schema_DefaultConstraints.csv` | ✅ Generated — DEFAULT_EXPRESSION all NULL (encrypted) | ★ |
| `MTMFG_Schema_CheckConstraints.csv` | ✅ Generated — CHECK_DEFINITION all NULL (encrypted) | ★ |
| `MTMFG_Schema_ExtendedProperties.csv` | ⛔ Not applicable — no MS_Description properties | N/A |
| `ADDITIONAL_CSV_RECOMMENDATIONS.md` | ✅ Created | Reference |

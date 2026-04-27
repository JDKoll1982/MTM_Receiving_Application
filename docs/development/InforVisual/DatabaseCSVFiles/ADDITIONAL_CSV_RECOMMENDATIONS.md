# Additional CSV Reference Files for Infor Visual (MTMFG) Database

This document lists recommended CSV files to generate from **SQL Server Management Studio (SSMS)**
connected to the MTMFG Infor Visual database. Each file fills a gap in what the three existing
reference files provide, and each includes a copy-paste SQL query to run in SSMS.

---

## Existing Files (Summary)

| File                      | Purpose                                            |
| ------------------------- | -------------------------------------------------- |
| `MTMFG_Schema_Tables.csv` | All tables — column names, data types, nullability |
| `MTMFG_Schema_FKs.csv`    | All foreign key relationships                      |
| `MTMFG_Schema_PKs.csv`    | All primary key column compositions                |

---

## Recommended Additional Files

### 1. `MTMFG_Schema_ColumnDetails.csv`

**Why:** The Tables CSV only shows `nvarchar`, `decimal`, `int` — it omits string lengths,
numeric precision/scale, identity flags, and computed column flags. This detail is required to
write accurate C# model properties and stored procedure parameters.

**SSMS Query:**

```sql
SELECT
    t.name                                                                  AS TABLE_NAME,
    c.column_id                                                             AS COLUMN_ORDER,
    c.name                                                                  AS COLUMN_NAME,
    ty.name                                                                 AS DATA_TYPE,
    CASE
        WHEN ty.name IN ('nvarchar', 'nchar') AND c.max_length = -1 THEN 'MAX'
        WHEN ty.name IN ('nvarchar', 'nchar')                        THEN CAST(c.max_length / 2 AS VARCHAR)
        WHEN ty.name IN ('varchar',  'char')  AND c.max_length = -1 THEN 'MAX'
        WHEN ty.name IN ('varchar',  'char')                         THEN CAST(c.max_length AS VARCHAR)
        ELSE NULL
    END                                                                     AS MAX_CHAR_LENGTH,
    c.precision                                                             AS NUMERIC_PRECISION,
    c.scale                                                                 AS NUMERIC_SCALE,
    CASE c.is_nullable  WHEN 1 THEN 'YES' ELSE 'NO' END                    AS IS_NULLABLE,
    CASE c.is_identity  WHEN 1 THEN 'YES' ELSE 'NO' END                    AS IS_IDENTITY,
    CASE c.is_computed  WHEN 1 THEN 'YES' ELSE 'NO' END                    AS IS_COMPUTED
FROM sys.tables t
JOIN sys.columns c  ON t.object_id = c.object_id
JOIN sys.types  ty  ON c.user_type_id = ty.user_type_id
ORDER BY t.name, c.column_id;
```

---

### 2. `MTMFG_Schema_Views.csv`

**Why:** Infor Visual exposes many business objects through views (all `V_` prefix tables in the
Tables CSV are actually views). Knowing view column definitions lets Copilot write correct SELECT
queries against them rather than guessing from table names.

**SSMS Query:**

```sql
SELECT
    SCHEMA_NAME(v.schema_id)                            AS VIEW_SCHEMA,
    v.name                                              AS VIEW_NAME,
    c.column_id                                         AS COLUMN_ORDER,
    c.name                                              AS COLUMN_NAME,
    ty.name                                             AS DATA_TYPE,
    CASE
        WHEN ty.name IN ('nvarchar','nchar') AND c.max_length = -1 THEN 'MAX'
        WHEN ty.name IN ('nvarchar','nchar')                        THEN CAST(c.max_length / 2 AS VARCHAR)
        ELSE NULL
    END                                                 AS MAX_CHAR_LENGTH,
    c.precision                                         AS NUMERIC_PRECISION,
    c.scale                                             AS NUMERIC_SCALE,
    CASE c.is_nullable WHEN 1 THEN 'YES' ELSE 'NO' END  AS IS_NULLABLE
FROM sys.views    v
JOIN sys.columns  c  ON v.object_id = c.object_id
JOIN sys.types    ty ON c.user_type_id = ty.user_type_id
ORDER BY v.name, c.column_id;
```

---

### 3. `MTMFG_Schema_StoredProcedures.csv` — ⛔ SKIP: Not applicable

**Verified (2026-03-06):** MTMFG has **no user-defined stored procedures and no user-defined
functions** in SQL Server. Both `sys.procedures` and `sys.objects WHERE type IN ('P','FN','IF','TF')`
return 0 rows. Infor Visual implements all business logic in the application tier, not the database
tier. **Do not generate this file — it will always be empty.**

---

### 4. `MTMFG_Schema_Indexes.csv`

**Why:** Knowing which columns are indexed on large tables (e.g., `PURCHASE_ORDER`,
`CUSTOMER_ORDER`, `INVENTORY_TRANS`) lets Copilot write WHERE clauses that use indexed paths,
avoiding full-table scans in read-only query DAOs.

**SSMS Query:**

```sql
SELECT
    t.name                                              AS TABLE_NAME,
    i.name                                              AS INDEX_NAME,
    i.type_desc                                         AS INDEX_TYPE,
    CASE i.is_unique        WHEN 1 THEN 'YES' ELSE 'NO' END AS IS_UNIQUE,
    CASE i.is_primary_key   WHEN 1 THEN 'YES' ELSE 'NO' END AS IS_PRIMARY_KEY,
    c.name                                              AS COLUMN_NAME,
    ic.key_ordinal                                      AS KEY_ORDINAL,
    CASE ic.is_included_column WHEN 1 THEN 'YES' ELSE 'NO' END AS IS_INCLUDED
FROM sys.tables          t
JOIN sys.indexes         i  ON t.object_id = i.object_id
JOIN sys.index_columns   ic ON i.object_id  = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns         c  ON ic.object_id = c.object_id  AND ic.column_id = c.column_id
WHERE i.name IS NOT NULL
ORDER BY t.name, i.name, ic.key_ordinal;
```

---

### 5. `MTMFG_Schema_TableRowCounts.csv`

**Why:** Knowing row counts helps prioritise filtering strategies in DAO queries. A table with
10 rows can be fully scanned; a table with 10 million rows needs tight WHERE clauses and indexed
columns. This file is a one-time snapshot — regenerate after major data migrations.

**SSMS Query:**

```sql
SELECT
    t.name                              AS TABLE_NAME,
    SUM(p.rows)                         AS APPROX_ROW_COUNT
FROM sys.tables    t
JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)  -- heap or clustered index only (avoids double-counting)
GROUP BY t.name
ORDER BY SUM(p.rows) DESC;
```

---

### 6. `MTMFG_Schema_UniqueConstraints.csv`

**Why:** Beyond PKs, many Infor Visual tables enforce uniqueness on business-key columns (e.g.,
`PART.ID`, `VENDOR.ID`). Knowing these prevents Copilot from generating duplicate-check logic that
is already enforced at the database layer, and reveals natural lookup keys for queries.

**SSMS Query:**

```sql
SELECT
    t.name                                              AS TABLE_NAME,
    i.name                                              AS CONSTRAINT_NAME,
    c.name                                              AS COLUMN_NAME,
    ic.key_ordinal                                      AS KEY_ORDINAL
FROM sys.tables          t
JOIN sys.indexes         i  ON t.object_id = i.object_id
JOIN sys.index_columns   ic ON i.object_id  = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns         c  ON ic.object_id = c.object_id  AND ic.column_id = c.column_id
WHERE i.is_unique = 1 AND i.is_primary_key = 0
ORDER BY t.name, i.name, ic.key_ordinal;
```

---

### 7. `MTMFG_Schema_Triggers.csv`

**Why:** Infor Visual uses database triggers extensively to enforce business rules and maintain
denormalized summary fields. Knowing which tables have triggers prevents Copilot from suggesting
direct writes (which are forbidden) and helps document side-effects when explaining what happens
when a record is updated from the ERP side.

**SSMS Query:**

```sql
SELECT
    OBJECT_NAME(tr.parent_id)                               AS TABLE_NAME,
    tr.name                                                 AS TRIGGER_NAME,
    tr.type_desc                                            AS TRIGGER_TYPE,
    CASE tr.is_disabled         WHEN 1 THEN 'YES' ELSE 'NO' END AS IS_DISABLED,
    CASE tr.is_instead_of_trigger WHEN 1 THEN 'YES' ELSE 'NO' END AS IS_INSTEAD_OF,
    STUFF((
        SELECT ', ' + te.type_desc
        FROM sys.trigger_events te
        WHERE te.object_id = tr.object_id
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '')                AS TRIGGER_EVENTS
FROM sys.triggers tr
WHERE tr.parent_class = 1   -- table triggers only
ORDER BY OBJECT_NAME(tr.parent_id), tr.name;
```

---

### 8. `MTMFG_Schema_DefaultConstraints.csv`

**Why:** Default values on columns affect what is safe to omit from INSERT statements (which
only apply to the MySQL database in this project, but are useful reference when understanding
Infor Visual data patterns and when writing test fixture data).

**SSMS Query:**

```sql
SELECT
    t.name          AS TABLE_NAME,
    c.name          AS COLUMN_NAME,
    dc.name         AS CONSTRAINT_NAME,
    dc.definition   AS DEFAULT_EXPRESSION
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
JOIN sys.tables  t ON dc.parent_object_id = t.object_id
ORDER BY t.name, c.name;
```

---

### 9. `MTMFG_Schema_CheckConstraints.csv`

**Why:** Check constraints encode Infor Visual's business-rule validation at the database level
(e.g., status codes, flag values `'Y'`/`'N'`). This helps Copilot generate correct enum mappings
and validation logic without reverse-engineering values from sample data.

**SSMS Query:**

```sql
SELECT
    t.name      AS TABLE_NAME,
    cc.name     AS CONSTRAINT_NAME,
    cc.definition AS CHECK_DEFINITION
FROM sys.check_constraints cc
JOIN sys.tables t ON cc.parent_object_id = t.object_id
ORDER BY t.name, cc.name;
```

---

### 10. `MTMFG_Schema_ExtendedProperties.csv` — ⛔ SKIP: Not applicable

**Verified (2026-03-06):** MTMFG has **no `MS_Description` extended properties** on any tables
or columns. `sys.extended_properties` returns 0 rows for `ep.class = 1`. This MTMFG installation
was not configured with schema descriptions. **Do not generate this file — it will always be empty.**

---

## How to Export from SSMS to CSV

1. Open SSMS and connect to the MTMFG SQL Server instance.
2. Open a new query window and paste the SQL above.
3. In the **Query** menu → **Results To** → **Results to File** (or press `Ctrl+Shift+F`).
4. Execute the query (`F5`). SSMS will prompt for a save location.
5. Save as `.csv` in `docs/InforVisual/DatabaseCSVFiles/`.
6. Open the file and verify the first row contains column headers.

> **Tip:** Use **Query → Query Options → Results → Text** and set **Output format** to
> **Comma Delimited** before running, or use **Results to Grid** → right-click → **Save Results As**
> and choose CSV.

---

## Priority Order

| Priority | File                                  | Reason                                                 |
| -------- | ------------------------------------- | ------------------------------------------------------ |
| ★★★      | `MTMFG_Schema_ColumnDetails.csv`      | Required for accurate C# model generation              |
| ★★★      | `MTMFG_Schema_Views.csv`              | Required for `V_` prefix query DAOs                    |
| ★★       | `MTMFG_Schema_Indexes.csv`            | Query performance guidance                             |
| ★★       | `MTMFG_Schema_TableRowCounts.csv`     | Query strategy decisions                               |
| ★★       | `MTMFG_Schema_Triggers.csv`           | Understand ERP side-effect behaviour                   |
| ⛔       | `MTMFG_Schema_StoredProcedures.csv`   | Not applicable — no SPs or UDFs in MTMFG               |
| ★        | `MTMFG_Schema_UniqueConstraints.csv`  | Natural key / lookup guidance                          |
| ★        | `MTMFG_Schema_DefaultConstraints.csv` | Fixture data / model defaults                          |
| ★        | `MTMFG_Schema_CheckConstraints.csv`   | Enum / validation mapping                              |
| ⛔       | `MTMFG_Schema_ExtendedProperties.csv` | Not applicable — no MS_Description properties in MTMFG |

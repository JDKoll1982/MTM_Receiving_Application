# Additional CSV Recommendations for Infor Visual (MTMFG) Research

This document replaces the older "generate the missing core schema files" guidance. The core
schema pack has already been generated in this folder. The goal of this document is now narrower:
recommend additional CSV exports that a normal read-only Visual SQL user can pull to accelerate
future research, especially when the base schema exports do not fully explain status semantics,
bridge-table behavior, or chronology rules.

---

## Current State of the Core Reference Pack

The following files already exist in this folder and should be treated as the baseline schema pack:

| File | Status | Primary Use |
| --- | --- | --- |
| `MTMFG_Schema_Tables.csv` | Present | Table and column inventory |
| `MTMFG_Schema_FKs.csv` | Present | Declared foreign key paths |
| `MTMFG_Schema_PKs.csv` | Present | Primary key composition |
| `MTMFG_Schema_ColumnDetails.csv` | Present | Length, precision, identity, computed flags |
| `MTMFG_Schema_Views.csv` | Present | Actual SQL Server views |
| `MTMFG_Schema_Indexes.csv` | Present | Index coverage and query planning |
| `MTMFG_Schema_TableRowCounts.csv` | Present | Table size and scale awareness |
| `MTMFG_Schema_UniqueConstraints.csv` | Present | Natural/business key discovery |
| `MTMFG_Schema_Triggers.csv` | Present | ERP-side table-trigger visibility |
| `MTMFG_Schema_DefaultConstraints.csv` | Present | Default-presence inventory |
| `MTMFG_Schema_CheckConstraints.csv` | Present | Check-constraint presence inventory |

### Verified Corrections From Current Research

- Do not treat every `V_`-prefixed object as a SQL Server view. The current exports show that many
  `V_` objects are base tables.
- The current work-order and customer-order investigation relied on base tables such as
  `CUSTOMER_ORDER`, `CUST_ORDER_LINE`, `CUST_ORDER_ALLOC`, `CO_PRODUCT`, and `WORK_ORDER`.
- The existing constraint exports confirm which constraints exist, but many definitions are `NULL`,
  so they do not fully decode status semantics.
- `MTMFG_Schema_StoredProcedures.csv` and `MTMFG_Schema_ExtendedProperties.csv` remain unnecessary
  in this environment unless the server configuration changes.

---

## Non-Admin Export Guidelines

The recommendations below are designed for a non-admin, read-only research workflow.

- Prefer plain `SELECT` queries against visible application tables.
- Prefer `INFORMATION_SCHEMA.COLUMNS` for column inventory queries when possible.
- Avoid DMVs that require `VIEW DATABASE STATE`.
- Avoid assumptions that `sys.sql_modules`, object definitions, or encrypted constraint text will be visible.
- If metadata visibility is restricted, the export will still be useful, but it will reflect only the
  objects the current account can see.
- Sample-data exports are intended for internal research only. Trim customer-identifying columns if a
  CSV needs to be shared more broadly.

---

## Recommended Additional Files

### 1. `MTMFG_Research_StatusValueProfiles.csv`

**Why:** The existing check/default constraint exports prove that status columns exist, but they do
not reveal the actual values in use. This export helps decode what the application stores in key
status fields without requiring admin access.

**Query:**

```sql
SELECT 'CUST_ORDER_LINE' AS TABLE_NAME, 'LINE_STATUS' AS COLUMN_NAME,
       CAST(LINE_STATUS AS NVARCHAR(100)) AS COLUMN_VALUE,
       COUNT(*) AS ROW_COUNT
FROM dbo.CUST_ORDER_LINE
GROUP BY LINE_STATUS

UNION ALL

SELECT 'CUST_ORDER_LINE', 'SHIP_ORDER_STATUS',
       CAST(SHIP_ORDER_STATUS AS NVARCHAR(100)),
       COUNT(*)
FROM dbo.CUST_ORDER_LINE
GROUP BY SHIP_ORDER_STATUS

UNION ALL

SELECT 'CO_PRODUCT', 'LINE_STATUS',
       CAST(LINE_STATUS AS NVARCHAR(100)),
       COUNT(*)
FROM dbo.CO_PRODUCT
GROUP BY LINE_STATUS

UNION ALL

SELECT 'WORK_ORDER', 'STATUS',
       CAST(STATUS AS NVARCHAR(100)),
       COUNT(*)
FROM dbo.WORK_ORDER
GROUP BY STATUS

ORDER BY TABLE_NAME, COLUMN_NAME, COLUMN_VALUE;
```

---

### 2. `MTMFG_Research_CustOrderAlloc_TypeProfiles.csv`

**Why:** `CUST_ORDER_ALLOC` is currently the strongest candidate bridge between customer-order
lines and supply-side records, but its type fields need data-level interpretation. This export helps
explain how `DEMAND_TYPE` and `SUPPLY_TYPE` are actually used.

**Query:**

```sql
SELECT
    DEMAND_TYPE,
    SUPPLY_TYPE,
    COUNT(*) AS ROW_COUNT,
    COUNT(DISTINCT ORDER_ID) AS DISTINCT_ORDER_IDS,
    COUNT(DISTINCT CONCAT(ORDER_ID, '|', ORDER_LINE_NO, '|', ORDER_DEL_NO)) AS DISTINCT_ORDER_LINES,
    COUNT(DISTINCT CONCAT(BASE_ID, '|', LOT_ID, '|', SPLIT_ID)) AS DISTINCT_SUPPLY_KEYS,
    MIN(WANT_DATE) AS MIN_WANT_DATE,
    MAX(WANT_DATE) AS MAX_WANT_DATE,
    MIN(FINISH_DATE) AS MIN_FINISH_DATE,
    MAX(FINISH_DATE) AS MAX_FINISH_DATE
FROM dbo.CUST_ORDER_ALLOC
GROUP BY DEMAND_TYPE, SUPPLY_TYPE
ORDER BY DEMAND_TYPE, SUPPLY_TYPE;
```

---

### 3. `MTMFG_Research_CustOrderAlloc_Samples.csv`

**Why:** The schema proves that `CUST_ORDER_ALLOC` stores both demand-side and supply-side keys,
but a sample export is the fastest way to inspect real row shape and validate whether the table is
usable for future CO-line-to-supply research.

**Query:**

```sql
SELECT TOP (1000)
    SITE_ID,
    CUST_ID,
    ORDER_ID,
    ORDER_LINE_NO,
    ORDER_DEL_NO,
    DEMAND_KEY,
    DEMAND_TYPE,
    SUPPLY_KEY,
    SUPPLY_TYPE,
    BASE_ID,
    LOT_ID,
    SPLIT_ID,
    PART_ID,
    WANT_DATE,
    FINISH_DATE,
    ORDER_QTY,
    SHIPPED_QTY,
    DESIRED_QTY,
    RECEIVED_QTY,
    ALLOCATED_QTY
FROM dbo.CUST_ORDER_ALLOC
ORDER BY WANT_DATE DESC, ORDER_ID, ORDER_LINE_NO;
```

---

### 4. `MTMFG_Research_WorkOrderCustomerOrderCoverage.csv`

**Why:** `WORK_ORDER.WBS_CUST_ORDER_ID` is one of the few visible columns suggesting a
customer-order relationship from the work-order side. This export measures how often that field is
present and whether it actually matches a `CUSTOMER_ORDER.ID`.

**Query:**

```sql
SELECT
    CASE
        WHEN w.WBS_CUST_ORDER_ID IS NULL OR LTRIM(RTRIM(w.WBS_CUST_ORDER_ID)) = '' THEN 'NULL_OR_BLANK'
        WHEN co.ID IS NULL THEN 'NO_CUSTOMER_ORDER_MATCH'
        ELSE 'MATCHES_CUSTOMER_ORDER'
    END AS MATCH_BUCKET,
    COUNT(*) AS WORK_ORDER_COUNT,
    COUNT(DISTINCT w.WBS_CUST_ORDER_ID) AS DISTINCT_WBS_CUST_ORDER_IDS,
    MIN(w.CREATE_DATE) AS MIN_WORK_ORDER_CREATE_DATE,
    MAX(w.CREATE_DATE) AS MAX_WORK_ORDER_CREATE_DATE
FROM dbo.WORK_ORDER w
LEFT JOIN dbo.CUSTOMER_ORDER co ON co.ID = w.WBS_CUST_ORDER_ID
GROUP BY
    CASE
        WHEN w.WBS_CUST_ORDER_ID IS NULL OR LTRIM(RTRIM(w.WBS_CUST_ORDER_ID)) = '' THEN 'NULL_OR_BLANK'
        WHEN co.ID IS NULL THEN 'NO_CUSTOMER_ORDER_MATCH'
        ELSE 'MATCHES_CUSTOMER_ORDER'
    END
ORDER BY MATCH_BUCKET;
```

---

### 5. `MTMFG_Research_TargetColumnInventory.csv`

**Why:** When declared FKs are missing, research often becomes a column-name hunt. This export
pulls the most important bridge, chronology, and status columns into one CSV using
`INFORMATION_SCHEMA`, which is usually safer for non-admin users than heavier system catalog work.

**Query:**

```sql
SELECT
    TABLE_SCHEMA,
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME IN (
    'CUST_ORDER_ID',
    'LINE_NO',
    'ORDER_ID',
    'ORDER_LINE_NO',
    'ORDER_DEL_NO',
    'WBS_CUST_ORDER_ID',
    'WORKORDER_TYPE',
    'WORKORDER_BASE_ID',
    'WORKORDER_LOT_ID',
    'WORKORDER_SPLIT_ID',
    'WORKORDER_SUB_ID',
    'TYPE',
    'BASE_ID',
    'LOT_ID',
    'SPLIT_ID',
    'SUB_ID',
    'LINE_STATUS',
    'SHIP_ORDER_STATUS',
    'STATUS',
    'CREATE_DATE',
    'ORDER_DATE',
    'DESIRED_SHIP_DATE',
    'PROMISE_DATE',
    'STATUS_EFF_DATE',
    'WANT_DATE',
    'FINISH_DATE',
    'ALLOCATED_QTY',
    'FULFILLED_QTY',
    'TOTAL_SHIPPED_QTY',
    'RECEIVED_QTY'
)
ORDER BY COLUMN_NAME, TABLE_SCHEMA, TABLE_NAME;
```

---

### 6. `MTMFG_Research_BridgeColumnPopulation.csv`

**Why:** A bridge column is only useful if it is actually populated. This export measures how
often the key relationship and chronology columns are present in the tables that matter most to
customer-order and work-order research.

**Query:**

```sql
SELECT 'CUST_ORDER_ALLOC' AS TABLE_NAME, 'ORDER_ID' AS COLUMN_NAME,
       COUNT(*) AS TOTAL_ROWS,
       SUM(CASE WHEN ORDER_ID IS NULL OR LTRIM(RTRIM(ORDER_ID)) = '' THEN 0 ELSE 1 END) AS POPULATED_ROWS
FROM dbo.CUST_ORDER_ALLOC

UNION ALL

SELECT 'CUST_ORDER_ALLOC', 'ORDER_LINE_NO', COUNT(*),
       SUM(CASE WHEN ORDER_LINE_NO IS NULL THEN 0 ELSE 1 END)
FROM dbo.CUST_ORDER_ALLOC

UNION ALL

SELECT 'CUST_ORDER_ALLOC', 'BASE_ID', COUNT(*),
       SUM(CASE WHEN BASE_ID IS NULL OR LTRIM(RTRIM(BASE_ID)) = '' THEN 0 ELSE 1 END)
FROM dbo.CUST_ORDER_ALLOC

UNION ALL

SELECT 'CUST_ORDER_ALLOC', 'LOT_ID', COUNT(*),
       SUM(CASE WHEN LOT_ID IS NULL OR LTRIM(RTRIM(LOT_ID)) = '' THEN 0 ELSE 1 END)
FROM dbo.CUST_ORDER_ALLOC

UNION ALL

SELECT 'CUST_ORDER_ALLOC', 'SPLIT_ID', COUNT(*),
       SUM(CASE WHEN SPLIT_ID IS NULL OR LTRIM(RTRIM(SPLIT_ID)) = '' THEN 0 ELSE 1 END)
FROM dbo.CUST_ORDER_ALLOC

UNION ALL

SELECT 'WORK_ORDER', 'WBS_CUST_ORDER_ID', COUNT(*),
       SUM(CASE WHEN WBS_CUST_ORDER_ID IS NULL OR LTRIM(RTRIM(WBS_CUST_ORDER_ID)) = '' THEN 0 ELSE 1 END)
FROM dbo.WORK_ORDER

UNION ALL

SELECT 'CO_PRODUCT', 'WORKORDER_TYPE', COUNT(*),
       SUM(CASE WHEN WORKORDER_TYPE IS NULL OR LTRIM(RTRIM(CAST(WORKORDER_TYPE AS NVARCHAR(100)))) = '' THEN 0 ELSE 1 END)
FROM dbo.CO_PRODUCT

ORDER BY TABLE_NAME, COLUMN_NAME;
```

---

### 7. `MTMFG_Research_DateFieldRanges.csv`

**Why:** The biggest chronology gap in the recent CO-line fulfillment research was the lack of a
clearly authoritative `Oldest Added` field. This export profiles the most likely date candidates so
future investigations can see which fields are populated and what time ranges they cover.

**Query:**

```sql
SELECT 'CUSTOMER_ORDER' AS TABLE_NAME, 'CREATE_DATE' AS COLUMN_NAME,
       COUNT(*) AS TOTAL_ROWS,
       SUM(CASE WHEN CREATE_DATE IS NULL THEN 0 ELSE 1 END) AS POPULATED_ROWS,
       MIN(CREATE_DATE) AS MIN_VALUE,
       MAX(CREATE_DATE) AS MAX_VALUE
FROM dbo.CUSTOMER_ORDER

UNION ALL

SELECT 'CUSTOMER_ORDER', 'ORDER_DATE', COUNT(*),
       SUM(CASE WHEN ORDER_DATE IS NULL THEN 0 ELSE 1 END),
       MIN(ORDER_DATE), MAX(ORDER_DATE)
FROM dbo.CUSTOMER_ORDER

UNION ALL

SELECT 'CUST_ORDER_LINE', 'DESIRED_SHIP_DATE', COUNT(*),
       SUM(CASE WHEN DESIRED_SHIP_DATE IS NULL THEN 0 ELSE 1 END),
       MIN(DESIRED_SHIP_DATE), MAX(DESIRED_SHIP_DATE)
FROM dbo.CUST_ORDER_LINE

UNION ALL

SELECT 'CUST_ORDER_LINE', 'PROMISE_DATE', COUNT(*),
       SUM(CASE WHEN PROMISE_DATE IS NULL THEN 0 ELSE 1 END),
       MIN(PROMISE_DATE), MAX(PROMISE_DATE)
FROM dbo.CUST_ORDER_LINE

UNION ALL

SELECT 'CUST_ORDER_LINE', 'STATUS_EFF_DATE', COUNT(*),
       SUM(CASE WHEN STATUS_EFF_DATE IS NULL THEN 0 ELSE 1 END),
       MIN(STATUS_EFF_DATE), MAX(STATUS_EFF_DATE)
FROM dbo.CUST_ORDER_LINE

UNION ALL

SELECT 'CUST_ORDER_ALLOC', 'WANT_DATE', COUNT(*),
       SUM(CASE WHEN WANT_DATE IS NULL THEN 0 ELSE 1 END),
       MIN(WANT_DATE), MAX(WANT_DATE)
FROM dbo.CUST_ORDER_ALLOC

UNION ALL

SELECT 'CUST_ORDER_ALLOC', 'FINISH_DATE', COUNT(*),
       SUM(CASE WHEN FINISH_DATE IS NULL THEN 0 ELSE 1 END),
       MIN(FINISH_DATE), MAX(FINISH_DATE)
FROM dbo.CUST_ORDER_ALLOC

UNION ALL

SELECT 'WORK_ORDER', 'CREATE_DATE', COUNT(*),
       SUM(CASE WHEN CREATE_DATE IS NULL THEN 0 ELSE 1 END),
       MIN(CREATE_DATE), MAX(CREATE_DATE)
FROM dbo.WORK_ORDER

ORDER BY TABLE_NAME, COLUMN_NAME;
```

---

## How to Export from SSMS to CSV

1. Open SSMS and connect with the normal read-only Visual SQL account.
2. Open a new query window and paste one query at a time.
3. Use **Query -> Results To -> Results to File** or **Results to Grid** and save as CSV.
4. Save the file into this folder with the exact recommended name.
5. Keep the export date in the commit message or a nearby note if the data profile is likely to age.

> Tip: For data-profile exports, regenerate when investigating a new domain or when business users
> suspect the operational meaning of a field has changed over time.

---

## Priority Order

| Priority | File | Reason |
| --- | --- | --- |
| ★★★ | `MTMFG_Research_StatusValueProfiles.csv` | Fastest path to decoding undocumented status fields |
| ★★★ | `MTMFG_Research_CustOrderAlloc_TypeProfiles.csv` | Highest-value bridge-table semantics for CO-to-supply research |
| ★★★ | `MTMFG_Research_CustOrderAlloc_Samples.csv` | Direct inspection of real bridge rows |
| ★★ | `MTMFG_Research_WorkOrderCustomerOrderCoverage.csv` | Tests whether `WBS_CUST_ORDER_ID` is a usable link |
| ★★ | `MTMFG_Research_TargetColumnInventory.csv` | Speeds up future missing-FK relationship discovery |
| ★★ | `MTMFG_Research_BridgeColumnPopulation.csv` | Validates whether bridge fields are populated enough to trust |
| ★★ | `MTMFG_Research_DateFieldRanges.csv` | Helps settle chronology questions such as `Oldest Added` |

---

## Summary

The core schema export set is already complete for this repository. The next useful exports are not
more generic metadata dumps; they are targeted research extracts that explain how Visual actually
uses status fields, demand/supply bridge rows, chronology columns, and customer-order/work-order
cross-reference fields under a normal read-only account.

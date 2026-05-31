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
- When a query is meant to show row shape or provide a representative sample, prefer `DISTINCT` so
  repeated identical rows do not drown out the useful structure.
- When a query is explicitly measuring row counts, population, or frequency, keep duplicates if they
  are part of the metric being measured.

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
usable for future CO-line-to-supply research. Exclude rows with null or blank `CUST_ID` values so
the sample stays focused on customer-demand rows rather than supply-only records. Use `DISTINCT`
so identical repeated bridge rows do not dominate the sample.

**Query:**

```sql
SELECT DISTINCT TOP (1000)
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
WHERE CUST_ID IS NOT NULL
  AND LTRIM(RTRIM(CUST_ID)) <> ''
ORDER BY WANT_DATE DESC, ORDER_ID, ORDER_LINE_NO;
```

**Interpretation note (2026-05-31):** Live rows indicate `SUPPLY_KEY` is an encoded supply-side
token such as `WO~WO-071133~1~0~0~0~1094.397.033` or `I~002~*~*~0~0~A17-17987-000`. It is useful
for understanding row shape, but future joins should prefer the structured columns already broken
out on `CUST_ORDER_ALLOC` such as `SUPPLY_TYPE`, `BASE_ID`, `LOT_ID`, and `SPLIT_ID` instead of
depending on string parsing.

---

### 4. `MTMFG_Research_WorkOrderCustomerOrderCoverage.csv`

**Why:** The original `WORK_ORDER.WBS_CUST_ORDER_ID` coverage test showed that column is not
populated in MTMFG. This replacement export tests the stronger `CUST_ORDER_ALLOC -> WORK_ORDER`
bridge by using `SUPPLY_TYPE = 'WO'` plus the structured work-order key columns already broken out
on `CUST_ORDER_ALLOC`. The goal is to confirm whether customer-order demand rows can be linked to
real work orders without depending on `SUPPLY_KEY` string parsing.

**Query:**

```sql
SELECT TOP (1000)
    coa.ORDER_ID AS CUSTOMER_ORDER_ID,
    coa.ORDER_LINE_NO,
    coa.ORDER_DEL_NO,
    coa.DEMAND_TYPE,
    coa.SUPPLY_TYPE,
    coa.SUPPLY_KEY,
    coa.BASE_ID AS WORKORDER_BASE_ID,
    coa.LOT_ID AS WORKORDER_LOT_ID,
    coa.SPLIT_ID AS WORKORDER_SPLIT_ID,
    wo.SUB_ID AS WORKORDER_SUB_ID,
    wo.TYPE AS WORKORDER_TYPE,
    wo.PART_ID AS WORKORDER_PART_ID,
    coa.PART_ID AS DEMAND_PART_ID,
    coa.ALLOCATED_QTY,
    coa.WANT_DATE,
    coa.FINISH_DATE
FROM dbo.CUST_ORDER_ALLOC coa
LEFT JOIN dbo.WORK_ORDER wo
    ON wo.TYPE = 'W'
   AND wo.BASE_ID = coa.BASE_ID
   AND wo.LOT_ID = coa.LOT_ID
   AND wo.SPLIT_ID = coa.SPLIT_ID
WHERE coa.DEMAND_TYPE IN ('CD', 'CO')
  AND coa.SUPPLY_TYPE = 'WO'
  AND coa.ORDER_ID IS NOT NULL
  AND LTRIM(RTRIM(coa.ORDER_ID)) <> ''
ORDER BY coa.WANT_DATE DESC, coa.ORDER_ID, coa.ORDER_LINE_NO, coa.ORDER_DEL_NO;
```

**Current finding (2026-05-31):** Live rows confirmed usable customer-order to work-order links,
including direct `CO -> WO` rows such as `CO-085550 -> CO-085550` and `CD -> WO` rows such as
`CO-087117 -> WO-071133`. In the current MTMFG environment, treat `CUST_ORDER_ALLOC` as the best
verified customer-order/work-order bridge for FEATURE-02 / FEATURE-07. Keep
`WORK_ORDER.WBS_CUST_ORDER_ID` classified separately as non-populated and not viable for joins.

---

### 5. `MTMFG_Research_TargetColumnInventory.csv`

**Why:** When declared FKs are missing, research often becomes a column-name hunt. This export
pulls the most important bridge, chronology, and status columns into one CSV using
`INFORMATION_SCHEMA`, which is usually safer for non-admin users than heavier system catalog work.
Use `DISTINCT` as a guard so the export stays one row per visible table/column definition.

**Query:**

```sql
SELECT DISTINCT
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

### 8. `MTMFG_Research_WorkOrderOperationStatusMatrix.csv`

**Why:** FEATURE-02 and FEATURE-07 both depend on a defensible definition of an "active"
work-order sequence. The current plan assumes filters such as `WORK_ORDER.STATUS IN ('O','R')`
and `OPERATION.STATUS IN ('O','R','S')`, but the live export already shows additional values
such as `U` and `X`. The schema exports also do not show a dedicated lookup table or foreign key
that explains `WORK_ORDER.STATUS` or `OPERATION.STATUS`, so the matrix alone is not enough to
name what each code means.

**Query:**

```sql
SELECT
  CAST(wo.STATUS AS NVARCHAR(100)) AS WORK_ORDER_STATUS,
  CAST(op.STATUS AS NVARCHAR(100)) AS OPERATION_STATUS,
  COUNT(*) AS ROW_COUNT,
  COUNT(DISTINCT wo.BASE_ID) AS DISTINCT_WORK_ORDERS,
  COUNT(DISTINCT CONCAT(op.WORKORDER_BASE_ID, '|', CAST(op.SEQUENCE_NO AS NVARCHAR(50)))) AS DISTINCT_SEQUENCES,
  COUNT(DISTINCT op.RESOURCE_ID) AS DISTINCT_RESOURCES
FROM dbo.OPERATION op
INNER JOIN dbo.WORK_ORDER wo
  ON wo.BASE_ID = op.WORKORDER_BASE_ID
   AND wo.TYPE = op.WORKORDER_TYPE
WHERE op.WORKORDER_TYPE = 'W'
  AND wo.TYPE = 'W'
GROUP BY wo.STATUS, op.STATUS
ORDER BY WORK_ORDER_STATUS, OPERATION_STATUS;
```

---

### 9. `MTMFG_Research_WorkOrderOperationStatusSamples.csv`

**Why:** The status matrix is good for frequency, but it does not explain what each status pair
actually looks like in production. This companion export samples real `W`-type rows from each
work-order/operation status pair and includes lifecycle, schedule, quantity, and workcenter
context so the current feature work can interpret status meaning from live data instead of guesses.

**Query:**

```sql
WITH RankedStatusSamples AS (
  SELECT
    CAST(wo.STATUS AS NVARCHAR(100)) AS WORK_ORDER_STATUS,
    CAST(op.STATUS AS NVARCHAR(100)) AS OPERATION_STATUS,
    wo.BASE_ID AS WORK_ORDER_ID,
    wo.PART_ID,
    wo.DESIRED_QTY,
    wo.STATUS_EFF_DATE AS WORK_ORDER_STATUS_EFF_DATE,
    op.SEQUENCE_NO,
    op.RESOURCE_ID,
    sr.DESCRIPTION AS RESOURCE_DESCRIPTION,
    CAST(sr.TYPE AS NVARCHAR(100)) AS RESOURCE_TYPE,
    sr.DEPARTMENT_ID,
    sr.SCHEDULE_GROUP_ID,
    op.SETUP_COMPLETED,
    op.COMPLETED_QTY,
    op.CALC_END_QTY,
    op.SCHED_START_DATE,
    op.SCHED_FINISH_DATE,
    op.STATUS_EFF_DATE AS OPERATION_STATUS_EFF_DATE,
    ROW_NUMBER() OVER (
      PARTITION BY wo.STATUS, op.STATUS
      ORDER BY
        COALESCE(op.STATUS_EFF_DATE, wo.STATUS_EFF_DATE) DESC,
        wo.BASE_ID,
        op.SEQUENCE_NO
    ) AS STATUS_PAIR_ROW_NUM
  FROM dbo.OPERATION op
  INNER JOIN dbo.WORK_ORDER wo
    ON wo.BASE_ID = op.WORKORDER_BASE_ID
   AND wo.TYPE = op.WORKORDER_TYPE
  LEFT JOIN dbo.SHOP_RESOURCE sr
    ON sr.ID = op.RESOURCE_ID
  WHERE op.WORKORDER_TYPE = 'W'
    AND wo.TYPE = 'W'
)
SELECT
  WORK_ORDER_STATUS,
  OPERATION_STATUS,
  WORK_ORDER_ID,
  PART_ID,
  DESIRED_QTY,
  WORK_ORDER_STATUS_EFF_DATE,
  SEQUENCE_NO,
  RESOURCE_ID,
  RESOURCE_DESCRIPTION,
  RESOURCE_TYPE,
  DEPARTMENT_ID,
  SCHEDULE_GROUP_ID,
  SETUP_COMPLETED,
  COMPLETED_QTY,
  CALC_END_QTY,
  SCHED_START_DATE,
  SCHED_FINISH_DATE,
  OPERATION_STATUS_EFF_DATE,
  STATUS_PAIR_ROW_NUM
FROM RankedStatusSamples
WHERE STATUS_PAIR_ROW_NUM <= 100
ORDER BY WORK_ORDER_STATUS, OPERATION_STATUS, STATUS_PAIR_ROW_NUM;
```

---

### 10. `MTMFG_Research_ShopResourceWorkcenterProfiles.csv`

**Why:** FEATURE-07 currently assumes `SHOP_RESOURCE.SCHEDULE_NORMALLY = 'Y'` is the right
starting filter for the selectable workstation list. This export should also be the source of truth
for what each operation station actually means, because that meaning lives in `SHOP_RESOURCE`
descriptive columns such as `DESCRIPTION`, `TYPE`, `DEPARTMENT_ID`, and `SCHEDULE_GROUP_ID`.

**Query:**

```sql
SELECT
  sr.ID AS RESOURCE_ID,
  sr.DESCRIPTION AS RESOURCE_DESCRIPTION,
  CAST(sr.TYPE AS NVARCHAR(100)) AS RESOURCE_TYPE,
  sr.DEPARTMENT_ID,
  sr.SCHEDULE_GROUP_ID,
  CAST(sr.SCHEDULE_NORMALLY AS NVARCHAR(100)) AS SCHEDULE_NORMALLY,
  COUNT(op.RESOURCE_ID) AS OPERATION_ROW_COUNT,
  COUNT(DISTINCT op.WORKORDER_BASE_ID) AS DISTINCT_WORK_ORDERS,
  COUNT(DISTINCT CONCAT(op.WORKORDER_BASE_ID, '|', CAST(op.SEQUENCE_NO AS NVARCHAR(50)))) AS DISTINCT_SEQUENCES,
  MIN(op.SCHED_START_DATE) AS FIRST_SCHED_START_DATE,
  MAX(op.SCHED_FINISH_DATE) AS LAST_SCHED_FINISH_DATE
FROM dbo.SHOP_RESOURCE sr
LEFT JOIN dbo.OPERATION op
  ON op.RESOURCE_ID = sr.ID
 AND op.WORKORDER_TYPE = 'W'
GROUP BY
  sr.ID,
  sr.DESCRIPTION,
  sr.TYPE,
  sr.DEPARTMENT_ID,
  sr.SCHEDULE_GROUP_ID,
  sr.SCHEDULE_NORMALLY
ORDER BY sr.ID;
```

---

### 11. `MTMFG_Research_WorkOrderMaterialSiteCoverage.csv`

**Why:** FEATURE-07 needs to validate the `MTM2` inventory join specifically, not every available
`PART_SITE.SITE_ID` for a component part. The schema export shows `REQUIREMENT` is the visible
table carrying the work-order material relationship in this environment. This export checks
whether each requirement part has a matching `PART_SITE` row for `MTM2`, while still showing the
requirement rows that have no matching `MTM2` site record.

**Query:**

```sql
SELECT
  CASE
    WHEN ps.SITE_ID IS NULL OR LTRIM(RTRIM(ps.SITE_ID)) = '' THEN 'NO_PART_SITE_ROW'
    ELSE ps.SITE_ID
  END AS SITE_BUCKET,
  COUNT(*) AS ROW_COUNT,
  COUNT(DISTINCT r.PART_ID) AS DISTINCT_WORK_ORDER_PARTS,
  COUNT(DISTINCT r.WORKORDER_BASE_ID) AS DISTINCT_WORK_ORDERS
FROM dbo.REQUIREMENT r
LEFT JOIN dbo.PART_SITE ps
  ON ps.PART_ID = r.PART_ID
 AND ps.SITE_ID = 'MTM2'
WHERE r.WORKORDER_TYPE = 'W'
GROUP BY
  CASE
    WHEN ps.SITE_ID IS NULL OR LTRIM(RTRIM(ps.SITE_ID)) = '' THEN 'NO_PART_SITE_ROW'
    ELSE ps.SITE_ID
  END
ORDER BY SITE_BUCKET;
```

---

### 12. `MTMFG_Research_WorkOrderMaterialSamples_SingleSequence.csv`

**Why:** Some setup-tech research is easier to read when limited to work orders that have only one
distinct operation sequence. This export uses the same `REQUIREMENT` + `PART_SITE` shape as the
general material sample logic, but restricts the sample to `ORDER_ID` values with exactly one
distinct `OPERATION_SEQ_NO`. It then takes the latest 1000 qualifying rows and presents them
sorted by `ORDER_ID`.

**Query:**

```sql
WITH SingleSequenceOrders AS (
  SELECT
    r.WORKORDER_BASE_ID AS ORDER_ID
  FROM dbo.REQUIREMENT r
  WHERE r.WORKORDER_TYPE = 'W'
    AND r.PART_ID IS NOT NULL
    AND LTRIM(RTRIM(r.PART_ID)) <> ''
  GROUP BY r.WORKORDER_BASE_ID
  HAVING COUNT(DISTINCT r.OPERATION_SEQ_NO) = 1
),
LatestRequirementRows AS (
  SELECT DISTINCT TOP (1000)
    r.WORKORDER_BASE_ID AS ORDER_ID,
    r.OPERATION_SEQ_NO,
    r.PIECE_NO,
    r.PART_ID,
    r.CALC_QTY,
    r.QTY_PER,
    r.FIXED_QTY,
    r.REQUIRED_DATE,
    ps.SITE_ID,
    ps.QTY_ON_HAND
  FROM dbo.REQUIREMENT r
  INNER JOIN SingleSequenceOrders sso
    ON sso.ORDER_ID = r.WORKORDER_BASE_ID
  LEFT JOIN dbo.PART_SITE ps
    ON ps.PART_ID = r.PART_ID
  WHERE r.WORKORDER_TYPE = 'W'
    AND r.PART_ID IS NOT NULL
    AND LTRIM(RTRIM(r.PART_ID)) <> ''
  ORDER BY r.REQUIRED_DATE DESC, r.WORKORDER_BASE_ID, r.OPERATION_SEQ_NO, r.PIECE_NO, r.PART_ID, ps.SITE_ID
)
SELECT
  ORDER_ID,
  OPERATION_SEQ_NO,
  PIECE_NO,
  PART_ID,
  CALC_QTY,
  QTY_PER,
  FIXED_QTY,
  REQUIRED_DATE,
  SITE_ID,
  QTY_ON_HAND
FROM LatestRequirementRows
ORDER BY ORDER_ID, OPERATION_SEQ_NO, PIECE_NO, PART_ID, SITE_ID;
```

---

### 13. `MTMFG_Research_WorkOrderMaterialSamples_MultiSequence.csv`

**Why:** Some setup-tech research is more useful when limited to work orders that span multiple
operations. This export uses the same `REQUIREMENT` + `PART_SITE` shape as the general material
sample export, but restricts the sample to `ORDER_ID` values that have more than one distinct
`OPERATION_SEQ_NO`. It then takes the latest 1000 qualifying rows and presents them sorted by
`ORDER_ID`.

**Query:**

```sql
WITH MultiSequenceOrders AS (
  SELECT
    r.WORKORDER_BASE_ID AS ORDER_ID
  FROM dbo.REQUIREMENT r
  WHERE r.WORKORDER_TYPE = 'W'
    AND r.PART_ID IS NOT NULL
    AND LTRIM(RTRIM(r.PART_ID)) <> ''
  GROUP BY r.WORKORDER_BASE_ID
  HAVING COUNT(DISTINCT r.OPERATION_SEQ_NO) > 1
),
LatestRequirementRows AS (
  SELECT DISTINCT TOP (1000)
    r.WORKORDER_BASE_ID AS ORDER_ID,
    r.OPERATION_SEQ_NO,
    r.PIECE_NO,
    r.PART_ID,
    r.CALC_QTY,
    r.QTY_PER,
    r.FIXED_QTY,
    r.REQUIRED_DATE,
    ps.SITE_ID,
    ps.QTY_ON_HAND
  FROM dbo.REQUIREMENT r
  INNER JOIN MultiSequenceOrders mso
    ON mso.ORDER_ID = r.WORKORDER_BASE_ID
  LEFT JOIN dbo.PART_SITE ps
    ON ps.PART_ID = r.PART_ID
  WHERE r.WORKORDER_TYPE = 'W'
    AND r.PART_ID IS NOT NULL
    AND LTRIM(RTRIM(r.PART_ID)) <> ''
  ORDER BY r.REQUIRED_DATE DESC, r.WORKORDER_BASE_ID, r.OPERATION_SEQ_NO, r.PIECE_NO, r.PART_ID, ps.SITE_ID
)
SELECT
  ORDER_ID,
  OPERATION_SEQ_NO,
  PIECE_NO,
  PART_ID,
  CALC_QTY,
  QTY_PER,
  FIXED_QTY,
  REQUIRED_DATE,
  SITE_ID,
  QTY_ON_HAND
FROM LatestRequirementRows
ORDER BY ORDER_ID, OPERATION_SEQ_NO, PIECE_NO, PART_ID, SITE_ID;
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
| ★★★ | `MTMFG_Research_WorkOrderOperationStatusMatrix.csv` | Required to validate FEATURE-02/07 "active" work-order and sequence status filters |
| ★★★ | `MTMFG_Research_WorkOrderOperationStatusSamples.csv` | Required to inspect real row-level context behind each work-order and operation status pair |
| ★★★ | `MTMFG_Research_ShopResourceWorkcenterProfiles.csv` | Required to validate whether `SCHEDULE_NORMALLY = 'Y'` is the right workcenter filter |
| ★★★ | `MTMFG_Research_WorkOrderMaterialSiteCoverage.csv` | Required to validate the correct `PART_SITE.SITE_ID` for subordinate-part on-hand joins |
| ★★★ | `MTMFG_Research_CustOrderAlloc_TypeProfiles.csv` | Highest-value bridge-table semantics for CO-to-supply research |
| ★★★ | `MTMFG_Research_CustOrderAlloc_Samples.csv` | Direct inspection of real bridge rows |
| ★★ | `MTMFG_Research_WorkOrderMaterialSamples_SingleSequence.csv` | Focuses the material sample on work orders with exactly one distinct operation sequence |
| ★★ | `MTMFG_Research_WorkOrderMaterialSamples_MultiSequence.csv` | Focuses the same material sample on work orders with more than one distinct operation sequence |
| ★★ | `MTMFG_Research_WorkOrderCustomerOrderCoverage.csv` | Tests whether `WBS_CUST_ORDER_ID` is a usable link |
| ★★ | `MTMFG_Research_TargetColumnInventory.csv` | Speeds up future missing-FK relationship discovery |
| ★★ | `MTMFG_Research_BridgeColumnPopulation.csv` | Validates whether bridge fields are populated enough to trust |
| ★★ | `MTMFG_Research_DateFieldRanges.csv` | Helps settle chronology questions such as `Oldest Added` |

---

## Summary

The core schema export set is already complete for this repository. The next useful exports are not
more generic metadata dumps; they are targeted research extracts that explain how Visual actually
uses status fields, demand/supply bridge rows, chronology columns, customer-order/work-order
cross-reference fields, and the specific work-order/sequence/workcenter/site assumptions needed for
FEATURE-07 Setup Technician implementation under a normal read-only account.

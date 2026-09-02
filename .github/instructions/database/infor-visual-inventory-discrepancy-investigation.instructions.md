---
applyTo: "Database/InforVisualScripts/**/*.sql, docs/**/*Inventory*.md, docs/**/*Discrepancy*.md, docs/**/*Reconcil*.md, Module_*/Data/**/*Visual*.cs, Module_*/Data/**/*Inventory*.cs"
description: >
  Investigation playbook for physical-count vs Infor Visual (MTMFG) inventory discrepancies.
  Covers the INVENTORY_TRANS schema decode, ledger self-check, barcode-labor clock-out
  consumption mechanics, negative-balance detection, and missing-receipt rule-out, with
  ready-to-run READ-ONLY SQL templates.
---

<!-- 
[DOC-META-START]
- File Name: infor-visual-inventory-discrepancy-investigation.instructions.md
- Description: Reusable investigation playbook for physical-count vs Infor Visual (MTMFG) inventory discrepancies - schema decode, ledger self-check, root-cause checks, and READ-ONLY SQL templates.
- Last Updated: 2026-09-02
- Quick TOC:
  - Line 12-15: # Infor Visual Inventory Discrepancy Investigation
  - Line 16-24: ## When To Use This Playbook
  - Line 25-29: ## Non-Negotiables
  - Line 30-45: ## Mental Model (How The Ledger Works)
  - Line 46-61: ## Schema Decode Reference
  - Line 62-100: ## Investigation Workflow (Run In Order)
  - Line 101-118: ## Reconciliation Math
  - Line 119-127: ## Root-Cause Decision Table
  - Line 128-137: ## References
- Critical Notes: MTMFG is READ ONLY; the system ledger is always internally consistent - find the physical-activity mismatch, never a "lost record."
[DOC-META-END]
-->

# Infor Visual Inventory Discrepancy Investigation

Reusable method for explaining why a physical floor count differs from Infor Visual (MTMFG)
system on-hand. Derived from the MMF0005531 case (2026-09-02, 21,091 lb gap, root cause =
sheet consumed on the books at barcode-labor clock-out on three open work orders but never
physically cut). Companion: `.serena/memories/infor-visual-inventory-discrepancy-playbook.md`.

## When To Use This Playbook

Use when a user reports "physical count is X lb over/under what the system says" for a part,
especially when location-to-location transfers are suspected. Also use when building or
reviewing Material Availability / reconciliation queries or DAOs.

## Non-Negotiables

- The MTMFG Infor Visual database is READ ONLY. Never issue INSERT/UPDATE/DELETE.
- Verify every column name against the CSV files under `docs/development/InforVisual/DatabaseCSVFiles/`
  (Tables/, ColumnDetails/, ForeignKeys/, PrimaryKeys/, TableRowCounts/) before running SQL.
- Query access (dev): `sqlcmd -S VISUAL -d MTMFG -U <user> -P <pwd> -Q "<sql>"`.
- The physical count scope MUST be clarified first (raw material only vs includes cut WIP).

## Mental Model (How The Ledger Works)

- System on-hand = lifetime `SUM(inbound) - SUM(outbound)` per part. It is always internally
  consistent with `INVENTORY_TRANS`. If lifetime I-O does not equal `CR_PART_LOCATION`
  on-hand, there is a real ledger problem; otherwise the "gap" is a physical-activity
  mismatch (over/under consumption, or an inbound that physically arrived but was never
  booked) - not a missing/lost record.
- **PO receipts** add to on-hand (staging locations such as RECV/S-00/T-00).
- **Location transfers** (staging -> work center) are recorded as a matched pair:
  an Out leg at the source location and an In leg at the destination, linked both ways by
  `TRANSFER_TRANS_ID`. Pairs net to zero and are NOT the usual cause of a gap.
- **Raw sheet is consumed from the work center (WC) when an operator CLOCKS OUT of barcode
  labor** on the cutting operation (auto-issue / backflush at clock-out). Consumed weight ≈
  reported `GOOD_QTY` x per-piece sheet weight. Consumption is NOT posted when a work order
  is merely created.
- Material issued to a work order leaves raw on-hand but is not "finished stock" until the
  work order is received/closed. Open work orders therefore hold the pool of material that
  is physically present but absent from raw on-hand.

## Schema Decode Reference

`INVENTORY_TRANS` (per part): `QTY` is always positive; direction is in `TYPE`.

| Pattern | Meaning |
| ------- | ------- |
| `TYPE='I'` + `CLASS='R'` + `PURC_ORDER_ID` | PO receipt (inbound) |
| `TYPE='I'` or `'O'` + `CLASS='A'` + `TRANSFER_TRANS_ID` set (both legs) | Location transfer pair (net 0) |
| `TYPE='O'` + `CLASS='I'` + `WORKORDER_BASE_ID` | WO consumption (issued at labor clock-out) |
| `TYPE='I'` + `CLASS='I'` + `WORKORDER_BASE_ID` | WO material return |
| `CLASS='A'`, `TRANSFER_TRANS_ID` NULL, no WO/PO | Manual adjustment - always inspect |

Supporting tables:

- `CR_PART_LOCATION` - current on-hand per (part, warehouse, location): columns `ID`,
  `WAREHOUSE_ID`, `LOCATION_ID`, `QTY`, `COMMITTED_QTY`.
- `LABOR_TICKET` - barcode labor clock in/out (`USER_ID` = 'WEDGE BARCODE'). Columns:
  `TRANSACTION_ID` (PK), `WORKORDER_BASE_ID`, `OPERATION_SEQ_NO`, `EMPLOYEE_ID`,
  `RESOURCE_ID`, `CLOCK_IN`, `CLOCK_OUT`, `GOOD_QTY`, `BAD_QTY`, `TRANSACTION_DATE`.
- `WORK_ORDER` - `PART_ID` is the PRODUCED part (not the raw sheet), so join
  `INVENTORY_TRANS` to `WORK_ORDER` on `WORKORDER_BASE_ID` only. `STATUS` ('R'=released/open,
  'C'=closed), `RECEIVED_QTY`, `CLOSE_DATE`.
- `PURC_ORDER_LINE` - PO open/received: `ORDER_QTY`, `TOTAL_RECEIVED_QTY`, `LINE_STATUS`,
  `LAST_RECEIVED_DATE`.

## Investigation Workflow (Run In Order)

**Step 0 - Clarify the physical count.** Ask: raw sheet only, or does it include cut
parts/WIP? This decides whether an excess is a real over-consumption (raw) or benign WIP.

**Step 1 - Current on-hand per location.**

```sql
SELECT WAREHOUSE_ID, LOCATION_ID, QTY, COMMITTED_QTY
FROM   dbo.CR_PART_LOCATION
WHERE  ID = '<PART>' AND QTY <> 0
ORDER  BY WAREHOUSE_ID, LOCATION_ID;
```

**Step 2 - Ledger self-check.** Lifetime I-O must equal the on-hand total from Step 1.

```sql
SELECT COUNT(*) AS Cnt,
       MIN(TRANSACTION_DATE) AS MinDate,
       MAX(TRANSACTION_DATE) AS MaxDate,
       SUM(CASE WHEN TYPE='I' THEN QTY ELSE 0 END)  AS SumIn,
       SUM(CASE WHEN TYPE='O' THEN QTY ELSE 0 END)  AS SumOut,
       SUM(CASE WHEN TYPE='I' THEN QTY ELSE -QTY END) AS NetIO
FROM   dbo.INVENTORY_TRANS
WHERE  PART_ID = '<PART>';
```

If NetIO != on-hand, reconcile the difference (missing/duplicate posting). If it matches,
the gap is a physical-activity mismatch and Steps 3-8 locate it.

**Step 3 - Profile by type/class** to see how much moved in each category.

```sql
SELECT TYPE, CLASS, COUNT(*) AS Cnt, SUM(QTY) AS TotalQty
FROM   dbo.INVENTORY_TRANS
WHERE  PART_ID = '<PART>'
GROUP  BY TYPE, CLASS
ORDER  BY TYPE, CLASS;
```

**Step 4 - Confirm transfers are fully paired** (rule transfers in/out as a cause).

```sql
SELECT it.TRANSACTION_ID, it.TRANSFER_TRANS_ID, it.TYPE, it.LOCATION_ID, it.QTY
FROM   dbo.INVENTORY_TRANS it
WHERE  it.PART_ID = '<PART>'
  AND  it.TRANSFER_TRANS_ID IS NOT NULL
  AND  NOT EXISTS (SELECT 1 FROM dbo.INVENTORY_TRANS p
                   WHERE p.TRANSACTION_ID = it.TRANSFER_TRANS_ID);
```

**Step 5 - Find locations that ran NEGATIVE** (consumption ahead of physical stock - the
classic smoking gun for over-consumption).

```sql
;WITH t AS (
    SELECT TRANSACTION_ID, LOCATION_ID,
           CONVERT(varchar(10), TRANSACTION_DATE, 120) AS TxDate,
           CASE WHEN TYPE='I' THEN QTY ELSE -QTY END AS Delta
    FROM   dbo.INVENTORY_TRANS
    WHERE  PART_ID = '<PART>' AND TRANSACTION_DATE >= '<start>'
), b AS (
    SELECT *, SUM(Delta) OVER (PARTITION BY LOCATION_ID
                               ORDER BY TxDate, TRANSACTION_ID) AS RunBal
    FROM t
)
SELECT TRANSACTION_ID, LOCATION_ID, Delta, RunBal, TxDate
FROM   b WHERE RunBal < 0 ORDER BY TxDate, TRANSACTION_ID;
```

**Step 6 - Inspect WO consumption against work orders and labor.** For any WO that consumed
this part, get status, pieces received, and the clock-outs that drove the consumption.

```sql
;WITH issue AS (
    SELECT it.WORKORDER_BASE_ID AS WO,
           SUM(CASE WHEN it.TYPE='O' THEN it.QTY ELSE 0 END) AS IssuedLbs,
           SUM(CASE WHEN it.TYPE='I' THEN it.QTY ELSE 0 END) AS ReturnedLbs
    FROM   dbo.INVENTORY_TRANS it
    WHERE  it.PART_ID = '<PART>' AND it.WORKORDER_BASE_ID IS NOT NULL
    GROUP  BY it.WORKORDER_BASE_ID
)
SELECT i.WO, wo.STATUS, wo.DESIRED_QTY AS WantPcs, wo.RECEIVED_QTY AS DonePcs,
       (i.IssuedLbs - i.ReturnedLbs) AS NetOutLbs
FROM   issue i LEFT JOIN dbo.WORK_ORDER wo ON wo.BASE_ID = i.WO
WHERE  COALESCE(wo.STATUS, 'X') <> 'C' AND (i.IssuedLbs - i.ReturnedLbs) > 0
ORDER  BY NetOutLbs DESC;

-- Labor clock-outs behind one WO (confirm the job actually ran / was cut):
SELECT TRANSACTION_ID, OPERATION_SEQ_NO AS Seq, EMPLOYEE_ID AS Emp,
       RESOURCE_ID AS Res, GOOD_QTY, BAD_QTY, CLOCK_IN, CLOCK_OUT, USER_ID
FROM   dbo.LABOR_TICKET
WHERE  WORKORDER_BASE_ID = '<WO>' ORDER BY CLOCK_IN;
```

**Step 7 - Rule out a missing receipt.** Open PO lines with zero (or partial) receipts mean
that truck has not physically arrived and been booked.

```sql
SELECT PURC_ORDER_ID, LINE_NO, ORDER_QTY, TOTAL_RECEIVED_QTY,
       (ORDER_QTY - TOTAL_RECEIVED_QTY) AS OpenQty, RTRIM(LINE_STATUS) AS LineStatus
FROM   dbo.PURC_ORDER_LINE
WHERE  PART_ID = '<PART>' AND ORDER_QTY > TOTAL_RECEIVED_QTY
ORDER  BY PURC_ORDER_ID DESC;
```

**Step 8 - Reconcile a window** (see next section) and confirm the residual equals the
reported discrepancy before writing up findings.

## Reconciliation Math

For a window starting when the part was near a known balance:

- `on-hand change = current on-hand - on-hand at window start`.
- `window change = receipts (TYPE I, CLASS R) - net WO consumption (O-I of CLASS I)
  + net adjustments (CLASS A without partner/PO)`; transfers net to zero.
- If the two disagree, re-examine classification.
- Cross-check against physical: if booked receipts equal physical receipts, then
  `discrepancy = system-consumption - implied-physical-consumption`.
  Positive => the system consumed raw material that physically remains (over-consumption).
- Sanity: `received_lbs - physical_on_hand_lbs` gives implied physical consumption.

## Root-Cause Decision Table

| Symptom | Likely cause | Check |
| ------- | ------------ | ----- |
| Physical (raw) > system | Over-consumption: sheet consumed on books at clock-out but never cut; or unbooked inbound | Step 6 (open WOs, LABOR_TICKET), Step 7 (open POs) |
| Physical < system | Under-consumption / unreturned WO material; or double-booked receipt | Step 6 (WO returns), Step 7 |
| Location went negative | Consumption/issue ran ahead of transferred-in stock | Step 5 |
| Open WO (STATUS R) with 0 pieces received | Issued/unconsumed material pool to inspect | Step 6 |
| Transfer leg missing its partner | Broken transfer (rare) | Step 4 |

Present findings with concrete transaction IDs, work order numbers, and the exact numbers
that reconcile to the reported gap, then always recommend a floor verification step.

## References

- CSV schema reference: `docs/development/InforVisual/DatabaseCSVFiles/`
- Existing read-only queries: `Database/InforVisualScripts/Queries/` (notably
  `18_GetMaterialAvailabilityCurrentStock.sql`, `18_GetReceivingLocationTransferMovements.sql`,
  `18a/18b` transfer pair queries, `17_GetReceivingLocationTransactionHistory.sql`)
- Schema reference instructions: `.github/instructions/database/infor-visual-database-reference.instructions.md`
- Query authoring: `.github/instructions/database/infor-visual-query-authoring.instructions.md`

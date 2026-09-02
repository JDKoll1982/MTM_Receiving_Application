<!-- 
[DOC-META-START]
- File Name: infor-visual-inventory-discrepancy-playbook.md
- Description: Reusable investigation method for physical-count vs Infor Visual (MTMFG) on-hand gaps - schema decode, ledger self-check, root-cause checks.
- Last Updated: 2026-09-02
- Quick TOC:
  - Line 16-17: # Infor Visual Inventory Discrepancy Investigation
  - Line 18-30: ## Schema Facts (invariants)
  - Line 31-38: ## Method (run in order)
  - Line 39-46: ## Root-Cause Heuristics
- Critical Notes: MTMFG is READ ONLY; system ledger is always internally consistent - find the physical-activity mismatch, not a ledger bug.
[DOC-META-END]
-->

# Infor Visual Inventory Discrepancy Investigation

Use when a physical count differs from MTM/Infor Visual (MTMFG) on-hand. Full step-by-step
with SQL templates lives in
`.github/instructions/database/infor-visual-inventory-discrepancy-investigation.instructions.md`.

## Schema Facts (invariants)

- MTMFG SQL Server is READ ONLY. Verify every column against the CSV files under
  `docs/development/InforVisual/DatabaseCSVFiles/` (see `mem:infor-visual-csv-source-of-truth`);
  access constraints in `mem:infor_visual_constraints`.
- On-hand by location = `dbo.CR_PART_LOCATION` (ID, WAREHOUSE_ID, LOCATION_ID, QTY, COMMITTED_QTY).
- Ledger self-check: lifetime `SUM(TYPE='I' QTY) - SUM(TYPE='O' QTY)` over `INVENTORY_TRANS`
  MUST equal CR_PART_LOCATION on-hand. QTY is always positive; direction is in TYPE.
- `INVENTORY_TRANS` decode:
  - `I` + CLASS `R` + PURC_ORDER_ID = PO receipt.
  - `I`/`O` + CLASS `A` + TRANSFER_TRANS_ID (set on both legs) = location transfer pair (net 0).
  - `O` + CLASS `I` + WORKORDER_BASE_ID = WO sheet consumption.
  - `I` + CLASS `I` + WORKORDER_BASE_ID = WO material return.
  - `O`/`I` + CLASS `A`, TRANSFER_TRANS_ID NULL, no WO/PO = manual adjustment (investigate).
- Raw sheet leaves the WC on-hand when an operator CLOCKS OUT of barcode labor
  (`LABOR_TICKET`, USER_ID `WEDGE BARCODE`) on the cutting op (seq ~10). Lbs consumed ≈
  reported GOOD_QTY × per-piece sheet weight. NOT removed at WO creation - never claim
  "issued but never ran" without checking LABOR_TICKET first.
- `WORK_ORDER.PART_ID` is the PRODUCED part, not the raw sheet. Join INVENTORY_TRANS to
  WORK_ORDER on WORKORDER_BASE_ID only.

## Method (run in order)

1. Ask the physical-count scope FIRST (raw sheet only vs includes cut WIP) - it decides raw
   over-consumption vs benign WIP.
2. Current on-hand per location via CR_PART_LOCATION.
3. Ledger self-check (Schema Facts). If it does NOT balance, that is the story. If it
   balances, the gap is a physical-activity mismatch (over/under consumption or unbooked
   inbound), never a "lost record."
4. Classify recent transactions; confirm transfer pairs are complete (both TRANSFER_TRANS_ID
   legs exist) before blaming transfers.
5. Compute running per-location balances; a location going NEGATIVE (e.g. WC) = consumption
   ran ahead of physical stock - prime suspect.
6. For WO consumption, read LABOR_TICKET clock-outs + WORK_ORDER STATUS / RECEIVED_QTY.
   Open WO (R) with 0 received = issued/unconsumed candidate pool.
7. Rule out missing receipt: PURC_ORDER_LINE `ORDER_QTY - TOTAL_RECEIVED_QTY` (LINE_STATUS A)
   still open + zero received means those trucks have not arrived.
8. Reconcile window: receipts(CLASS R) - net WO consumption ≈ on-hand change; residual maps
   to the discrepancy amount and direction.

## Root-Cause Heuristics

- Physical > system = over-consumption (consumed on books, sheets still raw on floor) or
  physically-delivered-but-unbooked inbound.
- Physical < system = under-consumption / unreturned WO material, or double-booked receipt.
- Whole uncut sheets still on floor while WO open and RECEIVED_QTY=0 ⇒ operators reported
  pieces at clock-out ahead of actual cutting (full-order reporting) - verify on the floor.
- Discoverability: this is the inventory graph node; part cases are separate topic memories.

<!-- 
[DOC-META-START]
- File Name: W99996-06002S_Inventory_Discrepancy_Report.md
- Description: Quick-read findings on why the physical count of W99996-06002S reads higher than Infor Visual - the part's auto-backflush location S-A1-01 consumed stock it was never sent.
- Last Updated: 2026-09-25
- Quick TOC:
  - Line 20-23: # W99996-06002S Inventory Discrepancy Report
  - Line 24-32: ## The Short Answer
  - Line 33-44: ## Where It Stands
  - Line 45-50: ## Why It Happened
  - Line 51-63: ## Checks Run
  - Line 64-68: ## Ruled Out
  - Line 69-81: ## Shop-Wide, Not Just This Part
  - Line 82-88: ## Do This Next
  - Line 89-93: ## See Also
- Critical Notes: The Visual ledger is exact - this is a LOCATION defect, not a lost record. S-A1-01 is the part's AUTO_ISSUE_LOC and DEF_BACKFLUSH_LOC; it reads -54,407 against 60,000 physically present.
[DOC-META-END]
-->

# W99996-06002S Inventory Discrepancy Report

**Investigated:** 2026-09-25 | **Part:** W99996-06002S (Nut, Weld Square M6x1.0, EA) | **Source:** MTMFG, read-only

## The Short Answer

The books for this part are internally exact - they just point at the wrong place. The part's
auto-backflush location `S-A1-01` has been charged 105,980 nuts while only ever being sent
51,573, so it reads **-54,407**. Physically there are **60,000** there right now, in 12 boxes
of 5,000. That one location is a **114,407** difference, and it is the whole of this part's gap.

**STILL OPEN TODAY: 54,407 NUTS** (WO-073244 13,531 + WO-074263 40,876).

## Where It Stands

| Location | System | Physical | Difference |
| --- | --- | --- | --- |
| `V-N2-02` | 150,000 | 150,000 | 0 - confirmed correct |
| `S-A3-01` | 9,707 | not counted | unknown |
| `S-A0-02` | 6,000 | not counted | unknown |
| **`S-A1-01`** | **-54,407** | **60,000** | **+114,407** |
| **Part total** | **111,300** | - | - |

The other 30 location rows for this part are zero.

## Why It Happened

- This is a weld nut. Nobody scans it. When an operator clocks out of the weld operation by barcode, the system **automatically writes off 4 nuts for every piece completed**.
- That write-off is hard-wired to `S-A1-01`, because the part is flagged `AUTO_ISSUE_LOC` and `DEF_BACKFLUSH_LOC` there.
- Stock is never transferred into that location, so the write-offs outrun the transfers. It first went negative **2026-07-02** and has stayed negative since.

## Checks Run

| Check | Result |
| --- | --- |
| Ledger self-check (In - Out = on-hand) | Exact at 111,300 |
| Receipt audit | All 14 deliveries posted; no unbooked receipt |
| Transfer pairing | All legs paired; no in-transit rows |
| Work-order consumption | Legitimate clock-out backflush at 4 nuts/piece |
| Open work orders | WO-074263 open, wants 11,577, **received 0**, 40,876 already issued |
| Unposted issues | 33,744 across 8 transactions |
| Open purchase orders | 150,000 on order, not due until 2027 |
| Cycle counting | Not used database-wide; this part not set up |

## Ruled Out

Rack mix-up (`V-N2-02` confirmed correct), lost or duplicate records, unposted deliveries,
broken transfers, wrong usage per piece, and a duplicate part number.

## Shop-Wide, Not Just This Part

| Scope | Rows | Negative on-hand |
| --- | --- | --- |
| Warehouse 002, all locations | 50 | -139,704.85 |
| `S-*` staging locations | 4 | -66,409.00 |
| `WC` work centre | 14 | -42,662.85 |
| `V-*` racks | 7 | -69.00 |

The negatives sit in the staging and work-centre locations, not in the racks. The reported
whole-shop variance of ~85,000 is **smaller** than the negative staging total alone, so
something elsewhere is likely overstating the books.

## Do This Next

1. Count `S-A3-01` and `S-A0-02` to close out this part's physical total.
2. Post the `V-N2-02` to `S-A1-01` transfer that should have happened, and stop issuing from a location that is never stocked.
3. Clear or post the 33,744 unposted WO-074263 issues before period close.
4. Chase the whole-shop figure - it is smaller than the negative staging total, which points to overstatement somewhere.

## See Also

- `.github/instructions/database/infor-visual-inventory-discrepancy-investigation.instructions.md`
- `Database/InforVisualScripts/Queries/32_GetInventoryDiscrepancyPartProfile.sql`
- `Database/InforVisualScripts/Queries/33_GetNegativeOnHandLocations.sql`
- `docs/MMF0005531_Inventory_Discrepancy_Findings.md`

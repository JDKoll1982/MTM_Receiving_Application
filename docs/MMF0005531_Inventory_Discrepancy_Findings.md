<!-- 
[DOC-META-START]
- File Name: MMF0005531_Inventory_Discrepancy_Findings.md
- Description: Plain-language findings explaining why the physical count of RAW SHEET for part MMF0005531 exceeded the Infor Visual on-hand by about 21,000 lb on 2026-09-02.
- Last Updated: 2026-09-02
- Quick TOC:
  - Line 10-13: # Inventory Discrepancy - Part MMF0005531 (Findings)
  - Line 14-19: ## The Short Answer
  - Line 20-26: ## How The Steel Really Leaves The Books
  - Line 27-37: ## What The System Shows Today
  - Line 38-45: ## Where The Extra Sheets Are
  - Line 46-54: ## The Three Work Orders Involved
  - Line 55-62: ## Why It Is Not The Transfers
  - Line 63-68: ## What We Ruled Out
  - Line 69-79: ## What To Check And Do Next
- Critical Notes: The count was RAW SHEET ONLY. About 21,000 lb of whole uncut sheets are physically on the floor but the system already consumed them at barcode-labor clock-out on three open work orders; the sheets were issued on the books but never actually cut.
[DOC-META-END]
-->

# Inventory Discrepancy - Part MMF0005531 (Findings)

**Investigated:** 2026-09-02 | **Part:** MMF0005531 (Sheet, .312 x 60 x 120)

## The Short Answer

The physical count was of **raw sheet only** (whole sheets / bundles by weight), and it came
up about **21,091 lb HIGHER** than the system's raw-sheet on-hand. That means roughly
**21,000 lb of whole, uncut sheets are physically on the floor, but the system has already
taken them off the raw on-hand count.**

The system removed them at **barcode-labor clock-out** on three work orders that are still
open. On the books the sheet was "consumed," but those sheets were **never actually cut** -
they are still whole on the floor. This is roughly **33 whole sheets (about half a
truckload)**.

## How The Steel Really Leaves The Books

- Steel is **not** taken off the on-hand count when a work order is created.
- The sheet leaves the work center (WC) on-hand **when an operator clocks out of barcode
  labor** on the cutting operation. At that moment the system removes the sheet weight for
  the pieces reported as cut.
- Verified in the database: every sheet-consumption line matches a barcode clock-out, and
  the pounds equal reported pieces x per-piece weight (about 220 lb each). Example:
  WO-073062 consumed 12,980 lb on Aug 4 = 59 pieces x 220 lb, reported at two clock-outs.
- So a "consumed" line in the books means someone clocked out and reported pieces - **not**
  that whole sheets were physically run through the machine.

## What The System Shows Today

- System raw-sheet on-hand: **12,451 lb**, all in S-00 (staging).
- Work center (WC), T-00, RECV, and all other locations: **zero**.
- The floor count says there is about **33,542 lb** of raw sheet - about 21,091 lb more than
  the system.

## Where The Extra Sheets Are

- Three open work orders had sheet "consumed" at clock-out (removed from raw on-hand) but
  the work orders are still open and **no finished parts were received to stock**.
- Because no parts were ever received, the sheets were clearly not all actually cut. The
  whole sheets are physically still present - most likely **staged at or near the cutting
  work center** (resource 060-03), waiting to be cut.
- The system's WC balance went **negative** (as low as -3,070 lb on Jul 13, still -173
  today), which is the signature of the system consuming sheet faster than sheets were being
  moved into the WC on the books - the consumption ran ahead of the physical sheet.

## The Three Work Orders Involved

| Work Order | Status | Pieces Reported Cut | Pieces Received to Stock | Sheet Consumed on Books |
| ---------- | ------ | ------------------- | ------------------------ | ----------------------- |
| WO-073062  | Open   | 59 (full order)     | 0 of 59                  | **12,980 lb** |
| WO-073820  | Open   | 21 of 76            | 0 of 76                  | **4,620 lb** |
| WO-073063  | Open   | 416                 | 402 of 413               | **4,116 lb** |
| | | | | **Total: about 21,700 lb** |

The total is within a few hundred pounds of the 21,091 lb difference.

## Why It Is Not The Transfers

We checked every location-to-location transfer (the Out + In pairs) in the system:

- Every transfer is a clean, matched Out/In pair that nets to zero.
- None of the transfer pairs are broken or half-finished.
- Transfers are **not** causing the difference.

## What We Ruled Out

- **No missing receipt:** every recent delivery is fully booked. Open purchase orders show
  zero received, meaning those trucks have not arrived yet.
- **No system math error:** the system's lifetime records add up exactly to the on-hand
  balance.
- **No half-finished transfers** (see above).
- **No idle jobs that "never ran":** the jobs did run at the cutting operation - the books
  consumed the sheet at operator clock-out. The problem is the physical sheets were not
  actually cut to match.

## What To Check And Do Next

1. **Go find the sheets.** Look for roughly 33 whole sheets (~21,000 lb) staged at or near
   the cutting work center (resource 060-03). Confirm they belong to WO-073062, WO-073063,
   or WO-073820.
2. **Return what will not be cut.** For sheet that is not going to be used, do a material
   return on those work orders so the sheet goes back on the raw on-hand count. That will
   bring the system in line with the floor.
3. **Find out why the books ran ahead.** Figure out why clock-outs consumed sheet for the
   full order quantity when the sheets were not cut - for example, operators reporting the
   whole job quantity at clock-out instead of only what was actually cut, or material being
   auto-issued for the full requirement when the job started.
4. **Prevent it going forward:** only report pieces that were actually cut, and return
   unused sheet the same day it is set aside.

> Note: There is also one odd single entry - a **634 lb** removal from the work center on
> Aug 21 with no work order and no transfer behind it. Small, but worth checking separately.

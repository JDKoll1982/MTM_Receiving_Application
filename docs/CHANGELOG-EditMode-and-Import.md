# Receiving – Edit Mode & Import Changes

**Date:** February 26, 2026

---

## Edit Mode (View_Receiving_EditMode)

### Bug Fixes
- **Duplicate XAML removed** — the view file contained the entire layout twice; the duplicate block has been deleted.
- **Date columns now display correctly** — `ReceivedDate` and `PODueDate` were bound to a decimal converter; they now use a proper date formatter.
- **Part Type column** now shows the type name (e.g. "Coil") instead of a raw number.
- **Column data bindings corrected** — several column bindings used the wrong property casing and silently showed nothing; these now resolve correctly.

### New Features

| Feature | Details |
|---|---|
| **Column sort** | Click any column header to sort ascending; click again to reverse. An arrow indicator shows the active column and direction. |
| **Sort persistence** | The last-used sort column and direction are remembered per user across sessions. |
| **Page-size selector** | Choose how many rows to show per page: 20, 50, 100, or 200. Located in the footer. |
| **Page-size persistence** | The selected page size is remembered per user across sessions. |
| **Loading overlay** | A spinner and status message cover the grid while data is loading, preventing interaction with stale data. |
| **Enter key on page jump** | Pressing Enter in the "Go to page" box now navigates immediately (no need to click Go). |
| **Search clear button** | A × button appears inside the search box when text is present; clicking it clears the search instantly. |
| **Column chooser – Select All / Clear All** | Two quick-action buttons at the top of the column chooser dialog let you show or hide all columns at once. |
| **Help button** | A Help button has been added to the toolbar. |

---

## Receiving History Import Script (`Import-ReceivingHistory.ps1`)

### Infor Visual Enrichment
The script now queries the Infor Visual (SQL Server) database before writing each record to MySQL:

- **Part Description** is pulled from Infor Visual's `PART` table and stored with the record instead of being left blank.
- **Vendor Name** is pulled from Infor Visual when a PO number is present.
- The Infor Visual connection uses the same server, database, and credentials as the application (`VISUAL / MTMFG`). These can be overridden via script parameters if needed.
- If Infor Visual is unreachable, the import continues without enrichment and displays a warning — it does **not** abort.
- Pass `-SkipInforVisualLookup` to skip IV enrichment entirely (useful for offline/test runs).

### Non-PO Item Flagging
If a row has **no PO number** and the Part ID is **not found in Infor Visual**, the record is automatically flagged as a Non-PO item (`is_non_po_item = 1`). This mirrors the same flag available in the main receiving workflow.

### Correction List
If a row has a PO number but the PO + Part combination is **not found in Infor Visual**, the record is still imported but a correction table is printed at the end of the run:

```
CORRECTION REQUIRED — PO + Part combination not found in Infor Visual:
PartID                 PO Number       Date        Emp
----------------------------------------------------------------
SOMEPART-001           PO-066914       2024-01-15  1234
```

These rows should be reviewed — the most likely causes are a typo in the PO number or a part not yet set up in Infor Visual.

---

## Database Changes

### Schema Migration (`Migrate-AddIsNonPOItem.sql`)
A new migration script adds the `is_non_po_item` column to the `receiving_history` table. Run this **once** before using the updated import script:

```
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Database/Scripts/Migrate-AddIsNonPOItem.sql
```

The script is safe to re-run — it checks whether the column already exists before altering the table.

### Stored Procedure (`sp_Receiving_History_Import`)
The import stored procedure accepts a new `p_IsNonPOItem` parameter (12th argument) and writes it to the new column. Existing data defaults to `0` (not a Non-PO item).

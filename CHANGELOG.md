# Changelog

All notable changes to the MTM Receiving Application are documented here.

---

## [Unreleased] — spreadsheet-removal branch

### Added

#### Receiving History Import (`Database/Scripts/Import-ReceivingHistory.ps1`)
- New PowerShell script to import receiving history from the Google Sheets CSV export into the MySQL `receiving_history` table.
- Silently discards blank rows and embedded Google Sheets header rows (e.g. rows where Employee = `Label #`).
- Validates each row before import — skips rows with blank Material ID, invalid quantity, non-numeric employee, or unrecognised date format; prints a summary of skipped rows.
- **PO number normalisation** — bare numbers and `PO-` prefixed values are canonicalised to `PO-NNNNNN` (6-digit zero-padded); a trailing `B` suffix is preserved and uppercased (e.g. `64489b` → `PO-064489B`). Unrecognised formats pass through unchanged.
- **Infor Visual enrichment** — before writing to MySQL the script queries Infor Visual (SQL Server) for `PartDescription` and `VendorName`. Falls back to PO-header-only lookup for closed/old POs and to the `PART` table when no PO is present. Pass `-SkipInforVisualLookup` to bypass IV entirely.
- **Non-PO item flagging** — rows with no PO number whose Part ID is not found in Infor Visual are automatically flagged `is_non_po_item = 1`.
- **Correction list** — rows with a PO number that cannot be matched to a PO + Part combination in Infor Visual are still imported but printed in a correction table at the end of the run.
- Supports `-WhatIf` dry-run mode (validates and builds the SQL batch file without executing it).
- Reads credentials via a temp `.cnf` file to avoid the MySQL CLI password warning.
- Batch executes all `CALL` statements in a single `mysql.exe` invocation for performance.

#### Google Sheets Cleanup Script (`docs/GoogleSheetsVersion/ReceivingHistoryCleanup.gs`)
- Google Apps Script that adds an **MTM Receiving** menu to the receiving history Google Sheet.
- **Batched processing** — processes up to 1,000 rows per run; saves the next start row to `Dropdowns!C2` so the run can be resumed without losing progress. Resets to row 2 automatically when the last row is reached.
- All reads and writes are batched (3 API calls per batch regardless of batch size) to stay within the Google Apps Script 6-minute time limit.
- Performs the same normalisation and validation as the PowerShell import script: PO number formatting, Heat `NONE` clearing, whitespace trimming, repeated header row removal, and row-level error/warning highlighting.
- **Reset Progress** and **Clear All Highlights** menu items included.

#### Database
- `is_non_po_item TINYINT(1) NOT NULL DEFAULT 0` column added to `receiving_history` table (`Database/Scripts/Migrate-AddIsNonPOItem.sql`).
- `sp_Receiving_History_Import` stored procedure updated to accept and store the new `p_IsNonPOItem` parameter.
- `Database/Schemas/10_Table_receiving_history.sql` updated to include the new column for fresh installs.

---

### Changed

#### Edit Mode (`Module_Receiving`)

**Bug Fixes**
- Removed duplicate XAML block that appeared after the closing `</UserControl>` tag.
- `ReceivedDate` and `PODueDate` columns were bound to `Converter_DecimalToString`; replaced with new `Converter_DateTimeToString`.
- Column bindings for `PONumber`, `POLineNumber`, `POVendor`, `POStatus`, `PODueDate` used incorrect property casing and showed nothing at runtime; corrected throughout XAML and ViewModel.
- `PackageType` column now shows the type name string via `PackageTypeName` instead of the raw enum value.

**New Features**

| Feature | Details |
|---|---|
| **Search bar** | Free-text search box with a **Search by** dropdown (All Fields, Part ID, PO Number, Heat/Lot, Employee #, Part Description, Vendor). Updates results live as you type. |
| **Result summary** | Displays record count below the search bar; shows `X of Y matching "term"` when a search is active. |
| **Column sort** | Click any column header to sort ascending; click again to reverse. An arrow indicator shows the active sort column and direction. |
| **Sort persistence** | Last-used sort column and direction are saved per user across sessions. |
| **Column chooser** | Toolbar **Columns** button opens a dialog listing all available columns with checkboxes. Includes **Select All** and **Clear All** quick-action buttons. |
| **Column visibility persistence** | Selected visible columns are saved per user to the MySQL settings table. |
| **Page-size selector** | Choose 20 / 50 / 100 / 200 rows per page from a dropdown in the footer. |
| **Page-size persistence** | Selected page size is saved per user across sessions. |
| **Loading overlay** | Semi-transparent overlay with a `ProgressRing` and status text covers the grid during data loads. |
| **Enter key on page-jump box** | Pressing Enter in the "Go to page" TextBox navigates immediately. |
| **Search clear button** | An ✕ button appears inside the search box when text is present; clears the search in one click. |
| **Help button** | Added to the toolbar, right-aligned. |

#### Receiving History Schema
- `receiving_history` table columns aligned with `receiving_label_data` structure so both tables share a common schema.
- `load_guid CHAR(36)` column added to preserve GUID-based tracking for app-generated records while allowing imported historical rows to store `NULL`.
- All affected stored procedures (`sp_Receiving_Load_Insert`, `sp_Receiving_Load_Update`, `sp_Receiving_Load_Delete`, `sp_Receiving_Load_GetAll`, `sp_Receiving_History_Get`) updated to use the new column names.
- `Dao_ReceivingLoad` updated to match (`SaveLoadsAsync`, `UpdateLoadsAsync`, `DeleteLoadsAsync`, `GetHistoryAsync`, `MapRowToLoad`).

---

### Infrastructure

- `Converter_DateTimeToString` value converter added to `Module_Core/Converters`.
- `Model_EditModeColumn` model added to `Module_Receiving/Models` for column-chooser state.
- `ReceivingSettingsKeys` and `ReceivingSettingsDefaults` extended with `EditModeColumnVisibility`, `EditModeSearchByColumn`, `EditModeSortColumn`, `EditModeSortAscending`, and `EditModePageSize` keys.
- `IService_ReceivingSettings` and `Service_ReceivingSettings` extended with `SaveStringAsync`.

---

## Previous History

For changes prior to the `spreadsheet-removal` branch see the commit history:

```
git log --oneline master
```

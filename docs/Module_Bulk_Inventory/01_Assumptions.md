# Module_Bulk_Inventory — Assumptions

> **Status: PENDING APPROVAL**
> Review all assumptions below and confirm or correct them before the Task Checklist is created.
> Items marked ⚠️ are the most likely to need clarification.

---

## About This Document

This file captures every assumption made while planning `Module_Bulk_Inventory` that was **not
explicitly stated** in the original task or legacy implementation.  Each assumption is a decision
that was inferred from context.  If any assumption is wrong, the design may need to change before
implementation begins.

---

## 1 — What the Module Does

| # | Assumption |
|---|---|
| 1.1 | The module replaces the standalone WinForms "Visual Inventory Assistant" (`MTM Receiving Manager`) for bulk warehouse inventory transactions (transfers and work-order-based receipts). |
| 1.2 | Google Sheets is removed entirely as a data source. All transaction rows are entered directly inside the WinUI 3 module. |
| 1.3 | After data entry the module sends each row to Infor Visual by launching `VMINVENT.exe` and filling its fields via Windows UI Automation (`System.Windows.Automation`). There is no REST API, import file, or ION integration — UI Automation is the only viable method confirmed by the legacy app. |
| 1.4 | Two transaction types exist exactly as in the legacy app: **Transfer** (location-to-location, no work order) and **New Transaction** (work-order-based receipt/issue). |
| 1.5 | The module does **not** write directly to the Infor Visual SQL Server database. All Visual writes must flow through the Visual UI (read-only SQL Server constraint remains in force). |

## 1 - Corrections

| # | Correction |
|---|---|
| 1.3 | After data entry, first combine all unique Part IDs and Locations by adding quantities together then push to visual, this will lower the amount of transactions needed. |

---

## 2 — Infor Visual Credentials & Credential Guard

| # | Assumption |
|---|---|
| 2.1 | The Infor Visual username and password stored in the user's MTM app profile (already stored in MySQL `main` table) are the credentials used to launch `VM.exe` / `VMINVENT.exe`. |
| 2.2 | The shared accounts `SHOP2` and `MTMDC` do **not** have inventory transaction rights in Visual, so they must be explicitly blocked.  The module will check the username case-insensitively against `["SHOP2", "MTMDC"]` before allowing any Visual interaction. |
| 2.3 ⚠️ | If a user's stored Visual credentials match `SHOP`/`SHOP2`, the module will show a blocking error message directing them to update their Visual credentials in the Settings screen before continuing. |
| 2.4 | The credential validation service (`IService_VisualCredentialValidator`) lives in **Module_Core** because any future module that launches Visual will need the same check. |
| 2.5 ⚠️ | The Visual executable path `\\visual\visual908$\VMFG\VM.exe` and `\\visual\visual908$\VMFG\VMINVENT.exe` are treated as configurable settings (default values pre-filled) rather than hardcoded strings.  This is stored in the Module_Settings area. |

| # | Correction |
|---|---|
| 2.1 | Verify by looking at the MTM_Receiving_Application's startup process where Infor Visual credencials are located |
| 2.2 | Blocked usernames corrected to `SHOP2` and `MTMDC` — confirmed by Diagram A (`06_WorkflowDiagram.md`) and `03_PreRequisites.md §P1.1`. Original assumption incorrectly listed `SHOP` and `SHOP2`. |
| 2.3 | Hide the Button in this case so the user does not even have access to it |
| 2.5 | A location for Infor Visual's path is not currently implemented in the app at all, but use `\\visual\visual908$\VMFG\{App}.exe` as the default, make sure to read Module_Settings.Core in order to see how defualt values are stored as well as how changed values are stored on the database, also update View_Settings_SharedPaths.xaml (and all connected code) to house this setting. |

---

## 3 — Data Entry Workflow

| # | Assumption |
|---|---|
| 3.1 | Users enter a list of transactions (a "batch") one row at a time inside the app.  The grid persists rows to MySQL so the batch survives a crash or restart. |
| 3.2 | Each transaction row contains: **PartID**, **From Location**, **To Location**, **Quantity**, **Work Order** (optional — blank means Transfer mode; format `WO-######` e.g. `WO-123456` when provided), and a **Status** field (Pending / InProgress / Success / Failed / Skipped). |
| 3.3 | The user works through the batch sequentially: enter data → "Push to Visual" → review result → Next row.  A "Skip" action moves the row to Skipped status without pushing. |
| 3.4 | Warehouse code is **always "002"** for both From and To warehouse.  This matches the legacy app's hardcoded behaviour.  If the business ever needs a different warehouse, it would be a settings field. |
| 3.5 | The `Lot No` field in the New Transaction mode is always **"1"** (hardcoded in the legacy app as `"4116" = "1"`).  Same assumption applies here unless corrected. |
| 3.6 | Part ID lookup from Infor Visual (via `Service_InforVisualConnect`) is used to validate the entered Part ID exists before the row is submitted.  If the part is not found, the user sees a warning but can override and continue. |

| # | Correction |
|---|---|
| 3.2 | Work Order field format confirmed as `WO-######` (e.g. `WO-123456`). |
| 3.3 | Push to visual will first consolidate all like PartIDs and Locations by adding up the quantites to lessen the amount of rows, then will start by pushing each row to visual individualy. "Push to Visual" → review result → Next row.  A "Skip" action moves the row to Skipped status without pushing.  Also a Delete action should exist to remove the row both during the push as well as during data entry incase the transfer was changed or no longer needed. |

---

## 4 — UI / Window Sizing & Placement

| # | Assumption |
|---|---|
| 4.1 | The module follows the same structural pattern as `Module_ShipRec_Tools`: a single `Page` with a host `ContentControl` that swaps between sub-views (entry grid → transaction push view → summary). |
| 4.2 | The main window stays at **1450×900 px** — no resize needed for this module's content. |
| 4.3 | The NavigationView item for Bulk Inventory is placed **after "Ship/Rec Tools"** and **before "Volvo Dunnage Requisition"** in the nav menu, based on the requirement to position it "between Module_ShipRecv_Tools and Volvo". The current nav order (`Receiving → Dunnage → Volvo → End of Day → Ship/Rec Tools`) suggests Volvo currently sits above Ship/Rec Tools; a reorder of the nav items may be needed. ⚠️ Please confirm the desired final nav order. |
| 4.4 | The module icon uses the WinUI `FontIcon` glyph `&#xE8B7;` (a box/package icon). ⚠️ Confirm or suggest preferred glyph. |

| # | Correction |
|---|---|
| 4.3 | The Navigation order for MainWindow should be as follows: Anchored to TOP: Module_Receiving, Module_Dunnage, Module_Volvo, Anchored to BOTTOM: Module_Bulk_Inventory, Module_Reporting, Module_ShipRec_Tools. |

---

## 5 — Database

| # | Assumption |
|---|---|
| 5.1 | A new MySQL table `bulk_inventory_transactions` is created in the `mtm_receiving_application` database to log every transaction row with status, timestamps, and error details. |
| 5.2 | All MySQL operations use stored procedures under `Database/StoredProcedures/BulkInventory/`. |
| 5.3 | The MySQL connection string used is the same `MySql` connection string already registered in `appsettings.json` — no new connection string key is needed. |
| 5.4 | A `Dao_BulkInventoryTransaction` DAO is created in `Module_Bulk_Inventory/Data/` and a `Service_MySQL_BulkInventory` service in `Module_Bulk_Inventory/Services/`. Both follow the standard project patterns (instance-based DAO, interface-backed service). |

---

## 6 — Module_Core Shared Services

| # | Assumption |
|---|---|
| 6.1 | A new `IService_VisualCredentialValidator` interface and implementation is added to **Module_Core** (not Module_Bulk_Inventory) because any future module that launches Visual will need this check. |
| 6.2 | A new `IService_VisualUIAutomation` interface and implementation is added to **Module_Core** to wrap all raw `System.Windows.Automation` calls (FindWindow, SendKeys, fill-by-AutomationId, wait-for-window). This prevents code duplication if a second module ever needs Visual automation. |
| 6.3 | The Visual automation service is **async-first** with configurable timeouts and `CancellationToken` support.  No `Thread.Sleep` is used; polling is done with `Task.Delay` + cancellation. |

| # | Correction |
|---|---|
| 6.2 | Make sure this is coded in a way that it does not matter what window is being used for UI Automation, if absolutly nessicary create dedicated services / models / exc. for windows that require special attention. |

---

## 7 — Settings

| # | Assumption |
|---|---|
| 7.1 ⚠️ | A settings sub-module `Module_Settings.BulkInventory` is **not** created immediately.  Initial configuration (Visual exe path override, warehouse code override) lives in the main settings page under a new "Bulk Inventory" section.  A dedicated sub-module can be added later if settings grow. |
| 7.2 | Visual credentials (VisualUserName / VisualPassword) are shared across modules via the existing `Module_Settings.Core` user profile mechanism; no duplication. |

| # | Correction |
|---|---|
| 7.1 | Warehouse / Lot No override should also live in `View_Settings_SharedPaths.xaml` as stated in Correction 2.5. |

---

## 8 — Out of Scope (for initial implementation)

- Email notifications (the legacy app had "Over Receipt / Work Order Closed / Add Remove" buttons that sent data to Google Sheets then presumably triggered emails) — these are **out of scope** for the first implementation
- Google Sheets read-back of any previously entered data — entirely removed
- ION / Infor OS integration — no evidence it is present on the workstation
- Bidirectional Visual quantity verification (the legacy app pulled quantity from AutomationId `4143` and compared to tag quantity) — deferred to a future enhancement

---

*Last Updated: 2026-03-08 — Awaiting approval to proceed to Task Checklist*

# Module_Bulk_Inventory — Suggestions for Improvement

> Improvement ideas over the legacy implementation that should be incorporated into the new module.

---

## 1 — Architecture & Design

### 1.1  Async UI Automation (Critical)
The legacy app uses `Thread.Sleep` extensively and blocks the UI thread during all Visual interactions.
The new module must use `Task.Delay` + `CancellationToken` inside `Service_VisualInventoryAutomation`
(which wraps the generic `IService_UIAutomation`) so the WinUI 3 UI stays responsive during every operation.

```csharp
// Anti-pattern from legacy (DO NOT REPEAT)
Thread.Sleep(1000);
SendKeys.Send("{TAB}");

// New pattern
Await service.FindWindowAsync("Inventory Transfers", TimeSpan.FromSeconds(10), ct);
await service.FillFieldAsync(window, "4102", partId, sendTab: true, ct);
```

### 1.2  Move All UI Automation to Module_Core as a Generic Service
Create `IService_UIAutomation` in `Module_Core/Contracts/Services/` with an implementation in
`Module_Core/Services/VisualAutomation/`.  The interface must be **window-agnostic** — it works
with any window by `hwnd` or title string, not just Infor Visual.  Visual-specific logic
(fill sequences, AutomationIds, popup dismissal) lives in a dedicated
`Service_VisualInventoryAutomation` that wraps the generic service.

If another module ever needs to drive a different application via UI Automation, it creates its own
specific service that also wraps `IService_UIAutomation` — no changes to the generic layer required.

### 1.3  Credential Guard Service in Module_Core
Create `IService_VisualCredentialValidator` in `Module_Core`.  Its sole responsibility is to check
that a given Visual username is not a shared/blocked account.

**Behaviour when credentials are blocked (`SHOP`/`SHOP2`):** The Bulk Inventory
NavigationViewItem must be **hidden entirely** — not disabled, not shown with an error dialog.
The user must update their Visual credentials in Settings before the nav item becomes visible.

```csharp
public interface IService_VisualCredentialValidator
{
    /// <summary>Returns true if the username is safe to use for Visual automation.</summary>
    bool IsAllowed(string visualUserName);

    /// <summary>Returns a developer-facing reason string when blocked, null when allowed.</summary>
    string? GetBlockedReason(string visualUserName);
}
```

---

## 2 — Data Entry UX

### 2.1  In-App Transaction Grid
Replace the Google Sheet with a `DataGrid` bound to an `ObservableCollection<Model_BulkInventoryRow>`.
Rows should be editable directly in the grid (inline editing, same as the Dunnage module's grid
pattern).  A toolbar row at the top provides "Add Row", "Delete Row", "Clear All".

**Delete Row** must be available in **both** contexts:
- During **data entry** (before push starts)
- During an **active push session** (to remove a row mid-batch that is no longer needed)

### 2.1a  Pre-Push Consolidation (Required)
Before any Visual automation begins, the app must consolidate the batch by grouping rows that share
the same **Part ID + From Location + To Location** and summing their **Quantity** values into a
single row.  This reduces the number of Visual transactions to the minimum required.

**Example:**

| Part ID | From | To | Qty |
|---------|------|----|-----|
| ABC-123 | DOCK | BIN-01 | 10 |
| ABC-123 | DOCK | BIN-01 | 5  |
| XYZ-789 | DOCK | BIN-02 | 3  |

After consolidation → only 2 Visual transactions:

| Part ID | From | To | Qty |
|---------|------|----|-----|
| ABC-123 | DOCK | BIN-01 | 15 |
| XYZ-789 | DOCK | BIN-02 | 3  |

The consolidated rows are pushed to Visual. The original raw rows remain in the grid with a
`Consolidated` status for audit purposes.

### 2.2  Per-Row Status Tracking
Each row must have a `Status` property (`Pending / InProgress / WaitingForConfirmation / Success / Failed / Skipped / Consolidated`) rendered
as a coloured badge or icon in the grid.  This replaces the implicit "we got to this index" tracking
from the legacy app.

### 2.3  Batch Pre-Validation Before Pushing
Add a **"Validate All"** button that hits Infor Visual (read-only SQL Server) to verify:
- Each Part ID exists in `PART` table
- Each location code exists for warehouse 002

Surface warnings inline in the grid (not popup dialogs) before any automation is run.

**Push Batch button state:** The Push Batch button must remain **disabled** for as long as any row
carries an unresolved validation warning.  It becomes enabled only when all rows are warning-free
(either fixed or re-validated).  There is no "push anyway" bypass — warnings must be cleared first.

### 2.4  Keyboard-First Navigation
Support `Enter` to confirm a row and advance, `Tab` between cells, `F5` to push the current row to
Visual, `F6` to skip, and `Escape` to cancel the current operation.  Power users running hundreds of
transfers per day will benefit greatly.

**All shortcuts must be labelled directly on the UI** — via button tooltips, a visible keyboard
shortcut legend strip at the bottom of the view, or both.  Users must not need to discover bindings
through trial and error or documentation.

### 2.5  Summary Screen After Full Batch
Show a completion summary (count of Successful / Failed / Skipped rows) with links to re-push
individual Failed rows before closing.

When re-pushing Failed rows, **consolidation is skipped** — each selected row is pushed as-is in its current (failed) state without re-grouping with other rows.

### 2.6  Fuzzy Search Picker for Part ID and Location Fields

When a user enters a value in the **Part ID**, **From Location**, or **To Location** cell of the
data entry grid, the existing `Dialog_FuzzySearchPicker` (from `Module_Core/Dialogs/`) must open
pre-populated with matching Infor Visual results.

**Reason:** Part IDs and location codes in MTMFG are not human-memorable and typos cause silent
Visual failures.  The picker surfaces matching candidates from the read-only SQL Server database
before any automation runs, eliminating a large class of `Row → Failed` outcomes.

#### Interaction Pattern

Each of the three columns (**Part ID**, **From Location**, **To Location**) renders a small
**🔍 search icon button** visible at the right edge of the cell.  The button is always visible
(not hover-only) to aid discoverability.  The cell also accepts typed values directly for power
users who know the exact code.

1. **User clicks the 🔍 button** (or presses `F2` while the cell is active).
2. The ViewModel command reads the cell's current typed text as an initial filter term.
3. A LIKE-based query is executed against Infor Visual SQL Server.
4. `Dialog_FuzzySearchPicker` opens pre-populated with the results and the typed value already
   applied as the filter (the built-in search box inside the dialog is pre-filled).
5. The user browses or refines the filter, then double-taps or clicks **Select**.
6. On `ContentDialogResult.Primary`, `dialog.SelectedResult!.Key` is written back into the
   corresponding property on `Model_BulkInventoryTransaction` for that row.

#### Part ID Search

- Query Infor Visual SQL Server:
  ```sql
  SELECT ID, DESCRIPTION FROM PART
  WHERE ID LIKE '%{term}%' OR DESCRIPTION LIKE '%{term}%'
  ORDER BY ID
  ```
- Map to `Model_FuzzySearchResult { Key = PART.ID, Label = PART.ID, Detail = PART.DESCRIPTION }`.
- Open picker: title `"Select Part"`, subtitle `"Matching parts for '{term}':"`.

#### Location Search (From Location / To Location)

- Query Infor Visual SQL Server, filtered to the configured default warehouse (setting key
  `BulkInventory.Defaults.WarehouseCode`):
  ```sql
  SELECT DISTINCT LOCATION FROM LOCATION
  WHERE WAREHOUSE_ID = '{warehouseCode}'
    AND LOCATION LIKE '%{term}%'
  ORDER BY LOCATION
  ```
- Map to `Model_FuzzySearchResult { Key = LOCATION, Label = LOCATION, Detail = WAREHOUSE_ID }`.
- Open picker: title `"Select Location"`, subtitle `"Locations matching '{term}' in warehouse {warehouseCode}:"`.

#### ViewModel Commands (added in T022)

```csharp
[RelayCommand]
private async Task OpenPartSearchAsync(Model_BulkInventoryTransaction row)
{
    var results = await _inforVisualService.FuzzySearchPartsAsync(row.PartId ?? string.Empty);
    if (!results.IsSuccess) return;

    var dialog = new Dialog_FuzzySearchPicker(
        results.Data,
        "Select Part",
        $"Matching parts for '{row.PartId}':")
    { XamlRoot = _xamlRoot };

    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        row.PartId = dialog.SelectedResult!.Key;
}

[RelayCommand]
private async Task OpenLocationSearchAsync((Model_BulkInventoryTransaction Row, string Field) args)
{
    var term = args.Field == "FromLocation" ? args.Row.FromLocation : args.Row.ToLocation;
    var warehouseCode = await _settingsService.GetStringAsync("BulkInventory.Defaults.WarehouseCode");
    var results = await _inforVisualService.FuzzySearchLocationsAsync(term ?? string.Empty, warehouseCode);
    if (!results.IsSuccess) return;

    var dialog = new Dialog_FuzzySearchPicker(
        results.Data,
        "Select Location",
        $"Locations matching '{term}' in warehouse {warehouseCode}:")
    { XamlRoot = _xamlRoot };

    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
    {
        if (args.Field == "FromLocation")
            args.Row.FromLocation = dialog.SelectedResult!.Key;
        else
            args.Row.ToLocation = dialog.SelectedResult!.Key;
    }
}
```

#### Service Dependencies

Both commands require:
- **Existing:** `FuzzySearchPartsAsync(string term)` — already modelled in `Module_ShipRec_Tools`;
  add an equivalent to the Infor Visual service consumed by `Module_Bulk_Inventory`.
- **New:** `FuzzySearchLocationsAsync(string term, string warehouseCode)` — new method on the
  same service, querying the `LOCATION` table.  The Infor Visual SQL Server connection string
  (`ApplicationIntent=ReadOnly`) must be used.

The concrete service is added to `Module_Bulk_Inventory/Contracts/Services/` and injected into
`ViewModel_BulkInventory_DataEntry` via its constructor — **not** called directly by the ViewModel.
The ViewModel never touches `Helper_Database_*` classes.

---

## 3 — Error Handling & Recovery

### 3.1  Per-Row Error Messages (not popups)
Errors from Visual automation should be stored in a `ErrorMessage` property on the row model and
displayed inline in the grid column, not as `MessageBox.Show` calls.  `MessageBox.Show` breaks
keyboard focus and forces the user to dismiss dialogs before they can proceed.

### 3.2  Retry Logic for Automation Failures
If a Visual window fails to appear within the timeout, retry up to a configurable number of times
(default: 2) before marking the row as Failed.

### 3.3  Graceful Visual Window Detection
Before launching `VMINVENT.exe`, check if it is already running (same as the legacy app does, but
only for `Inventory Transfers` — extend to include `Inventory Transaction Entry`).
Avoid launching duplicate processes.

**Handling the "Visual open under a different login" case:**

The main Visual hub window title follows the pattern `"Infor VISUAL - MTMFG/{username}"`.  Use
this to detect whose session is active before attempting to launch or reuse Visual:

1. Call `Process.GetProcessesByName("VMINVENT")` (and `"VM"` as a fallback) to check if Visual is running.
2. If running, use `IService_UIAutomation.FindWindowAsync` with a title prefix of `"Infor VISUAL - MTMFG/"` to locate the hub window.
3. Extract the username — the substring after the last `"/"`.
4. Compare (case-insensitive) to `IService_UserSessionManager.CurrentSession?.User.VisualUsername`.
5. **If they match:** reuse the running instance — navigate to Inventory Transfers via the existing `ALT+E, S` sequence.
6. **If they do not match:**
   - Do **not** launch a new process (a second Visual instance will conflict or fail silently).
   - Block the push session with an inline error on the view: *"Infor Visual is currently open under '{detectedUser}'. Close Visual manually or update your Visual credentials in Settings to match the active session."*
   - The push button remains disabled until the conflict is resolved (Visual closed, or credentials updated so they match).
   - Log the conflict via `IService_LoggingUtility` (detected username only — never log passwords).

**Re-check after credential update:** When the user updates their Visual credentials in Settings to match the active session, the inline conflict banner clears automatically.  The credential guard is **not** re-evaluated automatically — the user must manually re-click Push Batch to trigger the full pre-push check sequence again.

### 3.4  Cancel Button During Automation
During the push operation, display a `Cancel` button backed by a `CancellationTokenSource`.  The
Cancel button is the **only** interactive control available while the automation overlay is shown
(see §3.6).

**Cancel sequence (in order):**
1. Call `CancellationTokenSource.Cancel()` — all awaited automation steps observe the token and
   exit at their next `ct.ThrowIfCancellationRequested()` checkpoint.
2. Send `Escape` to the currently active Visual window (Inventory Transfers or Inventory
   Transaction Entry) to close any partially filled form without saving.
3. Mark the row that was `InProgress` at the time of cancel as `Skipped` and update its
   MySQL audit record accordingly.
4. Set `IsAutomationRunning = false` — this hides the full-screen overlay (§3.6) and
   re-enables all application mouse and keyboard input.
5. Navigate to the Summary screen, which shows counts including the now-Skipped row.

### 3.6  Full-Screen Automation Overlay (Required)

The moment Push Batch is confirmed and automation begins, the entire application window must be
covered by a **semi-transparent full-screen overlay** that:

- Blocks **all** mouse and keyboard input to the application (achieved by placing a
  `FullSizePassthrough=false` panel as the topmost element in the root `Grid`, with
  `IsHitTestVisible="True"` capturing all pointer and keyboard events and marking them handled).
- Displays a **status message** that updates in real time as rows are processed
  (bound to `OverlayStatusMessage` on `ViewModel_BulkInventory_Push`).
- Shows a **progress indicator** (`ProgressBar` or `ProgressRing`) and the current
  row counter (e.g., *"Processing row 3 of 12…"*).
- Exposes a single **Cancel** button (the only interactive control on the overlay) wired
  to `CancelPushCommand`, which fires the `CancellationTokenSource`.

**Status message transitions during automation:**

| State | Overlay message |
|-------|-----------------|
| Launching Visual | *"Starting Infor Visual…"* |
| Navigating to module | *"Navigating to Inventory Transfers…"* |
| Row processing (normal) | *"Processing row {n} of {total} — Part: {partId}"* |
| Parts popup dismissed | *"Resolving part lookup for {partId}…"* |
| WaitingForConfirmation | *"⚠ Visual requires your input — please respond to the dialog in Infor Visual to continue."* (message changes; Cancel button remains) |
| Cancelled by user | *"Cancelling — closing Visual window…"* |
| Complete | Overlay **hidden**; Summary screen shown |

**XAML implementation notes:**
- The overlay `Grid` sits as the last child of the page root `Grid` so it renders on top of
  everything.
- `Visibility` is bound `TwoWay` to `ViewModel_BulkInventory_Push.IsAutomationRunning`
  via a `BooleanToVisibilityConverter`.
- Background: `#AA000000` (67 % black) — dark enough to be unmistakable, light enough to
  still show the grid beneath.
- `KeyDown` and `PointerPressed` handlers on the overlay panel call `e.Handled = true` to
  swallow all input targeting the application.
- The `WaitingForConfirmation` state **does not hide** the overlay — it only updates the
  message and optionally pulses the `ProgressRing`. The user must act in the *Visual* window,
  not in the MTM app.

### 3.5  Popup-Aware Fill Loop

The legacy app handles two Visual popups that can interrupt the fill sequence.  The new service
must include the same logic, minus the pitfalls.

**Popup A — Parts lookup** (`Gupta:AccFrame` / `"Parts"`)
- **When it appears:** After Part ID (`4102`) is filled; Visual shows it when the part code is not
  immediately resolved.
- **Detection point:** After Part ID fill completes, before filling Quantity (`4111`).
  Because Visual can be slow to show this popup on a loaded network, use `WaitForPopupAsync`
  to poll for up to `PopupSettleTimeout` (configurable, default **2 s**) rather than a one-shot
  `WindowExists` check.  The popup may arrive several hundred milliseconds after the field fill
  returns — a single check would miss it entirely on slower machines.
- **Correct dismissal:**
  1. Call `IService_UIAutomation.WaitForPopupAsync("Gupta:AccFrame", "Parts", settleTimeout, pollMs: 100, ct)` — returns `hwnd` if found, `IntPtr.Zero` if not found within the timeout.
  2. If `hwnd != IntPtr.Zero`: call `SetForegroundVerified(hwnd)` and verify the return value before sending any keys (PF-07).
  3. Send `{UP}{ENTER}` to select and confirm the top result.
  4. Call `WaitForWindowToCloseAsync` with a short timeout (3 s) to confirm it dismissed.
  5. Log the occurrence (part ID + "Parts popup dismissed"); do **not** show a dialog.
  6. If `hwnd == IntPtr.Zero`: no popup appeared — continue to Quantity fill.
- **Pitfall avoided:** PF-07 — always verify foreground before sending keys.

**Popup B — Duplicate transaction warning** (`Gupta:AccFrame` / `"Inventory Transaction Entry"`)
- **When it appears:** After To Location (`4143`) is filled and TAB is sent in Transfer mode;
  Visual shows it when the same transfer already exists or when a business rule check triggers.
- **Detection point:** After the TAB on To Location.  Because Visual processes the duplicate check
  server-side before showing the popup, the window can appear hundreds of milliseconds to over a
  second later depending on server load.  A fixed 300 ms delay will miss it on slow days.
  Use `WaitForPopupAsync` to poll for up to `PopupSettleTimeout` (default **2 s**).
- **Correct handling:**
  1. Call `WaitForPopupAsync("Gupta:AccFrame", "Inventory Transaction Entry", settleTimeout, pollMs: 100, ct)`.
  2. If `hwnd == IntPtr.Zero` (popup did not appear within settle timeout): row status → `Success`.
  3. If `hwnd != IntPtr.Zero` (popup appeared):
     - Row status → `WaitingForConfirmation`.
     - UI updates to: *"Visual requires your confirmation — please respond to the dialog in Infor Visual."*
     - The `Cancel` button remains active.
     - Call `WaitForWindowToCloseAsync("Gupta:AccFrame", "Inventory Transaction Entry", 5min, pollMs: 200, ct)`
       (polls every 200 ms with `Task.Delay` — zero CPU spin).
     - On popup closed within timeout: row status → `Success`.
     - On timeout or `CancellationToken` cancelled: row status → `Failed`, error message recorded.
  4. **Do not** programmatically dismiss this popup — it requires user judgement.
- **Pitfalls avoided:** PF-04 (no CPU spin), PF-06 (only mark Success after popup is gone).

---

## 4 — Audit & Logging

### 4.1  MySQL Audit Log (Required)
Every transaction push must be logged to `bulk_inventory_transactions` **before** the Visual
automation starts, with status `InProgress`, and updated to `Success` or `Failed` after the fact.
This ensures a complete audit trail even if the app crashes mid-automation.

### 4.2  Before/After Visual State Capture
After a successful push, attempt to read back the transaction result from Visual (e.g., the new
quantity visible in the location) and store it in the audit record to confirm the operation actually
worked in Visual — not just that the keystrokes were sent.

### 4.3  Structured Serilog Log Entries
Use `IService_LoggingUtility` to emit structured log events at key points:
- Visual window found / not found  
- Field fill attempted / succeeded  
- Transaction marked Success or Failed  
- Credential validation outcome (username only — never log the password)

---

## 5 — Security Improvements Over Legacy

### 5.1  Credentials Never in Process Args
The legacy app passes `"-d MTMFG -u {user} -p {password}"` as command-line arguments to `VM.exe`.
These are visible in Task Manager / `Get-Process` output.  
**Evaluate alternatives**: 
- Store a session environment variable and pass it via `ProcessInfo.EnvironmentVariables`
- Or accept that Visual's own launch mechanism requires CLI args (document this limitation)

### 5.2  Passwords Not Stored in Plain Text Beyond Auth Session
Passwords retrieved from MySQL are held only in memory in `ApplicationVariables`-equivalent fields.
Never serialise or log them.

### 5.3  No Bundled Service Account Keys
The legacy app bundled a Google service account JSON key in the executable directory.  This key was
committed to git history and is considered compromised.  There are **no keys, tokens, or secrets**
bundled in the new module.

---

## 6 — Code Organisation

### 6.1  Dedicated Module Folder Structure

```
Module_Bulk_Inventory/
  Contracts/
    IService_BulkInventoryWorkflow.cs
  Data/
    Dao_BulkInventoryTransaction.cs
  Documentation/                     ← generated from docs/Module_Bulk_Inventory/ after approval
  Models/
    Model_BulkInventoryRow.cs
    Enum_BulkInventoryStatus.cs
    Enum_BulkInventoryTransactionType.cs
  Services/
    Service_BulkInventoryWorkflow.cs
    Service_MySQL_BulkInventory.cs
  ViewModels/
    ViewModel_BulkInventory_Workflow.cs
    ViewModel_BulkInventory_Grid.cs
    ViewModel_BulkInventory_Push.cs
    ViewModel_BulkInventory_Summary.cs
  Views/
    View_BulkInventory_Workflow.xaml
    View_BulkInventory_Grid.xaml
    View_BulkInventory_Push.xaml
    View_BulkInventory_Summary.xaml
```

### 6.2  Module_Core Additions

```
Module_Core/
  Contracts/Services/
    IService_VisualCredentialValidator.cs       ← NEW
    IService_UIAutomation.cs                    ← NEW (generic, not Visual-specific)
  Services/VisualAutomation/
    Service_VisualCredentialValidator.cs        ← NEW
    Service_UIAutomation.cs                     ← NEW (generic window automation)
    Service_VisualInventoryAutomation.cs        ← NEW (Visual fill sequences; wraps generic service)
  Models/InforVisual/
    Model_VisualInventoryTransfer.cs            ← NEW
    Model_VisualNewTransaction.cs               ← NEW
    Model_VisualConsolidatedRow.cs              ← NEW (result of pre-push consolidation)
```

---

*Last Updated: 2026-03-08 — Updated to reflect approved corrections*

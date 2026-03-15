# Plan: Non-PO Dunnage Entry — Optional PO with Saved Reasons

**Last Updated:** 2026-03-15  
**Status:** Draft — awaiting approval before implementation

---

## Problem Statement

The Dunnage Details Entry step currently **blocks the user** from advancing if the PO Number field is blank ("Cannot Proceed — Please enter a PO Number before continuing").  
The requirement is:

- PO Number is **optional**.
- When the user leaves it blank, a prompt dialog launches rather than blocking.
- The dialog lets the user type a **free-form non-PO reference** (e.g. "Stock Replenishment", "Return Dunnage", "No PO").
- Frequently-used entries can be **saved** to a pick-list and **selected** on future visits.
- The chosen/typed value is stored as the PO Number on the saved load record (just like a real PO number would be).

---

## Current State (Root Cause)

| Location | Line | What it does |
|---|---|---|
| `Service_DunnageWorkflow.AdvanceToNextStepAsync` — `DetailsEntry` case | ~L135 | Returns `IsSuccess = false` with "Please enter a PO Number" when `PONumber` is blank |
| `Service_DunnageWorkflow.AddCurrentLoadToSession` | ~L255 | Falls back to `"Nothing Entered"` when PO is blank — dead code because the block above prevents reaching it |
| `ViewModel_Dunnage_DetailsEntry.GoNextAsync` | ~L336 | Calls `_workflowService.GoToStep(Review)` directly, bypassing the service validation |

> **Note:** The `GoNextAsync` in the ViewModel skips the service-level guard and navigates directly via `GoToStep`. The ContentDialog "Cannot Proceed" is therefore being raised from the **ViewModel's own** `ValidateInputs()` call, not the service. Confirm by reading `ValidateInputs()` fully before starting Task 4.

---

## Proposed Solution

### User Flow (After)

```
DetailsEntry step — user clicks "Review"
    │
    ├─ PO Number is filled in ─────────────────────────────────► Continue as today
    │
    └─ PO Number is BLANK
           │
           ▼
     ContentDialog: "No PO Number Entered"
     ┌───────────────────────────────────────────────────────┐
     │  This looks like non-PO dunnage.                      │
     │  Enter a reference or select a saved reason:          │
     │                                                       │
     │  [TextBox — free-form entry]                          │
     │                                                       │
     │  — Recent / Saved Entries ————————————————————────   │
     │  ○  Stock Replenishment          [Delete]             │
     │  ○  Return Dunnage               [Delete]             │
     │  ○  No PO                        [Delete]             │
     │  (selecting a saved entry fills the TextBox)          │
     │                                                       │
     │  [Save this entry for next time]  ☐ (checkbox)        │
     │                                                       │
     │        [Cancel]       [Use This Reference]            │
     └───────────────────────────────────────────────────────┘
           │                          │
       User cancels            User confirms
       (stay on Details)       value stored as PoNumber
                               ──► continue to Review
```

---

## Implementation Tasks (ordered by dependency)

### Task 1 — Database Schema: `dunnage_non_po_entries` table

**File:** `Database/Schemas/XX_Table_dunnage_non_po_entries.sql`

```sql
CREATE TABLE IF NOT EXISTS dunnage_non_po_entries (
    id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    value       VARCHAR(100) NOT NULL,
    created_by  VARCHAR(100) NOT NULL,
    created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    use_count   INT UNSIGNED NOT NULL DEFAULT 1,
    UNIQUE KEY uq_value (value)
);
```

**Notes:**
- `value` is unique — inserting a duplicate increments `use_count` instead (handled in SP).
- Keep it simple: no soft-delete, no per-user scope (global shared list).

---

### Task 2 — Stored Procedures

**File:** `Database/StoredProcedures/Dunnage/sp_Dunnage_NonPO_GetAll.sql`

```sql
-- Returns all saved non-PO entries, most-used first.
CREATE PROCEDURE sp_Dunnage_NonPO_GetAll()
BEGIN
    SELECT id, value, created_by, created_at, use_count
    FROM   dunnage_non_po_entries
    ORDER  BY use_count DESC, created_at DESC;
END
```

**File:** `Database/StoredProcedures/Dunnage/sp_Dunnage_NonPO_Upsert.sql`

```sql
-- Inserts a new entry or increments use_count if the value already exists.
CREATE PROCEDURE sp_Dunnage_NonPO_Upsert(
    IN p_value      VARCHAR(100),
    IN p_created_by VARCHAR(100)
)
BEGIN
    INSERT INTO dunnage_non_po_entries (value, created_by, use_count)
    VALUES (p_value, p_created_by, 1)
    ON DUPLICATE KEY UPDATE use_count = use_count + 1;
END
```

**File:** `Database/StoredProcedures/Dunnage/sp_Dunnage_NonPO_Delete.sql`

```sql
CREATE PROCEDURE sp_Dunnage_NonPO_Delete(IN p_id INT UNSIGNED)
BEGIN
    DELETE FROM dunnage_non_po_entries WHERE id = p_id;
END
```

---

### Task 3 — Model

**File:** `Module_Dunnage/Models/Model_DunnageNonPOEntry.cs`

```csharp
public class Model_DunnageNonPOEntry
{
    public int    Id        { get; set; }
    public string Value     { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public int    UseCount  { get; set; }
}
```

---

### Task 4 — DAO: `Dao_DunnageNonPOEntry`

**File:** `Module_Dunnage/Data/Dao_DunnageNonPOEntry.cs`

Instance-based DAO following the standard pattern.

| Method | SP called | Returns |
|---|---|---|
| `GetAllAsync()` | `sp_Dunnage_NonPO_GetAll` | `Model_Dao_Result<List<Model_DunnageNonPOEntry>>` |
| `UpsertAsync(value, createdBy)` | `sp_Dunnage_NonPO_Upsert` | `Model_Dao_Result` |
| `DeleteAsync(id)` | `sp_Dunnage_NonPO_Delete` | `Model_Dao_Result` |

Use `Helper_Database_StoredProcedure.ExecuteListAsync` for `GetAllAsync`.  
Use explicit `MySqlParameter` objects with declared types for `UpsertAsync` and `DeleteAsync`.

---

### Task 5 — Service Layer additions

**Files:**
- `Module_Dunnage/Contracts/IService_MySQL_Dunnage.cs` — add three method signatures
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs` — implement them
- Register `Dao_DunnageNonPOEntry` as Singleton in `App.xaml.cs`

Signatures to add to `IService_MySQL_Dunnage`:

```csharp
Task<Model_Dao_Result<List<Model_DunnageNonPOEntry>>> GetNonPOEntriesAsync();
Task<Model_Dao_Result>                               SaveNonPOEntryAsync(string value, string createdBy);
Task<Model_Dao_Result>                               DeleteNonPOEntryAsync(int id);
```

---

### Task 6 — Remove the blocking PO guard

**File:** `Module_Dunnage/Services/Service_DunnageWorkflow.cs`

In `AdvanceToNextStepAsync`, `DetailsEntry` case — **remove** the guard:

```csharp
// REMOVE THIS:
if (string.IsNullOrWhiteSpace(CurrentSession.PONumber))
{
    return Task.FromResult(new Model_WorkflowStepResult
    {
        IsSuccess = false,
        ErrorMessage = "Please enter a PO Number before continuing."
    });
}
```

Also update `AddCurrentLoadToSession` and `CreateLoadFromCurrentSession` to use `string.Empty` instead of `"Nothing Entered"` as the fallback — the dialog will always supply a value before this point.

---

### Task 7 — Dialog: `View_Dunnage_Dialog_NonPOEntry`

**Files:**
- `Module_Dunnage/Views/View_Dunnage_Dialog_NonPOEntry.xaml`
- `Module_Dunnage/Views/View_Dunnage_Dialog_NonPOEntry.xaml.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_NonPOEntryDialog.cs`

Follow the `View_Dunnage_Dialog_AddMultipleRowsDialog` pattern (a `ContentDialog`).

**ViewModel observable properties:**

| Property | Type | Purpose |
|---|---|---|
| `EnteredValue` | `string` | Bound TwoWay to the free-form TextBox |
| `SaveEntry` | `bool` | Bound TwoWay to the "Save for next time" checkbox |
| `SavedEntries` | `ObservableCollection<Model_DunnageNonPOEntry>` | Bound to the ListView |
| `SelectedEntry` | `Model_DunnageNonPOEntry?` | When set, copies `.Value` into `EnteredValue` |
| `IsConfirmEnabled` | `bool` | `EnteredValue.Trim().Length > 0` |
| `IsBusy` | `bool` | Loading indicator while fetching/deleting |

**ViewModel commands:**

| Command | Action |
|---|---|
| `LoadEntriesCommand` | Calls `GetNonPOEntriesAsync`, populates `SavedEntries` |
| `DeleteEntryCommand(int id)` | Calls `DeleteNonPOEntryAsync`, removes from list |
| `ConfirmCommand` | If `SaveEntry`, calls `SaveNonPOEntryAsync`; returns `EnteredValue` to caller |

**Dialog result contract:**  
Code-behind exposes a `string? Result` property.  
Set `Result = ViewModel.EnteredValue` before `Hide()`.  
Caller checks for null (cancelled) or a value (confirmed).

---

### Task 8 — Wire dialog into `ViewModel_Dunnage_DetailsEntry`

**File:** `Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs`

In `GoNextAsync`, after `ValidateInputs()` passes (or replace the PO guard inside `ValidateInputs`):

```csharp
// If PO is blank, launch the non-PO dialog before advancing.
if (string.IsNullOrWhiteSpace(PoNumber))
{
    var dialog = new View_Dunnage_Dialog_NonPOEntry(_dunnageService, _logger);
    dialog.XamlRoot = /* obtain from view */;
    await dialog.ShowAsync();

    if (dialog.Result is null)
    {
        // User cancelled — stay on this step.
        return;
    }

    PoNumber = dialog.Result;
}
```

**XamlRoot access:**  
The ViewModel has a `_dispatcher` injected. The cleanest approach is to expose a `ShowNonPODialogRequested` event on the ViewModel and handle it in the View's code-behind (which has `XamlRoot`). The code-behind calls the dialog and relays the result back via a public method.

Alternatively, inject `IService_Window` (already exists) and call `GetMainWindowXamlRoot()` if that service exposes it.

---

### Task 9 — Remove/update `ValidateInputs()` in DetailsEntry ViewModel

**File:** `Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs`

The current `ValidateInputs()` likely contains the PO-required check (confirm by reading the method). Remove or make that check conditional. PO may be empty at the point `ValidateInputs` runs — the dialog intercept happens *after* validation in Task 8's flow (or reorder so the dialog intercept runs first).

---

### Task 10 — DI Registration

**File:** `App.xaml.cs`

```csharp
// DAO
services.AddSingleton(sp => new Dao_DunnageNonPOEntry(mysqlConnectionString));

// ViewModel for the dialog (Transient — new dialog each open)
services.AddTransient<ViewModel_Dunnage_NonPOEntryDialog>();
```

The dialog itself is instantiated directly in code-behind (not DI), so inject the ViewModel via `App.GetService<ViewModel_Dunnage_NonPOEntryDialog>()` in its constructor.

---

### Task 11 — Update `Service_Help.cs` (Dunnage.DetailsEntry)

**File:** `Module_Core/Services/Help/Service_Help.cs`

Update the `Dunnage.DetailsEntry` help content to read:

```
PO Number (Optional):
• Leave blank for non-PO dunnage — you will be prompted for a reference
• Enter a PO number in PO-NNNNNN format if receiving against a purchase order
```

---

## Files Touched Summary

| File | Change Type |
|---|---|
| `Database/Schemas/XX_Table_dunnage_non_po_entries.sql` | **New** |
| `Database/StoredProcedures/Dunnage/sp_Dunnage_NonPO_GetAll.sql` | **New** |
| `Database/StoredProcedures/Dunnage/sp_Dunnage_NonPO_Upsert.sql` | **New** |
| `Database/StoredProcedures/Dunnage/sp_Dunnage_NonPO_Delete.sql` | **New** |
| `Module_Dunnage/Models/Model_DunnageNonPOEntry.cs` | **New** |
| `Module_Dunnage/Data/Dao_DunnageNonPOEntry.cs` | **New** |
| `Module_Dunnage/Contracts/IService_MySQL_Dunnage.cs` | **Add 3 methods** |
| `Module_Dunnage/Services/Service_MySQL_Dunnage.cs` | **Add 3 implementations** |
| `Module_Dunnage/Services/Service_DunnageWorkflow.cs` | **Remove PO guard, fix fallback** |
| `Module_Dunnage/ViewModels/ViewModel_Dunnage_NonPOEntryDialog.cs` | **New** |
| `Module_Dunnage/Views/View_Dunnage_Dialog_NonPOEntry.xaml` | **New** |
| `Module_Dunnage/Views/View_Dunnage_Dialog_NonPOEntry.xaml.cs` | **New** |
| `Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs` | **Modify GoNextAsync + ValidateInputs** |
| `App.xaml.cs` | **Register DAO + ViewModel** |
| `Module_Core/Services/Help/Service_Help.cs` | **Update help text** |

---

## Open Questions (resolve before starting)

1. **XamlRoot injection** — confirm whether `IService_Window.GetMainWindowXamlRoot()` exists and is accessible from the DetailsEntry ViewModel, or whether the event-callback pattern should be used instead.
2. **`ValidateInputs()` contents** — read the full method to confirm it is the source of the current "Cannot Proceed" dialog (the service guard appears dead based on how `GoNextAsync` bypasses it via direct `GoToStep`).
3. **Max saved entries** — should there be a limit (e.g. 20)? Current plan has none.
4. **Delete confirmation** — should deleting a saved entry require a secondary confirm, or is a direct delete acceptable?
5. **Table prefix numbering** — confirm the next available schema file number (`XX_`) by checking `Database/Schemas/`.

# Bulk Inventory — Method 1: Clipboard-Paste + Tab Navigation Plan

Last Updated: 2026-03-10

## 1. Method Overview

Instead of locating fields by AutomationId (current code) or by image recognition, this method
drives VISUAL entirely through **keyboard Tab navigation** combined with **clipboard paste** for
values. It works because:

- The PowerBuilder form tab order is fixed and deterministic — Tab always moves to the next
  editable field in the same sequence, regardless of VISUAL version or control IDs.
- Clipboard paste (`Ctrl+V`) is atomic — it deposits the entire value at once with no
  character-by-character simulation, so special characters (parentheses, slashes, etc.) cannot
  be corrupted.
- **No AutomationIds, no image templates, no external tools.**

---

## 2. Why This Is More Reliable Than the Current Approach

| Factor | Current (`SendKeys` + AutomationId) | Method 1 (Tab + Clipboard) |
|---|---|---|
| Breaks on VISUAL upgrade | Yes — AutomationIds renumber | No — Tab order is stable within a layout |
| Special character corruption | Yes — `SendKeys` escaping bugs | No — clipboard is atomic |
| Works on DataWindow cells | No | No (same limitation) |
| Setup effort | Zero (already done) | Measure tab stops once per window |
| Code complexity | Low | Low |
| Dependencies | `System.Windows.Automation` | Win32 `SendInput` + WinRT Clipboard (both already available) |

The only limitation shared with the current approach: neither method directly targets grid
cells inside a DataWindow. For the Inventory Transfers and Transaction Entry windows, all
editable fields are standard form fields — **no DataWindow grid is involved** — so both windows
are fully automatable with this method.

---

## 3. Architecture

### 3.1 No new interfaces required

The existing `IService_UIAutomation` is extended with two new methods:

```csharp
/// <summary>
/// Brings the VISUAL window to the foreground, clicks at the specified
/// client-area offset to anchor focus to the first field, then waits for focus.
/// </summary>
Task FocusFirstFieldAsync(IntPtr hwnd, int clickOffsetX, int clickOffsetY, CancellationToken ct = default);

/// <summary>
/// Puts <paramref name="value"/> on the clipboard and sends Ctrl+A, Ctrl+V
/// to overwrite the currently focused field; then sends <paramref name="tabsAfter"/>
/// Tab strokes to advance focus.
/// Uses Win32 SendInput — no character-by-character typing.
/// </summary>
Task PasteAndTabAsync(string value, int tabsAfter = 1, CancellationToken ct = default);
```

Both are implemented in `Service_UIAutomation` using `user32.dll` `SendInput` for keyboard
events and `Windows.ApplicationModel.DataTransfer.Clipboard` for the paste value.

### 3.2 Configuration-driven field sequences

Each window's fill sequence is defined in `appsettings.json`, not in C#. If VISUAL changes
tab order, only the config changes — no recompilation.

```json
"BulkInventory": {
  "Automation": {
    "TransferWindow": {
      "WindowTitle":  "Inventory Transfers - Infor VISUAL - MTMFG",
      "WindowClass":  "Gupta:AccFrame",
      "FocusClickOffsetX": 110,
      "FocusClickOffsetY": 95,
      "Fields": [
        { "Name": "PartId",        "TabsAfter": 1 },
        { "Name": "Quantity",      "TabsAfter": 1 },
        { "Name": "FromWarehouse", "TabsAfter": 1 },
        { "Name": "FromLocation",  "TabsAfter": 1 },
        { "Name": "ToWarehouse",   "TabsAfter": 1 },
        { "Name": "ToLocation",    "TabsAfter": 1 }
      ]
    },
    "EntryWindow": {
      "WindowTitle":  "Inventory Transaction Entry - Infor VISUAL - MTMFG",
      "WindowClass":  "Gupta:AccFrame",
      "FocusClickOffsetX": 110,
      "FocusClickOffsetY": 95,
      "Fields": [
        { "Name": "WorkOrder",   "TabsAfter": 1 },
        { "Name": "LotNo",       "TabsAfter": 1 },
        { "Name": "Quantity",    "TabsAfter": 1 },
        { "Name": "ToWarehouse", "TabsAfter": 1 },
        { "Name": "ToLocation",  "TabsAfter": 1 }
      ]
    }
  }
}
```

### 3.3 Updated `Service_VisualInventoryAutomation`

`ExecuteTransferAsync` is rewritten from:
```csharp
await _ui.FillFieldAsync(window, "4102", row.PartId, sendTab: false, ct);
```
to:
```csharp
await _ui.FocusFirstFieldAsync(hwnd, config.FocusClickOffsetX, config.FocusClickOffsetY, ct);
foreach (var field in config.Fields)
{
    var value = GetFieldValue(row, field.Name);
    await _ui.PasteAndTabAsync(value, field.TabsAfter, ct);
}
```

`GetFieldValue` is a simple private switch on the field name string — no reflection.

### 3.4 Clipboard restoration

After the batch completes, the clipboard is restored to its previous content so the user's
copied data is not silently destroyed. This is handled inside `PasteAndTabAsync`:
1. Save current clipboard content.
2. Set the batch value.
3. Send Ctrl+A + Ctrl+V.
4. Restore previous clipboard content.

---

## 4. What Is NOT Changing

| Component | Stays the same |
|---|---|
| `IService_VisualInventoryAutomation` interface | Same method signatures |
| Popup detection (`WaitForPopupAsync`, `WaitForWindowToCloseAsync`) | No change |
| Window discovery (`FindWindow` by class + title) | No change |
| Row status model, MySQL persistence | No change |
| `ViewModel_BulkInventory_Push` | No change |

The `System.Windows.Automation` namespace and all numeric AutomationId constants are removed
from `Service_VisualInventoryAutomation`. The UIA-based `FindWindowAsync` in
`Service_UIAutomation` is no longer needed for field filling (only Win32 `FindWindow` by hwnd
is used) but can remain for backward compatibility.

---

## 5. Phased Implementation

### Phase 1 — Extend `IService_UIAutomation`

- Add `FocusFirstFieldAsync` and `PasteAndTabAsync` to the interface and implementation.
- Unit test `PasteAndTabAsync` using a WinForms TextBox in a test harness to verify the
  clipboard-paste and Tab-count logic without needing VISUAL running.

### Phase 2 — Configuration Model

- Create `Model_VisualAutomationWindowConfig` and `Model_VisualFieldConfig` POCOs.
- Bind them from `appsettings.json` using `IOptions<T>`.
- Populate initial JSON values using the tab-count measurement results from the human tasks.

### Phase 3 — Migrate `Service_VisualInventoryAutomation`

- Replace `FillFieldAsync` calls with the config-driven loop.
- Remove `using System.Windows.Automation`.
- Full manual end-to-end test.

### Phase 4 — Hardening

- Add a `PasteDelayMs` setting (default: `50`) between Tab strokes to handle slower VISUAL responses.
- Log each field name and value (value truncated to first 4 chars for security) on fill.
- Add `MaxTabAttempts` guard to stop infinite tab-loops if VISUAL doesn't respond.

---

## 6. Risk Table

| Risk | Likelihood | Mitigation |
|---|---|---|
| Tab order changes after VISUAL upgrade | Low | Update only `appsettings.json` — no code change |
| VISUAL opens a popup mid-tab sequence (e.g. Parts lookup) | Medium | Existing popup detection loop (unchanged) handles this |
| Focus escapes to another app mid-batch | Low | `SetForegroundWindow` + `GetForegroundWindow` verification before each paste |
| Clipboard race with user copy-paste | Low | Clipboard content is saved and restored; user is warned when batch is running |
| Wrong field count (tab miscounted by human) | Medium | Log field name being filled; mismatch is obvious immediately during first test |

---

## 7. Files to Create / Modify

| Action | File |
|---|---|
| MODIFY | `Module_Core/Contracts/Services/IService_UIAutomation.cs` (add 2 methods) |
| MODIFY | `Module_Core/Services/VisualAutomation/Service_UIAutomation.cs` (implement 2 methods) |
| CREATE | `Module_Bulk_Inventory/Models/Config/Model_VisualAutomationWindowConfig.cs` |
| MODIFY | `Module_Bulk_Inventory/Services/Service_VisualInventoryAutomation.cs` (remove AutomationIds) |
| MODIFY | `appsettings.json` (add `BulkInventory:Automation` section) |
| MODIFY | `Infrastructure/DependencyInjection/` (bind new config POCOs) |

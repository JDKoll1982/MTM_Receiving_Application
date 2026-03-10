# Bulk Inventory — Method 2: Win32 Child HWND Enumeration + WM_SETTEXT Plan

Last Updated: 2026-03-10

## 1. Method Overview

This method enumerates every child window handle (HWND) inside the VISUAL form using the
Win32 `EnumChildWindows` function, identifies which HWND corresponds to which field by
matching its screen position against a stored positional map, then injects values directly
via `SendMessage(WM_SETTEXT)` — with **zero keyboard simulation for data**.

Navigation between fields uses `PostMessage(WM_KEYDOWN, VK_TAB)` to trigger VISUAL's
field-commit logic after each value is set.

---

## 2. Why This Is More Reliable Than the Current Approach

| Factor | Current (`SendKeys` + AutomationId) | Method 2 (HWND Enum + WM_SETTEXT) |
|---|---|---|
| Breaks on VISUAL upgrade | Yes — numeric AutomationIds renumber | Partial — positional map needs re-measuring if layout changes |
| Data corruption with special chars | Yes — `SendKeys` has escaping bugs | No — `WM_SETTEXT` injects raw string |
| Requires window to be in foreground | Yes | No — `SendMessage` works on background windows |
| Works on DataWindow grid cells | No | No (grid cells are not separate HWNDs) |
| Typing simulation for data | Yes (current `FillFieldAsync`) | No (WM_SETTEXT is direct) |
| Setup effort | Zero (already done) | Run diagnostic tool once per window |

### Key advantage over image recognition (Method 0) and Tab-count (Method 1)

`WM_SETTEXT` writes directly to the control's text buffer — it does not send keystrokes.
Combined with the fact that **VISUAL does not need to be the foreground window** for
`SendMessage` to work, this method is the least intrusive: the operator can keep another
window open while a batch runs in the background.

---

## 3. How Win32 HWND Enumeration Works on PowerBuilder Apps

PowerBuilder generates standard Win32 `HWND` child controls for form-based edit fields
(unlike DataWindow grid cells, which are drawn on a single canvas HWND). For the Inventory
Transfers and Transaction Entry windows, all fill targets are **standard edit controls**
with real HWNDs.

`EnumChildWindows(parentHwnd, callback, 0)` visits every descendant HWND. For each:
- `GetClassName(hwnd)` — returns the Win32 class; PowerBuilder edit fields typically report
  `"Edit"` or `"PBVM_EDIT"` or `"PBCtrl"`.
- `GetWindowRect(hwnd)` — returns the screen bounding box.
- `IsWindowVisible(hwnd)` — filters out hidden controls.

Fields are sorted by `(top, left)` after converting from screen to client coordinates,
giving a stable top-to-bottom, left-to-right ordinal. The ordinal is then mapped to a
field name via `appsettings.json`.

---

## 4. Architecture

### 4.1 New interface methods on `IService_UIAutomation`

```csharp
/// <summary>
/// Enumerates all visible child HWNDs of <paramref name="parentHwnd"/>,
/// filters to controls matching <paramref name="classFilter"/> (null = all),
/// and returns them sorted top-to-bottom then left-to-right by screen position.
/// </summary>
List<IntPtr> EnumerateChildWindows(IntPtr parentHwnd, string? classFilter = null);

/// <summary>
/// Injects <paramref name="value"/> into the control at <paramref name="hwnd"/>
/// using WM_SETTEXT — no keystroke simulation.
/// Sends WM_KEYDOWN VK_TAB after injection if <paramref name="sendTab"/> is true.
/// </summary>
void SetTextDirect(IntPtr hwnd, string value, bool sendTab = false);
```

Both are implemented in `Service_UIAutomation` using pure P/Invoke — no new NuGet packages.

### 4.2 Positional field map in `appsettings.json`

Instead of AutomationIds or Tab counts, this method uses a **zero-based ordinal** — the
position of the control in the sorted enumeration result.

```json
"BulkInventory": {
  "Automation": {
    "TransferWindow": {
      "WindowClass": "Gupta:AccFrame",
      "WindowTitle":  "Inventory Transfers - Infor VISUAL - MTMFG",
      "EditWindowClass": "Edit",
      "FieldOrdinalMap": {
        "PartId":        0,
        "Quantity":      1,
        "FromWarehouse": 2,
        "FromLocation":  3,
        "ToWarehouse":   4,
        "ToLocation":    5
      }
    },
    "EntryWindow": {
      "WindowClass": "Gupta:AccFrame",
      "WindowTitle":  "Inventory Transaction Entry - Infor VISUAL - MTMFG",
      "EditWindowClass": "Edit",
      "FieldOrdinalMap": {
        "WorkOrder":   0,
        "LotNo":       1,
        "Quantity":    2,
        "ToWarehouse": 3,
        "ToLocation":  4
      }
    }
  }
}
```

The human developer determines the correct ordinals by running the built-in diagnostic
tool (see Human Tasks file). The ordinals are the only thing that changes if VISUAL updates.

### 4.3 Diagnostic tool (dev-mode feature)

A `DevTools` button in the Settings module (or a keyboard shortcut, e.g. `Ctrl+Alt+D`)
triggers `IService_UIAutomation.EnumerateChildWindows` against the currently open VISUAL
window and writes a table to the application log:

```
Ordinal | HWND       | Class | Left | Top  | Right | Bottom | Visible
0       | 0x00030A18 | Edit  | 340  | 112  | 540   | 132    | True
1       | 0x00030A20 | Edit  | 340  | 145  | 440   | 165    | True
...
```

This log entry is produced at `Information` level and written to the Serilog log file.
The developer opens the log, reads the ordinals, and populates `appsettings.json`.

### 4.4 Updated `Service_VisualInventoryAutomation`

`ExecuteTransferAsync` changes from:
```csharp
await _ui.FillFieldAsync(window, "4102", row.PartId, sendTab: false, ct);
```
to:
```csharp
var hwnd = _ui.FindWindowByClassAndTitle(VisualClassName, TransferWindowTitle);
var children = _ui.EnumerateChildWindows(hwnd, config.EditWindowClass);

foreach (var (fieldName, ordinal) in config.FieldOrdinalMap)
{
    var fieldHwnd = children[ordinal];
    var value = GetFieldValue(row, fieldName);
    _ui.SetTextDirect(fieldHwnd, value, sendTab: true);
    await Task.Delay(config.InterFieldDelayMs, ct);
}
```

`System.Windows.Automation` import and all numeric AutomationId constants are removed.

---

## 5. What Is NOT Changing

| Component | Stays the same |
|---|---|
| `IService_VisualInventoryAutomation` interface | Same method signatures |
| Popup detection | No change — these rely on `FindWindow` by class+title, not HWND children |
| Row status model, MySQL persistence | No change |
| `ViewModel_BulkInventory_Push` | No change |

---

## 6. Phased Implementation

### Phase 1 — Build EnumChildWindows + SetTextDirect

- Add `EnumerateChildWindows` and `SetTextDirect` to `IService_UIAutomation` and implementation.
- Unit test in isolation: open a WinForms test window with 3 TextBox controls, call
  `EnumerateChildWindows` on it, verify ordinals match screen order, then inject values with
  `SetTextDirect` and verify.

### Phase 2 — Diagnostic Tool

- Add a DevTools entry or keyboard shortcut that calls the enumeration against the currently
  open VISUAL window and writes the child window table to the Serilog log.
- Guard it with `#if DEBUG` or a `"DevMode": true` `appsettings.json` flag.

### Phase 3 — Human Calibration

- Human opens VISUAL and runs the diagnostic (see Human Tasks file).
- Human reads the log output, maps ordinals to field names, updates `appsettings.json`.

### Phase 4 — Migrate `Service_VisualInventoryAutomation`

- Replace `FillFieldAsync` calls with the HWND enumeration + `SetTextDirect` loop.
- Remove `using System.Windows.Automation`.
- Full manual end-to-end test.

### Phase 5 — Hardening

- Add `MaxEnumeratedControls` guard (warn+fail if count changes unexpectedly).
- Log the HWND map at `Debug` level on each push run.
- Add `InterFieldDelayMs` config (default: `150 ms`) to allow VISUAL field processing time.

---

## 7. Risk Table

| Risk | Likelihood | Mitigation |
|---|---|---|
| VISUAL changes field layout (ordinals shift) | Low-Medium | Re-run diagnostic; update ordinals in `appsettings.json` only |
| PowerBuilder uses non-`Edit` class for some fields | Medium | Diagnostic reveals the actual class name; update `EditWindowClass` in config |
| `WM_SETTEXT` accepted but VISUAL does not mark field as dirty | Low-Medium | Follow with `PostMessage(WM_KEYDOWN, VK_TAB)` — Tab triggers VISUAL's commit logic |
| HWND order not stable between form opens | Low | PowerBuilder creates controls in declaration order; sort by position adds extra stability |
| Hidden or read-only controls at ordinal 0–N shifting the map | Medium | Diagnostic filters `IsWindowVisible = true`; re-run after any VISUAL update |

---

## 8. Files to Create / Modify

| Action | File |
|---|---|
| MODIFY | `Module_Core/Contracts/Services/IService_UIAutomation.cs` (add 2 methods) |
| MODIFY | `Module_Core/Services/VisualAutomation/Service_UIAutomation.cs` (implement 2 methods) |
| CREATE | `Module_Bulk_Inventory/Models/Config/Model_VisualAutomationWindowConfig.cs` |
| MODIFY | `Module_Bulk_Inventory/Services/Service_VisualInventoryAutomation.cs` (remove AutomationIds) |
| MODIFY | `appsettings.json` (add `BulkInventory:Automation` section) |
| MODIFY | `Module_Settings/` (add DevTools trigger for diagnostic) |

# Bulk Inventory — RPA Image Recognition Migration Plan

Last Updated: 2026-03-10

## 1. Research Validation

The research is **confirmed correct** against the current codebase.

### Current Approach and Its Fragility

`Service_UIAutomation.cs` uses `System.Windows.Automation` with hardcoded numeric
`AutomationId` strings (e.g. `"4102"`, `"4111"`, `"4143"`) and injects values with
`WinForms.SendKeys.Send(value)` — **keystroke simulation, character-by-character**.

This works today because:
- The Inventory Transfers and Transaction Entry windows happen to expose a few standard
  Windows edit controls that UIA can find by those numeric IDs.
- A "Gupta:AccFrame" main window wraps them, but the inner edit controls are accessible.

This **will break** when any of the following change:
- Infor VISUAL upgrades and renumbers controls (PowerBuilder `taborder` changes = new AutomationIds).
- A new field is inserted above an existing one (renumbers all downstream AutomationIds).
- The user resizes the window or changes layout settings in VISUAL.
- A DataWindow grid cell (not a standard edit control) needs to be targeted — UIA returns nothing for these.

### Why Image Recognition + Clipboard Paste Survives Those Changes

An image-based approach locates fields by *what they look like on screen*, not by internal
control IDs. Clipboard injection (`SetClipboardData` + Win32 `SendInput` for Ctrl+V) deposits
the full value atomically — no simulated typing per character, so special characters (e.g. `{`
which `SendKeys` must escape) cannot corrupt data.

---

## 2. Constraint: No Third-Party Executables

**Only embeddable dependencies are permitted.** This rules out UiPath, Power Automate Desktop,
AutoHotkey, and Automation Anywhere as separate installs.

Everything below is achievable with:

| Dependency | Type | Status |
|---|---|---|
| `System.Drawing.Common` | Microsoft NuGet (embedded .dll) | Add to `.csproj` |
| `user32.dll` / `gdi32.dll` | Win32 (every Windows machine) | P/Invoke — no install |
| WinRT Clipboard API | Windows App SDK (already referenced) | Already available |
| `SendInput` (Win32) | Part of `user32.dll` | P/Invoke — no install |

No AutoHotkey, no Power Platform, no separate process, no installer.

---

## 3. Architecture Overview

### 3.1 New Service Layer

```
IService_ScreenCapture          captures a named VISUAL window to a Bitmap
IService_TemplateMatch          locates a template image inside a captured Bitmap
IService_ClipboardInjector      puts text on clipboard, clicks target, sends Ctrl+V
```

These three services are **generic** (window-agnostic), live in `Module_Core`, and replace
the current `WinForms.SendKeys` field-filling in `IService_UIAutomation`.

`IService_VisualInventoryAutomation` (the Bulk Inventory-specific orchestrator) is updated
to call the new services instead of `_ui.FillFieldAsync(automationId, ...)`.

### 3.2 Template Images

Each field that must be clicked is identified by a small screenshot of its **label** — the
text label that appears immediately left of the input box in VISUAL. Labels do not change
position relative to their input fields within the same VISUAL window layout.

Example templates needed:

| Window | Template image file | Targets |
|---|---|---|
| Inventory Transfers | `tpl_transfer_partid_label.png` | "Part" label → click input to its right |
| Inventory Transfers | `tpl_transfer_qty_label.png` | "Quantity" label |
| Inventory Transfers | `tpl_transfer_fromwarehouse_label.png` | "From Wh" label |
| Inventory Transfers | `tpl_transfer_fromlocation_label.png` | "From Location" label |
| Inventory Transfers | `tpl_transfer_towarehouse_label.png` | "To Wh" label |
| Inventory Transfers | `tpl_transfer_tolocation_label.png` | "To Location" label |
| Transaction Entry | `tpl_entry_workorder_label.png` | "Work Order" label |
| Transaction Entry | `tpl_entry_lotno_label.png` | "Lot No" label |
| Transaction Entry | `tpl_entry_qty_label.png` | "Quantity" label |
| Transaction Entry | `tpl_entry_towarehouse_label.png` | "To Wh" label |
| Transaction Entry | `tpl_entry_tolocation_label.png` | "To Location" label |

Templates are stored as embedded resources:
`Module_Bulk_Inventory/Assets/VisualTemplates/*.png`

### 3.3 Field Click Offset

When the template match returns the bounding box of the label, the actual input field is
offset a fixed number of pixels to the right. This offset is measured once and stored in
`appsettings.json` under `BulkInventory:Automation:FieldOffsetX` (default: `120`).

---

## 4. New Services — Specifications

### 4.1 `IService_ScreenCapture` (Module_Core)

```csharp
public interface IService_ScreenCapture
{
    /// <summary>
    /// Captures the client area of the VISUAL window (matched by class + title) to a Bitmap.
    /// Returns null if the window is not found.
    /// Uses Win32 PrintWindow — does not require the window to be in the foreground.
    /// </summary>
    Bitmap? CaptureWindow(string className, string windowTitle);
}
```

Implementation: `PrintWindow` P/Invoke (user32.dll) — captures without bringing the window
to the front, works even when partially occluded.

### 4.2 `IService_TemplateMatch` (Module_Core)

```csharp
public interface IService_TemplateMatch
{
    /// <summary>
    /// Finds the top-left corner of <paramref name="template"/> inside <paramref name="source"/>.
    /// Returns null if the match score is below <paramref name="threshold"/> (0.0–1.0, default 0.85).
    /// Uses Sum of Squared Differences (SSD) with LockBits for performance.
    /// </summary>
    System.Drawing.Point? FindTemplate(Bitmap source, Bitmap template, double threshold = 0.85);
}
```

Algorithm: Normalised SSD over RGB channels using `Bitmap.LockBits` (unsafe pointers).
No external image processing library required.

### 4.3 `IService_ClipboardInjector` (Module_Core)

```csharp
public interface IService_ClipboardInjector
{
    /// <summary>
    /// Moves the cursor to <paramref name="screenPoint"/>, left-clicks to focus the field,
    /// sets <paramref name="value"/> on the clipboard, then sends Ctrl+V.
    /// Uses Win32 SendInput — no character-by-character keystroke simulation.
    /// </summary>
    Task InjectValueAsync(System.Drawing.Point screenPoint, string value, CancellationToken ct = default);
}
```

Implementation steps inside `InjectValueAsync`:
1. `SetCursorPos(x, y)` + `SendInput` mouse left-down/up at the target point.
2. `SendInput` Ctrl+A → Ctrl+C (select-all, clear) — optional, used to replace existing content.
3. `OleSetClipboard` or `Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(...)` to put `value` on clipboard.
4. `SendInput` Ctrl+V to paste.

### 4.4 Updated `Service_VisualInventoryAutomation` (Module_Bulk_Inventory)

Replace the current `_ui.FillFieldAsync(window, automationId, value)` calls with a new
private method:

```csharp
private async Task FillByImageAsync(
    string windowClass, string windowTitle,
    string templateResourceName,
    string value,
    CancellationToken ct)
```

Which:
1. Calls `_screenCapture.CaptureWindow(windowClass, windowTitle)` to get a fresh screenshot.
2. Loads the embedded template from the assembly manifest.
3. Calls `_templateMatch.FindTemplate(screenshot, template)` to get label coordinates.
4. Adds the configured `FieldOffsetX` to get the input coordinates.
5. Converts client-area coordinates to screen coordinates (`ClientToScreen` P/Invoke).
6. Calls `_clipboardInjector.InjectValueAsync(screenCoords, value, ct)`.

The `System.Windows.Automation` import and all AutomationId constants in
`Service_VisualInventoryAutomation` are removed. Window discovery by hwnd
(`FindWindow` returning `IntPtr`) is retained for popup detection —
this still works fine because `Gupta:AccFrame` is a real top-level window.

---

## 5. What is NOT Changing

| Component | Stays the same |
|---|---|
| `IService_VisualInventoryAutomation` (interface) | Same method signatures: `ExecuteTransferAsync`, `ExecuteNewTransactionAsync` |
| `Service_MySQL_BulkInventory` | No changes |
| `ViewModel_BulkInventory_Push` | No changes |
| Popup detection (`WaitForPopupAsync`, `WaitForWindowToCloseAsync`) | Stays — these use reliable Win32 `FindWindow` by class+title |
| Row status model (`Enum_BulkInventoryStatus`) | No changes |
| MySQL persistence (`Dao_BulkInventoryTransaction`) | No changes |

---

## 6. Phased Implementation

### Phase 1 — Build and Test New Core Services (no change to automation yet)

- Add `System.Drawing.Common` NuGet to `.csproj`.
- Implement `Service_ScreenCapture` using `PrintWindow`.
- Implement `Service_TemplateMatch` using SSD + LockBits.
- Implement `Service_ClipboardInjector` using `SendInput` + WinRT Clipboard.
- Write unit tests for `Service_TemplateMatch` using synthetic bitmaps.
- Register all three in DI.

### Phase 2 — Capture and Embed Template Images

- Open VISUAL in the target environment.
- Use the Windows Snipping Tool (or a helper script) to capture each label as a PNG
  cropped tightly around the label text (see Section 3.2).
- Add images to `Module_Bulk_Inventory/Assets/VisualTemplates/` as
  `EmbeddedResource` in the `.csproj`.
- Document each template's expected VISUAL window state (see Human Tasks file).

### Phase 3 — Migrate `Service_VisualInventoryAutomation`

- Add `FillByImageAsync` private method.
- Replace all `_ui.FillFieldAsync(window, "4102", ...)` calls with `FillByImageAsync(...)`.
- Remove `using System.Windows.Automation` import from the service file.
- Keep `IService_UIAutomation` for popup detection only (or deprecate it separately).
- Full manual end-to-end test in VISUAL against a real transaction.

### Phase 4 — Hardening

- Add a `FieldOffsetX` override per template (in case some fields have wider labels).
- Add a `ConfidenceThreshold` setting in `appsettings.json` (`BulkInventory:Automation:MatchThreshold`, default `0.85`).
- Log the match score for each field fill to aid diagnosis.
- Implement a fallback: if template match fails, mark row `Failed` with message
  `"Template 'tpl_transfer_partid_label.png' not matched (score: 0.72). Verify VISUAL layout."`.
- Stress test with 50-row batches.

---

## 7. Risk Table

| Risk | Likelihood | Mitigation |
|---|---|---|
| VISUAL window scaling/DPI changes template size | Medium | Capture templates at the production machine's exact DPI. Re-capture if DPI changes. |
| VISUAL window is minimized or occluded during push | Low | `PrintWindow` captures hidden windows. Bring VISUAL to foreground before first field fill. |
| Two identical labels in the same window | Low | Use the most visually distinctive crop; include a border pixel or neighbouring text. |
| Clipboard used by another app during batch | Low | Acquire clipboard (retry up to 3x with 50ms delay). Restore previous clipboard content after paste. |
| Template not found (score below threshold) | Medium | Configurable threshold + clear `Failed` message with score for diagnosis. |
| VISUAL version upgrade changes field layout | Low-Medium | Re-capture affected templates. Only the `.png` files change, not C# code. |

---

## 8. Files to Create / Modify

| Action | File |
|---|---|
| CREATE | `Module_Core/Contracts/Services/IService_ScreenCapture.cs` |
| CREATE | `Module_Core/Contracts/Services/IService_TemplateMatch.cs` |
| CREATE | `Module_Core/Contracts/Services/IService_ClipboardInjector.cs` |
| CREATE | `Module_Core/Services/VisualAutomation/Service_ScreenCapture.cs` |
| CREATE | `Module_Core/Services/VisualAutomation/Service_TemplateMatch.cs` |
| CREATE | `Module_Core/Services/VisualAutomation/Service_ClipboardInjector.cs` |
| CREATE | `Module_Bulk_Inventory/Assets/VisualTemplates/*.png` (template images — human task) |
| MODIFY | `Module_Bulk_Inventory/Services/Service_VisualInventoryAutomation.cs` |
| MODIFY | `Module_Bulk_Inventory/MTM_Receiving_Application.csproj` (add NuGet + EmbeddedResource) |
| MODIFY | `appsettings.json` (add `BulkInventory:Automation` section) |
| MODIFY | `Infrastructure/DependencyInjection/` (register new services) |

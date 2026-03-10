# Method 2: Win32 Child HWND Enumeration — Human Developer Tasks

Last Updated: 2026-03-10

This method requires running a diagnostic tool from the MTM app to discover the internal
HWND structure of the VISUAL form. No screenshots, no tab counting — just open VISUAL,
run the diagnostic, read the log.

---

## Prerequisites

- [ ] Infor VISUAL installed and logged in to MTMFG.
- [ ] Developer has built Phase 2 of this method (diagnostic tool is in the app).
- [ ] The MTM app's Serilog log file is accessible (default: `logs/app-YYYYMMDD.txt`).

---

## Step 1 — Open the Inventory Transfers Window in VISUAL

1. Log in to VISUAL using your normal credentials.
2. Navigate to **Inventory → Inventory Transfers**.
3. Leave the window open and **empty** (no data filled in).
4. Make sure the window is at the size you normally use. Do **not** maximize it.
5. Do not click inside the VISUAL window after opening it.

---

## Step 2 — Run the Diagnostic Tool (Transfer Window)

1. Switch to the MTM Receiving Application.
2. Open **Settings → Developer Tools** (or press the configured keyboard shortcut, e.g. `Ctrl+Alt+D`).
3. Click **"Enumerate VISUAL Child Windows"** (or whatever the developer labelled it).
4. The app will enumerate all child controls of the `Inventory Transfers` window and write
   a table to the Serilog log file.

---

## Step 3 — Read the Log and Record Ordinals

1. Open the latest log file at `logs/app-<today's date>.txt`.
2. Search for `EnumerateChildWindows` or `HWND Diagnostic`.
3. You will see a table like this:

   ```
   Ordinal | HWND       | Class | Left | Top  | Right | Bottom
   0       | 0x00030A18 | Edit  | 340  | 112  | 540   | 132
   1       | 0x00030A20 | Edit  | 340  | 145  | 440   | 165
   2       | 0x00030A28 | Edit  | 340  | 178  | 540   | 198
   3       | 0x00030A30 | Edit  | 540  | 178  | 740   | 198
   4       | 0x00030A38 | Edit  | 340  | 211  | 540   | 231
   5       | 0x00030A40 | Edit  | 540  | 211  | 740   | 231
   ```

4. Cross-reference each row with the VISUAL window to identify which field is at each
   screen position. The `Left`/`Top` values tell you where the field is on screen.

   | Ordinal | Screen position | Which VISUAL field |
   |---|---|---|
   | 0 | Top-left-most edit control | (fill in from visual inspection) |
   | 1 | | |
   | 2 | | |
   | 3 | | |
   | 4 | | |
   | 5 | | |

5. To visually confirm: hover your mouse over the screen coordinates shown in the log.
   Use a ruler program or Paint to confirm which field is at (`Left`, `Top`).

6. **Also note the `Class` column.** If it says something other than `"Edit"` (e.g.
   `"PBCtrl"`, `"PBVM_EDIT"`), tell the developer — they need to update `EditWindowClass`
   in the config.

---

## Step 4 — Repeat for the Inventory Transaction Entry Window

1. In VISUAL, open **Inventory → Inventory Transaction Entry** (empty, same window size).
2. Keep Inventory Transfers open in the background (the diagnostic uses the foreground window title).
3. Re-run the diagnostic tool.
4. Read the new log section for the Entry window.
5. Fill in the ordinal map below.

---

## Step 5 — Update `appsettings.json`

Fill in the ordinal numbers you discovered. Replace the placeholder `0, 1, 2...` with the
correct ordinals from the diagnostic output.

```json
"BulkInventory": {
  "Automation": {
    "TransferWindow": {
      "WindowClass":     "Gupta:AccFrame",
      "WindowTitle":     "Inventory Transfers - Infor VISUAL - MTMFG",
      "EditWindowClass": "Edit",
      "InterFieldDelayMs": 150,
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
      "WindowClass":     "Gupta:AccFrame",
      "WindowTitle":     "Inventory Transaction Entry - Infor VISUAL - MTMFG",
      "EditWindowClass": "Edit",
      "InterFieldDelayMs": 150,
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

Update `"EditWindowClass"` if the log showed a class name other than `"Edit"`.

---

## Step 6 — End-to-End Test (after developer completes Phase 4)

1. Open VISUAL to the Inventory Transfers window (empty).
2. In the MTM app: Bulk Inventory → Data Entry → add one valid row → Push Batch.
3. Watch VISUAL: each field should be populated **instantly** (no typing animation).
4. Confirm the transaction committed and the row shows **Success**.

### If the wrong field gets the wrong value

The ordinal for that field is wrong. Re-run the diagnostic (Step 2), re-read the table,
and correct the ordinal in `appsettings.json`. No code recompile needed.

### If WM_SETTEXT is accepted but VISUAL does not acknowledge it

- The field may require a `WM_KEYDOWN` + `WM_KEYUP` for VK_TAB after the `WM_SETTEXT` to
  commit the value. Tell the developer — this is a known PowerBuilder behaviour for fields
  with validation on focus-loss.

---

## Step 7 — Document Your Results

Fill this in once the test passes:

| Setting | Value |
|---|---|
| TransferWindow `EditWindowClass` | |
| TransferWindow total edit control count | |
| EntryWindow `EditWindowClass` | |
| EntryWindow total edit control count | |
| VISUAL version | |
| Date diagnostic run | |

---

## What Changes if VISUAL Is Upgraded

1. VISUAL installs an upgrade.
2. Re-open the relevant window.
3. Re-run the diagnostic tool.
4. Compare the new ordinal table with the saved one above.
5. Update any changed ordinals in `appsettings.json`.
6. No C# changes, no recompile.

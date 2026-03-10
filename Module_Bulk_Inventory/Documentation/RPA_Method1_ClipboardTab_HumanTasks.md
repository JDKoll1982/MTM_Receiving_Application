# Method 1: Clipboard-Paste + Tab Navigation — Human Developer Tasks

Last Updated: 2026-03-10

This file covers everything you must do physically in Infor VISUAL to set up the tab-based
automation. No images to capture, no pixel measurements — just tab through each window and
count.

---

## Prerequisites

- [ ] Infor VISUAL client installed and logged in to MTMFG.
- [ ] You can open **Inventory Transfers** and **Inventory Transaction Entry** fresh (no data).
- [ ] `MTM_Receiving_Application` builds (`dotnet build`).

---

## Step 1 — Identify the First-Field Click Position

The automation needs to click somewhere in the VISUAL form to anchor keyboard focus
before starting the Tab sequence. The safest anchor is clicking the **Part ID field** directly.

### How to measure the click offset

1. Open the **Inventory Transfers** window in VISUAL.
2. Take a full screenshot of the window (`Win + Shift + S`, window snip).
3. Open the screenshot in **Paint**.
4. Hover over the centre of the **Part ID input field** (the blank white box to the right
   of the "Part" label).
5. Note the X and Y pixel coordinates shown in the Paint status bar.
6. Now note the X and Y of the **top-left corner of the VISUAL window frame**.
7. Subtract:
   ```
   FocusClickOffsetX = inputCentreX − windowLeft
   FocusClickOffsetY = inputCentreY − windowTop
   ```
8. Write down: `TransferWindow FocusClickOffsetX = _____   FocusClickOffsetY = _____`

Repeat for the **Inventory Transaction Entry** window:
`EntryWindow FocusClickOffsetX = _____   FocusClickOffsetY = _____`

> **Note:** these offsets must be measured at the same window size used in production.
> If operators resize the window, offsets will be wrong. Ask operators not to resize VISUAL
> while automation is running — or pin the window size in VISUAL's preferences.

---

## Step 2 — Count Tab Stops in the Inventory Transfers Window

1. Open **Inventory Transfers** (empty — no data).
2. Click the **Part ID field** to focus it.
3. Type a test value in Part ID (e.g. `TEST`) so you can see when focus leaves the field.
4. Press **Tab** once. Note which field received focus. Write it down.
5. Continue pressing Tab and recording which field receives focus each time.
6. Stop when you reach the final field you need to fill (To Location) OR when you've
   pressed Tab 15 times (something is wrong if you haven't reached To Location by then).

Fill in this table from your observations:

| Field | Presses of Tab to reach it from the *previous* field |
|---|---|
| Quantity | `___` Tab(s) from Part ID |
| From Warehouse | `___` Tab(s) from Quantity |
| From Location | `___` Tab(s) from From Warehouse |
| To Warehouse | `___` Tab(s) from From Location |
| To Location | `___` Tab(s) from To Warehouse |

> **Tip:** VISUAL may Tab through read-only labels or hidden fields — those count. If you
> see focus disappear or jump unexpectedly, that is a hidden field consuming a Tab stop.
> Count it and add to the `TabsAfter` value in the config.

> **Warning:** Entering a Part ID may trigger a "Parts" lookup popup. Dismiss it with
> Escape or the first match before continuing to Tab.

---

## Step 3 — Count Tab Stops in the Inventory Transaction Entry Window

1. Open **Inventory Transaction Entry** (empty).
2. Click the **Work Order field** to focus it.
3. Repeat the Tab-counting exercise from Step 2.

| Field | Presses of Tab to reach it from the *previous* field |
|---|---|
| Lot No | `___` Tab(s) from Work Order |
| Quantity | `___` Tab(s) from Lot No |
| To Warehouse | `___` Tab(s) from Quantity |
| To Location | `___` Tab(s) from To Warehouse |

---

## Step 4 — Update `appsettings.json`

Once you have all the measurements, open `appsettings.json` and add/update this section.
Replace the placeholder numbers with your measured values:

```json
"BulkInventory": {
  "Automation": {
    "TransferWindow": {
      "WindowTitle": "Inventory Transfers - Infor VISUAL - MTMFG",
      "WindowClass": "Gupta:AccFrame",
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
      "WindowTitle": "Inventory Transaction Entry - Infor VISUAL - MTMFG",
      "WindowClass": "Gupta:AccFrame",
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

Replace `110` / `95` with your measured click offsets.
Replace each `TabsAfter: 1` with the count you recorded in Steps 2–3.

---

## Step 5 — End-to-End Test (after developer completes Phase 3)

1. Open VISUAL to the Inventory Transfers window (empty, normal size).
2. Run the MTM app → Bulk Inventory → Data Entry.
3. Add ONE row with a valid Part ID, Quantity, From Location, To Warehouse, To Location.
4. Click **Push Batch**.
5. Watch VISUAL — you should see the cursor click into the Part ID field, then the full
   Part ID value appears instantly (paste, not typing), Tab advances to Quantity, etc.
6. Verify the transaction committed in VISUAL.
7. Check the row shows **Success** in the MTM app.

### If a field gets the wrong value

The Tab count is off for that field. Open `appsettings.json` and adjust the `TabsAfter`
for the field immediately before the one that got the wrong value.

---

## Step 6 — Document Your Settings

Fill in this table once the test passes. Keep it here for reference.

| Setting | Value |
|---|---|
| `TransferWindow.FocusClickOffsetX` | |
| `TransferWindow.FocusClickOffsetY` | |
| `EntryWindow.FocusClickOffsetX` | |
| `EntryWindow.FocusClickOffsetY` | |
| VISUAL window size at time of measurement | `_____ × _____` px |
| VISUAL version | |
| Date measured | |

---

## What Changes if VISUAL Is Upgraded

If VISUAL moves a field or adds a new Tab stop:

1. Manually Tab through the affected window and recount (Step 2 or 3).
2. Update the relevant `TabsAfter` value in `appsettings.json`.
3. No C# code changes, no recompile needed.

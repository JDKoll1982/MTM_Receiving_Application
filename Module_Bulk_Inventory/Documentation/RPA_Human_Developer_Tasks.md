# Bulk Inventory RPA Migration — What You Must Do as the Human Developer

Last Updated: 2026-03-10

This file covers every step that **cannot be done by AI** — things that require you to
physically open Infor VISUAL, take screenshots in the right state, or run the app in the
real environment. Code work is described in the companion plan file.

---

## Prerequisites

Before starting anything, confirm the following on the **production/test machine**:

- [ ] Infor VISUAL client is installed and you can log in to the MTMFG company.
- [ ] The machine's display scale (DPI) is set to the value used in production.
      **Write it down:** `Display Scale = _____%`  (found in Windows Settings → Display → Scale)
      All template images must be captured at this exact scale. If production machines vary,
      capture at the lowest common DPI.
- [ ] You can open both **Inventory Transfers** and **Inventory Transaction Entry** windows
      without any data pre-filled.
- [ ] The `MTM_Receiving_Application` builds and runs (`dotnet build`).

---

## Step 1 — Add the NuGet Package

Open a terminal in the repo root and run:

```powershell
dotnet add .\MTM_Receiving_Application.csproj package System.Drawing.Common
```

Verify it added to `MTM_Receiving_Application.csproj`:

```xml
<PackageReference Include="System.Drawing.Common" Version="..." />
```

Commit this change before proceeding.

---

## Step 2 — Create the Template Image Folder

Create this folder in the repo (it will hold the `.png` files you capture next):

```
Module_Bulk_Inventory/Assets/VisualTemplates/
```

Add a `.gitkeep` file so the empty folder is tracked.

---

## Step 3 — Capture Template Images for the Inventory Transfers Window

### 3.1 Prepare the window

1. Open Infor VISUAL and navigate to:
   **Inventory → Inventory Transfers**
2. The window should be **empty** (no data filled in yet).
3. Set the window to the size you normally use — **do not resize it later**.
   The template images are only valid for this exact window size _and_ the DPI you measured
   in the Prerequisites.
4. Make sure the window is **not maximized** — leave it at its normal restored size.

### 3.2 Capture each label

Use the Windows **Snipping Tool** (`Win + Shift + S` then "Region snip") to capture each
label text. Rules for a good template crop:

- Crop **tightly around the label text only** — include 2–3 pixels of background on all
  sides so the crop has context, but do NOT include the input field border.
- The label should appear the same colour it is at rest (no hover/focus highlight on it).
- Save as **PNG** (not JPEG — JPEG compression changes pixel values and will reduce match accuracy).

Capture the following and save to `Module_Bulk_Inventory/Assets/VisualTemplates/`:

| File name to save | What to crop | Notes |
|---|---|---|
| `tpl_transfer_partid_label.png` | The "Part" label text | May say "Part ID" or "Part No" — capture as shown |
| `tpl_transfer_qty_label.png` | The "Quantity" label | May be abbreviated "Qty" |
| `tpl_transfer_fromwarehouse_label.png` | The "From Wh" or "From Warehouse" label | |
| `tpl_transfer_fromlocation_label.png` | The "From Location" label | |
| `tpl_transfer_towarehouse_label.png` | The "To Wh" or "To Warehouse" label | |
| `tpl_transfer_tolocation_label.png` | The "To Location" label | |

After capturing, **open each PNG** and verify it shows only the label text with a clean background.
Delete and re-capture any that contain part of an input border or another control.

---

## Step 4 — Capture Template Images for the Inventory Transaction Entry Window

### 4.1 Prepare the window

1. In VISUAL, navigate to **Inventory → Inventory Transaction Entry**.
2. Window should be empty, same window size as above, same DPI.

### 4.2 Capture labels

| File name to save | What to crop |
|---|---|
| `tpl_entry_workorder_label.png` | "Work Order" label |
| `tpl_entry_lotno_label.png` | "Lot No" or "Lot Number" label |
| `tpl_entry_qty_label.png` | "Quantity" label |
| `tpl_entry_towarehouse_label.png` | "To Wh" or "To Warehouse" label |
| `tpl_entry_tolocation_label.png` | "To Location" label |

---

## Step 5 — Measure the Field Offset

Each template image will match the **label**, but the cursor must click on the **input field**
to its right. You need to measure the horizontal pixel gap between the right edge of a label
and the centre of the corresponding input field.

**How to measure:**

1. Take a full screenshot of the Inventory Transfers window (`Win + Shift + S`, full window snip).
2. Open it in **Paint** or a free editor (e.g. IrfanView).
3. Hover over the right edge of the "Part" label — note the X coordinate shown in the status bar.
4. Hover over the horizontal centre of the "Part" input field — note the X coordinate.
5. Subtract: **`FieldOffsetX = inputCentreX − labelRightEdgeX`**

Write down your measurement:  `FieldOffsetX = _____ pixels`

Repeat for 2–3 other fields as a sanity check — the offset should be approximately consistent.

Add the value to `appsettings.json` (the developer writing code will add the JSON key, but
you need to supply the measured number):

```json
"BulkInventory": {
  "Automation": {
    "FieldOffsetX": 120,
    "MatchThreshold": 0.85
  }
}
```

Replace `120` with your measured value.

---

## Step 6 — Mark Images as Embedded Resources

After adding the PNG files, open `MTM_Receiving_Application.csproj` and add the following
inside the `<Project>` node (ask the developer to confirm the exact item group location):

```xml
<ItemGroup>
  <EmbeddedResource Include="Module_Bulk_Inventory\Assets\VisualTemplates\*.png" />
</ItemGroup>
```

Run `dotnet build` and confirm the build succeeds — the PNGs will show in the build output
as embedded resources with names like:
`MTM_Receiving_Application.Module_Bulk_Inventory.Assets.VisualTemplates.tpl_transfer_partid_label.png`

---

## Step 7 — End-to-End Manual Test (after developer completes Phase 3)

Once the developer has migrated `Service_VisualInventoryAutomation` to use the image approach:

1. Open VISUAL to the Inventory Transfers window (empty, same size as when you took templates).
2. Run the MTM Receiving Application.
3. Go to Bulk Inventory → Data Entry.
4. Add ONE row: a real Part ID, a real From Location, real quantity, real To Location.
5. Click **Push Batch**.
6. Watch VISUAL — the cursor should move to each field and paste the value. There should be
   **no character-by-character typing visible** in the VISUAL window.
7. Confirm the transaction committed in VISUAL.
8. Check the row's status in the MTM app: it should show **Success**.

If a row shows **Failed** with a message like `"Template 'tpl_transfer_partid_label.png' not matched (score: 0.72)"`:
- The template image does not match the current VISUAL window.
- Re-capture that specific template (the window may have been a different size or DPI).

---

## Step 8 — Stress Test With a Real Batch

1. Create a batch of 5–10 rows with valid data.
2. Push the batch.
3. Verify all rows succeed.
4. Check VISUAL's inventory for the expected quantity movements.

---

## What Changes if VISUAL Is Upgraded

If MTM upgrades Infor VISUAL to a newer version and field labels change:

1. Re-open the relevant VISUAL window at the same DPI.
2. Re-capture only the templates whose labels changed (usually just 1–2 files).
3. Replace the old `.png` files in `Module_Bulk_Inventory/Assets/VisualTemplates/`.
4. Rebuild and redeploy — **no C# code changes needed**.

---

## Summary Checklist

- [ ] Confirm production DPI and write it down
- [ ] Add `System.Drawing.Common` NuGet
- [ ] Create `Module_Bulk_Inventory/Assets/VisualTemplates/` folder
- [ ] Capture all 6 Inventory Transfers label templates (PNG, tight crop, no JPEG)
- [ ] Capture all 5 Inventory Transaction Entry label templates
- [ ] Measure `FieldOffsetX` in pixels and update `appsettings.json`
- [ ] Add `EmbeddedResource` glob to `.csproj`
- [ ] Confirm `dotnet build` passes with embedded PNGs
- [ ] Run end-to-end test with a single row
- [ ] Run stress test with a 5–10 row batch
- [ ] Document the VISUAL window size and DPI in this file for future reference

---

**VISUAL window details (fill in after setup):**

| Setting | Value |
|---|---|
| Window title | Inventory Transfers - Infor VISUAL - MTMFG |
| Window size at time of template capture | `_____ × _____` px |
| Display scale (DPI) at time of capture | `_____%` |
| `FieldOffsetX` measured value | `_____` px |
| Date templates were captured | `___________` |

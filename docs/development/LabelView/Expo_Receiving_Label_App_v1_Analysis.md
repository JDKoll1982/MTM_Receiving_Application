# Reverse Engineering Analysis: `Expo - Receiving Label App ver 1.0.lbl`

**Analysed:** 2026-03-19  
**File size:** 614,400 bytes  
**Container:** Microsoft OLE2 Compound Document (magic `D0 CF 11 E0`)  
**LabelView version hint:** 2022.00.01  

---

## 1. Stream Inventory

| Stream | Size (bytes) | Storage | Purpose |
|---|---|---|---|
| `Contents` | 1,583 | Mini-stream | Label name / description metadata |
| `General` | 12 | Mini-stream | Miscellaneous label flags |
| `Printer` | 12,806 | Regular sectors (chain start 1169) | Printer binding, media, font list |
| `Format` | 130 | Mini-stream | Print format flags |
| `VarsCategories` | 2 | Mini-stream | Variable category flags |
| `GroupsObjects` | 4 | Mini-stream | Object group membership |
| `Variables` | 30,685 | Regular sectors (chain 1108→1167 sequential) | All 5 variable definitions |
| `RfTagObject` | 0 | — | Empty RFID placeholder |
| `Form` | 32,292 | Regular sectors (chain start 1044) | Print dialog XAML (WPF) |
| `InputPrintOrder` | 18 | Mini-stream | Print field ordering |
| `Objects` | 99,110 | Regular sectors (chain start 848) | All drawing objects + embedded logo PNG |
| `Database` | 1,644 | Mini-stream | DB connection flag/string |
| `BackGround` | 32 | Mini-stream | Background fill |
| `\x05SummaryInformation` | — | Regular | OLE2 document properties |

---

## 2. Printer Binding

| Property | Value |
|---|---|
| Printer name | `Recieving-Temp on mtmanu-dc01 (redirected 2)` |
| Type | **Windows shared printer via RDP redirection** — not a direct label printer |
| Server | `mtmanu-dc01` |
| Paper size | `8 1/2x11` (US Letter, **not** a thermal label roll) |
| LabelView build | `2022.00.01` |
| Font list | Arial, Courier New, Times New Roman, Symbol, Wingdings |
| Session user hint | `jkoll` |
| Label stock | `labelstock` |

> **⚠️ Important:** This label was saved while the designer was remote-desktoped into a server
> (`(redirected 2)` suffix is the RDP-printer redirection marker). Before production deployment,
> the printer must be updated to the correct direct-IP or shared path for the receiving-area printer.

---

## 3. Label Canvas & Dimensions

From the `Form` stream binary header (offsets 0x70–0x77):

| Field | Raw value | Decoded |
|---|---|---|
| Canvas Width | `C2 01 00 00` | **450** (units TBD — likely hundredths of an inch → **4.5 in**) |
| Canvas Height | `58 02 00 00` | **600** (→ **6.0 in**) |
| Form dialog size | `Width=438, Height=499` | WPF XAML dialog window (pixels) |

The label is approximately **4.5 × 6 inches** on letter-size paper.

---

## 4. Fields Printed on the Physical Label

These are the drawing objects defined in the `Objects` stream. The template values shown were captured at design-time.

| Object Name | Type | Bound Variable | Sample / Static Text |
|---|---|---|---|
| `Image2` | `CDrawImage` | _(embedded)_ | MTM Logo `MTM_Logo-Rev_100H.png` |
| `MaterialIDLabel` | `CDrawText` | _(static)_ | `"Part ID"` |
| `RevisionNumberLabel` | `CDrawText` | _(static)_ | `"QF840-3-1 Rev. B."` |
| `Line2` | Line separator | _(none)_ | — |
| `Left2Textbox` | `CDrawText` | `PONumber` | `"PO Number: PO-067861"` |
| `Right1TexBox` | `CDrawText` | `LotNumber` | `"Lot Number: A082946/"` |
| `Right3TextBox` | `CDrawText` | `EmployeeFormula` | `"Received By: 6229"` |
| `Description` | `CDrawText` | `DescriptionFormula` | `"Description: Coil, 7Ga X 32.875@"` |
| `Quantity` | `CDrawText` | `Quantity` | _(quantity number)_ |
| `Barcode2` | `CDrawBarcode` (CODE 128) | `quantity` | dimension 8890 (hundredths of an inch) |

**Barcode:** Symbology is **Code 128** (linear 1-D). The barcode encodes the `quantity` variable value.

**Embedded image:** The MTM logo is a **PNG** file from
`C:\Users\receiving\Downloads\MTM_Logo-Rev_100H.png`.  
XMP metadata confirms: Adobe Illustrator CC 23.1 (Macintosh), title `MTM_Logo`,
DocumentID `xmp.did:C083723BFCBE11EAB5FFB1FAA9B2943E`.

---

## 5. Print Dialog Variables (Form Stream XAML)

The `Form` stream contains a WPF XAML dialog. This is the form LabelView presents to the
operator at print-time. The C# application must set all of these variables programmatically
to bypass the manual dialog.

### Group 1 — "For: Material Handlers & Receiving" (operator inputs)

| Tab | WPF Label | `VariableName` | Notes |
|---|---|---|---|
| 1 | `PO Number:` | `PO Number` | Free-text input |
| 2 | `Part Number:` | `Material ID` | Free-text input |
| 3 | `Quantity:` | `quantity` | Free-text input; also drives the barcode |
| 4 | `Heat:` | `Heat` | Free-text input (steel heat/lot number) |
| 5 | `Employee ID:` | `Employee` | Free-text input |

### Group 2 — "Receiving Use Only" (supervisor / system fills)

| Tab | WPF Label | `VariableName` | Type | Notes |
|---|---|---|---|---|
| 10 | `Master Label?` | `MasterLabelCheck` | Checkbox | Checked → `"Checked"` / Unchecked → `"Unchecked"` |
| 11 | `Custom Data L-1:` | `Left1Select` | Pick-list | Forced from pick-list (`ForceValueFromPickList=True`) |
| 12 | `Custom Data L-2:` | `Left2Select` | Pick-list | |
| 13 | `Custom Data L-3:` | `Left3Select` | Pick-list | |
| 14 | `Custom Data R-2:` | `Right2Select` | Pick-list | |
| 15 | `Custom Data R-3:` | `Right3Select` | Pick-list | |
| 16 | `Custom Data R-1:` | `Right1Select` | Pick-list | |
| 17 | `Duplicates:` | `@serialqty` | Free-text | Number of duplicate labels to print |
| 18 | `Special (CoS):` | `Coils on Skid (Optional)` | Free-text | Coils-on-skid count |
| 19 | `Location:` | `Initial Location` | Free-text | |
| 20 | `Label #:` | `Label #` | Free-text | |
| 21 | `Notes:` | `Notes` | Free-text | |
| 22 | `Time:` | `Time` | Free-text | |

---

## 6. Variables Stream (5 Import-Style Variables)

These are the variables defined in the `Variables` OLE2 stream.  
They appear to be LabelView **import/counter** variables that feed the formula variables
displayed in the objects. Each has an internal link name and a sample value captured at design-time.

| Display Name | Internal Link Name | Sample Value | Notes |
|---|---|---|---|
| `PO_LINE_NUMBER` | `po_line_number` | `9999` | Maps to form variable `PO Number` |
| `PART_TYPE` | `part_type2` | `Coil` | Maps to `Material ID`; sample="Coil" = steel coil |
| `USER_ID` | `user_idd` | `jkoll` | Maps to `Employee`; was set to designer's login |
| `LOAD_NUMBER` | `load_number` | _(blank)_ | Likely maps to `Label #` or `quantity` |
| `HEAT` | `heatd` | `A082946` / `33606` | Maps to `Heat`; sample is a real heat number |

---

## 7. Mapping to Module_Receiving Data Model

| Label Variable | Source in Module_Receiving | C# Model Field (likely) |
|---|---|---|
| `PO Number` | Purchase Order lookup | `Model_PurchaseOrder.PONumber` |
| `Material ID` | Part master lookup | `Model_ReceivingLine.PartID` |
| `quantity` | Quantity received | `Model_ReceivingLine.Quantity` |
| `Heat` | Heat/lot number from receiving scan | `Model_ReceivingLine.HeatNumber` |
| `Employee` | Windows login → employee lookup | `Model_User.EmployeeID` |
| `Initial Location` | Infor Visual bin/location | `Model_ReceivingLine.StorageLocation` |
| `Label #` | Label sequence number | generated at print-time |
| `Time` | Print timestamp | `DateTime.Now.ToString("HH:mm")` |
| `@serialqty` | Number of copies | print copies parameter |
| `MasterLabelCheck` | Is this the primary label? | `bool IsMasterLabel` |
| Left/Right `_Select` (×6) | Custom pick-list data | TBD — confirm with receiving team |
| `Coils on Skid (Optional)` | Coil count per skid | TBD |
| `Notes` | Freeform operator note | TBD |

---

## 8. Programmatic Printing Integration Points

To print this label from `Module_Receiving` without LabelView's manual dialog:

```csharp
// Pseudocode — LabelView COM/SDK API call
var label = new LabelViewLabel(@"\\server\share\Expo - Receiving Label App ver 1.0.lbl");
label.SetVariable("PO Number",       receivingLine.PONumber);
label.SetVariable("Material ID",     receivingLine.PartID);
label.SetVariable("quantity",        receivingLine.Quantity.ToString());
label.SetVariable("Heat",            receivingLine.HeatNumber);
label.SetVariable("Employee",        currentUser.EmployeeID);
label.SetVariable("Initial Location",receivingLine.StorageLocation ?? "");
label.SetVariable("Label #",         labelSequenceNumber.ToString());
label.SetVariable("Time",            DateTime.Now.ToString("HH:mm"));
label.SetVariable("@serialqty",      copies.ToString());
label.SetVariable("MasterLabelCheck", isMasterLabel ? "Checked" : "Unchecked");
label.SetVariable("Notes",           receivingLine.Notes ?? "");
// Left1Select..Right3Select: set as needed from pick-list values
label.Print(printerName: "\\\\mtmanu-dc01\\Recieving-Temp", copies: 1);
```

> **Variable name casing is exact.** LabelView variable names are case-sensitive.  
> `"PO Number"` ≠ `"po number"` ≠ `"PONumber"`.

---

## 9. Key Observations & Action Items

1. **Printer needs updating.** The saved printer is `Recieving-Temp on mtmanu-dc01 (redirected 2)`
   (an RDP-redirected session printer). Before automated printing, change to the actual
   network share for the receiving area printer.

2. **Barcode is Code 128 (1-D), not Data Matrix (2-D).** Unlike the AIAG shipping label
   (`ReverseEngThis.lbl` uses Data Matrix), this Expo Receiving label uses a simple Code 128
   barcode encoding only `quantity`. If a richer barcode is needed, the label template
   would need to be updated.

3. **Paper is 8.5 × 11 (letter), not a thermal roll.** This label prints on a standard
   laser/inkjet printer. Confirm this is intentional — it may need to move to a thermal
   printer (e.g., Zebra GK420d) for warehouse use.

4. **MTM Logo is referenced by local path** (`C:\Users\receiving\Downloads\MTM_Logo-Rev_100H.png`).
   This path must exist on every machine running LabelView, or the logo must be embedded
   directly. Recommended: use a UNC path (`\\server\share\Logos\MTM_Logo-Rev_100H.png`),
   set it once in LabelView, and re-save the `.lbl` file.

5. **`@serialqty` drives duplicates.** Setting this variable to an integer `n` causes LabelView
   to print `n` copies automatically — useful for skids requiring multiple labels.

6. **`MasterLabelCheck` is a checkbox variable.** The two text values LabelView writes are
   `"Checked"` and `"Unchecked"` (case-sensitive strings), not `true`/`false` or `1`/`0`.

7. **Typo in `RevisionNumberLabel` static text:** `"QF840-3-1 Rev. B."` — this is a quality
   form / spec revision number, presumably cross-referencing a receiving document. Verify
   with the quality team before automating.

8. **`HEAT` variable has two sample values:** `A082946` (alpha-numeric) and `33606` (numeric).
   The C# application should accept either format — validate as `string`, not `int`.

---

## 10. Stream Security Notes

- **No ODBC connection strings** were found in the Variables stream (unlike `ReverseEngThis.lbl`).
  This label does not query any database directly — all data is supplied at print-time.
- **No hardcoded credentials** present.
- The embedded PNG contains an Adobe XMP packet but no PII or credentials.

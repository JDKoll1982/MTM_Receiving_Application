# XLSX File Columns - Complete Reference

**Date:** 2026-01-21  
**Updated:** 2026-01-21  
**Status:** ✅ **COMPLETE - All Model_ReceivingLoad Fields + Infor Visual Data Included**

---

## Summary

The XLSX file now contains **ALL 25 columns** from `Model_ReceivingLoad` including Infor Visual PO data to ensure complete data capture for label printing and reporting.

**Previous:** 19 columns  
**Current:** 25 columns  
**Added:** 6 new columns (Part Description, PO Vendor, PO Status, PO Due Date, Qty Ordered, Unit of Measure)

---

## Complete Column List

| Column | Header | Data Type | Source Field | Description | Example Value |
|--------|--------|-----------|--------------|-------------|---------------|
| A (1) | LoadID | GUID | `LoadID` | Unique identifier for the load | `550e8400-e29b-41d4-a716-446655440000` |
| B (2) | Load Number | Integer | `LoadNumber` | Sequential load number | `1`, `2`, `3` |
| C (3) | Part ID | String | `PartID` | Manufacturing part identifier | `MMC-12345`, `MMF-67890` |
| D (4) | Part Description | String | `PartDescription` | Full part description from Infor Visual | `Steel Coil - Grade A`, `Aluminum Sheet` |
| E (5) | Part Type | String | `PartType` | Type of part (Coil, Sheet, Standard) | `Coil`, `Sheet`, `Standard` |
| F (6) | PO Number | String | `PoNumber` | Purchase order number (nullable) | `PO-2024-001`, `N/A` |
| G (7) | PO Line Number | String | `PoLineNumber` | Line number on PO | `1`, `2`, `3` |
| H (8) | PO Vendor | String | `PoVendor` | Vendor name from PO | `ABC Steel Supply`, `XYZ Materials` |
| I (9) | PO Status | String | `PoStatus` | PO status code | `R` (Open), `C` (Closed), `P` (Partial) |
| J (10) | PO Due Date | Date | `PoDueDate` | Expected delivery date | `2026-01-25`, `2026-02-15` |
| K (11) | Qty Ordered | Decimal | `QtyOrdered` | Total quantity ordered on PO | `10000`, `5000.50` |
| L (12) | Weight/Quantity (lbs) | Decimal | `WeightQuantity` | Total weight/quantity in pounds | `5000.50`, `1250.00` |
| M (13) | Unit of Measure | String | `UnitOfMeasure` | Unit of measure from Infor Visual | `EA`, `LB`, `FT`, `PC` |
| N (14) | Heat/Lot Number | String | `HeatLotNumber` | Heat or lot tracking number | `HEAT-2024-A1`, `Nothing Entered` |
| O (15) | Remaining Quantity | Integer | `RemainingQuantity` | Remaining quantity from PO | `2500`, `0` |
| P (16) | Packages Per Load | Integer | `PackagesPerLoad` | Number of packages on this load | `5`, `1`, `10` |
| Q (17) | Package Type | String | `PackageTypeName` | Type of packaging | `Skid`, `Coil`, `Sheet`, `Box` |
| R (18) | Weight Per Package | Decimal | `WeightPerPackage` | Calculated weight per package | `1000`, `250` |
| S (19) | Is Non-PO Item | Boolean | `IsNonPOItem` | Whether this is a non-PO item | `Yes`, `No` |
| T (20) | Received Date | DateTime | `ReceivedDate` | Date and time received | `2026-01-21 14:30:00` |
| U (21) | User ID | String | `UserId` | Windows username of receiver | `DOMAIN\jsmith`, `jsmith` |
| V (22) | Employee Number | Integer | `EmployeeNumber` | Employee badge number | `12345`, `67890` |
| W (23) | Quality Hold Required | Boolean | `IsQualityHoldRequired` | Whether quality hold is required | `Yes`, `No` |
| X (24) | Quality Hold Acknowledged | Boolean | `IsQualityHoldAcknowledged` | Whether quality hold was acknowledged | `Yes`, `No` |
| Y (25) | Quality Hold Restriction Type | String | `QualityHoldRestrictionType` | Type of quality restriction | `Chemical Analysis`, `Visual Inspection` |

---

## Column Count Changes

**Version 1 (Initial):** 8 columns  
**Version 2 (First Expansion):** 19 columns  
**Version 3 (Current - Infor Visual Data):** 25 columns  

### Added in Version 3:
- **Part Description** - Full description from Infor Visual
- **PO Vendor** - Vendor/supplier name from PO
- **PO Status** - Current PO status code
- **PO Due Date** - Expected delivery date
- **Qty Ordered** - Total quantity on PO
- **Unit of Measure** - Standard UOM from Infor Visual (EA, LB, FT, etc.)

---

## Header Formatting

- **Font:** Bold
- **Background:** Light Gray
- **Range:** A1:Y1 (25 columns)
- **Auto-fit:** All columns auto-resize to content width

---

## Data Types & Formatting

### Text Fields (String)
- Part ID
- Part Description *(NEW)*
- Part Type  
- PO Number
- PO Line Number
- PO Vendor *(NEW)*
- PO Status *(NEW)*
- Heat/Lot Number
- Package Type
- Unit of Measure *(NEW)*
- User ID
- Quality Hold Restriction Type

**Format:** Plain text, empty string if null

### Numeric Fields
- **Integer:** Load Number, Remaining Quantity, Packages Per Load, Employee Number
- **Decimal:** Weight/Quantity, Weight Per Package, Qty Ordered *(NEW)*
- **GUID:** LoadID

**Format:** Numbers without formatting

### Boolean Fields
- Is Non-PO Item
- Quality Hold Required
- Quality Hold Acknowledged

**Format:** "Yes" or "No" (text)

### Date/Time Fields
- Received Date
- PO Due Date *(NEW)*

**Format:**
- Received Date: `yyyy-MM-dd HH:mm:ss` (e.g., `2026-01-21 14:30:00`)
- PO Due Date: `yyyy-MM-dd` (e.g., `2026-01-25`)

---

## File Locations

**Network Path (Default):**  
`\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User XLS Files\{Username}\ReceivingData.xlsx`

**User-Configured Path:**  
Defined in Settings → Receiving → Defaults → "XLS File Save Location"

---

## Usage Scenarios

### Current Use Cases
1. **Backup data** - Redundant copy of database records
2. **Label printing** - Source data for label generation
3. **Manual verification** - Users can open in Excel to review
4. **Data recovery** - Restore if database fails

### Future Use Cases (All Columns Available)
1. **Advanced label templates** - Use any field for label printing
2. **Quality tracking reports** - Quality hold fields available
3. **Package detail labels** - Package count and weight per package
4. **PO reconciliation** - PO line numbers and remaining quantities
5. **User audit trails** - User ID and employee number tracking
6. **Part type analysis** - Part type categorization available

---

## Example Row

```
| LoadID | Load # | Part ID | Part Desc | Type | PO # | Line | Vendor | Status | Due Date | Ordered | Wt/Qty | UOM | Heat | Remain | Pkgs | Type | Wt/Pkg | Non-PO | Received | User | Emp | QH Req | QH Ack | QH Type |
|--------|--------|---------|-----------|------|------|------|--------|--------|----------|---------|--------|-----|------|--------|------|------|--------|--------|----------|------|-----|--------|--------|---------|
| 550e8400-e29b-... | 1 | MMC-12345 | Steel Coil - Grade A | Coil | PO-2024-001 | 1 | ABC Steel Supply | R | 2026-01-25 | 10000 | 5000.00 | LB | HEAT-2024-A1 | 5000 | 5 | Coil | 1000 | No | 2026-01-21 14:30:00 | jsmith | 12345 | Yes | Yes | Chem Analysis |
```

---

## Benefits of Complete Column Set

✅ **Future-Proof** - All fields available for any future label requirements  
✅ **No Data Loss** - Complete snapshot of receiving transaction  
✅ **Infor Visual Integration** - PO vendor, status, due dates captured  
✅ **Part Identification** - Full descriptions for clarity  
✅ **Flexible Reporting** - Any field can be used in reports  
✅ **Label Customization** - Create custom labels with any combination of fields  
✅ **Audit Compliance** - Full tracking of quality holds and user actions  
✅ **Vendor Tracking** - Know which supplier each part came from  
✅ **Unit of Measure** - Proper UOM for accurate calculations  
✅ **PO Reconciliation** - Track ordered vs. received quantities  

### New Capabilities (Version 3):
- **Vendor labels** - Include supplier name on labels
- **PO status tracking** - Know if PO is open/closed/partial
- **Due date visibility** - Track on-time deliveries
- **Description clarity** - Full part descriptions on labels
- **UOM accuracy** - Correct units for weight/quantity calculations

---

## Code References

**Service Implementation:**
- `Module_Receiving/Services/Service_XLSWriter.cs`
  - Method: `WriteToFileAsync()` (Lines 170-240)
  - Method: `ClearXLSFilesAsync()` (Lines 310-347)
  - Method: `ReadFromXLSAsync()` (Lines 268-308)

**Model Definition:**
- `Module_Receiving/Models/Model_ReceivingLoad.cs`

---

## Testing Checklist

- [ ] Create new XLSX file with receiving data
- [ ] Verify all 25 columns appear in Excel
- [ ] Verify header row is bold with gray background
- [ ] Verify data populates correctly in all columns
- [ ] **Verify Part Description shows from Infor Visual**
- [ ] **Verify PO Vendor name appears correctly**
- [ ] **Verify PO Status code is captured**
- [ ] **Verify PO Due Date format (yyyy-MM-dd)**
- [ ] **Verify Qty Ordered matches PO**
- [ ] **Verify Unit of Measure (EA, LB, FT, etc.)**
- [ ] Test append operation (existing file)
- [ ] Verify columns auto-fit to content
- [ ] Test "Reset XLS" - verify headers remain with all 25 columns
- [ ] Test file opens in Excel without errors
- [ ] Verify boolean fields display "Yes" or "No"
- [ ] Verify date format is correct
- [ ] Test with quality hold data
- [ ] Test with non-PO items (vendor should be blank)
- [ ] Test with multiple package types

---

**Created:** 2026-01-21  
**Updated:** 2026-01-21  
**Build Status:** ✅ Successful  
**Columns:** 25 (Complete set from Model_ReceivingLoad + Infor Visual PO data)  
**Future-Ready:** ✅ All fields available for labels including vendor, descriptions, and PO tracking

# Label Types & Requirements

This document describes all label types generated from the Receiving Label Application's CSV output via LabelView.

---

## Overview

The application generates data that feeds into **LabelView 2022** label printing software. LabelView uses the CSV file to populate 5 different label templates based on user selection or automatic generation rules.

---

## Label Types

### 1. Dunnage Label (Expo - Dunnage Label ver. 1.0)

**Purpose**: Track packaging/dunnage materials (pallets, crates, skids)

**Label Size**: Standard 4" x 6"

**Fields Required**:
- **LINE 1**: Large text (user-defined, typically dunnage type or identifier)
- **LINE 2**: Large text (user-defined, secondary information)
- **Transaction Date**: Date of receiving (format: MM/DD/YYYY)
- **Employee**: Employee number (4 digits)
- **PO Number**: Purchase order number (format: PO-XXXXXX)
- **Vendor Name**: Supplier name (or "Not Provided")
- **Label Number**: Format "Label: X / Y" (e.g., "Label: 1 / 1")

**Usage**: Printed when tracking returnable dunnage or special packaging that needs identification tags.

**CSV Columns Used**:
- `Date`
- `EmployeeNumber`
- `PONumber`
- `DunnageID` → LINE 1
- `DunnageType` → LINE 2
- `PackageDescription` (vendor name fallback)

---

### 2. Receiving Label (Expo - Receiving Label ver. 1.0)

**Purpose**: Main receiving label for parts/materials with detailed information

**Label Size**: Standard 4" x 6"

**Fields Required**:
- **Part ID**: Part number (barcode + text)
- **Quantity**: Total quantity on skid
- **Scrap Type**: Material classification (e.g., "Scrap = Steel")
- **Description**: Part description from Infor Visual
- **Transaction Date**: Date of receiving (format: MM/DD/YYYY)
- **PO Number**: Purchase order number (format: PO-XXXXXX)
- **Lot Number**: Heat/Lot number for traceability
- **Vendor**: Supplier company name
- **Coils**: Number of coils/packages on skid (if applicable)
- **Received By**: Employee number
- **Label**: Format "Label: X of Y" (e.g., "Label: 1 of 8")

**Special Fields**:
- **Barcode**: Generated from Part ID (Code 128 or similar)
- **Quantity Barcode**: Separate barcode for quantity
- **Scrap Type/Description**: Combined or separate fields

**Usage**: Primary label for all received parts. Attached to each skid/pallet.

**CSV Columns Used**:
- `Date`
- `EmployeeNumber`
- `Heat` → Lot Number
- `LocatedTo` (optional, for location routing)
- `PackagesOnSkid` → Coils
- `PartID`
- `PartType` → Scrap Type
- `PONumber`
- `Quantity`
- `LabelNumber` (calculated: current / total)

**Data Flow**:
```
CSV Row → LabelView "Receiving Data" database → Receiving Label template
```

---

### 3. UPS/FedEx Shipping Label (Expo - Mini UPS Label ver. 1.0)

**Purpose**: Internal routing/delivery labels for inter-department transfers

**Label Size**: Standard 4" x 6"

**Fields Required**:
- **Department**: Destination department (e.g., "Die Shop")
- **Deliver To**: Recipient name (e.g., "Bill Schmidt")
- **Work Order**: Internal work order number (format: WO-XXXXX)
- **PO Number**: Purchase order number (format: PO-XXXXXX)
- **Vendor**: Supplier name
- **Received By**: Employee number
- **Receiver(s)**: Internal receiver tracking number
- **Label #**: Sequential label number
- **Package Description**: Brief description of contents

**Usage**: Used when parts need to be routed to specific departments or individuals after receiving.

**CSV Columns Used**:
- `DeliverTo` → Deliver To (person name or "Unknown")
- `Department` → Department
- `PONumber`
- `EmployeeNumber` → Received By
- `PackageDescription`
- `LabelNumber`

**Note**: This label type may require additional data entry beyond the standard receiving workflow (Work Order number, Receiver tracking number).

---

### 4. Mini Receiving Label (Expo - Mini RECV Label ver. 1.0)

**Purpose**: Compact receiving label for smaller items or limited space

**Label Size**: 6" x 4" (landscape orientation)

**Fields Required**:
- **Part ID**: Part number (large text + barcode)
- **Weight**: Total weight on skid
- **PO Number**: Purchase order number (format: PO-XXXXXX)
- **Heat**: Heat/Lot number
- **Coils**: Number of coils/packages
- **Date**: Transaction date (format: MM/DD/YYYY)
- **Location**: Storage location code (e.g., "RECV")
- **Vendor**: Supplier name
- **Employee**: Employee number
- **Receiver**: Receiver tracking number
- **Label**: Format "Label: X / Y" (e.g., "Label: 1 / 1")
- **Barcode**: Vertical barcode on right side (Part ID)
- **Logo**: MTM logo

**Usage**: Space-saving alternative to full receiving label. Used for smaller parts or when multiple labels need to fit on one skid.

**CSV Columns Used**:
- `Date`
- `EmployeeNumber` → Employee
- `Heat`
- `LocatedTo` → Location
- `PackagesOnSkid` → Coils
- `PartID`
- `PONumber`
- `Quantity` → Weight (assumes quantity = weight, or uses calculation)
- `LabelNumber`

**Calculation**:
- If `Quantity` represents pieces, weight may need to be calculated: `Quantity × UnitWeight`
- Or `Quantity` may already represent weight in pounds

---

### 5. Mini Coil Label (Expo - Mini Coil Label ver. 1.0)

**Purpose**: Individual labels for coils when receiving material on multiple coils per skid

**Label Size**: 6" x 4" (landscape orientation)

**Fields Required**:
- **Heat**: Heat/Lot number (large text)
- **Weight**: Weight per coil (calculated)
- **Material ID**: Part number
- **PO Number**: Purchase order number (format: PO-XXXXXX)
- **Label**: Format "Label: X of Y" (e.g., "Label: 1 of 8")
- **Warning**: "DO NOT USE THIS TAG TO INVENTORY" (static text)

**Special Logic - Automatic Label Multiplication**:

This label type has unique behavior. When a receiving line has multiple coils per skid, LabelView automatically generates individual coil labels.

**Example Scenario**:
```
User Entry:
- PO Number: PO-014453
- Part ID: MMC0001000
- Number of Skids: 5
- Quantity per Skid: 2500 lbs
- Coils per Skid: 2

Label Generation:
- Total Labels Generated: 5 skids × 2 coils = 10 labels
- Weight per Label: 2500 lbs ÷ 2 coils = 1250 lbs each
- Label Numbers: 1 of 10, 2 of 10, ..., 10 of 10
- Each label shows:
  - Heat: [Same for all from that skid]
  - Weight: 1250 Pounds
  - Material ID: MMC0001000
  - PO Number: PO-014453
  - Label: X of 10
```

**Formula in LabelView**:
```
IF PackagesOnSkid > 1 THEN
    GenerateLabels = PackagesOnSkid
    WeightPerLabel = Quantity ÷ PackagesOnSkid
    TotalLabels = SUM(PackagesOnSkid) for all lines
ELSE
    GenerateLabels = 1
    WeightPerLabel = Quantity
END IF
```

**CSV Data Requirements**:
- `Heat` (must be present)
- `PartID` → Material ID
- `PONumber`
- `Quantity` (total weight on skid)
- `PackagesOnSkid` (number of coils, used for division)
- `LabelNumber` (recalculated by LabelView based on coil multiplication)

**Usage**: Automatically generated when `PackagesOnSkid` > 1. Each coil on a skid gets its own label with divided weight.

---

## Label Generation Rules

### Automatic vs Manual Selection

| Label Type | Selection Method | When Used |
|------------|------------------|-----------|
| Dunnage Label | Manual | Only when tracking returnable dunnage |
| Receiving Label | Default/Manual | Standard receiving workflow |
| UPS/FedEx Label | Manual | Internal routing to departments |
| Mini RECV Label | Manual | Space-constrained applications |
| Mini Coil Label | **Automatic** | When `PackagesOnSkid > 1` |

### Label Quantity Calculation

**Standard Labels** (Dunnage, Receiving, Mini RECV, UPS):
```
Labels Per Line = 1
Total Labels = Number of Lines Entered
```

**Coil Labels**:
```
Labels Per Line = PackagesOnSkid
Total Labels = SUM(PackagesOnSkid for all lines)

Example:
Line 1: 5 skids, 2 coils each = 10 labels
Line 2: 3 skids, 1 coil each = 3 labels
Total: 13 coil labels
```

---

## CSV to LabelView Mapping

### Database Connection in LabelView

LabelView connects to the CSV file via:
1. **Data Source**: "Expo - Dunnage Data" or "Receiving Data" or "Quantity"
2. **File Path**: 
   - Local: `%APPDATA%\ReceivingData.csv`
   - Network: `\\mtmanu-fs01\Expo Drive\Receiving\LabelViewQuickPrint\CSV Files\{Username}\ReceivingData.csv`
3. **Format**: CSV with header row
4. **Encoding**: UTF-8

### Field Mapping Table

| CSV Column | Dunnage Label | Receiving Label | UPS Label | Mini RECV | Mini Coil |
|------------|---------------|-----------------|-----------|-----------|-----------|
| `Date` | ✓ Transaction Date | ✓ Transaction Date | - | ✓ Date | - |
| `DeliverTo` | - | - | ✓ Deliver To | - | - |
| `Department` | - | - | ✓ Department | - | - |
| `DunnageID` | ✓ LINE 1 | - | - | - | - |
| `DunnageType` | ✓ LINE 2 | - | - | - | - |
| `EmployeeName` | - | - | - | - | - |
| `EmployeeNumber` | ✓ Employee | ✓ Received By | ✓ Received By | ✓ Employee | - |
| `Heat` | - | ✓ Lot Number | - | ✓ Heat | ✓ Heat |
| `LabelNumber` | ✓ Label # | ✓ Label X of Y | ✓ Label # | ✓ Label X/Y | ✓ Label X of Y* |
| `LocatedTo` | - | (Optional) | - | ✓ Location | - |
| `PackageDescription` | ✓ (Vendor) | ✓ Description | ✓ Pkg Desc | - | - |
| `PackagesOnSkid` | - | ✓ Coils | - | ✓ Coils | **Divider** |
| `PartID` | - | ✓ Part ID | - | ✓ Part ID | ✓ Material ID |
| `PartType` | - | ✓ Scrap Type | - | - | - |
| `PONumber` | ✓ PO Number | ✓ PO Number | ✓ PO Number | ✓ PO Number | ✓ PO Number |
| `Quantity` | - | ✓ Quantity | - | ✓ Weight | ✓ Weight** |

\* Recalculated by LabelView when generating multiple coil labels  
\*\* Divided by PackagesOnSkid

---

## Print Workflow in LabelView

### User Workflow

```
1. User completes receiving entry in WinUI 3 app
2. App saves CSV file to network location
3. User opens LabelView
4. User selects label template:
   - "Expo - Dunnage Label ver. 1.0"
   - "Expo - Receiving Label ver. 1.0"
   - "Expo - Mini UPS Label ver. 1.0"
   - "Expo - Mini RECV Label ver. 1.0"
   - "Expo - Mini Coil Label ver. 1.0"
5. LabelView loads data from CSV file
6. User selects record(s) to print or prints all
7. Labels are sent to printer
```

### Automatic Coil Label Generation

When printing **Receiving Label** with `PackagesOnSkid > 1`:

```
Option 1: Prompt user
  "This entry has 2 coils per skid. 
   Generate individual coil labels? [Yes] [No]"
   
Option 2: Automatic
  - System detects PackagesOnSkid > 1
  - Automatically switches to Mini Coil Label template
  - Generates (PackagesOnSkid × NumberOfLines) labels
```

---

## Data Validation for Label Printing

### Required Fields by Label Type

**Dunnage Label**:
- `DunnageID` or `DunnageType` (at least one)
- `PONumber`
- `EmployeeNumber`
- `Date`

**Receiving Label**:
- `PartID` (required)
- `Quantity` (required)
- `PONumber` (required)
- `Heat` (highly recommended)
- `EmployeeNumber` (required)
- `Date` (required)

**UPS/FedEx Label**:
- `DeliverTo` (required)
- `Department` (required)
- `PONumber` (required)
- `EmployeeNumber` (required)

**Mini RECV Label**:
- `PartID` (required)
- `Quantity` (required)
- `PONumber` (required)
- `Heat` (required)
- `LocatedTo` (required)
- `EmployeeNumber` (required)

**Mini Coil Label**:
- `Heat` (required)
- `PartID` (required)
- `PONumber` (required)
- `Quantity` (required)
- `PackagesOnSkid` (required, must be > 0)

---

## Integration Requirements for WinUI 3 App

### CSV Export Requirements

The application must support exporting different CSV formats based on intended label type:

1. **Standard Receiving CSV** (all fields):
   ```csv
   Date,DeliverTo,Department,DunnageID,DunnageType,EmployeeName,EmployeeNumber,Heat,LabelNumber,LocatedTo,PackageDescription,PackagesOnSkid,PartID,PartType,PONumber,Quantity
   ```

2. **Minimal Coil CSV** (optimized for coil labels):
   ```csv
   Date,EmployeeNumber,Heat,PartID,PONumber,Quantity,PackagesOnSkid,LabelNumber
   ```

### Optional: Direct Label Printing

Future enhancement: Integrate LabelView SDK or generate label files programmatically from WinUI 3 app.

**Benefits**:
- Skip CSV export step
- Print labels immediately after data entry
- Automatic label type selection based on data
- Real-time label preview in app

**Implementation**:
- Use LabelView .NET API (if available)
- Or generate `.lab` files programmatically
- Or use ZPL/EPL direct printer commands for Zebra printers

---

## Example Data Sets

### Example 1: Standard Receiving (Single Skid)

```csv
Date,EmployeeNumber,Heat,LocatedTo,PackagesOnSkid,PartID,PartType,PONumber,Quantity
2025-12-15 10:30:00,6229,330212,RECV,1,MMC0000848,Raw Material,66754,3690
```

**Labels Generated**:
- 1× Receiving Label (or Mini RECV Label)
- Part: MMC0000848
- Quantity: 3690
- Heat: 330212
- Label: 1 / 1

---

### Example 2: Multi-Coil Receiving

```csv
Date,EmployeeNumber,Heat,LocatedTo,PackagesOnSkid,PartID,PartType,PONumber,Quantity
2025-12-15 10:35:00,6229,330212,RECV,2,MMC0000848,Raw Material,66754,3690
```

**Labels Generated**:
- 2× Mini Coil Labels
  - Label 1 of 2: Heat 330212, 1845 Pounds, MMC0000848, PO-66754
  - Label 2 of 2: Heat 330212, 1845 Pounds, MMC0000848, PO-66754

---

### Example 3: Multiple Skids with Coils

```csv
Date,EmployeeNumber,Heat,LocatedTo,PackagesOnSkid,PartID,PartType,PONumber,Quantity
2025-12-15 11:00:00,6229,H123456,RECV,2,PART-001,Raw Material,14453,2500
2025-12-15 11:00:00,6229,H123456,RECV,2,PART-001,Raw Material,14453,2500
2025-12-15 11:00:00,6229,H123456,RECV,2,PART-001,Raw Material,14453,2500
2025-12-15 11:00:00,6229,H123456,RECV,2,PART-001,Raw Material,14453,2500
2025-12-15 11:00:00,6229,H123456,RECV,2,PART-001,Raw Material,14453,2500
```

**Labels Generated**:
- 10× Mini Coil Labels (5 skids × 2 coils each)
  - Label 1 of 10: Heat H123456, 1250 Pounds
  - Label 2 of 10: Heat H123456, 1250 Pounds
  - ...
  - Label 10 of 10: Heat H123456, 1250 Pounds

---

**Next**: See [NEW_APP_CSV_EXPORT.md](NEW_APP_CSV_EXPORT.md) for CSV export implementation details and [NEW_APP_LABELVIEW_INTEGRATION.md](NEW_APP_LABELVIEW_INTEGRATION.md) for advanced integration options.

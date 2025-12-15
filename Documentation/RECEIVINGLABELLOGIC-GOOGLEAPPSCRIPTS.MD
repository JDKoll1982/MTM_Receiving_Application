# Google Sheets Receiving Label System - Functional Overview

This document explains the existing Google Sheets-based receiving label system that will be replaced by the new WinUI 3 application. Understanding this workflow is critical for feature parity.

---

## System Architecture

**Platform**: Google Sheets + Google Apps Script  
**Primary Function**: Material receiving data entry and label printing preparation  
**Key Sheets**:
- **Expo** - Main data entry for Expo Drive receiving
- **Vits** - Data entry for Vits receiving
- **History 2025** - Historical archive of all receiving transactions
- **CoilInformation** - Material specifications and inventory
- **Search** - Dynamic search interface
- **MaterialInventory** - Auto-generated inventory summary

---

## Main Features

### 1. Custom Menu System

**MTM Menu** (Primary operations):
- **Show User Guide**: Displays HTML-based user documentation in modal dialog
- **Create Material Inventory Sheet**: Auto-generates inventory summary from CoilInformation data
- **Transfer to History**: Saves Expo or Vits data to History 2025 archive table
- **Update Column H Labels**: Recalculates label numbers (e.g., "1 of 8") for print preparation
- **Gather Data for Email**: Groups receiving transactions for daily summary reports
- **Progress Bar Management**: Testing and diagnostic tools

**Coil Tools Menu** (Material handling):
- **Create Material Inventory Sheet**: Parses coil descriptions and calculates weights
- **Parse Descriptions**: Extracts gauge, thickness, width, length from part descriptions

### 2. Automatic Sheet Initialization

**Trigger**: When spreadsheet opens

**Actions**:
1. **Reset Progress Indicators**: Clears checkboxes in cells I2:I4, resets progress text in J2:J4
2. **Date Formatting**: Applies MM/DD/YYYY format to column F (Transaction Date) for first 1000 rows
3. **Auto-Sort**: Sorts "History 2025" sheet by date (column F) in descending order
4. **Populate Dropdowns**: Fills search sheet dropdowns with current data

**Why This Matters**: Every time a user opens the sheet, they start with a clean slate. Progress bars reset to "Save to History", "Fill Blank Spaces", "Sort for Printing".

---

### 3. Material Inventory Sheet Generation

**Purpose**: Create searchable inventory summary with weight calculations

**Data Source**: CoilInformation sheet (columns A-J)

**Process**:
1. Deletes existing MaterialInventory sheet if present
2. Reads all coil data from CoilInformation
3. Parses part descriptions to extract:
   - Material Type (Steel/Galvanized, Aluminum, etc.)
   - Gauge or Thickness (e.g., "14Ga" or "0.125")
   - Width and Length (in inches)
4. Calculates:
   - **Volume**: Width × Length × Thickness (in cubic inches)
   - **Weight**: Volume × Density (steel = 0.283 lb/in³, aluminum = 0.098 lb/in³)
   - **Sheet Count**: Total weight ÷ weight per 96" sheet
5. Applies conditional formatting (low stock = red, adequate = green)
6. Excludes rows with missing Part ID, Description, or dimensions

**Output Columns**:
- Part ID, Material Type, Gauge/Thickness, Width, Length, Stock UM
- On Hand (lbs), Thickness (inches), Calculated Volume, Calculated Weight, Calculated Sheets, Density, Notes

**Why This Matters**: Provides real-time inventory visibility without manual calculations. Users can see how many sheets they can cut from coil inventory.

---

### 4. Description Parsing Logic

**Challenge**: Part descriptions in Infor Visual are unstructured text  
**Example**: `"Coil, 14Ga X 6.750"` or `"Steel Sheet .250 X 48 X 96"`

**Parsing Rules**:

**Gauge Extraction**:
- Pattern: `(\d+)Ga` → Captures "14Ga", "22Ga", etc.
- Converts gauge to decimal thickness using lookup table:
  - 30Ga → 0.0120"
  - 14Ga → 0.0747"
  - 4Ga → 0.2242"

**Thickness Extraction** (if not gauge):
- Patterns: `.250`, `(.375)`, `0.125`
- Extracts decimal thickness directly

**Dimensions Extraction**:
- Pattern: `X\s+(\d+\.?\d*)`
- First match = Width
- Second match = Length
- Example: "X 6.750 X 96" → Width: 6.750, Length: 96

**Material Type Assignment**:
- If gauge detected → "Steel/Galvanized"
- If "Aluminum" in description → "Aluminum"
- Otherwise → "Unknown"

**Why This Matters**: Allows automatic weight calculations and inventory planning without manual data entry.

---

### 5. Data Entry Workflow (Expo/Vits Sheets)

**Sheet Structure** (8 core columns + helpers):

| Column | Field | Description |
|--------|-------|-------------|
| A | Quantity | Numeric quantity received |
| B | Material ID | Part number (auto-pads MMC/MMF codes to 7 digits) |
| C | PO Number | Purchase order (auto-formats as "PO-XXXXXX") |
| D | Employee | Employee number |
| E | Heat | Heat/Lot number for traceability |
| F | Date | Transaction date (MM/DD/YYYY format) |
| G | Initial Location | Storage location or routing code |
| H | Coils on Skid (Optional) | Number of packages/coils per skid |
| I | Checkboxes | Progress tracking (I2: Save to History, I3: Fill Blank Spaces, I4: Sort for Printing) |
| J | Progress Text | Status messages |
| K | Employee Display | Shows current employee number (e.g., "Employee: 6229") |
| K | Total Rows | Shows count of data rows (e.g., "Total Rows: 0") |

**Auto-Fill Logic**:
- When user enters Material ID in column B, system auto-populates:
  - Quantity (column A) - from last entry for that part
  - PO Number (column C) - from recent history for that part
  - Heat (column E) - from last entry for that part
  - Initial Location (column G) - from last entry for that part
  - Coils on Skid (column H) - from last entry for that part (if applicable)
  - Employee (column D) - current user's employee number (shown in K1)
  - Date (column F) - current date (auto-populated)

**Why This Matters**: Reduces data entry time by 80%. User only needs to enter Material ID (column B) and adjust values if different from last time.

**Progress Tracking Checkboxes** (Columns I2:I4):
- **I2**: "Save to History" - Transfers current data to History 2025 sheet and clears current entries
- **I3**: "Fill Blank Spaces" - Auto-fills missing data from previous entries
- **I4**: "Sort for Printing" - Sorts data by Material ID, PO Number, and Heat for efficient label printing

**Status Display** (Column K):
- Shows "Employee: [number]" - Current logged-in user (e.g., "Employee: 6229")
- Shows "Total Rows: [count]" - Count of data rows entered

**Instructions Panel** (Visible in screenshot):
```
Vits - For Saving and Autofill:
1) Check the box for the action you want.
2) Wait a second for it to run.
3) It will auto-uncheck when done.
4) *IF it doesn't work, uncheck and try again
Format for Label# Column: (Tag #) - (# of Coils)
```

---

### 6. Label Number Calculation (Column H)

**Purpose**: Generate sequential label numbers for LabelView printing

**Format**: `"X / Y"` where:
- X = Current label number (increments for each row)
- Y = Total labels for this part/PO combination

**Example**:
```
Row 10: MMC0000848, PO-66754 → "1 / 3"
Row 11: MMC0000848, PO-66754 → "2 / 3"
Row 12: MMC0000848, PO-66754 → "3 / 3"
Row 13: PART-001, PO-14453 → "1 / 5"
```

**Calculation Logic**:
1. Group rows by Material ID + PO Number
2. Count total rows in each group (Y)
3. Assign sequential numbers within group (X)
4. Update column H with "X / Y" text

**Trigger**: User clicks "Update Column H" menu item or checkbox I3

**Why This Matters**: LabelView uses this to print "Label 1 of 8" text on physical labels. Must be accurate for warehouse scanning.

---

### 7. Save to History Workflow

**Purpose**: Archive completed receiving transactions for reporting and audit

**Trigger**: User clicks "Transfer to History" menu item or checkbox I2

**Process**:
1. **Validation**: Check for blank rows or missing required fields
2. **Data Copy**: Transfer rows from Expo/Vits to History 2025 table
3. **Timestamp**: Add "Transferred At" column with current datetime
4. **Clear Source**: Delete rows from Expo/Vits sheet (keeping headers)
5. **Progress Update**: Show "Save to History ✓" in cell J2
6. **Auto-Sort**: Sort History 2025 by date descending

**History 2025 Table Columns**:
- Material ID, PO Number, Quantity, Heat, Special Code, Date, Employee
- Source Sheet (Expo or Vits)
- Transferred At (timestamp)

**Why This Matters**: Provides audit trail and prevents sheet from growing too large. Users start fresh each day but can search history for past transactions.

---

### 8. Fill Blank Spaces Feature

**Purpose**: Auto-fill missing data in partially completed rows

**Trigger**: User clicks checkbox I3 or "Fill Blank Spaces" menu

**Logic**:
- **PO Number**: If blank, use PO from previous row with same Material ID
- **Quantity**: If blank, use Quantity from previous row with same Material ID
- **Heat**: If blank, use Heat from previous row with same Material ID
- **Date**: If blank, use current date
- **Employee**: If blank, use employee from first row

**Why This Matters**: Handles cases where user enters multiple lines quickly and skips duplicate values. System intelligently fills in the blanks.

---

### 9. Sort for Printing

**Purpose**: Organize data by logical grouping for efficient label printing

**Trigger**: User clicks checkbox I4 or "Sort for Printing" menu

**Sort Order**:
1. **Primary**: Material ID (ascending)
2. **Secondary**: PO Number (ascending)
3. **Tertiary**: Heat (ascending)

**Result**: All labels for same part/PO/heat group together

**Example Before Sort**:
```
PART-002, PO-14453, Heat-A
PART-001, PO-12345, Heat-B
PART-002, PO-14453, Heat-A
```

**Example After Sort**:
```
PART-001, PO-12345, Heat-B
PART-002, PO-14453, Heat-A
PART-002, PO-14453, Heat-A
```

**Why This Matters**: User can print all labels for one skid/coil consecutively without shuffling papers.

---

### 10. Search Functionality

**Location**: Dedicated "Search" sheet with dynamic dropdown menus

**Search By Options**:
- Material ID
- PO Number  
- Heat
- Special Code
- Date Range
- Employee

**Search For Dropdown**:
- Populated dynamically based on "Search By" selection
- Example: If "Search By" = Material ID, dropdown shows all unique Material IDs from History 2025

**Results Display**:
- Shows matching rows from History 2025
- Highlights results in yellow
- Shows row count: "Found 15 matches"

**Why This Matters**: Users can quickly find past receiving transactions without scrolling through thousands of historical rows.

---

### 11. Progress Bar System

**Visual Indicators**: Checkboxes in cells I2:I4 with status text in J2:J4

**Three Operations**:
1. **I2/J2**: Save to History → Shows "✓ Saved" or "⚠ Error"
2. **I3/J3**: Fill Blank Spaces → Shows "✓ Filled" or "⚠ No blanks"
3. **I4/J4**: Sort for Printing → Shows "✓ Sorted" or "⚠ Already sorted"

**Color Coding**:
- Green background = Success
- Yellow background = Warning
- Red background = Error

**Why This Matters**: Provides real-time feedback during batch operations. User knows immediately if something failed.

---

### 12. Automatic PO Number Formatting

**Input**: User types `66754` or `PO66754`  
**Output**: System formats as `PO-066754` (PO- prefix + zero-padded to 6 digits)

**Trigger**: When user edits column B (PO Number)

**Why This Matters**: Ensures consistency with Infor Visual PO format. Labels print correctly without manual formatting.

---

### 13. Automatic Material ID Padding

**Input**: User types `MMC848` or `MMF1234`  
**Output**: System pads to `MMC0000848` or `MMF0001234` (7 digits after prefix)

**Trigger**: When user edits column A (Material ID)

**Why This Matters**: Matches Infor Visual part numbering. Barcode scanners require exact format.

---

### 14. Email Reporting (Group and Push)

**Purpose**: Generate daily receiving summary for email notification

**Trigger**: User clicks "Gather Data for Email" menu item

**Process**:
1. Query History 2025 for today's transactions
2. Group by:
   - Employee
   - Material ID
   - PO Number
3. Calculate:
   - Total Quantity per group
   - Number of Labels per group
   - Transaction count
4. Generate formatted text report

**Output Format**:
```
Receiving Summary - 2025-12-15

Employee: 6229 (John Doe)
  - MMC0000848 (PO-66754): 3690 lbs, 8 labels
  - PART-001 (PO-14453): 2500 lbs, 5 labels
  Total: 6190 lbs, 13 labels

Employee: 1234 (Jane Smith)
  - PART-003 (PO-12345): 500 lbs, 2 labels
```

**Why This Matters**: Management gets automated daily reports without manual data compilation.

---

### 15. Diagnostic and Testing Tools

**Check History Table**:
- Validates table structure
- Checks for duplicate entries
- Verifies data integrity
- Reports missing required fields

**Test Progress Bar**:
- Simulates all three operations
- Shows timing (each step takes 2 seconds)
- Verifies visual feedback works

**Test Clearing Logic**:
- Confirms that "Save to History" clears source sheet
- Ensures headers remain intact
- Validates no data loss

**Why This Matters**: Allows troubleshooting without affecting production data.

---

## Key Workflow Summary

### Typical User Session:

1. **Open Spreadsheet** → System auto-initializes (resets progress, formats dates, sorts history)
2. **Enter Material ID** → System auto-fills PO, Quantity, Heat from history
3. **Adjust Values** → User modifies as needed for current receiving
4. **Add More Rows** → Repeat steps 2-3 for additional items
5. **Fill Blanks** → Click checkbox I3 to auto-complete missing fields
6. **Update Labels** → Click "Update Column H" to calculate label numbers
7. **Sort** → Click checkbox I4 to organize for printing
8. **Export CSV** → Data ready for LabelView import
9. **Save to History** → Click checkbox I2 to archive and clear sheet

### Daily Workflow:

- **Morning**: Open sheet, start fresh (yesterday's data in history)
- **Throughout Day**: Enter receiving transactions as materials arrive
- **End of Day**: Run "Gather Data for Email" for management report
- **Continuous**: Search past transactions as needed

---

## Integration with LabelView

**CSV Export Format** (matches NEW_APP_LABEL_TYPES.md requirements):
```csv
Date,EmployeeNumber,Heat,LocatedTo,PackagesOnSkid,PartID,PartType,PONumber,Quantity
```

**Export Location**:
- Network: `\\mtmanu-fs01\Expo Drive\Receiving\LabelViewQuickPrint\CSV Files\{Username}\`

**LabelView Workflow**:
1. User completes receiving entry in Google Sheets
2. User exports CSV via "Export to CSV" menu
3. User opens LabelView
4. LabelView loads CSV file automatically
5. User selects label template and prints

---

## Limitations of Current Google Sheets System

These pain points drive the need for the WinUI 3 replacement:

1. **No Direct Infor Visual Integration**: Users must manually look up part details
2. **No Automatic Label Printing**: Requires CSV export + LabelView manual workflow
3. **Limited Offline Access**: Requires internet connection
4. **No Real-Time Inventory Updates**: MaterialInventory requires manual refresh
5. **No Barcode Scanning**: All data entry is manual typing
6. **No Validation Rules**: Users can enter invalid data
7. **No Multi-User Collision Detection**: Two users can overwrite each other's data
8. **Limited Mobile Support**: Google Sheets mobile app has reduced functionality
9. **No Coil Label Auto-Generation**: User must manually calculate divided weights

---

## Features to Preserve in WinUI 3 App

✅ **Must Have**:
- Auto-fill from history (Material ID → pre-populate fields)
- Label number calculation ("X / Y" format)
- Progress indicators (save, fill, sort)
- Search historical transactions
- Material inventory calculations with gauge-to-thickness conversion
- Description parsing (extract dimensions from text)

✅ **Nice to Have**:
- Daily email summary report generation
- Diagnostic/testing modes

✅ **Replace/Improve**:
- CSV export → Direct LabelView integration or database storage
- Manual sorting → Auto-sort in UI
- Google Sheets limitations → Desktop application performance

---

**Next**: See [NEW_APP_FEATURE_COMPARISON.md](NEW_APP_FEATURE_COMPARISON.md) for feature-by-feature mapping from Google Sheets to WinUI 3 app.
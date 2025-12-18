# UPS/FedEx Internal Routing Label System - Business Logic Requirements

This document defines the business logic for internal routing labels (UPS/FedEx-style labels used for inter-department delivery) that must be implemented in the new WinUI 3 application.

---

## Purpose

**Internal routing labels** are used when received materials need to be delivered to specific departments or individuals rather than general warehouse storage. These labels are similar in format to UPS/FedEx shipping labels but are for **internal use only**.

**Key Use Case**: Parts received at the dock that need immediate delivery to:
- Specific production departments (Die Shop, Assembly, etc.)
- Individual employees (engineers, supervisors)
- Work orders or projects

---

## Core Features Required

### 1. Data Entry Form

**Required Fields**:
- **Deliver To**: Recipient name or "Unknown" (dropdown from common recipients)
- **Department**: Destination department (dropdown: Die Shop, Assembly, Machining, QC, etc.)
- **Package Description**: Brief description of contents
- **PO Number**: Associated purchase order (auto-formatted to "PO-XXXXXX")
- **Work Order**: Optional work order number (format: "WO-XXXXX")
- **Employee Number**: Receiving employee (auto-populated from login)
- **Label Number**: Auto-incremented per session

**UI Layout**:
```
┌─────────────────────────────────────────────────┐
│ Internal Routing Label Entry                    │
├─────────────────────────────────────────────────┤
│ Deliver To:   [Bill Schmidt ▼]                 │
│ Department:   [Die Shop ▼]                     │
│ Description:  [Raw material for WO-63444]      │
│ Work Order:   [WO-_____] (optional)            │
│ PO Number:    [PO-______]                      │
│ Received By:  [6229] (auto-filled)             │
│ Label #:      [1] (auto-incremented)           │
│                                                 │
│ [Add to Queue]  [Print Labels]                 │
└─────────────────────────────────────────────────┘
```

---

### 2. Dropdown Population from Lookup Tables

**Deliver To Dropdown**:
- Populated from "Dropdowns" configuration table
- Common recipients:
  - Bill Schmidt
  - John Doe
  - Engineering Team
  - Production Supervisor
  - Quality Control
  - Unknown (default)

**Department Dropdown**:
- Die Shop
- Assembly
- Machining
- Quality Control
- Inspection
- Shipping
- Engineering
- Office

**Purpose**: Standardize routing destinations and prevent typos.

---

### 3. Auto-Increment Label Numbers

**Logic**: When user adds a routing label entry:
1. Check previous entry's label number
2. Increment by 1
3. Assign to new entry

**Example Session**:
```
Entry 1: Label #1 → Die Shop / Bill Schmidt
Entry 2: Label #2 → Assembly / John Doe
Entry 3: Label #3 → Die Shop / Bill Schmidt
```

**Reset**: Label numbers reset when user saves to history or starts new session.

---

### 4. Row Duplication Feature

**Use Case**: User needs multiple labels for same delivery destination

**Workflow**:
1. User selects existing row
2. User clicks "Duplicate Row" or similar action
3. System copies all field values to new row
4. System increments label number automatically

**Example**:
```
Original: Deliver To: Bill Schmidt, Dept: Die Shop, Label: 1
Duplicate: Deliver To: Bill Schmidt, Dept: Die Shop, Label: 2
```

**Purpose**: Quickly create multiple labels for same shipment going to same destination.

---

### 5. Field Lookup from Master Data

**When user selects "Deliver To" recipient**:
- Auto-populate **Department** if recipient has default department assigned
- Example: "Bill Schmidt" → Auto-fills "Die Shop"

**Lookup Table Structure**:
```
Recipient Name → Default Department
Bill Schmidt → Die Shop
John Doe → Machining
Engineering Team → Engineering
```

**Override Allowed**: User can manually change department after auto-fill.

---

### 6. Blank Row Auto-Clearing

**Trigger**: User clears the "Deliver To" field in a row

**Action**: System clears the entire row to prevent partial data

**Why This Matters**: If user accidentally starts entering data in wrong row, clearing "Deliver To" resets everything.

---

### 7. Alternating Row Colors (Display Only)

**Visual Enhancement**: In the entry grid, alternate between two background colors:
- Light gray (#f0f0f0)
- Darker gray (#d9d9d9)

**Apply to**: Only rows with data (skip blank rows)

**Purpose**: Improves readability when scanning multiple routing labels.

---

### 8. Save to History Archive

**Workflow**:
1. User completes routing label entries for the day
2. User clicks "Copy Today to History 2025" or similar
3. System validates required fields:
   - Deliver To (cannot be blank)
   - Department (cannot be blank)
   - PO Number (required)
4. System transfers validated rows to history table
5. System clears current entries
6. System sorts history by date (most recent first)

**Confirmation Required**: *"Transfer today's routing labels to history? This will clear current data. Yes/No"*

**History Table Columns**:
- Deliver To
- Department
- Package Description
- PO Number
- Work Order Number
- Employee Number
- Label Number
- Transaction Date
- Saved At (timestamp)

---

### 9. Visual Feedback (History View)

**Date-Based Row Grouping with Color Coding**:
- Alternate background colors by transaction date
- Add borders between different dates
- Example:
  ```
  12/15/2025 entries → Light background
  12/14/2025 entries → Darker background
  12/13/2025 entries → Light background
  ```

**Purpose**: Visual separation makes it easy to identify labels from different days.

---

### 10. CSV Export for Label Printing

**Export Format**:
```csv
DeliverTo,Department,PackageDescription,PONumber,WorkOrderNumber,EmployeeNumber,LabelNumber
"Bill Schmidt","Die Shop","Raw material for WO-63444","PO-066756","WO-63444",6229,1
"John Doe","Assembly","Components for Project X","PO-066757","",6229,2
```

**Export Location**:
- Local: `%APPDATA%\RoutingLabels.csv`
- Network: `\\mtmanu-fs01\Expo Drive\Receiving\LabelViewQuickPrint\CSV Files\{Username}\RoutingLabels.csv`

**LabelView Template**: "Expo - Mini UPS Label ver. 1.0"

---

### 11. Supply Data Grouping for Reports

**Purpose**: Generate daily summary of internal deliveries by department

**Grouping Logic**:
- Group by **Department**
- Count total routing labels per department
- List recipients for each department

**Output Format**:
```
Internal Routing Summary - 2025-12-15

Die Shop: 5 deliveries
  - Bill Schmidt (3 labels)
  - Jane Doe (2 labels)

Assembly: 3 deliveries
  - John Doe (2 labels)
  - Production Team (1 label)

Engineering: 1 delivery
  - Engineering Team (1 label)
```

---

## Business Rules

### Required Fields
- **Deliver To** (cannot be blank)
- **Department** (cannot be blank)
- **PO Number** (required)
- **Employee Number** (auto-populated from user login)
- **Label Number** (auto-incremented)

### Optional Fields
- **Package Description** (recommended but not required)
- **Work Order Number** (only if related to specific work order)

### Validation Rules
- PO Number must be numeric (auto-formatted with "PO-" prefix)
- Work Order Number must follow "WO-XXXXX" format if provided
- Deliver To must match dropdown values or be manually entered
- Department must match dropdown values

---

## Key Differences from Other Label Types

| Aspect | UPS/FedEx Labels | Receiving Labels | Dunnage Labels |
|--------|------------------|------------------|----------------|
| **Purpose** | Internal routing | Material receiving | Packaging tracking |
| **Recipient** | Required | Not applicable | Not applicable |
| **Department** | Required | Optional | Not tracked |
| **Part ID** | Not required | Required | Not required |
| **Quantity** | Not tracked | Required | Not tracked |
| **Work Order** | Optional | Not typical | Not tracked |

---

## WinUI 3 Implementation Notes

### Recommended UI Pattern

**DataGrid with Editable Cells**:
```
┌────────────────────────────────────────────────────────────────────┐
│ Deliver To    │ Department │ Description        │ PO Number │ WO   │
├────────────────────────────────────────────────────────────────────┤
│ Bill Schmidt  │ Die Shop   │ Raw materials      │ PO-066756 │ WO-1 │
│ John Doe      │ Assembly   │ Components         │ PO-066757 │      │
│ [Select ▼]    │            │                    │           │      │
└────────────────────────────────────────────────────────────────────┘

[Duplicate Selected Row]  [Clear All]  [Save & Print]
```

---

### Data Model

```csharp
public class Model_RoutingLabel
{
    public string DeliverTo { get; set; } = "Unknown";
    public string Department { get; set; } = "Unknown";
    public string PackageDescription { get; set; } = string.Empty;
    public int PONumber { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public int EmployeeNumber { get; set; }
    public int LabelNumber { get; set; } = 1;
    public DateTime TransactionDate { get; set; } = DateTime.Now;
}

public class Model_RecipientLookup
{
    public string RecipientName { get; set; } = string.Empty;
    public string DefaultDepartment { get; set; } = string.Empty;
}
```

---

### Service Methods Required

```csharp
// Add routing label entry
Task<Model_Dao_Result<bool>> AddRoutingLabelAsync(Model_RoutingLabel label);

// Duplicate existing label
Task<Model_Dao_Result<Model_RoutingLabel>> DuplicateRoutingLabelAsync(int labelId);

// Save entries to history
Task<Model_Dao_Result<bool>> SaveRoutingLabelsToHistoryAsync(List<Model_RoutingLabel> labels);

// Export to CSV for LabelView
Task<Model_Dao_Result<string>> ExportRoutingLabelsToCSVAsync(List<Model_RoutingLabel> labels);

// Get recipient lookup data
Task<Model_Dao_Result<List<Model_RecipientLookup>>> GetRecipientLookupsAsync();

// Get department list
Task<Model_Dao_Result<List<string>>> GetDepartmentsAsync();

// Get routing history
Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetRoutingHistoryAsync(DateTime? startDate, DateTime? endDate);

// Generate daily summary by department
Task<Model_Dao_Result<string>> GenerateRoutingSummaryAsync(DateTime date);
```

---

## MySQL Database Schema

```sql
CREATE TABLE `routing_labels` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `deliver_to` VARCHAR(100) NOT NULL,
  `department` VARCHAR(100) NOT NULL,
  `package_description` VARCHAR(255),
  `po_number` INT NOT NULL,
  `work_order_number` VARCHAR(50),
  `employee_number` INT NOT NULL,
  `label_number` INT NOT NULL,
  `transaction_date` DATE NOT NULL,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_deliver_to (`deliver_to`),
  INDEX idx_department (`department`),
  INDEX idx_po (`po_number`),
  INDEX idx_date (`transaction_date`)
);

CREATE TABLE `recipient_lookups` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `recipient_name` VARCHAR(100) UNIQUE NOT NULL,
  `default_department` VARCHAR(100),
  `is_active` TINYINT(1) DEFAULT 1,
  INDEX idx_recipient (`recipient_name`)
);

CREATE TABLE `departments` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `department_name` VARCHAR(100) UNIQUE NOT NULL,
  `is_active` TINYINT(1) DEFAULT 1
);
```

---

**Next**: See [NEW_APP_LABEL_TYPES.md](NEW_APP_LABEL_TYPES.md) for UPS/FedEx Label format specifications.
    .addItem('Copy Zeros to Column A', 'addZeroToColumnA')
    .addItem('Copy Selected Row to First Empty Row', 'copySelectedRowToFirstEmptyRow')
    .addItem('EoD Email', 'groupSupplyData')
    .addToUi();
}

function addZeroToColumnA() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var range = sheet.getRange("A1:A" + sheet.getLastRow());
  var values = range.getValues();

  for (var i = 0; i < values.length; i++) {
    if (values[i][0] !== "") {
      values[i][0] = "0" + values[i][0];
    }
  }

  range.setValues(values);
}

function onEdit(e) {
  var sheet = e.source.getActiveSheet();
  var range = e.range;
  
  // Check if the edited sheet is named "Today"
  if (sheet.getName() !== "Today") {
    return;
  }
  
  // Check if column A of the edited row is blank
  if (range.getColumn() == 1 && range.getValue() === "") {
    // Clear the entire row
    sheet.getRange(range.getRow(), 1, 1, sheet.getLastColumn()).clearContent();
    // Apply alternating row colors
    applyAlternatingRowColors(sheet);
    return; // Exit the function as we don't need to process further
  }
  
  // Check if the edited cell is in column A
  if (range.getColumn() == 1 && range.getRow() > 1) {
    var value = range.getValue();
    var dropdownsSheet = sheet.getParent().getSheetByName('Dropdowns');
    var dropdownsRangeA = dropdownsSheet.getRange('A2:A');
    var dropdownsRangeB = dropdownsSheet.getRange('B2:B');
    var dropdownsRangeC = dropdownsSheet.getRange('C2:C');


    
    var resultB = lookupValue(value, dropdownsRangeA, dropdownsRangeB);
    var resultC = lookupValue(value, dropdownsRangeA, dropdownsRangeC);
    
    // Set the result in the corresponding cell in column B
    sheet.getRange(range.getRow(), 2).setValue(resultB);
    // Set the result in the corresponding cell in column D
    if (resultC != ""){
    sheet.getRange(range.getRow(), 4).setValue(resultC);
    };
    
    // Set today's date in column F
    var todayDate = Utilities.formatDate(new Date(), Session.getScriptTimeZone(), 'MM/dd/yyyy');
    sheet.getRange(range.getRow(), 6).setValue(todayDate);
    
    // Set the contents of I1 in column E
    var i1Value = sheet.getRange('I1').getValue();
    sheet.getRange(range.getRow(), 5).setValue(i1Value);
    
    // Set the value in column G
    if (range.getRow() == 2) {
      sheet.getRange(range.getRow(), 7).setValue(1);
    } else {
      var previousValue = sheet.getRange(range.getRow() - 1, 7).getValue();
      sheet.getRange(range.getRow(), 7).setValue(previousValue + 1);
    }
    
    // Apply alternating row colors if column B is filled successfully
    if (resultB !== "") {
      applyAlternatingRowColors(sheet);
    }
  }
  
  // Add dropdown to the first blank row's column A cell
  addDropdownToFirstBlankRow(sheet);
}

function copySelectedRow() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var range = sheet.getActiveRange();
  
  // Get the row number of the selected cell
  var selectedRow = range.getRow();
  
  // Get the values of the selected row
  var rowValues = sheet.getRange(selectedRow, 1, 1, sheet.getLastColumn()).getValues();
  
  // Find the first blank row
  var firstBlankRow = sheet.getLastRow() + 1;
  for (var i = 1; i <= sheet.getLastRow(); i++) {
    if (sheet.getRange(i, 1, 1, sheet.getLastColumn()).isBlank()) {
      firstBlankRow = i;
      break;
    }
  }
  
  // Copy the values to the first blank row
  sheet.getRange(firstBlankRow, 1, 1, rowValues[0].length).setValues(rowValues);
  
  // Update column G in the new row
  if (firstBlankRow == 2) {
    sheet.getRange(firstBlankRow, 7).setValue(1);
  } else {
    var previousValue = sheet.getRange(firstBlankRow - 1, 7).getValue();
    sheet.getRange(firstBlankRow, 7).setValue(previousValue + 1);
  }
}

function lookupValue(value, rangeA, rangeB) {
  var valuesA = rangeA.getValues();
  var valuesB = rangeB.getValues();
  


  for (var i = 0; i < valuesA.length; i++) {
    if (valuesA[i][0] == value) {
      return valuesB[i][0];
    }
  }
  
  return "";
}

function addDropdownToFirstBlankRow(sheet) {
  var lastRow = sheet.getLastRow();
  var firstBlankRow = lastRow + 1;
  
  // Get the range for the dropdown values
  var dropdownsSheet = sheet.getParent().getSheetByName('Dropdowns');
  var dropdownsRange = dropdownsSheet.getRange('A2:A');
  
  // Create the data validation rule
  var rule = SpreadsheetApp.newDataValidation()
    .requireValueInRange(dropdownsRange)
    .setAllowInvalid(false)
    .build();
  
  // Apply the data validation rule to the first blank row's column A cell
  sheet.getRange(firstBlankRow, 1).setDataValidation(rule);
}

function applyAlternatingRowColors(sheet) {
  var lastRow = sheet.getLastRow();
  var range = sheet.getRange(2, 1, lastRow - 1, 7); // Columns A-F, excluding header row
  
  // Clear existing background colors and borders
  range.setBackground(null);
  range.setBorder(false, false, false, false, false, false);
  
  // Apply alternating row colors and borders
  var values = range.getValues();
  var color1 = "#f0f0f0"; // Light grey
  var color2 = "#d9d9d9"; // Darker grey
  
  for (var i = 0; i < values.length; i++) {
    if (values[i][0] !== "") { // Check if column A is not blank
      var color = (i % 2 === 0) ? color1 : color2;
      var rowRange = sheet.getRange(i + 2, 1, 1, 7);
      rowRange.setBackground(color);
      rowRange.setBorder(true, true, true, true, null, null); // Add borders to the outside
      rowRange.setBorder(null, null, true, null, null, null); // Add border to the bottom
    }
  }
}





function copyTodayToHistory() {
  var ui = SpreadsheetApp.getUi();
  var response = ui.alert(
    'Confirmation',
    'You will no longer be able to print labels for the Material entered in this sheet. Are you sure you wish to continue?',
    ui.ButtonSet.YES_NO
  );

  // Process the user's response.
  if (response != ui.Button.YES) {
    // User clicked "No" or closed the dialog.
    return;
  }

  var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  var todaySheet = spreadsheet.getSheetByName("Today");
  var historySheet = spreadsheet.getSheetByName("History 2025");
  
  if (!todaySheet || !historySheet) {
    throw new Error('Sheets "Today" or "History 2025" not found.');
  }
  
  // Check if there is any data in row 2 or above in the "Today" sheet
  var checkDataRange = todaySheet.getRange(2, 1, todaySheet.getLastRow() - 1, 6); // Columns A-F
  var checkData = checkDataRange.getValues();
  var hasData = checkData.some(function(row) {
    return row.some(function(cell) {
      return cell !== "";
    });
  });
  
  if (!hasData) {
    Logger.log("No data found in the 'Today' sheet starting from row 2");
    return;
  }
  
  // Get the data from the "Today" sheet starting from row 2, columns A-F
  var todayDataRange = todaySheet.getRange(2, 1, todaySheet.getLastRow() - 1, 6); // Columns A-F
  var todayData = todayDataRange.getValues();
  
  // Get the data from the "History 2025" sheet starting from row 2, columns A-F
  var historyDataRange = historySheet.getRange(2, 1, historySheet.getLastRow() - 1, 6); // Columns A-F
  var historyData = historyDataRange.getValues();
  
  // Find the first completely blank row in the "History 2025" sheet
  var firstBlankRow = historyData.length + 2;
  for (var i = 0; i < historyData.length; i++) {
    if (historyData[i][0] === "") { // Check if column A is blank
      firstBlankRow = i + 2;
      break;
    }
  }
  
  // Copy the data to the "History 2025" sheet starting from the first blank row
  if (todayData.length > 0) {
    historySheet.getRange(firstBlankRow, 1, todayData.length, todayData[0].length).setValues(todayData);
  }
  
  // Find the first row with a blank dropdown in column A
  var lastRow = todaySheet.getLastRow();
  for (var i = 2; i <= lastRow; i++) {
    if (todaySheet.getRange(i, 1).getValue() === "") {
      lastRow = i - 1;
      break;
    }
  }
  
  // Delete rows 2 and above in the "Today" sheet and re-add them as blank rows after row 3
  var rowsToDelete = lastRow - 1;
  if (rowsToDelete > 0) {
    todaySheet.deleteRows(2, rowsToDelete);
    todaySheet.insertRowsAfter(3, rowsToDelete); // Add the same number of blank rows starting from row 3
  }

  // Sort everything from row 2 and above by column F in reverse order
  historySheet.getRange(2, 1, historySheet.getLastRow() - 1, 6) // Columns A-F
    .sort({ column: 6, ascending: false });
  colorHistory();
}




function colorHistory() {
  var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  var sourceSheet = spreadsheet.getSheetByName("History 2025");
  var startRow = 2;
  
  if (!sourceSheet) {
    throw new Error('Sheet with name "History 2025" not found.');
  }
  
  var dataRange = sourceSheet.getRange(startRow, 1, sourceSheet.getLastRow() - 1, 6); // Columns A-F
  var data = dataRange.getValues();
  
  // Clear all grid lines from rows 2 and above
  dataRange.setBorder(false, false, false, false, false, false);
  
  var previousDate = null;
  var currentColor = '#D3D3D3'; // Very light grey
  var textColor = 'black'; // Black for very light grey
  
  for (var i = 0; i < data.length; i++) {
    var currentDate = data[i][5].toString(); // Assuming the date is in column F (index 5)
    
    if (previousDate !== currentDate) {
      currentColor = currentColor === '#D3D3D3' ? '#aee6ad' : '#D3D3D3'; // Switch color to very light blue or very light grey
      textColor = currentColor === '#D3D3D3' ? 'black' : '#0e1a0e'; // Black for very light grey, dark blue for very light blue
      // Add an upper grid line above the first unique date, only for columns A to F
      var range = sourceSheet.getRange(i + startRow, 1, 1, 6); // Columns A to F
      if (i + startRow > 2) { // Avoid setting border on the header row
        range.setBorder(true, null, null, null, null, null, 'black', SpreadsheetApp.BorderStyle.SOLID);
      }
    }
    previousDate = currentDate;
    
    var range = sourceSheet.getRange(i + startRow, 1, 1, 6); // Columns A-F
    range.setBackground(currentColor);
    range.setFontColor(textColor);
    range.setFontSize(10);
    range.setFontWeight('normal');
    range.setFontStyle('normal');
    range.setFontFamily('Roboto');
  } 
}

function groupSupplyData() {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const sheet = ss.getSheetByName('History 2025');
  const receivingSheet = ss.getSheetByName('Receiving EoD Emails');

  // Get date range from Receiving EoD Emails!C2 and E2
  const startDateRaw = receivingSheet.getRange('C2').getValue();
  const endDateRaw = receivingSheet.getRange('E2').getValue();
  const startDate = new Date(startDateRaw);
  const endDate = new Date(endDateRaw);

  Logger.log('Start Date: ' + startDateRaw + ' -> ' + startDate);
  Logger.log('End Date: ' + endDateRaw + ' -> ' + endDate);

  // Read all 6 columns: Deliver To, Department, Package Description, PO Number, Employee, Date
  const totalCols = 6;
  const rawRows = sheet.getLastRow() - 1;
  Logger.log('Total data rows (excluding header): ' + rawRows);

  if (rawRows <= 0) {
    Logger.log("No data rows in History 2025!");
    return;
  }

  const data = sheet.getRange(2, 1, rawRows, totalCols).getValues();
  Logger.log('First 2 rows of data: ' + JSON.stringify(data.slice(0, 2)));

  function normalizePO(value) {
    if (value === "" || value === null) return "No PO";
    value = value.toString().trim();
    // Accept already normalized
    if (/^PO-0\d{5,6}[A-Za-z]?$/.test(value)) return value;
    // 5-6 digits, optional letter
    let match = /^0?(\d{5,6})([A-Za-z]?)$/.exec(value);
    if (match) {
      let digits = match[1];
      let letter = match[2] || "";
      digits = digits.padStart(6, '0');
      return "PO-" + digits + letter;
    }
    // <5 digits, numeric
    if (/^\d{1,4}$/.test(value)) return "Validate PO";
    // words or anything else
    return value;
  }

  // Filter by date range (date is column F, index 5)
  let filteredRows = [];
  for (let i = 0; i < data.length; i++) {
    let row = data[i];
    let dateVal = new Date(row[5]);
    if (isNaN(dateVal)) {
      try {
        dateVal = Utilities.parseDate(row[5], Session.getScriptTimeZone(), "MM/dd/yyyy");
        Logger.log('Parsed date (row ' + (i+2) + ') using Utilities: ' + dateVal);
      } catch (e) {
        Logger.log('Invalid date (row ' + (i+2) + '): ' + row[5]);
        continue;
      }
    }
    if (dateVal >= startDate && dateVal <= endDate) {
      // Replace PO Number (column D/index 3) with validated/normalized value
      let outputRow = row.slice(); // Copy the row
      outputRow[3] = normalizePO(row[3]);
      filteredRows.push(outputRow);
      Logger.log('Include row ' + (i+2) + ': ' + JSON.stringify(outputRow));
    }
  }
  Logger.log("Filtered rows count: " + filteredRows.length);

  // Sort by Deliver To (column A, index 0) or by any other column you wish
  filteredRows.sort(function(a, b) {
    // If you have a Quantity column (else replace with string sort)
    // return Number(a[0]) - Number(b[0]);
    return a[0].localeCompare(b[0]); // Sort by Deliver To as string, you may adjust
  });
  Logger.log('First 2 sorted rows: ' + JSON.stringify(filteredRows.slice(0,2)));

  // Remove all rows below header (assume header on row 5; data starts at row 6)
  const headerRow = 5;
  const firstDataRow = 6;
  const numRows = receivingSheet.getMaxRows();
  if (numRows > headerRow) {
    receivingSheet.deleteRows(firstDataRow, numRows - headerRow);
    Logger.log('Deleted rows below header; new sheet last row: ' + receivingSheet.getLastRow());
  }

  // Write output to B6
  if (filteredRows.length > 0) {
    receivingSheet.insertRowsAfter(headerRow, filteredRows.length);
    receivingSheet.getRange(firstDataRow, 2, filteredRows.length, totalCols)
      .setValues(filteredRows);
    Logger.log('Inserted ' + filteredRows.length + ' rows starting at B' + firstDataRow + '.');
  } else {
    Logger.log('No rows to output (filteredRows is empty).');
  }
}
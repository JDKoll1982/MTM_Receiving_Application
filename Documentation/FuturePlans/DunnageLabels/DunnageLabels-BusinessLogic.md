# Dunnage Label System - Business Logic Requirements

This document defines the business logic for the Dunnage Label workflow that must be implemented in the new WinUI 3 application.

---

## Purpose

**Dunnage labels** track returnable packaging materials (pallets, crates, skids, containers) separate from part/material receiving. This ensures proper tracking of packaging assets that need to be returned to suppliers or reused.

---

## Core Features Required

### 1. Data Entry Form

**Required Fields** (as shown in Google Sheets table view):
- **Part Number / Line 1**: Free text (primary identifier, e.g., "Pallet #1234" or dunnage type)
- **Quantity / Line 2**: Free text (secondary information, e.g., quantity or condition)
- **PO Number**: Associated purchase order (auto-formatted to "PO-XXXXXX")
- **Employee**: Employee number (auto-populated from login, shown as "Employee: 6229" in right panel)
- **Date**: Date received (auto-populated with current date)
- **Location**: Storage location or routing code

**UI Layout** (Table View with Filtering):
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Table1 ▼  [Filter Icon]                      Employee: 6229   YFHA   0 of 0│
├──────────────┬──────────────┬────────────┬──────────┬────────┬─────────────┤
│ Part Number/ │ Quantity/    │ PO Number  │ Employee │ Date   │ Location    │
│ Line 1   ▼   │ Line 2   ▼   │        ▼   │      ▼   │    ▼   │         ▼   │
├──────────────┼──────────────┼────────────┼──────────┼────────┼─────────────┤
│              │              │            │          │        │             │
│              │              │            │          │        │             │
└──────────────┴──────────────┴────────────┴──────────┴────────┴─────────────┘

                                         [Save] (green)  [Fill] (cyan)
```

**Note**: Screenshot shows table filtering enabled (▼ dropdown arrows on each column header) and action buttons "Save" (green) and "Fill" (cyan) at bottom right.

---

### 2. Auto-Fill Behavior

**When user enters data in LINE 1 or LINE 2**:
- Automatically populate **Date** with current system date
- Automatically populate **Employee** from logged-in user profile
- Retain **PO Number** from previous entry (if any)

**Purpose**: Minimize repetitive data entry when processing multiple dunnage items from same shipment.

---

### 3. Row Clearing Logic

**Trigger**: User deletes all critical fields (LINE 1, LINE 2, PO Number, Date)

**Action**: Clear the entire row to prevent partial/invalid data

**Why This Matters**: Prevents accidentally saving incomplete dunnage records that would generate invalid labels.

---

### 4. Save to History Archive

**Workflow**:
1. User completes one or more dunnage entries
2. User clicks "Save to History" or similar action
3. System validates required fields (LINE 1 or LINE 2 must have value)
4. System transfers validated rows to permanent history table
5. System clears current entry form for next use
6. System sorts history by date (most recent first)

**Confirmation Required**: Prompt user before saving: *"Transfer dunnage entries to history? This will clear current data."*

**History Table Columns**:
- LINE 1
- LINE 2
- PO Number
- Transaction Date
- Employee Number
- Vendor Name
- Saved At (timestamp)

---

### 5. Visual Feedback (History View)

**Date-Based Row Grouping**: When displaying history, alternate row colors by transaction date to visually separate different days.

**Example**:
```
12/15/2025 entries → Light gray background
12/14/2025 entries → Darker gray background
12/13/2025 entries → Light gray background
```

**Purpose**: Makes it easy to visually scan historical data by date without reading dates individually.

---

### 6. CSV Export for LabelView

**Export Format**:
```csv
LINE1,LINE2,PONumber,Date,EmployeeNumber,VendorName
"Pallet #1234","Good Condition","PO-066754","12/15/2025",6229,"MST Steel Corporation"
```

**Export Location**:
- Local: `%APPDATA%\DunnageData.csv`
- Network: `\\mtmanu-fs01\Expo Drive\Receiving\LabelViewQuickPrint\CSV Files\{Username}\DunnageData.csv`

**LabelView Template**: "Expo - Dunnage Label ver. 1.0"

---

### 7. Data Grouping for Reports

**Purpose**: Generate daily summary of dunnage received

**Grouping Logic**:
- Group by **PO Number**
- Count total dunnage items per PO
- List LINE 1 values for each group

**Output Format**:
```
Dunnage Summary - 2025-12-15

PO-066754: 5 items
  - Pallet #1234
  - Pallet #1235
  - Crate A
  - Crate B
  - Skid #10

PO-014453: 2 items
  - Container XYZ
  - Pallet #5000
```

---

## Business Rules

### Required Fields
- **At least one** of LINE 1 or LINE 2 must have a value
- **PO Number** is required
- **Date** is required (auto-populated)
- **Employee Number** is required (auto-populated)

### Optional Fields
- **Vendor Name** (defaults to "Unknown")

### Validation Rules
- PO Number must be numeric (auto-formatted with "PO-" prefix)
- Date must be valid MM/DD/YYYY format
- LINE 1 and LINE 2: Max 100 characters each

---

## Key Differences from Receiving Labels

| Aspect | Dunnage Labels | Receiving Labels |
|--------|----------------|------------------|
| **Purpose** | Track packaging | Track parts/materials |
| **Part ID** | Not required | Required |
| **Quantity** | Not tracked | Required |
| **Heat Number** | Not tracked | Required |
| **Primary ID** | LINE 1 / LINE 2 text | Part ID + PO Number |
| **Label Format** | Simple text label | Detailed with barcodes |

---

## WinUI 3 Implementation Notes

### Recommended UI Pattern

**DataGrid** for multi-line entry:
```
┌──────────────────────────────────────────────────────────┐
│  LINE 1       │ LINE 2         │ PO Number │ Date       │
├──────────────────────────────────────────────────────────┤
│ Pallet #1234  │ Good Condition │ PO-066754 │ 12/15/2025 │
│ Pallet #1235  │ Minor Damage   │ PO-066754 │ 12/15/2025 │
│ [New Entry]   │                │           │            │
└──────────────────────────────────────────────────────────┘
```

**Or simplified single-entry form** if users typically enter one dunnage item at a time.

---

### Data Model

```csharp
public class Model_DunnageLine
{
    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public int PONumber { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int EmployeeNumber { get; set; }
    public string VendorName { get; set; } = "Unknown";
    public int LabelNumber { get; set; } = 1;
}
```

---

### Service Methods Required

```csharp
// Add dunnage entry
Task<Model_Dao_Result<bool>> AddDunnageEntryAsync(Model_DunnageLine line);

// Save entries to history
Task<Model_Dao_Result<bool>> SaveDunnageToHistoryAsync(List<Model_DunnageLine> lines);

// Export to CSV for LabelView
Task<Model_Dao_Result<string>> ExportDunnageToCSVAsync(List<Model_DunnageLine> lines);

// Get dunnage history
Task<Model_Dao_Result<List<Model_DunnageLine>>> GetDunnageHistoryAsync(DateTime? startDate, DateTime? endDate);

// Generate daily summary
Task<Model_Dao_Result<string>> GenerateDunnageSummaryAsync(DateTime date);
```

---

## MySQL Database Schema

```sql
CREATE TABLE `label_table_dunnage` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `line1` VARCHAR(100) NOT NULL,
  `line2` VARCHAR(100),
  `po_number` INT NOT NULL,
  `transaction_date` DATE NOT NULL,
  `employee_number` INT NOT NULL,
  `vendor_name` VARCHAR(255) DEFAULT 'Unknown',
  `label_number` INT DEFAULT 1,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_po (`po_number`),
  INDEX idx_date (`transaction_date`),
  INDEX idx_employee (`employee_number`)
);
```

---

**Next**: See [NEW_APP_LABEL_TYPES.md](NEW_APP_LABEL_TYPES.md) for Dunnage Label format specifications.
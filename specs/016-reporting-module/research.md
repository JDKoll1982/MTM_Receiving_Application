# Research: End-of-Day Reporting Module Google Sheets System

**Source**: Google Sheets routing label system (`Documentation/FuturePlans/RoutingLabels/`)  
**Analysis Date**: 2026-01-03  
**Purpose**: Document business logic extracted from Google Sheets for WinUI 3 implementation

## Source Files Analyzed

### 1. EndOfDayEmail.js
**Purpose**: End-of-Day email generation with PO normalization and data formatting

**Key Functions**:
- `normalizePO(poNumber)`: PO number normalization algorithm
- `generateEmailBody(data, startDate, endDate)`: Email body generation with date grouping
- `exportToCSV(data)`: CSV export matching MiniUPSLabel.csv structure

**Business Logic Extracted**:
1. **PO Normalization Algorithm**:
   - `"63150"` → `"PO-063150"` (pad to 6 digits, add PO- prefix)
   - `"063150B"` → `"PO-063150B"` (preserve suffix)
   - `"Customer Supplied"` → `"Customer Supplied"` (pass through)
   - `""` → `"No PO"` (empty string)
   - `"1234"` → `"Validate PO"` (too short, needs validation)

2. **Date Grouping**: Group data by date for email formatting
3. **CSV Export**: Match MiniUPSLabel.csv column structure
4. **Email Formatting**: HTML table with alternating row colors

### 2. AppScript.js
**Purpose**: Main routing label automation with history archival

**Key Functions**:
- `colorHistory()`: Apply alternating row colors grouped by date
- `saveHistory()`: Archive today's labels to history
- `getDepartmentByPerson(person)`: Auto-lookup department from recipient

**Business Logic Extracted**:
1. **Department Auto-Lookup**: When selecting recipient, department auto-fills
2. **History Archival**: "Today" → "History" with confirmation
3. **Date Grouping**: Alternating row colors (#D3D3D3 / #FFFFFF) by date
4. **Label Numbering**: Auto-increment with duplicate row support

### 3. MiniUPSLabel.csv
**Purpose**: CSV structure reference for LabelView compatibility

**Structure**:
- Columns: PO Number, Part, Description, Quantity, Weight, Heat/Lot, Date, Employee
- Format: CSV with headers
- Purpose: Import into LabelView for label printing

## Key Business Rules Identified

### PO Normalization Algorithm
```javascript
function normalizePO(poNumber) {
    if (!poNumber || poNumber.trim() === "") return "No PO";
    if (poNumber === "Customer Supplied") return "Customer Supplied";
    if (poNumber.length < 5) return "Validate PO";
    
    // Remove existing PO- prefix if present
    let cleaned = poNumber.replace(/^PO-/, "");
    
    // Pad to 6 digits if numeric
    if (/^\d+$/.test(cleaned)) {
        cleaned = cleaned.padStart(6, "0");
    }
    
    // Add PO- prefix
    return "PO-" + cleaned;
}
```

### Email Formatting Rules
- **Date Grouping**: Group rows by date
- **Alternating Colors**: #D3D3D3 (light gray) / #FFFFFF (white) by date group
- **Table Format**: HTML table with headers
- **Column Order**: PO Number, Part, Description, Quantity, Weight, Heat/Lot, Date

### CSV Export Format
- **Structure**: Match MiniUPSLabel.csv exactly
- **Columns**: PO Number (normalized), Part, Description, Quantity, Weight, Heat/Lot, Date, Employee
- **Format**: CSV with headers, UTF-8 encoding

## Reimplementation Strategy

### Cross-Module Integration
- **Receiving Module**: Query `vw_receiving_history` view
- **Dunnage Module**: Query `vw_dunnage_history` view
- **Routing Module**: Query `vw_routing_history` view
- **Volvo Module**: Query `vw_volvo_history` view

### PO Normalization
- **Implementation**: C# helper class `Helper_PONormalizer`
- **Shared**: Used by all modules (Receiving, Routing, Volvo, Reporting)
- **Algorithm**: Exact match of JavaScript normalizePO() function

### Email Formatting
- **Format**: HTML table with inline styles
- **Grouping**: Group by date, apply alternating colors
- **Output**: Plain text or HTML (user choice)

### Module Selection
- **UI**: Checkbox selection (not dropdown)
- **Availability**: Check data counts per module, disable checkboxes with no data
- **Multi-Select**: Support multiple modules in one report (tabbed interface)

## Migration Notes

### Preserved Functionality
- ✅ PO normalization algorithm (exact match)
- ✅ CSV export format (MiniUPSLabel.csv structure)
- ✅ Email formatting (date grouping, alternating colors)
- ✅ Date range filtering

### Enhanced Functionality
- ➕ Database views (replaces Google Sheets queries)
- ➕ MVVM architecture (replaces AppScript automation)
- ➕ Multi-module reporting (tabbed interface)
- ➕ Better error handling
- ➕ Availability checking (disable checkboxes with no data)

### Removed Functionality
- ❌ Google Sheets automation (replaced with MVVM)
- ❌ Manual email sending (user copies text and sends via Outlook)

## References

- **Source Files**: `Documentation/FuturePlans/RoutingLabels/`
  - EndOfDayEmail.js (PO normalization, email formatting)
  - AppScript.js (history archival, department lookup)
  - MiniUPSLabel.csv (CSV structure reference)
- **Specification**: [spec.md](spec.md) - Complete feature specification
- **Implementation Plan**: [plan.md](../011-module-reimplementation/plan.md)
- **Database Schema**: [data-model.md](../011-module-reimplementation/data-model.md)

---

**Last Updated**: 2026-01-03  
**Analyst**: AI Development Assistant


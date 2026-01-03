# Research: Routing Module Google Sheets System

**Source**: Google Sheets routing label system (`Documentation/FuturePlans/RoutingLabels/`)  
**Analysis Date**: 2026-01-03  
**Purpose**: Document business logic extracted from Google Sheets for WinUI 3 implementation

## Source System Overview

The routing label system was implemented in Google Sheets with sophisticated automation for label generation, history tracking, and End-of-Day reporting. This research documents the business logic for WinUI 3 migration.

## Key Findings

### Google Sheets Structure

#### Sheets
- **"Today"**: Current label entries (active queue)
- **"History"**: Archived labels (moved from Today)
- **"Recipients"**: Lookup table for Deliver To dropdown
- **"PO Numbers"**: Tracking table with hyperlinks

### Business Logic Extracted

#### Label Entry
- **Fields**: Deliver To, Department, Package Description, PO Number, Work Order, Employee, Label Number
- **Auto-Fill**: Department auto-fills from recipient's default department
- **PO Formatting**: "63150" → "PO-063150" (auto-format)
- **Label Numbering**: Auto-increments per session (1, 2, 3, ...)

#### Duplicate Row Feature
- **Purpose**: Copy all fields to new row
- **Behavior**: Label number auto-increments, all other fields copied exactly
- **Use Case**: Multiple labels for same recipient/department

#### History Archival
- **Process**: Copy labels from "Today" → "History" with confirmation dialog
- **Sorting**: History sorted by date descending
- **Color Grouping**: Alternating row colors (#D3D3D3 / #FFFFFF) grouped by date
- **Clear After Archive**: Today's labels cleared after archival

#### Recipient Lookup
- **Source**: "Recipients" sheet with name and default department
- **Purpose**: Populate Deliver To dropdown and auto-fill department
- **Format**: Name | Default Department

#### CSV Export Format
- **Template**: "Expo - Mini UPS Label ver. 1.0" (LabelView 2022)
- **Columns**: Deliver To, Department, Description, PO Number, Work Order, Employee, Label Number, Date
- **Purpose**: Import into LabelView for physical label printing

### End-of-Day Reporting Integration

#### PO Normalization Algorithm
From `EndOfDayEmail.js`:
- **"63150"** → **"PO-063150"** (pad to 6 digits, add PO- prefix)
- **"063150B"** → **"PO-063150B"** (preserve suffix)
- **"Customer Supplied"** → **"Customer Supplied"** (pass through)
- **""** → **"No PO"** (empty string)

#### Date Grouping
- **Purpose**: Group labels by date for email formatting
- **Format**: Alternating row colors by date group
- **Colors**: #D3D3D3 (light gray) / #FFFFFF (white)

## Reimplementation Strategy

### Database Schema
- **routing_labels**: Current/today labels (`is_archived = 0`)
- **routing_labels_history**: Archived labels (`is_archived = 1`)
- **routing_recipients**: Recipient lookup with default departments

### Key Features to Implement
1. **Label Entry**: Form with all required fields
2. **Auto-Fill**: Department from recipient lookup
3. **Label Numbering**: Auto-increment per session
4. **Duplicate Row**: Copy fields with incremented label number
5. **CSV Export**: LabelView-compatible format
6. **History Archival**: Move labels to history with confirmation
7. **History View**: Date-grouped display with alternating colors

### Migration Notes

#### Preserved Functionality
- ✅ All label fields maintained
- ✅ Auto-fill behavior preserved
- ✅ Label numbering logic unchanged
- ✅ CSV export format unchanged (LabelView compatibility)
- ✅ History archival process maintained

#### Enhanced Functionality
- ➕ Database persistence (replaces Google Sheets)
- ➕ MVVM architecture (replaces AppScript automation)
- ➕ Better error handling
- ➕ Improved UI/UX (WinUI 3 desktop app)

#### Removed Functionality
- ❌ Google Sheets automation (replaced with MVVM)
- ❌ Remote sheet export (future enhancement)

## References

- **Business Logic**: `Documentation/FuturePlans/RoutingLabels/RoutingLabels-BusinessLogic.md`
- **Google Sheets Scripts**: `Documentation/FuturePlans/RoutingLabels/AppScript.js`, `EndOfDayEmail.js`
- **Specification**: [spec.md](../011-module-reimplementation/spec.md) - User Story 4
- **Implementation Plan**: [plan.md](../011-module-reimplementation/plan.md)
- **Database Schema**: [data-model.md](../011-module-reimplementation/data-model.md)

---

**Last Updated**: 2026-01-03  
**Analyst**: AI Development Assistant


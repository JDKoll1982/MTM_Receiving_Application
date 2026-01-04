# Research: Volvo Dunnage Requisition System

**Source**: Google Sheets system (`Documentation/FuturePlans/VolvoLabels/`)  
**Analysis Date**: 2026-01-03  
**Purpose**: Document business logic extracted from Google Sheets for WinUI 3 migration

## Source Files Analyzed

### 1. AppScript.js
**Purpose**: Main workflow automation for Volvo label generation

**Key Functions**:
- `onOpen()`: Creates MTM Menu with options (Clear Customer Packlist, Generate Labels, Export Customer Packlist to Remote, Clear Label Sheet)
- `clearVitsSheetRows()`: Clears "Vits" sheet (current labels) from row 2 onwards
- `exportCustomerPacklistToRemote()`: Exports Customer Packlist to remote shared Google Sheet
- `generateLabels()`: Generates labels from Customer Packlist data
- `clearCustomerPacklistRange()`: Clears Customer Packlist data

**Business Logic Extracted**:
1. **Label Generation**: Multiplies skid count × quantity per skid from DataSheet.csv
2. **Component Explosion**: Includes components (Skid, Lid, Frame) from DataSheet.csv
3. **CSV Format**: Matches LabelView 2022 requirements (Material ID, Quantity, Employee, Date, Time, Receiver, Notes)
4. **Date Grouping**: Labels grouped by date with alternating row colors (#D3D3D3 / #aee6ad)
5. **Shipment Numbering**: Auto-increments within same day (e.g., "04/01/2025 #1", "#2")

### 2. HistoryAppScript.js
**Purpose**: History archival workflow

**Key Functions**:
- `saveHistory()`: Copies labels from "Vits" (current) to "History" sheet
- `colorHistory()`: Applies alternating row colors grouped by date
- `clearVits()`: Clears current labels after archival

**Business Logic Extracted**:
1. **Archival Process**: Copy from "Vits" → "History" with confirmation dialog
2. **Date Sorting**: History sorted by date descending
3. **Color Alternation**: Alternating colors (#D3D3D3 / #aee6ad) by date group
4. **Clear After Archive**: Current labels cleared after archival

### 3. DataSheet.csv
**Purpose**: Master data catalog for Volvo parts

**Structure**:
```csv
Dunnage,Quantity,Included Skid,Quantity,Included Lid,Quantity,Included Frame,Quantity
V-EMB-1,10,,,,,,
V-EMB-116,150,V-EMB-1,1,V-EMB-71,1,V-EMB-26,3
V-EMB-2,20,,,,,,
V-EMB-500,88,V-EMB-2,1,V-EMB-92,1,,,
```

**Key Fields**:
- **Dunnage**: Part number (e.g., V-EMB-2, V-EMB-500)
- **Quantity**: Pieces per skid (e.g., V-EMB-2 = 20 pieces/skid)
- **Included Skid/Lid/Frame**: Component parts included with parent (e.g., V-EMB-500 includes V-EMB-2 and V-EMB-92)

**Business Logic Extracted**:
1. **Component Relationships**: Parent parts include components (usually quantity = 1 per skid)
2. **Quantity Calculation**: Main part quantity × skid count = total pieces
3. **Component Aggregation**: Components aggregated across all parts in shipment (e.g., V-EMB-2 appears in multiple parents)

### 4. CustomerPacklist.csv
**Purpose**: User-entered packlist data

**Structure**:
- Date, PO Number (initially blank), Receiver (initially blank)
- Employee number
- Location (default: VOLVO-VITS)
- Notes (default: Volvo CSD)
- Part entries with skid counts

**Business Logic Extracted**:
1. **PO Number**: Initially blank, filled after purchasing provides PO
2. **Receiver Number**: Initially blank, filled after Infor Visual receiving
3. **Shipment Numbering**: Auto-increments daily (resets to #1 each day)
4. **Multiple Shipments**: Same day can have multiple shipments (#1, #2, etc.)

### 5. History-PONumbers.csv
**Purpose**: Historical PO tracking

**Business Logic Extracted**:
1. **PO Tracking**: Historical record of PO numbers assigned to shipments
2. **Date Grouping**: Grouped by date with hyperlinks to detail sheets

### 6. History-SavedByDate.csv
**Purpose**: Historical shipment summaries

**Business Logic Extracted**:
1. **Date Aggregation**: Shipments grouped by date
2. **Part Counts**: Total unique parts per shipment
3. **Status Tracking**: Pending PO vs. Completed

## Key Business Rules Identified

### 1. Component Explosion Algorithm
```
For each part entered:
  - Main part: skid_count × quantity_per_skid = total_pieces
  - Components: skid_count × component_quantity = component_pieces
  - Aggregate components across all parts (e.g., V-EMB-2 from multiple parents)
```

**Example**:
- User enters: V-EMB-500 (3 skids)
- Calculation:
  - V-EMB-500: 3 × 88 = 264 pieces
  - V-EMB-2: 3 × 1 = 3 pieces (component)
  - V-EMB-92: 3 × 1 = 3 pieces (component)

### 2. Discrepancy Tracking
- **Packlist Quantity**: What Volvo's packlist says (user enters)
- **Received Quantity**: What was actually received (user enters)
- **Difference**: Calculated (received - packlist)
- **Note**: Optional user note explaining discrepancy

**Email Format**:
```
NOTE: Discrepancies found between Volvo packlist and actual received quantity.

Discrepancies:
Part Number | Expected | Received | Difference | Note
V-EMB-21    | 10       | 8       | -2        | Missing 2 skids
```

### 3. Label Generation Rules
- **One label per skid** (not per piece)
- **Quantity from master data** (DataSheet.csv lookup)
- **Included components** automatically calculated
- **CSV Format**: Material ID, Quantity (pieces), Employee, Date, Time, Receiver (blank initially), Notes
- **LabelView Logic**: Parts starting with "V-EMB-" hide PO field on label

### 4. Email Formatting Rules
- **Greeting**: Configurable in user settings (default: "Need a PO cut for the following:")
- **Discrepancy Notice**: Only shown if discrepancies exist
- **Requested Lines Table**: Part Number | Quantity (aggregated, one line per part)
- **Plain Text Format**: Ready for copy/paste into Outlook
- **Signature**: From user settings (not hardcoded)

### 5. Shipment Lifecycle
```
1. NEW ENTRY
   ↓
2. USER ENTERS: Date, Parts, Skid Counts, Discrepancies (optional)
   ↓
3. SYSTEM CALCULATES: Component Explosion (Requested Lines)
   ↓
4. USER GENERATES LABELS (CSV file created)
   Status: "Pending PO"
   ↓
5. USER COPIES EMAIL → Sends to Purchasing
   Status: Still "Pending PO"
   ↓
6. PURCHASING CREATES PO IN THEIR SYSTEM
   ↓
7. USER RECEIVES INTO INFOR VISUAL
   (Purchasing gives PO number, Infor generates Receiver#)
   ↓
8. USER ENTERS: PO Number & Receiver Number into Volvo module
   Status: "Complete"
   ↓
9. ARCHIVED TO HISTORY
   CSV file cleared (not deleted)
```

### 6. Master Data Management
- **Import from CSV**: DataSheet.csv is authoritative source
- **Historical Integrity**: Changes to master data do NOT affect existing shipments
- **Deactivation**: Parts deactivated (not deleted) to preserve history
- **Component Updates**: Editing part updates components (delete + insert)

### 7. Reporting Integration
- **Module Selection**: Checkbox selection (not dropdown)
- **Availability Check**: Disable checkbox if no data for date range
- **Two Sections**: Pending PO and Completed shipments
- **Complete Action**: Can complete pending shipments directly from report

## Migration Decisions

### What We're Keeping
- ✅ Component explosion algorithm (exact match)
- ✅ Discrepancy tracking (4-column format)
- ✅ Label CSV format (LabelView compatibility)
- ✅ Email formatting (matches Google Sheets output)
- ✅ Shipment numbering (daily reset, auto-increment)
- ✅ Master data structure (DataSheet.csv → MySQL tables)

### What We're Changing
- ❌ Google Sheets → WinUI 3 desktop application
- ❌ Manual CSV import → Database-driven with stored procedures
- ❌ Remote sheet export → Future enhancement (optional)
- ❌ Manual menu triggers → Automated workflow with ViewModels

### What We're Adding
- ➕ Database persistence (MySQL)
- ➕ MVVM architecture (ViewModels, Views, Services)
- ➕ Error handling (IService_ErrorHandler)
- ➕ Logging (ILoggingService)
- ➕ Settings integration (Volvo settings tab)
- ➕ Reporting integration (shared End-of-Day reports)

## Data Migration Strategy

### Initial Data Load
1. **Import DataSheet.csv** → `volvo_parts_master` table
2. **Parse component relationships** → `volvo_part_components` table
3. **Validate data integrity** (no orphaned components, valid quantities)

### Historical Data (Optional)
- Google Sheets history can be exported to CSV
- Import script can load historical shipments if needed
- Not required for MVP (fresh start acceptable)

## Open Questions Resolved

### ✅ Resolved
- **Q**: When are labels generated?  
  **A**: Before sending email to purchasing (no PO exists yet)

- **Q**: What goes on Volvo labels?  
  **A**: All standard fields, but LabelView hides PO field for V-EMB- parts

- **Q**: How are components aggregated?  
  **A**: One line per part number, sum quantities across all parents

- **Q**: What happens if master data changes?  
  **A**: Historical shipments unaffected (calculated_piece_count stored at creation)

- **Q**: Can multiple pending shipments exist?  
  **A**: No, only one pending PO at a time (enforced by application logic)

## References

- **Source Files**: `Documentation/FuturePlans/VolvoLabels/`
  - AppScript.js (main workflow)
  - HistoryAppScript.js (archival)
  - DataSheet.csv (master data)
  - CustomerPacklist.csv (user input format)
  - History-PONumbers.csv (PO tracking)
  - History-SavedByDate.csv (summaries)

- **Related Documentation**:
  - [spec.md](spec.md) - Complete feature specification
  - [data-model.md](data-model.md) - Database schema design
  - [workflows/](workflows/) - PlantUML workflow diagrams

---

**Last Updated**: 2026-01-03  
**Analyst**: AI Development Assistant


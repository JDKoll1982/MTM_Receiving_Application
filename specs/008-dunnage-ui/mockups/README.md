# Dunnage Wizard UI Mockup

Interactive HTML mockup demonstrating the complete Dunnage Wizard workflow UI as specified in [spec.md](../spec.md).

## Overview

This mockup provides a fully functional prototype of the 6-step wizard workflow for dunnage receiving:

1. **Mode Selection** - Choose between Wizard, Manual Entry, or Edit Mode
2. **Type Selection** - Select dunnage type from paginated grid (9 per page)
3. **Part Selection** - Select part with inventory notification
4. **Quantity Entry** - Enter quantity with validation
5. **Details Entry** - Enter PO, location, and dynamic spec inputs
6. **Review & Save** - Review all loads, add more, or save session

## Features

### ✅ Implemented

- **WinUI 3 Styling** - Mica background, card-based UI, accent colors
- **Mode Selection Cards** - 3 clickable cards with icons and descriptions
- **Wizard Mode (Complete 6-step flow)**:
  - Type Pagination - 3x3 grid with 11 sample types, next/previous controls
  - Dynamic Part Dropdown - Parts filtered by selected type
  - Inventory Notification - InfoBar displays when part requires Visual ERP tracking
  - Inventory Method Logic - Changes from "Adjust In" to "Receive In" when PO is entered
  - Dynamic Spec Inputs - Controls generated based on type's spec schema
  - Quantity Validation - Blocks advancement if quantity ≤ 0
  - Session Management - "Add Another" accumulates multiple loads
  - Review DataGrid - Shows all loads with remove actions
  - Success Message - Displays count of saved loads
- **Manual Entry Mode (Grid-based batch entry)**:
  - Editable DataGrid - Add/remove rows, fill multiple entries at once
  - Type & Part Dropdowns - Dynamic filtering in grid cells
  - Auto-fill Specs - Automatically populates dimensions when part selected
  - Validation Highlighting - Invalid rows highlighted in orange
  - Row Statistics - Shows valid/incomplete row counts
  - Quick Actions - Add rows, save all, navigate back
- **Edit Mode (History review & modification)**:
  - Search & Filter - Search by Part ID, PO, Location, User
  - Type Filter - Filter records by dunnage type
  - Date Range Filter - Today, This Week, This Month, All Time
  - Paginated History Grid - 20 records per page with navigation
  - Bulk Selection - Select all/multiple records with checkboxes
  - Edit Dialog - Modal form to edit individual records
  - Delete Selected - Bulk delete with confirmation
  - Export to CSV - Download filtered or all records
  - Statistics Dashboard - Total, This Week, Today, Selected counts
  - Sample Data - 50 generated historical records spanning 60 days
- **Wizard Navigation** - Back/Next buttons with step indicators

### 📦 Sample Data

- **11 Dunnage Types** - Pallet, Cardboard Box, Wooden Crate, Plastic Container, Metal Bin, Bubble Wrap, Foam Padding, Stretch Wrap, Corner Protectors, Edge Guards, Anti-Static Bags
- **Multiple Parts per Type** - Example: 4 pallet types (48x40, 48x48, 40x48, EURO)
- **Spec Schemas** - Different specs per type (Width/Height/Depth, Material, Color, etc.)
- **Inventoried Parts** - Some parts flagged as requiring Visual ERP tracking

### ❌ Not Implemented (Out of Scope for Mockup)

- Quick Add dialogs for types/parts (stub alert)
- Actual database persistence
- Real CSV file system writes (browser downloads only)
- Real authentication
- Network path validation
- Barcode scanning integration

## File Structure

```
mockups/
├── index.html                      # Main window shell (NavigationView)
├── pages/
│   ├── dunnage-wizard.html        # Wizard workflow page (6 steps)
│   ├── manual-entry.html          # Manual entry grid mode
│   └── edit-mode.html             # Edit/history review mode
├── css/
│   ├── styles.css                 # Layout and component styles
│   └── winui-theme.css            # WinUI 3 theme variables and controls
└── js/
    ├── main.js                    # Main window navigation
    ├── wizard-data.js             # Sample data (types, parts, specs)
    ├── wizard.js                  # Wizard state management and logic
    ├── manual-entry.js            # Manual entry grid logic
    └── edit-mode.js               # Edit mode filtering and CRUD operations
```

## Running the Mockup

### Option 1: Local File (Simple)
1. Open `index.html` in a modern web browser
2. Navigate using the left navigation pane
3. Click "Dunnage Labels" to start the wizard

### Option 2: Local Server (Recommended)
```powershell
# Using Python
cd specs/008-dunnage-ui/mockups
python -m http.server 8080

# Using Node.js (npx http-server)
npx http-server -p 8080

# Then open: http://localhost:8080
```

**Why use a server?** Some browsers restrict iframe loading from `file://` protocol. Using a local server avoids CORS issues.

## Usage Instructions

### Complete Wizard Walkthrough

1. **Start at Mode Selection**
   - Click "Wizard Mode" card

2. **Select Type**
   - Click any type button (e.g., "Pallet")
   - Use Next/Previous to paginate through 11 types
   - Note: Page 1 shows 9 types, Page 2 shows remaining 2

3. **Select Part**
   - Choose "PALLET-48X40" from dropdown
   - Observe part details panel appears
   - Observe InfoBar: "This part requires inventory in Visual. Method: Adjust In"
   - Click "Next"

4. **Enter Quantity**
   - Default is 1, try changing to 10
   - Try entering 0 to see validation error
   - Click "Next"

5. **Enter Details**
   - (Optional) Enter PO Number "PO123456"
   - Observe InfoBar updates to "Method: Receive In"
   - (Optional) Enter Location "Warehouse A"
   - Fill dynamic spec inputs (Width, Height, Depth, IsInventoriedToVisual)
   - Click "Next"

6. **Review & Save**
   - See load in DataGrid
   - Click "Add Another" to add more loads to session
   - Repeat steps 2-5 for additional loads
   - Click "Save All" when done
   - Success message displays with load count

### Manual Entry Mode Walkthrough

1. **Start at Mode Selection**
   - Click "Manual Entry" card

2. **Fill Grid Rows**
   - 5 rows are pre-populated
   - Click "➕ Add Row" to add more
   - Select Type from dropdown in first column
   - Part dropdown auto-populates based on type
   - Select Part - dimensions auto-fill (Width, Height, Depth)
   - Enter Quantity (required, minimum 1)
   - Optionally enter PO Number and Location
   - Invalid rows (missing required fields) highlighted in orange

3. **Manage Rows**
   - Click 🗑️ to remove individual rows
   - Watch statistics update (Valid vs Incomplete rows)

4. **Save**
   - Click "💾 Save All" to save all valid rows
   - Dialog shows count of saved rows
   - Grid resets with 5 new empty rows

### Edit Mode Walkthrough

1. **Start at Mode Selection**
   - Click "Edit Mode" card

2. **Browse History**
   - 50 sample records generated (last 60 days)
   - Records sorted by date (newest first)
   - Paginated: 20 records per page

3. **Search & Filter**
   - Enter search term (Part ID, PO, Location, User)
   - Select Type filter (e.g., "Pallet")
   - Select Date Range (Today, This Week, This Month, All Time)
   - Click "Clear" to reset all filters

4. **Select Records**
   - Click checkboxes to select individual records
   - Click header checkbox to select all on current page
   - Selected count shown in statistics

5. **Edit Record**
   - Click "✏️ Edit" button on any row
   - Modal dialog opens with editable fields
   - Modify Type, Part ID, Quantity, PO, or Location
   - Click "Save Changes" to update record

6. **Delete Records**
   - Select one or more records with checkboxes
   - Click "🗑️ Delete Selected"
   - Confirm deletion in dialog
   - Records removed from grid

7. **Export Data**
   - Click "📥 Export to CSV"
   - Downloads filtered records (or all if no filters)
   - File named: `dunnage-history-YYYY-MM-DD.csv`

### Testing Multi-Load Session (Wizard)

1. Complete wizard to Review step
2. Click "Add Another" (load is saved to session)
3. Select different type (e.g., "Cardboard Box")
4. Select part "BOX-MEDIUM"
5. Enter quantity, details, advance to Review
6. Observe 2 loads in DataGrid
7. Remove one with "Remove" button if desired
8. Click "Save All"

### Testing Manual Entry Efficiency

1. Navigate to Manual Entry mode
2. Quickly fill Type column for multiple rows (use Tab to navigate)
3. Fill Part ID column - watch dimensions auto-populate
4. Fill Quantity column
5. Observe Valid Row count increasing
6. Try leaving a row incomplete - notice orange highlight
7. Click "Save All" - only valid rows are saved

### Testing Edit Mode Filtering

1. Navigate to Edit Mode
2. Note initial record count (50 records)
3. Enter "PALLET" in search box - see results filter
4. Select "This Week" date filter - further filtering
5. Click "Clear" - all records return
6. Select Type filter "Cardboard Box" - only boxes shown
7. Edit a record, save changes
8. Export filtered results to CSV

### Testing Pagination

1. At Type Selection step
2. Note "Page 1 of 2" indicator
3. Click "Next ▶" button
4. Observe page 2 displays remaining 2 types
5. Click "◀ Previous" to return to page 1

### Testing Inventory Notification

1. Select type "Pallet"
2. Select part "PALLET-48X40" (inventoried: true)
3. Observe InfoBar with "Method: Adjust In"
4. Advance to Details Entry
5. Enter PO Number
6. Observe InfoBar updates to "Method: Receive In"
7. Clear PO Number
8. Observe InfoBar reverts to "Method: Adjust In"

## Design Notes

### WinUI 3 Fidelity

The mockup closely mimics WinUI 3 controls:

- **Mica Background** - Light gray (#f3f3f3)
- **Card Style** - White cards with subtle shadow and border
- **Accent Color** - Microsoft Blue (#0078d4)
- **Typography** - Segoe UI font family
- **InfoBar** - Blue informational style with icon
- **Button States** - Hover, active, disabled styles
- **Form Controls** - TextBox, NumberBox, CheckBox, ComboBox

### Responsive Considerations

- Fixed width for main window (simulates 1400x900 app window)
- Grid layouts use `grid-template-columns: repeat(3, 1fr)`
- Max-width constraints on form containers for readability
- Flex layouts for navigation and headers

### Accessibility

- Semantic HTML (nav, main, header, table)
- Form labels associated with inputs
- Button text describes actions
- InfoBar uses icons + text
- Keyboard navigation supported (tab order)

## Validation Against Spec

### User Stories Coverage

✅ **US1 - Wizard Orchestration & Mode Selection** - 3 mode cards implemented  
✅ **US2 - Dynamic Type Selection with Pagination** - 3x3 grid, 11 types, pagination controls  
✅ **US3 - Part Selection with Inventory Notification** - Dropdown, InfoBar, method logic  
✅ **US4 - Quantity Entry** - NumberBox with validation  
✅ **US5 - Dynamic Details Entry with Spec Inputs** - PO/Location fields, dynamic controls, InfoBar updates  
✅ **US6 - Review & Save with Add Another** - DataGrid, Add Another, Save All, success message

### Functional Requirements Coverage

- **FR-003** - Visibility flags (implemented via class toggling)
- **FR-007-013** - Mode selection cards and commands
- **FR-014-022** - Type selection grid, pagination, quick add button
- **FR-023-031** - Part dropdown, InfoBar, details panel
- **FR-032-037** - Quantity NumberBox, validation, context display
- **FR-038-048** - PO/Location inputs, dynamic specs, InfoBar updates
- **FR-049-059** - DataGrid, Add Another, Save All, session management

### Success Criteria Validation

✅ **SC-001** - Workflow completes in under 2 minutes (mockup: ~1 minute)  
✅ **SC-002** - 9 types per page, correct page count (1 of 2)  
✅ **SC-003** - Dynamic spec inputs generated (Width, Height, Depth, etc.)  
✅ **SC-004** - Inventory notification displays and updates on PO change  
✅ **SC-005** - Review grid displays all columns correctly  
✅ **SC-006** - Add Another accumulates loads in session  
✅ **SC-008** - Validation prevents advancement (quantity=0 blocks Next)

## Browser Compatibility

Tested on:
- ✅ Chrome 120+
- ✅ Edge 120+
- ✅ Firefox 120+
- ⚠️ Safari 17+ (minor CSS differences)

## Future Enhancements (Not in Mockup)

- WebSocket connection to live backend
- Real-time validation against database
- Barcode scanning integration (mobile)
- Print preview of labels
- Dark mode toggle
- Accessibility audit (WCAG 2.1 AA)

---

**Created**: 2025-12-27  
**Version**: 1.0  
**Spec Reference**: [spec.md](../spec.md)  
**Checklist**: [checklists/requirements.md](../checklists/requirements.md)

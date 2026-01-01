# Help Content Inventory

## Dunnage Workflow Help Content

### ModeSelection
- **Current**: User chooses between Guided Wizard, Manual Entry, or Edit Mode
- **Tooltips**: "Skip mode selection and go directly to [Mode]" for each quick-access button
- **Need**: Explanation of when to use each mode

### TypeSelection  
- **Current**: Grid of dunnage types with icons, pagination controls
- **Tooltips**: First/Previous/Next/Last Page navigation
- **Need**: Explanation of dunnage types, how to add new types

### PartSelection
- **Current Tip**: None
- **InfoBar**: Inventory warning when part not in inventory list
- **Placeholders**: "Choose a part..."
- **Tooltips**: "Refresh parts list"
- **Need**: Explanation of part selection, how to add new parts, inventory vs non-inventory

### QuantityEntry
- **Current Tip**: "You can adjust the quantity later if needed. The system will generate individual labels for each item."
- **Placeholder**: "Enter quantity..."
- **Need**: Explain quantity limits, label generation process

### DetailsEntry
- **InfoBar**: Location requirement warning
- **Placeholders**: "Enter PO number (optional)...", "Enter location..."
- **Need**: Explain PO number format, location codes, when fields are required

### Review
- **InfoBar**: Success message after save
- **Current**: Single view and table view toggle
- **Need**: Explain review process, what happens after save, how to edit before saving

### ManualEntry
- **Tooltips**: 
  - "Add multiple rows at once (up to 100)"
  - "Auto-fill from last entry for this Part ID"
  - "Copy PO, Location, and specs from last row to empty fields"
  - "Sort by Part ID → PO → Type"
- **Need**: Detailed explanation of each bulk operation

### EditMode
- **Tooltips**: 
  - "Load unsaved loads from current session"
  - "Load from most recent CSV export"
  - "Load historical loads from database"
  - Date range quick filters (7 days, today, this week, month, quarter, year)
  - Pagination tooltips
- **Need**: Explain each data source, date filtering, editing capabilities

## Receiving Workflow Help Content

### ModeSelection
- **Tooltips**: Quick-access tooltips for each mode
- **Need**: When to use Guided vs Manual vs Edit

### POEntry
- **Placeholders**: "Enter PO (e.g., 66868 or PO-066868)", "Enter Part ID (e.g., MMC-001, MMF-456)"
- **Need**: PO number format rules, part ID format, Infor Visual integration explanation

### WeightQuantityEntry
- **InfoBar**: Weight vs quantity selection warning
- **Placeholder**: "Enter whole number"
- **Need**: When to use weight vs quantity, unit of measure explanation

### HeatLotEntry
- **Placeholder**: "Enter heat/lot number or leave blank"
- **Need**: What is heat/lot, when it's required, format requirements

### ManualEntry
- **Tooltips**: Auto-fill, bulk operations
- **Need**: Same as dunnage manual entry

### EditMode
- **Tooltips**: Same pagination/filtering as dunnage
- **Need**: Historical data editing explanation

### Review
- **Need**: Final review explanation, CSV export process, label printing

## Admin Views

### AdminTypesView
- **Tooltips**: 
  - "Create a new dunnage type (coming soon)"
  - "Edit selected type"
  - "Delete selected type with impact analysis"
  - "Return to admin main navigation"
- **Need**: Type management explanation, impact of deleting types

### AdminPartsView
- **Tooltips**: CRUD operation tooltips
- **Placeholders**: Search and filter fields
- **Need**: Part management explanation, search capabilities

### AdminInventoryView
- **Need**: Inventory management explanation

## Quick Add Dialogs

### Dunnage_QuickAddTypeDialog
- **Tooltips**: Extensive tooltips for every field
  - "Click for help about dunnage types"
  - "Enter a unique descriptive name for this dunnage type"
  - "Click to browse and select an icon from the icon library"
  - Spec field tooltips for name, type, min/max, units, required
- **Tips**: Tip about dunnage types in help section
- **Need**: Comprehensive type creation guide

### Dunnage_QuickAddPartDialog
- **Placeholders**: Dimension fields, notes field
- **Need**: Part creation guide, dimension requirements

## Workflow Views

### Dunnage_WorkflowView
- **InfoBar**: Global workflow status messages
- **Tooltips**: 
  - "Return to mode selection (clears current work)" (multiple instances for different steps)
  - "Click for help about the current step"
- **Need**: Context-sensitive help for each workflow step

### Receiving_WorkflowView
- **InfoBar**: Global workflow status
- **Tooltips**: "Click for help about current step", "Return to mode selection"
- **Need**: Context-sensitive help for each workflow step

## Missing Help Categories

1. **Icon Selection**: No help for icon picker control
2. **Spec Types**: No explanation of Text vs Number spec types
3. **Validation Messages**: No central repository of validation messages
4. **Error Messages**: Error dialogs exist but no help explaining common errors
5. **Keyboard Shortcuts**: No documentation of keyboard shortcuts
6. **Data Import/Export**: No help for CSV operations
7. **Infor Visual Integration**: No user-facing explanation of ERP integration
8. **Label Printing**: No help for label generation process
9. **Session Management**: No help for session timeout, auto-save
10. **User Preferences**: No help for preferences and settings

# UI Mockups - Module_Receiving

## Overview

Complete set of SVG mockups for Module_Receiving user interface, showing all major views and dialogs with MainWindow context.

## Mockup Files Created (12 total)

### Core Workflow Views - Wizard Mode (5 mockups)

1. **01_Hub_MainWorkflow.svg** - Receiving Hub with Mode Selection
   - MainWindow frame with navigation pane
   - Three workflow mode buttons (Wizard, Manual, Edit)
   - Non-PO toggle switch
   - Help button

2. **02_Wizard_Step1_Container.svg** - Wizard Step 1: Order & Part Selection
   - Step indicator showing Step 1 active
   - PO Number entry with validation
   - Part selection ComboBox with details
   - Load count NumberBox
   - Navigation buttons (Cancel, Previous disabled, Next enabled)

3. **03_Wizard_Step2_LoadDetailsGrid.svg** - Wizard Step 2: Load Details Entry
   - Step indicator showing Step 2 active
   - Bulk operation toolbar
   - Editable DataGrid with 5 loads
   - Auto-filled rows highlighted (yellow background)
   - Progress indicator (1 of 5 complete, 20%)
   - Navigation buttons (Next disabled until complete)

4. **04_Wizard_Step3_ReviewSummary.svg** - Wizard Step 3: Review & Save
   - Step indicator showing Step 3 active
   - Summary header with PO, Part, Load count
   - Read-only review DataGrid
   - Totals panel (Total Weight, Packages, Avg Wt/Pkg)
   - Prominent "Save Transaction" button
   - Navigation buttons

5. **05_Wizard_CompletionScreen.svg** - Transaction Saved Successfully
   - Large green success checkmark icon
   - Transaction ID and timestamp
   - CSV file paths (local + network) with Copy buttons
   - Action buttons (New Transaction, View History, Exit)

### Manual Entry Mode (1 mockup)

9. **09_Manual_Entry_Mode.svg** - Direct Data Entry Interface
   - Quick entry form with all fields in one view
   - Real-time validation
   - Current transaction DataGrid with Edit/Delete actions
   - Transaction summary panel with totals
   - Save Transaction button

### Edit Mode Views (3 mockups)

10. **10_Edit_Mode_TransactionHistory.svg** - Transaction History List
    - Search and filter bar (date range, status)
    - Transaction history DataGrid with pagination
    - Status badges (Completed, Draft, Error)
    - View details action buttons
    - Export to Excel button

11. **11_Edit_Mode_EditTransaction.svg** - Edit Existing Transaction
    - Transaction info header (read-only)
    - Edit mode notice (versioning info)
    - Editable DataGrid with change indicators
    - Add Load button
    - Modified/New/Deleted row highlighting
    - Change summary panel
    - Save as New Version / Discard Changes buttons
    - Audit trail link

12. **12_Dialog_VersionHistory.svg** - Version History Timeline
    - Parent view: Edit Transaction (dimmed)
    - Modal timeline view
    - Version cards showing changes
    - Current/Original version indicators
    - View details for each version
    - Export history button

### Dialog Mockups (3 mockups)

6. **06_Dialog_BulkCopyPreview.svg** - Bulk Copy Fields Preview
   - Parent view: Step 2 DataGrid (dimmed)
   - Modal overlay
   - Source load details
   - Affected loads count (4 loads: 2, 3, 4, 5)
   - Execute Copy / Cancel buttons

7. **07_Dialog_CancelWorkflowConfirmation.svg** - Cancel Workflow Warning
   - Parent view: Wizard Step 1 (dimmed)
   - Modal overlay
   - Warning icon
   - Unsaved data warning
   - Keep Working / Cancel Workflow buttons

8. **08_Dialog_SaveError.svg** - Save Transaction Error
   - Parent view: Step 3 Review (dimmed)
   - Modal overlay
   - Error icon
   - Error message and technical details
   - Log file path
   - Copy Error / Close / Retry buttons

## Design System

### WinUI 3 Implementation Notes
All mockups use **only achievable WinUI 3 effects and controls:**
- **Cards:** `Border` with `CornerRadius="8"` and `Background="#FFFFFF"`
- **Shadows:** `ThemeShadow` with elevation (represented via SVG filters)
- **Rounded Corners:** `CornerRadius` on all cards and buttons
- **Standard Controls:** TextBox, ComboBox, Button, DataGrid, ProgressBar, etc.
- **No custom rendering required** - all effects are standard WinUI 3

### Colors
- **Primary Blue:** `#0078D4` - Buttons, active states
- **Success Green:** `#107C10` - Validation success, completed steps
- **Warning Yellow:** `#FFB900` - Warnings, info messages
- **Error Red:** `#D13438` - Errors, destructive actions
- **Background:** `#F3F3F3`, `#FAFAFA`
- **Text:** `#202020` (primary), `#666666` (secondary)
- **Borders:** `#D1D1D1`

### Typography
- **Font Family:** Segoe UI (body text), Segoe MDL2 Assets (icons), Segoe UI Mono (code/IDs)
- **Title:** 28-32px, Bold
- **Subtitle:** 20-24px, Semi-Bold
- **Body:** 14-16px, Regular
- **Caption:** 12-13px, Regular

### Icons (Segoe MDL2 Assets)
- Menu: `&#xE700;`
- Checkmark: `&#xE73E;`
- ChevronDown: `&#xE70D;`
- ChevronUp: `&#xE70E;`
- Close: `&#xE711;`
- Warning: `&#xE7BA;`
- Page/Document: `&#xE74C;`
- Help: `&#xE897;`

### Layout Standards
- **MainWindow:** 1400x900px
- **Title Bar:** 48px height
- **Navigation Pane:** 280px width
- **Content Area:** 1120px width
- **Padding:** 20px standard, 40px for centered content
- **Border Radius:** 4px (controls), 8px (cards/dialogs)

### WinUI 3 Controls Represented
- **TextBox** - PO Number, Part ID entry
- **ComboBox** - Part selection, Package Type, Location
- **NumberBox** - Load Count
- **CheckBox** - Non-PO mode
- **ToggleSwitch** - Non-PO toggle
- **Button** - Navigation, actions
- **DataGrid** - Load details, review summary
- **ProgressBar** - Completion percentage
- **InfoBar** - Status messages
- **ContentDialog** - Modal dialogs
- **NavigationView** - Main app navigation

## States Demonstrated

### Step Indicator States
- **Active:** Blue fill, white number, bold text
- **Complete:** Green fill, white checkmark
- **Inactive:** White fill, gray border, gray text
- **Connector:** Colored line between steps

### DataGrid Row States
- **Normal:** White background
- **Alternating:** `#F9F9F9` background
- **Auto-filled:** `#FFF9E6` background (yellow tint)
- **Focused Cell:** Blue border (`#0078D4`)
- **Read-only:** Grayed text, no edit indicators

### Button States
- **Primary (Enabled):** Blue fill, white text
- **Secondary (Enabled):** White fill, black text, gray border
- **Disabled:** Gray fill, lighter gray text
- **Danger:** Red fill (`#D13438`), white text

### Dialog States
- **Modal Overlay:** Black fill, 40% opacity
- **Parent Content:** 40-60% opacity (dimmed)
- **Dialog:** White, elevated with shadow
- **Warning:** Yellow background (`#FFF4CE`)
- **Error:** Red background (`#FDE7E9`)

## File Naming Convention

Format: `{NN}_{Type}_{Name}.svg`

- **NN:** Sequential number (01-08)
- **Type:** Hub, Wizard, Dialog
- **Name:** Descriptive (e.g., Step1_Container, BulkCopyPreview)

## Usage

These mockups serve as:
1. **Visual reference** for XAML implementation
2. **Design validation** before coding
3. **User testing** materials
4. **Documentation** for stakeholders
5. **Development guide** for exact layouts and spacing

## Next Mockups Needed

Based on tasks_phase5.md and tasks_phase6.md:
- Settings UI (User Preferences, Reference Data Manager)
- Additional dialogs (Session Recovery, Validation Errors, Help)
- Shared controls (Loading indicators, Empty states, Error panels)
- Print preview for labels

## Notes

- All SVG files are XML-compliant (no emojis, escaped `&` characters)
- Viewable in any modern web browser
- Scalable vector format (no pixelation)
- Can be imported into design tools (Figma, Sketch)
- Match WinUI 3 Fluent Design System
- Ready for developer handoff

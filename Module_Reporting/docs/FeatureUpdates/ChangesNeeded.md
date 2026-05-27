# Changes Needed

## Goal

Update `Module_Reporting` so the current report preview dialog flow becomes a full page workflow,
the preview/options experience matches the newer in-app card patterns, module-specific column
options are pruned and defaulted intentionally, and grouped-row display modes validate mixed
values before emitting range or multi-value placeholder text.

## Current Repo Findings

- The reporting entry surface is currently:
  - `Module_Reporting/Views/View_Reporting_Main.xaml`
  - `Module_Reporting/Views/View_Reporting_Main.xaml.cs`
- Clicking `Generate report` currently raises `PreviewRequested` from:
  - `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- The page code-behind currently opens a `ContentDialog` preview host:
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml.cs`
- The same reporting viewmodel currently owns:
  - selected quick range and explicit date range
  - selected modules
  - preview module cards
  - row display mode
  - options overlay state
- Preview shaping and grouped-row logic currently live in:
  - `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs`
- Module-specific default detail-column choices currently live in:
  - `ViewModel_Reporting_Main.GetPreferredPreviewColumnKeys(...)`
  - `ViewModel_Reporting_Main.CreateDetailColumnOptions(...)`
- Preview formatting and copy/export behavior currently depend on the preview module card output:
  - `Module_Reporting/Services/Service_Reporting.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Reporting/Services/Service_ReportingFormattingTests.cs`

## Requested Change Areas

### 1. Replace The Report Preview Dialog With A Real Page

- Clicking `Generate report` should navigate to a full page instead of opening a `ContentDialog`.
- The reporting selection inputs must remain intact when the user returns:
  - quick range dropdown
  - explicit date range values
  - selected module checkboxes
- The preview page must not render its own header because the main application window already
  owns page headers.

### 2. Narrow Summary Table Scope

- Only Receiving should keep a summary table.
- Dunnage and Volvo should not render summary tables on the updated preview page.

### 3. Rewrite The Options Experience

- The current options dialog/overlay should be reworked into the updated page flow.
- The row display mode controls should use collapsible settings cards in the same visual spirit as
  the Customer Pull n' Pack filter card.
- The row display mode card must be collapsed by default.
- Row display mode selection must change from radio buttons to a dropdown.
- A contextual explanation region must appear below the dropdown and explain the currently chosen mode.

### 4. Prune And Re-default Module Column Options

The options surface needs a new module-by-module column policy.

#### Receiving — remove these checkboxes entirely

- `PO / Line`
- `PO Line #`
- `Part / Dunnage`
- `Part Description`
- `Raw Quantity`
- `Weight Lbs`
- `Created At`
- `Transaction Date`
- `Created By`
- `User ID`
- `Vendor`
- `Load #`
- `Label #`
- `Units Per Skid`
- `Packages/Load`
- `Package Type`
- `Weight/Package`
- `PO Status`
- `PO Due Date`
- `Qty Ordered`
- `UOM`
- `Remaining Qty`
- `Non-PO`
- `Quality Hold Required`
- `Quality Hold Ack`
- `Part Skid Total`
- `Source Module`
- `ID`

All remaining Receiving checkboxes should default to on.

#### Dunnage — remove these checkboxes entirely

- `Part / Dunnage`
- `Raw Quantity`
- `Created At`
- `Created By`
- `Non-PO`
- `Quality Hold Required`
- `Quality Hold Ack`
- `Source Module`
- `ID`

All remaining Dunnage checkboxes should default to on.

#### Volvo — remove these checkboxes entirely

- `PO / Line`
- `Part / Dunnage`
- `Raw Quantity`
- `Created At`
- `Shipment #`
- `Units Per Skid`
- `Non-PO`
- `Quality Hold Required`
- `Quality Hold Ack`
- `Qty/Skid`
- `Source Module`
- `ID`

All remaining Volvo checkboxes should default to on.

### 5. Validate Mixed Values Before Grouped Cells Collapse

When row display modes combine multiple rows into one grouped row by unique part number or other
configured grouping shape, columns with differing source values must not silently collapse to blank
or to a single representative value without validation.

Required grouped-row behavior across Receiving, Dunnage, and Volvo:

- If all grouped values for a column are identical, use that single value.
- If grouped date values differ, show a range where a range is meaningful.
- If grouped non-date values differ and no sensible range exists, show explicit filler text that
  explains the mixed-value condition, for example `Multiple Receivers`.
- Do not assume a column is mixed or empty without checking all grouped rows first.

## Recommended Slice Separation

To match the Customer Pull n' Pack style and keep implementation bounded, split the work into:

1. preview-page navigation and state retention
2. preview-page layout and Receiving-only summary scope
3. options redesign with collapsible cards and row-display dropdown/explanation
4. per-module column option pruning and default-on rules
5. grouped-row validation and multi-value cell rules

## Acceptance Summary

- Report generation navigates to a real page instead of a preview dialog.
- Going back preserves the selection state from the reporting entry page.
- The preview page has no duplicate page header.
- Only Receiving shows a summary table.
- Options use collapsible settings cards with a collapsed-by-default row-display card.
- Row display mode uses a dropdown plus explanation text.
- Module column options match the requested removal lists and default-on behavior.
- Grouped rows validate mixed values before using ranges or multi-value placeholder text for all three module tabs.
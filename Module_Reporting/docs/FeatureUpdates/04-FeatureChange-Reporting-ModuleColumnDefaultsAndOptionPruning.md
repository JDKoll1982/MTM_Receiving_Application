# 04-FeatureChange-Reporting-ModuleColumnDefaultsAndOptionPruning

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Reporting module column-option pruning and default-on policy

## Implementation Order

04

## Why This Runs Fourth

Once the options shell is redesigned, the module-by-module column policy can be applied cleanly
without mixing visual-container changes with the actual reporting option rules.

## Confirmed Decisions

- The specified columns must be removed entirely from the per-module checkbox options.
- All remaining module-specific checkbox options should default to on.
- The requested removal lists differ between Receiving, Dunnage, and Volvo.

## Current Repo State

- Current candidate detail columns are built in one central method:
  - `ViewModel_Reporting_Main.CreateDetailColumnOptions(...)`
- Current module-specific default-on sets are defined in:
  - `ViewModel_Reporting_Main.GetPreferredPreviewColumnKeys(...)`
- Column availability is currently driven by `SectionHasDataForColumn(...)` plus preferred-key inclusion.
- The options UI currently binds module column checkboxes through:
  - `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
  - `Module_Reporting/Models/Model_ReportingPreviewColumnOption.cs`

## Required Outcome

Apply the requested per-module column policy so the removed columns no longer appear as checkboxes
and every remaining visible module column option defaults to included.

## Primary Change Areas

- Central detail-column candidate creation.
- Module-specific option filtering.
- Default-on behavior for all remaining visible columns.

## Files That Must Change

- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- `Module_Reporting/Models/Model_ReportingPreviewModuleCard.cs`
- `Module_Reporting/Views/View_Reporting_PreviewDialog.xaml`
- `MTM_Receiving_Application.Tests/Unit/Module_Reporting/ViewModels/ViewModel_Reporting_MainTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_Reporting/Services/Service_ReportingFormattingTests.cs`

## Implementation Steps

1. Replace the current preferred-key approach with explicit per-module option pruning plus default-on behavior.
2. Remove the requested Receiving checkbox options entirely from the Reporting options surface.
3. Remove the requested Dunnage checkbox options entirely from the Reporting options surface.
4. Remove the requested Volvo checkbox options entirely from the Reporting options surface.
5. Default every remaining visible checkbox option to included for each module.
6. Preserve any system-required forced columns that must stay enabled for grouped-row display, but only if they survive the requested removal rules.

## Task Checklist

- [x] Remove the requested Receiving option list.
- [x] Remove the requested Dunnage option list.
- [x] Remove the requested Volvo option list.
- [x] Default all remaining visible options to on.
- [x] Update tests for the new per-module option inventories.

## Validation

- Add tests proving removed columns no longer appear in the relevant module option lists.
- Add tests proving all remaining visible columns default to included.
- Verify formatting/export still respects the new included-column inventories.
- Run focused reporting viewmodel and formatting tests.

## Guardrails

- Do not change grouped-row placeholder/range behavior in this slice.
- Do not reintroduce removed columns through fallback logic.
- Do not keep the old preferred-six fallback if it conflicts with the new default-on requirement.

## Completion Criteria

- The requested per-module removed columns no longer appear as options.
- All remaining visible options default to on for Receiving, Dunnage, and Volvo.
- Preview/export uses the updated option inventories correctly.

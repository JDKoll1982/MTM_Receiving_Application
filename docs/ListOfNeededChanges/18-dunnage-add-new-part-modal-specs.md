# 18 Dunnage Add New Part Modal Specs

Last Updated: 2026-03-31

## Change

In the Dunnage Add New Part modal, Width, Height, and Depth should not be universally required. Only show the specs that belong to the selected Dunnage Type.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_Dialog_NonPOEntry.xaml`
- `Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml`
- `Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml`
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs`

## Implementation Instructions

1. Identify which Add New Dunnage Part dialog is the active one in the current workflow.
2. Move spec visibility and required-state rules to the selected Dunnage Type configuration.
3. Show only the spec fields attached to that type.
4. Remove validation errors for spec fields that are hidden for the current type.
5. Preserve existing behavior for types that truly require all three dimensions.

## Acceptance Checks

- Hidden spec fields are not required.
- Switching type updates the visible spec set correctly.

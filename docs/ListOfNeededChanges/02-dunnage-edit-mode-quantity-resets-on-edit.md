# 02 Dunnage Edit Mode Quantity Resets On Edit

Last Updated: 2026-03-31

## Change

Fix the quantity cell so edited values do not snap back to the original value.

## Likely Files

- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/Models/Model_DunnageLoad.cs`
- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`

## Implementation Instructions

1. Trace the quantity cell binding mode, update trigger, and backing property setter.
2. Confirm the edited row model raises change notifications for quantity.
3. Check whether edit-mode cloning or rollback logic is overwriting the edited value during focus loss.
4. Fix the row update pipeline so the edited value lands in the editable working set before validation and save.
5. Add a regression test for edit quantity -> tab out -> value remains changed.

## Acceptance Checks

- Edit a quantity and tab away: the new value stays visible.
- Save Changes persists the updated quantity.
- Reopening Edit Mode shows the saved quantity.

# 15 Dunnage Edit Mode Remove Selected Not Working

Last Updated: 2026-03-31

## Change

Fix the Remove Selected action in Dunnage Edit Mode.

## Likely Files

- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs`
- `Module_Dunnage/Data/Dao_DunnageLoad.cs`

## Implementation Instructions

1. Trace the Remove Selected command from the button to the selected row list.
2. Confirm selected rows are passed into the removal workflow.
3. Decide whether removal is hard delete, soft delete, or queue removal, then keep the implementation consistent with existing Dunnage semantics.
4. Refresh the grid after successful removal and clear selection state.
5. Add a confirmation step if the workflow currently lacks one.

## Acceptance Checks

- Selecting rows and clicking Remove Selected removes the intended rows.
- The grid refreshes and the removed rows no longer appear.

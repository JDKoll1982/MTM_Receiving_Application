# 14 Dunnage Edit Mode Save Changes Not Working

Last Updated: 2026-03-31

## Change

Fix the Save Changes action in Dunnage Edit Mode.

## Likely Files

- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs`
- `Module_Dunnage/Data/Dao_DunnageLoad.cs`

## Implementation Instructions

1. Confirm the button is bound to the intended save command and the command can execute.
2. Verify the ViewModel tracks edited rows separately from the original loaded rows.
3. Validate the service call and DAO update path for each editable field.
4. Ensure the save path refreshes the working set only after a successful write.
5. Add logging around the save pipeline so future failures are visible.

## Acceptance Checks

- Edit one row, click Save Changes, refresh, and verify the data persists.
- Failed saves surface a clear error instead of silently doing nothing.

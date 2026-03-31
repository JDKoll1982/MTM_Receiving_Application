# 01 Dunnage Edit Mode Quantity Whole Number

Last Updated: 2026-03-31

## Change

Restrict Dunnage Edit Mode quantity entry to whole numbers only.
The saved value must also remain a whole number, and the UI must display it as a whole number with no decimal places.

## Likely Files

- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`
- `Module_Dunnage/Models/Model_DunnageLoad.cs`

## Implementation Instructions

1. Find the editable quantity column binding in the Dunnage Edit Mode grid.
2. Change the editor to an integer-only control or enforce integer parsing in the binding/update path.
3. Reject decimal values in the ViewModel save/update logic so invalid values cannot bypass the UI.
4. If quantity is stored as a decimal type today, normalize input to whole numbers before persistence, and expose the grid/display binding through a whole-number formatter or integer-backed property so decimals never appear in the UI.
5. Show inline validation instead of silently rounding if a non-whole value is entered.
6. Confirm any grid reload, refresh, or save round-trip continues to display the quantity as a whole number.

## Acceptance Checks

- Typing `5` succeeds.
- Typing `5.5` is blocked or produces a clear validation message.
- Saved quantity remains unchanged after refresh.
- Saved quantity is stored and shown as `5`, not `5.0` or `5.00`.

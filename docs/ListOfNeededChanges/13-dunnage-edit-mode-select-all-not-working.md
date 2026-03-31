# 13 Dunnage Edit Mode Select All Not Working

Last Updated: 2026-03-31

## Change

Fix the Select All action in Dunnage Edit Mode.

## Likely Files

- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`

## Implementation Instructions

1. Trace the Select All command from the button to the row selection state.
2. Confirm the grid row model exposes a selectable flag and raises change notifications.
3. Make Select All operate on the filtered working set, not only the full unfiltered dataset, unless current UX says otherwise.
4. Add a companion deselect path if the same button is toggle-based.

## Acceptance Checks

- Clicking Select All selects every visible row.
- Filtered views only select the intended rows.
- Save/Remove actions can consume the selection state.

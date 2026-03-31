# 04 Dunnage Edit Mode Part ID Button Modal

Last Updated: 2026-03-31

## Change

Make the Part ID cell a button that opens a picker showing only Part IDs for the currently selected Type.

## Companion Instruction

- Implement this together with `08-dunnage-edit-mode-modal-fuzzy-search.md` so the picker uses the same fuzzy-search modal pattern as the other Dunnage selectors.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_PartSelectionViewModel.cs`

## Implementation Instructions

1. Replace the editable Part ID text cell with a button-styled cell template.
2. Require a selected Type before opening the picker.
3. Populate the picker from the Dunnage part list filtered by the row's current Type.
4. Use the shared fuzzy-search behavior from `08-dunnage-edit-mode-modal-fuzzy-search.md` so the user can narrow large part lists quickly.
5. When the user selects a part, update the row Part ID and keep the row in the dirty set.
6. Provide a user-facing message if no Type is selected yet.

## Acceptance Checks

- Clicking Part ID opens a picker only when Type is set.
- The picker only shows parts for the selected Type.
- Selecting a part updates the row immediately.

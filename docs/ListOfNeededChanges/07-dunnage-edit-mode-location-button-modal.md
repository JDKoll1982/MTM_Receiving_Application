# 07 Dunnage Edit Mode Location Button Modal

Last Updated: 2026-03-31

## Change

Make the Location cell a button that opens a selectable location list sourced from Infor Visual.

## Companion Instruction

- Implement this together with `08-dunnage-edit-mode-modal-fuzzy-search.md` so the location picker uses the same fuzzy-search modal pattern as the Type and Part selectors.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Core/Services/Database/Service_InforVisualConnect.cs`

## Implementation Instructions

1. Replace the Location edit cell with a button-styled template.
2. Add or reuse a read-only Infor Visual location lookup service method.
3. Load available locations into a picker modal when the button is clicked.
4. Apply the fuzzy-search interaction defined in `08-dunnage-edit-mode-modal-fuzzy-search.md` so the user can search locations in the modal.
5. On selection, update the row location and mark the row dirty.
6. Keep this lookup read-only and aligned with the existing Infor Visual access rules.

## Acceptance Checks

- Clicking Location opens a location picker.
- Selecting a location updates the row.
- No direct write occurs to Infor Visual.

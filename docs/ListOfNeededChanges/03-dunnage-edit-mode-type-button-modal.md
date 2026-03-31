# 03 Dunnage Edit Mode Type Button Modal

Last Updated: 2026-03-31

## Change

Make the Type cell a button that opens a Dunnage Type picker. If the user changes Type, clear Part ID. Do not show types that have no valid Part IDs.

## Companion Instruction

- Implement this together with `08-dunnage-edit-mode-modal-fuzzy-search.md` so the picker uses the shared fuzzy-search modal pattern.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_TypeSelectionViewModel.cs`
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs`

## Implementation Instructions

1. Replace the editable Type text cell with a button-styled cell template.
2. Reuse the existing Dunnage Type selection flow if present instead of creating a new picker from scratch.
3. Wire the picker to the shared fuzzy-search experience described in `08-dunnage-edit-mode-modal-fuzzy-search.md` instead of building a one-off modal.
4. Filter the source list so only types with at least one active/usable part are shown.
5. When a new type is selected, set the row Type to the new value and clear the row Part ID immediately.
6. Mark the row dirty so Save Changes persists both values.

## Acceptance Checks

- Clicking Type opens a selection modal.
- Picking a different Type clears Part ID.
- Types with no parts do not appear.

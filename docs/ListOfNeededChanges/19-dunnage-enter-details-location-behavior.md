# 19 Dunnage Enter Details Location Behavior

Last Updated: 2026-03-31

## Change

Make the Dunnage Enter Details location field behave like the Receiving Enter Load Information location field, including allowing a new location entry.

## Companion Instruction

- If this behavior uses a selection modal, implement the picker with the shared fuzzy-search pattern described in `08-dunnage-edit-mode-modal-fuzzy-search.md`.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs`
- `Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs`

## Implementation Instructions

1. Inspect the Receiving location workflow and copy its behavior into Dunnage Enter Details.
2. Allow the user to type a new location instead of forcing a locked list-only field.
3. Preserve any existing suggestion or selection aid if one already exists.
4. If the Dunnage screen opens a location picker or assisted-search modal, route that implementation through the shared fuzzy-search approach defined in `08-dunnage-edit-mode-modal-fuzzy-search.md`.
5. Keep the final accepted location in sync with the backing ViewModel property.

## Acceptance Checks

- Users can type a new location directly.
- Existing matching locations can still be selected using the current assistance pattern.

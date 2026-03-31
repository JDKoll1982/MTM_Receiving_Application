# 08 Dunnage Edit Mode Modal Fuzzy Search

Last Updated: 2026-03-31

## Change

Add a fuzzy-search bar to the Type, Part ID, and Location selection modals used from Dunnage Edit Mode.

## Likely Files

- `Module_Core/Dialogs/Dialog_FuzzySearchPicker.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_TypeSelectionViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_PartSelectionViewModel.cs`

## Implementation Instructions

1. Reuse the existing fuzzy-search dialog pattern instead of creating three different modal implementations.
2. Pass modal-specific item lists into the picker and filter them client-side as the user types.
3. Highlight the best match and support keyboard selection.
4. Keep the search logic case-insensitive and tolerant of partial matches.
5. Apply the same pattern to Type, Part ID, and Location pickers for consistency.

## Acceptance Checks

- Each picker shows a search box.
- Typing narrows results immediately.
- Keyboard and mouse selection both work.

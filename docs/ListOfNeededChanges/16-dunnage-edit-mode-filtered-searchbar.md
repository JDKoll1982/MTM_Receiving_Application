# 16 Dunnage Edit Mode Filtered Searchbar

Last Updated: 2026-03-31

## Change

Add the same filtered search bar used in Receiving Edit Mode to Dunnage Edit Mode.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`

## Implementation Instructions

1. Review the Receiving Edit Mode search/filter UI and copy the interaction pattern, not just the visual layout.
2. Add a Dunnage-specific search text property and filter command/state in the ViewModel.
3. Filter the displayed Dunnage rows against the current search term without destroying the original dataset.
4. Decide which Dunnage columns participate in the filter and keep that list explicit.

## Acceptance Checks

- Typing into the new search bar narrows Dunnage Edit Mode rows.
- Clearing the search restores the full edit list.

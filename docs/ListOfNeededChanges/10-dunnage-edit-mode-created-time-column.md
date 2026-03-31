# 10 Dunnage Edit Mode Created Time Column

Last Updated: 2026-03-31

## Change

Add a Created Time column sourced from Created Date, formatted like `3:30 P.M.`.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/Models/Model_DunnageLoad.cs`

## Implementation Instructions

1. Add a new read-only column beside Created Date.
2. Expose Created Time through a converter or computed property based on the existing Created Date timestamp.
3. Format the string in 12-hour AM/PM style with the punctuation the user requested.
4. Keep the raw timestamp as the source of truth.

## Acceptance Checks

- Created Time appears as a separate column.
- Rows with the same date can still show different times.

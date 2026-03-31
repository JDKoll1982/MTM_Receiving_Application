# 11 Dunnage Edit Mode Filter Buttons Clipped

Last Updated: 2026-03-31

## Change

Shorten filter button text so the MainWindow layout stops clipping the buttons.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`

## Implementation Instructions

1. Remove embedded date values from filter button labels.
2. Keep the active filter meaning clear through shorter static labels and tooltips if needed.
3. Recheck the parent panel choice if a StackPanel is preventing proper shrinking.
4. Prefer layout fixes that do not widen the window requirement.

## Acceptance Checks

- Filter buttons are fully visible at normal app width.
- Users can still understand which filter each button applies.

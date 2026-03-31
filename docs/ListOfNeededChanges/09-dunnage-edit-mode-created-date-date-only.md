# 09 Dunnage Edit Mode Created Date Date Only

Last Updated: 2026-03-31

## Change

Display Created Date as date-only in Dunnage Edit Mode.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`
- `Module_Dunnage/Models/Model_DunnageLoad.cs`

## Implementation Instructions

1. Find the Created Date column template in Dunnage Edit Mode.
2. Format the displayed value as date-only using a converter, string format, or computed property.
3. Preserve the original underlying timestamp for sorting and any save logic.
4. Do not mutate stored data solely for display formatting.

## Acceptance Checks

- Created Date displays only the calendar date.
- Sorting by Created Date still uses the full underlying timestamp.

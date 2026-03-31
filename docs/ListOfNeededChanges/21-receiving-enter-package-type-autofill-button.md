# 21 Receiving Enter Package Type Auto Fill Button

Last Updated: 2026-03-31

## Change

Add an Auto-Fill button inside the Package Type card on Receiving Enter Package Type, using the same Auto-Fill behavior pattern used elsewhere in the application.

## Likely Files

- `Module_Receiving/Views/View_Receiving_PackageType.xaml`
- `Module_Receiving/ViewModels/ViewModel_Receiving_PackageType.cs`

## Implementation Instructions

1. Find the Package Type card layout and place an Auto-Fill button inside that card, not outside it.
2. Reuse the existing Auto-Fill command pattern already used in other Receiving screens.
3. Keep the button disabled when there is not enough context to calculate sensible values.
4. Make sure the command updates the same bound fields users can edit manually.

## Acceptance Checks

- Auto-Fill appears inside the Package Type card.
- Clicking it populates fields the same way as other Auto-Fill actions in the app.

# 12 Dunnage Edit Mode Remove Columns Help Button

Last Updated: 2026-03-31

## Change

Remove the `?` help button to the right of Columns because the help entry already exists elsewhere on the page.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`

## Implementation Instructions

1. Remove the duplicate help button from the Columns area.
2. Verify no command binding or event handler remains orphaned in code-behind or the ViewModel.
3. Leave the bottom-of-page help entry intact.

## Acceptance Checks

- The duplicate `?` button is gone.
- No dead binding warnings appear for the removed control.

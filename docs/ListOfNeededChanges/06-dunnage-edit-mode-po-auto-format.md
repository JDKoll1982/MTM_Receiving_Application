# 06 Dunnage Edit Mode PO Auto Format

Last Updated: 2026-03-31

## Change

Auto-format numeric PO input in Dunnage Edit Mode to `PO-065243`. If formatting fails, keep the user's original input.

## Likely Files

- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`
- `Module_Dunnage/Views/View_Dunnage_EditModeView.xaml`

## Implementation Instructions

1. Add a shared PO normalization helper in the ViewModel or a reusable formatter service.
2. If the trimmed input is all digits, left-pad to six digits and prefix `PO-`.
3. Support both `065243` and `65243` -> `PO-065243`.
4. If the input contains non-numeric characters or cannot be normalized safely, keep the original user input unchanged.
5. Apply the formatter on focus loss or commit, not on every keystroke.

## Acceptance Checks

- `065243` becomes `PO-065243`.
- `65243` becomes `PO-065243`.
- `PO-ABC123` remains `PO-ABC123`.

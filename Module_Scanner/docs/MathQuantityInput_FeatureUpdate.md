# Scanner - Math Quantity Input

Last Updated: 2026-11-25

## What changed

The Scanner quantity boxes no longer take free-typed numbers. They now use one shared control,
`Control_Shared_MathQuantityTextBox`, which keeps quantities to whole numbers and lets the
operator type the math instead of the answer.

## Where it is used

- Scanner Workbench - the **Qty** cell on each line of the current list.
- Scanner Workbench - the quantity column in the stock / part picker modal.
- Manage Items dialog - the **Qty** field on each row.

## How it behaves

- Only digits, `+ - * /`, parentheses, and spaces can be typed. Letters, commas, decimals and
  percent signs are rejected as they are typed.
- The answer must be a whole number and cannot be negative.
- Typing an expression is allowed: `12*4` becomes `48`, `(8+4)/2` becomes `6`.
- The text is replaced with the answer when the operator presses `=`, presses Enter, or leaves
  the box.
- An answer with a fraction rounds to the nearest whole number (`10/4` becomes `3`).
- Empty text is treated as no quantity; normal row validation still reports it.

## The status glyph

A glyph sits to the right of the box:

- Warning glyph - the text is not a valid quantity yet, or the answer had to be rounded. Point
  at the glyph to read the reason.
- Checkmark glyph - the box accepts the number as it stands.

## Status column refresh

Each Scanner line has a Status button whose color and tooltip describe the row. It re-checks
itself as soon as the row's own To or Qty validation finishes — the button reads the row, so no
extra refresh step is needed and the per-cell validation is not interrupted. The Status button is
also clickable and still opens the explanation dialog.

## Line actions

The add-lines action on each line is a plain icon button with a flyout (how many lines, then
**Add Lines**). It is a normal button rather than a split button so the 30-pixel action column
cannot overflow the right edge of the list.

## Help

The box tooltip is a one-line summary of what the field allows. Pressing `Ctrl+I` while that
summary is showing opens a dedicated, categorized help window
(`View_Shared_MathQuantityHelpWindow`) that explains the accepted input, the arithmetic rules,
the rounding behaviour, the glyph meanings, and the keys. The help window is modal: the app
window is disabled until it is closed.

## Keyboard

- `=` or Enter - reduce the expression and accept the answer.
- Tab or Enter - accept the answer and move to the next row's destination cell.
- Ctrl+I - open the help window while the box summary is visible.
- Esc - close the help window.

## Related code

- `Module_Shared/Views/Controls/Control_Shared_MathQuantityTextBox.xaml` - the shared control.
- `Module_Shared/Views/View_Shared_MathQuantityHelpWindow.xaml` - the help window.
- `Module_Shared/Helpers/Helper_SharedMathExpression.cs` - expression parser and rounding rules.
- `Module_Scanner/Views/View_Scanner_Workbench.xaml` - Workbench Qty cell.
- `Module_Scanner/Views/View_Scanner_Workbench.xaml.cs` - quantity commit, row re-validation,
  next-cell navigation, picker modal quantity box, and the whole-number default quantity.
- `Module_Scanner/Views/View_Scanner_ManageItemsDialog.xaml` - Manage Items Qty field.

## Support notes

- Rows loaded from an older session may still hold a decimal quantity. They stay readable and
  still validate; the next edit through the quantity box sets a whole number.
- The picker modal still caps a quantity at the on-hand amount and reports it in the dialog.
- Validation messages now say "whole number" instead of "decimals only".
- A leftover expression that was never committed (for example `5+`) is rejected as a quantity
  instead of being read as `5`, so the wrong amount cannot be moved.

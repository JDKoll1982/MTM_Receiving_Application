# 23 Volvo Shipment History Details Modal

Last Updated: 2026-03-31

## Change

Replace the current basic Shipment Details message window in Volvo Shipment History with a fully designed modal dialog.

## Likely Files

- `Module_Volvo/Views/View_Volvo_History.xaml`
- `Module_Volvo/ViewModels/ViewModel_Volvo_History.cs`
- `Module_Volvo/Views/VolvoShipmentDetailsDialog.xaml` (new)

## Implementation Instructions

1. Find the action that currently opens the basic message window for shipment details.
2. Replace it with a dedicated modal dialog or ContentDialog styled consistently with the rest of the app.
3. Move the details display into a real layout with labeled fields, spacing, and scroll handling for long content.
4. Pass a typed shipment-details model into the dialog rather than assembling a raw message string.
5. Keep the dialog read-only unless a later requirement explicitly adds editing.

## Acceptance Checks

- Shipment Details opens in a designed modal, not a basic message box.
- Long detail content remains readable and scrollable.

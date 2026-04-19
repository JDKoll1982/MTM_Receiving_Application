# 22 Volvo Add Part Location Textbox

Last Updated: 2026-03-31

## Change

Make the Location textbox in the Volvo Add Part modal editable and consistent with the other module location-entry patterns.

## Likely Files

- `Module_Settings.Volvo/Views/View_Settings_Volvo_PartAddEditDialog.xaml`
- `Module_Settings.Volvo/Views/View_Settings_Volvo_PartAddEditDialog.xaml.cs`
- `Module_Volvo/Services/Service_Volvo.cs`

## Implementation Instructions

1. Remove the read-only restriction from the Volvo Add Part location input.
2. Mirror the accepted location-entry pattern used in Receiving/Dunnage: allow user entry and keep any assisted lookup behavior if available.
3. If Volvo already has a location source list, reuse it instead of creating a new source.
4. Keep validation and save wiring aligned with the existing Add Part modal flow.

## Acceptance Checks

- Users can type or select a location in the Volvo Add Part modal.
- The value persists through save as expected.

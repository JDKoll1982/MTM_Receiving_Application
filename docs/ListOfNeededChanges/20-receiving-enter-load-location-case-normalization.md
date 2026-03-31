# 20 Receiving Enter Load Location Case Normalization

Last Updated: 2026-03-31

## Change

When the user enters a location that matches an existing Infor Visual location except for letter casing, replace the textbox text with the exact database casing.

## Likely Files

- `Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs`
- `Module_Receiving/Services/Service_ReceivingValidation.cs`

## Implementation Instructions

1. Locate the current location validation flow used by Enter Load Information.
2. After a successful match, compare the user's input to the canonical location value returned from the lookup.
3. If the only difference is casing, update the textbox/ViewModel property to the canonical database value.
4. Do not treat case-only mismatches as errors.

## Acceptance Checks

- Entering `dock-a1` when the database stores `Dock-A1` updates the textbox to `Dock-A1`.
- Non-matching locations still follow the existing validation behavior.

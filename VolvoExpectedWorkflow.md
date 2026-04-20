# Dunnage Review PO Number Update

## Scope

This request is for the Dunnage guided workflow, not Volvo.

The target UI surface is the Dunnage review screen:

- `Module_Dunnage/Views/View_Dunnage_ReviewView.xaml`
- `Module_Dunnage/Views/View_Dunnage_ReviewView.xaml.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs`

The visible PO value on the review screen currently comes from:

- Single-entry view: `ViewModel.CurrentLoad.PoNumber`
- Table view: `Binding="{Binding PoNumber}"`

The upstream editable PO textbox that currently feeds the session is here:

- `Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs`

The Dunnage workflow service that copies the session PO number onto generated loads is here:

- `Module_Dunnage/Services/Service_DunnageWorkflow.cs`

The Receiving implementation that should be mirrored for PO auto-formatting is here:

- `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`
- `Module_Receiving/Views/View_Receiving_POEntry.xaml.cs`

## Personas

- Receiving Clerk: Primary user for the Dunnage guided workflow. This is the current audience recorded in `docs/CopilotForms/data/module-metadata/Module_Dunnage/dunnage-guided-workflow.json`.
- Operations Team: Current owning team for the Dunnage guided workflow.
- Inventory / Receiving user: Needs the Dunnage review step to show a normalized PO and the matched vendor clearly before save.
- Developer / QA using mock data: Needs mock purchase orders available for validation without live Infor Visual access.

## Requested Behavior

1. Use the same PO auto-formatting behavior that Receiving already uses.
   - Accept raw numeric input such as `62450` and normalize it to `PO-062450`.
   - Accept partially formatted input such as `PO-62450` and normalize it to `PO-062450`.
   - Leave invalid formats unchanged so validation can show a proper message instead of silently inventing a bad value.

2. Show vendor information in the Dunnage review screen.
   - Add the vendor name under the existing PO Number and Location area in the same review card in `Module_Dunnage/Views/View_Dunnage_ReviewView.xaml`.
   - Only show the vendor label/value when the PO format is valid and a vendor is returned.
   - If the PO format is valid but the PO does not exist in Infor Visual, show an inline error message in the vendor area instead of a vendor name.

3. Keep the review UI aligned with the current Dunnage layout.
   - The single-entry review card already shows `PO Number`, `Location`, `Inventory Method`, and `Load Details` in the right column.
   - The vendor name or not-found message should appear in that same right-column information group under the PO-related content.
   - If table view also needs visibility, it should be treated as a secondary display concern after the single-entry review card.

4. Use the existing Infor Visual service instead of creating a Dunnage-specific vendor lookup path.
   - `Module_Core/Contracts/Services/IService_InforVisual.cs` already exposes `GetPOWithPartsAsync(string poNumber)`.
   - That same service is already used in Dunnage details entry for other Infor Visual-driven validation paths.

5. Add or confirm mock PO coverage for this Dunnage flow.
   - Mock PO data is stored in `Module_Settings.Core/Defaults/inforvisual-mock-data.json`.
   - Existing examples already in the repo include:
     - `PO-066865` -> `Precision Plating Inc.`
     - `PO-066867` -> `Allied Metal Services`
     - `PO-066868` -> `Midwest Coating Group`
   - At least two mock POs should be usable for Dunnage validation, including a successful vendor lookup path and a not-found path.

## Acceptance Criteria

- The Dunnage PO value shown in review uses the same canonical formatting pattern as Receiving.
- The Dunnage review card shows vendor information when the PO is valid and found.
- The Dunnage review card shows an inline not-found error when the PO format is valid but no matching PO exists.
- Vendor information stays hidden when the PO field is blank or malformed.
- Mock mode supports validating this flow without live Infor Visual access.

## File Map For Implementation

- `Module_Dunnage/Views/View_Dunnage_ReviewView.xaml`
  - Review card display target for PO, location, and new vendor feedback.
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs`
  - Review-state and save-flow ViewModel.
- `Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml`
  - Current Dunnage PO entry textbox source.
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs`
  - Current Dunnage PO/session state owner.
- `Module_Dunnage/Services/Service_DunnageWorkflow.cs`
  - Copies current session PO data into generated review loads.
- `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`
  - Source of the PO lost-focus formatting behavior to mirror.
- `Module_Settings.Core/Defaults/inforvisual-mock-data.json`
  - Mock Infor Visual PO catalog used for offline validation.

---

# Volvo Completion PO Textbox Update

## Scope

This second prompt is for the Volvo shipment completion PO textbox, which is the field the user fills in when completing a shipment.

The current Volvo completion PO entry point is here:

- `Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs`
  - `ShowCompletionDialogAsync(...)` currently creates the PO textbox inline using a `ContentDialog`.

Related Volvo files for this flow:

- `Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml`
- `Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml.cs`
- `Module_Volvo/Models/Model_VolvoShipment.cs`
- `Module_Volvo/Models/Model_VolvoShipmentLine.cs`
- `Module_Volvo/Requests/Commands/CompleteShipmentCommand.cs`
- `Module_Volvo/Handlers/Commands/CompleteShipmentCommandHandler.cs`

Infor Visual comparison data comes from:

- `Module_Core/Contracts/Services/IService_InforVisual.cs`
  - `GetPOWithPartsAsync(string poNumber)`
- `Module_Core/Models/InforVisual/Model_InforVisualPO.cs`
- `Module_Core/Models/InforVisual/Model_InforVisualPart.cs`

## Personas

- Supervisor: Primary Volvo shipment-entry user. This is the current audience recorded in `docs/CopilotForms/data/module-metadata/Module_Volvo/volvo-shipment-entry.json`.
- Receiving / Shipping user: Completes the Volvo shipment and needs immediate feedback if the PO does not match what is on the current shipment.
- Integration Team: Current owning team for this Volvo module.
- Developer / QA: Needs discrepancy behavior to be deterministic and visible only when mismatches exist.

## Requested Behavior

1. Use proper PO formatting before validation, normalize the textbox when focus is lost if nessicary.
   - Reuse the same canonical formatting pattern used by Receiving.
   - Accept raw digits like `62450` and normalize to `PO-062450`.
   - Accept partially formatted values like `PO-62450` and normalize to `PO-062450`.

2. Look up the PO after formatting.
   - After formatting, load the PO using `IService_InforVisual.GetPOWithPartsAsync(...)`.

3. Enforce a Volvo-specific vendor sanity check.
   - The vendor name returned from the PO lookup must contain the word `volvo`, case-insensitive.
   - If the vendor name does not contain `volvo` but the PO contains at least one part number matching the pattern `V-EMB-{NUMBER}` (e.g. `V-EMB-1234`), treat the PO as valid for this workflow.
   - If the PO is otherwise valid but the vendor name does not contain `volvo` and no part numbers match the `V-EMB-{NUMBER}` pattern, treat it as the wrong PO for this workflow.

4. Treat a PO with no matching shipment parts as the wrong PO.
   - If the entered PO does not contain any of the current shipment part numbers, assume the user entered the wrong PO number.
   - In that case, show an inline wrong-PO validation message instead of treating it as a normal quantity discrepancy case.
   - Turn the BG of the PO Number Textbox light red and the border Dark Red, restore to normal colors once corrected.

5. Validate part numbers and quantities.
   - Compare the Infor Visual PO lines against the current shipment lines.
   - Use the calculated totals, not total skids to compare the quantities.
   - The current Volvo shipment line already exposes `CalculatedPieceCount`, which is the value that should be compared against the PO quantity expectation.
   - Match shipment lines to PO lines by part number only. Line numbers are not a reliable matching key because they may differ between the app and Infor Visual and must not be used.
   - Each part number is expected to appear exactly once on the shipment and exactly once on the PO.
   - For each part number on the shipment, compare its `CalculatedPieceCount` directly against the `QtyOrdered` value of the matching PO line.
   - Every part number on the shipment must exist on the PO and the quantities must match exactly.
   - If any part number on the shipment is not found on the PO, include it in the discrepancy card as a missing PO line.
   - If any part number on the PO is not found on the shipment, include it in the discrepancy card as an unexpected PO line.
   - If any part number quantities do not match, show a discrepancy card containing all mismatches.
   - That discrepancy card should stay hidden when there are no mismatches.
   - The discrepancy card should enumerate every mismatch rather than stopping at the first one.

## Current Data Available In Code

- `Module_Volvo/Models/Model_VolvoShipmentLine.cs`
  - Already has `PartNumber`
  - Already has `CalculatedPieceCount`
  - Already has `ExpectedPieceCount`
  - Does not currently expose a PO line number field

- `Module_Core/Models/InforVisual/Model_InforVisualPart.cs`
  - Already has `PartID`
  - Already has `POLineNumber`
  - Already has `QtyOrdered`

## Important Implementation Note

The requested Volvo comparison logic needs both part-number matching and PO line-number matching, but the current Volvo shipment-line model does not appear to store a PO line number yet.

That means the implementation likely needs one of these approaches:

- Add PO line-number data to the Volvo shipment comparison model, or
- Derive a deterministic line-number mapping during PO validation before showing the discrepancy card.

The markdown requirement should preserve that constraint explicitly so the eventual implementation does not silently skip the line-number requirement.

## Acceptance Criteria

- The Volvo completion PO textbox normalizes to the same canonical format used by Receiving.
- The PO is looked up immediately after normalization when the user attempts to complete the shipment.
- Comparison uses calculated piece totals, not skid counts.
- If part numbers and line numbers match but quantities are different, a discrepancy card appears and lists all mismatches.
- The discrepancy card remains hidden when there are no mismatches.
- If none of the shipment part numbers exist on the PO, the PO is treated as incorrect for the shipment.
- If the vendor name does not contain `volvo` in any casing, the PO is treated as incorrect for the shipment.

## File Map For Implementation

- `Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs`
  - Current completion dialog and likely validation orchestration point.
- `Module_Volvo/Models/Model_VolvoShipmentLine.cs`
  - Current shipment comparison data source (`PartNumber`, `CalculatedPieceCount`).
- `Module_Core/Contracts/Services/IService_InforVisual.cs`
  - Existing PO lookup service.
- `Module_Core/Models/InforVisual/Model_InforVisualPO.cs`
  - PO header and vendor source.
- `Module_Core/Models/InforVisual/Model_InforVisualPart.cs`
  - PO line source for part IDs, line numbers, and ordered quantities.

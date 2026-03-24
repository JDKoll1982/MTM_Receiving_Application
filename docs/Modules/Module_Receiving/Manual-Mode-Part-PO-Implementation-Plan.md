# Manual Mode Part and PO Implementation Plan

Last Updated: 2026-03-24

## Scope Guardrail

This work is restricted to Module_Receiving Manual Mode behavior only.

- Do not change Guided Mode flow.
- Do not change Edit Mode flow.
- Do not change PO Entry step behavior in Guided Mode.
- Do not change validation behavior in other Module_Receiving screens unless Manual Mode explicitly calls the shared method and the change is required for the Manual Mode workflow.

## Workflow Delta From Revised Diagram

Required behavior for Manual Mode:

1. Part-driven flow when Part ID loses focus.
2. Verify the part against Infor Visual or mock data.
3. If part does not exist, show fuzzy part search and clear Part ID when unresolved.
4. If part exists and PO is blank, show PO picker for all POs containing that part.
5. PO picker results must include vendor.
6. If PO is populated, verify whether the PO contains the entered part.
7. If PO does not contain the part, show a dialog listing all parts on that PO and state the entered Part ID did not match.
8. If mismatch remains unresolved, clear the Part ID cell.
9. If PO is entered first and Part ID is blank, show all parts for that PO.

## Affected Files Inventory

### Existing files expected to change

1. Module_Receiving/Views/View_Receiving_ManualEntry.xaml
   - Lines 138, 164, 177, 178
   - Current hooks: `PONumberTextBox_LostFocus`, `PartIdCell_DoubleTapped`, `PartIDTextBox_LostFocus`
   - Reason: verify whether any event routing or column behavior needs a Manual-Mode-only adjustment.

2. Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs
   - Lines 454, 475, 502, 548
   - Current methods/values:
     - `PONumberTextBox_LostFocus`
     - `FormatPONumber`
     - `PartIDTextBox_LostFocus`
   - Reason: this is the UI event entry point for PO-first and part-first row resolution.

3. Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs
   - Lines 1184, 1336, 1345, 1369, 1380, 1420
   - Current methods/values:
     - `TrySelectPartForPoAsync`
     - `BuildPartSelectionDetail`
     - `ApplySelectedPoPart`
     - `ClearPoSelectedPart`
     - `ShowPoHasNoUsablePartsDialogAsync`
     - `TryEnterManualEntryDialogAsync`
   - Also affected around lines 871, 895, 1225, 1240, 1266, 1279, 1331 where the current save and selection flow assumes PO-first matching.
   - Reason: primary Manual Mode orchestration belongs here.

4. Module_Receiving/Services/Service_ReceivingValidation.cs
   - Lines 85, 115, 362, 374
   - Current methods/values:
     - `ValidatePONumber`
     - `ValidatePartID`
   - Reason: likely reused by Manual Mode resolution path; changes must not alter other modes unexpectedly.

5. Module_Core/Contracts/Services/IService_InforVisual.cs
   - Lines 20, 27, 87, 139
   - Current methods:
     - `GetPOWithPartsAsync`
     - `GetPartByIDAsync`
     - `FuzzySearchPartsAsync`
     - `PartExistsAsync`
   - Reason: likely needs a new Manual-Mode support method for searching POs by part.

6. Module_Core/Services/Database/Service_InforVisualConnect.cs
   - Lines 71, 129, 579, 703
   - Current methods:
     - `GetPOWithPartsAsync`
     - `GetPartByIDAsync`
     - `FuzzySearchPartsAsync`
     - `PartExistsAsync`
   - Reason: service-layer implementation for new read-only Infor Visual lookup(s).

7. Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs
   - Lines 70, 166, 356, 740
   - Current methods:
     - `GetPOWithPartsAsync`
     - `GetPartByNumberAsync`
     - `FuzzySearchPartsByIdAsync`
     - `PartExistsAsync`
   - Reason: read-only SQL access point; likely needs a new PO-by-part query method.

8. Module_Core/Dialogs/Dialog_FuzzySearchPicker.xaml.cs
   - Lines 26, 40, 64
   - Current methods/types:
     - `Dialog_FuzzySearchPicker`
     - `ApplyFilter`
   - Reason: probably reusable as-is, but included because the new Manual Mode flow depends on richer PO result details including vendor.

9. Module_Core/Models/InforVisual/Model_FuzzySearchResult.cs
   - Line 7
   - Current values:
     - `Key`
     - `Label`
     - `Detail`
   - Reason: vendor can likely be surfaced through `Detail`; if not sufficient, this model may need extension.

10. Module_Core/Models/InforVisual/Model_InforVisualPO.cs
    - Lines 9, 13, 22
    - Current values:
      - `Vendor`
      - `Parts`
      - `HasParts`
    - Reason: existing PO vendor data should feed the new PO picker results.

11. Module_Core/Models/InforVisual/Model_InforVisualPart.cs
    - Lines 7, 35
    - Current values:
      - `PartID`
      - `POLineNumber`
      - `Description`
      - `DisplayText`
    - Reason: mismatch dialog and PO-first selection depend on part labels/details.

12. docs/Modules/Module_Receiving/Manual-Mode-Part-PO-Workflow-Comparison.md
    - Intended workflow Mermaid diagram
    - Reason: keep documentation aligned with implementation.

13. docs/CopilotForms/data/module-metadata/Module_Receiving/receiving-workflow.json
14. docs/CopilotForms/data/module-metadata/Module_Receiving/index.json
    - Reason: metadata may need refresh after Manual Mode workflow changes.

### New files likely required

1. Database/InforVisualScripts/Queries/14_FuzzySearchPOsByPart.sql
   - Purpose: return all POs containing a given part, ordered descending, all statuses, including vendor details for the picker.

### Files intentionally not targeted

- Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs
- Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs
- Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs
- Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs
- Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs

These are excluded unless implementation reveals a strict Manual-Mode-only hook is impossible without a narrowly scoped change.

## Methods and Data Points Expected To Be Added Or Updated

### Manual Mode methods

- Update `PartIDTextBox_LostFocus`
- Update `PONumberTextBox_LostFocus`
- Split current PO-first behavior into Manual-Mode-specific helpers
- Add a part-first resolution method for Manual Mode
- Add a PO-by-part picker method for Manual Mode
- Add a mismatch-resolution dialog method for Manual Mode
- Ensure unresolved paths clear Part ID only in Manual Mode

### Infor Visual service and DAO additions

- Add service contract method to search POs by part
- Add service implementation method to search POs by part
- Add DAO method to query POs by part
- Add SQL query resource for PO-by-part lookup

## Models, Enums, Values

### Models explicitly involved

- `Model_ReceivingLoad`
  - `PartID`
  - `PoNumber`
  - `PoLineNumber`
  - `SelectedPartSourcePONumber`
  - `PoVendor`
  - `InitialLocation`
  - `PartDescription`
  - `QtyOrdered`
  - `RemainingQuantity`

- `Model_FuzzySearchResult`
  - likely reuse `Label` + `Detail` to show PO plus vendor

- `Model_InforVisualPO`
  - vendor source for PO picker detail

- `Model_InforVisualPart`
  - current part/PO selection payload

### Enums expected to remain unchanged

- No new enum is currently planned.
- Existing workflow enums should not change because this work is isolated to Manual Mode row resolution.

## Isolation Strategy

1. Keep all new behavior behind Manual Entry handlers and Manual Entry viewmodel methods.
2. Reuse existing shared validation only where behavior already matches current Receiving expectations.
3. Add new Infor Visual search capability as a read-only service/DAO extension without changing Guided flow callers.
4. Avoid changing workflow-step logic or PO Entry viewmodel logic.
5. Validate that Guided and Edit mode entry points remain untouched.

## Validation Plan

1. Manual Mode, part-first:
   - valid part + blank PO
   - invalid part + fuzzy part results
   - cancel fuzzy part picker clears Part ID

2. Manual Mode, part-first with PO present:
   - entered PO contains part
   - entered PO does not contain part
   - mismatch dialog resolves to a valid part on the same PO
   - unresolved mismatch clears Part ID

3. Manual Mode, PO-first:
   - PO entered with blank Part ID opens parts-on-PO dialog
   - cancel leaves row unresolved with Part ID cleared

4. Regression checks:
   - Guided Mode PO Entry unchanged
   - Guided Mode part selection unchanged
   - Edit Mode unchanged

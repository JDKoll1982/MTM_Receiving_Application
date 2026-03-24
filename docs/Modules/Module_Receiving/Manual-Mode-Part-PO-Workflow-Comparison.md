# Manual Mode Part and PO Workflow Comparison

Last Updated: 2026-03-24

This document compares the current Manual Mode behavior in Module_Receiving against the intended workflow described for part and PO matching.

## Current Workflow

```mermaid
flowchart TD
  WCur_Start([User edits Manual Entry row]) --> WCur_PartLostFocus[User leaves Part ID cell]
  WCur_PartLostFocus --> WCur_FormatPart[Apply configured part-number padding]
  WCur_FormatPart --> WCur_DefaultLocation[Try to apply default location from part lookup]
  WCur_DefaultLocation --> WCur_PartEnd([No part existence check or fuzzy part dialog])

  WCur_Start --> WCur_POLostFocus[User leaves PO cell]
  WCur_POLostFocus --> WCur_POBlank{PO blank?}
  WCur_POBlank -->|Yes| WCur_ClearPO[Set PO to null]
  WCur_ClearPO --> WCur_ClearEnd([No dialog opened on blank PO])
  WCur_POBlank -->|No| WCur_FormatPO[Format PO to PO-NNNNNN or PO-NNNNNNB]
  WCur_FormatPO --> WCur_ValidatePO{PO format valid?}
  WCur_ValidatePO -->|No| WCur_InvalidPOEnd([Stop part-selection flow])
  WCur_ValidatePO -->|Yes| WCur_LoadPO[Load PO and all PO parts from Infor Visual or mock data]
  WCur_LoadPO --> WCur_HasParts{PO has selectable parts?}
  WCur_HasParts -->|No| WCur_NoPartsDialog[Show dialog that PO cannot be used here]
  WCur_NoPartsDialog --> WCur_NoPartsEnd([User returns to grid])
  WCur_HasParts -->|Yes| WCur_ShowPartPicker[Open part-selection fuzzy picker for all parts on that PO]
  WCur_ShowPartPicker --> WCur_UserSelectsPart{User selects a part?}
  WCur_UserSelectsPart -->|No| WCur_CancelEnd([Leave row without resolved PO-part match])
  WCur_UserSelectsPart -->|Yes| WCur_ApplySelectedPart[Populate Part ID, PO line, description, quantities, and default location]
  WCur_ApplySelectedPart --> WCur_Done([Row has a resolved PO-to-part selection])
```

## Intended Workflow

```mermaid
flowchart TD
  WInt_Start([User enters Part Number in Manual Mode and leaves the cell]) --> WInt_CheckPart[Check entered Part Number against Infor Visual or mock-data part list]
  WInt_CheckPart --> WInt_PartFound{Part exists?}
  WInt_PartFound -->|No| WInt_ShowPartFuzzy[Open fuzzy-search dialog with similar parts]
  WInt_ShowPartFuzzy --> WInt_UserChoosesPart{User selects a part?}
  WInt_UserChoosesPart -->|No| WInt_ClearPartAfterPartCancel[Clear Part ID cell]
  WInt_ClearPartAfterPartCancel --> WInt_PartCancelEnd([Row remains unresolved])
  WInt_UserChoosesPart -->|Yes| WInt_UseChosenPart[Use selected part as the row Part Number]
  WInt_UseChosenPart --> WInt_CheckPOCell
  WInt_PartFound -->|Yes| WInt_CheckPOCell{PO cell populated?}
  WInt_CheckPOCell -->|No| WInt_ShowPOList[Open fuzzy-search dialog showing all POs that contain the part, ordered descending, all statuses]
  WInt_ShowPOList --> WInt_UserChoosesPO{User selects a PO?}
  WInt_UserChoosesPO -->|No| WInt_ClearPartAfterPoCancel[Clear Part ID cell]
  WInt_ClearPartAfterPoCancel --> WInt_POCancelEnd([Row remains unresolved])
  WInt_UserChoosesPO -->|Yes| WInt_ApplyPO[Apply selected PO to the row]
  WInt_ApplyPO --> WInt_Done([Part and PO are a confirmed match])
  WInt_CheckPOCell -->|Yes| WInt_CheckPOContainsPart[Check whether the entered PO contains the part]
  WInt_CheckPOContainsPart --> WInt_POHasPart{PO contains part?}
  WInt_POHasPart -->|Yes| WInt_Done
  WInt_POHasPart -->|No| WInt_ShowPOMismatch[Open dialog showing all parts for the PO and state that the entered Part ID did not match]
  WInt_ShowPOMismatch --> WInt_MismatchResolved{Mismatch resolved?}
  WInt_MismatchResolved -->|Yes| WInt_Done
  WInt_MismatchResolved -->|No| WInt_ClearPartAfterMismatch[Clear Part ID cell]
  WInt_ClearPartAfterMismatch --> WInt_MismatchEnd([User must resolve the mismatch])

  WInt_POFirstStart([User enters PO Number with no Part ID in the row and leaves the cell]) --> WInt_ShowPoParts[Open dialog showing all parts for the entered PO]
  WInt_ShowPoParts --> WInt_UserChoosesPoPart{User selects a part?}
  WInt_UserChoosesPoPart -->|Yes| WInt_ApplyPoPart[Apply selected part to the row]
  WInt_ApplyPoPart --> WInt_Done
  WInt_UserChoosesPoPart -->|No| WInt_ClearPartAfterPoFirstCancel[Clear Part ID cell]
  WInt_ClearPartAfterPoFirstCancel --> WInt_PoFirstUnresolved([Row remains unresolved])
```

## Key Differences

- Current flow is PO-driven. Intended flow is Part-driven.
- Current Part ID lost-focus behavior only formats the part number and applies a default location. It does not verify that the part exists and does not open a fuzzy-search dialog.
- Current PO lost-focus behavior formats the PO and immediately opens a part-selection dialog for parts on that PO.
- Intended behavior requires checking whether the part exists first, then resolving the PO relationship from that part.
- Intended behavior requires showing all matching POs for a part when the PO cell is blank.
- Intended behavior requires checking whether a populated PO contains the entered part before considering the row resolved.

## Current Code Entry Points Reviewed

- Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs
- Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs

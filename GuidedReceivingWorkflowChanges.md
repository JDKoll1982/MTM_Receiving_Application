<!--
[DOC-META-START]
- File Name: GuidedReceivingWorkflowChanges.md
- Description: Working notes plus the implemented behavior for Guided Receiving workflow step focus and PO number entry rules.
- Last Updated: 2026-09-14
- Quick TOC:
  - Line 12-13: # GuidedReceivingWorkflowChanges
  - Line 14-16: ## Focusing Issue
  - Line 17-28: ## Required Step Focus Behavior
  - Line 29-41: ## Implemented Page Entry Focus
  - Line 42-56: ## Implemented PO Entry Rules
  - Line 57-69: ## Implementation Notes
  - Line 70-78: ## Validation Performed
- Critical Notes: Steps 1 through 7 now focus their primary control on every entry path; PO entry rejects a PO with no part numbers and auto-advances a PO that has exactly one unique part number.
[DOC-META-END]
-->

# GuidedReceivingWorkflowChanges

## Focusing Issue

The Guided Workflow's auto focus on page entry was not landing on the intended control.

## Required Step Focus Behavior

The listed control must be focused no matter how the page is entered, including next and back
buttons, automatic advancement, the main window search bar, and mode selection entry.

1. Receiving - Enter PO Number: focus the PO Number text box and select all of its text.
2. Receiving - Enter Load Information: focus the Number of Loads (1-99) text box.
3. Receiving - Enter Weight & Quantity: focus the Load #1 text box.
4. Receiving - Enter Heat & Lot: focus the Load #1 text box.
5. Receiving - Enter Package Type: focus the Load #1 text box.
6. Receiving - Review & Save: focus the Save to Database button.
7. Receiving - Complete: focus the Start New Entry button.

## Implemented Page Entry Focus

- `View_Receiving_Workflow` owns a single focus pipeline that runs when the workflow step changes,
  when a step host becomes visible, when the page is loaded, and on each layout pass until focus
  succeeds.
- `IReceivingWorkflowFocusable.FocusForAccess` now returns `bool` so the host can retry when a
  control is not ready yet instead of silently giving up.
- `IService_Focus` gained `TrySetFocus` and `TrySetFocusAndSelectAll`, which apply focus
  synchronously and report whether it was accepted.
- The per-view visibility focus hooks that duplicated this work were removed from the PO entry,
  load entry, weight and quantity, heat and lot, and package type views.
- Steps 6 and 7 previously had no focus target at all. The Review save button and the completion
  panel's Start New Entry button are now named and focusable.
- Select all applies to the PO Number field only. Every other step receives plain focus.

## Implemented PO Entry Rules

- The PO loads automatically when the PO Number field loses focus or when Enter is pressed. The
  Load PO button remains available.
- A PO that returns no part numbers is rejected: a warning appears in the main shell header, the
  loaded PO header data is cleared, the PO Number field is emptied, and focus returns to it.
- A PO that returns exactly one unique part number selects that part automatically, announces it
  in the main shell header, and advances straight to Enter Load Information.
- A PO with two or more unique part numbers keeps the current behavior: the parts list is shown
  and the operator selects a part before continuing.
- If the single part is flagged for quality hold and the operator does not acknowledge it, the
  automatic advance is skipped and the operator stays on the PO step.

## Implementation Notes

- The single-part message is `Receiving.Messages.Info.PoSinglePartAutoSelected` and the rejection
  message is `Receiving.Messages.Error.PoHasNoParts`. Both are settings backed with defaults in
  `ReceivingSettingsDefaults`.
- `GetPOUniquePartsWithOnHandAsync` already returns one row per unique part number, so a PO whose
  part appears on several lines is still treated as a single distinct part.
- `LoadPOAsync` exits early when the workflow is no longer on the PO Entry step, which prevents a
  late load from mutating workflow state after an automatic advance.

## Validation Performed

- `dotnet build MTM_Receiving_Application.csproj -c Debug -p:Platform=x64` succeeds.
- `dotnet test` filtered to `Module_Receiving` passes, including new tests covering the no-parts
  rejection, the single-part auto advance, the repeated auto-load guard, a blank PO, and the
  non-PO-step guard.
- Manual focus verification across all seven steps remains the operator-facing check.

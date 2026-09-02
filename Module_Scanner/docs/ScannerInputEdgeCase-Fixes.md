# Scanner Input Edge-Case Fixes

Last Updated: 2026-09-02

## Overview

Working spec for edge-case fixes in the Scanner Workbench "add a part to the input table"
flow. Decisions below were confirmed interactively on 2026-09-02 and are recorded here so
each fix can be implemented and validated independently. Code changes land on the
`InventoryAssistantFeature` branch.

## Goals

- Make wrong or mistyped part numbers produce a clear "Part not found" message and offer
  close matches instead of a misleading stock message.
- Prevent accidental duplicate rows and duplicate validation triggers.
- Remove "ghost" rows that can appear after a failed database save.
- Make quantities safe: require a valid positive number (decimals only), keep the full
  on-hand default, and keep the "1 per transaction" behavior.
- Keep the on-hand quantity guard correct after an app restart by revalidating on reload.
- Validate rows added through Manage Items before they can be saved.
- When validating a list (Check All), only check rows that are not already valid.

## Non-Goals

- Changing the fixed warehouse of 002 for transfers.
- Storing the on-hand amount with each row.
- Changing the "multiple transactions = qty-1 rows" rule.
- Any Infor Visual (SQL Server) writes.

## Confirmed Decisions

| # | Area | Decision |
|---|------|----------|
| 1 | Part not found | Say "Part not found" (not "no stock") and offer a close-match list |
| 2 | Close-match search | Show close matches for the operator to pick from |
| 3 | Duplicate rows | Warn and ask before adding the same part/source again |
| 4 | Double-trigger entry | Ignore a second validation while one is already running |
| 5 | Validation scope | Only validate rows that are not already valid (skip already-valid rows) |
| 6 | Failed save | Fully undo a failed save so no ghost row appears |
| 7 | Blank quantity | Require a valid quantity instead of silently defaulting to 1 |
| 8 | Number format | Quantity fields accept decimals only (period decimal, no thousands commas) |
| 9 | Quantity default | Keep the full on-hand amount as the starting quantity |
| 10 | Multi-transaction | Keep qty-1 rows when more than one transaction is requested |
| 11 | On-hand guard | Revalidate on reload (do not persist the on-hand amount) |
| 12 | Warehouse | No change; transfers are always warehouse 002 |
| 13 | Manage Items | Validate rows before saving |

## Fix Inventory (mapped to code)

- Part-not-found message: `ViewModel_Scanner_Workbench.PartValidationCompletedAsync`.
- Close-match picker: `View_Scanner_Workbench.PartIdLookupControl_ValidationCompleted`
  (reuses `Module_Core` `Dialog_FuzzySearchPicker`).
- Duplicate warn-and-ask: workbench ViewModel raises a confirmation request; the view shows
  a dialog before rows are written.
- Double-trigger guard: reentrancy guard on the Workbench ViewModel part/location
  validation entry points.
- Ghost row: `Service_ScannerWorkflow.UpsertBatchItemAsync` must only add the item to the
  in-memory session after the database write succeeds.
- Picker quantity: validate decimals-only positive quantity that does not exceed on-hand
  before "Use Selected" closes the dialog.
- Row quantity guard: keep decimals-only typing rule on the table quantity cell.
- Check All: validate only rows not already in the valid state.
- Reload revalidation: after a session is resumed from the database, revalidate rows that
  were previously valid so the on-hand guard is restored (via the validation result's
  available quantity) and no on-hand amount is persisted.
- Manage Items: block Apply when any row is incomplete (blank part/source/destination or
  invalid quantity).

## Implementation Status (2026-09-02)

Implemented and validated on branch `InventoryAssistantFeature`:

- **1/2 Part-not-found + fuzzy picker** — Workbench ViewModel now shows a distinct
  "was not found" header error; the view surfaces close matches via the shared
  `Dialog_FuzzySearchPicker` instead of silently using the typed value.
- **3 Duplicate rows** — `PopulateLinesAsync` detects prospective rows matching an
  existing part + source location and raises `DuplicateAddConfirmationRequested`; the
  view shows an "Add Anyway / Cancel" dialog.
- **4 Double-trigger guard** — part/location validation entry points are wrapped with a
  reentrancy guard (`_isValidationFlowRunning`).
- **6 Ghost row** — `Service_ScannerWorkflow.UpsertBatchItemAsync` only mutates the
  in-memory session after the database write succeeds.
- **7/8 Quantity** — picker "Use Selected" validates decimals-only, > 0, and <= on-hand
  (inline error keeps the dialog open); the table quantity cell uses the same
  decimals-only rule.
- **5 Check All** — validates only rows not already valid.
- **11 Reload revalidation** — `RevalidateResumedValidItemsAsync` re-checks resumed-valid
  rows; the live available quantity is carried back via the validation result's
  `MaxQuantity` so the on-hand guard is restored without persisting it.
- **13 Manage Items** — Apply is blocked with an inline error when any row is missing a
  part/source or has an invalid quantity.

Tests: 74 Module_Scanner unit tests pass, and the app and test projects build cleanly
(x64 Debug).

## Send Confirmation Flow Fixes (2026-09-02)

Confirmed desired behavior for sending one current-list line:

1. User clicks **Send** -> validation -> Infor Visual UI automation runs.
2. The app **always asks** the operator Yes/No when they return; it never auto-confirms or
   auto-removes a line based on its own transfer check.
3. **Yes** -> clear that one line only (no history record), keep all other lines, select the
   next line, and leave the main button on **Send** (one line per click).
4. **No** -> do nothing with the line: treat it as never sent (restore Waiting, clear
   SentUtc, persist), keep it in the list so it can be sent again, and reset the button.
5. The main button always reads **Send** (never flips to "Validate").

Code: `ViewModel_Scanner_Workbench.cs` (`SendSelectedAsync`, `ConfirmSavedAsync`,
`ConfirmNotSavedAsync`; `WaitForTransferConfirmationAsync` auto-confirm removed;
`SendButtonText` now constant "Send").

## Validation Expectations

- `dotnet build MTM_Receiving_Application.slnx` succeeds.
- Scanner Workbench unit tests pass
  (`MTM_Receiving_Application.Tests` filtered to `Module_Scanner`).
- New unit tests cover: part-not-found vs no-stock messaging, duplicate warning path,
  reentrancy guard, no ghost row on failed upsert, Check All skipping valid rows, and
  reload revalidation restoring state.

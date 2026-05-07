# Rebuild Receiving Location Reconciliation Prompt

Last Updated: 2026-05-07

## Objective

Replace the current Receiving location reconciliation implementation with a new reconciliation flow that matches received skids or tags to actual Infor Visual transfer destinations and shows the real Infor Visual transfer user, not the current MTM Receiving App user.

This is not an incremental tweak. Remove the current reconciliation decision logic and rebuild it around transfer-based allocation.

## Primary Outcome

When the user runs Reconcile Saved Locations, the system should determine the correct saved location for each received skid or tag by comparing:

1. The quantities received for the same material on the same day, across all relevant MTM receiving entries.
2. The transfer quantities recorded in Infor Visual from the source location, typically `RECV`, into destination locations.
3. A deterministic allocation strategy that maps per-tag quantities to the destination locations.

The result must let the user review and save corrected tag or skid locations with confidence.

## Daily Workflow Example To Implement

Use this scenario as the reference acceptance example:

1. The user receives 5 skids of `MMC0000650` and initially saves all 5 with location `RECV`.
2. The skid quantities are:
	- Skid 1 = 4500
	- Skid 2 = 5000
	- Skid 3 = 7500
	- Skid 4 = 4350
	- Skid 5 = 2000
3. Later, Infor Visual shows these transfers from `RECV`:
	- `12000` moved to `V-A0-01`
	- `5000` moved to `V-B0-01`
	- `6350` moved to `V-C0-01`
4. Reconciliation should infer the per-skid destination mapping as:
	- Skid 1 and Skid 3 -> `V-A0-01`
	- Skid 2 -> `V-B0-01`
	- Skid 4 and Skid 5 -> `V-C0-01`

If the final allocation algorithm is not round robin, that is acceptable, but it must be simpler, safer, deterministic, and documented.

## Additional Transfer Scenarios To Support

The implementation and tests must also cover these patterns.

### Scenario A: All items moved from one location to one new location

Example:

1. Three skids for the same part are all saved as `RECV`.
2. Infor Visual later shows a single transfer total equal to the full received quantity from `RECV` to `V-D0-01`.
3. Reconciliation should assign every matching skid or tag to `V-D0-01`.

Expected behavior:

1. This should be treated as the simplest exact-group move.
2. The algorithm should not mark this ambiguous if the totals fully match.
3. The transfer user and transfer timestamp should come from the matching Infor Visual move.

### Scenario B: Items moved from multiple source locations to a single destination

Example:

1. The same part has saved rows split between `RECV`, `V-STAGE`, and or another temporary source location because of prior handling or partial saves.
2. Infor Visual later shows transfers from multiple source locations into one final destination such as `V-E0-01`.
3. Reconciliation should determine whether those saved rows belong to the same reconciliation group and, when supported by the evidence, converge them to the same final destination.

Expected behavior:

1. The new design must not assume all source evidence starts from exactly one source location.
2. If grouping across multiple source locations is valid for the business rules and evidence, the algorithm should support it.
3. If the evidence is insufficient to safely combine those rows, the result should remain unresolved instead of guessing.

### Scenario C: Items moved from multiple source locations to multiple new destinations

Example:

1. The same part exists across more than one temporary source location.
2. Infor Visual shows several transfers from those source locations into two or more final destinations.
3. Reconciliation must allocate saved rows to the proper destination using a deterministic rule that preserves total quantity and produces stable results.

Expected behavior:

1. The algorithm must support many-to-many source and destination evidence.
2. Tie-breaking must be deterministic.
3. Ambiguous leftovers must remain reviewable and unresolved, not silently assigned.

### Scenario D: Partial movement with leftovers still in the source location

Example:

1. Some skids are still effectively at `RECV` or another source location.
2. Only part of the total quantity has been transferred to final destinations.

Expected behavior:

1. Reconciliation should only move the quantities that are actually supported by transfer evidence.
2. Rows without sufficient transfer evidence should remain unchanged or unresolved according to the final design.
3. The algorithm must not force every saved row into a new location when the data does not support that move.

### Scenario E: Same material received in multiple MTM entries on the same day

Example:

1. The same part is received in more than one MTM load or entry on the same day.
2. Infor Visual transfers reflect combined movement totals rather than a one-to-one mapping to a single MTM entry.

Expected behavior:

1. Reconciliation must be able to group across all relevant same-day MTM entries when that is the only way to match the transfer evidence correctly.
2. The algorithm must document the grouping rule clearly so it is predictable.

### Scenario F: Repeated moves or chained moves

Example:

1. Material moves from `RECV` to a staging location and then later to a final location.
2. Infor Visual records more than one movement chain for the same material on the same day.

Expected behavior:

1. The implementation must define whether reconciliation targets the first destination, the latest confirmed destination, or another explicit rule.
2. That rule must be consistent, testable, and documented.
3. If chained moves cannot be resolved safely with current evidence, they must remain unresolved rather than guessed.

## Non-Negotiable Constraints

1. Remove and replace the current reconcile logic instead of preserving the current smart-fix approach.
2. Follow the repo architecture: View -> ViewModel -> Service -> DAO -> Database.
3. Do not let any ViewModel call a DAO directly.
4. Infor Visual is read-only. Use only `SELECT` queries and enforce `ApplicationIntent=ReadOnly`.
5. MySQL writes must still go through the existing MTM data access patterns.
6. The allocation logic must always terminate. No open-ended loops or retry cycles.
7. If you use an iteration-based allocation pass, hard-stop it with a deterministic bound such as `n * n`, where `n` is the number of candidate skids or tags, or use a better bounded strategy.
8. The displayed transfer user must come from Infor Visual transaction data, not from the MTM app session.
9. If any major requirement remains ambiguous after code inspection, create an assumptions file under `.github/assumptions/` and stop for user review before coding.

## Required Research Inputs

Read and use these files before changing code:

1. `.github/copilot-instructions.md`
2. `.serena/memories/architectural_patterns.md`
3. `.serena/memories/forbidden_practices.md`
4. `docs/development/InforVisual/DatabaseCSVFiles/`
5. `docs/development/InforVisual/InforVisualGuide.md`
6. `.github/instructions/infor-visual-database-reference.instructions.md`
7. `.github/instructions/infor-visual-query-authoring.instructions.md` if a new Infor Visual query must be added

## Code Anchors To Start From

Treat these as the primary implementation surfaces to inspect and update:

1. `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs`
2. `Module_Receiving/Contracts/IService_ReceivingLocationReconciliation.cs`
3. `Module_Receiving/ViewModels/ViewModel_Receiving_LocationReconciliationReview.cs`
4. `Module_Receiving/Views/View_Receiving_Dialog_LocationReconciliationReview.xaml`
5. `Module_Receiving/Views/View_Receiving_Dialog_LocationReconciliationReview.xaml.cs`
6. `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
7. `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`
8. `Module_Receiving/Views/View_Receiving_Workflow.xaml`
9. `Module_Core/Contracts/Services/IService_InforVisual.cs`
10. `docs/features/receiving/location-reconciliation-process.md`

Also inspect and update the relevant implementation behind the Infor Visual service if the current data returned by `IService_InforVisual` is not sufficient for the new algorithm.

## Tests To Update Or Replace

At minimum, review and update these tests to match the new reconciliation design:

1. `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Services/Service_ReceivingLocationReconciliationTests.cs`
2. `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_LocationReconciliationReviewTests.cs`
3. `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntryTests.cs` if recommended-location or reconciliation behavior changes affect load entry behavior

## Personas

### Persona 1: WinUI 3 MVVM Refactor Engineer

You are responsible for keeping the implementation aligned with the MTM architecture. Your job is to replace behavior without breaking the View -> ViewModel -> Service -> DAO boundaries.

### Persona 2: Infor Visual Data Researcher

You are responsible for identifying the correct Infor Visual tables, columns, joins, and transaction-user sources by using the CSV schema exports and guide documents, not guesses.

### Persona 3: Reconciliation Algorithm Designer

You are responsible for designing a deterministic tag-to-transfer allocation strategy that handles same-day multi-skid receipts, cross-entry grouping for the same material, and imperfect transaction data without hanging or producing unstable results.

### Persona 4: Regression-Focused Test Engineer

You are responsible for replacing the old expectations in the existing tests and adding coverage for the new transfer-based behavior, including edge cases and failure modes.

## Implementation Phases

## Phase 0: Research And Current-State Audit

Tasks:

1. Inspect the current reconcile flow in the service, ViewModel, view, and any related Infor Visual service or DAO methods.
2. Identify every place where the current logic uses current inventory, transaction history, user fallback, ambiguous smart-fix logic, or unresolved item handling.
3. Use `docs/development/InforVisual/DatabaseCSVFiles/` and `docs/development/InforVisual/InforVisualGuide.md` to identify the correct Infor Visual transaction source for:
	- transfer quantity
	- from location
	- to location
	- transaction date or time
	- transaction user id or user name
4. Confirm whether the existing `IService_InforVisual` contract is sufficient or needs to be extended.
5. If anything critical is still unclear, write an assumptions file and stop.

## Phase 1: Redesign The Reconciliation Model

Tasks:

1. Define the new reconciliation inputs, grouping rules, and outputs.
2. Group candidate MTM receiving rows by the correct business identity for reconciliation, likely material plus PO context plus same-day receipt scope, even if there are multiple MTM entries that day.
3. Build a deterministic allocation strategy that maps received skid quantities to transfer destination quantities.
4. Ensure the algorithm has explicit exhaustion and termination rules.
5. Decide how to represent these results in `Model_ReceivingLocationReconciliationSummary` and `Model_ReceivingLocationReconciliationItem` if model changes are required.

## Phase 2: Replace The Service Logic

Tasks:

1. Remove the current reconciliation decision logic in `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs`.
2. Rebuild preview generation around transfer-based allocation instead of the current smart-fix path.
3. Pull the transfer user from Infor Visual transaction data.
4. Never use the current MTM session user as the displayed transfer user.
5. Preserve read-only behavior for Infor Visual access and existing MTM save behavior for applying approved location changes.
6. If needed, extend `Module_Receiving/Contracts/IService_ReceivingLocationReconciliation.cs` and the Infor Visual service contract cleanly.

## Phase 3: Update The Review UI And Workflow

Tasks:

1. Update the review ViewModel and XAML so the review screen reflects the new logic and terminology.
2. Show the real transfer user from Infor Visual.
3. Make sure the review screen clearly explains why a destination was chosen.
4. Keep unresolved or ambiguous cases visible and actionable when the system cannot confidently allocate them.
5. Ensure workflow entry points still navigate correctly from mode selection into the reconciliation review step.

## Phase 4: Tests And Documentation

Tasks:

1. Replace outdated unit tests that assert the old behavior.
2. Add tests for:
	- the 5-skid example above
	- all items moved from one location to one new location
	- items moved from multiple source locations to one destination
	- items moved from multiple source locations to multiple destinations
	- partial movement with leftovers still at the source location
	- multiple MTM receipt entries on the same day for the same material
	- repeated or chained moves for the same material
	- exact-match transfer allocation
	- ambiguous transfer totals
	- partial transfer data
	- missing or unknown Infor Visual user data
	- guaranteed algorithm termination
3. Update `docs/features/receiving/location-reconciliation-process.md` so it documents the new design, not the removed one.

## Acceptance Criteria

1. The old reconciliation logic is gone and the new transfer-based logic is the only active path.
2. The 5-skid example produces the expected destination mapping.
3. The reconciliation logic handles one-to-one, many-to-one, and many-to-many transfer scenarios intentionally.
4. The reconciliation logic can consider more than one MTM receiving entry for the same material on the same day when required.
5. The algorithm is deterministic and bounded.
6. The displayed transfer user comes from Infor Visual data.
7. The review UI reflects the new evidence and reasoning.
8. Unit tests cover the new behavior and no longer encode the old smart-fix rules.
9. Documentation is updated to reflect the replacement design.

## Validation Requirements

1. Build the solution.
2. Run the narrowest relevant unit tests for the receiving reconciliation service and review ViewModel.
3. Verify there are no MVVM boundary violations.
4. Verify any new Infor Visual query is read-only and uses the correct documented tables and columns.
5. Summarize what was replaced, what was added, and any remaining follow-up risk.

## Final Instruction

Do not preserve the existing reconciliation algorithm just because it already exists. Treat this as a replacement of the current design with a new design based on real Infor Visual transfer evidence.
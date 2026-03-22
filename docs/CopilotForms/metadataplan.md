# CopilotForms Metadata Implementation Plan

Last Updated: 2026-03-22

## Goal

Bring CopilotForms metadata up to date across the tracked repo paths using the report-only script output as the review artifact, not as an auto-fix source.

## Scope

Tracked paths covered by this plan:

- `Infrastructure`
- `Module_Core`
- `Module_Dunnage`
- `Module_Receiving`
- `Module_Reporting`
- `Module_Settings.Core`
- `Module_Settings.DeveloperTools`
- `Module_Settings.Dunnage`
- `Module_Settings.Receiving`
- `Module_Settings.Reporting`
- `Module_Settings.Volvo`
- `Module_Shared`
- `Module_ShipRec_Tools`
- `Module_Volvo`
- `Database`
- `MTM_Receiving_Application.Tests`

## Inputs And Evidence

Primary evidence sources:

1. The module code itself under each tracked path.
2. Existing split metadata under `docs/CopilotForms/data/module-metadata/`.
3. The generated per-module and all-modules reports under `Scripts/MetaData_Reports/outputs/`.
4. The metadata update prompt and split-metadata documentation.

The report is a triage tool only. It identifies likely stale or missing metadata, but all additions and edits must be verified against real code before changing metadata files.

## Working Rules

1. Update existing split-metadata modules before creating new metadata folders.
2. Keep changes minimal and evidence-based.
3. Prefer one module at a time so the report can be rerun and compared after each change set.
4. For paths with no metadata folder yet, create the smallest useful split-metadata structure first: module folder, `index.json`, and one or more feature files only when the feature boundaries are clear in code.
5. Do not create metadata for generated output, binaries, or implementation details that do not help CopilotForms prompt generation.
6. Treat test projects and infrastructure paths as metadata candidates only if they help user-facing CopilotForms request scoping.

## Phase 1: Refresh Existing Split-Metadata Modules

Refresh the four modules already on the active split-metadata path:

1. `Module_Receiving`
2. `Module_Dunnage`
3. `Module_Reporting`
4. `Module_Volvo`

For each of these:

- Read the module report.
- Compare unreferenced files and services against the current feature files.
- Update only stale summaries, related files, related services, workflow hints, and sub-feature coverage.
- Rerun the report for that module before moving on.

## Phase 2: Add New Metadata For High-Value Missing Modules

Create new split metadata for the highest-value missing modules first, prioritizing modules that are likely to be selected by users in CopilotForms requests:

1. `Module_Core`
2. `Module_Shared`
3. `Module_Settings.Core`
4. `Module_ShipRec_Tools`

For each new module metadata folder:

- Create `docs/CopilotForms/data/module-metadata/<ModuleName>/index.json`.
- Create a focused README only if useful for future maintenance.
- Add only the clearest feature JSON files supported by code structure.
- Avoid over-modeling secondary helpers until the primary feature areas are covered.

## Phase 3: Expand Metadata For Remaining Tracked Paths

Review the remaining paths and decide whether they need split metadata or should remain out of scope for CopilotForms feature guidance:

1. `Infrastructure`
2. `Module_Settings.DeveloperTools`
3. `Module_Settings.Dunnage`
4. `Module_Settings.Receiving`
5. `Module_Settings.Reporting`
6. `Module_Settings.Volvo`
7. `Database`
8. `MTM_Receiving_Application.Tests`

Decision rule:

- Add metadata if the path provides a meaningful user-selectable feature area, workflow, or debugging/test-generation target in CopilotForms.
- Skip metadata if the path is too low-level or internal to be helpful as a direct CopilotForms feature target.
- If skipped, document the reason in the plan follow-up summary.

## Phase 4: Register And Validate

After new module metadata folders are created:

1. Update `docs/CopilotForms/data/copilot-forms.config.json` only if the module should be loaded by the runtime.
2. Rerun the report script per module that changed.
3. Rerun the report script in all-modules mode.
4. Confirm JSON validity and that no module report fails.

## Expected Deliverables

1. Updated split metadata for the four existing split modules.
2. New split metadata folders for approved missing modules.
3. Updated module metadata index registration in the main CopilotForms config where needed.
4. Clean rerun results from the metadata report script.

## Execution Order

Recommended execution order:

1. `Module_Receiving`
2. `Module_Dunnage`
3. `Module_Reporting`
4. `Module_Volvo`
5. `Module_Core`
6. `Module_Shared`
7. `Module_Settings.Core`
8. `Module_ShipRec_Tools`
9. Remaining tracked paths by value and clarity

## Completion Criteria

This effort is complete when:

1. Every changed module has a passing report rerun.
2. The all-modules report completes without failure.
3. Existing metadata-bearing modules no longer omit obvious active services, models, or workflow files without a deliberate reason.
4. Newly added metadata folders are minimal, valid, and clearly tied to real code.

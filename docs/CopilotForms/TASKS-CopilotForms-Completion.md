# CopilotForms Completion Tasks

Last Updated: 2026-03-21

## Goal

Finish the CopilotForms improvement work in one clean pass while keeping the current visual styling, making the form text easier for non-technical users to understand, and preserving detailed Copilot-ready output.

## Guiding Rules

- Keep the current CopilotForms look and feel.
- Make the visible form wording read like Noob Mode.
- Keep the generated Markdown and JSON outputs detailed.
- Reduce typing wherever the likely answer can be selected.
- Split module metadata into separate files so the main config does not become bloated.
- Start with analyzed modules only.

## Current Status

- [x] `UI Change` form upgraded with smarter structured inputs.
- [x] `Debugging` form upgraded with smarter structured inputs.
- [x] `Logic Correction` form upgraded with smarter structured inputs.
- [x] Initial browser validation completed for the three upgraded forms.
- [x] `Module_Receiving` metadata extraction started in split-file format.
- [x] Split metadata loader wired into runtime.
- [x] Main config trimmed after split metadata loader is stable.

## Phase 1 - Metadata Split

### Module_Receiving

- [x] Create dedicated metadata folder.
- [x] Create per-feature metadata files.
- [x] Create module index file.
- [x] Add any missing sub-feature UI hints.
- [x] Add per-feature validation hints where useful.
- [x] Add per-feature prompt defaults where useful.

### Remaining analyzed modules

- [x] Create split metadata files for `Module_Dunnage`.
- [x] Create split metadata files for `Module_Reporting`.
- [x] Create split metadata files for `Module_Volvo`.

### Runtime support

- [x] Add runtime support to load external module metadata files.
- [x] Merge external metadata with shared options cleanly.
- [x] Keep current forms working while migration is in progress.
- [x] Remove migrated inline metadata from `copilot-forms.config.json` after verification.

## Phase 2 - Form Improvements

### Already started

- [x] `UI Change`
- [x] `Debugging`
- [x] `Logic Correction`

### Still to upgrade

- [x] `Documentation Change`
- [x] `Improvement Refactor`
- [x] `Test Generation`
- [x] `Code Review`
- [x] `Database Issue`
- [x] `Logging Refactor`
- [x] `UI Mockup`

### Upgrade goals for each form

- [x] Replace avoidable free-text fields with structured controls.
- [x] Add feature-aware suggestions.
- [x] Prefill likely files and verification steps.
- [x] Rewrite visible wording in simpler plain English.
- [x] Keep detailed final output.

## Phase 3 - Module_Receiving Depth Pass

- [x] Expand metadata for `Mode Selection`.
- [x] Expand metadata for `PO Entry`.
- [x] Expand metadata for `Load Entry`.
- [x] Expand metadata for `Weight / Quantity Entry`.
- [x] Expand metadata for `Heat / Lot Entry`.
- [x] Expand metadata for `Package Type Selection`.
- [x] Expand metadata for `Review & Submit`.
- [x] Expand metadata for `Manual Entry`.
- [x] Expand metadata for `Edit Grid`.
- [x] Expand metadata for `Save Changes`.

## Phase 4 - Output Quality

- [x] Improve Markdown output structure for `UI Change`.
- [x] Improve Markdown output structure for `Debugging`.
- [x] Improve Markdown output structure for `Logic Correction`.
- [x] Add clearer human summary plus detailed machine-ready request.
- [x] Make Noob Mode output wording more natural without losing detail.

## Phase 5 - Browser Validation And Polish

- [x] Test each upgraded form live in the browser.
- [x] Test feature switching and sub-feature switching.
- [x] Test no-feature empty states.
- [x] Test generated Markdown output quality.
- [x] Test generated JSON output quality.
- [x] Fix any confusing wording or stale selections.
- [x] Fix harmless console issues if they are worth cleaning up.

## Phase 6 - Cleanup

- [x] Add documentation for the split metadata file structure.
- [x] Keep naming consistent across all metadata files.
- [x] Ensure new metadata files stay easy to extend.
- [x] Remove duplicated data from the main config after migration is complete.

## Suggested Order From Here

1. Wire the new split metadata file approach into the runtime.
2. Finish `Module_Receiving` metadata depth.
3. Split the other analyzed modules.
4. Upgrade the remaining high-value forms.
5. Improve final output quality.
6. Run a full browser validation pass.

# Material Availability Board - Implementation Prompt

## Identity

You are GitHub Copilot acting as a senior WinUI 3, .NET 10, and MVVM engineer working in the MTM Receiving Application.

You must:

- Follow the existing MTM architecture and coding conventions.
- Preserve the View -> ViewModel -> Service -> DAO -> Database flow.
- Keep Infor Visual access read-only.
- Extend existing functionality instead of replacing stable behavior unnecessarily.
- Plan the implementation so each file is edited only once during the implementation run.

## Personas

### Primary End-User Persona

Shipping / Receiving Clerk:

- Searches by warehouse location or part number.
- Uses the board to understand on-hand material, incoming supply, and associated demand.
- Needs a print-friendly transaction sheet with maximum writable space so handwritten moves can be captured while physically moving coils.

### Secondary End-User Persona

Inventory / Material Coordinator:

- Uses the handwritten transaction sheet after physical movement to enter final transfers into Infor Visual.
- Needs each coil movement to support its own destination location because coils from the same part may be split across different destinations.

### Implementation Persona

Maintainer / Developer:

- Must normalize work-order formatting and search behavior.
- Must preserve existing summary printing while extending the print flow for the new transaction sheet.
- Must organize changes so each touched file is updated once, with all planned edits for that file completed in a single pass.

## Scope

Implement the following changes in the Material Availability Board:

1. When printing from a Warehouse Location search, add a user choice between:
   - Full Summary: keep the existing summary print behavior.
   - Transaction Sheet: add a second print layout intended for handwritten inventory transfers.

2. The Transaction Sheet print layout must:
   - Target an 8.5 x 11 printable page layout.
   - Repeat the column headers on each page if content spans multiple pages.
   - Include these main columns:
     - Part Number: each material ID in the selected location.
     - Taken From: the currently entered warehouse location.
   - Under each material row, include 3 blank sub-rows.
   - Do not use one shared `Take To` field for the whole material row, because different coils may be moved to different locations.
   - Each of those 3 blank sub-rows must contain 5 handwritten quantity entry cells for individual coils.
   - Each individual coil quantity entry must have its own corresponding `Take To` field so the destination can be recorded separately for each coil.
   - Use very small margins and padding so the page leaves as much writable area as possible.

3. For the existing Full Summary HTML printout:
   - Add a page break after each material's main card so every material starts on a new printed page.

4. In both the UI and the summary printout:
   - Associated parts must appear even when there is no scheduled job.
   - Do not hide associated parts just because a work order is not currently scheduled.

5. Fix work-order formatting everywhere relevant:
   - Remove duplicate `WO-` prefixes.
   - Work orders must display in normalized form, for example `WO-070352`.
   - They must never display as `WO-WO-070352` or with repeated prefixes.

6. Fix work-order search logic:
   - Normalize work-order values before searching or matching.
   - Searching must not fail because the source or displayed value contains duplicated `WO-` prefixes.
   - Ensure the effective search value is always the normalized single-prefix version.

## Single-Touch Implementation Rule

During implementation, each file must be edited once.

This means:

- Do all read-only analysis first.
- Build a complete per-file change plan before editing begins.
- If a file is expected to serve multiple concerns, consolidate all intended changes for that file into one edit.
- Do not return later to the same file for follow-up cleanup unless absolutely unavoidable.

## Phased Implementation Plan

### Phase 0 - Read-Only Analysis and File Mapping

Before editing anything:

- Identify the exact files involved in print option selection, HTML generation, associated-part visibility, work-order formatting, and search normalization.
- Build a file-touch map so every file is assigned to one implementation phase only.
- Confirm which existing files already own:
  - location-search print flow,
  - summary HTML generation,
  - associated-part filtering,
  - work-order display formatting,
  - work-order search normalization.

### Phase 1 - Data and Normalization Foundation

Edit each foundational file once to support the downstream behavior.

Likely file types in this phase:

- Infor Visual query files if associated-part visibility or work-order normalization needs query-level support.
- DAO mapping files if work-order values or associated-part rows need normalization or additional handling.
- Data model files that represent associated-part runs or print-ready board data.

Goal:

- Ensure the service layer receives clean work-order values and associated-part records that can still appear when there is no scheduled job.

### Phase 2 - Service and Print Generation

Edit each service or print-generation file once.

Likely file types in this phase:

- Material Availability board service.
- HTML print generation helpers.
- Summary print formatting logic.
- Transaction sheet HTML generation logic.
- Shared work-order normalization helpers if they belong in service-level code.

Goal:

- Add the Full Summary vs Transaction Sheet print branching.
- Generate the new transaction sheet layout.
- Add summary page breaks.
- Preserve associated parts in UI-facing and print-facing data.
- Normalize work orders consistently for display and search.

### Phase 3 - UI and ViewModel Flow

Edit each UI-facing file once.

Likely file types in this phase:

- Material Availability board viewmodel.
- Material Availability board XAML.
- Board code-behind if needed for print option selection.
- Any related dialog or print-option picker files if they are required.

Goal:

- Present the print choice cleanly when the current search is based on Warehouse Location.
- Preserve existing summary behavior for non-location scenarios unless the requirements explicitly change it.
- Ensure associated parts are visible in the board UI even when no job is scheduled.

### Phase 4 - Tests

Edit each test file once.

Add or update tests for:

- print option branching,
- transaction sheet layout generation,
- summary page-break behavior,
- associated-part visibility without scheduled jobs,
- work-order normalization,
- work-order search behavior with duplicate prefixes.

Goal:

- Cover all new behavior and prevent regressions in summary printing, search behavior, and associated-part display.

### Phase 5 - Final Verification

Do not edit files in this phase unless a blocker is discovered that makes a single corrective pass unavoidable.

Validate all of the following:

- Warehouse Location print flow offers both print options.
- The Transaction Sheet renders with repeating headers and maximum practical writable space.
- The Full Summary inserts a page break after each main material card.
- Associated parts show in both the UI and summary output even without a scheduled job.
- Work orders render with only one `WO-` prefix in the UI and print output.
- Search and matching still work when source data contains duplicated `WO-` prefixes.
- Build and tests pass after the changes.

## Implementation Expectations

- Keep changes consistent with the existing Material Availability Board architecture.
- Preserve current behavior outside the requested scope.
- Extend the existing print infrastructure rather than replacing it.
- Update any related UI, viewmodels, print formatting, normalization helpers, and tests as needed.
- Prefer the smallest coherent implementation that satisfies the full request.

## Validation Checklist

1. Warehouse Location print flow offers both print options.
2. The Transaction Sheet renders with repeating headers and maximum practical writable space.
3. The Full Summary inserts a page break after each main material card.
4. Associated parts show in both the UI and summary output even without a scheduled job.
5. Work orders render with only one `WO-` prefix in the UI and print output.
6. Search and matching still work when source data contains duplicated `WO-` prefixes.
7. Build and tests pass after the changes.

Update the Material Availability Board print behavior, associated-parts visibility, and work-order formatting/search normalization.

## Scope

Implement the following changes in the Material Availability Board:

1. When printing from a Warehouse Location search, add a user choice between:
   - Full Summary: keep the existing summary print behavior.
   - Transaction Sheet: add a second print layout intended for handwritten inventory transfers.

2. The Transaction Sheet print layout must:
   - Target an 8.5 x 11 printable page layout.
   - Repeat the column headers on each page if content spans multiple pages.
   - Include these main columns:
     - Part Number: each material ID in the selected location.
     - Taken From: the currently entered warehouse location.
   - Under each material row, include 3 blank sub-rows.
   - Do not use one shared `Take To` field for the whole material row, because different coils may be moved to different locations.
   - Each of those 3 blank sub-rows must contain 5 handwritten quantity entry cells for individual coils.
   - Each individual coil quantity entry must have its own corresponding `Take To` field so the destination can be recorded separately for each coil.
   - Use very small margins and padding so the page leaves as much writable area as possible.

3. For the existing Full Summary HTML printout:
   - Add a page break after each material's main card so every material starts on a new printed page.

4. In both the UI and the summary printout:
   - Associated parts must appear even when there is no scheduled job.
   - Do not hide associated parts just because a work order is not currently scheduled.

5. Fix work-order formatting everywhere relevant:
   - Remove duplicate `WO-` prefixes.
   - Work orders must display in normalized form, for example `WO-070352`.
   - They must never display as `WO-WO-070352` or with repeated prefixes.

6. Fix work-order search logic:
   - Normalize work-order values before searching or matching.
   - Searching must not fail because the source or displayed value contains duplicated `WO-` prefixes.
   - Ensure the effective search value is always the normalized single-prefix version.

## Implementation Expectations

- Keep changes consistent with the existing Material Availability Board architecture.
- Preserve current behavior outside the requested scope.
- Extend the existing print infrastructure rather than replacing it.
- Update any related UI, viewmodels, print formatting, normalization helpers, and tests as needed.

## Validation

Verify all of the following after implementation:

1. Warehouse Location print flow offers both print options.
2. The Transaction Sheet renders with repeating headers and maximum practical writable space.
3. The Full Summary inserts a page break after each main material card.
4. Associated parts show in both the UI and summary output even without a scheduled job.
5. Work orders render with only one `WO-` prefix in the UI and print output.
6. Search and matching still work when source data contains duplicated `WO-` prefixes.
7. Build and tests pass after the changes.

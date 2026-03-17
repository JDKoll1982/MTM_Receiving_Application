# Reporting Refactor Audit

Last Updated: 2026-03-15

## Purpose

This document captures the current state of `Module_Reporting`, identifies where it no longer matches the current Receiving, Dunnage, and Volvo modules, and lists the changes needed to refactor it safely.

The goal is to give the next implementation pass a practical roadmap:

- file by file
- workflow by workflow
- ordered by difficulty

## Scope Reviewed

### Reporting module files

- `Module_Reporting/Contracts/IService_Reporting.cs`
- `Module_Reporting/Data/Dao_Reporting.cs`
- `Module_Reporting/Services/Service_Reporting.cs`
- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- `Module_Reporting/Views/View_Reporting_Main.xaml`
- `Module_Reporting/Views/View_Reporting_Main.xaml.cs`
- `Module_Reporting/README.md`
- `Module_Reporting/SETTABLE_OBJECTS_REPORT.md`

### Reporting data/model dependencies

- `Module_Core/Models/Reporting/Model_ReportRow.cs`
- `Database/Schemas/33_View_dunnage_history.sql`
- `Database/Schemas/34_View_receiving_history.sql`
- `Database/Schemas/36_View_volvo_history.sql`

### Upstream workflows compared against reporting assumptions

- Receiving queue/history lifecycle
- Dunnage history lifecycle
- Volvo active/history lifecycle

## Executive Summary

`Module_Reporting` needs a major refactor because it still reflects an older cross-module contract that no longer matches the current schema and workflow behavior.

The main problems are:

1. `Dao_Reporting.cs` assumes columns that do not exist in the current reporting views, especially for Receiving.
2. `Service_Reporting.cs` only formats Receiving and Dunnage correctly; Volvo falls through to a generic/default email layout.
3. `ViewModel_Reporting_Main.cs` and `View_Reporting_Main.xaml` still represent an earlier, simpler reporting workflow and do not expose enough module-specific context for current data shapes.
4. The module documentation is stale and references a Routing workflow that is not actually implemented in the live reporting module.

## Current Reporting Workflows

### Workflow 1: Availability Check

Current path:

- User selects date range in `View_Reporting_Main.xaml`
- `ViewModel_Reporting_Main.CheckAvailabilityAsync()` calls `IService_Reporting.CheckAvailabilityAsync()`
- `Service_Reporting.CheckAvailabilityAsync()` delegates to `Dao_Reporting.CheckAvailabilityAsync()`
- DAO counts rows in `view_receiving_history`, `view_dunnage_history`, and hardcodes Volvo to `0`

Current issues:

- Volvo availability is not actually queried even though `view_volvo_history` exists.
- The reporting page can under-report current module readiness.
- Checkbox enablement logic is only as accurate as the DAO count logic.

### Workflow 2: Generate Receiving Report

Current path:

- `GenerateReceivingReportCommand`
- `ViewModel_Reporting_Main.GenerateReceivingReportAsync()`
- `Service_Reporting.GetReceivingHistoryAsync()`
- `Dao_Reporting.GetReceivingHistoryAsync()`
- Query against `view_receiving_history`

Current issues:

- DAO expects `part_number`, `weight_lbs`, `heat_lot_number`, `created_date`, and `source_module`
- current view provides `part_id`, `heat`, `transaction_date`, `created_at`, and `source`
- this is a direct runtime mismatch and is the most immediate blocker in the module

### Workflow 3: Generate Dunnage Report

Current path:

- `GenerateDunnageReportCommand`
- `ViewModel_Reporting_Main.GenerateDunnageReportAsync()`
- `Service_Reporting.GetDunnageHistoryAsync()`
- `Dao_Reporting.GetDunnageHistoryAsync()`
- Query against `view_dunnage_history`

Current issues:

- This path is closer to valid than Receiving.
- The view aliases `created_by` to `employee_number`, but semantically that value is a username string, not a normalized employee number.
- Reporting currently collapses that distinction and may mislead downstream consumers.

### Workflow 4: Generate Volvo Report

Current path:

- `GenerateVolvoReportCommand`
- `ViewModel_Reporting_Main.GenerateVolvoReportAsync()`
- `Service_Reporting.GetVolvoHistoryAsync()`
- `Dao_Reporting.GetVolvoHistoryAsync()`
- Query against `view_volvo_history`

Current issues:

- The DAO query is closer to the live view than the other modules.
- Availability check still reports Volvo as `0`, so the UI path is inconsistent.
- Email formatting has no Volvo-specific case, so the generated email output is effectively incomplete.

### Workflow 5: Copy Email Format

Current path:

- `CopyEmailFormatCommand`
- `ViewModel_Reporting_Main.CopyEmailFormatAsync()`
- `Service_Reporting.FormatForEmailAsync()`

Current issues:

- Only Receiving and Dunnage have explicit layouts.
- Volvo uses the generic default branch.
- Module-specific columns are not centrally defined, so any upstream model change requires hand-edits in string-built HTML.

## File-by-File Change List

## Low Difficulty

### `Module_Reporting/Views/View_Reporting_Main.xaml`

Change needed:

- Replace the current generic `ListView` presentation with a module-aware layout or templated views.

Why:

- The current UI only shows `CreatedDate`, `PartNumber`, and `PONumber`.
- That is not enough for modern Receiving, Dunnage, or Volvo reporting workflows.

Suggested refactor:

- Create module-specific data templates or a dynamic column presentation.
- Show module-relevant fields instead of a minimal three-field list.

### `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`

Change needed:

- Fix availability logic so Volvo is actually counted from the live view.
- Re-evaluate checkbox/button enablement after each report load and date change.

Why:

- Current availability logic is stale.
- The view model still assumes a simpler module readiness model than the current modules provide.

Suggested refactor:

- Stop hardcoding Volvo availability.
- Add a clearer distinction between `no rows found` and `query failed`.

### `Module_Reporting/README.md`

Change needed:

- Rewrite the README to match the live implementation.

Why:

- It still describes Routing and earlier implementation assumptions.
- It no longer matches the actual file structure, live database artifacts, or supported workflows.

Suggested refactor:

- Document only currently-supported modules.
- Add a section for known gaps and pending refactor items.

### `Module_Reporting/SETTABLE_OBJECTS_REPORT.md`

Change needed:

- Remove or clearly mark stale Routing references.

Why:

- This file currently implies a broader reporting surface than what the module actually ships.

## Medium Difficulty

### `Module_Reporting/Services/Service_Reporting.cs`

Change needed:

- Add a Volvo-specific email formatter branch.
- Rework HTML formatting so layout definitions are not embedded inline per case.

Why:

- Volvo currently falls through to the default branch.
- The email output layer is brittle and tightly coupled to `Model_ReportRow` assumptions.

Suggested refactor:

- Extract module-specific email layouts into dedicated formatter methods.
- Consider a per-module definition object for headers, cell selectors, and display labels.

### `Module_Reporting/Contracts/IService_Reporting.cs`

Change needed:

- Revisit the contract after the DAO/view changes are complete.

Why:

- The interface itself compiles, but it still describes a reporting shape that hides meaningful module differences.
- A future refactor may need richer typed DTOs instead of one flattened row model.

Suggested refactor:

- Keep the current contract during stabilization.
- After data correctness is restored, evaluate whether the interface should split into per-module report result types.

### `Module_Core/Models/Reporting/Model_ReportRow.cs`

Change needed:

- Reconcile field names and semantics with the actual views.

Why:

- The model tries to be a universal row for all modules.
- That is workable only if all backing views expose consistent meanings for the same property names.
- Right now they do not.

Suggested refactor:

- Either standardize the views to this model, or replace the model with per-module DTOs plus a presentation adapter.

### `Database/Schemas/33_View_dunnage_history.sql`

Change needed:

- Decide whether `employee_number` should remain a username alias or become a real normalized employee field.

Why:

- The alias works syntactically but is semantically misleading.

Suggested refactor:

- Rename the projected field if the module should surface usernames.
- Or join to a user table if true employee identity is required.

### `Database/Schemas/36_View_volvo_history.sql`

Change needed:

- Verify whether this view should remain a single flat shipment summary or also expose line-level report detail.

Why:

- Current reporting only gets shipment-level counts.
- If the business now expects more detailed Volvo reporting, this view is too thin.

Suggested refactor:

- Keep the summary view for availability and high-level reports.
- Add a separate detailed reporting view if line-level reporting is needed.

## High Difficulty

### `Module_Reporting/Data/Dao_Reporting.cs`

Change needed:

- Rewrite the DAO queries to match the live views or replace the views so they match the DAO contract.

Why:

- Receiving is currently broken by direct column mismatch.
- Availability check is incomplete.
- The DAO is the primary execution-time failure point in this module.

Suggested refactor:

- Decide one source of truth: DAO contract or SQL view contract.
- Then align all three report queries to that contract.
- Add integration coverage for each query against deployed views.

### `Database/Schemas/34_View_receiving_history.sql`

Change needed:

- Rebuild the Receiving view to match modern Receiving lifecycle expectations.

Why:

- Receiving has changed the most.
- The current view still exposes legacy naming and does not project the fields the reporting DAO expects.
- The queue/history lifecycle in Receiving has evolved, but reporting has not evolved with it.

Suggested refactor:

- Standardize the output column names used by reporting.
- Explicitly decide whether weight belongs in this report.
- Explicitly decide whether the source should be `Receiving`, `history`, or `label_data`.
- Align date semantics: `transaction_date` vs. `created_at` vs. reporting-created date.

### `Module_Reporting` overall architecture

Change needed:

- Decide whether this module stays unified or is refactored into module-specific report pipelines behind a shared shell.

Why:

- Receiving, Dunnage, and Volvo are no longer similar enough for a shallow shared reporting layer.
- The single flat `Model_ReportRow` abstraction is showing strain.

Suggested refactor:

- Keep one reporting page if the business wants one entry point.
- Internally split logic into per-module query/result/formatter paths.
- Use the shared shell only for date selection, module selection, and email/export orchestration.

## Workflow-by-Workflow Refactor Plan

### Workflow A: Availability and Module Enablement

Difficulty: Low

Changes needed:

1. Query all live module views, including Volvo.
2. Distinguish `0 rows` from `query error` in the ViewModel status messages.
3. Re-enable or disable module actions based on live counts, not placeholders.

### Workflow B: Receiving Reporting

Difficulty: High

Changes needed:

1. Reconcile view column names with DAO expectations.
2. Decide whether Receiving reporting is transaction-based, created-at-based, or queue/history hybrid.
3. Decide whether weight belongs in the report.
4. Validate PO normalization against current Receiving data realities.

### Workflow C: Dunnage Reporting

Difficulty: Medium

Changes needed:

1. Confirm semantic meaning of `employee_number` in dunnage reporting.
2. Validate spec concatenation output with current dunnage product/spec patterns.
3. Verify that the report shape still matches what operations expects to email or review.

### Workflow D: Volvo Reporting

Difficulty: Medium

Changes needed:

1. Fix availability counts.
2. Add a Volvo-specific email layout.
3. Decide if shipment summary is enough or if line-level detail is now required.
4. Align report semantics with the current Volvo queue/history flow.

### Workflow E: Email / Copy Workflow

Difficulty: Medium

Changes needed:

1. Extract module-specific layouts out of a single switch.
2. Centralize headers, value selectors, and formatting rules.
3. Add explicit formatting support for Volvo.
4. Remove any stale Routing assumptions unless Routing is being restored.

## Recommended Implementation Order

### Phase 1: Stabilize Execution

Difficulty: Low to Medium

1. Fix `Dao_Reporting.CheckAvailabilityAsync()` for Volvo.
2. Fix `Service_Reporting.FormatForEmailAsync()` for Volvo.
3. Update the README and remove stale Routing claims.
4. Improve `View_Reporting_Main.xaml` to show richer module data.

### Phase 2: Repair Data Contracts

Difficulty: High

1. Refactor `view_receiving_history` and `Dao_Reporting.GetReceivingHistoryAsync()` together.
2. Validate Dunnage field semantics.
3. Confirm Volvo report depth requirements.

### Phase 3: Reshape the Module

Difficulty: High

1. Decide whether to keep `Model_ReportRow` as the single shared shape.
2. If not, split reporting into per-module DTOs and formatters.
3. Keep one reporting shell page, but move data logic to module-specific pipelines.

## Open Questions To Resolve Before Refactor Starts

1. Should Receiving reports include weight from current Receiving data, or should weight be removed from the reporting contract?
2. Is Routing reporting still a real requirement, or should all Routing references be removed from this module?
3. For Dunnage, should the report show username, employee number, or both?
4. For Volvo, is shipment-level summary enough, or do users now need line-level detail in the reporting UI and email output?
5. Should reporting continue to use raw SQL against views, or should it move to stored procedures for maintainability and versioning consistency?

## Clipboard And Outlook Formatting Research

This section documents the research findings for making report output paste into Outlook the way copied spreadsheet cells do today.

### Current problem

The current reporting module does not create a proper rich clipboard payload for Outlook-style paste.

Current implementation behavior in `ViewModel_Reporting_Main.CopyEmailFormatAsync()`:

- builds a raw HTML string
- passes that raw HTML directly to `DataPackage.SetHtmlFormat(...)`
- also passes the same raw HTML string to `DataPackage.SetText(...)`
- sets clipboard content

This is likely why paste targets appear to receive raw text or weakly formatted content instead of a spreadsheet-like table.

### Verified research findings

From Microsoft documentation:

1. `Windows.ApplicationModel.DataTransfer.DataPackage.SetHtmlFormat` should be given content prepared by `HtmlFormatHelper.CreateHtmlFormat(...)`, not a raw HTML string.
2. Windows rich HTML clipboard paste uses the `HTML Format` / CF_HTML payload structure.
3. CF_HTML requires fragment metadata, including fragment boundaries and offsets.
4. `HtmlFormatHelper.CreateHtmlFormat(...)` exists specifically to generate the required wrapping and metadata for clipboard/share operations.
5. `Clipboard.SetContent(...)` writes the `DataPackage` to the clipboard, but the richness of paste depends on the formats actually populated in that package.
6. If the plain-text fallback is just the raw HTML string, paste behavior in fallback targets is poor and confusing.

### Practical implication for this module

If Outlook-like paste is the goal, then the reporting module must stop treating “HTML string” and “HTML clipboard format” as the same thing.

The module needs to generate:

- a spreadsheet-like HTML table fragment
- a CF_HTML wrapped version of that fragment
- a true plain-text fallback

### Recommended implementation plan

#### Step 1: Separate clipboard formatting from report formatting

Create a dedicated helper/service for clipboard packaging.

Suggested responsibilities:

- build rich HTML table fragment
- build plain-text fallback
- wrap HTML fragment with `HtmlFormatHelper.CreateHtmlFormat(...)`
- populate `DataPackage`

Suggested shape:

- `ReportingClipboardFormatter`
- or `Service_ReportingClipboard`

#### Step 2: Build Outlook-friendly HTML table markup

The HTML fragment should be optimized for paste, not browser rendering.

Use:

- `<table>` with `border-collapse: collapse;`
- inline styles only
- explicit borders on `table`, `th`, and `td`
- explicit padding
- explicit font family and font size
- explicit header background color
- alternating row colors
- explicit widths on columns or `<colgroup>`
- explicit text alignment per column where needed

The resulting fragment should resemble copied worksheet cells rather than a generic email body.

#### Step 3: Wrap the fragment correctly

Implementation requirement:

- build the HTML fragment first
- call `HtmlFormatHelper.CreateHtmlFormat(htmlFragment)`
- pass the wrapped result into `DataPackage.SetHtmlFormat(...)`

This is the key change required for rich clipboard paste behavior.

#### Step 4: Provide a real plain-text fallback

Do not send the raw HTML string to `SetText(...)`.

Instead, generate a real text fallback such as:

- tab-separated values for quick readability
- line-by-line textual summary for plain editors

That preserves usability in non-HTML paste targets without degrading the HTML clipboard path.

#### Step 5: Validate against real targets

Validation targets should include:

- Outlook desktop
- New Outlook
- Word
- Notepad/plain editor

Validation criteria:

- headers preserve bold/background color
- borders render clearly
- column widths look intentional
- row spacing feels spreadsheet-like
- plain-text fallback remains readable

### Difficulty assessment

Clipboard refactor difficulty: Medium to High

Reason:

- the code change itself is moderate
- the real complexity is getting Outlook paste behavior stable and visually acceptable across modules

## Additional Cleanup Required During Reporting Refactor

The reporting refactor should include cleanup work, not just data-contract repairs.

### Remove stale spreadsheet/export terminology

Current stale references exist in:

- `Module_Reporting/README.md`
- `Module_Reporting/SETTABLE_OBJECTS_REPORT.md`

Cleanup required:

- remove CSV/XLS/XLSX/XLS references that imply file export workflows
- remove “network CSV write” language from reporting docs
- replace that language with clipboard-based formatted-table output terminology

### Remove dead or misleading report-output assumptions

Cleanup required:

- remove any remaining text that implies spreadsheet file generation is part of reporting
- make it explicit that the current desired output is formatted clipboard content for Outlook paste

### Remove stale UI text

Potential UI text cleanup:

- `Copy Email Format`

Possible replacements after refactor:

- `Copy Formatted Table`
- `Copy Outlook Table`
- `Copy Formatted Report`

The final label should match the actual user action instead of implying mail-send behavior.

### Remove comment noise and stale notes

The user requested that notes above methods and inside methods be cleaned out during refactor.

That cleanup should focus on:

- removing comments that restate obvious behavior
- removing stale implementation notes tied to old spreadsheet assumptions
- keeping only comments that explain non-obvious business rules or clipboard-format constraints

Primary cleanup targets:

- `Module_Reporting/Data/Dao_Reporting.cs`
- `Module_Reporting/Services/Service_Reporting.cs`
- `Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs`
- `Module_Reporting/Views/View_Reporting_Main.xaml.cs`

### Remove unused code paths

Current audit result:

- no active CSV/XLS/XLSX generation code was found inside `Module_Reporting`
- the stale footprint is currently mostly documentation, naming, expectations, and clipboard behavior rather than live export code

Refactor expectation:

- if any report-export helper or leftover spreadsheet path is found during implementation, remove it instead of preserving parallel output strategies

## Expanded Recommended Implementation Order

### Phase 1A: Reporting cleanup

1. Remove stale spreadsheet/export language from module docs.
2. Rename UI wording to reflect formatted clipboard output.
3. Remove stale comments and notes from reporting files.

### Phase 1B: Clipboard correctness

1. Introduce a dedicated reporting clipboard formatter.
2. Replace raw `SetHtmlFormat(...)` usage with `HtmlFormatHelper.CreateHtmlFormat(...)`.
3. Replace raw HTML `SetText(...)` fallback with a real plain-text representation.
4. Test paste behavior in Outlook.

### Phase 2: Reporting data repair

1. Fix Receiving view/DAO mismatch.
2. Fix Volvo availability counting.
3. Add Volvo-specific email/table formatting.

### Phase 3: Structural refactor

1. Split module-specific formatting from the shared reporting shell.
2. Revisit whether `Model_ReportRow` should survive as the shared shape.
3. Finish documentation rewrite after code stabilization.

## Added Open Questions

6. Should the clipboard output target Outlook desktop first, with New Outlook as best-effort, or must both have identical output fidelity?
7. Should the final plain-text fallback be tab-separated or narrative text?
8. Should the reporting module still use the word `Email` anywhere in the UI if the primary workflow is actually clipboard-based rich table paste?

## Recommended Deliverable Split

To reduce risk, the reporting refactor should not be done as one large rewrite. It should be split into:

1. Data-contract repair
2. UI/report-output modernization
3. Architecture cleanup and documentation

That sequence gives the team a working report module sooner while reducing the chance of breaking all three report paths at once.
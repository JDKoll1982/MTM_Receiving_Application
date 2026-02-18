# Spreadsheet Logic Removal Plan (CSV/XLS/XLSX)

Last Updated: 2026-02-18

## Comprehensive Task List (Phases and Sub-Phases)

### Phase 1 - Discovery and Inventory

#### 1.1 Search Baseline (Executable Code Only)

- [ ] Run case-insensitive searches for `.csv`, `.xls`, `.xlsx`, `csv`, `excel`, `spreadsheet`, `labelview`.
- [ ] Run library/API searches for `EPPlus`, `ClosedXML`, `NPOI`, `ExcelDataReader`, `CsvHelper`, `OleDb`, `TextFieldParser`.
- [ ] Capture file I/O usage associated with spreadsheet workflows (`OpenFileDialog`, `SaveFileDialog`, `StreamReader`, `StreamWriter`, `File.*`).

#### 1.2 Inventory Artifacts

- [ ] Build a spreadsheet-reference inventory table with columns: project, file, symbol/method, layer, workflow, action.
- [ ] Mark each entry as `Remove`, `ReplaceWithPlaceholder`, or `Keep (non-executable/docs-only)`.
- [ ] Validate no app/test executable path is missing from inventory.

#### 1.3 Dependency/Call-Chain Mapping

- [ ] Map each workflow chain: `UI -> ViewModel -> Service -> Helper/Utility -> DAO`.
- [ ] Identify shared components reused across modules.
- [ ] Tag high-risk chains where one change affects multiple modules.

### Phase 2 - Placeholder Contract and Standards

#### 2.1 User-Facing Placeholder Standard

- [ ] Standardize UI placeholder text: **"Not implemented yet: spreadsheet workflow is being replaced by MySQL."**
- [ ] Define where and how this is shown (status message, dialog, notification pattern per existing UX).
- [ ] Ensure placeholder flow does not execute any file I/O.

#### 2.2 Service-Level Placeholder Contract

- [ ] Standardize temporary service return behavior (project-standard result object and message).
- [ ] Keep method signatures stable where possible.
- [ ] Add structured logging entry for placeholder invocation.

#### 2.3 Shared Placeholder Utility (Optional)

- [ ] Decide if a shared helper/service is needed for consistency.
- [ ] If used, implement once and route all temporary spreadsheet paths through it.
- [ ] Verify no duplicate placeholder logic remains.

### Phase 3 - Refactor Application Code (Module-by-Module)

#### 3.1 Module Execution Order

- [ ] Execute in order: shared/core utilities first, then module-specific flows.
- [ ] Freeze module scope before edits (avoid cross-module drift).
- [ ] Complete and validate one module before moving to the next.

#### 3.2 UI Layer Refactor

- [ ] Replace UI command handlers that trigger spreadsheet import/export/generation with placeholder calls.
- [ ] Remove direct file picker/file path operations tied to spreadsheet workflows.
- [ ] Keep UI navigation and screen structure intact unless disabling is required.

#### 3.3 ViewModel and Service Refactor

- [ ] Remove spreadsheet logic branches and dispatch to placeholder path.
- [ ] Keep async command/service flow stable to avoid UI regression.
- [ ] Preserve error handling patterns and logging conventions.

#### 3.4 Helper/Infrastructure Refactor

- [ ] Remove executable use of spreadsheet utilities.
- [ ] Convert required legacy interfaces to temporary stubs where necessary.
- [ ] Ensure no reachable code path attempts spreadsheet reads/writes.

#### 3.5 Data Layer Safeguard

- [ ] Verify DAO layer contains no spreadsheet file logic.
- [ ] Add TODO anchors for future MySQL table-backed replacement points.
- [ ] Confirm no accidental SQL Server write behavior is introduced.

### Phase 4 - Refactor Test Projects

#### 4.1 Test Inventory and Categorization

- [ ] Identify tests directly validating CSV/XLS/XLSX generation/import.
- [ ] Classify each as `Replace`, `Retire`, or `Rewrite for Placeholder`.
- [ ] Document rationale for removed/retired tests.

#### 4.2 Test Updates

- [ ] Replace spreadsheet behavior assertions with placeholder assertions.
- [ ] Add/adjust tests to confirm no file artifacts are produced.
- [ ] Keep contract-level behavior covered for impacted commands/services.

#### 4.3 Test Stability

- [ ] Ensure test setup/fixtures no longer depend on spreadsheet fixtures or paths.
- [ ] Remove obsolete test data files for spreadsheet flows if no longer needed.
- [ ] Verify updated tests remain deterministic.

### Phase 5 - Dependency and Configuration Cleanup

#### 5.1 Package Cleanup

- [ ] Remove unused spreadsheet NuGet dependencies from app/test projects.
- [ ] Remove transitive package references no longer needed.
- [ ] Rebuild immediately after package cleanup to catch hidden dependencies.

#### 5.2 Config/Constants Cleanup

- [ ] Remove settings/constants exclusively used by spreadsheet workflows.
- [ ] Remove dead feature flags related to spreadsheet output paths.
- [ ] Keep placeholders or configuration hooks needed for future MySQL migration.

### Phase 6 - Validation Gates

#### 6.1 Build/Compile Gate

- [ ] Build full solution and resolve all errors from spreadsheet logic removal.
- [ ] Validate affected projects compile cleanly.
- [ ] Confirm no new warnings indicate unreachable placeholder regressions.

#### 6.2 Test Gate

- [ ] Run impacted tests and resolve failures caused by migration.
- [ ] Confirm placeholder behavior is covered by automated tests.
- [ ] Document any intentionally skipped tests with reason.

#### 6.3 Search Gate (Final)

- [ ] Re-run executable-scope search for spreadsheet extensions, libraries, and APIs.
- [ ] Confirm remaining hits are non-executable (docs/scripts) or explicitly approved exceptions.
- [ ] Produce a final search-gate summary for merge readiness.

### Phase 7 - Handoff and Future Migration Anchors

#### 7.1 Removal Completion Report

- [ ] Publish final inventory + resolution status for each item.
- [ ] Summarize module-by-module outcomes and residual risks.
- [ ] Confirm all UI spreadsheet entry points now route to placeholders.

#### 7.2 MySQL Migration Readiness

- [ ] List per-module replacement targets for MySQL table implementation.
- [ ] Identify required future schema/service/DAO tasks by module.
- [ ] Keep this plan as baseline for next-phase DB replacement execution.

## Objective

Remove all CSV/XLS/XLSX-related logic from executable code across the solution (application + test projects), and replace any UI-driven spreadsheet workflows with temporary placeholders while we transition to MySQL table-backed storage per module.

## Scope and Constraints

- In scope:
	- Application code paths in the main app and module projects.
	- Test code paths in test projects.
	- UI event flows that call spreadsheet logic.
- Out of scope for this phase:
	- Documentation-only references (`*.md`) unless they block build/test.
	- Scripts that are not part of app/test execution path.
- Temporary behavior standard:
	- UI calls that previously triggered spreadsheet behavior must show a user-visible placeholder message: **"Not implemented yet: spreadsheet workflow is being replaced by MySQL."**

## Success Criteria

1. No executable code references spreadsheet file formats (`.csv`, `.xls`, `.xlsx`) for read/write/import/export logic.
2. No direct usage of spreadsheet libraries in app/test executable paths.
3. All UI entry points that formerly invoked spreadsheet logic route to temporary placeholders.
4. Solution builds successfully.
5. Tests compile and pass for impacted areas (or are explicitly updated/skipped with rationale if they depended on removed spreadsheet flows).

## Phase 1 - Discovery and Inventory

### 1.1 Code Inventory Search

Create a complete inventory of executable references using case-insensitive searches for:

- Extensions and terms:
	- `.csv`, `.xls`, `.xlsx`, `csv`, `excel`, `spreadsheet`, `labelview`
- Typical APIs/libraries:
	- `EPPlus`, `ClosedXML`, `NPOI`, `ExcelDataReader`, `CsvHelper`, `OleDb`, `TextFieldParser`
- File APIs and hardcoded names:
	- `File.Open*`, `StreamWriter`, `StreamReader`, `SaveFileDialog`, `OpenFileDialog` in suspected export/import areas.

Output artifact:

- A categorized inventory table (file, symbol/method, project, type: UI/service/DAO/test, action: remove/replace).

### 1.2 Dependency Mapping

For each inventory hit, classify:

- UI trigger location (View/ViewModel/command).
- Service boundary currently handling spreadsheet behavior.
- Any downstream helpers/utilities.
- Test coverage tied to that behavior.

Output artifact:

- Call chain map for each workflow (UI -> ViewModel -> Service -> utility/helper).

## Phase 2 - Temporary Placeholder Contract

### 2.1 Define Placeholder Response Standard

Create a single temporary response contract for spreadsheet-replaced actions:

- For service methods: return existing project-standard failure/pending result object (e.g., `Model_Dao_Result`) with explicit message.
- For UI commands: show user-visible placeholder message and do not attempt file operations.
- For logging: log as informational warning with method/module name.

### 2.2 Add Centralized Placeholder Helper (if beneficial)

If repeated in multiple modules, define one shared helper/service method for consistent placeholder messaging and avoid duplication.

## Phase 3 - Refactor by Layer (Module-by-Module)

> Execute module-by-module to reduce risk. Recommended order: shared/core utilities first, then each functional module.

### 3.1 UI Layer Changes

- Locate all UI commands/buttons/menu actions that trigger spreadsheet import/export/generation.
- Replace command handlers to call placeholder flow only.
- Keep UI intact where possible; disable only if necessary.

### 3.2 ViewModel/Service Changes

- Remove spreadsheet logic branches from ViewModels and Services.
- Replace method bodies with temporary placeholder result handling.
- Preserve method signatures where feasible to avoid cascading breaks.

### 3.3 Utility/Helper/Infrastructure Changes

- Remove spreadsheet helper classes/method implementations from executable usage.
- For methods still referenced by current interfaces, keep stubs that return placeholder response.

### 3.4 DAO/Data Layer Preparation

- Ensure no DAO reads/writes spreadsheet files.
- Leave TODO anchors for future MySQL table integration per module (without implementing new DB logic in this phase).

## Phase 4 - Test Project Updates

### 4.1 Remove/Replace Spreadsheet-Coupled Tests

- Identify tests validating CSV/XLS/XLSX behavior and either:
	- Replace with placeholder behavior assertions, or
	- Retire tests that are no longer applicable with explicit reason in test name/comment.

### 4.2 Preserve Contract/Flow Validation

- Add/adjust tests to verify:
	- UI command/service returns placeholder result/message.
	- No file artifacts are created.
	- Logging path executes as expected (if testable).

## Phase 5 - Dependency and Package Cleanup

### 5.1 Remove Unused Spreadsheet Packages

- After code changes, remove spreadsheet-related NuGet packages that are no longer used.
- Rebuild to ensure no hidden references remain.

### 5.2 Clean Configuration and Constants

- Remove spreadsheet file path settings/constants used only by removed logic.
- Retain only configs needed for non-spreadsheet flows.

## Phase 6 - Validation and Hardening

### 6.1 Build and Compile Validation

- Build full solution and resolve compile errors caused by removed logic.

### 6.2 Regression Validation (Targeted)

- Validate each impacted UI entry point now shows the placeholder behavior.
- Validate no spreadsheet read/write paths are reachable.

### 6.3 Search-Based Final Gate

Run final executable-scope search gate for:

- `.csv`, `.xls`, `.xlsx`
- Spreadsheet library type names/usings
- File export/import symbols tied to prior spreadsheet flows

Accept only known false positives in non-executable docs/scripts.

## Module Execution Checklist

Repeat for each module/project affected:

- [ ] Inventory complete
- [ ] UI triggers identified
- [ ] Spreadsheet service logic removed/replaced with placeholder
- [ ] Utility/helper spreadsheet code disconnected or stubbed
- [ ] Tests updated
- [ ] Build succeeds
- [ ] Final search gate clear for executable code

## Risk Management

### Key Risks

1. Hidden transitive calls to spreadsheet helpers causing runtime failures.
2. UI flows silently doing nothing instead of clear placeholder messaging.
3. Tests failing due to stale assumptions around file generation.

### Mitigations

- Use call-chain mapping before edits.
- Enforce one consistent placeholder message contract.
- Update tests in same module pass as code edits.

## Deliverables

1. Spreadsheet-removal implementation PR with module-by-module commits.
2. Updated executable code with no active spreadsheet workflows.
3. Updated tests aligned to placeholder behavior.
4. Final inventory + search-gate report proving removal in app/test execution paths.

## Next Phase (Future Work - Not in This Plan)

After this removal baseline is complete, implement MySQL table-backed replacements per module:

- Define table schemas per module workflow.
- Implement DAO/service methods for DB-backed equivalents.
- Rewire UI placeholders to new DB flows.
- Introduce migration and data validation strategy.

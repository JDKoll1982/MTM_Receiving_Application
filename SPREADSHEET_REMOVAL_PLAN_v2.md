# CSV / XLS / XLSX Removal Plan — v2

> Last Updated: 2026-02-18
> Status: Draft — pending execution

---

## Comprehensive Task List

> Check each item as you complete it. Work through phases sequentially; do not begin a phase until all prior phases are marked complete.

---

### Phase 1 — Codebase Discovery

#### 1.1 — Keyword Sweep (App + Test Projects Only)

- [ ] Search for file extensions as string literals: `".csv"`, `".xls"`, `".xlsx"`
- [ ] Search for format-agnostic terms: `csv`, `excel`, `Export`, `Import`, `spreadsheet`
- [ ] Search all `*.csproj` files for spreadsheet-related `<PackageReference>` entries
- [ ] Search for NuGet library namespaces and types:
  - `EPPlus` / `OfficeOpenXml`
  - `ClosedXML`
  - `ExcelDataReader`
  - `CsvHelper` / `CsvParser`
  - `NPOI`
  - `System.Data.OleDb`
  - `TextFieldParser`
- [ ] Search for file-system APIs that exclusively serve spreadsheet workflows:
  - `OpenFileDialog`, `SaveFileDialog`
  - `File.WriteAllLines`, `File.ReadAllLines`, `File.WriteAllBytes`
  - `StreamWriter`, `StreamReader` instances adjacent to spreadsheet path variables
- [ ] Search constants and config keys for hardcoded spreadsheet file paths or folder references

#### 1.2 — Inventory Table

Capture every hit from 1.1 in a table with these columns:

| Project | File | Symbol / Method | Layer | Workflow | Action |
|---------|------|-----------------|-------|----------|--------|

- Valid `Layer` values: `View`, `ViewModel`, `Service`, `Helper`, `DAO`, `Test`
- Valid `Action` values: `Remove`, `ReplaceWithPlaceholder`, `Stub`, `RetireTest`

#### 1.3 — Per-Module Workflow Map

For each unique spreadsheet workflow found:

- [ ] Identify the originating UI element (button, menu item, keyboard shortcut)
- [ ] Trace the full call chain: `View → ViewModel command → Service method → Helper/DAO`
- [ ] Note any cross-module dependencies (e.g. a shared `Module_Core` helper used by multiple modules)
- [ ] Flag any workflow where removal would leave a broken UI control with no replacement

Output: a per-workflow call-chain summary attached to the inventory table.

#### 1.4 — Module Priority Order

Based on the inventory, establish the refactor sequence. Default order:

1. `Module_Core` / shared infrastructure
2. `Module_Shared`
3. `Module_Receiving`
4. `Module_Dunnage`
5. `Module_Reporting`
6. `Module_Volvo`
7. `Module_Settings.*`
8. `MTM_Receiving_Application.Tests`

Adjust order if inventory reveals tighter dependencies.

---

### Phase 2 — Placeholder Design

#### 2.1 — UI Placeholder Behavior

- [ ] Confirm which UX pattern to use for placeholder feedback:
  - Status bar / `StatusMessage` binding
  - `ContentDialog` or `InfoBar` notification
  - Disabled control with tooltip
- [ ] Define the standard placeholder message text:
  > _"This feature is not available yet. The spreadsheet workflow is being replaced with a MySQL-backed implementation."_
- [ ] Confirm that placeholder path performs **zero file I/O** (no partial writes, no temp files)

#### 2.2 — Service-Layer Placeholder Contract

- [ ] Use existing `Model_Dao_Result` / `Model_Dao_Result<T>` to return a pending/not-implemented state
- [ ] Return `IsSuccess = false` with a consistent pending message (not an exception)
- [ ] Keep method signatures unchanged to avoid ViewModel/test changes cascading
- [ ] Log a structured warning via `IService_LoggingUtility` on every placeholder invocation, including: caller name, module, timestamp

#### 2.3 — Centralized Placeholder Helper Decision

- [ ] If 3 or more modules share the same pattern, create a single shared placeholder helper in `Module_Core`
- [ ] If fewer than 3 modules, inline the pattern to avoid premature abstraction
- [ ] Document the decision with a comment referencing this plan

---

### Phase 3 — Application Code Refactor

> Process one module at a time. Do not move to the next module until the current module builds and its tests pass.

#### 3.1 — View Layer

- [ ] Locate all `Button`, `MenuFlyoutItem`, `AppBarButton`, or keyboard-shortcut handlers that invoke spreadsheet commands
- [ ] Wire those controls to the temporary placeholder command (or the existing command if it has been stubbed at service level)
- [ ] Do not remove the UI control itself — keep it visible but route it to the placeholder
- [ ] Remove any hardcoded file path strings from XAML resources

#### 3.2 — ViewModel Layer

- [ ] Remove code inside spreadsheet-related `[RelayCommand]` methods
- [ ] Replace removed body with: call to placeholder service path → set `StatusMessage` to placeholder text → return
- [ ] Keep the `[RelayCommand]` method and its observable properties in place
- [ ] Ensure `IsBusy` is still properly toggled (set `true` before, `false` in `finally`)

#### 3.3 — Service Layer

- [ ] Remove all CSV/XLS/XLSX generation, reading, and writing logic from service implementations
- [ ] Replace removed logic with a `Model_Dao_Result` that carries the not-implemented message
- [ ] Preserve the interface method signatures (`IService_*` contracts) — do not change them
- [ ] Add a structured log warning at the service boundary

#### 3.4 — Helper / Utility Layer

- [ ] Remove or empty the bodies of any helper methods whose sole purpose is spreadsheet formatting or I/O
- [ ] If a helper method is referenced by an interface that cannot be changed yet, leave a stub returning a pending result
- [ ] Mark each stub with:
  ```csharp
  // TODO(SpreadsheetRemoval): Replace stub with MySQL-backed implementation.
  ```
- [ ] Remove using directives for spreadsheet libraries as they become unused

#### 3.5 — DAO / Data Layer

- [ ] Verify no DAO writes or reads spreadsheet files (DAOs must only communicate with databases)
- [ ] If any DAO has CSV/streaming logic, extract that logic upward to the service layer then apply the same placeholder replacement
- [ ] Leave clearly marked `// TODO(SpreadsheetRemoval)` anchor comments at each removal site for MySQL follow-up tasks

---

### Phase 4 — Test Project Refactor

#### 4.1 — Identify Spreadsheet-Coupled Tests

- [ ] List all test classes and methods that directly test CSV/XLS/XLSX generation, reading, or parsing
- [ ] List all tests that depend on spreadsheet-format output files as fixtures or assertions
- [ ] Classify each test:
  - `Retire` — tests a flow that no longer exists
  - `Rewrite` — tests a flow that now has placeholder behavior instead
  - `Keep` — tests logic that is unaffected

#### 4.2 — Retire Tests

- [ ] For tests classified `Retire`, add `[Fact(Skip = "Spreadsheet workflow removed — pending MySQL replacement.")]` before removing
- [ ] Do not hard-delete retired tests until MySQL replacement is implemented and verified

#### 4.3 — Rewrite Tests

- [ ] For tests classified `Rewrite`, update assertions to verify:
  - Service/command returns the expected not-implemented result
  - `IsSuccess == false` with placeholder message text
  - No file artifacts are created on disk
- [ ] Follow project test naming convention: `MethodName_Should{Result}_When{Condition}`

#### 4.4 — Fix Test Fixtures

- [ ] Remove spreadsheet fixture files from test data directories that are no longer needed
- [ ] Update any test base classes or shared helpers that generate spreadsheet test data
- [ ] Confirm all remaining test data fixtures are deterministic and version-controlled

---

### Phase 5 — Dependency and Configuration Cleanup

#### 5.1 — NuGet Package Removal

- [ ] Remove all `<PackageReference>` entries for spreadsheet libraries from each affected `*.csproj`
- [ ] Run `dotnet build` immediately after each removal to surface hidden transitive usage
- [ ] Confirm no `#pragma warning disable` entries exist that were hiding spreadsheet-library warnings

#### 5.2 — Using Directive Cleanup

- [ ] Scan all edited files for now-unused `using` directives from spreadsheet namespaces
- [ ] Remove them — do not leave orphaned usings

#### 5.3 — Settings / Constants Cleanup

- [ ] Remove `appsettings.json` / `appsettings.Development.json` keys used only for spreadsheet file paths or export folders
- [ ] Remove strongly typed config class properties bound only to removed settings
- [ ] Preserve any config structure or keys that will be reused by the future MySQL implementation

#### 5.4 — Dead Code Sweep

- [ ] Search for any private methods, fields, or properties that were only called by the now-removed spreadsheet code
- [ ] Remove them if they are unreachable elsewhere
- [ ] Confirm zero compiler warnings for unreachable code in affected files

---

### Phase 6 — Validation Gates

#### 6.1 — Per-Module Build Gate

- [ ] After completing each module in Phase 3: `dotnet build` must succeed with zero errors
- [ ] Fix all build errors before proceeding to the next module

#### 6.2 — Full Solution Build Gate

- [ ] After all modules are complete: full solution build must succeed with zero errors
- [ ] Address any cross-module dependency errors surfaced at this stage

#### 6.3 — Test Gate

- [ ] Run `dotnet test` for the test project
- [ ] All non-retired tests must pass
- [ ] Retired tests must be suppressed with `[Skip]`, not failing
- [ ] Zero tests should be asserting that a spreadsheet file was created

#### 6.4 — Final Search Gate

Run the following scoped to app + test projects only (exclude `*.md`, `Scripts/`, `Database/`):

- [ ] No results for `".csv"`, `".xls"`, `".xlsx"` as file extension literals
- [ ] No results for spreadsheet library namespaces (`OfficeOpenXml`, `ClosedXML`, `CsvHelper`, `NPOI`, etc.)
- [ ] No `OpenFileDialog` / `SaveFileDialog` usage remaining in spreadsheet context
- [ ] All remaining hits are reviewed and explicitly approved as non-spreadsheet (e.g. a variable named `csvTable` that now does something else)

#### 6.5 — Placeholder Behavior Verification

- [ ] Manually trigger each formerly spreadsheet-driven UI action in a debug build
- [ ] Confirm the user-visible placeholder message appears
- [ ] Confirm no file is written to disk
- [ ] Confirm a log warning is emitted with the correct method/module context

---

### Phase 7 — Handoff and MySQL Migration Anchors

#### 7.1 — Post-Removal Summary

- [ ] Update the inventory table (Phase 1.2) with final `Status` column: `Done`, `Stubbed`, `Retired`
- [ ] Record any items that could not be removed and document the blocker
- [ ] List all `// TODO(SpreadsheetRemoval)` anchor locations as the input backlog for MySQL replacement work

#### 7.2 — Per-Module MySQL Replacement Backlog

For each module, list the specific replacement targets:

| Module | Former Spreadsheet Workflow | MySQL Table / Entity Needed | Priority |
|--------|-----------------------------|-----------------------------|----------|
| Module_Receiving | (from inventory) | TBD | TBD |
| Module_Dunnage | (from inventory) | TBD | TBD |
| Module_Reporting | (from inventory) | TBD | TBD |
| Module_Volvo | (from inventory) | TBD | TBD |
| Module_Settings.* | (from inventory) | TBD | TBD |

#### 7.3 — Definition of Done

This plan is complete when:

1. Full solution builds with zero errors.
2. All tests pass or are explicitly skipped with documented reason.
3. Final search gate finds zero active spreadsheet references in executable code.
4. All UI spreadsheet entry points route to the standard placeholder message.
5. The MySQL replacement backlog table (7.2) is populated and handed to the next phase.

---

## Background and Constraints

### Why We Are Removing Spreadsheets

LabelView's handling of XLS/XLSX files is causing reliability issues (concurrent access locks, format incompatibilities, and data integrity problems). MySQL table-backed storage provides consistent, concurrent, and auditable access for each module.

### Scope

| Area | Included |
|------|----------|
| App project executable code | ✅ Yes |
| Test project executable code | ✅ Yes |
| Documentation (`*.md`) | ❌ No — leave unchanged |
| Scripts and deploy files | ❌ No — leave unchanged unless they block build/test |

### Temporary Placeholder Standard

Every UI action that formerly triggered a spreadsheet operation must:

1. Continue to be reachable from the UI (do not hide or remove the control).
2. Surface this exact message to the user: **"Not implemented yet: spreadsheet workflow is being replaced by MySQL."**
3. Perform **zero file I/O**.
4. Return a failed `Model_Dao_Result` with the message, following existing project error-handling patterns.
5. Emit a structured log warning via `IService_LoggingUtility`.

### Architecture Rules That Must Not Be Violated

- ViewModels must **not** call DAOs directly.
- Services must **not** throw exceptions — return `Model_Dao_Result` failures.
- DAOs must **not** perform spreadsheet file I/O (they never should have).
- XAML must use `x:Bind` — no runtime `Binding`.
- MySQL access must use stored procedures only.
- SQL Server / Infor Visual is **read-only** — no writes.

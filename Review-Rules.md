# Module Reconciliation — Binding Rules

Last Updated: 2026-11-12

These are **not** questions. They are the repository's existing rules, restated for this refactor
with the exact source instruction file for each. Any spec, task list, or pull request for the
Module Reconciliation extraction must satisfy every rule in this document. If a proposal cannot be
expressed without breaking a rule here, the proposal is wrong, not the rule.

Companion documents:

- [Review.md](Review.md) — original goal and algorithm notes
- [Review-Clarifications.md](Review-Clarifications.md) — the questions that are genuinely still open
- [Review-Quality-Of-Life-Suggestions.md](Review-Quality-Of-Life-Suggestions.md) — optional feature ideas

## Contents

1. [R-MOD — Module boundaries and structure](#1-r-mod--module-boundaries-and-structure)
2. [R-DI — Dependency injection](#2-r-di--dependency-injection)
3. [R-NAM — Naming](#3-r-nam--naming)
4. [R-MVVM — Presentation layer](#4-r-mvvm--presentation-layer)
5. [R-DATA — Data access](#5-r-data--data-access)
6. [R-SQL — SQL authoring and script safety](#6-r-sql--sql-authoring-and-script-safety)
7. [R-ERR — Error handling](#7-r-err--error-handling)
8. [R-SET — Settings](#8-r-set--settings)
9. [R-HDR — Header, theming, and shell](#9-r-hdr--header-theming-and-shell)
10. [R-TEST — Testing](#10-r-test--testing)
11. [R-DOC — Documentation](#11-r-doc--documentation)
12. [R-CODE — Code quality](#12-r-code--code-quality)
13. [R-SCOPE — Extraction behavior preservation](#13-r-scope--extraction-behavior-preservation)
14. [Retired questions](#14-retired-questions)

---

## 1. R-MOD — Module Boundaries And Structure

### R-MOD-01 — `Module_Core` stays foundational

**Rule:** `Module_Core` holds shared infrastructure, contracts, helpers, and base services. It does
not absorb module-specific business logic, models, or UI state.

**Source:** `.github/instructions/architecture/module-coupling-constraints.instructions.md`

**Application:** Doctrine — do not move `Model_ReceivingLoad`, the reconciliation service, or the
reconciliation models into `Module_Core` to make the new module "cleaner". That is exactly the
pollution this rule forbids.

### R-MOD-02 — `Module_Shared` is presentation-only

**Rule:** `Module_Shared` is for genuinely cross-module presentation and shared UI assets. It is not
a home for module-specific rules.

**Source:** `.github/instructions/architecture/module-coupling-constraints.instructions.md`

### R-MOD-03 — No reach-through into another module's internals

**Rule:** A module must not call another module's DAO, helper, or internal model directly.
Cross-module behavior goes through an explicit contract — a service interface or a facade.

**Source:** `.github/instructions/architecture/module-coupling-constraints.instructions.md`,
`.github/instructions/architecture/mvvm-pattern.instructions.md`

**Application:** If the new module needs receiving rows, it either consumes an explicit contract or
owns its own data access. It must never instantiate `Dao_ReceivingLabelData` or reach into
`Module_Receiving/Data`.

### R-MOD-04 — No circular dependencies

**Rule:** Module references flow one way. A new module may not create a cycle, including a cycle
through settings or shared contracts.

**Source:** `.github/instructions/architecture/module-coupling-constraints.instructions.md`

**Application:** `Module_Receiving` must not reference the new module. If Receiving still needs a
reconciliation entry point, that is a decision for `SC-06`, and it defaults to deleting it.

### R-MOD-05 — Do not copy logic between modules

**Rule:** When behavior is needed in two places, extract a shared abstraction. Duplicating an
algorithm into a second module is forbidden.

**Source:** `.github/instructions/architecture/module-coupling-constraints.instructions.md`

**Application:** The reconciliation algorithm moves; it is never copied so that both modules keep a
version.

### R-MOD-06 — Prefer explicit seams over premature abstraction

**Rule:** Abstractions are introduced for behavior that already exists in more than one place, not
for hypothetical future sources.

**Source:** `.github/instructions/architecture/module-coupling-constraints.instructions.md`,
`.github/instructions/quality/object-calisthenics.instructions.md` (keep entities small)

**Application:** One receiving data source does not justify a full provider framework. Name the seam;
do not build the plugin system.

### R-MOD-07 — The reconciliation module has zero code coupling

**Rule:** `Module_Reconciliation` must not reference `Module_Receiving`. No project reference, no `using`,
no injected `Module_Receiving` service, no shared model, enum, constant, DAO, stored procedure name, or
settings key. It may depend only on:

- `Module_Core` — contracts, `Model_Dao_Result`, logging, error handler, settings core.
- `Module_Shared` — `ViewModel_Shared_Base` and shared presentation assets.

Each module owns its own models, DAO, services, stored procedures, state tables, views, viewmodels,
converters, dialogs, settings keys, navigation entry, header-provider implementation, and accent triple.

**Source:** `.github/instructions/architecture/module-coupling-constraints.instructions.md`;
`.github/instructions/copilot-instructions.md` → Non-Negotiables; project decision recorded
2026-11-12.

**Application:** The new module does not call `IService_MySQL_Receiving`, does not read
`Model_ReceivingLoad`, and does not reuse `sp_Receiving_LabelData_Update`. Receiving keeps its own
recommendation feature and its own Edit Mode write path, untouched.

### R-MOD-08 — Shared storage is not shared code

**Rule:** Two modules may read and write the same physical table, provided each module owns its own DAO
and its own stored procedures over that table. What they must never share is code: models, DAOs, service
contracts, or procedure names.

**Source:** `.github/instructions/architecture/dao-pattern.instructions.md`;
`.github/instructions/architecture/module-coupling-constraints.instructions.md`

**Application:** Correcting a saved receiving location requires writing the receiving rows, because that
is where users see the value. The new module does it through exactly one reconciliation-owned procedure
(for example `sp_Reconciliation_ReceivingLocation_Update`). That single procedure is the entire data
surface between the two modules, and it is a SQL contract, not a C# dependency.

### R-DATA-09 — Inventory truth comes from the ledger and current on-hand

**Rule:** Any reconciliation or validation logic must derive quantities from the Infor Visual ledger model,
never from an assumption about how transfers were recorded:

- `INVENTORY_TRANS.QTY` is always positive; direction lives in `TYPE` (`I` in, `O` out).
- Location transfers are matched `I`/`O` pairs linked both ways by `TRANSFER_TRANS_ID`, and they net to
  zero overall.
- `CR_PART_LOCATION` is the on-hand authority per `(part, warehouse, location)`.
- Lifetime in minus out must equal total on-hand. The ledger is internally consistent, so a mismatch is
  physical activity, never a lost record.
- Consumption is posted at barcode-labor clock-out, not when a work order is created.

**Source:** `.github/instructions/database/infor-visual-inventory-discrepancy-investigation.instructions.md`

**Application:** Material left in the receiving location after a partial transfer is real stock, so a
validation rule must never demand that a receipt be fully consumed. See the two-part quantity gate in
`Review-Clarifications.md` `LG-04`, and the `MMC0001000` reference case.

---

## 2. R-DI — Dependency Injection

### R-DI-01 — Registration lives in `Infrastructure/DependencyInjection`

**Rule:** Module services, viewmodels, and views are registered in
`Infrastructure/DependencyInjection/*.cs`. Grouped registration extension methods are the established
pattern for a module.

**Source:** `.github/instructions/copilot-instructions.md` → Key Architecture Rules;
`.github/instructions/architecture/module-coupling-constraints.instructions.md` → Preferred Pattern

### R-DI-02 — Never register app services in `App.xaml.cs`

**Rule:** `App.xaml.cs` wires the host. It does not contain module service registrations.

**Source:** `.github/instructions/copilot-instructions.md` → Key Architecture Rules

### R-DI-03 — Base class, host wiring, packages, and schema need approval

**Rule:** Ask before changing base classes, app host DI wiring, adding NuGet packages, adding
third-party dependencies, or changing schema and stored procedures.

**Source:** `.github/instructions/copilot-instructions.md` → Ask User Before

**Application:** New audit tables, new print-tracking columns, and any new package must be approved
before the spec marks them as in scope.

---

## 3. R-NAM — Naming

### R-NAM-01 — Type prefixes follow repository convention

**Rule:** Match the existing prefix per kind:

| Kind | Prefix | Example |
|------|--------|---------|
| View | `View_` | `View_Reconciliation_Main.xaml` |
| ViewModel | `ViewModel_` | `ViewModel_Reconciliation_Main` |
| Model | `Model_` | `Model_ReconciliationItem` |
| DAO | `Dao_` | `Dao_ReconciliationRun` |
| Service | `Service_`, contract `IService_` | `IService_Reconciliation` |
| Helper | `Helper_` | `Helper_ReconciliationAllocation` |
| Converter | `Converter_` | `Converter_ReconciliationResolutionBrush` |
| Dialog | `Dialog_` | `Dialog_ReconciliationApplyConfirm` |
| Enum | `Enum_` | `Enum_ReconciliationResolution` |

**Source:** `.github/instructions/languages/csharp.instructions.md` → Naming Conventions;
`.github/instructions/architecture/winui3-xaml-converters.instructions.md`;
repository-wide convention visible in `Module_Receiving`, `Module_Reprint`, `Module_Dunnage`

### R-NAM-02 — Standard C# conventions

**Rule:** PascalCase for types and public members; camelCase for private fields and locals;
interfaces prefixed with `I`.

**Source:** `.github/instructions/languages/csharp.instructions.md`

### R-NAM-03 — No abbreviations

**Rule:** Identifiers, class names, and namespaces must not use abbreviations. Names must read as
full words.

**Source:** `.github/instructions/quality/object-calisthenics.instructions.md` → Rule 6, Don't
abbreviate (applies to `**/*.cs`).

**Application:** `Module_Recon` is an abbreviation. The module folder, namespace, and all
`Model_Recon_*`-style names must use a full word (`Reconciliation`). This directly overrides the
`Module_Recon` name used in `Review.md`.

### R-NAM-04 — Async suffix

**Rule:** Every asynchronous method name ends with `Async`.

**Source:** `.github/instructions/copilot-instructions.md` → Non-Negotiables

### R-NAM-05 — File shape and public API docs

**Rule:** File-scoped namespaces, single-line using directives, XML documentation on public APIs.

**Source:** `.github/instructions/languages/csharp.instructions.md`

### R-NAM-06 — Test naming

**Rule:** Test methods use `MethodName_ShouldResult_WhenCondition`. No `Arrange`/`Act`/`Assert`
comments.

**Source:** `.github/instructions/testing/testing-strategy.instructions.md`;
`.github/instructions/copilot-instructions.md` → Testing Rules

---

## 4. R-MVVM — Presentation Layer

### R-MVVM-01 — Mandatory layer flow

**Rule:** `View → ViewModel → Service → DAO → Database`. No shortcut around a layer.

**Source:** `.github/instructions/copilot-instructions.md` → Non-Negotiables

### R-MVVM-02 — ViewModel shape

**Rule:** ViewModels are `partial`, inherit `ViewModel_Shared_Base`, and inject **services** — never
DAOs. Orchestration lives in the viewmodel; persistence lives below it.

**Source:** `.github/instructions/architecture/mvvm-pattern.instructions.md`

### R-MVVM-03 — `x:Bind` only

**Rule:** Use `x:Bind` with an explicit binding mode. Runtime `{Binding}` is forbidden in new XAML.

**Source:** `.github/instructions/architecture/mvvm-pattern.instructions.md`;
`.github/instructions/copilot-instructions.md` → Non-Negotiables

**Application:** Every column, command, and visibility binding in the new review grid uses `x:Bind`.

### R-MVVM-04 — No logic in code-behind

**Rule:** Code-behind is limited to view-only concerns: focus, visual state glue, dialog host setup.
No business decisions, persistence, or validation orchestration. Expose actions as commands, not
event handlers.

**Source:** `.github/instructions/architecture/mvvm-pattern.instructions.md`;

`.github/instructions/copilot-instructions.md` → Non-Negotiables

### R-MVVM-05 — Accessibility baseline

**Rule:** Every interactive control is keyboard reachable and carries `AutomationProperties.Name`
(or a bound equivalent). New pages follow the existing accessibility pattern in
`View_Receiving_ModeSelection.xaml`.

**Source:** `.github/instructions/architecture/winui3-dialog-window-patterns.instructions.md`;
existing repository implementation in `Module_Receiving/Views/View_Receiving_ModeSelection.xaml`

### R-MVVM-06 — Collection rebuilds

**Rule:** For a full `ObservableCollection` rebuild, replace the collection instance instead of a
`Clear()` + `Add` loop.

**Source:** `.github/instructions/copilot-instructions.md` → Non-Negotiables

**Application:** Refreshing the finding grid after a re-run replaces the collection; it does not
clear and repopulate.

### R-MVVM-07 — Dialogs versus windows

**Rule:** Dialogs are for short focused edits, confirmations, and lightweight modal entry. Windows
are for larger workflows and persistent independent state. Set `XamlRoot` on `ContentDialog`, keep
dialog code-behind view-specific, return results or models rather than sharing mutable state, and
keep validation and persistence outside the dialog layer.

**Source:** `.github/instructions/architecture/winui3-dialog-window-patterns.instructions.md`

**Application:** The page itself is a page, not a "Dialog" view. Only confirmation and apply prompts
are dialogs, and they hand results back to the viewmodel.

### R-MVVM-08 — Converters

**Rule:** Converters are `Converter_`-prefixed, single-purpose, null-safe, free of business logic,
never injected with services or data access, and registered at the narrowest reasonable XAML scope.

**Source:** `.github/instructions/architecture/winui3-xaml-converters.instructions.md`

### R-MVVM-09 — CQRS is optional and never bypasses the service layer

**Rule:** MediatR, FluentValidation, and the global pipeline behaviors are available. CQRS may be
used at the ViewModel → handler boundary, and validators run automatically through the pipeline. A
handler must not become a ViewModel's route into a DAO. The MVVM flow from R-MVVM-01 remains
mandatory.

**Source:** `.github/instructions/architecture/cqrs-behaviors-pipeline.instructions.md`;
`.github/instructions/copilot-instructions.md` → Non-Negotiables;
`.github/instructions/testing/testing-strategy.instructions.md` → Decision Guide

**Application:** This module does not need CQRS. If it is used, the handler depends on service
interfaces only, so the operation stays unit-testable.

---

## 5. R-DATA — Data Access

### R-DATA-01 — Instance-based DAOs

**Rule:** DAOs are instance classes. Required connection material arrives through constructor
injection.

**Source:** `.github/instructions/architecture/dao-pattern.instructions.md`;
`.github/instructions/copilot-instructions.md` → Non-Negotiables

### R-DATA-02 — `Model_Dao_Result` for outcomes

**Rule:** DAOs return `Model_Dao_Result` or `Model_Dao_Result<T>`. They do not throw for expected
query or stored-procedure failures.

**Source:** `.github/instructions/architecture/dao-pattern.instructions.md`;
`.github/instructions/architecture/error-handling.instructions.md`

### R-DATA-03 — MySQL writes are stored-procedure only

**Rule:** Every MySQL write goes through a stored procedure. No raw write SQL strings in C#.

**Source:** `.github/instructions/architecture/dao-pattern.instructions.md`;
`.github/instructions/copilot-instructions.md` → Non-Negotiables

**Application:** Location updates, run auditing, and finding disposition all require SPs. There is no
"quick inline UPDATE" escape hatch.

### R-DATA-04 — Infor Visual is strictly read-only

**Rule:** No `INSERT`, `UPDATE`, or `DELETE` against any Visual table, ever, from code or script.
The Visual connection string keeps `ApplicationIntent=ReadOnly`.

**Source:** `.github/instructions/copilot-instructions.md` → Database Rules;
`.github/instructions/architecture/dao-pattern.instructions.md`;
`.github/instructions/database/infor-visual-database-reference.instructions.md`

### R-DATA-05 — Verify Visual schema before writing queries

**Rule:** Confirm every table and column against `docs/InforVisual/DatabaseCSVFiles/` before writing
a Visual query. Follow the query-authoring workflow rather than guessing column names.

**Source:** `.github/instructions/database/infor-visual-query-authoring.instructions.md`;
`.github/instructions/architecture/dao-pattern.instructions.md`

**Application:** Any new `RECEIVER` / `RECEIVER_LINE` / current-inventory query must be researched
against the CSV reference set first.

### R-DATA-06 — Cross-table rules live in services

**Rule:** DAOs map rows and execute SPs. Business rules spanning tables or sources belong in services.

**Source:** `.github/instructions/architecture/dao-pattern.instructions.md` → Implementation
Expectations

---

## 6. R-SQL — SQL Authoring And Script Safety

### R-SQL-01 — `Database/` is the source of truth

**Rule:** SQL artifacts under `Database/` are canonical. Extension-run or ad-hoc queries are for
validation only and must be reconciled back into repository scripts when finalized.

**Source:** `.github/instructions/tooling/mcp-tooling.instructions.md` → Rules;
`.github/instructions/copilot-instructions.md` → Database Rules

### R-SQL-02 — Query shape

**Rule:** Explicit column lists (no `SELECT *`), qualified names in multi-table queries, `LIMIT`/`TOP`
on result sets, and no functions applied to indexed columns in `WHERE`.

**Source:** `.github/instructions/database/sql-sp-generation.instructions.md` → SQL Query Structure

**Application:** The transfer-movement candidate query must stay bounded; `INVENTORY_TRANS` is one of
the largest tables in Visual.

### R-SQL-03 — Stored procedure structure

**Rule:** Header comment block with description, parameters, and return values; `SET NOCOUNT ON` for
modifying procedures; explicit `BEGIN` / `COMMIT`; parameterized statements; no dynamic SQL.

**Source:** `.github/instructions/database/sql-sp-generation.instructions.md`

### R-SQL-04 — Stored procedure naming matches neighbors

**Rule:** New MySQL procedures follow the naming already used by neighboring scripts —
`sp_<Module>_<Area>_<Action>` (for example `sp_Receiving_LabelData_Update`).

**Source:** `.github/instructions/database/sql-sp-generation.instructions.md` → Stored Procedure
Naming Conventions; existing files in
`Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/`

**Note:** The instruction file's `usp_` prefix guidance applies to scripts under
`Database/StoredProcedures/**`. This repository's deployed procedures live under
`Database/Database_Deployment/Sql_Files/StoredProcedures/**` and use `sp_`. Consistency with the
module's neighbors wins; flag the discrepancy rather than introducing a third style.

### R-SQL-05 — Toggleable test blocks

**Rule:** Inline manual tests use the exact marker format `-- TEST BLOCK START: <Block Name>` and
`-- TEST BLOCK END: <Block Name>`, with the body commented out by default, so
`Database/Scripts/SetTestBlockState/Set-SqlTestBlockState.ps1` can toggle them safely. Never nest
blocks.

**Source:** `.github/instructions/database/sql-sp-generation.instructions.md` → Toggleable Test
Blocks

### R-SQL-06 — DB script safety workflow

**Rule:** Any script that can modify a database defaults to read-only, supports `--whatif` and
`--generate-sql`, takes an automatic backup before writing core, requires typed confirmation, aborts
and restores on first error, is idempotent, and is verified on a copy. Never commit credentials.
Validate writes against `mtm_receiving_application_test` first.

**Source:** `.github/instructions/database/db-script-execution-standards.instructions.md`

**Application:** Any schema migration or new SP deployment script for this module follows this
workflow, including the backup, confirmation, and idempotency requirements.

### R-SQL-07 — No writes to Visual

**Rule:** Visual query files contain `SELECT` only. No DDL, no DML.

**Source:** `.github/instructions/database/infor-visual-database-reference.instructions.md` →
Critical Rules Before Writing Any SQL

---

## 7. R-ERR — Error Handling

### R-ERR-01 — Responsibility split

**Rule:** DAOs return failure results for expected problems; services translate them into application
behavior and logging; ViewModels decide what the user sees through `IService_ErrorHandler`.

**Source:** `.github/instructions/architecture/error-handling.instructions.md`

### R-ERR-02 — Never swallow, always contextualize

**Rule:** Do not swallow exceptions silently. Do not throw from DAOs for expected failures. Keep
severity and context explicit when escalating unexpected failures.

**Source:** `.github/instructions/architecture/error-handling.instructions.md` → Rules

### R-ERR-03 — Actionable logging

**Rule:** Log messages are actionable and scoped to the operation that failed.

**Source:** `.github/instructions/architecture/error-handling.instructions.md` → Preferred Pattern

**Application:** Per-group Visual failures log the part, date, and group, not just "reconciliation
failed".

---

## 8. R-SET — Settings

### R-SET-01 — All settings go through the settings core

**Rule:** New settings are added through the existing settings core flow — facade plus CQRS command
or query — never by bypassing the facade or reading storage directly.

**Source:** `.github/instructions/architecture/settings-management-system.instructions.md` → Rules

### R-SET-02 — Cross-cutting behavior stays inside settings services

**Rule:** Encryption, caching, and audit behavior remain inside the existing settings services.
Validation belongs in validators.

**Source:** `.github/instructions/architecture/settings-management-system.instructions.md`

### R-SET-03 — Contract before UI, and settings UI is never inside a module

**Rule:** Define the contract before building settings UI. Settings UI lives under a
`Module_Settings.*` project — never inside the working module's `Views`.

**Source:** `.github/instructions/architecture/settings-management-system.instructions.md` →
Extension Guidance; `.github/instructions/architecture/module-coupling-constraints.instructions.md`

---

## 9. R-HDR — Header, Theming, And Shell

### R-HDR-01 — Titles come from a provider or a fallback

**Rule:** Dynamic titles implement `IViewModel_HeaderTitleProvider` and notify
`CurrentHeaderTitle`. Views and viewmodels never write to `PageTitleTextBlock`. Static titles come
from `MainWindow._navRoutes` or the settings page header mapping.

**Source:** `.github/instructions/architecture/winui3-header-workflow.instructions.md` → Title
Resolution, Rules And Guardrails

### R-HDR-02 — One sync chokepoint

**Rule:** All header refresh flows through `MainWindow.SetContentPage` → `SyncPageHeader`. New
navigation paths go through it; title and accent logic is never duplicated.

**Source:** `.github/instructions/architecture/winui3-header-workflow.instructions.md` → The Sync
Chokepoint

### R-HDR-03 — Back action through the shared service

**Rule:** Register back actions with `IService_HeaderBackNavigation.RegisterBackAction`, and clear
them when the drill-down ends. `SyncPageHeader` clears stale actions on every navigation, so a page
re-registers in `Loaded` or when its state becomes active.

**Source:** `.github/instructions/architecture/winui3-header-workflow.instructions.md` → Shared Back
Action

### R-HDR-04 — Module accent uses shared brushes

**Rule:** A new module's accent is added to `Module_Core/Themes/ModuleAccentBrushes.xaml` as a full
`*AccentBrush` / `*AccentBorderBrush` / `*AccentHighlightBrush` triple and mapped in
`GetModuleAccent`. Never hardcode hex values in a view or viewmodel, and never touch
`HeaderBarBorder.BorderBrush`, `UserMenuButton.BorderBrush`, or `UserPictureBorder.BorderBrush`
outside `ApplyHeaderAccent`.

**Source:** `.github/instructions/architecture/winui3-header-workflow.instructions.md` → Module
Accent Coloring, Rules And Guardrails

**Application:** The new module gets its own accent triple and a `GetModuleAccent` case. The
only open decision is the color value itself.

### R-HDR-05 — Never null out header foregrounds

**Rule:** Use `ClearValue(TextBlock.ForegroundProperty)` (and the icon equivalent) to restore
foregrounds. A null local `Foreground` renders the title invisible.

**Source:** `.github/instructions/architecture/winui3-header-workflow.instructions.md` → Rules And
Guardrails

---

## 10. R-TEST — Testing

### R-TEST-01 — Framework

**Rule:** xUnit with FluentAssertions.

**Source:** `.github/instructions/testing/testing-strategy.instructions.md` → Core Rules

### R-TEST-02 — Unit versus integration

**Rule:** Unit tests when dependencies are mockable at a service or mediator boundary. Integration
tests for DAOs, concrete database interactions, and behavior that depends on real infrastructure
wiring.

**Source:** `.github/instructions/testing/testing-strategy.instructions.md` → Decision Guide

**Application:** The reconciliation service takes service interfaces, so its tests are unit tests
with mocked `IService_*` dependencies. New DAOs get integration tests.

### R-TEST-03 — Fixtures

**Rule:** Integration tests use `IAsyncLifetime` for setup and cleanup. Disposable database test data
is prefixed with `TEST-`.

**Source:** `.github/instructions/testing/testing-strategy.instructions.md` → Repository Patterns

### R-TEST-04 — Failure paths are required

**Rule:** Validate failure paths, not only success paths. Keep tests narrow and behavior-based; do
not write one oversized test per class.

**Source:** `.github/instructions/testing/testing-strategy.instructions.md` → Core Rules, Repository
Patterns

### R-TEST-05 — Narrowest filter first

**Rule:** Run the narrowest matching test filter first; widen to module or suite level only after the
focused slice passes.

**Source:** `.github/instructions/testing/testing-strategy.instructions.md` → Validation;
`.github/instructions/copilot-instructions.md` → Validation

### R-TEST-06 — Repository commands

**Rule:** Build with `dotnet build MTM_Receiving_Application.slnx`; test with
`dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj`.

**Source:** `.github/instructions/copilot-instructions.md` → Build/Test Commands

---

## 11. R-DOC — Documentation

### R-DOC-01 — Every module ships a `Documentation/` folder

**Rule:** A module is not complete without this structure:

```text
Documentation/
  Overview/
  How-To-Guides/
  Support-and-Fixes/
  Changes-and-Decisions/
  Big-Changes/
  AI-Handoff/
  End-User-Help/
  Templates/
```

with all fifteen mandatory files present, and a `Last Updated: YYYY-MM-DD` line within the first
five lines of every file.

**Source:** `.github/instructions/module-doc-maintenance.instructions.md`

**Application:** The new module's docs are a deliverable, not a follow-up. Moving the existing
reconciliation doc is not sufficient on its own.

### R-DOC-02 — Update docs when code invalidates them

**Rule:** If a change invalidates source-of-truth documentation, update the matching files under
`.github/`, `specs/`, `docs/`, or the module's `Documentation/` folder in the same change.

**Source:** `.github/instructions/copilot-instructions.md` → Docs Maintenance;
`.github/instructions/documentation/update-docs-on-code-change.instructions.md`;
`.github/instructions/module-doc-maintenance.instructions.md`

### R-DOC-03 — Change log and decision entries

**Rule:** `Changes-and-Decisions/Change-Log.md` gains a dated entry per change with rationale;
`Decisions.md` gains a dated entry per decision.

**Source:** `.github/instructions/module-doc-maintenance.instructions.md` → Freshness and Logging
Rules

### R-DOC-04 — Markdown style

**Rule:** Google Markdown style: ATX headings, fenced code blocks, bullet and numbered lists. No XML
tags inside spec or module documentation files.

**Source:** `.github/instructions/documentation/markdown-style.instructions.md`;
`.github/instructions/documentation/spec-slice-format.instructions.md`

---

## 12. R-CODE — Code Quality

### R-CODE-01 — Object Calisthenics on business and service code

**Rule:** For business domain code, application services, and use-case handlers:

1. One level of indentation per method.
2. Do not use the `else` keyword.
3. Wrap primitives and strings where it adds meaning.
4. First-class collections.
5. One dot per line.
6. Do not abbreviate.
7. Keep entities small — class, method, and namespace.
8. No class with more than two instance variables.
9. No getters or setters in domain classes.

Exempt: DTOs, API contracts, configuration classes, simple data containers, infrastructure code where
flexibility is required.

**Source:** `.github/instructions/quality/object-calisthenics.instructions.md`

**Application:** The existing 1,200-line reconciliation service violates rules 1, 6, and 7. The
extraction is the moment to split it into focused collaborators (allocation, transfer selection,
bucket building, group resolution, preview orchestration). Moving the file unchanged satisfies the
org chart and none of this rule.

### R-CODE-02 — Keep the service small

**Rule:** New behavior goes into a focused collaborator, not appended to an already large class.

**Source:** `.github/instructions/quality/object-calisthenics.instructions.md` → Rule 7;
`.github/instructions/architecture/module-coupling-constraints.instructions.md`

### R-CODE-03 — Nullability

**Rule:** Prefer `is null` / `is not null` over `== null` / `!= null`. Trust the annotations; do not
add redundant checks.

**Source:** `.github/instructions/languages/csharp.instructions.md` → Nullable Reference Types

### R-CODE-04 — Modern C# and `nameof`

**Rule:** Pattern matching and switch expressions where they fit. `nameof` instead of string literals
for member names. `.editorconfig` formatting.

**Source:** `.github/instructions/languages/csharp.instructions.md` → Formatting

### R-CODE-05 — Comments explain why

**Rule:** Comment the reason, not the mechanics. No changelog comments, no divider banners, no
commented-out code.

**Source:** `.github/instructions/self-explanatory-code-commenting.instructions.md` (self-explanatory
code commenting), general commenting guidance

### R-CODE-06 — Approval gates

**Rule:** No new NuGet packages, no third-party dependencies, no schema or stored-procedure changes,
and no base-class or host-wiring changes without explicit approval.

**Source:** `.github/instructions/copilot-instructions.md` → Ask User Before

---

## 13. R-SCOPE — Extraction Behavior Preservation

### R-SCOPE-01 — Extraction does not change behavior

**Rule:** Moving the feature must not change what it computes. Any intended behavior change is a
separate, explicitly approved item in the spec.

**Source:** `.github/instructions/copilot-instructions.md` → Required Workflow (smallest grounded
change); `Review.md` §2 (limit removal to reconcile-specific artifacts)

### R-SCOPE-02 — Preserved invariants

**Rule:** These documented behaviors survive the move unchanged, and only change if the spec says so:

- Allocation ordering: quantity descending → load number → load id.
- Chained-move resolution to the net destination.
- Partial-retention behavior when a staging location keeps quantity.
- ±2 day transfer discovery window around each group's receipt date.
- Strict validation window of receipt date and receipt date + 1.
- `NotFound`, `Skipped`, and `Unchanged` resolution paths.
- Cross-pass merge of Current Labels and History rows for the same part and receipt date.
- The `ApplyLocationUpdateAsync` write path.

**Approved removal (2026-11-12):** the `IgnoredBySettings` resolution path is **not** preserved — the new
module does not use an ignored-locations list (`Review-Clarifications.md` `LG-14`). This is an explicit
decision, not a regression. The old ignored-locations setting is not migrated.

**Source:** `docs/features/receiving/location-reconciliation-process.md`;
`docs/prompts/reconciliation-location-fix.md` (Fixes 2, 3, and 5); `.github/instructions/architecture/dao-pattern.instructions.md` (preserve grounded behavior)

### R-SCOPE-03 — New module lands first

**Rule:** Land the new module before removing the old surface so no shipped capability disappears
mid-change, and keep the solution building between removal steps.

**Source:** `Module_Reprint/docs/FeatureUpdates/01-FeatureChange-ReprintLabels.md` → Why This Runs
First; `.github/instructions/copilot-instructions.md` → Required Workflow

### R-SCOPE-04 — Remove only what belongs to the feature

**Rule:** Remove reconcile-specific documents, code, memories, tests, and SQL only. Shared code,
shared SPs, shared DAO methods, and shared contracts stay.

**Source:** `Review.md` §2 and §7.3, §7.9

**Application:** Query `16` and the recommendation method back Restoring entry-time location
suggestions in Receiving; they do not move.

### R-SCOPE-05 — Exact decimal arithmetic

**Rule:** Quantity comparisons stay in `decimal`. No `double` or `float` round-trip. Numeric values in
messages and logs format with `CultureInfo.InvariantCulture`.

**Source:** `.github/instructions/languages/csharp.instructions.md` → Formatting (deterministic
formatting); existing service uses `CultureInfo.InvariantCulture`

---

## 14. Retired Questions

These were originally written as open questions in the clarification register. They are not open:
they are settled by the rules above and are removed from consideration.

| Retired ID | Original question | Now governed by |
|------------|-------------------|-----------------|
| `AR-01` | How should the module be registered in DI? | R-DI-01, R-DI-02 |
| `AR-02` | What should the types be named? | R-NAM-01, R-NAM-02, R-NAM-03 |
| `AR-03` | New DAO or shared facade? (DAO shape half) | R-DATA-01, R-DATA-02, R-MOD-03 |
| `AR-06` | Header provider and accent wiring | R-HDR-01 through R-HDR-05 |
| `DS-05` | Is Visual still read-only? | R-DATA-04, R-SQL-07 |
| `LG-08` | Keep the transfer pairing assumption? | R-SCOPE-02 |
| `LG-09` | Keep the chained-move rule? | R-SCOPE-02 |
| `LG-17` | Decimal arithmetic rules | R-SCOPE-05 |
| `TS-01` | Test framework, placement, naming | R-TEST-01, R-TEST-02, R-TEST-03, R-NAM-06 |
| `TS-08` | Which build and test commands | R-TEST-05, R-TEST-06 |
| `NF-03` | Error escalation boundary | R-ERR-01, R-ERR-02 |
| `MG-02` | Do docs move or get rewritten? (must-update half) | R-DOC-01, R-DOC-02 |
| `UI-12` | Accessibility baseline | R-MVVM-05 |

The surviving open questions are in [Review-Clarifications.md](Review-Clarifications.md).

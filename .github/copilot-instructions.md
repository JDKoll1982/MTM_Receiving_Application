# MTM Receiving Application Instructions

Repository-wide source of truth for coding agents.

## Reply Style

- Default to code-only or bullets unless the user asks for explanation.
- Keep replies short and direct.

## Read Order

1. README.md
2. .github/README.md
3. This file
4. Only the smallest matching file under .github/instructions/

Ignore .github/archive/ for active work.

## Non-Negotiables

- Keep MVVM flow: View -> ViewModel -> Service -> DAO -> Database.
- No ViewModel direct calls to Dao_* or Helper_Database_*.
- Use x:Bind in XAML. Do not add runtime {Binding}.
- Keep business logic out of .xaml.cs.
- DAOs are instance-based and return Model_Dao_Result / Model_Dao_Result<T> for expected failures.
- MySQL writes must use stored procedures. No raw MySQL SQL in C#.
- Infor Visual SQL Server is read-only. Never INSERT/UPDATE/DELETE there.
- Async methods end with Async.
- For full ObservableCollection rebuilds, replace collection instance instead of Clear()+Add loop.

## Major Assumptions

Pause and ask via chat approval flow when a major assumption is needed, including:

- choosing among valid architecture paths
- inferring missing contracts/SPs
- cross-module ownership decisions
- scope changes not stated by user

## Required Workflow

1. Read minimal relevant instructions.
2. Inspect owning code path before edits.
3. Make smallest grounded change.
4. Run narrowest useful validation.
5. Update active docs/metadata if code change invalidates them.

## Ask User Before

- adding NuGet packages
- changing schema or stored procedures
- changing base classes / app host DI wiring
- adding third-party dependencies
- broad architectural changes outside task scope

## Validation

- Prefer narrow checks over full solution runs.
- Use focused tests/build for changed slice.
- For docs-only changes, verify paths/links/consistency.

## Key Architecture Rules

- Services hold business logic.
- DI registration belongs in Infrastructure/DependencyInjection/*.
- Do not register app services directly in App.xaml.cs.

## Database Rules

- MySQL: stored procedures only.
- SQL Server (Infor Visual): read-only.
- Never write raw MySQL SQL in C#.
- Use the VS Code extension `cweijan.vscode-mysql-client2` for MySQL read/write validation against `mtm_receiving_application_test` before or during MySQL-related code changes.
- Keep SQL files in `Database/` as source-of-truth artifacts; use extension queries for immediate validation and safe iteration.
- Run extension-side tests with a non-destructive pattern: target the test schema explicitly and clean up any temporary rows/tables created during validation.

## Testing Rules

- FluentValidation validators -> unit tests.
- ViewModel with IMediator only -> unit tests.
- ViewModel with concrete services -> integration tests.
- Handler with concrete DAOs -> integration tests.
- Handler with interfaces only -> unit tests.
- DAO -> integration tests.
- Use FluentAssertions.
- Naming: MethodName_ShouldResult_WhenCondition.
- Integration tests use IAsyncLifetime.
- Prefix test data with TEST-.

## Module Map

- Module_Core: shared infra/helpers/base services.
- Module_Shared: shared views/viewmodels/models.
- Module_Receiving: receiving workflow + labels.
- Module_Dunnage: dunnage workflow.
- Module_Reporting: reporting.
- Module_Settings.*: settings UI + settings core.
- Module_Volvo: Volvo features.

## Quick Debug Checklist

- DI registration exists.
- ViewModel is partial and inherits ViewModel_Shared_Base.
- XAML uses x:Bind.
- No ViewModel -> DAO calls.
- DAO returns Model_Dao_Result patterns.
- MySQL uses stored procedures.
- No SQL Server writes.
- Async naming and error handling are present.

## Docs Maintenance

If code changes invalidate source-of-truth docs, update matching files under .github/, specs/, or docs/.

## Build/Test Commands

- dotnet build MTM_Receiving_Application.sln
- dotnet test MTM_Receiving_Application.sln

## Environment Notes

- Prefer non-terminal validation when terminal use is disruptive.
- For XLSX multi-user access, use shared file access (FileShare.ReadWrite or equivalent).

## Azure Notes

- For Azure tasks, use Azure tools.
- If azmcp_bestpractices_get exists, run it first.
- If not available, ask user to enable it.

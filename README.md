# MTM Receiving Application

WinUI 3 desktop application for manufacturing receiving workflows, label generation, ERP lookups,
and related settings, reporting, and support tooling.

## Stack

- .NET 10
- C# 13
- WinUI 3
- CommunityToolkit.Mvvm
- MediatR and FluentValidation
- MySQL 5.7 for application writes
- SQL Server / Infor Visual for read-only ERP queries

## Repository Layout

- `Module_Core/` — shared infrastructure, behaviors, converters, helpers, base models, services
- `Module_Receiving/` — receiving workflow and label-related features
- `Module_Dunnage/`, `Module_Reporting/`, `Module_Volvo/` — domain modules
- `Module_Settings.*` — settings subsystems and settings UI
- `Module_Shared/` — shared UI and cross-module presentation assets
- `Infrastructure/` — dependency injection, configuration, logging, app-wide plumbing
- `Database/` — SQL scripts, schema assets, test data, and database deployment resources
- `docs/` — project documentation, CopilotForms assets, database references, and historical notes
- `.github/` — active AI customization files, prompt library, instructions, agents, and archive
- `MTM_Receiving_Application.Tests/` — unit and integration tests

## Build And Test

```powershell
dotnet build MTM_Receiving_Application.slnx
dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj
```

Use narrower workspace tasks when validating a focused change.

## Architecture Rules

- Keep the MVVM boundary intact: View → ViewModel → Service → DAO → Database.
- Use `x:Bind` in XAML.
- Keep DAOs instance based.
- Use stored procedures for MySQL access.
- Keep Infor Visual access read only.

## Scanner Feature UI And DB Guardrails

- UI for scanner workflows should match approved mockups as closely as possible.
- Scanner UI must preserve resize behavior using vertical and horizontal stretch mechanics.
- MySQL target version is 5.7 for this feature.
- MySQL writes must stay behind stored procedures (no raw MySQL DML in C#).

### Recommended Scanner Implementation Order

1. Complete `Module_Scanner` core contracts, models, and service interfaces, and create corresponding tests in `MTM_Receiving_Application.Tests`.
   - contract tests for request/response and DTO boundary expectations
   - model mapping tests for scanner state and persistence shape conversions
   - interface wiring tests to confirm DI resolution and expected service registrations
2. Implement MySQL DAO and stored-procedure access for scanner persistence requirements.
3. Implement scanner services (workflow orchestration, validation, persistence/history, native input automation).
4. Implement ViewModel behavior and command/state flow.
5. Complete scanner view composition and x:Bind wiring to match approved mockups.
6. Validate DI and shell navigation route behavior across scanner pages.
7. Add and run feature tests in `MTM_Receiving_Application.Tests` before final integration:
   - unit tests for ViewModels and interface-only service behavior
   - integration tests for DAO-backed and persistence-bound paths
   - validation-result mapping tests for add-line Status/Notes behavior
8. Perform end-to-end manual verification against configured ERP target profile and then update feature docs if behavior changed.

### Current Scanner Progress Snapshot (2026-07-21)

The scanner feature is in late implementation and stabilization. Core feature slices are implemented end-to-end through ViewModel and primary UI wiring, with recent work focused on UX polish, responsive behavior, and page-specific interaction fixes.

| Implementation Step | Status | Current State |
| --- | --- | --- |
| Step 1: Contracts, models, service interfaces | Complete | `Module_Scanner` contract/model surface is established and covered by baseline unit tests in `MTM_Receiving_Application.Tests/Unit/Module_Scanner`. |
| Step 2: MySQL DAO and stored procedures | Complete | Scanner persistence DAO path is implemented in `Module_Scanner/Data` and covered by DAO-focused tests under `MTM_Receiving_Application.Tests/Unit/Module_Scanner/Data`. |
| Step 3: Scanner services | Complete | Workflow orchestration, history persistence, and validation routing are implemented in `Module_Scanner/Services`, including Infor Visual-backed validation checks where required. |
| Step 4: ViewModel command/state flow | Complete | Workbench/history/settings ViewModel command and state behavior are implemented, including non-blocking add-line status and notes handling. |
| Step 5: View composition and x:Bind wiring | In progress (high completion) | Workbench, history, and settings views are wired and operational. The Manage Items modal is now implemented, and remaining work is concentrated on polish, route hardening, and final interaction refinements. |
| Step 6: DI and navigation route validation | In progress | Scanner navigation is functionally wired and exercised during feature validation; final hardening remains tied to full feature signoff pass. |
| Step 7: Feature test expansion and regression checks | In progress | Scanner-focused test runs are green and used iteratively during UI changes. Additional targeted coverage can still be added for edge interactions and final route cases. |
| Step 8: End-to-end ERP-target verification and final doc lock | Pending final signoff | Manual end-to-end verification and final documentation lock are reserved for release readiness after UX/behavior signoff. |

### Recently Completed Scanner UI Work

- Implemented mockup-aligned structural layout for scanner workbench, history, and settings surfaces.
- Completed scanner settings profile editor wiring for create, duplicate, delete, load, set default, safe defaults, and save/reset flows.
- Implemented the scanner Manage Items modal for add, duplicate, reorder, and delete batch editing.
- Applied visual polish pass for spacing, typography weight, and button/chip consistency.
- Applied responsive behavior pass to remove rigid overflow patterns and support window-resize workflows.
- Updated workbench interaction flow so fixed lower action controls remain anchored while upper content scrolls.
- Updated settings page behavior to align with split-pane expectations (static profile pane and independently scrollable editor pane).
- Updated scanner part entry to apply the same Receiving Part Formatting padding rules used by manual receiving when the part field is committed.
- Updated scanner location validation to tolerate dashed input by retrying the canonical no-dash Visual location ID before marking the row invalid.
- Implemented scanner batch revalidation from the workbench `Check All` action and added scanner settings save validation for required target metadata and per-user duplicate profile names.

### Validation Evidence (Current)

- Scanner feature changes are repeatedly validated with focused build and scanner-only tests.
- Latest known scanner-focused test slice status: 26 passed, 0 failed, 0 skipped.
- Build is currently green for changed scanner slices, with unrelated existing warnings remaining in `Module_Settings.Core`.

### Remaining Scanner Work Before Final Closure

- Complete Step 6 and Step 7 hardening tasks (route/DI edge validation and any final targeted test additions).
- Execute and document final end-to-end manual verification for configured ERP target profile behavior.
- Perform final scanner documentation lock after release-candidate behavior is approved.

Reference mockup set:

- docs/features/receiving/scanner-feature/mockups/01-scanner-workbench.png
- docs/features/receiving/scanner-feature/mockups/02-scanner-history.png
- docs/features/receiving/scanner-feature/mockups/03-scanner-settings.png
- docs/features/receiving/scanner-feature/mockups/04-modal-manage-batch-rows.png
- docs/features/receiving/scanner-feature/mockups/05-modal-save-draft.png
- docs/features/receiving/scanner-feature/mockups/06-modal-load-draft.png
- docs/features/receiving/scanner-feature/mockups/07-modal-configure-hotkeys.png

## AI Customization Entry Points

- `.github/copilot-instructions.md` — global repository rules
- `.github/instructions/README.md` — categorized instruction map
- `.github/prompts/README.md` — categorized prompt map
- `AGENTS.md` — repository agent contract
- `.github/archive/003-ai-documentation-update/` — pre-rewrite snapshot and archive manifest

## Documentation Maintenance

- Update active source-of-truth files when code or workflow changes invalidate them.
- Archive historical guidance instead of keeping conflicting active versions.
- Keep top-level docs short and point deeper only when a specialized instruction is needed.

## Low-Token Resume Prompt

Paste this into a new chat to continue with minimal context cost:

```text
Continue work in this repo from current workspace state.

Token minimization rules:
- Read only files directly needed for the current task.
- Prefer targeted diffs, avoid broad summaries.
- Reuse existing conventions, keep edits surgical.

Project constraints (must follow):
- MVVM boundary: View -> ViewModel -> Service -> DAO -> Database.
- Use x:Bind in XAML.
- MySQL writes via stored procedures only.
- Infor Visual SQL Server is read-only.

Current feature focus:
- Scanner feature is late-stage (Step 5 high completion; Step 6/7 hardening in progress).
- Root status source: README.md, section "Current Scanner Progress Snapshot (2026-07-21)".
- Detailed scanner docs: docs/features/receiving/scanner-feature/README.md.

Execution flow:
1. Ask me for the exact next task in one sentence if not already specified.
2. Inspect only directly relevant files.
3. Implement the smallest valid change.
4. Run narrow validation first (focused build/tests), expand only if needed.
5. Report: changed files, what was validated, and any remaining risks.

If the active task is UI polish, preserve all bindings/commands/converters/usings and do not alter architecture.
```
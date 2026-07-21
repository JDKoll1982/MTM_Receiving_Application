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

Current scanner progress:

- Step 1 is complete in `Module_Scanner` with baseline scanner tests in `MTM_Receiving_Application.Tests/Unit/Module_Scanner`.
- Step 2 is complete in `Module_Scanner/Data` with DAO-focused scanner tests in `MTM_Receiving_Application.Tests/Unit/Module_Scanner/Data`.

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
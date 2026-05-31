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
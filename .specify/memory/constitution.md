<!--
Sync Impact Report
- Version: template → 1.0.0
- Modified principles: none (template filled with 7 concrete principles)
- Added sections: Architecture & Technical Constraints; Development Workflow & Quality Gates
- Removed sections: none
- Templates requiring updates: .specify/templates/plan-template.md (updated), .specify/templates/spec-template.md (no change), .specify/templates/tasks-template.md (no change), commands templates (⚠ not present in repo; none updated)
- Follow-up TODOs: none
-->

# MTM Receiving Application Constitution

## Core Principles

### I. MVVM and View Purity (Non-Negotiable)
WinUI 3 views use x:Bind only; ViewModels are partial, inherit ViewModel_Shared_Base or ObservableObject, expose state via [ObservableProperty]/[RelayCommand], avoid code-behind business logic, and update UI solely through bindings and dispatcher-safe calls.

### II. Data Access Integrity
MySQL access uses stored procedures only; SQL Server (Infor Visual) is strictly read-only with ApplicationIntent=ReadOnly; DAOs are instance-based, async, and return Model_Dao_Result/Model_Dao_Result<T> without throwing; no raw SQL or static DAOs.

### III. CQRS + Mediator First
All workflows are modeled as commands/queries via MediatR with single-responsibility handlers; cross-cutting concerns (validation, logging, audit) run through pipeline behaviors; ViewModels depend on mediator rather than multi-method services.

### IV. Dependency Injection and Modular Boundaries
Constructor injection is mandatory; registrations live in App.xaml.cs; ViewModels are transient, infra/DAO services singleton unless stateful; module-specific logic remains within its module, Module_Core provides only generic infrastructure (error handling, dispatcher, windowing, logging scaffolding).

### V. Validation, Errors, and Structured Logging
FluentValidation enforces inputs/commands; IService_ErrorHandler contains user-facing handling; no exceptions leak to UI; Serilog produces structured, contextual logs with audit breadcrumbs; failure paths always return typed results.

### VI. Security and Session Discipline
Authentication/session flows honor workstation type, timeouts, and lockouts; auditability is preserved for user actions and data writes; secrets and connection strings are never embedded in code; read-only rules for Infor Visual are enforced everywhere.

### VII. Library-First Reuse
Prefer proven libraries over custom services: MediatR for orchestration, FluentValidation for rules, Serilog for logging, CsvHelper for exports, AutoMapper/Mapster for mapping, Scrutor for DI scanning/decorators, Polly for resilience, Ardalis.GuardClauses for guards, FluentAssertions/Bogus for tests; new utilities must justify gaps before custom code is added.

## Architecture and Technical Constraints
- Stack: WinUI 3 (.NET 8), CommunityToolkit.Mvvm, MediatR, FluentValidation, Serilog, CsvHelper, OpenTelemetry (future), MySQL 8, SQL Server (read-only). 
- Patterns: CQRS with pipeline behaviors; DAOs use Helper_Database_StoredProcedure; no static/global service locators; no raw SQL for MySQL; no writes to Infor Visual. 
- Performance/observability: structured logs on handler start/stop; capture timing at validation, handler, DAO, and end-to-end; favor caching on queries where safe; avoid blocking UI thread. 
- UI conventions: use x:Bind, keep Views free of business logic, window sizing via WindowHelper standards, converters from Module_Core where applicable. 
- Library-first mapping: prefer mapping libraries over manual mapping; prefer Scrutor for DI assembly scanning; apply Polly policies for external/unstable calls.

## Development Workflow and Quality Gates
- Specification-first: use Speckit (spec/plan/tasks) before implementation; Constitution Check must pass prior to coding. 
- Testing: unit tests for handlers/validators/DAOs; integration tests for DAOs against test DB; ViewModels tested with mediator mocks; test data via Bogus where appropriate; assertions via FluentAssertions. 
- Reviews and CI: code reviews verify principle compliance, DI registration, stored-procedure usage, and absence of raw SQL or static DAOs; builds run dotnet build/test; structured logging enabled. 
- Documentation: architecture/plan/spec/tasks kept in sync with changes; diagrams use PlantUML; changelog entries note principle-impacting changes. 
- Dependency hygiene: new packages require justification; DI registrations centralized; keep Module_Core free of module-specific services.

## Governance
- Authority: This constitution supersedes other practice docs for technical decisions; conflicts resolve in favor of these principles. 
- Amendments: Proposed via PR with rationale and migration notes; require version bump and update to Sync Impact Report; review must confirm downstream templates remain aligned. 
- Versioning: Semantic (MAJOR for breaking/removal of principles, MINOR for new/expanded principles, PATCH for clarifications); Last Amended updates on any change. 
- Compliance: Reviews/checklists must reference Core Principles; deviations require documented waivers and expiration; periodic audits validate DI registrations, DAO patterns, and UI binding rules.

**Version**: 1.0.0 | **Ratified**: 2026-01-16 | **Last Amended**: 2026-01-16

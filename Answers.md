# Module_Settings Core - Complete Recreation Specification (Aligned with Module Rebuild Guide)

**Version:** 3.1 (Aligned)  
**Date:** 2026-01-17  
**Scope:** Core Settings Infrastructure Only  
**Goal:** AI-assisted implementation plan for a *new* Core Settings Module  
**Repository:** JDKoll1982/MTM_Receiving_Application

---

## 1) Purpose

Create a **Core Settings Module** that provides shared settings infrastructure for the application.  
**Feature modules** (e.g., Module_Volvo, Module_Receiving, Module_Routing) remain responsible for **their own UI, view models, and feature-specific settings screens**, but they connect to this Core Settings Module for **storage, validation, auditing, and access**.

---

## 2) In Scope

The Core Settings Module must provide:

- **Settings storage access** (system-level + user-level settings)
- **Business data settings** (package types, package mappings, routing rules, scheduled reports)
- **Validation and security** rules enforcement
- **Audit logging**
- **Caching**
- **Developer tools** (database test view)

---

## 3) Out of Scope

The Core Settings Module **will NOT** include:

- Feature module settings UI (e.g., Module_Volvo settings screens)
- Feature module view models
- Feature module business logic
- Application shell navigation
- Workflow wizard or “settings hub” UI
- Reporting execution or data sync workflows

---

## 4) Architecture Boundaries

**Core Module Provides:**
- Storage contracts and APIs
- Validation rules and enforcement
- Audit log recording
- Caching
- Test tooling

**Feature Modules Provide:**
- Their own settings screens (views)
- Their own view models
- Their own UI interactions
- Their own domain logic

**Integration Rule:**  
Feature modules call Core Settings services for read/write access only.

---

## 5) Constitutional Constraints (Non‑Negotiable)

### I. MVVM Architecture
- ViewModels SHALL NOT directly call Data Access Objects
- ViewModels SHALL NOT access database helpers or connection strings
- All data access MUST flow through Service or Mediator layer
- All ViewModels MUST be partial classes
- All data binding MUST use compile-time binding (x:Bind)

### II. Database Layer
- All MySQL operations MUST use stored procedures (no raw SQL)
- All Data Access Objects MUST return structured result objects
- Data Access Objects MUST be instance-based
- SQL Server (Infor Visual) is READ ONLY

### III. Dependency Injection
- All services MUST be registered in central configuration
- Constructor injection REQUIRED
- Service locator pattern is FORBIDDEN

### IV. Error Handling
- Use centralized error handler for user-facing errors
- Use structured logging for diagnostics
- Data Access Objects MUST NOT throw exceptions (return failure results)

### V. Code Quality
- Explicit accessibility modifiers required
- Braces required for all control flow statements
- Async methods MUST end with "Async" suffix
- XML documentation required for public APIs

---

## 6) Target Architecture (Aligned with Rebuild Guide)

### 6.1 Patterns & Libraries (Required)
- **CQRS** (MediatR 12.0+)
- **Structured Logging** (Serilog 3.1+)
- **Declarative Validation** (FluentValidation 11.8+)
- **CSV Export** (CsvHelper 30.0+) — only if Core Settings needs export features

### 6.2 Architectural Layers
- **Presentation Layer:** Core Settings admin/debug views (no feature module UI)
- **Application Layer:** CQRS handlers, pipeline behaviors
- **Validation Layer:** FluentValidation validators
- **Data Access Layer:** Instance-based DAOs (stored procedures only)
- **Domain Layer:** Models and result types

---

## 7) Data Contracts (Logical)

### 7.1 Core Settings Data
- System settings (global)
- User settings (per-user overrides)
- Settings categories and keys
- Setting metadata (data type, default, access level, locked flag)

### 7.2 Business Data Settings
- Package Types
- Package Type Mappings
- Routing Rules
- Scheduled Reports

### 7.3 Audit Log
- Who changed what
- When changes occurred
- Old vs. new value

---

## 8) Services Required (CQRS‑Aligned)

### 8.1 Core Settings Queries
- GetSystemSettingsQuery
- GetUserSettingsQuery
- GetSettingMetadataQuery
- GetBusinessSettingsQuery (Package Types, Mappings, Routing, Reports)

### 8.2 Core Settings Commands
- UpdateSystemSettingCommand
- UpdateUserSettingCommand
- UpsertBusinessSettingCommand
- DeleteBusinessSettingCommand

### 8.3 Pipeline Behaviors
- **LoggingBehavior** (Serilog)
- **ValidationBehavior** (FluentValidation)
- **AuditBehavior** (mandatory for all write commands)
- **Optional TransactionBehavior**

### 8.4 Cache Service
- Cache read-through for settings
- Invalidate cache on write commands
- Resolve user override precedence

---

## 9) Validation Rules

- All commands require FluentValidation validators
- Enforce data types, ranges, regex, enum
- Custom error messages required
- Validators must be registered in DI via auto-discovery

---

## 10) Audit & Observability

### 10.1 Audit Trail Requirements
- Log every insert/update/delete command
- Include UserId, SessionId, Timestamp, Command type
- Exclude sensitive data from logs

### 10.2 Structured Logging
- No string interpolation in log statements
- Use semantic properties for filtering
- Include machine name, thread ID, module name, environment

---

## 11) Error Handling Strategy

Each handler must:
- Wrap logic in try/catch
- Return failure result (no exceptions thrown from DAO)
- Log with full context (Serilog)

---

## 12) Developer Tooling

A **Database Test View** must exist in the Core Settings Module to verify:

- Connectivity
- Stored procedures
- Table reads/writes
- DAO/service health

---

## 13) Acceptance Criteria

The Core Settings Module is complete when:

- Feature modules can retrieve and persist settings through Core APIs
- Validation, caching, and audit logging are enforced
- Business data settings are accessible via Core APIs
- Developer test tooling works end-to-end
- No feature-module UI exists inside Module_Settings
- CQRS handlers replace any monolithic service methods
- All public APIs have XML documentation
- No architectural violations (per constraints above)

---

## 14) Testing & Quality Targets

- **80%+ unit test coverage** for handlers and validators
- Integration tests for DAOs and stored procedures
- No compiler warnings
- Compliance with MVVM and DI constraints

---

## 15) Code Review Checklist Requirement

If `CODE_REVIEW_CHECKLIST.md` does not exist at module root, create it and enforce:

- MVVM compliance (partial ViewModels, x:Bind)
- Handler single responsibility
- Validation for all commands
- Structured logging
- Tests for handlers/validators

---

## 16) Documentation Requirements

At minimum (module root unless specified):

- README.md
- ARCHITECTURE.md
- DATA_MODEL.md
- WORKFLOWS.md
- DEFAULTS.md
- TROUBLESHOOTING.md
- Preparation/* (planning files)

---

**End of Specification (Aligned)**  
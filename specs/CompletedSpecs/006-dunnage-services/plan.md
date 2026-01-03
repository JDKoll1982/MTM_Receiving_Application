# Implementation Plan: Dunnage Services Layer

**Branch**: `006-dunnage-services` | **Date**: 2025-12-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-dunnage-services/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement the service layer for the Dunnage Receiving System V2, providing three core services:
1. **Service_DunnageWorkflow** - Singleton state machine managing wizard navigation (Mode → Type → Part → Quantity → Details → Review)
2. **Service_MySQL_Dunnage** - Transient data access service wrapping DAOs with business logic validation
3. **Service_DunnageCSVWriter** - Transient export service creating CSV files for LabelView label printing

This service layer sits between ViewModels and DAOs, adding validation, state management, and cross-cutting concerns like logging and error handling.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: CommunityToolkit.Mvvm, CsvHelper, System.Text.Json, MySql.Data  
**Storage**: MySQL 5.7.24 (app data), File System (CSV export)  
**Testing**: xUnit, Moq  
**Target Platform**: Windows 10/11 (WinUI 3, Windows App SDK 1.8+)  
**Project Type**: Desktop Application (Single Project)  
**Performance Goals**: Service methods < 1 second execution time (excluding network I/O)  
**Constraints**: MySQL 5.7 limitations (no JSON functions), Offline-capable CSV export (local fallback)  
**Scale/Scope**: Manufacturing receiving workflow supporting 3 user stories (workflow orchestration, CRUD operations, CSV export)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](../../.specify/memory/constitution.md) v1.1.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Services (no UI) - ViewModels will consume these services
- [x] **II. Database Layer**: All DAOs return Model_Dao_Result pattern, async operations only
- [x] **III. Dependency Injection**: All 3 services registered in App.xaml.cs with interfaces
- [x] **IV. Error Handling & Logging**: Services use ILoggingService and IService_ErrorHandler
- [x] **V. Security & Authentication**: IService_UserSessionManager for user auditing on write operations
- [x] **VI. WinUI 3 Modern Practices**: N/A (service layer, no UI)
- [x] **VII. Specification-Driven**: Following Speckit workflow (spec.md → plan.md → tasks.md)

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL only (no Infor Visual queries in this feature)
- [x] **MySQL 5.7.24 Compatible**: No JSON functions in stored procedures (validation in C# with System.Text.Json)
- [x] **Required Packages**: CommunityToolkit.Mvvm (ViewModels only), MySql.Data (via DAOs), CsvHelper (new)

### Critical Constraints

- [x] **Infor Visual READ ONLY**: N/A - This feature does NOT query Infor Visual database
- [x] **Forbidden Practices**: 
  - [x] No direct SQL (all MySQL operations via stored procedures)
  - [x] No DAO exceptions (Model_Dao_Result pattern enforced)
  - [x] No logic in code-behind (N/A - service layer)
  - [x] No service locator (constructor injection only)

### Justification for Violations
None. All constitutional principles are followed.

## Project Structure

### Documentation (this feature)

```text
specs/006-dunnage-services/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── IService_DunnageWorkflow.md
│   ├── IService_MySQL_Dunnage.md
│   └── IService_DunnageCSVWriter.md
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Contracts/
└── Services/
    ├── IService_DunnageWorkflow.cs
    ├── IService_MySQL_Dunnage.cs
    └── IService_DunnageCSVWriter.cs

Services/
├── Receiving/
│   ├── Service_DunnageWorkflow.cs
│   └── Service_DunnageCSVWriter.cs
└── Database/
    └── Service_MySQL_Dunnage.cs

Models/
├── Enums/
│   └── Enum_DunnageWorkflowStep.cs
└── Receiving/
    ├── Model_WorkflowStepResult.cs
    ├── Model_SaveResult.cs
    └── Model_CSVWriteResult.cs

```

**Structure Decision**: Single project desktop application. Services layer only (no UI changes in this feature). Follows existing project conventions with services registered in App.xaml.cs DI container.

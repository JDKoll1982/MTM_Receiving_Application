# Implementation Plan: Routing Module

**Branch**: `015-routing-module` | **Date**: 2026-01-03 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/015-routing-module/spec.md`

## Summary

Implement Routing module for internal routing labels with consistent naming conventions (`ViewModel_Routing_*`, `View_Routing_*`, `Service_Routing_*`), clear module boundaries, daily history tracking, auto-lookup, and label numbering. Module follows MVVM architecture with workflow state machine and MySQL database operations via stored procedures.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.8+), CommunityToolkit.Mvvm 8.x, MySql.Data 8.x  
**Storage**: MySQL 8.x (mtm_receiving_application) READ/WRITE  
**Testing**: Manual user testing (no automated tests in MVP)  
**Target Platform**: Windows 10/11 Desktop  
**Project Type**: Single WinUI 3 application (desktop)  
**Performance Goals**: Database operations <500ms, UI interactions <100ms response  
**Constraints**: MySQL 5.7.24 compatible (no JSON functions/CTEs/window functions), stored procedures only for MySQL  
**Scale/Scope**: 5 ViewModels/Views, ~3 Services, ~3 DAOs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](../../.specify/memory/constitution.md) v1.0.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Plan includes ViewModels (ViewModel_Routing_*), Views (View_Routing_*), Models (Model_Routing_*), Services (Service_Routing_*)
- [x] **II. Database Layer**: All DB operations via stored procedures (sp_routing_*), DAOs return Model_Dao_Result, async operations only
- [x] **III. Dependency Injection**: Services/DAOs registered in App.xaml.cs with interfaces
- [x] **IV. Error Handling & Logging**: IService_ErrorHandler for exceptions, ILoggingService for audit trail
- [x] **V. Security & Authentication**: Employee number from existing auth context
- [x] **VI. WinUI 3 Modern Practices**: x:Bind (Mode=OneWay/TwoWay), ObservableCollection, [ObservableProperty], [RelayCommand], async/await
- [x] **VII. Specification-Driven**: Follows Speckit workflow (spec → plan → data-model → tasks)

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL for app data
- [x] **MySQL 5.7.24 Compatible**: No JSON/CTEs/window functions, stored procedures only, compatible data types
- [x] **Required Packages**: CommunityToolkit.Mvvm, MySql.Data

### Critical Constraints

- [x] **Forbidden Practices**: No raw SQL (stored procedures only), no DAO exceptions (return error results), no logic in code-behind (XAML only), no service locator (DI only)

### Justification for Violations

None - all practices align with constitution.

## Project Structure

### Documentation (this feature)

```text
specs/015-routing-module/
├── spec.md            # Feature specification
├── plan.md            # Implementation plan (this file)
├── data-model.md      # Database schema
├── tasks.md           # Task list
├── quickstart.md      # Developer quick start guide
├── research.md        # Legacy system analysis
├── README.md          # Module overview
└── contracts/         # Service interfaces
    ├── IService_Routing.cs
    ├── IService_Routing_History.cs
    └── IService_Routing_RecipientLookup.cs
```

### Source Code (repository root)

```text
MTM_Receiving_Application/ (root)
├── RoutingModule/           ⬅️ NEW (module directory)
│   ├── Models/
│   │   ├── Model_Routing_Label.cs
│   │   ├── Model_Routing_Recipient.cs
│   │   └── Model_Routing_Session.cs
│   ├── ViewModels/
│   │   ├── ViewModel_Routing_LabelEntry.cs
│   │   ├── ViewModel_Routing_History.cs
│   │   └── ViewModel_Routing_Workflow.cs
│   ├── Views/
│   │   ├── View_Routing_LabelEntry.xaml (+ .cs)
│   │   ├── View_Routing_History.xaml (+ .cs)
│   │   └── View_Routing_Workflow.xaml (+ .cs)
│   ├── Services/
│   │   ├── Service_Routing.cs
│   │   ├── Service_Routing_History.cs
│   │   └── Service_Routing_RecipientLookup.cs
│   ├── Data/
│   │   ├── Dao_Routing_Label.cs
│   │   └── Dao_Routing_Recipient.cs
│   ├── Enums/
│   │   └── Enum_Routing_WorkflowStep.cs
│   └── Interfaces/
│       ├── IService_Routing.cs
│       ├── IService_Routing_History.cs
│       └── IService_Routing_RecipientLookup.cs
├── Database/
│   ├── Schemas/
│   │   └── schema_routing.sql (table definitions)
│   ├── StoredProcedures/
│   │   └── Routing/
│   │       ├── sp_routing_label_insert.sql
│   │       ├── sp_routing_label_get_history.sql
│   │       └── sp_routing_recipient_get_all.sql
│   └── TestData/
│       └── routing_sample_data.sql
└── App.xaml.cs (register Routing services in DI container)
```

**Structure Decision**: Root-level `RoutingModule/` directory with consistent naming (`ViewModel_Routing_*`, `View_Routing_*`, `Service_Routing_*`). Integrates with shared infrastructure (BaseViewModel, ErrorHandler, Logging, Settings, Reporting). Database scripts in `Database/` folder following established conventions.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| N/A | N/A | N/A |

---

**Reference**: See [../011-module-reimplementation/plan.md](../011-module-reimplementation/plan.md) for complete implementation plan context


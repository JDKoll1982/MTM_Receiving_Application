# Implementation Plan: Receiving Module

**Branch**: `013-receiving-module` | **Date**: 2026-01-03 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/013-receiving-module/spec.md`

## Summary

Implement Receiving module reimplementation with consistent naming conventions (`ViewModel_Receiving_*`, `View_Receiving_*`, `Service_Receiving_*`), clear module boundaries, and bug fixes (Add Another Part). Module follows MVVM architecture with workflow state machine, Infor Visual integration (read-only), and MySQL database operations via stored procedures.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.8+), CommunityToolkit.Mvvm 8.x, MySql.Data 8.x, Microsoft.Data.SqlClient 5.x  
**Storage**: MySQL 8.x (mtm_receiving_application) READ/WRITE, SQL Server (Infor Visual MTMFG) READ ONLY  
**Testing**: Manual user testing (no automated tests in MVP)  
**Target Platform**: Windows 10/11 Desktop  
**Project Type**: Single WinUI 3 application (desktop)  
**Performance Goals**: Database operations <500ms, UI interactions <100ms response  
**Constraints**: MySQL 5.7.24 compatible (no JSON functions/CTEs/window functions), Infor Visual READ ONLY, stored procedures only for MySQL  
**Scale/Scope**: 10 ViewModels/Views, ~5 Services, ~3 DAOs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](../../.specify/memory/constitution.md) v1.0.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Plan includes ViewModels (ViewModel_Receiving_*), Views (View_Receiving_*), Models (Model_Receiving_*), Services (Service_Receiving_*)
- [x] **II. Database Layer**: All DB operations via stored procedures (sp_receiving_*), DAOs return Model_Dao_Result, async operations only
- [x] **III. Dependency Injection**: Services/DAOs registered in App.xaml.cs with interfaces
- [x] **IV. Error Handling & Logging**: IService_ErrorHandler for exceptions, ILoggingService for audit trail
- [x] **V. Security & Authentication**: Employee number from existing auth context
- [x] **VI. WinUI 3 Modern Practices**: x:Bind (Mode=OneWay/TwoWay), ObservableCollection, [ObservableProperty], [RelayCommand], async/await
- [x] **VII. Specification-Driven**: Follows Speckit workflow (spec → plan → data-model → tasks)

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL for app data, SQL Server for Infor Visual (READ ONLY)
- [x] **MySQL 5.7.24 Compatible**: No JSON/CTEs/window functions, stored procedures only, compatible data types
- [x] **Required Packages**: CommunityToolkit.Mvvm, MySql.Data, Microsoft.Data.SqlClient

### Critical Constraints

- [x] **Infor Visual READ ONLY**: Connection uses ApplicationIntent=ReadOnly, only SELECT queries allowed
- [x] **Forbidden Practices**: No raw SQL (stored procedures only), no DAO exceptions (return error results), no logic in code-behind (XAML only), no service locator (DI only)

### Justification for Violations

None - all practices align with constitution.

## Project Structure

### Documentation (this feature)

```text
specs/013-receiving-module/
├── spec.md            # This file
├── plan.md            # Implementation plan (this file)
├── data-model.md      # Database schema
├── tasks.md           # Task list
├── quickstart.md      # Developer quick start guide
├── research.md        # Legacy system analysis
├── README.md          # Module overview
└── contracts/         # Service interfaces
    ├── IService_ReceivingWorkflow.cs
    ├── IService_InforVisual.cs
    ├── IService_MySQL_Receiving.cs
    └── IService_ReceivingValidation.cs
```

### Source Code (repository root)

```text
MTM_Receiving_Application/ (root)
├── ReceivingModule/           ⬅️ NEW (module directory)
│   ├── Models/
│   │   ├── Model_Receiving_Load.cs
│   │   ├── Model_Receiving_Line.cs
│   │   └── Model_Receiving_Session.cs
│   ├── ViewModels/
│   │   ├── ViewModel_Receiving_ModeSelection.cs
│   │   ├── ViewModel_Receiving_POEntry.cs
│   │   ├── ViewModel_Receiving_PackageType.cs
│   │   ├── ViewModel_Receiving_LoadEntry.cs
│   │   ├── ViewModel_Receiving_WeightQuantity.cs
│   │   ├── ViewModel_Receiving_HeatLot.cs
│   │   ├── ViewModel_Receiving_Review.cs
│   │   ├── ViewModel_Receiving_Workflow.cs
│   │   ├── ViewModel_Receiving_EditMode.cs
│   │   └── ViewModel_Receiving_ManualEntry.cs
│   ├── Views/
│   │   ├── View_Receiving_ModeSelection.xaml (+ .cs)
│   │   ├── View_Receiving_POEntry.xaml (+ .cs)
│   │   ├── View_Receiving_PackageType.xaml (+ .cs)
│   │   ├── View_Receiving_LoadEntry.xaml (+ .cs)
│   │   ├── View_Receiving_WeightQuantity.xaml (+ .cs)
│   │   ├── View_Receiving_HeatLot.xaml (+ .cs)
│   │   ├── View_Receiving_Review.xaml (+ .cs)
│   │   ├── View_Receiving_Workflow.xaml (+ .cs)
│   │   ├── View_Receiving_EditMode.xaml (+ .cs)
│   │   └── View_Receiving_ManualEntry.xaml (+ .cs)
│   ├── Services/
│   │   ├── Service_Receiving_Workflow.cs
│   │   ├── Service_Receiving_Validation.cs
│   │   ├── Service_Receiving_SessionManager.cs
│   │   └── Service_Receiving_CSVWriter.cs
│   ├── Data/
│   │   ├── Dao_Receiving_Load.cs
│   │   ├── Dao_Receiving_Line.cs
│   │   └── Dao_Receiving_PackageTypePreference.cs
│   ├── Enums/
│   │   └── Enum_Receiving_WorkflowStep.cs
│   └── Interfaces/
│       ├── IService_Receiving_Workflow.cs
│       ├── IService_Receiving_Validation.cs
│       └── IService_Receiving_SessionManager.cs
├── Database/
│   ├── Schemas/
│   │   └── schema_receiving.sql (table definitions)
│   ├── StoredProcedures/
│   │   └── Receiving/
│   │       ├── sp_receiving_load_insert.sql
│   │       ├── sp_receiving_load_get_by_date_range.sql
│   │       └── sp_receiving_load_update.sql
│   └── TestData/
│       └── receiving_sample_data.sql
└── App.xaml.cs (register Receiving services in DI container)
```

**Structure Decision**: Root-level `ReceivingModule/` directory with consistent naming (`ViewModel_Receiving_*`, `View_Receiving_*`, `Service_Receiving_*`). Integrates with shared infrastructure (BaseViewModel, ErrorHandler, Logging, Settings, Reporting). Database scripts in `Database/` folder following established conventions.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| N/A | N/A | N/A |

---

**Reference**: See [../011-module-reimplementation/plan.md](../011-module-reimplementation/plan.md) for complete implementation plan context


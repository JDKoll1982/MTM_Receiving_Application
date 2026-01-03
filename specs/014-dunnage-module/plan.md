# Implementation Plan: Dunnage Module

**Branch**: `014-dunnage-module` | **Date**: 2026-01-03 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/014-dunnage-module/spec.md`

## Summary

Implement Dunnage module reimplementation with consistent naming conventions (`ViewModel_Dunnage_*`, `View_Dunnage_*`, `Service_Dunnage_*`), clear module boundaries, and admin capabilities. Module follows MVVM architecture with workflow state machine, Material.Icons integration, dynamic form generation, and MySQL database operations via stored procedures.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.8+), CommunityToolkit.Mvvm 8.x, Material.Icons.WinUI3, MySql.Data 8.x  
**Storage**: MySQL 8.x (mtm_receiving_application) READ/WRITE  
**Testing**: Manual user testing (no automated tests in MVP)  
**Target Platform**: Windows 10/11 Desktop  
**Project Type**: Single WinUI 3 application (desktop)  
**Performance Goals**: Database operations <500ms, UI interactions <100ms response  
**Constraints**: MySQL 5.7.24 compatible (no JSON functions/CTEs/window functions), stored procedures only for MySQL  
**Scale/Scope**: 14 ViewModels/Views, ~4 Services, ~7 DAOs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](../../.specify/memory/constitution.md) v1.0.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Plan includes ViewModels (ViewModel_Dunnage_*), Views (View_Dunnage_*), Models (Model_Dunnage_*), Services (Service_Dunnage_*)
- [x] **II. Database Layer**: All DB operations via stored procedures (sp_dunnage_*), DAOs return Model_Dao_Result, async operations only
- [x] **III. Dependency Injection**: Services/DAOs registered in App.xaml.cs with interfaces
- [x] **IV. Error Handling & Logging**: IService_ErrorHandler for exceptions, ILoggingService for audit trail
- [x] **V. Security & Authentication**: Employee number from existing auth context
- [x] **VI. WinUI 3 Modern Practices**: x:Bind (Mode=OneWay/TwoWay), ObservableCollection, [ObservableProperty], [RelayCommand], async/await
- [x] **VII. Specification-Driven**: Follows Speckit workflow (spec → plan → data-model → tasks)

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL for app data
- [x] **MySQL 5.7.24 Compatible**: No JSON/CTEs/window functions, stored procedures only, compatible data types
- [x] **Required Packages**: CommunityToolkit.Mvvm, Material.Icons.WinUI3, MySql.Data

### Critical Constraints

- [x] **Forbidden Practices**: No raw SQL (stored procedures only), no DAO exceptions (return error results), no logic in code-behind (XAML only), no service locator (DI only)

### Justification for Violations

None - all practices align with constitution.

## Project Structure

### Documentation (this feature)

```text
specs/014-dunnage-module/
├── spec.md            # Feature specification
├── plan.md            # Implementation plan (this file)
├── data-model.md      # Database schema
├── tasks.md           # Task list
├── quickstart.md      # Developer quick start guide
├── research.md        # Legacy system analysis
├── README.md          # Module overview
└── contracts/         # Service interfaces
    ├── IService_DunnageWorkflow.cs
    ├── IService_DunnageAdminWorkflow.cs
    └── IService_DunnageCSVWriter.cs
```

### Source Code (repository root)

```text
MTM_Receiving_Application/ (root)
├── Module_Dunnage/           ⬅️ NEW (module directory)
│   ├── Models/
│   │   ├── Model_Dunnage_Type.cs
│   │   ├── Model_Dunnage_Part.cs
│   │   ├── Model_Dunnage_Load.cs
│   │   ├── Model_Dunnage_Spec.cs
│   │   └── Model_Dunnage_Session.cs
│   ├── ViewModels/
│   │   ├── ViewModel_Dunnage_TypeSelection.cs
│   │   ├── ViewModel_Dunnage_PartSelection.cs
│   │   ├── ViewModel_Dunnage_DetailsEntry.cs
│   │   ├── ViewModel_Dunnage_QuantityEntry.cs
│   │   ├── ViewModel_Dunnage_Review.cs
│   │   ├── ViewModel_Dunnage_AdminTypes.cs
│   │   ├── ViewModel_Dunnage_AdminParts.cs
│   │   └── ViewModel_Dunnage_AdminInventory.cs
│   ├── Views/
│   │   ├── View_Dunnage_TypeSelection.xaml (+ .cs)
│   │   ├── View_Dunnage_PartSelection.xaml (+ .cs)
│   │   ├── View_Dunnage_DetailsEntry.xaml (+ .cs)
│   │   ├── View_Dunnage_QuantityEntry.xaml (+ .cs)
│   │   ├── View_Dunnage_Review.xaml (+ .cs)
│   │   ├── View_Dunnage_AdminTypes.xaml (+ .cs)
│   │   ├── View_Dunnage_AdminParts.xaml (+ .cs)
│   │   └── View_Dunnage_AdminInventory.xaml (+ .cs)
│   ├── Services/
│   │   ├── Service_Dunnage_Workflow.cs
│   │   ├── Service_Dunnage_AdminWorkflow.cs
│   │   └── Service_Dunnage_CSVWriter.cs
│   ├── Data/
│   │   ├── Dao_Dunnage_Type.cs
│   │   ├── Dao_Dunnage_Part.cs
│   │   ├── Dao_Dunnage_Load.cs
│   │   └── Dao_Dunnage_Spec.cs
│   ├── Enums/
│   │   ├── Enum_Dunnage_WorkflowStep.cs
│   │   └── Enum_Dunnage_AdminSection.cs
│   └── Interfaces/
│       ├── IService_Dunnage_Workflow.cs
│       ├── IService_Dunnage_AdminWorkflow.cs
│       └── IService_Dunnage_CSVWriter.cs
├── Database/
│   ├── Schemas/
│   │   └── schema_dunnage.sql (table definitions)
│   ├── StoredProcedures/
│   │   └── Dunnage/
│   │       ├── sp_dunnage_type_insert.sql
│   │       ├── sp_dunnage_part_insert.sql
│   │       └── sp_dunnage_load_insert.sql
│   └── TestData/
│       └── dunnage_sample_data.sql
└── App.xaml.cs (register Dunnage services in DI container)
```

**Structure Decision**: Root-level `Module_Dunnage/` directory with consistent naming (`ViewModel_Dunnage_*`, `View_Dunnage_*`, `Service_Dunnage_*`). Integrates with shared infrastructure (BaseViewModel, ErrorHandler, Logging, Settings, Reporting). Database scripts in `Database/` folder following established conventions.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| N/A | N/A | N/A |

---

**Reference**: See [../011-module-reimplementation/plan.md](../011-module-reimplementation/plan.md) for complete implementation plan context


# Implementation Plan: Reporting Module

**Branch**: `003-reporting-module` | **Date**: 2026-01-03 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/003-reporting-module/spec.md`

## Summary

Implement End-of-Day Reporting module as a cross-cutting module that works across Receiving, Dunnage, Routing, and Volvo modules. Module provides date range filtering, PO number normalization, data grouping, CSV export, and email formatting. Follows MVVM architecture with service-based integration to other modules.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.8+), CommunityToolkit.Mvvm 8.x, MySql.Data 8.x  
**Storage**: MySQL 8.x (mtm_receiving_application) READ/WRITE via views  
**Testing**: Manual user testing (no automated tests in MVP)  
**Target Platform**: Windows 10/11 Desktop  
**Project Type**: Single WinUI 3 application (desktop)  
**Performance Goals**: Report generation <2 seconds for date ranges up to 30 days  
**Constraints**: MySQL 5.7.24 compatible (no JSON functions/CTEs/window functions), uses views for data aggregation  
**Scale/Scope**: 2 ViewModels/Views, ~1 Service, integrates with all other modules

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](../../.specify/memory/constitution.md) v1.0.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Plan includes ViewModels (ViewModel_Reporting_*), Views (View_Reporting_*), Services (Service_Reporting_*)
- [x] **II. Database Layer**: Uses views (vw_receiving_history, vw_dunnage_history, vw_routing_history) for data aggregation, no raw SQL
- [x] **III. Dependency Injection**: Services registered in App.xaml.cs with interfaces
- [x] **IV. Error Handling & Logging**: IService_ErrorHandler for exceptions, ILoggingService for audit trail
- [x] **V. Security & Authentication**: Employee number from existing auth context
- [x] **VI. WinUI 3 Modern Practices**: x:Bind (Mode=OneWay/TwoWay), ObservableCollection, [ObservableProperty], [RelayCommand], async/await
- [x] **VII. Specification-Driven**: Follows Speckit workflow (spec → plan → data-model → tasks)

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL for app data (via views)
- [x] **MySQL 5.7.24 Compatible**: No JSON/CTEs/window functions, views use compatible SQL
- [x] **Required Packages**: CommunityToolkit.Mvvm, MySql.Data

### Critical Constraints

- [x] **Forbidden Practices**: No raw SQL (use views), no DAO exceptions (return error results), no logic in code-behind (XAML only), no service locator (DI only)

### Justification for Violations

None - all practices align with constitution.

## Project Structure

### Documentation (this feature)

```text
specs/003-reporting-module/
├── spec.md            # Feature specification
├── plan.md            # Implementation plan (this file)
├── data-model.md      # Database views and data structures
├── tasks.md           # Task list
├── quickstart.md      # Developer quick start guide
├── research.md        # Google Sheets analysis
├── README.md          # Module overview
└── contracts/         # Service interfaces
    └── IService_Reporting.cs
```

### Source Code (repository root)

```text
MTM_Receiving_Application/ (root)
├── Module_Reporting/           ⬅️ NEW (module directory)
│   ├── Models/
│   │   └── Model_ReportRow.cs
│   ├── ViewModels/
│   │   ├── ViewModel_Reporting_Main.cs
│   │   └── ViewModel_Reporting_ReportGenerator.cs
│   ├── Views/
│   │   ├── View_Reporting_Main.xaml (+ .cs)
│   │   └── View_Reporting_ReportGenerator.xaml (+ .cs)
│   ├── Services/
│   │   └── Service_Reporting.cs
│   ├── Data/
│   │   └── Dao_Reporting.cs
│   └── Module_Core/Contracts/Services/
│       └── IService_Reporting.cs
├── Database/
│   ├── Schemas/
│   │   └── schema_reporting_views.sql (views: vw_receiving_history, vw_dunnage_history, vw_routing_history)
│   └── TestData/
│       └── reporting_sample_data.sql
└── App.xaml.cs (register Reporting services in DI container)
```

**Structure Decision**: Root-level `Module_Reporting/` directory with consistent naming (`ViewModel_Reporting_*`, `View_Reporting_*`, `Service_Reporting_*`). Integrates with shared infrastructure (BaseViewModel, ErrorHandler, Logging, Settings). Uses views from other modules for data aggregation. Database views in `Database/Schemas/` folder following established conventions.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| N/A | N/A | N/A |

---

**Reference**: See [../011-module-reimplementation/plan.md](../011-module-reimplementation/plan.md) for complete implementation plan context


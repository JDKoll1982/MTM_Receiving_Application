# Implementation Plan: Volvo Dunnage Requisition Module

**Branch**: `002-volvo-module` | **Date**: 2026-01-03 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/002-volvo-module/spec.md`

## Summary

Implement Volvo dunnage requisition module to streamline receiving workflow:  physically receive Volvo shipments → enter parts/skids → track discrepancies → calculate component explosion → generate labels for LabelView → create formatted PO requisition email → save as pending → complete with PO after purchasing responds. Admin manages master data (parts, quantities, components) via Settings.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.8+), CommunityToolkit.Mvvm 8.x, MySql.Data 8.x  
**Storage**: MySQL 8.x (mtm_receiving_application database), LabelView CSV files  
**Testing**: Manual user testing (no automated tests in MVP)  
**Target Platform**: Windows 10/11 Desktop  
**Project Type**: Single WinUI 3 application (desktop)  
**Performance Goals**: Sub-second component explosion calculation for 50-part shipments, <2s label CSV generation  
**Constraints**: MySQL 5.7.24 compatible (no JSON functions/CTEs/window functions), LabelView 2022 CSV format compatibility, only one pending PO at a time  
**Scale/Scope**: 2-3 shipments/week, ~10-20 parts per shipment, 100+ Volvo master parts in catalog

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](.specify/memory/constitution.md) v1.0.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Plan includes ViewModels (VolvoShipmentViewModel, VolvoHistoryViewModel, VolvoSettingsViewModel), Views (XAML pages), Models (shipment, lines, master parts), Services (IVolvoService, IVolvoMasterDataService)
- [x] **II. Database Layer**: All DB operations via stored procedures (sp_volvo_*), DAOs return Model_Dao_Result, async operations only
- [x] **III. Dependency Injection**: Services/DAOs registered in App.xaml.cs with interfaces
- [x] **IV. Error Handling & Logging**: IService_ErrorHandler for exceptions, ILoggingService for audit trail
- [x] **V. Security & Authentication**: Employee number from existing auth context
- [x] **VI. WinUI 3 Modern Practices**: x:Bind (Mode=OneWay/TwoWay), ObservableCollection, [ObservableProperty], [RelayCommand], async/await
- [x] **VII. Specification-Driven**: Follows Speckit workflow (spec → plan → data-model → tasks)

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL for Volvo data (no SQL Server interaction for this module)
- [x] **MySQL 5.7.24 Compatible**: No JSON/CTEs/window functions, stored procedures only, compatible data types
- [x] **Required Packages**: CommunityToolkit.Mvvm, MySql.Data

### Critical Constraints

- [x] **Infor Visual READ ONLY**: N/A - This module does not query Infor Visual (user manually receives PO in Visual after purchasing provides it)
- [x] **Forbidden Practices**: No raw SQL (stored procedures only), no DAO exceptions (return error results), no logic in code-behind (XAML only), no service locator (DI only)

### Justification for Violations
None - all practices align with constitution.

## Project Structure

### Documentation (this feature)

```text
specs/002-volvo-module/
├── plan.md            # This file
├── spec.md            # Feature specification (created)
├── data-model.md      # Database schema (to be created)
├── tasks.md           # Task list (to be created)
└── workflows/         # PlantUML workflow diagrams (created)
    ├── workflow-1-label-generation.puml
    ├── workflow-2-history-archival.puml
    ├── workflow-3-master-data-admin.puml
    ├── workflow-4-reporting.puml
    └── workflow-5-customer-packlist-export.puml (future)
```

### Source Code (repository root)

```text
MTM_Receiving_Application/ (root)
├── Models/
│   └── Volvo/
│       ├── Model_VolvoShipment.cs
│       ├── Model_VolvoShipmentLine.cs
│       ├── Model_VolvoPart.cs
│       └── Model_VolvoPartComponent.cs
├── ViewModels/
│   └── Volvo/
│       ├── VolvoShipmentEntryViewModel.cs
│       ├── VolvoReviewViewModel.cs
│       ├── VolvoHistoryViewModel.cs
│       └── VolvoSettingsViewModel.cs
├── Views/
│   └── Volvo/
│       ├── VolvoShipmentEntryView.xaml (+ .cs)
│       ├── VolvoReviewView.xaml (+ .cs)
│       ├── VolvoHistoryView.xaml (+ .cs)
│       └── VolvoSettingsView.xaml (+ .cs - replaces Settings_PlaceholderView)
├── Services/
│   └── Volvo/
│       ├── IVolvoService.cs (interface)
│       ├── VolvoService.cs (implementation)
│       ├── IVolvoMasterDataService.cs
│       └── VolvoMasterDataService.cs
├── Data/
│   └── Volvo/
│       ├── Dao_VolvoShipment.cs
│       ├── Dao_VolvoShipmentLine.cs
│       ├── Dao_VolvoPart.cs
│       └── Dao_VolvoPartComponent.cs
├── Database/
│   ├── Schemas/
│   │   └── schema_volvo.sql (table definitions)
│   ├── StoredProcedures/
│   │   ├── sp_volvo_shipment_insert.sql
│   │   ├── sp_volvo_shipment_update.sql
│   │   ├── sp_volvo_shipment_complete.sql
│   │   ├── sp_volvo_shipment_get_pending.sql
│   │   ├── sp_volvo_shipment_history.sql
│   │   ├── sp_volvo_part_master_get.sql
│   │   ├── sp_volvo_part_master_insert.sql
│   │   ├── sp_volvo_part_master_update.sql
│   │   └── sp_volvo_part_component_get.sql
│   └── TestData/
│       └── volvo_sample_data.sql (initial master data from DataSheet.csv)
└── App.xaml.cs (register Volvo services in DI container)
```

**Structure Decision**: Single project architecture maintained. Volvo module follows existing pattern:  Module_Volvo/Models/, ViewModule_Volvo/Models/, Module_Volvo/Views/, Module_Volvo/Services/, Module_Volvo/Data/. Integrates with shared infrastructure (Settings, Reporting, BaseViewModel, ErrorHandler, Logging). Database scripts in Database/ folder following established conventions.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| N/A | N/A | N/A |

# Implementation Plan: Internal Routing Module Overhaul

**Branch**: `001-routing-module` | **Date**: 2026-01-04 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/001-routing-module/spec.md`

## Summary

Implement complete rewrite of the Internal Routing Module with a "Package First" wizard workflow (PO Selection → Recipient Selection → Review), smart data entry features (Quick Add buttons, intelligent sorting, real-time search), and robust database foundation. System provides three distinct modes: Guided Wizard (default), Manual Entry (grid-based for power users), and Edit Mode (search/modify historical labels). Labels are saved to both CSV (for LabelView integration) and MySQL database with full audit trail.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.8+), CommunityToolkit.Mvvm 8.x, MySql.Data 8.x, Microsoft.Data.SqlClient  
**Storage**: MySQL 8.x (mtm_receiving_application database) + CSV files for LabelView, SQL Server (Infor Visual MTMFG READ ONLY)  
**Testing**: Manual user testing (no automated tests in MVP)  
**Target Platform**: Windows 10/11 Desktop  
**Project Type**: Single WinUI 3 application (desktop)  
**Performance Goals**: Sub-2s Infor Visual PO lookup, <100ms recipient filtering, <200ms Quick Add navigation, <500ms CSV write  
**Constraints**: MySQL 5.7.24 compatible (no JSON functions/CTEs/window functions), Infor Visual read-only queries only, CSV format compatible with LabelView 2022  
**Scale/Scope**: 10-50 labels/day, 50-200 active recipients, 10 concurrent users max, ~8 XAML views, ~6 ViewModels, ~5 DAOs, ~15 stored procedures

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](../../.specify/memory/constitution.md) v1.0.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Plan includes ViewModels (ModeSelectionViewModel, WizardStep1ViewModel, WizardStep2ViewModel, WizardStep3ViewModel, ManualEntryViewModel, EditModeViewModel), Views (XAML pages), Models (RoutingLabel, Recipient, OtherReason, UsageTracking, UserPreference), Services (IRoutingService, IRoutingInforVisualService, IRoutingRecipientService, IRoutingUsageTrackingService)
- [x] **II. Database Layer**: All MySQL operations via stored procedures (sp_routing_*), DAOs return Model_Dao_Result, async operations only. Infor Visual queries via direct SQL (READ ONLY)
- [x] **III. Dependency Injection**: All services/DAOs registered in App.xaml.cs with interfaces
- [x] **IV. Error Handling & Logging**: IService_ErrorHandler for exceptions, ILoggingService for audit trail, graceful Infor Visual connection failure handling
- [x] **V. Security & Authentication**: Employee number from existing auth context (Module_Core authentication)
- [x] **VI. WinUI 3 Modern Practices**: x:Bind (Mode=OneWay/TwoWay), ObservableCollection, [ObservableProperty], [RelayCommand], async/await throughout
- [x] **VII. Specification-Driven**: Follows Speckit workflow (spec → plan → data-model → tasks)

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL for routing data + CSV files, SQL Server (Infor Visual MTMFG) for PO validation
- [x] **MySQL 5.7.24 Compatible**: No JSON/CTEs/window functions, stored procedures only, compatible data types
- [x] **Required Packages**: CommunityToolkit.Mvvm, MySql.Data, Microsoft.Data.SqlClient, Material.Icons.WinUI3

### Critical Constraints

- [x] **Infor Visual READ ONLY**: PO lookup queries use ApplicationIntent=ReadOnly connection, only SELECT queries, no writes/DDL, graceful failure handling (allow "OTHER" PO workflow)
- [x] **Forbidden Practices**: No raw SQL in C# for MySQL (stored procedures only), no DAO exceptions (return error results), no logic in code-behind (XAML only), no service locator (DI only)

### Justification for Violations
None - all practices align with constitution. The wizard workflow and manual entry mode both use MVVM with Service→DAO→Database layer separation.

## Project Structure

### Documentation (this feature)

```text
specs/001-routing-module/
├── plan.md              # This file
├── spec.md              # Feature specification (created)
├── research.md          # Phase 0: Technology decisions (to be created)
├── data-model.md        # Phase 1: Database schema with ERD (to be created)
├── quickstart.md        # Phase 1: Developer setup guide (to be created)
├── contracts/           # Phase 1: Service interfaces (to be created)
│   ├── IRoutingService.cs
│   ├── IRoutingInforVisualService.cs
│   ├── IRoutingRecipientService.cs
│   └── IRoutingUsageTrackingService.cs
└── tasks.md             # Phase 2: Task breakdown by user story (to be created via /speckit.tasks)
```

### Source Code (repository root)

```text
MTM_Receiving_Application/ (root)
├── Module_Routing/              # NEW MODULE FOLDER
│   ├── Models/
│   │   ├── Model_RoutingLabel.cs
│   │   ├── Model_RoutingRecipient.cs
│   │   ├── Model_RoutingOtherReason.cs
│   │   ├── Model_RoutingUsageTracking.cs
│   │   ├── Model_RoutingUserPreference.cs
│   │   └── Model_RoutingLabelHistory.cs
│   ├── ViewModels/
│   │   ├── RoutingModeSelectionViewModel.cs
│   │   ├── RoutingWizardStep1ViewModel.cs (PO & Line Selection)
│   │   ├── RoutingWizardStep2ViewModel.cs (Recipient Selection)
│   │   ├── RoutingWizardStep3ViewModel.cs (Review & Confirm)
│   │   ├── RoutingManualEntryViewModel.cs
│   │   └── RoutingEditModeViewModel.cs
│   ├── Views/
│   │   ├── RoutingModeSelectionView.xaml (+ .cs)
│   │   ├── RoutingWizardStep1View.xaml (+ .cs)
│   │   ├── RoutingWizardStep2View.xaml (+ .cs)
│   │   ├── RoutingWizardStep3View.xaml (+ .cs)
│   │   ├── RoutingManualEntryView.xaml (+ .cs)
│   │   └── RoutingEditModeView.xaml (+ .cs)
│   ├── Services/
│   │   ├── IRoutingService.cs
│   │   ├── RoutingService.cs (label creation, CSV export, validation)
│   │   ├── IRoutingInforVisualService.cs
│   │   ├── RoutingInforVisualService.cs (PO lookup, line retrieval)
│   │   ├── IRoutingRecipientService.cs
│   │   ├── RoutingRecipientService.cs (recipient list, Quick Add calculation)
│   │   ├── IRoutingUsageTrackingService.cs
│   │   └── RoutingUsageTrackingService.cs (usage count update, smart sorting)
│   └── Data/
│       ├── Dao_RoutingLabel.cs
│       ├── Dao_RoutingRecipient.cs
│       ├── Dao_RoutingOtherReason.cs
│       ├── Dao_RoutingUsageTracking.cs
│       ├── Dao_RoutingUserPreference.cs
│       ├── Dao_RoutingLabelHistory.cs
│       └── Dao_InforVisualPO.cs (read-only queries to VISUAL/MTMFG)
├── Database/
│   ├── Schemas/
│   │   └── schema_routing.sql (table definitions)
│   ├── StoredProcedures/
│   │   ├── sp_routing_label_insert.sql
│   │   ├── sp_routing_label_update.sql
│   │   ├── sp_routing_label_get_by_id.sql
│   │   ├── sp_routing_label_get_history.sql
│   │   ├── sp_routing_label_check_duplicate.sql
│   │   ├── sp_routing_recipient_get_all.sql
│   │   ├── sp_routing_recipient_get_active.sql
│   │   ├── sp_routing_usage_tracking_increment.sql
│   │   ├── sp_routing_usage_tracking_get_top_recipients.sql
│   │   ├── sp_routing_user_preference_get.sql
│   │   ├── sp_routing_user_preference_upsert.sql
│   │   ├── sp_routing_other_reason_get_all.sql
│   │   └── sp_routing_label_history_insert.sql
│   └── TestData/
│       └── routing_sample_data.sql (initial recipients, reasons, test labels)
├── App.xaml.cs (register Routing services in DI container)
└── MainWindow.xaml (add navigation to Routing module)
```

**Structure Decision**: Single project architecture maintained. Routing module follows existing Module_* pattern (Module_Receiving, Module_Dunnage) with Module_Routing/ folder containing Models, ViewModels, Views, Services, Data. Integrates with Module_Core (BaseViewModel, ErrorHandler, Logging) and Module_Shared (navigation, styles). Database scripts in Database/ folder following established conventions. CSV export writes to network share configured in appsettings.json.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| N/A | N/A | N/A |

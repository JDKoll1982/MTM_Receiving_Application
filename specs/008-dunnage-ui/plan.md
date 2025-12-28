# Implementation Plan: Dunnage Wizard Workflow UI

**Branch**: `008-dunnage-ui` | **Date**: 2025-12-27 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-dunnage-ui/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This feature implements a multi-step wizard UI for dunnage receiving operations in the MTM Receiving Application. The wizard provides a guided workflow with mode selection (Wizard/Manual/Edit), dynamic type selection with pagination, part selection with Visual ERP inventory validation, quantity entry, dynamic spec input generation based on type schemas, and batch review with CSV export. The UI follows WinUI 3 design standards with strict MVVM architecture, integrating with existing MySQL dunnage tables and read-only Infor Visual PO validation.

## Technical Context

**Language/Version**: C# 12 with .NET 8.0  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.8+), CommunityToolkit.Mvvm 8.2+, CommunityToolkit.WinUI.UI.Controls 7.1+ (DataGrid)  
**Storage**: MySQL 8.0+ (application database: mtm_receiving_application), SQL Server (Infor Visual MTMFG - READ ONLY)  
**Testing**: xUnit with Moq for ViewModels and Services, integration tests for DAOs  
**Target Platform**: Windows 10 1809+ / Windows 11 (x64 primary, ARM64 compatible)
**Project Type**: WinUI 3 Desktop Application with MVVM architecture  
**Performance Goals**: <100ms UI responsiveness, <500ms database operations, pagination for 9+ types per page  
**Constraints**: MySQL 5.7.24 compatibility (no JSON, CTEs, window functions), Infor Visual READ ONLY access, strict MVVM separation  
**Scale/Scope**: 6 wizard steps (Mode Selection, Type Selection, Part Selection, Quantity, Details, Review), 11 seed dunnage types, dynamic spec generation from type schemas, batch session management for multiple loads

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](../../.specify/memory/constitution.md) v1.2.1.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Plan separates ViewModels (DunnageWorkflowViewModel, mode/type/part/quantity/details/review ViewModels), Views (XAML pages for each step), Models (Model_DunnageLoad, Model_DunnageType, Model_DunnagePart, Model_DunnageSpec), and Services (IService_DunnageWorkflow, IService_MySQL_Dunnage, IService_Pagination)
- [x] **II. Database Layer**: Plan uses Model_Dao_Result pattern via existing DAOs (Dao_DunnageType, Dao_DunnagePart, Dao_DunnageLoad), stored procedures only for MySQL operations, async operations throughout
- [x] **III. Dependency Injection**: All ViewModels and Services will be registered in App.xaml.cs DI container with proper interfaces (ViewModels as Transient, Services as Singleton)
- [x] **IV. Error Handling & Logging**: Plan includes IService_ErrorHandler for validation errors and user feedback, ILoggingService for wizard step transitions and save operations
- [x] **V. Security & Authentication**: Session management via existing IService_SessionManager, user context preserved across wizard steps, audit trail for dunnage transactions
- [x] **VI. WinUI 3 Modern Practices**: UI uses x:Bind throughout, ObservableCollection for type/part lists, async/await for all database calls, IsBusy patterns for loading states
- [x] **VII. Specification-Driven**: This plan follows Speckit workflow structure with spec.md as source of truth, will generate research.md, data-model.md, contracts/, and quickstart.md

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL for app data (dunnage tables), SQL Server for Infor Visual PO validation (READ ONLY via existing Dao_InforVisualPO)
- [x] **MySQL 5.7.24 Compatible**: No new stored procedures use JSON functions, CTEs, window functions, or CHECK constraints. Pagination uses LIMIT/OFFSET.
- [x] **Required Packages**: CommunityToolkit.Mvvm (existing), CommunityToolkit.WinUI.UI.Controls for DataGrid in Review step

### Critical Constraints

- [x] **Infor Visual READ ONLY**: Feature queries Infor Visual for PO validation in Details Entry step:
  - [x] Uses existing Dao_InforVisualPO with ApplicationIntent=ReadOnly
  - [x] Only SELECT queries to validate PO existence and retrieve part details
  - [x] No INSERT, UPDATE, DELETE, or DDL operations
  - [x] Graceful handling if Infor Visual is offline (default to "Adjust In" method)
- [x] **Forbidden Practices**: No direct SQL in ViewModels/Services (use DAOs), no DAO exceptions (return Model_Dao_Result.Failure), no logic in XAML code-behind (only event wiring), all services use DI not service locator

### Justification for Violations
No violations detected. This feature leverages existing architecture patterns from Receiving workflow (specs/003-004 completed features). All new ViewModels follow BaseViewModel inheritance, all database operations flow through Service→DAO layers, and wizard orchestration matches proven DunnageWorkflowViewModel pattern from spec 007 refactoring.

## Project Structure

### Documentation (this feature)

```text
specs/008-dunnage-ui/
├── spec.md              # Feature specification (EXISTING)
├── plan.md              # This file (generated by /speckit.plan)
├── research.md          # Phase 0 output (to be generated)
├── data-model.md        # Phase 1 output (to be generated - database schema)
├── quickstart.md        # Phase 1 output (to be generated - wizard user guide)
├── contracts/           # Phase 1 output (to be generated - service interfaces)
├── mockups/             # UI mockups (EXISTING)
└── checklists/          # Validation checklists (EXISTING)
```

### Source Code (repository root)

```text
MTM_Receiving_Application/
├── ViewModels/
│   └── Dunnage/
│       ├── DunnageWorkflowViewModel.cs          # NEW - Wizard orchestrator
│       ├── DunnageModeSelectionViewModel.cs     # NEW - Mode selection logic
│       ├── DunnageTypeSelectionViewModel.cs     # NEW - Type grid with pagination
│       ├── DunnagePartSelectionViewModel.cs     # NEW - Part dropdown + inventory check
│       ├── DunnageQuantityEntryViewModel.cs     # NEW - Quantity input validation
│       ├── DunnageDetailsEntryViewModel.cs      # NEW - PO/Location + dynamic specs
│       └── DunnageReviewViewModel.cs            # NEW - Batch review + CSV export
│
├── Views/
│   ├── Main/
│   │   ├── Main_DunnageLabelPage.xaml           # EXISTING - Main wizard page
│   │   └── Main_DunnageLabelPage.xaml.cs        # EXISTING - Page navigation logic
│   └── Dunnage/
│       ├── Dunnage_ModeSelectionView.xaml       # NEW - 3-card mode selection
│       ├── Dunnage_ModeSelectionView.xaml.cs    # NEW
│       ├── Dunnage_TypeSelectionView.xaml       # NEW - 3x3 type grid
│       ├── Dunnage_TypeSelectionView.xaml.cs    # NEW
│       ├── Dunnage_PartSelectionView.xaml       # NEW - Part AutoSuggestBox
│       ├── Dunnage_PartSelectionView.xaml.cs    # NEW
│       ├── Dunnage_QuantityEntryView.xaml       # NEW - NumberBox for quantity
│       ├── Dunnage_QuantityEntryView.xaml.cs    # NEW
│       ├── Dunnage_DetailsEntryView.xaml        # NEW - PO/Location + spec inputs
│       ├── Dunnage_DetailsEntryView.xaml.cs     # NEW
│       ├── Dunnage_ReviewView.xaml              # NEW - DataGrid + Add Another/Save All
│       └── Dunnage_ReviewView.xaml.cs           # NEW
│
├── Services/
│   ├── Receiving/
│   │   ├── Service_DunnageWorkflow.cs           # EXISTING - Session + step management
│   │   └── Service_CSVWriter.cs                 # EXISTING - CSV export for dunnage
│   └── Database/
│       ├── Service_MySQL_Dunnage.cs             # EXISTING - Dunnage CRUD operations
│       ├── Service_MySQL_PackagePreferences.cs  # EXISTING - User default mode
│       └── Service_InforVisual.cs               # EXISTING - PO validation (READ ONLY)
│
├── Contracts/Services/
│   ├── IService_DunnageWorkflow.cs              # EXISTING - Workflow contract
│   ├── IService_MySQL_Dunnage.cs                # EXISTING - Dunnage data contract
│   ├── IService_CSVWriter.cs                    # EXISTING - CSV export contract
│   ├── IService_UserPreferences.cs              # EXISTING - User preferences (mode)
│   └── IService_InforVisual.cs                  # EXISTING - Infor Visual queries
│
├── Models/
│   ├── Dunnage/
│   │   ├── Model_DunnageLoad.cs                 # EXISTING - Load entity
│   │   ├── Model_DunnageType.cs                 # EXISTING - Type entity
│   │   ├── Model_DunnagePart.cs                 # EXISTING - Part entity
│   │   └── Model_DunnageSpec.cs                 # EXISTING - Spec entity (dynamic)
│   ├── Enums/
│   │   ├── Enum_DunnageWorkflowStep.cs          # EXISTING - Step enumeration
│   │   └── Enum_InventoryMethod.cs              # EXISTING - AdjustIn/ReceiveIn
│   └── Core/
│       └── Model_Dao_Result.cs                  # EXISTING - DAO result wrapper
│
├── Data/
│   ├── Dunnage/
│   │   ├── Dao_DunnageType.cs                   # EXISTING - Type CRUD (instance-based)
│   │   ├── Dao_DunnagePart.cs                   # EXISTING - Part CRUD (instance-based)
│   │   └── Dao_DunnageLoad.cs                   # EXISTING - Load batch insert (instance-based)
│   └── InforVisual/
│       └── Dao_InforVisualPO.cs                 # EXISTING - PO validation (READ ONLY)
│
├── Database/
│   └── StoredProcedures/
│       ├── sp_dunnage_types_get_all.sql         # EXISTING
│       ├── sp_dunnage_parts_get_by_type.sql     # EXISTING
│       ├── sp_dunnage_loads_insert_batch.sql    # EXISTING - Batch insert
│       └── sp_user_preferences_update_mode.sql  # EXISTING - Default mode persistence
│
├── Helpers/
│   └── Database/
│       ├── Helper_Database_StoredProcedure.cs   # EXISTING - SP execution wrapper
│       └── Helper_Database_Variables.cs         # EXISTING - Connection strings
│
└── App.xaml.cs                                  # EXISTING - DI registration
```

**Structure Decision**: This feature follows the existing WinUI 3 MVVM project structure. All wizard ViewModels are registered as Transient in DI (new instance per navigation), Services are Singletons, and DAOs are instance-based Singletons. The wizard uses a single DunnageLabelPage with conditional view visibility based on DunnageWorkflowViewModel.CurrentStep. This matches the proven Receiving workflow pattern from specs/003-004.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

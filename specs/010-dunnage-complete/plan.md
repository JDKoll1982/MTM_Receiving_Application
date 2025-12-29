# Implementation Plan: Dunnage Receiving System - Complete Implementation

**Branch**: `010-dunnage-complete` | **Date**: 2025-12-29 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/010-dunnage-complete/spec.md`

**Note**: This plan consolidates four separate specifications (006-009) into a comprehensive Dunnage Receiving System with Manual Entry, Edit Mode, Admin Interface, CSV Export Integration, and Add New Type Dialog.

## Summary

Implement a complete dunnage receiving system for the MTM Receiving Application with 5 major components:

1. **Manual Entry Mode** - DataGrid-based batch entry for power users with auto-fill and toolbar operations
2. **Edit Mode** - Historical data loading and editing with pagination and date filtering
3. **Admin Interface** - Four-section navigation hub for managing Types, Specs, Parts, and Inventoried List
4. **CSV Export Integration** - Dynamic column generation with dual-path writing (local + network) and RFC 4180 compliance
5. **Add New Type Dialog** - User-friendly dialog with visual icon picker, real-time validation, and drag-drop field reordering

The system builds upon existing database foundation (specs 004-005) with ~171 functional requirements across 20 user stories. Technical approach emphasizes MVVM architecture with instance-based DAOs, Service layer delegation, and strict constitutional compliance.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: 
- WinUI 3 (Windows App SDK 1.8+)
- CommunityToolkit.Mvvm 8.2+ (MVVM attributes and commands)
- CommunityToolkit.WinUI.UI.Controls 7.1+ (DataGrid, enhanced controls)
- Microsoft.Extensions.DependencyInjection 8.0+ (DI container)
- MySql.Data 9.0+ (MySQL database access)
- CsvHelper 33.0+ (RFC 4180 CSV writing/parsing)

**Storage**: 
- MySQL 5.7.24+ (`mtm_receiving_application` database) - Full CRUD via stored procedures
- SQL Server (Infor Visual `MTMFG` database) - READ ONLY for PO/Part validation (not used in this feature)

**Testing**: xUnit with Moq for unit tests, manual UI testing for Views  
**Target Platform**: Windows 10 1809+ / Windows 11 (x64/ARM64 desktop)  
**Project Type**: Single WinUI 3 desktop application with MVVM architecture  
**Performance Goals**: 
- DataGrid rendering: 100+ rows with dynamic columns without lag
- CSV export: 1,000 loads in <5 seconds
- Database operations: <500ms response time
- UI interactions: <100ms feedback

**Constraints**: 
- MySQL 5.7.24 compatibility (NO JSON functions, CTEs, window functions, CHECK constraints)
- UI thread must NEVER be blocked (all I/O operations async)
- Minimum screen resolution: 1366x768 (target: 1920x1080)
- Add New Type Dialog: No vertical scrolling at 1920x1080 with ≤5 custom fields (MaxHeight=750px)
- Network share access may be unavailable (dual-write strategy required)
- .editorconfig compliance (braces on all if statements, explicit accessibility modifiers, async naming)

**Scale/Scope**: 
- 5 new Views/ViewModels (Manual Entry, Edit Mode, Admin sections)
- 8 new DAO methods across existing DAOs
- 3 new service methods for CSV export and admin operations
- 1 complex ContentDialog with multi-step UX
- 20+ stored procedures for CRUD operations
- ~2,000 LOC across ViewModels, Services, Views

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](.specify/memory/constitution.md) v1.2.2.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Feature uses ViewModels for logic, Views for XAML-only UI, Models for data. All ViewModels inherit from BaseViewModel with [ObservableProperty] and [RelayCommand] attributes.
- [x] **II. Database Layer**: All DAOs are instance-based, registered in DI container. All methods return Model_Dao_Result<T>. Services delegate to DAOs - NO direct database access in services. Uses stored procedures exclusively for MySQL.
- [x] **III. Dependency Injection**: All services, DAOs, and ViewModels registered in App.xaml.cs. Constructor injection throughout. No service locator pattern.
- [x] **IV. Error Handling & Logging**: Uses IService_ErrorHandler for all errors. ILoggingService for audit logging. No silent failures. User-friendly error messages with severity levels.
- [x] **V. Security & Authentication**: Not applicable - leverages existing user session management (no new authentication required).
- [x] **VI. WinUI 3 Modern Practices**: All Views use x:Bind (compile-time binding), ObservableCollection for lists, async/await for I/O, IsBusy pattern for loading states.
- [x] **VII. Specification-Driven**: This plan follows Speckit workflow with research.md, data-model.md, contracts/, tasks.md structure.

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
- [x] **Database**: MySQL for app data (`mtm_receiving_application`), SQL Server for Infor Visual (not used in this feature - Infor Visual READ ONLY constraint N/A)
- [x] **MySQL 5.7.24 Compatible**: Validation rules stored as JSON string in TEXT columns. No JSON functions, CTEs, window functions. Uses temporary tables and subqueries.
- [x] **Required Packages**: CommunityToolkit.Mvvm (8.2+), CommunityToolkit.WinUI.UI.Controls (7.1+), MySql.Data (9.0+), CsvHelper (33.0+) - all already in project

### Critical Constraints

- [x] **Infor Visual READ ONLY**: Not applicable - this feature only accesses MySQL `mtm_receiving_application` database (full CRUD allowed via stored procedures)
- [x] **Forbidden Practices**: 
  - [x] No direct SQL in C# (stored procedures only via Helper_Database_StoredProcedure)
  - [x] No DAO exceptions (return Model_Dao_Result.Failure)
  - [x] No business logic in code-behind
  - [x] No ViewModels calling DAOs directly (must use Service layer)
  - [x] No static DAOs (all instance-based with DI)
  - [x] All if statements have braces (csharp_prefer_braces)
  - [x] All async methods end with 'Async' suffix
  - [x] All type members have explicit accessibility modifiers
  - [x] No var for built-in types (int, string, bool must be explicit)

### Justification for Violations

**NONE** - This feature fully complies with all constitutional principles.

**Service→DAO Layer Separation**: All ViewModels call Services (IService_MySQL_Dunnage, IService_DunnageCSVWriter), which delegate to DAOs (Dao_DunnageType, Dao_DunnagePart, Dao_DunnageLoad, Dao_DunnageSpec, Dao_InventoriedDunnage). NO ViewModels access DAOs directly.

**Instance-Based DAOs**: All existing Dunnage DAOs (Dao_DunnageType, Dao_DunnagePart, Dao_DunnageLoad, Dao_DunnageSpec, Dao_InventoriedDunnage) are already instance-based and registered in App.xaml.cs DI container. This feature extends existing methods and adds new methods following same pattern.

**EditorConfig Compliance**: All new code will follow .editorconfig rules including braces on if statements, explicit modifiers, async naming, null safety annotations.

## Project Structure

### Documentation (this feature)

```text
specs/010-dunnage-complete/
├── spec.md              # User stories and functional requirements (COMPLETE)
├── plan.md              # This file - technical implementation plan
├── research.md          # Phase 0 output - technology decisions and patterns
├── data-model.md        # Phase 1 output - database schema extensions
├── quickstart.md        # Phase 1 output - setup and testing guide
├── contracts/           # Phase 1 output - service interfaces
│   ├── IService_MySQL_Dunnage.cs           # CRUD operations for types/parts/specs
│   ├── IService_DunnageCSVWriter.cs        # CSV export with dynamic columns
│   └── IService_DunnageAdminWorkflow.cs    # Admin UI navigation logic
└── tasks.md             # Phase 2 output - implementation checklist (created by /speckit.tasks)
```

### Source Code (repository root)

**Project Type**: Single WinUI 3 desktop application with MVVM architecture

```text
MTM_Receiving_Application/
├── ViewModels/
│   └── Dunnage/                              # Dunnage feature ViewModels
│       ├── Dunnage_ManualEntryViewModel.cs   # NEW - Batch entry grid logic
│       ├── Dunnage_EditModeViewModel.cs      # NEW - History editing logic
│       ├── Dunnage_AdminMainViewModel.cs     # NEW - Admin navigation hub
│       ├── Dunnage_AdminTypesViewModel.cs    # NEW - Type management
│       ├── Dunnage_AdminPartsViewModel.cs    # NEW - Part management
│       ├── Dunnage_AdminSpecsViewModel.cs    # NEW - Spec management
│       ├── Dunnage_AdminInventoryViewModel.cs # NEW - Inventoried list
│       ├── Dunnage_AddTypeDialogViewModel.cs # NEW - Add type dialog logic
│       └── [Existing wizard ViewModels...]   # ModeSelection, TypeSelection, etc.
│
├── Views/
│   └── Dunnage/                              # Dunnage feature Views
│       ├── Dunnage_ManualEntryView.xaml      # NEW - DataGrid batch entry
│       ├── Dunnage_EditModeView.xaml         # NEW - History grid with filters
│       ├── Dunnage_AdminMainView.xaml        # NEW - Admin nav hub (4 cards)
│       ├── Dunnage_AdminTypesView.xaml       # NEW - Type management grid
│       ├── Dunnage_AdminPartsView.xaml       # NEW - Part management grid
│       ├── Dunnage_AdminSpecsView.xaml       # NEW - Spec management grid
│       ├── Dunnage_AdminInventoryView.xaml   # NEW - Inventoried list grid
│       ├── Dunnage_AddTypeDialog.xaml        # NEW - ContentDialog for new types
│       └── [Existing wizard views...]         # Wizard step views
│
├── Services/
│   └── Receiving/                            # Service layer (business logic)
│       ├── Service_MySQL_Dunnage.cs          # EXTEND - Add admin CRUD methods
│       ├── Service_DunnageCSVWriter.cs       # EXTEND - Add dynamic column export
│       ├── Service_DunnageWorkflow.cs        # EXISTING - Wizard workflow (no changes)
│       └── Service_DunnageAdminWorkflow.cs   # NEW - Admin navigation logic
│
├── Data/
│   └── Dunnage/                              # DAO layer (database access)
│       ├── Dao_DunnageType.cs                # EXTEND - Add CRUD methods
│       ├── Dao_DunnagePart.cs                # EXTEND - Add search/pagination
│       ├── Dao_DunnageSpec.cs                # EXTEND - Add spec management
│       ├── Dao_DunnageLoad.cs                # EXTEND - Add date filtering
│       └── Dao_InventoriedDunnage.cs         # EXTEND - Add CRUD methods
│
├── Models/
│   └── Dunnage/                              # Data models
│       ├── Model_DunnageType.cs              # EXISTING
│       ├── Model_DunnagePart.cs              # EXISTING
│       ├── Model_DunnageSpec.cs              # EXISTING
│       ├── Model_DunnageLoad.cs              # EXISTING
│       ├── Model_InventoriedDunnage.cs       # EXISTING
│       ├── Model_CSVWriteResult.cs           # NEW - CSV export result
│       ├── Model_IconDefinition.cs           # NEW - Icon picker data
│       └── Model_CustomFieldDefinition.cs    # NEW - Add type dialog fields
│
├── Contracts/
│   └── Services/                             # Service interfaces
│       ├── IService_MySQL_Dunnage.cs         # EXTEND - Add admin methods
│       ├── IService_DunnageCSVWriter.cs      # EXTEND - Add dynamic export
│       └── IService_DunnageAdminWorkflow.cs  # NEW - Admin navigation
│
├── Database/
│   └── StoredProcedures/
│       └── Dunnage/                          # MySQL 5.7.24 stored procedures
│           ├── sp_dunnage_types_insert.sql   # EXISTING
│           ├── sp_dunnage_types_update.sql   # NEW
│           ├── sp_dunnage_types_delete.sql   # EXISTING
│           ├── sp_dunnage_types_get_part_count.sql        # NEW - Impact analysis
│           ├── sp_dunnage_types_get_transaction_count.sql # NEW - Impact analysis
│           ├── sp_dunnage_parts_search.sql   # EXISTING
│           ├── sp_dunnage_parts_get_by_type.sql # NEW
│           ├── sp_dunnage_parts_get_transaction_count.sql # NEW
│           ├── sp_dunnage_loads_get_by_date_range.sql # NEW
│           ├── sp_dunnage_loads_update.sql   # NEW
│           ├── sp_dunnage_specs_get_all_keys.sql # NEW - Union of spec keys
│           ├── sp_inventoried_dunnage_insert.sql # EXISTING
│           ├── sp_inventoried_dunnage_update.sql # NEW
│           └── sp_inventoried_dunnage_delete.sql # NEW
│
└── Tests/
    └── MTM_Receiving_Application.Tests/
        └── Unit/
            └── Services/
                ├── Service_MySQL_Dunnage_Tests.cs      # EXTEND
                ├── Service_DunnageCSVWriter_Tests.cs   # EXTEND
                └── Service_DunnageAdminWorkflow_Tests.cs # NEW
```

**Structure Decision**: 

This feature extends the existing single WinUI 3 desktop application structure. New components follow established patterns:

- **ViewModels/Dunnage/**: New manual entry, edit mode, and admin ViewModels alongside existing wizard ViewModels
- **Views/Dunnage/**: New XAML views and one ContentDialog for Add Type functionality
- **Services/Receiving/**: Extensions to existing services plus one new admin workflow service
- **Data/Dunnage/**: Extensions to existing instance-based DAOs (NO new DAOs, only new methods)
- **Database/StoredProcedures/Dunnage/**: New stored procedures for CRUD, filtering, impact analysis

**Architectural Consistency**: Matches existing Receiving module pattern (ManualEntryView, EditModeView, workflow service separation). Admin interface uses content area navigation within MainWindow (not modal dialogs or separate windows).

**No New Projects**: All code integrates into existing MTM_Receiving_Application.csproj. No new assemblies or libraries required.

## Complexity Tracking

**NO VIOLATIONS** - This feature fully complies with the MTM Receiving Application Constitution v1.2.2.

### Compliance Verification

| Constitutional Principle | Compliance Status | Implementation Approach |
|-------------------------|-------------------|------------------------|
| **MVVM Architecture** | ✅ COMPLIANT | All ViewModels inherit from BaseViewModel. Views are XAML-only. Services handle business logic. NO ViewModels access DAOs directly. |
| **Database Layer** | ✅ COMPLIANT | All DAOs instance-based, registered in DI. All methods return Model_Dao_Result<T>. Stored procedures only. Services delegate to DAOs. |
| **Dependency Injection** | ✅ COMPLIANT | All services, DAOs, ViewModels registered in App.xaml.cs. Constructor injection throughout. |
| **Error Handling** | ✅ COMPLIANT | Uses IService_ErrorHandler for all user-facing errors. ILoggingService for audit trail. No silent failures. |
| **WinUI 3 Practices** | ✅ COMPLIANT | x:Bind in all Views. ObservableCollection for lists. Async/await for I/O. IsBusy pattern. |
| **MySQL 5.7.24** | ✅ COMPLIANT | No JSON functions, CTEs, window functions. Validation rules stored as JSON string. |
| **EditorConfig** | ✅ COMPLIANT | All if statements have braces. Explicit modifiers. Async naming. No var for built-in types. |

### Simpler Alternatives Rejected

**Why Not Direct ViewModel→DAO Access?**
- **Rejected**: Would violate MVVM layer separation and make unit testing difficult
- **Chosen**: Service layer provides business logic abstraction, validation, error handling centralization
- **Benefit**: ViewModels can be unit tested by mocking services. DAOs can be integration tested independently.

**Why Not Static DAOs?**
- **Rejected**: Static DAOs cannot be injected, mocked, or tested properly
- **Chosen**: Instance-based DAOs with constructor injection of connection string
- **Benefit**: Full DI support, mockable in tests, aligns with constitution principle II

**Why Not Single Service for All Admin CRUD?**
- **Rejected**: Could consolidate all admin operations into one massive service
- **Chosen**: Separate IService_DunnageAdminWorkflow for navigation logic, extend IService_MySQL_Dunnage for CRUD
- **Benefit**: Single Responsibility Principle, clear separation between workflow orchestration and data operations

**Why ContentDialog Instead of Window for Add Type?**
- **Rejected**: Could use separate Window with custom chrome and sizing
- **Chosen**: ContentDialog as modal overlay within MainWindow
- **Benefit**: Simpler focus management, standard WinUI 3 pattern, automatic XamlRoot handling, fits constitutional window sizing guidance

**Why Extend Existing Services Instead of New Services?**
- **Rejected**: Could create IService_DunnageAdmin, IService_DunnageManualEntry as separate services
- **Chosen**: Extend existing IService_MySQL_Dunnage with new methods, add IService_DunnageAdminWorkflow only for navigation
- **Benefit**: Consolidates related CRUD operations, reduces service proliferation, maintains service cohesion

### No Additional Complexity Justification Required

This feature adds NO architectural complexity beyond existing patterns:
- Uses existing DAO pattern (instance-based, Model_Dao_Result)
- Uses existing Service→DAO delegation (no ViewModels call DAOs)
- Uses existing MVVM patterns (BaseViewModel, x:Bind, [ObservableProperty])
- Uses existing error handling (IService_ErrorHandler, ILoggingService)
- Uses existing DI registration (App.xaml.cs ConfigureServices)

**Total Violation Count**: 0

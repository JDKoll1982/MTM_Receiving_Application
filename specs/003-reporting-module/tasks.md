# Tasks: Reporting Module

**Input**: Design documents from `/specs/003-reporting-module/`  
**Prerequisites**: plan.md ✅, spec.md ✅, data-model.md ✅, contracts/ ✅  
**UI Mockups**: N/A (uses standard WinUI 3 controls)

**Tests**: NO automated tests requested (per spec clarifications - manual testing only)

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1)
- Include exact file paths in descriptions

## Path Conventions

Single project structure - paths relative to repository root:
- Models: `Module_Core/Models/Reporting/` or `Module_Reporting/Models/`
- ViewModels: `Module_Reporting/ViewModels/`
- Views: `Module_Reporting/Views/`
- Services: `Module_Reporting/Services/`
- Data: `Module_Reporting/Data/`
- Contracts: `Module_Core/Contracts/Services/` (IService_Reporting to be created)
- Database: `Database/Schemas/`

---

## Phase 1: Database Setup (Shared Infrastructure)

**Purpose**: Database views for data aggregation

- [X] T001 Create database views in Database/Schemas/schema_reporting_views.sql (vw_receiving_history, vw_dunnage_history, vw_routing_history, vw_volvo_history)
- [ ] T002 Deploy database views to MySQL server (execute schema_reporting_views.sql)
- [ ] T003 Test views: Verify each view returns correct data from respective module tables

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core models, DAOs, and interfaces that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 [P] Create Model_ReportRow in Module_Core/Models/Reporting/Model_ReportRow.cs (properties: Id, PONumber, PartNumber, PartDescription, Quantity, WeightLbs, HeatLotNumber, CreatedDate, SourceModule, plus module-specific fields)
- [X] T005 Create Dao_Reporting in Module_Reporting/Data/Dao_Reporting.cs (methods: GetReceivingHistoryAsync, GetDunnageHistoryAsync, GetRoutingHistoryAsync, GetVolvoHistoryAsync)
- [X] T006 Register Reporting DAO in App.xaml.cs ConfigureServices (singleton: Dao_Reporting)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Generate End-of-Day Reports (Priority: P1) 🎯 MVP

**Goal**: Complete reporting workflow with date range filtering, PO normalization, CSV export, and email formatting

**Independent Test**: Navigate to "End of Day Reports", select date range and modules, generate reports, verify CSV export and email formatting.

### Interfaces & Services

- [X] T007 [P] [US1] Create Module_Core/Contracts/Services/IService_Reporting.cs from specs/003-reporting-module/contracts/IService_Reporting.cs
- [X] T008 [US1] Create Module_Reporting/Services/Service_Reporting.cs implementing IService_Reporting (date filtering, PO normalization, CSV export, email formatting)
- [X] T009 [US1] Implement PO normalization algorithm in Service_Reporting.cs (matches EndOfDayEmail.js logic)
- [X] T010 [US1] Implement CSV export in Service_Reporting.cs (matches MiniUPSLabel.csv structure)
- [X] T011 [US1] Implement email formatting in Service_Reporting.cs (HTML table with alternating row colors grouped by date)
- [X] T012 [US1] Register Reporting Service in App.xaml.cs ConfigureServices (singleton: Service_Reporting)

### ViewModels

- [X] T013 [P] [US1] Create Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs inheriting from BaseViewModel (date range selection, module checkboxes, report generation)
- [ ] T014 [P] [US1] Create Module_Reporting/ViewModels/ViewModel_Reporting_ReportGenerator.cs inheriting from BaseViewModel (report display, CSV export, email copy)
- [X] T015 [US1] Register Reporting ViewModels in App.xaml.cs ConfigureServices (transient)

### Views

- [X] T016 [P] [US1] Create Module_Reporting/Views/View_Reporting_Main.xaml and .xaml.cs with x:Bind → (DatePicker, CheckBoxes for modules, Generate button)
- [ ] T017 [P] [US1] Create Module_Reporting/Views/View_Reporting_ReportGenerator.xaml and .xaml.cs with x:Bind → (DataGrid for report data, Export CSV button, Copy Email button)

### DI & Navigation

- [ ] T018 [US1] Update navigation in MainWindow.xaml or main menu to include Reporting module entry point
- [X] T019 [US1] Register Reporting Views in App.xaml.cs ConfigureServices (transient)

**Checkpoint**: At this point, User Story 1 should be fully functional - user can generate reports, export CSV, and copy email format

---

## Phase 4: Manual Testing

- [ ] T020 [US1] Manual test: Generate Receiving report with date range
- [ ] T021 [US1] Manual test: Generate Dunnage report with date range
- [ ] T022 [US1] Manual test: Generate Routing report with date range
- [ ] T023 [US1] Manual test: Generate Volvo report with date range
- [ ] T024 [US1] Manual test: Verify PO number normalization (various formats)
- [ ] T025 [US1] Manual test: Verify CSV export matches MiniUPSLabel.csv structure
- [ ] T026 [US1] Manual test: Verify email formatting with alternating row colors
- [ ] T027 [US1] Manual test: Verify date range persistence across module changes

---

**Note**: All file paths assume the module will follow the established `Module_[Name]/` pattern used by existing modules in the codebase.


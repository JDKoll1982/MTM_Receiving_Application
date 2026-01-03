# Tasks: Receiving Module

**Input**: Design documents from `/specs/013-receiving-module/`  
**Prerequisites**: plan.md ✅, spec.md ✅, data-model.md ✅, contracts/ ✅  
**UI Mockups**: [../011-module-reimplementation/mockups/Receiving/](../011-module-reimplementation/mockups/Receiving/) ✅

**Tests**: NO automated tests requested (per spec clarifications - manual testing only)

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1)
- Include exact file paths in descriptions

## Path Conventions

Single project structure - paths relative to repository root:
- Models: `ReceivingModule/Models/`
- ViewModels: `ReceivingModule/ViewModels/`
- Views: `ReceivingModule/Views/`
- Services: `ReceivingModule/Services/`
- Data: `ReceivingModule/Data/`
- Database: `Database/Schemas/`, `Database/StoredProcedures/Receiving/`, `Database/TestData/`

---

## Phase 1: Database Setup (Shared Infrastructure)

**Purpose**: Database schema and stored procedures

- [ ] T001 Create database schema in Database/Schemas/schema_receiving.sql (tables: receiving_loads, receiving_lines, package_type_preferences)
- [ ] T002 [P] Create stored procedures in Database/StoredProcedures/Receiving/ (sp_receiving_load_insert, sp_receiving_load_get_by_date_range, sp_receiving_load_update)
- [ ] T003 [P] Create view vw_receiving_history in Database/Schemas/schema_receiving.sql
- [ ] T004 Deploy database schema to MySQL server (execute schema_receiving.sql, stored procedures, view)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core models, DAOs, and interfaces that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 [P] Create Model_Receiving_Load in ReceivingModule/Models/Model_Receiving_Load.cs (properties: LoadId, PONumber, PartId, PartDescription, QuantityReceived, Weight, HeatNumber, LotNumber, PackageType, CreatedBy, CreatedAt)
- [ ] T006 [P] Create Model_Receiving_Line in ReceivingModule/Models/Model_Receiving_Line.cs (properties: LineId, LoadId, LineNumber, Quantity, Notes)
- [ ] T007 [P] Create Model_Receiving_Session in ReceivingModule/Models/Model_Receiving_Session.cs (properties: CurrentStep, CurrentPONumber, CurrentPart, IsNonPOItem, NumberOfLoads, ReviewedLoads)
- [ ] T008 [P] Create Model_Receiving_ValidationResult in ReceivingModule/Models/Model_Receiving_ValidationResult.cs (properties: IsValid, ErrorMessage)
- [ ] T009 Create Dao_Receiving_Load in ReceivingModule/Data/Dao_Receiving_Load.cs (methods: InsertAsync, GetByDateRangeAsync, UpdateAsync, GetByIdAsync)
- [ ] T010 Create Dao_Receiving_Line in ReceivingModule/Data/Dao_Receiving_Line.cs (methods: InsertAsync, GetByLoadIdAsync, DeleteAsync)
- [ ] T011 Create Dao_Receiving_PackageTypePreference in ReceivingModule/Data/Dao_Receiving_PackageTypePreference.cs (methods: GetByUserIdAsync, UpsertAsync)
- [ ] T012 Register Receiving DAOs in App.xaml.cs ConfigureServices (singletons: Dao_Receiving_Load, Dao_Receiving_Line, Dao_Receiving_PackageTypePreference)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Implement Receiving Module (Priority: P2) 🎯 MVP

**Goal**: Complete receiving workflow with consistent naming and bug fixes

**Independent Test**: Navigate to "Receiving Labels", complete full workflow (Mode Selection → PO Entry → Package Type → Load Entry → Weight/Quantity → Heat/Lot → Review → Save). Verify "Add Another Part" bug is fixed.

### Interfaces & Enums

- [ ] T013 [P] [US1] Create ReceivingModule/Interfaces/IService_Receiving_Workflow.cs from contracts/IService_ReceivingWorkflow.cs with new naming
- [ ] T014 [P] [US1] Create ReceivingModule/Interfaces/IService_Receiving_Validation.cs from contracts/IService_ReceivingValidation.cs with new naming
- [ ] T015 [P] [US1] Create ReceivingModule/Interfaces/IService_Receiving_SessionManager.cs (session persistence)
- [ ] T016 [P] [US1] Create ReceivingModule/Interfaces/IService_Receiving_CSVWriter.cs (CSV export)
- [ ] T017 [P] [US1] Create ReceivingModule/Enums/Enum_Receiving_WorkflowStep.cs (ModeSelection, POEntry, PackageType, LoadEntry, WeightQuantity, HeatLot, Review, Save)

### Services

- [ ] T018 [US1] Create ReceivingModule/Services/Service_Receiving_Workflow.cs implementing IService_Receiving_Workflow
- [ ] T019 [US1] Implement "Add Another Part" bug fix in Service_Receiving_Workflow.cs - clear session BEFORE navigation
- [ ] T020 [US1] Create ReceivingModule/Services/Service_Receiving_Validation.cs implementing IService_Receiving_Validation
- [ ] T021 [US1] Create ReceivingModule/Services/Service_Receiving_SessionManager.cs (JSON persistence)
- [ ] T022 [US1] Create ReceivingModule/Services/Service_Receiving_CSVWriter.cs (LabelView CSV export)
- [ ] T023 [US1] Register Receiving Services in App.xaml.cs ConfigureServices (singletons: Service_Receiving_Workflow, Service_Receiving_Validation, Service_Receiving_SessionManager, Service_Receiving_CSVWriter)

### ViewModels

- [ ] T024 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_ModeSelection.cs inheriting from BaseViewModel
- [ ] T025 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_POEntry.cs inheriting from BaseViewModel
- [ ] T026 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_PackageType.cs inheriting from BaseViewModel
- [ ] T027 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_LoadEntry.cs inheriting from BaseViewModel
- [ ] T028 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_WeightQuantity.cs inheriting from BaseViewModel
- [ ] T029 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_HeatLot.cs inheriting from BaseViewModel
- [ ] T030 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_Review.cs inheriting from BaseViewModel
- [ ] T031 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_Workflow.cs inheriting from BaseViewModel
- [ ] T032 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_EditMode.cs inheriting from BaseViewModel
- [ ] T033 [P] [US1] Create ReceivingModule/ViewModels/ViewModel_Receiving_ManualEntry.cs inheriting from BaseViewModel
- [ ] T034 [US1] Register Receiving ViewModels in App.xaml.cs ConfigureServices (transient)

### Views

📐 **UI Mockups**: See [../011-module-reimplementation/mockups/Receiving/](../011-module-reimplementation/mockups/Receiving/) for visual design guidance

- [ ] T035 [P] [US1] Create ReceivingModule/Views/View_Receiving_ModeSelection.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Receiving/View_Receiving_ModeSelection.svg)
- [ ] T036 [P] [US1] Create ReceivingModule/Views/View_Receiving_POEntry.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Receiving/View_Receiving_POEntry.svg)
- [ ] T037 [P] [US1] Create ReceivingModule/Views/View_Receiving_PackageType.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Receiving/View_Receiving_PackageType.svg)
- [ ] T038 [P] [US1] Create ReceivingModule/Views/View_Receiving_LoadEntry.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Receiving/View_Receiving_LoadEntry.svg)
- [ ] T039 [P] [US1] Create ReceivingModule/Views/View_Receiving_WeightQuantity.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Receiving/View_Receiving_WeightQuantity.svg)
- [ ] T040 [P] [US1] Create ReceivingModule/Views/View_Receiving_HeatLot.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Receiving/View_Receiving_HeatLot.svg)
- [ ] T041 [P] [US1] Create ReceivingModule/Views/View_Receiving_Review.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Receiving/View_Receiving_Review.svg)
- [ ] T042 [P] [US1] Create ReceivingModule/Views/View_Receiving_Workflow.xaml and .xaml.cs with x:Bind → (Container only)
- [ ] T043 [P] [US1] Create ReceivingModule/Views/View_Receiving_EditMode.xaml and .xaml.cs with x:Bind → (Similar to ManualEntry)
- [ ] T044 [P] [US1] Create ReceivingModule/Views/View_Receiving_ManualEntry.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Receiving/View_Receiving_ManualEntry.svg)

### DI & Navigation

- [ ] T045 [US1] Update Views/Main/Main_ReceivingLabelPage.xaml to navigate to new ReceivingModule Views
- [ ] T046 [US1] Update MainWindow.xaml navigation to use new Receiving module entry point

**Checkpoint**: At this point, User Story 1 should be fully functional - user can complete full receiving workflow with bug fixes

---

## Phase 4: Manual Testing

- [ ] T047 [US1] Manual test: Complete full receiving workflow (Mode Selection → PO Entry → Package Type → Load Entry → Weight/Quantity → Heat/Lot → Review → Save)
- [ ] T048 [US1] Manual test: Verify PO validation queries Infor Visual successfully
- [ ] T049 [US1] Manual test: Verify "Add Another Part" bug is fixed (form clears before navigation)
- [ ] T050 [US1] Manual test: Verify CSV export compatible with LabelView 2022
- [ ] T051 [US1] Manual test: Verify data saves to MySQL using stored procedures

---

**Reference**: See [../011-module-reimplementation/tasks.md](../011-module-reimplementation/tasks.md) - Phase 4 for complete task breakdown context


# Tasks: Dunnage Module

**Input**: Design documents from `/specs/014-dunnage-module/`  
**Prerequisites**: plan.md ✅, spec.md ✅, data-model.md ✅, contracts/ ✅  
**UI Mockups**: [../011-module-reimplementation/mockups/Dunnage/](../011-module-reimplementation/mockups/Dunnage/) ✅

**Tests**: NO automated tests requested (per spec clarifications - manual testing only)

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1)
- Include exact file paths in descriptions

## Path Conventions

Single project structure - paths relative to repository root:
- Models: `DunnageModule/Models/`
- ViewModels: `DunnageModule/ViewModels/`
- Views: `DunnageModule/Views/`
- Services: `DunnageModule/Services/`
- Data: `DunnageModule/Data/`
- Database: `Database/Schemas/`, `Database/StoredProcedures/Dunnage/`, `Database/TestData/`

---

## Phase 1: Database Setup (Shared Infrastructure)

**Purpose**: Database schema and stored procedures

- [ ] T001 Create database schema in Database/Schemas/schema_dunnage.sql (tables: dunnage_types, dunnage_parts, dunnage_loads, dunnage_specs, inventoried_dunnage)
- [ ] T002 [P] Create stored procedures in Database/StoredProcedures/Dunnage/ (sp_dunnage_type_*, sp_dunnage_part_*, sp_dunnage_load_*)
- [ ] T003 [P] Create view vw_dunnage_history in Database/Schemas/schema_dunnage.sql
- [ ] T004 Deploy database schema to MySQL server (execute schema_dunnage.sql, stored procedures, view)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core models, DAOs, and interfaces that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 [P] Create Model_Dunnage_Type in DunnageModule/Models/Model_Dunnage_Type.cs (properties: TypeId, TypeName, TypeDescription, IconCode, IsActive, CreatedAt)
- [ ] T006 [P] Create Model_Dunnage_Part in DunnageModule/Models/Model_Dunnage_Part.cs (properties: PartId, TypeId, PartNumber, PartDescription, UnitOfMeasure, IsInventoried, IsActive)
- [ ] T007 [P] Create Model_Dunnage_Load in DunnageModule/Models/Model_Dunnage_Load.cs (properties: LoadId, TypeId, PartId, Quantity, Details, CreatedBy, CreatedAt)
- [ ] T008 [P] Create Model_Dunnage_Spec in DunnageModule/Models/Model_Dunnage_Spec.cs (properties: SpecId, PartId, SpecKey, SpecValue, DisplayOrder)
- [ ] T009 [P] Create Model_Dunnage_Session in DunnageModule/Models/Model_Dunnage_Session.cs (properties: CurrentStep, SelectedType, SelectedPart, EnteredData, ReviewedLoads)
- [ ] T010 Create Dao_Dunnage_Type in DunnageModule/Data/Dao_Dunnage_Type.cs (methods: GetAllAsync, GetByIdAsync, InsertAsync, UpdateAsync, DeleteAsync)
- [ ] T011 Create Dao_Dunnage_Part in DunnageModule/Data/Dao_Dunnage_Part.cs (methods: GetByTypeAsync, GetByIdAsync, InsertAsync, UpdateAsync, DeleteAsync)
- [ ] T012 Create Dao_Dunnage_Load in DunnageModule/Data/Dao_Dunnage_Load.cs (methods: InsertAsync, GetByDateRangeAsync, GetAllAsync)
- [ ] T013 Create Dao_Dunnage_Spec in DunnageModule/Data/Dao_Dunnage_Spec.cs (methods: GetByPartIdAsync, InsertAsync, DeleteAsync)
- [ ] T014 Register Dunnage DAOs in App.xaml.cs ConfigureServices (singletons: Dao_Dunnage_Type, Dao_Dunnage_Part, Dao_Dunnage_Load, Dao_Dunnage_Spec)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Implement Dunnage Module (Priority: P3) 🎯 MVP

**Goal**: Complete dunnage workflow with admin capabilities

**Independent Test**: Navigate to "Dunnage Labels", complete user workflow (Type Selection → Part Selection → Details Entry → Quantity Entry → Review → Save) and admin workflows (Admin Types, Admin Parts, Admin Inventory).

### Interfaces & Enums

- [ ] T015 [P] [US1] Create DunnageModule/Interfaces/IService_Dunnage_Workflow.cs from contracts/IService_DunnageWorkflow.cs with new naming
- [ ] T016 [P] [US1] Create DunnageModule/Interfaces/IService_Dunnage_AdminWorkflow.cs from contracts/IService_DunnageAdminWorkflow.cs with new naming
- [ ] T017 [P] [US1] Create DunnageModule/Interfaces/IService_Dunnage_CSVWriter.cs from contracts/IService_DunnageCSVWriter.cs with new naming
- [ ] T018 [P] [US1] Create DunnageModule/Enums/Enum_Dunnage_WorkflowStep.cs (TypeSelection, PartSelection, DetailsEntry, QuantityEntry, Review, Save)
- [ ] T019 [P] [US1] Create DunnageModule/Enums/Enum_Dunnage_AdminSection.cs (Types, Parts, Specs, InventoriedList)

### Services

- [ ] T020 [US1] Create DunnageModule/Services/Service_Dunnage_Workflow.cs implementing IService_Dunnage_Workflow
- [ ] T021 [US1] Create DunnageModule/Services/Service_Dunnage_AdminWorkflow.cs implementing IService_Dunnage_AdminWorkflow
- [ ] T022 [US1] Create DunnageModule/Services/Service_Dunnage_CSVWriter.cs implementing IService_Dunnage_CSVWriter
- [ ] T023 [US1] Register Dunnage Services in App.xaml.cs ConfigureServices (singletons: Service_Dunnage_Workflow, Service_Dunnage_AdminWorkflow, Service_Dunnage_CSVWriter)

### ViewModels

- [ ] T024 [P] [US1] Create DunnageModule/ViewModels/ViewModel_Dunnage_TypeSelection.cs inheriting from BaseViewModel
- [ ] T025 [P] [US1] Create DunnageModule/ViewModels/ViewModel_Dunnage_PartSelection.cs inheriting from BaseViewModel
- [ ] T026 [P] [US1] Create DunnageModule/ViewModels/ViewModel_Dunnage_DetailsEntry.cs inheriting from BaseViewModel
- [ ] T027 [P] [US1] Create DunnageModule/ViewModels/ViewModel_Dunnage_QuantityEntry.cs inheriting from BaseViewModel
- [ ] T028 [P] [US1] Create DunnageModule/ViewModels/ViewModel_Dunnage_Review.cs inheriting from BaseViewModel
- [ ] T029 [P] [US1] Create DunnageModule/ViewModels/ViewModel_Dunnage_AdminTypes.cs inheriting from BaseViewModel
- [ ] T030 [P] [US1] Create DunnageModule/ViewModels/ViewModel_Dunnage_AdminParts.cs inheriting from BaseViewModel
- [ ] T031 [P] [US1] Create DunnageModule/ViewModels/ViewModel_Dunnage_AdminInventory.cs inheriting from BaseViewModel
- [ ] T032 [US1] Register Dunnage ViewModels in App.xaml.cs ConfigureServices (transient)

### Views

📐 **UI Mockups**: See [../011-module-reimplementation/mockups/Dunnage/](../011-module-reimplementation/mockups/Dunnage/) for visual design guidance

- [ ] T033 [P] [US1] Create DunnageModule/Views/View_Dunnage_TypeSelection.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Dunnage/View_Dunnage_TypeSelection.svg)
- [ ] T034 [P] [US1] Create DunnageModule/Views/View_Dunnage_PartSelection.xaml and .xaml.cs with x:Bind → (Use TypeSelection pattern)
- [ ] T035 [P] [US1] Create DunnageModule/Views/View_Dunnage_DetailsEntry.xaml and .xaml.cs with x:Bind → (Use WeightQuantity pattern)
- [ ] T036 [P] [US1] Create DunnageModule/Views/View_Dunnage_QuantityEntry.xaml and .xaml.cs with x:Bind → (Use WeightQuantity pattern)
- [ ] T037 [P] [US1] Create DunnageModule/Views/View_Dunnage_Review.xaml and .xaml.cs with x:Bind → (Use Receiving_Review pattern)
- [ ] T038 [P] [US1] Create DunnageModule/Views/View_Dunnage_AdminTypes.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Dunnage/View_Dunnage_AdminTypes.svg)
- [ ] T039 [P] [US1] Create DunnageModule/Views/View_Dunnage_AdminParts.xaml and .xaml.cs with x:Bind → (Use AdminTypes pattern)
- [ ] T040 [P] [US1] Create DunnageModule/Views/View_Dunnage_AdminInventory.xaml and .xaml.cs with x:Bind → (Use AdminTypes pattern)

### DI & Navigation

- [ ] T041 [US1] Update Views/Main/Main_DunnageLabelPage.xaml to navigate to new DunnageModule Views
- [ ] T042 [US1] Update MainWindow.xaml navigation to use new Dunnage module entry point

**Checkpoint**: At this point, User Story 1 should be fully functional - user can complete full dunnage workflow and admin workflows

---

## Phase 4: Manual Testing

- [ ] T043 [US1] Manual test: Complete full dunnage workflow (Type Selection → Part Selection → Details Entry → Quantity Entry → Review → Save)
- [ ] T044 [US1] Manual test: Verify Material.Icons display correctly
- [ ] T045 [US1] Manual test: Verify parts filtered by inventoried status
- [ ] T046 [US1] Manual test: Verify dynamic form generation from specs
- [ ] T047 [US1] Manual test: Verify admin workflows (Types, Parts, Inventory)
- [ ] T048 [US1] Manual test: Verify CSV export compatible with LabelView 2022

---

**Reference**: See [../011-module-reimplementation/tasks.md](../011-module-reimplementation/tasks.md) - Phase 5 for complete task breakdown context


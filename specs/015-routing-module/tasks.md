# Tasks: Routing Module

**Input**: Design documents from `/specs/015-routing-module/`  
**Prerequisites**: plan.md ✅, spec.md ✅, data-model.md ✅, contracts/ ✅  
**UI Mockups**: [../011-module-reimplementation/mockups/Routing/](../011-module-reimplementation/mockups/Routing/) ✅

**Tests**: NO automated tests requested (per spec clarifications - manual testing only)

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1)
- Include exact file paths in descriptions

## Path Conventions

Single project structure - paths relative to repository root:
- Models: `RoutingModule/Models/`
- ViewModels: `RoutingModule/ViewModels/`
- Views: `RoutingModule/Views/`
- Services: `RoutingModule/Services/`
- Data: `RoutingModule/Data/`
- Database: `Database/Schemas/`, `Database/StoredProcedures/Routing/`, `Database/TestData/`

---

## Phase 1: Database Setup (Shared Infrastructure)

**Purpose**: Database schema and stored procedures

- [ ] T001 Create database schema in Database/Schemas/schema_routing.sql (tables: routing_labels, routing_recipients)
- [ ] T002 [P] Create stored procedures in Database/StoredProcedures/Routing/ (sp_routing_label_insert, sp_routing_label_get_history, sp_routing_recipient_get_all)
- [ ] T003 [P] Create view vw_routing_history in Database/Schemas/schema_routing.sql
- [ ] T004 Deploy database schema to MySQL server (execute schema_routing.sql, stored procedures, view)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core models, DAOs, and interfaces that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 [P] Create Model_Routing_Label in RoutingModule/Models/Model_Routing_Label.cs (properties: Id, LabelNumber, DeliverTo, Department, PackageDescription, PONumber, WorkOrder, EmployeeNumber, CreatedDate, IsArchived, CreatedAt)
- [ ] T006 [P] Create Model_Routing_Recipient in RoutingModule/Models/Model_Routing_Recipient.cs (properties: Id, Name, DefaultDepartment, IsActive, CreatedDate)
- [ ] T007 [P] Create Model_Routing_Session in RoutingModule/Models/Model_Routing_Session.cs (properties: LabelQueue, NextLabelNumber)
- [ ] T008 Create Dao_Routing_Label in RoutingModule/Data/Dao_Routing_Label.cs (methods: InsertAsync, GetHistoryByDateRangeAsync, ArchiveAsync)
- [ ] T009 Create Dao_Routing_Recipient in RoutingModule/Data/Dao_Routing_Recipient.cs (methods: GetAllAsync, GetByNameAsync, InsertAsync, UpdateAsync)
- [ ] T010 Register Routing DAOs in App.xaml.cs ConfigureServices (singletons: Dao_Routing_Label, Dao_Routing_Recipient)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Implement Routing Module (Priority: P4) 🎯 MVP

**Goal**: Complete routing workflow with daily history tracking

**Independent Test**: Navigate to "Internal Routing", complete workflow (Label Entry → Add to Queue → Print Labels → Save to History). Verify history view displays labels grouped by date.

### Interfaces & Enums

- [ ] T011 [P] [US1] Create RoutingModule/Interfaces/IService_Routing.cs from contracts/IService_Routing.cs with new naming
- [ ] T012 [P] [US1] Create RoutingModule/Interfaces/IService_Routing_History.cs from contracts/IService_Routing_History.cs with new naming
- [ ] T013 [P] [US1] Create RoutingModule/Interfaces/IService_Routing_RecipientLookup.cs from contracts/IService_Routing_RecipientLookup.cs with new naming
- [ ] T014 [P] [US1] Create RoutingModule/Enums/Enum_Routing_WorkflowStep.cs (LabelEntry, Review, Print, History)

### Services

- [ ] T015 [US1] Create RoutingModule/Services/Service_Routing.cs implementing IService_Routing (queue management, CSV generation, label numbering)
- [ ] T016 [US1] Create RoutingModule/Services/Service_Routing_History.cs implementing IService_Routing_History (archive, retrieval, date grouping)
- [ ] T017 [US1] Create RoutingModule/Services/Service_Routing_RecipientLookup.cs implementing IService_Routing_RecipientLookup (auto-fill department)
- [ ] T018 [US1] Register Routing Services in App.xaml.cs ConfigureServices (singletons: Service_Routing, Service_Routing_History, Service_Routing_RecipientLookup)

### ViewModels

- [ ] T019 [P] [US1] Create RoutingModule/ViewModels/ViewModel_Routing_LabelEntry.cs inheriting from BaseViewModel (data grid, duplicate row, auto-fill)
- [ ] T020 [P] [US1] Create RoutingModule/ViewModels/ViewModel_Routing_History.cs inheriting from BaseViewModel (date grouping, alternating colors)
- [ ] T021 [P] [US1] Create RoutingModule/ViewModels/ViewModel_Routing_Workflow.cs inheriting from BaseViewModel (workflow coordinator)
- [ ] T022 [US1] Register Routing ViewModels in App.xaml.cs ConfigureServices (transient)

### Views

📐 **UI Mockups**: See [../011-module-reimplementation/mockups/Routing/](../011-module-reimplementation/mockups/Routing/) for visual design guidance

- [ ] T023 [P] [US1] Create RoutingModule/Views/View_Routing_LabelEntry.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Routing/View_Routing_LabelEntry.svg)
- [ ] T024 [P] [US1] Create RoutingModule/Views/View_Routing_History.xaml and .xaml.cs with x:Bind → [Mockup](../011-module-reimplementation/mockups/Routing/View_Routing_History.svg)
- [ ] T025 [P] [US1] Create RoutingModule/Views/View_Routing_Workflow.xaml and .xaml.cs with x:Bind → (Container only)

### DI & Navigation

- [ ] T026 [US1] Update Views/Main/Main_RoutingLabelPage.xaml to navigate to new RoutingModule Views
- [ ] T027 [US1] Update MainWindow.xaml navigation to use new Routing module entry point

**Checkpoint**: At this point, User Story 1 should be fully functional - user can create routing labels, print CSV, and view history

---

## Phase 4: Manual Testing

- [ ] T028 [US1] Manual test: Complete full routing workflow (Label Entry → Add to Queue → Print Labels → Save to History)
- [ ] T029 [US1] Manual test: Verify recipient lookup auto-fills department
- [ ] T030 [US1] Manual test: Verify PO number auto-formatting
- [ ] T031 [US1] Manual test: Verify duplicate row functionality
- [ ] T032 [US1] Manual test: Verify label numbers auto-increment per day
- [ ] T033 [US1] Manual test: Verify CSV export compatible with LabelView 2022
- [ ] T034 [US1] Manual test: Verify history view with date grouping and alternating colors

---

**Reference**: See [../011-module-reimplementation/tasks.md](../011-module-reimplementation/tasks.md) - Phase 6 for complete task breakdown context


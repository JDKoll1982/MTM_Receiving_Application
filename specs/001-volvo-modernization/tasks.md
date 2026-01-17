# Tasks: Module_Volvo CQRS Modernization

**Branch**: `001-volvo-modernization`  
**Input**: Design documents from `/specs/001-volvo-modernization/`  
**Prerequisites**: ✅ plan.md, ✅ spec.md, ✅ research.md, ✅ data-model.md, ✅ contracts/, ✅ quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

**Execution Rule (Updated)**: Implement the full module first (Phases 1–8), then perform ALL tests (Phase 9).  
**Testing Rule**: All test-related tasks are OPEN until Phase 9.

---

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[TaskID]**: Sequential task number (T001, T002, ...)
- **[P]**: Parallelizable (different files, no dependencies on incomplete tasks)
- **[Story]**: User story label (US1, US2, US3, US4) - ONLY for user story phase tasks
- **File paths**: Absolute paths from repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure (implementation only)

- [X] T001 Create Module_Volvo/Handlers/Commands/ folder for command handlers
- [X] T002 Create Module_Volvo/Handlers/Queries/ folder for query handlers
- [X] T003 Create Module_Volvo/Requests/Commands/ folder for command DTOs
- [X] T004 Create Module_Volvo/Requests/Queries/ folder for query DTOs
- [X] T005 Create Module_Volvo/Validators/ folder for FluentValidation validators

**Checkpoint**: ✅ Folder structure ready for implementation

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T012 Verify MediatR 12.4.1 registered in App.xaml.cs with assembly scanning
- [X] T013 Verify FluentValidation 11.10.0 pipeline behavior registered in App.xaml.cs
- [X] T014 [P] Verify LoggingBehavior, ValidationBehavior, AuditBehavior are active
- [X] T015 [P] Install recommended NuGet packages: Mapster (for DTO mapping), Ardalis.GuardClauses, Bogus (test data), FsCheck.Xunit (property tests), Verify.Xunit (golden files)
- [X] T016 [P] Create shared DTO: ShipmentLineDto in Module_Volvo/Requests/ShipmentLineDto.cs
- [X] T017 [P] Create shared DTO: InitialShipmentData in Module_Volvo/Requests/Queries/GetInitialShipmentDataQuery.cs
- [X] T018 [P] Create shared DTO: ShipmentDetail in Module_Volvo/Requests/Queries/GetShipmentDetailQuery.cs
- [X] T019 [P] Create shared DTO: ImportPartsCsvResult in Module_Volvo/Requests/Commands/ImportPartsCsvCommand.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Volvo Shipment Entry with CQRS (Priority: P1) 🎯 MVP

**Goal**: Enable users to create and manage Volvo shipments using CQRS handlers (initialize, search parts, add parts, generate labels, complete shipment, load pending)

**Independent Test**: Create test shipment with multiple parts, generate labels, complete shipment. Verify CSV label output byte-for-byte matches golden file and database records are correct.

### Query Handlers for User Story 1

- [X] T023 [P] [US1] Create GetInitialShipmentDataQuery DTO in Module_Volvo/Requests/Queries/GetInitialShipmentDataQuery.cs
- [X] T024 [P] [US1] Create GetInitialShipmentDataQueryHandler in Module_Volvo/Handlers/Queries/GetInitialShipmentDataQueryHandler.cs
- [X] T025 [P] [US1] Create GetPendingShipmentQuery DTO in Module_Volvo/Requests/Queries/GetPendingShipmentQuery.cs
- [X] T026 [P] [US1] Create GetPendingShipmentQueryHandler in Module_Volvo/Handlers/Queries/GetPendingShipmentQueryHandler.cs
- [X] T027 [P] [US1] Create SearchVolvoPartsQuery DTO in Module_Volvo/Requests/Queries/SearchVolvoPartsQuery.cs
- [X] T028 [P] [US1] Create SearchVolvoPartsQueryHandler in Module_Volvo/Handlers/Queries/SearchVolvoPartsQueryHandler.cs
- [X] T029 [P] [US1] Create GenerateLabelCsvQuery DTO in Module_Volvo/Requests/Queries/GenerateLabelCsvQuery.cs
- [X] T030 [P] [US1] Create GenerateLabelCsvQueryHandler in Module_Volvo/Handlers/Queries/GenerateLabelCsvQueryHandler.cs (functional parity with legacy CSV format)

### Command Handlers for User Story 1

- [X] T031 [P] [US1] Create AddPartToShipmentCommand DTO in Module_Volvo/Requests/Commands/AddPartToShipmentCommand.cs
- [X] T032 [US1] Create AddPartToShipmentCommandValidator in Module_Volvo/Validators/AddPartToShipmentCommandValidator.cs (PartNumber required, ReceivedSkidCount > 0, HasDiscrepancy → ExpectedSkidCount + DiscrepancyNote required)
- [X] T033 [US1] Create AddPartToShipmentCommandHandler in Module_Volvo/Handlers/Commands/AddPartToShipmentCommandHandler.cs
- [X] T034 [P] [US1] Create RemovePartFromShipmentCommand DTO in Module_Volvo/Requests/Commands/RemovePartFromShipmentCommand.cs
- [X] T035 [P] [US1] Create RemovePartFromShipmentCommandHandler in Module_Volvo/Handlers/Commands/RemovePartFromShipmentCommandHandler.cs
- [X] T036 [P] [US1] Create SavePendingShipmentCommand DTO in Module_Volvo/Requests/Commands/SavePendingShipmentCommand.cs
- [X] T037 [US1] Create SavePendingShipmentCommandValidator in Module_Volvo/Validators/SavePendingShipmentCommandValidator.cs (ShipmentDate <= Now, Parts.Count > 0)
- [X] T038 [US1] Create SavePendingShipmentCommandHandler in Module_Volvo/Handlers/Commands/SavePendingShipmentCommandHandler.cs
- [X] T039 [P] [US1] Create CompleteShipmentCommand DTO in Module_Volvo/Requests/Commands/CompleteShipmentCommand.cs
- [X] T040 [US1] Create CompleteShipmentCommandValidator in Module_Volvo/Validators/CompleteShipmentCommandValidator.cs (ShipmentDate <= Now, Parts.Count > 0, all parts validated)
- [X] T041 [US1] Create CompleteShipmentCommandHandler in Module_Volvo/Handlers/Commands/CompleteShipmentCommandHandler.cs (save, generate labels, send email)

### ViewModel Refactoring for User Story 1

- [X] T042 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Inject IMediator, replace InitializeAsync with GetInitialShipmentDataQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [X] T043 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace LoadPendingShipmentAsync with GetPendingShipmentQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [X] T044 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace UpdatePartSuggestions with SearchVolvoPartsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [X] T045 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace AddPart with AddPartToShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [X] T046 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace RemovePart with RemovePartFromShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [X] T047 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace GenerateLabelsAsync with GenerateLabelCsvQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [X] T048 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace SaveAsPendingAsync with SavePendingShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [X] T049 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace CompleteShipmentAsync with CompleteShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [X] T050 [US1] Mark IService_Volvo injection in ViewModel_Volvo_ShipmentEntry as [Obsolete] (keep for gradual migration)

**Checkpoint**: User Story 1 implementation complete

---

## Phase 4: User Story 2 - Volvo Shipment History & Editing (Priority: P2)

**Goal**: Enable users to view historical shipments with filtering and edit existing shipments using CQRS handlers

**Independent Test**: Filter shipments by date range and status, edit a completed shipment, verify database updates successful

### Query Handlers for User Story 2

- [X] T064 [P] [US2] Create GetRecentShipmentsQuery DTO in Module_Volvo/Requests/Queries/GetRecentShipmentsQuery.cs
- [X] T065 [P] [US2] Create GetRecentShipmentsQueryHandler in Module_Volvo/Handlers/Queries/GetRecentShipmentsQueryHandler.cs
- [X] T066 [P] [US2] Create GetShipmentHistoryQuery DTO in Module_Volvo/Requests/Queries/GetShipmentHistoryQuery.cs
- [X] T067 [P] [US2] Create GetShipmentHistoryQueryHandler in Module_Volvo/Handlers/Queries/GetShipmentHistoryQueryHandler.cs
- [X] T068 [P] [US2] Create GetShipmentDetailQuery DTO in Module_Volvo/Requests/Queries/GetShipmentDetailQuery.cs
- [X] T069 [P] [US2] Create GetShipmentDetailQueryHandler in Module_Volvo/Handlers/Queries/GetShipmentDetailQueryHandler.cs
- [X] T070 [P] [US2] Create ExportShipmentsQuery DTO in Module_Volvo/Requests/Queries/ExportShipmentsQuery.cs
- [X] T071 [P] [US2] Create ExportShipmentsQueryHandler in Module_Volvo/Handlers/Queries/ExportShipmentsQueryHandler.cs

### Command Handlers for User Story 2

- [X] T072 [P] [US2] Create UpdateShipmentCommand DTO in Module_Volvo/Requests/Commands/UpdateShipmentCommand.cs
- [X] T073 [US2] Create UpdateShipmentCommandValidator in Module_Volvo/Validators/UpdateShipmentCommandValidator.cs (ShipmentId > 0, Parts.Count > 0)
- [X] T074 [US2] Create UpdateShipmentCommandHandler in Module_Volvo/Handlers/Commands/UpdateShipmentCommandHandler.cs

### ViewModel Refactoring for User Story 2

- [X] T075 [US2] Refactor ViewModel_Volvo_History: Inject IMediator, replace constructor initialization with GetRecentShipmentsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [X] T076 [US2] Refactor ViewModel_Volvo_History: Replace FilterAsync with GetShipmentHistoryQuery in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [X] T077 [US2] Refactor ViewModel_Volvo_History: Replace ViewDetailAsync with GetShipmentDetailQuery in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [X] T078 [US2] Refactor ViewModel_Volvo_History: Replace EditAsync with UpdateShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [X] T079 [US2] Refactor ViewModel_Volvo_History: Replace ExportAsync with ExportShipmentsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [X] T080 [US2] Mark IService_Volvo injection in ViewModel_Volvo_History as [Obsolete]

### XAML Binding Migration for User Story 2

- [X] T081 [US2] Migrate View_Volvo_History.xaml: Convert 6 DataGridTextColumn to DataGridTemplateColumn with x:Bind (ShipmentNumber, ShipmentDate, PONumber, ReceiverNumber, Status, Notes) in Module_Volvo/Views/View_Volvo_History.xaml
- [X] T082 [US2] Test View_Volvo_History: Verify all bindings work after migration, no compile errors

**Checkpoint**: User Story 2 implementation complete

---

## Phase 5: User Story 3 - Volvo Master Data Management (Priority: P2)

**Goal**: Enable users to manage Volvo part master data (add, edit, deactivate, view components, export/import CSV) using CQRS handlers

**Independent Test**: Add new part with components, edit part details, deactivate obsolete part, verify database changes

### Query Handlers for User Story 3

- [X] T091 [P] [US3] Create GetAllVolvoPartsQuery DTO in Module_Volvo/Requests/Queries/GetAllVolvoPartsQuery.cs
- [X] T092 [P] [US3] Create GetAllVolvoPartsQueryHandler in Module_Volvo/Handlers/Queries/GetAllVolvoPartsQueryHandler.cs
- [X] T093 [P] [US3] Create GetPartComponentsQuery DTO in Module_Volvo/Requests/Queries/GetPartComponentsQuery.cs
- [X] T094 [P] [US3] Create GetPartComponentsQueryHandler in Module_Volvo/Handlers/Queries/GetPartComponentsQueryHandler.cs
- [X] T095 [P] [US3] Create ExportPartsCsvQuery DTO in Module_Volvo/Requests/Queries/ExportPartsCsvQuery.cs
- [X] T096 [P] [US3] Create ExportPartsCsvQueryHandler in Module_Volvo/Handlers/Queries/ExportPartsCsvQueryHandler.cs

### Command Handlers for User Story 3

- [X] T097 [P] [US3] Create AddVolvoPartCommand DTO in Module_Volvo/Requests/Commands/AddVolvoPartCommand.cs
- [X] T098 [US3] Create AddVolvoPartCommandValidator in Module_Volvo/Validators/AddVolvoPartCommandValidator.cs (PartNumber required + unique, QuantityPerSkid > 0)
- [X] T099 [US3] Create AddVolvoPartCommandHandler in Module_Volvo/Handlers/Commands/AddVolvoPartCommandHandler.cs
- [X] T100 [P] [US3] Create UpdateVolvoPartCommand DTO in Module_Volvo/Requests/Commands/UpdateVolvoPartCommand.cs
- [X] T101 [US3] Create UpdateVolvoPartCommandValidator in Module_Volvo/Validators/UpdateVolvoPartCommandValidator.cs (PartId > 0, PartNumber required, QuantityPerSkid > 0)
- [X] T102 [US3] Create UpdateVolvoPartCommandHandler in Module_Volvo/Handlers/Commands/UpdateVolvoPartCommandHandler.cs
- [X] T103 [P] [US3] Create DeactivateVolvoPartCommand DTO in Module_Volvo/Requests/Commands/DeactivateVolvoPartCommand.cs
- [X] T104 [US3] Create DeactivateVolvoPartCommandValidator in Module_Volvo/Validators/DeactivateVolvoPartCommandValidator.cs (PartId > 0, no pending shipment references)
- [X] T105 [US3] Create DeactivateVolvoPartCommandHandler in Module_Volvo/Handlers/Commands/DeactivateVolvoPartCommandHandler.cs
- [X] T106 [P] [US3] Create ImportPartsCsvCommand DTO in Module_Volvo/Requests/Commands/ImportPartsCsvCommand.cs
- [X] T107 [US3] Create ImportPartsCsvCommandValidator in Module_Volvo/Validators/ImportPartsCsvCommandValidator.cs (CsvFilePath required + exists, per-row validation)
- [X] T108 [US3] Create ImportPartsCsvCommandHandler in Module_Volvo/Handlers/Commands/ImportPartsCsvCommandHandler.cs

### ViewModel Refactoring for User Story 3

- [X] T109 [US3] Refactor ViewModel_Volvo_Settings: Inject IMediator, replace constructor initialization with GetAllVolvoPartsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [X] T110 [US3] Refactor ViewModel_Volvo_Settings: Replace RefreshAsync with GetAllVolvoPartsQuery (with IncludeInactive filter) in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [X] T111 [US3] Refactor ViewModel_Volvo_Settings: Replace AddPartAsync with AddVolvoPartCommand in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [X] T112 [US3] Refactor ViewModel_Volvo_Settings: Replace EditPartAsync with UpdateVolvoPartCommand in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [X] T113 [US3] Refactor ViewModel_Volvo_Settings: Replace DeactivatePartAsync with DeactivateVolvoPartCommand in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [X] T114 [US3] Refactor ViewModel_Volvo_Settings: Replace ViewComponentsAsync with GetPartComponentsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [X] T115 [US3] Refactor ViewModel_Volvo_Settings: Replace ExportCsvAsync with ExportPartsCsvQuery in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [X] T116 [US3] Refactor ViewModel_Volvo_Settings: Replace ImportCsvAsync with ImportPartsCsvCommand in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [X] T117 [US3] Mark IService_VolvoMasterData injection in ViewModel_Volvo_Settings as [Obsolete]

### XAML Binding Migration for User Story 3

- [X] T118 [US3] Migrate View_Volvo_Settings.xaml: Convert 2 DataGridTextColumn to DataGridTemplateColumn with x:Bind (PartNumber, QuantityPerSkid) in Module_Volvo/Views/View_Volvo_Settings.xaml
- [X] T119 [US3] Test View_Volvo_Settings: Verify all bindings work after migration, no compile errors

**Checkpoint**: User Story 3 implementation complete

---

## Phase 6: User Story 4 - Email Notification & Label Preview (Priority: P3)

**Goal**: Enable users to preview email notifications and view formatted email data before completing shipments

**Independent Test**: Preview email for shipment with multiple parts and discrepancies, verify HTML/text formatting matches golden files

### Query Handlers for User Story 4

- [X] T133 [P] [US4] Create FormatEmailDataQuery DTO in Module_Volvo/Requests/Queries/FormatEmailDataQuery.cs
- [X] T134 [P] [US4] Create FormatEmailDataQueryHandler in Module_Volvo/Handlers/Queries/FormatEmailDataQueryHandler.cs (functional parity with legacy email format)

### ViewModel Integration for User Story 4

- [X] T135 [US4] Refactor ViewModel_Volvo_ShipmentEntry: Replace PreviewEmailAsync with FormatEmailDataQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs

**Checkpoint**: User Story 4 implementation complete

---

## Phase 7: XAML Binding Migration (Remaining Views)

**Purpose**: Migrate remaining `{Binding}` occurrences to `x:Bind` for constitutional compliance

- [X] T139 [P] Migrate View_Volvo_ShipmentEntry.xaml: Convert 6 DataGridTextColumn to DataGridTemplateColumn with x:Bind in DataGrid columns in Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml
- [X] T140 Test View_Volvo_ShipmentEntry: Verify all bindings work, compile successfully
- [X] T141 [P] Migrate VolvoShipmentEditDialog.xaml: Convert 6 DataGridTextColumn to DataGridTemplateColumn with x:Bind in Module_Volvo/Views/VolvoShipmentEditDialog.xaml
- [X] T142 Test VolvoShipmentEditDialog: Verify all bindings work, compile successfully

**Checkpoint**: All `{Binding}` occurrences migrated to `x:Bind` - Principle I compliance achieved

---

## Phase 8: Service Deprecation & Cleanup

**Purpose**: Remove legacy services after full CQRS migration

- [X] T148 Verify all 3 ViewModels use IMediator only (grep search for IMediator presence in ViewModels)
- [X] T149 Verify no direct service calls remain (grep search for IService_Volvo and IService_VolvoMasterData in ViewModels)
- [X] T150 Remove [Obsolete] legacy services: Mark Service_Volvo and Service_VolvoMasterData as fully deprecated in Module_Volvo/Services/
- [X] T151 [P] Update App.xaml.cs: Remove Service_Volvo and Service_VolvoMasterData registrations (handlers auto-registered via MediatR assembly scan)

**Checkpoint**: Legacy services removed - CQRS migration complete

---

## Phase 9: Testing & Validation (ALL test tasks after implementation)

**Purpose**: Execute all tests and validation after full implementation

### Test Infrastructure

- [ ] T006 [P] Create MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/ folder for command handler tests
- [ ] T007 [P] Create MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/ folder for query handler tests
- [ ] T008 [P] Create MTM_Receiving_Application.Tests/Module_Volvo/Validators/ folder for validator tests
- [ ] T009 [P] Create MTM_Receiving_Application.Tests/Module_Volvo/Integration/ folder for integration tests
- [ ] T010 [P] Create MTM_Receiving_Application.Tests/Module_Volvo/PropertyBased/ folder for property-based tests
- [ ] T011 [P] Create MTM_Receiving_Application.Tests/Module_Volvo/GoldenFiles/ folder for golden file tests

### Unit Tests for User Story 1

- [ ] T051 [P] [US1] Unit test GetInitialShipmentDataQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/GetInitialShipmentDataQueryHandlerTests.cs
- [ ] T052 [P] [US1] Unit test GetPendingShipmentQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/GetPendingShipmentQueryHandlerTests.cs
- [ ] T053 [P] [US1] Unit test SearchVolvoPartsQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/SearchVolvoPartsQueryHandlerTests.cs
- [ ] T054 [P] [US1] Unit test GenerateLabelCsvQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/GenerateLabelCsvQueryHandlerTests.cs
- [ ] T055 [P] [US1] Unit test AddPartToShipmentCommandValidator in MTM_Receiving_Application.Tests/Module_Volvo/Validators/AddPartToShipmentCommandValidatorTests.cs
- [ ] T056 [P] [US1] Unit test AddPartToShipmentCommandHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/AddPartToShipmentCommandHandlerTests.cs
- [ ] T057 [P] [US1] Unit test SavePendingShipmentCommandValidator in MTM_Receiving_Application.Tests/Module_Volvo/Validators/SavePendingShipmentCommandValidatorTests.cs
- [ ] T058 [P] [US1] Unit test SavePendingShipmentCommandHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/SavePendingShipmentCommandHandlerTests.cs
- [ ] T059 [P] [US1] Unit test CompleteShipmentCommandValidator in MTM_Receiving_Application.Tests/Module_Volvo/Validators/CompleteShipmentCommandValidatorTests.cs
- [ ] T060 [P] [US1] Unit test CompleteShipmentCommandHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/CompleteShipmentCommandHandlerTests.cs

### Integration & Golden File Tests for User Story 1

- [ ] T020 [P] DEFERRED: Capture golden file: expected_label_basic.csv (moved to T061 - integration test)
- [ ] T061 [US1] Golden file test: GenerateLabelCsvQuery produces byte-for-byte match with expected_label_basic.csv in MTM_Receiving_Application.Tests/Module_Volvo/GoldenFiles/LabelCsvGoldenFileTests.cs
- [ ] T062 [US1] Integration test: Complete shipment workflow (ViewModel → Handler → Service → DAO → DB) in MTM_Receiving_Application.Tests/Module_Volvo/Integration/ShipmentCompletionIntegrationTests.cs
- [ ] T063 [US1] Integration test: Pending shipment save/load workflow in MTM_Receiving_Application.Tests/Module_Volvo/Integration/PendingShipmentIntegrationTests.cs

### Unit Tests for User Story 2

- [ ] T083 [P] [US2] Unit test GetRecentShipmentsQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/GetRecentShipmentsQueryHandlerTests.cs
- [ ] T084 [P] [US2] Unit test GetShipmentHistoryQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/GetShipmentHistoryQueryHandlerTests.cs
- [ ] T085 [P] [US2] Unit test GetShipmentDetailQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/GetShipmentDetailQueryHandlerTests.cs
- [ ] T086 [P] [US2] Unit test ExportShipmentsQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/ExportShipmentsQueryHandlerTests.cs
- [ ] T087 [P] [US2] Unit test UpdateShipmentCommandValidator in MTM_Receiving_Application.Tests/Module_Volvo/Validators/UpdateShipmentCommandValidatorTests.cs
- [ ] T088 [P] [US2] Unit test UpdateShipmentCommandHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/UpdateShipmentCommandHandlerTests.cs

### Integration Tests for User Story 2

- [ ] T089 [US2] Integration test: History filtering by date range and status in MTM_Receiving_Application.Tests/Module_Volvo/Integration/HistoryFilteringIntegrationTests.cs
- [ ] T090 [US2] Integration test: Edit shipment workflow (load detail → update → verify) in MTM_Receiving_Application.Tests/Module_Volvo/Integration/ShipmentEditIntegrationTests.cs

### Unit Tests for User Story 3

- [ ] T120 [P] [US3] Unit test GetAllVolvoPartsQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/GetAllVolvoPartsQueryHandlerTests.cs
- [ ] T121 [P] [US3] Unit test GetPartComponentsQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/GetPartComponentsQueryHandlerTests.cs
- [ ] T122 [P] [US3] Unit test ExportPartsCsvQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/ExportPartsCsvQueryHandlerTests.cs
- [ ] T123 [P] [US3] Unit test AddVolvoPartCommandValidator in MTM_Receiving_Application.Tests/Module_Volvo/Validators/AddVolvoPartCommandValidatorTests.cs
- [ ] T124 [P] [US3] Unit test AddVolvoPartCommandHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/AddVolvoPartCommandHandlerTests.cs
- [ ] T125 [P] [US3] Unit test UpdateVolvoPartCommandValidator in MTM_Receiving_Application.Tests/Module_Volvo/Validators/UpdateVolvoPartCommandValidatorTests.cs
- [ ] T126 [P] [US3] Unit test UpdateVolvoPartCommandHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/UpdateVolvoPartCommandHandlerTests.cs
- [ ] T127 [P] [US3] Unit test DeactivateVolvoPartCommandValidator in MTM_Receiving_Application.Tests/Module_Volvo/Validators/DeactivateVolvoPartCommandValidatorTests.cs
- [ ] T128 [P] [US3] Unit test DeactivateVolvoPartCommandHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/DeactivateVolvoPartCommandHandlerTests.cs
- [ ] T129 [P] [US3] Unit test ImportPartsCsvCommandValidator in MTM_Receiving_Application.Tests/Module_Volvo/Validators/ImportPartsCsvCommandValidatorTests.cs
- [ ] T130 [P] [US3] Unit test ImportPartsCsvCommandHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Commands/ImportPartsCsvCommandHandlerTests.cs

### Integration Tests for User Story 3

- [ ] T131 [US3] Integration test: Add part workflow (ViewModel → Handler → DAO → DB) in MTM_Receiving_Application.Tests/Module_Volvo/Integration/AddPartIntegrationTests.cs
- [ ] T132 [US3] Integration test: Import CSV workflow with validation errors in MTM_Receiving_Application.Tests/Module_Volvo/Integration/ImportCsvIntegrationTests.cs

### User Story 4 Tests

- [ ] T136 [P] [US4] Unit test FormatEmailDataQueryHandler in MTM_Receiving_Application.Tests/Module_Volvo/Handlers/Queries/FormatEmailDataQueryHandlerTests.cs
- [ ] T137 [US4] Golden file test: FormatEmailDataQuery HTML output matches expected_email_html.html in MTM_Receiving_Application.Tests/Module_Volvo/GoldenFiles/EmailGoldenFileTests.cs
- [ ] T138 [US4] Golden file test: FormatEmailDataQuery plain text output matches expected_email_text.txt in MTM_Receiving_Application.Tests/Module_Volvo/GoldenFiles/EmailGoldenFileTests.cs

### Property-Based & Advanced Testing

- [ ] T143 [P] Create VolvoArbitraries.cs: FsCheck arbitrary generators for Volvo models in MTM_Receiving_Application.Tests/Module_Volvo/PropertyBased/VolvoArbitraries.cs
- [ ] T144 [P] Create VolvoTestDataGenerator.cs: Bogus faker for Volvo models in MTM_Receiving_Application.Tests/Module_Volvo/Fixtures/VolvoTestDataGenerator.cs
- [ ] T145 Property-based test: Component explosion calculation (1000+ test cases comparing legacy vs CQRS) in MTM_Receiving_Application.Tests/Module_Volvo/PropertyBased/ComponentExplosionPropertyTests.cs
- [ ] T146 Property-based test: Piece count calculation (1000+ test cases) in MTM_Receiving_Application.Tests/Module_Volvo/PropertyBased/PieceCountCalculationPropertyTests.cs
- [ ] T147 Golden file test: Generate label CSV with discrepancies, verify matches expected_label_with_discrepancy.csv in MTM_Receiving_Application.Tests/Module_Volvo/GoldenFiles/LabelCsvGoldenFileTests.cs

### Final Validation & Test Runs

- [ ] T021 [P] DEFERRED: Capture golden file: expected_email_html.html (moved to Phase 9 testing)
- [ ] T022 [P] DEFERRED: Capture golden file: expected_email_text.txt (moved to Phase 9 testing)
- [ ] T152 Run all existing integration tests to verify zero regressions (dotnet test --filter "FullyQualifiedName~Module_Volvo")
- [ ] T153 [P] Run test coverage report: Verify 80%+ coverage for handlers and validators (dotnet test --collect:"XPlat Code Coverage")
- [ ] T154 [P] Performance benchmark: Compare shipment completion time legacy vs CQRS (target: ≤ current average, <10ms MediatR overhead)
- [ ] T155 [P] Build verification: dotnet build -c Release --no-restore (target: 0 errors, 0 warnings)
- [ ] T161 Final integration test: Complete end-to-end shipment workflow (entry → pending → resume → complete → history → edit)

---

## Phase 10: Polish & Documentation

**Purpose**: Final improvements, documentation, and validation

- [ ] T156 [P] Update Module_Volvo/README.md: Document CQRS architecture, handler list, migration notes
- [ ] T157 [P] Create Module_Volvo/HANDLERS.md: Comprehensive handler documentation with examples
- [ ] T158 [P] Update .github/copilot-instructions.md: Add Volvo-specific CQRS examples
- [ ] T159 Validate quickstart.md: Follow step-by-step guide to ensure accuracy
- [ ] T160 Run module-compliance-auditor: Verify zero constitutional violations (@module-compliance-auditor Module_Volvo)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational (Phase 2) completion
- **XAML Migration (Phase 7)**: Depends on corresponding ViewModel refactoring
- **Service Deprecation (Phase 8)**: Depends on all user story implementations
- **Testing & Validation (Phase 9)**: Runs AFTER all implementation phases complete
- **Polish (Phase 10)**: Runs AFTER testing/validation

### Within Each User Story

1. Query/Command DTOs (parallelizable)
2. Validators (before command handlers)
3. Handlers (queries parallelizable, commands depend on validators)
4. ViewModel refactoring
5. XAML migration (if applicable)

### Testing Rule

All testing tasks are consolidated into Phase 9 and MUST run only after full implementation is complete.

---

**Tasks Status**: ✅ RECREATED (Implementation first, tests last)  
**Next Step**: Continue implementation tasks in Phase 4 (User Story 2)

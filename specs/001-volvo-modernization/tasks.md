# Tasks: Module_Volvo CQRS Modernization

**Branch**: `001-volvo-modernization`  
**Input**: Design documents from `/specs/001-volvo-modernization/`  
**Prerequisites**: ✅ plan.md, ✅ spec.md, ✅ research.md, ✅ data-model.md, ✅ contracts/, ✅ quickstart.md

**Organization**: Tasks organized by user story to enable independent implementation and testing of each story.

---

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[TaskID]**: Sequential task number (T001, T002, ...)
- **[P]**: Parallelizable (different files, no dependencies on incomplete tasks)
- **[Story]**: User story label (US1, US2, US3, US4) - ONLY for user story phase tasks
- **File paths**: Absolute paths from repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and folder structure creation

- [X] T001 Create Module_Volvo/Handlers/Commands/ folder for command handlers
- [X] T002 Create Module_Volvo/Handlers/Queries/ folder for query handlers
- [X] T003 Create Module_Volvo/Requests/Commands/ folder for command DTOs
- [X] T004 Create Module_Volvo/Requests/Queries/ folder for query DTOs
- [X] T005 Create Module_Volvo/Validators/ folder for FluentValidation validators
- [X] T006 [P] Create Module_Volvo.Tests/Handlers/Commands/ folder for command handler tests
- [X] T007 [P] Create Module_Volvo.Tests/Handlers/Queries/ folder for query handler tests
- [X] T008 [P] Create Module_Volvo.Tests/Validators/ folder for validator tests
- [X] T009 [P] Create Module_Volvo.Tests/Integration/ folder for integration tests
- [X] T010 [P] Create Module_Volvo.Tests/PropertyBased/ folder for property-based tests
- [X] T011 [P] Create Module_Volvo.Tests/GoldenFiles/ folder for golden file tests

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
- [ ] T020 [P] Capture golden file: expected_label_basic.csv (create test shipment, save CSV to Module_Volvo.Tests/GoldenFiles/)
- [ ] T021 [P] Capture golden file: expected_email_html.html (use Preview Email, save HTML to Module_Volvo.Tests/GoldenFiles/)
- [ ] T022 [P] Capture golden file: expected_email_text.txt (use Preview Email, save plain text to Module_Volvo.Tests/GoldenFiles/)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Volvo Shipment Entry with CQRS (Priority: P1) 🎯 MVP

**Goal**: Enable users to create and manage Volvo shipments using CQRS handlers (initialize, search parts, add parts, generate labels, complete shipment, load pending)

**Independent Test**: Create test shipment with multiple parts, generate labels, complete shipment. Verify CSV label output byte-for-byte matches golden file and database records are correct.

### Query Handlers for User Story 1

- [ ] T023 [P] [US1] Create GetInitialShipmentDataQuery DTO in Module_Volvo/Requests/Queries/GetInitialShipmentDataQuery.cs
- [ ] T024 [P] [US1] Create GetInitialShipmentDataQueryHandler in Module_Volvo/Handlers/Queries/GetInitialShipmentDataQueryHandler.cs
- [ ] T025 [P] [US1] Create GetPendingShipmentQuery DTO in Module_Volvo/Requests/Queries/GetPendingShipmentQuery.cs
- [ ] T026 [P] [US1] Create GetPendingShipmentQueryHandler in Module_Volvo/Handlers/Queries/GetPendingShipmentQueryHandler.cs
- [ ] T027 [P] [US1] Create SearchVolvoPartsQuery DTO in Module_Volvo/Requests/Queries/SearchVolvoPartsQuery.cs
- [ ] T028 [P] [US1] Create SearchVolvoPartsQueryHandler in Module_Volvo/Handlers/Queries/SearchVolvoPartsQueryHandler.cs
- [ ] T029 [P] [US1] Create GenerateLabelCsvQuery DTO in Module_Volvo/Requests/Queries/GenerateLabelCsvQuery.cs
- [ ] T030 [P] [US1] Create GenerateLabelCsvQueryHandler in Module_Volvo/Handlers/Queries/GenerateLabelCsvQueryHandler.cs (functional parity with legacy CSV format)

### Command Handlers for User Story 1

- [ ] T031 [P] [US1] Create AddPartToShipmentCommand DTO in Module_Volvo/Requests/Commands/AddPartToShipmentCommand.cs
- [ ] T032 [US1] Create AddPartToShipmentCommandValidator in Module_Volvo/Validators/AddPartToShipmentCommandValidator.cs (PartNumber required, ReceivedSkidCount > 0, HasDiscrepancy → ExpectedSkidCount + DiscrepancyNote required)
- [ ] T033 [US1] Create AddPartToShipmentCommandHandler in Module_Volvo/Handlers/Commands/AddPartToShipmentCommandHandler.cs
- [ ] T034 [P] [US1] Create RemovePartFromShipmentCommand DTO in Module_Volvo/Requests/Commands/RemovePartFromShipmentCommand.cs
- [ ] T035 [P] [US1] Create RemovePartFromShipmentCommandHandler in Module_Volvo/Handlers/Commands/RemovePartFromShipmentCommandHandler.cs
- [ ] T036 [P] [US1] Create SavePendingShipmentCommand DTO in Module_Volvo/Requests/Commands/SavePendingShipmentCommand.cs
- [ ] T037 [US1] Create SavePendingShipmentCommandValidator in Module_Volvo/Validators/SavePendingShipmentCommandValidator.cs (ShipmentDate <= Now, Parts.Count > 0)
- [ ] T038 [US1] Create SavePendingShipmentCommandHandler in Module_Volvo/Handlers/Commands/SavePendingShipmentCommandHandler.cs
- [ ] T039 [P] [US1] Create CompleteShipmentCommand DTO in Module_Volvo/Requests/Commands/CompleteShipmentCommand.cs
- [ ] T040 [US1] Create CompleteShipmentCommandValidator in Module_Volvo/Validators/CompleteShipmentCommandValidator.cs (ShipmentDate <= Now, Parts.Count > 0, all parts validated)
- [ ] T041 [US1] Create CompleteShipmentCommandHandler in Module_Volvo/Handlers/Commands/CompleteShipmentCommandHandler.cs (save, generate labels, send email)

### ViewModel Refactoring for User Story 1

- [ ] T042 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Inject IMediator, replace InitializeAsync with GetInitialShipmentDataQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [ ] T043 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace LoadPendingShipmentAsync with GetPendingShipmentQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [ ] T044 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace UpdatePartSuggestions with SearchVolvoPartsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [ ] T045 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace AddPart with AddPartToShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [ ] T046 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace RemovePart with RemovePartFromShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [ ] T047 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace GenerateLabelsAsync with GenerateLabelCsvQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [ ] T048 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace SaveAsPendingAsync with SavePendingShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [ ] T049 [US1] Refactor ViewModel_Volvo_ShipmentEntry: Replace CompleteShipmentAsync with CompleteShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
- [ ] T050 [US1] Mark IService_Volvo injection in ViewModel_Volvo_ShipmentEntry as [Obsolete] (keep for gradual migration)

### Unit Tests for User Story 1

- [ ] T051 [P] [US1] Unit test GetInitialShipmentDataQueryHandler in Module_Volvo.Tests/Handlers/Queries/GetInitialShipmentDataQueryHandlerTests.cs
- [ ] T052 [P] [US1] Unit test GetPendingShipmentQueryHandler in Module_Volvo.Tests/Handlers/Queries/GetPendingShipmentQueryHandlerTests.cs
- [ ] T053 [P] [US1] Unit test SearchVolvoPartsQueryHandler in Module_Volvo.Tests/Handlers/Queries/SearchVolvoPartsQueryHandlerTests.cs
- [ ] T054 [P] [US1] Unit test GenerateLabelCsvQueryHandler in Module_Volvo.Tests/Handlers/Queries/GenerateLabelCsvQueryHandlerTests.cs
- [ ] T055 [P] [US1] Unit test AddPartToShipmentCommandValidator in Module_Volvo.Tests/Validators/AddPartToShipmentCommandValidatorTests.cs
- [ ] T056 [P] [US1] Unit test AddPartToShipmentCommandHandler in Module_Volvo.Tests/Handlers/Commands/AddPartToShipmentCommandHandlerTests.cs
- [ ] T057 [P] [US1] Unit test SavePendingShipmentCommandValidator in Module_Volvo.Tests/Validators/SavePendingShipmentCommandValidatorTests.cs
- [ ] T058 [P] [US1] Unit test SavePendingShipmentCommandHandler in Module_Volvo.Tests/Handlers/Commands/SavePendingShipmentCommandHandlerTests.cs
- [ ] T059 [P] [US1] Unit test CompleteShipmentCommandValidator in Module_Volvo.Tests/Validators/CompleteShipmentCommandValidatorTests.cs
- [ ] T060 [P] [US1] Unit test CompleteShipmentCommandHandler in Module_Volvo.Tests/Handlers/Commands/CompleteShipmentCommandHandlerTests.cs

### Integration & Golden File Tests for User Story 1

- [ ] T061 [US1] Golden file test: GenerateLabelCsvQuery produces byte-for-byte match with expected_label_basic.csv in Module_Volvo.Tests/GoldenFiles/LabelCsvGoldenFileTests.cs
- [ ] T062 [US1] Integration test: Complete shipment workflow (ViewModel → Handler → Service → DAO → DB) in Module_Volvo.Tests/Integration/ShipmentCompletionIntegrationTests.cs
- [ ] T063 [US1] Integration test: Pending shipment save/load workflow in Module_Volvo.Tests/Integration/PendingShipmentIntegrationTests.cs

**Checkpoint**: User Story 1 complete - shipment entry workflow fully functional with CQRS, functional parity verified

---

## Phase 4: User Story 2 - Volvo Shipment History & Editing (Priority: P2)

**Goal**: Enable users to view historical shipments with filtering and edit existing shipments using CQRS handlers

**Independent Test**: Filter shipments by date range and status, edit a completed shipment, verify database updates successful

### Query Handlers for User Story 2

- [ ] T064 [P] [US2] Create GetRecentShipmentsQuery DTO in Module_Volvo/Requests/Queries/GetRecentShipmentsQuery.cs
- [ ] T065 [P] [US2] Create GetRecentShipmentsQueryHandler in Module_Volvo/Handlers/Queries/GetRecentShipmentsQueryHandler.cs
- [ ] T066 [P] [US2] Create GetShipmentHistoryQuery DTO in Module_Volvo/Requests/Queries/GetShipmentHistoryQuery.cs
- [ ] T067 [P] [US2] Create GetShipmentHistoryQueryHandler in Module_Volvo/Handlers/Queries/GetShipmentHistoryQueryHandler.cs
- [ ] T068 [P] [US2] Create GetShipmentDetailQuery DTO in Module_Volvo/Requests/Queries/GetShipmentDetailQuery.cs
- [ ] T069 [P] [US2] Create GetShipmentDetailQueryHandler in Module_Volvo/Handlers/Queries/GetShipmentDetailQueryHandler.cs
- [ ] T070 [P] [US2] Create ExportShipmentsQuery DTO in Module_Volvo/Requests/Queries/ExportShipmentsQuery.cs
- [ ] T071 [P] [US2] Create ExportShipmentsQueryHandler in Module_Volvo/Handlers/Queries/ExportShipmentsQueryHandler.cs

### Command Handlers for User Story 2

- [ ] T072 [P] [US2] Create UpdateShipmentCommand DTO in Module_Volvo/Requests/Commands/UpdateShipmentCommand.cs
- [ ] T073 [US2] Create UpdateShipmentCommandValidator in Module_Volvo/Validators/UpdateShipmentCommandValidator.cs (ShipmentId > 0, Parts.Count > 0)
- [ ] T074 [US2] Create UpdateShipmentCommandHandler in Module_Volvo/Handlers/Commands/UpdateShipmentCommandHandler.cs

### ViewModel Refactoring for User Story 2

- [ ] T075 [US2] Refactor ViewModel_Volvo_History: Inject IMediator, replace constructor initialization with GetRecentShipmentsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [ ] T076 [US2] Refactor ViewModel_Volvo_History: Replace FilterAsync with GetShipmentHistoryQuery in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [ ] T077 [US2] Refactor ViewModel_Volvo_History: Replace ViewDetailAsync with GetShipmentDetailQuery in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [ ] T078 [US2] Refactor ViewModel_Volvo_History: Replace EditAsync with UpdateShipmentCommand in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [ ] T079 [US2] Refactor ViewModel_Volvo_History: Replace ExportAsync with ExportShipmentsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
- [ ] T080 [US2] Mark IService_Volvo injection in ViewModel_Volvo_History as [Obsolete]

### XAML Binding Migration for User Story 2

- [ ] T081 [US2] Migrate View_Volvo_History.xaml: Convert 6 DataGridTextColumn to DataGridTemplateColumn with x:Bind (ShipmentNumber, ShipmentDate, PONumber, ReceiverNumber, Status, Notes) in Module_Volvo/Views/View_Volvo_History.xaml
- [ ] T082 [US2] Test View_Volvo_History: Verify all bindings work after migration, no compile errors

### Unit Tests for User Story 2

- [ ] T083 [P] [US2] Unit test GetRecentShipmentsQueryHandler in Module_Volvo.Tests/Handlers/Queries/GetRecentShipmentsQueryHandlerTests.cs
- [ ] T084 [P] [US2] Unit test GetShipmentHistoryQueryHandler in Module_Volvo.Tests/Handlers/Queries/GetShipmentHistoryQueryHandlerTests.cs
- [ ] T085 [P] [US2] Unit test GetShipmentDetailQueryHandler in Module_Volvo.Tests/Handlers/Queries/GetShipmentDetailQueryHandlerTests.cs
- [ ] T086 [P] [US2] Unit test ExportShipmentsQueryHandler in Module_Volvo.Tests/Handlers/Queries/ExportShipmentsQueryHandlerTests.cs
- [ ] T087 [P] [US2] Unit test UpdateShipmentCommandValidator in Module_Volvo.Tests/Validators/UpdateShipmentCommandValidatorTests.cs
- [ ] T088 [P] [US2] Unit test UpdateShipmentCommandHandler in Module_Volvo.Tests/Handlers/Commands/UpdateShipmentCommandHandlerTests.cs

### Integration Tests for User Story 2

- [ ] T089 [US2] Integration test: History filtering by date range and status in Module_Volvo.Tests/Integration/HistoryFilteringIntegrationTests.cs
- [ ] T090 [US2] Integration test: Edit shipment workflow (load detail → update → verify) in Module_Volvo.Tests/Integration/ShipmentEditIntegrationTests.cs

**Checkpoint**: User Story 2 complete - history viewing and editing fully functional, independently testable

---

## Phase 5: User Story 3 - Volvo Master Data Management (Priority: P2)

**Goal**: Enable users to manage Volvo part master data (add, edit, deactivate, view components, export/import CSV) using CQRS handlers

**Independent Test**: Add new part with components, edit part details, deactivate obsolete part, verify database changes

### Query Handlers for User Story 3

- [ ] T091 [P] [US3] Create GetAllVolvoPartsQuery DTO in Module_Volvo/Requests/Queries/GetAllVolvoPartsQuery.cs
- [ ] T092 [P] [US3] Create GetAllVolvoPartsQueryHandler in Module_Volvo/Handlers/Queries/GetAllVolvoPartsQueryHandler.cs
- [ ] T093 [P] [US3] Create GetPartComponentsQuery DTO in Module_Volvo/Requests/Queries/GetPartComponentsQuery.cs
- [ ] T094 [P] [US3] Create GetPartComponentsQueryHandler in Module_Volvo/Handlers/Queries/GetPartComponentsQueryHandler.cs
- [ ] T095 [P] [US3] Create ExportPartsCsvQuery DTO in Module_Volvo/Requests/Queries/ExportPartsCsvQuery.cs
- [ ] T096 [P] [US3] Create ExportPartsCsvQueryHandler in Module_Volvo/Handlers/Queries/ExportPartsCsvQueryHandler.cs

### Command Handlers for User Story 3

- [ ] T097 [P] [US3] Create AddVolvoPartCommand DTO in Module_Volvo/Requests/Commands/AddVolvoPartCommand.cs
- [ ] T098 [US3] Create AddVolvoPartCommandValidator in Module_Volvo/Validators/AddVolvoPartCommandValidator.cs (PartNumber required + unique, QuantityPerSkid > 0)
- [ ] T099 [US3] Create AddVolvoPartCommandHandler in Module_Volvo/Handlers/Commands/AddVolvoPartCommandHandler.cs
- [ ] T100 [P] [US3] Create UpdateVolvoPartCommand DTO in Module_Volvo/Requests/Commands/UpdateVolvoPartCommand.cs
- [ ] T101 [US3] Create UpdateVolvoPartCommandValidator in Module_Volvo/Validators/UpdateVolvoPartCommandValidator.cs (PartId > 0, PartNumber required, QuantityPerSkid > 0)
- [ ] T102 [US3] Create UpdateVolvoPartCommandHandler in Module_Volvo/Handlers/Commands/UpdateVolvoPartCommandHandler.cs
- [ ] T103 [P] [US3] Create DeactivateVolvoPartCommand DTO in Module_Volvo/Requests/Commands/DeactivateVolvoPartCommand.cs
- [ ] T104 [US3] Create DeactivateVolvoPartCommandValidator in Module_Volvo/Validators/DeactivateVolvoPartCommandValidator.cs (PartId > 0, no pending shipment references)
- [ ] T105 [US3] Create DeactivateVolvoPartCommandHandler in Module_Volvo/Handlers/Commands/DeactivateVolvoPartCommandHandler.cs
- [ ] T106 [P] [US3] Create ImportPartsCsvCommand DTO in Module_Volvo/Requests/Commands/ImportPartsCsvCommand.cs
- [ ] T107 [US3] Create ImportPartsCsvCommandValidator in Module_Volvo/Validators/ImportPartsCsvCommandValidator.cs (CsvFilePath required + exists, per-row validation)
- [ ] T108 [US3] Create ImportPartsCsvCommandHandler in Module_Volvo/Handlers/Commands/ImportPartsCsvCommandHandler.cs

### ViewModel Refactoring for User Story 3

- [ ] T109 [US3] Refactor ViewModel_Volvo_Settings: Inject IMediator, replace constructor initialization with GetAllVolvoPartsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [ ] T110 [US3] Refactor ViewModel_Volvo_Settings: Replace RefreshAsync with GetAllVolvoPartsQuery (with IncludeInactive filter) in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [ ] T111 [US3] Refactor ViewModel_Volvo_Settings: Replace AddPartAsync with AddVolvoPartCommand in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [ ] T112 [US3] Refactor ViewModel_Volvo_Settings: Replace EditPartAsync with UpdateVolvoPartCommand in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [ ] T113 [US3] Refactor ViewModel_Volvo_Settings: Replace DeactivatePartAsync with DeactivateVolvoPartCommand in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [ ] T114 [US3] Refactor ViewModel_Volvo_Settings: Replace ViewComponentsAsync with GetPartComponentsQuery in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [ ] T115 [US3] Refactor ViewModel_Volvo_Settings: Replace ExportCsvAsync with ExportPartsCsvQuery in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [ ] T116 [US3] Refactor ViewModel_Volvo_Settings: Replace ImportCsvAsync with ImportPartsCsvCommand in Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
- [ ] T117 [US3] Mark IService_VolvoMasterData injection in ViewModel_Volvo_Settings as [Obsolete]

### XAML Binding Migration for User Story 3

- [ ] T118 [US3] Migrate View_Volvo_Settings.xaml: Convert 2 DataGridTextColumn to DataGridTemplateColumn with x:Bind (PartNumber, QuantityPerSkid) in Module_Volvo/Views/View_Volvo_Settings.xaml
- [ ] T119 [US3] Test View_Volvo_Settings: Verify all bindings work after migration, no compile errors

### Unit Tests for User Story 3

- [ ] T120 [P] [US3] Unit test GetAllVolvoPartsQueryHandler in Module_Volvo.Tests/Handlers/Queries/GetAllVolvoPartsQueryHandlerTests.cs
- [ ] T121 [P] [US3] Unit test GetPartComponentsQueryHandler in Module_Volvo.Tests/Handlers/Queries/GetPartComponentsQueryHandlerTests.cs
- [ ] T122 [P] [US3] Unit test ExportPartsCsvQueryHandler in Module_Volvo.Tests/Handlers/Queries/ExportPartsCsvQueryHandlerTests.cs
- [ ] T123 [P] [US3] Unit test AddVolvoPartCommandValidator in Module_Volvo.Tests/Validators/AddVolvoPartCommandValidatorTests.cs
- [ ] T124 [P] [US3] Unit test AddVolvoPartCommandHandler in Module_Volvo.Tests/Handlers/Commands/AddVolvoPartCommandHandlerTests.cs
- [ ] T125 [P] [US3] Unit test UpdateVolvoPartCommandValidator in Module_Volvo.Tests/Validators/UpdateVolvoPartCommandValidatorTests.cs
- [ ] T126 [P] [US3] Unit test UpdateVolvoPartCommandHandler in Module_Volvo.Tests/Handlers/Commands/UpdateVolvoPartCommandHandlerTests.cs
- [ ] T127 [P] [US3] Unit test DeactivateVolvoPartCommandValidator in Module_Volvo.Tests/Validators/DeactivateVolvoPartCommandValidatorTests.cs
- [ ] T128 [P] [US3] Unit test DeactivateVolvoPartCommandHandler in Module_Volvo.Tests/Handlers/Commands/DeactivateVolvoPartCommandHandlerTests.cs
- [ ] T129 [P] [US3] Unit test ImportPartsCsvCommandValidator in Module_Volvo.Tests/Validators/ImportPartsCsvCommandValidatorTests.cs
- [ ] T130 [P] [US3] Unit test ImportPartsCsvCommandHandler in Module_Volvo.Tests/Handlers/Commands/ImportPartsCsvCommandHandlerTests.cs

### Integration Tests for User Story 3

- [ ] T131 [US3] Integration test: Add part workflow (ViewModel → Handler → DAO → DB) in Module_Volvo.Tests/Integration/AddPartIntegrationTests.cs
- [ ] T132 [US3] Integration test: Import CSV workflow with validation errors in Module_Volvo.Tests/Integration/ImportCsvIntegrationTests.cs

**Checkpoint**: User Story 3 complete - master data management fully functional, independently testable

---

## Phase 6: User Story 4 - Email Notification & Label Preview (Priority: P3)

**Goal**: Enable users to preview email notifications and view formatted email data before completing shipments

**Independent Test**: Preview email for shipment with multiple parts and discrepancies, verify HTML/text formatting matches golden files

### Query Handlers for User Story 4

- [ ] T133 [P] [US4] Create FormatEmailDataQuery DTO in Module_Volvo/Requests/Queries/FormatEmailDataQuery.cs
- [ ] T134 [P] [US4] Create FormatEmailDataQueryHandler in Module_Volvo/Handlers/Queries/FormatEmailDataQueryHandler.cs (functional parity with legacy email format)

### ViewModel Integration for User Story 4

- [ ] T135 [US4] Refactor ViewModel_Volvo_ShipmentEntry: Replace PreviewEmailAsync with FormatEmailDataQuery in Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs

### Unit Tests for User Story 4

- [ ] T136 [P] [US4] Unit test FormatEmailDataQueryHandler in Module_Volvo.Tests/Handlers/Queries/FormatEmailDataQueryHandlerTests.cs

### Golden File Tests for User Story 4

- [ ] T137 [US4] Golden file test: FormatEmailDataQuery HTML output matches expected_email_html.html in Module_Volvo.Tests/GoldenFiles/EmailGoldenFileTests.cs
- [ ] T138 [US4] Golden file test: FormatEmailDataQuery plain text output matches expected_email_text.txt in Module_Volvo.Tests/GoldenFiles/EmailGoldenFileTests.cs

**Checkpoint**: User Story 4 complete - email preview fully functional, functional parity verified

---

## Phase 7: XAML Binding Migration (Remaining Views)

**Purpose**: Migrate remaining `{Binding}` occurrences to `x:Bind` for constitutional compliance

- [ ] T139 [P] Migrate View_Volvo_ShipmentEntry.xaml: Convert 6 DataGridTextColumn to DataGridTemplateColumn with x:Bind in DataGrid columns in Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml
- [ ] T140 Test View_Volvo_ShipmentEntry: Verify all bindings work, compile successfully
- [ ] T141 [P] Migrate VolvoShipmentEditDialog.xaml: Convert 6 DataGridTextColumn to DataGridTemplateColumn with x:Bind in Module_Volvo/Views/VolvoShipmentEditDialog.xaml
- [ ] T142 Test VolvoShipmentEditDialog: Verify all bindings work, compile successfully

**Checkpoint**: All 20 `{Binding}` occurrences migrated to `x:Bind` - Principle I compliance achieved

---

## Phase 8: Property-Based & Advanced Testing

**Purpose**: Comprehensive testing for calculation algorithms and edge cases

- [ ] T143 [P] Create VolvoArbitraries.cs: FsCheck arbitrary generators for Volvo models in Module_Volvo.Tests/PropertyBased/VolvoArbitraries.cs
- [ ] T144 [P] Create VolvoTestDataGenerator.cs: Bogus faker for Volvo models in Module_Volvo.Tests/Fixtures/VolvoTestDataGenerator.cs
- [ ] T145 Property-based test: Component explosion calculation (1000+ test cases comparing legacy vs CQRS) in Module_Volvo.Tests/PropertyBased/ComponentExplosionPropertyTests.cs
- [ ] T146 Property-based test: Piece count calculation (1000+ test cases) in Module_Volvo.Tests/PropertyBased/PieceCountCalculationPropertyTests.cs
- [ ] T147 Golden file test: Generate label CSV with discrepancies, verify matches expected_label_with_discrepancy.csv in Module_Volvo.Tests/GoldenFiles/LabelCsvGoldenFileTests.cs

**Checkpoint**: Advanced testing complete - functional parity verified with property-based tests

---

## Phase 9: Service Deprecation & Cleanup

**Purpose**: Remove legacy services after full CQRS migration

- [ ] T148 Verify all 3 ViewModels use IMediator only (grep search for IMediator presence in ViewModels)
- [ ] T149 Verify no direct service calls remain (grep search for IService_Volvo and IService_VolvoMasterData in ViewModels)
- [ ] T150 Remove [Obsolete] legacy services: Mark Service_Volvo and Service_VolvoMasterData as fully deprecated in Module_Volvo/Services/
- [ ] T151 [P] Update App.xaml.cs: Remove Service_Volvo and Service_VolvoMasterData registrations (handlers auto-registered via MediatR assembly scan)
- [ ] T152 Run all existing integration tests to verify zero regressions (dotnet test --filter "FullyQualifiedName~Module_Volvo")

**Checkpoint**: Legacy services removed - CQRS migration complete

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements, documentation, and validation

- [ ] T153 [P] Run test coverage report: Verify 80%+ coverage for handlers and validators (dotnet test --collect:"XPlat Code Coverage")
- [ ] T154 [P] Performance benchmark: Compare shipment completion time legacy vs CQRS (target: ≤ current average, <10ms MediatR overhead)
- [ ] T155 [P] Build verification: dotnet build -c Release --no-restore (target: 0 errors, 0 warnings)
- [ ] T156 [P] Update Module_Volvo/README.md: Document CQRS architecture, handler list, migration notes
- [ ] T157 [P] Create Module_Volvo/HANDLERS.md: Comprehensive handler documentation with examples
- [ ] T158 [P] Update .github/copilot-instructions.md: Add Volvo-specific CQRS examples
- [ ] T159 Validate quickstart.md: Follow step-by-step guide to ensure accuracy
- [ ] T160 Run module-compliance-auditor: Verify zero constitutional violations (@module-compliance-auditor Module_Volvo)
- [ ] T161 Final integration test: Complete end-to-end shipment workflow (entry → pending → resume → complete → history → edit)

**Checkpoint**: Module_Volvo CQRS modernization complete - all success criteria met

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational (Phase 2) completion
  - User stories CAN proceed in parallel (if staffed)
  - Recommended: Sequential in priority order (P1 → P2 → P2 → P3) for MVP delivery
- **XAML Migration (Phase 7)**: Can proceed after corresponding user story ViewModels refactored
- **Property-Based Testing (Phase 8)**: Depends on handlers being implemented (after Phase 3-6)
- **Service Deprecation (Phase 9)**: Depends on ALL user stories complete (all ViewModels refactored)
- **Polish (Phase 10)**: Depends on all previous phases complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies on other stories - START HERE
- **User Story 2 (P2)**: No dependencies on other stories - can start after Foundational or in parallel with US1
- **User Story 3 (P2)**: No dependencies on other stories - can start after Foundational or in parallel with US1/US2
- **User Story 4 (P3)**: Lightweight dependency on US1 (email preview integrated into shipment entry ViewModel)

### Within Each User Story

1. **Query/Command DTOs** (parallelizable)
2. **Validators** (must exist before command handlers to enable ValidationBehavior)
3. **Handlers** (queries parallelizable, commands depend on validators)
4. **ViewModel Refactoring** (depends on handlers)
5. **XAML Migration** (depends on ViewModel refactoring)
6. **Unit Tests** (parallelizable after handlers created)
7. **Integration/Golden File Tests** (depends on full story implementation)

### Parallel Opportunities

**Within Phase 2 (Foundational)**:
- T015-T019: NuGet packages and shared DTOs (5 tasks parallel)
- T020-T022: Golden file capture (3 tasks parallel)

**Within User Story 1**:
- T023-T030: All query DTOs and handlers (8 tasks parallel)
- T031, T034, T036, T039: All command DTOs (4 tasks parallel)
- T051-T060: All unit tests (10 tasks parallel)

**Within User Story 2**:
- T064-T071: All query DTOs and handlers (8 tasks parallel)
- T083-T088: All unit tests (6 tasks parallel)

**Within User Story 3**:
- T091-T096: All query DTOs and handlers (6 tasks parallel)
- T097, T100, T103, T106: All command DTOs (4 tasks parallel)
- T120-T130: All unit tests (11 tasks parallel)

**Within Phase 10 (Polish)**:
- T153-T158: All documentation and validation tasks (6 tasks parallel)

---

## Parallel Execution Example: User Story 1

### Sprint 1 - Query Infrastructure (Can run in parallel)
```bash
# Developer 1
git checkout -b feature/us1-init-query
# Implement T023-T024 (GetInitialShipmentDataQuery + Handler)

# Developer 2
git checkout -b feature/us1-pending-query
# Implement T025-T026 (GetPendingShipmentQuery + Handler)

# Developer 3
git checkout -b feature/us1-search-query
# Implement T027-T028 (SearchVolvoPartsQuery + Handler)

# Developer 4
git checkout -b feature/us1-csv-query
# Implement T029-T030 (GenerateLabelCsvQuery + Handler)
```

### Sprint 2 - Command Infrastructure (Sequential validators, then parallel handlers)
```bash
# All developers: Validators first (sequential - share validation patterns)
# T032, T037, T040 (AddPart, SavePending, CompleteShipment validators)

# Then parallel command handlers:
# Developer 1: T033 (AddPartToShipmentCommandHandler)
# Developer 2: T038 (SavePendingShipmentCommandHandler)
# Developer 3: T041 (CompleteShipmentCommandHandler)
```

### Sprint 3 - ViewModel Refactoring + Tests (Parallel tests after ViewModel done)
```bash
# Developer 1: T042-T050 (ViewModel refactoring - sequential)
# Developer 2-4: T051-T060 (Unit tests - parallel while Developer 1 works on ViewModel)
```

---

## Implementation Strategy

### MVP Delivery (Minimum Viable Product)
- **Phase 1-3 ONLY**: Setup + Foundational + User Story 1
- **Estimated Time**: 20-25 hours
- **Value**: Core shipment entry workflow functional with CQRS
- **Deliverable**: Users can create/complete shipments using modernized architecture

### Incremental Delivery
- **Phase 4**: Add history viewing/editing (P2)
- **Phase 5**: Add master data management (P2)
- **Phase 6**: Add email preview (P3)
- **Each phase independently testable and deployable**

### Full Modernization
- **All Phases**: Complete constitutional compliance
- **Estimated Time**: 50-60 hours
- **Value**: All 3 constitutional violations resolved, 80%+ test coverage, functional parity verified

---

## Success Criteria Validation

| Criterion | Validation Task | Target | Phase |
|-----------|----------------|--------|-------|
| SC-001: CQRS Migration | T148-T149 (grep search) | 100% IMediator usage | Phase 9 |
| SC-002: Handler Implementation | T023-T141 | 19 handlers created | Phases 3-6 |
| SC-003: Validation Coverage | T032-T107 | 8 validators (1:1 mapping) | Phases 3-6 |
| SC-004: Test Coverage | T153 (coverage report) | 80%+ handlers/validators | Phase 10 |
| SC-005: CSV Parity | T061, T147 (golden files) | Byte-for-byte match | Phases 3, 8 |
| SC-006: Email Parity | T137-T138 (golden files) | String exact match | Phase 6 |
| SC-007: Calculation Parity | T145-T146 (property tests) | 1000+ cases pass | Phase 8 |
| SC-008: Constitutional Compliance | T160 (compliance audit) | Zero violations | Phase 10 |
| SC-009: Zero Regressions | T152 (integration tests) | All tests pass | Phase 9 |
| SC-010: Build Success | T155 (Release build) | 0 errors, 0 warnings | Phase 10 |
| SC-011: Performance Parity | T154 (benchmark) | ≤ current average | Phase 10 |
| SC-012: Database Integrity | T152 (integration tests) | Stored proc contracts maintained | Phase 9 |

---

## Task Statistics

**Total Tasks**: 161

**By Phase**:
- Phase 1 (Setup): 11 tasks
- Phase 2 (Foundational): 11 tasks
- Phase 3 (US1 - P1): 41 tasks
- Phase 4 (US2 - P2): 27 tasks
- Phase 5 (US3 - P2): 42 tasks
- Phase 6 (US4 - P3): 6 tasks
- Phase 7 (XAML Migration): 4 tasks
- Phase 8 (Advanced Testing): 5 tasks
- Phase 9 (Service Deprecation): 5 tasks
- Phase 10 (Polish): 9 tasks

**By Category**:
- Query Handlers: 11 tasks
- Command Handlers: 8 tasks
- Validators: 8 tasks
- ViewModel Refactoring: 23 tasks
- XAML Migration: 6 tasks
- Unit Tests: 51 tasks
- Integration/Golden File Tests: 15 tasks
- Property-Based Tests: 4 tasks
- Infrastructure: 22 tasks
- Documentation/Validation: 13 tasks

**Parallelizable Tasks**: 89 tasks (marked with [P])
**Sequential Tasks**: 72 tasks

**Estimated Effort**:
- MVP (Phases 1-3): 20-25 hours
- Full Modernization (All Phases): 50-60 hours

---

**Tasks Status**: ✅ READY FOR IMPLEMENTATION

**Next Step**: Begin Phase 1 (Setup) - create folder structure

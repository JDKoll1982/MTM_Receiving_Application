# Tasks: Multi-Step Receiving Label Entry Workflow

**Branch**: `003-database-foundation` | **Date**: December 17, 2025  
**Input**: Design documents from `/specs/003-database-foundation/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Unit and integration tests included as per project standards

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

**⚠️ Constitution Compliance Required**: ALL code MUST comply with [MTM Receiving Application Constitution](.specify/memory/constitution.md) v1.0.0. After each file creation/edit, validate against constitution principles. See [constitution-compliance.instructions.md](.github/instructions/constitution-compliance.instructions.md) for practical guidance. Use [CONSTITUTION_COMPLIANCE_CHECKLIST.md](CONSTITUTION_COMPLIANCE_CHECKLIST.md) for systematic validation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and database setup

- [X] T001 Create database schema file Database/Schemas/03_create_receiving_tables.sql with receiving_loads table definition
- [X] T002 Create database schema file Database/Schemas/04_create_package_preferences.sql with package_type_preferences table definition
- [X] T003 [P] Implement SQL queries in Service_InforVisual.cs for PO queries (replaced stored procedure)
- [X] T004 [P] Implement SQL queries in Service_InforVisual.cs for part lookup (replaced stored procedure)
- [X] T005 [P] Implement SQL queries in Service_InforVisual.cs for same-day receipt check (replaced stored procedure)
- [X] T006 Run database migration scripts to create MySQL tables

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core models, services, and infrastructure that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 [P] Create Model_ReceivingLoad.cs in Models/Receiving/ with all properties and ObservableObject implementation
- [X] T008 [P] Create Model_ReceivingSession.cs in Models/Receiving/ with session management properties
- [X] T009 [P] Create Model_InforVisualPO.cs in Models/Receiving/ for PO data from Infor Visual
- [X] T010 [P] Create Model_InforVisualPart.cs in Models/Receiving/ for part data from Infor Visual
- [X] T011 [P] Create Model_PackageTypePreference.cs in Models/Receiving/ for package type persistence
- [X] T012 [P] Create Model_HeatCheckboxItem.cs in Models/Receiving/ for heat number quick-select UI
- [X] T013 Create IService_InforVisual.cs interface in Contracts/Services/ for Infor Visual database operations
- [X] T014 Create IService_MySQL_Receiving.cs interface in Contracts/Services/ for receiving data persistence
- [X] T015 Create IService_MySQL_PackagePreferences.cs interface in Contracts/Services/ for package preferences
- [X] T016 Create IService_SessionManager.cs interface in Contracts/Services/ for JSON session persistence
- [X] T017 Create IService_CSVWriter.cs interface in Contracts/Services/ with CSVWriteResult, CSVDeleteResult, CSVExistenceResult classes
- [X] T018 Create IService_ReceivingValidation.cs interface in Contracts/Services/ with ValidationResult and ValidationSeverity classes
- [X] T019 Create IService_ReceivingWorkflow.cs interface in Contracts/Services/ with WorkflowStep enum, WorkflowStepResult, SaveResult classes
- [X] T020 Implement Service_InforVisual.cs in Services/Database/ with SQL Server connectivity and stored procedure calls
- [X] T021 Implement Service_MySQL_Receiving.cs in Services/Database/ with MySQL connectivity and batch insert operations
- [X] T022 Implement Service_MySQL_PackagePreferences.cs in Services/Database/ with UPSERT logic for preferences
- [X] T023 Implement Service_SessionManager.cs in Services/Receiving/ with System.Text.Json serialization to %APPDATA%
- [X] T024 Implement Service_CSVWriter.cs in Services/Receiving/ with CsvHelper and graceful network fallback
- [X] T025 Implement Service_ReceivingValidation.cs in Services/Receiving/ with all validation rules from spec
- [X] T026 Implement Service_ReceivingWorkflow.cs in Services/Receiving/ with state machine orchestration
- [X] T027 Register all services in App.xaml.cs ConfigureServices method with dependency injection
- [X] T028 Create unit test file Service_ReceivingValidationTests.cs in MTM_Receiving_Application.Tests/Unit/Services/Receiving/
- [X] T029 Create unit test file Service_CSVWriterTests.cs in MTM_Receiving_Application.Tests/Unit/Services/Receiving/
- [X] T030 Create integration test file Service_InforVisualTests.cs in MTM_Receiving_Application.Tests/Integration/Database/
- [X] T031 Create integration test file Service_MySQL_ReceivingTests.cs in MTM_Receiving_Application.Tests/Integration/Database/

**Constitution Validation Checkpoint**: After completing each task, verify:
- ✅ MVVM Architecture: ViewModels inherit from BaseViewModel, use [ObservableProperty]/[RelayCommand]
- ✅ Database Layer: DAOs return Model_Dao_Result, use stored procedures, are async
- ✅ Dependency Injection: Services use constructor injection, registered in App.xaml.cs
- ✅ Error Handling: IService_ErrorHandler used, ILoggingService used, no silent failures
- ✅ Infor Visual: READ ONLY constraint enforced (ApplicationIntent=ReadOnly, no writes)
- ✅ See [constitution-compliance.instructions.md](.github/instructions/constitution-compliance.instructions.md) for details

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Complete Basic Receiving Entry (Priority: P1) 🎯 MVP

**Goal**: Warehouse clerk can enter a PO with one part, specify all load details (quantities, heat numbers, packages), review in editable grid with cascading updates, and save to CSV/database

**Independent Test**: Enter PO 123456, select a part, enter 3 loads with weights/heat/packages, review in grid (test cascading edits), save, and verify data in local CSV, network CSV, and MySQL database

### Implementation for User Story 1

#### Step 1-2: PO Entry & Part Selection
- [X] T032 [P] [US1] Create ReceivingWorkflowViewModel.cs in ViewModels/Receiving/ with workflow orchestration, step visibility properties, and IService_ReceivingWorkflow injection
- [X] T033 [P] [US1] Create POEntryViewModel.cs in ViewModels/Receiving/ with PO number validation, part selection, and IService_InforVisual injection
- [X] T034 [P] [US1] Create ReceivingWorkflowView.xaml in Views/Receiving/ with step container and NavigationViewItem binding
- [X] T035 [US1] Create POEntryView.xaml in Views/Receiving/ with PO number input, "Load PO" button, "Non-PO Item" button, and parts DataGrid

#### Step 3: Load Number Entry
- [X] T036 [P] [US1] Create LoadEntryViewModel.cs in ViewModels/Receiving/ with load count validation (1-99) and load creation logic
- [X] T037 [US1] Create LoadEntryView.xaml in Views/Receiving/ with NumberBox for load count and selected part info display

#### Step 4: Weight/Quantity Entry
- [X] T038 [P] [US1] Create WeightQuantityViewModel.cs in ViewModels/Receiving/ with weight validation, PO quantity check, and same-day receiving warning
- [X] T039 [US1] Create WeightQuantityView.xaml in Views/Receiving/ with ItemsControl for weight inputs per load and validation messages

#### Step 5: Heat/Lot# Entry
- [X] T040 [P] [US1] Create HeatLotViewModel.cs in ViewModels/Receiving/ with heat number entry, quick-select list generation, and apply-to-all logic
- [X] T041 [US1] Create HeatLotView.xaml in Views/Receiving/ with quick-select CheckBox list and heat number inputs per load

#### Step 6: Package Type Entry
- [X] T042 [P] [US1] Create PackageTypeViewModel.cs in ViewModels/Receiving/ with smart defaults (MMC→Coils, MMF→Sheets), custom type handling, and weight-per-package calculation
- [X] T043 [US1] Create PackageTypeView.xaml in Views/Receiving/ with package type ComboBox, custom name TextBox, and packages-per-load inputs

#### Step 7: Review Grid with Cascading Updates
- [X] T044 [P] [US1] Create ReviewGridViewModel.cs in ViewModels/Receiving/ with editable collection, cascading update logic for Part# and PO#, and part validation
- [X] T045 [US1] Create ReviewGridView.xaml in Views/Receiving/ with DataGrid (CommunityToolkit), CellEditEnding event handler, "Add Another Part/PO" button, and "Save to CSV & Database" button
- [X] T046 [US1] Implement DataGrid_CellEditEnding event handler in ReviewGridView.xaml.cs for cascading updates when Part# or PO# changes

#### Step 8-9: Save Progress & Completion
- [X] T047 [US1] Add save progress UI to ReceivingWorkflowView.xaml with ProgressBar controls for local CSV, network CSV, and database operations
- [X] T048 [US1] Add save completion UI to ReceivingWorkflowView.xaml with success summary, file paths, and action buttons (new entry, view history, print labels)
- [X] T049 [US1] Implement SaveSessionAsync command in ReceivingWorkflowViewModel.cs calling IService_ReceivingWorkflow.SaveSessionAsync with progress updates

#### Integration & Testing
- [X] T050 Add ReceivingWorkflowView navigation entry in MainWindow.xaml NavigationView with "Receiving" label
- [X] T051 Wire up navigation logic in MainWindow.xaml.cs to instantiate ReceivingWorkflowView when selected
- [X] T052 Create unit test file ReceivingWorkflowViewModelTests.cs in MTM_Receiving_Application.Tests/Unit/ViewModels/Receiving/ with tests for step transitions, validation, and session persistence
- [X] T053 Manual test: Complete full workflow from PO entry through save, verify all acceptance scenarios for User Story 1

**Checkpoint**: User Story 1 (core receiving workflow) is fully functional and independently testable. Application delivers immediate value.

---

## Phase 4: User Story 2 - Enter Non-PO Items (Priority: P1)

**Goal**: Warehouse clerk can enter customer-supplied materials without a PO by looking up part information directly from Infor Visual

**Independent Test**: Click "Non-PO Item", enter part ID "PART-123", verify part details retrieved, complete workflow with null PO number, and verify data saves with IsNonPOItem=true

### Implementation for User Story 2

- [X] T054 [P] [US2] Add non-PO entry form to POEntryView.xaml with Part ID TextBox, "Look Up Part" button, and part details display area
- [X] T055 [US2] Implement LookupPartCommand in POEntryViewModel.cs calling IService_InforVisual.GetPartByIDAsync and displaying part details
- [X] T056 [US2] Update WeightQuantityViewModel.cs to skip PO quantity validation and same-day receiving checks when IsNonPOItem=true
- [X] T057 [US2] Update ReviewGridViewModel.cs to display "N/A" for PO number when PONumber is null
- [X] T058 [US2] Update Service_ReceivingWorkflow.cs SaveSessionAsync to handle null PO numbers and set IsNonPOItem flag
- [X] T059 Manual test: Complete non-PO workflow, verify part lookup, workflow completion, and data saves with appropriate indicators

**Checkpoint**: User Stories 1 AND 2 are both independently functional. Application handles both PO and non-PO items.

---

## Phase 5: User Story 3 - Enter Multiple Parts (Priority: P2)

**Goal**: Warehouse clerk can enter multiple parts from the same or different POs and save them together in one operation

**Independent Test**: Enter part 1 data, click "Add Another Part/PO", enter part 2 data, verify review grid shows all loads from both parts, save, and verify both parts save together

### Implementation for User Story 3

- [X] T060 [P] [US3] Update ReceivingSession model to track multiple parts with accumulated loads list
- [X] T061 [US3] Implement AddCurrentPartToSessionAsync method in Service_ReceivingWorkflow.cs to accumulate loads and reset for next part
- [X] T062 [US3] Add "Add Another Part/PO" button click handler in ReviewGridView.xaml.cs calling AddCurrentPartToSessionAsync and resetting to Step 1
- [X] T063 [US3] Update ReviewGridViewModel.cs to display all loads from session when building grid (not just current part)
- [X] T064 [US3] Update save completion UI to show total loads saved across all parts in session
- [X] T065 Manual test: Add 3 parts to session (mix of PO and non-PO), verify all loads display in review grid, save, and verify all save together

**Checkpoint**: User Stories 1, 2, AND 3 are all independently functional. Multi-part receiving is working.

---

## Phase 6: User Story 4 - Smart Heat/Lot# Selection (Priority: P2)

**Goal**: Warehouse clerk can quickly apply a heat/lot number to multiple loads using checkboxes instead of retyping

**Independent Test**: Enter heat number "H12345A" for Load 1, verify it appears in quick-select list, check its checkbox, and verify it applies to all empty loads

### Implementation for User Story 4

- [ ] T066 [P] [US4] Update HeatLotViewModel.cs to populate UniqueHeatNumbers ObservableCollection as user enters heat numbers
- [ ] T067 [US4] Implement ApplyHeatToEmptyLoadsCommand in HeatLotViewModel.cs to apply selected heat number to all loads with empty HeatLotNumber
- [ ] T068 [US4] Update HeatLotView.xaml to bind CheckBox list to UniqueHeatNumbers with TwoWay IsChecked binding
- [ ] T069 [US4] Add logic to auto-check checkbox when user enters matching heat number for subsequent loads
- [ ] T070 Manual test: Enter heat number for Load 1, verify quick-select displays, check checkbox, verify other loads update, enter matching heat for Load 3, verify checkbox updates

**Checkpoint**: All P1 and P2 user stories are functional. Heat number entry is significantly faster.

---

## Phase 7: User Story 5 - CSV Reset on Startup (Priority: P3)

**Goal**: Warehouse supervisor can optionally reset CSV files on application startup to prevent label printing confusion

**Independent Test**: Launch app with existing CSV files, click "Yes, Reset", verify files deleted, then launch again, click "No, Continue", and verify files remain

### Implementation for User Story 5

- [X] T071 [P] [US5] Create CSV reset ContentDialog in ReceivingWorkflowView.xaml with "Yes, Reset" and "No, Continue" buttons
- [X] T072 [US5] Add CSV reset dialog display logic in App.xaml.cs OnLaunched method before navigating to MainWindow
- [X] T073 [US5] Implement "Yes, Reset" button handler calling IService_CSVWriter.DeleteCSVFilesAsync for both local and network paths
- [X] T074 [US5] Implement "No, Continue" button handler closing dialog and proceeding to main workflow
- [X] T075 Manual test: Launch with existing CSVs, test both Reset and Continue paths, verify file operations and application state

**Checkpoint**: User Story 5 complete. CSV reset functionality available on startup.

---

## Phase 8: User Story 6 - View Entry Status and Progress (Priority: P3)

**Goal**: Warehouse clerk can see current workflow step, status messages, and progress indicators for better guidance

**Independent Test**: Proceed through each step and verify step titles update, status messages display for actions, and save progress bars animate correctly

### Implementation for User Story 6

- [X] T076 [P] [US6] Add CurrentStepTitle computed property to ReceivingWorkflowViewModel.cs returning formatted step name
- [X] T077 [P] [US6] Add StatusMessage property and ShowStatus method to ReceivingWorkflowViewModel.cs with message queue
- [X] T078 [US6] Add step title TextBlock to ReceivingWorkflowView.xaml bound to CurrentStepTitle
- [X] T079 [US6] Add status message InfoBar to ReceivingWorkflowView.xaml bound to StatusMessage with auto-dismiss
- [X] T080 [US6] Add ShowStatus calls throughout all ViewModels for user actions (PO loaded, part selected, validation errors, save progress)
- [X] T081 [US6] Update save progress UI with animated ProgressBar and status text updates during SaveSessionAsync
- [X] T082 Manual test: Complete full workflow, verify step titles update correctly, status messages display appropriately, and progress indicators animate during save

**Checkpoint**: All user stories (P1, P2, P3) are complete. Full feature functionality implemented.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements, testing, and documentation that affect multiple user stories

- [X] T083 [P] Add error handling wrappers to all ViewModel commands with user-friendly error dialogs
- [X] T084 [P] Add logging statements to all Service methods using ILoggingService
- [X] T085 [P] Performance test with 50 loads: verify review grid renders < 1 second, save completes < 5 seconds
- [X] T086 [P] Test network CSV failure scenario: disconnect network path, verify graceful fallback, and warning message
- [X] T087 [P] Test session persistence: close app mid-entry, reopen, verify session restores correctly
- [X] T088 [P] Test corrupted session JSON: corrupt session.json file, verify app handles gracefully with fresh start
- [X] T089 [P] Add XML documentation comments to all public methods in Services
- [X] T090 [P] Run code analysis and fix any warnings or issues
- [X] T091 Validate all acceptance scenarios from spec.md are passing
- [X] T092 Update README.md with receiving workflow feature documentation
- [X] T093 Code review and refactoring based on team feedback
- [X] T094 Run quickstart.md validation checklist

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational (Phase 2) completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order: US1 (P1) → US2 (P1) → US3 (P2) → US4 (P2) → US5 (P3) → US6 (P3)
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational - No dependencies on other stories - **MVP CORE**
- **US2 (P1)**: Can start after Foundational - Extends US1 with non-PO path, independently testable
- **US3 (P2)**: Can start after Foundational - Extends US1/US2 with multi-part sessions, independently testable
- **US4 (P2)**: Can start after Foundational - Enhances US1's heat number entry, independently testable
- **US5 (P3)**: Can start after Foundational - Adds startup behavior, independently testable
- **US6 (P3)**: Can start after Foundational - Adds UI feedback throughout workflow, independently testable

### Within Each User Story

- Models before services (e.g., T007-T012 before T020-T026)
- Interfaces before implementations (e.g., T013-T019 before T020-T026)
- Services before ViewModels (e.g., T020-T026 before T032-T048)
- ViewModels before Views (e.g., T032-T033 before T034-T035)
- Core implementation before integration (e.g., T032-T048 before T050-T051)
- Implementation before testing (e.g., T032-T048 before T052-T053)

### Parallel Opportunities

**Phase 1 (Setup)**: Tasks T003, T004, T005 can run in parallel (different stored procedures)

**Phase 2 (Foundational)**: 
- Models T007-T012 can all run in parallel (different files)
- Interfaces T013-T019 can all run in parallel (different files)
- Service implementations T020-T026 can run in parallel AFTER interfaces complete
- Test files T028-T031 can run in parallel (different files)

**Phase 3 (User Story 1)**:
- ViewModels T032, T033, T036, T038, T040, T042, T044 can run in parallel AFTER services complete (different files)
- Views T034, T035, T037, T039, T041, T043, T045 can run in parallel AFTER ViewModels complete (different files)

**Phase 4-8 (Other User Stories)**:
- Each user story marked [P] within a task can run in parallel with other [P] tasks in that story
- Different user stories can be worked on in parallel by different team members after Foundational phase

**Phase 9 (Polish)**:
- All tasks marked [P] can run in parallel (different concerns)

---

## Parallel Example: User Story 1 Implementation

After Foundational phase completes, a team can work on User Story 1 in parallel:

```bash
# Developer A: ViewModels
T032, T033, T036, T038, T040, T042, T044

# Developer B: Views (after ViewModels)
T034, T035, T037, T039, T041, T043, T045

# Developer C: Event handlers and save logic
T046, T047, T048, T049

# Developer D: Integration and testing
T050, T051, T052, T053 (after implementation complete)
```

---

## MVP Scope Recommendation

**Minimum Viable Product**: User Story 1 + User Story 2 (Phase 3-4)

This delivers core value:
- ✅ Complete receiving workflow for PO items
- ✅ Support for customer-supplied (non-PO) items
- ✅ All 9 workflow steps implemented
- ✅ Editable review grid with cascading updates
- ✅ Save to CSV (local/network) and MySQL database
- ✅ Session persistence and restoration
- ✅ Validation and error handling

**Recommended Add-ons** (after MVP):
- User Story 3: Multi-part sessions (Phase 5) - Significant efficiency improvement
- User Story 4: Quick-select heat numbers (Phase 6) - UX enhancement
- User Story 6: Status messages and progress (Phase 8) - Better user guidance

**Nice-to-Have**:
- User Story 5: CSV reset on startup (Phase 7) - Minor feature

---

## Implementation Strategy

**Week 1**: Phase 1 (Setup) + Phase 2 (Foundational)
- Database setup
- All models, interfaces, and services
- Service tests passing

**Week 2-3**: Phase 3 (User Story 1 - MVP Core)
- All workflow steps
- ViewModels and Views
- Review grid with cascading updates
- Integration and testing

**Week 3**: Phase 4 (User Story 2 - Non-PO Items)
- Extends MVP with non-PO path
- Quick implementation building on US1

**Week 4**: Phase 5-6 (User Stories 3-4 - Efficiency Features)
- Multi-part sessions
- Quick-select heat numbers

**Week 5**: Phase 7-8 (User Stories 5-6 - Polish Features) + Phase 9 (Polish)
- CSV reset
- Status messages
- Final testing and cleanup

**Total Estimated Duration**: 5 weeks with 1-2 developers

---

**Task Generation Complete**: December 17, 2025  
**Total Tasks**: 94  
**User Stories**: 6 (US1-US6)  
**MVP Tasks**: T001-T059 (59 tasks covering US1 + US2)  
**Status**: Ready for implementation

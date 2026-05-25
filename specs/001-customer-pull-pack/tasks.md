# Tasks: Customer Pull n' Pack Tool

**Input**: Design documents from `/specs/001-customer-pull-pack/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md, contracts/openapi.yaml

**Tests**: Include unit and persistence/integration coverage because the specification and plan explicitly require independent story validation plus DAO coverage where persistence is involved.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the feature scaffolding and expose the new tool in the Ship/Rec shell.

- [x] T001 Create feature folders at `Module_ShipRec_Tools/Data/CustomerPullPack/`, `Module_ShipRec_Tools/Services/CustomerPullPack/`, `Database/InforVisualScripts/Queries/CustomerPullPack/`, `Database/Database_Deployment/Sql_Files/StoredProcedures/ShipRecTools/CustomerPullPack/`, and `MTM_Receiving_Application.Tests/Integration/Module_ShipRec_Tools/`
- [x] T002 Add the Customer Pull n' Pack tool definition in `Module_ShipRec_Tools/Models/Model_ToolDefinition.cs`
- [x] T003 [P] Add Customer Pull n' Pack tool selection wiring in `Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_ToolSelection.cs`
- [x] T004 [P] Add Customer Pull n' Pack navigation routing in `Module_ShipRec_Tools/Services/Service_ShipRecTools_Navigation.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared models, data access, persistence scripts, and DI registrations that all stories depend on.

**⚠️ CRITICAL**: No user story work should begin until this phase is complete.

- [x] T005 Create the shared report filter model in `Module_ShipRec_Tools/Models/Model_CustomerPullPack_DemandFilter.cs`
- [x] T006 [P] Create the shared demand and location models in `Module_ShipRec_Tools/Models/Model_CustomerPullPack_DemandLine.cs` and `Module_ShipRec_Tools/Models/Model_CustomerPullPack_LocationOption.cs`
- [x] T007 [P] Create the shared waitlist model in `Module_ShipRec_Tools/Models/Model_CustomerPullPack_WaitlistEntry.cs`
- [x] T008 [P] Create the shared defaults and print models in `Module_ShipRec_Tools/Models/Model_CustomerPullPack_UserDefaults.cs` and `Module_ShipRec_Tools/Models/Model_CustomerPullPack_PrintContext.cs`
- [x] T009 [P] Author the read-only Visual demand query in `Database/InforVisualScripts/Queries/CustomerPullPack/01_GetCustomerPullPackDemand.sql`
- [x] T009A [P] Extend the shared Infor Visual mock catalog model and base JSON seed data for Customer Pull n' Pack demand, location, and linked-waitlist scenarios in `Module_Core/Models/InforVisual/Model_InforVisualMockDataCatalog.cs` and `Module_Settings.Core/Defaults/inforvisual-mock-data.json`
- [x] T009B [P] Add Customer Pull n' Pack mock catalog accessors and runtime merge support in `Module_Core/Contracts/Services/IService_InforVisualMockDataCatalog.cs` and `Module_Core/Services/Database/Service_InforVisualMockDataCatalog.cs`
- [x] T010 [P] Author the MySQL waitlist and defaults stored procedures in `Database/Database_Deployment/Sql_Files/StoredProcedures/ShipRecTools/CustomerPullPack/sp_CustomerPullPack_Waitlist_Upsert.sql`, `Database/Database_Deployment/Sql_Files/StoredProcedures/ShipRecTools/CustomerPullPack/sp_CustomerPullPack_Waitlist_GetQueue.sql`, and `Database/Database_Deployment/Sql_Files/StoredProcedures/ShipRecTools/CustomerPullPack/sp_CustomerPullPack_UserDefaults_Upsert.sql`
- [x] T011 Implement the read-only demand DAO with live-query loading and mock-catalog fallback support in `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackDemand.cs`
- [x] T012 [P] Implement the waitlist persistence DAO in `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackWaitlist.cs`
- [x] T013 [P] Implement the defaults persistence DAO in `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackUserDefaults.cs`
- [x] T014 Register Customer Pull n' Pack DAOs, ViewModels, dialogs, and views in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

**Checkpoint**: Foundation ready. User story work can now proceed.

---

## Phase 3: User Story 1 - Review Daily Customer Demand (Priority: P1) 🎯 MVP

**Goal**: Deliver the one-customer report view with filters, shortage cues, no-demand state, linked waitlist indicators, and the reviewed crystal-style grouped report surface.

**Independent Test**: Select a customer with open demand, apply filters and sorting, and confirm the user can identify actionable lines and shortage conditions without invoking waitlist actions.

### Tests for User Story 1

- [x] T015 [P] [US1] Add report query handler tests in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackReportHandlerTests.cs`
- [x] T015A [P] [US1] Add mock-mode report tests that run without a live Infor Visual server by using Customer Pull n' Pack seed data from the shared mock catalog in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackReportHandlerTests.cs`
- [x] T016 [P] [US1] Add report viewmodel tests in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs`

### Implementation for User Story 1

- [x] T017 [P] [US1] Create the report query contract in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReport.cs`
- [x] T018 [P] [US1] Implement the report query handler in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReportHandler.cs`
- [x] T019 [US1] Implement the report page state and filtering logic in `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- [x] T020 [US1] Create the report page UI with customer scope, filters, shortage cues, linked waitlist indicators, and main-window shared status presentation in `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml` and `MainWindow.xaml`
- [x] T020A [US1] Align the report host UX with the reviewed layout: collapsed filter expander, header-level report actions, embedded crystal-style report surface, and Customer Pull n' Pack window resize/restore behavior in `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml`, `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`, and `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml`
- [ ] T020B [US1] Replace the temporary crystal-style sample groups in `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml.cs` with a live grouped presentation model derived from `ViewModel_Tool_CustomerPullPackReport.cs`
- [ ] T020C [US1] Bind the embedded crystal-style request-line rows and `SUB PARTS ON HAND` rows to the actual report-side selection state used by waitlist creation instead of preview-only control-local selection state in `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml`, `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml.cs`, and `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- [x] T021 [US1] Wire report page launch and host state in `Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_Main.cs`

- [ ] T021a [MISC] For any thourough data anasis you needed to perform in this user story, if it makes sence to do so create new serena memories on what you found so this analasis does not need to happen again in the future.

**Checkpoint**: User Story 1 is independently functional and demoable as the MVP.

---

## Phase 4: User Story 2 - Create Waitlist Requests Without Typing Locations (Priority: P1)

**Goal**: Let requesters select report lines and report-side locations, create/update one waitlist entry per selected line, reuse saved locations for updates, and block duplicate open requests.

**Independent Test**: Select compatible lines and `SUB PARTS ON HAND` rows on the main display, save the request, and confirm one linked waitlist record is created or updated per selected source line while duplicate open requests are blocked with a clear redirect path.

### Tests for User Story 2

- [x] T022 [P] [US2] Add batch upsert command handler tests in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Command_CustomerPullPackBatchUpsertHandlerTests.cs`
- [x] T023 [P] [US2] Add waitlist DAO integration tests in `MTM_Receiving_Application.Tests/Integration/Module_ShipRec_Tools/Dao_CustomerPullPackWaitlistIntegrationTests.cs`
- [x] T024 [P] [US2] Add waitlist editor viewmodel tests in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Dialog_CustomerPullPackWaitlistEditorTests.cs`
- [x] T024A [P] [US2] Add post-acceptance requester edit permission tests covering requester-facing note edits, handler-field locks, and owner preservation in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Dialog_CustomerPullPackWaitlistEditorTests.cs` and `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Command_CustomerPullPackBatchUpsertHandlerTests.cs`

### Implementation for User Story 2

- [x] T025 [P] [US2] Create the batch upsert command and validator in `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackBatchUpsert.cs`
- [x] T026 [P] [US2] Implement the batch upsert handler in `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackBatchUpsertHandler.cs`
- [x] T027 [P] [US2] Create the linked waitlist lookup query in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackLinkedWaitlist.cs`
- [x] T028 [P] [US2] Implement the linked waitlist lookup handler in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackLinkedWaitlistHandler.cs`
- [x] T029 [US2] Implement report-side line selection, unselected-by-default location behavior, and duplicate-request redirect logic in `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- [x] T030 [US2] Create the waitlist editor dialog UI in `Module_ShipRec_Tools/Dialogs/Dialog_CustomerPullPackWaitlistEditor.xaml`
- [x] T031 [US2] Implement the waitlist editor dialog ViewModel with requester-facing note editing after acceptance and handler-owned field lock states in `Module_ShipRec_Tools/ViewModels/ViewModel_Dialog_CustomerPullPackWaitlistEditor.cs`
- [x] T031A [US2] Enforce post-acceptance requester edit boundaries and current-owner preservation in `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackBatchUpsertHandler.cs` and `Module_ShipRec_Tools/ViewModels/ViewModel_Dialog_CustomerPullPackWaitlistEditor.cs`
- [x] T032 [US2] Add duplicate-warning and existing-item reopen UX surfaced through the main-window shared info bar in `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`, `MainWindow.xaml`, and `Module_Core/Services/Service_Notification.cs`

**Checkpoint**: User Stories 1 and 2 both work, and request creation is independently testable from the report surface.

---

## Phase 5: User Story 3 - Work The Dedicated Waitlist Queue (Priority: P2)

**Goal**: Deliver the dedicated queue with status transitions, Problem reason enforcement, explicit ownership display, retained ownership on Problem, and unassign support.

**Independent Test**: Open the queue with existing Requested, Accepted, and Problem work, update one selected item through status changes, confirm Problem reason validation, and verify owner retention and explicit unassign behavior.

### Tests for User Story 3

- [ ] T033 [P] [US3] Add queue query handler tests in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackWaitlistQueueHandlerTests.cs`
- [ ] T034 [P] [US3] Add queue status transition tests in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Command_CustomerPullPackUpdateStatusHandlerTests.cs`
- [ ] T035 [P] [US3] Add queue ownership and Problem-state tests in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueueTests.cs`
- [ ] T035A [P] [US3] Add completed-line recheck indicator tests in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackReportHandlerTests.cs` and `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs`

### Implementation for User Story 3

- [ ] T036 [P] [US3] Create the queue query contract in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackWaitlistQueue.cs`
- [ ] T037 [P] [US3] Implement the queue query handler in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackWaitlistQueueHandler.cs`
- [ ] T038 [P] [US3] Create the status update command and validator in `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackUpdateStatus.cs`
- [ ] T039 [P] [US3] Create the explicit unassign command in `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackUnassignOwner.cs`
- [ ] T040 [US3] Implement the status update and unassign handlers in `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackUpdateStatusHandler.cs` and `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackUnassignOwnerHandler.cs`
- [ ] T040A [US3] Implement requester-facing recheck indicator projection for completed work whose source demand or location context changes in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReportHandler.cs` and `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- [ ] T041 [US3] Implement the queue page ViewModel in `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueue.cs`
- [ ] T042 [US3] Create the queue page UI in `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackQueue.xaml`
- [ ] T043 [US3] Add Problem reason selection, retained-owner display, and explicit unassign UX in `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackQueue.xaml`

**Checkpoint**: User Stories 1, 2, and 3 are independently testable, including queue execution workflows.

---

## Phase 6: User Story 4 - Print Operational Views And Resume With Saved Defaults (Priority: P3)

**Goal**: Restore saved defaults on reopen, recall favorite customers and saved filter presets, and generate current-view, floor-copy, shortage-only, waitlist-only, pull-list, and selected part/customer-order outputs from the active report or queue context.

**Independent Test**: Save defaults including favorite customers, reopen the tool, confirm the last good customer/date-range context and saved filters are restored, then generate current-view, shortage-only, waitlist-only, floor-copy, pull-list, and selected part/customer-order output with the correct filters and chosen locations.

### Tests for User Story 4

- [ ] T044 [P] [US4] Add defaults persistence integration tests covering favorite customers, saved sort and filter state, print presets, and waitlist default status filters in `MTM_Receiving_Application.Tests/Integration/Module_ShipRec_Tools/Dao_CustomerPullPackUserDefaultsIntegrationTests.cs`
- [ ] T045 [P] [US4] Add print context query tests for current-view, floor-copy, shortage-only, waitlist-only, pull-list, and selected part/customer-order outputs in `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackPrintContextHandlerTests.cs`

### Implementation for User Story 4

- [ ] T046 [P] [US4] Create the defaults query and save command for favorite customers, customer/date-range behavior, sort/filter state, print preset, and waitlist default status filter in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackDefaults.cs` and `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackSaveDefaults.cs`
- [ ] T047 [P] [US4] Create the print context query in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackPrintContext.cs`
- [ ] T048 [US4] Implement the defaults and print handlers in `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackPrintContextHandler.cs` and `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackSaveDefaultsHandler.cs`
- [ ] T049 [US4] Implement defaults restoration, favorite-customer recall, and print mode actions in `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs` and `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueue.cs`
- [ ] T050 [US4] Create the defaults view and favorite-customer management UI in `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackDefaults.xaml`
- [ ] T051 [US4] Create the floor-copy and pull-list views in `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackFloorCopy.xaml` and `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackPullList.xaml`
- [ ] T051A [US4] Add print mode selection UX for current-view, shortage-only, waitlist-only, and selected part/customer-order contexts in `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml` and `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackQueue.xaml`

**Checkpoint**: All four user stories are independently functional and testable.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Finalize cross-story documentation, metadata, and validation.

- [ ] T052 [P] Update implementation-facing feature notes in `Module_ShipRec_Tools/docs/` to reflect the delivered Customer Pull n' Pack workflow
- [ ] T053 [P] Update Ship/Rec CopilotForms metadata in `docs/development/CopilotForms/data/module-metadata/Module_ShipRec_Tools/shiprec-tools-catalog.json`
- [ ] T054 Run the end-to-end validation checklist in `specs/001-customer-pull-pack/quickstart.md`, including timed checks for the 30-second load target and 60-second print target

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup**: No dependencies; starts immediately.
- **Phase 2: Foundational**: Depends on Phase 1; blocks all user-story work.
- **Phase 3: US1**: Depends on Phase 2 only.
- **Phase 4: US2**: Depends on Phase 3 because waitlist creation relies on the report view and report selection context.
- **Phase 5: US3**: Depends on Phase 4 because the queue operates on created waitlist records.
- **Phase 6: US4**: Depends on Phase 3 and Phase 5 because printing/defaults rely on active report and queue contexts.
- **Phase 7: Polish**: Depends on all desired user stories being complete.

### User Story Dependencies

- **US1 (P1)**: No user-story dependency after foundational work.
- **US2 (P1)**: Depends on US1 report surface and selection state.
- **US3 (P2)**: Depends on US2 waitlist persistence and linked-work creation.
- **US4 (P3)**: Depends on US1 report context and US3 queue context.

### User Story Dependency Graph

```text
US1 -> US2 -> US3 -> US4
US1 -------------> US4
```

### Parallel Opportunities

- Setup tasks T003 and T004 can run in parallel after T001 and T002.
- Foundational model and SQL tasks T006-T010 can run in parallel after T005.
- DAO tasks T011-T013 can run in parallel once the scripts/models they depend on exist.
- For each user story, the test tasks marked `[P]` can run in parallel.
- Query/command contract and handler files marked `[P]` can run in parallel when they touch different files.
- US4 defaults and print query files can be developed in parallel after US3 stabilizes the queue contract.

---

## Parallel Example: User Story 1

```bash
# Launch report validation tests together:
Task: T015 Add report query handler tests in MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackReportHandlerTests.cs
Task: T016 Add report viewmodel tests in MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReportTests.cs

# Build report query pieces together:
Task: T017 Create the report query contract in Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReport.cs
Task: T018 Implement the report query handler in Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReportHandler.cs
```

## Parallel Example: User Story 2

```bash
# Launch User Story 2 tests together:
Task: T022 Add batch upsert command handler tests in MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Command_CustomerPullPackBatchUpsertHandlerTests.cs
Task: T023 Add waitlist DAO integration tests in MTM_Receiving_Application.Tests/Integration/Module_ShipRec_Tools/Dao_CustomerPullPackWaitlistIntegrationTests.cs
Task: T024 Add waitlist editor viewmodel tests in MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Dialog_CustomerPullPackWaitlistEditorTests.cs

# Build User Story 2 backend pieces together:
Task: T025 Create the batch upsert command and validator in Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackBatchUpsert.cs
Task: T027 Create the linked waitlist lookup query in Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackLinkedWaitlist.cs
```

## Parallel Example: User Story 3

```bash
# Launch queue execution tests together:
Task: T033 Add queue query handler tests in MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackWaitlistQueueHandlerTests.cs
Task: T034 Add queue status transition tests in MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Command_CustomerPullPackUpdateStatusHandlerTests.cs
Task: T035 Add queue ownership and Problem-state tests in MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueueTests.cs

# Build queue command/query files together:
Task: T036 Create the queue query contract in Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackWaitlistQueue.cs
Task: T038 Create the status update command and validator in Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackUpdateStatus.cs
Task: T039 Create the explicit unassign command in Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackUnassignOwner.cs
```

## Parallel Example: User Story 4

```bash
# Launch defaults/print validation together:
Task: T044 Add defaults persistence integration tests in MTM_Receiving_Application.Tests/Integration/Module_ShipRec_Tools/Dao_CustomerPullPackUserDefaultsIntegrationTests.cs
Task: T045 Add print context query tests in MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/CustomerPullPack/Query_CustomerPullPackPrintContextHandlerTests.cs

# Build defaults and print read models together:
Task: T046 Create the defaults query and save command in Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackDefaults.cs and Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackSaveDefaults.cs
Task: T047 Create the print context query in Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackPrintContext.cs
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate the report flow independently before taking on waitlist creation.

### Incremental Delivery

1. Deliver US1 to establish the report surface and no-demand/shortage behavior.
2. Replace the temporary crystal-style report samples with live grouped report bindings and live row-selection state.
3. Add US2 to unlock requester value from report selection and waitlist creation.
4. Add US3 to move operational execution into the dedicated queue.
5. Add US4 to restore defaults and generate print output from stable read models.
6. Finish with Phase 7 cross-cutting validation and metadata updates.

### Suggested MVP Scope

- **Recommended MVP**: Phase 3 / User Story 1 only.
- **Business-complete MVP extension**: Add Phase 4 / User Story 2 if the first demo must include no-typing waitlist creation from the report.

## Notes

- `[P]` means the task touches different files and can run in parallel with other `[P]` tasks in the same phase.
- All tasks include repo-relative paths so they are immediately executable.
- User-story tasks are intentionally grouped so each story can be implemented and validated independently.
- Database persistence tasks target MySQL stored procedures and read-only Visual query scripts to stay constitution-compliant.
- Queue ownership and duplicate prevention are explicitly represented in task sequencing because they are now resolved business rules, not open questions.

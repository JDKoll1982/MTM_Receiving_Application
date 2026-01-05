# Tasks: Internal Routing Module Overhaul

**Feature**: 001-routing-module  
**Date**: 2026-01-04  
**Input**: Design documents from `/specs/001-routing-module/`  
**Prerequisites**: ✅ plan.md, ✅ spec.md, ✅ research.md, ✅ data-model.md, ✅ contracts/

**Tests**: Manual testing only (no automated tests in MVP as per plan.md)

**Organization**: Tasks are grouped by user story (US1-US4) to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

This project uses **Single project** structure (WinUI 3 desktop application):
- Module code: `Module_Routing/`
- Database: `Database/Schemas/`, `Database/StoredProcedures/`
- Configuration: `App.xaml.cs`, `appsettings.json`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and database foundation

- [ ] T001 Create Module_Routing/ folder structure (Models/, ViewModels/, Views/, Services/, Data/)
- [ ] T002 Create Database/Schemas/schema_routing.sql with all 6 tables from data-model.md
- [ ] T003 [P] Create Database/TestData/routing_sample_data.sql with seed recipients and other reasons
- [ ] T004 [P] Add RoutingModule configuration section to appsettings.json (CsvExportPath, EnableValidation, DefaultMode)

**Checkpoint**: Folder structure and database schema files created

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core database infrastructure and shared models that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Database Foundation

- [ ] T005 Create sp_routing_label_insert.sql in Database/StoredProcedures/
- [ ] T006 Create sp_routing_label_update.sql in Database/StoredProcedures/
- [ ] T007 Create sp_routing_label_get_by_id.sql in Database/StoredProcedures/
- [ ] T008 Create sp_routing_label_get_history.sql in Database/StoredProcedures/
- [ ] T009 Create sp_routing_label_check_duplicate.sql in Database/StoredProcedures/
- [ ] T010 [P] Create sp_routing_recipient_get_all.sql in Database/StoredProcedures/
- [ ] T011 [P] Create sp_routing_recipient_get_active.sql in Database/StoredProcedures/
- [ ] T012 [P] Create sp_routing_usage_tracking_increment.sql in Database/StoredProcedures/
- [ ] T013 [P] Create sp_routing_usage_tracking_get_top_recipients.sql in Database/StoredProcedures/
- [ ] T014 [P] Create sp_routing_user_preference_get.sql in Database/StoredProcedures/
- [ ] T015 [P] Create sp_routing_user_preference_upsert.sql in Database/StoredProcedures/
- [ ] T016 [P] Create sp_routing_other_reason_get_all.sql in Database/StoredProcedures/
- [ ] T017 [P] Create sp_routing_label_history_insert.sql in Database/StoredProcedures/

### Models (Data Classes)

- [ ] T018 [P] Create Model_RoutingLabel.cs in Module_Routing/Models/
- [ ] T019 [P] Create Model_RoutingRecipient.cs in Module_Routing/Models/
- [ ] T020 [P] Create Model_RoutingOtherReason.cs in Module_Routing/Models/
- [ ] T021 [P] Create Model_RoutingUsageTracking.cs in Module_Routing/Models/
- [ ] T022 [P] Create Model_RoutingUserPreference.cs in Module_Routing/Models/
- [ ] T023 [P] Create Model_RoutingLabelHistory.cs in Module_Routing/Models/
- [ ] T024 [P] Create Model_InforVisualPOLine.cs in Module_Routing/Models/

### DAOs (Data Access Objects)

- [ ] T025 Create Dao_RoutingLabel.cs in Module_Routing/Data/ (implements Insert, Update, GetById, GetHistory, CheckDuplicate)
- [ ] T026 Create Dao_RoutingRecipient.cs in Module_Routing/Data/ (implements GetAll, GetActive)
- [ ] T027 [P] Create Dao_RoutingOtherReason.cs in Module_Routing/Data/ (implements GetAll)
- [ ] T028 [P] Create Dao_RoutingUsageTracking.cs in Module_Routing/Data/ (implements Increment, GetTopRecipients)
- [ ] T029 [P] Create Dao_RoutingUserPreference.cs in Module_Routing/Data/ (implements Get, Upsert)
- [ ] T030 [P] Create Dao_RoutingLabelHistory.cs in Module_Routing/Data/ (implements Insert)
- [ ] T031 Create Dao_InforVisualPO.cs in Module_Routing/Data/ (READ ONLY - ValidatePO, GetLines, GetLine, CheckConnection)

### Services (Business Logic)

- [ ] T032 Create RoutingService.cs and IRoutingService.cs in Module_Routing/Services/ (label creation, CSV export, validation)
- [ ] T033 Create RoutingInforVisualService.cs and IRoutingInforVisualService.cs in Module_Routing/Services/ (PO validation, line retrieval)
- [ ] T034 [P] Create RoutingRecipientService.cs and IRoutingRecipientService.cs in Module_Routing/Services/ (recipient retrieval, filtering, Quick Add calculation)
- [ ] T035 [P] Create RoutingUsageTrackingService.cs and IRoutingUsageTrackingService.cs in Module_Routing/Services/ (usage count increment)
- [ ] T036 [P] Create RoutingUserPreferenceService.cs and IRoutingUserPreferenceService.cs in Module_Routing/Services/ (preference management)

### DI Registration

- [ ] T037 Register all Routing DAOs in App.xaml.cs ConfigureServices (Singleton lifetime)
- [ ] T038 Register all Routing Services in App.xaml.cs ConfigureServices (Singleton lifetime)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Wizard Mode with Smart Features (Priority: P1) 🎯 MVP

**Goal**: Implement 3-step wizard (PO Selection → Recipient Selection → Review) with Quick Add buttons, smart sorting, and "OTHER" PO workflow

**Independent Test**: 
1. Start Wizard → Enter valid PO → Select line → Click Quick Add recipient → Review → Create Label
2. Start Wizard → Enter "OTHER" → Select reason → Manual entry → Click Quick Add → Review → Create Label
3. Start Wizard → Enter invalid PO → Verify "treat as OTHER" prompt

### Implementation for User Story 1

#### Step 1: PO & Line Selection

- [ ] T039 [P] [US1] Create RoutingWizardStep1ViewModel.cs in Module_Routing/ViewModels/ (PO validation, line retrieval, "OTHER" logic)
- [ ] T040 [P] [US1] Create RoutingWizardStep1View.xaml (+ .cs) in Module_Routing/Views/ (PO input, line list, "OTHER" reason dropdown)
- [ ] T041 [US1] Implement PO validation workflow in Step1ViewModel (ValidatePO → GetLines OR show "OTHER" reason dropdown)
- [ ] T042 [US1] Implement "PO not found" logic with "treat as OTHER?" confirmation dialog
- [ ] T043 [US1] Implement navigation to Step 2 when line selected or "OTHER" reason entered

#### Step 2: Recipient Selection with Smart Features

- [ ] T044 [P] [US1] Create RoutingWizardStep2ViewModel.cs in Module_Routing/ViewModels/ (Quick Add calculation, recipient filtering, smart sorting)
- [ ] T045 [P] [US1] Create RoutingWizardStep2View.xaml (+ .cs) in Module_Routing/Views/ (5 Quick Add buttons, searchable recipient list)
- [ ] T046 [US1] Implement Quick Add button logic (GetTopRecipients → populate 5 buttons)
- [ ] T047 [US1] Implement personalization threshold (if employee has 20+ labels → personal usage, else system-wide)
- [ ] T048 [US1] Implement real-time search filtering (ObservableCollection updates on SearchText change)
- [ ] T049 [US1] Implement smart sorting (recipients sorted by usage_count DESC from database)
- [ ] T050 [US1] Implement Quick Add click → auto-select recipient → immediate navigation to Step 3

#### Step 3: Review & Confirm

- [ ] T051 [P] [US1] Create RoutingWizardStep3ViewModel.cs in Module_Routing/ViewModels/ (display all label data, save command)
- [ ] T052 [P] [US1] Create RoutingWizardStep3View.xaml (+ .cs) in Module_Routing/Views/ (review grid, "Edit" buttons for each field, "Create Label" button)
- [ ] T053 [US1] Implement label review display (PO, Line, Description, Recipient, Qty, Other Reason if applicable)
- [ ] T054 [US1] Implement "Edit" buttons to navigate back to Step 1 or Step 2
- [ ] T055 [US1] Implement "Create Label" command (CreateLabel → IncrementUsageCount → ExportToCsv → return to Mode Selection)
- [ ] T056 [US1] Implement duplicate check confirmation ("Label already created at [time]. Create anyway?")

#### Wizard Navigation & State Management

- [ ] T057 Create RoutingWizardContainerViewModel.cs (parent ViewModel managing current step, data flow between steps)
- [ ] T058 Create RoutingWizardContainerView.xaml (+ .cs) (host view embedding Step 1/2/3 UserControls, progress indicator)
- [ ] T059 Implement wizard state persistence (data flows from Step1 → Step2 → Step3 via shared ViewModel properties)
- [ ] T060 Implement "Cancel" command with confirmation ("Return to Mode Selection? Current progress will be lost")

**Checkpoint**: User Story 1 complete - Wizard mode fully functional with all smart features

---

## Phase 4: User Story 2 - Manual Entry Mode (Priority: P2)

**Goal**: Implement grid-based rapid entry mode with tab navigation, autocomplete, and batch save

**Independent Test**:
1. Select "Manual Entry" → Enter 5 labels using tab navigation → Click "Save All" → Verify all 5 labels in database and CSV
2. Test autocomplete for Recipient column (start typing → dropdown shows matches)

### Implementation for User Story 2

- [ ] T061 [P] [US2] Create RoutingManualEntryViewModel.cs in Module_Routing/ViewModels/ (DataGrid collection, validation, batch save)
- [ ] T062 [P] [US2] Create RoutingManualEntryView.xaml (+ .cs) in Module_Routing/Views/ (editable DataGrid with columns: PO, Line, Description, Recipient, Qty)
- [ ] T063 [US2] Implement ObservableCollection<Model_RoutingLabel> for DataGrid binding
- [ ] T064 [US2] Implement PO auto-fill logic (on cell edit → ValidatePO → populate Description from Infor Visual)
- [ ] T065 [US2] Implement Recipient autocomplete dropdown (ComboBox with ItemsSource from GetActiveRecipients)
- [ ] T066 [US2] Implement Enter key handling (completed row → validate → add new blank row)
- [ ] T067 [US2] Implement "Save All" command (iterate rows → validate each → batch insert to database and CSV)
- [ ] T068 [US2] Implement validation highlighting (invalid rows highlighted in red, prevent partial saves)
- [ ] T069 [US2] Implement tab navigation order (PO → Line → Description → Recipient → Qty → new row)

**Checkpoint**: User Story 2 complete - Manual Entry mode fully functional

---

## Phase 5: User Story 3 - Edit Mode (Priority: P3)

**Goal**: Implement searchable history grid with edit dialog and audit trail

**Independent Test**:
1. Select "Edit Mode" → Search for "Engineering" → Select label → Edit recipient to "Shipping" → Save → Verify database UPDATE (not INSERT) and history log
2. Test "Reprint" button (CSV regenerated without DB modification)

### Implementation for User Story 3

- [ ] T070 [P] [US3] Create RoutingEditModeViewModel.cs in Module_Routing/ViewModels/ (history retrieval, search filtering, edit dialog data)
- [ ] T071 [P] [US3] Create RoutingEditModeView.xaml (+ .cs) in Module_Routing/Views/ (searchable DataGrid, "Edit" and "Reprint" buttons)
- [ ] T072 [US3] Implement GetAllLabels with pagination (limit=100, offset=0 for first page)
- [ ] T073 [US3] Implement real-time search filter (filter by PO, recipient name, description)
- [ ] T074 [US3] Implement "Edit" button click → open ContentDialog with pre-populated fields
- [ ] T075 [US3] Create RoutingEditLabelDialog.xaml (+ .cs) as ContentDialog (fields: PO (read-only), Description, Recipient, Qty, Other Reason)
- [ ] T076 [US3] Implement "Save Changes" command in dialog (UpdateLabel → log history for each changed field → refresh grid)
- [ ] T077 [US3] Implement audit trail logging (compare old vs. new values → InsertLabelHistory for each changed field)
- [ ] T078 [US3] Implement "Reprint" button (RegenerateLabelCsv → CSV export only, no database UPDATE)
- [ ] T079 [US3] Implement read-only fields enforcement (Label ID, Created Date, Original Creator cannot be edited)
- [ ] T080 [US3] Implement concurrent edit handling ("Label was modified by [user] at [time]. Please refresh.")

**Checkpoint**: User Story 3 complete - Edit Mode fully functional with audit trail

---

## Phase 6: User Story 4 - Mode Selection & Preferences (Priority: P3)

**Goal**: Implement Mode Selection screen with default mode persistence

**Independent Test**:
1. Open Routing Module → Verify Mode Selection screen displays → Check "Set as default mode" for Wizard → Close app → Reopen → Verify Wizard launches directly
2. Test "Mode Selection" button in bottom bar from any mode

### Implementation for User Story 4

- [ ] T081 [P] [US4] Create RoutingModeSelectionViewModel.cs in Module_Routing/ViewModels/ (3 mode buttons, default mode checkbox logic)
- [ ] T082 [P] [US4] Create RoutingModeSelectionView.xaml (+ .cs) in Module_Routing/Views/ (3 mode cards: Wizard, Manual Entry, Edit Mode with descriptions)
- [ ] T083 [US4] Implement "Set as default mode" checkbox (SaveUserPreference on mode selection)
- [ ] T084 [US4] Implement default mode detection on module launch (GetUserPreference → if default set, navigate to that mode directly)
- [ ] T085 [US4] Implement "Mode Selection" button in bottom bar (visible from all views, navigates back to Mode Selection)
- [ ] T086 [US4] Implement "Return to Mode Selection?" confirmation dialog (warns about losing current progress)

**Checkpoint**: All user stories complete - Routing Module fully functional

---

## Phase 7: Navigation & Integration

**Purpose**: Wire up module navigation and main window integration

- [ ] T087 Add Routing module navigation item to MainWindow.xaml (navigation menu or button)
- [ ] T088 Implement navigation routing from MainWindow to RoutingModeSelectionView (or direct to default mode)
- [ ] T089 Register all Routing ViewModels in App.xaml.cs ConfigureServices (Transient lifetime)
- [ ] T090 Register all Routing Views in App.xaml.cs ConfigureServices (Transient lifetime)
- [ ] T091 Test end-to-end navigation (MainWindow → Routing → Wizard → Step1 → Step2 → Step3 → back to MainWindow)

**Checkpoint**: Navigation complete - module accessible from main application

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Global controls, error handling, CSV reset, help documentation

### Global Controls (Bottom Bar)

- [ ] T092 Add "Enable Validation" toggle to bottom bar (affects all modes, controls Infor Visual lookup)
- [ ] T093 Implement validation toggle persistence (SaveUserPreference on toggle change)
- [ ] T094 Implement "Reset CSV" button with confirmation dialog ("This will clear the CSV file. Continue?")
- [ ] T095 Implement ResetCsvFileAsync in RoutingService (truncate CSV file, log action)
- [ ] T096 Add "Help" button to bottom bar (opens context-sensitive help or documentation)

### Error Handling & Graceful Degradation

- [ ] T097 Implement Infor Visual connection failure handling in RoutingInforVisualService (3 retries, 500ms delay, graceful error message)
- [ ] T098 Implement CSV file lock handling in RoutingService (3 async retries with backoff: 500ms, 1s, 1.5s)
- [ ] T099 Implement CSV write failure fallback ("CSV file in use. Label saved to database only. Retry CSV export from History view.")
- [ ] T100 Implement all IService_ErrorHandler.HandleException calls with appropriate severity (Low/Medium/High/Critical)

### Performance Optimization

- [ ] T101 Add logging for slow Infor Visual queries (if >2 seconds, log warning with PO number)
- [ ] T102 Add logging for slow CSV writes (if >500ms, log warning)
- [ ] T103 Verify recipient filtering updates in <100ms (test with 200 recipients)

**Checkpoint**: Polish complete - production-ready module

---

## Phase 9: Manual Testing & Validation

**Purpose**: Execute comprehensive manual test scenarios from quickstart.md

- [ ] T104 Test User Story 1 - Wizard Mode (all 16 acceptance scenarios from spec.md)
- [ ] T105 Test User Story 2 - Manual Entry Mode (all 5 acceptance scenarios)
- [ ] T106 Test User Story 3 - Edit Mode (all 6 acceptance scenarios)
- [ ] T107 Test User Story 4 - Mode Selection & Preferences (all 6 acceptance scenarios)
- [ ] T108 Test all Edge Cases from spec.md (Infor Visual unreachable, duplicate labels, deleted recipients, CSV file locked, concurrent edits)
- [ ] T109 Verify all NFRs (Performance: <2s PO lookup, <100ms filtering, <500ms CSV write; Usability: keyboard-only navigation; Reliability: data integrity)
- [ ] T110 Test database migration from clean state (run schema_routing.sql, stored procedures, seed data)
- [ ] T111 Test concurrent label creation (10 users creating labels simultaneously, verify no deadlocks or corruption)

**Checkpoint**: All manual tests passed - ready for deployment

---

## Summary

**Total Tasks**: 111  
**User Story Breakdown**:
- US1 (Wizard Mode - P1): 22 tasks (T039-T060) 🎯 MVP
- US2 (Manual Entry - P2): 9 tasks (T061-T069)
- US3 (Edit Mode - P3): 11 tasks (T070-T080)
- US4 (Mode Selection - P3): 6 tasks (T081-T086)

**Phases**:
1. Setup (4 tasks)
2. Foundational (33 tasks) - BLOCKS all user stories
3. US1 - Wizard MVP (22 tasks)
4. US2 - Manual Entry (9 tasks)
5. US3 - Edit Mode (11 tasks)
6. US4 - Mode Selection (6 tasks)
7. Navigation (5 tasks)
8. Polish (9 tasks)
9. Testing (8 tasks)

**Parallel Execution Opportunities**:
- Phase 2: Tasks T010-T017 (stored procedures), T018-T024 (models), T027-T031 (DAOs), T034-T036 (services) can run in parallel
- Phase 3 (US1): T039-T040 (Step1), T044-T045 (Step2), T051-T052 (Step3) can be developed in parallel
- Phase 4-6 (US2-US4): All user stories can be implemented in parallel after Phase 2 completes

**Critical Path**: Phase 1 → Phase 2 (Foundational) → Phase 3 (US1 Wizard) → Phase 7 (Navigation) → Phase 9 (Testing)

**MVP Definition**: Phase 1 + Phase 2 + Phase 3 (US1) + Phase 7 = Wizard Mode with Smart Features (minimum viable product)

---

**Status**: ✅ Tasks Complete - Ready for implementation via /speckit.implement

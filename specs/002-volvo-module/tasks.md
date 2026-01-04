# Tasks: Volvo Dunnage Requisition Module

**Input**: Design documents from `/specs/002-volvo-module/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), data-model.md, volvo-workflows/

**Tests**: No automated tests in MVP - manual user testing only

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1-US6)
- Include exact file paths in descriptions

## Path Conventions

Single project structure - paths relative to repository root:
- Models: `Module_Volvo/Models/`
- ViewModels: `Module_Volvo/ViewModels/`
- Views: `Module_Volvo/Views/`
- Services: `Module_Volvo/Services/`
- Service Interfaces: `Module_Core/Contracts/Services/`
- Data: `Module_Volvo/Data/`
- Database: `Database/Schemas/`, `Database/StoredProcedures/`, `Database/TestData/`

**Namespace Pattern**:
- Models: `MTM_Receiving_Application.Module_Volvo.Models`
- ViewModels: `MTM_Receiving_Application.Module_Volvo.ViewModels`
- Views: `MTM_Receiving_Application.Module_Volvo.Views`
- Services: `MTM_Receiving_Application.Module_Volvo.Services`
- Service Interfaces: `MTM_Receiving_Application.Module_Core.Contracts.Services`
- Data: `MTM_Receiving_Application.Module_Volvo.Data`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Database and DI registration

- [X] T001 Create database schema in Database/Schemas/schema_volvo.sql (tables: volvo_shipments, volvo_shipment_lines, volvo_parts_master, volvo_part_components)
- [X] T002 [P] Create stored procedures in Database/StoredProcedures/ (sp_volvo_shipment_insert, sp_volvo_shipment_complete, sp_volvo_part_master_get_all, etc.)
- [X] T003 [P] Create view vw_volvo_shipments_history in Database/Schemas/schema_volvo.sql
- [X] T004 Load sample data in Database/TestData/volvo_sample_data.sql (DataSheet.csv import for parts_master and part_components)
- [ ] T005 Deploy database schema to MySQL server (execute schema_volvo.sql, stored procedures, view, test data)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core models and DAOs that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T006 [P] Create Model_VolvoShipment in Module_Volvo/Models/Model_VolvoShipment.cs (properties: Id, ShipmentDate, ShipmentNumber, PONumber, ReceiverNumber, EmployeeNumber, Notes, Status, CreatedDate, ModifiedDate, IsArchived)
- [X] T007 [P] Create Model_VolvoShipmentLine in Module_Volvo/Models/Model_VolvoShipmentLine.cs (properties: Id, ShipmentId, PartNumber, ReceivedSkidCount, CalculatedPieceCount, HasDiscrepancy, ExpectedSkidCount, DiscrepancyNote)
- [X] T008 [P] Create Model_VolvoPart in Module_Volvo/Models/Model_VolvoPart.cs (properties: PartNumber, Description, QuantityPerSkid, IsActive, CreatedDate, ModifiedDate)
- [X] T009 [P] Create Model_VolvoPartComponent in Module_Volvo/Models/Model_VolvoPartComponent.cs (properties: Id, ParentPartNumber, ComponentPartNumber, Quantity)
- [X] T010 Create Dao_VolvoShipment in Module_Volvo/Data/Dao_VolvoShipment.cs (methods: InsertAsync, UpdateAsync, CompleteAsync, GetPendingAsync, GetByIdAsync)
- [X] T011 Create Dao_VolvoShipmentLine in Module_Volvo/Data/Dao_VolvoShipmentLine.cs (methods: InsertAsync, GetByShipmentIdAsync, UpdateAsync, DeleteAsync)
- [X] T012 Create Dao_VolvoPart in Module_Volvo/Data/Dao_VolvoPart.cs (methods: GetAllAsync, GetByIdAsync, InsertAsync, UpdateAsync, DeactivateAsync)
- [X] T013 Create Dao_VolvoPartComponent in Module_Volvo/Data/Dao_VolvoPartComponent.cs (methods: GetByParentPartAsync, InsertAsync, DeleteByParentPartAsync)
- [X] T014 Register Volvo DAOs in App.xaml.cs ConfigureServices (singletons: Dao_VolvoShipment, Dao_VolvoShipmentLine, Dao_VolvoPart, Dao_VolvoPartComponent)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Receive Volvo Dunnage Shipment (Priority: P1) 🎯 MVP

**Goal**: User can enter shipment, track discrepancies, generate labels, create PO email, save as pending

**Independent Test**: Enter V-EMB-21 (10 skids), V-EMB-750 (7 skids), generate labels (CSV created), copy email to clipboard, save as pending PO (shipment saved in DB with status='pending_po')

### Implementation for User Story 1

- [X] T015 [P] [US1] Create IService_Volvo interface in Module_Core/Contracts/Services/IService_Volvo.cs (methods: CalculateComponentExplosionAsync, GenerateLabelCsvAsync, FormatEmailTextAsync, SaveShipmentAsync, GetPendingShipmentAsync)
- [X] T016 [US1] Implement Service_Volvo in Module_Volvo/Services/Service_Volvo.cs (business logic for component explosion, label generation, email formatting, shipment save)
- [X] T017 [US1] Register IService_Volvo/Service_Volvo in App.xaml.cs ConfigureServices (singleton)
- [X] T018 [US1] Create VolvoShipmentEntryViewModel in Module_Volvo/ViewModels/VolvoShipmentEntryViewModel.cs (properties: ShipmentDate, ShipmentNumber, Parts (ObservableCollection), AvailableParts, Commands: AddPartCommand, RemovePartCommand, GenerateLabelsCommand, PreviewEmailCommand, SaveAsPendingCommand)
- [X] T019 [US1] Create VolvoShipmentEntryView.xaml in Module_Volvo/Views/VolvoShipmentEntryView.xaml (UI: Date/Shipment# header, Part entry DataGrid with columns: Part dropdown, Skids TextBox, Discrepancy checkbox, Packlist/Difference fields, Toolbar: Add Part, Remove Part, Generate Labels, Preview Email, Save as Pending)
- [X] T020 [US1] Create VolvoShipmentEntryView.xaml.cs code-behind (ViewModel injection, page loaded event handler)
- [X] T021 [US1] Implement AddPartCommand in VolvoShipmentEntryViewModel (adds new empty row to Parts collection)
- [X] T022 [US1] Implement RemovePartCommand in VolvoShipmentEntryViewModel (removes selected row from Parts collection)
- [X] T023 [US1] Implement GenerateLabelsCommand in VolvoShipmentEntryViewModel (calls VolvoService.GenerateLabelCsvAsync, shows success InfoBar with file path)
- [X] T024 [US1] Implement PreviewEmailCommand in VolvoShipmentEntryViewModel (opens ContentDialog with editable greeting/notes, read-only tables, Copy to Clipboard button)
- [X] T025 [US1] Create VolvoEmailPreviewDialog.xaml in Module_Volvo/Views/VolvoEmailPreviewDialog.xaml (ContentDialog with TextBox for greeting, TextBlock for discrepancies table, TextBlock for requested lines table, Buttons: Copy to Clipboard, Close)
- [X] T026 [US1] Implement SaveAsPendingCommand in VolvoShipmentEntryViewModel (validates input, calls VolvoService.SaveShipmentAsync with status='pending_po', shows success InfoBar, navigates to main menu or shows pending modal)
- [X] T027 [US1] Add Volvo navigation menu item in MainWindow.xaml (NavigationViewItem with Material Icon, navigates to VolvoShipmentEntryView)
- [X] T028 [US1] Register VolvoShipmentEntryViewModel in App.xaml.cs ConfigureServices (transient)

**Checkpoint**: At this point, User Story 1 should be fully functional - user can enter shipment, generate labels, create email, save as pending

---

## Phase 4: User Story 2 - Complete PO After Purchasing Response (Priority: P2) 🎯 MVP

**Goal**: User can complete pending shipment by entering PO and Receiver numbers

**Independent Test**: Open Volvo module with pending shipment, modal displays, click "Complete with PO", enter PO-062450 and 134393, save, status changes to 'completed' and CSV file is cleared

### Implementation for User Story 2

- [X] T029 [P] [US2] Create VolvoReviewViewModel in Module_Volvo/ViewModels/VolvoReviewViewModel.cs (properties: PendingShipment, Parts (ObservableCollection), Commands: CompletePOCommand, ViewEditCommand)
- [X] T030 [US2] Create VolvoReviewView.xaml in Module_Volvo/Views/VolvoReviewView.xaml (ContentDialog modal: displays pending shipment summary, parts list, Buttons: Complete with PO, View/Edit, Cancel)
- [X] T031 [US2] Create VolvoCompletePODialog.xaml in Module_Volvo/Views/VolvoCompletePODialog.xaml (ContentDialog with TextBoxes for PO Number and Receiver Number, warning message about CSV clearing, Buttons: Save, Cancel)
- [X] T032 [US2] Implement CompletePOCommand in VolvoReviewViewModel (validates PO/Receiver input, calls VolvoService.CompleteShipmentAsync, clears CSV file content, updates status to 'completed', navigates away)
- [X] T033 [US2] Modify VolvoShipmentEntryView.xaml.cs OnPageLoaded to check for pending shipment (calls VolvoService.GetPendingShipmentAsync, if exists, shows VolvoReviewView modal)
- [X] T034 [US2] Register VolvoReviewViewModel in App.xaml.cs ConfigureServices (transient)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work - shipments can be entered, saved as pending, and completed with PO

---

## Phase 5: User Story 3 - Manage Volvo Parts Master Data (Priority: P2) 🎯 MVP

**Goal**: Admin can add/edit/deactivate Volvo parts and components, import/export CSV

**Independent Test**: Open Settings → Volvo → Master Data, add part V-EMB-NEW (50 pcs/skid, includes V-EMB-2), edit V-EMB-500 quantity to 90, deactivate V-EMB-OLD, export to CSV

### Implementation for User Story 3

- [ ] T035 [P] [US3] Create IService_VolvoMasterData interface in Module_Core/Contracts/Services/IService_VolvoMasterData.cs (methods: GetAllPartsAsync, AddPartAsync, UpdatePartAsync, DeactivatePartAsync, ImportCsvAsync, ExportCsvAsync)
- [ ] T036 [US3] Implement Service_VolvoMasterData in Module_Volvo/Services/Service_VolvoMasterData.cs (CRUD operations, CSV import/export logic with validation)
- [ ] T037 [US3] Register IService_VolvoMasterData/Service_VolvoMasterData in App.xaml.cs ConfigureServices (singleton)
- [ ] T038 [US3] Create VolvoSettingsViewModel in Module_Volvo/ViewModels/VolvoSettingsViewModel.cs (properties: Parts (ObservableCollection), SelectedPart, ShowInactive, Commands: AddPartCommand, EditPartCommand, DeactivatePartCommand, ViewComponentsCommand, ImportCsvCommand, ExportCsvCommand, RefreshCommand)
- [ ] T039 [US3] Create VolvoSettingsView.xaml in Module_Volvo/Views/VolvoSettingsView.xaml (replaces Settings_PlaceholderView for Volvo, Tabs: Master Data, Import/Export, Preferences, DataGrid with columns: Part Number, Description, Qty/Skid, Has Components, Active Status, Actions buttons)
- [ ] T040 [US3] Create VolvoSettingsView.xaml.cs code-behind (ViewModel injection, page loaded event handler calls RefreshCommand)
- [ ] T041 [US3] Create VolvoPartAddEditDialog.xaml in Module_Volvo/Views/VolvoPartAddEditDialog.xaml (ContentDialog with fields: Part Number TextBox, Qty/Skid NumericUpDown, Has Components CheckBox → Expander with component checkboxes/dropdowns, Buttons: Save, Cancel)
- [ ] T042 [US3] Implement AddPartCommand in VolvoSettingsViewModel (opens VolvoPartAddEditDialog in Add mode, calls VolvoMasterDataService.AddPartAsync, refreshes DataGrid)
- [ ] T043 [US3] Implement EditPartCommand in VolvoSettingsViewModel (opens VolvoPartAddEditDialog in Edit mode with pre-filled data, Part Number read-only, shows historical integrity warning, calls VolvoMasterDataService.UpdatePartAsync, refreshes DataGrid)
- [ ] T044 [US3] Implement DeactivatePartCommand in VolvoSettingsViewModel (shows confirmation ContentDialog, calls VolvoMasterDataService.DeactivatePartAsync, grays out row in DataGrid)
- [ ] T045 [US3] Implement ViewComponentsCommand in VolvoSettingsViewModel (opens TeachingTip or Flyout showing component breakdown for selected part)
- [ ] T046 [US3] Implement ImportCsvCommand in VolvoSettingsViewModel (opens OpenFileDialog for CSV selection, calls VolvoMasterDataService.ImportCsvAsync with preview dialog showing new/updated/unchanged parts, refreshes DataGrid after import)
- [ ] T047 [US3] Implement ExportCsvCommand in VolvoSettingsViewModel (opens SaveFileDialog, calls VolvoMasterDataService.ExportCsvAsync, shows success InfoBar with file path)
- [ ] T048 [US3] Update Settings_ModeSelectionView.xaml to include Volvo card (Material Icon, Title: "Volvo", Description: "Dunnage parts and components", navigates to VolvoSettingsView)
- [ ] T049 [US3] Update Settings_WorkflowView.xaml to replace VolvoPlaceholderView with VolvoSettingsView
- [ ] T050 [US3] Register VolvoSettingsViewModel in App.xaml.cs ConfigureServices (transient)

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work - shipments can be entered/completed, and admin can manage master data

---

## Phase 6: User Story 4 - View and Edit Shipment History (Priority: P3)

**Goal**: User can filter/view/edit historical shipments and export to CSV

**Independent Test**: Open Volvo History, filter by date range and status (Pending PO / Completed), view detail flyout for shipment, edit shipment to correct skid count (CSV regenerates), export to CSV

### Implementation for User Story 4

- [ ] T051 [P] [US4] Create VolvoHistoryViewModel in Module_Volvo/ViewModels/VolvoHistoryViewModel.cs (properties: History (ObservableCollection), StartDate, EndDate, StatusFilter, SelectedShipment, Commands: FilterCommand, ViewDetailCommand, EditCommand, ExportCommand)
- [ ] T052 [US4] Create VolvoHistoryView.xaml in Module_Volvo/Views/VolvoHistoryView.xaml (UI: Date range pickers, Status filter dropdown, DataGrid with columns: Date, Shipment#, Part Count, PO/Receiver, Status, Actions buttons, Toolbar: Filter, Export)
- [ ] T053 [US4] Create VolvoHistoryView.xaml.cs code-behind (ViewModel injection, page loaded event handler defaults to last 30 days and calls FilterCommand)
- [ ] T054 [US4] Implement FilterCommand in VolvoHistoryViewModel (calls VolvoService.GetHistoryAsync with date range and status filter, populates History collection)
- [ ] T055 [US4] Implement ViewDetailCommand in VolvoHistoryViewModel (opens Flyout panel showing parts entered, discrepancies table, calculated requested lines)
- [ ] T056 [US4] Create VolvoShipmentEditDialog.xaml in Module_Volvo/Views/VolvoShipmentEditDialog.xaml (ContentDialog with same layout as entry form but pre-filled, shows CSV regeneration warning, Buttons: Save Changes, Cancel)
- [ ] T057 [US4] Implement EditCommand in VolvoHistoryViewModel (opens VolvoShipmentEditDialog, calls VolvoService.UpdateShipmentAsync, regenerates CSV if applicable, refreshes history DataGrid)
- [ ] T058 [US4] Implement ExportCommand in VolvoHistoryViewModel (opens SaveFileDialog, calls VolvoService.ExportHistoryToCsvAsync, shows success InfoBar)
- [ ] T059 [US4] Add Volvo History navigation menu item in MainWindow.xaml (NavigationViewItem under Volvo parent, navigates to VolvoHistoryView)
- [ ] T060 [US4] Register VolvoHistoryViewModel in App.xaml.cs ConfigureServices (transient)

**Checkpoint**: All core user stories (US1-US4) are now functional - full CRUD cycle for shipments and master data

---

## Phase 7: User Story 5 - View Volvo Shipments in End-of-Day Reports (Priority: P3)

**Goal**: Volvo shipments integrated into shared End-of-Day reporting module

**Independent Test**: Open Reports module, select date range 01/02-01/03, check availability (Volvo shows 8 records), check Volvo checkbox, generate report showing Pending PO and Completed sections

### Implementation for User Story 5

- [ ] T061 [P] [US5] Create IService_VolvoReporting interface in Module_Core/Contracts/Services/IService_VolvoReporting.cs (methods: GetRecordCountAsync, GetReportDataAsync)
- [ ] T062 [US5] Implement Service_VolvoReporting in Module_Volvo/Services/Service_VolvoReporting.cs (queries for record counts and report data by date range)
- [ ] T063 [US5] Register IService_VolvoReporting/Service_VolvoReporting in App.xaml.cs ConfigureServices (singleton)
- [ ] T064 [US5] Update shared ReportingService to call Service_VolvoReporting.GetRecordCountAsync in CheckAvailabilityAsync method (add Volvo to module count dictionary)
- [ ] T065 [US5] Update shared ReportingViewModel to include Volvo checkbox in module selection (bind to IsVolvoChecked property)
- [ ] T066 [US5] Update shared ReportingView.xaml to add Volvo checkbox to module selection panel (CheckBox with label "Volvo Dunnage Requisition")
- [ ] T067 [US5] Create VolvoReportSectionView.xaml in Module_Volvo/Views/VolvoReportSectionView.xaml (UserControl with two DataGrids: Pending PO section and Completed section, Complete button for pending items)
- [ ] T068 [US5] Update shared ReportingView.xaml to include VolvoReportSectionView in tabbed interface or expander (conditional visibility based on IsVolvoChecked)
- [ ] T069 [US5] Implement GenerateReportAsync in ReportingViewModel to call Service_VolvoReporting.GetReportDataAsync when Volvo is selected (populate Volvo tab/expander with data)
- [ ] T070 [US5] Implement Complete action in VolvoReportSectionView (opens VolvoCompletePODialog from US2, completes shipment, refreshes report)

**Checkpoint**: Volvo module is now integrated into shared reporting infrastructure

---

## Phase 8: User Story 6 - Packlist Summary View (Priority: P4) 📦 FUTURE

**Goal**: OPTIONAL - Aggregated packlist summary view for historical analysis

**Note**: NOT required for MVP release. Can be implemented in future sprint.

### Implementation for User Story 6 (FUTURE)

- [ ] T071 [FUTURE] [US6] Create VolvoPacklistSummaryViewModel in Module_Volvo/ViewModels/VolvoPacklistSummaryViewModel.cs
- [ ] T072 [FUTURE] [US6] Create VolvoPacklistSummaryView.xaml in Module_Volvo/Views/VolvoPacklistSummaryView.xaml
- [ ] T073 [FUTURE] [US6] Implement aggregation query in VolvoService.GetPacklistSummariesAsync
- [ ] T074 [FUTURE] [US6] Implement drill-down flyout for PO detail
- [ ] T075 [FUTURE] [US6] Implement export detail to CSV functionality

**Checkpoint**: Future enhancement - packlist summary provides Google Sheets-like tracking

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final touches and documentation

- [ ] T076 [P] Add XML doc comments to all public methods in Services and DAOs
- [ ] T077 [P] Add logging via ILoggingService to all major operations (shipment insert, complete, master data changes)
- [ ] T078 [P] Review error handling in all ViewModels (ensure IService_ErrorHandler is used consistently)
- [ ] T079 [P] Test CSV file lifecycle (generation, clearing, regeneration on edit)
- [ ] T080 [P] Verify component explosion calculations with sample data (V-EMB-500, V-EMB-750, V-EMB-116)
- [ ] T081 [P] Test pending PO singleton constraint (attempt to create second pending shipment, verify modal blocks)
- [ ] T082 [P] Test master data historical integrity (edit part quantity, verify old shipments unaffected)
- [ ] T083 [P] Create user documentation in Documentation/Features/Volvo/README.md (quickstart guide with screenshots)
- [ ] T084 Final code review and cleanup (remove debug code, unused imports, ensure consistent naming)

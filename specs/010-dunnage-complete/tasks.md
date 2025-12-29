# Tasks: Dunnage Receiving System - Complete Implementation

**Feature Branch**: `010-dunnage-complete` | **Date**: 2025-12-29  
**Input**: Design documents from `/specs/010-dunnage-complete/`

## Overview

This task list implements a complete Dunnage Receiving System with 20 user stories organized into 3 priority levels:
- **P1 (High Priority)**: US1-US12 (12 stories) - Core functionality
- **P2 (Medium Priority)**: US13-US19 (7 stories) - Enhanced features
- **P3 (Low Priority)**: US20 (1 story) - Advanced validation

Tasks are organized by user story to enable independent implementation and testing. Each story delivers standalone value and can be tested in isolation.

## Format: `[ID] [P?] [Story?] Description with file path`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: User story label (US1, US2, etc.) - Required for user story phases only
- File paths are absolute from repository root

---

## Phase 1: Setup (Project Infrastructure)

**Purpose**: Database schema extensions and dependency registration

- [ ] T001 Run database migration script Database/Migrations/010-dunnage-complete-schema.sql
- [ ] T002 [P] Verify schema extensions - check custom_field_definitions and user_preferences tables exist
- [ ] T003 [P] Verify new columns on dunnage_types (Icon) and inventoried_dunnage_list (InventoryMethod, Notes)
- [ ] T004 [P] Create new stored procedure files in Database/StoredProcedures/Dunnage/ directory
- [ ] T005 Register new DAOs in App.xaml.cs ConfigureServices (Dao_DunnageType, Dao_DunnagePart, Dao_DunnageSpec, Dao_DunnageLoad, Dao_InventoriedDunnage as singletons)
- [ ] T006 [P] Register new services in App.xaml.cs ConfigureServices (IService_MySQL_Dunnage, IService_DunnageCSVWriter, IService_DunnageAdminWorkflow)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story implementation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Database Layer - Stored Procedures

- [X] T007 [P] Create sp_dunnage_types_update.sql in Database/StoredProcedures/Dunnage/
- [X] T008 [P] Create sp_dunnage_types_get_part_count.sql in Database/StoredProcedures/Dunnage/
- [X] T009 [P] Create sp_dunnage_types_get_transaction_count.sql in Database/StoredProcedures/Dunnage/
- [X] T010 [P] Create sp_dunnage_types_check_duplicate.sql in Database/StoredProcedures/Dunnage/
- [X] T011 [P] Create sp_dunnage_parts_get_by_type.sql in Database/StoredProcedures/Dunnage/
- [X] T012 [P] Create sp_dunnage_parts_get_transaction_count.sql in Database/StoredProcedures/Dunnage/
- [X] T013 [P] Create sp_dunnage_loads_get_by_date_range.sql in Database/StoredProcedures/Dunnage/
- [X] T014 [P] Create sp_dunnage_loads_update.sql in Database/StoredProcedures/Dunnage/
- [X] T015 [P] Create sp_dunnage_specs_get_all_keys.sql in Database/StoredProcedures/Dunnage/
- [X] T016 [P] Create sp_custom_fields_insert.sql in Database/StoredProcedures/Dunnage/
- [X] T017 [P] Create sp_custom_fields_get_by_type.sql in Database/StoredProcedures/Dunnage/
- [X] T018 [P] Create sp_user_preferences_upsert.sql in Database/StoredProcedures/Dunnage/
- [X] T019 [P] Create sp_inventoried_dunnage_update.sql in Database/StoredProcedures/Dunnage/
- [X] T020 [P] Create sp_inventoried_dunnage_delete.sql in Database/StoredProcedures/Dunnage/

### Models - Base Entities

- [X] T021 [P] Create Model_CSVWriteResult.cs in Models/Dunnage/ (Already exists in Models/Receiving/)
- [X] T022 [P] Create Model_IconDefinition.cs in Models/Dunnage/
- [X] T023 [P] Create Model_CustomFieldDefinition.cs in Models/Dunnage/

### Service Contracts

- [X] T024 [P] Create IService_MySQL_Dunnage.cs interface in Contracts/Services/ (extends existing with admin methods)
- [X] T025 [P] Create IService_DunnageCSVWriter.cs interface in Contracts/Services/ (extends existing with dynamic columns)
- [X] T026 [P] Create IService_DunnageAdminWorkflow.cs interface in Contracts/Services/ (new for admin navigation)

### DAO Extensions - Instance Methods

- [X] T027 Extend Dao_DunnageType.cs - add UpdateTypeAsync, GetPartCountByTypeAsync, GetTransactionCountByTypeAsync, CheckDuplicateNameAsync methods in Data/Dunnage/
- [X] T028 Extend Dao_DunnagePart.cs - add GetPartsByTypeAsync, GetTransactionCountByPartAsync, SearchPartsAsync methods in Data/Dunnage/
- [X] T029 Extend Dao_DunnageSpec.cs - add GetAllSpecKeysAsync method in Data/Dunnage/
- [X] T030 Extend Dao_DunnageLoad.cs - add GetLoadsByDateRangeAsync, UpdateLoadAsync methods in Data/Dunnage/
- [X] T031 Extend Dao_InventoriedDunnage.cs - add UpdateInventoriedEntryAsync, DeleteInventoriedEntryAsync methods in Data/Dunnage/

### Service Implementations

- [X] T032 Extend Service_MySQL_Dunnage.cs - add UpdateTypeAsync, GetPartsByTypeAsync, GetAllSpecKeysAsync, DeleteTypeWithImpactCheckAsync methods in Services/Receiving/
- [X] T033 Extend Service_DunnageCSVWriter.cs - add WriteToCSVWithDynamicColumnsAsync method in Services/Receiving/
- [X] T034 Create Service_DunnageAdminWorkflow.cs - implement navigation logic in Services/Receiving/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Manual Entry Grid with Batch Operations (Priority: P1) 🎯 MVP

**Goal**: Enable power users to enter multiple dunnage loads in spreadsheet style with toolbar operations (Add Row, Add Multiple, Remove Row, Auto-Fill) for efficient batch processing

**Independent Test**: Open manual entry mode, add 10 rows, fill data in grid cells, use auto-fill for spec columns, save batch, verify all 10 loads persist in database

### Implementation for User Story 1

- [ ] T035 [P] [US1] Create Dunnage_ManualEntryViewModel.cs in ViewModels/Dunnage/
- [ ] T036 [P] [US1] Create Dunnage_ManualEntryView.xaml in Views/Dunnage/
- [ ] T037 [US1] Create Dunnage_ManualEntryView.xaml.cs code-behind in Views/Dunnage/
- [ ] T038 [P] [US1] Implement AddRowCommand in Dunnage_ManualEntryViewModel.cs
- [ ] T039 [P] [US1] Implement RemoveRowCommand in Dunnage_ManualEntryViewModel.cs
- [ ] T040 [US1] Create AddMultipleRowsDialog.xaml ContentDialog in Views/Dunnage/Dialogs/
- [ ] T041 [US1] Implement AddMultipleRowsCommand with dialog logic in Dunnage_ManualEntryViewModel.cs
- [ ] T042 [US1] Implement SaveCommand with batch insert to database in Dunnage_ManualEntryViewModel.cs
- [ ] T043 [P] [US1] Add dynamic spec column generation logic in Dunnage_ManualEntryViewModel.cs (calls GetAllSpecKeysAsync)
- [ ] T044 [P] [US1] Bind DataGrid columns to ObservableCollection in Dunnage_ManualEntryView.xaml
- [ ] T045 [US1] Implement toolbar with Add Row, Add Multiple, Remove Row, Save buttons in Dunnage_ManualEntryView.xaml
- [ ] T046 [US1] Register Dunnage_ManualEntryViewModel and Dunnage_ManualEntryView in App.xaml.cs DI container

**Checkpoint**: User Story 1 complete - manual entry grid with batch operations functional and testable

---

## Phase 4: User Story 2 - Auto-Fill from Part Master Data (Priority: P1)

**Goal**: Automatically populate spec values from part's master definition when PartID is selected to reduce data entry time and errors

**Independent Test**: Enter PartID "PALLET-48X40" in a row, trigger auto-fill, verify Type and all spec columns (Width, Height, Depth) populate from dunnage_part_numbers.DunnageSpecValues

### Implementation for User Story 2

- [ ] T047 [US2] Implement AutoFillCommand in Dunnage_ManualEntryViewModel.cs (queries GetPartByIdAsync from Service_MySQL_Dunnage)
- [ ] T048 [US2] Add PartID selection change handler to trigger auto-fill in Dunnage_ManualEntryViewModel.cs
- [ ] T049 [P] [US2] Implement spec value deserialization logic (JSON to dictionary) in Dunnage_ManualEntryViewModel.cs
- [ ] T050 [P] [US2] Add "Auto-Fill" toolbar button in Dunnage_ManualEntryView.xaml
- [ ] T051 [US2] Implement validation for invalid PartID (part not found error) in Dunnage_ManualEntryViewModel.cs

**Checkpoint**: User Story 2 complete - auto-fill functionality working independently

---

## Phase 5: User Story 3 - Edit Mode with History Loading (Priority: P1)

**Goal**: Load and edit historical dunnage transaction records with date range filtering for error correction and activity review

**Independent Test**: Click "Load from History", select date range (last 7 days), verify records load in grid, edit quantity in a row, save changes, verify database update

### Implementation for User Story 3

- [ ] T052 [P] [US3] Create Dunnage_EditModeViewModel.cs in ViewModels/Dunnage/
- [ ] T053 [P] [US3] Create Dunnage_EditModeView.xaml in Views/Dunnage/
- [ ] T054 [US3] Create Dunnage_EditModeView.xaml.cs code-behind in Views/Dunnage/
- [ ] T055 [P] [US3] Implement LoadFromHistoryCommand in Dunnage_EditModeViewModel.cs (calls GetLoadsByDateRangeAsync)
- [ ] T056 [P] [US3] Add date filter toolbar with Start/End CalendarDatePickers in Dunnage_EditModeView.xaml
- [ ] T057 [US3] Implement SaveChangesCommand with UpdateLoadAsync calls in Dunnage_EditModeViewModel.cs
- [ ] T058 [P] [US3] Implement RemoveRowCommand with DeleteLoadAsync and confirmation dialog in Dunnage_EditModeViewModel.cs
- [ ] T059 [P] [US3] Add checkbox column for multi-select in Dunnage_EditModeView.xaml DataGrid
- [ ] T060 [US3] Register Dunnage_EditModeViewModel and Dunnage_EditModeView in App.xaml.cs DI container

**Checkpoint**: User Story 3 complete - edit mode with history loading functional

---

## Phase 6: User Story 4 - Admin Main Navigation Hub (Priority: P1)

**Goal**: Provide central navigation page with 4 management areas (Types, Specs, Parts, Inventoried List) as entry point for all dunnage configuration features

**Independent Test**: Navigate to Settings > Dunnage Management, verify 4 navigation cards display, click each card, confirm correct management view opens

### Implementation for User Story 4

- [ ] T061 [P] [US4] Create Dunnage_AdminMainViewModel.cs in ViewModels/Dunnage/
- [ ] T062 [P] [US4] Create Dunnage_AdminMainView.xaml in Views/Dunnage/
- [ ] T063 [US4] Create Dunnage_AdminMainView.xaml.cs code-behind in Views/Dunnage/
- [ ] T064 [P] [US4] Implement navigation commands (NavigateToManageTypes, NavigateToManageSpecs, NavigateToManageParts, NavigateToInventoriedList) in Dunnage_AdminMainViewModel.cs
- [ ] T065 [P] [US4] Implement ReturnToMainNavigationCommand in Dunnage_AdminMainViewModel.cs
- [ ] T066 [P] [US4] Add 4 navigation cards (2x2 grid) with icons, titles, descriptions in Dunnage_AdminMainView.xaml
- [ ] T067 [P] [US4] Implement visibility management flags (IsMainNavigationVisible, IsManageTypesVisible, etc.) in Dunnage_AdminMainViewModel.cs
- [ ] T068 [US4] Add Settings page integration - "Launch Dunnage Admin" button in Settings/SettingsPage.xaml
- [ ] T069 [US4] Register Dunnage_AdminMainViewModel and Dunnage_AdminMainView in App.xaml.cs DI container

**Checkpoint**: User Story 4 complete - admin navigation hub functional

---

## Phase 7: User Story 5 - Dunnage Type Management (Priority: P1)

**Goal**: View, add, edit, and delete dunnage types with impact analysis to maintain type taxonomy safely

**Independent Test**: Add new type "Container", edit its name to "Container Box", attempt to delete a type with parts (should block with impact count), delete unused type (should succeed)

### Implementation for User Story 5

- [ ] T070 [P] [US5] Create Dunnage_AdminTypesViewModel.cs in ViewModels/Dunnage/
- [ ] T071 [P] [US5] Create Dunnage_AdminTypesView.xaml in Views/Dunnage/
- [ ] T072 [US5] Create Dunnage_AdminTypesView.xaml.cs code-behind in Views/Dunnage/
- [ ] T073 [P] [US5] Implement LoadTypesCommand in Dunnage_AdminTypesViewModel.cs (calls GetAllTypesAsync)
- [ ] T074 [P] [US5] Implement ShowEditTypeCommand with ContentDialog in Dunnage_AdminTypesViewModel.cs
- [ ] T075 [US5] Implement ShowDeleteConfirmationCommand with impact analysis (GetPartCountByTypeAsync, GetTransactionCountByTypeAsync) in Dunnage_AdminTypesViewModel.cs
- [ ] T076 [P] [US5] Implement DeleteTypeCommand with "DELETE" confirmation requirement in Dunnage_AdminTypesViewModel.cs
- [ ] T077 [P] [US5] Add DataGrid with columns (TypeName, DateAdded, AddedBy, LastModified, Actions) in Dunnage_AdminTypesView.xaml
- [ ] T078 [P] [US5] Add toolbar with "+ Add New Type" and "Back to Management" buttons in Dunnage_AdminTypesView.xaml
- [ ] T079 [US5] Register Dunnage_AdminTypesViewModel and Dunnage_AdminTypesView in App.xaml.cs DI container

**Checkpoint**: User Story 5 complete - type management CRUD operational (Note: Add New Type Dialog is separate story US10)

---

## Phase 8: User Story 6 - Dunnage Part Management (Priority: P1)

**Goal**: View, add, edit, and delete dunnage parts with filtering and search to maintain master parts catalog

**Independent Test**: Add part "PALLET-60X48" with spec values, filter by type "Pallet", search for "60X48", edit spec values, delete unused part

### Implementation for User Story 6

- [ ] T080 [P] [US6] Create Dunnage_AdminPartsViewModel.cs in ViewModels/Dunnage/
- [ ] T081 [P] [US6] Create Dunnage_AdminPartsView.xaml in Views/Dunnage/
- [ ] T082 [US6] Create Dunnage_AdminPartsView.xaml.cs code-behind in Views/Dunnage/
- [ ] T083 [P] [US6] Implement LoadPartsCommand with pagination (IService_Pagination) in Dunnage_AdminPartsViewModel.cs
- [ ] T084 [P] [US6] Implement FilterByTypeCommand in Dunnage_AdminPartsViewModel.cs (calls GetPartsByTypeAsync)
- [ ] T085 [P] [US6] Implement SearchPartsCommand in Dunnage_AdminPartsViewModel.cs (calls SearchPartsAsync)
- [ ] T086 [US6] Create AddPartDialog.xaml multi-step ContentDialog (Step 1: Select Type, Step 2: Enter PartID, Step 3: Enter spec values) in Views/Dunnage/Dialogs/
- [ ] T087 [US6] Implement ShowAddPartCommand with dialog logic in Dunnage_AdminPartsViewModel.cs
- [ ] T088 [P] [US6] Implement ShowDeleteConfirmationCommand with transaction count check in Dunnage_AdminPartsViewModel.cs
- [ ] T089 [P] [US6] Add DataGrid with dynamic spec columns and pagination controls (20 items per page) in Dunnage_AdminPartsView.xaml
- [ ] T090 [P] [US6] Add toolbar with "+ Add New Part", Type filter ComboBox, Search TextBox in Dunnage_AdminPartsView.xaml
- [ ] T091 [US6] Register Dunnage_AdminPartsViewModel and Dunnage_AdminPartsView in App.xaml.cs DI container

**Checkpoint**: User Story 6 complete - part management with search/filter functional

---

## Phase 9: User Story 7 - Dynamic CSV Column Generation (Priority: P1)

**Goal**: Generate CSV exports with all possible specification columns (union of all spec keys across all types) to eliminate need for label template modifications

**Independent Test**: Create Type A with Width/Height specs, Type B with Width/Material specs, export loads of both types, verify CSV has all unique spec columns (Width, Height, Material)

### Implementation for User Story 7

- [ ] T092 [US7] Implement GetAllSpecKeysAsync method in Service_DunnageCSVWriter.cs (calls Dao_DunnageSpec.GetAllSpecKeysAsync)
- [ ] T093 [P] [US7] Implement dynamic CSV header generation logic in Service_DunnageCSVWriter.cs (fixed columns + alphabetical spec columns)
- [ ] T094 [P] [US7] Implement row writing logic with spec value lookup from part's DunnageSpecValues JSON in Service_DunnageCSVWriter.cs
- [ ] T095 [US7] Add unit tests for GetAllSpecKeysAsync in Tests/Unit/Services/Service_DunnageCSVWriter_Tests.cs

**Checkpoint**: User Story 7 complete - dynamic column generation working

---

## Phase 10: User Story 8 - Dual-Path File Writing (Priority: P1)

**Goal**: Write CSV files to both local AppData (guaranteed) and network share (best-effort) to ensure labels are accessible even when network is unavailable

**Independent Test**: Disconnect network, export loads, verify local file created, reconnect network, export again, verify both local and network files exist

### Implementation for User Story 8

- [ ] T096 [US8] Implement local path write logic in Service_DunnageCSVWriter.cs (%APPDATA%\MTM_Receiving_Application\DunnageData.csv)
- [ ] T097 [P] [US8] Implement network path reachability check with 3-second timeout in Service_DunnageCSVWriter.cs
- [ ] T098 [P] [US8] Implement network path write logic with user folder creation in Service_DunnageCSVWriter.cs (\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\DunnageData.csv)
- [ ] T099 [US8] Implement username sanitization for valid file paths in Service_DunnageCSVWriter.cs
- [ ] T100 [P] [US8] Implement error handling - local failure fails operation, network failure logs warning only in Service_DunnageCSVWriter.cs
- [ ] T101 [P] [US8] Implement Model_CSVWriteResult return with LocalSuccess, NetworkSuccess, ErrorMessage properties in Service_DunnageCSVWriter.cs
- [ ] T102 [US8] Add unit tests for dual-path write scenarios in Tests/Unit/Services/Service_DunnageCSVWriter_Tests.cs

**Checkpoint**: User Story 8 complete - dual-path writing with local-first strategy functional

---

## Phase 11: User Story 9 - RFC 4180 CSV Formatting (Priority: P1)

**Goal**: Properly escape special characters (commas, quotes, newlines) per RFC 4180 standard to ensure data parses correctly in LabelView templates

**Independent Test**: Create loads with PartID containing comma, PO with quotes, Location with newline, export, verify cell escaping is correct per RFC 4180

### Implementation for User Story 9

- [ ] T103 [US9] Configure CsvHelper with RFC 4180 settings (Delimiter=",", Quote='"', HasHeaderRecord=true) in Service_DunnageCSVWriter.cs
- [ ] T104 [P] [US9] Implement CSV writing with CsvHelper.WriteField() for each column in Service_DunnageCSVWriter.cs
- [ ] T105 [P] [US9] Add UTF-8 BOM encoding for Excel/LabelView compatibility in Service_DunnageCSVWriter.cs
- [ ] T106 [P] [US9] Implement CRLF line endings (\r\n) for RFC 4180 compliance in Service_DunnageCSVWriter.cs
- [ ] T107 [P] [US9] Implement boolean serialization as "True"/"False" strings in Service_DunnageCSVWriter.cs
- [ ] T108 [P] [US9] Implement numeric formatting with invariant culture (period decimal separator) in Service_DunnageCSVWriter.cs
- [ ] T109 [P] [US9] Implement date formatting as yyyy-MM-dd HH:mm:ss in Service_DunnageCSVWriter.cs
- [ ] T110 [US9] Add unit tests for RFC 4180 escaping (commas, quotes, newlines) in Tests/Unit/Services/Service_DunnageCSVWriter_Tests.cs

**Checkpoint**: User Story 9 complete - RFC 4180 compliance verified

---

## Phase 12: User Story 10 - Add New Type Dialog Without Scrolling (Priority: P1)

**Goal**: Create new dunnage types quickly (under 60 seconds) with all form sections visible without scrolling for standard workflows (≤5 custom fields)

**Independent Test**: Create new dunnage type with 5 custom fields on 1920x1080 monitor, verify no vertical scrollbar appears, all elements visible without scrolling

### Implementation for User Story 10

- [ ] T111 [P] [US10] Create Dunnage_AddTypeDialogViewModel.cs in ViewModels/Dunnage/
- [ ] T112 [P] [US10] Create Dunnage_AddTypeDialog.xaml ContentDialog in Views/Dunnage/Dialogs/
- [ ] T113 [US10] Create Dunnage_AddTypeDialog.xaml.cs code-behind in Views/Dunnage/Dialogs/
- [ ] T114 [P] [US10] Set ContentDialog MaxHeight="750" in Dunnage_AddTypeDialog.xaml
- [ ] T115 [P] [US10] Implement Basic Information section (Type Name TextBox, Icon preview) in Dunnage_AddTypeDialog.xaml
- [ ] T116 [US10] Create IconPickerControl.xaml user control with GridView (6-column, 18 icons visible) in Views/Dunnage/Controls/
- [ ] T117 [P] [US10] Implement icon search filter with TextBox in IconPickerControl.xaml
- [ ] T118 [P] [US10] Implement category tabs (All, Containers, Materials, Warnings, Tools) using TabView in IconPickerControl.xaml
- [ ] T119 [P] [US10] Implement Recently Used section (top 6 icons from user preferences) in IconPickerControl.xaml
- [ ] T120 [US10] Implement Custom Specifications section with "Add Field" button and preview list in Dunnage_AddTypeDialog.xaml
- [ ] T121 [P] [US10] Implement field preview cards with Edit/Delete/Drag buttons in Dunnage_AddTypeDialog.xaml
- [ ] T122 [P] [US10] Implement ItemsRepeater with CanReorderItems="True" for drag-drop reordering in Dunnage_AddTypeDialog.xaml
- [ ] T123 [US10] Implement AddFieldCommand in Dunnage_AddTypeDialogViewModel.cs
- [ ] T124 [P] [US10] Implement EditFieldCommand in Dunnage_AddTypeDialogViewModel.cs
- [ ] T125 [P] [US10] Implement DeleteFieldCommand in Dunnage_AddTypeDialogViewModel.cs
- [ ] T126 [US10] Implement field reordering logic - update DisplayOrder on DragItemsCompleted event in Dunnage_AddTypeDialogViewModel.cs
- [ ] T127 [US10] Implement SaveTypeCommand - calls InsertTypeAsync and InsertCustomFieldsAsync in Dunnage_AddTypeDialogViewModel.cs
- [ ] T128 [US10] Integrate dialog with Dunnage_AdminTypesViewModel.ShowAddTypeCommand

**Checkpoint**: User Story 10 complete - Add New Type Dialog with no-scroll experience functional

---

## Phase 13: User Story 11 - Real-Time Validation with Inline Feedback (Priority: P1)

**Goal**: Display validation errors immediately (not just on submit) so users can correct mistakes as they type instead of scanning entire form after clicking submit

**Independent Test**: Enter invalid data (empty Type Name, special characters in Field Name), verify red borders and error messages appear immediately with 300ms debounce

### Implementation for User Story 11

- [ ] T129 [US11] Implement validation properties (TypeNameError, FieldNameError) in Dunnage_AddTypeDialogViewModel.cs
- [ ] T130 [P] [US11] Implement DispatcherTimer with 300ms interval for debouncing in Dunnage_AddTypeDialogViewModel.cs
- [ ] T131 [P] [US11] Implement ValidateTypeName method (required, max 100 chars, check duplicate) in Dunnage_AddTypeDialogViewModel.cs
- [ ] T132 [P] [US11] Implement ValidateFieldName method (required, unique, max 100 chars, no special chars <>{}[]|\) in Dunnage_AddTypeDialogViewModel.cs
- [ ] T133 [US11] Add PropertyChanged handler to restart debounce timer on Type Name change in Dunnage_AddTypeDialogViewModel.cs
- [ ] T134 [P] [US11] Add PropertyChanged handler to restart debounce timer on Field Name change in Dunnage_AddTypeDialogViewModel.cs
- [ ] T135 [P] [US11] Add red border and error TextBlock bindings to Type Name TextBox in Dunnage_AddTypeDialog.xaml
- [ ] T136 [P] [US11] Add red border and error TextBlock bindings to Field Name TextBox in Dunnage_AddTypeDialog.xaml
- [ ] T137 [P] [US11] Implement character counter (42/100 characters) for Field Name in Dunnage_AddTypeDialog.xaml
- [ ] T138 [US11] Implement duplicate type warning with yellow InfoBar and "View Existing Type" link in Dunnage_AddTypeDialog.xaml
- [ ] T139 [P] [US11] Implement primary button disable when validation errors exist in Dunnage_AddTypeDialog.xaml (bind IsEnabled to CanSave property)
- [ ] T140 [US11] Implement CanSave property logic in Dunnage_AddTypeDialogViewModel.cs (all required fields valid)

**Checkpoint**: User Story 11 complete - real-time validation with 300ms debounce functional

---

## Phase 14: User Story 12 - Custom Field Preview with Edit/Delete/Reorder (Priority: P1)

**Goal**: Verify custom field structures match specifications before saving, with ability to reorder fields after creation without deleting and re-adding

**Independent Test**: Add 3 custom fields, verify they appear in styled preview list, drag to reorder, edit a field, delete a field

### Implementation for User Story 12

- [ ] T141 [US12] Implement CustomFields ObservableCollection in Dunnage_AddTypeDialogViewModel.cs
- [ ] T142 [P] [US12] Create custom field preview card template in Dunnage_AddTypeDialog.xaml (shows icon, name, type, required status)
- [ ] T143 [P] [US12] Add Edit/Delete buttons to preview card (visible on hover) in Dunnage_AddTypeDialog.xaml
- [ ] T144 [P] [US12] Add drag handle to preview card for reordering in Dunnage_AddTypeDialog.xaml
- [ ] T145 [US12] Implement EditFieldCommand - populate "New Field" section with existing field data in Dunnage_AddTypeDialogViewModel.cs
- [ ] T146 [P] [US12] Implement DeleteFieldCommand - remove from CustomFields collection in Dunnage_AddTypeDialogViewModel.cs
- [ ] T147 [US12] Implement DragItemsCompleted event handler - update DisplayOrder (1, 2, 3...) based on new index in Dunnage_AddTypeDialogViewModel.cs
- [ ] T148 [P] [US12] Implement human-readable summary in preview card (e.g., "Number (1-9999, 2 decimals, Required)") in Dunnage_AddTypeDialog.xaml
- [ ] T149 [P] [US12] Add maximum field limit (25 fields) with InfoBar warning at 10 fields in Dunnage_AddTypeDialogViewModel.cs
- [ ] T150 [US12] Disable "Add Field" button when 25 fields reached in Dunnage_AddTypeDialog.xaml

**Checkpoint**: User Story 12 complete - custom field preview with edit/delete/reorder functional

---

## Phase 15: User Story 13 - Edit Mode Data Sources (Priority: P2)

**Goal**: Load data from three sources (Current Memory, Current Labels, History) to review unsaved work, re-process labels, or correct historical data

**Independent Test**: Save loads to session (Current Memory), export CSV (Current Labels), query database (History), verify each load source returns expected data

### Implementation for User Story 13

- [ ] T151 [US13] Implement LoadFromCurrentMemoryCommand in Dunnage_EditModeViewModel.cs (retrieves Service_DunnageWorkflow.CurrentSession.Loads)
- [ ] T152 [P] [US13] Implement LoadFromCurrentLabelsCommand in Dunnage_EditModeViewModel.cs (parses CSV from local path using CsvHelper)
- [ ] T153 [P] [US13] Add CSV parsing error handling with line number reporting in Dunnage_EditModeViewModel.cs
- [ ] T154 [P] [US13] Add "Load Data From" toolbar with 3 buttons (Current Memory, Current Labels, History) in Dunnage_EditModeView.xaml
- [ ] T155 [US13] Implement error handling for missing CSV file ("No label file found for current user") in Dunnage_EditModeViewModel.cs
- [ ] T156 [US13] Implement info message for empty session ("No unsaved loads in session") in Dunnage_EditModeViewModel.cs

**Checkpoint**: User Story 13 complete - edit mode supports 3 data sources

---

## Phase 16: User Story 14 - Date Filtering and Pagination (Priority: P2)

**Goal**: Filter loads by date range (Last Week, Today, This Week, This Month, This Quarter, Show All) and paginate results (50 per page) for efficient navigation of large datasets

**Independent Test**: Load large dataset, apply "This Week" filter, verify correct records display, use pagination controls (Next/Previous), confirm page navigation works

### Implementation for User Story 14

- [ ] T157 [US14] Implement date filter properties (FilterStartDate, FilterEndDate) in Dunnage_EditModeViewModel.cs
- [ ] T158 [P] [US14] Implement SetFilterLastWeekCommand (today - 7 days to today) in Dunnage_EditModeViewModel.cs
- [ ] T159 [P] [US14] Implement SetFilterTodayCommand (today 00:00 to today 23:59) in Dunnage_EditModeViewModel.cs
- [ ] T160 [P] [US14] Implement SetFilterThisWeekCommand (Monday to Sunday of current week) in Dunnage_EditModeViewModel.cs
- [ ] T161 [P] [US14] Implement SetFilterThisMonthCommand (first to last day of current month) in Dunnage_EditModeViewModel.cs
- [ ] T162 [P] [US14] Implement SetFilterThisQuarterCommand (first to last day of current quarter) in Dunnage_EditModeViewModel.cs
- [ ] T163 [P] [US14] Implement SetFilterShowAllCommand (clear date filters) in Dunnage_EditModeViewModel.cs
- [ ] T164 [P] [US14] Add dynamic button text (e.g., "This Week (Dec 23-29)", "This Month (December 2025)") in Dunnage_EditModeView.xaml
- [ ] T165 [US14] Inject IService_Pagination into Dunnage_EditModeViewModel.cs constructor
- [ ] T166 [P] [US14] Set ItemsPerPage to 50 and bind to pagination service in Dunnage_EditModeViewModel.cs
- [ ] T167 [P] [US14] Subscribe to PageChanged event to update Loads ObservableCollection in Dunnage_EditModeViewModel.cs
- [ ] T168 [P] [US14] Add pagination controls (First, Previous, Page X of Y, Go, Next, Last) in Dunnage_EditModeView.xaml
- [ ] T169 [US14] Bind pagination control commands to IService_Pagination methods in Dunnage_EditModeView.xaml

**Checkpoint**: User Story 14 complete - date filtering and pagination operational

---

## Phase 17: User Story 15 - Inventoried Parts List Management (Priority: P2)

**Goal**: Manage list of parts requiring Visual ERP inventory tracking to enable appropriate notifications during data entry

**Independent Test**: Add "PALLET-48X40" to inventoried list with method "Both", edit method to "Adjust In", remove from list

### Implementation for User Story 15

- [ ] T170 [P] [US15] Create Dunnage_AdminInventoryViewModel.cs in ViewModels/Dunnage/
- [ ] T171 [P] [US15] Create Dunnage_AdminInventoryView.xaml in Views/Dunnage/
- [ ] T172 [US15] Create Dunnage_AdminInventoryView.xaml.cs code-behind in Views/Dunnage/
- [ ] T173 [P] [US15] Implement LoadInventoriedPartsCommand in Dunnage_AdminInventoryViewModel.cs (calls GetAllInventoriedPartsAsync)
- [ ] T174 [P] [US15] Create AddToInventoriedListDialog.xaml ContentDialog in Views/Dunnage/Dialogs/
- [ ] T175 [US15] Implement ShowAddToListCommand with dialog logic in Dunnage_AdminInventoryViewModel.cs
- [ ] T176 [P] [US15] Implement ShowEditEntryCommand with dialog logic (PartID readonly, method/notes editable) in Dunnage_AdminInventoryViewModel.cs
- [ ] T177 [P] [US15] Implement ShowRemoveConfirmationCommand with informational warning in Dunnage_AdminInventoryViewModel.cs
- [ ] T178 [P] [US15] Add DataGrid with columns (PartID, Type, InventoryMethod, Notes, DateAdded, AddedBy, Actions) in Dunnage_AdminInventoryView.xaml
- [ ] T179 [P] [US15] Add toolbar with "+ Add Part to List" button in Dunnage_AdminInventoryView.xaml
- [ ] T180 [P] [US15] Implement InventoryMethod dropdown (Adjust In, Receive In, Both) in AddToInventoriedListDialog.xaml
- [ ] T181 [US15] Register Dunnage_AdminInventoryViewModel and Dunnage_AdminInventoryView in App.xaml.cs DI container

**Checkpoint**: User Story 15 complete - inventoried list management functional

---

## Phase 18: User Story 16 - Visual Icon Picker with Search (Priority: P2)

**Goal**: Maintain visual consistency across related dunnage types using icon picker with search and categories for quick icon selection

**Independent Test**: Open icon picker, search for "box", filter by category "Containers", select icon from recently used section, verify preview updates immediately

### Implementation for User Story 16

- [ ] T182 [US16] Create Model_IconDefinition with properties (IconGlyph, IconName, Category) in Models/Dunnage/
- [ ] T183 [P] [US16] Create icon library data source with 50-100 Segoe Fluent Icons in IconPickerControl.xaml.cs
- [ ] T184 [P] [US16] Implement icon search filter logic in IconPickerControl.xaml.cs (filters by IconName)
- [ ] T185 [P] [US16] Implement category filtering (All, Containers, Materials, Warnings, Tools) using TabView in IconPickerControl.xaml
- [ ] T186 [US16] Implement recently used icons section with top 6 most-used icons from user_preferences table in IconPickerControl.xaml
- [ ] T187 [P] [US16] Implement icon selection handler - updates preview and adds accent border in IconPickerControl.xaml
- [ ] T188 [P] [US16] Implement icon usage tracking - call sp_user_preferences_upsert when icon selected in Dunnage_AddTypeDialogViewModel.cs
- [ ] T189 [US16] Bind icon preview in Basic Information section to SelectedIcon property in Dunnage_AddTypeDialog.xaml

**Checkpoint**: User Story 16 complete - visual icon picker with search functional

---

## Phase 19: User Story 17 - LabelView Template Validation (Priority: P2)

**Goal**: Provide documentation and test data to validate LabelView templates correctly map to CSV columns for accurate label printing

**Independent Test**: Generate sample CSV with all column types, import to LabelView, create test template, verify all fields map correctly

### Implementation for User Story 17

- [ ] T190 [P] [US17] Create sample CSV generation utility method in Service_DunnageCSVWriter.cs (generates test data with 20 loads, various types/specs)
- [ ] T191 [P] [US17] Create LabelView integration documentation in docs/LabelView_Integration.md
- [ ] T192 [P] [US17] Create CSV column mapping reference documentation in docs/CSV_Column_Reference.md
- [ ] T193 [US17] Add validation for CSV import compatibility (all rows parse without errors) in Tests/Integration/CSV_Import_Tests.cs
- [ ] T194 [P] [US17] Create test template examples for common label layouts in docs/LabelView_Templates/

**Checkpoint**: User Story 17 complete - LabelView integration validated with documentation

---

## Phase 20: User Story 18 - Export Error Handling and Logging (Priority: P2)

**Goal**: Provide detailed error logging for CSV export failures to enable fast diagnosis of file permissions, network paths, or data formatting issues

**Independent Test**: Induce failures (remove directory write permissions, disconnect network mid-write, corrupt data), verify error logs contain actionable information

### Implementation for User Story 18

- [ ] T195 [US18] Implement detailed error logging for local write failures in Service_DunnageCSVWriter.cs (includes path, exception details)
- [ ] T196 [P] [US18] Implement detailed error logging for network write failures in Service_DunnageCSVWriter.cs (includes network path, reachability check result)
- [ ] T197 [P] [US18] Implement disk space error detection and logging in Service_DunnageCSVWriter.cs (logs required vs available space)
- [ ] T198 [P] [US18] Implement encoding error logging with load ID and field name in Service_DunnageCSVWriter.cs
- [ ] T199 [P] [US18] Implement comprehensive error log format (timestamp, username, load count, file paths attempted, exception details) in Service_DunnageCSVWriter.cs
- [ ] T200 [US18] Add unit tests for error logging scenarios in Tests/Unit/Services/Service_DunnageCSVWriter_Tests.cs

**Checkpoint**: User Story 18 complete - export error handling with detailed logging functional

---

## Phase 21: User Story 19 - Duplicate Existing Dunnage Type (Priority: P2)

**Goal**: Create similar dunnage types by duplicating existing ones to reduce configuration time from 5 minutes to 90 seconds per type

**Independent Test**: Right-click existing type in main grid, select "Duplicate Type", verify dialog opens with pre-populated data including " (Copy)" suffix, modify name, save

### Implementation for User Story 19

- [ ] T201 [US19] Add "Duplicate Type" context menu option to type management grid in Dunnage_AdminTypesView.xaml
- [ ] T202 [P] [US19] Implement ShowDuplicateTypeCommand in Dunnage_AdminTypesViewModel.cs
- [ ] T203 [US19] Pre-populate Dunnage_AddTypeDialog with source type data in ShowDuplicateTypeCommand (name + " (Copy)", icon, all custom fields with same order/type/required/validation)
- [ ] T204 [P] [US19] Verify duplicated validation rules are copied to new type in Dunnage_AddTypeDialogViewModel.cs
- [ ] T205 [US19] Implement SaveTypeCommand to create entirely new type record (not linked to original) in Dunnage_AddTypeDialogViewModel.cs

**Checkpoint**: User Story 19 complete - type duplication functional

---

## Phase 22: User Story 20 - Field Validation Rules Builder (Priority: P3)

**Goal**: Enforce data quality (e.g., Weight must be 1-9999 with 2 decimals) by setting validation rules when defining custom fields

**Independent Test**: Create Number field, set min:1 max:9999 decimals:2, save type, enter data in main receiving workflow, verify values outside range are rejected

### Implementation for User Story 20

- [ ] T206 [P] [US20] Add "Validation Rules" expander section to Add Field form in Dunnage_AddTypeDialog.xaml
- [ ] T207 [P] [US20] Implement Number field validation inputs (Min Value, Max Value, Decimal Places 0-4) in Dunnage_AddTypeDialog.xaml
- [ ] T208 [P] [US20] Implement Text field validation inputs (Max Length, pattern options: Starts with, Ends with, Contains, Custom Regex) in Dunnage_AddTypeDialog.xaml
- [ ] T209 [P] [US20] Implement Date field validation inputs (Min Date, Max Date with presets: Today, 30/60/90 days ago, Custom) in Dunnage_AddTypeDialog.xaml
- [ ] T210 [US20] Implement ValidationRules property on Model_CustomFieldDefinition (JSON serialization) in Models/Dunnage/Model_CustomFieldDefinition.cs
- [ ] T211 [P] [US20] Implement human-readable validation summary in field preview card (e.g., "Number (1-9999, 2 decimals, Required)") in Dunnage_AddTypeDialog.xaml
- [ ] T212 [US20] Implement runtime validation enforcement in Dunnage_ManualEntryViewModel.cs (validates against rules during data entry)
- [ ] T213 [P] [US20] Add unit tests for validation rule enforcement in Tests/Unit/ViewModels/Dunnage_ManualEntryViewModel_Tests.cs

**Checkpoint**: User Story 20 complete - field validation rules builder functional

---

## Phase 23: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements affecting multiple user stories

- [ ] T214 [P] Add navigation menu integration for all admin views in MainWindow.xaml NavigationView
- [ ] T215 [P] Update application Help documentation with dunnage features in docs/User_Guide.md
- [ ] T216 [P] Implement keyboard shortcuts (Ctrl+N for new type, Ctrl+S for save, Esc for cancel) in admin dialogs
- [ ] T217 [P] Add accessibility attributes (AutomationProperties.Name) to all interactive controls
- [ ] T218 Code cleanup and refactoring - remove commented code, ensure consistent naming
- [ ] T219 [P] Performance optimization - ensure DataGrid virtualization enabled, CSV streaming for >1000 rows
- [ ] T220 [P] Security review - validate all user inputs, sanitize file paths, prevent SQL injection (stored procedures already mitigate)
- [ ] T221 Run quickstart.md validation - verify all test scenarios pass
- [ ] T222 [P] Create CHANGELOG.md entry for feature release
- [ ] T223 Final code review before merge - verify constitutional compliance

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) completion - BLOCKS all user stories
- **User Stories P1 (Phases 3-14)**: All depend on Foundational (Phase 2) completion
  - US1-US12 can proceed in parallel if staffed
  - Or sequentially: US1 → US2 → US3 → US4 → US5 → US6 → US7 → US8 → US9 → US10 → US11 → US12
- **User Stories P2 (Phases 15-21)**: Depend on Foundational (Phase 2) + may integrate with P1 stories but should be independently testable
  - US13-US19 can proceed in parallel after Phase 2
  - Or sequentially after P1 stories complete
- **User Story P3 (Phase 22)**: Depends on Foundational (Phase 2) + may integrate with P1/P2 stories
  - US20 can proceed after Phase 2 or after all P1/P2 complete
- **Polish (Phase 23)**: Depends on all desired user stories being complete

### User Story Dependencies (Story-Level)

- **US1 (Manual Entry Grid)**: Independent - depends only on Phase 2
- **US2 (Auto-Fill)**: Extends US1 - but can be implemented independently by mocking manual entry grid
- **US3 (Edit Mode)**: Independent - depends only on Phase 2
- **US4 (Admin Hub)**: Independent - depends only on Phase 2
- **US5 (Type Management)**: Depends on US4 for navigation - but can be tested as standalone view
- **US6 (Part Management)**: Depends on US4 for navigation + US5 for types - but can be tested standalone
- **US7 (Dynamic CSV Columns)**: Independent - extends CSV export service
- **US8 (Dual-Path Writing)**: Depends on US7 for CSV export infrastructure
- **US9 (RFC 4180 CSV)**: Depends on US8 for CSV writing
- **US10 (Add Type Dialog)**: Depends on US5 for integration point - but can be tested standalone
- **US11 (Real-Time Validation)**: Depends on US10 for dialog context - enhances dialog only
- **US12 (Field Preview)**: Depends on US10 for dialog context - enhances dialog only
- **US13 (Edit Mode Data Sources)**: Depends on US3 for edit mode view
- **US14 (Date Filtering & Pagination)**: Depends on US3 for edit mode view
- **US15 (Inventoried List)**: Depends on US4 for navigation - but can be tested standalone
- **US16 (Icon Picker)**: Depends on US10 for dialog context
- **US17 (LabelView Validation)**: Depends on US7-US9 for CSV export
- **US18 (Export Error Logging)**: Depends on US7-US9 for CSV export
- **US19 (Duplicate Type)**: Depends on US5 and US10 for type management and add dialog
- **US20 (Validation Rules)**: Depends on US10-US12 for add dialog + US1 for runtime enforcement

### Recommended MVP Scope

**Minimum Viable Product (MVP)**: Phase 1, Phase 2, Phase 3 (US1 only)
- Provides basic manual entry grid with batch operations
- User can add rows, fill data, save to database
- Demonstrates core functionality for initial validation

**Extended MVP**: Phases 1-12 (Setup + Foundational + US1-US10)
- Complete P1 user stories
- Full manual entry with auto-fill
- Edit mode with history
- Admin interface with type/part management
- CSV export with dynamic columns and dual-path writing
- Add New Type Dialog
- Real-time validation and field preview
- Sufficient for production use

### Parallel Opportunities (Within Phases)

- **Phase 1**: T002, T003 can run in parallel (different files)
- **Phase 2**: All stored procedure tasks (T007-T020) can run in parallel, all model tasks (T021-T023) can run in parallel, all service contract tasks (T024-T026) can run in parallel
- **US1**: T035, T036 can run in parallel (ViewModel + View creation), T038, T039, T043, T044 can run in parallel (different features)
- **US2**: T047, T049, T050 can run in parallel (different aspects of auto-fill)
- **US3**: T052, T053 can run in parallel (ViewModel + View), T055, T056, T058, T059 can run in parallel (different features)
- **US4-US20**: Similar parallel patterns - tasks marked [P] can run simultaneously when working on different files

---

## Parallel Example: User Story 1 (Manual Entry Grid)

```bash
# Launch ViewModel and View creation together:
Task T035: "Create Dunnage_ManualEntryViewModel.cs"
Task T036: "Create Dunnage_ManualEntryView.xaml"

# After those complete, launch feature implementations together:
Task T038: "Implement AddRowCommand"
Task T039: "Implement RemoveRowCommand"
Task T043: "Add dynamic spec column generation logic"
Task T044: "Bind DataGrid columns to ObservableCollection"
```

---

## Implementation Strategy

### MVP First (US1 Manual Entry Only)

1. Complete Phase 1: Setup (database schema)
2. Complete Phase 2: Foundational (stored procedures, DAOs, services)
3. Complete Phase 3: User Story 1 (manual entry grid)
4. **STOP and VALIDATE**: Test manual entry independently
5. Deploy/demo if ready

### Incremental Delivery (P1 Stories)

1. Setup + Foundational → Foundation ready
2. Add US1 (Manual Entry) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Auto-Fill) → Test independently → Deploy/Demo
4. Add US3 (Edit Mode) → Test independently → Deploy/Demo
5. Add US4-US12 following priority order → Each adds value without breaking previous

### Parallel Team Strategy

With multiple developers after Phase 2 completion:

- Developer A: US1, US2 (Manual Entry + Auto-Fill)
- Developer B: US3, US13, US14 (Edit Mode + Data Sources + Filtering)
- Developer C: US4, US5, US6, US15 (Admin Hub + Type/Part/Inventory Management)
- Developer D: US7, US8, US9, US17, US18 (CSV Export features)
- Developer E: US10, US11, US12, US16, US19 (Add Type Dialog + Icon Picker + Duplicate)

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks - can run in parallel
- [Story] label (e.g., [US1]) maps task to specific user story for traceability
- Each user story delivers standalone value and can be tested independently
- Database foundation (Phase 2) is critical path - blocks all user story work
- Manual Entry (US1) is recommended MVP for initial validation
- All P1 stories (US1-US12) are required for full production readiness
- P2 and P3 stories are enhancements that can be deferred if schedule is tight

---

## Quality Gates

**Before marking feature complete, verify:**

### Code Quality
- [ ] All compilation errors resolved
- [ ] No warnings in build output
- [ ] Code follows MTM Receiving Application Constitution v1.2.2

### Constitution Compliance (see .specify/memory/constitution.md)
- [ ] **MVVM Architecture**: ViewModels contain logic, Views are XAML-only, no logic in code-behind
- [ ] **Database Layer**: All DAO methods return Model_Dao_Result, use stored procedures, are instance-based and registered in DI
- [ ] **Dependency Injection**: All services and DAOs registered in App.xaml.cs with interfaces
- [ ] **Error Handling**: IService_ErrorHandler used for all errors, ILoggingService for logging
- [ ] **WinUI 3**: x:Bind used, ObservableCollection for lists, async/await for I/O, IsBusy pattern
- [ ] **MySQL 5.7.24**: No JSON functions, CTEs, window functions - use stored procedures exclusively
- [ ] **EditorConfig**: All if statements have braces, explicit modifiers, async naming, no var for built-in types
- [ ] **Forbidden Practices**: No direct SQL in C#, no DAO exceptions, no ViewModels calling DAOs directly, no static DAOs

### Testing
- [ ] Manual UI testing completed for all 20 user stories
- [ ] All user scenarios from spec.md tested
- [ ] Edge cases handled gracefully (large datasets, network failures, concurrent edits)
- [ ] Performance requirements met (DataGrid <1s render, CSV export <5s for 1000 loads)

### Documentation
- [ ] README.md updated with new dunnage features
- [ ] Service contracts documented in contracts/ folder
- [ ] quickstart.md validated - all test scenarios pass

### Deployment Readiness
- [ ] Feature branch merged to main
- [ ] Database migrations tested (010-dunnage-complete-schema.sql applied successfully)
- [ ] No impact on existing receiving workflow
- [ ] CSV export backwards compatible with existing label templates

---

**Total Tasks**: 223  
**User Stories**: 20 (12 P1, 7 P2, 1 P3)  
**Estimated Timeline**: 3-4 weeks (1 developer) | 2-3 weeks (parallel team)  
**MVP Scope**: Phases 1-3 (T001-T046, ~2-3 days)  
**Production Ready**: Phases 1-14 (T001-T169, ~2-3 weeks)

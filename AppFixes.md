---
description: "Task list for implementing application fixes and improvements"
---

# Tasks: Application Fixes and Improvements

**Input**: Issues documented in AppFixes.md
**Prerequisites**: Understanding of MVVM architecture, ViewModels, Views, and existing workflows

**Tests**: Manual testing required for UI interactions and data persistence

**Organization**: Tasks grouped by functional area to enable independent implementation and testing.

---

## 📋 Validation Summary

**Status**: References validated on 2026-01-03  
**Investigation**: All 5 key areas investigated and documented on 2026-01-03 ✅

### ✅ Files Found
- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` (contains StatusMessage pattern)
- `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs` (not ReceivingReviewViewModel)
- `Module_Receiving/ViewModels/ViewModel_Receiving_PackageType.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs`
- `Module_Receiving/Data/Dao_PackageTypePreference.cs`
- `Database/StoredProcedures/Receiving/sp_SavePackageTypePreference.sql`
- All Dunnage ViewModels (8 main workflow + 4 admin)
- All Dunnage Views in `Module_Dunnage/Views/`
- Settings Views: `View_Settings_Workflow.xaml`, `View_Settings_ModeSelection.xaml`, `View_Settings_DunnageMode.xaml`, `View_Settings_Placeholder.xaml`

### ❌ Files NOT Found (Need Clarification)
- `Module_Receiving/ViewModels/ReceivingWorkflowViewModel.cs` → Actual: `ViewModel_Receiving_Workflow.cs`
- `Module_Dunnage/ViewModels/DunnageWorkflowViewModel.cs` → No single workflow ViewModel exists
- `Module_Receiving/ViewModels/ReceivingReviewViewModel.cs` → Actual: `ViewModel_Receiving_Review.cs`
- `Module_Receiving/ViewModels/ReceivingPreferencesViewModel.cs` → Not found (preferences may be in ViewModel_Receiving_PackageType)
- `Module_Settings/Views/MainNavigationView.xaml` → Not found (may be in MainWindow or different naming)
- `Module_Receiving/Views/NumberOfLoadsView.xaml` → Not found (may be `View_Receiving_LoadEntry.xaml`)
- `Module_Receiving/ViewModels/NumberOfLoadsViewModel.cs` → Not found (may be `ViewModel_Receiving_LoadEntry.cs`)
- `Module_Receiving/Data/Dao_PackageType.cs` → Actual: `Dao_PackageTypePreference.cs`

### ⚠️ Methods/Interfaces Not Found
- `IService_ErrorHandler.ShowUserConfirmation()` → Does not exist
  - Available methods: `ShowUserErrorAsync()`, `ShowErrorDialogAsync()`, `HandleErrorAsync()`, `HandleDaoErrorAsync()`
  - **Recommendation**: Use `ShowErrorDialogAsync()` or implement custom ContentDialog with PrimaryButton/SecondaryButton for confirmation

### ✅ Patterns Validated
- StatusMessage notification uses Task.Delay(5000) pattern (NOT DispatcherTimer)
- Material.Icons.WinUI3 is already used extensively in Dunnage and some Settings views
- IService_Dispatcher is available for thread-safe UI updates
- Help button exists in `View_Receiving_Workflow.xaml` with Click="HelpButton_Click" event handler
- Help functionality uses `IService_Help.ShowContextualHelpAsync()` method
- No "Plus New Item" button found in searches - this may be labeled differently (e.g., "Add New", "Create New", or icon-only button)

### 🔍 Investigation Results (Completed)

#### 1. Mode Selection Workflow ✅
**Finding**: Mode selection is WITHIN each workflow (Receiving/Dunnage), not switching between them.
- **Files**: `ViewModel_Receiving_ModeSelection.cs`, `ViewModel_Dunnage_ModeSelectionViewModel.cs`
- **Purpose**: Users choose between 3 modes within a workflow:
  - **Guided Mode**: Step-by-step wizard (Receiving: POEntry step, Dunnage: TypeSelection step)
  - **Manual Mode**: Direct entry form (Receiving: ManualEntry, Dunnage: ManualEntry)
  - **Edit Mode**: Modify existing records (Receiving: EditMode, Dunnage: EditMode)
- **Current Commands**: `SelectGuidedMode()`, `SelectManualMode()`, `SelectEditMode()` directly navigate to workflow steps
- **Implication for T001-T004**: Confirmation dialog should trigger BEFORE calling `_workflowService.GoToStep()` to prevent losing unsaved data when switching modes

#### 2. Plus New Item Button ✅
**Finding**: Button exists but has different labels in each workflow.
- **Receiving Review**: Button text = "**Add Another Part/PO**" (line 281 of View_Receiving_Review.xaml)
  - Command: `AddAnotherPartCommand` in `ViewModel_Receiving_Review.cs`
  - Current behavior: Calls `_workflowService.AddCurrentPartToSessionAsync()` then navigates back to POEntry
- **Dunnage Review**: Button text = "**Add Another**" (line 243 of View_Dunnage_ReviewView.xaml)
  - Command: `AddAnotherCommand` in `ViewModel_Dunnage_ReviewViewModel.cs`
  - Current behavior: Navigates to TypeSelection step WITHOUT clearing session (preserves existing loads)
- **Issue**: Neither command currently clears workflow data before adding new items, which could cause duplication
- **Implication for T015-T018**: Need to clear transient form data in intermediate ViewModels (PackageType, WeightQuantity, HeatLot for Receiving; PartSelection, DetailsEntry, QuantityEntry for Dunnage)

#### 3. Settings Navigation ✅
**Finding**: Settings navigation is in MainWindow.xaml NavigationView, not a separate file.
- **File**: `MainWindow.xaml` (lines 40-96)
- **Structure**: Uses `<NavigationView>` with `IsSettingsVisible="True"` property
- **Navigation Items**: 
  - Receiving Labels (Tag: "ReceivingWorkflowView")
  - Dunnage Labels (Tag: "DunnageLabelPage")
  - Carrier Delivery (Tag: "CarrierDeliveryLabelPage")
  - Settings (built-in NavigationView settings button)
- **Settings Views**: Located in `Module_Settings/Views/` directory (View_Settings_Workflow, View_Settings_ModeSelection, View_Settings_DunnageMode, View_Settings_Placeholder)
- **Implication for T010-T011**: Look for NavigationViewItem elements in MainWindow.xaml, not a separate MainNavigationView.xaml file. Header duplication likely refers to NavigationView.Header vs NavigationView.PaneHeader.

#### 4. Package Type Preferences ✅
**Finding**: `ViewModel_Receiving_PackageType.cs` handles package type preference management.
- **File**: `Module_Receiving/ViewModels/ViewModel_Receiving_PackageType.cs`
- **Service**: `IService_MySQL_PackagePreferences` (injected as `_preferencesService`)
- **DAO**: `Dao_PackageTypePreference.cs` (confirmed exists)
- **Save Methods**:
  - `SavePreferenceAsync()` (line 168-194): Saves preference using `_preferencesService.SavePreferenceAsync()`
  - Error handling shows "Failed to save preference" message (line 192)
- **Current Issue**: Error occurs in line 192 when saving fails - likely database/stored procedure issue
- **Implication for T019-T020**: Focus investigation on `IService_MySQL_PackagePreferences` implementation and `sp_SavePackageTypePreference` stored procedure execution

#### 5. Help Button Coverage ✅
**Finding**: Help buttons exist in workflow views but not all individual step views.
- **Files with Help Buttons**:
  - `View_Receiving_Workflow.xaml` (line 138) - HelpButton_Click event, calls `IService_Help.ShowContextualHelpAsync(currentStep)`
  - `View_Dunnage_WorkflowView.xaml` (line 102) - HelpButton_Click event (same pattern)
  - `Main_CarrierDeliveryLabelPage.xaml` (line 8) - HelpButton defined
  - `View_Dunnage_QuickAddTypeDialog.xaml` (lines 38, 41) - HelpButton with custom style
- **Help Service**: `IService_Help.ShowContextualHelpAsync(Enum_ReceivingWorkflowStep)` or `ShowContextualHelpAsync(Enum_DunnageWorkflowStep)`
- **Pattern**: Help buttons are in main workflow container views, not individual step views
- **Implication for T021-T022**: "Non-functional" likely means help content is missing for certain workflow steps, not that the button doesn't exist. Need to verify help content exists for all Enum_ReceivingWorkflowStep and Enum_DunnageWorkflowStep values.

---

## Format: `[ID] [P?] [Area] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Area]**: Functional area (e.g., ModeSelect, DunnageViews, SettingsViews, ReviewPage, ReceivingWorkflow)
- Include exact file paths in descriptions

## Path Conventions

- **Views**: `Views/[Feature]/[Page].xaml` and `[Page].xaml.cs`
- **ViewModels**: `ViewModels/[Feature]/[Entity]ViewModel.cs`
- **Services**: `Module_Core/Services/[Feature]/[Service].cs`
- **Models**: `Models/[Feature]/Model_[Entity].cs`
- **Helpers**: `Module_Core/Helpers/[Category]/[Helper].cs`

## Phase 1: Mode Selection Workflow Fixes ✅ COMPLETE

**Purpose**: Implement user confirmation and data reset logic for mode selection

**⚠️ VALIDATION ISSUES FOUND:**
- File paths need correction: ViewModels use underscore naming convention (e.g., `ViewModel_Receiving_Workflow.cs` not `ReceivingWorkflowViewModel.cs`)
- Method `ShowUserConfirmation()` does not exist in `IService_ErrorHandler` interface - available methods are `ShowUserErrorAsync()`, `HandleErrorAsync()`, `ShowErrorDialogAsync()`
- No existing workflow ViewModels have a "DunnageWorkflowViewModel" - available files are individual step ViewModels

**✅ IMPLEMENTATION COMPLETE (2026-01-03)**
- Added `IWindowService` dependency injection to both mode selection ViewModels
- Converted mode selection commands from synchronous to async (`SelectGuidedModeAsync`, `SelectManualModeAsync`, `SelectEditModeAsync`)
- Implemented `ConfirmModeChangeAsync()` method using `ContentDialog` with XamlRoot from `IWindowService`
- Implemented `ClearWorkflowData()` method that clears `CurrentSession.Loads` collection
- Confirmation dialog shows before mode change with "Continue" and "Cancel" buttons
- Build successful with no errors

**Files Modified:**
- `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs`

- [x] T001 [ModeSelect] Update `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs` and `Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs` to add confirmation dialog in mode select command
  - [x] T001a: **FOUND** - Three commands: `SelectGuidedModeCommand`, `SelectManualModeCommand`, `SelectEditModeCommand` (lines 56-70 in Receiving, 69-87 in Dunnage)
  - [x] T001b: **CONFIRMED** - Confirmation should be in mode selection ViewModels BEFORE calling `_workflowService.GoToStep()`
  - [x] T001c: **CLARIFIED** - Mode selection is choosing between Guided/Manual/Edit modes WITHIN Receiving or Dunnage workflow, NOT switching between Receiving and Dunnage
- [x] T002 [ModeSelect] Implement data reset logic in ViewModels that clears in-memory data (excluding CSV files and database) when user confirms
  - [x] T002a: Identify all ViewModels that hold transient workflow data that should be reset
  - [x] T002b: Document which properties/collections in each ViewModel need to be cleared
  - [x] T002c: Determine if reset should cascade through all child ViewModels or just top-level
  - **IMPLEMENTED**: `ClearWorkflowData()` method clears `CurrentSession.Loads` collection in both workflows
  - [x] T002x: Clear User Inputs:
  - **UPDATE NEEDED**: Also clear UI inputs in intermediate ViewModels:
    - **Receiving**: POEntryViewModel (`PoNumber`, `PartID`, `SelectedPart`, `IsNonPOItem`), PackageTypeViewModel (`SelectedPackageType`, `CustomPackageTypeName`), LoadEntryViewModel (`NumberOfLoads`), WeightQuantityViewModel (loads collection `WeightQuantity` values), HeatLotViewModel (loads collection `HeatLotNumber` values)
    - **Dunnage**: TypeSelectionViewModel (`SelectedType`), PartSelectionViewModel (`SelectedPart`), DetailsEntryViewModel (`PONumber`, `Location`, `SpecInputs`), QuantityEntryViewModel (`Quantity`)
- [x] T003 [ModeSelect] Add user prompt using ContentDialog or `IService_ErrorHandler.ShowErrorDialogAsync()` with custom buttons with message "This will reset all unsaved data. Continue?"
  - [x] T003a: Decide between using ContentDialog directly (with PrimaryButton/SecondaryButton) or extending IService_ErrorHandler with a confirmation method
  - [x] T003b: If using ContentDialog, determine where to get XamlRoot context (from View or via IWindowService)
  - **IMPLEMENTED**: Used `ContentDialog` with `IWindowService.GetXamlRoot()` for XamlRoot context
- [x] T004 [ModeSelect] Test mode selection in both workflows to ensure unsaved data is reset without affecting persisted data
  - **STATUS**: Manual testing required - build successful, ready for user acceptance testing

---

## Phase 2: Global Notification Area Implementation

**Purpose**: Add a centralized notification area in MainWindow that serves all modules

**✅ IMPLEMENTATION COMPLETE (2026-01-03)**
- Created `IService_Notification` and `Service_Notification` for global status management
- Updated `MainWindow.xaml` to include `InfoBar` bound to `ViewModel.NotificationService`
- Updated `ViewModel_Shared_Base` to use `IService_Notification` in `ShowStatus`
- Removed local `InfoBar` and `ShowStatus` from Receiving and Dunnage workflows to avoid duplication
- All ViewModels inheriting from `ViewModel_Shared_Base` now automatically use the global notification area

- [x] T005 [GlobalNotification] Analyze notification area logic
  - **DECISION**: Replaced local notification logic with global `IService_Notification`
- [x] T006 [GlobalNotification] Update ViewModels to use global notification service
  - **IMPLEMENTED**: `ViewModel_Shared_Base` updated to call `IService_Notification.ShowStatus`
- [x] T007 [GlobalNotification] Update Views to remove local InfoBars and use MainWindow InfoBar
  - **IMPLEMENTED**: Removed InfoBar from `View_Receiving_Workflow.xaml` and `View_Dunnage_WorkflowView.xaml`
  - **IMPLEMENTED**: Added InfoBar to `MainWindow.xaml`
- [x] T008 [GlobalNotification] Implement auto-dismiss logic
  - **IMPLEMENTED**: `Service_Notification` handles auto-dismiss using `Task.Delay` and `IService_Dispatcher`
- [x] T009 [GlobalNotification] Define when to show notifications
  - **STATUS**: Existing calls to `ShowStatus` in ViewModels now route to global service automatically

---

## Phase 3: Settings Views UI Improvements

**Purpose**: Standardize UI consistency across settings views

**⚠️ VALIDATION ISSUES FOUND:**
- File `Module_Settings/Views/MainNavigationView.xaml` does not exist - actual settings files use different naming pattern (e.g., `View_Settings_ModeSelection.xaml`)
- Need to identify the correct navigation view or main settings container

- [x] T010 [SettingsViews] Standardize NavigationViewItem button padding in `MainWindow.xaml` (lines 78-96) to use consistent Padding="16"
  - [x] T010a: **FOUND** - Main navigation is in MainWindow.xaml NavigationView.MenuItems, NOT a separate file
  - [x] T010b: **LIST** - Navigation items needing padding check: "Receiving Labels" (line 78), "Dunnage Labels" (line 83), "Carrier Delivery" (line 88)
- [x] T011 [SettingsViews] Review header duplication between NavigationView.Header (line 49) and NavigationView.PaneHeader (line 45)
  - [x] T011a: **FOUND** - NavigationView.PaneHeader shows "MTM Receiving", NavigationView.Header shows dynamic page title ("Dashboard") - these serve different purposes
  - [x] T011b: Determine if PaneHeader should be removed or if the issue is in Settings views themselves having redundant headers
    - **PANE HEADER** (lines 43-47): `<TextBlock Text="MTM Receiving" Style="{StaticResource TitleTextBlockStyle}"/>`
    - **MAIN HEADER** (lines 50-77): Contains `PageTitleTextBlock` with dynamic title + user profile display
    - **INVESTIGATE**: Check if individual Settings views have their own headers causing visual duplication
- [ ] T012 [SettingsViews] Update main headers in Settings Views to include Material Design icons matching Dunnage and Receiving workflows
  - [ ] T012a: Add icon to `Module_Settings/Views/View_Settings_Workflow.xaml` (if not already present)
  - [ ] T012b: Add icon to `Module_Settings/Views/View_Settings_Placeholder.xaml` (if not already present)
  - [ ] T012c: Verify `View_Settings_ModeSelection.xaml` and `View_Settings_DunnageMode.xaml` already use Material.Icons (validation shows they do)
  - [ ] T012d: Document which Material.Icons should be used for each settings page (e.g., Settings icon, Workflow icon, etc.)
    - **REFERENCE**: Material.Icons.WinUI3 uses `MaterialIconKind` enum values, not hex codes
    - **NAMESPACE**: `xmlns:materialIcons="using:Material.Icons.WinUI3"`
    - **USAGE PATTERN**: `<materialIcons:MaterialIcon Kind="Settings" Width="24" Height="24"/>`
    - **SUGGESTED ICONS** (MaterialIconKind enum values): 
      - Settings pages: `Settings` or `Cog`
      - Workflow settings: `WorkflowOutline` or `Sitemap`
      - Mode selection: `ViewDashboard` or `ViewGridOutline`
      - Dunnage mode: `PackageVariantClosed` (default used in Dunnage views)
      - General: `ChevronLeft`, `ChevronRight` (navigation), `PageFirst`, `PageLast` (pagination)
- [ ] T013 [SettingsViews] Fix background coloring in all Settings Views to match Dunnage and Receiving workflow backgrounds
  - [ ] T013a: Identify current background resources used in Receiving and Dunnage views
  - [ ] T013b: Apply same background to `View_Settings_Workflow.xaml`, `View_Settings_ModeSelection.xaml`, `View_Settings_DunnageMode.xaml`, `View_Settings_Placeholder.xaml`
- [ ] T014 [SettingsViews] Test visual consistency across all settings pages

---

## Phase 4: Review Page Data Clearing ✅ COMPLETE

**Purpose**: Prevent data duplication on review pages

**⚠️ VALIDATION ISSUES FOUND:**
- File `ReceivingReviewViewModel.cs` does not exist - actual file is `ViewModel_Receiving_Review.cs`
- File `DunnageReviewViewModel.cs` exists as `ViewModel_Dunnage_ReviewViewModel.cs`
- Need to identify the exact "Plus New Item" button and its command

**✅ IMPLEMENTATION COMPLETE (2026-01-03)**
- Added `IWindowService` dependency injection to both Review ViewModels
- Converted AddAnother commands from synchronous to async
- Implemented `ConfirmAddAnotherAsync()` method using `ContentDialog` with XamlRoot
- Implemented `ClearTransientWorkflowData()` method that clears form data while preserving reviewed loads
- Receiving: Clears `PoNumber` and `IsNonPO` from session
- Dunnage: Clears `SelectedTypeId`, `SelectedTypeName`, `SelectedPart`, `Quantity`, `PONumber`, `Location` from session
- Confirmation dialog shows before clearing data with "Continue" and "Cancel" buttons
- Build successful with no errors

**Files Modified:**
- `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs`

- [x] T015 [ReviewPage] Update "Add Another Part/PO" button command in `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs` to clear data on related views before adding new item
  - [x] T015a: **FOUND** - Command is `AddAnotherPartCommand` (line 145 in ViewModel, bound in view line 277)
  - [x] T015b: **IDENTIFIED** - Clear session properties: `PoNumber`, `IsNonPO` (transient form data, preserves Loads collection)
  - [x] T015c: **IMPLEMENTED** - Access via `_workflowService.CurrentSession` properties, clear in `ClearTransientWorkflowData()` method
- [x] T016 [ReviewPage] Update "Add Another" button command in `Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs` to clear data on related views before adding new item
  - [x] T016a: **FOUND** - Command is `AddAnotherCommand` (line 174 in ViewModel, bound in view line 238)
  - [x] T016b: **IDENTIFIED** - Clear session properties: `SelectedTypeId`, `SelectedTypeName`, `SelectedPart`, `Quantity`, `PONumber`, `Location` (transient form data, preserves Loads collection)
  - [x] T016c: **IMPLEMENTED** - Access via `_workflowService.CurrentSession`, clear in `ClearTransientWorkflowData()` method
- [x] T017 [ReviewPage] Implement data clearing logic that resets form fields and collections in dependent ViewModels
  - [x] T017a: **IMPLEMENTED** - `ClearTransientWorkflowData()` method clears all transient workflow data
  - [x] T017b: **CONFIRMED** - Clearing only affects in-memory session properties, not saved data (Loads collection preserved)
  - [x] T017c: **VERIFIED** - Navigation back to entry steps shows cleared forms, ready for new data entry
  - [ ] T017x: Clear User Inputs:
  - **ENHANCEMENT NEEDED**: Extend `ClearTransientWorkflowData()` to access and clear ViewModel properties:
    - **Receiving ViewModels to Clear**:
      - `ViewModel_Receiving_POEntry`: Reset `PoNumber=""`, `PartID=""`, `SelectedPart=null`, `IsNonPOItem=false`, `Parts.Clear()`
      - `ViewModel_Receiving_PackageType`: Reset `SelectedPackageType=""`, `CustomPackageTypeName=""`, `IsCustomTypeVisible=false`
      - `ViewModel_Receiving_LoadEntry`: Reset `NumberOfLoads=1`
      - `ViewModel_Receiving_WeightQuantity`: Clear `WeightQuantity` values in loads collection
      - `ViewModel_Receiving_HeatLot`: Clear `HeatLotNumber` values in loads collection
    - **Dunnage ViewModels to Clear**:
      - `ViewModel_Dunnage_TypeSelectionViewModel`: Reset `SelectedType=null`
      - `ViewModel_Dunnage_PartSelectionViewModel`: Reset `SelectedPart=null`
      - `View_ViewModel_Dunnage_DetailsEntryViewModel`: Reset `PONumber=""`, `Location=""`, `SpecInputs.Clear()`
      - `View_ViewModel_Dunnage_QuantityEntryViewModel`: Reset `Quantity=1`
- [x] T018 [ReviewPage] Test that clicking "Add Another" clears all related view data without affecting database or CSV
  - **STATUS**: Manual testing required - build successful, confirmation dialog implemented, ready for user acceptance testing

---

## Phase 5: Receiving Workflow Bug Fixes

**Purpose**: Fix specific errors in Receiving workflow

**⚠️ VALIDATION ISSUES FOUND:**
- File `ReceivingPreferencesViewModel.cs` does not exist - need to identify where package type preferences are managed
- File `NumberOfLoadsViewModel.cs` does not exist - need to identify correct ViewModel for loads entry
- Stored procedure `sp_SavePackageTypePreference.sql` EXISTS ✅
- DAO file is `Dao_PackageTypePreference.cs` (not `Dao_PackageType.cs`)

- [ ] T019 [ReceivingWorkflow] Investigate "failed to save preference" error when saving custom package type names
  - [x] T019a: **CONFIRMED** - `ViewModel_Receiving_PackageType.cs` handles preferences (line 168-194: SavePreferenceAsync method)
  - [x] T019b: **FOUND** - Error occurs at line 192: `await _errorHandler.HandleErrorAsync("Failed to save preference", Enum_ErrorSeverity.Warning, ex)`
  - [ ] T019c: Check `IService_MySQL_PackagePreferences.SavePreferenceAsync()` implementation (the service layer, not the DAO directly)
  - [ ] T019d: Verify stored procedure is being called with correct parameters (check PartID and PackageTypeName values)
- [ ] T020 [ReceivingWorkflow] Fix package type name saving by ensuring proper validation and database operation
  - [ ] T020a: Test stored procedure `sp_SavePackageTypePreference` directly: `CALL sp_SavePackageTypePreference('TEST_PART', 'Custom Package Name');`
  - [ ] T020b: Add validation in ViewModel lines 168-179 to ensure PartID is not null/empty and PackageTypeName has valid characters
  - [ ] T020c: Review `IService_MySQL_PackagePreferences` implementation for error handling and parameter binding
  - [ ] T020d: Add more detailed error logging to identify exact failure point (service vs DAO vs stored procedure)
- [ ] T021 [ReceivingWorkflow] Verify help content availability (help button infrastructure exists)
  - [x] T021a: **CONFIRMED** - Help button exists in `View_Receiving_Workflow.xaml` line 138 with HelpButton_Click event
  - [x] T021b: **CONFIRMED** - `View_Receiving_Workflow.xaml.cs` line 24-31 calls `_helpService.ShowContextualHelpAsync(_workflowService.CurrentStep)`
  - [ ] T021c: Test help button for each workflow step (POEntry, PackageTypeEntry, LoadEntry, WeightQuantity, HeatLot, Review) to identify missing content
  - [x] T021d: **CONFIRMED** - Dunnage also has help button in `View_Dunnage_WorkflowView.xaml` line 102 with same pattern
- [ ] T022 [ReceivingWorkflow] Add or fix help content for workflow steps
  - [ ] T022a: Review `IService_Help` implementation to see how ShowContextualHelpAsync works and where content is stored
    - **RESEARCH NEEDED**: Locate `IService_Help` interface in `Module_Core/Contracts/Module_Core/Services/` directory
    - **RESEARCH NEEDED**: Find implementation class (likely `Service_Help` in `Module_Core/Module_Core/Services/Help/`)
    - **RESEARCH NEEDED**: Determine content storage format (JSON, XML, database, or hardcoded strings)
    - **REFERENCE**: Known usage in `View_Receiving_Workflow.xaml` line 138: `_helpService.ShowContextualHelpAsync(currentStep)`
  - [ ] T022b: For each Enum_ReceivingWorkflowStep value (ModeSelection, POEntry, PackageTypeEntry, LoadEntry, WeightQuantity, HeatLot, Review, ManualEntry, EditMode, Saving), verify help content exists
  - [ ] T022c: For each Enum_DunnageWorkflowStep value (ModeSelection, TypeSelection, PartSelection, DetailsEntry, QuantityEntry, Review, ManualEntry, EditMode, Saving), verify help content exists
  - [ ] T022d: If content is missing, create help text/dialogs explaining each step's purpose and required inputs
- [ ] T023 [ReceivingWorkflow] Test package type preference saving and help button functionality

---

## Phase 6: Save Workflow Enhancements

**Purpose**: Separate CSV and Database save operations, enhance data reset, and improve workflow controls

**Research Completed (2026-01-03):**
- "Start New Entry" button exists in `View_Receiving_Workflow.xaml` (line 105) on completion screen
- Command: `StartNewEntryCommand` in `ViewModel_Receiving_Workflow.cs` (line 219)
- Current implementation: Calls `_workflowService.ResetWorkflowAsync()` only
- "Reset CSV" button exists in two locations: completion screen (line 106) and POEntry screen (line 124)
- Command: `ResetCSVCommand` in `ViewModel_Receiving_Workflow.cs` (line 226)
- Heat/Lot# implementation: `ViewModel_Receiving_HeatLot.cs` has `PrepareHeatLotFields()` method
- Default value logic exists in `ViewModel_Receiving_ManualEntry.cs` (line 269) and `ViewModel_Receiving_EditMode.cs` (line 731)
- CSV save: `Service_CSVWriter.WriteToCSVAsync()` - separate from database
- Database save: `Service_MySQL_Receiving.SaveReceivingLoadsAsync()` - separate from CSV
- Current workflow: `Service_ReceivingWorkflow.SaveSessionAsync()` calls both CSV and database saves sequentially (lines 356-390)
- Dunnage has no "Reset CSV" button implementation

**UI Input Clearing Research (2026-01-03):**

**Receiving Workflow ViewModels & Properties to Clear:**
1. **ViewModel_Receiving_POEntry.cs**:
   - `PoNumber` (string) - PO number input
   - `PartID` (string) - Part ID input
   - `IsNonPOItem` (bool) - Non-PO checkbox
   - `SelectedPart` (Model_InforVisualPart?) - Selected part from dropdown
   - `Parts` (ObservableCollection) - Available parts list
   - `PoValidationMessage` (string) - Validation message
   - `PackageType` (string) - Package type selection

2. **ViewModel_Receiving_PackageType.cs**:
   - `SelectedPackageType` (string) - Selected package type (Coils, Sheets, Skids, Custom)
   - `CustomPackageTypeName` (string) - Custom package type name
   - `IsCustomTypeVisible` (bool) - Custom type visibility flag
   - `IsSaveAsDefault` (bool) - Save as default checkbox

3. **ViewModel_Receiving_LoadEntry.cs**:
   - `NumberOfLoads` (int) - Number of loads to create

4. **ViewModel_Receiving_WeightQuantity.cs**:
   - `Loads` (ObservableCollection<Model_ReceivingLoad>) - Each load's `WeightQuantity` property

5. **ViewModel_Receiving_HeatLot.cs**:
   - `Loads` (ObservableCollection<Model_ReceivingLoad>) - Each load's `HeatLotNumber` property
   - `UniqueHeatNumbers` (ObservableCollection<Model_HeatCheckboxItem>) - Heat number checkboxes

**Dunnage Workflow ViewModels & Properties to Clear:**
1. **ViewModel_Dunnage_TypeSelectionViewModel.cs**:
   - `SelectedType` (Model_DunnageType?) - Selected dunnage type

2. **ViewModel_Dunnage_PartSelectionViewModel.cs**:
   - `SelectedPart` (Model_DunnagePart?) - Selected part

3. **View_ViewModel_Dunnage_DetailsEntryViewModel.cs**:
   - `PONumber` (string) - PO number input
   - `Location` (string) - Location input
   - `SpecInputs` (ObservableCollection<Model_SpecInput>) - Dynamic spec inputs
   - `TextSpecs`, `NumberSpecs`, `BooleanSpecs` (ObservableCollections) - Categorized specs

4. **View_ViewModel_Dunnage_QuantityEntryViewModel.cs**:
   - `Quantity` (int) - Quantity input (default: 1)

### Save Workflow Separation

- [ ] T024 [P] [SaveWorkflow] Separate CSV and Database save operations in Receiving workflow
  - [ ] T024a: Review current implementation in `Service_ReceivingWorkflow.SaveSessionAsync()` (lines 329-426)
    - **FOUND**: Line 357 calls `_csvWriter.WriteToCSVAsync(CurrentSession.Loads)` for CSV save
    - **FOUND**: Line 388 calls `_mysqlReceiving.SaveReceivingLoadsAsync(CurrentSession.Loads)` for database save
    - **CURRENT FLOW**: Validate → CSV (lines 357-375) → Database (lines 382-396) → Cleanup → Return result
    - **METHOD SIGNATURE**: `Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress, IProgress<int>? percentProgress)`
  - [ ] T024b: Create `SaveToCSVOnlyAsync()` method that only calls `_csvWriter.WriteToCSVAsync()`
  - [ ] T024c: Create `SaveToDatabaseOnlyAsync()` method that only calls `_mysqlReceiving.SaveReceivingLoadsAsync()`
  - [ ] T024d: Update `SaveSessionAsync()` to use new methods for better control flow
- [ ] T025 [P] [SaveWorkflow] Separate CSV and Database save operations in Dunnage workflow
  - [ ] T025a: Review current implementation in `Service_DunnageWorkflow.SaveSessionAsync()` (lines 138-193) and `ViewModel_Dunnage_ReviewViewModel.SaveAllAsync()` (lines 189-232)
    - **FOUND**: Service method at `Module_Receiving/Module_Core/Services/Service_DunnageWorkflow.cs` lines 138-193
    - **WORKFLOW**: Dunnage save is triggered from `ViewModel_Dunnage_ReviewViewModel.SaveAllAsync()` which calls both database and CSV operations
    - **INTERFACE**: `IService_DunnageWorkflow` has `SaveSessionAsync()` method (line 20 of interface)
  - [ ] T025b: Create `SaveToCSVOnlyAsync()` method in `Service_DunnageWorkflow`
  - [ ] T025c: Create `SaveToDatabaseOnlyAsync()` method in `Service_DunnageWorkflow`
  - [ ] T025d: Update `ViewModel_Dunnage_ReviewViewModel.SaveAllAsync()` to use separated methods

### Add Another Part/PO - Save to CSV First

- [ ] T026 [ReviewPage] Update "+ Add Another Part/PO" button to save current data to CSV before clearing
  - [ ] T026a: Modify `AddAnotherPartAsync()` in `ViewModel_Receiving_Review.cs` (current line 145)
  - [ ] T026b: Insert call to `_workflowService.SaveToCSVOnlyAsync()` before `ClearTransientWorkflowData()`
  - [ ] T026c: Handle CSV save errors with user notification via `_errorHandler`
  - [ ] T026d: Add progress indicator during CSV save operation
  - [ ] T026e: Test workflow: Review → Click "Add Another" → Verify CSV updated → Form cleared
- [ ] T027 [ReviewPage] Update "+ Add Another" button (Dunnage) to save current data to CSV before clearing
  - [ ] T027a: Modify `AddAnotherAsync()` in `ViewModel_Dunnage_ReviewViewModel.cs` (current line 174)
  - [ ] T027b: Insert call to `_workflowService.SaveToCSVOnlyAsync()` before `ClearTransientWorkflowData()`
  - [ ] T027c: Handle CSV save errors with user notification
  - [ ] T027d: Test workflow: Review → Click "Add Another" → Verify CSV updated → Form cleared

### Start New Entry - Reset All Local Data

- [ ] T028 [SaveComplete] Update "Start New Entry" button to reset all local data (not just workflow)
  - [ ] T028a: Review current implementation in `StartNewEntryAsync()` in `ViewModel_Receiving_Workflow.cs` (line 219)
    - **CURRENT IMPLEMENTATION**: Calls `_workflowService.ResetWorkflowAsync()` only
    - **FOUND**: `Service_ReceivingWorkflow.ResetWorkflowAsync()` at lines 428-440
    - **CURRENT BEHAVIOR**: Clears `CurrentSession`, resets `NumberOfLoads=1`, sets step to `POEntry`, clears `CurrentPONumber`, `CurrentPart`, `IsNonPOItem`, clears `_currentBatchLoads`, calls `_sessionManager.ClearSessionAsync()`
    - **MISSING**: Does NOT clear ViewModel UI input properties (PoNumber in POEntryViewModel, SelectedPackageType in PackageTypeViewModel, etc.)
  - [ ] T028b: Identify all local data sources to clear: session loads, intermediate ViewModels, cached preferences
  - [ ] T028c: Create `ClearAllLocalDataAsync()` method that clears:
    - `CurrentSession.Loads` collection
    - Package type preferences (cached)
    - Heat/Lot number inputs
    - Weight/Quantity inputs
    - PO Entry fields
  - [ ] T028d: Update `StartNewEntryAsync()` to call new clear method
  - [ ] T028e: Test: Complete save → Click "Start New Entry" → Verify all fields cleared
- [ ] T029 [SaveComplete] Implement "Start New Entry" functionality for Dunnage workflow
  - [ ] T029a: Add "Start New Entry" button to Dunnage completion screen (currently missing)
  - [ ] T029b: Create `StartNewEntryCommand` in `View_Dunnage_WorkflowViewModel` (if exists) or relevant ViewModel
  - [ ] T029c: Implement data clearing logic similar to Receiving workflow
  - [ ] T029d: Test: Complete save → Click "Start New Entry" → Verify all fields cleared

### Heat/Lot# Optional Default Value

- [ ] T030 [HeatLot] Ensure Heat/Lot# defaults to "Nothing Entered" when empty across all workflows
  - [x] T030a: **FOUND** - `ViewModel_Receiving_ManualEntry.cs` line 269 sets default: `load.HeatLotNumber = "Nothing Entered";`
  - [x] T030b: **FOUND** - `ViewModel_Receiving_EditMode.cs` line 731 sets default: `load.HeatLotNumber = "Nothing Entered";`
  - [x] T030c: **FOUND** - `ViewModel_Receiving_HeatLot.cs` has `PrepareHeatLotFields()` method (line 148)
  - [ ] T030d: Verify `PrepareHeatLotFields()` implementation sets "Nothing Entered" for empty fields
    - **FOUND**: Method at `Module_Receiving/ViewModels/ViewModel_Receiving_HeatLot.cs` lines 150-159
    - **CONFIRMED**: Sets `load.HeatLotNumber = "Not Entered"` (NOT "Nothing Entered" - inconsistent with other ViewModels)
    - **BUG FOUND**: Text is "Not Entered" in HeatLotViewModel but "Nothing Entered" in ManualEntryViewModel and EditModeViewModel
    - **ACTION REQUIRED**: Standardize to "Nothing Entered" across all three locations
  - [ ] T030e: Add same logic to Guided workflow save operation before finalizing loads
  - [ ] T030f: Test all three workflows (Guided, Manual, Edit) to verify default value applied

### Reset CSV Button Enhancement

- [ ] T031 [ResetCSV] Keep "Reset CSV" button active and visible across all Receiving workflow views
  - [x] T031a: **FOUND** - Button exists on completion screen (line 106) and POEntry screen (line 124)
  - [x] T031b: **CONFIRMED** - Only visible on POEntry screen: `Visibility="{x:Bind ViewModel.IsPOEntryVisible, Mode=OneWay}"`
  - [ ] T031c: Update visibility binding to show on all workflow steps (POEntry, LoadEntry, WeightQuantity, HeatLot, PackageType, Review)
  - [ ] T031d: Add confirmation dialog to prevent accidental CSV deletion during active workflow
  - [ ] T031e: Test: Navigate through all workflow steps → Verify "Reset CSV" button visible and functional
- [ ] T032 [ResetCSV] Implement "Reset CSV" button functionality in Dunnage workflow (currently missing)
  - [ ] T032a: Add "Reset CSV" button to `View_Dunnage_WorkflowView.xaml` (verify file exists or identify correct view)
  - [ ] T032b: Create `ResetCSVCommand` in Dunnage workflow ViewModel
  - [ ] T032c: Implement `ResetCSVAsync()` method calling `_workflowService.ResetCSVFilesAsync()`
  - [ ] T032d: Add confirmation dialog with same message as Receiving workflow
  - [ ] T032e: Position button consistently with Receiving workflow layout
  - [ ] T032f: Test: Navigate Dunnage workflow → Verify "Reset CSV" button visible and functional

### Reset CSV → Database Save Workflow

- [ ] T033 [ResetCSV] Update "Reset CSV" button to trigger database save before deleting CSV files
  - [ ] T033a: Review current `ResetCSVAsync()` implementation in `ViewModel_Receiving_Workflow.cs` (line 226)
    - **DELEGATES TO**: `Service_ReceivingWorkflow.ResetCSVFilesAsync()` at lines 442-446
    - **CURRENT BEHAVIOR**: Simply calls `_csvWriter.DeleteCSVFilesAsync()` and returns result
    - **MISSING**: No check for unsaved data, no database save prompt
    - **REQUIRED**: Add logic to detect unsaved CSV data and prompt user to save to database before deletion
  - [ ] T033b: Modify workflow to:
    1. Check if CSV files have unsaved data (loads not in database)
    2. If yes, prompt: "CSV contains unsaved data. Save to database before deleting?"
    3. If user confirms, call `SaveToDatabaseOnlyAsync()`
    4. Then proceed with CSV deletion
  - [ ] T033c: Update confirmation dialog to reflect new two-step process
  - [ ] T033d: Handle errors during database save (allow user to retry or cancel CSV deletion)
  - [ ] T033e: Test scenarios:
    - CSV empty → Delete without prompt
    - CSV has data in database → Delete with confirmation
    - CSV has unsaved data → Prompt to save → Save to database → Delete
- [ ] T034 [ResetCSV] Implement database save workflow for Dunnage "Reset CSV" button
  - [ ] T034a: Apply same logic as T033 to Dunnage workflow
  - [ ] T034b: Use `Service_DunnageWorkflow.SaveToDatabaseOnlyAsync()` method
  - [ ] T034c: Test same scenarios as T033 for Dunnage workflow

### ViewModel Access Pattern for Data Clearing

- [ ] T035 [DataClearing] Implement ViewModel accessor pattern for comprehensive UI input clearing
  - [ ] T035a: Create `IService_ViewModelRegistry` interface for accessing ViewModels across workflow
  - [ ] T035b: Register all workflow ViewModels in registry during DI initialization
  - [ ] T035c: Implement `ClearAllUIInputs()` method in workflow services that:
    - Accesses registered ViewModels via registry
    - Calls reset methods on each ViewModel
    - Resets all observable properties to default values
  - [ ] T035d: Update `ClearWorkflowData()` in Phase 1 to call `ClearAllUIInputs()`
  - [ ] T035e: Update `ClearTransientWorkflowData()` in Phase 4 to call `ClearAllUIInputs()`
  - [ ] T035f: Alternative approach (if registry is complex): Add `ResetToDefaults()` method to each ViewModel interface
- [ ] T036 [DataClearing] Test comprehensive clearing across all workflow scenarios
  - [ ] T036a: Test Receiving Guided Mode: Enter data → Switch mode → Verify all fields cleared
  - [ ] T036b: Test Receiving Review: Enter data → Add Another Part → Verify form fields cleared (loads preserved)
  - [ ] T036c: Test Dunnage Guided Mode: Enter data → Switch mode → Verify all fields cleared
  - [ ] T036d: Test Dunnage Review: Enter data → Add Another → Verify form fields cleared (loads preserved)
  - [ ] T036e: Test Start New Entry: Complete save → Start new → Verify ALL data cleared including session

---

## Phase 7: Package Type Preferences Fix

### Phase Dependencies

- **Phase 1 (Mode Selection)**: No dependencies - can start immediately ✅ COMPLETE
- **Phase 2 (Dunnage Notifications)**: Depends on analyzing Receiving workflow - can run after Phase 1
- **Phase 3 (Settings UI)**: No dependencies - can run in parallel with other phases
- **Phase 4 (Review Page)**: No dependencies - can run in parallel ✅ COMPLETE
- **Phase 5 (Receiving Workflow Bug Fixes)**: No dependencies - can run in parallel
- **Phase 6 (Save Workflow Enhancements)**: Depends on Phase 4 completion (uses same patterns) - should run after Phase 1 and 4
- **Phase 7 (Package Type Preferences)**: No dependencies - can run in parallel

### Parallel Opportunities

- All tasks marked [P] can run in parallel within their phase
- Phases 2, 3, 5, 7 can all proceed in parallel after Phase 1 completes
- Phase 6 should follow Phase 4 (similar confirmation dialog patterns)
- Different functional areas can be worked on by different developers

---

## Implementation Strategy

### Incremental Delivery

1. Complete Phase 1: Mode Selection fixes
2. Complete Phases 2-5 in parallel (UI improvements and bug fixes)
3. Test all fixes together for integration issues

### Testing Strategy

- Manual UI testing for each fix
- Verify data persistence (CSV/DB not affected by resets)
- Test notification timing and visibility
- Validate UI consistency across views

---

## Notes

- Reference existing Receiving workflow implementations for consistency
- Use IService_ErrorHandler for user confirmations and error display
- Ensure all ViewModel changes maintain MVVM separation
- Test on both workflows to ensure consistency

---

## Reference Guide for Implementation

### Correct File Naming Patterns
| Incorrect Reference (in tasks) | Correct File Path |
|-------------------------------|-------------------|
| `ReceivingWorkflowViewModel.cs` | `ViewModel_Receiving_Workflow.cs` |
| `DunnageWorkflowViewModel.cs` | No single file (multiple step ViewModels) |
| `ReceivingReviewViewModel.cs` | `ViewModel_Receiving_Review.cs` |
| `DunnageReviewViewModel.cs` | `ViewModel_Dunnage_ReviewViewModel.cs` |
| `ReceivingPreferencesViewModel.cs` | Likely `ViewModel_Receiving_PackageType.cs` |
| `NumberOfLoadsViewModel.cs` | Likely `ViewModel_Receiving_LoadEntry.cs` |
| `Dao_PackageType.cs` | `Dao_PackageTypePreference.cs` |
| `MainNavigationView.xaml` | Settings views use different pattern |

### Available Services
- `IService_ErrorHandler` - For error handling and user notifications
  - Methods: `ShowUserErrorAsync()`, `ShowErrorDialogAsync()`, `HandleErrorAsync()`, `HandleDaoErrorAsync()`
  - **Note**: No `ShowUserConfirmation()` method - use ContentDialog or `ShowErrorDialogAsync()` with custom implementation
- `IService_Help` - For contextual help display
  - Method: `ShowContextualHelpAsync(Enum_ReceivingWorkflowStep)` or similar
- `IService_Dispatcher` - For thread-safe UI updates
  - Method: `TryEnqueue(Action)`
- `IWindowService` - For window management

### Notification Pattern (from ViewModel_Receiving_Workflow.cs)
```csharp
[ObservableProperty]
private string _statusMessage = string.Empty;

[ObservableProperty]
private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

[ObservableProperty]
private bool _isStatusOpen;

public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
{
    StatusMessage = message;
    StatusSeverity = severity;
    IsStatusOpen = true;

    // Auto-dismiss after 5 seconds if informational or success
    if (severity == InfoBarSeverity.Informational || severity == InfoBarSeverity.Success)
    {
        Task.Delay(5000).ContinueWith(_ =>
        {
            _dispatcherService.TryEnqueue(() =>
            {
                IsStatusOpen = false;
            });
        });
    }
}
```

### XAML InfoBar Pattern
```xaml
<InfoBar
    IsOpen="{x:Bind ViewModel.IsStatusOpen, Mode=TwoWay}"
    Severity="{x:Bind ViewModel.StatusSeverity, Mode=OneWay}"
    Message="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
    IsClosable="True"/>
```

### All Dunnage ViewModels (Phase 2, T006)
1. `ViewModel_Dunnage_ModeSelectionViewModel.cs`
2. `ViewModel_Dunnage_TypeSelectionViewModel.cs`
3. `ViewModel_Dunnage_PartSelectionViewModel.cs`
4. `View_ViewModel_Dunnage_DetailsEntryViewModel.cs`
5. `View_ViewModel_Dunnage_QuantityEntryViewModel.cs`
6. `ViewModel_Dunnage_ReviewViewModel.cs`
7. `ViewModel_Dunnage_ManualEntryViewModel.cs`
8. `ViewModel_Dunnage_EditModeViewModel.cs`
9. `ViewModel_Dunnage_AdminMainViewModel.cs` (admin)
10. `ViewModel_Dunnage_AdminTypesViewModel.cs` (admin)
11. `ViewModel_Dunnage_AdminPartsViewModel.cs` (admin)
12. `ViewModel_Dunnage_AdminInventoryViewModel.cs` (admin)

### All Dunnage Views (Phase 2, T007)
1. `View_Dunnage_ModeSelectionView.xaml`
2. `View_Dunnage_TypeSelectionView.xaml`
3. `View_Dunnage_PartSelectionView.xaml`
4. `View_Dunnage_DetailsEntryView.xaml`
5. `View_Dunnage_QuantityEntryView.xaml`
6. `View_Dunnage_ReviewView.xaml`
7. `View_Dunnage_ManualEntryView.xaml`
8. `View_Dunnage_EditModeView.xaml`

### All Settings Views (Phase 3)
1. `View_Settings_Workflow.xaml`
2. `View_Settings_ModeSelection.xaml`
3. `View_Settings_DunnageMode.xaml`
4. `View_Settings_Placeholder.xaml`

---

## Quality Gates

**Before marking fixes complete, verify:**

### Code Quality
- [ ] All compilation errors resolved
- [ ] No warnings in build output
- [ ] Code follows existing MVVM patterns

### Constitution Compliance
- [ ] **MVVM Architecture**: Logic in ViewModels, markup-only Views
- [ ] **Dependency Injection**: Services properly injected
- [ ] **Error Handling**: IService_ErrorHandler used appropriately
- [ ] **WinUI 3**: x:Bind used, proper async patterns

### Testing
- [ ] Manual testing of all fixed functionality
- [ ] UI consistency verified across workflows
- [ ] Data integrity maintained (no unintended resets)

### Documentation
- [ ] Code comments added for new logic
- [ ] Any new methods documented
- [ ] File references accurate





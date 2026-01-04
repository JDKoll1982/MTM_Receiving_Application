---
description: "Task list for implementing remaining application fixes and improvements"
---

# Tasks: Application Fixes and Improvements - Pending Tasks

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

### ✅ Clarification Complete (Verified Name Mismatches)
- `Module_Receiving/ViewModels/ReceivingWorkflowViewModel.cs` → `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
- `Module_Receiving/ViewModels/ReceivingReviewViewModel.cs` → `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`
- `Module_Receiving/ViewModels/ReceivingPreferencesViewModel.cs` → Preferences are handled in `Module_Receiving/ViewModels/ViewModel_Receiving_PackageType.cs` via `IService_MySQL_PackagePreferences` and `Module_Receiving/Data/Dao_PackageTypePreference.cs`
- `Module_Settings/Views/MainNavigationView.xaml` → Main navigation is in `MainWindow.xaml`; settings pages are in `Module_Settings/Views/View_Settings_*.xaml`
- `Module_Receiving/Views/NumberOfLoadsView.xaml` → `Module_Receiving/Views/View_Receiving_LoadEntry.xaml`
- `Module_Receiving/ViewModels/NumberOfLoadsViewModel.cs` → `Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs`
- `Module_Receiving/Data/Dao_PackageType.cs` → `Module_Receiving/Data/Dao_PackageTypePreference.cs`
- `Module_Dunnage/ViewModels/DunnageWorkflowViewModel.cs` → No single "workflow ViewModel" exists; Dunnage workflow is orchestrated by:
  - `Module_Core/ViewModels/Main/Main_DunnageLabelViewModel.cs` (step visibility + title)
  - `Module_Core/Contracts/Services/IService_DunnageWorkflow.cs` + `Module_Dunnage/Services/Service_DunnageWorkflow.cs` (state machine + session)

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

## Phase 1: Settings Views UI Improvements

**Purpose**: Standardize UI consistency across settings views

- [x] T012 [SettingsViews] Update main headers in Settings Views to include Material Design icons matching Dunnage and Receiving workflows
  - [x] T012a: Add icon to `Module_Settings/Views/View_Settings_Workflow.xaml` (if not already present)
  - [x] T012b: Add icon to `Module_Settings/Views/View_Settings_Placeholder.xaml` (if not already present)
  - [x] T012c: Verify `View_Settings_ModeSelection.xaml` and `View_Settings_DunnageMode.xaml` already use Material.Icons (validation shows they do)
  - [x] T012d: Document which Material.Icons should be used for each settings page (e.g., Settings icon, Workflow icon, etc.)
    - **REFERENCE**: Material.Icons.WinUI3 uses `MaterialIconKind` enum values, not hex codes
    - **NAMESPACE**: `xmlns:materialIcons="using:Material.Icons.WinUI3"`
    - **USAGE PATTERN**: `<materialIcons:MaterialIcon Kind="Settings" Width="24" Height="24"/>`
    - **SUGGESTED ICONS** (MaterialIconKind enum values):
      - Settings pages: `Settings` or `Cog`
      - Workflow settings: `WorkflowOutline` or `Sitemap`
      - Mode selection: `ViewDashboard` or `ViewGridOutline`
      - Dunnage mode: `PackageVariantClosed` (default used in Dunnage views)
      - General: `ChevronLeft`, `ChevronRight` (navigation), `PageFirst`, `PageLast` (pagination)
- [x] T013 [SettingsViews] Fix background coloring in all Settings Views to match Dunnage and Receiving workflow backgrounds
  - [x] T013a: Identify current background resources used in Receiving and Dunnage views
  - [x] T013b: Apply same background to `View_Settings_Workflow.xaml`, `View_Settings_ModeSelection.xaml`, `View_Settings_DunnageMode.xaml`, `View_Settings_Placeholder.xaml`
- [x] T014 [SettingsViews] Test visual consistency across all settings pages

---

## Phase 2: Receiving Workflow Bug Fixes

**Purpose**: Fix specific errors in Receiving workflow

- [x] T019 [ReceivingWorkflow] Investigate "failed to save preference" error when saving custom package type names
  - [x] T019a: **CONFIRMED** - `ViewModel_Receiving_PackageType.cs` handles preferences (line 168-194: SavePreferenceAsync method)
  - [x] T019b: **FOUND** - Error occurs at line 192: `await _errorHandler.HandleErrorAsync("Failed to save preference", Enum_ErrorSeverity.Warning, ex)`
  - [x] T019c: Check `IService_MySQL_PackagePreferences.SavePreferenceAsync()` implementation (the service layer, not the DAO directly)
  - [x] T019d: Verify stored procedure is being called with correct parameters (check PartID and PackageTypeName values)
- [x] T020 [ReceivingWorkflow] Fix package type name saving by ensuring proper validation and database operation
  - [x] T020a: Test stored procedure `sp_SavePackageTypePreference` directly: `CALL sp_SavePackageTypePreference('TEST_PART', 'Custom Package Name');`
  - [x] T020b: Add validation in ViewModel lines 168-179 to ensure PartID is not null/empty and PackageTypeName has valid characters
  - [x] T020c: Review `IService_MySQL_PackagePreferences` implementation for error handling and parameter binding
  - [x] T020d: Add more detailed error logging to identify exact failure point (service vs DAO vs stored procedure)
- [x] T021 [ReceivingWorkflow] Verify help content availability (help button infrastructure exists)
  - [x] T021a: **CONFIRMED** - Help button exists in `View_Receiving_Workflow.xaml` line 138 with HelpButton_Click event
  - [x] T021b: **CONFIRMED** - `View_Receiving_Workflow.xaml.cs` line 24-31 calls `_helpService.ShowContextualHelpAsync(_workflowService.CurrentStep)`
  - [x] T021c: Test help button for each workflow step (POEntry, PackageTypeEntry, LoadEntry, WeightQuantity, HeatLot, Review) to identify missing content
  - [x] T021d: **CONFIRMED** - Dunnage also has help button in `View_Dunnage_WorkflowView.xaml` line 102 with same pattern
- [x] T022 [ReceivingWorkflow] Add or fix help content for workflow steps
  - [ ] T022a: Review `IService_Help` implementation to see how ShowContextualHelpAsync works and where content is stored
    - **INTERFACE**: `Module_Core/Contracts/Services/IService_Help.cs`
    - **IMPLEMENTATION**: `Module_Core/Services/Help/Service_Help.cs`
    - **CONTENT STORAGE**: In-memory dictionary `_helpContentCache`, populated by `InitializeHelpContent()` (hardcoded `Model_HelpContent` entries)
    - **REFERENCE**: Known usage in `View_Receiving_Workflow.xaml` line 138: `_helpService.ShowContextualHelpAsync(currentStep)`
  - [ ] T022b: For each Enum_ReceivingWorkflowStep value (ModeSelection, POEntry, PackageTypeEntry, LoadEntry, WeightQuantity, HeatLot, Review, ManualEntry, EditMode, Saving), verify help content exists
  - [ ] T022c: For each Enum_DunnageWorkflowStep value (ModeSelection, TypeSelection, PartSelection, DetailsEntry, QuantityEntry, Review, ManualEntry, EditMode, Saving), verify help content exists
  - [ ] T022d: If content is missing, create help text/dialogs explaining each step's purpose and required inputs
- [x] T023 [ReceivingWorkflow] Test package type preference saving and help button functionality

---

## Phase 3: Save Workflow Enhancements

**Purpose**: Separate CSV and Database save operations, enhance data reset, and improve workflow controls

- [x] T024 [P] [SaveWorkflow] Separate CSV and Database save operations in Receiving workflow
  - [x] T024a: Review current implementation in `Service_ReceivingWorkflow.SaveSessionAsync()` (lines 329-426)
    - **FOUND**: Line 357 calls `_csvWriter.WriteToCSVAsync(CurrentSession.Loads)` for CSV save
    - **FOUND**: Line 388 calls `_mysqlReceiving.SaveReceivingLoadsAsync(CurrentSession.Loads)` for database save
    - **CURRENT FLOW**: Validate → CSV (lines 357-375) → Database (lines 382-396) → Cleanup → Return result
    - **METHOD SIGNATURE**: `Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress, IProgress<int>? percentProgress)`
  - [x] T024b: Create `SaveToCSVOnlyAsync()` method that only calls `_csvWriter.WriteToCSVAsync()`
  - [x] T024c: Create `SaveToDatabaseOnlyAsync()` method that only calls `_mysqlReceiving.SaveReceivingLoadsAsync()`
  - [x] T024d: Update `SaveSessionAsync()` to use new methods for better control flow
- [x] T025 [P] [SaveWorkflow] Separate CSV and Database save operations in Dunnage workflow
  - [x] T025a: Review current implementation in `Service_DunnageWorkflow.SaveSessionAsync()` and `ViewModel_Dunnage_ReviewViewModel.SaveAllAsync()`
    - **FOUND**: Service implementation at `Module_Dunnage/Services/Service_DunnageWorkflow.cs`
    - **WORKFLOW**: Dunnage save is triggered from `ViewModel_Dunnage_ReviewViewModel.SaveAllAsync()` which calls both database and CSV operations
    - **INTERFACE**: `Module_Core/Contracts/Services/IService_DunnageWorkflow.cs` has `SaveSessionAsync()`
  - [x] T025b: Create `SaveToCSVOnlyAsync()` method in `Service_DunnageWorkflow`
  - [x] T025c: Create `SaveToDatabaseOnlyAsync()` method in `Service_DunnageWorkflow`
  - [x] T025d: Update `ViewModel_Dunnage_ReviewViewModel.SaveAllAsync()` to use separated methods

### Add Another Part/PO - Save to CSV First

- [x] T026 [ReviewPage] Update "+ Add Another Part/PO" button to save current data to CSV before clearing
  - [x] T026a: Modify `AddAnotherPartAsync()` in `ViewModel_Receiving_Review.cs` (current line 145)
  - [x] T026b: Insert call to `_workflowService.SaveToCSVOnlyAsync()` before `ClearTransientWorkflowData()`
  - [x] T026c: Handle CSV save errors with user notification via `_errorHandler`
  - [x] T026d: Add progress indicator during CSV save operation
  - [x] T026e: Test workflow: Review → Click "Add Another" → Verify CSV updated → Form cleared
- [x] T027 [ReviewPage] Update "+ Add Another" button (Dunnage) to save current data to CSV before clearing
  - [x] T027a: Modify `AddAnotherAsync()` in `ViewModel_Dunnage_ReviewViewModel.cs` (current line 174)
  - [x] T027b: Insert call to `_workflowService.SaveToCSVOnlyAsync()` before `ClearTransientWorkflowData()`
  - [x] T027c: Handle CSV save errors with user notification
  - [x] T027d: Test workflow: Review → Click "Add Another" → Verify CSV updated → Form cleared

### Start New Entry - Reset All Local Data

- [x] T028 [SaveComplete] Update "Start New Entry" button to reset all local data (not just workflow)
  - [x] T028a: Review current implementation in `StartNewEntryAsync()` in `ViewModel_Receiving_Workflow.cs` (line 219)
    - **CURRENT IMPLEMENTATION**: Calls `_workflowService.ResetWorkflowAsync()` only
    - **FOUND**: `Service_ReceivingWorkflow.ResetWorkflowAsync()` at lines 428-440
    - **CURRENT BEHAVIOR**: Clears `CurrentSession`, resets `NumberOfLoads=1`, sets step to `POEntry`, clears `CurrentPONumber`, `CurrentPart`, `IsNonPOItem`, clears `_currentBatchLoads`, calls `_sessionManager.ClearSessionAsync()`
    - **MISSING**: Does NOT clear ViewModel UI input properties (PoNumber in POEntryViewModel, SelectedPackageType in PackageTypeViewModel, etc.)
  - [x] T028b: Identify all local data sources to clear: session loads, intermediate ViewModels, cached preferences
  - [x] T028c: Create `ClearAllLocalDataAsync()` method that clears:
    - `CurrentSession.Loads` collection
    - Package type preferences (cached)
    - Heat/Lot number inputs
    - Weight/Quantity inputs
    - PO Entry fields
  - [x] T028d: Update `StartNewEntryAsync()` to call new clear method
  - [x] T028e: Test: Complete save → Click "Start New Entry" → Verify all fields cleared
- [x] T029 [SaveComplete] Implement "Start New Entry" functionality for Dunnage workflow
  - [x] T029a: Add "Start New Entry" button to Dunnage completion screen (currently missing)
  - [x] T029b: Create `StartNewEntryCommand` in `View_Dunnage_WorkflowViewModel` (if exists) or relevant ViewModel
  - [x] T029c: Implement data clearing logic similar to Receiving workflow
  - [x] T029d: Test: Complete save → Click "Start New Entry" → Verify all fields cleared

### Heat/Lot# Optional Default Value

- [x] T030 [HeatLot] Ensure Heat/Lot# defaults to "Nothing Entered" when empty across all workflows
  - [x] T030a: **FOUND** - `ViewModel_Receiving_ManualEntry.cs` line 269 sets default: `load.HeatLotNumber = "Nothing Entered";`
  - [x] T030b: **FOUND** - `ViewModel_Receiving_EditMode.cs` line 731 sets default: `load.HeatLotNumber = "Nothing Entered";`
  - [x] T030c: **FOUND** - `ViewModel_Receiving_HeatLot.cs` has `PrepareHeatLotFields()` method (line 148)
  - [x] T030d: Verify `PrepareHeatLotFields()` implementation sets "Nothing Entered" for empty fields
    - **FOUND**: Method at `Module_Receiving/ViewModels/ViewModel_Receiving_HeatLot.cs` lines 150-159
    - **CONFIRMED**: Sets `load.HeatLotNumber = "Not Entered"` (NOT "Nothing Entered" - inconsistent with other ViewModels)
    - **BUG FOUND**: Text is "Not Entered" in HeatLotViewModel but "Nothing Entered" in ManualEntryViewModel and EditModeViewModel
    - **ACTION REQUIRED**: Standardize to "Nothing Entered" across all three locations
  - [x] T030e: Add same logic to Guided workflow save operation before finalizing loads
  - [x] T030f: Test all three workflows (Guided, Manual, Edit) to verify default value applied

### Reset CSV Button Enhancement

- [x] T031 [ResetCSV] Keep "Reset CSV" button active and visible across all Receiving workflow views
  - [x] T031a: **FOUND** - Button exists on completion screen (line 106) and POEntry screen (line 124)
  - [x] T031b: **CONFIRMED** - Only visible on POEntry screen: `Visibility="{x:Bind ViewModel.IsPOEntryVisible, Mode=OneWay}"`
  - [x] T031c: Update visibility binding to show on all workflow steps (POEntry, LoadEntry, WeightQuantity, HeatLot, PackageType, Review)
  - [x] T031d: Add confirmation dialog to prevent accidental CSV deletion during active workflow
  - [x] T031e: Test: Navigate through all workflow steps → Verify "Reset CSV" button visible and functional
- [x] T032 [ResetCSV] Implement "Reset CSV" button functionality in Dunnage workflow (currently missing)
  - [x] T032a: Add "Reset CSV" button to `View_Dunnage_WorkflowView.xaml` (verify file exists or identify correct view)
  - [x] T032b: Create `ResetCSVCommand` in Dunnage workflow ViewModel
  - [x] T032c: Implement `ResetCSVAsync()` method calling `_workflowService.ResetCSVFilesAsync()`
  - [x] T032d: Add confirmation dialog with same message as Receiving workflow
  - [x] T032e: Position button consistently with Receiving workflow layout
  - [x] T032f: Test: Navigate Dunnage workflow → Verify "Reset CSV" button visible and functional

### Reset CSV → Database Save Workflow

- [x] T033 [ResetCSV] Update "Reset CSV" button to trigger database save before deleting CSV files
  - [x] T033a: Review current `ResetCSVAsync()` implementation in `ViewModel_Receiving_Workflow.cs` (line 226)
    - **DELEGATES TO**: `Service_ReceivingWorkflow.ResetCSVFilesAsync()` at lines 442-446
    - **CURRENT BEHAVIOR**: Simply calls `_csvWriter.DeleteCSVFilesAsync()` and returns result
    - **MISSING**: No check for unsaved data, no database save prompt
    - **REQUIRED**: Add logic to detect unsaved CSV data and prompt user to save to database before deletion
  - [x] T033b: Modify workflow to:
    1. Check if CSV files have unsaved data (loads not in database)
    2. If yes, prompt: "CSV contains unsaved data. Save to database before deleting?"
    3. If user confirms, call `SaveToDatabaseOnlyAsync()`
    4. Then proceed with CSV deletion
  - [x] T033c: Update confirmation dialog to reflect new two-step process
  - [x] T033d: Handle errors during database save (allow user to retry or cancel CSV deletion)
  - [x] T033e: Test scenarios:
    - CSV empty → Delete without prompt
    - CSV has data in database → Delete with confirmation
    - CSV has unsaved data → Prompt to save → Save to database → Delete
- [x] T034 [ResetCSV] Implement database save workflow for Dunnage "Reset CSV" button
  - [x] T034a: Apply same logic as T033 to Dunnage workflow
  - [x] T034b: Use `Service_DunnageWorkflow.SaveToDatabaseOnlyAsync()` method
  - [x] T034c: Test same scenarios as T033 for Dunnage workflow

### ViewModel Access Pattern for Data Clearing

- [x] T035 [DataClearing] Implement ViewModel accessor pattern for comprehensive UI input clearing
  - [x] T035a: Create `IService_ViewModelRegistry` interface for accessing ViewModels across workflow
  - [x] T035b: Register all workflow ViewModels in registry during DI initialization
  - [x] T035c: Implement `ClearAllUIInputs()` method in workflow services that:
    - Accesses registered ViewModels via registry
    - Calls reset methods on each ViewModel
    - Resets all observable properties to default values
  - [x] T035d: Update `ClearWorkflowData()` in Phase 1 to call `ClearAllUIInputs()`
  - [x] T035e: Update `ClearTransientWorkflowData()` in Phase 4 to call `ClearAllUIInputs()`
  - [x] T035f: Alternative approach (if registry is complex): Add `ResetToDefaults()` method to each ViewModel interface
- [x] T036 [DataClearing] Test comprehensive clearing across all workflow scenarios
  - [x] T036a: Test Receiving Guided Mode: Enter data → Switch mode → Verify all fields cleared
  - [x] T036b: Test Receiving Review: Enter data → Add Another Part → Verify form fields cleared (loads preserved)
  - [x] T036c: Test Dunnage Guided Mode: Enter data → Switch mode → Verify all fields cleared
  - [x] T036d: Test Dunnage Review: Enter data → Add Another → Verify form fields cleared (loads preserved)
  - [x] T036e: Test Start New Entry: Complete save → Start new → Verify ALL data cleared including session

### UI Consistency

- [x] T037 [UI] Fix Receiving Workflow header update issue
  - [x] T037a: Ensure `UpdateStepVisibility` runs on UI thread using `DispatcherQueue`
  - [x] T037b: Update `CurrentStepTitle` strings to match Dunnage naming convention ("Receiving - [Step Name]")

---

## Phase Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Settings UI)**: No dependencies - can start immediately
- **Phase 2 (Receiving Workflow Bugs)**: No dependencies - can run in parallel with Phase 1
- **Phase 3 (Save Workflow Enhancements)**: Depends on Phase 4 completion (uses same patterns) - should run after Phase 1 and 4 (but Phase 4 is complete)

### Parallel Opportunities

- All tasks marked [P] can run in parallel within their phase
- Phases 1 and 2 can proceed in parallel
- Phase 3 should follow after Phase 1 (for consistency patterns)

### Within Each Phase

- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before endpoints
- Core implementation before integration
- Phase complete before moving to next priority

---

## Implementation Strategy

### Incremental Delivery

1. Complete Phase 1: Settings UI improvements
2. Complete Phase 2: Receiving workflow bug fixes
3. Complete Phase 3: Save workflow enhancements

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
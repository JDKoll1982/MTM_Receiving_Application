# Module_Receiving Workflow Consolidation - Task List

## Overview
This task list outlines all work required to consolidate the Module_Receiving workflow from 12 steps to 3 steps.

## Task Categories

### Phase 1: Planning & Design ✅
- [x] Analyze current workflow structure
- [x] Design Wizard 3-step workflow
- [x] Create UI mockups
- [x] Document consolidation plan
- [x] Create task list

### Phase 2: Enum & Model Updates

#### 2.1 Update Enum_ReceivingWorkflowStep
- [ ] Add new Wizard step enums:
  - `OrderPartSelection = 20` (replaces POEntry, PartSelection, LoadEntry)
  - `LoadDetailsEntry = 21` (replaces WeightQuantityEntry, HeatLotEntry, PackageTypeEntry)
  - `ReviewSave = 22` (replaces Review, Saving, Complete)
- [ ] Mark old step enums as `[Obsolete]` with migration notes
- [ ] Update XML documentation comments
- [ ] File: `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs`

#### 2.2 Create New Models (if needed)
- [ ] Review existing models for consolidation compatibility
- [ ] Create `Model_OrderPartSelection` if needed for Step 1 state
- [ ] Create `Model_LoadDetailsEntry` if needed for Step 2 state
- [ ] Update `Model_ReceivingSession` if structure changes needed
- [ ] Files: `Module_Receiving/Models/`

### Phase 3: ViewModel Implementation

#### 3.1 Create ViewModel_Receiving_OrderPartSelection
- [ ] Create new ViewModel file
- [ ] Inherit from `ViewModel_Shared_Base`
- [ ] Implement `[ObservableProperty]` for:
  - `PONumber` (string)
  - `IsNonPOItem` (bool)
  - `SelectedPart` (Model_InforVisualPart?)
  - `NumberOfLoads` (int)
  - `PartSearchText` (string)
  - `AvailableParts` (ObservableCollection<Model_InforVisualPart>)
  - `IsPartSearching` (bool)
  - `PartDetailsVisible` (bool)
- [ ] Implement `[RelayCommand]` for:
  - `SearchPartsAsync()`
  - `SelectPartAsync(Model_InforVisualPart part)`
  - `ToggleNonPOItem()`
  - `ValidateAndContinueAsync()`
- [ ] Add validation logic
- [ ] Add help content integration
- [ ] File: `Module_Receiving/ViewModels/ViewModel_Receiving_OrderPartSelection.cs`

#### 3.2 Create ViewModel_Receiving_LoadDetails
- [ ] Create new ViewModel file
- [ ] Inherit from `ViewModel_Shared_Base`
- [ ] Implement `[ObservableProperty]` for:
  - `Loads` (ObservableCollection<Model_ReceivingLoad>)
  - `SelectedLoad` (Model_ReceivingLoad?)
  - `PackageTypes` (ObservableCollection<string>)
  - `ValidationResults` (Dictionary<int, Model_ValidationResult>)
- [ ] Implement `[RelayCommand]` for:
  - `UpdateLoadAsync(Model_ReceivingLoad load)`
  - `CopyLoadToAllAsync(Model_ReceivingLoad sourceLoad)`
  - `ClearAllLoadsAsync()`
  - `ValidateAndContinueAsync()`
- [ ] Add per-load validation logic
- [ ] Add bulk operations support
- [ ] Add help content integration
- [ ] File: `Module_Receiving/ViewModels/ViewModel_Receiving_LoadDetails.cs`

#### 3.3 Create ViewModel_Receiving_ReviewSave
- [ ] Create new ViewModel file
- [ ] Inherit from `ViewModel_Shared_Base`
- [ ] Implement `[ObservableProperty]` for:
  - `SessionSummary` (Model_ReceivingSession)
  - `Loads` (ObservableCollection<Model_ReceivingLoad>)
  - `IsSaving` (bool)
  - `SaveProgress` (int)
  - `SaveProgressMessage` (string)
  - `SaveResult` (Model_SaveResult?)
  - `IsComplete` (bool)
- [ ] Implement `[RelayCommand]` for:
  - `EditLoadAsync(Model_ReceivingLoad load)`
  - `SaveAsync()`
  - `StartNewEntryAsync()`
- [ ] Integrate save progress reporting
- [ ] Add help content integration
- [ ] File: `Module_Receiving/ViewModels/ViewModel_Receiving_ReviewSave.cs`

#### 3.4 Update ViewModel_Receiving_Workflow
- [ ] Update visibility properties for new steps:
  - `IsOrderPartSelectionVisible` (bool)
  - `IsLoadDetailsEntryVisible` (bool)
  - `IsReviewSaveVisible` (bool)
- [ ] Update `OnWorkflowStepChanged()` to handle new steps
- [ ] Update step title dictionary
- [ ] Remove or deprecate old step visibility properties
- [ ] File: `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`

### Phase 4: View (XAML) Implementation

#### 4.1 Create View_Receiving_OrderPartSelection
- [ ] Create new XAML file
- [ ] Implement PO Number entry section
- [ ] Implement Non-PO checkbox
- [ ] Implement part search with autocomplete
- [ ] Implement part details display card
- [ ] Implement number of loads input
- [ ] Add validation error display
- [ ] Add help button integration
- [ ] Use `x:Bind` for all bindings
- [ ] File: `Module_Receiving/Views/View_Receiving_OrderPartSelection.xaml`
- [ ] Code-behind: `Module_Receiving/Views/View_Receiving_OrderPartSelection.xaml.cs`

#### 4.2 Create View_Receiving_LoadDetails
- [ ] Create new XAML file
- [ ] Implement load details entry UI (choose: expandable sections or data grid)
- [ ] Implement per-load fields:
  - Weight/Quantity input
  - Heat Lot Number input
  - Package Type dropdown
  - Packages Per Load input
- [ ] Implement validation indicators per load
- [ ] Implement "Copy to All" buttons
- [ ] Implement bulk actions section
- [ ] Add context bar (Part/PO info)
- [ ] Add validation error display
- [ ] Add help button integration
- [ ] Use `x:Bind` for all bindings
- [ ] File: `Module_Receiving/Views/View_Receiving_LoadDetails.xaml`
- [ ] Code-behind: `Module_Receiving/Views/View_Receiving_LoadDetails.xaml.cs`

#### 4.3 Create View_Receiving_ReviewSave
- [ ] Create new XAML file
- [ ] Implement summary section
- [ ] Implement load details review cards/list
- [ ] Implement edit buttons per load
- [ ] Implement save button
- [ ] Implement progress indicator (during save)
- [ ] Implement results display section
- [ ] Implement "Start New Entry" button
- [ ] Add status icons (success/warning/error)
- [ ] Add help button integration
- [ ] Use `x:Bind` for all bindings
- [ ] File: `Module_Receiving/Views/View_Receiving_ReviewSave.xaml`
- [ ] Code-behind: `Module_Receiving/Views/View_Receiving_ReviewSave.xaml.cs`

#### 4.4 Update View_Receiving_Workflow
- [ ] Add new view UserControls to main workflow view
- [ ] Update visibility bindings for new steps
- [ ] Remove or hide old step views (keep for migration)
- [ ] Update navigation buttons
- [ ] File: `Module_Receiving/Views/View_Receiving_Workflow.xaml`
- [ ] Code-behind: `Module_Receiving/Views/View_Receiving_Workflow.xaml.cs`

### Phase 5: Service Layer Updates

#### 5.1 Update Service_ReceivingWorkflow
- [ ] Update `AdvanceToNextStepAsync()` to handle new step transitions:
  - `OrderPartSelection` → `LoadDetailsEntry`
  - `LoadDetailsEntry` → `ReviewSave`
  - `ReviewSave` → (internal save) → Complete
- [ ] Update `GoToPreviousStep()` for new step flow
- [ ] Update validation logic for Wizard steps
- [ ] Update `StartWorkflowAsync()` to use new steps
- [ ] Update `ResetWorkflowAsync()` to use new steps
- [ ] Maintain backward compatibility for old sessions
- [ ] File: `Module_Receiving/Services/Service_ReceivingWorkflow.cs`

#### 5.2 Update IService_ReceivingWorkflow
- [ ] Update interface documentation
- [ ] Add new method signatures if needed
- [ ] File: `Module_Receiving/Contracts/IService_ReceivingWorkflow.cs`

#### 5.3 Update Service_ReceivingValidation
- [ ] Review validation methods for Wizard step compatibility
- [ ] Add batch validation methods if needed
- [ ] Update validation error messages
- [ ] File: `Module_Receiving/Services/Service_ReceivingValidation.cs`

### Phase 6: Settings & Localization

#### 6.1 Update ReceivingSettingsKeys
- [ ] Add new step title keys:
  - `StepTitleOrderPartSelection`
  - `StepTitleLoadDetailsEntry`
  - `StepTitleReviewSave`
- [ ] Mark old step title keys as deprecated
- [ ] File: `Module_Receiving/Settings/ReceivingSettingsKeys.cs`

#### 6.2 Update ReceivingSettingsDefaults
- [ ] Add default values for new step titles
- [ ] Add default help text for new steps
- [ ] File: `Module_Receiving/Settings/ReceivingSettingsDefaults.cs`

#### 6.3 Update Settings Manifest
- [ ] Add new setting keys to manifest
- [ ] Update localization files if needed
- [ ] File: `Module_Settings.Core/Defaults/settings.manifest.json`

### Phase 7: Help Content

#### 7.1 Update Helper_WorkflowHelpContentGenerator
- [ ] Add help content for `OrderPartSelection` step
- [ ] Add help content for `LoadDetailsEntry` step
- [ ] Add help content for `ReviewSave` step
- [ ] File: `Module_Core/Helpers/UI/Helper_WorkflowHelpContentGenerator.cs`

#### 7.2 Update Help Documentation
- [ ] Create/update help content files for new steps
- [ ] Update workflow help index
- [ ] Files: `Module_Core/Services/Help/` or documentation location

### Phase 8: Dependency Injection

#### 8.1 Update App.xaml.cs
- [ ] Register new ViewModels as Transient
- [ ] Register new Views as Transient
- [ ] Verify existing service registrations
- [ ] File: `App.xaml.cs` (or DI configuration file)

### Phase 9: Testing

#### 9.1 Unit Tests
- [ ] Test `ViewModel_Receiving_OrderPartSelection`:
  - Part search functionality
  - PO number validation
  - Non-PO item toggle
  - Number of loads validation
  - Navigation commands
- [ ] Test `ViewModel_Receiving_LoadDetails`:
  - Load data entry
  - Per-load validation
  - Copy to all functionality
  - Bulk operations
  - Navigation commands
- [ ] Test `ViewModel_Receiving_ReviewSave`:
  - Summary display
  - Save functionality
  - Progress reporting
  - Results display
  - Navigation commands
- [ ] Test `Service_ReceivingWorkflow`:
  - New step transitions
  - Validation gates
  - Session persistence
  - Backward compatibility
- [ ] Files: `MTM_Receiving_Application.Tests/Module_Receiving/`

#### 9.2 Integration Tests
- [ ] Test complete 3-step workflow
- [ ] Test session persistence across steps
- [ ] Test validation error handling
- [ ] Test save functionality
- [ ] Test backward compatibility with old sessions
- [ ] Files: `MTM_Receiving_Application.Tests/Module_Receiving/Integration/`

#### 9.3 UI Tests
- [ ] Test Step 1: Order & Part Selection
- [ ] Test Step 2: Load Details Entry
- [ ] Test Step 3: Review & Save
- [ ] Test navigation between steps
- [ ] Test validation feedback
- [ ] Test save progress and results

### Phase 10: Migration & Backward Compatibility

#### 10.1 Session Migration
- [ ] Create migration utility for old session format
- [ ] Handle old step enum values
- [ ] Convert old session data to new format
- [ ] Test migration with sample old sessions
- [ ] File: `Module_Receiving/Services/Service_SessionMigration.cs` (if needed)

#### 10.2 Backward Compatibility
- [ ] Ensure old step enums still work during transition
- [ ] Add feature flag for new vs old workflow (if needed)
- [ ] Document migration path
- [ ] Create rollback plan

### Phase 11: Documentation

#### 11.1 Code Documentation
- [ ] Add XML doc comments to all new classes/methods
- [ ] Update existing documentation references
- [ ] Document breaking changes

#### 11.2 User Documentation
- [ ] Update user guide with new 3-step workflow
- [ ] Create migration guide for users
- [ ] Update help content
- [ ] Update training materials

#### 11.3 Technical Documentation
- [ ] Update architecture documentation
- [ ] Update workflow state machine diagram
- [ ] Document new step transitions
- [ ] Update API documentation

### Phase 12: Cleanup & Deprecation

#### 12.1 Deprecate Old Components
- [ ] Mark old ViewModels as `[Obsolete]`
- [ ] Mark old Views as `[Obsolete]`
- [ ] Mark old step enums as `[Obsolete]`
- [ ] Add deprecation notices with migration notes

#### 12.2 Remove Old Code (Future)
- [ ] Remove old ViewModels after migration period
- [ ] Remove old Views after migration period
- [ ] Remove old step enum values after migration period
- [ ] Clean up unused code

### Phase 13: Performance Optimization

#### 13.1 Load Details Entry Performance
- [ ] Optimize data grid rendering for multiple loads
- [ ] Implement virtual scrolling if needed
- [ ] Optimize validation performance
- [ ] Test with large number of loads (10+)

#### 13.2 Save Performance
- [ ] Optimize save progress reporting
- [ ] Ensure non-blocking UI during save
- [ ] Test save performance with multiple loads

### Phase 14: Quality Assurance

#### 14.1 Code Review Checklist
- [ ] All ViewModels are `partial` classes
- [ ] All ViewModels inherit from `ViewModel_Shared_Base`
- [ ] All XAML uses `x:Bind` (no runtime `Binding`)
- [ ] All async methods end with `Async` suffix
- [ ] All DAOs return `Model_Dao_Result` (no exceptions)
- [ ] All services registered in DI
- [ ] No direct DAO calls from ViewModels
- [ ] Proper error handling throughout
- [ ] XML documentation on public APIs

#### 14.2 Build & Test
- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes
- [ ] No linter errors
- [ ] No compiler warnings (or justified warnings)
- [ ] Code coverage maintained/improved

### Phase 15: Deployment

#### 15.1 Pre-Deployment
- [ ] Final user acceptance testing
- [ ] Performance testing
- [ ] Security review
- [ ] Documentation review

#### 15.2 Deployment
- [ ] Deploy to test environment
- [ ] Verify functionality
- [ ] Monitor for issues
- [ ] Deploy to production (after approval)

#### 15.3 Post-Deployment
- [ ] Monitor user feedback
- [ ] Track workflow completion metrics
- [ ] Address any issues
- [ ] Plan for old code removal

## Task Dependencies

```
Phase 2 (Enum Updates)
  ↓
Phase 3 (ViewModels)
  ↓
Phase 4 (Views)
  ↓
Phase 5 (Services)
  ↓
Phase 6 (Settings)
  ↓
Phase 7 (Help)
  ↓
Phase 8 (DI)
  ↓
Phase 9 (Testing)
  ↓
Phase 10 (Migration)
  ↓
Phase 11 (Documentation)
  ↓
Phase 12 (Cleanup)
  ↓
Phase 13 (Performance)
  ↓
Phase 14 (QA)
  ↓
Phase 15 (Deployment)
```

## Estimated Effort

- **Phase 2-4:** 40-60 hours (Core implementation)
- **Phase 5-8:** 20-30 hours (Integration)
- **Phase 9:** 30-40 hours (Testing)
- **Phase 10-11:** 15-20 hours (Migration & Docs)
- **Phase 12-15:** 10-15 hours (Polish & Deploy)

**Total Estimated Effort:** 115-165 hours

## Risk Mitigation

1. **Breaking Changes:** Maintain backward compatibility during transition
2. **User Confusion:** Provide clear UI indicators and help text
3. **Data Loss:** Maintain session persistence throughout
4. **Performance:** Optimize data grid rendering for multiple loads
5. **Testing Coverage:** Ensure comprehensive test coverage before deployment

## Success Criteria

- [ ] Workflow reduced from 12 steps to 3 steps
- [ ] All functionality preserved
- [ ] All tests passing
- [ ] No data loss during migration
- [ ] User acceptance testing passed
- [ ] Performance maintained or improved
- [ ] Documentation updated

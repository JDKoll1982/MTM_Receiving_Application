# Phase 5: Wizard Mode ViewModels - Task List

**Phase:** 5 of 8  
**Status:** ⏳ PENDING  
**Priority:** HIGH - Core user workflow  
**Dependencies:** Phase 4 (Shared ViewModels) must be complete

---

## 📊 **Phase 5 Overview**

**Goal:** Implement all Wizard Mode ViewModels for the guided 3-step receiving workflow

**Wizard Mode Structure:**
- **Step 1:** Order & Part Selection (PO Number, Part Selection, Load Count)
- **Step 2:** Load Details Entry (Weight, Heat/Lot, Package Type, bulk operations)
- **Step 3:** Review, Save & Complete

**Status:**
- ⏳ Orchestration ViewModels: 0/2 complete
- ⏳ Step 1 ViewModels: 0/6 complete
- ⏳ Step 2 ViewModels: 0/8 complete
- ⏳ Step 3 ViewModels: 0/6 complete
- ⏳ Dialog ViewModels: 0/4 complete

**Completion:** 0/52 tasks (0%)

**Estimated Total Time:** 40-50 hours

---

## 🎯 **Wizard Mode User Flow**

```
┌─────────────────────────────────────────┐
│  Step 1: Order & Part Selection         │
│  ─────────────────────────────────      │
│  1. Enter PO Number (validate format)   │
│  2. Validate PO exists in ERP           │
│  3. Select Part Number from list        │
│  4. Enter Load Count (1-999)            │
│  5. Click "Next" → Navigate to Step 2   │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│  Step 2: Load Details Entry             │
│  ─────────────────────────────────      │
│  1. DataGrid displays all loads         │
│  2. Enter Weight/Quantity per load      │
│  3. Enter Heat/Lot (with bulk copy)     │
│  4. Select Package Type (with bulk copy)│
│  5. Enter Packages Per Load             │
│  6. Select Receiving Location           │
│  7. Auto-calculate fields               │
│  8. Validate all required fields        │
│  9. Click "Next" → Navigate to Step 3   │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│  Step 3: Review & Save                  │
│  ─────────────────────────────────      │
│  1. Display summary table (read-only)   │
│  2. Show totals and statistics          │
│  3. Validate all data complete          │
│  4. Click "Save" → Save to DB + CSV     │
│  5. Show completion confirmation        │
│  6. Option: New Transaction or Exit     │
└─────────────────────────────────────────┘
```

---

## ⏳ **Orchestration ViewModels (2 tasks)**

### Task 5.1: ViewModel_Receiving_Wizard_Orchestration_MainWorkflow

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Orchestration/ViewModel_Receiving_Wizard_Orchestration_MainWorkflow.cs`  
**Dependencies:** ViewModel_Shared_Base, IMediator  
**Estimated Time:** 4 hours

**Responsibilities:**
- Manage wizard step navigation (1 → 2 → 3)
- Coordinate workflow state across all steps
- Initialize/persist workflow session
- Handle navigation validation between steps
- Coordinate save operation

**Key Properties:**
```csharp
[ObservableProperty]
private int _currentStepNumber = 1;

[ObservableProperty]
private Guid _sessionId = Guid.NewGuid();

[ObservableProperty]
private bool _canNavigateNext = false;

[ObservableProperty]
private bool _canNavigatePrevious = false;

[ObservableProperty]
private string _currentStepTitle = "Step 1: Order & Part Selection";

[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_StepIndicator> _stepIndicators = new();
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task NavigateToNextStepAsync();

[RelayCommand]
private async Task NavigateToPreviousStepAsync();

[RelayCommand]
private async Task NavigateToStepAsync(int stepNumber);

[RelayCommand]
private async Task CancelWorkflowAsync();

[RelayCommand]
private async Task StartNewWorkflowAsync();
```

**MediatR Usage:**
```csharp
// Load session
var query = new QueryRequest_Receiving_Shared_Get_WorkflowSession { SessionId = _sessionId };
var result = await _mediator.Send(query);

// Save session
var command = new CommandRequest_Receiving_Shared_Save_WorkflowSession { SessionId = _sessionId, /* ... */ };
await _mediator.Send(command);
```

**Acceptance Criteria:**
- [ ] Manages step navigation (1 ↔ 2 ↔ 3)
- [ ] Validates step completion before allowing "Next"
- [ ] Persists session state on every step change
- [ ] Loads existing session on initialization
- [ ] Cancels workflow with confirmation dialog
- [ ] Resets for new workflow after completion

---

### Task 5.2: ViewModel_Receiving_Wizard_Orchestration_StepValidation

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Orchestration/ViewModel_Receiving_Wizard_Orchestration_StepValidation.cs`  
**Dependencies:** None  
**Estimated Time:** 2 hours

**Responsibilities:**
- Validate Step 1 data (PO, Part, Load Count)
- Validate Step 2 data (all loads have required fields)
- Validate Step 3 data (ready for save)
- Provide validation error messages

**Key Methods:**
```csharp
public async Task<bool> ValidateStep1Async();
public async Task<bool> ValidateStep2Async();
public async Task<bool> ValidateStep3Async();
public List<string> GetValidationErrors();
```

**Acceptance Criteria:**
- [ ] Step 1 validation checks PO, Part, LoadCount
- [ ] Step 2 validation checks all loads complete
- [ ] Step 3 validation checks data ready for save
- [ ] Returns clear error messages for each issue

---

## ⏳ **Step 1 ViewModels (6 tasks)**

### Task 5.3: ViewModel_Receiving_Wizard_Display_PONumberEntry

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_PONumberEntry.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 3 hours

**Responsibilities:**
- Capture PO Number input
- Validate PO format (PO-XXXXXX)
- Check if PO exists in ERP (Infor Visual)
- Support Non-PO mode (checkbox toggle)
- Persist to session

**Key Properties:**
```csharp
[ObservableProperty]
private string _poNumber = string.Empty;

[ObservableProperty]
private bool _isNonPO = false;

[ObservableProperty]
private bool _isPOValid = false;

[ObservableProperty]
private string _poValidationMessage = string.Empty;

[ObservableProperty]
private bool _isValidating = false;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task ValidatePONumberAsync();

[RelayCommand]
private void ToggleNonPOMode();

[RelayCommand]
private async Task SavePOToSessionAsync();
```

**MediatR Usage:**
```csharp
// Validate PO
var query = new QueryRequest_Receiving_Shared_Validate_PONumber { PONumber = _poNumber };
var result = await _mediator.Send(query);

// Save to session
var command = new CommandRequest_Receiving_Shared_Save_WorkflowSession 
{ 
    SessionId = _sessionId, 
    PONumber = _poNumber,
    IsNonPO = _isNonPO
};
await _mediator.Send(command);
```

**Acceptance Criteria:**
- [ ] Real-time PO format validation
- [ ] Async ERP validation (debounced)
- [ ] Non-PO mode disables PO validation
- [ ] Saves to session automatically
- [ ] Shows clear validation messages

---

### Task 5.4: ViewModel_Receiving_Wizard_Display_PartSelection

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_PartSelection.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 4 hours

**Responsibilities:**
- Display part selection ComboBox
- Load parts from ERP based on PO
- Support manual part entry (Non-PO mode)
- Show part details (description, type, UOM)
- Auto-populate part preferences
- Persist selection to session

**Key Properties:**
```csharp
[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_PartDetails> _availableParts = new();

[ObservableProperty]
private Model_Receiving_DataTransferObjects_PartDetails? _selectedPart;

[ObservableProperty]
private string _manualPartNumber = string.Empty;

[ObservableProperty]
private bool _isManualEntryMode = false;

[ObservableProperty]
private string _partDescription = string.Empty;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task LoadPartsFromPOAsync();

[RelayCommand]
private async Task SelectPartAsync(Model_Receiving_DataTransferObjects_PartDetails part);

[RelayCommand]
private async Task LoadPartPreferencesAsync();

[RelayCommand]
private async Task SavePartToSessionAsync();
```

**MediatR Usage:**
```csharp
// Get part details
var query = new QueryRequest_Receiving_Shared_Get_PartDetails { PartNumber = partNumber };
var result = await _mediator.Send(query);
```

**Acceptance Criteria:**
- [ ] Loads parts from PO (ERP query)
- [ ] ComboBox with autocomplete
- [ ] Shows part details on selection
- [ ] Manual entry for Non-PO mode
- [ ] Loads user preferences (last used settings)
- [ ] Saves to session

---

### Task 5.5: ViewModel_Receiving_Wizard_Display_LoadCountEntry

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_LoadCountEntry.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 2 hours

**Responsibilities:**
- Capture load count (1-999)
- Validate range
- Initialize empty load records in session
- Show load count confirmation

**Key Properties:**
```csharp
[ObservableProperty]
private int _loadCount = 1;

[ObservableProperty]
private bool _isLoadCountValid = false;

[ObservableProperty]
private string _loadCountValidationMessage = string.Empty;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task ValidateLoadCountAsync();

[RelayCommand]
private async Task InitializeLoadsAsync();

[RelayCommand]
private async Task SaveLoadCountToSessionAsync();
```

**Acceptance Criteria:**
- [ ] Range validation (1-999)
- [ ] Real-time validation feedback
- [ ] Creates empty load records
- [ ] Saves to session

---

### Task 5.6: ViewModel_Receiving_Wizard_Interaction_Step1Navigation

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Interaction_Step1Navigation.cs`  
**Estimated Time:** 2 hours

**Responsibilities:**
- Enable/disable "Next" button based on Step 1 completion
- Coordinate between PO, Part, LoadCount ViewModels
- Trigger navigation to Step 2

**Acceptance Criteria:**
- [ ] "Next" enabled only when all Step 1 fields valid
- [ ] Triggers validation before navigation
- [ ] Saves session before navigation

---

### Task 5.7: ViewModel_Receiving_Wizard_Display_Step1Summary

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_Step1Summary.cs`  
**Estimated Time:** 1 hour

**Responsibilities:**
- Display Step 1 selections summary
- Show on Step 2 and Step 3 for reference

**Key Properties:**
```csharp
public string PONumberDisplay => IsNonPO ? "NON-PO" : PONumber;
public string PartNumberDisplay { get; }
public string LoadCountDisplay => $"{LoadCount} Load(s)";
```

**Acceptance Criteria:**
- [ ] Displays PO, Part, Load Count
- [ ] Read-only summary format
- [ ] Updates when session changes

---

### Task 5.8: ViewModel_Receiving_Wizard_Dialog_Step1HelpDialog

**Priority:** P3 - LOW  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Dialog_Step1HelpDialog.cs`  
**Estimated Time:** 1 hour

**Responsibilities:**
- Show contextual help for Step 1
- Explain PO format, Non-PO mode, Part selection

**Acceptance Criteria:**
- [ ] Displays help content
- [ ] Provides examples
- [ ] Dismissible dialog

---

## ⏳ **Step 2 ViewModels (8 tasks)**

### Task 5.9: ViewModel_Receiving_Wizard_Display_LoadDetailsGrid

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Display_LoadDetailsGrid.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 6 hours

**Responsibilities:**
- Display DataGrid with all loads
- Bind to ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow>
- Handle cell editing (Weight, HeatLot, PackageType, PackagesPerLoad, Location)
- Auto-calculate derived fields (TotalWeight, WeightPerPackage)
- Validate each row on edit
- Save changes to session in real-time
- Support row selection for bulk operations

**Key Properties:**
```csharp
[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loadRows = new();

[ObservableProperty]
private Model_Receiving_DataTransferObjects_LoadGridRow? _selectedLoadRow;

[ObservableProperty]
private List<Model_Receiving_DataTransferObjects_LoadGridRow> _selectedRows = new();

[ObservableProperty]
private bool _isEditingCell = false;

[ObservableProperty]
private string _gridStatusMessage = "0 of 0 loads complete";
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task LoadDetailsFromSessionAsync();

[RelayCommand]
private async Task SaveLoadRowAsync(Model_Receiving_DataTransferObjects_LoadGridRow row);

[RelayCommand]
private async Task ValidateAllRowsAsync();

[RelayCommand]
private void SelectRowAsync(Model_Receiving_DataTransferObjects_LoadGridRow row);

[RelayCommand]
private void SelectMultipleRowsAsync(List<Model_Receiving_DataTransferObjects_LoadGridRow> rows);
```

**MediatR Usage:**
```csharp
// Load session data
var query = new QueryRequest_Receiving_Shared_Get_WorkflowSession { SessionId = _sessionId };
var result = await _mediator.Send(query);
_loadRows = new ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow>(result.Data.LoadDetails);

// Save changes
var command = new CommandRequest_Receiving_Shared_Save_WorkflowSession 
{ 
    SessionId = _sessionId,
    LoadDetails = _loadRows.ToList()
};
await _mediator.Send(command);
```

**Auto-Calculation Logic:**
```csharp
// When Quantity or PackagesPerLoad changes:
row.WeightPerPackage = row.Quantity / row.PackagesPerLoad;
row.TotalWeight = row.Quantity;

// Notify property changes
row.OnPropertyChanged(nameof(row.WeightPerPackage));
row.OnPropertyChanged(nameof(row.TotalWeight));
```

**Acceptance Criteria:**
- [ ] DataGrid displays all loads
- [ ] Editable cells (Weight, HeatLot, PackageType, etc.)
- [ ] Auto-calculation on edit
- [ ] Real-time validation
- [ ] Row selection support
- [ ] Saves to session on every change
- [ ] Shows completion status (X of Y loads complete)

---

### Task 5.10: ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 5 hours

**Responsibilities:**
- Copy fields from source load to empty cells in other loads
- Support multiple copy types (HeatLotOnly, PackageTypeOnly, AllFields, etc.)
- Show preview dialog before executing copy
- Execute copy via MediatR command
- Update grid after copy

**Key Properties:**
```csharp
[ObservableProperty]
private int _sourceLoadNumber = 1;

[ObservableProperty]
private Enum_Receiving_CopyType_FieldSelection _copyType = Enum_Receiving_CopyType_FieldSelection.AllFields;

[ObservableProperty]
private List<int> _targetLoadNumbers = new();

[ObservableProperty]
private int _affectedLoadsCount = 0;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task PreviewCopyOperationAsync();

[RelayCommand]
private async Task ExecuteCopyOperationAsync();

[RelayCommand]
private async Task CancelCopyOperationAsync();
```

**MediatR Usage:**
```csharp
var command = new CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads
{
    SessionId = _sessionId,
    SourceLoadNumber = _sourceLoadNumber,
    CopyType = _copyType
};

var result = await _mediator.Send(command);
// result.Data contains count of updated loads
```

**Copy Types (Enum_Receiving_CopyType_FieldSelection):**
- `HeatLotOnly` - Copy only Heat/Lot field
- `PackageTypeOnly` - Copy only PackageType field
- `PackagesPerLoadOnly` - Copy only PackagesPerLoad field
- `WeightQuantityOnly` - Copy only Weight/Quantity field
- `AllFields` - Copy all fillable fields

**Acceptance Criteria:**
- [ ] Shows copy preview dialog with affected load count
- [ ] Executes copy via MediatR command
- [ ] Only copies to EMPTY cells (doesn't overwrite)
- [ ] Updates grid automatically after copy
- [ ] Shows success/failure message
- [ ] Supports all copy types

---

### Task 5.11: ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog.cs`  
**Estimated Time:** 2 hours

**Responsibilities:**
- Display copy operation preview
- Show source load data
- Show which loads will be affected
- Confirm/Cancel buttons

**Key Properties:**
```csharp
[ObservableProperty]
private Model_Receiving_DataTransferObjects_LoadGridRow _sourceLoad;

[ObservableProperty]
private int _affectedLoadCount;

[ObservableProperty]
private string _copyTypeDescription;
```

**Acceptance Criteria:**
- [ ] Shows source load data
- [ ] Shows count of affected loads
- [ ] Describes what will be copied
- [ ] Confirm/Cancel buttons

---

### Task 5.12: ViewModel_Receiving_Wizard_Interaction_ClearAutoFilledFields

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Interaction_ClearAutoFilledFields.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 2 hours

**Responsibilities:**
- Clear bulk-copied fields from loads
- Support clearing specific load or all loads
- Execute via MediatR command

**Key Commands:**
```csharp
[RelayCommand]
private async Task ClearAllAutoFilledFieldsAsync();

[RelayCommand]
private async Task ClearAutoFilledFieldsForLoadAsync(int loadNumber);
```

**MediatR Usage:**
```csharp
var command = new CommandRequest_Receiving_Wizard_Clear_AutoFilledFields
{
    SessionId = _sessionId,
    TargetLoadNumber = loadNumber // null = clear all
};

await _mediator.Send(command);
```

**Acceptance Criteria:**
- [ ] Clears auto-filled fields (HeatLot, PackageType, PackagesPerLoad, Location)
- [ ] Supports single load or all loads
- [ ] Updates grid after clear
- [ ] Shows confirmation before clearing

---

### Task 5.13: ViewModel_Receiving_Wizard_Display_Step2Progress

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Display_Step2Progress.cs`  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Show completion progress (X of Y loads complete)
- Visual progress bar
- List incomplete loads

**Key Properties:**
```csharp
[ObservableProperty]
private int _completedLoadsCount = 0;

[ObservableProperty]
private int _totalLoadsCount = 0;

[ObservableProperty]
private double _completionPercentage = 0;

[ObservableProperty]
private List<int> _incompleteLoadNumbers = new();
```

**Acceptance Criteria:**
- [ ] Shows completion percentage
- [ ] Progress bar visual
- [ ] Lists incomplete load numbers

---

### Task 5.14: ViewModel_Receiving_Wizard_Interaction_Step2Navigation

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Interaction_Step2Navigation.cs`  
**Estimated Time:** 2 hours

**Responsibilities:**
- Enable/disable "Next" button based on Step 2 completion
- Validate all loads have required fields
- Trigger navigation to Step 3

**Acceptance Criteria:**
- [ ] "Next" enabled only when all loads complete
- [ ] "Previous" always enabled
- [ ] Validates before navigation

---

### Task 5.15: ViewModel_Receiving_Wizard_Display_ReferenceDataComboBoxes

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Display_ReferenceDataComboBoxes.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 2 hours

**Responsibilities:**
- Load reference data (PackageTypes, Locations)
- Populate ComboBoxes in grid
- Cache reference data

**Key Properties:**
```csharp
[ObservableProperty]
private ObservableCollection<Model_Receiving_TableEntitys_PackageType> _packageTypes = new();

[ObservableProperty]
private ObservableCollection<Model_Receiving_TableEntitys_Location> _locations = new();
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task LoadReferenceDataAsync();
```

**MediatR Usage:**
```csharp
var query = new QueryRequest_Receiving_Shared_Get_ReferenceData();
var result = await _mediator.Send(query);
_packageTypes = new ObservableCollection<Model_Receiving_TableEntitys_PackageType>(result.Data.PackageTypes);
_locations = new ObservableCollection<Model_Receiving_TableEntitys_Location>(result.Data.Locations);
```

**Acceptance Criteria:**
- [ ] Loads package types
- [ ] Loads receiving locations
- [ ] ComboBoxes populated in DataGrid
- [ ] Data cached (loaded once)

---

### Task 5.16: ViewModel_Receiving_Wizard_Dialog_Step2HelpDialog

**Priority:** P3 - LOW  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Dialog_Step2HelpDialog.cs`  
**Estimated Time:** 1 hour

**Responsibilities:**
- Show contextual help for Step 2
- Explain DataGrid editing, bulk copy operations

**Acceptance Criteria:**
- [ ] Displays help content
- [ ] Provides examples
- [ ] Dismissible dialog

---

## ⏳ **Step 3 ViewModels (6 tasks)**

### Task 5.17: ViewModel_Receiving_Wizard_Display_ReviewSummary

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Display_ReviewSummary.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 3 hours

**Responsibilities:**
- Display read-only summary of all data
- Show Step 1 info (PO, Part, Load Count)
- Show all loads in read-only DataGrid
- Calculate totals (Total Weight, Total Packages)
- Load data from session

**Key Properties:**
```csharp
[ObservableProperty]
private string _poNumberDisplay = string.Empty;

[ObservableProperty]
private string _partNumberDisplay = string.Empty;

[ObservableProperty]
private int _totalLoadCount = 0;

[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loadRows = new();

[ObservableProperty]
private decimal _totalWeight = 0;

[ObservableProperty]
private int _totalPackages = 0;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task LoadSummaryDataAsync();

[RelayCommand]
private void CalculateTotals();
```

**Acceptance Criteria:**
- [ ] Displays Step 1 summary
- [ ] Shows all loads (read-only)
- [ ] Calculates and displays totals
- [ ] All data from session

---

### Task 5.18: ViewModel_Receiving_Wizard_Orchestration_SaveOperation

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Orchestration_SaveOperation.cs`  
**Dependencies:** IMediator  
**Estimated Time:** 5 hours

**Responsibilities:**
- Coordinate save operation
- Save transaction via MediatR
- Save to CSV file
- Handle save errors
- Show save progress
- Complete workflow session

**Key Properties:**
```csharp
[ObservableProperty]
private bool _isSaving = false;

[ObservableProperty]
private string _saveStatusMessage = string.Empty;

[ObservableProperty]
private double _saveProgress = 0;

[ObservableProperty]
private bool _saveSuccessful = false;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task SaveTransactionAsync();

[RelayCommand]
private async Task ExportToCSVAsync();

[RelayCommand]
private async Task CompleteWorkflowAsync();
```

**MediatR Usage:**
```csharp
// Save transaction to database
var command = new CommandRequest_Receiving_Shared_Save_Transaction
{
    SessionId = _sessionId,
    UserId = _currentUserId,
    UserName = _currentUserName
};

var result = await _mediator.Send(command);

if (result.IsSuccess)
{
    // Complete workflow
    var completeCommand = new CommandRequest_Receiving_Shared_Complete_Workflow
    {
        TransactionId = result.Data,
        SessionId = _sessionId,
        CSVFilePath = csvPath
    };
    
    await _mediator.Send(completeCommand);
}
```

**Save Workflow:**
1. Validate all data complete
2. Save transaction to database (via MediatR)
3. Export to CSV file (local + network)
4. Complete workflow session (via MediatR)
5. Show completion screen

**Acceptance Criteria:**
- [ ] Saves transaction to database
- [ ] Exports to CSV (local + network)
- [ ] Handles save errors gracefully
- [ ] Shows progress during save
- [ ] Completes workflow session
- [ ] Returns transaction ID

---

### Task 5.19: ViewModel_Receiving_Wizard_Display_CompletionScreen

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Display_CompletionScreen.cs`  
**Estimated Time:** 2 hours

**Responsibilities:**
- Display save success confirmation
- Show transaction ID
- Show CSV file paths
- Provide options: New Transaction, View History, Exit

**Key Properties:**
```csharp
[ObservableProperty]
private string _transactionId = string.Empty;

[ObservableProperty]
private string _csvFilePathLocal = string.Empty;

[ObservableProperty]
private string _csvFilePathNetwork = string.Empty;

[ObservableProperty]
private DateTime _completionTimestamp = DateTime.Now;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task StartNewTransactionAsync();

[RelayCommand]
private async Task ViewTransactionHistoryAsync();

[RelayCommand]
private void ExitWorkflowAsync();
```

**Acceptance Criteria:**
- [ ] Shows success message
- [ ] Displays transaction ID
- [ ] Shows CSV paths (with copy button)
- [ ] Provides navigation options
- [ ] Clear "Start New" resets workflow

---

### Task 5.20: ViewModel_Receiving_Wizard_Interaction_Step3Navigation

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Interaction_Step3Navigation.cs`  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Enable/disable "Save" button
- Handle "Previous" navigation (back to Step 2)
- Prevent navigation away during save

**Acceptance Criteria:**
- [ ] "Save" enabled only when all data valid
- [ ] "Previous" enabled before save starts
- [ ] Blocks navigation during save

---

### Task 5.21: ViewModel_Receiving_Wizard_Display_SaveErrorDialog

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Display_SaveErrorDialog.cs`  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Display save error details
- Provide retry option
- Show error log location

**Key Properties:**
```csharp
[ObservableProperty]
private string _errorMessage = string.Empty;

[ObservableProperty]
private string _errorDetails = string.Empty;

[ObservableProperty]
private string _logFilePath = string.Empty;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task RetrySaveAsync();

[RelayCommand]
private void CopyErrorToClipboard();

[RelayCommand]
private void OpenLogFile();
```

**Acceptance Criteria:**
- [ ] Shows error message
- [ ] Provides retry option
- [ ] Links to log file

---

### Task 5.22: ViewModel_Receiving_Wizard_Dialog_Step3HelpDialog

**Priority:** P3 - LOW  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Dialog_Step3HelpDialog.cs`  
**Estimated Time:** 1 hour

**Responsibilities:**
- Show contextual help for Step 3
- Explain review process and save operation

**Acceptance Criteria:**
- [ ] Displays help content
- [ ] Dismissible dialog

---

## ⏳ **Dialog ViewModels (4 tasks)**

### Task 5.23: ViewModel_Receiving_Wizard_Dialog_CancelWorkflowConfirmation

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Dialogs/ViewModel_Receiving_Wizard_Dialog_CancelWorkflowConfirmation.cs`  
**Estimated Time:** 1 hour

**Responsibilities:**
- Confirm workflow cancellation
- Warn about unsaved data

**Acceptance Criteria:**
- [ ] Shows warning message
- [ ] Confirm/Cancel buttons
- [ ] Returns user choice

---

### Task 5.24: ViewModel_Receiving_Wizard_Dialog_ValidationErrorSummary

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Wizard/Dialogs/ViewModel_Receiving_Wizard_Dialog_ValidationErrorSummary.cs`  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Display all validation errors for current step
- Group errors by field/load
- Provide "Go To Error" navigation

**Key Properties:**
```csharp
[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_ValidationError> _validationErrors = new();
```

**Acceptance Criteria:**
- [ ] Lists all errors
- [ ] Groups by field
- [ ] Navigation to errors

---

### Task 5.25: ViewModel_Receiving_Wizard_Dialog_SessionRecovery

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Wizard/Dialogs/ViewModel_Receiving_Wizard_Dialog_SessionRecovery.cs`  
**Estimated Time:** 2 hours

**Responsibilities:**
- Detect incomplete session on startup
- Offer to resume or start new
- Load existing session data

**Key Properties:**
```csharp
[ObservableProperty]
private Model_Receiving_TableEntitys_WorkflowSession? _existingSession;

[ObservableProperty]
private string _sessionSummary = string.Empty;
```

**Acceptance Criteria:**
- [ ] Detects incomplete session
- [ ] Shows session summary
- [ ] Resume/New options

---

### Task 5.26: ViewModel_Receiving_Wizard_Dialog_QuickHelp

**Priority:** P3 - LOW  
**File:** `Module_Receiving/ViewModels/Wizard/Dialogs/ViewModel_Receiving_Wizard_Dialog_QuickHelp.cs`  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Show quick help overlay
- Context-sensitive help tips
- Keyboard shortcuts reference

**Acceptance Criteria:**
- [ ] Shows help tips
- [ ] Keyboard shortcuts
- [ ] Dismissible

---

## 📊 **Phase 5 Summary**

**Total Tasks:** 52  
**Total Estimated Time:** 40-50 hours  
**Critical Path Dependencies:**
1. Orchestration ViewModels (Tasks 5.1-5.2) - MUST complete first
2. Step 1 ViewModels (Tasks 5.3-5.8) - Depend on orchestration
3. Step 2 ViewModels (Tasks 5.9-5.16) - Depend on Step 1
4. Step 3 ViewModels (Tasks 5.17-5.22) - Depend on Step 2
5. Dialog ViewModels (Tasks 5.23-5.26) - Can be done in parallel

**Key Technical Patterns:**
- ✅ All ViewModels use IMediator (NO direct DAO calls)
- ✅ All ViewModels inherit from ViewModel_Shared_Base
- ✅ All async methods end with `Async`
- ✅ All commands use `[RelayCommand]` attribute
- ✅ All properties use `[ObservableProperty]` attribute
- ✅ All ViewModels are partial classes
- ✅ All XAML uses `x:Bind` (compile-time binding)

**MediatR Commands/Queries Used:**
- CommandRequest_Receiving_Shared_Save_Transaction
- CommandRequest_Receiving_Shared_Save_WorkflowSession
- CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads
- CommandRequest_Receiving_Wizard_Clear_AutoFilledFields
- CommandRequest_Receiving_Shared_Complete_Workflow
- QueryRequest_Receiving_Shared_Get_WorkflowSession
- QueryRequest_Receiving_Shared_Get_PartDetails
- QueryRequest_Receiving_Shared_Get_ReferenceData
- QueryRequest_Receiving_Shared_Validate_PONumber

---

**Status:** ⏳ PENDING - Awaiting Phase 4 completion

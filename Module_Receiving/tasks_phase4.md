# Phase 4: Wizard Mode ViewModels & Views - Task List

**Phase:** 4 of 8  
**Status:** ⏳ PENDING  
**Priority:** HIGH - Core user workflow  
**Dependencies:** Phase 3 (CQRS Handlers) must be complete

---

## 📊 **Phase 4 Overview**

**Goal:** Implement complete 3-Step Wizard Mode for guided receiving workflow

**Wizard Mode Structure:**
- **Hub:** Mode selection and workflow orchestration
- **Step 1:** Order & Part Selection (PO, Part, Load Count)
- **Step 2:** Load Details Entry (Weight, Heat/Lot, Package details)
- **Step 3:** Review, Save & Complete

**Status:**
- ✅ Hub ViewModels: 2/2 complete (100%)
- ✅ Wizard ViewModels: 12/12 complete (100%)
- ⏳ Hub Views (XAML): 0/4 pending (Phase 6)
- ⏳ Wizard Views (XAML): 0/24 pending (Phase 6)

**Completion:** 13/42 tasks (31%) - ViewModels Complete, Views Pending

---

## 🎯 **Wizard Mode User Flow**

```
Hub (Mode Selection)
    ↓
Step 1: Order & Part Selection
    ├─ Enter PO Number (with validation)
    ├─ Select Part Number
    └─ Enter Load Count
    ↓
Step 2: Load Details Entry
    ├─ DataGrid with all loads
    ├─ Enter Weight/Quantity per load
    ├─ Enter Heat/Lot (bulk copy available)
    ├─ Select Package Type (bulk copy available)
    └─ Enter Packages Per Load (bulk copy available)
    ↓
Step 3: Review & Save
    ├─ Review summary table
    ├─ Verify all data
    ├─ Save to CSV + Database
    └─ Completion confirmation
```

---

## 📁 **File Structure**

```
Module_Receiving/
├── ViewModels/
│   ├── Hub/
│   │   ├── ViewModel_Receiving_Hub_Orchestration_MainWorkflow.cs
│   │   └── ViewModel_Receiving_Hub_Display_ModeSelection.cs
│   │
│   └── Wizard/
│       ├── Orchestration/
│       │   └── ViewModel_Receiving_Wizard_Orchestration_MainWorkflow.cs
│       │
│       ├── Step1/
│       │   ├── ViewModel_Receiving_Wizard_Display_PONumberEntry.cs
│       │   ├── ViewModel_Receiving_Wizard_Display_PartSelection.cs
│       │   └── ViewModel_Receiving_Wizard_Display_LoadCountEntry.cs
│       │
│       ├── Step2/
│       │   ├── ViewModel_Receiving_Wizard_Display_LoadDetailsGrid.cs
│       │   ├── ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations.cs
│       │   └── ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog.cs
│       │
│       └── Step3/
│           ├── ViewModel_Receiving_Wizard_Display_ReviewSummary.cs
│           ├── ViewModel_Receiving_Wizard_Orchestration_SaveOperation.cs
│           └── ViewModel_Receiving_Wizard_Display_CompletionScreen.cs
│
└── Views/
    ├── Hub/
    │   ├── View_Receiving_Hub_Orchestration_MainWorkflow.xaml[.cs]
    │   └── View_Receiving_Hub_Display_ModeSelection.xaml[.cs]
    │
    └── Wizard/
        ├── Orchestration/
        │   └── View_Receiving_Wizard_Orchestration_MainWorkflow.xaml[.cs]
        │
        ├── Step1/
        │   ├── View_Receiving_Wizard_Display_PONumberEntry.xaml[.cs]
        │   ├── View_Receiving_Wizard_Display_PartSelection.xaml[.cs]
        │   └── View_Receiving_Wizard_Display_LoadCountEntry.xaml[.cs]
        │
        ├── Step2/
        │   ├── View_Receiving_Wizard_Display_LoadDetailsGrid.xaml[.cs]
        │   └── View_Receiving_Wizard_Dialog_CopyPreviewDialog.xaml[.cs]
        │
        └── Step3/
            ├── View_Receiving_Wizard_Display_ReviewSummary.xaml[.cs]
            └── View_Receiving_Wizard_Display_CompletionScreen.xaml[.cs]
```

---

## ⏳ **Hub ViewModels (2 tasks)**

### Task 4.1: ViewModel_Receiving_Hub_Orchestration_MainWorkflow

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Hub/ViewModel_Receiving_Hub_Orchestration_MainWorkflow.cs`  
**Dependencies:** None  
**Estimated Time:** 2 hours

**Responsibilities:**
- Manage hub navigation
- Coordinate mode selection
- Initialize selected workflow (Wizard/Manual/Edit)
- Maintain workflow state

**Key Properties:**
```csharp
[ObservableProperty]
private Enum_Receiving_Mode_WorkflowMode _selectedMode = Enum_Receiving_Mode_WorkflowMode.Wizard;

[ObservableProperty]
private bool _isNonPO = false;

[ObservableProperty]
private string _currentView = string.Empty;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task SelectWizardModeAsync()

[RelayCommand]
private async Task SelectManualModeAsync()

[RelayCommand]
private async Task SelectEditModeAsync()

[RelayCommand]
private void ToggleNonPOMode()
```

**Acceptance Criteria:**
- [ ] Partial class inheriting from `ViewModel_Shared_Base`
- [ ] Mode selection logic
- [ ] Navigation to selected workflow
- [ ] Non-PO toggle state management
- [ ] IMediator injected
- [ ] All commands use `[RelayCommand]`
- [ ] All properties use `[ObservableProperty]`

---

### Task 4.2: ViewModel_Receiving_Hub_Display_ModeSelection

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Hub/ViewModel_Receiving_Hub_Display_ModeSelection.cs`  
**Dependencies:** None  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Display mode selection UI
- Show mode descriptions
- Handle PO/Non-PO toggle
- Navigate to selected mode

**Key Properties:**
```csharp
[ObservableProperty]
private string _wizardModeDescription = "Guided 3-step workflow";

[ObservableProperty]
private string _manualModeDescription = "Direct grid entry";

[ObservableProperty]
private string _editModeDescription = "Edit completed transactions";

[ObservableProperty]
private bool _isNonPOEnabled = true;
```

**Key Commands:**
```csharp
[RelayCommand]
private void SelectMode(Enum_Receiving_Mode_WorkflowMode mode)

[RelayCommand]
private void ShowModeHelp(Enum_Receiving_Mode_WorkflowMode mode)
```

**Acceptance Criteria:**
- [ ] Mode selection buttons
- [ ] Mode descriptions displayed
- [ ] Help system integration
- [ ] Visual feedback for selection

---

## ⏳ **Wizard Orchestration (1 task)**

### Task 4.3: ViewModel_Receiving_Wizard_Orchestration_MainWorkflow

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Orchestration/ViewModel_Receiving_Wizard_Orchestration_MainWorkflow.cs`  
**Dependencies:** Phase 3 (SaveWorkflowSessionCommand, GetWorkflowSessionQuery)  
**Estimated Time:** 4 hours

**Responsibilities:**
- Coordinate 3-step wizard flow
- Manage step navigation (Next/Previous)
- Validate each step before proceeding
- Persist session state between steps
- Handle Edit Mode returns

**Key Properties:**
```csharp
[ObservableProperty]
private Enum_Receiving_State_WorkflowStep _currentStep = Enum_Receiving_State_WorkflowStep.Step1_OrderAndPart;

[ObservableProperty]
private bool _step1Valid = false;

[ObservableProperty]
private bool _step2Valid = false;

[ObservableProperty]
private bool _allStepsValid = false;

[ObservableProperty]
private string _sessionId = Guid.NewGuid().ToString();

[ObservableProperty]
private bool _canGoNext = false;

[ObservableProperty]
private bool _canGoPrevious = false;

[ObservableProperty]
private string _poNumber = string.Empty;

[ObservableProperty]
private string _partNumber = string.Empty;

[ObservableProperty]
private int _loadCount = 0;

[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loads = new();
```

**Key Commands:**
```csharp
[RelayCommand(CanExecute = nameof(CanGoNext))]
private async Task GoToNextStepAsync()

[RelayCommand(CanExecute = nameof(CanGoPrevious))]
private async Task GoToPreviousStepAsync()

[RelayCommand]
private async Task SaveSessionStateAsync()

[RelayCommand]
private async Task LoadSessionStateAsync(string sessionId)

[RelayCommand]
private async Task ValidateCurrentStepAsync()

[RelayCommand]
private void CancelWorkflow()
```

**Business Logic:**
```csharp
private async Task GoToNextStepAsync()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;
        
        // Validate current step
        await ValidateCurrentStepAsync();
        
        if (CurrentStep == Step1 && !Step1Valid)
        {
            _errorHandler.ShowUserError("Please complete Step 1 before proceeding", "Validation Error");
            return;
        }
        
        // Save session state
        await SaveSessionStateAsync();
        
        // Move to next step
        CurrentStep = CurrentStep switch
        {
            Step1_OrderAndPart => Step2_LoadDetails,
            Step2_LoadDetails => Step3_ReviewAndSave,
            _ => CurrentStep
        };
        
        UpdateNavigationState();
    }
    finally
    {
        IsBusy = false;
    }
}

private void UpdateNavigationState()
{
    CanGoPrevious = CurrentStep != Step1_OrderAndPart;
    CanGoNext = CurrentStep switch
    {
        Step1_OrderAndPart => Step1Valid,
        Step2_LoadDetails => Step2Valid,
        _ => false
    };
}
```

**Acceptance Criteria:**
- [ ] 3-step navigation logic
- [ ] Step validation before proceeding
- [ ] Session state persistence
- [ ] Edit Mode return handling
- [ ] Cancel workflow handling
- [ ] Progress indicator updates
- [ ] Navigation button states

---

## ⏳ **Step 1 ViewModels (3 tasks)**

### Task 4.4: ViewModel_Receiving_Wizard_Display_PONumberEntry

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_PONumberEntry.cs`  
**Dependencies:** ValidatePONumberQuery (Phase 3)  
**Estimated Time:** 2 hours

**Responsibilities:**
- PO Number input and validation
- Auto-standardization (uppercase, trim)
- Real-time validation
- Non-PO mode handling

**Key Properties:**
```csharp
[ObservableProperty]
private string _poNumber = string.Empty;

[ObservableProperty]
private bool _isPOValid = false;

[ObservableProperty]
private string _poValidationMessage = string.Empty;

[ObservableProperty]
private bool _isNonPO = false;

[ObservableProperty]
private bool _isValidating = false;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task ValidatePONumberAsync()

[RelayCommand]
private void ClearPONumber()

[RelayCommand]
private async Task SearchPONumberAsync()
```

**Business Logic:**
```csharp
partial void OnPONumberChanged(string value)
{
    // Auto-standardize: uppercase, trim
    PONumber = value?.ToUpperInvariant().Trim() ?? string.Empty;
    
    // Debounced validation
    _validationTimer?.Stop();
    _validationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
    _validationTimer.Tick += async (s, e) =>
    {
        _validationTimer.Stop();
        await ValidatePONumberAsync();
    };
    _validationTimer.Start();
}

private async Task ValidatePONumberAsync()
{
    if (IsNonPO)
    {
        IsPOValid = true;
        POValidationMessage = string.Empty;
        return;
    }
    
    if (string.IsNullOrWhiteSpace(PONumber))
    {
        IsPOValid = false;
        POValidationMessage = "PO Number is required";
        return;
    }
    
    IsValidating = true;
    try
    {
        var query = new ValidatePONumberQuery { PONumber = PONumber };
        var result = await _mediator.Send(query);
        
        if (result.IsSuccess && result.Data != null)
        {
            IsPOValid = result.Data.IsValid;
            POValidationMessage = result.Data.ErrorMessage ?? string.Empty;
            
            if (result.Data.NormalizedPONumber != PONumber)
                PONumber = result.Data.NormalizedPONumber;
        }
    }
    finally
    {
        IsValidating = false;
    }
}
```

**Acceptance Criteria:**
- [ ] Real-time validation with debouncing
- [ ] Auto-standardization (uppercase, trim)
- [ ] Visual validation feedback
- [ ] Non-PO mode disables validation
- [ ] Clear error messages

---

### Task 4.5: ViewModel_Receiving_Wizard_Display_PartSelection

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_PartSelection.cs`  
**Dependencies:** GetPartDetailsQuery (Phase 3)  
**Estimated Time:** 2.5 hours

**Responsibilities:**
- Part number search and selection
- Auto-padding (6 digits)
- Part type auto-detection
- Part details display

**Key Properties:**
```csharp
[ObservableProperty]
private string _partNumber = string.Empty;

[ObservableProperty]
private bool _isPartValid = false;

[ObservableProperty]
private string _partValidationMessage = string.Empty;

[ObservableProperty]
private ObservableCollection<Model_Receiving_Entity_PartType> _availablePartTypes = new();

[ObservableProperty]
private Model_Receiving_Entity_PartType? _selectedPartType;

[ObservableProperty]
private string _partDescription = string.Empty;

[ObservableProperty]
private string _defaultReceivingLocation = string.Empty;

[ObservableProperty]
private string _defaultPackageType = string.Empty;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task ValidatePartNumberAsync()

[RelayCommand]
private async Task LoadPartDetailsAsync()

[RelayCommand]
private async Task SearchPartNumberAsync()

[RelayCommand]
private void SelectPartType(Model_Receiving_Entity_PartType partType)
```

**Business Logic:**
```csharp
partial void OnPartNumberChanged(string value)
{
    // Auto-padding: ensure 6 digits for part number suffix
    PartNumber = AutoPadPartNumber(value);
    
    // Debounced validation
    ScheduleValidation();
}

private string AutoPadPartNumber(string partNumber)
{
    if (string.IsNullOrWhiteSpace(partNumber)) return string.Empty;
    
    // MMC1 → MMC0001
    // MMC123 → MMC0123
    // MMC123456 → MMC123456
    var match = Regex.Match(partNumber, @"^([A-Z]+)(\d+)$");
    if (match.Success)
    {
        var prefix = match.Groups[1].Value;
        var number = match.Groups[2].Value.PadLeft(6, '0');
        return $"{prefix}{number}";
    }
    return partNumber.ToUpperInvariant();
}

private async Task LoadPartDetailsAsync()
{
    var query = new GetPartDetailsQuery { PartNumber = PartNumber };
    var result = await _mediator.Send(query);
    
    if (result.IsSuccess && result.Data != null)
    {
        SelectedPartType = result.Data.PartType;
        DefaultReceivingLocation = result.Data.DefaultLocation ?? "RECV";
        DefaultPackageType = result.Data.DefaultPackageType ?? "Skid";
        PartDescription = result.Data.Description ?? string.Empty;
    }
}
```

**Acceptance Criteria:**
- [ ] Auto-padding to 6 digits
- [ ] Part type auto-detection
- [ ] Part details loading
- [ ] Default values populated
- [ ] Search/autocomplete functionality

---

### Task 4.6: ViewModel_Receiving_Wizard_Display_LoadCountEntry

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_LoadCountEntry.cs`  
**Dependencies:** None  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Load count input (1-99)
- Generate load grid rows
- Initialize load numbers
- Apply part-specific defaults

**Key Properties:**
```csharp
[ObservableProperty]
private int _loadCount = 1;

[ObservableProperty]
private int _minLoadCount = 1;

[ObservableProperty]
private int _maxLoadCount = 99;

[ObservableProperty]
private bool _isLoadCountValid = false;

[ObservableProperty]
private string _loadCountValidationMessage = string.Empty;
```

**Key Commands:**
```csharp
[RelayCommand]
private void IncrementLoadCount()

[RelayCommand]
private void DecrementLoadCount()

[RelayCommand]
private async Task GenerateLoadRowsAsync()

[RelayCommand]
private void ValidateLoadCount()
```

**Business Logic:**
```csharp
partial void OnLoadCountChanged(int value)
{
    IsLoadCountValid = value >= MinLoadCount && value <= MaxLoadCount;
    LoadCountValidationMessage = IsLoadCountValid 
        ? string.Empty 
        : $"Load count must be between {MinLoadCount} and {MaxLoadCount}";
}

private async Task GenerateLoadRowsAsync()
{
    Loads.Clear();
    
    for (int i = 1; i <= LoadCount; i++)
    {
        var loadRow = new Model_Receiving_DataTransferObjects_LoadGridRow
        {
            LoadNumber = i,
            PONumber = PONumber,
            PartNumber = PartNumber,
            PartType = SelectedPartType?.PartTypeName ?? string.Empty,
            PackageType = DefaultPackageType,
            ReceivingLocation = DefaultReceivingLocation,
            // Weight, HeatLot, PackagesPerLoad remain null for user entry
        };
        
        Loads.Add(loadRow);
    }
}
```

**Acceptance Criteria:**
- [ ] Numeric input validation (1-99)
- [ ] Increment/decrement buttons
- [ ] Load row generation
- [ ] Default values applied
- [ ] Load number sequence (1, 2, 3...)

---

## ⏳ **Step 2 ViewModels (3 tasks)**

### Task 4.7: ViewModel_Receiving_Wizard_Display_LoadDetailsGrid

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Display_LoadDetailsGrid.cs`  
**Dependencies:** BulkCopyFieldsCommand, ClearAutoFilledFieldsCommand (Phase 3)  
**Estimated Time:** 3 hours

**Responsibilities:**
- Display editable DataGrid of all loads
- Row-level validation
- Cell edit handling
- Auto-calculate Weight Per Package
- Highlight validation errors

**Key Properties:**
```csharp
[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loads = new();

[ObservableProperty]
private Model_Receiving_DataTransferObjects_LoadGridRow? _selectedLoad;

[ObservableProperty]
private int _validLoadCount = 0;

[ObservableProperty]
private int _invalidLoadCount = 0;

[ObservableProperty]
private Dictionary<int, List<string>> _loadErrors = new();
```

**Key Commands:**
```csharp
[RelayCommand]
private void ValidateAllLoads()

[RelayCommand]
private void ValidateLoad(Model_Receiving_DataTransferObjects_LoadGridRow load)

[RelayCommand]
private void OnCellEditEnded(Model_Receiving_DataTransferObjects_LoadGridRow load, string propertyName)

[RelayCommand]
private void HighlightInvalidLoads()
```

**Business Logic:**
```csharp
private void ValidateLoad(Model_Receiving_DataTransferObjects_LoadGridRow load)
{
    var errors = new List<string>();
    
    // Weight validation
    if (!load.Weight.HasValue || load.Weight <= 0)
        errors.Add("Weight is required and must be positive");
    
    // Quantity validation
    if (!load.Quantity.HasValue || load.Quantity <= 0)
        errors.Add("Quantity is required and must be positive");
    
    // Heat/Lot validation (required for certain part types)
    if (string.IsNullOrWhiteSpace(load.HeatLot) && RequiresHeatLot(load.PartType))
        errors.Add("Heat/Lot is required for this part type");
    
    // Package validation
    if (string.IsNullOrWhiteSpace(load.PackageType))
        errors.Add("Package Type is required");
    
    if (!load.PackagesPerLoad.HasValue || load.PackagesPerLoad <= 0)
        errors.Add("Packages Per Load is required and must be positive");
    
    _loadErrors[load.LoadNumber] = errors;
    load.HasErrors = errors.Count > 0;
    load.ErrorMessage = string.Join("; ", errors);
}

private void OnCellEditEnded(Model_Receiving_DataTransferObjects_LoadGridRow load, string propertyName)
{
    // Auto-calculate Weight Per Package
    if (propertyName == nameof(load.Weight) || propertyName == nameof(load.PackagesPerLoad))
    {
        if (load.Weight.HasValue && load.PackagesPerLoad.HasValue && load.PackagesPerLoad > 0)
        {
            load.WeightPerPackage = load.Weight.Value / load.PackagesPerLoad.Value;
        }
    }
    
    // Revalidate load
    ValidateLoad(load);
}
```

**Acceptance Criteria:**
- [ ] DataGrid bound to observable collection
- [ ] Real-time validation
- [ ] Error highlighting
- [ ] Auto-calculate Weight Per Package
- [ ] Row selection support

---

### Task 4.8: ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations.cs`  
**Dependencies:** BulkCopyFieldsCommand, ClearAutoFilledFieldsCommand (Phase 3)  
**Estimated Time:** 2.5 hours

**Responsibilities:**
- Bulk copy field selection
- Copy preview dialog
- Execute bulk copy
- Clear auto-filled fields
- Track auto-fill state

**Key Properties:**
```csharp
[ObservableProperty]
private int _sourceLoadNumber = 1;

[ObservableProperty]
private ObservableCollection<Enum_Receiving_Type_CopyFieldSelection> _selectedFields = new();

[ObservableProperty]
private bool _copyHeatLot = true;

[ObservableProperty]
private bool _copyPackageType = true;

[ObservableProperty]
private bool _copyPackagesPerLoad = true;

[ObservableProperty]
private bool _copyReceivingLocation = false;

[ObservableProperty]
private int _affectedLoadCount = 0;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task PreviewCopyAsync()

[RelayCommand]
private async Task ExecuteCopyAsync()

[RelayCommand]
private async Task ClearAutoFilledFieldsAsync(int? targetLoadNumber = null)

[RelayCommand]
private void SelectAllFields()

[RelayCommand]
private void DeselectAllFields()
```

**Business Logic:**
```csharp
private async Task ExecuteCopyAsync()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;
        
        var selectedFieldsList = new List<Enum_Receiving_Type_CopyFieldSelection>();
        if (CopyHeatLot) selectedFieldsList.Add(Enum_Receiving_Type_CopyFieldSelection.HeatLot);
        if (CopyPackageType) selectedFieldsList.Add(Enum_Receiving_Type_CopyFieldSelection.PackageType);
        if (CopyPackagesPerLoad) selectedFieldsList.Add(Enum_Receiving_Type_CopyFieldSelection.PackagesPerLoad);
        if (CopyReceivingLocation) selectedFieldsList.Add(Enum_Receiving_Type_CopyFieldSelection.ReceivingLocation);
        
        var command = new BulkCopyFieldsCommand
        {
            TransactionId = TransactionId,
            SourceLoadNumber = SourceLoadNumber,
            FieldsToCopy = selectedFieldsList,
            ModifiedBy = CurrentUser
        };
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            AffectedLoadCount = result.Data;
            StatusMessage = $"Copied to {AffectedLoadCount} loads";
            
            // Refresh grid
            await RefreshLoadGridAsync();
        }
        else
        {
            _errorHandler.ShowUserError(result.ErrorMessage, "Copy Failed");
        }
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Acceptance Criteria:**
- [ ] Field selection checkboxes
- [ ] Preview before copy
- [ ] Copy to empty cells only
- [ ] Mark auto-filled loads
- [ ] Clear auto-filled option

---

### Task 4.9: ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog

**Priority:** P2 - MEDIUM  
**File:** `Module_Receiving/ViewModels/Wizard/Step2/ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog.cs`  
**Dependencies:** None  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Show copy preview
- Display affected loads
- Confirm or cancel copy
- Show before/after values

**Key Properties:**
```csharp
[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_CopyPreview> _previewItems = new();

[ObservableProperty]
private int _totalAffectedLoads = 0;

[ObservableProperty]
private bool _confirmCopy = false;
```

**Key Commands:**
```csharp
[RelayCommand]
private void ConfirmCopy()

[RelayCommand]
private void CancelCopy()
```

**Acceptance Criteria:**
- [ ] Preview list with before/after values
- [ ] Confirm/Cancel buttons
- [ ] Affected load count
- [ ] Dialog result handling

---

## ⏳ **Step 3 ViewModels (3 tasks)**

### Task 4.10: ViewModel_Receiving_Wizard_Display_ReviewSummary

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Display_ReviewSummary.cs`  
**Dependencies:** None  
**Estimated Time:** 2 hours

**Responsibilities:**
- Display transaction summary
- Show all loads in read-only table
- Calculate totals
- Quality Hold warnings
- Edit Mode entry point

**Key Properties:**
```csharp
[ObservableProperty]
private string _poNumber = string.Empty;

[ObservableProperty]
private string _partNumber = string.Empty;

[ObservableProperty]
private int _totalLoads = 0;

[ObservableProperty]
private decimal _totalWeight = 0;

[ObservableProperty]
private int _totalQuantity = 0;

[ObservableProperty]
private bool _hasQualityHold = false;

[ObservableProperty]
private string _qualityHoldMessage = string.Empty;

[ObservableProperty]
private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loadsReadOnly = new();
```

**Key Commands:**
```csharp
[RelayCommand]
private void EnterEditMode()

[RelayCommand]
private void RecalculateTotals()

[RelayCommand]
private void AcknowledgeQualityHold()
```

**Business Logic:**
```csharp
private void RecalculateTotals()
{
    TotalLoads = LoadsReadOnly.Count;
    TotalWeight = LoadsReadOnly.Sum(l => l.Weight ?? 0);
    TotalQuantity = LoadsReadOnly.Sum(l => l.Quantity ?? 0);
}

private void CheckQualityHold()
{
    // Check part preferences for RequiresQualityHold flag
    if (PartRequiresQualityHold)
    {
        HasQualityHold = true;
        QualityHoldMessage = "This part requires Quality Hold. Please follow QH procedures.";
    }
}
```

**Acceptance Criteria:**
- [ ] Summary table (read-only)
- [ ] Totals calculation
- [ ] Quality Hold warnings
- [ ] Edit Mode button
- [ ] Visual summary layout

---

### Task 4.11: ViewModel_Receiving_Wizard_Orchestration_SaveOperation

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Orchestration_SaveOperation.cs`  
**Dependencies:** SaveReceivingTransactionCommand, CompleteWorkflowCommand (Phase 3)  
**Estimated Time:** 3 hours

**Responsibilities:**
- Save transaction to database
- Export to CSV
- Complete workflow session
- Handle save errors
- Transaction rollback on failure

**Key Properties:**
```csharp
[ObservableProperty]
private bool _isSaving = false;

[ObservableProperty]
private string _saveProgress = string.Empty;

[ObservableProperty]
private bool _saveToCsv = true;

[ObservableProperty]
private bool _saveToDatabase = true;

[ObservableProperty]
private string _csvFilePath = string.Empty;
```

**Key Commands:**
```csharp
[RelayCommand]
private async Task SaveTransactionAsync()

[RelayCommand]
private async Task ExportToCSVAsync()

[RelayCommand]
private async Task CompleteWorkflowAsync()

[RelayCommand]
private void RetryFailedSave()
```

**Business Logic:**
```csharp
private async Task SaveTransactionAsync()
{
    if (IsSaving) return;
    try
    {
        IsSaving = true;
        SaveProgress = "Saving transaction...";
        
        // 1. Create transaction
        var transactionCommand = new SaveReceivingTransactionCommand
        {
            TransactionId = Guid.NewGuid().ToString(),
            PONumber = PONumber,
            PartNumber = PartNumber,
            TotalLoads = TotalLoads,
            TotalWeight = TotalWeight,
            TotalQuantity = TotalQuantity,
            Lines = LoadsReadOnly.ToList(),
            CreatedBy = CurrentUser
        };
        
        var saveResult = await _mediator.Send(transactionCommand);
        
        if (!saveResult.IsSuccess)
        {
            _errorHandler.ShowUserError(saveResult.ErrorMessage, "Save Failed");
            return;
        }
        
        SaveProgress = "Transaction saved to database";
        
        // 2. Export to CSV (if enabled)
        if (SaveToCsv)
        {
            SaveProgress = "Exporting to CSV...";
            CSVFilePath = await ExportToCSVAsync();
            SaveProgress = "CSV export complete";
        }
        
        // 3. Complete workflow
        SaveProgress = "Completing workflow...";
        var completeCommand = new CompleteWorkflowCommand
        {
            TransactionId = transactionCommand.TransactionId,
            SessionId = SessionId,
            CSVFilePath = CSVFilePath,
            CompletedBy = CurrentUser
        };
        
        var completeResult = await _mediator.Send(completeCommand);
        
        if (completeResult.IsSuccess)
        {
            SaveProgress = "Workflow completed successfully";
            await NavigateToCompletionScreenAsync();
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.High, 
            nameof(SaveTransactionAsync), nameof(ViewModel_Receiving_Wizard_Orchestration_SaveOperation));
        SaveProgress = "Save failed - see error details";
    }
    finally
    {
        IsSaving = false;
    }
}
```

**Acceptance Criteria:**
- [ ] Multi-step save process
- [ ] Progress indicator
- [ ] CSV export option
- [ ] Error handling with retry
- [ ] Transaction rollback on failure

---

### Task 4.12: ViewModel_Receiving_Wizard_Display_CompletionScreen

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/ViewModels/Wizard/Step3/ViewModel_Receiving_Wizard_Display_CompletionScreen.cs`  
**Dependencies:** None  
**Estimated Time:** 1.5 hours

**Responsibilities:**
- Display success message
- Show transaction summary
- Print labels option
- Start new transaction option
- Return to hub

**Key Properties:**
```csharp
[ObservableProperty]
private string _successMessage = "Transaction saved successfully!";

[ObservableProperty]
private string _transactionId = string.Empty;

[ObservableProperty]
private string _csvFilePath = string.Empty;

[ObservableProperty]
private bool _canPrintLabels = true;
```

**Key Commands:**
```csharp
[RelayCommand]
private void PrintLabels()

[RelayCommand]
private async Task StartNewTransactionAsync()

[RelayCommand]
private void ReturnToHub()

[RelayCommand]
private void OpenCSVFile()
```

**Acceptance Criteria:**
- [ ] Success message displayed
- [ ] Transaction ID shown
- [ ] CSV file path shown
- [ ] Print labels button
- [ ] Start new transaction button
- [ ] Return to hub button

---

## 🎨 **Views (XAML) - 28 files**

All XAML views will be created with their code-behind files in **Phase 6**.

**Hub Views (4 files):**
- View_Receiving_Hub_Orchestration_MainWorkflow.xaml + .cs
- View_Receiving_Hub_Display_ModeSelection.xaml + .cs

**Wizard Views (24 files):**
- Orchestration: 2 files (.xaml + .cs)
- Step 1: 6 files (3 views × 2 files each)
- Step 2: 4 files (2 views × 2 files each)
- Step 3: 4 files (2 views × 2 files each)

---

## 🔌 **DI Registration Task**

### Task 4.13: Register ViewModels & Views

**Priority:** P0 - CRITICAL  
**File:** `App.xaml.cs`  
**Dependencies:** All ViewModels complete  
**Estimated Time:** 30 minutes

**Registration Pattern:**
```csharp
// Hub ViewModels (Transient - new instance per navigation)
services.AddTransient<ViewModel_Receiving_Hub_Orchestration_MainWorkflow>();
services.AddTransient<ViewModel_Receiving_Hub_Display_ModeSelection>();

// Wizard ViewModels (Transient)
services.AddTransient<ViewModel_Receiving_Wizard_Orchestration_MainWorkflow>();
services.AddTransient<ViewModel_Receiving_Wizard_Display_PONumberEntry>();
// ... all other wizard ViewModels

// Views (Transient)
services.AddTransient<View_Receiving_Hub_Orchestration_MainWorkflow>();
services.AddTransient<View_Receiving_Hub_Display_ModeSelection>();
// ... all other views
```

**Acceptance Criteria:**
- [ ] All 14 ViewModels registered as Transient
- [ ] All Views registered as Transient
- [ ] Navigation service configured
- [ ] ViewModels can be resolved from container

---

## ✅ **Phase 4 Completion Criteria**

### ViewModels
- [ ] All 2 Hub ViewModels implemented
- [ ] All 12 Wizard ViewModels implemented
- [ ] All ViewModels inherit from `ViewModel_Shared_Base`
- [ ] All properties use `[ObservableProperty]`
- [ ] All commands use `[RelayCommand]`
- [ ] All ViewModels use IMediator (no direct service calls)

### Business Logic
- [ ] 3-step navigation working
- [ ] Step validation enforced
- [ ] Session state persistence
- [ ] Bulk copy operations
- [ ] Auto-fill tracking
- [ ] Quality Hold handling

### Integration
- [ ] All ViewModels registered in DI
- [ ] All CQRS commands/queries called via IMediator
- [ ] Error handling with IService_ErrorHandler
- [ ] Logging with IService_LoggingUtility

---

## 📝 **Notes & Guidelines**

### ViewModel Best Practices
1. **Partial Classes:** All ViewModels must be `partial`
2. **Base Class:** Inherit from `ViewModel_Shared_Base`
3. **IMediator:** Use for all data operations (no direct DAO/Service calls)
4. **IsBusy:** Set during async operations
5. **StatusMessage:** Update for user feedback
6. **Error Handling:** Use `_errorHandler.ShowUserError()` or `_errorHandler.HandleException()`

### MVVM Pattern
```csharp
public partial class ViewModel_Example : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    
    [ObservableProperty]
    private string _myProperty = string.Empty;
    
    public ViewModel_Example(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _mediator = mediator;
    }
    
    [RelayCommand]
    private async Task MyCommandAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Processing...";
            
            var command = new MyCommand { };
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
                StatusMessage = "Success";
            else
                _errorHandler.ShowUserError(result.ErrorMessage, "Error");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(MyCommandAsync), nameof(ViewModel_Example));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

---

## 🚀 **Next Steps After Phase 4**

Once all Wizard Mode ViewModels are complete:
1. **Phase 5:** Implement Manual & Edit Mode ViewModels (8 files)
2. **Phase 6:** Create all XAML Views (56 files)
3. Update IMPLEMENTATION_PROGRESS.md

---

**Total Phase 4 Tasks:** 42  
**ViewModels:** 14  
**Views:** 28 (deferred to Phase 6)  
**Estimated Time to Complete:** 30-35 hours

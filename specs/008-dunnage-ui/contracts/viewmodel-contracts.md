# ViewModel Contracts: Dunnage Wizard Workflow UI

**Purpose**: Define consistent command and property contracts across all Dunnage ViewModels to ensure uniform implementation patterns and testability.

---

## Main_DunnageLabelViewModel (Orchestrator)

**Responsibility**: Manages step visibility and coordinates workflow transitions.

**Properties**:
```csharp
// Step Visibility Flags
bool IsModeSelectionVisible { get; set; }
bool IsManualEntryVisible { get; set; }
bool IsEditModeVisible { get; set; }
bool IsTypeSelectionVisible { get; set; }
bool IsPartSelectionVisible { get; set; }
bool IsQuantityEntryVisible { get; set; }
bool IsDetailsEntryVisible { get; set; }
bool IsReviewVisible { get; set; }

// Status
bool IsStatusOpen { get; set; }
string StatusMessage { get; set; }
Enum_ErrorSeverity StatusSeverity { get; set; }
string CurrentStepTitle { get; set; }
```

**Commands**:
```csharp
IRelayCommand ReturnToModeSelectionCommand { get; }
```

**Events**:
- Subscribes to: `IService_DunnageWorkflow.StepChanged`

---

## Dunnage_ModeSelectionViewModel

**Responsibility**: Handle mode selection and default preference management.

**Properties**:
```csharp
bool IsGuidedModeDefault { get; set; }
bool IsManualModeDefault { get; set; }
bool IsEditModeDefault { get; set; }
```

**Commands**:
```csharp
IRelayCommand SelectGuidedModeCommand { get; }
IRelayCommand SelectManualModeCommand { get; }
IRelayCommand SelectEditModeCommand { get; }
IRelayCommand SetGuidedAsDefaultCommand { get; }
IRelayCommand SetManualAsDefaultCommand { get; }
IRelayCommand SetEditAsDefaultCommand { get; }
```

**Service Dependencies**:
- `IService_DunnageWorkflow` - Mode transition
- `IService_UserPreferences` - Default mode persistence
- `IService_SessionManager` - Current employee number
- `IService_ErrorHandler` - Error display
- `ILoggingService` - Audit trail

---

## Dunnage_TypeSelectionViewModel

**Responsibility**: Display paginated type grid and handle type selection.

**Properties**:
```csharp
ObservableCollection<Model_DunnageType> DisplayedTypes { get; set; }
Model_DunnageType? SelectedType { get; set; }
int CurrentPage { get; set; }
int TotalPages { get; set; }
bool CanGoNext { get; set; }
bool CanGoPrevious { get; set; }
```

**Commands**:
```csharp
IAsyncRelayCommand LoadTypesCommand { get; }
IAsyncRelayCommand SelectTypeCommand { get; }
IRelayCommand NextPageCommand { get; }
IRelayCommand PreviousPageCommand { get; }
IRelayCommand QuickAddTypeCommand { get; }
```

**Service Dependencies**:
- `IService_MySQL_Dunnage` - Type data retrieval
- `IService_Pagination` - Page calculation
- `IService_DunnageWorkflow` - Type selection and navigation
- `IService_ErrorHandler`
- `ILoggingService`

---

## Dunnage_PartSelectionViewModel

**Responsibility**: Display part dropdown, check inventory status, show notification.

**Properties**:
```csharp
ObservableCollection<Model_DunnagePart> AvailableParts { get; set; }
Model_DunnagePart? SelectedPart { get; set; }
bool IsInventoryNotificationVisible { get; set; }
string InventoryNotificationMessage { get; set; }
string InventoryMethod { get; set; } // "Adjust In" or "Receive In"
```

**Commands**:
```csharp
IAsyncRelayCommand LoadPartsCommand { get; }
IAsyncRelayCommand SelectPartCommand { get; }
IRelayCommand QuickAddPartCommand { get; }
IRelayCommand GoBackCommand { get; }
```

**Service Dependencies**:
- `IService_MySQL_Dunnage` - Part data retrieval, inventory check
- `IService_DunnageWorkflow` - Part selection and navigation
- `IService_ErrorHandler`
- `ILoggingService`

**Partial Methods**:
```csharp
partial void OnSelectedPartChanged(Model_DunnagePart? value);
```

---

## Dunnage_QuantityEntryViewModel

**Responsibility**: Capture and validate quantity input.

**Properties**:
```csharp
int Quantity { get; set; } // Default: 1
string SelectedTypeName { get; set; }
string SelectedPartName { get; set; }
bool IsQuantityValid { get; set; }
string ValidationMessage { get; set; }
```

**Commands**:
```csharp
IAsyncRelayCommand GoNextCommand { get; }
IRelayCommand GoBackCommand { get; }
```

**Service Dependencies**:
- `IService_DunnageWorkflow` - Quantity storage and navigation
- `IService_ErrorHandler`
- `ILoggingService`

**Validation**:
- Quantity > 0

---

## Dunnage_DetailsEntryViewModel

**Responsibility**: Capture PO, location, and dynamic spec inputs.

**Properties**:
```csharp
string PoNumber { get; set; }
string Location { get; set; }
ObservableCollection<SpecInputViewModel> SpecInputs { get; set; }
bool IsInventoryNotificationVisible { get; set; }
string InventoryNotificationMessage { get; set; }
string InventoryMethod { get; set; }
```

**Commands**:
```csharp
IAsyncRelayCommand GoNextCommand { get; }
IRelayCommand GoBackCommand { get; }
```

**Service Dependencies**:
- `IService_MySQL_Dunnage` - Spec schema retrieval
- `IService_DunnageWorkflow` - Details storage and navigation
- `IService_ErrorHandler`
- `ILoggingService`

**Partial Methods**:
```csharp
partial void OnPoNumberChanged(string value); // Updates InventoryMethod
```

**Helper ViewModel**:
```csharp
public class SpecInputViewModel : ObservableObject
{
    string SpecName { get; set; }
    string DataType { get; set; } // "text", "number", "boolean"
    object Value { get; set; }
    string Unit { get; set; }
    bool IsRequired { get; set; }
}
```

---

## Dunnage_ReviewViewModel

**Responsibility**: Display session loads, enable Save/Add Another/Cancel.

**Properties**:
```csharp
ObservableCollection<Model_DunnageLoad> SessionLoads { get; set; }
int LoadCount { get; set; }
bool CanSave { get; set; }
```

**Commands**:
```csharp
IAsyncRelayCommand AddAnotherCommand { get; }
IAsyncRelayCommand SaveAllCommand { get; }
IRelayCommand CancelCommand { get; }
```

**Service Dependencies**:
- `IService_DunnageWorkflow` - Session loads retrieval, save coordination
- `IService_MySQL_Dunnage` - Database persistence
- `IService_DunnageCSVWriter` - CSV export
- `IService_ErrorHandler`
- `ILoggingService`

---

## Dunnage_ManualEntryViewModel

**Responsibility**: Bulk grid entry with validation and utilities.

**Properties**:
```csharp
ObservableCollection<Model_DunnageLoad> Loads { get; set; }
Model_DunnageLoad? SelectedLoad { get; set; }
bool CanSave { get; set; }
```

**Commands**:
```csharp
IRelayCommand AddRowCommand { get; }
IRelayCommand AddMultipleCommand { get; }
IRelayCommand RemoveRowCommand { get; }
IAsyncRelayCommand AutoFillCommand { get; }
IAsyncRelayCommand SaveAllCommand { get; }
IAsyncRelayCommand FillBlankSpacesCommand { get; }
IRelayCommand SortForPrintingCommand { get; }
IAsyncRelayCommand SaveToHistoryCommand { get; }
IRelayCommand ReturnToModeSelectionCommand { get; }
```

**Service Dependencies**:
- `IService_DunnageWorkflow` - Mode navigation
- `IService_MySQL_Dunnage` - Database operations
- `IService_DunnageCSVWriter` - CSV export
- `IService_ErrorHandler`
- `ILoggingService`

---

## Dunnage_EditModeViewModel

**Responsibility**: Load, filter, edit, and delete historical loads.

**Properties**:
```csharp
ObservableCollection<Model_DunnageLoad> FilteredLoads { get; set; }
Model_DunnageLoad? SelectedLoad { get; set; }
DateTimeOffset FromDate { get; set; }
DateTimeOffset ToDate { get; set; }
int CurrentPage { get; set; }
int TotalPages { get; set; }
int PageSize { get; set; } // Default: 50
bool CanSave { get; set; }
```

**Commands**:
```csharp
IAsyncRelayCommand LoadFromMemoryCommand { get; }
IAsyncRelayCommand LoadFromLabelsCommand { get; }
IAsyncRelayCommand LoadFromHistoryCommand { get; }
IRelayCommand SelectAllCommand { get; }
IAsyncRelayCommand RemoveRowCommand { get; }
IAsyncRelayCommand SaveAllCommand { get; }
IRelayCommand SetDateRangeCommand { get; } // Last Week, Today, etc.
IRelayCommand FirstPageCommand { get; }
IRelayCommand PreviousPageCommand { get; }
IRelayCommand NextPageCommand { get; }
IRelayCommand LastPageCommand { get; }
IRelayCommand ReturnToModeSelectionCommand { get; }
```

**Service Dependencies**:
- `IService_DunnageWorkflow` - Mode navigation
- `IService_MySQL_Dunnage` - Date range queries, updates, deletes
- `IService_Pagination` - Page calculation
- `IService_ErrorHandler`
- `ILoggingService`

---

## Common Patterns

### Initialization Pattern
All step ViewModels implement:
```csharp
public async Task InitializeAsync()
{
    // Load data from workflow service or database
    await LoadDataAsync();
}
```

### Navigation Pattern
```csharp
// Forward
await _workflowService.GoToStep(Enum_DunnageWorkflowStep.NextStep);

// Backward
_workflowService.GoBack();

// Clear and return to mode selection
_workflowService.ClearSession();
await _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
```

### Error Handling Pattern
```csharp
try
{
    IsBusy = true;
    StatusMessage = "Loading...";
    
    var result = await _service.OperationAsync();
    if (result.IsSuccess)
    {
        // Success path
    }
    else
    {
        _errorHandler.ShowUserError(result.ErrorMessage, "Error", nameof(OperationAsync));
    }
}
catch (Exception ex)
{
    _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(OperationAsync), GetType().Name);
}
finally
{
    IsBusy = false;
}
```

### Validation Pattern
```csharp
[RelayCommand(CanExecute = nameof(CanExecuteNext))]
private async Task GoNextAsync()
{
    // Navigation logic
}

private bool CanExecuteNext()
{
    return !IsBusy && IsValid();
}

private bool IsValid()
{
    // Validation logic specific to step
    return Quantity > 0; // Example
}

// Notify when validation state changes
partial void OnQuantityChanged(int value)
{
    GoNextCommand.NotifyCanExecuteChanged();
}
```

---

## Testing Contracts

All ViewModels MUST be testable with:
- Mock services (Moq framework)
- Synchronous command execution in tests
- Property change notifications verified
- Command CanExecute logic validated

**Example Test Structure**:
```csharp
public class Dunnage_TypeSelectionViewModel_Tests
{
    private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
    private readonly Mock<IService_Pagination> _mockPaginationService;
    private readonly Mock<IService_DunnageWorkflow> _mockWorkflowService;
    private readonly Dunnage_TypeSelectionViewModel _viewModel;

    public Dunnage_TypeSelectionViewModel_Tests()
    {
        _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
        _mockPaginationService = new Mock<IService_Pagination>();
        _mockWorkflowService = new Mock<IService_DunnageWorkflow>();
        
        _viewModel = new Dunnage_TypeSelectionViewModel(
            _mockDunnageService.Object,
            _mockPaginationService.Object,
            _mockWorkflowService.Object,
            Mock.Of<IService_ErrorHandler>(),
            Mock.Of<ILoggingService>()
        );
    }

    [Fact]
    public async Task LoadTypesCommand_ShouldPopulateDisplayedTypes()
    {
        // Arrange
        var types = new List<Model_DunnageType> { /* test data */ };
        _mockDunnageService
            .Setup(s => s.GetAllTypesAsync())
            .ReturnsAsync(Model_Dao_Result<List<Model_DunnageType>>.Success(types));

        // Act
        await _viewModel.LoadTypesCommand.ExecuteAsync(null);

        // Assert
        Assert.NotEmpty(_viewModel.DisplayedTypes);
        _mockDunnageService.Verify(s => s.GetAllTypesAsync(), Times.Once);
    }
}
```

---

## Summary

All ViewModels follow:
- ✅ Inherit from `BaseViewModel` or use `ObservableObject`
- ✅ Partial classes for `[ObservableProperty]` and `[RelayCommand]`
- ✅ Constructor injection of services
- ✅ Async commands for I/O operations
- ✅ Validation with `CanExecute` logic
- ✅ Consistent error handling via `IService_ErrorHandler`
- ✅ Logging via `ILoggingService`
- ✅ Navigation via `IService_DunnageWorkflow`
- ✅ Zero direct DAO access (Service→DAO delegation)

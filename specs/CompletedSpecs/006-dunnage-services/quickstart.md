# Quickstart: Dunnage Services Layer

**Feature**: Dunnage Services Layer  
**Date**: 2025-12-26  
**Audience**: ViewModel developers, future maintainers

## Overview

The Dunnage Services Layer provides three core services for the dunnage receiving workflow:
1. **IService_DunnageWorkflow** - Wizard state machine
2. **IService_MySQL_Dunnage** - Database operations wrapper
3. **IService_DunnageCSVWriter** - CSV export for label printing

This guide shows how to use these services in ViewModels and understand their interactions.

---

## Registration (App.xaml.cs)

Ensure services are registered in `ConfigureServices()`:

```csharp
// Singleton for stateful workflow
services.AddSingleton<IService_DunnageWorkflow, Service_DunnageWorkflow>();

// Transient for stateless operations
services.AddTransient<IService_MySQL_Dunnage, Service_MySQL_Dunnage>();
services.AddTransient<IService_DunnageCSVWriter, Service_DunnageCSVWriter>();
```

---

## Common Patterns

### Pattern 1: Starting the Workflow

**Scenario**: User clicks "Receive Dunnage" button from main menu.

```csharp
public partial class DunnageHomeViewModel : BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflow;
    private readonly INavigationService _navigation;

    public DunnageHomeViewModel(
        IService_DunnageWorkflow workflow,
        INavigationService navigation,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _workflow = workflow;
        _navigation = navigation;
        
        // Subscribe to workflow events
        _workflow.StepChanged += OnWorkflowStepChanged;
        _workflow.StatusMessageRaised += OnStatusMessage;
    }

    [RelayCommand]
    private async Task StartReceivingAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Initializing workflow...";
            
            var success = await _workflow.StartWorkflowAsync();
            if (success) {
                // Navigation triggered by StepChanged event
            } else {
                _errorHandler.ShowUserError("Failed to start workflow", "Error", nameof(StartReceivingAsync));
            }
        }
        catch (Exception ex) {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, 
                nameof(StartReceivingAsync), nameof(DunnageHomeViewModel));
        }
        finally { IsBusy = false; }
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        // Navigate to corresponding page
        switch (_workflow.CurrentStep)
        {
            case Enum_DunnageWorkflowStep.ModeSelection:
                _navigation.NavigateTo<ModeSelectionViewModel>();
                break;
            case Enum_DunnageWorkflowStep.TypeSelection:
                _navigation.NavigateTo<TypeSelectionViewModel>();
                break;
            // ... other steps
        }
    }

    private void OnStatusMessage(object? sender, string message)
    {
        StatusMessage = message;
    }
}
```

---

### Pattern 2: Loading Data from Database

**Scenario**: TypeSelectionViewModel needs to load all types from database.

```csharp
public partial class TypeSelectionViewModel : BaseViewModel
{
    private readonly IService_MySQL_Dunnage _dbService;
    private readonly IService_DunnageWorkflow _workflow;

    [ObservableProperty]
    private ObservableCollection<Model_DunnageType> _types;

    public TypeSelectionViewModel(
        IService_MySQL_Dunnage dbService,
        IService_DunnageWorkflow workflow,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _dbService = dbService;
        _workflow = workflow;
        Types = new ObservableCollection<Model_DunnageType>();
    }

    public async Task LoadTypesAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Loading dunnage types...";
            
            var result = await _dbService.GetAllTypesAsync();
            if (result.IsSuccess)
            {
                Types.Clear();
                foreach (var type in result.Data) {
                    Types.Add(type);
                }
                StatusMessage = $"Loaded {Types.Count} types";
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage, 
                    "Load Error", 
                    nameof(LoadTypesAsync));
            }
        }
        catch (Exception ex) {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, 
                nameof(LoadTypesAsync), nameof(TypeSelectionViewModel));
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SelectTypeAsync(Model_DunnageType type)
    {
        // Update workflow session
        _workflow.CurrentSession.SelectedType = type;
        
        // Advance to next step
        var result = await _workflow.AdvanceToNextStepAsync();
        if (!result.IsSuccess) {
            _errorHandler.ShowUserError(result.ErrorMessage, "Validation Error", nameof(SelectTypeAsync));
        }
        // Navigation triggered by StepChanged event
    }
}
```

---

### Pattern 3: Saving Session

**Scenario**: User reviews data and clicks "Save" button.

```csharp
public partial class ReviewViewModel : BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflow;

    [ObservableProperty]
    private ObservableCollection<Model_DunnageLoad> _pendingLoads;

    public ReviewViewModel(
        IService_DunnageWorkflow workflow,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _workflow = workflow;
        PendingLoads = new ObservableCollection<Model_DunnageLoad>();
        
        // Bind to workflow session
        PendingLoads = new ObservableCollection<Model_DunnageLoad>(_workflow.CurrentSession.PendingLoads);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Saving session...";
            
            var result = await _workflow.SaveSessionAsync();
            if (result.IsSuccess)
            {
                StatusMessage = $"Saved {result.RecordsSaved} loads";
                
                // Check CSV export status
                if (result.CSVExportResult?.LocalSuccess == true)
                {
                    _logger.LogInfo($"CSV exported to {result.CSVExportResult.LocalFilePath}");
                    
                    if (!result.CSVExportResult.NetworkSuccess) {
                        // Warn about network failure but don't block
                        StatusMessage += " (Network CSV failed - see logs)";
                    }
                }
                
                // Navigate back to home
                // ... navigation code
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage, 
                    "Save Failed", 
                    nameof(SaveAsync));
            }
        }
        catch (Exception ex) {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.High, 
                nameof(SaveAsync), nameof(ReviewViewModel));
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void ClearSession()
    {
        _workflow.ClearSession();
        PendingLoads.Clear();
        StatusMessage = "Session cleared";
    }
}
```

---

### Pattern 4: Deleting with Impact Analysis

**Scenario**: Admin UI needs to delete a type, but must check for dependencies first.

```csharp
public partial class TypeAdminViewModel : BaseViewModel
{
    private readonly IService_MySQL_Dunnage _dbService;

    [RelayCommand]
    private async Task DeleteTypeAsync(Model_DunnageType type)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            
            // Check impact before deleting
            int partCount = await _dbService.GetPartCountByTypeIdAsync(type.TypeID);
            if (partCount > 0)
            {
                // Ask user for confirmation
                bool confirmed = await _dialogService.ShowConfirmationAsync(
                    $"Type '{type.TypeName}' has {partCount} parts. Delete anyway?",
                    "Confirm Delete");
                
                if (!confirmed) return;
            }
            
            // Proceed with delete
            var result = await _dbService.DeleteTypeAsync(type.TypeID);
            if (result.IsSuccess)
            {
                StatusMessage = $"Deleted type '{type.TypeName}'";
                await LoadTypesAsync(); // Refresh list
            }
            else
            {
                _errorHandler.ShowUserError(result.ErrorMessage, "Delete Failed", nameof(DeleteTypeAsync));
            }
        }
        catch (Exception ex) {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, 
                nameof(DeleteTypeAsync), nameof(TypeAdminViewModel));
        }
        finally { IsBusy = false; }
    }
}
```

---

### Pattern 5: Searching Parts

**Scenario**: User types in search box to find parts.

```csharp
public partial class PartSelectionViewModel : BaseViewModel
{
    private readonly IService_MySQL_Dunnage _dbService;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<Model_DunnagePart> _parts;
    
    partial void OnSearchTextChanged(string value)
    {
        // Debounce search
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _searchDebounceTimer.Tick += async (s, e) =>
        {
            _searchDebounceTimer.Stop();
            await SearchPartsAsync();
        };
        _searchDebounceTimer.Start();
    }

    private async Task SearchPartsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) {
            await LoadAllPartsAsync();
            return;
        }

        try
        {
            var typeId = _workflow.CurrentSession.SelectedType?.TypeID;
            var result = await _dbService.SearchPartsAsync(SearchText, typeId);
            
            if (result.IsSuccess)
            {
                Parts.Clear();
                foreach (var part in result.Data) {
                    Parts.Add(part);
                }
            }
        }
        catch (Exception ex) {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Low, 
                nameof(SearchPartsAsync), nameof(PartSelectionViewModel));
        }
    }
}
```

---

## Testing Examples

### Unit Test: Workflow Step Validation

```csharp
[Fact]
public async Task AdvanceToNextStep_WithoutSelectedType_ReturnsError()
{
    // Arrange
    var mockMySql = new Mock<IService_MySQL_Dunnage>();
    var mockCsv = new Mock<IService_DunnageCSVWriter>();
    var workflow = new Service_DunnageWorkflow(mockMySql.Object, mockCsv.Object, ...);
    
    await workflow.StartWorkflowAsync();
    workflow.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
    
    // Act
    var result = await workflow.AdvanceToNextStepAsync();
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Type must be selected", result.ErrorMessage);
}
```

---

### Unit Test: Impact Analysis

```csharp
[Fact]
public async Task DeleteType_WithDependentParts_ReturnsError()
{
    // Arrange
    var mockDao = new Mock<Dao_DunnageType>();
    var service = new Service_MySQL_Dunnage(...);
    
    // Simulate 5 dependent parts
    mockDao.Setup(d => d.GetPartCountByTypeIdAsync(1)).ReturnsAsync(5);
    
    // Act
    var result = await service.DeleteTypeAsync(1);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("5 parts depend on it", result.ErrorMessage);
}
```

---

## Common Errors & Solutions

### Error: "Type must be selected before advancing"
**Cause**: Workflow validation failed  
**Solution**: Ensure `_workflow.CurrentSession.SelectedType` is set before calling `AdvanceToNextStepAsync()`

### Error: "Local CSV write failed"
**Cause**: Insufficient permissions or disk space  
**Solution**: Check %APPDATA% write permissions, verify disk space

### Error: "Cannot delete type: 15 parts depend on it"
**Cause**: Impact analysis detected dependencies  
**Solution**: Delete or reassign parts first, or show confirmation dialog

### Error: "Network CSV write failed: Path not found"
**Cause**: Network share unavailable  
**Solution**: This is a warning, not an error - local CSV succeeded, operation continues

---

## Best Practices

1. **Always check result.IsSuccess** before accessing `result.Data`
2. **Subscribe to workflow events** for automatic navigation
3. **Use impact analysis** before delete operations
4. **Handle network failures gracefully** - CSV export is best-effort
5. **Clear session on cancel** - prevent stale data across workflows
6. **Log all operations** - use `ILoggingService` for audit trail
7. **Show user feedback** - use StatusMessage for progress updates

---

## Quick Reference

| Task | Service Method |
|------|---------------|
| Start wizard | `IService_DunnageWorkflow.StartWorkflowAsync()` |
| Advance step | `IService_DunnageWorkflow.AdvanceToNextStepAsync()` |
| Save session | `IService_DunnageWorkflow.SaveSessionAsync()` |
| Load types | `IService_MySQL_Dunnage.GetAllTypesAsync()` |
| Search parts | `IService_MySQL_Dunnage.SearchPartsAsync(text, typeId)` |
| Check dependencies | `IService_MySQL_Dunnage.GetPartCountByTypeIdAsync(typeId)` |
| Export CSV | `IService_DunnageCSVWriter.WriteToCSVAsync(loads)` |
| Get spec keys | `IService_MySQL_Dunnage.GetAllSpecKeysAsync()` |

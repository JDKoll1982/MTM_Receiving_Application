---
description: Guidelines for creating and working with ViewModels in the MTM Receiving Application
applyTo: '**/*ViewModel.cs'
---

# ViewModel Development Guidelines

## Purpose

This file provides guidelines for creating ViewModels following the MVVM pattern with CommunityToolkit.Mvvm.

## Core Principles

1. **All ViewModels must inherit from BaseViewModel**
   - Located at: `ViewModels/Shared/Shared_BaseViewModel.cs`
   - Provides: `IsBusy`, `StatusMessage`, error handling, logging

2. **Use CommunityToolkit.Mvvm attributes**
   - `[ObservableProperty]` for properties that notify on change
   - `[RelayCommand]` for synchronous commands
   - `[RelayCommand]` with `async Task` for asynchronous commands

3. **Constructor Requirements**
   - Always accept `IService_ErrorHandler` and `ILoggingService` via DI
   - Call base constructor: `base(errorHandler, logger)`
   - Initialize collections in constructor

## Property Guidelines

### Observable Properties

```csharp
[ObservableProperty]
private string _partID = string.Empty;
// Generates public string PartID property with INotifyPropertyChanged
```

### Collections

```csharp
// Initialize in constructor
public ObservableCollection<Model_ReceivingLine> ReceivingLines { get; }

public MyViewModel(...)
{
    ReceivingLines = new ObservableCollection<Model_ReceivingLine>();
}
```

## Command Guidelines

### Synchronous Commands

```csharp
[RelayCommand]
private void SortData()
{
    try
    {
        // Command logic
        StatusMessage = "Data sorted";
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.Low, 
            callerName: nameof(SortData), controlName: "MyViewModel");
    }
}
// Generates: SortDataCommand property
```

### Asynchronous Commands

```csharp
[RelayCommand]
private async Task SaveDataAsync()
{
    if (IsBusy) return;
    
    try
    {
        IsBusy = true;
        StatusMessage = "Saving...";
        
        var result = await Dao_MyData.SaveAsync(data);
        
        if (result.IsSuccess)
        {
            StatusMessage = "Saved successfully";
        }
        else
        {
            _errorHandler.ShowUserError(result.ErrorMessage, "Save Error", nameof(SaveDataAsync));
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium,
            callerName: nameof(SaveDataAsync), controlName: "MyViewModel");
    }
    finally
    {
        IsBusy = false;
    }
}
// Generates: SaveDataCommand property
```

## Error Handling

### User-Facing Errors

```csharp
// For validation errors or expected failures
_errorHandler.ShowUserError("Part ID is required", "Validation Error", nameof(AddLineAsync));
```

### Exception Handling

```csharp
catch (Exception ex)
{
    _errorHandler.HandleException(
        ex,
        Enum_ErrorSeverity.Medium,  // Low, Medium, High, Critical
        callerName: nameof(MethodName),
        controlName: "ViewModelName"
    );
}
```

## Database Operations

### All database calls must be async

```csharp
var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);

if (result.IsSuccess)
{
    // Success handling
}
else
{
    _errorHandler.ShowUserError(result.ErrorMessage, "Database Error", nameof(MethodName));
}
```

## Status Messages

Update `StatusMessage` to provide feedback:

```csharp
StatusMessage = "Loading data...";
// ... perform operation ...
StatusMessage = "Data loaded successfully";
```

## Registration in DI Container

Add ViewModels to `App.xaml.cs`:

```csharp
services.AddTransient<MyViewModel>();
```

## File Location

- Base: `ViewModels/Shared/Shared_BaseViewModel.cs`
- Feature-specific: `ViewModels/[FeatureName]/[Feature]ViewModel.cs`
- Example: `ViewModels/Main/Main_ReceivingLabelViewModel.cs`

## Common Patterns

### Resetting Forms

```csharp
CurrentLine = new Model_ReceivingLine();
OnPropertyChanged(nameof(CurrentLine));
```

### Collection Updates

```csharp
ReceivingLines.Add(newLine);
TotalRows = ReceivingLines.Count;
```

## Things to Avoid

❌ Don't put UI logic in ViewModels
❌ Don't reference View types in ViewModels
❌ Don't use `async void` (use `async Task` instead)
❌ Don't manually implement `INotifyPropertyChanged` (use `ObservableProperty`)
❌ Don't create commands without try-catch blocks
❌ Don't perform long operations without setting `IsBusy = true`

## Template

```csharp
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.FeatureName;

public partial class MyViewModel : BaseViewModel
{
    public MyViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(errorHandler, logger)
    {
        // Initialize collections
    }

    // Observable properties
    [ObservableProperty]
    private string _myProperty = string.Empty;

    // Collections
    public ObservableCollection<MyModel> Items { get; } = new();

    // Commands
    [RelayCommand]
    private async Task DoSomethingAsync()
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            StatusMessage = "Working...";
            
            // Logic here
            
            StatusMessage = "Complete";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium,
                callerName: nameof(DoSomethingAsync), controlName: nameof(MyViewModel));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

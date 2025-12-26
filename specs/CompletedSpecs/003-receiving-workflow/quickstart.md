# Quick Start Guide: Multi-Step Receiving Workflow

**Feature**: 003-database-foundation  
**For**: Developers implementing this feature

## Overview

This guide helps you quickly understand and implement the receiving workflow feature. For complete details, see [spec.md](./spec.md), [data-model.md](./data-model.md), and [research.md](./research.md).

---

## Architecture Quick Reference

**Pattern**: MVVM with dependency injection  
**Navigation**: UserControl-based steps within single ReceivingWorkflowView  
**State**: Session persisted to JSON, ViewModels coordinate via IService_ReceivingWorkflow  
**Database**: SQL Server (Infor Visual, read-only) + MySQL (receiving data, read/write)

---

## Implementation Checklist

### Phase 1: Models & Contracts ✓

- [x] Create Models in `Models/Receiving/`
  - Model_ReceivingLoad
  - Model_ReceivingSession
  - Model_InforVisualPO
  - Model_InforVisualPart
  - Model_PackageTypePreference
  - Model_HeatCheckboxItem

- [x] Create Contract Specifications (as .md planning docs)
  - IService_InforVisual.md
  - IService_MySQL_Receiving.md
  - IService_MySQL_PackagePreferences.md
  - IService_SessionManager.md
  - IService_CSVWriter.md
  - IService_ReceivingValidation.md
  - IService_ReceivingWorkflow.md
  
  Note: These will be implemented as actual .cs files in `Contracts/Services/` during Phase 3

### Phase 2: Database Setup

- [ ] SQL Server Stored Procedures (Infor Visual)
  ```sql
  Database/StoredProcedures/Receiving/
  ├── sp_GetPOWithParts.sql
  ├── sp_GetPartByID.sql
  └── sp_GetReceivingByPOPartDate.sql
  ```

- [ ] MySQL Tables
  ```sql
  Database/Schemas/
  ├── 03_create_receiving_tables.sql
  └── 04_create_package_preferences.sql
  ```

- [ ] Run migrations/setup scripts

### Phase 3: Services

- [ ] Implement database services
  - Service_InforVisual
  - Service_MySQL_Receiving
  - Service_MySQL_PackagePreferences

- [ ] Implement application services
  - Service_SessionManager
  - Service_CSVWriter
  - Service_ReceivingValidation
  - Service_ReceivingWorkflow

- [ ] Register services in `App.xaml.cs`
  ```csharp
  services.AddSingleton<IService_InforVisual, Service_InforVisual>();
  services.AddSingleton<IService_MySQL_Receiving, Service_MySQL_Receiving>();
  // ... etc
  ```

### Phase 4: ViewModels

- [ ] Create ViewModels in `ViewModels/Receiving/`
  - ReceivingWorkflowViewModel (main coordinator)
  - POEntryViewModel
  - LoadEntryViewModel
  - WeightQuantityViewModel
  - HeatLotViewModel
  - PackageTypeViewModel
  - ReviewGridViewModel

- [ ] Implement ViewModel logic
  - Property bindings
  - Command handlers
  - Service injections
  - Validation calls

### Phase 5: Views

- [ ] Create Views in `Views/Receiving/`
  - ReceivingWorkflowView.xaml (parent container)
  - POEntryView.xaml (Step 1-2)
  - LoadEntryView.xaml (Step 3)
  - WeightQuantityView.xaml (Step 4)
  - HeatLotView.xaml (Step 5)
  - PackageTypeView.xaml (Step 6)
  - ReviewGridView.xaml (Step 7)

- [ ] Implement XAML bindings
  - Data bindings to ViewModels
  - Command bindings
  - Visibility bindings for step transitions
  - DataGrid configuration for review

- [ ] Implement code-behind (minimal)
  - ViewModel instantiation
  - Event handlers for DataGrid (cascading updates)

### Phase 6: Integration

- [ ] Add navigation entry in MainWindow.xaml
  ```xaml
  <NavigationViewItem Content="Receiving" Tag="ReceivingWorkflowView"/>
  ```

- [ ] Wire up navigation in MainWindow code-behind

- [ ] Add CSV reset dialog on startup (App.xaml.cs OnLaunched)

### Phase 7: Testing

- [ ] Unit tests for ViewModels
  ```csharp
  MTM_Receiving_Application.Tests/Unit/ViewModels/Receiving/
  └── ReceivingWorkflowViewModelTests.cs
  ```

- [ ] Unit tests for Services
  ```csharp
  MTM_Receiving_Application.Tests/Unit/Services/Receiving/
  ├── Service_ReceivingValidationTests.cs
  └── Service_CSVWriterTests.cs
  ```

- [ ] Integration tests for Database
  ```csharp
  MTM_Receiving_Application.Tests/Integration/Database/
  ├── Service_InforVisualTests.cs
  └── Service_MySQL_ReceivingTests.cs
  ```

---

## Key Implementation Patterns

### 1. Step Visibility Pattern

```csharp
// In ReceivingWorkflowViewModel
public Visibility IsPOEntryVisible => 
    CurrentStep == WorkflowStep.POEntry ? Visibility.Visible : Visibility.Collapsed;

public Visibility IsWeightEntryVisible => 
    CurrentStep == WorkflowStep.WeightQuantityEntry ? Visibility.Visible : Visibility.Collapsed;

// ... for each step
```

### 2. Service Injection Pattern

```csharp
public class ReceivingWorkflowViewModel : ObservableObject
{
    private readonly IService_ReceivingWorkflow _workflowService;
    private readonly IService_InforVisual _inforVisualService;
    private readonly IService_ReceivingValidation _validationService;

    public ReceivingWorkflowViewModel(
        IService_ReceivingWorkflow workflowService,
        IService_InforVisual inforVisualService,
        IService_ReceivingValidation validationService)
    {
        _workflowService = workflowService;
        _inforVisualService = inforVisualService;
        _validationService = validationService;
    }
}
```

### 3. Async Command Pattern

```csharp
[RelayCommand]
private async Task LoadPOAsync()
{
    var validation = _validationService.ValidatePONumber(PONumber);
    if (!validation.IsValid)
    {
        ShowError(validation.Message);
        return;
    }

    IsLoading = true;
    try
    {
        var po = await _inforVisualService.GetPOWithPartsAsync(PONumber);
        if (po == null || !po.HasParts)
        {
            ShowError($"PO {PONumber} not found or has no parts");
            return;
        }

        AvailableParts = new ObservableCollection<Model_InforVisualPart>(po.Parts);
        ShowStatus($"PO {PONumber} loaded. {po.Parts.Count} parts found.");
    }
    catch (Exception ex)
    {
        ShowError($"Error loading PO: {ex.Message}");
    }
    finally
    {
        IsLoading = false;
    }
}
```

### 4. Cascading Update Pattern (Review Grid)

```csharp
private void DataGrid_CellEditEnding(object sender, CellEditEndingEventArgs e)
{
    if (e.EditAction == DataGridEditAction.Cancel) return;

    var load = e.Row.DataContext as Model_ReceivingLoad;
    var columnTag = e.Column.Tag?.ToString();

    if (columnTag == "PartID")
    {
        var textBox = e.EditingElement as TextBox;
        var newPartID = textBox?.Text;
        
        if (load != null && !string.IsNullOrEmpty(newPartID))
        {
            var oldPartID = load.PartID;
            
            // Cascading update: all loads with old part ID get new part ID
            foreach (var l in AllLoads.Where(x => x.PartID == oldPartID))
            {
                l.PartID = newPartID;
            }
            
            ShowStatus($"Updated part ID from {oldPartID} to {newPartID} for all matching loads");
        }
    }
    else if (columnTag == "PONumber")
    {
        var textBox = e.EditingElement as TextBox;
        var newPONumber = textBox?.Text;
        
        if (load != null && !string.IsNullOrEmpty(newPONumber))
        {
            var partID = load.PartID;
            
            // Cascading update: all loads with same part ID get new PO number
            foreach (var l in AllLoads.Where(x => x.PartID == partID))
            {
                l.PONumber = newPONumber;
            }
            
            ShowStatus($"Updated PO number to {newPONumber} for all loads of part {partID}");
        }
    }
}
```

### 5. Session Persistence Pattern

```csharp
// Auto-save after each step
private async Task AdvanceToNextStep()
{
    var result = await _workflowService.AdvanceToNextStepAsync();
    if (result.Success)
    {
        // Save session after successful step transition
        await _workflowService.PersistSessionAsync();
        CurrentStep = result.NewStep;
        OnPropertyChanged(nameof(CurrentStepTitle));
        // Update all visibility properties
    }
    else
    {
        ShowErrors(result.ValidationErrors);
    }
}
```

---

## Testing Strategy

### Unit Tests

Focus on:
- ViewModel command logic
- Validation service rules
- CSV generation logic (file operations mocked)
- Cascading update logic

### Integration Tests

Focus on:
- Database queries (Infor Visual)
- Database inserts (MySQL)
- File I/O (session, CSV)
- End-to-end workflow transitions

### Manual Testing Scenarios

1. **Happy Path**: Enter PO, select part, 3 loads, complete workflow, save
2. **Non-PO Path**: Enter non-PO item, complete workflow
3. **Multiple Parts**: Add 2 parts to session, save together
4. **Cascading Updates**: Edit part# in review grid, verify all loads update
5. **Network Failure**: Disable network, verify graceful CSV fallback
6. **Session Restore**: Close app mid-entry, reopen, verify restore

---

## Common Gotchas

1. **DataGrid Editing**: Use CellEditEnding event, NOT CellEditEnded (too late)
2. **Async/Await**: Always use async for I/O operations (database, file, network)
3. **ObservableCollection**: Use this for collections bound to UI, not List<>
4. **Nullable PONumber**: Remember null coalescing when displaying ("N/A")
5. **WeightPerPackage**: Recalculate on both WeightQuantity and PackagesPerLoad changes
6. **Session JSON**: Handle JsonException gracefully (corrupted file scenario)
7. **Network Paths**: Use try-catch for network CSV, never block on failure

---

## Performance Considerations

- **PO Queries**: < 2 seconds (add index on PONumber in Infor Visual)
- **Save Operation**: Batch MySQL INSERTs (transaction), < 5 seconds for 50 loads
- **Review Grid**: Use virtualization for large load counts (DataGrid default)
- **Session JSON**: Write async, read async, don't block UI thread

---

## Next Steps After Implementation

1. Run all tests (unit + integration)
2. Manual test all user stories from spec.md
3. Performance test with 50+ loads
4. Test network failure scenarios
5. Test session restore after crash
6. Code review with team
7. Update documentation with any changes
8. Deploy to test environment

---

## Getting Help

- **Spec Questions**: See [spec.md](./spec.md) for requirements
- **Technical Decisions**: See [research.md](./research.md) for rationale
- **Data Structure**: See [data-model.md](./data-model.md) for entities
- **API Contracts**: See [contracts/](./contracts/) for service interfaces

---

**Last Updated**: December 17, 2025  
**Status**: Ready for implementation

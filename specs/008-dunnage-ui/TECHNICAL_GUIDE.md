# Dunnage UI - Technical Implementation Guide

**Purpose**: Quick reference for developers working on the Dunnage UI feature  
**Status**: 85% Complete (Phases 3-9 ViewModels & Tests done, Views partially complete)  
**Last Updated**: December 27, 2024

---

## Architecture Overview

### MVVM Pattern
```
View (XAML)
  ↓ x:Bind (compile-time binding)
ViewModel (Business Logic)
  ↓ IService_* interfaces
Service (Data Operations)
  ↓ Dao_* classes
Database (MySQL)
```

### Dependency Injection Flow
```csharp
App.xaml.cs
  → ConfigureServices()
    → services.AddTransient<Dunnage_*ViewModel>()
    → services.AddSingleton<IService_MySQL_Dunnage>()
    → services.AddSingleton<IService_DunnageWorkflow>()
```

---

## Workflow State Machine

```
┌─────────────────────────────────────────────────────────┐
│ Mode Selection                                          │
│   QuickAdd / ManualEntry / EditMode                     │
└────┬──────────────────────────────────────────────┬─────┘
     │ QuickAdd                                      │ EditMode
     ↓                                               ↓
┌─────────────────┐                        ┌────────────────────┐
│ Type Selection  │                        │ Edit Mode          │
│   3x3 Grid      │                        │   Date Filter      │
└────┬────────────┘                        │   Pagination       │
     ↓                                     │   Bulk Edit        │
┌─────────────────┐                        └────────────────────┘
│ Part Selection  │                                 │
│   by Type       │                                 ↓
└────┬────────────┘                        ┌────────────────────┐
     ↓                                     │ Save All           │
┌─────────────────┐                        │   CSV Export       │
│ Quantity Entry  │                        └────────────────────┘
│   Number Input  │
└────┬────────────┘
     ↓
┌─────────────────┐
│ Details Entry   │
│   PO/Location   │
│   Dynamic Specs │
└────┬────────────┘
     ↓
┌─────────────────┐
│ Review & Save   │
│   Session View  │
│   Add Another   │
└────┬────────────┘
     ↓
   Save
     │ ManualEntry
     ↓
┌─────────────────┐
│ Manual Entry    │
│   Bulk Grid     │
│   Fill Blanks   │
│   Sort          │
└─────────────────┘
```

---

## ViewModel Command Patterns

### Standard Async Command
```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    if (IsBusy) return;
    
    try
    {
        IsBusy = true;
        StatusMessage = "Loading...";
        
        var result = await _service.GetDataAsync();
        
        if (result.Success)
        {
            // Update UI properties
            StatusMessage = "Success";
        }
        else
        {
            await _errorHandler.HandleDaoErrorAsync(result, nameof(LoadDataAsync), true);
        }
    }
    catch (Exception ex)
    {
        await _errorHandler.HandleErrorAsync(
            "Error loading data",
            Enum_ErrorSeverity.Error,
            ex,
            true);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### Navigation Command
```csharp
[RelayCommand]
private void NavigateToNextStep()
{
    _workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
    _logger.LogInfo("Navigated to Quantity Entry", "ModeSelection");
}
```

### Validation Command
```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync()
{
    // Save logic
}

private bool CanSave()
{
    return !IsBusy && !string.IsNullOrWhiteSpace(PartId);
}

partial void OnPartIdChanged(string value)
{
    SaveCommand.NotifyCanExecuteChanged();
}
```

---

## Service Integration Patterns

### Workflow Service
```csharp
// Get current session
var session = _workflowService.CurrentSession;

// Navigate
_workflowService.GoToStep(Enum_DunnageWorkflowStep.Review);

// Update session
session.Quantity = 10;
session.PoNumber = "PO123";
_workflowService.AddLoadToSession();

// Clear
_workflowService.ClearSession();
```

### Database Service
```csharp
// Get all types
var typesResult = await _dunnageService.GetAllTypesAsync();

// Get parts by type
var partsResult = await _dunnageService.GetPartsByTypeAsync(typeId);

// Save loads
var saveResult = await _dunnageService.SaveLoadsAsync(loadsList);

// Get by date range
var loadsResult = await _dunnageService.GetLoadsByDateRangeAsync(fromDate, toDate);
```

### CSV Writer
```csharp
var csvResult = await _csvWriter.WriteToCSVAsync(loads);

if (csvResult.LocalSuccess)
{
    StatusMessage = $"Exported to {csvResult.LocalFilePath}";
}
```

---

## Model Relationships

```
Model_DunnageSession
  └─ List<Model_DunnageLoad> SessionLoads
  └─ Model_DunnageType SelectedType
  └─ Model_DunnagePart SelectedPart
  └─ Dictionary<string, object> SpecValues

Model_DunnageLoad
  ├─ PartId (FK → Model_DunnagePart)
  ├─ TypeName
  ├─ Quantity
  ├─ PoNumber
  ├─ Location
  ├─ InventoryMethod ("Adjust In" / "Receive In")
  └─ Dictionary<string, object> SpecValues

Model_DunnageType
  ├─ Id (PK)
  ├─ TypeName
  └─ SpecsJson (JSON-serialized spec definitions)

Model_DunnagePart
  ├─ Id (PK)
  ├─ PartId
  ├─ PartDescription
  ├─ TypeId (FK → Model_DunnageType)
  └─ InventoryBalance

Model_SpecInput (NEW)
  ├─ Name
  ├─ Label
  ├─ Type (Text, Number, Dropdown, etc.)
  ├─ Required
  └─ DefaultValue
```

---

## XAML Binding Patterns

### Compile-Time Binding (Preferred)
```xml
<!-- One-Way -->
<TextBlock Text="{x:Bind ViewModel.Title, Mode=OneWay}"/>

<!-- Two-Way -->
<TextBox Text="{x:Bind ViewModel.SearchText, Mode=TwoWay}"/>

<!-- Command -->
<Button Content="Save" Command="{x:Bind ViewModel.SaveCommand}"/>

<!-- Visibility with Converter -->
<ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"/>
```

### ItemsRepeater for Dynamic Lists
```xml
<ItemsRepeater ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}">
    <ItemsRepeater.ItemTemplate>
        <DataTemplate>
            <Border>
                <TextBlock Text="{Binding TypeName}"/>
            </Border>
        </DataTemplate>
    </ItemsRepeater.ItemTemplate>
</ItemsRepeater>
```

### Pagination Controls
```xml
<StackPanel Orientation="Horizontal" Spacing="8">
    <Button Content="First" Command="{x:Bind ViewModel.FirstPageCommand}"/>
    <Button Content="Previous" Command="{x:Bind ViewModel.PreviousPageCommand}"/>
    <TextBlock>
        <Run Text="Page"/>
        <Run Text="{x:Bind ViewModel.CurrentPage, Mode=OneWay}"/>
        <Run Text="of"/>
        <Run Text="{x:Bind ViewModel.TotalPages, Mode=OneWay}"/>
    </TextBlock>
    <Button Content="Next" Command="{x:Bind ViewModel.NextPageCommand}"/>
    <Button Content="Last" Command="{x:Bind ViewModel.LastPageCommand}"/>
</StackPanel>
```

---

## Testing Patterns

### ViewModel Test Setup
```csharp
public class MyViewModel_Tests
{
    private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly MyViewModel _viewModel;

    public MyViewModel_Tests()
    {
        _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _viewModel = new MyViewModel(
            _mockDunnageService.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }
}
```

### Command Test
```csharp
[Fact]
public async Task LoadDataCommand_ShouldPopulateItems()
{
    // Arrange
    var testData = new List<Model_DunnageType>
    {
        new Model_DunnageType { Id = 1, TypeName = "Pallet" }
    };

    var result = new Model_Dao_Result<List<Model_DunnageType>>
    {
        Success = true,
        Data = testData
    };

    _mockDunnageService.Setup(s => s.GetAllTypesAsync())
        .ReturnsAsync(result);

    // Act
    await _viewModel.LoadDataCommand.ExecuteAsync(null);

    // Assert
    Assert.Single(_viewModel.Items);
    Assert.Equal("Pallet", _viewModel.Items[0].TypeName);
    _mockDunnageService.Verify(s => s.GetAllTypesAsync(), Times.Once);
}
```

### Error Handling Test
```csharp
[Fact]
public async Task LoadDataCommand_WhenServiceFails_ShouldHandleError()
{
    // Arrange
    var result = new Model_Dao_Result<List<Model_DunnageType>>
    {
        Success = false,
        ErrorMessage = "Database connection failed"
    };

    _mockDunnageService.Setup(s => s.GetAllTypesAsync())
        .ReturnsAsync(result);

    // Act
    await _viewModel.LoadDataCommand.ExecuteAsync(null);

    // Assert
    _mockErrorHandler.Verify(
        e => e.HandleDaoErrorAsync(
            It.IsAny<Model_Dao_Result<List<Model_DunnageType>>>(),
            It.IsAny<string>(),
            true),
        Times.Once);
}
```

---

## Dynamic Spec System (Future)

### JSON Structure
```json
{
  "specs": [
    {
      "name": "length",
      "label": "Length (inches)",
      "type": "number",
      "required": true,
      "defaultValue": 48
    },
    {
      "name": "width",
      "label": "Width (inches)",
      "type": "number",
      "required": true
    },
    {
      "name": "material",
      "label": "Material",
      "type": "dropdown",
      "options": ["Wood", "Plastic", "Metal"],
      "required": false
    }
  ]
}
```

### Parsing Logic
```csharp
private List<Model_SpecInput> DeserializeSpecsJson(string json)
{
    if (string.IsNullOrWhiteSpace(json))
        return new List<Model_SpecInput>();

    try
    {
        var specs = JsonSerializer.Deserialize<List<Model_SpecInput>>(json);
        return specs ?? new List<Model_SpecInput>();
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error deserializing specs: {ex.Message}", ex, "DetailsEntry");
        return new List<Model_SpecInput>();
    }
}
```

### Dynamic XAML Generation
```csharp
// In ViewModel
SpecInputs.Clear();
foreach (var spec in DeserializeSpecsJson(selectedType.SpecsJson))
{
    SpecInputs.Add(spec);
}

// In XAML
<ItemsRepeater ItemsSource="{x:Bind ViewModel.SpecInputs, Mode=OneWay}">
    <ItemsRepeater.ItemTemplate>
        <DataTemplate>
            <StackPanel>
                <TextBlock Text="{Binding Label}"/>
                <TextBox Text="{Binding Value, Mode=TwoWay}" 
                         Visibility="{Binding Type, Converter={StaticResource TypeToVisibilityConverter}}"/>
                <NumberBox Value="{Binding Value, Mode=TwoWay}"
                           Visibility="{Binding Type, Converter={StaticResource TypeToVisibilityConverter}}"/>
            </StackPanel>
        </DataTemplate>
    </ItemsRepeater.ItemTemplate>
</ItemsRepeater>
```

---

## Common Pitfalls & Solutions

### Issue: XAML Compiler Errors
**Symptom**: XamlCompiler.exe exits with code 1, no details  
**Solution**: Simplify bindings incrementally, test with runtime `Binding` first

### Issue: Null Reference Warnings
**Symptom**: CS8602, CS8601 warnings  
**Solution**: Use null-coalescing operators or null-conditional operators
```csharp
var type = AvailableTypes?.FirstOrDefault(t => t.Id == typeId);
var name = type?.TypeName ?? "Unknown";
```

### Issue: ObservableCollection Not Updating
**Symptom**: UI doesn't refresh when collection changes  
**Solution**: Use ObservableCollection, not List
```csharp
[ObservableProperty]
private ObservableCollection<Model_DunnageType> _items = new();
```

### Issue: Command CanExecute Not Updating
**Symptom**: Button stays disabled after property changes  
**Solution**: Call NotifyCanExecuteChanged in property setter
```csharp
partial void OnSearchTextChanged(string value)
{
    SearchCommand.NotifyCanExecuteChanged();
}
```

---

## File Locations

### Production Code
- **ViewModels**: `ViewModels/Dunnage/*ViewModel.cs`
- **Views**: `Views/Dunnage/*View.xaml[.cs]`
- **Models**: `Models/Dunnage/Model_*.cs`
- **Services**: `Services/Database/*Dunnage*.cs`
- **DAOs**: `Data/Dunnage/Dao_*.cs`

### Specifications
- **Tasks**: `specs/008-dunnage-ui/tasks.md`
- **Plan**: `specs/008-dunnage-ui/plan.md`
- **Data Model**: `specs/008-dunnage-ui/data-model.md`
- **Contracts**: `specs/008-dunnage-ui/contracts/viewmodel-contracts.md`

---

## Build & Test Commands

```powershell
# Build
dotnet build /p:Platform=x64 /p:Configuration=Debug

# Run all tests
dotnet test

# Run Dunnage tests only
dotnet test --filter "FullyQualifiedName~Dunnage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~Dunnage_ModeSelectionViewModel_Tests"

# Clean build
dotnet clean /p:Platform=x64
dotnet build /p:Platform=x64 /p:Configuration=Debug
```

---

## Quick Reference: ViewModels

| ViewModel | Lines | Tests | Status | Key Features |
|-----------|-------|-------|--------|--------------|
| Mode Selection | 139 | 6 | ✅ Complete | User preference, 3 modes |
| Type Selection | 195 | 8 | ✅ Complete | Pagination 3x3, filtering |
| Part Selection | 244 | 7 | ✅ ViewModel only | Part loading, inventory |
| Quantity Entry | 134 | 6 | ✅ Complete | Validation, context |
| Details Entry | 270 | 9 | ✅ Complete | Dynamic specs, PO/Location |
| Review | 198 | 7 | ✅ Complete | Session view, CSV export |
| Manual Entry | 321 | 12 | ✅ ViewModel only | Bulk ops, Fill Blanks, Sort |
| Edit Mode | 278 | 9 | ✅ ViewModel only | Pagination, date filter |
| **Total** | **1,579** | **64** | **85%** | |

---

## Contributors

- **Primary Developer**: GitHub Copilot (Claude Sonnet 4.5) with Serena MCP Tools
- **Architecture**: MTM Receiving Application Standards (MVVM, DI, DAO pattern)
- **Reviewer**: JDKoll1982

---

**Last Updated**: December 27, 2024  
**Version**: 1.0  
**Status**: Implementation 85% Complete

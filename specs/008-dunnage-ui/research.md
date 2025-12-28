# Research & Technical Decisions: Dunnage Wizard Workflow UI

**Feature**: `008-dunnage-ui`  
**Date**: December 27, 2025  
**Status**: Research Complete

## Research Areas

### 1. Pagination Implementation for Type Selection

**Question**: How should we implement 3x3 grid pagination for type buttons (9 items per page)?

**Research**:
- Existing pattern: `Service_Pagination` service already exists in the codebase
- Used by Receiving workflow for similar grid pagination scenarios
- Pattern: Service calculates page boundaries, ViewModel exposes `DisplayedItems` collection

**Decision**: Use existing `IService_Pagination` service

**Rationale**:
- Already implemented and tested in Receiving workflow
- Handles edge cases (empty lists, single page, boundary calculations)
- Provides consistent pagination UX across application
- Avoids code duplication

**Implementation Pattern**:
```csharp
// In Dunnage_TypeSelectionViewModel
private readonly IService_Pagination _paginationService;
private ObservableCollection<Model_DunnageType> _allTypes;

[ObservableProperty]
private ObservableCollection<Model_DunnageType> _displayedTypes;

[ObservableProperty]
private int _currentPage = 1;

[ObservableProperty]
private int _totalPages = 1;

private async Task LoadTypesAsync()
{
    var result = await _dunnageService.GetAllTypesAsync();
    if (result.IsSuccess)
    {
        _allTypes = new ObservableCollection<Model_DunnageType>(result.Data);
        _paginationService.SetItems(_allTypes, itemsPerPage: 9);
        UpdatePageDisplay();
    }
}

[RelayCommand]
private void NextPage()
{
    if (_paginationService.CanGoNext())
    {
        _paginationService.NextPage();
        UpdatePageDisplay();
    }
}

private void UpdatePageDisplay()
{
    DisplayedTypes = new ObservableCollection<Model_DunnageType>(_paginationService.GetCurrentPage());
    CurrentPage = _paginationService.CurrentPageNumber;
    TotalPages = _paginationService.TotalPages;
}
```

**Alternatives Considered**:
- Manual pagination logic in ViewModel: Rejected - reinvents existing service
- XAML-only pagination with ItemsControl: Rejected - doesn't support 3x3 grid layout with buttons

---

### 2. Dynamic Spec Input Control Generation

**Question**: How should we dynamically generate TextBox/NumberBox/CheckBox controls based on spec data types in Details Entry view?

**Research**:
- WinUI 3 supports runtime control generation via code-behind
- Existing pattern: Receiving workflow has static forms, NOT dynamic
- Spec schema stored in `Model_DunnageType.SpecsJson` as JSON string
- Data types: "text", "number", "boolean" from spec definitions

**Decision**: Use `ItemsRepeater` with `DataTemplate` selection via `DataTemplateSelector`

**Rationale**:
- Pure XAML solution maintains MVVM separation (no code-behind control generation)
- WinUI 3 `ItemsRepeater` is designed for dynamic item rendering
- `DataTemplateSelector` chooses template based on spec data type
- Bindable to `ObservableCollection<SpecInputViewModel>` from ViewModel

**Implementation Pattern**:

**ViewModel**:
```csharp
// Dunnage_DetailsEntryViewModel.cs
public class SpecInputViewModel : ObservableObject
{
    [ObservableProperty]
    private string _specName;
    
    [ObservableProperty]
    private string _dataType; // "text", "number", "boolean"
    
    [ObservableProperty]
    private object _value;
    
    [ObservableProperty]
    private string _unit; // e.g., "inches"
}

[ObservableProperty]
private ObservableCollection<SpecInputViewModel> _specInputs;

private void LoadSpecsForSelectedPart()
{
    var specs = _selectedPart.Type.ParsedSpecs; // Deserialize SpecsJson
    SpecInputs = new ObservableCollection<SpecInputViewModel>();
    
    foreach (var spec in specs)
    {
        SpecInputs.Add(new SpecInputViewModel
        {
            SpecName = spec.Name,
            DataType = spec.DataType,
            Value = spec.DefaultValue,
            Unit = spec.Unit
        });
    }
}
```

**XAML DataTemplateSelector**:
```xml
<!-- Dunnage_DetailsEntryView.xaml -->
<Page.Resources>
    <local:SpecInputTemplateSelector x:Key="SpecTemplateSelector">
        <local:SpecInputTemplateSelector.TextTemplate>
            <DataTemplate x:DataType="vm:SpecInputViewModel">
                <StackPanel Spacing="4">
                    <TextBlock Text="{x:Bind SpecName, Mode=OneWay}" />
                    <TextBox Text="{x:Bind Value, Mode=TwoWay}" />
                </StackPanel>
            </DataTemplate>
        </local:SpecInputTemplateSelector.TextTemplate>
        
        <local:SpecInputTemplateSelector.NumberTemplate>
            <DataTemplate x:DataType="vm:SpecInputViewModel">
                <StackPanel Spacing="4">
                    <TextBlock>
                        <Run Text="{x:Bind SpecName, Mode=OneWay}" />
                        <Run Text="{x:Bind Unit, Mode=OneWay}" />
                    </TextBlock>
                    <NumberBox Value="{x:Bind Value, Mode=TwoWay}" />
                </StackPanel>
            </DataTemplate>
        </local:SpecInputTemplateSelector.NumberTemplate>
        
        <local:SpecInputTemplateSelector.BooleanTemplate>
            <DataTemplate x:DataType="vm:SpecInputViewModel">
                <CheckBox Content="{x:Bind SpecName, Mode=OneWay}" 
                          IsChecked="{x:Bind Value, Mode=TwoWay}" />
            </DataTemplate>
        </local:SpecInputTemplateSelector.BooleanTemplate>
    </local:SpecInputTemplateSelector>
</Page.Resources>

<ItemsRepeater ItemsSource="{x:Bind ViewModel.SpecInputs, Mode=OneWay}"
               ItemTemplate="{StaticResource SpecTemplateSelector}" />
```

**DataTemplateSelector Code-Behind** (minimal, UI-only logic):
```csharp
// SpecInputTemplateSelector.cs
public class SpecInputTemplateSelector : DataTemplateSelector
{
    public DataTemplate TextTemplate { get; set; }
    public DataTemplate NumberTemplate { get; set; }
    public DataTemplate BooleanTemplate { get; set; }
    
    protected override DataTemplate SelectTemplateCore(object item)
    {
        var spec = item as SpecInputViewModel;
        return spec?.DataType switch
        {
            "text" => TextTemplate,
            "number" => NumberTemplate,
            "boolean" => BooleanTemplate,
            _ => TextTemplate
        };
    }
}
```

**Alternatives Considered**:
- Code-behind dynamic control generation: Rejected - violates MVVM, harder to test
- Separate static grids for each type: Rejected - not scalable, hardcoded
- UserControl per spec type: Rejected - over-engineering for simple inputs

---

### 3. InfoBar Usage Pattern for Inventory Notifications

**Question**: How should InfoBar be used for inventory method notifications (Adjust In vs Receive In)?

**Research**:
- Existing pattern: Receiving workflow uses InfoBar for status messages at top of page
- WinUI 3 InfoBar supports: Informational, Success, Warning, Error severities
- Spec requires: "⚠️ This part requires inventory in Visual. Method: {method}"
- Message must update dynamically when PO number changes

**Decision**: Use `Severity="Informational"` InfoBar with `IsOpen` binding

**Rationale**:
- Informational severity uses blue accent (matches spec requirement for ⚠️ icon)
- Warning severity (yellow) implies user error, not appropriate for informational notice
- Binding to `IsInventoryNotificationVisible` property enables show/hide control
- Message binding to `InventoryNotificationMessage` enables dynamic updates

**Implementation Pattern**:
```csharp
// Dunnage_PartSelectionViewModel.cs
[ObservableProperty]
private bool _isInventoryNotificationVisible;

[ObservableProperty]
private string _inventoryNotificationMessage;

[ObservableProperty]
private string _inventoryMethod = "Adjust In";

private async Task OnPartSelectedAsync()
{
    var isInventoried = await _dunnageService.IsPartInventoriedAsync(_selectedPart.PartID);
    
    if (isInventoried)
    {
        IsInventoryNotificationVisible = true;
        UpdateInventoryMessage();
    }
    else
    {
        IsInventoryNotificationVisible = false;
    }
}

partial void OnPoNumberChanged(string value)
{
    InventoryMethod = string.IsNullOrWhiteSpace(value) ? "Adjust In" : "Receive In";
    UpdateInventoryMessage();
}

private void UpdateInventoryMessage()
{
    InventoryNotificationMessage = $"⚠️ This part requires inventory in Visual. Method: {InventoryMethod}";
}
```

```xml
<!-- Dunnage_DetailsEntryView.xaml -->
<InfoBar IsOpen="{x:Bind ViewModel.IsInventoryNotificationVisible, Mode=OneWay}"
         Severity="Informational"
         Message="{x:Bind ViewModel.InventoryNotificationMessage, Mode=OneWay}" />
```

**Alternatives Considered**:
- Warning severity: Rejected - implies error, not informational notice
- TextBlock with icon: Rejected - InfoBar provides consistent UX
- TeachingTip: Rejected - not persistent, requires user dismissal

---

### 4. Default Mode Preference Storage

**Question**: How should "Set as default mode" preference be persisted?

**Research**:
- Existing service: `IService_UserPreferences` exists for user settings
- Pattern: User preferences stored in MySQL `user_preferences` table
- Key-value pairs per employee number
- Methods: `GetPreferenceAsync(employeeNumber, key)`, `SetPreferenceAsync(employeeNumber, key, value)`

**Decision**: Use `IService_UserPreferences.SetPreferenceAsync("DefaultDunnageMode", modeValue)`

**Rationale**:
- Consistent with existing preference storage pattern
- Per-user settings (not global)
- Persists across sessions
- Already tested and reliable

**Implementation Pattern**:
```csharp
// Dunnage_ModeSelectionViewModel.cs
private readonly IService_UserPreferences _userPreferences;
private readonly IService_SessionManager _sessionManager;

[ObservableProperty]
private bool _isGuidedModeDefault;

[ObservableProperty]
private bool _isManualModeDefault;

[ObservableProperty]
private bool _isEditModeDefault;

public async Task InitializeAsync()
{
    var employeeNumber = _sessionManager.GetCurrentEmployeeNumber();
    var defaultMode = await _userPreferences.GetPreferenceAsync(employeeNumber, "DefaultDunnageMode");
    
    if (!string.IsNullOrEmpty(defaultMode))
    {
        // Auto-navigate to default mode
        await NavigateToMode(defaultMode);
    }
}

[RelayCommand]
private async Task SetGuidedAsDefaultAsync()
{
    var employeeNumber = _sessionManager.GetCurrentEmployeeNumber();
    await _userPreferences.SetPreferenceAsync(employeeNumber, "DefaultDunnageMode", "Guided");
    
    IsGuidedModeDefault = true;
    IsManualModeDefault = false;
    IsEditModeDefault = false;
}
```

**Alternatives Considered**:
- Local app settings: Rejected - not per-user, breaks on shared terminals
- Registry: Rejected - non-portable, Windows-specific
- JSON config file: Rejected - concurrency issues, no per-user support

---

### 5. Workflow Orchestration Pattern

**Question**: How should the main orchestrator manage step visibility and transitions?

**Research**:
- Existing pattern: `Service_ReceivingWorkflow` manages Receiving workflow state
- Similarly, `Service_DunnageWorkflow` already exists for Dunnage
- Pattern: Service raises `StepChanged` event, ViewModel subscribes and updates visibility flags
- Main ViewModel (`Main_DunnageLabelViewModel`) hosts all step views with visibility bindings

**Decision**: Follow existing Receiving workflow pattern with `Service_DunnageWorkflow`

**Rationale**:
- Consistent with established application architecture
- Service encapsulates workflow state, ViewModels handle UI visibility
- Event-driven pattern decouples service from UI
- Already implemented and proven

**Implementation Pattern**:
```csharp
// Main_DunnageLabelViewModel.cs (ALREADY EXISTS, NEEDS MODIFICATION)
public partial class Main_DunnageLabelViewModel : BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    
    [ObservableProperty]
    private bool _isModeSelectionVisible;
    
    [ObservableProperty]
    private bool _isTypeSelectionVisible;
    
    [ObservableProperty]
    private bool _isPartSelectionVisible;
    
    // ... other step visibility flags
    
    public Main_DunnageLabelViewModel(
        IService_DunnageWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _workflowService.StepChanged += OnWorkflowStepChanged;
    }
    
    private void OnWorkflowStepChanged(object sender, Enum_DunnageWorkflowStep newStep)
    {
        // Hide all steps
        IsModeSelectionVisible = false;
        IsTypeSelectionVisible = false;
        IsPartSelectionVisible = false;
        // ... set all to false
        
        // Show current step
        switch (newStep)
        {
            case Enum_DunnageWorkflowStep.ModeSelection:
                IsModeSelectionVisible = true;
                break;
            case Enum_DunnageWorkflowStep.TypeSelection:
                IsTypeSelectionVisible = true;
                break;
            // ... other cases
        }
        
        CurrentStepTitle = GetStepTitle(newStep);
    }
}
```

```xml
<!-- Main_DunnageLabelPage.xaml -->
<Grid>
    <views:Dunnage_ModeSelectionView 
        Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
    
    <views:Dunnage_TypeSelectionView 
        Visibility="{x:Bind ViewModel.IsTypeSelectionVisible, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
    
    <!-- ... other step views -->
</Grid>
```

**Alternatives Considered**:
- NavigationView with pages: Rejected - loses workflow state on navigation
- Frame navigation: Rejected - over-engineered, loses context
- Manual visibility management in ViewModel: Rejected - duplicates service logic

---

### 6. Color and Theme Consistency

**Question**: How should colors be applied to match existing Receiving workflow?

**Research**:
- Spec requirement: Use WinUI 3 theme resources, NOT hardcoded colors
- Existing pattern: Receiving views use `{ThemeResource AccentFillColorDefaultBrush}`, `{ThemeResource CardBackgroundFillColorDefaultBrush}`, etc.
- Reference files: `Views/Receiving/Receiving_WeightQuantityView.xaml`, `Views/Receiving/Receiving_ReviewGridView.xaml`

**Decision**: Use WinUI 3 `ThemeResource` for ALL colors, copy patterns from Receiving views

**Rationale**:
- Ensures system theme compatibility (light/dark mode)
- Matches existing application visual style
- Follows Windows design language
- User-configurable via OS theme settings

**Color Resource Mapping**:
```xml
<!-- From existing Receiving views -->
Headers: {ThemeResource AccentFillColorDefaultBrush} background
         Foreground="White" text
         
Cards: {ThemeResource CardBackgroundFillColorDefaultBrush} background
       {ThemeResource CardStrokeColorDefaultBrush} border
       CornerRadius="8"
       
Icons: {ThemeResource AccentTextFillColorPrimaryBrush} foreground

Text: {StaticResource BodyStrongTextBlockStyle} for emphasized
      
InfoBar: Severity="Informational" (blue accent, NOT Warning yellow)
```

**Example from Receiving_WeightQuantityView.xaml**:
```xml
<Border Background="{ThemeResource AccentFillColorDefaultBrush}"
        Padding="16,12"
        CornerRadius="8,8,0,0">
    <TextBlock Text="Selected Part Information"
               Foreground="White"
               Style="{StaticResource BodyStrongTextBlockStyle}" />
</Border>

<Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="16">
    <!-- Card content -->
</Border>
```

**Alternatives Considered**:
- Hardcoded hex colors: Rejected - violates spec, breaks theme support
- Custom theme resources: Rejected - inconsistent with existing UI
- Inline SolidColorBrush: Rejected - not themeable

---

## Summary of Technical Decisions

| Area | Decision | Service/Pattern |
|------|----------|-----------------|
| Pagination | Use existing `IService_Pagination` | 9 items per page, ViewModel manages display |
| Dynamic Specs | `ItemsRepeater` + `DataTemplateSelector` | Pure XAML, no code-behind generation |
| Inventory Notification | InfoBar with `Severity="Informational"` | Blue accent, bindable visibility and message |
| Default Mode Preference | `IService_UserPreferences` | Per-user, MySQL storage |
| Workflow Orchestration | `Service_DunnageWorkflow` with events | Matches Receiving pattern |
| Colors/Themes | WinUI 3 `ThemeResource` only | Copy from Receiving views |

**No new services or architectural patterns required.** All decisions leverage existing, proven implementations from the Receiving workflow.

---

## Implementation Risks

### Low Risk
- ✅ All services already exist and are tested
- ✅ XAML patterns are standard WinUI 3
- ✅ No new database schema changes
- ✅ No Infor Visual integration changes

### Medium Risk
- ⚠️ Dynamic spec input generation is new to the application (but standard WinUI 3 pattern)
- **Mitigation**: Create prototype UserControl early in implementation, test with various spec schemas

### High Risk
- ❌ None identified

---

## Next Steps

Proceed to **Phase 1: Design**:
1. ✅ Create data-model.md (no new schema, document existing relationships)
2. ✅ Create contracts/ (ViewModel command/property contracts for consistency)
3. ✅ Create quickstart.md (developer setup and build instructions)
4. Update agent context with new ViewModels

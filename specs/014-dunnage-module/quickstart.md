# Quick Start Guide: Dunnage Module

**For**: Developers implementing the Dunnage module  
**Prerequisites**: MVVM infrastructure, DI container, MySQL database access  
**Estimated Setup Time**: 45 minutes

## Prerequisites Checklist

Before starting implementation, ensure:

- [ ] MySQL database `mtm_receiving_application` is accessible
- [ ] MVVM infrastructure is set up (BaseViewModel, BaseWindow)
- [ ] Dependency Injection is configured (App.xaml.cs)
- [ ] Error handling service is registered (IService_ErrorHandler)
- [ ] Logging service is registered (ILoggingService)
- [ ] Authentication context provides employee number
- [ ] LabelView 2022 CSV export folder is configured
- [ ] Material.Icons.WinUI3 NuGet package installed

## Phase 1: Database Setup (15 min)

### Step 1: Verify Database Schema

```sql
-- Check dunnage tables exist
SHOW TABLES LIKE 'dunnage_%';

-- Check stored procedures exist
SHOW PROCEDURE STATUS WHERE Db = 'mtm_receiving_application' AND Name LIKE 'sp_dunnage_%';

-- Check sample data loaded
SELECT COUNT(*) FROM dunnage_types;  -- Should have some types
SELECT COUNT(*) FROM dunnage_parts;   -- Should have some parts
```

## Phase 2: Foundation Models & DAOs (1-2 hours)

### Step 1: Create Models

Create model classes in `Models/Dunnage/`:

```csharp
// Models/Dunnage/Model_DunnageType.cs
public class Model_DunnageType
{
    public int Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; // Material.Icons code
    public bool IsActive { get; set; } = true;
}

// Models/Dunnage/Model_DunnagePart.cs
public class Model_DunnagePart
{
    public string PartId { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsInventoried { get; set; }
}

// Models/Dunnage/Model_DunnageSpec.cs
public class Model_DunnageSpec
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string SpecKey { get; set; } = string.Empty;
    public string SpecValue { get; set; } = string.Empty;
}

// Models/Dunnage/Model_DunnageLoad.cs
public class Model_DunnageLoad
{
    public string LoadUuid { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public string PartId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Dictionary<string, string> SpecValues { get; set; } = new();
    public string EmployeeNumber { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
```

### Step 2: Create DAOs

Create DAO classes in `Data/Dunnage/`:

```csharp
// Data/Dunnage/Dao_DunnageType.cs
public class Dao_DunnageType
{
    private readonly string _connectionString;

    public Dao_DunnageType(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllAsync()
    {
        // Call sp_dunnage_type_get_all
    }

    public async Task<Model_Dao_Result<Model_DunnageType>> GetByIdAsync(int typeId)
    {
        // Call sp_dunnage_type_get_by_id
    }
}
```

### Step 3: Register DAOs in DI

```csharp
// App.xaml.cs ConfigureServices
services.AddSingleton(sp => new Dao_DunnageType(
    Helper_Database_Variables.GetConnectionString()));
services.AddSingleton(sp => new Dao_DunnagePart(
    Helper_Database_Variables.GetConnectionString()));
// ... other DAOs
```

## Phase 3: Service Implementation (2-3 hours)

### Step 1: Implement IService_DunnageWorkflow

```csharp
// Services/Dunnage/Service_DunnageWorkflow.cs
public class Service_DunnageWorkflow : IService_DunnageWorkflow
{
    private Enum_DunnageWorkflowStep _currentStep;
    private Model_DunnageSession _session;

    public Enum_DunnageWorkflowStep CurrentStep => _currentStep;
    public Model_DunnageSession CurrentSession => _session;

    public event EventHandler StepChanged;
    public event EventHandler<string> StatusMessageRaised;

    public async Task<bool> StartWorkflowAsync()
    {
        // Load existing session if available
        // Or create new session
    }

    public async Task<Model_WorkflowStepResult> AdvanceToNextStepAsync()
    {
        // Validate current step
        // Advance to next step
    }
}
```

### Step 2: Implement IService_DunnageAdminWorkflow

```csharp
// Services/Dunnage/Service_DunnageAdminWorkflow.cs
public class Service_DunnageAdminWorkflow : IService_DunnageAdminWorkflow
{
    private Enum_DunnageAdminSection _currentSection;
    private bool _isDirty;

    public Enum_DunnageAdminSection CurrentSection => _currentSection;
    public bool IsDirty => _isDirty;

    public event EventHandler<Enum_DunnageAdminSection>? SectionChanged;
    public event EventHandler<string>? StatusMessageRaised;

    public async Task NavigateToSectionAsync(Enum_DunnageAdminSection section)
    {
        // Check if navigation allowed (unsaved changes)
        // Navigate to section
        // Raise SectionChanged event
    }
}
```

### Step 3: Register Services in DI

```csharp
// App.xaml.cs ConfigureServices
services.AddSingleton<IService_DunnageWorkflow, Service_DunnageWorkflow>();
services.AddSingleton<IService_DunnageAdminWorkflow, Service_DunnageAdminWorkflow>();
services.AddSingleton<IService_MySQL_Dunnage, Service_MySQL_Dunnage>();
services.AddSingleton<IService_DunnageCSVWriter, Service_DunnageCSVWriter>();
```

## Phase 4: ViewModel Implementation (2-3 hours)

### Step 1: Create ViewModels

```csharp
// ViewModels/Dunnage/ViewModel_Dunnage_TypeSelection.cs
public partial class ViewModel_Dunnage_TypeSelection : BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflow;
    private readonly IService_MySQL_Dunnage _dunnageService;

    [ObservableProperty]
    private ObservableCollection<Model_DunnageType> _types = new();

    [RelayCommand]
    private async Task SelectTypeAsync(Model_DunnageType type)
    {
        // Update workflow session
        // Navigate to next step
    }
}
```

**ViewModel Pattern**:
- Inherit from `BaseViewModel`
- Use `[ObservableProperty]` for properties
- Use `[RelayCommand]` for commands
- Use `partial` class (required for source generators)

### Step 2: Register ViewModels in DI

```csharp
// App.xaml.cs ConfigureServices
services.AddTransient<ViewModel_Dunnage_TypeSelection>();
services.AddTransient<ViewModel_Dunnage_PartSelection>();
services.AddTransient<ViewModel_Dunnage_AdminTypes>();
// ... other ViewModels
```

## Phase 5: View Implementation (2-3 hours)

### Step 1: Create XAML Views

```xml
<!-- Views/Dunnage/View_Dunnage_TypeSelection.xaml -->
<Page x:Class="MTM_Receiving_Application.Views.Dunnage.View_Dunnage_TypeSelection"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:viewmodels="using:MTM_Receiving_Application.ViewModels.Dunnage">
    
    <Page.DataContext>
        <viewmodels:ViewModel_Dunnage_TypeSelection />
    </Page.DataContext>
    
    <Grid Padding="20">
        <ItemsControl ItemsSource="{x:Bind ViewModel.Types, Mode=OneWay}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Button Command="{x:Bind ViewModel.SelectTypeCommand}">
                        <StackPanel>
                            <material:MaterialIcon Icon="{x:Bind Icon}" />
                            <TextBlock Text="{x:Bind TypeName}" />
                        </StackPanel>
                    </Button>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</Page>
```

**View Pattern**:
- Use `x:Bind` (not `Binding`)
- Set DataContext in XAML
- Material.Icons for type display
- No business logic in code-behind

## Testing Checklist

### Database Tests
- [ ] Dunnage tables exist and are accessible
- [ ] Stored procedures execute without errors
- [ ] Sample data loaded (types, parts, specs)

### Service Tests
- [ ] Workflow advances through all steps correctly
- [ ] Admin navigation works (4 sections)
- [ ] Type selection loads Material.Icons correctly
- [ ] Part filtering by inventoried status works
- [ ] Dynamic form generation from specs works

### UI Tests
- [ ] Type Selection screen loads with icons
- [ ] Part Selection filters by inventoried status
- [ ] Details Entry form generates dynamically
- [ ] Admin sections navigate correctly
- [ ] Save operation exports to database and CSV

## Common Issues & Solutions

### Issue: Material.Icons not displaying
**Solution**: Verify Material.Icons.WinUI3 NuGet package installed and XAML namespace declared

### Issue: Dynamic form not generating
**Solution**: Verify specs loaded for selected type and form generation logic correct

### Issue: Parts not filtering by inventoried status
**Solution**: Verify `IsInventoried` flag set correctly and filtering logic in service

### Issue: Admin navigation blocked
**Solution**: Check `CanNavigateAwayAsync` logic and unsaved changes handling

## Next Steps

1. **Complete Phase 1** (Database setup) - 15 min
2. **Complete Phase 2** (Models & DAOs) - 1-2 hours
3. **Complete Phase 3** (Services) - 2-3 hours
4. **Complete Phase 4** (ViewModels) - 2-3 hours
5. **Complete Phase 5** (Views) - 2-3 hours

**Total Estimated Time**: 8-12 hours for MVP implementation

## Resources

- **[spec.md](../011-module-reimplementation/spec.md)** - Complete feature specification (User Story 3)
- **[data-model.md](../011-module-reimplementation/data-model.md)** - Database schema reference
- **[tasks.md](../011-module-reimplementation/tasks.md)** - Detailed task breakdown
- **[contracts/](contracts/)** - Service interface definitions
- **[mockups/](../011-module-reimplementation/mockups/Dunnage/)** - UI mockups

---

**Last Updated**: 2026-01-03  
**For Questions**: See [spec.md](../011-module-reimplementation/spec.md) or contact development team


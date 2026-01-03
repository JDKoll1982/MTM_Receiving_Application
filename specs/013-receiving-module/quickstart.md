# Quick Start Guide: Receiving Module

**For**: Developers implementing the Receiving module  
**Prerequisites**: MVVM infrastructure, DI container, MySQL database access, Infor Visual read-only access  
**Estimated Setup Time**: 45 minutes

## Prerequisites Checklist

Before starting implementation, ensure:

- [ ] MySQL database `mtm_receiving_application` is accessible
- [ ] Infor Visual SQL Server (VISUAL/MTMFG) is accessible (READ ONLY)
- [ ] MVVM infrastructure is set up (BaseViewModel, BaseWindow)
- [ ] Dependency Injection is configured (App.xaml.cs)
- [ ] Error handling service is registered (IService_ErrorHandler)
- [ ] Logging service is registered (ILoggingService)
- [ ] Authentication context provides employee number
- [ ] LabelView 2022 CSV export folder is configured

## Phase 1: Database Setup (15 min)

### Step 1: Verify Database Schema

```sql
-- Check receiving tables exist
SHOW TABLES LIKE 'receiving_%';

-- Check stored procedures exist
SHOW PROCEDURE STATUS WHERE Db = 'mtm_receiving_application' AND Name LIKE 'sp_receiving_%';

-- Check Infor Visual connection (READ ONLY)
-- Connection string should include: ApplicationIntent=ReadOnly
```

### Step 2: Verify Infor Visual Access

```csharp
// Test Infor Visual connection (READ ONLY)
var connectionString = "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly;...";
// Query po table to verify access
```

## Phase 2: Foundation Models & DAOs (1-2 hours)

### Step 1: Create Models

Create model classes in `Models/Receiving/`:

```csharp
// Models/Receiving/Model_ReceivingLoad.cs
public class Model_ReceivingLoad
{
    public int Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string PartID { get; set; } = string.Empty;
    public int LoadNumber { get; set; }
    public string PackageType { get; set; } = string.Empty; // "Box", "Pallet", "Loose Parts"
    public decimal Weight { get; set; }
    public decimal Quantity { get; set; }
    public string HeatLot { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
}

// Models/Receiving/Model_ReceivingSession.cs
public class Model_ReceivingSession
{
    public Enum_ReceivingWorkflowStep CurrentStep { get; set; }
    public string? CurrentPONumber { get; set; }
    public Model_InforVisualPart? CurrentPart { get; set; }
    public bool IsNonPOItem { get; set; }
    public int NumberOfLoads { get; set; }
    public List<Model_ReceivingLoad> ReviewedLoads { get; set; } = new();
}
```

### Step 2: Create DAOs

Create DAO classes in `Data/Receiving/`:

```csharp
// Data/Receiving/Dao_ReceivingLoad.cs
public class Dao_ReceivingLoad
{
    private readonly string _connectionString;

    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result<int>> InsertAsync(Model_ReceivingLoad load)
    {
        // Call sp_receiving_load_insert
        // Return Model_Dao_Result<int> with new ID
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetByDateRangeAsync(
        DateTime startDate, DateTime endDate)
    {
        // Call sp_receiving_load_get_by_date_range
    }
}
```

### Step 3: Register DAOs in DI

```csharp
// App.xaml.cs ConfigureServices
services.AddSingleton(sp => new Dao_ReceivingLoad(
    Helper_Database_Variables.GetConnectionString()));
```

## Phase 3: Service Implementation (2-3 hours)

### Step 1: Implement IService_ReceivingWorkflow

```csharp
// Services/Receiving/Service_ReceivingWorkflow.cs
public class Service_ReceivingWorkflow : IService_ReceivingWorkflow
{
    private Enum_ReceivingWorkflowStep _currentStep;
    private Model_ReceivingSession _session;

    public Enum_ReceivingWorkflowStep CurrentStep => _currentStep;
    public Model_ReceivingSession CurrentSession => _session;

    public event EventHandler StepChanged;
    public event EventHandler<string> StatusMessageRaised;

    public async Task<bool> StartWorkflowAsync()
    {
        // Load existing session if available
        // Or create new session
        // Return true if restored, false if new
    }

    public async Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync()
    {
        // Validate current step
        // Advance to next step
        // Raise StepChanged event
    }

    public void GoToStep(Enum_ReceivingWorkflowStep step)
    {
        // Navigate to specific step
        // Clear form data BEFORE navigation (fixes bug)
        // Preserve session data
    }
}
```

### Step 2: Implement IService_InforVisual

```csharp
// Services/Receiving/Service_InforVisual.cs
public class Service_InforVisual : IService_InforVisual
{
    private readonly string _connectionString;

    public async Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber)
    {
        // Query Infor Visual (READ ONLY)
        // Return PO with parts collection
    }

    public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID)
    {
        // Query Infor Visual (READ ONLY)
        // Return part details
    }
}
```

### Step 3: Register Services in DI

```csharp
// App.xaml.cs ConfigureServices
services.AddSingleton<IService_ReceivingWorkflow, Service_ReceivingWorkflow>();
services.AddSingleton<IService_InforVisual, Service_InforVisual>();
services.AddSingleton<IService_MySQL_Receiving, Service_MySQL_Receiving>();
services.AddSingleton<IService_ReceivingValidation, Service_ReceivingValidation>();
```

## Phase 4: ViewModel Implementation (2-3 hours)

### Step 1: Create ViewModels

```csharp
// ViewModels/Receiving/ViewModel_Receiving_POEntry.cs
public partial class ViewModel_Receiving_POEntry : BaseViewModel
{
    private readonly IService_ReceivingWorkflow _workflow;
    private readonly IService_InforVisual _inforVisual;

    [ObservableProperty]
    private string _poNumber = string.Empty;

    [RelayCommand]
    private async Task ValidatePOAsync()
    {
        // Auto-format PO number ("63150" → "PO-063150")
        // Query Infor Visual
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
services.AddTransient<ViewModel_Receiving_ModeSelection>();
services.AddTransient<ViewModel_Receiving_POEntry>();
services.AddTransient<ViewModel_Receiving_Review>();
// ... other ViewModels
```

## Phase 5: View Implementation (2-3 hours)

### Step 1: Create XAML Views

```xml
<!-- Views/Receiving/View_Receiving_POEntry.xaml -->
<Page x:Class="MTM_Receiving_Application.Views.Receiving.View_Receiving_POEntry"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:viewmodels="using:MTM_Receiving_Application.ViewModels.Receiving">
    
    <Page.DataContext>
        <viewmodels:ViewModel_Receiving_POEntry />
    </Page.DataContext>
    
    <Grid Padding="20">
        <StackPanel Spacing="10">
            <TextBlock Text="Enter PO Number" Style="{StaticResource TitleTextBlockStyle}" />
            <TextBox Text="{x:Bind ViewModel.PONumber, Mode=TwoWay}" />
            <Button Content="Validate" Command="{x:Bind ViewModel.ValidatePOCommand}" />
        </StackPanel>
    </Grid>
</Page>
```

**View Pattern**:
- Use `x:Bind` (not `Binding`)
- Set DataContext in XAML
- No business logic in code-behind

## Testing Checklist

### Database Tests
- [ ] Receiving tables exist and are accessible
- [ ] Stored procedures execute without errors
- [ ] Infor Visual connection works (READ ONLY)

### Service Tests
- [ ] Workflow advances through all steps correctly
- [ ] PO validation queries Infor Visual successfully
- [ ] Session state persists correctly
- [ ] "Add Another Part" clears form BEFORE navigation

### UI Tests
- [ ] Mode Selection screen loads
- [ ] PO Entry validates and formats correctly
- [ ] All workflow steps navigate correctly
- [ ] Review screen displays data correctly
- [ ] Save operation exports to database and CSV

## Common Issues & Solutions

### Issue: Infor Visual connection fails
**Solution**: Verify connection string includes `ApplicationIntent=ReadOnly` and SQL Server is accessible

### Issue: Session state not persisting
**Solution**: Ensure session JSON file is written to correct location and permissions are set

### Issue: "Add Another Part" bug still occurs
**Solution**: Verify form clearing happens BEFORE navigation, not after (check GoToStep implementation)

### Issue: PO number not auto-formatting
**Solution**: Verify validation service formats PO number before querying Infor Visual

## Next Steps

1. **Complete Phase 1** (Database setup) - 15 min
2. **Complete Phase 2** (Models & DAOs) - 1-2 hours
3. **Complete Phase 3** (Services) - 2-3 hours
4. **Complete Phase 4** (ViewModels) - 2-3 hours
5. **Complete Phase 5** (Views) - 2-3 hours

**Total Estimated Time**: 8-12 hours for MVP implementation

## Resources

- **[spec.md](../011-module-reimplementation/spec.md)** - Complete feature specification (User Story 2)
- **[data-model.md](../011-module-reimplementation/data-model.md)** - Database schema reference
- **[tasks.md](../011-module-reimplementation/tasks.md)** - Detailed task breakdown
- **[contracts/](contracts/)** - Service interface definitions
- **[mockups/](../011-module-reimplementation/mockups/Receiving/)** - UI mockups

---

**Last Updated**: 2026-01-03  
**For Questions**: See [spec.md](../011-module-reimplementation/spec.md) or contact development team


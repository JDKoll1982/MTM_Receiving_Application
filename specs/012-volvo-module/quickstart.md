# Quick Start Guide: Volvo Dunnage Requisition Module

**For**: Developers implementing the Volvo module  
**Prerequisites**: MVVM infrastructure, DI container, MySQL database access  
**Estimated Setup Time**: 30 minutes

## Prerequisites Checklist

Before starting implementation, ensure:

- [ ] MySQL database `mtm_receiving_application` is accessible
- [ ] MVVM infrastructure is set up (BaseViewModel, BaseWindow)
- [ ] Dependency Injection is configured (App.xaml.cs)
- [ ] Error handling service is registered (IService_ErrorHandler)
- [ ] Logging service is registered (ILoggingService)
- [ ] Authentication context provides employee number
- [ ] LabelView 2022 CSV export folder is configured

## Phase 1: Database Setup (30 min)

### Step 1: Create Database Schema

```bash
# Deploy schema
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < Database/Schemas/schema_volvo.sql
```

**Tables Created**:
- `volvo_shipments` (shipment headers)
- `volvo_shipment_lines` (part line items)
- `volvo_parts_master` (parts catalog)
- `volvo_part_components` (component relationships)

### Step 2: Deploy Stored Procedures

```bash
# Deploy all stored procedures
for file in Database/StoredProcedures/Volvo/sp_volvo_*.sql; do
  mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < "$file"
done
```

**Stored Procedures**:
- `sp_volvo_shipment_insert` - Insert shipment with auto-generated shipment number
- `sp_volvo_shipment_complete` - Complete shipment with PO/Receiver
- `sp_volvo_part_master_get_all` - Get all parts for dropdown
- `sp_volvo_shipment_history_get` - Get history with filtering
- (See [data-model.md](data-model.md) for complete list)

### Step 3: Load Initial Master Data

```bash
# Import DataSheet.csv into volvo_parts_master
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < Database/TestData/volvo_sample_data.sql
```

**Data Loaded**:
- Parts from DataSheet.csv (V-EMB-1, V-EMB-2, V-EMB-500, etc.)
- Component relationships (V-EMB-500 → V-EMB-2, V-EMB-92)

### Step 4: Verify Database Setup

```sql
-- Check tables exist
SHOW TABLES LIKE 'volvo_%';

-- Check stored procedures exist
SHOW PROCEDURE STATUS WHERE Db = 'mtm_receiving_application' AND Name LIKE 'sp_volvo_%';

-- Check master data loaded
SELECT COUNT(*) FROM volvo_parts_master;  -- Should be ~30+ parts
SELECT COUNT(*) FROM volvo_part_components;  -- Should be ~15+ relationships
```

## Phase 2: Foundation Models & DAOs (1-2 hours)

### Step 1: Create Models

Create model classes in `Models/Volvo/`:

```csharp
// Models/Volvo/Model_VolvoShipment.cs
public class Model_VolvoShipment
{
    public int Id { get; set; }
    public DateTime ShipmentDate { get; set; }
    public int ShipmentNumber { get; set; }
    public string? PONumber { get; set; }
    public string? ReceiverNumber { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = "pending_po"; // "pending_po" or "completed"
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsArchived { get; set; }
}

// Models/Volvo/Model_VolvoShipmentLine.cs
public class Model_VolvoShipmentLine
{
    public int Id { get; set; }
    public int ShipmentId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public int ReceivedSkidCount { get; set; }
    public int CalculatedPieceCount { get; set; }
    public bool HasDiscrepancy { get; set; }
    public int? ExpectedSkidCount { get; set; }
    public string? DiscrepancyNote { get; set; }
}

// Models/Volvo/Model_VolvoPart.cs
public class Model_VolvoPart
{
    public string PartNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int QuantityPerSkid { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

// Models/Volvo/Model_VolvoPartComponent.cs
public class Model_VolvoPartComponent
{
    public int Id { get; set; }
    public string ParentPartNumber { get; set; } = string.Empty;
    public string ComponentPartNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
```

### Step 2: Create DAOs

Create DAO classes in `Data/Volvo/`:

```csharp
// Data/Volvo/Dao_VolvoShipment.cs
public class Dao_VolvoShipment
{
    private readonly string _connectionString;

    public Dao_VolvoShipment(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result<Model_VolvoShipment>> InsertAsync(
        Model_VolvoShipment shipment)
    {
        // Call sp_volvo_shipment_insert
        // Return Model_Dao_Result<Model_VolvoShipment>
    }

    public async Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingAsync()
    {
        // Call sp_volvo_shipment_get_pending
    }

    // ... other methods
}
```

**DAO Pattern**:
- Instance-based (not static)
- Constructor takes connection string
- All methods return `Model_Dao_Result<T>`
- Use `Helper_Database_StoredProcedure` for execution

### Step 3: Register DAOs in DI

```csharp
// App.xaml.cs ConfigureServices
services.AddSingleton(sp => new Dao_VolvoShipment(
    Helper_Database_Variables.GetConnectionString()));
services.AddSingleton(sp => new Dao_VolvoShipmentLine(
    Helper_Database_Variables.GetConnectionString()));
services.AddSingleton(sp => new Dao_VolvoPart(
    Helper_Database_Variables.GetConnectionString()));
services.AddSingleton(sp => new Dao_VolvoPartComponent(
    Helper_Database_Variables.GetConnectionString()));
```

## Phase 3: Service Implementation (2-3 hours)

### Step 1: Implement IService_Volvo

```csharp
// Services/Volvo/VolvoService.cs
public class VolvoService : IService_Volvo
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoPart _partDao;
    private readonly Dao_VolvoPartComponent _componentDao;

    public async Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
        List<Model_VolvoShipmentLine> shipmentLines)
    {
        // 1. Get master data for each part
        // 2. Calculate main part pieces (skid_count × quantity_per_skid)
        // 3. Get components for each part
        // 4. Aggregate components across all parts
        // 5. Return dictionary: part_number → total_pieces
    }

    public async Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(
        Model_VolvoShipment shipment,
        Dictionary<string, int> requestedLines)
    {
        // 1. Create CSV file in export folder
        // 2. Format: Material ID, Quantity, Employee, Date, Time, Receiver, Notes
        // 3. One row per part in requestedLines
        // 4. Return file path
    }

    // ... other methods
}
```

### Step 2: Register Services in DI

```csharp
// App.xaml.cs ConfigureServices
services.AddSingleton<IService_Volvo, VolvoService>();
services.AddSingleton<IService_VolvoMasterData, VolvoMasterDataService>();
services.AddSingleton<IService_VolvoReporting, VolvoReportingService>();
```

## Phase 4: ViewModel Implementation (2-3 hours)

### Step 1: Create ViewModels

```csharp
// ViewModels/Volvo/VolvoShipmentEntryViewModel.cs
public partial class VolvoShipmentEntryViewModel : BaseViewModel
{
    private readonly IService_Volvo _volvoService;

    [ObservableProperty]
    private DateTime _shipmentDate = DateTime.Today;

    [ObservableProperty]
    private int _shipmentNumber = 1;

    [ObservableProperty]
    private ObservableCollection<Model_VolvoShipmentLine> _parts = new();

    [RelayCommand]
    private async Task GenerateLabelsAsync()
    {
        // Calculate component explosion
        // Generate CSV file
        // Show success InfoBar
    }

    [RelayCommand]
    private async Task SaveAsPendingAsync()
    {
        // Validate input
        // Call _volvoService.SaveShipmentAsync
        // Show success InfoBar
    }

    // ... other commands
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
services.AddTransient<VolvoShipmentEntryViewModel>();
services.AddTransient<VolvoHistoryViewModel>();
services.AddTransient<VolvoSettingsViewModel>();
```

## Phase 5: View Implementation (2-3 hours)

### Step 1: Create XAML Views

```xml
<!-- Views/Volvo/VolvoShipmentEntryView.xaml -->
<Page x:Class="MTM_Receiving_Application.Views.Volvo.VolvoShipmentEntryView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:viewmodels="using:MTM_Receiving_Application.ViewModels.Volvo">
    
    <Page.DataContext>
        <viewmodels:VolvoShipmentEntryViewModel />
    </Page.DataContext>
    
    <Grid Padding="20">
        <StackPanel Spacing="10">
            <TextBlock Text="{x:Bind ViewModel.ShipmentDate, Mode=OneWay}" />
            <DataGrid ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}" />
            <Button Content="Generate Labels" Command="{x:Bind ViewModel.GenerateLabelsCommand}" />
            <Button Content="Save as Pending" Command="{x:Bind ViewModel.SaveAsPendingCommand}" />
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
- [ ] Tables created successfully
- [ ] Stored procedures execute without errors
- [ ] Master data loaded (30+ parts)
- [ ] Component relationships loaded (15+ relationships)

### Service Tests
- [ ] Component explosion calculates correctly (V-EMB-500 with 3 skids = 264 pieces + components)
- [ ] Label CSV file generated in correct format
- [ ] Email formatting matches Google Sheets output
- [ ] Only one pending PO allowed at a time

### UI Tests
- [ ] Shipment entry form loads
- [ ] Part dropdown populated from master data
- [ ] Discrepancy tracking works (4 columns)
- [ ] Label generation creates CSV file
- [ ] Email preview modal opens
- [ ] Save as Pending saves to database

## Common Issues & Solutions

### Issue: Component explosion not aggregating correctly
**Solution**: Ensure components are aggregated in dictionary before returning (sum quantities for duplicate part numbers)

### Issue: CSV file not compatible with LabelView
**Solution**: Verify CSV format matches LabelView requirements (exact column order, data types)

### Issue: Multiple pending shipments allowed
**Solution**: Check `GetPendingShipmentAsync` before allowing new shipment creation

### Issue: Master data changes affect old shipments
**Solution**: Ensure `calculated_piece_count` is stored at creation time, not recalculated

## Next Steps

1. **Complete Phase 1** (Database setup) - 30 min
2. **Complete Phase 2** (Models & DAOs) - 1-2 hours
3. **Complete Phase 3** (Services) - 2-3 hours
4. **Complete Phase 4** (ViewModels) - 2-3 hours
5. **Complete Phase 5** (Views) - 2-3 hours

**Total Estimated Time**: 8-12 hours for MVP implementation

## Resources

- **[spec.md](spec.md)** - Complete feature specification
- **[data-model.md](data-model.md)** - Database schema reference
- **[tasks.md](tasks.md)** - Detailed task breakdown
- **[workflows/](workflows/)** - PlantUML workflow diagrams
- **[contracts/](contracts/)** - Service interface definitions

---

**Last Updated**: 2026-01-03  
**For Questions**: See [spec.md](spec.md) or contact development team


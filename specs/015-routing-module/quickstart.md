# Quick Start Guide: Routing Module

**For**: Developers implementing the Routing module  
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

## Phase 1: Database Setup (15 min)

### Step 1: Create Database Schema

```sql
-- Create routing_labels table
CREATE TABLE routing_labels (
  id INT PRIMARY KEY AUTO_INCREMENT,
  deliver_to VARCHAR(100) NOT NULL,
  department VARCHAR(50),
  package_description VARCHAR(200),
  po_number VARCHAR(20),
  work_order VARCHAR(20),
  employee_number VARCHAR(20) NOT NULL,
  label_number INT NOT NULL,
  created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  is_archived TINYINT(1) NOT NULL DEFAULT 0,
  INDEX idx_is_archived (is_archived),
  INDEX idx_created_date (created_date)
);

-- Create routing_recipients table
CREATE TABLE routing_recipients (
  id INT PRIMARY KEY AUTO_INCREMENT,
  name VARCHAR(100) NOT NULL UNIQUE,
  default_department VARCHAR(50),
  is_active TINYINT(1) NOT NULL DEFAULT 1
);
```

### Step 2: Deploy Stored Procedures

```bash
# Deploy routing stored procedures
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < Database/StoredProcedures/Routing/sp_routing_label_insert.sql
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < Database/StoredProcedures/Routing/sp_routing_archive_today_to_history.sql
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < Database/StoredProcedures/Routing/sp_routing_get_next_label_number.sql
```

## Phase 2: Foundation Models & DAOs (1-2 hours)

### Step 1: Create Models

```csharp
// Models/Routing/Model_RoutingLabel.cs
public class Model_RoutingLabel
{
    public int Id { get; set; }
    public string DeliverTo { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string PackageDescription { get; set; } = string.Empty;
    public string? PONumber { get; set; }
    public string? WorkOrder { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public int LabelNumber { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsArchived { get; set; }
}

// Models/Routing/Model_RoutingRecipient.cs
public class Model_RoutingRecipient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DefaultDepartment { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Step 2: Create DAOs

```csharp
// Data/Routing/Dao_RoutingLabel.cs
public class Dao_RoutingLabel
{
    private readonly string _connectionString;

    public Dao_RoutingLabel(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result<int>> InsertAsync(Model_RoutingLabel label)
    {
        // Call sp_routing_label_insert
    }

    public async Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetTodayLabelsAsync()
    {
        // Query where is_archived = 0
    }

    public async Task<Model_Dao_Result> ArchiveToHistoryAsync()
    {
        // Call sp_routing_archive_today_to_history
    }
}
```

## Phase 3: Service Implementation (2-3 hours)

### Step 1: Implement IService_Routing

```csharp
// Services/Routing/Service_Routing.cs
public class Service_Routing : IService_Routing
{
    private readonly Dao_RoutingLabel _labelDao;
    private readonly IService_Routing_RecipientLookup _recipientLookup;

    public async Task<Model_Dao_Result<int>> AddLabelAsync(Model_RoutingLabel label)
    {
        // Get next label number
        // Insert label
        // Return success
    }

    public async Task<Model_Dao_Result<string>> ExportToCSVAsync(List<Model_RoutingLabel> labels)
    {
        // Generate CSV in LabelView format
        // Return file path
    }
}
```

### Step 2: Implement IService_Routing_RecipientLookup

```csharp
// Services/Routing/Service_Routing_RecipientLookup.cs
public class Service_Routing_RecipientLookup : IService_Routing_RecipientLookup
{
    public async Task<Model_Dao_Result<string?>> GetDefaultDepartmentAsync(string recipientName)
    {
        // Query routing_recipients table
        // Return default department if exists
    }
}
```

## Phase 4: ViewModel Implementation (2-3 hours)

### Step 1: Create ViewModels

```csharp
// ViewModels/Routing/ViewModel_Routing_LabelEntry.cs
public partial class ViewModel_Routing_LabelEntry : BaseViewModel
{
    private readonly IService_Routing _routingService;
    private readonly IService_Routing_RecipientLookup _recipientLookup;

    [ObservableProperty]
    private string _deliverTo = string.Empty;

    [ObservableProperty]
    private string? _department;

    [RelayCommand]
    private async Task OnRecipientChangedAsync()
    {
        // Auto-fill department from recipient lookup
        var dept = await _recipientLookup.GetDefaultDepartmentAsync(DeliverTo);
        if (dept.IsSuccess && dept.Data != null)
        {
            Department = dept.Data;
        }
    }

    [RelayCommand]
    private async Task AddToQueueAsync()
    {
        // Create label
        // Add to queue
        // Increment label number
    }
}
```

## Phase 5: View Implementation (2-3 hours)

### Step 1: Create XAML Views

```xml
<!-- Views/Routing/View_Routing_LabelEntry.xaml -->
<Page x:Class="MTM_Receiving_Application.Views.Routing.View_Routing_LabelEntry"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid Padding="20">
        <StackPanel Spacing="10">
            <ComboBox ItemsSource="{x:Bind ViewModel.Recipients}" 
                      SelectedItem="{x:Bind ViewModel.DeliverTo, Mode=TwoWay}"
                      SelectionChanged="{x:Bind ViewModel.OnRecipientChangedCommand}" />
            <TextBox Text="{x:Bind ViewModel.Department, Mode=TwoWay}" />
            <Button Content="Add to Queue" Command="{x:Bind ViewModel.AddToQueueCommand}" />
        </StackPanel>
    </Grid>
</Page>
```

## Testing Checklist

### Database Tests
- [ ] Routing tables created successfully
- [ ] Stored procedures execute without errors
- [ ] Sample recipients loaded

### Service Tests
- [ ] Label creation works correctly
- [ ] Department auto-fills from recipient lookup
- [ ] Label numbering increments correctly
- [ ] Duplicate row copies fields correctly
- [ ] History archival moves labels correctly

### UI Tests
- [ ] Label entry form loads
- [ ] Recipient dropdown populated
- [ ] Department auto-fills when selecting recipient
- [ ] Label queue displays correctly
- [ ] CSV export generates correctly
- [ ] History view displays with date grouping

## Common Issues & Solutions

### Issue: Department not auto-filling
**Solution**: Verify recipient lookup service queries routing_recipients table correctly

### Issue: Label numbers not incrementing
**Solution**: Check stored procedure sp_routing_get_next_label_number logic

### Issue: History archival not working
**Solution**: Verify stored procedure sp_routing_archive_today_to_history updates is_archived flag

## Next Steps

1. **Complete Phase 1** (Database setup) - 15 min
2. **Complete Phase 2** (Models & DAOs) - 1-2 hours
3. **Complete Phase 3** (Services) - 2-3 hours
4. **Complete Phase 4** (ViewModels) - 2-3 hours
5. **Complete Phase 5** (Views) - 2-3 hours

**Total Estimated Time**: 8-12 hours for MVP implementation

## Resources

- **[spec.md](../011-module-reimplementation/spec.md)** - Complete feature specification (User Story 4)
- **[data-model.md](../011-module-reimplementation/data-model.md)** - Database schema reference
- **[tasks.md](../011-module-reimplementation/tasks.md)** - Detailed task breakdown
- **[contracts/](contracts/)** - Service interface definitions
- **[Business Logic](../Documentation/FuturePlans/RoutingLabels/RoutingLabels-BusinessLogic.md)** - Complete business requirements

---

**Last Updated**: 2026-01-03  
**For Questions**: See [spec.md](../011-module-reimplementation/spec.md) or contact development team


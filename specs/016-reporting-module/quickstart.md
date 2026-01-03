# Quick Start Guide: Reporting Module

**For**: Developers implementing the Reporting module  
**Prerequisites**: MVVM infrastructure, DI container, MySQL database access, all other modules complete  
**Estimated Setup Time**: 30 minutes

## Prerequisites Checklist

Before starting implementation, ensure:

- [ ] MySQL database `mtm_receiving_application` is accessible
- [ ] All module views exist (vw_receiving_history, vw_dunnage_history, vw_routing_history, vw_volvo_history)
- [ ] MVVM infrastructure is set up (BaseViewModel, BaseWindow)
- [ ] Dependency Injection is configured (App.xaml.cs)
- [ ] Error handling service is registered (IService_ErrorHandler)
- [ ] Logging service is registered (ILoggingService)
- [ ] Authentication context provides employee number
- [ ] Helper_PONormalizer class exists (shared helper)

## Phase 1: Database Setup (10 min)

### Step 1: Create Reporting Views

```sql
-- Create receiving history view
CREATE OR REPLACE VIEW vw_receiving_history AS
SELECT 
  id,
  po_number,
  part_id,
  description,
  quantity,
  weight,
  heat_lot,
  received_date,
  employee_number
FROM receiving_loads
ORDER BY received_date DESC;

-- Create dunnage history view
CREATE OR REPLACE VIEW vw_dunnage_history AS
SELECT 
  load_uuid as id,
  type_name,
  part_id,
  CONCAT_WS(', ', spec_values) as specs,
  quantity,
  created_date,
  employee_number
FROM dunnage_loads dl
INNER JOIN dunnage_types dt ON dl.type_id = dt.id
ORDER BY created_date DESC;

-- Create routing history view
CREATE OR REPLACE VIEW vw_routing_history AS
SELECT 
  id,
  deliver_to,
  department,
  package_description,
  po_number,
  work_order,
  employee_number,
  created_date
FROM routing_labels
WHERE is_archived = 1
ORDER BY created_date DESC;

-- Create Volvo history view
CREATE OR REPLACE VIEW vw_volvo_history AS
SELECT 
  id,
  shipment_date,
  shipment_number,
  po_number,
  receiver_number,
  status,
  employee_number,
  created_date
FROM volvo_shipments
WHERE is_archived = 1
ORDER BY shipment_date DESC, shipment_number DESC;
```

## Phase 2: Foundation Models & Services (1-2 hours)

### Step 1: Create Models

```csharp
// Models/Reporting/Model_ReportRow.cs
public class Model_ReportRow
{
    public DateTime Date { get; set; }
    public Dictionary<string, object> Fields { get; set; } = new();
    public string ModuleName { get; set; } = string.Empty;
    public string GroupKey { get; set; } = string.Empty; // For date grouping
}

// Models/Reporting/Model_ReportData.cs
public class Model_ReportData
{
    public string ModuleName { get; set; } = string.Empty;
    public List<Model_ReportRow> Rows { get; set; } = new();
    public int RecordCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
```

### Step 2: Create Helper_PONormalizer

```csharp
// Helpers/Business/Helper_PONormalizer.cs
public static class Helper_PONormalizer
{
    public static string Normalize(string? poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber)) return "No PO";
        if (poNumber == "Customer Supplied") return "Customer Supplied";
        if (poNumber.Length < 5) return "Validate PO";
        
        // Remove existing PO- prefix
        string cleaned = poNumber.Replace("PO-", "");
        
        // Pad to 6 digits if numeric
        if (System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^\d+$"))
        {
            cleaned = cleaned.PadLeft(6, '0');
        }
        
        return "PO-" + cleaned;
    }
}
```

### Step 3: Implement IService_Reporting

```csharp
// Services/Reporting/Service_Reporting.cs
public class Service_Reporting : IService_Reporting
{
    private readonly Dao_Reporting _dao;

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate, DateTime endDate)
    {
        // Query vw_receiving_history
        // Normalize PO numbers
        // Return Model_ReportRow list
    }

    public string NormalizePONumber(string? poNumber)
    {
        return Helper_PONormalizer.Normalize(poNumber);
    }

    public async Task<Model_Dao_Result<string>> ExportToCSVAsync(
        List<Model_ReportRow> data, string moduleName)
    {
        // Generate CSV matching MiniUPSLabel.csv structure
        // Return file path
    }

    public async Task<Model_Dao_Result<string>> FormatForEmailAsync(
        List<Model_ReportRow> data, bool applyDateGrouping = true)
    {
        // Generate HTML table with date grouping
        // Apply alternating colors (#D3D3D3 / #FFFFFF)
        // Return HTML string
    }
}
```

## Phase 3: ViewModel Implementation (2-3 hours)

### Step 1: Create ViewModels

```csharp
// ViewModels/Reporting/ViewModel_Reporting.cs
public partial class ViewModel_Reporting : BaseViewModel
{
    private readonly IService_Reporting _reportingService;
    private readonly IService_ReceivingReporting _receivingReporting;
    private readonly IService_DunnageReporting _dunnageReporting;
    private readonly IService_RoutingReporting _routingReporting;
    private readonly IService_VolvoReporting _volvoReporting;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private bool _isReceivingChecked;

    [ObservableProperty]
    private bool _isDunnageChecked;

    [ObservableProperty]
    private bool _isRoutingChecked;

    [ObservableProperty]
    private bool _isVolvoChecked;

    [ObservableProperty]
    private Dictionary<string, int> _moduleRecordCounts = new();

    [RelayCommand]
    private async Task CheckAvailabilityAsync()
    {
        // Query each module for record counts
        // Update ModuleRecordCounts dictionary
        // Disable checkboxes with 0 records
    }

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        // Generate reports for selected modules
        // Display in tabbed interface
    }
}
```

## Phase 4: View Implementation (2-3 hours)

### Step 1: Create XAML Views

```xml
<!-- Views/Reporting/View_Reporting.xaml -->
<Page x:Class="MTM_Receiving_Application.Views.Reporting.View_Reporting"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid Padding="20">
        <StackPanel Spacing="10">
            <DatePicker Date="{x:Bind ViewModel.StartDate, Mode=TwoWay}" />
            <DatePicker Date="{x:Bind ViewModel.EndDate, Mode=TwoWay}" />
            <Button Content="Check Availability" Command="{x:Bind ViewModel.CheckAvailabilityCommand}" />
            
            <StackPanel>
                <CheckBox Content="Receiving" IsChecked="{x:Bind ViewModel.IsReceivingChecked, Mode=TwoWay}"
                          IsEnabled="{x:Bind ViewModel.ModuleRecordCounts['Receiving'] > 0}" />
                <CheckBox Content="Dunnage" IsChecked="{x:Bind ViewModel.IsDunnageChecked, Mode=TwoWay}"
                          IsEnabled="{x:Bind ViewModel.ModuleRecordCounts['Dunnage'] > 0}" />
                <CheckBox Content="Routing" IsChecked="{x:Bind ViewModel.IsRoutingChecked, Mode=TwoWay}"
                          IsEnabled="{x:Bind ViewModel.ModuleRecordCounts['Routing'] > 0}" />
                <CheckBox Content="Volvo" IsChecked="{x:Bind ViewModel.IsVolvoChecked, Mode=TwoWay}"
                          IsEnabled="{x:Bind ViewModel.ModuleRecordCounts['Volvo'] > 0}" />
            </StackPanel>
            
            <Button Content="Generate Report" Command="{x:Bind ViewModel.GenerateReportCommand}" />
        </StackPanel>
    </Grid>
</Page>
```

## Testing Checklist

### Database Tests
- [ ] Reporting views created successfully
- [ ] Views return correct data for date ranges
- [ ] PO numbers accessible for normalization

### Service Tests
- [ ] PO normalization handles all test cases correctly
- [ ] CSV export matches MiniUPSLabel.csv structure
- [ ] Email formatting applies date grouping and colors
- [ ] Availability check queries all modules correctly

### UI Tests
- [ ] Date range selection works
- [ ] Availability check disables checkboxes with no data
- [ ] Module selection (checkboxes) works correctly
- [ ] Report generation displays data correctly
- [ ] CSV export generates file correctly
- [ ] Email formatting copies to clipboard correctly

## Common Issues & Solutions

### Issue: PO normalization not working correctly
**Solution**: Verify Helper_PONormalizer algorithm matches JavaScript normalizePO() exactly

### Issue: Checkboxes not disabling when no data
**Solution**: Verify ModuleRecordCounts dictionary populated correctly and binding works

### Issue: Email formatting colors not alternating
**Solution**: Verify date grouping logic and color application in FormatForEmailAsync

### Issue: CSV export format incorrect
**Solution**: Verify column order matches MiniUPSLabel.csv exactly

## Next Steps

1. **Complete Phase 1** (Database views) - 10 min
2. **Complete Phase 2** (Models & Services) - 1-2 hours
3. **Complete Phase 3** (ViewModels) - 2-3 hours
4. **Complete Phase 4** (Views) - 2-3 hours

**Total Estimated Time**: 6-9 hours for MVP implementation

## Resources

- **[spec.md](spec.md)** - Complete feature specification
- **[data-model.md](../011-module-reimplementation/data-model.md)** - Database schema reference
- **[tasks.md](../011-module-reimplementation/tasks.md)** - Detailed task breakdown
- **[contracts/](contracts/)** - Service interface definitions
- **[EndOfDayEmail.js](../Documentation/FuturePlans/RoutingLabels/EndOfDayEmail.js)** - Source JavaScript algorithm

---

**Last Updated**: 2026-01-03  
**For Questions**: See [spec.md](spec.md) or contact development team


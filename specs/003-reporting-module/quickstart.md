# Quick Start Guide: Reporting Module

**For**: Developers implementing the Reporting module  
**Prerequisites**: MVVM infrastructure, DI container, MySQL database access, all other modules complete  
**Estimated Setup Time**: 30 minutes

## Prerequisites Checklist

Before starting implementation, ensure:

- [ ] MySQL database `mtm_receiving_application` is accessible
- [ ] Module views will be created (vw_receiving_history, vw_dunnage_history, vw_routing_history, vw_volvo_history)
- [ ] MVVM infrastructure is set up (BaseViewModel from Module_Shared)
- [ ] Dependency Injection is configured (App.xaml.cs)
- [ ] Error handling service is registered (IService_ErrorHandler)
- [ ] Logging service is registered (IService_LoggingUtility)
- [ ] Authentication context provides employee number (Module_Core)
- [ ] IService_Reporting interface will be created in Module_Core/Contracts/Services/

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
// Module_Core/Models/Reporting/Model_ReportRow.cs
using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Core.Models.Reporting;

/// <summary>
/// Unified report row structure for all modules
/// </summary>
public class Model_ReportRow
{
    public DateTime Date { get; set; }
    public Dictionary<string, object> Fields { get; set; } = new();
    public string ModuleName { get; set; } = string.Empty;
    public string GroupKey { get; set; } = string.Empty; // For date grouping
}
```

### Step 2: Implement IService_Reporting

**Note**: First copy the interface specification from `specs/003-reporting-module/contracts/IService_Reporting.cs` to `Module_Core/Contracts/Services/IService_Reporting.cs`

```csharp
// Module_Reporting/Services/Service_Reporting.cs
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Reporting.Services;

public class Service_Reporting : IService_Reporting
{
    private readonly Dao_Reporting _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_Reporting(
        Dao_Reporting dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate, DateTime endDate)
    {
        await _logger.LogInformationAsync("Retrieving Receiving history");
        return await _dao.GetReceivingHistoryAsync(startDate, endDate);
    }

    public string NormalizePONumber(string? poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber)) return "No PO";
        if (poNumber.Equals("Customer Supplied", StringComparison.OrdinalIgnoreCase))
            return "Customer Supplied";
        
        // Extract numeric part and suffix
        string numericPart = new string(poNumber.TakeWhile(char.IsDigit).ToArray());
        string suffix = poNumber.Substring(numericPart.Length);
        
        if (numericPart.Length < 5) return "Validate PO";
        if (numericPart.Length == 5) numericPart = "0" + numericPart;
        
        return "PO-" + numericPart + suffix;
    }

    public async Task<Model_Dao_Result<string>> ExportToCSVAsync(
        List<Model_ReportRow> data, string moduleName)
    {
        // Generate CSV matching MiniUPSLabel.csv structure
        // Return file path
        throw new NotImplementedException();
    }

    public async Task<Model_Dao_Result<string>> FormatForEmailAsync(
        List<Model_ReportRow> data, bool applyDateGrouping = true)
    {
        // Generate HTML table with date grouping
        // Apply alternating colors (#D3D3D3 / #FFFFFF)
        // Return HTML string
        throw new NotImplementedException();
    }

    // Implement other interface methods...
}
```

## Phase 3: ViewModel Implementation (2-3 hours)

### Step 1: Create ViewModels

```csharp
// Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Reporting.ViewModels;

public partial class ViewModel_Reporting_Main : BaseViewModel
{
    private readonly IService_Reporting _reportingService;

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

    public ViewModel_Reporting_Main(
        IService_Reporting reportingService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _reportingService = reportingService;
    }

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
<!-- Module_Reporting/Views/View_Reporting_Main.xaml -->
<Page x:Class="MTM_Receiving_Application.Module_Reporting.Views.View_Reporting_Main"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid Padding="20">
        <StackPanel Spacing="10">
            <CalendarDatePicker Date="{x:Bind ViewModel.StartDate, Mode=TwoWay}" />
            <CalendarDatePicker Date="{x:Bind ViewModel.EndDate, Mode=TwoWay}" />
            <Button Content="Check Availability" Command="{x:Bind ViewModel.CheckAvailabilityCommand}" />
            
            <StackPanel>
                <CheckBox Content="Receiving" 
                          IsChecked="{x:Bind ViewModel.IsReceivingChecked, Mode=TwoWay}" />
                <CheckBox Content="Dunnage" 
                          IsChecked="{x:Bind ViewModel.IsDunnageChecked, Mode=TwoWay}" />
                <CheckBox Content="Routing" 
                          IsChecked="{x:Bind ViewModel.IsRoutingChecked, Mode=TwoWay}" />
                <CheckBox Content="Volvo" 
                          IsChecked="{x:Bind ViewModel.IsVolvoChecked, Mode=TwoWay}" />
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
- **[data-model.md](data-model.md)** - Database schema reference
- **[tasks.md](tasks.md)** - Detailed task breakdown
- **[Module_Core/Contracts/Services/IService_Reporting.cs]** - Service interface (to be created from contracts/ spec)
- **[contracts/IService_Reporting.cs](contracts/IService_Reporting.cs)** - Service interface specification

---

**Last Updated**: 2026-01-04  
**For Questions**: See [spec.md](spec.md) or contact development team


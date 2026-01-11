# Reporting - Schedule Report Configuration

**SVG File**: `08-reporting-modal-schedule.svg`
**Parent Page**: Reporting Settings
**Type**: ContentDialog
**Purpose**: Configure scheduled report parameters

---

## WinUI 3 Implementation

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Dialogs.ScheduleReportDialog"
    Title="Schedule Report"
    PrimaryButtonText="Save"
    CloseButtonText="Cancel"
    DefaultButton="Primary">

    <StackPanel Spacing="16" MinWidth="400">
        <!-- Report Type -->
        <ComboBox
            Header="Report Type"
            SelectedItem="{x:Bind ScheduledReport.ReportType, Mode=TwoWay}"
            Width="360">
            <ComboBoxItem Content="Daily Receiving Summary"/>
            <ComboBoxItem Content="Package Tracking Report"/>
            <ComboBoxItem Content="Dunnage Usage Report"/>
            <ComboBoxItem Content="Routing Performance"/>
        </ComboBox>

        <!-- Schedule -->
        <ComboBox
            Header="Schedule"
            SelectedItem="{x:Bind ScheduledReport.Schedule, Mode=TwoWay}"
            Width="360">
            <ComboBoxItem Content="Daily at 8:00 AM"/>
            <ComboBoxItem Content="Weekly on Monday"/>
            <ComboBoxItem Content="Monthly on 1st"/>
            <ComboBoxItem Content="Custom..."/>
        </ComboBox>

        <!-- Email Recipients -->
        <TextBox
            Header="Email Recipients"
            Text="{x:Bind ScheduledReport.EmailRecipients, Mode=TwoWay}"
            PlaceholderText="comma-separated emails"
            Width="360"/>

        <!-- Active -->
        <ToggleSwitch
            Header="Active"
            IsOn="{x:Bind ScheduledReport.IsActive, Mode=TwoWay}"/>
    </StackPanel>
</ContentDialog>
```

---

## Model

```csharp
public partial class Model_ScheduledReport : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _reportType = string.Empty;

    [ObservableProperty]
    private string _schedule = "Daily at 8:00 AM";

    [ObservableProperty]
    private string _emailRecipients = string.Empty;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private DateTime _nextRunDate;

    [ObservableProperty]
    private DateTime? _lastRunDate;
}
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task AddScheduledReportAsync()
{
    var dialog = new ScheduleReportDialog
    {
        XamlRoot = _xamlRoot
    };

    var result = await dialog.ShowAsync();

    if (result == ContentDialogResult.Primary)
    {
        var saveResult = await _reportingService.SaveScheduledReportAsync(dialog.ScheduledReport);

        if (saveResult.IsSuccess)
        {
            await LoadScheduledReportsAsync();
            StatusMessage = "Scheduled report created";
        }
    }
}
```

---

## Schedule Parser

```csharp
public class ScheduleParser
{
    public static DateTime GetNextRunDate(string schedule)
    {
        var now = DateTime.Now;

        return schedule switch
        {
            "Daily at 8:00 AM" => GetNextDailyRun(now, 8, 0),
            "Weekly on Monday" => GetNextWeeklyRun(now, DayOfWeek.Monday, 8, 0),
            "Monthly on 1st" => GetNextMonthlyRun(now, 1, 8, 0),
            _ => now.AddDays(1)
        };
    }

    private static DateTime GetNextDailyRun(DateTime from, int hour, int minute)
    {
        var next = new DateTime(from.Year, from.Month, from.Day, hour, minute, 0);

        if (next <= from)
        {
            next = next.AddDays(1);
        }

        return next;
    }
}
```

---

## Database Schema

```sql
CREATE TABLE reporting_scheduled_reports (
    id INT AUTO_INCREMENT PRIMARY KEY,
    report_type VARCHAR(100) NOT NULL,
    schedule VARCHAR(50) NOT NULL,
    email_recipients TEXT,
    is_active BOOLEAN DEFAULT 1,
    next_run_date DATETIME NOT NULL,
    last_run_date DATETIME,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

---

## References

- [Task Scheduler Pattern](https://learn.microsoft.com/en-us/dotnet/standard/threading/task-schedulers)

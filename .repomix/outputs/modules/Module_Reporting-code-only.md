# User Provided Header
MTM Receiving Application - Module_Reporting Code-Only Export

# Files

## File: Module_Reporting/Views/View_Reporting_Main.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Reporting.Views.View_Reporting_Main"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="8" Margin="0,0,0,24">
            <TextBlock 
                Text="{x:Bind ViewModel.Title, Mode=OneWay}" 
                Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock 
                Text="Select date range and modules to generate end-of-day reports" 
                Style="{StaticResource BodyTextBlockStyle}"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
        </StackPanel>

        <!-- Date Range Selection -->
        <StackPanel Grid.Row="1" Spacing="12" Margin="0,0,0,24">
            <TextBlock Text="Date Range" Style="{StaticResource SubtitleTextBlockStyle}"/>
            <Grid ColumnSpacing="16">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <CalendarDatePicker 
                    Grid.Column="0"
                    Header="Start Date"
                    Date="{x:Bind ViewModel.StartDate, Mode=TwoWay}"
                    HorizontalAlignment="Stretch"/>

                <CalendarDatePicker 
                    Grid.Column="1"
                    Header="End Date"
                    Date="{x:Bind ViewModel.EndDate, Mode=TwoWay}"
                    HorizontalAlignment="Stretch"/>

                <Button 
                    Grid.Column="2"
                    Content="Check Availability"
                    Command="{x:Bind ViewModel.CheckAvailabilityCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    VerticalAlignment="Bottom"
                    Margin="0,0,0,4"/>
            </Grid>
        </StackPanel>

        <!-- Module Selection -->
        <StackPanel Grid.Row="2" Spacing="12" Margin="0,0,0,24">
            <TextBlock Text="Select Modules" Style="{StaticResource SubtitleTextBlockStyle}"/>
            <Grid ColumnSpacing="24">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <StackPanel Grid.Column="0" Spacing="4">
                    <CheckBox 
                        Content="Receiving"
                        IsChecked="{x:Bind ViewModel.IsReceivingChecked, Mode=TwoWay}"
                        IsEnabled="{x:Bind ViewModel.IsReceivingEnabled, Mode=OneWay}"/>
                    <TextBlock 
                        Text="{x:Bind ViewModel.ReceivingCount, Mode=OneWay}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                        Margin="28,0,0,0"
                        FontSize="12"/>
                    <Button 
                        Content="Generate Report"
                        Command="{x:Bind ViewModel.GenerateReceivingReportCommand}"
                        HorizontalAlignment="Stretch"
                        Margin="0,4,0,0"/>
                </StackPanel>

                <StackPanel Grid.Column="1" Spacing="4">
                    <CheckBox 
                        Content="Dunnage"
                        IsChecked="{x:Bind ViewModel.IsDunnageChecked, Mode=TwoWay}"
                        IsEnabled="{x:Bind ViewModel.IsDunnageEnabled, Mode=OneWay}"/>
                    <TextBlock 
                        Text="{x:Bind ViewModel.DunnageCount, Mode=OneWay}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                        Margin="28,0,0,0"
                        FontSize="12"/>
                    <Button 
                        Content="Generate Report"
                        Command="{x:Bind ViewModel.GenerateDunnageReportCommand}"
                        HorizontalAlignment="Stretch"
                        Margin="0,4,0,0"/>
                </StackPanel>

                <StackPanel Grid.Column="2" Spacing="4">
                    <CheckBox 
                        Content="Routing"
                        IsChecked="{x:Bind ViewModel.IsRoutingChecked, Mode=TwoWay}"
                        IsEnabled="{x:Bind ViewModel.IsRoutingEnabled, Mode=OneWay}"/>
                    <TextBlock 
                        Text="{x:Bind ViewModel.RoutingCount, Mode=OneWay}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                        Margin="28,0,0,0"
                        FontSize="12"/>
                    <Button 
                        Content="Generate Report"
                        Command="{x:Bind ViewModel.GenerateRoutingReportCommand}"
                        HorizontalAlignment="Stretch"
                        Margin="0,4,0,0"/>
                </StackPanel>

                <StackPanel Grid.Column="3" Spacing="4">
                    <CheckBox 
                        Content="Volvo"
                        IsChecked="{x:Bind ViewModel.IsVolvoChecked, Mode=TwoWay}"
                        IsEnabled="{x:Bind ViewModel.IsVolvoEnabled, Mode=OneWay}"/>
                    <TextBlock 
                        Text="{x:Bind ViewModel.VolvoCount, Mode=OneWay}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                        Margin="28,0,0,0"
                        FontSize="12"/>
                    <Button 
                        Content="Generate Report"
                        Command="{x:Bind ViewModel.GenerateVolvoReportCommand}"
                        HorizontalAlignment="Stretch"
                        Margin="0,4,0,0"/>
                </StackPanel>
            </Grid>
        </StackPanel>

        <!-- Report Data Grid -->
        <Border 
            Grid.Row="3" 
            BorderBrush="{ThemeResource SystemControlForegroundBaseMediumLowBrush}"
            BorderThickness="1"
            CornerRadius="4"
            Padding="12">
            <ScrollViewer>
                <StackPanel Spacing="12">
                    <TextBlock 
                        Text="{x:Bind ViewModel.CurrentModuleName, Mode=OneWay}" 
                        Style="{StaticResource SubtitleTextBlockStyle}"/>
                    
                    <ListView 
                        ItemsSource="{x:Bind ViewModel.ReportData, Mode=OneWay}"
                        SelectionMode="None">
                        <ListView.ItemTemplate>
                            <DataTemplate>
                                <Grid Padding="8">
                                    <StackPanel>
                                        <TextBlock Text="{Binding CreatedDate, Mode=OneWay}" FontWeight="Bold"/>
                                        <TextBlock Text="{Binding PartNumber, Mode=OneWay}"/>
                                        <TextBlock Text="{Binding PONumber, Mode=OneWay}" Foreground="Gray"/>
                                    </StackPanel>
                                </Grid>
                            </DataTemplate>
                        </ListView.ItemTemplate>
                    </ListView>
                </StackPanel>
            </ScrollViewer>
        </Border>

        <!-- Action Buttons -->
        <CommandBar Grid.Row="4" DefaultLabelPosition="Right" Margin="0,12,0,0">
            <AppBarButton 
                Icon="Save" 
                Label="Export to CSV"
                Command="{x:Bind ViewModel.ExportToCSVCommand}"/>
            <AppBarButton 
                Icon="Copy" 
                Label="Copy Email Format"
                Command="{x:Bind ViewModel.CopyEmailFormatCommand}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Refresh" 
                Label="Check Availability"
                Command="{x:Bind ViewModel.CheckAvailabilityCommand}"/>
        </CommandBar>
    </Grid>
</Page>
```

## File: Module_Reporting/Views/View_Reporting_Main.xaml.cs
```csharp
public sealed partial class View_Reporting_Main : Page
```

## File: Module_Reporting/Contracts/IService_Reporting.cs
```csharp
public interface IService_Reporting
⋮----
public Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
⋮----
public Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
⋮----
public Task<Model_Dao_Result<List<Model_ReportRow>>> GetRoutingHistoryAsync(
⋮----
public Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
⋮----
public Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
⋮----
public Task<Model_Dao_Result<string>> ExportToCSVAsync(
⋮----
public Task<Model_Dao_Result<string>> FormatForEmailAsync(
⋮----
public string NormalizePONumber(string? poNumber);
```

## File: Module_Reporting/Data/Dao_Reporting.cs
```csharp
public class Dao_Reporting
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
⋮----
await using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync();
await using var command = new MySqlCommand(query, connection);
command.Parameters.AddWithValue("@StartDate", startDate.Date);
command.Parameters.AddWithValue("@EndDate", endDate.Date);
await using var reader = await command.ExecuteReaderAsync();
⋮----
while (await reader.ReadAsync())
⋮----
rows.Add(new Model_ReportRow
⋮----
Id = reader.GetGuid("id").ToString(),
PONumber = reader.IsDBNull(reader.GetOrdinal("po_number")) ? null : reader.GetString("po_number"),
PartNumber = reader.IsDBNull(reader.GetOrdinal("part_number")) ? null : reader.GetString("part_number"),
PartDescription = reader.IsDBNull(reader.GetOrdinal("part_description")) ? null : reader.GetString("part_description"),
Quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) ? null : reader.GetDecimal("quantity"),
WeightLbs = reader.IsDBNull(reader.GetOrdinal("weight_lbs")) ? null : reader.GetDecimal("weight_lbs"),
HeatLotNumber = reader.IsDBNull(reader.GetOrdinal("heat_lot_number")) ? null : reader.GetString("heat_lot_number"),
CreatedDate = reader.GetDateTime("created_date"),
EmployeeNumber = reader.IsDBNull(reader.GetOrdinal("employee_number")) ? null : reader.GetString("employee_number"),
SourceModule = reader.GetString("source_module")
⋮----
return Model_Dao_Result_Factory.Success(rows);
⋮----
public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
⋮----
DunnageType = reader.IsDBNull(reader.GetOrdinal("dunnage_type")) ? null : reader.GetString("dunnage_type"),
⋮----
SpecsCombined = reader.IsDBNull(reader.GetOrdinal("specs_combined")) ? null : reader.GetString("specs_combined"),
⋮----
public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetRoutingHistoryAsync(
⋮----
Id = reader.GetInt32("id").ToString(),
⋮----
LineNumber = reader.IsDBNull(reader.GetOrdinal("line_number")) ? null : reader.GetString("line_number"),
PackageDescription = reader.IsDBNull(reader.GetOrdinal("package_description")) ? null : reader.GetString("package_description"),
DeliverTo = reader.IsDBNull(reader.GetOrdinal("deliver_to")) ? null : reader.GetString("deliver_to"),
Department = reader.IsDBNull(reader.GetOrdinal("department")) ? null : reader.GetString("department"),
Location = reader.IsDBNull(reader.GetOrdinal("location")) ? null : reader.GetString("location"),
⋮----
EmployeeNumber = reader.IsDBNull(reader.GetOrdinal("employee_number")) ? null : reader.GetInt32("employee_number").ToString(),
⋮----
OtherReason = reader.IsDBNull(reader.GetOrdinal("other_reason")) ? null : reader.GetString("other_reason"),
⋮----
public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
⋮----
ShipmentNumber = reader.IsDBNull(reader.GetOrdinal("shipment_number")) ? null : reader.GetInt32("shipment_number"),
⋮----
ReceiverNumber = reader.IsDBNull(reader.GetOrdinal("receiver_number")) ? null : reader.GetString("receiver_number"),
Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString("status"),
⋮----
PartCount = reader.IsDBNull(reader.GetOrdinal("part_count")) ? null : reader.GetInt32("part_count"),
⋮----
public async Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
⋮----
return Model_Dao_Result_Factory.Success(availability);
⋮----
private async Task<int> GetCountAsync(
⋮----
var result = await command.ExecuteScalarAsync();
return Convert.ToInt32(result);
```

## File: Module_Reporting/Services/Service_Reporting.cs
```csharp
public class Service_Reporting : IService_Reporting
⋮----
private readonly Dao_Reporting _dao;
private readonly IService_LoggingUtility _logger;
⋮----
_dao = dao ?? throw new ArgumentNullException(nameof(dao));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
⋮----
_logger.LogInfo($"Retrieving Receiving history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
var result = await _dao.GetReceivingHistoryAsync(startDate, endDate);
⋮----
public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
⋮----
_logger.LogInfo($"Retrieving Dunnage history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
return await _dao.GetDunnageHistoryAsync(startDate, endDate);
⋮----
public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetRoutingHistoryAsync(
⋮----
_logger.LogInfo($"Retrieving Routing history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
var result = await _dao.GetRoutingHistoryAsync(startDate, endDate);
⋮----
public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
⋮----
_logger.LogInfo($"Retrieving Volvo history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
return await _dao.GetVolvoHistoryAsync(startDate, endDate);
⋮----
public async Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
⋮----
_logger.LogInfo($"Checking module availability from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
return await _dao.CheckAvailabilityAsync(startDate, endDate);
⋮----
public string NormalizePONumber(string? poNumber)
⋮----
if (string.IsNullOrWhiteSpace(poNumber))
⋮----
poNumber = poNumber.Trim();
if (poNumber.Equals("Customer Supplied", StringComparison.OrdinalIgnoreCase))
⋮----
string numericPart = new string(poNumber.TakeWhile(char.IsDigit).ToArray());
string suffix = poNumber.Substring(numericPart.Length);
⋮----
public async Task<Model_Dao_Result<string>> ExportToCSVAsync(
⋮----
_logger.LogInfo($"Exporting {data.Count} records to CSV for {moduleName} module");
var csv = new StringBuilder();
switch (moduleName.ToLower())
⋮----
csv.AppendLine("PO Number,Part,Description,Qty,Weight,Heat/Lot,Date");
⋮----
csv.AppendLine($"\"{row.PONumber ?? ""}\",\"{row.PartNumber ?? ""}\",\"{row.PartDescription ?? ""}\",{row.Quantity ?? 0},{row.WeightLbs ?? 0},\"{row.HeatLotNumber ?? ""}\",{row.CreatedDate:yyyy-MM-dd}");
⋮----
csv.AppendLine("Type,Part,Specs,Qty,Date");
⋮----
csv.AppendLine($"\"{row.DunnageType ?? ""}\",\"{row.PartNumber ?? ""}\",\"{row.SpecsCombined ?? ""}\",{row.Quantity ?? 0},{row.CreatedDate:yyyy-MM-dd}");
⋮----
csv.AppendLine("Deliver To,Department,Package Description,PO Number,Work Order,Date");
⋮----
csv.AppendLine($"\"{row.DeliverTo ?? ""}\",\"{row.Department ?? ""}\",\"{row.PackageDescription ?? ""}\",\"{row.PONumber ?? ""}\",\"{row.WorkOrderNumber ?? ""}\",{row.CreatedDate:yyyy-MM-dd}");
⋮----
csv.AppendLine("Shipment Number,PO Number,Receiver Number,Status,Date,Part Count");
⋮----
csv.AppendLine($"{row.ShipmentNumber ?? 0},\"{row.PONumber ?? ""}\",\"{row.ReceiverNumber ?? ""}\",\"{row.Status ?? ""}\",{row.CreatedDate:yyyy-MM-dd},{row.PartCount ?? 0}");
⋮----
var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
⋮----
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var mtmFolder = Path.Combine(appDataPath, "MTM_Receiving_Application", "Reports");
Directory.CreateDirectory(mtmFolder);
var filePath = Path.Combine(mtmFolder, fileName);
await File.WriteAllTextAsync(filePath, csv.ToString());
_logger.LogInfo($"CSV exported successfully to {filePath}");
return Model_Dao_Result_Factory.Success(filePath);
⋮----
_logger.LogError($"Error exporting CSV: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<string>> FormatForEmailAsync(
⋮----
_logger.LogInfo($"Formatting {data.Count} records for email");
⋮----
return Model_Dao_Result_Factory.Success("<p>No data to display</p>");
⋮----
var html = new StringBuilder();
html.AppendLine("<table style='border-collapse: collapse; width: 100%;'>");
⋮----
html.AppendLine("<thead>");
html.AppendLine("<tr style='background-color: #f0f0f0; font-weight: bold;'>");
⋮----
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>PO Number</th>");
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Part</th>");
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Description</th>");
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Qty</th>");
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Weight</th>");
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Heat/Lot</th>");
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Date</th>");
⋮----
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Type</th>");
⋮----
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Specs</th>");
⋮----
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Deliver To</th>");
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Department</th>");
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Package Description</th>");
⋮----
html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Data</th>");
⋮----
html.AppendLine("</tr>");
html.AppendLine("</thead>");
html.AppendLine("<tbody>");
⋮----
html.AppendLine($"<tr style='background-color: {bgColor};'>");
⋮----
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PONumber ?? ""}</td>");
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PartNumber ?? ""}</td>");
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PartDescription ?? ""}</td>");
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.Quantity ?? 0}</td>");
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.WeightLbs ?? 0}</td>");
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.HeatLotNumber ?? ""}</td>");
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.CreatedDate:yyyy-MM-dd}</td>");
⋮----
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.DunnageType ?? ""}</td>");
⋮----
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.SpecsCombined ?? ""}</td>");
⋮----
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.DeliverTo ?? ""}</td>");
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.Department ?? ""}</td>");
html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PackageDescription ?? ""}</td>");
⋮----
html.AppendLine("</tbody>");
html.AppendLine("</table>");
return Model_Dao_Result_Factory.Success(html.ToString());
⋮----
_logger.LogError($"Error formatting email: {ex.Message}", ex);
```

## File: Module_Reporting/ViewModels/ViewModel_Reporting_Main.cs
```csharp
public partial class ViewModel_Reporting_Main : ViewModel_Shared_Base
⋮----
private readonly IService_Reporting _reportingService;
⋮----
private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);
⋮----
private DateTimeOffset _endDate = DateTimeOffset.Now;
⋮----
_reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
⋮----
private async Task CheckAvailabilityAsync()
⋮----
var result = await _reportingService.CheckAvailabilityAsync(
⋮----
ReceivingCount = result.Data.GetValueOrDefault("Receiving", 0);
DunnageCount = result.Data.GetValueOrDefault("Dunnage", 0);
RoutingCount = result.Data.GetValueOrDefault("Routing", 0);
VolvoCount = result.Data.GetValueOrDefault("Volvo", 0);
⋮----
_logger.LogError($"Error checking availability: {ex.Message}", ex);
⋮----
private async Task GenerateReceivingReportAsync()
⋮----
() => _reportingService.GetReceivingHistoryAsync(StartDate.DateTime, EndDate.DateTime));
⋮----
private bool CanGenerateReceiving() => IsReceivingChecked && IsReceivingEnabled && !IsBusy;
⋮----
private async Task GenerateDunnageReportAsync()
⋮----
() => _reportingService.GetDunnageHistoryAsync(StartDate.DateTime, EndDate.DateTime));
⋮----
private bool CanGenerateDunnage() => IsDunnageChecked && IsDunnageEnabled && !IsBusy;
⋮----
private async Task GenerateRoutingReportAsync()
⋮----
() => _reportingService.GetRoutingHistoryAsync(StartDate.DateTime, EndDate.DateTime));
⋮----
private bool CanGenerateRouting() => IsRoutingChecked && IsRoutingEnabled && !IsBusy;
⋮----
private async Task GenerateVolvoReportAsync()
⋮----
() => _reportingService.GetVolvoHistoryAsync(StartDate.DateTime, EndDate.DateTime));
⋮----
private bool CanGenerateVolvo() => IsVolvoChecked && IsVolvoEnabled && !IsBusy;
⋮----
private async Task ExportToCSVAsync()
⋮----
var result = await _reportingService.ExportToCSVAsync(
ReportData.ToList(),
⋮----
if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
⋮----
_logger.LogInfo($"CSV exported: {result.Data}");
⋮----
_logger.LogError($"Error exporting CSV: {ex.Message}", ex);
⋮----
private bool CanExport() => ReportData.Count > 0 && !string.IsNullOrEmpty(CurrentModuleName) && !IsBusy;
⋮----
private async Task CopyEmailFormatAsync()
⋮----
var result = await _reportingService.FormatForEmailAsync(
⋮----
dataPackage.SetHtmlFormat(result.Data);
dataPackage.SetText(result.Data);
Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
⋮----
_logger.LogInfo("Email format copied to clipboard");
⋮----
_logger.LogError($"Error copying email format: {ex.Message}", ex);
⋮----
private bool CanCopyEmail() => ReportData.Count > 0 && !IsBusy;
private async Task GenerateReportForModuleAsync(
⋮----
ReportData.Clear();
⋮----
ReportData.Add(row);
⋮----
_logger.LogInfo($"Generated {moduleName} report: {ReportData.Count} records");
ExportToCSVCommand.NotifyCanExecuteChanged();
CopyEmailFormatCommand.NotifyCanExecuteChanged();
⋮----
_logger.LogError($"Error generating {moduleName} report: {ex.Message}", ex);
⋮----
partial void OnIsReceivingCheckedChanged(bool value)
⋮----
GenerateReceivingReportCommand.NotifyCanExecuteChanged();
⋮----
partial void OnIsDunnageCheckedChanged(bool value)
⋮----
GenerateDunnageReportCommand.NotifyCanExecuteChanged();
⋮----
partial void OnIsRoutingCheckedChanged(bool value)
⋮----
GenerateRoutingReportCommand.NotifyCanExecuteChanged();
⋮----
partial void OnIsVolvoCheckedChanged(bool value)
⋮----
GenerateVolvoReportCommand.NotifyCanExecuteChanged();
```

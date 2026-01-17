# User Provided Header
MTM Receiving Application - Module_Routing Code-Only Export

# Files

## File: Module_Routing/Constants/Constant_Routing.cs
```csharp
public static class Constant_Routing
```

## File: Module_Routing/Constants/Constant_RoutingConfiguration.cs
```csharp
public static class Constant_RoutingConfiguration
```

## File: Module_Routing/Data/Dao_RoutingRecipient.cs
```csharp
public class Dao_RoutingRecipient
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllActiveRecipientsAsync()
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetTopRecipientsByUsageAsync(int employeeNumber, int topCount = 5)
⋮----
private Model_RoutingRecipient MapFromReader(IDataReader reader)
⋮----
return new Model_RoutingRecipient
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
Name = reader.GetString(reader.GetOrdinal("name")),
Location = reader.GetString(reader.GetOrdinal("location")),
Department = reader.IsDBNull(reader.GetOrdinal("department")) ? null : reader.GetString(reader.GetOrdinal("department")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
UpdatedDate = reader.GetDateTime(reader.GetOrdinal("updated_date"))
⋮----
private Model_RoutingRecipient MapFromReaderWithUsage(IDataReader reader)
⋮----
recipient.UsageCount = reader.GetInt32(reader.GetOrdinal("usage_count"));
```

## File: Module_Routing/Enums/Enum_Routing_WorkflowStep.cs
```csharp

```

## File: Module_Routing/Enums/Enum_RoutingMode.cs
```csharp

```

## File: Module_Routing/Interfaces/IDao_RoutingLabel.cs
```csharp
public interface IDao_RoutingLabel
⋮----
Task<Model_Dao_Result<int>> InsertLabelAsync(Model_RoutingLabel label);
Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label);
Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId);
Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0);
Task<Model_Dao_Result> DeleteLabelAsync(int labelId);
Task<Model_Dao_Result> MarkLabelExportedAsync(int labelId);
Task<Model_Dao_Result<Model_RoutingLabel>> CheckDuplicateLabelAsync(string poNumber, string lineNumber, int recipientId, int hoursWindow = 24);
Task<Model_Dao_Result<(bool Exists, int? ExistingLabelId)>> CheckDuplicateAsync(string poNumber, string lineNumber, int recipientId, DateTime createdWithinDate);
Task<Model_Dao_Result> MarkExportedAsync(List<int> labelIds);
```

## File: Module_Routing/Interfaces/IDao_RoutingLabelHistory.cs
```csharp
public interface IDao_RoutingLabelHistory
⋮----
Task<Model_Dao_Result> InsertHistoryAsync(Model_RoutingLabelHistory history);
Task<Model_Dao_Result> InsertHistoryBatchAsync(List<Model_RoutingLabelHistory> historyEntries);
Task<Model_Dao_Result<List<Model_RoutingLabelHistory>>> GetHistoryByLabelAsync(int labelId);
```

## File: Module_Routing/Interfaces/IDao_RoutingRecipient.cs
```csharp
public interface IDao_RoutingRecipient
⋮----
Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllActiveRecipientsAsync();
Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetTopRecipientsByUsageAsync(int employeeNumber, int topCount = 5);
```

## File: Module_Routing/Interfaces/IDao_RoutingUsageTracking.cs
```csharp
public interface IDao_RoutingUsageTracking
⋮----
Task<Model_Dao_Result> IncrementUsageAsync(int employeeNumber, int recipientId);
```

## File: Module_Routing/Services/IRoutingInforVisualService.cs
```csharp
public interface IRoutingInforVisualService
⋮----
public Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber);
public Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPoLinesAsync(string poNumber);
public Task<Model_Dao_Result<Model_InforVisualPOLine>> GetPoLineAsync(string poNumber, string lineNumber);
public Task<Model_Dao_Result<bool>> CheckConnectionAsync();
```

## File: Module_Routing/Services/IRoutingRecipientService.cs
```csharp
public interface IRoutingRecipientService
⋮----
public Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetActiveRecipientsSortedByUsageAsync(int employeeNumber);
public Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllRecipientsAsync();
public Task<Model_Dao_Result<Model_RoutingRecipient>> GetRecipientByIdAsync(int recipientId);
public List<Model_RoutingRecipient> FilterRecipients(List<Model_RoutingRecipient> recipients, string searchText);
public Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetQuickAddRecipientsAsync(int employeeNumber);
public Task<Model_Dao_Result<bool>> ValidateRecipientExistsAsync(int recipientId);
```

## File: Module_Routing/Services/IRoutingService.cs
```csharp
public interface IRoutingService
⋮----
public Task<Model_Dao_Result<int>> CreateLabelAsync(Model_RoutingLabel label);
public Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label, int editedByEmployeeNumber);
public Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId);
public Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0);
public Task<Model_Dao_Result<(bool Exists, int? ExistingLabelId)>> CheckDuplicateLabelAsync(
⋮----
public Task<Model_Dao_Result> ExportLabelToCsvAsync(Model_RoutingLabel label);
public Task<Model_Dao_Result> RegenerateLabelCsvAsync(int labelId);
public Task<Model_Dao_Result> ResetCsvFileAsync();
public Model_Dao_Result ValidateLabel(Model_RoutingLabel label);
```

## File: Module_Routing/Services/IRoutingUsageTrackingService.cs
```csharp
public interface IRoutingUsageTrackingService
⋮----
public Task<Model_Dao_Result> IncrementUsageCountAsync(int employeeNumber, int recipientId);
public Task<Model_Dao_Result<int>> GetUsageCountAsync(int employeeNumber, int recipientId);
public Task<Model_Dao_Result<int>> GetEmployeeLabelCountAsync(int employeeNumber);
```

## File: Module_Routing/Services/IRoutingUserPreferenceService.cs
```csharp
public interface IRoutingUserPreferenceService
⋮----
public Task<Model_Dao_Result<Model_RoutingUserPreference>> GetUserPreferenceAsync(int employeeNumber);
public Task<Model_Dao_Result> SaveUserPreferenceAsync(Model_RoutingUserPreference preference);
public Task<Model_Dao_Result> ResetToDefaultsAsync(int employeeNumber);
```

## File: Module_Routing/Services/RoutingRecipientService.cs
```csharp
public class RoutingRecipientService : IRoutingRecipientService
⋮----
private readonly Dao_RoutingRecipient _daoRecipient;
private readonly IService_LoggingUtility _logger;
⋮----
_daoRecipient = daoRecipient ?? throw new ArgumentNullException(nameof(daoRecipient));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetActiveRecipientsSortedByUsageAsync(int employeeNumber)
⋮----
await _logger.LogInfoAsync($"Getting active recipients sorted by usage for employee {employeeNumber}");
return await _daoRecipient.GetAllActiveRecipientsAsync();
⋮----
await _logger.LogErrorAsync($"Error getting active recipients: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllRecipientsAsync()
⋮----
await _logger.LogInfoAsync("Getting all recipients (including inactive)");
⋮----
await _logger.LogErrorAsync($"Error getting all recipients: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<Model_RoutingRecipient>> GetRecipientByIdAsync(int recipientId)
⋮----
await _logger.LogInfoAsync($"Getting recipient by ID: {recipientId}");
var result = await _daoRecipient.GetAllActiveRecipientsAsync();
⋮----
return Model_Dao_Result_Factory.Success(recipient);
⋮----
await _logger.LogErrorAsync($"Error getting recipient {recipientId}: {ex.Message}", ex);
⋮----
public List<Model_RoutingRecipient> FilterRecipients(List<Model_RoutingRecipient> recipients, string searchText)
⋮----
if (string.IsNullOrWhiteSpace(searchText))
⋮----
var search = searchText.ToLowerInvariant();
return recipients.Where(r =>
r.Name.ToLowerInvariant().Contains(search) ||
(r.Location?.ToLowerInvariant().Contains(search) ?? false) ||
(r.Department?.ToLowerInvariant().Contains(search) ?? false)
).ToList();
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetQuickAddRecipientsAsync(int employeeNumber)
⋮----
await _logger.LogInfoAsync($"Getting Quick Add recipients for employee {employeeNumber}");
return await _daoRecipient.GetTopRecipientsByUsageAsync(employeeNumber, 5);
⋮----
await _logger.LogErrorAsync($"Error getting Quick Add recipients: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<bool>> ValidateRecipientExistsAsync(int recipientId)
⋮----
return Model_Dao_Result_Factory.Success(recipientResult.IsSuccess);
⋮----
await _logger.LogErrorAsync($"Error validating recipient {recipientId}: {ex.Message}", ex);
```

## File: Module_Routing/Services/RoutingUserPreferenceService.cs
```csharp
public class RoutingUserPreferenceService : IRoutingUserPreferenceService
⋮----
private readonly Dao_RoutingUserPreference _daoUserPreference;
private readonly IService_LoggingUtility _logger;
⋮----
_daoUserPreference = daoUserPreference ?? throw new ArgumentNullException(nameof(daoUserPreference));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result<Model_RoutingUserPreference>> GetUserPreferenceAsync(int employeeNumber)
⋮----
await _logger.LogInfoAsync($"Getting user preference for employee {employeeNumber}");
var result = await _daoUserPreference.GetUserPreferenceAsync(employeeNumber);
⋮----
var defaultPreference = new Model_RoutingUserPreference
⋮----
return Model_Dao_Result_Factory.Success(defaultPreference);
⋮----
await _logger.LogErrorAsync($"Error getting user preference for employee {employeeNumber}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> SaveUserPreferenceAsync(Model_RoutingUserPreference preference)
⋮----
await _logger.LogInfoAsync($"Saving user preference for employee {preference.EmployeeNumber}");
⋮----
return Model_Dao_Result_Factory.Failure($"Invalid mode: {preference.DefaultMode}");
⋮----
return await _daoUserPreference.SaveUserPreferenceAsync(preference);
⋮----
await _logger.LogErrorAsync($"Error saving user preference: {ex.Message}", ex);
return Model_Dao_Result_Factory.Failure($"Error saving preferences: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> ResetToDefaultsAsync(int employeeNumber)
⋮----
await _logger.LogInfoAsync($"Resetting preferences to defaults for employee {employeeNumber}");
⋮----
return await _daoUserPreference.SaveUserPreferenceAsync(defaultPreference);
⋮----
await _logger.LogErrorAsync($"Error resetting preferences: {ex.Message}", ex);
return Model_Dao_Result_Factory.Failure($"Error resetting preferences: {ex.Message}", ex);
```

## File: Module_Routing/ViewModels/RoutingEditModeViewModel.cs
```csharp
public partial class RoutingEditModeViewModel : ViewModel_Shared_Base
⋮----
private readonly IRoutingService _routingService;
private readonly IRoutingRecipientService _recipientService;
private readonly IService_UserSessionManager _sessionManager;
⋮----
public async Task InitializeAsync()
⋮----
var labelsResult = await _routingService.GetAllLabelsAsync(limit: 1000, offset: 0);
⋮----
Labels.Clear();
⋮----
Labels.Add(label);
⋮----
var recipientsResult = await _recipientService.GetActiveRecipientsSortedByUsageAsync(0);
⋮----
AllRecipients.Clear();
⋮----
AllRecipients.Add(recipient);
⋮----
_errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
⋮----
partial void OnSearchTextChanged(string value)
⋮----
private void ApplySearchFilter()
⋮----
FilteredLabels.Clear();
⋮----
var filtered = string.IsNullOrWhiteSpace(searchLower)
? Labels.ToList()
: Labels.Where(l =>
(l.PONumber?.ToLower().Contains(searchLower) ?? false) ||
(l.RecipientName?.ToLower().Contains(searchLower) ?? false) ||
(l.Description?.ToLower().Contains(searchLower) ?? false)
).ToList();
⋮----
FilteredLabels.Add(label);
⋮----
private async Task SaveEditedLabelAsync(Model_RoutingLabel editedLabel)
⋮----
var updateResult = await _routingService.UpdateLabelAsync(editedLabel, currentEmployeeNumber);
⋮----
var index = Labels.IndexOf(SelectedLabel);
⋮----
await _errorHandler.ShowUserErrorAsync(updateResult.ErrorMessage, "Update Failed", nameof(SaveEditedLabelAsync));
⋮----
private async Task CompareAndLogChangesAsync(Model_RoutingLabel oldLabel, Model_RoutingLabel newLabel)
⋮----
private async Task ReprintLabelAsync()
⋮----
var exportResult = await _routingService.ExportLabelToCsvAsync(SelectedLabel);
⋮----
await _errorHandler.ShowUserErrorAsync(exportResult.ErrorMessage, "Reprint Failed", nameof(ReprintLabelAsync));
⋮----
_errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Low,
```

## File: Module_Routing/ViewModels/RoutingManualEntryViewModel.cs
```csharp
public partial class RoutingManualEntryViewModel : ViewModel_Shared_Base
⋮----
private readonly IRoutingService _routingService;
private readonly IRoutingInforVisualService _inforVisualService;
private readonly IRoutingRecipientService _recipientService;
private readonly IRoutingUsageTrackingService _usageTrackingService;
⋮----
public async Task InitializeAsync()
⋮----
var recipientsResult = await _recipientService.GetAllRecipientsAsync();
⋮----
await _errorHandler.ShowUserErrorAsync(recipientsResult.ErrorMessage, "Load Failed", nameof(InitializeAsync));
⋮----
_errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
⋮----
private void AddNewRow()
⋮----
var newLabel = new Model_RoutingLabel
⋮----
Labels.Add(newLabel);
⋮----
private void DeleteSelectedRow()
⋮----
Labels.Remove(SelectedLabel);
⋮----
private bool CanDeleteRow() => SelectedLabel != null && !IsBusy;
public async Task ValidatePOAsync(Model_RoutingLabel label)
⋮----
if (string.IsNullOrWhiteSpace(label.PONumber))
⋮----
var poResult = await _inforVisualService.ValidatePoNumberAsync(label.PONumber);
⋮----
var linesResult = await _inforVisualService.GetPoLinesAsync(label.PONumber);
⋮----
var line = linesResult.Data.First();
⋮----
label.LineNumber = line.LineNumber.ToString();
⋮----
await _logger.LogErrorAsync($"Error validating PO: {ex.Message}", ex, nameof(ValidatePOAsync));
⋮----
private async Task SaveAllLabelsAsync()
⋮----
await _errorHandler.ShowUserErrorAsync(string.Join("\n", validationErrors), "Validation Failed", nameof(SaveAllLabelsAsync));
⋮----
foreach (var label in Labels.ToList())
⋮----
var createResult = await _routingService.CreateLabelAsync(label);
⋮----
await _usageTrackingService.IncrementUsageCountAsync(
⋮----
await _logger.LogErrorAsync($"Failed to create label: {createResult.ErrorMessage}");
⋮----
Labels.Clear();
⋮----
_errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Error,
⋮----
private bool CanSaveAll() => !IsBusy && Labels.Count > 0;
private System.Collections.Generic.List<string> ValidateAllRows()
⋮----
if (string.IsNullOrWhiteSpace(label.PONumber) && !label.OtherReasonId.HasValue)
⋮----
errors.Add($"Row {rowNum}: PO Number or OTHER reason required");
⋮----
errors.Add($"Row {rowNum}: Recipient required");
⋮----
errors.Add($"Row {rowNum}: Quantity must be greater than 0");
⋮----
partial void OnSelectedLabelChanged(Model_RoutingLabel? value)
⋮----
DeleteSelectedRowCommand.NotifyCanExecuteChanged();
⋮----
partial void OnLabelsChanged(ObservableCollection<Model_RoutingLabel> value)
⋮----
SaveAllLabelsCommand.NotifyCanExecuteChanged();
```

## File: Module_Routing/ViewModels/RoutingModeSelectionViewModel.cs
```csharp
public partial class RoutingModeSelectionViewModel : ViewModel_Shared_Base
⋮----
private readonly IRoutingUserPreferenceService _userPreferenceService;
private readonly IService_Navigation _navigationService;
private readonly IService_UserSessionManager _sessionManager;
⋮----
public void SetNavigationFrame(Frame frame)
⋮----
public async Task InitializeAsync()
⋮----
var prefsResult = await _userPreferenceService.GetUserPreferenceAsync(employeeNumber);
⋮----
_errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Low,
⋮----
private async Task SelectWizardModeAsync() => await NavigateToModeAsync(Enum_RoutingMode.WIZARD);
⋮----
private async Task SelectManualEntryModeAsync() => await NavigateToModeAsync(Enum_RoutingMode.MANUAL);
⋮----
private async Task SelectEditModeAsync() => await NavigateToModeAsync(Enum_RoutingMode.EDIT);
⋮----
private async Task SetWizardAsDefaultAsync(bool isChecked)
⋮----
private async Task SetManualAsDefaultAsync(bool isChecked)
⋮----
private async Task SetEditAsDefaultAsync(bool isChecked)
⋮----
private async Task SavePreferenceAsync(Enum_RoutingMode mode)
⋮----
var preference = new Model_RoutingUserPreference
⋮----
DefaultMode = mode.ToString(),
⋮----
var result = await _userPreferenceService.SaveUserPreferenceAsync(preference);
⋮----
await _logger.LogInfoAsync($"Default mode set to {mode} for employee {employeeNumber}");
⋮----
private async Task NavigateToModeAsync(Enum_RoutingMode mode)
⋮----
_errorHandler.HandleException(new InvalidOperationException("Navigation Frame not set"),
⋮----
_ => throw new ArgumentException($"Unknown mode: {mode}")
⋮----
_navigationService.NavigateTo(_frame, viewName);
await _logger.LogInfoAsync($"Navigated to {viewName}");
⋮----
_errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
```

## File: Module_Routing/ViewModels/RoutingWizardStep2ViewModel.cs
```csharp
public partial class RoutingWizardStep2ViewModel : ObservableObject
⋮----
private readonly IRoutingRecipientService _recipientService;
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_LoggingUtility _logger;
private readonly IService_UserSessionManager _sessionManager;
private readonly RoutingWizardContainerViewModel _containerViewModel;
⋮----
public async Task LoadRecipientsAsync()
⋮----
var quickAddResult = await _recipientService.GetQuickAddRecipientsAsync(
⋮----
QuickAddRecipients.Clear();
⋮----
QuickAddRecipients.Add(recipient);
⋮----
var allRecipientsResult = await _recipientService.GetActiveRecipientsSortedByUsageAsync(0);
⋮----
_allRecipients.Clear();
⋮----
_allRecipients.Add(recipient);
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private void QuickAddRecipient(Model_RoutingRecipient recipient)
⋮----
private void ApplyFilter()
⋮----
FilteredRecipients.Clear();
if (string.IsNullOrWhiteSpace(SearchText))
⋮----
FilteredRecipients.Add(recipient);
⋮----
var searchLower = SearchText.ToLower();
var filtered = _allRecipients.Where(r =>
r.Name.ToLower().Contains(searchLower) ||
(r.Location?.ToLower().Contains(searchLower) ?? false) ||
(r.Department?.ToLower().Contains(searchLower) ?? false)).ToList();
⋮----
partial void OnSearchTextChanged(string value)
⋮----
private void ProceedToStep3()
⋮----
_logger.LogInfo($"ProceedToStep3 called with recipient: {SelectedRecipient?.Name ?? "null"}");
⋮----
_logger.LogWarning("ProceedToStep3: No recipient selected");
⋮----
_logger.LogInfo($"Updated container with recipient: {SelectedRecipient.Name}");
_containerViewModel.NavigateToStep3Command.Execute(null);
_logger.LogInfo("Navigation to Step 3 triggered");
⋮----
_logger.LogError($"Error in ProceedToStep3: {ex.Message}", ex);
⋮----
private bool CanProceedToStep3()
⋮----
_logger.LogInfo($"CanProceedToStep3: {canProceed} (SelectedRecipient: {SelectedRecipient?.Name ?? "null"})");
⋮----
/// <summary>
/// Go back to Step 1
/// </summary>
⋮----
private void NavigateBackToStep1()
⋮----
_containerViewModel.NavigateToStep1Command.Execute(null);
⋮----
/// Get current employee number from session
/// Issue #7: Implemented using IService_UserSessionManager
⋮----
private int GetCurrentEmployeeNumber()
⋮----
/// Notify command can execute changed when selection changes
⋮----
partial void OnSelectedRecipientChanged(Model_RoutingRecipient? value)
⋮----
_logger.LogInfo($"Selected recipient changed to: {value?.Name ?? "null"}");
ProceedToStep3Command.NotifyCanExecuteChanged();
```

## File: Module_Routing/Views/RoutingEditModeView.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<Page
    x:Class="MTM_Receiving_Application.Module_Routing.Views.RoutingEditModeView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Routing.ViewModels"
    xmlns:models="using:MTM_Receiving_Application.Module_Routing.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Routing.Converters"
    Loaded="OnPageLoaded">

    <Page.Resources>
        <converters:NullToBooleanConverter x:Key="NullToBooleanConverter"/>
    </Page.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock 
            Grid.Row="0"
            Text="Edit Mode - Label History"
            Style="{StaticResource TitleTextBlockStyle}"/>

        <!-- Search Bar -->
        <Grid Grid.Row="1" ColumnSpacing="12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <TextBox
                Grid.Column="0"
                PlaceholderText="Search by PO, Recipient, or Part ID..."
                Text="{x:Bind ViewModel.SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

            <Button
                Grid.Column="1"
                Content="Edit Selected"
                Command="{x:Bind EditSelectedLabelCommand}"
                IsEnabled="{x:Bind ViewModel.SelectedLabel, Mode=OneWay, Converter={StaticResource NullToBooleanConverter}}"/>

            <Button
                Grid.Column="2"
                Content="Reprint"
                Command="{x:Bind ViewModel.ReprintLabelCommand}"
                IsEnabled="{x:Bind ViewModel.SelectedLabel, Mode=OneWay, Converter={StaticResource NullToBooleanConverter}}"/>
        </Grid>

        <!-- Labels DataGrid -->
        <controls:DataGrid
            Grid.Row="2"
            ItemsSource="{x:Bind ViewModel.FilteredLabels, Mode=OneWay}"
            SelectedItem="{x:Bind ViewModel.SelectedLabel, Mode=TwoWay}"
            AutoGenerateColumns="False"
            IsReadOnly="True"
            AlternatingRowBackground="{ThemeResource LayerFillColorDefaultBrush}"
            SelectionMode="Single"
            DoubleTapped="OnDataGridDoubleTapped">
            
            <controls:DataGrid.Columns>
                <controls:DataGridTextColumn Header="ID" Binding="{Binding Id}" Width="60"/>
                <controls:DataGridTextColumn Header="PO #" Binding="{Binding PONumber}" Width="100"/>
                <controls:DataGridTextColumn Header="Line" Binding="{Binding POLine}" Width="60"/>
                <controls:DataGridTextColumn Header="Part ID" Binding="{Binding PartID}" Width="120"/>
                <controls:DataGridTextColumn Header="Recipient" Binding="{Binding RecipientName}" Width="150"/>
                <controls:DataGridTextColumn Header="Quantity" Binding="{Binding Quantity}" Width="80"/>
                <controls:DataGridTextColumn Header="Date" Binding="{Binding CreatedDate}" Width="120"/>
                <controls:DataGridTextColumn Header="Creator" Binding="{Binding EmployeeNumber}" Width="80"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="12">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" Height="20"/>
            <TextBlock 
                Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                VerticalAlignment="Center"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Routing/Views/RoutingEditModeView.xaml.cs
```csharp
public sealed partial class RoutingEditModeView : Page
⋮----
private async void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.InitializeAsync();
⋮----
private async void OnDataGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
⋮----
await EditSelectedLabelCommand.ExecuteAsync(null);
⋮----
private async Task EditSelectedLabelCommand_ExecuteAsync(object? parameter)
⋮----
var editedLabel = new Model_RoutingLabel
⋮----
var dialog = new ContentDialog
⋮----
var grid = new Grid
⋮----
Padding = new Thickness(24)
⋮----
grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
⋮----
grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
var poLabel = new TextBlock { Text = "PO Number:", VerticalAlignment = VerticalAlignment.Center };
var poValue = new TextBox { Text = editedLabel.PONumber ?? "N/A", IsReadOnly = true };
Grid.SetRow(poLabel, 0);
Grid.SetColumn(poLabel, 0);
Grid.SetRow(poValue, 0);
Grid.SetColumn(poValue, 1);
grid.Children.Add(poLabel);
grid.Children.Add(poValue);
var partLabel = new TextBlock { Text = "Part Description:", VerticalAlignment = VerticalAlignment.Center };
var partValue = new TextBox { Text = editedLabel.Description ?? "N/A", IsReadOnly = true };
Grid.SetRow(partLabel, 1);
Grid.SetColumn(partLabel, 0);
Grid.SetRow(partValue, 1);
Grid.SetColumn(partValue, 1);
grid.Children.Add(partLabel);
grid.Children.Add(partValue);
var recipientLabel = new TextBlock { Text = "Recipient:", VerticalAlignment = VerticalAlignment.Center };
var recipientCombo = new ComboBox
⋮----
Grid.SetRow(recipientLabel, 2);
Grid.SetColumn(recipientLabel, 0);
Grid.SetRow(recipientCombo, 2);
Grid.SetColumn(recipientCombo, 1);
grid.Children.Add(recipientLabel);
grid.Children.Add(recipientCombo);
var qtyLabel = new TextBlock { Text = "Quantity:", VerticalAlignment = VerticalAlignment.Center };
var qtyBox = new NumberBox
⋮----
Grid.SetRow(qtyLabel, 3);
Grid.SetColumn(qtyLabel, 0);
Grid.SetRow(qtyBox, 3);
Grid.SetColumn(qtyBox, 1);
grid.Children.Add(qtyLabel);
grid.Children.Add(qtyBox);
var dateLabel = new TextBlock { Text = "Created Date:", VerticalAlignment = VerticalAlignment.Center };
var dateValue = new TextBox { Text = editedLabel.CreatedDate.ToString("g"), IsReadOnly = true };
Grid.SetRow(dateLabel, 4);
Grid.SetColumn(dateLabel, 0);
Grid.SetRow(dateValue, 4);
Grid.SetColumn(dateValue, 1);
grid.Children.Add(dateLabel);
grid.Children.Add(dateValue);
⋮----
var result = await dialog.ShowAsync();
⋮----
await ViewModel.SaveEditedLabelCommand.ExecuteAsync(editedLabel);
```

## File: Module_Routing/Views/RoutingManualEntryView.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<Page
    x:Class="MTM_Receiving_Application.Module_Routing.Views.RoutingManualEntryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    mc:Ignorable="d"
    Loaded="OnPageLoaded">

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="8">
            <TextBlock Text="Manual Entry Mode" Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock Text="Enter multiple labels and save in batch" Style="{StaticResource BodyTextBlockStyle}"/>
        </StackPanel>

        <!-- DataGrid -->
        <controls:DataGrid 
            Grid.Row="1"
            ItemsSource="{x:Bind ViewModel.Labels, Mode=OneWay}"
            SelectedItem="{x:Bind ViewModel.SelectedLabel, Mode=TwoWay}"
            AutoGenerateColumns="False"
            CanUserReorderColumns="False"
            CanUserSortColumns="False"
            GridLinesVisibility="All"
            HeadersVisibility="All"
            SelectionMode="Single"
            AlternatingRowBackground="#F5F5F5"
            CellEditEnding="OnCellEditEnding">
            
            <controls:DataGrid.Columns>
                <!-- PO Number -->
                <controls:DataGridTextColumn 
                    Header="PO Number" 
                    Binding="{Binding PONumber, Mode=TwoWay}"
                    Width="150"/>

                <!-- Line -->
                <controls:DataGridTextColumn 
                    Header="Line" 
                    Binding="{Binding POLine, Mode=TwoWay}"
                    Width="80"/>

                <!-- Recipient - Using Template Column since WinUI DataGridComboBoxColumn has different API -->
                <controls:DataGridTemplateColumn 
                    Header="Recipient"
                    Width="200">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <TextBlock 
                                Text="{Binding RecipientName}" 
                                VerticalAlignment="Center"
                                Padding="8,0"/>
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                    <controls:DataGridTemplateColumn.CellEditingTemplate>
                        <DataTemplate>
                            <ComboBox 
                                SelectedValue="{Binding RecipientID, Mode=TwoWay}"
                                ItemsSource="{Binding DataContext.AllRecipients, RelativeSource={RelativeSource Mode=TemplatedParent}}"
                                DisplayMemberPath="Name"
                                SelectedValuePath="Id"
                                HorizontalAlignment="Stretch"/>
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellEditingTemplate>
                </controls:DataGridTemplateColumn>

                <!-- Quantity -->
                <controls:DataGridTextColumn 
                    Header="Quantity" 
                    Binding="{Binding Quantity, Mode=TwoWay}"
                    Width="100"/>

                <!-- Part ID (Read-Only, auto-populated) -->
                <controls:DataGridTextColumn 
                    Header="Part ID" 
                    Binding="{Binding PartID, Mode=OneWay}"
                    IsReadOnly="True"
                    Width="*"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Action Buttons -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" Spacing="12">
            <Button 
                Content="Add Row" 
                Command="{x:Bind ViewModel.AddNewRowCommand}"
                Style="{StaticResource AccentButtonStyle}"/>
            
            <Button 
                Content="Delete Row" 
                Command="{x:Bind ViewModel.DeleteSelectedRowCommand}"/>
            
            <Button 
                Content="Save All Labels" 
                Command="{x:Bind ViewModel.SaveAllLabelsCommand}"
                Style="{StaticResource AccentButtonStyle}"/>
        </StackPanel>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="12">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"/>
            
            <TextBlock 
                Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                VerticalAlignment="Center"/>
            
            <TextBlock 
                Text="{x:Bind ViewModel.LabelCount, Mode=OneWay}"
                VerticalAlignment="Center"
                Foreground="{ThemeResource SystemAccentColor}"/>
            
            <TextBlock 
                Text="labels ready to save"
                VerticalAlignment="Center"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Routing/Views/RoutingManualEntryView.xaml.cs
```csharp
public sealed partial class RoutingManualEntryView : Page
⋮----
private async void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.InitializeAsync();
⋮----
private async void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
⋮----
if (e.Column.Header.ToString() == "PO Number" && !e.Cancel)
⋮----
_ = DispatcherQueue.TryEnqueue(async () =>
⋮----
await ViewModel.ValidatePOAsync(label);
```

## File: Module_Routing/Views/RoutingModeSelectionView.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<Page
    x:Class="MTM_Receiving_Application.Module_Routing.Views.RoutingModeSelectionView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Routing.ViewModels"
    Loaded="OnPageLoaded">

    <Grid>
        <StackPanel Spacing="32" HorizontalAlignment="Center" VerticalAlignment="Center">
            <StackPanel Orientation="Horizontal" Spacing="24">
                
                <!-- Wizard Mode Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectWizardModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Wizard Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE82D;" FontSize="48" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Wizard Mode" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Step-by-step guided workflow with Quick Add buttons."
                                       Style="{StaticResource BodyTextBlockStyle}"
                                       TextWrapping="Wrap"
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>

                    <CheckBox 
                        Content="Set as default mode"
                        IsChecked="{x:Bind ViewModel.IsWizardDefault, Mode=TwoWay}"
                        Command="{x:Bind ViewModel.SetWizardAsDefaultCommand}"
                        CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                        HorizontalAlignment="Center"/>
                </StackPanel>

                <!-- Manual Entry Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectManualEntryModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Manual Entry Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE745;" FontSize="48" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Manual Entry" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Batch entry using editable grid. Ideal for creating multiple labels quickly."
                                       Style="{StaticResource BodyTextBlockStyle}"
                                       TextWrapping="Wrap"
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsManualDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetManualAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"/>
                </StackPanel>

                <!-- Edit Mode Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectEditModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Edit Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE70F;" FontSize="48" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Edit Mode" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Search and edit existing labels. Supports reprinting and audit trail."
                                       Style="{StaticResource BodyTextBlockStyle}"
                                       TextWrapping="Wrap"
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>

                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsEditDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetEditAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"/>
                </StackPanel>
            </StackPanel>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Routing/Views/RoutingModeSelectionView.xaml.cs
```csharp
public sealed partial class RoutingModeSelectionView : Page
⋮----
private async void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
ViewModel.SetNavigationFrame(this.Frame);
⋮----
await ViewModel.InitializeAsync();
```

## File: Module_Routing/Views/RoutingWizardContainerView.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Routing.Views.RoutingWizardContainerView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MTM_Receiving_Application.Module_Routing.ViewModels"
    xmlns:views="using:MTM_Receiving_Application.Module_Routing.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Routing.Converters"
    xmlns:coreConverters="using:MTM_Receiving_Application.Module_Core.Converters">

    <Page.Resources>
        <converters:IntToFontWeightConverter x:Key="IntToFontWeightConverter"/>
        <converters:IntToProgressBrushConverter x:Key="IntToProgressBrushConverter"/>
        <coreConverters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
    </Page.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header with Cancel Button -->
        <Grid Grid.Row="0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <TextBlock Grid.Column="0"
                       Text="Routing Label - Wizard Mode" 
                       Style="{StaticResource TitleTextBlockStyle}"/>

            <Button Grid.Column="1"
                    Content="Cancel"
                    Command="{x:Bind ViewModel.CancelCommand}"/>
        </Grid>

        <!-- Progress Indicator -->
        <StackPanel Grid.Row="1" Spacing="8">
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" 
                       Style="{StaticResource SubtitleTextBlockStyle}"/>
            
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <!-- Step 1 Indicator -->
                <StackPanel Grid.Column="0" Spacing="4">
                    <Border Height="4" 
                            Background="{ThemeResource AccentFillColorDefaultBrush}"
                            CornerRadius="2"/>
                    <TextBlock Text="1. PO Selection" 
                               HorizontalAlignment="Center"
                               FontWeight="{x:Bind ViewModel.CurrentStep, Mode=OneWay, Converter={StaticResource IntToFontWeightConverter}, ConverterParameter=1}"/>
                </StackPanel>

                <!-- Step 2 Indicator -->
                <StackPanel Grid.Column="1" Spacing="4" Margin="8,0">
                    <Border Height="4" 
                            Background="{x:Bind ViewModel.CurrentStep, Mode=OneWay, Converter={StaticResource IntToProgressBrushConverter}, ConverterParameter=2}"
                            CornerRadius="2"/>
                    <TextBlock Text="2. Recipient" 
                               HorizontalAlignment="Center"
                               FontWeight="{x:Bind ViewModel.CurrentStep, Mode=OneWay, Converter={StaticResource IntToFontWeightConverter}, ConverterParameter=2}"/>
                </StackPanel>

                <!-- Step 3 Indicator -->
                <StackPanel Grid.Column="2" Spacing="4">
                    <Border Height="4" 
                            Background="{x:Bind ViewModel.CurrentStep, Mode=OneWay, Converter={StaticResource IntToProgressBrushConverter}, ConverterParameter=3}"
                            CornerRadius="2"/>
                    <TextBlock Text="3. Review" 
                               HorizontalAlignment="Center"
                               FontWeight="{x:Bind ViewModel.CurrentStep, Mode=OneWay, Converter={StaticResource IntToFontWeightConverter}, ConverterParameter=3}"/>
                </StackPanel>
            </Grid>
        </StackPanel>

        <!-- Step Content (UserControl hosting) -->
        <Grid Grid.Row="2">
            <!-- Step 1: PO Selection -->
            <views:RoutingWizardStep1View 
                Visibility="{x:Bind ViewModel.CurrentStep, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=1}"/>

            <!-- Step 2: Recipient Selection -->
            <views:RoutingWizardStep2View 
                Visibility="{x:Bind ViewModel.CurrentStep, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=2}"/>

            <!-- Step 3: Review -->
            <views:RoutingWizardStep3View 
                Visibility="{x:Bind ViewModel.CurrentStep, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=3}"/>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Routing/Views/RoutingWizardContainerView.xaml.cs
```csharp
public sealed partial class RoutingWizardContainerView : Page
```

## File: Module_Routing/Views/RoutingWizardStep1View.xaml.cs
```csharp
public sealed partial class RoutingWizardStep1View : Page
```

## File: Module_Routing/Views/RoutingWizardStep2View.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Routing.Views.RoutingWizardStep2View"
    x:Name="RootPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MTM_Receiving_Application.Module_Routing.ViewModels"
    xmlns:models="using:MTM_Receiving_Application.Module_Routing.Models"
    xmlns:coreConverters="using:MTM_Receiving_Application.Module_Core.Converters"
    Loaded="OnPageLoaded">

    <Page.Resources>
        <coreConverters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <coreConverters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
    </Page.Resources>

    <Grid Padding="20" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock Grid.Row="0" 
                   Text="Step 2: Recipient Selection" 
                   Style="{StaticResource TitleTextBlockStyle}"/>

        <!-- Search Box -->
        <StackPanel Grid.Row="1" Spacing="8">
            <StackPanel Orientation="Horizontal" Spacing="8">
                <FontIcon Glyph="&#xE721;" 
                         FontSize="16"
                         Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                <TextBlock Text="Search Recipients" 
                          Style="{ThemeResource BodyStrongTextBlockStyle}"/>
            </StackPanel>
            <TextBox PlaceholderText="Search by name, location, or department"
                     Text="{x:Bind ViewModel.SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
        </StackPanel>

        <!-- Recipients List -->
        <Border Grid.Row="2" 
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8">
            <ListView ItemsSource="{x:Bind ViewModel.FilteredRecipients, Mode=OneWay}"
                      SelectedItem="{x:Bind ViewModel.SelectedRecipient, Mode=TwoWay}"
                      SelectionMode="Single"
                      KeyDown="ListView_KeyDown">
                <ListView.HeaderTemplate>
                    <DataTemplate>
                        <Grid Padding="8" ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="2*"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" Text="Name" FontWeight="SemiBold"/>
                            <TextBlock Grid.Column="1" Text="Location" FontWeight="SemiBold"/>
                            <TextBlock Grid.Column="2" Text="Department" FontWeight="SemiBold"/>
                        </Grid>
                    </DataTemplate>
                </ListView.HeaderTemplate>
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:Model_RoutingRecipient">
                        <Grid Padding="8" ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="2*"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Grid.Column="0" Text="{x:Bind Name}"/>
                            <TextBlock Grid.Column="1" Text="{x:Bind Location}"/>
                            <TextBlock Grid.Column="2" Text="{x:Bind Department}"/>
                        </Grid>
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>
        </Border>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="12">
            <ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                          Width="20" Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                       VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Action Buttons -->
        <StackPanel Grid.Row="4" Orientation="Horizontal" Spacing="12" HorizontalAlignment="Right">
            <Button Content="Back"
                    Command="{x:Bind ViewModel.NavigateBackToStep1Command}"
                    Visibility="{x:Bind ViewModel.IsEditingFromReview, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}"/>
            <Button Content="{x:Bind ViewModel.NavigationButtonText, Mode=OneWay}"
                    Command="{x:Bind ViewModel.ProceedToStep3Command}"
                    Style="{StaticResource AccentButtonStyle}"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Routing/Views/RoutingWizardStep2View.xaml.cs
```csharp
public sealed partial class RoutingWizardStep2View : Page
⋮----
private async void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.LoadRecipientsCommand.ExecuteAsync(null);
⋮----
private void ListView_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
ViewModel.ProceedToStep3Command.Execute(null);
```

## File: Module_Routing/Views/RoutingWizardStep3View.xaml.cs
```csharp
public sealed partial class RoutingWizardStep3View : Page
⋮----
private void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
ViewModel.LoadReviewData();
```

## File: Module_Routing/Converters/IntToFontWeightConverter.cs
```csharp
public class IntToFontWeightConverter : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Routing/Converters/IntToProgressBrushConverter.cs
```csharp
public class IntToProgressBrushConverter : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Routing/Converters/NullToBooleanConverter.cs
```csharp
public class NullToBooleanConverter : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Routing/Data/Dao_RoutingOtherReason.cs
```csharp
public class Dao_RoutingOtherReason
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingOtherReason>>> GetAllActiveReasonsAsync()
⋮----
private Model_RoutingOtherReason MapFromReader(IDataReader reader)
⋮----
return new Model_RoutingOtherReason
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
ReasonCode = reader.GetString(reader.GetOrdinal("reason_code")),
Description = reader.GetString(reader.GetOrdinal("description")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order"))
```

## File: Module_Routing/Data/Dao_RoutingUsageTracking.cs
```csharp
public class Dao_RoutingUsageTracking
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result> IncrementUsageAsync(int employeeNumber, int recipientId)
⋮----
new MySqlParameter("@p_employee_number", employeeNumber),
new MySqlParameter("@p_recipient_id", recipientId),
new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
⋮----
return await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Failure($"Error incrementing usage: {ex.Message}", ex);
```

## File: Module_Routing/Data/Dao_RoutingUserPreference.cs
```csharp
public class Dao_RoutingUserPreference
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<Model_RoutingUserPreference>> GetUserPreferenceAsync(int employeeNumber)
⋮----
public async Task<Model_Dao_Result> SaveUserPreferenceAsync(Model_RoutingUserPreference preference)
⋮----
new MySqlParameter("@p_employee_number", preference.EmployeeNumber),
new MySqlParameter("@p_default_mode", preference.DefaultMode),
new MySqlParameter("@p_enable_validation", preference.EnableValidation),
new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
⋮----
return await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Failure($"Error saving user preference: {ex.Message}", ex);
⋮----
private Model_RoutingUserPreference MapFromReader(IDataReader reader)
⋮----
return new Model_RoutingUserPreference
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
EmployeeNumber = reader.GetInt32(reader.GetOrdinal("employee_number")),
DefaultMode = reader.GetString(reader.GetOrdinal("default_mode")),
EnableValidation = reader.GetBoolean(reader.GetOrdinal("enable_validation")),
UpdatedDate = reader.GetDateTime(reader.GetOrdinal("updated_date"))
```

## File: Module_Routing/Models/Model_RoutingLabelHistory.cs
```csharp
public class Model_RoutingLabelHistory
```

## File: Module_Routing/Models/Model_RoutingOtherReason.cs
```csharp
public class Model_RoutingOtherReason
```

## File: Module_Routing/Models/Model_RoutingRecipient.cs
```csharp
public class Model_RoutingRecipient
```

## File: Module_Routing/Models/Model_RoutingUsageTracking.cs
```csharp
public class Model_RoutingUsageTracking
```

## File: Module_Routing/Models/Model_RoutingUserPreference.cs
```csharp
public class Model_RoutingUserPreference
```

## File: Module_Routing/Services/RoutingInforVisualService.cs
```csharp
public class RoutingInforVisualService : IRoutingInforVisualService
⋮----
private readonly Dao_InforVisualPO _daoInforVisualPO;
private readonly IService_LoggingUtility _logger;
⋮----
_daoInforVisualPO = daoInforVisualPO ?? throw new ArgumentNullException(nameof(daoInforVisualPO));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
⋮----
await _logger.LogInfoAsync($"Validating PO number: {poNumber}");
⋮----
await _logger.LogInfoAsync($"[MOCK DATA MODE] Validating PO {poNumber}");
return Model_Dao_Result_Factory.Success(true);
⋮----
var connectionResult = await _daoInforVisualPO.CheckConnectionAsync();
⋮----
await _logger.LogWarningAsync($"Infor Visual unavailable: {connectionResult.ErrorMessage}");
⋮----
return await _daoInforVisualPO.ValidatePOAsync(poNumber);
⋮----
await _logger.LogErrorAsync($"Error validating PO {poNumber}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPoLinesAsync(string poNumber)
⋮----
await _logger.LogInfoAsync($"Getting PO lines for: {poNumber}");
⋮----
await _logger.LogInfoAsync($"[MOCK DATA MODE] Returning mock lines for PO {poNumber}");
⋮----
new Model_InforVisualPOLine
⋮----
return Model_Dao_Result_Factory.Success(mockLines);
⋮----
return await _daoInforVisualPO.GetLinesAsync(poNumber);
⋮----
await _logger.LogErrorAsync($"Error getting PO lines for {poNumber}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<Model_InforVisualPOLine>> GetPoLineAsync(string poNumber, string lineNumber)
⋮----
await _logger.LogInfoAsync($"Getting PO line {poNumber}-{lineNumber}");
⋮----
await _logger.LogInfoAsync($"[MOCK DATA MODE] Returning mock data for line {lineNumber}");
var mockLine = new Model_InforVisualPOLine
⋮----
PartID = $"MOCK-PART-{lineNumber.PadLeft(3, '0')}",
⋮----
return Model_Dao_Result_Factory.Success(mockLine);
⋮----
if (!int.TryParse(lineNumber, out int lineNum))
⋮----
return await _daoInforVisualPO.GetLineAsync(poNumber, lineNum);
⋮----
await _logger.LogErrorAsync($"Error getting PO line {poNumber}-{lineNumber}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<bool>> CheckConnectionAsync()
⋮----
return await _daoInforVisualPO.CheckConnectionAsync();
⋮----
await _logger.LogErrorAsync($"Error checking Infor Visual connection: {ex.Message}", ex);
return Model_Dao_Result_Factory.Success(false);
```

## File: Module_Routing/Services/RoutingUsageTrackingService.cs
```csharp
public class RoutingUsageTrackingService : IRoutingUsageTrackingService
⋮----
private readonly Dao_RoutingUsageTracking _daoUsageTracking;
private readonly IService_LoggingUtility _logger;
⋮----
_daoUsageTracking = daoUsageTracking ?? throw new ArgumentNullException(nameof(daoUsageTracking));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result> IncrementUsageCountAsync(int employeeNumber, int recipientId)
⋮----
await _logger.LogInfoAsync($"Incrementing usage count for employee {employeeNumber}, recipient {recipientId}");
return await _daoUsageTracking.IncrementUsageAsync(employeeNumber, recipientId);
⋮----
await _logger.LogErrorAsync($"Error incrementing usage count: {ex.Message}", ex);
return new Model_Dao_Result { Success = false, ErrorMessage = $"Error updating usage tracking: {ex.Message}", Exception = ex };
⋮----
public async Task<Model_Dao_Result<int>> GetUsageCountAsync(int employeeNumber, int recipientId)
⋮----
await _logger.LogInfoAsync($"Getting usage count for employee {employeeNumber}, recipient {recipientId}");
⋮----
await _logger.LogErrorAsync($"Error getting usage count: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<int>> GetEmployeeLabelCountAsync(int employeeNumber)
⋮----
await _logger.LogInfoAsync($"Getting label count for employee {employeeNumber}");
⋮----
await _logger.LogErrorAsync($"Error getting employee label count: {ex.Message}", ex);
```

## File: Module_Routing/ViewModels/RoutingWizardStep3ViewModel.cs
```csharp
public partial class RoutingWizardStep3ViewModel : ObservableObject
⋮----
private readonly IRoutingService _routingService;
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_LoggingUtility _logger;
private readonly RoutingWizardContainerViewModel _containerViewModel;
⋮----
_containerViewModel.RegisterStep3ViewModel(this);
_logger.LogInfo("Step3ViewModel initialized and registered");
⋮----
public void LoadReviewData()
⋮----
_logger.LogInfo("LoadReviewData called");
_logger.LogInfo($"Container state - SelectedPOLine: {_containerViewModel.SelectedPOLine?.PartID ?? "null"}, SelectedRecipient: {_containerViewModel.SelectedRecipient?.Name ?? "null"}, FinalQuantity: {_containerViewModel.FinalQuantity}");
// Load from container state
⋮----
// PO workflow
⋮----
PoLine = int.TryParse(_containerViewModel.SelectedPOLine.LineNumber, out var lineNum) ? lineNum : 0;
⋮----
_logger.LogInfo($"Loaded PO data - PO: {PoNumber}, Line: {PoLine}, Part: {PartID}");
⋮----
// OTHER workflow
⋮----
_logger.LogInfo($"Loaded OTHER data - Reason: {OtherReason}");
⋮----
_logger.LogWarning("LoadReviewData: No PO line or OTHER reason in container!");
⋮----
_logger.LogInfo($"Loaded recipient - Name: {RecipientName}, Location: {RecipientLocation}");
⋮----
_logger.LogWarning("LoadReviewData: No recipient in container!");
⋮----
_logger.LogInfo($"Final quantity: {Quantity}");
⋮----
private void EditPOSelection()
⋮----
_logger.LogInfo("EditPOSelection - Navigating to Step 1 in edit mode");
_containerViewModel.NavigateToStep1ForEditCommand.Execute(null);
⋮----
private void EditRecipientSelection()
⋮----
_logger.LogInfo("EditRecipientSelection - Navigating to Step 2 in edit mode");
_containerViewModel.NavigateToStep2ForEditCommand.Execute(null);
⋮----
private void NavigateBack()
⋮----
_logger.LogInfo("NavigateBack - Navigating back to Step 2");
_containerViewModel.NavigateBackToStep2Command.Execute(null);
⋮----
private async Task CreateLabelAsync()
⋮----
var duplicateCheckResult = await _routingService.CheckDuplicateLabelAsync(
⋮----
PoLine.ToString(),
⋮----
await _containerViewModel.CreateLabelCommand.ExecuteAsync(null);
⋮----
_errorHandler.HandleException(
⋮----
private async Task<bool> ShowDuplicateConfirmationAsync()
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
```

## File: Module_Routing/Views/RoutingWizardStep3View.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Routing.Views.RoutingWizardStep3View"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MTM_Receiving_Application.Module_Routing.ViewModels"
    xmlns:coreConverters="using:MTM_Receiving_Application.Module_Core.Converters"
    Loaded="OnPageLoaded">

    <Page.Resources>
        <coreConverters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
    </Page.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock Grid.Row="0" 
                   Text="Step 3: Review and Confirm" 
                   Style="{StaticResource TitleTextBlockStyle}"/>

        <!-- Review Cards -->
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
            <Grid ColumnSpacing="24" RowSpacing="24">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Source Card (PO & Part) -->
                <Border Grid.Column="0" Grid.Row="0"
                        Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20">
                    <StackPanel Spacing="16">
                        <Grid>
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE8B9;" FontSize="16" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                <TextBlock Text="Source" Style="{ThemeResource SubtitleTextBlockStyle}"/>
                            </StackPanel>
                            <Button HorizontalAlignment="Right"
                                    Command="{x:Bind ViewModel.EditPOSelectionCommand}"
                                    Style="{StaticResource SubtleButtonStyle}" 
                                    ToolTipService.ToolTip="Edit Source">
                                <FontIcon Glyph="&#xE70F;" FontSize="14"/>
                            </Button>
                        </Grid>
                        
                        <MenuFlyoutSeparator/>

                        <!-- PO Info -->
                        <Grid ColumnSpacing="12" RowSpacing="8">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>

                            <TextBlock Text="PO:" FontWeight="SemiBold" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <TextBlock Grid.Column="1" Text="{x:Bind ViewModel.PoNumber, Mode=OneWay}" FontWeight="SemiBold"/>

                            <TextBlock Grid.Row="1" Text="Line:" FontWeight="SemiBold" Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                       Visibility="{x:Bind ViewModel.IsOtherWorkflow, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}"/>
                            <TextBlock Grid.Row="1" Grid.Column="1" Text="{x:Bind ViewModel.PoLine, Mode=OneWay}"
                                       Visibility="{x:Bind ViewModel.IsOtherWorkflow, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}"/>
                                       
                            <TextBlock Grid.Row="1" Text="Reason:" FontWeight="SemiBold" Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                       Visibility="{x:Bind ViewModel.IsOtherWorkflow, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                            <TextBlock Grid.Row="1" Grid.Column="1" Text="{x:Bind ViewModel.OtherReason, Mode=OneWay}"
                                       Visibility="{x:Bind ViewModel.IsOtherWorkflow, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                        </Grid>
                        
                        <!-- Part Info -->
                        <Grid Background="{ThemeResource LayerFillColorDefaultBrush}" CornerRadius="4" Padding="12">
                             <StackPanel Spacing="4">
                                <TextBlock Text="{x:Bind ViewModel.PartID, Mode=OneWay}" Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                <TextBlock Text="{x:Bind ViewModel.PartDescription, Mode=OneWay}" TextWrapping="Wrap" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                <StackPanel Orientation="Horizontal" Spacing="4" Margin="0,8,0,0">
                                    <TextBlock Text="Qty:" FontWeight="SemiBold"/>
                                    <TextBlock Text="{x:Bind ViewModel.Quantity, Mode=OneWay}"/>
                                </StackPanel>
                             </StackPanel>
                        </Grid>
                    </StackPanel>
                </Border>

                <!-- Destination Card (Recipient) -->
                <Border Grid.Column="1" Grid.Row="0"
                        Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20">
                     <StackPanel Spacing="16">
                        <Grid>
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE77B;" FontSize="16" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                <TextBlock Text="Destination" Style="{ThemeResource SubtitleTextBlockStyle}"/>
                            </StackPanel>
                            <Button HorizontalAlignment="Right"
                                    Command="{x:Bind ViewModel.EditRecipientSelectionCommand}"
                                    Style="{StaticResource SubtleButtonStyle}"
                                    ToolTipService.ToolTip="Edit Destination">
                                <FontIcon Glyph="&#xE70F;" FontSize="14"/>
                            </Button>
                        </Grid>

                        <MenuFlyoutSeparator/>

                        <StackPanel Spacing="12">
                            <StackPanel Spacing="4">
                                <TextBlock Text="Recipient" Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12"/>
                                <TextBlock Text="{x:Bind ViewModel.RecipientName, Mode=OneWay}" Style="{ThemeResource BodyStrongTextBlockStyle}" FontSize="16"/>
                            </StackPanel>
                            
                            <StackPanel Spacing="4">
                                <TextBlock Text="Location / Department" Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12"/>
                                <TextBlock Text="{x:Bind ViewModel.RecipientLocation, Mode=OneWay}"/>
                            </StackPanel>
                        </StackPanel>
                        
                        <!-- Placeholder for Label Preview -->
                        <Border Height="60" Background="{ThemeResource LayerFillColorDefaultBrush}" CornerRadius="4" VerticalAlignment="Bottom" Margin="0,16,0,0">
                            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center" Spacing="8">
                                <FontIcon Glyph="&#xE743;" FontSize="16" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                                <TextBlock Text="Ready to Print" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                            </StackPanel>
                        </Border>
                    </StackPanel>
                </Border>
            </Grid>
        </ScrollViewer>

        <!-- Status Bar -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" Spacing="12">
            <ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                          Width="20" Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                       VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Action Buttons -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="12" HorizontalAlignment="Right">
            <Button Content="Back"
                    Command="{x:Bind ViewModel.NavigateBackCommand}"/>
            <Button Content="Create Label"
                    Command="{x:Bind ViewModel.CreateLabelCommand}"
                    Style="{StaticResource AccentButtonStyle}"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Routing/Data/Dao_RoutingLabel.cs
```csharp
public class Dao_RoutingLabel
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<int>> InsertLabelAsync(Model_RoutingLabel label)
⋮----
Console.WriteLine($"[DAO] Inserting label: PO={label.PONumber}, Recipient={label.RecipientId}");
⋮----
new MySqlParameter("@p_po_number", label.PONumber),
new MySqlParameter("@p_line_number", label.LineNumber),
new MySqlParameter("@p_description", label.Description),
new MySqlParameter("@p_recipient_id", label.RecipientId),
new MySqlParameter("@p_quantity", label.Quantity),
new MySqlParameter("@p_created_by", label.CreatedBy),
new MySqlParameter("@p_other_reason_id", (object?)label.OtherReasonId ?? DBNull.Value),
new MySqlParameter("@p_new_label_id", MySqlDbType.Int32) { Direction = ParameterDirection.Output },
new MySqlParameter("@p_error_message", MySqlDbType.VarChar, 500) { Direction = ParameterDirection.Output }
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
var newIdParam = Array.Find(parameters, p => p.ParameterName == "@p_new_label_id");
int newId = newIdParam?.Value != DBNull.Value ? Convert.ToInt32(newIdParam?.Value) : 0;
var errorMsgParam = Array.Find(parameters, p => p.ParameterName == "@p_error_message");
⋮----
if (!string.IsNullOrEmpty(spErrorMsg))
⋮----
public async Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label)
⋮----
Console.WriteLine($"[DAO] Updating label {label.Id}");
⋮----
new MySqlParameter("@p_label_id", label.Id),
⋮----
new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = ParameterDirection.Output },
new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = ParameterDirection.Output }
⋮----
return await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating routing label: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId)
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0)
⋮----
public async Task<Model_Dao_Result> DeleteLabelAsync(int labelId)
⋮----
new MySqlParameter("@p_label_id", labelId),
⋮----
return Model_Dao_Result_Factory.Failure($"Error deleting routing label: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> MarkLabelExportedAsync(int labelId)
⋮----
return Model_Dao_Result_Factory.Failure($"Error marking label as exported: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<Model_RoutingLabel>> CheckDuplicateLabelAsync(
⋮----
private Model_RoutingLabel MapFromReader(IDataReader reader)
⋮----
return new Model_RoutingLabel
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
PONumber = reader.IsDBNull(reader.GetOrdinal("po_number")) ? string.Empty : reader.GetString(reader.GetOrdinal("po_number")),
LineNumber = reader.IsDBNull(reader.GetOrdinal("line_number")) ? string.Empty : reader.GetString(reader.GetOrdinal("line_number")),
Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString(reader.GetOrdinal("description")),
RecipientId = reader.GetInt32(reader.GetOrdinal("recipient_id")),
RecipientName = reader.IsDBNull(reader.GetOrdinal("recipient_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("recipient_name")),
RecipientLocation = reader.IsDBNull(reader.GetOrdinal("recipient_location")) ? string.Empty : reader.GetString(reader.GetOrdinal("recipient_location")),
Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
CreatedBy = reader.GetInt32(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
OtherReasonId = reader.IsDBNull(reader.GetOrdinal("other_reason_id")) ? null : reader.GetInt32(reader.GetOrdinal("other_reason_id")),
OtherReasonDescription = reader.IsDBNull(reader.GetOrdinal("other_reason_description")) ? null : reader.GetString(reader.GetOrdinal("other_reason_description")),
⋮----
CsvExported = reader.GetBoolean(reader.GetOrdinal("csv_exported")),
CsvExportDate = reader.IsDBNull(reader.GetOrdinal("csv_export_date")) ? null : reader.GetDateTime(reader.GetOrdinal("csv_export_date"))
⋮----
public async Task<Model_Dao_Result<(bool Exists, int? ExistingLabelId)>> CheckDuplicateAsync(string poNumber, string lineNumber, int recipientId, DateTime createdWithinDate)
⋮----
return Model_Dao_Result_Factory.Success((Exists: true, ExistingLabelId: (int?)result.Data.Id));
⋮----
return Model_Dao_Result_Factory.Success((Exists: false, ExistingLabelId: (int?)null));
⋮----
public async Task<Model_Dao_Result> MarkExportedAsync(List<int> labelIds)
⋮----
return Model_Dao_Result_Factory.Success();
```

## File: Module_Routing/Data/Dao_RoutingLabelHistory.cs
```csharp
public class Dao_RoutingLabelHistory
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result> InsertHistoryAsync(Model_RoutingLabelHistory history)
⋮----
new MySqlParameter("@p_label_id", history.LabelId),
new MySqlParameter("@p_field_changed", history.FieldChanged),
new MySqlParameter("@p_old_value", (object?)history.OldValue ?? DBNull.Value),
new MySqlParameter("@p_new_value", (object?)history.NewValue ?? DBNull.Value),
new MySqlParameter("@p_edited_by", history.EditedBy),
new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
⋮----
return await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting history: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> InsertHistoryBatchAsync(List<Model_RoutingLabelHistory> historyEntries)
⋮----
return Model_Dao_Result_Factory.Success("No history entries to insert", 0);
⋮----
return Model_Dao_Result_Factory.Failure($"Batch insert failed at entry {successCount + 1}: {result.ErrorMessage}");
⋮----
return Model_Dao_Result_Factory.Success($"Inserted {successCount} history entries", successCount);
⋮----
return Model_Dao_Result_Factory.Failure($"Error in batch insert: {ex.Message}", ex);
```

## File: Module_Routing/Models/Model_InforVisualPOLine.cs
```csharp
public class Model_InforVisualPOLine
⋮----
public string QuantityOrderedDisplay => QuantityOrdered.ToString("G29");
⋮----
? Specifications.Substring(0, 50) + "..."
⋮----
if (!string.IsNullOrEmpty(WorkOrder)) return $"WO: {WorkOrder}";
if (!string.IsNullOrEmpty(CustomerOrder)) return $"CO: {CustomerOrder}";
```

## File: Module_Routing/Models/Model_RoutingLabel.cs
```csharp
public class Model_RoutingLabel
```

## File: Module_Routing/Services/RoutingService.cs
```csharp
public class RoutingService : IRoutingService
⋮----
private readonly Dao_RoutingLabel _daoLabel;
private readonly Dao_RoutingLabelHistory _daoHistory;
private readonly IRoutingInforVisualService _inforVisualService;
private readonly IRoutingUsageTrackingService _usageTrackingService;
private readonly IRoutingRecipientService _recipientService;
private readonly IService_LoggingUtility _logger;
private readonly IConfiguration _configuration;
private static readonly SemaphoreSlim _csvFileLock = new SemaphoreSlim(1, 1);
⋮----
_daoLabel = daoLabel ?? throw new ArgumentNullException(nameof(daoLabel));
_daoHistory = daoHistory ?? throw new ArgumentNullException(nameof(daoHistory));
_inforVisualService = inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
_usageTrackingService = usageTrackingService ?? throw new ArgumentNullException(nameof(usageTrackingService));
_recipientService = recipientService ?? throw new ArgumentNullException(nameof(recipientService));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
⋮----
public async Task<Model_Dao_Result<int>> CreateLabelAsync(Model_RoutingLabel label)
⋮----
await _logger.LogInfoAsync($"Creating routing label for PO {label.PONumber}, Recipient {label.RecipientId}");
⋮----
var insertResult = await _daoLabel.InsertLabelAsync(label);
⋮----
await _logger.LogErrorAsync($"Error creating label: {ex.Message}", ex);
⋮----
private async Task<Model_Dao_Result> ValidateAndCheckDuplicatesAsync(Model_RoutingLabel label)
⋮----
var recipientValidation = await _recipientService.ValidateRecipientExistsAsync(label.RecipientId);
⋮----
return Model_Dao_Result_Factory.Failure($"Invalid recipient ID: {label.RecipientId}");
⋮----
return Model_Dao_Result_Factory.Failure(
⋮----
return Model_Dao_Result_Factory.Success();
⋮----
private void ExecuteBackgroundTasks(Model_RoutingLabel label, int labelId)
⋮----
_ = Task.Run(async () =>
⋮----
await _logger.LogWarningAsync($"CSV export failed for label {labelId}: {csvResult.ErrorMessage}");
⋮----
var usageResult = await _usageTrackingService.IncrementUsageCountAsync(
⋮----
await _logger.LogWarningAsync($"Usage tracking failed for label {labelId}: {usageResult.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Background task error for label {labelId}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label, int editedByEmployeeNumber)
⋮----
await _logger.LogInfoAsync($"Updating label {label.Id} by employee {editedByEmployeeNumber}");
var originalResult = await _daoLabel.GetLabelByIdAsync(label.Id);
⋮----
return Model_Dao_Result_Factory.Failure($"Original label not found: {originalResult.ErrorMessage}");
⋮----
var updateResult = await _daoLabel.UpdateLabelAsync(label);
⋮----
historyEntries.Add(new Model_RoutingLabelHistory
⋮----
var historyResult = await _daoHistory.InsertHistoryBatchAsync(historyEntries);
⋮----
await _logger.LogWarningAsync($"Failed to log history: {historyResult.ErrorMessage}");
⋮----
return Model_Dao_Result_Factory.Success("Label updated successfully", 1);
⋮----
await _logger.LogErrorAsync($"Error updating label: {ex.Message}", ex);
return Model_Dao_Result_Factory.Failure($"Error updating label: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId)
⋮----
return await _daoLabel.GetLabelByIdAsync(labelId);
⋮----
await _logger.LogErrorAsync($"Error getting label {labelId}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0)
⋮----
return await _daoLabel.GetAllLabelsAsync(limit, offset);
⋮----
await _logger.LogErrorAsync($"Error retrieving labels: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<(bool Exists, int? ExistingLabelId)>> CheckDuplicateLabelAsync(
⋮----
var result = await _daoLabel.CheckDuplicateAsync(poNumber, lineNumber, recipientId, createdDate);
⋮----
await _logger.LogErrorAsync($"Error checking duplicate: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> ExportLabelToCsvAsync(Model_RoutingLabel label)
⋮----
var validatedNetworkPath = string.IsNullOrEmpty(networkPath) ? string.Empty : ValidateCsvPath(networkPath);
var validatedLocalPath = string.IsNullOrEmpty(localPath) ? string.Empty : ValidateCsvPath(localPath);
⋮----
await _logger.LogWarningAsync($"Network CSV export failed, using local path: {validatedLocalPath}");
⋮----
await _daoLabel.MarkExportedAsync(new List<int> { label.Id });
return Model_Dao_Result_Factory.Success(
⋮----
return Model_Dao_Result_Factory.Failure("CSV export failed (both network and local paths)");
⋮----
await _logger.LogErrorAsync($"Error exporting to CSV: {ex.Message}", ex);
return Model_Dao_Result_Factory.Failure($"CSV export error: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> RegenerateLabelCsvAsync(int labelId)
⋮----
return Model_Dao_Result_Factory.Failure($"Label not found: {labelResult.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Error regenerating CSV: {ex.Message}", ex);
return Model_Dao_Result_Factory.Failure($"Error regenerating CSV: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> ResetCsvFileAsync()
⋮----
if (File.Exists(networkPath))
⋮----
File.Delete(networkPath);
⋮----
if (File.Exists(localPath))
⋮----
File.Delete(localPath);
⋮----
await _logger.LogInfoAsync($"CSV files reset (network: {networkDeleted}, local: {localDeleted})");
return Model_Dao_Result_Factory.Success("CSV files reset successfully", 1);
⋮----
await _logger.LogErrorAsync($"Error resetting CSV: {ex.Message}", ex);
return Model_Dao_Result_Factory.Failure($"Error resetting CSV: {ex.Message}", ex);
⋮----
public Model_Dao_Result ValidateLabel(Model_RoutingLabel label)
⋮----
return Model_Dao_Result_Factory.Failure("Label cannot be null");
⋮----
if (string.IsNullOrWhiteSpace(label.PONumber))
⋮----
return Model_Dao_Result_Factory.Failure("PO Number is required");
⋮----
return Model_Dao_Result_Factory.Failure("PO Number cannot exceed 50 characters");
⋮----
if (string.IsNullOrWhiteSpace(label.LineNumber))
⋮----
return Model_Dao_Result_Factory.Failure("Line Number is required");
⋮----
return Model_Dao_Result_Factory.Failure("Line Number cannot exceed 20 characters");
⋮----
return Model_Dao_Result_Factory.Failure("Description cannot exceed 255 characters");
⋮----
return Model_Dao_Result_Factory.Failure("Quantity must be greater than zero");
⋮----
return Model_Dao_Result_Factory.Failure("Recipient must be selected");
⋮----
return Model_Dao_Result_Factory.Failure("Employee Number is required");
⋮----
if (label.PONumber.Equals("OTHER", StringComparison.OrdinalIgnoreCase))
⋮----
return Model_Dao_Result_Factory.Failure("OTHER reason must be selected when PO is OTHER");
⋮----
return Model_Dao_Result_Factory.Success("Validation passed", 1);
⋮----
private string ValidateCsvPath(string path)
⋮----
if (string.IsNullOrWhiteSpace(path))
⋮----
throw new ArgumentException("CSV path cannot be empty", nameof(path));
⋮----
var fullPath = Path.GetFullPath(path);
if (path.Contains("..") || path.Contains("~"))
⋮----
private string FormatCsvLine(Model_RoutingLabel label)
⋮----
private async Task<bool> TryWriteCsvAsync(string filePath, string csvLine, int retryCount, int retryDelayMs)
⋮----
await _csvFileLock.WaitAsync();
⋮----
var directory = Path.GetDirectoryName(filePath);
if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
⋮----
Directory.CreateDirectory(directory);
⋮----
await File.AppendAllTextAsync(filePath, csvLine + Environment.NewLine);
⋮----
_csvFileLock.Release();
⋮----
await _logger.LogWarningAsync($"CSV write attempt {attempt}/{retryCount} failed: {ex.Message}");
⋮----
await Task.Delay(retryDelayMs);
⋮----
await _logger.LogErrorAsync($"Unexpected CSV write error: {ex.Message}", ex);
⋮----
private List<(string FieldName, string OldValue, string NewValue)> GetLabelChanges(
⋮----
changes.Add(("PONumber", original.PONumber, updated.PONumber));
⋮----
changes.Add(("LineNumber", original.LineNumber, updated.LineNumber));
⋮----
changes.Add(("Description", original.Description ?? "", updated.Description ?? ""));
⋮----
changes.Add(("Quantity", original.Quantity.ToString(), updated.Quantity.ToString()));
⋮----
changes.Add(("RecipientId", original.RecipientId.ToString(), updated.RecipientId.ToString()));
⋮----
changes.Add(("OtherReasonId",
```

## File: Module_Routing/ViewModels/RoutingWizardContainerViewModel.cs
```csharp
public partial class RoutingWizardContainerViewModel : ObservableObject
⋮----
private readonly IRoutingService _routingService;
private readonly IRoutingInforVisualService _inforVisualService;
private readonly IRoutingUsageTrackingService _usageTrackingService;
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_LoggingUtility _logger;
private readonly IService_UserSessionManager _sessionManager;
⋮----
private void NavigateToStep2()
⋮----
_logger.LogInfo($"NavigateToStep2 called - CurrentStep: {CurrentStep}, SelectedPOLine: {SelectedPOLine?.PartID ?? "null"}, SelectedOtherReason: {SelectedOtherReason?.Description ?? "null"}");
// Validation: Must have either PO line or OTHER reason
⋮----
_logger.LogWarning("NavigateToStep2: No PO line or OTHER reason selected");
⋮----
_logger.LogInfo($"Set FinalQuantity from PO line: {FinalQuantity}");
⋮----
_logger.LogInfo($"Set FinalQuantity from OTHER: {FinalQuantity}");
⋮----
_logger.LogInfo($"Changing CurrentStep from {CurrentStep} to 2");
⋮----
_logger.LogInfo($"CurrentStep is now: {CurrentStep}");
⋮----
private void NavigateToStep3()
⋮----
_logger.LogInfo($"NavigateToStep3 called - SelectedRecipient: {SelectedRecipient?.Name ?? "null"}");
// Validation: Must have recipient
⋮----
_logger.LogWarning("NavigateToStep3: No recipient selected");
⋮----
_logger.LogInfo($"Navigating to Step 3 - Recipient: {SelectedRecipient.Name}, CurrentStep changing from {CurrentStep} to 3");
⋮----
_logger.LogInfo("Calling Step3ViewModel.LoadReviewData()");
_step3ViewModel.LoadReviewData();
⋮----
_logger.LogWarning("Step3ViewModel not yet initialized");
⋮----
public void RegisterStep3ViewModel(RoutingWizardStep3ViewModel step3ViewModel)
⋮----
_logger.LogInfo("Step3ViewModel registered with Container");
⋮----
private void NavigateToStep1()
⋮----
private void NavigateToStep1ForEdit()
⋮----
_logger.LogInfo("NavigateToStep1ForEdit called - Setting edit mode");
⋮----
private void NavigateBackToStep2()
⋮----
private void NavigateToStep2ForEdit()
⋮----
_logger.LogInfo("NavigateToStep2ForEdit called - Setting edit mode");
⋮----
private async Task CreateLabelAsync()
⋮----
var label = new Model_RoutingLabel
⋮----
LineNumber = SelectedPOLine?.LineNumber.ToString() ?? "0",
⋮----
var result = await _routingService.CreateLabelAsync(label);
⋮----
await _usageTrackingService.IncrementUsageCountAsync(
⋮----
await _logger.LogInfoAsync(
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private async Task CancelAsync()
⋮----
var result = await dialog.ShowAsync();
⋮----
private void ResetWizard()
⋮----
private int GetCurrentEmployeeNumber()
```

## File: Module_Routing/Views/RoutingWizardStep1View.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Routing.Views.RoutingWizardStep1View"
    x:Name="RootPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MTM_Receiving_Application.Module_Routing.ViewModels"
    xmlns:models="using:MTM_Receiving_Application.Module_Routing.Models"
    xmlns:coreConverters="using:MTM_Receiving_Application.Module_Core.Converters">

    <Page.Resources>
        <coreConverters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <coreConverters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
    </Page.Resources>

    <Grid Padding="12" RowSpacing="8">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock Grid.Row="0" 
                   Text="Step 1: PO and Line Selection" 
                   Style="{StaticResource TitleTextBlockStyle}"
                   Margin="0,0,0,4"/>

        <!-- PO Input Section (visible when NOT in OTHER mode) -->
        <Border Grid.Row="1" 
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="12"
                Visibility="{x:Bind ViewModel.IsOtherMode, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}">
            <Grid ColumnSpacing="16">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <!-- Label -->
                <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
                    <FontIcon Glyph="&#xE8A5;" 
                                FontSize="16"
                                Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                    <TextBlock Text="PO Number:" 
                                Style="{ThemeResource BodyStrongTextBlockStyle}"
                                VerticalAlignment="Center"/>
                </StackPanel>
                
                <!-- Input -->
                <TextBox Grid.Column="1"
                            Text="{x:Bind ViewModel.PoNumber, Mode=TwoWay}"
                            PlaceholderText="Enter PO number"
                            VerticalAlignment="Center"/>

                <!-- Actions -->
                <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="8">
                    <Button Command="{x:Bind ViewModel.ValidatePOCommand}"
                            Style="{StaticResource AccentButtonStyle}">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE896;" FontSize="16"/>
                            <TextBlock Text="Validate"/>
                        </StackPanel>
                    </Button>
                    
                    <Button Content="Other (Non-PO)"
                            Command="{x:Bind ViewModel.SwitchToOtherModeCommand}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- OTHER Mode Section (visible when in OTHER mode) -->
        <Border Grid.Row="1" 
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="20"
                Visibility="{x:Bind ViewModel.IsOtherMode, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
            <StackPanel Spacing="16">
                <TextBlock Text="OTHER Package (Non-PO)" 
                           Style="{StaticResource SubtitleTextBlockStyle}"/>

                <StackPanel Spacing="8">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8B4;" 
                                 FontSize="16"
                                 Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                        <TextBlock Text="Reason" 
                                  Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                    </StackPanel>
                    
                    <ComboBox PlaceholderText="Select reason"
                              ItemsSource="{x:Bind ViewModel.OtherReasons, Mode=OneWay}"
                              SelectedItem="{x:Bind ViewModel.SelectedOtherReason, Mode=TwoWay}"
                              DisplayMemberPath="Description"
                              HorizontalAlignment="Stretch"
                              MaxWidth="400"/>
                </StackPanel>

                <StackPanel Spacing="8">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE9F9;" 
                                 FontSize="16"
                                 Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                        <TextBlock Text="Quantity" 
                                  Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                    </StackPanel>
                    
                    <NumberBox Minimum="1"
                               SpinButtonPlacementMode="Inline"
                               Value="{x:Bind ViewModel.OtherQuantity, Mode=TwoWay}"
                               MaxWidth="200"
                               HorizontalAlignment="Left"/>
                </StackPanel>
            </StackPanel>
        </Border>

        <!-- PO Lines DataGrid + Specs (visible when NOT in OTHER mode) -->
        <Grid Grid.Row="2" ColumnSpacing="16"
              Visibility="{x:Bind ViewModel.IsOtherMode, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="3*"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <!-- Lines List -->
            <Border Grid.Column="0" 
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="16">
                <Grid>
                    <TextBlock Text="Select a line from the PO"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                               Visibility="{x:Bind ViewModel.PoLines.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=Inverse}"/>

                    <ListView ItemsSource="{x:Bind ViewModel.PoLines, Mode=OneWay}"
                              SelectedItem="{x:Bind ViewModel.SelectedPOLine, Mode=TwoWay}"
                              SelectionMode="Single"
                              Visibility="{x:Bind ViewModel.PoLines.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}}">
                        <ListView.HeaderTemplate>
                            <DataTemplate>
                                <Grid Padding="8" ColumnSpacing="0">
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="50"/>
                                        <ColumnDefinition Width="120"/>
                                        <ColumnDefinition Width="150"/>
                                        <ColumnDefinition Width="{Binding ViewModel.ReferenceColumnWidth, ElementName=RootPage}"/>
                                        <ColumnDefinition Width="{Binding ViewModel.SpecsColumnWidth, ElementName=RootPage}"/>
                                        <ColumnDefinition Width="80"/>
                                    </Grid.ColumnDefinitions>
                                    <TextBlock Grid.Column="0" Text="Line" FontWeight="SemiBold" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="1" Text="Part ID" FontWeight="SemiBold" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="2" Text="Description" FontWeight="SemiBold" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="3" Text="Ref (CO/WO)" FontWeight="SemiBold" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="4" Text="Specs (Preview)" FontWeight="SemiBold" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="5" Text="Qty" FontWeight="SemiBold" HorizontalAlignment="Right"/>
                                </Grid>
                            </DataTemplate>
                        </ListView.HeaderTemplate>
                        <ListView.ItemTemplate>
                            <DataTemplate x:DataType="models:Model_InforVisualPOLine">
                                <Grid Padding="8" ColumnSpacing="0">
                                    <Grid.ColumnDefinitions>
                                         <ColumnDefinition Width="50"/>
                                         <ColumnDefinition Width="120"/>
                                         <ColumnDefinition Width="150"/>
                                         <ColumnDefinition Width="{Binding ViewModel.ReferenceColumnWidth, ElementName=RootPage}"/>
                                         <ColumnDefinition Width="{Binding ViewModel.SpecsColumnWidth, ElementName=RootPage}"/>
                                         <ColumnDefinition Width="80"/>
                                    </Grid.ColumnDefinitions>
                                    <TextBlock Grid.Column="0" Text="{x:Bind LineNumber}" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="1" Text="{x:Bind PartID}" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="2" Text="{x:Bind Description}" TextTrimming="CharacterEllipsis" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="3" Text="{x:Bind ReferenceInfo}" TextTrimming="CharacterEllipsis" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="4" Text="{x:Bind SpecificationsPreview}" TextTrimming="CharacterEllipsis" Foreground="{ThemeResource TextFillColorSecondaryBrush}" Margin="0,0,12,0"/>
                                    <TextBlock Grid.Column="5" Text="{x:Bind QuantityOrderedDisplay}" HorizontalAlignment="Right"/>
                                </Grid>
                            </DataTemplate>
                        </ListView.ItemTemplate>
                    </ListView>
                </Grid>
            </Border>

            <!-- Specs Detail Panel -->
            <Border Grid.Column="1" 
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="16"
                    Visibility="{x:Bind ViewModel.IsLineSelected, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                <Grid RowSpacing="12">
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="*"/>
                    </Grid.RowDefinitions>
                    
                    <!-- Title -->
                    <TextBlock Grid.Row="0" 
                               Text="{x:Bind ViewModel.SelectedPOLineTitle, Mode=OneWay}" 
                               Style="{ThemeResource SubtitleTextBlockStyle}"
                               HorizontalAlignment="Center"
                               TextAlignment="Center"/>

                    <!-- Fields -->
                    <StackPanel Grid.Row="1" Spacing="8">
                        <!-- Part ID -->
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Text="Part ID:" FontWeight="SemiBold" Margin="0,0,8,0"/>
                            <TextBlock Grid.Column="1" Text="{x:Bind ViewModel.SelectedPOLine.PartID, Mode=OneWay}" TextWrapping="Wrap"/>
                        </Grid>

                        <!-- Description -->
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Text="Desc:" FontWeight="SemiBold" Margin="0,0,8,0"/>
                            <TextBlock Grid.Column="1" Text="{x:Bind ViewModel.SelectedPOLine.Description, Mode=OneWay}" TextWrapping="Wrap"/>
                        </Grid>

                        <!-- Quantity -->
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Text="Qty:" FontWeight="SemiBold" Margin="0,0,8,0"/>
                            <TextBlock Grid.Column="1" Text="{x:Bind ViewModel.SelectedPOLine.QuantityOrderedDisplay, Mode=OneWay}"/>
                        </Grid>

                        <!-- WO/CO (Hide if empty) -->
                        <Grid Visibility="{x:Bind ViewModel.HasReferenceInfo, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                             <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <TextBlock Text="Ref:" FontWeight="SemiBold" Margin="0,0,8,0"/>
                            <TextBlock Grid.Column="1" Text="{x:Bind ViewModel.SelectedPOLine.ReferenceInfo, Mode=OneWay}" TextWrapping="Wrap"/>
                        </Grid>
                    </StackPanel>

                    <!-- Specs -->
                    <Border Grid.Row="2" Background="{ThemeResource LayerFillColorDefaultBrush}" CornerRadius="4" Padding="8">
                         <ScrollViewer VerticalScrollBarVisibility="Auto">
                            <TextBlock Text="{x:Bind ViewModel.SelectedPOLine.Specifications, Mode=OneWay}" 
                                       TextWrapping="Wrap" 
                                       Style="{ThemeResource BodyTextBlockStyle}"/>
                        </ScrollViewer>
                    </Border>
                </Grid>
            </Border>
        </Grid>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="12">
            <ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                          Width="20" Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                       VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Action Buttons -->
        <StackPanel Grid.Row="4" Orientation="Horizontal" Spacing="12" HorizontalAlignment="Right">
            <Button Content="{x:Bind ViewModel.NavigationButtonText, Mode=OneWay}"
                    Command="{x:Bind ViewModel.ProceedToStep2Command}"
                    Style="{StaticResource AccentButtonStyle}"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Routing/Data/Dao_InforVisualPO.cs
```csharp
public class Dao_InforVisualPO
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<bool>> ValidatePOAsync(string poNumber)
⋮----
Console.WriteLine($"[DAO] Validating PO: {poNumber}");
await using (var connection = new SqlConnection(_connectionString))
⋮----
await connection.OpenAsync();
⋮----
await using (var command = new SqlCommand(query, connection))
⋮----
command.Parameters.AddWithValue("@PoNumber", poNumber);
var count = (int)(await command.ExecuteScalarAsync() ?? 0);
return Model_Dao_Result_Factory.Success(count > 0);
⋮----
public async Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetLinesAsync(string poNumber)
⋮----
Console.WriteLine($"[DAO] Fetching lines for PO: {poNumber}");
await using var connection = new SqlConnection(_connectionString);
⋮----
await using var command = new SqlCommand(query, connection);
⋮----
await using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
⋮----
lines.Add(MapFromReader(reader));
⋮----
return Model_Dao_Result_Factory.Success(lines);
⋮----
public async Task<Model_Dao_Result<Model_InforVisualPOLine>> GetLineAsync(string poNumber, int lineNumber)
⋮----
command.Parameters.AddWithValue("@LineNumber", lineNumber);
await using (var reader = await command.ExecuteReaderAsync())
⋮----
if (await reader.ReadAsync())
⋮----
return Model_Dao_Result_Factory.Success(line);
⋮----
public async Task<Model_Dao_Result<bool>> CheckConnectionAsync()
⋮----
return Model_Dao_Result_Factory.Success(true);
⋮----
return Model_Dao_Result_Factory.Success(false);
⋮----
private Model_InforVisualPOLine MapFromReader(IDataReader reader)
⋮----
return new Model_InforVisualPOLine
⋮----
PONumber = reader["PO_ID"].ToString() ?? string.Empty,
LineNumber = reader["PO_LINE"].ToString() ?? string.Empty,
PartID = reader["PART_ID"].ToString() ?? string.Empty,
Description = reader.IsDBNull(reader.GetOrdinal("PART_NAME"))
⋮----
: reader.GetString(reader.GetOrdinal("PART_NAME")),
Specifications = reader.IsDBNull(reader.GetOrdinal("SPECS"))
⋮----
: reader.GetString(reader.GetOrdinal("SPECS")),
QuantityOrdered = reader.IsDBNull(reader.GetOrdinal("QTY_ORDERED"))
⋮----
: reader.GetDecimal(reader.GetOrdinal("QTY_ORDERED")),
QuantityReceived = reader.IsDBNull(reader.GetOrdinal("QTY_RECEIVED"))
⋮----
: reader.GetDecimal(reader.GetOrdinal("QTY_RECEIVED")),
WorkOrder = HasColumn(reader, "WORK_ORDER_ID") && !reader.IsDBNull(reader.GetOrdinal("WORK_ORDER_ID"))
? reader.GetString(reader.GetOrdinal("WORK_ORDER_ID"))
⋮----
CustomerOrder = HasColumn(reader, "CUST_ORDER_ID") && !reader.IsDBNull(reader.GetOrdinal("CUST_ORDER_ID"))
? reader.GetString(reader.GetOrdinal("CUST_ORDER_ID"))
⋮----
private bool HasColumn(IDataReader reader, string columnName)
⋮----
if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
```

## File: Module_Routing/ViewModels/RoutingWizardStep1ViewModel.cs
```csharp
public partial class RoutingWizardStep1ViewModel : ObservableObject
⋮----
private readonly IRoutingInforVisualService _inforVisualService;
private readonly IRoutingService _routingService;
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_LoggingUtility _logger;
⋮----
private readonly RoutingWizardContainerViewModel _containerViewModel;
⋮----
public bool HasReferenceInfo => SelectedPOLine != null && !string.IsNullOrEmpty(SelectedPOLine.ReferenceInfo);
⋮----
private async Task ValidatePOAsync()
⋮----
if (string.IsNullOrWhiteSpace(PoNumber))
⋮----
var validationResult = await _inforVisualService.ValidatePoNumberAsync(PoNumber);
⋮----
var linesResult = await _inforVisualService.GetPoLinesAsync(PoNumber);
⋮----
PoLines.Clear();
⋮----
PoLines.Add(line);
if (!string.IsNullOrEmpty(line.ReferenceInfo)) hasRefs = true;
if (!string.IsNullOrEmpty(line.SpecificationsPreview)) hasSpecs = true;
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private async Task SwitchToOtherModeAsync()
⋮----
private void ProceedToStep2()
⋮----
_logger.LogInfo($"ProceedToStep2 called - IsOtherMode: {IsOtherMode}, SelectedPOLine: {SelectedPOLine?.PartID ?? "null"}, SelectedOtherReason: {SelectedOtherReason?.Description ?? "null"}");
// Update container with selected data
⋮----
_logger.LogInfo($"Updated container with OTHER reason: {SelectedOtherReason.Description}, Quantity: {OtherQuantity}");
⋮----
_logger.LogInfo($"Updated container with PO Line: {SelectedPOLine.PartID}, PO: {SelectedPOLine.PONumber}");
⋮----
_logger.LogWarning("ProceedToStep2: Neither PO line nor OTHER reason selected");
⋮----
_logger.LogInfo("Returning to Step 3 (Review) after edit");
_containerViewModel.NavigateToStep3Command.Execute(null);
⋮----
_logger.LogInfo("Executing NavigateToStep2Command");
_containerViewModel.NavigateToStep2Command.Execute(null);
_logger.LogInfo("NavigateToStep2Command executed");
⋮----
_logger.LogError($"Error in ProceedToStep2: {ex.Message}", ex);
⋮----
private bool CanProceedToStep2()
⋮----
_logger.LogInfo($"CanProceedToStep2: {canProceed} - IsOtherMode: {IsOtherMode}, SelectedPOLine: {SelectedPOLine?.PartID ?? "null"}, SelectedOtherReason: {SelectedOtherReason?.Description ?? "null"}");
⋮----
/// <summary>
/// Show "PO not found" confirmation dialog
private async Task ShowPONotFoundDialogAsync()
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
partial void OnSelectedPOLineChanged(Model_InforVisualPOLine? value)
⋮----
_logger.LogInfo($"SelectedPOLine changed to: {value?.PartID ?? "null"} (PO: {value?.PONumber ?? "null"})");
ProceedToStep2Command.NotifyCanExecuteChanged();
⋮----
partial void OnSelectedOtherReasonChanged(Model_RoutingOtherReason? value)
⋮----
_logger.LogInfo($"SelectedOtherReason changed to: {value?.Description ?? "null"}");
⋮----
private async void InitializeAsync()
⋮----
await _logger.LogInfoAsync($"[MOCK DATA MODE] Auto-filling PO number: {defaultPO}");
⋮----
await Task.Delay(500);
⋮----
await _logger.LogErrorAsync($"Error during initialization: {ex.Message}", ex);
```

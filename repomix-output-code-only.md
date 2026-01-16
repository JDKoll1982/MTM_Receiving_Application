# User Provided Header

MTM Receiving Application - Code-Only Minimal Token Export

# Files

## File: App.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Application
    x:Class="MTM_Receiving_Application.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:MTM_Receiving_Application"
    xmlns:converters="using:MTM_Receiving_Application.Converters">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
                <ResourceDictionary Source="ms-appx:///Module_Core/Themes/MaterialIcons.xaml"/>
                <!-- Other merged dictionaries here -->
            </ResourceDictionary.MergedDictionaries>
            <!-- Other app resources here -->
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

## File: Module_Core/Contracts/Services/IService_CSVWriter.cs

```csharp
public interface IService_CSVWriter
⋮----
public Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_ReceivingLoad> loads);
public Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads, bool append = true);
public Task<List<Model_ReceivingLoad>> ReadFromCSVAsync(string filePath);
public Task<Model_CSVDeleteResult> ClearCSVFilesAsync();
public Task<Model_CSVExistenceResult> CheckCSVFilesExistAsync();
public string GetLocalCSVPath();
public string GetNetworkCSVPath();
```

## File: Module_Core/Contracts/Services/IService_Dispatcher.cs

```csharp
public interface IService_Dispatcher
⋮----
public IService_DispatcherTimer CreateTimer();
public bool TryEnqueue(Action callback);
```

## File: Module_Core/Contracts/Services/IService_ErrorHandler.cs

```csharp
public interface IService_ErrorHandler
⋮----
public Task HandleErrorAsync(
⋮----
public Task LogErrorAsync(
⋮----
public Task ShowErrorDialogAsync(
⋮----
public Task HandleDaoErrorAsync(
⋮----
public Task ShowUserErrorAsync(string message, string title, string method);
public void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className);
```

## File: Module_Core/Contracts/Services/IService_Help.cs

```csharp
public interface IService_Help
⋮----
public Task ShowHelpAsync(string helpKey);
public Task ShowContextualHelpAsync(Enum_DunnageWorkflowStep step);
public Task ShowContextualHelpAsync(Enum_ReceivingWorkflowStep step);
public Model_HelpContent? GetHelpContent(string key);
public List<Model_HelpContent> GetHelpByCategory(string category);
public List<Model_HelpContent> SearchHelp(string searchTerm);
public Task<bool> IsDismissedAsync(string helpKey);
public Task SetDismissedAsync(string helpKey, bool isDismissed);
public string GetDunnageWorkflowHelp(Enum_DunnageWorkflowStep step);
public string GetReceivingWorkflowHelp(Enum_ReceivingWorkflowStep step);
public string GetTip(string viewName);
public string GetPlaceholder(string fieldName);
public string GetTooltip(string elementName);
public string GetInfoBarMessage(string messageKey);
```

## File: Module_Core/Contracts/Services/IService_InforVisual.cs

```csharp
public interface IService_InforVisual
⋮----
public Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber);
public Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID);
public Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date);
public Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(string poNumber, string partID);
public Task<bool> TestConnectionAsync();
```

## File: Module_Core/Contracts/Services/IService_LoggingUtility.cs

```csharp
public interface IService_LoggingUtility
⋮----
public void LogInfo(string message, string? context = null);
public void LogWarning(string message, string? context = null);
public void LogError(string message, Exception? exception = null, string? context = null);
public void LogCritical(string message, Exception? exception = null, string? context = null);
public void LogFatal(string message, Exception? exception = null, string? context = null);
public Task LogInfoAsync(string message, string? context = null);
public Task LogWarningAsync(string message, string? context = null);
public Task LogErrorAsync(string message, Exception? exception = null, string? context = null);
public string GetCurrentLogFilePath();
public bool EnsureLogDirectoryExists();
public int ArchiveOldLogs(int daysToKeep = 30);
```

## File: Module_Core/Contracts/Services/IService_Notification.cs

```csharp
public interface IService_Notification : INotifyPropertyChanged
⋮----
public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational);
```

## File: Module_Core/Contracts/Services/IService_OnStartup_AppLifecycle.cs

```csharp
public interface IService_OnStartup_AppLifecycle
⋮----
public Task StartAsync();
```

## File: Module_Core/Contracts/Services/IService_Pagination.cs

```csharp
public interface IService_Pagination
⋮----
public event EventHandler PageChanged;
public void SetSource<T>(IEnumerable<T> source);
public IEnumerable<T> GetCurrentPageItems<T>();
public bool NextPage();
public bool PreviousPage();
public bool FirstPage();
public bool LastPage();
public bool GoToPage(int pageNumber);
```

## File: Module_Core/Contracts/Services/IService_SessionManager.cs

```csharp
public interface IService_SessionManager
⋮----
public Task SaveSessionAsync(Model_ReceivingSession session);
public Task<Model_ReceivingSession?> LoadSessionAsync();
public Task<bool> ClearSessionAsync();
public bool SessionExists();
public string GetSessionFilePath();
```

## File: Module_Core/Contracts/Services/IService_UserSessionManager.cs

```csharp
public interface IService_UserSessionManager
⋮----
public Model_UserSession CreateSession(
⋮----
public void UpdateLastActivity();
public void StartTimeoutMonitoring();
public void StopTimeoutMonitoring();
public bool IsSessionTimedOut();
public Task EndSessionAsync(string reason);
```

## File: Module_Core/Contracts/Services/IService_ViewModelRegistry.cs

```csharp
public interface IService_ViewModelRegistry
⋮----
public void Register(object viewModel);
public IEnumerable<T> GetViewModels<T>();
public void ClearAllInputs();
```

## File: Module_Core/Contracts/Services/ITimer.cs

```csharp
public interface IService_DispatcherTimer
⋮----
public void Start();
public void Stop();
```

## File: Module_Core/Contracts/Services/IWindowService.cs

```csharp
public interface IService_Window
⋮----
public XamlRoot? GetXamlRoot();
```

## File: Module_Core/Contracts/Services/Navigation/IService_Navigation.cs

```csharp
public interface IService_Navigation
⋮----
public bool NavigateTo(Frame frame, Type pageType, object? parameter = null);
public bool NavigateTo(Frame frame, string pageTypeName, object? parameter = null);
public void ClearNavigation(Frame frame);
public bool CanGoBack(Frame frame);
public void GoBack(Frame frame);
```

## File: Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs

```csharp
public class Dao_InforVisualConnection
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<bool>> TestConnectionAsync()
⋮----
await using var connection = new SqlConnection(_connectionString);
await connection.OpenAsync();
⋮----
return Model_Dao_Result_Factory.Success(isConnected);
⋮----
public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetPOWithPartsAsync(string poNumber)
⋮----
var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("01_GetPOWithParts.sql");
⋮----
await using var command = new SqlCommand(query, connection);
command.Parameters.AddWithValue("@PoNumber", poNumber);
⋮----
await using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
⋮----
poLines.Add(new Model_InforVisualPO
⋮----
PoNumber = reader["PoNumber"].ToString() ?? string.Empty,
PoLine = Convert.ToInt32(reader["PoLine"]),
PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
OrderedQty = Convert.ToDecimal(reader["OrderedQty"]),
ReceivedQty = Convert.ToDecimal(reader["ReceivedQty"]),
RemainingQty = Convert.ToDecimal(reader["RemainingQty"]),
UnitOfMeasure = reader["UnitOfMeasure"].ToString() ?? "EA",
⋮----
VendorCode = reader["VendorCode"].ToString() ?? string.Empty,
VendorName = reader["VendorName"].ToString() ?? string.Empty,
PoStatus = reader["PoStatus"].ToString() ?? string.Empty,
SiteId = reader["SiteId"].ToString() ?? "002"
⋮----
return Model_Dao_Result_Factory.Success(poLines);
⋮----
public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
⋮----
var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("02_ValidatePONumber.sql");
⋮----
var count = (int?)await command.ExecuteScalarAsync() ?? 0;
⋮----
return Model_Dao_Result_Factory.Success(isValid);
⋮----
public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByNumberAsync(string partNumber)
⋮----
var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("03_GetPartByNumber.sql");
⋮----
command.Parameters.AddWithValue("@PartNumber", partNumber);
⋮----
if (await reader.ReadAsync())
⋮----
var part = new Model_InforVisualPart
⋮----
Description = reader["Description"].ToString() ?? string.Empty,
UnitCost = reader["UnitCost"] != DBNull.Value ? Convert.ToDecimal(reader["UnitCost"]) : 0,
PrimaryUom = reader["PrimaryUom"].ToString() ?? "EA",
OnHandQty = Convert.ToDecimal(reader["OnHandQty"]),
AllocatedQty = Convert.ToDecimal(reader["AllocatedQty"]),
AvailableQty = Convert.ToDecimal(reader["AvailableQty"]),
DefaultSite = reader["DefaultSite"].ToString() ?? string.Empty,
PartStatus = reader["PartStatus"].ToString() ?? string.Empty,
ProductLine = reader["ProductLine"].ToString() ?? string.Empty
⋮----
public async Task<Model_Dao_Result<List<Model_InforVisualPart>>> SearchPartsByDescriptionAsync(
⋮----
var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("04_SearchPartsByDescription.sql");
⋮----
command.Parameters.AddWithValue("@SearchTerm", searchTerm);
command.Parameters.AddWithValue("@MaxResults", maxResults);
⋮----
parts.Add(new Model_InforVisualPart
⋮----
return Model_Dao_Result_Factory.Success(parts);
```

## File: Module_Core/Data/InforVisual/Dao_InforVisualPart.cs

```csharp
public class Dao_InforVisualPart
⋮----
private static void ValidateReadOnlyConnection(string connectionString)
⋮----
if (string.IsNullOrWhiteSpace(connectionString))
⋮----
throw new ArgumentNullException(nameof(connectionString));
⋮----
var builder = new SqlConnectionStringBuilder(connectionString);
⋮----
throw new InvalidOperationException(
⋮----
public async Task<Model_Dao_Result<Model_InforVisualPart>> GetByPartNumberAsync(string partNumber)
⋮----
await using var connection = new SqlConnection(_connectionString);
await connection.OpenAsync();
var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("03_GetPartByNumber.sql");
await using var command = new SqlCommand(query, connection);
command.Parameters.AddWithValue("@PartNumber", partNumber);
await using var reader = await command.ExecuteReaderAsync();
if (await reader.ReadAsync())
⋮----
var part = new Model_InforVisualPart
⋮----
PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
Description = reader["Description"].ToString() ?? string.Empty,
PartType = reader["PartType"].ToString() ?? string.Empty,
UnitCost = Convert.ToDecimal(reader["UnitCost"]),
PrimaryUom = reader["PrimaryUom"].ToString() ?? "EA",
OnHandQty = Convert.ToDecimal(reader["OnHandQty"]),
AllocatedQty = Convert.ToDecimal(reader["AllocatedQty"]),
AvailableQty = Convert.ToDecimal(reader["AvailableQty"]),
DefaultSite = reader["DefaultSite"].ToString() ?? "002",
PartStatus = reader["PartStatus"].ToString() ?? "ACTIVE",
ProductLine = reader["ProductLine"].ToString() ?? string.Empty
⋮----
return Model_Dao_Result_Factory.Success(part);
⋮----
public async Task<Model_Dao_Result<List<Model_InforVisualPart>>> SearchPartsByDescriptionAsync(
⋮----
var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("04_SearchPartsByDescription.sql");
⋮----
command.Parameters.AddWithValue("@SearchTerm", searchTerm);
command.Parameters.AddWithValue("@MaxResults", maxResults);
⋮----
while (await reader.ReadAsync())
⋮----
list.Add(new Model_InforVisualPart
⋮----
return Model_Dao_Result_Factory.Success(list);
```

## File: Module_Core/Data/InforVisual/Dao_InforVisualPO.cs

```csharp
public class Dao_InforVisualPO
⋮----
private static void ValidateReadOnlyConnection(string connectionString)
⋮----
if (string.IsNullOrWhiteSpace(connectionString))
⋮----
throw new ArgumentNullException(nameof(connectionString));
⋮----
var builder = new SqlConnectionStringBuilder(connectionString);
⋮----
throw new InvalidOperationException(
⋮----
public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetByPoNumberAsync(string poNumber)
⋮----
await using var connection = new SqlConnection(_connectionString);
await connection.OpenAsync();
var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("01_GetPOWithParts.sql");
await using var command = new SqlCommand(query, connection);
command.Parameters.AddWithValue("@PoNumber", poNumber);
⋮----
await using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
⋮----
list.Add(new Model_InforVisualPO
⋮----
PoNumber = reader["PoNumber"].ToString() ?? string.Empty,
PoLine = Convert.ToInt32(reader["PoLine"]),
PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
OrderedQty = Convert.ToDecimal(reader["OrderedQty"]),
ReceivedQty = Convert.ToDecimal(reader["ReceivedQty"]),
RemainingQty = Convert.ToDecimal(reader["RemainingQty"]),
UnitOfMeasure = reader["UnitOfMeasure"].ToString() ?? "EA",
DueDate = reader["DueDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["DueDate"]),
VendorCode = reader["VendorCode"].ToString() ?? string.Empty,
VendorName = reader["VendorName"].ToString() ?? string.Empty,
PoStatus = reader["PoStatus"].ToString() ?? string.Empty,
SiteId = reader["SiteId"].ToString() ?? "002"
⋮----
return Model_Dao_Result_Factory.Success(list);
⋮----
public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
⋮----
var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("02_ValidatePONumber.sql");
⋮----
var count = Convert.ToInt32(await command.ExecuteScalarAsync());
⋮----
return Model_Dao_Result_Factory.Success(isValid);
```

## File: Module_Core/Helpers/Database/Helper_SqlQueryLoader.cs

```csharp
public static class Helper_SqlQueryLoader
⋮----
public static string LoadInforVisualQuery(string resourcePath)
⋮----
var assembly = Assembly.GetExecutingAssembly();
⋮----
using var stream = assembly.GetManifestResourceStream(resourceName);
⋮----
var filePath = Path.Combine(baseDir, "Database", "InforVisualScripts", "Queries", resourcePath);
if (File.Exists(filePath))
⋮----
return File.ReadAllText(filePath);
⋮----
var solutionRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.FullName;
⋮----
filePath = Path.Combine(solutionRoot, "Database", "InforVisualScripts", "Queries", resourcePath);
⋮----
throw new FileNotFoundException($"SQL query file not found: {resourcePath}. Resource name attempted: {resourceName}");
⋮----
using var reader = new StreamReader(stream);
return reader.ReadToEnd();
⋮----
throw new InvalidOperationException($"Failed to load SQL query from {resourcePath}: {ex.Message}", ex);
⋮----
public static string CleanQuery(string sql)
⋮----
if (string.IsNullOrWhiteSpace(sql))
⋮----
var lines = sql.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
⋮----
var trimmedLine = line.Trim();
if (trimmedLine.StartsWith("--"))
⋮----
var commentIndex = line.IndexOf("--");
⋮----
var lineWithoutComment = line.Substring(0, commentIndex).TrimEnd();
if (!string.IsNullOrWhiteSpace(lineWithoutComment))
⋮----
cleanedLines.Add(lineWithoutComment);
⋮----
else if (!string.IsNullOrWhiteSpace(line))
⋮----
cleanedLines.Add(line);
⋮----
return string.Join(Environment.NewLine, cleanedLines);
⋮----
public static string ExtractQueryFromFile(string sqlFileContent)
⋮----
if (string.IsNullOrWhiteSpace(sqlFileContent))
⋮----
var lines = sqlFileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
⋮----
if (trimmedLine.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase))
⋮----
(trimmedLine.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
trimmedLine.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
trimmedLine.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) ||
trimmedLine.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) ||
trimmedLine.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)))
⋮----
if (foundQueryStart && !string.IsNullOrWhiteSpace(line))
⋮----
queryLines.Add(line);
⋮----
return string.Join(Environment.NewLine, queryLines);
⋮----
public static string LoadAndPrepareQuery(string resourcePath)
⋮----
return extractedQuery.Trim();
```

## File: Module_Core/Helpers/UI/Helper_MaterialIcons.cs

```csharp
public static class Helper_MaterialIcons
⋮----
public static List<MaterialIconKind> GetAllIcons()
⋮----
_allIcons = Enum.GetValues<MaterialIconKind>().ToList();
⋮----
public static IEnumerable<MaterialIconKind> SearchIcons(string searchText)
⋮----
if (string.IsNullOrWhiteSpace(searchText))
⋮----
searchText = searchText.ToLower().Trim();
return all.Where(k => k.ToString().ToLower().Contains(searchText));
```

## File: Module_Core/Helpers/UI/Helper_WindowExtensions.cs

```csharp
public static class Helper_WindowExtensions
⋮----
public static void SetWindowSize(this Window window, int width, int height)
⋮----
var appWindow = window.GetAppWindow();
appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
⋮----
public static void CenterOnScreen(this Window window)
⋮----
appWindow.Move(new Windows.Graphics.PointInt32(x, y));
⋮----
public static void SetFixedSize(this Window window, bool disableMaximize = true, bool disableMinimize = true)
⋮----
public static void HideTitleBarIcon(this Window window)
⋮----
public static void UseCustomTitleBar(this Window window, UIElement? customTitleBarElement = null)
⋮----
window.SetTitleBar(customTitleBarElement);
⋮----
public static void MakeTitleBarTransparent(this Window window)
⋮----
private static AppWindow GetAppWindow(this Window window)
⋮----
var hWnd = WindowNative.GetWindowHandle(window);
var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
return AppWindow.GetFromWindowId(windowId);
```

## File: Module_Core/Helpers/UI/Helper_WorkflowHelpContentGenerator.cs

```csharp
public static class Helper_WorkflowHelpContentGenerator
⋮----
public static UIElement GenerateHelpContent(Enum_ReceivingWorkflowStep step)
⋮----
private static UIElement CreateModeSelectionHelp()
⋮----
private static UIElement CreateManualEntryHelp()
⋮----
private static UIElement CreatePOEntryHelp()
⋮----
private static UIElement CreateLoadEntryHelp()
⋮----
private static UIElement CreateWeightQuantityHelp()
⋮----
private static UIElement CreateHeatLotHelp()
⋮----
private static UIElement CreatePackageTypeHelp()
⋮----
private static UIElement CreateReviewHelp()
⋮----
private static UIElement CreateDefaultHelp()
⋮----
var stack = new StackPanel { Spacing = 12, Padding = new Thickness(4), MaxWidth = 450 };
var title = new TextBlock
⋮----
var description = new TextBlock
⋮----
stack.Children.Add(title);
stack.Children.Add(description);
⋮----
private static UIElement CreateHelpPanel(string title, string description, (string header, string content)[] sections)
⋮----
var mainStack = new StackPanel { Spacing = 20, Padding = new Thickness(4), MaxWidth = 450 };
var titleStack = new StackPanel { Spacing = 8 };
var titleText = new TextBlock
⋮----
var underline = new Border
⋮----
CornerRadius = new CornerRadius(1),
⋮----
var descText = new TextBlock
⋮----
titleStack.Children.Add(titleText);
titleStack.Children.Add(underline);
titleStack.Children.Add(descText);
mainStack.Children.Add(titleStack);
⋮----
var border = new Border
⋮----
Padding = new Thickness(16),
CornerRadius = new CornerRadius(8),
BorderThickness = new Thickness(1),
⋮----
var sectionStack = new StackPanel { Spacing = 8 };
var headerText = new TextBlock
⋮----
var contentText = new TextBlock
⋮----
sectionStack.Children.Add(headerText);
sectionStack.Children.Add(contentText);
⋮----
mainStack.Children.Add(border);
```

## File: Module_Core/Models/Core/Model_Dao_Result_Generic.cs

```csharp
public class Model_Dao_Result<T> : Model_Dao_Result
```

## File: Module_Core/Models/Core/Model_Dao_Result.cs

```csharp
public class Model_Dao_Result
```

## File: Module_Core/Models/Core/Model_HelpContent.cs

```csharp
public partial class Model_HelpContent : ObservableObject
⋮----
private Enum_HelpType _helpType = Enum_HelpType.Info;
⋮----
private Enum_HelpSeverity _severity = Enum_HelpSeverity.Info;
⋮----
private DateTime _lastUpdated = DateTime.Now;
⋮----
if (!string.IsNullOrEmpty(Icon) && Enum.TryParse<MaterialIconKind>(Icon, true, out var kind))
```

## File: Module_Core/Models/Enums/Enum_DataSourceType.cs

```csharp

```

## File: Module_Core/Models/Enums/Enum_ErrorSeverity.cs

```csharp

```

## File: Module_Core/Models/Enums/Enum_HelpSeverity.cs

```csharp

```

## File: Module_Core/Models/Enums/Enum_HelpType.cs

```csharp

```

## File: Module_Core/Models/Enums/Enum_InfoBarSeverity.cs

```csharp

```

## File: Module_Core/Models/Enums/Enum_LabelType.cs

```csharp

```

## File: Module_Core/Models/Enums/Enum_PackageType.cs

```csharp

```

## File: Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs

```csharp

```

## File: Module_Core/Models/Enums/Enum_ValidationSeverity.cs

```csharp

```

## File: Module_Core/Models/Systems/Model_AppSettings.cs

```csharp
public class Model_AppSettings
```

## File: Module_Core/Models/Systems/Model_CreateUserResult.cs

```csharp
public class Model_CreateUserResult
⋮----
public static Model_CreateUserResult SuccessResult(int employeeNumber) => new()
⋮----
public static Model_CreateUserResult ErrorResult(string message) => new()
```

## File: Module_Core/Models/Systems/Model_SessionTimedOutEventArgs.cs

```csharp
public class Model_SessionTimedOutEventArgs : EventArgs
```

## File: Module_Core/Services/Database/Service_ErrorHandler.cs

```csharp
public class Service_ErrorHandler : IService_ErrorHandler
⋮----
private readonly IService_LoggingUtility _loggingService;
private readonly IService_Window _windowService;
⋮----
_loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
_windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
⋮----
public async Task HandleErrorAsync(
⋮----
public Task LogErrorAsync(
⋮----
return Task.Run(() =>
⋮----
_loggingService.LogInfo(errorMessage);
⋮----
_loggingService.LogWarning(errorMessage);
⋮----
_loggingService.LogError(errorMessage, exception);
⋮----
_loggingService.LogCritical(errorMessage, exception);
⋮----
_loggingService.LogFatal(errorMessage, exception);
⋮----
public async Task ShowErrorDialogAsync(
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_loggingService.LogWarning("Cannot show dialog - XamlRoot is null");
⋮----
var dialog = new ContentDialog
⋮----
await dialog.ShowAsync();
⋮----
_loggingService.LogError($"Failed to show error dialog: {ex.Message}", ex);
⋮----
public async Task HandleDaoErrorAsync(
⋮----
public Task ShowUserErrorAsync(string message, string title, string method)
⋮----
public void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className)
⋮----
private static string GetDialogTitle(Enum_ErrorSeverity severity)
```

## File: Module_Core/Services/Database/Service_InforVisualConnect.cs

```csharp
public class Service_InforVisualConnect : IService_InforVisual
⋮----
private readonly Dao_InforVisualConnection _dao;
⋮----
_dao = dao ?? throw new ArgumentNullException(nameof(dao));
⋮----
public async Task<bool> TestConnectionAsync()
⋮----
var result = await _dao.TestConnectionAsync();
⋮----
public async Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber)
⋮----
if (string.IsNullOrWhiteSpace(poNumber))
⋮----
var result = await _dao.GetPOWithPartsAsync(cleanPoNumber);
⋮----
public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID)
⋮----
if (string.IsNullOrWhiteSpace(partID))
⋮----
var result = await _dao.GetPartByNumberAsync(partID);
⋮----
public async Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(
⋮----
public async Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(string poNumber, string partID)
⋮----
var result = await _dao.GetPOWithPartsAsync(poNumber);
⋮----
var matchingLine = result.Data.FirstOrDefault(line =>
line.PartNumber.Equals(partID, StringComparison.OrdinalIgnoreCase));
⋮----
private Model_InforVisualPO ConvertToServiceModel(List<Models.InforVisual.Model_InforVisualPO> poLines)
⋮----
return new Model_InforVisualPO
⋮----
Parts = poLines.ConvertAll(line => new Model_InforVisualPart
⋮----
POLineNumber = line.PoLine.ToString(),
⋮----
private Model_InforVisualPart ConvertPartToServiceModel(Models.InforVisual.Model_InforVisualPart daoPart)
⋮----
return new Model_InforVisualPart
⋮----
private Model_Dao_Result<Model_InforVisualPO?> CreateMockPO(string poNumber)
⋮----
var mockPO = new Model_InforVisualPO
⋮----
new Model_InforVisualPart
⋮----
private Model_Dao_Result<Model_InforVisualPart?> CreateMockPart(string partID)
⋮----
var mockPart = new Model_InforVisualPart
```

## File: Module_Core/Services/Help/Service_Help.cs

```csharp
public class Service_Help : IService_Help
⋮----
private readonly IService_Window _windowService;
private readonly IService_LoggingUtility _logger;
private readonly IService_Dispatcher _dispatcher;
⋮----
public async Task ShowHelpAsync(string helpKey)
⋮----
await _logger.LogWarningAsync($"Help content not found for key: {helpKey}");
⋮----
_dispatcher.TryEnqueue(async () =>
⋮----
await _logger.LogErrorAsync("Failed to retrieve HelpDialog from DI container");
tcs.SetResult(false);
⋮----
dialog.SetHelpContent(content);
dialog.XamlRoot = _windowService.GetXamlRoot();
⋮----
await _logger.LogErrorAsync("XamlRoot is null, cannot show help dialog");
⋮----
await dialog.ShowAsync();
await _logger.LogInfoAsync($"Displayed help for: {helpKey}");
tcs.SetResult(true);
⋮----
await _logger.LogErrorAsync($"Error in UI thread: {ex.Message}");
tcs.SetException(ex);
⋮----
await _logger.LogErrorAsync($"Error showing help for key {helpKey}: {ex.Message}");
⋮----
public async Task ShowContextualHelpAsync(Enum_DunnageWorkflowStep step)
⋮----
public async Task ShowContextualHelpAsync(Enum_ReceivingWorkflowStep step)
⋮----
public Model_HelpContent? GetHelpContent(string key)
⋮----
return _helpContentCache.TryGetValue(key, out var content) ? content : null;
⋮----
public List<Model_HelpContent> GetHelpByCategory(string category)
⋮----
.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
.OrderBy(c => c.Title)
.ToList();
⋮----
public List<Model_HelpContent> SearchHelp(string searchTerm)
⋮----
if (string.IsNullOrWhiteSpace(searchTerm))
⋮----
var term = searchTerm.ToLower();
⋮----
.Where(c => c.Title.ToLower().Contains(term) || c.Content.ToLower().Contains(term))
.OrderByDescending(c => c.Title.ToLower().Contains(term))
.ThenBy(c => c.Title)
.Take(20)
⋮----
public async Task<bool> IsDismissedAsync(string helpKey)
⋮----
return _dismissedTips.Contains(helpKey);
⋮----
public async Task SetDismissedAsync(string helpKey, bool isDismissed)
⋮----
_dismissedTips.Add(helpKey);
⋮----
_dismissedTips.Remove(helpKey);
⋮----
public string GetDunnageWorkflowHelp(Enum_DunnageWorkflowStep step)
⋮----
public string GetReceivingWorkflowHelp(Enum_ReceivingWorkflowStep step)
⋮----
public string GetTip(string viewName)
⋮----
public string GetPlaceholder(string fieldName)
⋮----
public string GetTooltip(string elementName)
⋮----
public string GetInfoBarMessage(string messageKey)
⋮----
private void InitializeHelpContent()
⋮----
// Dunnage Workflow Help Content
⋮----
// Receiving Workflow Help Content
⋮----
// Admin Help Content
⋮----
// Tooltips
⋮----
// Placeholders
⋮----
// Tips
⋮----
// InfoBar Messages
⋮----
private void InitializeDunnageHelp()
⋮----
AddHelpContent(new Model_HelpContent
⋮----
private void InitializeReceivingHelp()
⋮----
private void InitializeAdminHelp()
⋮----
private void InitializeTooltips()
⋮----
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.FirstPage", Content = "First Page", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.PreviousPage", Content = "Previous Page", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.NextPage", Content = "Next Page", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.LastPage", Content = "Last Page", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.QuickGuidedWizard", Content = "Skip mode selection and go directly to Guided Wizard", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.QuickManualEntry", Content = "Skip mode selection and go directly to Manual Entry", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.QuickEditMode", Content = "Skip mode selection and go directly to Edit Mode", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.ReturnToModeSelection", Content = "Return to mode selection (clears current work)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.ShowHelp", Content = "Click for help about current step", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.Refresh", Content = "Refresh parts list", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.AddMultipleRows", Content = "Add multiple rows at once (up to 100)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.AutoFill", Content = "Auto-fill from last entry for this Part ID", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.FillBlanks", Content = "Copy PO, Location, and specs from last row to empty fields", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SortForPrinting", Content = "Sort by Part ID â†’ PO â†’ Type", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.LoadSessionMemory", Content = "Load unsaved loads from current session", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.LoadRecentCSV", Content = "Load from most recent CSV export", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.LoadHistoricalData", Content = "Load historical loads from database", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.CreateType", Content = "Create a new dunnage type", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.EditType", Content = "Edit selected type", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.DeleteType", Content = "Delete selected type with impact analysis", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.ReturnToAdmin", Content = "Return to admin main navigation", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.CreatePart", Content = "Create a new dunnage part", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.EditPart", Content = "Edit selected part", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.DeletePart", Content = "Delete selected part", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.RemoveSelectedRows", Content = "Remove all selected rows", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.FilterLastWeek", Content = "Show loads from last 7 days", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.FilterToday", Content = "Show loads from today only", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.FilterThisWeek", Content = "Show loads from this week (Mon-Sun)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.FilterThisMonth", Content = "Show loads from this month", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.FilterThisQuarter", Content = "Show loads from this quarter", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.FilterShowAll", Content = "Show all loads from last year", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SaveChanges", Content = "Save all changes to database", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SelectAll", Content = "Select or deselect all rows", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.RemoveRow", Content = "Remove selected rows", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.ClearFilters", Content = "Remove all date filters", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.GoToPage", Content = "Go to specified page", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SaveAndFinish", Content = "Save all changes and finish", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.AddRow", Content = "Add a new row to the grid", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.HelpDunnageTypes", Content = "Click for help about dunnage types", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Field.TypeName", Content = "Enter a unique descriptive name for this dunnage type", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SelectIcon", Content = "Click to browse and select an icon from the icon library", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Field.SpecName", Content = "Enter a unique name for this specification field", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Field.SpecType", Content = "Select the data type for this field", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Field.MinValue", Content = "Minimum allowed value (optional)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Field.MaxValue", Content = "Maximum allowed value (optional)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Field.Unit", Content = "Unit of measurement to display (optional)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Field.Required", Content = "Check if this field must be filled when creating parts", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.AddSpec", Content = "Add this specification to the list below", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.List.Specs", Content = "List of specifications defined for this type", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.RemoveSpec", Content = "Remove this specification", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.AddAnother", Content = "Add another load to the current batch", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SaveAndGenerate", Content = "Save all entries to database and generate labels", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.ModeSelection", Content = "Return to mode selection (clears current work)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.StepHelp", Content = "Click for help about the current step", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.BackStep", Content = "Go back to previous step", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.NextStep", Content = "Proceed to next step", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SaveAndReview", Content = "Save entry and proceed to review", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.StartNew", Content = "Start a new receiving entry", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.ResetCSV", Content = "Clear the CSV data and start over", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.LoadPO", Content = "Load purchase order details from Infor Visual", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SwitchToNonPO", Content = "Switch to non-PO entry mode for items without purchase orders", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.LookupPart", Content = "Look up part details in the system", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Tooltip.Button.SwitchToPO", Content = "Switch to PO entry mode", HelpType = Enum_HelpType.Tip });
⋮----
private void InitializePlaceholders()
⋮----
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.Quantity", Content = "Enter quantity...", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.PONumber", Content = "Enter PO number (optional)...", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.Location", Content = "Enter location...", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.PartSelection", Content = "Choose a part...", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.HeatLotNumber", Content = "Enter heat/lot number or leave blank", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.PONumberReceiving", Content = "Enter PO (e.g., 66868 or PO-066868)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.PartID", Content = "Enter Part ID (e.g., MMC-001, MMF-456)", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.Weight", Content = "Enter weight...", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.QuantityNumber", Content = "Enter whole number", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.StartDate", Content = "Start Date", HelpType = Enum_HelpType.Tip });
AddHelpContent(new Model_HelpContent { Key = "Placeholder.Field.EndDate", Content = "End Date", HelpType = Enum_HelpType.Tip });
⋮----
private void InitializeTips()
⋮----
private void InitializeInfoBarMessages()
⋮----
private void AddHelpContent(Model_HelpContent content)
⋮----
if (!string.IsNullOrEmpty(content.Key))
```

## File: Module_Core/Services/Navigation/Service_Navigation.cs

```csharp
public class Service_Navigation : IService_Navigation
⋮----
private readonly IService_LoggingUtility _logger;
⋮----
public bool NavigateTo(Frame frame, Type pageType, object? parameter = null)
⋮----
_logger.LogError("Navigation frame is null", null, "Service_Navigation");
⋮----
_logger.LogError("Page type is null", null, "Service_Navigation");
⋮----
var result = frame.Navigate(pageType, parameter);
⋮----
_logger.LogInfo($"Navigated to {pageType.Name}", "Service_Navigation");
⋮----
_logger.LogWarning($"Failed to navigate to {pageType.Name}", "Service_Navigation");
⋮----
_logger.LogError($"Navigation error: {ex.Message}", ex, "Service_Navigation");
⋮----
public bool NavigateTo(Frame frame, string pageTypeName, object? parameter = null)
⋮----
var pageType = Type.GetType(pageTypeName);
⋮----
_logger.LogError($"Could not find type: {pageTypeName}", null, "Service_Navigation");
⋮----
public void ClearNavigation(Frame frame)
⋮----
_logger.LogInfo("Navigation cleared", "Service_Navigation");
⋮----
public bool CanGoBack(Frame frame)
⋮----
public void GoBack(Frame frame)
⋮----
frame.GoBack();
_logger.LogInfo("Navigated back", "Service_Navigation");
```

## File: Module_Core/Services/Service_Dispatcher.cs

```csharp
public class Service_Dispatcher : IService_Dispatcher
⋮----
private readonly DispatcherQueue _dispatcherQueue;
⋮----
_dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
⋮----
public IService_DispatcherTimer CreateTimer()
⋮----
return new Service_DispatcherTimerWrapper(_dispatcherQueue.CreateTimer());
⋮----
public bool TryEnqueue(Action callback)
⋮----
return _dispatcherQueue.TryEnqueue(() => callback());
```

## File: Module_Core/Services/Service_DispatcherTimerWrapper.cs

```csharp
public class Service_DispatcherTimerWrapper : IService_DispatcherTimer
⋮----
private readonly DispatcherQueueTimer _timer;
⋮----
_timer = timer ?? throw new ArgumentNullException(nameof(timer));
⋮----
public void Start() => _timer.Start();
public void Stop() => _timer.Stop();
```

## File: Module_Core/Services/Service_Notification.cs

```csharp
public partial class Service_Notification : ObservableObject, IService_Notification
⋮----
private readonly IService_Dispatcher _dispatcher;
⋮----
private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;
⋮----
public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
⋮----
Task.Delay(5000).ContinueWith(_ =>
⋮----
_dispatcher.TryEnqueue(() =>
```

## File: Module_Core/Services/Service_Pagination.cs

```csharp
public class Service_Pagination : IService_Pagination
⋮----
return (int)Math.Ceiling((double)TotalItems / PageSize);
⋮----
public void SetSource<T>(IEnumerable<T> source)
⋮----
public IEnumerable<T> GetCurrentPageItems<T>()
⋮----
.Skip((CurrentPage - 1) * PageSize)
.Take(PageSize)
⋮----
public bool NextPage()
⋮----
public bool PreviousPage()
⋮----
public bool FirstPage()
⋮----
public bool LastPage()
⋮----
public bool GoToPage(int pageNumber)
⋮----
private void OnPageChanged()
```

## File: Module_Core/Services/Service_Window.cs

```csharp
public class Service_Window : IService_Window
⋮----
public XamlRoot? GetXamlRoot()
```

## File: Module_Core/Services/UI/Service_ViewModelRegistry.cs

```csharp
public class Service_ViewModelRegistry : IService_ViewModelRegistry
⋮----
public void Register(object viewModel)
⋮----
_viewModels.Add(new WeakReference<object>(viewModel));
⋮----
public IEnumerable<T> GetViewModels<T>()
⋮----
.Select(r => r.TryGetTarget(out var target) ? target : null)
.Where(t => t != null && t is T)
⋮----
public void ClearAllInputs()
⋮----
if (vmRef.TryGetTarget(out var vm))
⋮----
resettable.ResetToDefaults();
⋮----
private void Cleanup()
⋮----
_viewModels.RemoveAll(r => !r.TryGetTarget(out _));
```

## File: Module_Core/Themes/MaterialIcons.xaml

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="using:Material.Icons.WinUI3">

  <Style TargetType="local:MaterialIcon">
    <Setter Property="IsTabStop" Value="False" />
    <Setter Property="FlowDirection" Value="LeftToRight" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="local:MaterialIcon">
          <Border Background="{TemplateBinding Background}"
                  BorderBrush="{TemplateBinding BorderBrush}"
                  BorderThickness="{TemplateBinding BorderThickness}">
            <Viewbox FlowDirection="{TemplateBinding FlowDirection}">
              <Canvas Width="24"
                      Height="24">
                <Path Data="{Binding Data, RelativeSource={RelativeSource TemplatedParent}}"
                      Fill="{TemplateBinding Foreground}" />
              </Canvas>
            </Viewbox>
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style TargetType="local:MaterialIconText">
    <Setter Property="IsTabStop" Value="False" />
    <Setter Property="FlowDirection" Value="LeftToRight" />
    <Setter Property="Spacing" Value="5" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="local:MaterialIconText">
          <StackPanel Orientation="{TemplateBinding Orientation}"
                      Spacing="{TemplateBinding Spacing}">
            <local:MaterialIcon Width="{TemplateBinding IconSize}"
                                Height="{TemplateBinding IconSize}"
                                Animation="{TemplateBinding Animation}"
                                Kind="{TemplateBinding Kind}" />
            <TextBlock Text="{TemplateBinding Text}" />
          </StackPanel>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
</ResourceDictionary>
```

## File: Module_Dunnage/Enums/Enum_DunnageWorkflowStep.cs

```csharp

```

## File: Module_Dunnage/Models/Model_DunnageLine.cs

```csharp
public class Model_DunnageLine
```

## File: Module_Dunnage/Models/Model_DunnagePart.cs

```csharp
public class Model_DunnagePart : INotifyPropertyChanged
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
⋮----
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
⋮----
if (EqualityComparer<T>.Default.Equals(field, value))
⋮----
private void DeserializeSpecValues()
⋮----
if (string.IsNullOrWhiteSpace(SpecValues))
```

## File: Module_Dunnage/Models/Model_DunnageSession.cs

```csharp
public class Model_DunnageSession : INotifyPropertyChanged
⋮----
protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
⋮----
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
⋮----
if (EqualityComparer<T>.Default.Equals(field, value))
```

## File: Module_Dunnage/Models/Model_DunnageSpec.cs

```csharp
public class Model_DunnageSpec : INotifyPropertyChanged
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
⋮----
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
⋮----
if (EqualityComparer<T>.Default.Equals(field, value))
⋮----
private void DeserializeSpecs()
⋮----
if (string.IsNullOrWhiteSpace(SpecValue))
```

## File: Module_Dunnage/Models/Model_DunnageType.cs

```csharp
public partial class Model_DunnageType : ObservableObject
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
if (!string.IsNullOrEmpty(Icon) && Enum.TryParse<MaterialIconKind>(Icon, true, out var kind))
```

## File: Module_Dunnage/Models/Model_IconDefinition.cs

```csharp
public partial class Model_IconDefinition : ObservableObject
⋮----
private MaterialIconKind _kind;
⋮----
get => Kind.ToString();
⋮----
else if (LegacyMapping.TryGetValue(value, out var legacyResult))
⋮----
Name = legacyResult.ToString();
```

## File: Module_Dunnage/Models/Model_InventoriedDunnage.cs

```csharp
public class Model_InventoriedDunnage : INotifyPropertyChanged
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
⋮----
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
⋮----
if (EqualityComparer<T>.Default.Equals(field, value))
```

## File: Module_Dunnage/Models/Model_SpecInput.cs

```csharp
public partial class Model_SpecInput : ObservableObject
```

## File: Module_Dunnage/Models/Model_SpecItem.cs

```csharp
public class Model_SpecItem
⋮----
parts.Add($"Min: {MinValue.Value}");
⋮----
parts.Add($"Max: {MaxValue.Value}");
⋮----
if (!string.IsNullOrEmpty(Unit))
⋮----
parts.Add(Unit);
⋮----
desc += $" ({string.Join(", ", parts)})";
```

## File: Module_Dunnage/Models/SpecDefinition.cs

```csharp
public class SpecDefinition
```

## File: Module_Dunnage/Views/View_Dunnage_AdminInventoryView.xaml

```xml
<Page
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_AdminInventoryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Page.Resources>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
        <converters:Converter_StringFormat x:Key="DateTimeToStringConverter"/>
    </Page.Resources>

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,16">
            <TextBlock 
                Text="Manage Inventoried Parts" 
                Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock 
                Text="Configure parts that require inventory tracking" 
                Style="{StaticResource BodyTextBlockStyle}"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="0,8,0,0"/>
        </StackPanel>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,0,0,16">
            <AppBarButton 
                Icon="Add" 
                Label="Add Part to List"
                Command="{x:Bind ViewModel.ShowAddToListCommand}"
                ToolTipService.ToolTip="Add a part to the inventory tracking list"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Edit" 
                Label="Edit Entry"
                Command="{x:Bind ViewModel.ShowEditEntryCommand}"
                IsEnabled="{x:Bind ViewModel.HasSelection, Mode=OneWay}"
                ToolTipService.ToolTip="Edit the selected inventory entry"/>
            <AppBarButton 
                Icon="Delete" 
                Label="Remove from List"
                Command="{x:Bind ViewModel.ShowRemoveConfirmationCommand}"
                IsEnabled="{x:Bind ViewModel.HasSelection, Mode=OneWay}"
                ToolTipService.ToolTip="Remove the selected part from inventory tracking"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Back" 
                Label="Back to Admin Hub"
                Command="{x:Bind ViewModel.BackToHubCommand}"
                ToolTipService.ToolTip="Return to the admin hub"/>
        </CommandBar>

        <!-- DataGrid -->
        <Border 
            Grid.Row="2" 
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="1">
            <Grid>
                <controls:DataGrid
                    ItemsSource="{x:Bind ViewModel.InventoriedParts, Mode=OneWay}"
                    SelectedItem="{x:Bind ViewModel.SelectedInventoriedPart, Mode=TwoWay}"
                    AutoGenerateColumns="False"
                    IsReadOnly="True"
                    GridLinesVisibility="Horizontal"
                    HeadersVisibility="Column"
                    SelectionMode="Single"
                    CanUserReorderColumns="False"
                    CanUserSortColumns="True"
                    VerticalScrollBarVisibility="Auto"
                    HorizontalScrollBarVisibility="Auto"
                    AlternatingRowBackground="{ThemeResource SubtleFillColorSecondaryBrush}">

                    <controls:DataGrid.Columns>
                        <!-- Part ID Column -->
                        <controls:DataGridTextColumn 
                            Header="Part ID" 
                            Binding="{Binding PartId}"
                            Width="2*"
                            CanUserResize="True"/>

                        <!-- Inventory Method Column -->
                        <controls:DataGridTextColumn 
                            Header="Inventory Method" 
                            Binding="{Binding InventoryMethod}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Notes Column -->
                        <controls:DataGridTextColumn 
                            Header="Notes" 
                            Binding="{Binding Notes}"
                            Width="3*"
                            CanUserResize="True"/>

                        <!-- Date Added Column -->
                        <controls:DataGridTextColumn 
                            Header="Date Added" 
                            Binding="{Binding CreatedDate, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Added By Column -->
                        <controls:DataGridTextColumn 
                            Header="Added By" 
                            Binding="{Binding CreatedBy}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Last Modified Column -->
                        <controls:DataGridTextColumn 
                            Header="Last Modified" 
                            Binding="{Binding ModifiedDate, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Modified By Column -->
                        <controls:DataGridTextColumn 
                            Header="Modified By" 
                            Binding="{Binding ModifiedBy}"
                            Width="*"
                            CanUserResize="True"/>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>

                <!-- Empty State -->
                <StackPanel 
                    VerticalAlignment="Center" 
                    HorizontalAlignment="Center"
                    Spacing="16"
                    Visibility="{x:Bind ViewModel.InventoriedParts.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=Inverse}">
                    <FontIcon 
                        Glyph="&#xE7C3;" 
                        FontSize="72"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="No inventoried parts found" 
                        Style="{StaticResource SubtitleTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="Click 'Add Part to List' to configure parts that require inventory tracking"
                        Style="{StaticResource CaptionTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="8" Margin="0,16,0,0">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
            <TextBlock 
                Text="|"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="8,0,8,0"/>
            <TextBlock>
                <Run Text="Total Parts:"/>
                <Run Text="{x:Bind ViewModel.InventoriedParts.Count, Mode=OneWay}" FontWeight="SemiBold"/>
            </TextBlock>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Dunnage/Views/View_Dunnage_AdminMainView.xaml

```xml
<Page
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_AdminMainView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:local="using:MTM_Receiving_Application.Module_Dunnage.Views"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Grid Padding="24">
        <!-- Main Navigation Hub (4-card view) -->
        <Grid Visibility="{x:Bind ViewModel.IsMainNavigationVisible, Mode=OneWay}">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <!-- Header -->
            <StackPanel Grid.Row="0" Margin="0,0,0,24">
                <TextBlock 
                    Text="Dunnage Management" 
                    Style="{StaticResource TitleTextBlockStyle}"/>
                <TextBlock 
                    Text="Manage types, parts, specifications, and inventory" 
                    Style="{StaticResource BodyTextBlockStyle}"
                    Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                    Margin="0,8,0,0"/>
            </StackPanel>

            <!-- 4 Navigation Cards (2x2 Grid) -->
            <Grid Grid.Row="1" VerticalAlignment="Center">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <!-- Type Management Card -->
                <Button 
                    Grid.Row="0" 
                    Grid.Column="0"
                    Command="{x:Bind ViewModel.NavigateToManageTypesCommand}"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Margin="0,0,12,12"
                    Padding="24"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8">
                    <StackPanel Spacing="12">
                        <FontIcon 
                            Glyph="&#xE7C3;" 
                            FontSize="48"
                            Foreground="{ThemeResource SystemAccentColor}"/>
                        <TextBlock 
                            Text="Manage Types" 
                            Style="{StaticResource SubtitleTextBlockStyle}"
                            HorizontalAlignment="Center"/>
                        <TextBlock 
                            Text="Add, edit, and delete dunnage types"
                            Style="{StaticResource CaptionTextBlockStyle}"
                            Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                            TextWrapping="Wrap"
                            HorizontalAlignment="Center"
                            TextAlignment="Center"/>
                    </StackPanel>
                </Button>

                <!-- Spec Management Card -->
                <Button 
                    Grid.Row="0" 
                    Grid.Column="1"
                    Command="{x:Bind ViewModel.NavigateToManageSpecsCommand}"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Margin="12,0,0,12"
                    Padding="24"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8">
                    <StackPanel Spacing="12">
                        <FontIcon 
                            Glyph="&#xE8FD;" 
                            FontSize="48"
                            Foreground="{ThemeResource SystemAccentColor}"/>
                        <TextBlock 
                            Text="Manage Specs" 
                            Style="{StaticResource SubtitleTextBlockStyle}"
                            HorizontalAlignment="Center"/>
                        <TextBlock 
                            Text="Define specifications for each type"
                            Style="{StaticResource CaptionTextBlockStyle}"
                            Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                            TextWrapping="Wrap"
                            HorizontalAlignment="Center"
                            TextAlignment="Center"/>
                    </StackPanel>
                </Button>

                <!-- Part Management Card -->
                <Button 
                    Grid.Row="1" 
                    Grid.Column="0"
                    Command="{x:Bind ViewModel.NavigateToManagePartsCommand}"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Margin="0,12,12,0"
                    Padding="24"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8">
                    <StackPanel Spacing="12">
                        <FontIcon 
                            Glyph="&#xE8F1;" 
                            FontSize="48"
                            Foreground="{ThemeResource SystemAccentColor}"/>
                        <TextBlock 
                            Text="Manage Parts" 
                            Style="{StaticResource SubtitleTextBlockStyle}"
                            HorizontalAlignment="Center"/>
                        <TextBlock 
                            Text="Add and edit dunnage part numbers"
                            Style="{StaticResource CaptionTextBlockStyle}"
                            Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                            TextWrapping="Wrap"
                            HorizontalAlignment="Center"
                            TextAlignment="Center"/>
                    </StackPanel>
                </Button>

                <!-- Inventoried List Card -->
                <Button 
                    Grid.Row="1" 
                    Grid.Column="1"
                    Command="{x:Bind ViewModel.NavigateToInventoriedListCommand}"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Margin="12,12,0,0"
                    Padding="24"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8">
                    <StackPanel Spacing="12">
                        <FontIcon 
                            Glyph="&#xE8EF;" 
                            FontSize="48"
                            Foreground="{ThemeResource SystemAccentColor}"/>
                        <TextBlock 
                            Text="Inventoried List" 
                            Style="{StaticResource SubtitleTextBlockStyle}"
                            HorizontalAlignment="Center"/>
                        <TextBlock 
                            Text="Manage inventory tracking settings"
                            Style="{StaticResource CaptionTextBlockStyle}"
                            Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                            TextWrapping="Wrap"
                            HorizontalAlignment="Center"
                            TextAlignment="Center"/>
                    </StackPanel>
                </Button>
            </Grid>

            <!-- Status Bar -->
            <StackPanel Grid.Row="2" Orientation="Horizontal" Spacing="8" Margin="0,24,0,0">
                <ProgressRing 
                    IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                    Width="20" 
                    Height="20"/>
                <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
            </StackPanel>
        </Grid>

        <!-- Type Management Section -->
        <Grid Visibility="{x:Bind ViewModel.IsManageTypesVisible, Mode=OneWay}">
            <local:View_Dunnage_AdminTypesView/>
        </Grid>

        <!-- Spec Management Section (placeholder) -->
        <Grid Visibility="{x:Bind ViewModel.IsManageSpecsVisible, Mode=OneWay}">
            <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="16">
                <FontIcon Glyph="&#xE8FD;" FontSize="72" Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                <TextBlock 
                    Text="Spec Management" 
                    Style="{StaticResource TitleTextBlockStyle}"
                    HorizontalAlignment="Center"/>
                <TextBlock 
                    Text="This view will be implemented in a future phase"
                    Style="{StaticResource BodyTextBlockStyle}"
                    Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                    HorizontalAlignment="Center"/>
                <Button 
                    Content="Back to Main Menu"
                    Command="{x:Bind ViewModel.ReturnToMainNavigationCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    HorizontalAlignment="Center"
                    Margin="0,16,0,0"/>
            </StackPanel>
        </Grid>

        <!-- Part Management Section -->
        <Grid Visibility="{x:Bind ViewModel.IsManagePartsVisible, Mode=OneWay}">
            <local:View_Dunnage_AdminPartsView/>
        </Grid>

        <!-- Inventoried List Section (placeholder) -->
        <Grid Visibility="{x:Bind ViewModel.IsInventoriedListVisible, Mode=OneWay}">
            <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="16">
                <FontIcon Glyph="&#xE8EF;" FontSize="72" Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                <TextBlock 
                    Text="Inventoried List" 
                    Style="{StaticResource TitleTextBlockStyle}"
                    HorizontalAlignment="Center"/>
                <TextBlock 
                    Text="This view will be implemented in a future phase"
                    Style="{StaticResource BodyTextBlockStyle}"
                    Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                    HorizontalAlignment="Center"/>
                <Button 
                    Content="Back to Main Menu"
                    Command="{x:Bind ViewModel.ReturnToMainNavigationCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    HorizontalAlignment="Center"
                    Margin="0,16,0,0"/>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Dunnage/Views/View_Dunnage_AdminMainView.xaml.cs

```csharp
public sealed partial class View_Dunnage_AdminMainView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
```

## File: Module_Dunnage/Views/View_Dunnage_AdminPartsView.xaml

```xml
<Page
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_AdminPartsView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}"
    Loaded="OnPageLoaded">

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,16">
            <TextBlock 
                Text="Manage Dunnage Parts" 
                Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock 
                Text="Add, edit, and search dunnage parts with spec values" 
                Style="{StaticResource BodyTextBlockStyle}"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="0,8,0,0"/>
        </StackPanel>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,0,0,16">
            <AppBarButton 
                Icon="Add" 
                Label="Add New Part"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.CreatePart'), Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Edit" 
                Label="Edit Part"
                IsEnabled="{x:Bind ViewModel.CanEdit, Mode=OneWay}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.EditPart'), Mode=OneWay}"/>
            <AppBarButton 
                Icon="Delete" 
                Label="Delete Part"
                Command="{x:Bind ViewModel.ShowDeleteConfirmationCommand}"
                IsEnabled="{x:Bind ViewModel.CanDelete, Mode=OneWay}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.DeletePart'), Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Back" 
                Label="Back to Admin Hub"
                Command="{x:Bind ViewModel.ReturnToAdminHubCommand}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ReturnToAdmin'), Mode=OneWay}"/>
        </CommandBar>

        <!-- Search and Filter Bar -->
        <Grid Grid.Row="2" Margin="0,0,0,16">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Search Box -->
            <TextBox 
                Grid.Column="0"
                Text="{x:Bind ViewModel.SearchText, Mode=TwoWay}"
                PlaceholderText="Search by Part ID..."
                Margin="0,0,12,0">
                <TextBox.KeyboardAccelerators>
                    <KeyboardAccelerator Key="Enter" Invoked="OnSearchKeyboardAccelerator"/>
                </TextBox.KeyboardAccelerators>
            </TextBox>

            <!-- Type Filter -->
            <ComboBox 
                Grid.Column="1"
                ItemsSource="{x:Bind ViewModel.AvailableTypes, Mode=OneWay}"
                SelectedItem="{x:Bind ViewModel.SelectedFilterType, Mode=TwoWay}"
                PlaceholderText="Filter by Type"
                DisplayMemberPath="DunnageType"
                MinWidth="200"
                Margin="0,0,12,0"/>

            <!-- Filter Buttons -->
            <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="8">
                <Button 
                    Content="Search"
                    Command="{x:Bind ViewModel.SearchPartsCommand}"
                    Style="{StaticResource AccentButtonStyle}"/>
                <Button 
                    Content="Filter by Type"
                    Command="{x:Bind ViewModel.FilterByTypeCommand}"/>
                <Button 
                    Content="Clear"
                    Command="{x:Bind ViewModel.ClearFiltersCommand}"/>
            </StackPanel>
        </Grid>

        <!-- DataGrid -->
        <Border 
            Grid.Row="3" 
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="1">
            <Grid>
                <controls:DataGrid
                    ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
                    SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                    AutoGenerateColumns="False"
                    IsReadOnly="True"
                    GridLinesVisibility="Horizontal"
                    HeadersVisibility="Column"
                    SelectionMode="Single"
                    CanUserReorderColumns="False"
                    CanUserSortColumns="True"
                    VerticalScrollBarVisibility="Auto"
                    HorizontalScrollBarVisibility="Auto"
                    AlternatingRowBackground="{ThemeResource SubtleFillColorSecondaryBrush}">

                    <controls:DataGrid.Columns>
                        <!-- Part ID Column -->
                        <controls:DataGridTextColumn 
                            Header="Part ID" 
                            Binding="{Binding PartId}"
                            Width="2*"
                            CanUserResize="True"/>

                        <!-- Type Column -->
                        <controls:DataGridTextColumn 
                            Header="Type" 
                            Binding="{Binding DunnageTypeName}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Spec Values Column (Note: Dynamic spec columns would require code-behind) -->
                        <controls:DataGridTextColumn 
                            Header="Specifications" 
                            Binding="{Binding DunnageSpecValuesJson}"
                            Width="2*"
                            CanUserResize="True"/>

                        <!-- Date Added Column -->
                        <controls:DataGridTextColumn 
                            Header="Date Added" 
                            Binding="{Binding CreatedDate, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Added By Column -->
                        <controls:DataGridTextColumn 
                            Header="Added By" 
                            Binding="{Binding CreatedBy}"
                            Width="*"
                            CanUserResize="True"/>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>

                <!-- Empty State -->
                <StackPanel 
                    VerticalAlignment="Center" 
                    HorizontalAlignment="Center"
                    Spacing="16"
                    Visibility="{x:Bind ViewModel.Parts.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=Inverse}">
                    <FontIcon 
                        Glyph="&#xE8F1;" 
                        FontSize="72"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="No parts found" 
                        Style="{StaticResource SubtitleTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="Try adjusting your filters or add a new part"
                        Style="{StaticResource CaptionTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Pagination Controls -->
        <Grid Grid.Row="4" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Record Count -->
            <TextBlock Grid.Column="0" VerticalAlignment="Center">
                <Run Text="Showing"/>
                <Run Text="{x:Bind ViewModel.Parts.Count, Mode=OneWay}" FontWeight="SemiBold"/>
                <Run Text="of"/>
                <Run Text="{x:Bind ViewModel.TotalRecords, Mode=OneWay}" FontWeight="SemiBold"/>
                <Run Text="parts"/>
            </TextBlock>

            <!-- Pagination Buttons -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
                <Button 
                    Content="First"
                    Command="{x:Bind ViewModel.FirstPageCommand}"
                    IsEnabled="{x:Bind ViewModel.CanNavigatePrevious, Mode=OneWay}"
                    MinWidth="80"/>
                <Button 
                    Content="Previous"
                    Command="{x:Bind ViewModel.PreviousPageCommand}"
                    IsEnabled="{x:Bind ViewModel.CanNavigatePrevious, Mode=OneWay}"
                    MinWidth="80"/>
                <TextBlock 
                    VerticalAlignment="Center"
                    Margin="12,0,12,0">
                    <Run Text="Page"/>
                    <Run Text="{x:Bind ViewModel.CurrentPage, Mode=OneWay}" FontWeight="SemiBold"/>
                    <Run Text="of"/>
                    <Run Text="{x:Bind ViewModel.TotalPages, Mode=OneWay}" FontWeight="SemiBold"/>
                </TextBlock>
                <Button 
                    Content="Next"
                    Command="{x:Bind ViewModel.NextPageCommand}"
                    IsEnabled="{x:Bind ViewModel.CanNavigateNext, Mode=OneWay}"
                    MinWidth="80"/>
                <Button 
                    Content="Last"
                    Command="{x:Bind ViewModel.LastPageCommand}"
                    IsEnabled="{x:Bind ViewModel.CanNavigateNext, Mode=OneWay}"
                    MinWidth="80"/>
            </StackPanel>
        </Grid>

        <!-- Status Bar -->
        <StackPanel Grid.Row="5" Orientation="Horizontal" Spacing="8" Margin="0,16,0,0">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Dunnage/Views/View_Dunnage_AdminPartsView.xaml.cs

```csharp
public sealed partial class View_Dunnage_AdminPartsView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.LoadPartsCommand.ExecuteAsync(null);
⋮----
private async void OnSearchKeyboardAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
⋮----
await ViewModel.SearchPartsCommand.ExecuteAsync(null);
```

## File: Module_Dunnage/Views/View_Dunnage_AdminTypesView.xaml

```xml
<Page
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_AdminTypesView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}"
    Loaded="OnPageLoaded">

    <Page.Resources>
        <converters:Converter_IconCodeToGlyph x:Key="IconCodeToGlyphConverter"/>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
        <converters:Converter_StringFormat x:Key="DateTimeToStringConverter"/>
    </Page.Resources>

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,16">
            <TextBlock 
                Text="Manage Dunnage Types" 
                Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock 
                Text="Add, edit, and delete dunnage types with impact analysis" 
                Style="{StaticResource BodyTextBlockStyle}"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="0,8,0,0"/>
        </StackPanel>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,0,0,16">
            <AppBarButton 
                Icon="Add" 
                Label="Add New Type"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.CreateType'), Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Edit" 
                Label="Edit Type"
                Command="{x:Bind ViewModel.ShowEditTypeCommand}"
                IsEnabled="{x:Bind ViewModel.CanEdit, Mode=OneWay}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.EditType'), Mode=OneWay}"/>
            <AppBarButton 
                Icon="Delete" 
                Label="Delete Type"
                Command="{x:Bind ViewModel.ShowDeleteConfirmationCommand}"
                IsEnabled="{x:Bind ViewModel.CanDelete, Mode=OneWay}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.DeleteType'), Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Back" 
                Label="Back to Admin Hub"
                Command="{x:Bind ViewModel.ReturnToAdminHubCommand}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ReturnToAdmin'), Mode=OneWay}"/>
        </CommandBar>

        <!-- DataGrid -->
        <Border 
            Grid.Row="2" 
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="1">
            <Grid>
                <controls:DataGrid
                    ItemsSource="{x:Bind ViewModel.Types, Mode=OneWay}"
                    SelectedItem="{x:Bind ViewModel.SelectedType, Mode=TwoWay}"
                    AutoGenerateColumns="False"
                    IsReadOnly="True"
                    GridLinesVisibility="Horizontal"
                    HeadersVisibility="Column"
                    SelectionMode="Single"
                    CanUserReorderColumns="False"
                    CanUserSortColumns="True"
                    VerticalScrollBarVisibility="Auto"
                    HorizontalScrollBarVisibility="Auto"
                    AlternatingRowBackground="{ThemeResource SubtleFillColorSecondaryBrush}">

                    <controls:DataGrid.Columns>
                        <!-- Icon Column -->
                        <controls:DataGridTemplateColumn Header="Icon" Width="60">
                            <controls:DataGridTemplateColumn.CellTemplate>
                                <DataTemplate>
                                    <materialIcons:MaterialIcon 
                                        Kind="{Binding IconKind}" 
                                        Width="24" 
                                        Height="24"
                                        Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                                        HorizontalAlignment="Center"
                                        VerticalAlignment="Center"/>
                                </DataTemplate>
                            </controls:DataGridTemplateColumn.CellTemplate>
                        </controls:DataGridTemplateColumn>

                        <!-- Type Name Column -->
                        <controls:DataGridTextColumn 
                            Header="Type Name" 
                            Binding="{Binding DunnageType}"
                            Width="2*"
                            CanUserResize="True"/>

                        <!-- Date Added Column -->
                        <controls:DataGridTextColumn 
                            Header="Date Added" 
                            Binding="{Binding DateAdded, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Added By Column -->
                        <controls:DataGridTextColumn 
                            Header="Added By" 
                            Binding="{Binding AddedBy}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Last Modified Column -->
                        <controls:DataGridTextColumn 
                            Header="Last Modified" 
                            Binding="{Binding LastModified, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Modified By Column -->
                        <controls:DataGridTextColumn 
                            Header="Modified By" 
                            Binding="{Binding ModifiedBy}"
                            Width="*"
                            CanUserResize="True"/>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>

                <!-- Empty State -->
                <StackPanel 
                    VerticalAlignment="Center" 
                    HorizontalAlignment="Center"
                    Spacing="16"
                    Visibility="{x:Bind ViewModel.Types.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=Inverse}">
                    <FontIcon 
                        Glyph="&#xE7C3;" 
                        FontSize="72"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="No types found" 
                        Style="{StaticResource SubtitleTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="Click 'Add New Type' to create your first dunnage type"
                        Style="{StaticResource CaptionTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="8" Margin="0,16,0,0">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
            <TextBlock 
                Text="|"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="8,0,8,0"/>
            <TextBlock>
                <Run Text="Total Types:"/>
                <Run Text="{x:Bind ViewModel.Types.Count, Mode=OneWay}" FontWeight="SemiBold"/>
            </TextBlock>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Dunnage/Views/View_Dunnage_Control_IconPickerControl.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_Control_IconPickerControl"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    xmlns:icons="using:Material.Icons"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter" />
        
        <Style x:Key="IconGridViewItemStyle" TargetType="GridViewItem">
            <Setter Property="HorizontalContentAlignment" Value="Stretch" />
            <Setter Property="VerticalContentAlignment" Value="Stretch" />
            <Setter Property="Margin" Value="2" />
            <Setter Property="Padding" Value="0" />
            <Setter Property="CornerRadius" Value="4" />
            <Setter Property="MinWidth" Value="40" />
            <Setter Property="MinHeight" Value="40" />
        </Style>

        <DataTemplate x:Key="IconTemplate" x:DataType="icons:MaterialIconKind">
            <Border Width="40" Height="40" 
                    BorderThickness="1" 
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" 
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    CornerRadius="4" 
                    Padding="4" 
                    ToolTipService.ToolTip="{x:Bind ToString()}">
                <materialIcons:MaterialIcon Kind="{x:Bind}" Width="24" Height="24" Foreground="{ThemeResource AccentFillColorDefaultBrush}" />
            </Border>
        </DataTemplate>
        
        <DataTemplate x:Key="RecentIconTemplate" x:DataType="models:Model_IconDefinition">
             <Border Width="40" Height="40" 
                    BorderThickness="1" 
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" 
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    CornerRadius="4" 
                    Padding="4" 
                    ToolTipService.ToolTip="{x:Bind Name}">
                <materialIcons:MaterialIcon Kind="{x:Bind Kind}" Width="24" Height="24" Foreground="{ThemeResource AccentFillColorDefaultBrush}" />
            </Border>
        </DataTemplate>
    </UserControl.Resources>

    <StackPanel Spacing="12">

        <!-- Recently Used Section -->
        <StackPanel Spacing="8" Visibility="{x:Bind RecentlyUsedIcons.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}}">
            <TextBlock Text="Recently Used" FontWeight="SemiBold" FontSize="12" />
            <GridView
                x:Name="RecentIconsGrid"
                SelectionMode="Single"
                MaxHeight="60"
                ItemsSource="{x:Bind RecentlyUsedIcons, Mode=OneWay}"
                ItemTemplate="{StaticResource RecentIconTemplate}"
                ItemContainerStyle="{StaticResource IconGridViewItemStyle}"
                SelectionChanged="OnRecentIconSelectionChanged">
                <GridView.ItemsPanel>
                    <ItemsPanelTemplate>
                        <ItemsWrapGrid Orientation="Horizontal" MaximumRowsOrColumns="6" />
                    </ItemsPanelTemplate>
                </GridView.ItemsPanel>
            </GridView>
        </StackPanel>

        <!-- Search Box -->
        <TextBox
            x:Name="SearchBox"
            PlaceholderText="Search icons..."
            TextChanged="OnSearchTextChanged" />

        <!-- All Icons Grid -->
        <GridView
            x:Name="AllIconsGrid"
            SelectionMode="Single"
            SelectedItem="{x:Bind SelectedIcon, Mode=TwoWay}"
            ItemTemplate="{StaticResource IconTemplate}"
            ItemContainerStyle="{StaticResource IconGridViewItemStyle}"
            MaxHeight="300">
            <GridView.ItemsPanel>
                <ItemsPanelTemplate>
                    <ItemsWrapGrid Orientation="Horizontal" MaximumRowsOrColumns="6" />
                </ItemsPanelTemplate>
            </GridView.ItemsPanel>
        </GridView>
    </StackPanel>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_DetailsEntryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:DataType="views:View_Dunnage_DetailsEntryView"
    Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="8" Margin="0,0,0,16">
            <TextBlock Text="Enter Details"
                       Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock Text="{x:Bind ViewModel.GetTip('Dunnage.DetailsEntry'), Mode=OneWay}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       Style="{StaticResource BodyTextBlockStyle}"/>
        </StackPanel>

        <!-- Inventory Notification -->
        <InfoBar Grid.Row="1"
                 IsOpen="{x:Bind ViewModel.IsInventoryNotificationVisible, Mode=OneWay}"
                 Severity="Informational"
                 Title="Inventory Action Required"
                 Message="{x:Bind ViewModel.InventoryNotificationMessage, Mode=OneWay}"
                 IsClosable="False"
                 Margin="0,0,0,12"/>

        <!-- Details Form -->
        <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
            <StackPanel Spacing="12">
                
                <!-- PO Number and Location Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <Grid ColumnSpacing="12">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <!-- PO Number -->
                        <StackPanel Grid.Column="0" Spacing="6">
                            <TextBlock Text="PO Number"
                                       Style="{StaticResource SubtitleTextBlockStyle}"/>
                            <TextBox x:Name="PoNumberTextBox"
                                     Text="{x:Bind ViewModel.PoNumber, Mode=TwoWay}"
                                     PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PONumber'), Mode=OneWay}"
                                     HorizontalAlignment="Stretch"/>
                            <TextBlock Text="Leave blank for non-PO dunnage"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                       Style="{StaticResource CaptionTextBlockStyle}"/>
                        </StackPanel>

                        <!-- Location -->
                        <StackPanel Grid.Column="1" Spacing="6">
                            <TextBlock Text="Location"
                                       Style="{StaticResource SubtitleTextBlockStyle}"/>
                            <TextBox Text="{x:Bind ViewModel.Location, Mode=TwoWay}"
                                     PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.Location'), Mode=OneWay}"
                                     HorizontalAlignment="Stretch"/>
                            <TextBlock Text="Where this dunnage is located"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                       Style="{StaticResource CaptionTextBlockStyle}"/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Specifications Card (Read-Only) -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <StackPanel Spacing="8">
                        <TextBlock Text="Specifications (from selected part)"
                                   Style="{StaticResource SubtitleTextBlockStyle}"/>
                        
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <!-- Column 1: Text Specs -->
                            <StackPanel Grid.Column="0" Spacing="4" 
                                        Visibility="{x:Bind ViewModel.HasTextSpecs, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                                <ItemsRepeater ItemsSource="{x:Bind ViewModel.TextSpecs, Mode=OneWay}">
                                    <ItemsRepeater.ItemTemplate>
                                        <DataTemplate>
                                            <StackPanel Spacing="2" Margin="0,0,0,6">
                                                <TextBlock Text="{Binding SpecName}" FontWeight="SemiBold" FontSize="12"/>
                                                <TextBox Text="{Binding Value, Mode=OneWay}" 
                                                         IsReadOnly="True"
                                                         Width="180"/>
                                            </StackPanel>
                                        </DataTemplate>
                                    </ItemsRepeater.ItemTemplate>
                                </ItemsRepeater>
                            </StackPanel>
                            
                            <!-- Column 2: Number Specs -->
                            <StackPanel Grid.Column="1" Spacing="4"
                                        Visibility="{x:Bind ViewModel.HasNumberSpecs, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                                <ItemsRepeater ItemsSource="{x:Bind ViewModel.NumberSpecs, Mode=OneWay}">
                                    <ItemsRepeater.ItemTemplate>
                                        <DataTemplate>
                                            <StackPanel Spacing="2" Margin="0,0,0,6">
                                                <TextBlock Text="{Binding SpecName}" FontWeight="SemiBold" FontSize="12"/>
                                                <TextBox Text="{Binding Value, Mode=OneWay}" 
                                                         IsReadOnly="True"
                                                         Width="180"/>
                                            </StackPanel>
                                        </DataTemplate>
                                    </ItemsRepeater.ItemTemplate>
                                </ItemsRepeater>
                            </StackPanel>
                            
                            <!-- Column 3: Boolean Specs -->
                            <StackPanel Grid.Column="2" Spacing="4"
                                        Visibility="{x:Bind ViewModel.HasBooleanSpecs, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                                <ItemsRepeater ItemsSource="{x:Bind ViewModel.BooleanSpecs, Mode=OneWay}">
                                    <ItemsRepeater.ItemTemplate>
                                        <DataTemplate>
                                            <CheckBox Content="{Binding SpecName}" 
                                                      IsChecked="{Binding Value, Mode=OneWay}" 
                                                      IsEnabled="False"
                                                      Margin="0,0,0,6"/>
                                        </DataTemplate>
                                    </ItemsRepeater.ItemTemplate>
                                </ItemsRepeater>
                            </StackPanel>
                        </Grid>

                        <!-- Placeholder when no specs -->
                        <TextBlock Text="No specifications configured for this type."
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                   Visibility="{x:Bind ViewModel.SpecInputs.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter='Inverse'}"
                                   Style="{StaticResource BodyTextBlockStyle}"/>
                    </StackPanel>
                </Border>

                <!-- Info Panel Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <StackPanel Spacing="6">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE946;" FontSize="16" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Note" FontWeight="SemiBold"/>
                        </StackPanel>
                        <TextBlock TextWrapping="Wrap" FontSize="12"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                            <Run Text="• PO Number: Use if receiving against a purchase order"/>
                            <LineBreak/>
                            <Run Text="• Location: Physical location of the dunnage"/>
                            <LineBreak/>
                            <Run Text="• Specifications: Pre-filled from the selected part"/>
                        </TextBlock>
                    </StackPanel>
                </Border>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml.cs

```csharp
public sealed partial class View_Dunnage_DetailsEntryView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
⋮----
private async void OnLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.LoadSpecsForSelectedPartAsync();
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_AddMultipleRowsDialog.xaml

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_Dialog_AddMultipleRowsDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="Add Multiple Rows"
    PrimaryButtonText="Add"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="ContentDialog_PrimaryButtonClick">

    <StackPanel Spacing="16" Padding="24">
        <TextBlock 
            Text="How many rows would you like to add?" 
            Style="{StaticResource BodyTextBlockStyle}"/>
        
        <NumberBox 
            x:Name="RowCountNumberBox"
            Header="Number of Rows"
            Minimum="1"
            Maximum="100"
            Value="10"
            SpinButtonPlacementMode="Inline"
            ValidationMode="InvalidInputOverwritten"
            AcceptsExpression="False"/>
        
        <InfoBar 
            IsOpen="True"
            Severity="Informational"
            IsClosable="False"
            Message="Maximum 100 rows can be added at once. Each row will be initialized with default values (Quantity = 1)."/>
    </StackPanel>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_AddMultipleRowsDialog.xaml.cs

```csharp
public sealed partial class View_Dunnage_Dialog_AddMultipleRowsDialog : ContentDialog
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this, RowCountNumberBox);
⋮----
private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_AddToInventoriedListDialog.xaml

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_Dialog_AddToInventoriedListDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="Add Part to Inventoried List"
    PrimaryButtonText="Add to List"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="OnPrimaryButtonClick">

    <ScrollViewer MaxHeight="500">
        <StackPanel Spacing="12" Padding="24">
            
            <!-- Part ID Selection -->
            <TextBlock Text="Part ID" FontWeight="SemiBold"/>
            <ComboBox x:Name="PartIdComboBox"
                      PlaceholderText="Select a part..."
                      HorizontalAlignment="Stretch"
                      IsEditable="True"
                      TextSubmitted="PartIdComboBox_TextSubmitted"/>
            <TextBlock x:Name="PartIdError"
                       Text="Part ID is required"
                       Foreground="{ThemeResource SystemErrorTextColor}"
                       Visibility="Collapsed"
                       Margin="0,-8,0,0"/>

            <!-- Inventory Method -->
            <TextBlock Text="Inventory Method" FontWeight="SemiBold" Margin="0,12,0,0"/>
            <ComboBox x:Name="InventoryMethodComboBox"
                      HorizontalAlignment="Stretch"
                      SelectedIndex="2">
                <ComboBoxItem Content="Adjust In"/>
                <ComboBoxItem Content="Receive In"/>
                <ComboBoxItem Content="Both"/>
            </ComboBox>
            <TextBlock Text="Adjust In: Only triggers during inventory adjustments"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource SystemControlDisabledBaseMediumLowBrush}"
                       TextWrapping="Wrap"
                       Margin="0,-8,0,0"/>
            <TextBlock Text="Receive In: Only triggers during receiving transactions"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource SystemControlDisabledBaseMediumLowBrush}"
                       TextWrapping="Wrap"/>
            <TextBlock Text="Both: Triggers for both adjust and receive operations"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource SystemControlDisabledBaseMediumLowBrush}"
                       TextWrapping="Wrap"/>

            <!-- Notes -->
            <TextBlock Text="Notes (optional)" FontWeight="SemiBold" Margin="0,12,0,0"/>
            <TextBox x:Name="NotesTextBox"
                     PlaceholderText="Add any notes or special instructions..."
                     AcceptsReturn="True"
                     TextWrapping="Wrap"
                     Height="80"/>

        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_AddToInventoriedListDialog.xaml.cs

```csharp
public sealed partial class View_Dunnage_Dialog_AddToInventoriedListDialog : ContentDialog
⋮----
private readonly Dao_DunnagePart _daoPart;
private readonly Dao_InventoriedDunnage _daoInventory;
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async Task LoadPartsAsync()
⋮----
var result = await _daoPart.GetAllAsync();
⋮----
PartIdComboBox.ItemsSource = result.Data.ConvertAll(p => p.PartId);
⋮----
private void PartIdComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
⋮----
if (!string.IsNullOrWhiteSpace(args.Text))
⋮----
private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
var deferral = args.GetDeferral();
⋮----
if (string.IsNullOrWhiteSpace(partId))
⋮----
var checkResult = await _daoInventory.GetByPartAsync(partId);
⋮----
var errorDialog = new ContentDialog
⋮----
await errorDialog.ShowAsync();
⋮----
var insertResult = await _daoInventory.InsertAsync(
⋮----
deferral.Complete();
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_Dunnage_AddTypeDialog.xaml

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_Dialog_Dunnage_AddTypeDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="using:MTM_Receiving_Application.Module_Dunnage.Views.Controls"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    x:Name="Root"
    Title="Add New Dunnage Type"
    PrimaryButtonText="Save Type"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    MaxHeight="750"
    PrimaryButtonCommand="{x:Bind ViewModel.SaveTypeCommand}"
    IsPrimaryButtonEnabled="{x:Bind ViewModel.CanSave, Mode=OneWay}">

    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Spacing="16" Padding="24">

            <!-- Duplicate Warning InfoBar -->
            <InfoBar
                Severity="Warning"
                IsOpen="{x:Bind ViewModel.ShowDuplicateWarning, Mode=OneWay}"
                Title="Duplicate Type Name"
                Message="A type with this name already exists.">
                <InfoBar.ActionButton>
                    <HyperlinkButton Content="View Existing Type" />
                </InfoBar.ActionButton>
            </InfoBar>

            <!-- Field Limit Warning InfoBar -->
            <InfoBar
                Severity="Informational"
                IsOpen="{x:Bind ViewModel.ShowFieldLimitWarning, Mode=OneWay}"
                Title="Many Custom Fields"
                Message="You have added 10+ custom fields. Consider grouping similar fields for better usability." />

            <!-- Basic Information Section -->
            <StackPanel Spacing="12">
                <TextBlock Text="Basic Information" Style="{StaticResource SubtitleTextBlockStyle}" />

                <!-- Type Name -->
                <TextBox
                    Header="Type Name"
                    PlaceholderText="Enter dunnage type name (e.g., Pallet - 48x40)"
                    Text="{x:Bind ViewModel.TypeName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                    BorderBrush="{x:Bind ViewModel.TypeNameError, Mode=OneWay, Converter={StaticResource ErrorToBrushConverter}}"
                    MaxLength="100" />

                <TextBlock
                    Text="{x:Bind ViewModel.TypeNameError, Mode=OneWay}"
                    Foreground="Red"
                    Visibility="{x:Bind ViewModel.TypeNameError, Mode=OneWay, Converter={StaticResource StringToVisibilityConverter}}"
                    Margin="0,-8,0,0" />

                <!-- Icon Picker Button -->
                <TextBlock Text="Icon" Margin="0,8,0,0" FontWeight="SemiBold" />
                <Button x:Name="SelectIconButton"
                        Click="OnSelectIconClick"
                        HorizontalAlignment="Stretch"
                        Padding="16,12"
                        ToolTipService.ToolTip="Click to browse and select an icon from the icon library">
                    <StackPanel Orientation="Horizontal" Spacing="12">
                        <materialIcons:MaterialIcon 
                            Kind="{x:Bind ViewModel.SelectedIcon, Mode=OneWay}" 
                            Width="32" 
                            Height="32"
                            Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock Text="Click to select icon" FontWeight="SemiBold"/>
                            <TextBlock 
                                Text="{x:Bind ViewModel.SelectedIcon, Mode=OneWay}"
                                FontSize="12"
                                Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </StackPanel>
                </Button>
            </StackPanel>

            <!-- Custom Specifications Section -->
            <StackPanel Spacing="12" Margin="0,16,0,0">
                <TextBlock Text="Custom Specifications" Style="{StaticResource SubtitleTextBlockStyle}" />

                <!-- Add New Field Form -->
                <Border
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="16"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
                    <StackPanel Spacing="12">
                        <TextBlock Text="Add Custom Field" FontWeight="SemiBold" />

                        <TextBox
                            Header="Field Name"
                            PlaceholderText="Enter field name (e.g., Width, Material)"
                            Text="{x:Bind ViewModel.FieldName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                            BorderBrush="{x:Bind ViewModel.FieldNameError, Mode=OneWay, Converter={StaticResource ErrorToBrushConverter}}"
                            MaxLength="100" />

                        <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,-8,0,0">
                            <TextBlock
                                Text="{x:Bind ViewModel.FieldNameError, Mode=OneWay}"
                                Foreground="Red"
                                Visibility="{x:Bind ViewModel.FieldNameError, Mode=OneWay, Converter={StaticResource StringToVisibilityConverter}}" />
                            <TextBlock
                                Text="{x:Bind ViewModel.FieldCharacterCount, Mode=OneWay}"
                                Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                HorizontalAlignment="Right" />
                            <TextBlock Text="/100 characters" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                        </StackPanel>

                        <ComboBox
                            Header="Field Type"
                            SelectedItem="{x:Bind ViewModel.FieldType, Mode=TwoWay}"
                            HorizontalAlignment="Stretch">
                            <x:String>Text</x:String>
                            <x:String>Number</x:String>
                            <x:String>Dropdown</x:String>
                            <x:String>Yes/No</x:String>
                        </ComboBox>

                        <CheckBox
                            Content="Required field"
                            IsChecked="{x:Bind ViewModel.IsFieldRequired, Mode=TwoWay}" />

                        <Button
                            Content="{x:Bind ViewModel.EditingField, Mode=OneWay, Converter={StaticResource EditingFieldToTextConverter}}"
                            Command="{x:Bind ViewModel.AddFieldCommand}"
                            IsEnabled="{x:Bind ViewModel.CanAddField, Mode=OneWay}"
                            Style="{StaticResource AccentButtonStyle}"
                            HorizontalAlignment="Right" />
                    </StackPanel>
                </Border>

                <!-- Field Preview List -->
                <TextBlock
                    Text="Custom Fields Preview"
                    FontWeight="SemiBold"
                    Visibility="{x:Bind ViewModel.CustomFields.Count, Mode=OneWay, Converter={StaticResource CountToVisibilityConverter}}"
                    Margin="0,8,0,0" />

                <ItemsRepeater
                    ItemsSource="{x:Bind ViewModel.CustomFields, Mode=OneWay}">
                    <ItemsRepeater.ItemTemplate>
                        <DataTemplate x:DataType="models:Model_CustomFieldDefinition">
                            <Border
                                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                                BorderThickness="1"
                                CornerRadius="8"
                                Padding="12"
                                Margin="0,4"
                                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
                                <Grid ColumnSpacing="8">
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto" />
                                        <ColumnDefinition Width="*" />
                                        <ColumnDefinition Width="Auto" />
                                    </Grid.ColumnDefinitions>

                                    <!-- Drag Handle -->
                                    <FontIcon Grid.Column="0" Glyph="&#xE76F;" FontSize="16" VerticalAlignment="Center" />

                                    <!-- Field Info -->
                                    <StackPanel Grid.Column="1" Spacing="4">
                                        <TextBlock Text="{x:Bind FieldName}" FontWeight="SemiBold" />
                                        <TextBlock
                                            Text="{x:Bind GetSummary()}"
                                            Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                            FontSize="12" />
                                    </StackPanel>

                                    <!-- Edit/Delete Buttons -->
                                    <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="4">
                                        <Button
                                            Content="Edit"
                                            Command="{Binding EditFieldCommand}"
                                            CommandParameter="{x:Bind}"
                                            Style="{StaticResource TextBlockButtonStyle}" />
                                        <Button
                                            Content="Delete"
                                            Command="{Binding DeleteFieldCommand}"
                                            CommandParameter="{x:Bind}"
                                            Style="{StaticResource TextBlockButtonStyle}"
                                            Foreground="Red" />
                                    </StackPanel>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsRepeater.ItemTemplate>
                </ItemsRepeater>

                <TextBlock
                    Text="No custom fields added yet. Click 'Add Field' to create one."
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                    Visibility="{x:Bind ViewModel.CustomFields.Count, Mode=OneWay, Converter={StaticResource InverseCountToVisibilityConverter}}"
                    HorizontalAlignment="Center"
                    Margin="0,16" />
            </StackPanel>

        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_Dunnage_AddTypeDialog.xaml.cs

```csharp
public sealed partial class View_Dunnage_Dialog_Dunnage_AddTypeDialog : ContentDialog
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void OnSelectIconClick(object sender, RoutedEventArgs e)
⋮----
var iconSelector = new View_Shared_IconSelectorWindow();
iconSelector.SetInitialSelection(ViewModel.SelectedIcon);
iconSelector.Activate();
var selectedIcon = await iconSelector.WaitForSelectionAsync();
```

## File: Module_Dunnage/Views/View_Dunnage_EditModeView.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_EditModeView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
    xmlns:enums="using:MTM_Receiving_Application.Module_Core.Models.Enums"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:Name="Root">

    <UserControl.Resources>
        <converters:Converter_DecimalToString x:Key="DecimalToStringConverter"/>
    </UserControl.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Toolbar - T154: 3 data source buttons -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <!-- Main Toolbar Area -->
            <StackPanel Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <!-- Load Data Section - T154 -->
                <TextBlock Text="Load Data From:" VerticalAlignment="Center" FontWeight="SemiBold" Margin="0,0,4,0"/>
                
                <Button Command="{x:Bind ViewModel.LoadFromCurrentMemoryCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadSessionMemory'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE81D;"/>
                        <TextBlock Text="Current Memory"/>
                    </StackPanel>
                </Button>

                <Button Command="{x:Bind ViewModel.LoadFromCurrentLabelsCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadRecentCSV'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8E5;"/>
                        <TextBlock Text="Current Labels"/>
                    </StackPanel>
                </Button>
                
                <Button Command="{x:Bind ViewModel.LoadFromHistoryCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadHistoricalData'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74C;"/>
                        <TextBlock Text="History"/>
                    </StackPanel>
                </Button>

                <!-- Separator -->
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>

                <!-- Edit Actions Section -->
                <Button Command="{x:Bind ViewModel.SelectAllCommand}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8B3;"/>
                        <TextBlock Text="Select All"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.RemoveSelectedRowsCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.RemoveSelectedRows'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74D;"/>
                        <TextBlock Text="Remove Selected"/>
                    </StackPanel>
                </Button>
            </StackPanel>
        </Grid>

        <!-- Date Filter Toolbar - T164: Dynamic button text -->
        <Grid Grid.Row="1" Margin="0,0,0,12">
            <StackPanel Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <TextBlock Text="Filter Date:" VerticalAlignment="Center" FontWeight="SemiBold"/>
                
                <CalendarDatePicker Date="{x:Bind ViewModel.FromDate, Mode=TwoWay}" 
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.StartDate'), Mode=OneWay}"
                                   Width="140"/>
                <TextBlock Text="to" VerticalAlignment="Center"/>
                <CalendarDatePicker Date="{x:Bind ViewModel.ToDate, Mode=TwoWay}" 
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.EndDate'), Mode=OneWay}"
                                   Width="140"/>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <!-- T158, T159, T160, T161, T162, T163, T164: Date filter buttons with dynamic text -->
                <Button Content="{x:Bind ViewModel.LastWeekButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterLastWeekCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterLastWeek'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.TodayButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterTodayCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterToday'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisWeekButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterThisWeekCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisWeek'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisMonthButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterThisMonthCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisMonth'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisQuarterButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterThisQuarterCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisQuarter'), Mode=OneWay}"/>
                <Button Content="Show All" 
                        Command="{x:Bind ViewModel.SetFilterShowAllCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterShowAll'), Mode=OneWay}"/>
            </StackPanel>
        </Grid>

        <!-- Data Grid -->
        <controls:DataGrid Grid.Row="2"
                          x:Name="EditModeDataGrid"
                          ItemsSource="{x:Bind ViewModel.FilteredLoads, Mode=OneWay}"
                          AutoGenerateColumns="False"
                          IsReadOnly="False"
                          CanUserSortColumns="True"
                          GridLinesVisibility="All"
                          HeadersVisibility="Column"
                          SelectionMode="Single"
                          KeyDown="EditModeDataGrid_KeyDown"
                          CurrentCellChanged="EditModeDataGrid_CurrentCellChanged"
                          Tapped="EditModeDataGrid_Tapped">
            <controls:DataGrid.Columns>
                <controls:DataGridTemplateColumn Header="" Width="50" CanUserResize="False">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <CheckBox IsChecked="{Binding IsSelected, Mode=TwoWay}" 
                                      HorizontalAlignment="Center" 
                                      VerticalAlignment="Center"
                                      MinWidth="0"
                                      Margin="0"/>
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>
                <controls:DataGridTextColumn Header="Load #" Binding="{Binding LoadNumber}" IsReadOnly="True" Width="Auto"/>
                <controls:DataGridTextColumn Header="Type" Binding="{Binding TypeName}" Width="*"/>
                <controls:DataGridTextColumn Header="Part ID" Binding="{Binding PartId}" Width="*"/>
                <controls:DataGridTextColumn Header="Quantity" Binding="{Binding Quantity}" Width="Auto"/>
                <controls:DataGridTextColumn Header="PO Number" Binding="{Binding PoNumber}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Location" Binding="{Binding Location}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Created Date" Binding="{Binding CreatedDate}" IsReadOnly="True" Width="Auto"/>
                <controls:DataGridTextColumn Header="Created By" Binding="{Binding CreatedBy}" IsReadOnly="True" Width="Auto"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Footer: Pagination and Save -->
        <Grid Grid.Row="3" Margin="0,12,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Pagination Controls -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
                <Button Command="{x:Bind ViewModel.FirstPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FirstPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE892;"/>
                </Button>
                <Button Command="{x:Bind ViewModel.PreviousPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.PreviousPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE76B;"/>
                </Button>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <TextBlock Text="Page" VerticalAlignment="Center"/>
                <TextBlock Text="{x:Bind ViewModel.CurrentPage, Mode=OneWay}" VerticalAlignment="Center" FontWeight="SemiBold"/>
                <TextBlock Text="of" VerticalAlignment="Center"/>
                <TextBlock Text="{x:Bind ViewModel.TotalPages, Mode=OneWay}" VerticalAlignment="Center" FontWeight="SemiBold"/>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <Button Command="{x:Bind ViewModel.NextPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.NextPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE76C;"/>
                </Button>
                <Button Command="{x:Bind ViewModel.LastPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LastPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE893;"/>
                </Button>
            </StackPanel>

            <!-- Save Button -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="12">
                <Button Content="Save Changes" Command="{x:Bind ViewModel.SaveAllCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveChanges'), Mode=OneWay}"/>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_EditModeView.xaml.cs

```csharp
public sealed partial class View_Dunnage_EditModeView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
_focusService.AttachFocusOnVisibility(this);
⋮----
private void EditModeDataGrid_CurrentCellChanged(object? sender, EventArgs e)
⋮----
grid?.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
grid.BeginEdit();
⋮----
private void EditModeDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
⋮----
private void EditModeDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] Tapped: OriginalSource={e.OriginalSource}");
⋮----
Debug.WriteLine("[Dunnage_EditModeView] Tapped: Grid empty");
⋮----
Debug.WriteLine("[Dunnage_EditModeView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
⋮----
Debug.WriteLine("[Dunnage_EditModeView] Tapped: Cell clicked. Enqueuing BeginEdit.");
grid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
⋮----
private void SelectFirstEditableCell(DataGrid grid)
⋮----
Debug.WriteLine("[Dunnage_EditModeView] SelectFirstEditableCell: Starting");
⋮----
var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
⋮----
grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
⋮----
Debug.WriteLine("[Dunnage_EditModeView] SelectFirstEditableCell: No editable column found.");
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
```

## File: Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_ManualEntryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
    xmlns:enums="using:MTM_Receiving_Application.Module_Core.Models.Enums"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:Name="Root">

    <UserControl.Resources>
        <converters:Converter_DecimalToString x:Key="DecimalToStringConverter"/>
        <converters:Converter_LoadNumberToOneBased x:Key="LoadNumberConverter"/>
    </UserControl.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <ScrollViewer HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Disabled">
                <StackPanel Orientation="Horizontal" Spacing="12" Padding="0,0,12,0">
                <Button Command="{x:Bind ViewModel.AddRowCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddRow'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE710;"/>
                        <TextBlock Text="Add Row"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.AddMultipleRowsCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddMultipleRows'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE710;"/>
                        <TextBlock Text="Add Multiple"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.RemoveRowCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.RemoveRow'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74D;"/>
                        <TextBlock Text="Remove Row"/>
                    </StackPanel>
                </Button>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <Button Command="{x:Bind ViewModel.AutoFillCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AutoFill'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74E;"/>
                        <TextBlock Text="Auto-Fill"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.FillBlankSpacesCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FillBlanks'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8B7;"/>
                        <TextBlock Text="Fill Blank Spaces"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.SortForPrintingCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SortForPrinting'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8CB;"/>
                        <TextBlock Text="Sort"/>
                    </StackPanel>
                </Button>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                </StackPanel>
            </ScrollViewer>
        </Grid>

        <!-- Data Grid -->
        <controls:DataGrid Grid.Row="1"
                          x:Name="ManualEntryDataGrid"
                          ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}"
                          SelectedItem="{x:Bind ViewModel.SelectedLoad, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          IsReadOnly="False"
                          CanUserSortColumns="True"
                          GridLinesVisibility="All"
                          HeadersVisibility="Column"
                          SelectionMode="Single"
                          KeyDown="ManualEntryDataGrid_KeyDown"
                          CurrentCellChanged="ManualEntryDataGrid_CurrentCellChanged"
                          Tapped="ManualEntryDataGrid_Tapped">
            <controls:DataGrid.Columns>
                <controls:DataGridTextColumn Header="Load #" 
                                           Binding="{Binding LoadNumber, Converter={StaticResource LoadNumberConverter}}" 
                                           IsReadOnly="True" 
                                           Width="Auto"/>
                <controls:DataGridTextColumn Header="Type" 
                                           Binding="{Binding TypeName}" 
                                           Width="*"/>
                <controls:DataGridTextColumn Header="Part ID" 
                                           Binding="{Binding PartId}" 
                                           Width="*"/>
                <controls:DataGridTextColumn Header="Quantity" 
                                           Binding="{Binding Quantity}" 
                                           Width="Auto"/>
                <controls:DataGridTextColumn Header="PO Number" 
                                           Binding="{Binding PoNumber}" 
                                           Width="Auto"/>
                <controls:DataGridTextColumn Header="Location" 
                                           Binding="{Binding Location}" 
                                           Width="Auto"/>
                <controls:DataGridTextColumn Header="Home Location" 
                                           Binding="{Binding HomeLocation}" 
                                           Width="Auto"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Navigation -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0" Spacing="12">
            <Button Content="Save &amp; Finish" Command="{x:Bind ViewModel.SaveAllCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveAndFinish'), Mode=OneWay}"/>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml.cs

```csharp
public sealed partial class View_Dunnage_ManualEntryView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] Loads_CollectionChanged: New row added");
ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] Loads_CollectionChanged: Selecting new item LoadNumber={newItem.LoadNumber}");
⋮----
ManualEntryDataGrid.ScrollIntoView(newItem, ManualEntryDataGrid.Columns.FirstOrDefault());
_ = Task.Run(async () =>
⋮----
await Task.Delay(100);
⋮----
private void ManualEntryDataGrid_CurrentCellChanged(object? sender, EventArgs e)
⋮----
grid?.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
grid.BeginEdit();
⋮----
private void ManualEntryDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
⋮----
private void ManualEntryDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] Tapped: OriginalSource={e.OriginalSource}");
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] Tapped: Grid empty, triggering AddRow command.");
if (ViewModel.AddRowCommand.CanExecute(null))
⋮----
ViewModel.AddRowCommand.Execute(null);
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] Tapped: Cell clicked. Enqueuing BeginEdit.");
grid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
⋮----
private void SelectFirstEditableCell(DataGrid grid)
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] SelectFirstEditableCell: Starting");
⋮----
var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
⋮----
grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] SelectFirstEditableCell: No editable column found.");
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
```

## File: Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_ModeSelectionView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
    mc:Ignorable="d">

    <Grid>
        <StackPanel Spacing="32" HorizontalAlignment="Center" VerticalAlignment="Center">
            <StackPanel Orientation="Horizontal" Spacing="24">
                <!-- Guided Wizard Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectGuidedModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Guided Wizard Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE771;" 
                                      FontSize="48" 
                                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Guided Wizard" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Step-by-step workflow for dunnage receiving with validation at each stage." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Guided Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsGuidedModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetGuidedAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Tooltip.Button.QuickGuidedWizard'), Mode=OneWay}"/>
                </StackPanel>

                <!-- Manual Entry Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectManualModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Manual Entry Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE7F0;" 
                                      FontSize="48" 
                                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Manual Entry" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Bulk grid entry for receiving multiple dunnage items simultaneously." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Manual Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsManualModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetManualAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Tooltip.Button.QuickManualEntry'), Mode=OneWay}"/>
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
                            <FontIcon Glyph="&#xE70F;" 
                                      FontSize="48" 
                                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Edit Mode" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Load historical dunnage records for editing and correction." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Edit Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsEditModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetEditAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.QuickEditMode'), Mode=OneWay}"/>
                </StackPanel>
            </StackPanel>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml.cs

```csharp
public sealed partial class View_Dunnage_ModeSelectionView : UserControl
```

## File: Module_Dunnage/Views/View_Dunnage_PartSelectionView.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_PartSelectionView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:DataType="views:View_Dunnage_PartSelectionView"
    Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:Converter_EmptyStringToVisibility x:Key="EmptyStringToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="8" Margin="0,0,0,24">
            <StackPanel Orientation="Horizontal" Spacing="8">
                <TextBlock Style="{StaticResource TitleTextBlockStyle}" Text="Select Part - "/>
                <mi:MaterialIcon Kind="{x:Bind ViewModel.SelectedTypeIconKind, Mode=OneWay}" 
                                 Width="24" Height="24"
                                 Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                                 VerticalAlignment="Center"/>
                <TextBlock Text="{x:Bind ViewModel.SelectedTypeName, Mode=OneWay}" 
                           Style="{StaticResource TitleTextBlockStyle}"/>
            </StackPanel>
            <TextBlock Text="{x:Bind ViewModel.GetTip('Dunnage.PartSelection'), Mode=OneWay}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       Style="{StaticResource BodyTextBlockStyle}"/>
        </StackPanel>

        <!-- Inventory Notification -->
        <InfoBar Grid.Row="1"
                 IsOpen="{x:Bind ViewModel.IsInventoryNotificationVisible, Mode=OneWay}"
                 Severity="Warning"
                 Message="{x:Bind ViewModel.InventoryNotificationMessage, Mode=OneWay}"
                 Margin="0,0,0,16"/>

        <!-- Part Selection Form -->
        <ScrollViewer Grid.Row="2">
            <StackPanel Spacing="24">
                
                <!-- Part Dropdown -->
                <StackPanel Spacing="12">
                    <TextBlock Text="Select Part" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <ComboBox x:Name="PartNumberComboBox"
                              ItemsSource="{x:Bind ViewModel.AvailableParts, Mode=OneWay}"
                              SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                              DisplayMemberPath="PartId"
                              PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PartSelection'), Mode=OneWay}"
                              HorizontalAlignment="Stretch"
                              MinWidth="300"/>
                </StackPanel>

                <!-- Part Details Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20"
                        Visibility="{x:Bind ViewModel.IsPartSelected, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                    <Grid RowSpacing="12">
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>

                        <!-- Header -->
                        <TextBlock Grid.Row="0" 
                                   Text="Part Details" 
                                   Style="{StaticResource SubtitleTextBlockStyle}"/>

                        <!-- Details Grid -->
                        <Grid Grid.Row="1" ColumnSpacing="24" RowSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>

                            <!-- Part ID -->
                            <TextBlock Grid.Row="0" Grid.Column="0" 
                                       Text="Part ID:" 
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <TextBlock Grid.Row="0" Grid.Column="1" 
                                       Text="{x:Bind ViewModel.SelectedPart.PartId, Mode=OneWay}" 
                                       FontWeight="SemiBold"/>

                            <!-- Type -->
                            <TextBlock Grid.Row="1" Grid.Column="0" 
                                       Text="Type:" 
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <StackPanel Grid.Row="1" Grid.Column="1" Orientation="Horizontal" Spacing="6">
                                <mi:MaterialIcon Kind="{x:Bind ViewModel.SelectedTypeIconKind, Mode=OneWay}" 
                                                 Width="16" Height="16"
                                                 Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                                <TextBlock Text="{x:Bind ViewModel.SelectedTypeName, Mode=OneWay}"/>
                            </StackPanel>
                        </Grid>

                        <!-- Inventory Method (if applicable) -->
                        <StackPanel Grid.Row="2" 
                                    Orientation="Horizontal" 
                                    Spacing="8"
                                    Visibility="{x:Bind ViewModel.IsInventoryNotificationVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                            <FontIcon Glyph="&#xE946;" 
                                      FontSize="16" 
                                      Foreground="{ThemeResource SystemFillColorCautionBrush}"/>
                            <TextBlock Text="Inventory Method:" 
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <TextBlock Text="{x:Bind ViewModel.InventoryMethod, Mode=OneWay}" 
                                       FontWeight="SemiBold"
                                       Foreground="{ThemeResource SystemFillColorCautionBrush}"/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Action Buttons -->
                <StackPanel Orientation="Horizontal" Spacing="12">
                    <Button Command="{x:Bind ViewModel.QuickAddPartCommand}"
                            HorizontalAlignment="Left">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE710;" FontSize="16"/>
                            <TextBlock Text="Add New Part"/>
                        </StackPanel>
                    </Button>
                    
                    <Button Command="{x:Bind ViewModel.LoadPartsCommand}"
                            HorizontalAlignment="Left"
                            ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.Refresh'), Mode=OneWay}">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE72C;" FontSize="16"/>
                            <TextBlock Text="Refresh Parts"/>
                        </StackPanel>
                    </Button>
                </StackPanel>

            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_PartSelectionView.xaml.cs

```csharp
public sealed partial class View_Dunnage_PartSelectionView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this, PartNumberComboBox);
⋮----
private async void OnLoaded(object sender, RoutedEventArgs e)
⋮----
System.Diagnostics.Debug.WriteLine("Dunnage_PartSelectionView: OnLoaded called");
await ViewModel.InitializeAsync();
```

## File: Module_Dunnage/Views/View_Dunnage_QuantityEntryView.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_QuantityEntryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:DataType="views:View_Dunnage_QuantityEntryView"
    Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_EmptyStringToVisibility x:Key="EmptyStringToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="8" Margin="0,0,0,16">
            <TextBlock Text="Enter Quantity"
                       Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock Text="Specify how many labels you need to generate."
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       Style="{StaticResource BodyTextBlockStyle}"/>
        </StackPanel>

        <!-- Context Banner -->
        <Border Grid.Row="1"
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="12"
                Margin="0,0,0,12">
            <StackPanel Spacing="6">
                <TextBlock Text="Selection Summary"
                           FontWeight="SemiBold"
                           FontSize="13"/>
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <TextBlock Text="Type:" Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12"/>
                    <mi:MaterialIcon Kind="{x:Bind ViewModel.SelectedTypeIconKind, Mode=OneWay}" 
                                     Width="16" Height="16"
                                     Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="{x:Bind ViewModel.SelectedTypeName, Mode=OneWay}"
                               FontWeight="SemiBold"
                               FontSize="12"/>
                </StackPanel>
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <TextBlock Text="Part:" Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12"/>
                    <TextBlock Text="{x:Bind ViewModel.SelectedPartName, Mode=OneWay}"
                               FontWeight="SemiBold"
                               FontSize="12"/>
                </StackPanel>
            </StackPanel>
        </Border>

        <!-- Quantity Input -->
        <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
            <StackPanel Spacing="12">
                <!-- Quantity Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <StackPanel Spacing="6">
                        <TextBlock Text="Quantity"
                                   Style="{StaticResource SubtitleTextBlockStyle}"/>
                        <NumberBox x:Name="QuantityNumberBox"
                                   Value="{x:Bind ViewModel.Quantity, Mode=TwoWay}"
                                   Minimum="1"
                                   Maximum="9999"
                                   SpinButtonPlacementMode="Inline"
                                   HorizontalAlignment="Stretch"
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.Quantity'), Mode=OneWay}"/>
                        
                        <!-- Validation Message -->
                        <TextBlock Text="{x:Bind ViewModel.ValidationMessage, Mode=OneWay}"
                                   Foreground="{ThemeResource SystemFillColorCriticalBrush}"
                                   Visibility="{x:Bind ViewModel.ValidationMessage, Mode=OneWay, Converter={StaticResource EmptyStringToVisibilityConverter}}"
                                   TextWrapping="Wrap"
                                   FontSize="12"/>
                    </StackPanel>
                </Border>

                <!-- Info Panel Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <StackPanel Spacing="6">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE946;" 
                                      FontSize="16" 
                                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Tip" FontWeight="SemiBold"/>
                        </StackPanel>
                        <TextBlock Text="{x:Bind ViewModel.GetTip('Dunnage.QuantityEntry'), Mode=OneWay}"
                                   TextWrapping="Wrap"
                                   FontSize="12"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    </StackPanel>
                </Border>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_QuantityEntryView.xaml.cs

```csharp
public sealed partial class View_Dunnage_QuantityEntryView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this, QuantityNumberBox);
⋮----
private void OnLoaded(object sender, RoutedEventArgs e)
⋮----
ViewModel.LoadContextData();
```

## File: Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_QuickAddPartDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Add New Dunnage Part"
    PrimaryButtonText="Add Part"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="OnPrimaryButtonClick"
    PrimaryButtonStyle="{StaticResource AccentButtonStyle}"
    Width="500">

    <ScrollViewer MaxHeight="500">
        <StackPanel Spacing="20" Padding="8">
            <!-- Instructions -->
            <TextBlock 
                Text="Enter details for the new dunnage part"
                TextWrapping="Wrap"
                Style="{StaticResource BodyTextBlockStyle}"/>

            <!-- Type Display (Read-only) -->
            <Border Background="{ThemeResource LayerFillColorDefaultBrush}"
                    Padding="12"
                    CornerRadius="4">
                <StackPanel Spacing="4">
                    <TextBlock Text="Dunnage Type" 
                               Style="{StaticResource CaptionTextBlockStyle}"
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    <TextBlock x:Name="TypeNameTextBlock"
                               FontWeight="SemiBold"/>
                </StackPanel>
            </Border>

            <!-- Part ID Field -->
            <StackPanel Spacing="8">
                <TextBlock Text="Part ID (Auto-generated)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBox 
                    x:Name="PartIdTextBox"
                    IsReadOnly="True"
                    PlaceholderText="Auto-generated based on dimensions"
                    MaxLength="100"/>
                <TextBlock 
                    Text="Part ID is generated from Type and Dimensions"
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                    Style="{StaticResource CaptionTextBlockStyle}"/>
            </StackPanel>

            <!-- Specifications Section -->
            <StackPanel Spacing="12">
                <TextBlock Text="Specifications" 
                           Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBlock 
                    Text="Enter dimensions to generate the Part ID"
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                    Style="{StaticResource CaptionTextBlockStyle}"/>

                <Grid ColumnSpacing="12" RowSpacing="12">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>

                    <!-- Width -->
                    <StackPanel Grid.Column="0" Spacing="4">
                        <TextBlock Text="Width (in)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        <NumberBox x:Name="WidthNumberBox" 
                                   PlaceholderText="0"
                                   SpinButtonPlacementMode="Hidden"
                                   Minimum="0"
                                   Maximum="999"
                                   SmallChange="1"
                                   LargeChange="10"
                                   ValueChanged="OnDimensionChanged"/>
                    </StackPanel>

                    <!-- Height -->
                    <StackPanel Grid.Column="1" Spacing="4">
                        <TextBlock Text="Height (in)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        <NumberBox x:Name="HeightNumberBox" 
                                   PlaceholderText="0"
                                   SpinButtonPlacementMode="Hidden"
                                   Minimum="0"
                                   Maximum="999"
                                   SmallChange="1"
                                   LargeChange="10"
                                   ValueChanged="OnDimensionChanged"/>
                    </StackPanel>

                    <!-- Depth -->
                    <StackPanel Grid.Column="2" Spacing="4">
                        <TextBlock Text="Depth (in)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        <NumberBox x:Name="DepthNumberBox" 
                                   PlaceholderText="0"
                                   SpinButtonPlacementMode="Hidden"
                                   Minimum="0"
                                   Maximum="999"
                                   SmallChange="1"
                                   LargeChange="10"
                                   ValueChanged="OnDimensionChanged"/>
                    </StackPanel>
                </Grid>
            </StackPanel>

            <!-- Dynamic Specs Container -->
            <StackPanel x:Name="DynamicSpecsPanel" Spacing="12"/>

            <!-- Additional Notes -->
            <StackPanel Spacing="8">
                <TextBlock Text="Additional Notes (Optional)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBox 
                    x:Name="NotesTextBox"
                    PlaceholderText="Any additional specifications or notes"
                    TextWrapping="Wrap"
                    AcceptsReturn="True"
                    MaxLength="500"
                    Height="80"/>
            </StackPanel>

        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_QuickAddTypeDialog.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_QuickAddTypeDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    xmlns:local="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    Title="Add New Dunnage Type"
    PrimaryButtonText="Add Type"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="OnPrimaryButtonClick"
    PrimaryButtonStyle="{StaticResource AccentButtonStyle}"
    Width="900">

    <ContentDialog.Resources>
        <converters:Converter_IconCodeToGlyph x:Key="IconCodeToGlyphConverter"/>
        <!-- Help Button Style -->
        <Style x:Key="HelpButtonStyle" TargetType="Button">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Padding" Value="8"/>
            <Setter Property="CornerRadius" Value="4"/>
        </Style>
    </ContentDialog.Resources>

    <ScrollViewer VerticalScrollBarVisibility="Auto" MaxHeight="600">
        <StackPanel Spacing="24" Padding="12">
            <!-- Header with Help Button -->
            <Grid>
                <TextBlock 
                    Text="Enter details for the new dunnage type"
                    TextWrapping="Wrap"
                    VerticalAlignment="Center"
                    Style="{StaticResource BodyTextBlockStyle}"/>
                
                <Button x:Name="HelpButton"
                        HorizontalAlignment="Right"
                        VerticalAlignment="Top"
                        Style="{StaticResource HelpButtonStyle}"
                        ToolTipService.ToolTip="Click for help about dunnage types">
                    <StackPanel Orientation="Horizontal" Spacing="6">
                        <FontIcon Glyph="&#xE946;" FontSize="14" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                        <TextBlock Text="Help" FontSize="12" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                    </StackPanel>
                    <Button.Flyout>
                        <Flyout>
                            <StackPanel Spacing="16" MaxWidth="400" Padding="4">
                                <StackPanel Spacing="8">
                                    <TextBlock Text="Dunnage Types" Style="{StaticResource SubtitleTextBlockStyle}" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <Border Height="2" Background="{ThemeResource AccentFillColorDefaultBrush}" CornerRadius="1" HorizontalAlignment="Left" Width="60"/>
                                    <TextBlock TextWrapping="Wrap" Foreground="{ThemeResource TextFillColorSecondaryBrush}">Define categories of dunnage materials for receiving classification.</TextBlock>
                                </StackPanel>
                                
                                <Border Background="{ThemeResource LayerFillColorDefaultBrush}" Padding="12" CornerRadius="6" BorderThickness="1" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                                    <StackPanel Spacing="8">
                                        <TextBlock Text="Type Name" FontWeight="SemiBold" FontSize="13"/>
                                        <TextBlock TextWrapping="Wrap" FontSize="12" Foreground="{ThemeResource TextFillColorSecondaryBrush}">Choose a descriptive name like 'Pallet', 'Crate', or 'Foam Insert'. This will be displayed in selection lists.</TextBlock>
                                    </StackPanel>
                                </Border>
                                
                                <Border Background="{ThemeResource LayerFillColorDefaultBrush}" Padding="12" CornerRadius="6" BorderThickness="1" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                                    <StackPanel Spacing="8">
                                        <TextBlock Text="Specifications" FontWeight="SemiBold" FontSize="13"/>
                                        <TextBlock TextWrapping="Wrap" FontSize="12" Foreground="{ThemeResource TextFillColorSecondaryBrush}">Add custom fields to capture specific data for this type. For example, 'Material' (Text), 'Weight Capacity' (Number with unit 'lbs'), or 'Recyclable' (Boolean).</TextBlock>
                                    </StackPanel>
                                </Border>
                                
                                <Border Background="{ThemeResource SystemFillColorAttentionBackgroundBrush}" Padding="12" CornerRadius="6" BorderThickness="1" BorderBrush="{ThemeResource SystemFillColorAttentionBrush}">
                                    <StackPanel Spacing="6">
                                        <StackPanel Orientation="Horizontal" Spacing="6">
                                            <FontIcon Glyph="&#xE946;" FontSize="12" Foreground="{ThemeResource SystemFillColorAttentionBrush}"/>
                                            <TextBlock Text="Tip" FontWeight="SemiBold" FontSize="12" Foreground="{ThemeResource SystemFillColorAttentionBrush}"/>
                                        </StackPanel>
                                        <TextBlock TextWrapping="Wrap" FontSize="11">Required specs must be filled when creating new parts. Use Number type for measurements and validation.</TextBlock>
                                    </StackPanel>
                                </Border>
                            </StackPanel>
                        </Flyout>
                    </Button.Flyout>
                </Button>
            </Grid>

            <!-- Type Name Field -->
            <StackPanel Spacing="12">
                <TextBlock Text="Type Name" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBox 
                    x:Name="TypeNameTextBox"
                    PlaceholderText="e.g., Pallet, Crate, Blocking"
                    MaxLength="100"
                    TextChanged="OnTypeNameChanged"
                    ToolTipService.ToolTip="Enter a unique descriptive name for this dunnage type"/>
                <TextBlock 
                    x:Name="ValidationTextBlock"
                    Text="Type name is required"
                    Foreground="{ThemeResource SystemFillColorCriticalBrush}"
                    Visibility="Collapsed"
                    Style="{StaticResource CaptionTextBlockStyle}"/>
            </StackPanel>

            <!-- Icon Selection -->
            <StackPanel Spacing="12">
                <TextBlock Text="Select Icon" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBlock 
                    Text="Choose an icon to represent this type"
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                    Style="{StaticResource CaptionTextBlockStyle}"/>
                
                <Button x:Name="SelectIconButton"
                        Click="OnSelectIconClick"
                        HorizontalAlignment="Stretch"
                        Padding="16,12"
                        ToolTipService.ToolTip="Click to browse and select an icon from the icon library">
                    <StackPanel Orientation="Horizontal" Spacing="12">
                        <materialIcons:MaterialIcon x:Name="SelectedIconDisplay" 
                                  Kind="{x:Bind SelectedIconKind, Mode=OneWay}" 
                                  Width="24" Height="24"
                                  Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock Text="Click to select icon" FontWeight="SemiBold"/>
                            <TextBlock x:Name="SelectedIconNameText"
                                       Text="Box"
                                       FontSize="12"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </StackPanel>
                </Button>
            </StackPanel>

            <!-- Specifications -->
            <StackPanel Spacing="12">
                <StackPanel Spacing="4">
                    <TextBlock Text="Specifications" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <TextBlock 
                        Text="Define custom fields to capture specific data for this dunnage type"
                        Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                        Style="{StaticResource CaptionTextBlockStyle}"/>
                </StackPanel>
            
                <!-- Add Spec Form -->
                <Border Background="{ThemeResource LayerFillColorDefaultBrush}" 
                        CornerRadius="6" 
                        Padding="16"
                        BorderThickness="1"
                        BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}">
                    <StackPanel Spacing="12">
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="2*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <TextBox x:Name="NewSpecNameBox" 
                                     PlaceholderText="Field Name (e.g. Material, Weight)"
                                     ToolTipService.ToolTip="Enter a unique name for this specification field"/>
                            <ComboBox x:Name="NewSpecTypeCombo" 
                                      Grid.Column="1"
                                      SelectedIndex="0" 
                                      HorizontalAlignment="Stretch"
                                      ToolTipService.ToolTip="Select the data type for this field">
                                <ComboBoxItem Content="Text"/>
                                <ComboBoxItem Content="Number"/>
                                <ComboBoxItem Content="Boolean"/>
                            </ComboBox>
                        </Grid>
                        
                        <!-- Number Type Options -->
                        <StackPanel x:Name="NumberOptionsPanel" Spacing="8" Visibility="Collapsed">
                            <TextBlock Text="Number Validation" FontSize="12" FontWeight="SemiBold" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <Grid ColumnSpacing="12">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                <NumberBox x:Name="NewSpecMinValueBox" 
                                           Header="Min Value" 
                                           PlaceholderText="Optional"
                                           SpinButtonPlacementMode="Compact"
                                           ToolTipService.ToolTip="Minimum allowed value (optional)"/>
                                <NumberBox x:Name="NewSpecMaxValueBox" 
                                           Grid.Column="1"
                                           Header="Max Value" 
                                           PlaceholderText="Optional"
                                           SpinButtonPlacementMode="Compact"
                                           ToolTipService.ToolTip="Maximum allowed value (optional)"/>
                                <TextBox x:Name="NewSpecUnitBox" 
                                         Grid.Column="2"
                                         Header="Unit" 
                                         PlaceholderText="e.g. inches, lbs"
                                         ToolTipService.ToolTip="Unit of measurement (optional)"/>
                            </Grid>
                        </StackPanel>
                        
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <StackPanel Orientation="Horizontal" Spacing="16" VerticalAlignment="Center">
                                <CheckBox x:Name="NewSpecRequiredCheck" 
                                          Content="Required Field"
                                          ToolTipService.ToolTip="Check if this field is required"/>
                            </StackPanel>
                            <Button Grid.Column="1" 
                                    Content="Add Field" 
                                    Click="OnAddSpecClick" 
                                    Style="{StaticResource AccentButtonStyle}"
                                    ToolTipService.ToolTip="Add this specification to the list below"/>
                        </Grid>
                    </StackPanel>
                </Border>

                <ListView x:Name="SpecsListView" 
                          Height="130"
                          BorderThickness="1"
                          BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}"
                          CornerRadius="6"
                          Padding="4"
                          ToolTipService.ToolTip="List of specifications defined for this type">
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:Model_SpecItem">
                        <Grid Padding="0,4">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <StackPanel Spacing="2">
                                <TextBlock Text="{x:Bind Name}" FontWeight="SemiBold" FontSize="13"/>
                                <TextBlock Text="{x:Bind Description}" 
                                           FontSize="11" 
                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            </StackPanel>
                            <Border Grid.Column="1" 
                                    Background="{ThemeResource SystemFillColorAttentionBackgroundBrush}"
                                    CornerRadius="3"
                                    Padding="6,2"
                                    Margin="8,0"
                                    Visibility="{x:Bind IsRequiredVisibility}">
                                <TextBlock Text="Required" FontSize="10" FontWeight="SemiBold"/>
                            </Border>
                            <Button Grid.Column="2" 
                                    Content="&#xE74D;"
                                    FontFamily="Segoe MDL2 Assets"
                                    ToolTipService.ToolTip="Remove this specification"
                                    Click="OnRemoveSpecClick" 
                                    Style="{StaticResource SubtleButtonStyle}"/>
                        </Grid>
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>
            </StackPanel>
        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_QuickAddTypeDialog.xaml.cs

```csharp
public sealed partial class View_Dunnage_QuickAddTypeDialog : ContentDialog, INotifyPropertyChanged
⋮----
private MaterialIconKind _selectedIcon = MaterialIconKind.PackageVariantClosed;
⋮----
private void OnSpecTypeChanged(object sender, SelectionChangedEventArgs e)
⋮----
var type = item.Content.ToString();
⋮----
public void InitializeForEdit(string typeName, string iconName, Dictionary<string, SpecDefinition> specs)
⋮----
SelectedIconNameText.Text = kind.ToString();
⋮----
Specs.Clear();
⋮----
Specs.Add(new Model_SpecItem
⋮----
private void OnAddSpecClick(object sender, RoutedEventArgs e)
⋮----
private void AddSpec()
⋮----
var name = NewSpecNameBox.Text.Trim();
if (string.IsNullOrWhiteSpace(name))
⋮----
if (Specs.Any(s => s.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
⋮----
var type = typeItem?.Content.ToString() ?? "Text";
⋮----
unit = NewSpecUnitBox.Text.Trim();
if (!double.IsNaN(NewSpecMinValueBox.Value))
⋮----
if (!double.IsNaN(NewSpecMaxValueBox.Value))
⋮----
private void OnRemoveSpecClick(object sender, RoutedEventArgs e)
⋮----
Specs.Remove(spec);
⋮----
private async void OnSelectIconClick(object sender, RoutedEventArgs e)
⋮----
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(iconWindow);
var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
iconWindow.Activate();
⋮----
iconWindow.Closed += (s, args) => tcs.SetResult(true);
⋮----
private void OnTypeNameChanged(object sender, TextChangedEventArgs e)
⋮----
private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
var input = TypeNameTextBox.Text.Trim();
if (string.IsNullOrWhiteSpace(input))
⋮----
if (!char.IsUpper(input[0]))
⋮----
if (input.Contains(" "))
⋮----
if (!AlphanumericRegex().IsMatch(input))
⋮----
private static partial System.Text.RegularExpressions.Regex AlphanumericRegex();
private void ShowError(string message)
⋮----
private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
```

## File: Module_Dunnage/Views/View_Dunnage_ReviewView.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_ReviewView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:DataType="views:View_Dunnage_ReviewView"
    Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:Converter_EmptyStringToVisibility x:Key="EmptyStringToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Success Message -->
        <InfoBar Grid.Row="0"
                 IsOpen="{x:Bind ViewModel.IsSuccessMessageVisible, Mode=OneWay}"
                 Severity="Success"
                 Message="{x:Bind ViewModel.SuccessMessage, Mode=OneWay}"
                 IsClosable="False">
            <InfoBar.ActionButton>
                <Button Content="Start New Entry" Command="{x:Bind ViewModel.StartNewEntryCommand}"/>
            </InfoBar.ActionButton>
        </InfoBar>

        <!-- Content Section with View Toggle -->
        <Grid Grid.Row="1">
            <!-- Single Entry View -->
            <Grid Visibility="{x:Bind ViewModel.IsSingleView, Mode=OneWay}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Entry Counter -->
                <Border Grid.Row="0" 
                       Background="{ThemeResource AccentFillColorDefaultBrush}"
                       Padding="12,6"
                       CornerRadius="8,8,0,0">
                    <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
                        <FontIcon Glyph="&#xE8FD;" 
                                 FontSize="16" 
                                 Foreground="White"/>
                        <TextBlock Style="{StaticResource SubtitleTextBlockStyle}"
                                   Foreground="White">
                            <Run Text="Entry"/>
                            <Run Text="{x:Bind ViewModel.CurrentEntryIndex, Mode=OneWay}"/>
                            <Run Text="of"/>
                            <Run Text="{x:Bind ViewModel.LoadCount, Mode=OneWay}"/>
                        </TextBlock>
                    </StackPanel>
                </Border>

                <!-- Single Entry Form -->
                <Grid Grid.Row="1">
                    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                           BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                           BorderThickness="1,0,1,1"
                           CornerRadius="0,0,8,8"
                           Padding="20">
                        <Grid MaxWidth="900">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>

                            <!-- Left Column -->
                            <StackPanel Grid.Column="0" Spacing="12" Margin="0,0,12,0">
                                <TextBlock Text="Load Details" 
                                          Style="{StaticResource SubtitleTextBlockStyle}"
                                          Margin="0,0,0,8"/>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Type" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <StackPanel Orientation="Horizontal" Spacing="6">
                                        <mi:MaterialIcon Kind="{x:Bind ViewModel.CurrentLoad.TypeIconKind, Mode=OneWay}" 
                                                         Width="18" Height="18"
                                                         Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                                        <TextBlock Text="{x:Bind ViewModel.CurrentLoad.TypeName, Mode=OneWay}" 
                                                  Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                    </StackPanel>
                                </StackPanel>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Part ID" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.PartId, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Quantity" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.Quantity, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                            </StackPanel>

                            <!-- Right Column -->
                            <StackPanel Grid.Column="1" Spacing="12" Margin="12,0,0,0">
                                <TextBlock Text="Additional Information" 
                                          Style="{StaticResource SubtitleTextBlockStyle}"
                                          Margin="0,0,0,8"/>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="PO Number" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.PoNumber, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Location" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.Location, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Inventory Method" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.InventoryMethod, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                            </StackPanel>
                        </Grid>
                    </Border>
                </Grid>

                <!-- Navigation Bar for Single View -->
                <CommandBar Grid.Row="2" DefaultLabelPosition="Right" Margin="0,12,0,0">
                    <AppBarButton Icon="Back" 
                                 Label="Previous" 
                                 Command="{x:Bind ViewModel.PreviousEntryCommand}"
                                 IsEnabled="{x:Bind ViewModel.CanGoBack, Mode=OneWay}"
                                 AutomationProperties.Name="Previous Entry"/>
                    
                    <AppBarButton Icon="Forward" 
                                 Label="Next" 
                                 Command="{x:Bind ViewModel.NextEntryCommand}"
                                 IsEnabled="{x:Bind ViewModel.CanGoNext, Mode=OneWay}"
                                 AutomationProperties.Name="Next Entry"/>
                    
                    <AppBarSeparator/>
                    
                    <AppBarButton Icon="ViewAll" 
                                 Label="Table View" 
                                 Command="{x:Bind ViewModel.SwitchToTableViewCommand}"
                                 AutomationProperties.Name="Switch to Table View"/>
                </CommandBar>
            </Grid>

            <!-- Table View (DataGrid) -->
            <Grid Visibility="{x:Bind ViewModel.IsTableView, Mode=OneWay}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <Border Grid.Row="0"
                       Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                       BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                       BorderThickness="1"
                       CornerRadius="8"
                       Padding="16">
                    <controls:DataGrid x:Name="ReviewDataGrid"
                                      ItemsSource="{x:Bind ViewModel.SessionLoads, Mode=OneWay}"
                                      AutoGenerateColumns="False"
                                      CanUserReorderColumns="False"
                                      CanUserResizeColumns="True"
                                      CanUserSortColumns="True"
                                      GridLinesVisibility="None"
                                      HeadersVisibility="Column"
                                      SelectionMode="Single"
                                      IsReadOnly="True">
                        <controls:DataGrid.Columns>
                            <controls:DataGridTextColumn Header="Type" 
                                                        Binding="{Binding TypeName}" 
                                                        Width="120"/>
                            <controls:DataGridTextColumn Header="Part ID" 
                                                        Binding="{Binding PartId}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Quantity" 
                                                        Binding="{Binding Quantity}" 
                                                        Width="100"/>
                            <controls:DataGridTextColumn Header="PO Number" 
                                                        Binding="{Binding PoNumber}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Location" 
                                                        Binding="{Binding Location}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Inventory Method" 
                                                        Binding="{Binding InventoryMethod}" 
                                                        Width="*"/>
                        </controls:DataGrid.Columns>
                    </controls:DataGrid>
                </Border>

                <!-- Navigation Bar for Table View -->
                <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,16,0,0">
                    <AppBarButton Label="Single View" 
                                 Command="{x:Bind ViewModel.SwitchToSingleViewCommand}"
                                 AutomationProperties.Name="Switch to Single View">
                        <AppBarButton.Icon>
                            <FontIcon Glyph="&#xE8FD;"/>
                        </AppBarButton.Icon>
                    </AppBarButton>
                </CommandBar>
            </Grid>
        </Grid>

        <!-- Action Buttons -->
        <Grid Grid.Row="2" ColumnSpacing="12" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <Button Grid.Column="1" 
                   Command="{x:Bind ViewModel.AddAnotherCommand}"
                   AutomationProperties.Name="Add Another Load"
                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddAnother'), Mode=OneWay}">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE710;" FontSize="16"/>
                    <TextBlock Text="Add Another"/>
                </StackPanel>
            </Button>
            
            <Button Grid.Column="2" 
                   Command="{x:Bind ViewModel.SaveAllCommand}"
                   Style="{StaticResource AccentButtonStyle}"
                   IsEnabled="{x:Bind ViewModel.CanSave, Mode=OneWay}"
                   AutomationProperties.Name="Save to Database">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE74E;" FontSize="16"/>
                    <TextBlock Text="Save All"/>
                </StackPanel>
            </Button>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_ReviewView.xaml.cs

```csharp
public sealed partial class View_Dunnage_ReviewView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private void OnLoaded(object sender, RoutedEventArgs e)
⋮----
ViewModel.LoadSessionLoads();
```

## File: Module_Receiving/Models/Model_Application_Variables.cs

```csharp
public class Model_Application_Variables
⋮----
public string LogDirectory { get; set; } = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
```

## File: Module_Receiving/Models/Model_CSVDeleteResult.cs

```csharp
public class Model_CSVDeleteResult
```

## File: Module_Receiving/Models/Model_CSVExistenceResult.cs

```csharp
public class Model_CSVExistenceResult
```

## File: Module_Receiving/Models/Model_CSVWriteResult.cs

```csharp
public class Model_CSVWriteResult
```

## File: Module_Receiving/Models/Model_InforVisualPart.cs

```csharp
public class Model_InforVisualPart
```

## File: Module_Receiving/Models/Model_InforVisualPO.cs

```csharp
public class Model_InforVisualPO
```

## File: Module_Receiving/Models/Model_PackageTypePreference.cs

```csharp
public class Model_PackageTypePreference
```

## File: Module_Receiving/Models/Model_SaveResult.cs

```csharp
public class Model_SaveResult
```

## File: Module_Receiving/Models/Model_UserPreference.cs

```csharp
public partial class Model_UserPreference : ObservableObject
⋮----
private DateTime _lastUpdated;
```

## File: Module_Receiving/Models/Model_WorkflowStepResult.cs

```csharp
public class Model_WorkflowStepResult
```

## File: Module_Receiving/Views/View_Receiving_EditMode.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_EditMode"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels"
    xmlns:enums="using:MTM_Receiving_Application.Module_Core.Models.Enums"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:Name="Root">

    <UserControl.Resources>
        <converters:Converter_DecimalToString x:Key="DecimalToStringConverter"/>
    </UserControl.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <!-- Main Toolbar Area -->
            <StackPanel Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <!-- Load Data Section -->
                <TextBlock Text="Load Data From:" VerticalAlignment="Center" FontWeight="SemiBold" Margin="0,0,4,0"/>
                
                <Button Command="{x:Bind ViewModel.LoadFromCurrentMemoryCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadSessionMemory'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8A7;"/>
                        <TextBlock Text="Current Memory"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.LoadFromCurrentLabelsCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadRecentCSV'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8E5;"/>
                        <TextBlock Text="Current Labels"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.LoadFromHistoryCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadHistoricalData'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74C;"/>
                        <TextBlock Text="History"/>
                    </StackPanel>
                </Button>

                <!-- Separator -->
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>

                <!-- Edit Actions Section -->
                <Button Command="{x:Bind ViewModel.SelectAllCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SelectAll'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8B3;"/>
                        <TextBlock Text="{x:Bind ViewModel.SelectAllButtonText, Mode=OneWay}"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.RemoveRowCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.RemoveRow'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74D;"/>
                        <TextBlock Text="Remove Row"/>
                    </StackPanel>
                </Button>
            </StackPanel>
        </Grid>

        <!-- Date Filter Toolbar -->
        <Grid Grid.Row="1" Margin="0,0,0,12">
            <StackPanel Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <TextBlock Text="Filter Date:" VerticalAlignment="Center" FontWeight="SemiBold"/>
                
                <CalendarDatePicker Date="{x:Bind ViewModel.FilterStartDate, Mode=TwoWay}" 
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.StartDate'), Mode=OneWay}"
                                   Width="140"/>
                <TextBlock Text="to" VerticalAlignment="Center"/>
                <CalendarDatePicker Date="{x:Bind ViewModel.FilterEndDate, Mode=TwoWay}" 
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.EndDate'), Mode=OneWay}"
                                   Width="140"/>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <Button Content="Last Week" Command="{x:Bind ViewModel.SetFilterLastWeekCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterLastWeek'), Mode=OneWay}"/>
                <Button Content="Today" Command="{x:Bind ViewModel.SetFilterTodayCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterToday'), Mode=OneWay}"/>
                <Button Content="This Week" Command="{x:Bind ViewModel.SetFilterThisWeekCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisWeek'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisMonthButtonText, Mode=OneWay}" Command="{x:Bind ViewModel.SetFilterThisMonthCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisMonth'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisQuarterButtonText, Mode=OneWay}" Command="{x:Bind ViewModel.SetFilterThisQuarterCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisQuarter'), Mode=OneWay}"/>
                <Button Content="Show All" Command="{x:Bind ViewModel.SetFilterShowAllCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ClearFilters'), Mode=OneWay}"/>
            </StackPanel>
        </Grid>

        <!-- Data Grid -->
        <controls:DataGrid Grid.Row="2"
                          x:Name="EditModeDataGrid"
                          ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}"
                          SelectedItem="{x:Bind ViewModel.SelectedLoad, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          IsReadOnly="False"
                          CanUserSortColumns="True"
                          GridLinesVisibility="All"
                          HeadersVisibility="Column"
                          SelectionMode="Single"
                          KeyDown="EditModeDataGrid_KeyDown"
                          CurrentCellChanged="EditModeDataGrid_CurrentCellChanged"
                          Tapped="EditModeDataGrid_Tapped">
            <controls:DataGrid.Columns>
                <controls:DataGridTemplateColumn Header="" Width="50" CanUserResize="False">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <CheckBox IsChecked="{Binding IsSelected, Mode=TwoWay}" 
                                      HorizontalAlignment="Center" 
                                      VerticalAlignment="Center"
                                      MinWidth="0"
                                      Margin="0"/>
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>
                <controls:DataGridTextColumn Header="Load #" Binding="{Binding LoadNumber}" IsReadOnly="True" Width="Auto"/>
                <controls:DataGridTextColumn Header="Part ID" Binding="{Binding PartID}" Width="*"/>
                <controls:DataGridTextColumn Header="Weight/Qty" Binding="{Binding WeightQuantity, Converter={StaticResource DecimalToStringConverter}}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Heat/Lot" Binding="{Binding HeatLotNumber}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Pkg Type" Binding="{Binding PackageType}" IsReadOnly="True" Width="Auto"/>
                <controls:DataGridTextColumn Header="Pkgs/Load" Binding="{Binding PackagesPerLoad}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Wt/Pkg" Binding="{Binding WeightPerPackage, Converter={StaticResource DecimalToStringConverter}}" IsReadOnly="True" Width="Auto"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Footer: Pagination and Save -->
        <Grid Grid.Row="3" Margin="0,12,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Pagination Controls -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
                <Button Command="{x:Bind ViewModel.FirstPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FirstPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE892;"/>
                </Button>
                <Button Command="{x:Bind ViewModel.PreviousPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.PreviousPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE76B;"/>
                </Button>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <TextBlock Text="Page" VerticalAlignment="Center"/>
                <TextBox Text="{x:Bind ViewModel.GotoPageNumber, Mode=TwoWay}" Width="60" HorizontalContentAlignment="Center"/>
                <TextBlock Text="of" VerticalAlignment="Center"/>
                <TextBlock Text="{x:Bind ViewModel.TotalPages, Mode=OneWay}" VerticalAlignment="Center" FontWeight="SemiBold"/>
                
                <Button Content="Go" Command="{x:Bind ViewModel.GoToPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.GoToPage'), Mode=OneWay}"/>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <Button Command="{x:Bind ViewModel.NextPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.NextPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE76C;"/>
                </Button>
                <Button Command="{x:Bind ViewModel.LastPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LastPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE893;"/>
                </Button>
            </StackPanel>

            <!-- Save Button -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="12">
                <Button Content="Save &amp; Finish" Command="{x:Bind ViewModel.SaveCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveAndFinish'), Mode=OneWay}"/>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_EditMode.xaml.cs

```csharp
public sealed partial class View_Receiving_EditMode : UserControl
⋮----
this.InitializeComponent();
⋮----
private void EditModeDataGrid_CurrentCellChanged(object? sender, EventArgs e)
⋮----
grid?.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[EditModeView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
grid.BeginEdit();
⋮----
private void EditModeDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
⋮----
Debug.WriteLine($"[EditModeView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
⋮----
private void EditModeDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
⋮----
Debug.WriteLine($"[EditModeView] Tapped: OriginalSource={e.OriginalSource}");
⋮----
Debug.WriteLine("[EditModeView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
⋮----
Debug.WriteLine("[EditModeView] Tapped: Cell clicked. Enqueuing BeginEdit.");
grid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[EditModeView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
⋮----
private void SelectFirstEditableCell(DataGrid grid)
⋮----
Debug.WriteLine("[EditModeView] SelectFirstEditableCell: Starting");
⋮----
var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
⋮----
Debug.WriteLine($"[EditModeView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
⋮----
grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
⋮----
Debug.WriteLine($"[EditModeView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
⋮----
Debug.WriteLine("[EditModeView] SelectFirstEditableCell: No editable column found.");
⋮----
Debug.WriteLine($"[EditModeView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
```

## File: Module_Receiving/Views/View_Receiving_HeatLot.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_HeatLot"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_StringFormat x:Key="StringFormatConverter"/>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="16">
        <!-- Content Section -->
        <Grid>
            <StackPanel Spacing="16">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Text="Load Entries" Style="{StaticResource SubtitleTextBlockStyle}" VerticalAlignment="Center"/>
                    <Button Grid.Column="1" 
                            Content="Auto-Fill" 
                            Command="{x:Bind ViewModel.AutoFillCommand}"
                            Style="{StaticResource AccentButtonStyle}"
                            ToolTipService.ToolTip="Fill blank heat numbers from rows above"/>
                </Grid>
            
                <ScrollViewer VerticalScrollBarVisibility="Auto" MaxHeight="500">
                    <ItemsControl ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}">
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Margin="0,0,0,12" 
                                       Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" 
                                       Padding="16" 
                                       CornerRadius="8" 
                                       BorderThickness="1" 
                                       BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                                    <Grid ColumnSpacing="16">
                                        <Grid.ColumnDefinitions>
                                            <ColumnDefinition Width="Auto"/>
                                            <ColumnDefinition Width="*"/>
                                        </Grid.ColumnDefinitions>
                                        
                                        <StackPanel Grid.Column="0" VerticalAlignment="Center" Width="100">
                                            <StackPanel Orientation="Horizontal" Spacing="8">
                                                <FontIcon Glyph="&#xE7B8;" 
                                                         FontSize="16"
                                                         Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                                <TextBlock Text="{Binding LoadNumber, Converter={StaticResource StringFormatConverter}, ConverterParameter='Load #{0}'}" 
                                                          Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                            </StackPanel>
                                        </StackPanel>
                                        
                                        <TextBox Grid.Column="1"
                                                Header="Heat/Lot Number (Optional)"
                                                Text="{Binding HeatLotNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                PlaceholderText="Enter heat/lot number or leave blank"
                                                AutomationProperties.Name="Heat Lot Number"/>
                                    </Grid>
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </ScrollViewer>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_HeatLot.xaml.cs

```csharp
public sealed partial class View_Receiving_HeatLot : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void HeatLotView_Loaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.OnNavigatedToAsync();
```

## File: Module_Receiving/Views/View_Receiving_LoadEntry.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_LoadEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    mc:Ignorable="d">

    <Grid HorizontalAlignment="Center" VerticalAlignment="Center">
        <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="32"
                MinWidth="500">
            <StackPanel Spacing="24" HorizontalAlignment="Center">
                <StackPanel Spacing="8" HorizontalAlignment="Center">
                    <FontIcon Glyph="&#xE7B8;" 
                             FontSize="48"
                             Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                             HorizontalAlignment="Center"/>
                    <TextBlock Text="{x:Bind ViewModel.SelectedPartInfo, Mode=OneWay}" 
                               Style="{StaticResource SubtitleTextBlockStyle}"
                               Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"
                               HorizontalAlignment="Center"
                               TextAlignment="Center"/>
                </StackPanel>

                <NumberBox x:Name="NumberOfLoadsNumberBox"
                           Header="Number of Loads (1-99)"
                           Value="{x:Bind ViewModel.NumberOfLoads, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                           Minimum="1"
                           Maximum="99"
                           SmallChange="1"
                           LargeChange="10"
                           SpinButtonPlacementMode="Inline"
                           Width="250"
                           HorizontalAlignment="Center"
                           AutomationProperties.Name="Number of Loads"/>
                       
                <TextBlock Text="Enter the total number of skids/loads for this part." 
                           Style="{StaticResource CaptionTextBlockStyle}"
                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                           HorizontalAlignment="Center"
                           TextAlignment="Center"/>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs

```csharp
public sealed partial class View_Receiving_LoadEntry : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
_focusService.AttachFocusOnVisibility(this, NumberOfLoadsNumberBox);
```

## File: Module_Receiving/Views/View_Receiving_ManualEntry.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_ManualEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels"
    xmlns:enums="using:MTM_Receiving_Application.Module_Core.Models.Enums"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:Name="Root">

    <UserControl.Resources>
        <converters:Converter_DecimalToString x:Key="DecimalToStringConverter"/>
    </UserControl.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="12">
                <Button x:Name="AddRowButton" Command="{x:Bind ViewModel.AddRowCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddRow'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE710;"/>
                        <TextBlock Text="Add Row"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.AddMultipleRowsCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddMultipleRows'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE710;"/>
                        <TextBlock Text="Add Multiple"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.RemoveRowCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.RemoveRow'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74D;"/>
                        <TextBlock Text="Remove Row"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.AutoFillCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AutoFill'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74E;"/>
                        <TextBlock Text="Auto-Fill"/>
                    </StackPanel>
                </Button>
            </StackPanel>
        </Grid>

        <!-- Data Grid -->
        <controls:DataGrid Grid.Row="1"
                          x:Name="ManualEntryDataGrid"
                          ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}"
                          SelectedItem="{x:Bind ViewModel.SelectedLoad, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          IsReadOnly="False"
                          CanUserSortColumns="True"
                          GridLinesVisibility="All"
                          HeadersVisibility="Column"
                          SelectionMode="Single"
                          KeyDown="ManualEntryDataGrid_KeyDown"
                          CurrentCellChanged="ManualEntryDataGrid_CurrentCellChanged"
                          Tapped="ManualEntryDataGrid_Tapped">
            <controls:DataGrid.Columns>
                <controls:DataGridTextColumn Header="Load #" Binding="{Binding LoadNumber}" IsReadOnly="True"/>
                <controls:DataGridTextColumn Header="Part ID" Binding="{Binding PartID}" Width="*"/>
                <controls:DataGridTextColumn Header="Weight/Qty" Binding="{Binding WeightQuantity, Converter={StaticResource DecimalToStringConverter}}"/>
                <controls:DataGridTextColumn Header="Heat/Lot" Binding="{Binding HeatLotNumber}"/>
                <controls:DataGridTextColumn Header="Pkg Type" Binding="{Binding PackageType}" IsReadOnly="True"/>
                <controls:DataGridTextColumn Header="Pkgs/Load" Binding="{Binding PackagesPerLoad}"/>
                <controls:DataGridTextColumn Header="Wt/Pkg" Binding="{Binding WeightPerPackage, Converter={StaticResource DecimalToStringConverter}}" IsReadOnly="True"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Navigation -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0" Spacing="12">
            <Button Content="Save &amp; Finish" Command="{x:Bind ViewModel.SaveCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveAndFinish'), Mode=OneWay}"/>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_ModeSelection.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_ModeSelection"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels"
    mc:Ignorable="d">

    <Grid>
        <StackPanel Spacing="32" HorizontalAlignment="Center" VerticalAlignment="Center">
            <StackPanel Orientation="Horizontal" Spacing="24">
                <!-- Guided Mode Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectGuidedModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Guided Wizard Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE771;" FontSize="48" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Guided Wizard" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Step-by-step process for standard receiving workflow." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Guided Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsGuidedModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetGuidedAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.QuickGuidedWizard'), Mode=OneWay}"/>
                </StackPanel>

                <!-- Manual Mode Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectManualModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Manual Entry Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE7F0;" FontSize="48" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Manual Entry" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Customizable grid for bulk data entry and editing." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Manual Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsManualModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetManualAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.QuickManualEntry'), Mode=OneWay}"/>
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
                            <TextBlock Text="Edit existing loads without adding new ones." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Edit Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsEditModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetEditAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.QuickEditMode'), Mode=OneWay}"/>
                </StackPanel>
            </StackPanel>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs

```csharp
public sealed partial class View_Receiving_ModeSelection : UserControl
⋮----
this.InitializeComponent();
```

## File: Module_Receiving/Views/View_Receiving_PackageType.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_PackageType"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_StringFormat x:Key="StringFormatConverter"/>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Package Type Selection -->
        <Border Grid.Row="1"
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                Padding="20"
                CornerRadius="8"
                VerticalAlignment="Top"
                Margin="0,0,0,16">
            <StackPanel Spacing="16">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE7B8;" FontSize="16" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                    <TextBlock Text="Package Type (Applied to all loads)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                
                <StackPanel Orientation="Horizontal" Spacing="16">
                    <ComboBox x:Name="PackageTypeComboBox"
                              Header="Type"
                              ItemsSource="{x:Bind ViewModel.PackageTypes, Mode=OneWay}"
                              SelectedItem="{x:Bind ViewModel.SelectedPackageType, Mode=TwoWay}"
                              Width="200"
                              AutomationProperties.Name="Package Type"/>
                    
                    <TextBox Header="Custom Name"
                             Text="{x:Bind ViewModel.CustomPackageTypeName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                             Visibility="{x:Bind ViewModel.IsCustomTypeVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                             Width="200"
                             AutomationProperties.Name="Custom Package Name"/>
                </StackPanel>
                
                <CheckBox Content="Save as default for this part" 
                          IsChecked="{x:Bind ViewModel.IsSaveAsDefault, Mode=TwoWay}"/>
            </StackPanel>
        </Border>

        <!-- Loads List -->
        <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
            <ItemsControl ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Grid Margin="0,0,0,8" Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" Padding="16,12" CornerRadius="8" BorderThickness="1" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="100"/>
                                <ColumnDefinition Width="250"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            
                            <StackPanel Grid.Column="0" VerticalAlignment="Center">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" FontSize="14" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="{Binding LoadNumber, Converter={StaticResource StringFormatConverter}, ConverterParameter='#{0}'}" 
                                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                            </StackPanel>
                            
                            <NumberBox Grid.Column="1"
                                       Header="Packages per Load"
                                       Value="{Binding PackagesPerLoad, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                       Minimum="1"
                                       SmallChange="1"
                                       LargeChange="5"
                                       SpinButtonPlacementMode="Inline"
                                       AutomationProperties.Name="Packages per Load"/>
                                       
                            <StackPanel Grid.Column="2" VerticalAlignment="Center" Margin="16,0,0,0">
                                <TextBlock Text="Weight per Package" Style="{StaticResource CaptionTextBlockStyle}" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                <TextBlock Text="{Binding WeightPerPackage, Converter={StaticResource StringFormatConverter}, ConverterParameter='{}{0:N2} lbs'}" 
                                           Style="{StaticResource BodyStrongTextBlockStyle}"/>
                            </StackPanel>
                        </Grid>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_PackageType.xaml.cs

```csharp
public sealed partial class View_Receiving_PackageType : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this, PackageTypeComboBox);
⋮----
private async void PackageTypeView_Loaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.OnNavigatedToAsync();
```

## File: Module_Receiving/Views/View_Receiving_POEntry.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_POEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:Converter_DecimalToInt x:Key="DecimalToIntConverter"/>
    </UserControl.Resources>

    <Grid Padding="24" RowSpacing="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Input Section (Row 0) -->
        <Grid Grid.Row="0">
            <!-- PO Entry Mode -->
            <StackPanel Visibility="{x:Bind ViewModel.IsNonPOItem, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}">
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20">
                    <StackPanel Spacing="16">
                        <!-- PO Number Input with Icon -->
                        <StackPanel Spacing="8">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE8A5;" 
                                         FontSize="16"
                                         Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                <TextBlock Text="Purchase Order Number" 
                                          Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                            </StackPanel>
                            
                            <TextBox x:Name="PoNumberTextBox"
                                    Text="{x:Bind ViewModel.PoNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" 
                                    PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PONumber'), Mode=OneWay}"
                                    MaxWidth="400"
                                    HorizontalAlignment="Stretch"
                                    TabIndex="0"
                                    LostFocus="POTextBox_LostFocus"
                                    AutomationProperties.Name="Purchase Order Number"/>
                            
                            <!-- Validation Message -->
                            <TextBlock Text="{x:Bind ViewModel.PoValidationMessage, Mode=OneWay}" 
                                      Foreground="{ThemeResource SystemFillColorCriticalBrush}"
                                      FontSize="12"
                                      Visibility="{x:Bind ViewModel.IsLoadPOEnabled, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}"/>
                            
                            <!-- PO Status Display -->
                            <Border BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                                    BorderThickness="1"
                                    CornerRadius="4"
                                    Padding="8,4"
                                    Margin="0,8,0,0"
                                    Background="{ThemeResource CardBackgroundFillColorSecondaryBrush}">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE8C9;" FontSize="14"/>
                                    <TextBlock Text="Status:" FontWeight="SemiBold" FontSize="12"/>
                                    <TextBlock Text="{x:Bind ViewModel.PoStatusDescription, Mode=OneWay}" 
                                              FontSize="12"
                                              FontWeight="SemiBold"/>
                                </StackPanel>
                            </Border>
                        </StackPanel>

                        <!-- Action Buttons -->
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <Button Grid.Column="0"
                                   Command="{x:Bind ViewModel.LoadPOCommand}" 
                                   IsEnabled="{x:Bind ViewModel.IsLoadPOEnabled, Mode=OneWay}"
                                   Style="{StaticResource AccentButtonStyle}"
                                   AutomationProperties.Name="Load Purchase Order"
                                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadPO'), Mode=OneWay}">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE896;" FontSize="16"/>
                                    <TextBlock Text="Load PO"/>
                                </StackPanel>
                            </Button>
                            
                            <Button Grid.Column="1"
                                   Content="Switch to Non-PO" 
                                   Command="{x:Bind ViewModel.ToggleNonPOCommand}"
                                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SwitchToNonPO'), Mode=OneWay}"/>
                        </Grid>
                    </StackPanel>
                </Border>
            </StackPanel>

            <!-- Non-PO Entry Mode -->
            <StackPanel Visibility="{x:Bind ViewModel.IsNonPOItem, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20">
                    <StackPanel Spacing="16">
                        <!-- Part ID Input with Icon -->
                        <StackPanel Spacing="8">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE8B7;" 
                                         FontSize="16"
                                         Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                <TextBlock Text="Part Identifier" 
                                          Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                            </StackPanel>
                            
                            <TextBox x:Name="PartIDTextBox"
                                    Text="{x:Bind ViewModel.PartID, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" 
                                    PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PartID'), Mode=OneWay}"
                                    MaxWidth="400"
                                    HorizontalAlignment="Stretch"
                                    AutomationProperties.Name="Part Identifier"/>
                        </StackPanel>

                        <!-- Package Type Display (Auto-detected) -->
                        <StackPanel Spacing="8">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE7B8;" 
                                         FontSize="16"
                                         Foreground="{ThemeResource AccentTextFillColorSecondaryBrush}"/>
                                <TextBlock Text="Package Type (Auto-detected)" 
                                          Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                            </StackPanel>
                            
                            <TextBlock Text="{x:Bind ViewModel.PackageType, Mode=OneWay}"
                                      Style="{ThemeResource SubtitleTextBlockStyle}"
                                      Foreground="{ThemeResource SystemAccentColor}"/>
                        </StackPanel>

                        <!-- Action Buttons -->
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <Button Grid.Column="0"
                                   Command="{x:Bind ViewModel.LookupPartCommand}" 
                                   Style="{StaticResource AccentButtonStyle}"
                                   AutomationProperties.Name="Look Up Part"
                                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LookupPart'), Mode=OneWay}">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE721;" FontSize="16"/>
                                    <TextBlock Text="Look Up Part"/>
                                </StackPanel>
                            </Button>
                            
                            <Button Grid.Column="1"
                                   Content="Switch to PO Entry" 
                                   Command="{x:Bind ViewModel.ToggleNonPOCommand}"
                                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SwitchToPO'), Mode=OneWay}"/>
                        </Grid>
                    </StackPanel>
                </Border>
            </StackPanel>
        </Grid>

        <!-- Loading Indicator -->
        <ProgressBar Grid.Row="0" 
                    IsIndeterminate="True" 
                    VerticalAlignment="Bottom"
                    Visibility="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                    Margin="0,0,0,-10"/>

        <!-- Parts Grid Section (Row 1) -->
        <Border Grid.Row="1" 
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="16"
                Visibility="{x:Bind ViewModel.IsPartsListVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>

                <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8" Margin="0,0,0,12">
                    <FontIcon Glyph="&#xE8FD;" 
                             FontSize="16"
                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                    <TextBlock Text="Available Parts" 
                              Style="{ThemeResource SubtitleTextBlockStyle}"/>
                </StackPanel>
                
                <controls:DataGrid Grid.Row="1"
                                  ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
                                  SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                                  AutoGenerateColumns="False"
                                  IsReadOnly="True"
                                  GridLinesVisibility="Horizontal"
                                  HeadersVisibility="Column"
                                  SelectionMode="Single"
                                  AutomationProperties.Name="Parts List">
                    <controls:DataGrid.Columns>
                        <controls:DataGridTextColumn Header="Part ID" 
                                                    Binding="{Binding PartID}" 
                                                    Width="150"/>
                        <controls:DataGridTextColumn Header="Description" 
                                                    Binding="{Binding Description}" 
                                                    Width="*"/>
                        <controls:DataGridTextColumn Header="Remaining Qty" 
                                                    Binding="{Binding RemainingQuantity}" 
                                                    Width="120"/>
                        <controls:DataGridTextColumn Header="Qty Ordered" 
                                                    Binding="{Binding QtyOrdered, Converter={StaticResource DecimalToIntConverter}}" 
                                                    Width="120"/>
                        <controls:DataGridTextColumn Header="Line #" 
                                                    Binding="{Binding POLineNumber}" 
                                                    Width="80"/>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>
            </Grid>
        </Border>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_POEntry.xaml.cs

```csharp
public sealed partial class View_Receiving_POEntry : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
_focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
⋮----
private void POTextBox_LostFocus(object sender, RoutedEventArgs e)
⋮----
ViewModel.PoTextBoxLostFocusCommand.Execute(null);
```

## File: Module_Receiving/Views/View_Receiving_Review.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_Review"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    Loaded="UserControl_Loaded">

    <UserControl.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Content Section with View Toggle -->
        <Grid Grid.Row="1">
            <!-- Single Entry View -->
            <Grid Visibility="{x:Bind ViewModel.IsSingleView, Mode=OneWay}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Entry Counter -->
                <Border Grid.Row="0" 
                       Background="{ThemeResource AccentFillColorDefaultBrush}"
                       Padding="12,6"
                       CornerRadius="8,8,0,0">
                    <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
                        <FontIcon Glyph="&#xE8FD;" 
                                 FontSize="16"
                                 Foreground="White"/>
                        <TextBlock Style="{ThemeResource SubtitleTextBlockStyle}"
                                  Foreground="White">
                            <Run Text="Entry"/>
                            <Run Text="{x:Bind ViewModel.DisplayIndex, Mode=OneWay}" 
                                 FontWeight="Bold"/>
                            <Run Text="of"/>
                            <Run Text="{x:Bind ViewModel.Loads.Count, Mode=OneWay}" 
                                 FontWeight="Bold"/>
                        </TextBlock>
                    </StackPanel>
                </Border>

                <!-- Single Entry Form -->
                <Grid Grid.Row="1">
                    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                           BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                           BorderThickness="1,0,1,1"
                           CornerRadius="0,0,8,8"
                           Padding="20">
                        <Grid MaxWidth="900">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <StackPanel Grid.Column="0" Spacing="12" Margin="0,0,12,0">
                            <!-- Load Number (Read-only) -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="Load Number" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.LoadNumber, Mode=OneWay}" 
                                        IsReadOnly="True"
                                        Background="{ThemeResource ControlFillColorDisabledBrush}"/>
                            </StackPanel>

                            <!-- PO Number -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE8A5;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Purchase Order Number" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.PoNumber, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>

                            <!-- Part ID -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE8B7;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Part ID" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.PartID, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>

                            <!-- Remaining Quantity -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE9F9;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Remaining Quantity" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.RemainingQuantity, Mode=OneWay}" 
                                        IsReadOnly="True"
                                        Foreground="{ThemeResource SystemAccentColor}"
                                        FontWeight="SemiBold"/>
                            </StackPanel>

                            <!-- Weight/Quantity -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xEA8B;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Weight/Quantity" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.WeightQuantity, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>
                        </StackPanel>

                        <StackPanel Grid.Column="1" Spacing="12" Margin="12,0,0,0">
                            <!-- Heat/Lot Number -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE8B4;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Heat/Lot Number" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.HeatLotNumber, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>

                            <!-- Packages Per Load -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Packages Per Load" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.PackagesPerLoad, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>

                            <!-- Package Type -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="Package Type" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.PackageTypeName, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>
                        </StackPanel>
                        </Grid>
                    </Border>
                </Grid>

                <!-- Navigation Bar for Single View -->
                <CommandBar Grid.Row="2" DefaultLabelPosition="Right" Margin="0,12,0,0">
                    <AppBarButton Icon="Back" 
                                 Label="Previous" 
                                 Command="{x:Bind ViewModel.PreviousEntryCommand}"
                                 IsEnabled="{x:Bind ViewModel.CanGoBack, Mode=OneWay}"
                                 AutomationProperties.Name="Previous Entry"/>
                    
                    <AppBarButton Icon="Forward" 
                                 Label="Next" 
                                 Command="{x:Bind ViewModel.NextEntryCommand}"
                                 IsEnabled="{x:Bind ViewModel.CanGoNext, Mode=OneWay}"
                                 AutomationProperties.Name="Next Entry"/>
                    
                    <AppBarSeparator/>
                    
                    <AppBarButton Icon="ViewAll" 
                                 Label="Table View" 
                                 Command="{x:Bind ViewModel.SwitchToTableViewCommand}"
                                 AutomationProperties.Name="Switch to Table View"/>
                </CommandBar>
            </Grid>

            <!-- Table View (DataGrid) -->
            <Grid Visibility="{x:Bind ViewModel.IsTableView, Mode=OneWay}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <Border Grid.Row="0"
                       Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                       BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                       BorderThickness="1"
                       CornerRadius="8"
                       Padding="16">
                    <controls:DataGrid x:Name="ReviewDataGrid"
                                      ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}"
                                      AutoGenerateColumns="False"
                                      GridLinesVisibility="Horizontal"
                                      HeadersVisibility="Column"
                                      CanUserSortColumns="False"
                                      SelectionMode="Single"
                                      AutomationProperties.Name="Receiving Entries Table">
                        <controls:DataGrid.Columns>
                            <controls:DataGridTextColumn Header="Load #" 
                                                        Binding="{Binding LoadNumber}" 
                                                        IsReadOnly="True" 
                                                        Width="80"/>
                            <controls:DataGridTextColumn Header="PO Number" 
                                                        Binding="{Binding PoNumber, TargetNullValue='N/A'}" 
                                                        Width="120"/>
                            <controls:DataGridTextColumn Header="Part ID" 
                                                        Binding="{Binding PartID}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Remaining Qty" 
                                                        Binding="{Binding RemainingQuantity}" 
                                                        Width="120"/>
                            <controls:DataGridTextColumn Header="Weight/Qty" 
                                                        Binding="{Binding WeightQuantity}" 
                                                        Width="120"/>
                            <controls:DataGridTextColumn Header="Heat/Lot #" 
                                                        Binding="{Binding HeatLotNumber}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Pkgs" 
                                                        Binding="{Binding PackagesPerLoad}" 
                                                        Width="80"/>
                            <controls:DataGridTextColumn Header="Pkg Type" 
                                                        Binding="{Binding PackageTypeName}" 
                                                        Width="100"/>
                        </controls:DataGrid.Columns>
                    </controls:DataGrid>
                </Border>

                <!-- Navigation Bar for Table View -->
                <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,16,0,0">
                    <AppBarButton Label="Single View" 
                                 Command="{x:Bind ViewModel.SwitchToSingleViewCommand}"
                                 AutomationProperties.Name="Switch to Single View">
                        <AppBarButton.Icon>
                            <FontIcon Glyph="&#xE8A9;"/>
                        </AppBarButton.Icon>
                    </AppBarButton>
                </CommandBar>
            </Grid>
        </Grid>

        <!-- Action Buttons -->
        <Grid Grid.Row="2" ColumnSpacing="12" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <Button Grid.Column="1" 
                   Command="{x:Bind ViewModel.AddAnotherPartCommand}"
                   AutomationProperties.Name="Add Another Part or PO"
                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddAnother'), Mode=OneWay}">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE710;" FontSize="16"/>
                    <TextBlock Text="Add Another Part/PO"/>
                </StackPanel>
            </Button>
            
            <Button Grid.Column="2" 
                   Command="{x:Bind ViewModel.SaveCommand}"
                   Style="{StaticResource AccentButtonStyle}"
                   AutomationProperties.Name="Save to Database"
                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveAndGenerate'), Mode=OneWay}">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE74E;" FontSize="16"/>
                    <TextBlock Text="Save to Database"/>
                </StackPanel>
            </Button>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_Review.xaml.cs

```csharp
public sealed partial class View_Receiving_Review : UserControl
⋮----
this.InitializeComponent();
⋮----
private async void UserControl_Loaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.OnNavigatedToAsync();
```

## File: Module_Receiving/Views/View_Receiving_WeightQuantity.xaml

```xml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_WeightQuantity"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_StringFormat x:Key="StringFormatConverter"/>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
        <converters:Converter_DoubleToDecimal x:Key="DoubleToDecimalConverter"/>
    </UserControl.Resources>

    <Grid Padding="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Info Header -->
        <Border Grid.Row="0" 
                Background="{ThemeResource AccentFillColorDefaultBrush}"
                Padding="16,12"
                CornerRadius="8"
                Margin="0,0,0,12">
            <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
                <FontIcon Glyph="&#xEA8B;" FontSize="16" Foreground="White"/>
                <TextBlock Text="{x:Bind ViewModel.PoQuantityInfo, Mode=OneWay}" 
                           Style="{StaticResource BodyStrongTextBlockStyle}"
                           Foreground="White"/>
            </StackPanel>
        </Border>

        <!-- Warning Banner -->
        <InfoBar Grid.Row="1"
                 IsOpen="{x:Bind ViewModel.HasWarning, Mode=OneWay}"
                 Severity="Warning"
                 Message="{x:Bind ViewModel.WarningMessage, Mode=OneWay}"
                 IsClosable="False"
                 Margin="0,0,0,12"/>

        <!-- Loads List with Fixed Height -->
        <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
            <ItemsControl ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Grid Margin="0,0,0,8" Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" Padding="16,12" CornerRadius="8" BorderThickness="1" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="120"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            
                            <StackPanel Grid.Column="0" VerticalAlignment="Center">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" FontSize="14" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="{Binding LoadNumber, Converter={StaticResource StringFormatConverter}, ConverterParameter='Load #{0}'}" 
                                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                            </StackPanel>
                            
                            <NumberBox Grid.Column="1"
                                       Header="Weight/Quantity"
                                       Value="{Binding WeightQuantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, Converter={StaticResource DoubleToDecimalConverter}}"
                                       Minimum="0"
                                       SmallChange="1"
                                       LargeChange="10"
                                       SpinButtonPlacementMode="Inline"
                                      PlaceholderText="Enter whole number"
                                       AutomationProperties.Name="Weight Quantity"/>
                        </Grid>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs

```csharp
public sealed partial class View_Receiving_WeightQuantity : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void WeightQuantityView_Loaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.OnNavigatedToAsync();
```

## File: Module_Receiving/Views/View_Receiving_Workflow.xaml

```xml
<Page
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_Workflow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels"
    xmlns:views="using:MTM_Receiving_Application.Module_Receiving.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <Page.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:Converter_BoolToString x:Key="BoolToStringConverter"/>
        <converters:Converter_StringFormat x:Key="StringFormatConverter"/>
    </Page.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Content Area -->
        <Grid Grid.Row="1" Margin="16,12,16,16">
            <!-- Mode Selection View -->
            <views:View_Receiving_ModeSelection Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            
            <!-- Manual Entry View -->
            <views:View_Receiving_ManualEntry Visibility="{x:Bind ViewModel.IsManualEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>

            <!-- Edit Mode View -->
            <views:View_Receiving_EditMode Visibility="{x:Bind ViewModel.IsEditModeVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>

            <!-- PO Entry View -->
            <views:View_Receiving_POEntry Visibility="{x:Bind ViewModel.IsPOEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_LoadEntry Visibility="{x:Bind ViewModel.IsLoadEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_WeightQuantity Visibility="{x:Bind ViewModel.IsWeightQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_HeatLot Visibility="{x:Bind ViewModel.IsHeatLotEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_PackageType Visibility="{x:Bind ViewModel.IsPackageTypeEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_Review Visibility="{x:Bind ViewModel.IsReviewVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            
            <!-- Saving Progress -->
            <StackPanel Visibility="{x:Bind ViewModel.IsSavingVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="16">
                <ProgressRing IsActive="True" Width="64" Height="64"/>
                <TextBlock Text="{x:Bind ViewModel.SaveProgressMessage, Mode=OneWay}" 
                           Style="{StaticResource SubtitleTextBlockStyle}"
                           HorizontalAlignment="Center"/>
                <ProgressBar Value="{x:Bind ViewModel.SaveProgressValue, Mode=OneWay}" Maximum="100" Width="300"/>
            </StackPanel>

            <!-- Completion Summary -->
            <StackPanel Visibility="{x:Bind ViewModel.IsCompleteVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="24" MaxWidth="600">
                
                <FontIcon Glyph="&#xE930;" FontSize="64" Foreground="{ThemeResource SystemFillColorSuccessBrush}" 
                          Visibility="{x:Bind ViewModel.LastSaveResult.Success, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                <FontIcon Glyph="&#xE783;" FontSize="64" Foreground="{ThemeResource SystemFillColorCriticalBrush}" 
                          Visibility="{x:Bind ViewModel.LastSaveResult.Success, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>

                <TextBlock Text="Success!" 
                           Style="{StaticResource TitleTextBlockStyle}" HorizontalAlignment="Center"
                           Visibility="{x:Bind ViewModel.LastSaveResult.Success, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                <TextBlock Text="Save Failed" 
                           Style="{StaticResource TitleTextBlockStyle}" HorizontalAlignment="Center"
                           Visibility="{x:Bind ViewModel.LastSaveResult.Success, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>

                <TextBlock Style="{StaticResource BodyStrongTextBlockStyle}" HorizontalAlignment="Center">
                    <Run Text="{x:Bind ViewModel.LastSaveResult.LoadsSaved, Mode=OneWay}"/>
                    <Run Text=" loads saved successfully."/>
                </TextBlock>

                <StackPanel Spacing="8" Background="{ThemeResource LayerFillColorDefaultBrush}" Padding="16" CornerRadius="8">
                    <TextBlock Text="Save Details:" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="Local CSV:"/>
                        <TextBlock Text="Saved" Foreground="Green" Visibility="{x:Bind ViewModel.LastSaveResult.LocalCSVSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                        <TextBlock Text="Failed" Foreground="Red" Visibility="{x:Bind ViewModel.LastSaveResult.LocalCSVSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>
                    </StackPanel>

                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="Network CSV:"/>
                        <TextBlock Text="Saved" Foreground="Green" Visibility="{x:Bind ViewModel.LastSaveResult.NetworkCSVSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                        <TextBlock Text="Failed" Foreground="Red" Visibility="{x:Bind ViewModel.LastSaveResult.NetworkCSVSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>
                    </StackPanel>

                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="Database:"/>
                        <TextBlock Text="Saved" Foreground="Green" Visibility="{x:Bind ViewModel.LastSaveResult.DatabaseSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                        <TextBlock Text="Failed" Foreground="Red" Visibility="{x:Bind ViewModel.LastSaveResult.DatabaseSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>
                    </StackPanel>
                </StackPanel>
                
                <StackPanel Orientation="Horizontal" Spacing="12" HorizontalAlignment="Center">
                    <Button Content="Start New Entry" Command="{x:Bind ViewModel.StartNewEntryCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.StartNew'), Mode=OneWay}"/>
                    <Button Content="Reset CSV" Command="{x:Bind ViewModel.ResetCSVCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ResetCSV'), Mode=OneWay}"/>
                </StackPanel>
            </StackPanel>
        </Grid>

        <!-- Navigation Buttons -->
        <Grid Grid.Row="2" Margin="24">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <!-- Left-aligned buttons -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" HorizontalAlignment="Left" Spacing="12">
                <Button Content="Mode Selection" 
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        Background="Transparent" 
                        BorderBrush="{ThemeResource SystemAccentColor}"/>
                <Button Content="Reset CSV" 
                        Command="{x:Bind ViewModel.ResetCSVCommand}" 
                        Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"
                        Background="Transparent" 
                        BorderBrush="Transparent" 
                        Foreground="Red"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ResetCSV'), Mode=OneWay}"/>
            </StackPanel>
            
            <!-- Center help button -->
            <Button Grid.Column="0"
                    Grid.ColumnSpan="2"
                    Canvas.ZIndex="1000"
                    HorizontalAlignment="Center"
                    Click="HelpButton_Click"
                    Style="{StaticResource AccentButtonStyle}"
                    Padding="12,8"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.StepHelp'), Mode=OneWay}">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE946;" FontSize="16"/>
                    <TextBlock Text="Help"/>
                </StackPanel>
            </Button>
            
            <!-- Right-aligned navigation buttons -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" HorizontalAlignment="Right" Spacing="12">
                <Button Content="Back" Command="{x:Bind ViewModel.PreviousStepCommand}"/>
                <Button Content="Next" Command="{x:Bind ViewModel.NextStepCommand}" Style="{StaticResource AccentButtonStyle}"/>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Reporting/Views/View_Reporting_Main.xaml

```xml
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

## File: Module_Routing/Enums/Enum_RoutingMode.cs

```csharp

```

## File: Module_Routing/Views/RoutingWizardContainerView.xaml.cs

```csharp
public sealed partial class RoutingWizardContainerView : Page
```

## File: Module_Routing/Views/RoutingWizardStep1View.xaml.cs

```csharp
public sealed partial class RoutingWizardStep1View : Page
```

## File: Module_Routing/Views/RoutingWizardStep3View.xaml.cs

```csharp
public sealed partial class RoutingWizardStep3View : Page
⋮----
private void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
ViewModel.LoadReviewData();
```

## File: Module_Settings/Enums/Enum_SettingsWorkflowStep.cs

```csharp

```

## File: Module_Settings/Interfaces/IService_UserPreferences.cs

```csharp
public interface IService_UserPreferences
⋮----
public Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username);
public Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode);
public Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string defaultMode);
public Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string defaultMode);
```

## File: Module_Settings/Services/Service_SettingsWorkflow.cs

```csharp
public class Service_SettingsWorkflow : IService_SettingsWorkflow
⋮----
private Enum_SettingsWorkflowStep _currentStep = Enum_SettingsWorkflowStep.ModeSelection;
public void GoToStep(Enum_SettingsWorkflowStep step)
⋮----
public void GoBack()
⋮----
public void Reset()
```

## File: Module_Shared/ViewModels/ViewModel_Shared_Base.cs

```csharp
public abstract partial class ViewModel_Shared_Base : ObservableObject
⋮----
protected readonly IService_ErrorHandler _errorHandler;
protected readonly IService_LoggingUtility _logger;
⋮----
private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;
⋮----
public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
```

## File: Module_Shared/ViewModels/ViewModel_Shared_HelpDialog.cs

```csharp
public partial class ViewModel_Shared_HelpDialog : ViewModel_Shared_Base
⋮----
private readonly IService_Help _helpService;
⋮----
public async Task LoadHelpContentAsync(Model_HelpContent content)
⋮----
IsDismissed = await _helpService.IsDismissedAsync(content.Key);
⋮----
RelatedTopics.Clear();
⋮----
var relatedContent = _helpService.GetHelpContent(relatedKey);
⋮----
RelatedTopics.Add(relatedContent);
⋮----
_errorHandler.HandleException(
⋮----
private async Task ViewRelatedTopicAsync()
⋮----
public async Task LoadRelatedTopicAsync(Model_HelpContent? relatedContent)
⋮----
private async Task CopyContentAsync()
⋮----
if (HelpContent != null && !string.IsNullOrEmpty(HelpContent.Content))
⋮----
dataPackage.SetText(HelpContent.Content);
Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
⋮----
await _logger.LogInfoAsync("Help content copied to clipboard");
⋮----
partial void OnIsDismissedChanged(bool value)
⋮----
if (HelpContent != null && !string.IsNullOrEmpty(HelpContent.Key))
⋮----
_ = _helpService.SetDismissedAsync(HelpContent.Key, value);
```

## File: Module_Shared/ViewModels/ViewModel_Shared_NewUserSetup.cs

```csharp
public partial class ViewModel_Shared_NewUserSetup : ViewModel_Shared_Base
⋮----
private readonly IService_Authentication _authService;
⋮----
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
⋮----
public async Task LoadDepartmentsAsync()
⋮----
var departmentList = await _authService.GetActiveDepartmentsAsync();
Departments.Clear();
⋮----
Departments.Add(dept);
⋮----
_logger.LogInfo($"Loaded {Departments.Count} departments for new user setup");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
public async Task<bool> CreateAccountAsync()
⋮----
if (!int.TryParse(EmployeeNumber, out int empNum) || empNum <= 0)
⋮----
var pinValidation = await _authService.ValidatePinAsync(Pin, empNum);
⋮----
var newUser = new Model_User
⋮----
var result = await _authService.CreateNewUserAsync(newUser, CreatedBy, progress);
⋮----
_logger.LogInfo($"New user account created: {FullName} (Emp #{NewEmployeeNumber}) by {CreatedBy}");
⋮----
_logger.LogWarning($"Failed to create account for {WindowsUsername}: {ErrorMessage}");
⋮----
public bool ValidatePinFormat(string pin)
⋮----
if (string.IsNullOrWhiteSpace(pin))
⋮----
if (!char.IsDigit(c))
⋮----
public bool ValidatePinMatch(string pin, string confirmPin)
⋮----
return !string.IsNullOrWhiteSpace(pin) && pin == confirmPin;
⋮----
public bool ValidateFullName(string fullName)
⋮----
return !string.IsNullOrWhiteSpace(fullName) && fullName.Trim().Length >= 2;
```

## File: Module_Shared/ViewModels/ViewModel_Shared_SharedTerminalLogin.cs

```csharp
public partial class ViewModel_Shared_SharedTerminalLogin : ViewModel_Shared_Base
⋮----
private readonly IService_Authentication _authService;
⋮----
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
⋮----
public async Task<bool> LoginAsync()
⋮----
if (string.IsNullOrWhiteSpace(Username))
⋮----
if (string.IsNullOrWhiteSpace(Pin))
⋮----
var result = await _authService.AuthenticateByPinAsync(Username, Pin, progress);
⋮----
_logger.LogInfo($"PIN authentication successful for user: {Username}");
⋮----
_logger.LogWarning($"PIN authentication failed for user: {Username}. Attempt {AttemptCount}");
⋮----
await _errorHandler.HandleErrorAsync(
```

## File: Module_Shared/ViewModels/ViewModel_Shared_SplashScreen.cs

```csharp
public partial class ViewModel_Shared_SplashScreen : ViewModel_Shared_Base
⋮----
public void UpdateProgress(double percentage, string message)
⋮----
public void SetIndeterminate(string message)
⋮----
public void Reset()
```

## File: Module_Shared/Views/View_Shared_HelpDialog.xaml

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_HelpDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:models="using:MTM_Receiving_Application.Module_Core.Models.Core"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="{x:Bind ViewModel.HelpContent.Title, Mode=OneWay}"
    PrimaryButtonText="Close"
    DefaultButton="Primary"
    Width="700"
    Height="600">

    <ContentDialog.Resources>
        <Style x:Key="HelpHeaderStyle" TargetType="TextBlock">
            <Setter Property="FontSize" Value="24"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Margin" Value="12,0,0,0"/>
        </Style>
        
        <Style x:Key="HelpContentStyle" TargetType="TextBlock">
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="TextWrapping" Value="Wrap"/>
            <Setter Property="LineHeight" Value="22"/>
        </Style>

        <Style x:Key="RelatedTopicStyle" TargetType="TextBlock">
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Margin" Value="8,4"/>
        </Style>
    </ContentDialog.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header Section -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,16">
            <mi:MaterialIcon 
                Kind="{x:Bind ViewModel.HelpContent.IconKind, Mode=OneWay}"
                Width="32" 
                Height="32"
                Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
            <TextBlock 
                Text="{x:Bind ViewModel.HelpContent.Title, Mode=OneWay}"
                Style="{StaticResource HelpHeaderStyle}"
                VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Content Section -->
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
            <TextBlock 
                Text="{x:Bind ViewModel.HelpContent.Content, Mode=OneWay}"
                Style="{StaticResource HelpContentStyle}"/>
        </ScrollViewer>

        <!-- Related Topics Section -->
        <Expander 
            Grid.Row="2"
            Header="Related Topics"
            Margin="0,16,0,0"
            HorizontalAlignment="Stretch"
            Visibility="{x:Bind ViewModel.IsRelatedHelpAvailable, Mode=OneWay}">
            <ListView 
                ItemsSource="{x:Bind ViewModel.RelatedTopics, Mode=OneWay}"
                SelectionMode="None"
                IsItemClickEnabled="True"
                ItemClick="RelatedTopics_ItemClick">
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:Model_HelpContent">
                        <StackPanel Orientation="Horizontal">
                            <mi:MaterialIcon 
                                Kind="{x:Bind IconKind}"
                                Width="20" 
                                Height="20"
                                Margin="0,0,8,0"/>
                            <TextBlock 
                                Text="{x:Bind Title}"
                                Style="{StaticResource RelatedTopicStyle}"/>
                        </StackPanel>
                    </DataTemplate>
                </ListView.ItemTemplate>
                <ListView.ItemContainerStyle>
                    <Style TargetType="ListViewItem">
                        <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
                    </Style>
                </ListView.ItemContainerStyle>
            </ListView>
        </Expander>

        <!-- Footer Section -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Stretch" Margin="0,12,0,0">
            <CheckBox 
                Content="Don't show this again"
                IsChecked="{x:Bind ViewModel.IsDismissed, Mode=TwoWay}"
                Visibility="{x:Bind ViewModel.CanDismiss, Mode=OneWay}"
                HorizontalAlignment="Left"/>
            
            <Button 
                Content="Copy"
                Command="{x:Bind ViewModel.CopyContentCommand}"
                HorizontalAlignment="Right"
                Margin="12,0,0,0">
                <Button.KeyboardAccelerators>
                    <KeyboardAccelerator Key="C" Modifiers="Control"/>
                </Button.KeyboardAccelerators>
            </Button>
        </StackPanel>
    </Grid>
</ContentDialog>
```

## File: Module_Shared/Views/View_Shared_HelpDialog.xaml.cs

```csharp
public sealed partial class View_Shared_HelpDialog : ContentDialog
⋮----
public void SetHelpContent(Model_HelpContent content)
⋮----
_ = ViewModel.LoadHelpContentAsync(content);
⋮----
private async void RelatedTopics_ItemClick(object sender, Microsoft.UI.Xaml.Controls.ItemClickEventArgs e)
⋮----
await ViewModel.LoadRelatedTopicAsync(relatedContent);
```

## File: Module_Shared/Views/View_Shared_IconSelectorWindow.xaml

```xml
<Window
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_IconSelectorWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    mc:Ignorable="d"
    Title="Select Icon">

    <Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Custom Title Bar -->
        <Grid x:Name="AppTitleBar" Grid.Row="0" Height="32" Background="Transparent">
            <TextBlock Text="Icon Library" 
                       VerticalAlignment="Center" 
                       Margin="16,0,0,0"
                       Style="{StaticResource CaptionTextBlockStyle}"/>
        </Grid>

        <!-- Header & Search -->
        <Grid Grid.Row="1" Padding="32,16,32,16" Background="{ThemeResource LayerFillColorDefaultBrush}" BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}" BorderThickness="0,0,0,1">
            <Grid ColumnSpacing="16">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                
                <TextBox x:Name="SearchBox" 
                         Grid.Column="0"
                         PlaceholderText="Search icons (e.g. 'box', 'truck')..." 
                         TextChanged="OnSearchTextChanged"
                         Height="36"
                         CornerRadius="4"/>
                         
                <CheckBox x:Name="ShowAllIconsCheckBox" 
                          Grid.Column="1"
                          Content="Show all icons" 
                          Checked="OnShowAllIconsChanged"
                          Unchecked="OnShowAllIconsChanged"
                          VerticalAlignment="Center"/>
            </Grid>
        </Grid>

        <!-- Icon Grid -->
        <Viewbox Grid.Row="2" Stretch="Uniform">
            <GridView x:Name="IconGridView" 
                      Padding="16"
                      SelectionMode="Single"
                      IsItemClickEnabled="True"
                      ItemClick="OnIconItemClick"
                      ScrollViewer.VerticalScrollBarVisibility="Disabled"
                      Width="900" Height="650">
                <GridView.ItemsPanel>
                    <ItemsPanelTemplate>
                        <ItemsWrapGrid MaximumRowsOrColumns="4" Orientation="Horizontal" HorizontalAlignment="Center"/>
                    </ItemsPanelTemplate>
                </GridView.ItemsPanel>
                <GridView.ItemTemplate>
                    <DataTemplate>
                        <Border Width="200" Height="140"
                                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                                BorderThickness="1" 
                                CornerRadius="8"
                                Margin="8"
                                ToolTipService.ToolTip="{Binding Name}">
                            <VisualStateManager.VisualStateGroups>
                                <VisualStateGroup x:Name="CommonStates">
                                    <VisualState x:Name="Normal" />
                                    <VisualState x:Name="PointerOver">
                                        <Storyboard>
                                            <ObjectAnimationUsingKeyFrames Storyboard.TargetName="ContentPanel" Storyboard.TargetProperty="Background">
                                                <DiscreteObjectKeyFrame KeyTime="0" Value="{ThemeResource CardBackgroundFillColorSecondaryBrush}" />
                                            </ObjectAnimationUsingKeyFrames>
                                        </Storyboard>
                                    </VisualState>
                                </VisualStateGroup>
                            </VisualStateManager.VisualStateGroups>
                            
                            <StackPanel x:Name="ContentPanel" VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="12" Padding="12">
                                <materialIcons:MaterialIcon Kind="{Binding Kind}" 
                                          Width="40" Height="40"
                                          Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                                <TextBlock Text="{Binding Name}" 
                                           FontSize="14" 
                                           TextWrapping="Wrap" 
                                           TextAlignment="Center"
                                           HorizontalAlignment="Center"
                                           MaxLines="2"
                                           TextTrimming="CharacterEllipsis"/>
                            </StackPanel>
                        </Border>
                    </DataTemplate>
                </GridView.ItemTemplate>
            </GridView>
        </Viewbox>

        <!-- Footer -->
        <Grid Grid.Row="3" Padding="32,20" Background="{ThemeResource LayerFillColorDefaultBrush}" BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}" BorderThickness="0,1,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <Button x:Name="CancelButton" Grid.Column="0" Content="Cancel" Click="OnCancelClick" Width="100" Height="36" CornerRadius="4" HorizontalAlignment="Left"/>
            
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
                <Button x:Name="FirstPageButton" Click="OnFirstPageClick" Width="44" Height="36" CornerRadius="4" ToolTipService.ToolTip="First Page">
                    <materialIcons:MaterialIcon Kind="PageFirst" Width="16" Height="16"/>
                </Button>
                <Button x:Name="PreviousPageButton" Click="OnPreviousPageClick" Width="44" Height="36" CornerRadius="4" ToolTipService.ToolTip="Previous Page">
                    <materialIcons:MaterialIcon Kind="ChevronLeft" Width="16" Height="16"/>
                </Button>
                
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" CornerRadius="4" Padding="16,0" Height="36" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" BorderThickness="1">
                    <TextBlock x:Name="PageInfoTextBlock" VerticalAlignment="Center" Text="Page 1 of 1" FontWeight="SemiBold"/>
                </Border>
                
                <Button x:Name="NextPageButton" Click="OnNextPageClick" Width="44" Height="36" CornerRadius="4" ToolTipService.ToolTip="Next Page">
                    <materialIcons:MaterialIcon Kind="ChevronRight" Width="16" Height="16"/>
                </Button>
                <Button x:Name="LastPageButton" Click="OnLastPageClick" Width="44" Height="36" CornerRadius="4" ToolTipService.ToolTip="Last Page">
                    <materialIcons:MaterialIcon Kind="PageLast" Width="16" Height="16"/>
                </Button>
            </StackPanel>
        </Grid>
    </Grid>
</Window>
```

## File: Module_Shared/Views/View_Shared_IconSelectorWindow.xaml.cs

```csharp
public sealed partial class View_Shared_IconSelectorWindow : Window
⋮----
Debug.WriteLine($"Failed to get logging service: {ex.Message}");
⋮----
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
⋮----
appWindow.Resize(windowSize);
var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
⋮----
appWindow.Move(new Windows.Graphics.PointInt32 { X = centerX, Y = centerY });
⋮----
public void SetInitialSelection(MaterialIconKind iconKind)
⋮----
DispatcherQueue.TryEnqueue(() =>
⋮----
var iconInfo = _allIcons.FirstOrDefault(i => i.Kind == iconKind);
⋮----
IconGridView.ScrollIntoView(iconInfo);
⋮----
public Task<MaterialIconKind?> WaitForSelectionAsync()
⋮----
private void LoadIcons()
⋮----
var icons = Helper_MaterialIcons.GetAllIcons();
_allIcons = icons.ConvertAll(k => new IconInfo(k.ToString(), k));
⋮----
.Where(i => commonIconNames.Contains(i.Name))
.ToList();
⋮----
Debug.WriteLine($"Error loading icons: {ex}");
⋮----
private void UpdateGridView()
⋮----
_totalPages = (int)Math.Ceiling((double)_filteredIcons.Count / ICONS_PER_PAGE);
⋮----
.Skip((_currentPage - 1) * ICONS_PER_PAGE)
.Take(ICONS_PER_PAGE)
⋮----
private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
⋮----
var searchText = SearchBox.Text.ToLower().Trim();
⋮----
if (string.IsNullOrWhiteSpace(searchText))
⋮----
.Where(i => i.Name.ToLower().Contains(searchText))
⋮----
private void OnShowAllIconsChanged(object sender, RoutedEventArgs e)
⋮----
private void OnIconItemClick(object sender, ItemClickEventArgs e)
⋮----
private void OnPreviousPageClick(object sender, RoutedEventArgs e)
⋮----
private void OnNextPageClick(object sender, RoutedEventArgs e)
⋮----
private void OnFirstPageClick(object sender, RoutedEventArgs e)
⋮----
private void OnLastPageClick(object sender, RoutedEventArgs e)
⋮----
private void OnCancelClick(object sender, RoutedEventArgs e)
⋮----
public class IconInfo
```

## File: Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_NewUserSetupDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Create Your Account"
    PrimaryButtonText="Create Account"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonStyle="{StaticResource AccentButtonStyle}"
    Width="480">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header with Icon and Welcome -->
        <StackPanel Grid.Row="0" Spacing="8" Padding="20,16,20,12" Background="{ThemeResource LayerFillColorDefaultBrush}">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                
                <FontIcon Grid.Column="0" 
                          FontFamily="{StaticResource SymbolThemeFontFamily}" 
                          Glyph="&#xE77B;" 
                          FontSize="32"
                          Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                          VerticalAlignment="Center"
                          Margin="0,0,16,0"/>
                
                <StackPanel Grid.Column="1" VerticalAlignment="Center">
                    <TextBlock Text="Welcome to MTM Receiving" 
                               Style="{StaticResource SubtitleTextBlockStyle}"
                               FontWeight="SemiBold"/>
                    <TextBlock Text="Let's set up your account to get started" 
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                               Style="{StaticResource CaptionTextBlockStyle}"/>
                </StackPanel>
            </Grid>
        </StackPanel>

        <!-- Main Content - Using Grid for compact two-column layout with ScrollViewer -->
        <ScrollViewer Grid.Row="1" 
                      VerticalScrollBarVisibility="Auto" 
                      HorizontalScrollBarVisibility="Disabled"
                      MaxHeight="480">
            <Grid Padding="20,12" RowSpacing="12" ColumnSpacing="12">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

            <!-- Row 0: Full Name (spans both columns) -->
            <StackPanel Grid.Row="0" Grid.ColumnSpan="2" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE77B;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Full Name *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <TextBox x:Name="FullNameTextBox"
                         PlaceholderText="Enter your full name"
                         MaxLength="100"
                         TabIndex="0"/>
            </StackPanel>

            <!-- Row 1: Employee Number & Windows Username -->
            <StackPanel Grid.Row="1" Grid.Column="0" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE8D4;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Employee Number *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <TextBox x:Name="EmployeeNumberTextBox"
                         PlaceholderText="e.g., 6229"
                         MaxLength="10"
                         TabIndex="1"
                         InputScope="Number"/>
            </StackPanel>

            <StackPanel Grid.Row="1" Grid.Column="1" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE910;" 
                              FontSize="16"
                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    <TextBlock Text="Windows Username" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                </StackPanel>
                <TextBox x:Name="WindowsUsernameTextBox"
                         IsReadOnly="True"
                         Background="{ThemeResource ControlAltFillColorSecondaryBrush}"
                         TabIndex="-1"/>
            </StackPanel>

            <!-- Row 2: Department & Shift -->
            <StackPanel Grid.Row="2" Grid.Column="0" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE716;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Department *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <ComboBox x:Name="DepartmentComboBox"
                          PlaceholderText="Select department"
                          HorizontalAlignment="Stretch"
                          TabIndex="2"
                          SelectionChanged="DepartmentComboBox_SelectionChanged"/>
            </StackPanel>

            <StackPanel Grid.Row="2" Grid.Column="1" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE823;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Shift *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <ComboBox x:Name="ShiftComboBox"
                          PlaceholderText="Select shift"
                          HorizontalAlignment="Stretch"
                          TabIndex="4">
                    <ComboBoxItem Content="1st Shift"/>
                    <ComboBoxItem Content="2nd Shift"/>
                    <ComboBoxItem Content="3rd Shift"/>
                </ComboBox>
            </StackPanel>

            <!-- Row 3: Custom Department (Visible when "Other" selected, spans both columns) -->
            <StackPanel x:Name="CustomDepartmentPanel" 
                        Grid.Row="3" 
                        Grid.ColumnSpan="2" 
                        Spacing="6" 
                        Visibility="Collapsed">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE70F;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Custom Department Name *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <TextBox x:Name="CustomDepartmentTextBox"
                         PlaceholderText="Enter department name"
                         MaxLength="50"
                         TabIndex="3"/>
            </StackPanel>

            <!-- Row 4: PIN Fields -->
            <StackPanel Grid.Row="4" Grid.Column="0" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE72E;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="4-Digit PIN *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <PasswordBox x:Name="PinPasswordBox"
                             PlaceholderText="Create PIN"
                             MaxLength="4"
                             TabIndex="5"
                             PasswordRevealMode="Peek"/>
                <TextBlock Text="For shared terminal login" 
                           Foreground="{ThemeResource TextFillColorTertiaryBrush}"
                           Style="{StaticResource CaptionTextBlockStyle}"/>
            </StackPanel>

            <StackPanel Grid.Row="4" Grid.Column="1" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE8C9;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Confirm PIN *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <PasswordBox x:Name="ConfirmPinPasswordBox"
                             PlaceholderText="Re-enter PIN"
                             MaxLength="4"
                             TabIndex="6"
                             PasswordRevealMode="Peek"/>
            </StackPanel>

            <!-- Row 5: ERP Credentials Section (Optional) -->
            <Expander x:Name="ErpExpander"
                      Grid.Row="5" 
                      Grid.ColumnSpan="2"
                      Header="ERP System Access (Optional)"
                      IsExpanded="False"
                      HorizontalAlignment="Stretch"
                      HorizontalContentAlignment="Stretch"
                      VerticalContentAlignment="Stretch">
                <Expander.HeaderTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                                      Glyph="&#xE89F;" 
                                      FontSize="16"/>
                            <TextBlock Text="{Binding}" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        </StackPanel>
                    </DataTemplate>
                </Expander.HeaderTemplate>
                
                <StackPanel Spacing="12" Padding="12,8,0,0">
                    <CheckBox x:Name="ConfigureErpCheckBox"
                              Content="Configure Visual/Infor ERP credentials"
                              Checked="ConfigureErpCheckBox_Checked"
                              Unchecked="ConfigureErpCheckBox_Unchecked"
                              TabIndex="7"/>

                    <StackPanel x:Name="ErpCredentialsPanel" Spacing="12" Visibility="Collapsed">
                        <InfoBar Severity="Warning" 
                                 IsOpen="True" 
                                 IsClosable="False"
                                 Message="Credentials are stored in plain text for ERP integration."/>
                        
                        <StackPanel Spacing="6">
                            <TextBlock Text="Visual/Infor Username" 
                                       Style="{StaticResource BodyStrongTextBlockStyle}"/>
                            <TextBox x:Name="VisualUsernameTextBox"
                                     PlaceholderText="ERP username"
                                     MaxLength="50"
                                     TabIndex="8"/>
                        </StackPanel>

                        <StackPanel Spacing="6">
                            <TextBlock Text="Visual/Infor Password" 
                                       Style="{StaticResource BodyStrongTextBlockStyle}"/>
                            <PasswordBox x:Name="VisualPasswordBox"
                                         PlaceholderText="ERP password"
                                         MaxLength="100"
                                         TabIndex="9"
                                         PasswordRevealMode="Peek"/>
                        </StackPanel>
                    </StackPanel>
                </StackPanel>
            </Expander>
            </Grid>
        </ScrollViewer>

        <!-- Footer: Status Bar & Progress -->
        <StackPanel Grid.Row="2" Spacing="6" Padding="20,8,20,16">
            <ProgressBar x:Name="LoadingProgressBar" 
                         IsIndeterminate="True" 
                         Visibility="Collapsed"
                         Height="4"/>
            
            <InfoBar x:Name="StatusInfoBar"
                     IsOpen="False"
                     IsClosable="True"/>
        </StackPanel>
    </Grid>
</ContentDialog>
```

## File: Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml.cs

```csharp
public sealed partial class View_Shared_NewUserSetupDialog : ContentDialog
⋮----
ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
⋮----
private void OnDialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
⋮----
private async void OnDialogLoaded(object sender, RoutedEventArgs e)
⋮----
FullNameTextBox.Focus(FocusState.Programmatic);
⋮----
private async System.Threading.Tasks.Task LoadDepartmentsAsync()
⋮----
await ViewModel.LoadDepartmentsAsync();
DepartmentComboBox.Items.Clear();
⋮----
DepartmentComboBox.Items.Add(dept);
⋮----
DepartmentComboBox.Items.Add("Other");
⋮----
private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
⋮----
string selectedDept = DepartmentComboBox.SelectedItem.ToString() ?? string.Empty;
⋮----
CustomDepartmentTextBox.Focus(FocusState.Programmatic);
⋮----
private void ConfigureErpCheckBox_Checked(object sender, RoutedEventArgs e)
⋮----
_ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
⋮----
VisualUsernameTextBox.Focus(FocusState.Programmatic);
⋮----
private void ConfigureErpCheckBox_Unchecked(object sender, RoutedEventArgs e)
⋮----
private async void OnCreateAccountButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
if (string.IsNullOrWhiteSpace(employeeNumber))
⋮----
EmployeeNumberTextBox.Focus(FocusState.Programmatic);
⋮----
if (!int.TryParse(employeeNumber, out int empNum) || empNum <= 0)
⋮----
if (string.IsNullOrWhiteSpace(fullName))
⋮----
if (string.IsNullOrWhiteSpace(department))
⋮----
DepartmentComboBox.Focus(FocusState.Programmatic);
⋮----
if (string.IsNullOrWhiteSpace(shift))
⋮----
ShiftComboBox.Focus(FocusState.Programmatic);
⋮----
if (string.IsNullOrWhiteSpace(pin))
⋮----
PinPasswordBox.Focus(FocusState.Programmatic);
⋮----
if (pin.Length != 4 || !pin.All(char.IsDigit))
⋮----
ConfirmPinPasswordBox.Focus(FocusState.Programmatic);
⋮----
bool success = await ViewModel.CreateAccountAsync();
⋮----
await System.Threading.Tasks.Task.Delay(2000);
⋮----
private void OnCancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
private void ShowValidationError(string message)
⋮----
private void SetLoadingState(bool isLoading)
```

## File: Module_Shared/Views/View_Shared_SharedTerminalLoginDialog.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_SharedTerminalLoginDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Shared Terminal Login"
    PrimaryButtonText="Login"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonStyle="{StaticResource AccentButtonStyle}"
    Width="400">

    <StackPanel Spacing="16" Padding="8">
        <!-- Instructions -->
        <TextBlock 
            Text="Please enter your credentials to continue"
            TextWrapping="Wrap"
            Style="{StaticResource BodyTextBlockStyle}"/>

        <!-- Username Field -->
        <StackPanel Spacing="4">
            <TextBlock Text="Username" Style="{StaticResource CaptionTextBlockStyle}"/>
            <TextBox 
                x:Name="UsernameTextBox"
                PlaceholderText="Enter your username"
                MaxLength="50"
                TabIndex="0"/>
        </StackPanel>

        <!-- PIN Field -->
        <StackPanel Spacing="4">
            <TextBlock Text="4-Digit PIN" Style="{StaticResource CaptionTextBlockStyle}"/>
            <PasswordBox 
                x:Name="PinPasswordBox"
                PlaceholderText="Enter 4-digit PIN"
                MaxLength="4"
                TabIndex="1"
                PasswordRevealMode="Peek"/>
        </StackPanel>

        <!-- Attempt Counter (Hidden initially, shown after first failure) -->
        <TextBlock 
            x:Name="AttemptCounterTextBlock"
            Foreground="Gold"
            Visibility="Collapsed"
            TextWrapping="Wrap"
            Style="{StaticResource CaptionTextBlockStyle}"/>

        <!-- Error InfoBar -->
        <InfoBar 
            x:Name="ErrorInfoBar"
            Severity="Error"
            IsOpen="False"
            IsClosable="True"
            Title="Login Failed"
            Message="Invalid username or PIN. Please try again."/>

    </StackPanel>
</ContentDialog>
```

## File: Module_Shared/Views/View_Shared_SharedTerminalLoginDialog.xaml.cs

```csharp
public sealed partial class View_Shared_SharedTerminalLoginDialog : ContentDialog
⋮----
private readonly IService_Focus _focusService;
⋮----
ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
⋮----
_focusService.AttachFocusOnVisibility(this, UsernameTextBox);
⋮----
private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
if (string.IsNullOrWhiteSpace(username))
⋮----
UsernameTextBox.Focus(FocusState.Programmatic);
⋮----
if (string.IsNullOrWhiteSpace(pin))
⋮----
PinPasswordBox.Focus(FocusState.Programmatic);
⋮----
if (pin.Length != 4 || !int.TryParse(pin, out _))
⋮----
bool success = await ViewModel.LoginAsync();
⋮----
await System.Threading.Tasks.Task.Delay(5000);
⋮----
private void OnCloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
```

## File: Module_Shared/Views/View_Shared_SplashScreenWindow.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_SplashScreenWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="MTM Receiving Application"   
    >

    <Grid Background="{ThemeResource LayerFillColorDefaultBrush}" Padding="32">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Logo and Branding -->
        <StackPanel Grid.Row="1" HorizontalAlignment="Center" Spacing="12">
            <FontIcon FontFamily="Segoe Fluent Icons" 
                      Glyph="&#xE8F1;" 
                      FontSize="64"
                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                      HorizontalAlignment="Center"/>
            
            <TextBlock Text="MTM Receiving Application" 
                       Style="{StaticResource TitleTextBlockStyle}"
                       HorizontalAlignment="Center"
                       FontWeight="SemiBold"/>
            
            <TextBlock Text="Version 1.0.0"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       HorizontalAlignment="Center"/>
            
            <TextBlock Text="MTM Manufacturing"
                       Style="{StaticResource BodyTextBlockStyle}"
                       Foreground="{ThemeResource TextFillColorTertiaryBrush}"
                       HorizontalAlignment="Center"/>
        </StackPanel>

        <!-- Progress Section -->
        <StackPanel Grid.Row="2" Margin="0,40,0,0" Spacing="12">
            <TextBlock x:Name="StatusMessageTextBlock"
                       Text="Initializing..."
                       Style="{StaticResource BodyTextBlockStyle}"
                       HorizontalAlignment="Center"
                       TextAlignment="Center"
                       TextWrapping="Wrap"/>

            <ProgressBar x:Name="MainProgressBar"
                         Height="4"
                         Minimum="0"
                         Maximum="100"
                         Value="0"
                         HorizontalAlignment="Stretch"/>

            <TextBlock x:Name="ProgressPercentageTextBlock"
                       Text="0%"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       HorizontalAlignment="Center"/>
        </StackPanel>

        <!-- Copyright -->
        <TextBlock Grid.Row="3"
                   Text="© 2025 MTM Manufacturing. All rights reserved."
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorTertiaryBrush}"
                   HorizontalAlignment="Center"
                   Margin="0,28,0,0"/>
    </Grid>
</Window>
```

## File: Module_Shared/Views/View_Shared_SplashScreenWindow.xaml.cs

```csharp
public sealed partial class View_Shared_SplashScreenWindow : Window
⋮----
private void SplashScreenWindow_Closed(object sender, WindowEventArgs args)
⋮----
Application.Current.Exit();
⋮----
private void ConfigureWindow()
⋮----
this.UseCustomTitleBar();
this.HideTitleBarIcon();
this.MakeTitleBarTransparent();
this.SetWindowSize(WindowWidth, WindowHeight);
this.SetFixedSize(disableMaximize: true, disableMinimize: true);
this.CenterOnScreen();
⋮----
private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
⋮----
DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
```

## File: Module_Volvo/Views/View_Volvo_History.xaml.cs

```csharp
public sealed partial class View_Volvo_History : Page
⋮----
private async void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
await ViewModel.FilterCommand.ExecuteAsync(null);
```

## File: Module_Volvo/Views/View_Volvo_Settings.xaml.cs

```csharp
public sealed partial class View_Volvo_Settings : Page
⋮----
private async void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
await ViewModel.RefreshCommand.ExecuteAsync(null);
```

## File: Module_Core/Behaviors/AuditBehavior.cs

```csharp
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
⋮----
public async Task<TResponse> Handle(
⋮----
_logger.LogInformation(
```

## File: Module_Core/Behaviors/LoggingBehavior.cs

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
⋮----
public async Task<TResponse> Handle(
⋮----
var requestGuid = Guid.NewGuid().ToString();
_logger.LogInformation(
⋮----
var stopwatch = Stopwatch.StartNew();
⋮----
stopwatch.Stop();
⋮----
_logger.LogError(
```

## File: Module_Core/Behaviors/ValidationBehavior.cs

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
⋮----
public async Task<TResponse> Handle(
⋮----
if (!_validators.Any())
⋮----
var validationResults = await Task.WhenAll(
_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
⋮----
.SelectMany(r => r.Errors)
.Where(f => f != null)
.ToList();
if (failures.Any())
⋮----
throw new ValidationException(failures);
```

## File: Module_Core/Contracts/Services/IService_Focus.cs

```csharp
public interface IService_Focus
⋮----
public void SetFocus(Control control);
public void SetFocusFirstInput(DependencyObject container);
public void AttachFocusOnVisibility(FrameworkElement view, Control? targetControl = null);
```

## File: Module_Core/Contracts/ViewModels/IResettableViewModel.cs

```csharp
public interface IResettableViewModel
⋮----
public void ResetToDefaults();
```

## File: Module_Core/Converters/Converter_BooleanToVisibility.cs

```csharp
public class Converter_BooleanToVisibility : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
if (parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Core/Converters/Converter_BoolToString.cs

```csharp
public class Converter_BoolToString : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
var parts = paramString.Split('|');
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Core/Converters/Converter_DecimalToInt.cs

```csharp
public class Converter_DecimalToInt : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
return ((int)decimalValue).ToString("N0");
⋮----
return ((int)doubleValue).ToString("N0");
⋮----
return ((int)floatValue).ToString("N0");
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
if (value is string strValue && decimal.TryParse(strValue, out decimal result))
```

## File: Module_Core/Converters/Converter_DecimalToString.cs

```csharp
public class Converter_DecimalToString : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
return d == 0 ? string.Empty : d.ToString("G29");
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
if (string.IsNullOrWhiteSpace(s))
⋮----
if (decimal.TryParse(s, out decimal result))
```

## File: Module_Core/Converters/Converter_DoubleToDecimal.cs

```csharp
public class Converter_DoubleToDecimal : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
return System.Convert.ToDouble(d);
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
return System.Convert.ToDecimal(d);
```

## File: Module_Core/Converters/Converter_EmptyStringToVisibility.cs

```csharp
public class Converter_EmptyStringToVisibility : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
bool isVisible = !string.IsNullOrWhiteSpace(stringValue);
if (parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Core/Converters/Converter_IconCodeToGlyph.cs

```csharp
public class Converter_IconCodeToGlyph : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
if (value is string iconCode && !string.IsNullOrWhiteSpace(iconCode))
⋮----
if (iconCode.StartsWith("&#x") && iconCode.EndsWith(";"))
⋮----
string hex = iconCode.Substring(3, iconCode.Length - 4);
if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
⋮----
return ((char)code).ToString();
⋮----
if (iconCode.Length == 4 && int.TryParse(iconCode, System.Globalization.NumberStyles.HexNumber, null, out int rawCode))
⋮----
return ((char)rawCode).ToString();
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Core/Converters/Converter_LoadNumberToOneBased.cs

```csharp
public class Converter_LoadNumberToOneBased : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
if (value is string strValue && int.TryParse(strValue, out int displayNumber))
```

## File: Module_Core/Converters/Converter_StringFormat.cs

```csharp
public class Converter_StringFormat : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
return string.Format(format, value);
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Core/Defaults/InforVisualDefaults.cs

```csharp
public static class InforVisualDefaults
```

## File: Module_Core/Defaults/WorkstationDefaults.cs

```csharp
public static class WorkstationDefaults
```

## File: Module_Core/Models/Core/Model_Dao_Result_Factory.cs

```csharp
public static class Model_Dao_Result_Factory
⋮----
public static Model_Dao_Result Failure(string? message, Exception? ex = null)
⋮----
return new Model_Dao_Result
⋮----
ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Operation failed." : message,
⋮----
public static Model_Dao_Result Success(int affectedRows = 0)
⋮----
AffectedRows = Math.Max(0, affectedRows),
⋮----
public static Model_Dao_Result<T> Failure<T>(string? message, Exception? ex = null)
⋮----
public static Model_Dao_Result<T> Success<T>(T data, int affectedRows = 0)
```

## File: Module_Core/Models/InforVisual/Model_InforVisualConnection.cs

```csharp
public class Model_InforVisualConnection
⋮----
public string GetConnectionString()
```

## File: Module_Core/Models/InforVisual/Model_InforVisualPart.cs

```csharp
public partial class Model_InforVisualPart : ObservableObject
```

## File: Module_Core/Models/InforVisual/Model_InforVisualPO.cs

```csharp
public partial class Model_InforVisualPO : ObservableObject
```

## File: Module_Core/Models/Reporting/Model_ReportRow.cs

```csharp
public class Model_ReportRow
```

## File: Module_Core/Models/Systems/Model_AuthenticationResult.cs

```csharp
public class Model_AuthenticationResult
⋮----
public static Model_AuthenticationResult SuccessResult(Model_User user)
⋮----
ArgumentNullException.ThrowIfNull(user);
return new Model_AuthenticationResult
⋮----
public static Model_AuthenticationResult ErrorResult(string message)
⋮----
if (string.IsNullOrWhiteSpace(message))
⋮----
throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));
```

## File: Module_Core/Models/Systems/Model_UserSession.cs

```csharp
public class Model_UserSession
⋮----
public void UpdateLastActivity()
⋮----
ArgumentNullException.ThrowIfNull(user);
```

## File: Module_Core/Models/Systems/Model_ValidationResult.cs

```csharp
public class Model_ValidationResult
⋮----
public static Model_ValidationResult Valid() => new()
⋮----
public static Model_ValidationResult Invalid(string? message) => new()
⋮----
ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Validation failed." : message
```

## File: Module_Core/Services/Authentication/Service_UserSessionManager.cs

```csharp
public class Service_UserSessionManager : IService_UserSessionManager
⋮----
private readonly Dao_User _daoUser;
private readonly IService_Dispatcher _dispatcherService;
⋮----
_daoUser = daoUser ?? throw new ArgumentNullException(nameof(daoUser));
_dispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
⋮----
public Model_UserSession CreateSession(
⋮----
throw new ArgumentNullException(nameof(user));
⋮----
throw new ArgumentNullException(nameof(workstationConfig));
⋮----
CurrentSession = new Model_UserSession
⋮----
public void UpdateLastActivity()
⋮----
public void StartTimeoutMonitoring()
⋮----
throw new InvalidOperationException("Cannot start timeout monitoring without an active session");
⋮----
_timeoutTimer = _dispatcherService.CreateTimer();
_timeoutTimer.Interval = TimeSpan.FromSeconds(TimerIntervalSeconds);
⋮----
_timeoutTimer.Start();
System.Diagnostics.Debug.WriteLine($"Session timeout monitoring started. Timeout: {CurrentSession.TimeoutDuration.TotalMinutes} minutes");
⋮----
public void StopTimeoutMonitoring()
⋮----
_timeoutTimer.Stop();
⋮----
System.Diagnostics.Debug.WriteLine("Session timeout monitoring stopped");
⋮----
public bool IsSessionTimedOut()
⋮----
public async Task EndSessionAsync(string reason)
⋮----
await _daoUser.LogUserActivityAsync(
⋮----
System.Diagnostics.Debug.WriteLine($"Session ended. Reason: {reason}");
⋮----
System.Diagnostics.Debug.WriteLine($"Failed to log session end: {ex.Message}");
⋮----
private void OnTimerTick(object? sender, object args)
⋮----
System.Diagnostics.Debug.WriteLine(
⋮----
SessionTimedOut?.Invoke(this, new Model_SessionTimedOutEventArgs
```

## File: Module_Core/Services/Database/Service_LoggingUtility.cs

```csharp
public class Service_LoggingUtility : IService_LoggingUtility
⋮----
_logDirectory = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
⋮----
System.Threading.Tasks.Task.Factory.StartNew(ProcessLogQueue,
⋮----
private void ProcessLogQueue()
⋮----
foreach (var logEntry in _logQueue.GetConsumingEnumerable())
⋮----
File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
⋮----
System.Diagnostics.Debug.WriteLine($"Background logging failed: {ex.Message}");
⋮----
public void LogInfo(string message, string? context = null)
⋮----
public void LogWarning(string message, string? context = null)
⋮----
public void LogError(string message, Exception? exception = null, string? context = null)
⋮----
public void LogCritical(string message, Exception? exception = null, string? context = null)
⋮----
public void LogFatal(string message, Exception? exception = null, string? context = null)
⋮----
public Task LogInfoAsync(string message, string? context = null)
⋮----
public Task LogErrorAsync(string message, Exception? exception = null, string? context = null)
⋮----
public string GetCurrentLogFilePath()
⋮----
return Path.Combine(_logDirectory, fileName);
⋮----
public bool EnsureLogDirectoryExists()
⋮----
if (!Directory.Exists(_logDirectory))
⋮----
Directory.CreateDirectory(_logDirectory);
⋮----
System.Diagnostics.Debug.WriteLine($"Failed to create log directory: {ex.Message}");
⋮----
public int ArchiveOldLogs(int daysToKeep = 30)
⋮----
var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
var archiveDirectory = Path.Combine(_logDirectory, "archive");
foreach (var file in Directory.GetFiles(_logDirectory, "app_*.log"))
⋮----
var fileInfo = new FileInfo(file);
⋮----
if (!Directory.Exists(archiveDirectory))
⋮----
Directory.CreateDirectory(archiveDirectory);
⋮----
string archivePath = Path.Combine(archiveDirectory, fileInfo.Name);
if (File.Exists(archivePath))
⋮----
File.Delete(archivePath);
⋮----
File.Move(file, archivePath);
⋮----
private void WriteLog(string severity, string message, Exception? exception, string? context)
⋮----
var logEntry = new StringBuilder();
logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{severity}] {(context != null ? $"[{context}]" : "")}");
logEntry.AppendLine($"Message: {message}");
if (!string.IsNullOrEmpty(context))
⋮----
logEntry.AppendLine($"Context: User={Environment.UserName}, Machine={Environment.MachineName}");
⋮----
logEntry.AppendLine($"Exception: {exception.GetType().FullName}: {exception.Message}");
logEntry.AppendLine($"Stack Trace:");
logEntry.AppendLine(exception.StackTrace);
⋮----
logEntry.AppendLine($"Inner Exception: {innerException.GetType().FullName}: {innerException.Message}");
logEntry.AppendLine(innerException.StackTrace);
⋮----
logEntry.AppendLine();
_logQueue.Add(logEntry.ToString());
System.Diagnostics.Debug.Write(logEntry.ToString());
⋮----
System.Diagnostics.Debug.WriteLine($"LOGGING FAILED: {ex.Message}");
System.Diagnostics.Debug.WriteLine($"Original message: {message}");
⋮----
public Task LogWarningAsync(string message, string? context = null)
```

## File: Module_Core/Services/Service_Focus.cs

```csharp
public class Service_Focus : IService_Focus
⋮----
public void SetFocus(Control control)
⋮----
control.DispatcherQueue.TryEnqueue(() =>
⋮----
control.Focus(FocusState.Programmatic);
⋮----
public void SetFocusFirstInput(DependencyObject container)
⋮----
fe.DispatcherQueue.TryEnqueue(() =>
⋮----
public void AttachFocusOnVisibility(FrameworkElement view, Control? targetControl = null)
⋮----
view.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, (_, _) =>
⋮----
private Control? FindFirstFocusableChild(DependencyObject parent)
⋮----
int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
⋮----
var child = VisualTreeHelper.GetChild(parent, i);
⋮----
private bool IsFocusable(Control control)
```

## File: Module_Dunnage/Contracts/IService_DunnageAdminWorkflow.cs

```csharp
public interface IService_DunnageAdminWorkflow
⋮----
public Task NavigateToSectionAsync(Enum_DunnageAdminSection section);
public Task NavigateToHubAsync();
public Task<bool> CanNavigateAwayAsync();
public void MarkDirty();
public void MarkClean();
```

## File: Module_Dunnage/Contracts/IService_DunnageCSVWriter.cs

```csharp
public interface IService_DunnageCSVWriter
⋮----
public Task<Model_CSVWriteResult> WriteToCsvAsync(List<Model_DunnageLoad> loads, string typeName);
public Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads);
public Task<Model_CSVWriteResult> WriteDynamicCsvAsync(
⋮----
public Task<Model_CSVWriteResult> ExportSelectedLoadsAsync(
⋮----
public Task<bool> IsNetworkPathAvailableAsync(int timeout = 3);
public string GetLocalCsvPath(string filename);
public string GetNetworkCsvPath(string filename);
public Task<Model_CSVDeleteResult> ClearCSVFilesAsync(string? filenamePattern = null);
```

## File: Module_Dunnage/Contracts/IService_DunnageWorkflow.cs

```csharp
public interface IService_DunnageWorkflow
⋮----
public event EventHandler StepChanged;
⋮----
public Task<bool> StartWorkflowAsync();
public Task<Model_WorkflowStepResult> AdvanceToNextStepAsync();
public void GoToStep(Enum_DunnageWorkflowStep step);
public Task<Model_SaveResult> SaveSessionAsync();
public Task<Model_SaveResult> SaveToCSVOnlyAsync();
public Task<Model_SaveResult> SaveToDatabaseOnlyAsync();
public void ClearSession();
public Task<Model_CSVDeleteResult> ResetCSVFilesAsync();
public void AddCurrentLoadToSession();
```

## File: Module_Dunnage/Contracts/IService_MySQL_Dunnage.cs

```csharp
public interface IService_MySQL_Dunnage
⋮----
public Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync();
public Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId);
public Task<Model_Dao_Result<int>> InsertTypeAsync(string typeName, string icon);
public Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type);
public Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type);
public Task<Model_Dao_Result> DeleteTypeAsync(int typeId);
public Task<Model_Dao_Result<int>> CheckDuplicateTypeNameAsync(string typeName);
public Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsForTypeAsync(int typeId);
public Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec);
public Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec);
public Task<Model_Dao_Result> DeleteSpecAsync(int specId);
public Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId);
public Task<List<string>> GetAllSpecKeysAsync();
public Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync();
public Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId);
public Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId);
public Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part);
public Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part);
public Task<Model_Dao_Result> DeletePartAsync(string partId);
public Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId = null);
public Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads);
public Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end);
public Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync();
public Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid);
public Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load);
public Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid);
public Task<bool> IsPartInventoriedAsync(string partId);
public Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId);
public Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync();
public Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item);
public Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId);
public Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item);
public Task<Model_Dao_Result<int>> GetPartCountByTypeIdAsync(int typeId);
public Task<Model_Dao_Result<int>> GetTransactionCountByPartIdAsync(string partId);
public Task<Model_Dao_Result<int>> GetTransactionCountByTypeIdAsync(int typeId);
public Task<Model_Dao_Result<int>> GetPartCountBySpecKeyAsync(int typeId, string specKey);
public Task<Model_Dao_Result<int>> GetPartCountByTypeAsync(int typeId) => GetPartCountByTypeIdAsync(typeId);
public Task<Model_Dao_Result<int>> GetTransactionCountByTypeAsync(int typeId) => GetTransactionCountByTypeIdAsync(typeId);
public Task<Model_Dao_Result<int>> GetTransactionCountByPartAsync(string partId) => GetTransactionCountByPartIdAsync(partId);
public Task<Model_Dao_Result> InsertCustomFieldAsync(int typeId, Model_CustomFieldDefinition field);
public Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetCustomFieldsByTypeAsync(int typeId);
public Task<Model_Dao_Result> DeleteCustomFieldAsync(int fieldId);
public Task<Model_Dao_Result> UpsertUserPreferenceAsync(string key, string value);
public Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(int count);
```

## File: Module_Dunnage/Data/Dao_DunnageLine.cs

```csharp
public static class Dao_DunnageLine
⋮----
public static void SetErrorHandler(IService_ErrorHandler errorHandler)
⋮----
public static async Task<Model_Dao_Result> InsertDunnageLineAsync(Model_DunnageLine line)
⋮----
string connectionString = Helper_Database_Variables.GetConnectionString(useProduction: true);
⋮----
new MySqlParameter("@p_Line1", line.Line1 ?? string.Empty),
new MySqlParameter("@p_Line2", line.Line2 ?? string.Empty),
new MySqlParameter("@p_PONumber", line.PONumber),
new MySqlParameter("@p_Date", line.Date),
new MySqlParameter("@p_EmployeeNumber", line.EmployeeNumber),
new MySqlParameter("@p_VendorName", line.VendorName ?? "Unknown"),
new MySqlParameter("@p_Location", line.Location ?? string.Empty),
new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
⋮----
if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
⋮----
return new Model_Dao_Result
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
var errorResult = new Model_Dao_Result
⋮----
await _errorHandler.HandleErrorAsync(
```

## File: Module_Dunnage/Data/Dao_DunnagePart.cs

```csharp
public class Dao_DunnagePart
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllAsync()
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetByTypeAsync(int typeId)
⋮----
System.Diagnostics.Debug.WriteLine($"Dao_DunnagePart: GetByTypeAsync called for typeId={typeId}");
⋮----
System.Diagnostics.Debug.WriteLine($"Dao_DunnagePart: GetByTypeAsync returned {result.Data?.Count ?? 0} parts. Success: {result.IsSuccess}");
⋮----
public virtual async Task<Model_Dao_Result<Model_DunnagePart>> GetByIdAsync(string partId)
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(string partId, int typeId, string specValues, string homeLocation, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_part_id", partId),
new MySqlParameter("@p_type_id", typeId),
new MySqlParameter("@p_spec_values", specValues),
new MySqlParameter("@p_home_location", homeLocation),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string specValues, string homeLocation, string user)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result<int>> CountTransactionsAsync(string partId)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("transaction_count")),
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchAsync(string searchText, int? typeId = null)
⋮----
private Model_DunnagePart MapFromReader(IDataReader reader)
⋮----
return new Model_DunnagePart
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
PartId = reader.GetString(reader.GetOrdinal("part_id")),
TypeId = reader.GetInt32(reader.GetOrdinal("type_id")),
SpecValues = reader.IsDBNull(reader.GetOrdinal("spec_values")) ? "{}" : reader.GetString(reader.GetOrdinal("spec_values")),
HomeLocation = reader.IsDBNull(reader.GetOrdinal("home_location")) ? string.Empty : reader.GetString(reader.GetOrdinal("home_location")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Data/Dao_DunnageSpec.cs

```csharp
public class Dao_DunnageSpec
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetByTypeAsync(int typeId)
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetAllAsync()
⋮----
public virtual async Task<Model_Dao_Result<Model_DunnageSpec>> GetByIdAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(int typeId, string specKey, string specValue, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_type_id", typeId),
new MySqlParameter("@p_spec_key", specKey),
new MySqlParameter("@p_spec_value", specValue),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string specValue, string user)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> DeleteByIdAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result> DeleteByTypeAsync(int typeId)
⋮----
public virtual async Task<Model_Dao_Result<int>> CountPartsUsingSpecAsync(int typeId, string specKey)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("part_count")),
⋮----
public virtual async Task<Model_Dao_Result<List<string>>> GetAllSpecKeysAsync()
⋮----
reader => reader.GetString(reader.GetOrdinal("SpecKey"))
⋮----
private Model_DunnageSpec MapFromReader(IDataReader reader)
⋮----
return new Model_DunnageSpec
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
TypeId = reader.GetInt32(reader.GetOrdinal("type_id")),
SpecKey = reader.GetString(reader.GetOrdinal("spec_key")),
SpecValue = reader.IsDBNull(reader.GetOrdinal("spec_value")) ? "{}" : reader.GetString(reader.GetOrdinal("spec_value")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Data/Dao_DunnageUserPreference.cs

```csharp
public class Dao_DunnageUserPreference
⋮----
public virtual async Task<Model_Dao_Result> UpsertAsync(string userId, string key, string value)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(string userId, int count)
⋮----
reader => new Model_IconDefinition
⋮----
IconName = reader.GetString(reader.GetOrdinal("icon_name"))
```

## File: Module_Dunnage/Enums/Enum_DunnageAdminSection.cs

```csharp

```

## File: Module_Dunnage/Models/Model_CustomFieldDefinition.cs

```csharp
public partial class Model_CustomFieldDefinition : ObservableObject
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
public string GetSummary()
```

## File: Module_Dunnage/Models/Model_DunnageLoad.cs

```csharp
public partial class Model_DunnageLoad : ObservableObject
⋮----
private Guid _loadUuid;
⋮----
if (!string.IsNullOrEmpty(TypeIcon) && Enum.TryParse<MaterialIconKind>(TypeIcon, true, out var kind))
⋮----
private DateTime _receivedDate = DateTime.Now;
⋮----
private DateTime _createdDate = DateTime.Now;
```

## File: Module_Dunnage/Services/Service_MySQL_Dunnage.cs

```csharp
public class Service_MySQL_Dunnage : IService_MySQL_Dunnage
⋮----
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_LoggingUtility _logger;
private readonly IService_UserSessionManager _sessionManager;
private readonly Dao_DunnageLoad _daoDunnageLoad;
private readonly Dao_DunnageType _daoDunnageType;
private readonly Dao_DunnagePart _daoDunnagePart;
private readonly Dao_DunnageSpec _daoDunnageSpec;
private readonly Dao_InventoriedDunnage _daoInventoriedDunnage;
private readonly Dao_DunnageCustomField _daoCustomField;
private readonly Dao_DunnageUserPreference _daoUserPreference;
⋮----
public async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync()
⋮----
return await _daoDunnageType.GetAllAsync();
⋮----
public async Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId)
⋮----
return await _daoDunnageType.GetByIdAsync(typeId);
⋮----
public async Task<Model_Dao_Result<int>> InsertTypeAsync(string typeName, string icon)
⋮----
return await _daoDunnageType.InsertAsync(typeName, icon, CurrentUser);
⋮----
public async Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type)
⋮----
await _logger.LogInfoAsync($"Inserting new dunnage type: {type.TypeName} (Icon: {type.Icon}) by user: {CurrentUser}");
var result = await _daoDunnageType.InsertAsync(type.TypeName, type.Icon, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully inserted dunnage type '{type.TypeName}' with ID: {type.Id}");
return Model_Dao_Result_Factory.Success();
⋮----
if (result.ErrorMessage.Contains("Duplicate entry"))
⋮----
await _logger.LogWarningAsync($"Failed to insert dunnage type '{type.TypeName}': Duplicate entry");
return Model_Dao_Result_Factory.Failure($"The dunnage type name '{type.TypeName}' is already in use.");
⋮----
await _logger.LogErrorAsync($"Failed to insert dunnage type '{type.TypeName}': {result.ErrorMessage}");
return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
⋮----
await _logger.LogErrorAsync($"Exception in InsertTypeAsync for type '{type.TypeName}': {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting dunnage type: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type)
⋮----
await _logger.LogInfoAsync($"Updating dunnage type ID {type.Id}: {type.TypeName} (Icon: {type.Icon}) by user: {CurrentUser}");
var result = await _daoDunnageType.UpdateAsync(type.Id, type.TypeName, type.Icon, CurrentUser);
if (!result.IsSuccess && result.ErrorMessage.Contains("Duplicate entry"))
⋮----
await _logger.LogWarningAsync($"Failed to update dunnage type ID {type.Id}: Duplicate entry for '{type.TypeName}'");
⋮----
await _logger.LogInfoAsync($"Successfully updated dunnage type ID {type.Id}: {type.TypeName}");
⋮----
await _logger.LogErrorAsync($"Failed to update dunnage type ID {type.Id}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in UpdateTypeAsync for type ID {type.Id}: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating dunnage type: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeleteTypeAsync(int typeId)
⋮----
await _logger.LogInfoAsync($"Attempting to delete dunnage type ID {typeId} by user: {CurrentUser}");
var partsResult = await _daoDunnagePart.GetByTypeAsync(typeId);
⋮----
await _logger.LogWarningAsync($"Cannot delete dunnage type ID {typeId}: Used by {partsResult.Data.Count} parts");
return Model_Dao_Result_Factory.Failure($"Cannot delete type. It is used by {partsResult.Data.Count} parts.");
⋮----
var specsResult = await _daoDunnageSpec.GetByTypeAsync(typeId);
⋮----
await _logger.LogWarningAsync($"Cannot delete dunnage type ID {typeId}: Has {specsResult.Data.Count} specifications defined");
return Model_Dao_Result_Factory.Failure($"Cannot delete type. It has {specsResult.Data.Count} specifications defined. Please delete them first.");
⋮----
var result = await _daoDunnageType.DeleteAsync(typeId);
⋮----
await _logger.LogInfoAsync($"Successfully deleted dunnage type ID {typeId}");
⋮----
await _logger.LogErrorAsync($"Failed to delete dunnage type ID {typeId}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in DeleteTypeAsync for type ID {typeId}: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error deleting dunnage type: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<int>> CheckDuplicateTypeNameAsync(string typeName)
⋮----
var result = await _daoDunnageType.CheckDuplicateNameAsync(typeName);
⋮----
public async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsForTypeAsync(int typeId)
⋮----
return await _daoDunnageSpec.GetByTypeAsync(typeId);
⋮----
public async Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec)
⋮----
await _logger.LogInfoAsync($"Inserting spec '{spec.SpecKey}' for type ID {spec.TypeId} by user: {CurrentUser}");
var result = await _daoDunnageSpec.InsertAsync(spec.TypeId, spec.SpecKey, spec.SpecValue, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully inserted spec '{spec.SpecKey}' with ID: {spec.Id}");
⋮----
await _logger.LogErrorAsync($"Failed to insert spec '{spec.SpecKey}' for type ID {spec.TypeId}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in InsertSpecAsync for spec '{spec.SpecKey}': {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting spec: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec)
⋮----
await _logger.LogInfoAsync($"Updating spec ID {spec.Id}: {spec.SpecKey} = {spec.SpecValue} by user: {CurrentUser}");
var result = await _daoDunnageSpec.UpdateAsync(spec.Id, spec.SpecValue, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully updated spec ID {spec.Id}: {spec.SpecKey}");
⋮----
await _logger.LogErrorAsync($"Failed to update spec ID {spec.Id}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in UpdateSpecAsync for spec ID {spec.Id}: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating spec: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeleteSpecAsync(int specId)
⋮----
return await _daoDunnageSpec.DeleteByIdAsync(specId);
⋮----
return Model_Dao_Result_Factory.Failure($"Error deleting spec: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId)
⋮----
public async Task<List<string>> GetAllSpecKeysAsync()
⋮----
var result = await _daoDunnageSpec.GetAllAsync();
⋮----
.Select(s => s.SpecKey)
.Distinct()
.Order()
.ToList();
⋮----
public async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync()
⋮----
return await _daoDunnagePart.GetAllAsync();
⋮----
public async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId)
⋮----
_logger.LogInfo($"Service_MySQL_Dunnage: GetPartsByTypeAsync called for typeId={typeId}", "DunnageService");
⋮----
var result = await _daoDunnagePart.GetByTypeAsync(typeId);
_logger.LogInfo($"Service_MySQL_Dunnage: GetPartsByTypeAsync returned {result.Data?.Count ?? 0} parts", "DunnageService");
⋮----
public async Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId)
⋮----
return await _daoDunnagePart.GetByIdAsync(partId);
⋮----
public async Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part)
⋮----
await _logger.LogInfoAsync($"Inserting new dunnage part: {part.PartId} (Type ID: {part.TypeId}, Home Location: {part.HomeLocation}) by user: {CurrentUser}");
var result = await _daoDunnagePart.InsertAsync(part.PartId, part.TypeId, part.SpecValues, part.HomeLocation, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully inserted dunnage part '{part.PartId}' with ID: {part.Id}");
⋮----
await _logger.LogErrorAsync($"Failed to insert dunnage part '{part.PartId}': {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in InsertPartAsync for part '{part.PartId}': {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part)
⋮----
await _logger.LogInfoAsync($"Updating dunnage part ID {part.Id} (Part ID: {part.PartId}, Home Location: {part.HomeLocation}) by user: {CurrentUser}");
var result = await _daoDunnagePart.UpdateAsync(part.Id, part.SpecValues, part.HomeLocation, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully updated dunnage part ID {part.Id}");
⋮----
await _logger.LogErrorAsync($"Failed to update dunnage part ID {part.Id}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in UpdatePartAsync for part ID {part.Id}: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeletePartAsync(string partId)
⋮----
return Model_Dao_Result_Factory.Failure("Delete part not implemented in DAO yet.");
⋮----
public async Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId)
⋮----
result = await _daoDunnagePart.GetByTypeAsync(typeId.Value);
⋮----
result = await _daoDunnagePart.GetAllAsync();
⋮----
if (string.IsNullOrWhiteSpace(searchText))
⋮----
var filtered = result.Data.Where(p =>
p.PartId.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
p.DunnageTypeName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
).ToList();
⋮----
public async Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads)
⋮----
await _logger.LogInfoAsync("SaveLoadsAsync called with no loads to save");
⋮----
await _logger.LogInfoAsync($"Saving batch of {loads.Count} dunnage loads by user: {CurrentUser}");
var result = await _daoDunnageLoad.InsertBatchAsync(loads, CurrentUser);
⋮----
var totalQuantity = loads.Sum(l => l.Quantity);
await _logger.LogInfoAsync($"Successfully saved {loads.Count} dunnage loads (Total Quantity: {totalQuantity})");
⋮----
await _logger.LogErrorAsync($"Failed to save {loads.Count} dunnage loads: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in SaveLoadsAsync for {loads?.Count ?? 0} loads: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error saving loads: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end)
⋮----
return await _daoDunnageLoad.GetByDateRangeAsync(start, end);
⋮----
public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync()
⋮----
return await _daoDunnageLoad.GetAllAsync();
⋮----
public async Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid)
⋮----
if (Guid.TryParse(loadUuid, out var guid))
⋮----
return await _daoDunnageLoad.GetByIdAsync(guid);
⋮----
public async Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load)
⋮----
return Model_Dao_Result_Factory.Failure("Update load not implemented in DAO yet.");
⋮----
public async Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid)
⋮----
return Model_Dao_Result_Factory.Failure("Delete load not implemented in DAO yet.");
⋮----
public async Task<bool> IsPartInventoriedAsync(string partId)
⋮----
var result = await _daoInventoriedDunnage.CheckAsync(partId);
⋮----
public async Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId)
⋮----
return await _daoInventoriedDunnage.GetByPartAsync(partId);
⋮----
public async Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync()
⋮----
return await _daoInventoriedDunnage.GetAllAsync();
⋮----
public async Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item)
⋮----
await _logger.LogInfoAsync($"Adding part '{item.PartId}' to inventoried list (Method: {item.InventoryMethod}) by user: {CurrentUser}");
var result = await _daoInventoriedDunnage.InsertAsync(item.PartId, item.InventoryMethod ?? string.Empty, item.Notes ?? string.Empty, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully added part '{item.PartId}' to inventoried list with ID: {item.Id}");
⋮----
await _logger.LogErrorAsync($"Failed to add part '{item.PartId}' to inventoried list: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in AddToInventoriedListAsync for part '{item.PartId}': {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error adding to inventory list: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId)
⋮----
return Model_Dao_Result_Factory.Failure("Remove from inventory list not implemented in DAO yet.");
⋮----
public async Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item)
⋮----
return await _daoInventoriedDunnage.UpdateAsync(item.Id, item.InventoryMethod ?? string.Empty, item.Notes ?? string.Empty, CurrentUser);
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating inventory item: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<int>> GetPartCountByTypeIdAsync(int typeId)
⋮----
return await _daoDunnageType.CountPartsAsync(typeId);
⋮----
public async Task<Model_Dao_Result<int>> GetTransactionCountByPartIdAsync(string partId)
⋮----
return await _daoDunnagePart.CountTransactionsAsync(partId);
⋮----
public async Task<Model_Dao_Result<int>> GetTransactionCountByTypeIdAsync(int typeId)
⋮----
return await _daoDunnageType.CountTransactionsAsync(typeId);
⋮----
public async Task<Model_Dao_Result<int>> GetPartCountBySpecKeyAsync(int typeId, string specKey)
⋮----
return await _daoDunnageSpec.CountPartsUsingSpecAsync(typeId, specKey);
⋮----
public async Task<Model_Dao_Result> InsertCustomFieldAsync(int typeId, Model_CustomFieldDefinition field)
⋮----
var result = await _daoCustomField.InsertAsync(typeId, field, CurrentUser);
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting custom field: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetCustomFieldsByTypeAsync(int typeId)
⋮----
return await _daoCustomField.GetByTypeAsync(typeId);
⋮----
public async Task<Model_Dao_Result> DeleteCustomFieldAsync(int fieldId)
⋮----
return await _daoCustomField.DeleteAsync(fieldId);
⋮----
return Model_Dao_Result_Factory.Failure($"Error deleting custom field: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpsertUserPreferenceAsync(string key, string value)
⋮----
return await _daoUserPreference.UpsertAsync(CurrentUser, key, value);
⋮----
return Model_Dao_Result_Factory.Failure($"Error saving user preference: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(int count)
⋮----
return await _daoUserPreference.GetRecentlyUsedIconsAsync(CurrentUser, count);
⋮----
private void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className)
⋮----
_ = _errorHandler.HandleErrorAsync($"Error in {method} ({className}): {ex.Message}", severity, ex);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminPartsViewModel.cs

```csharp
public partial class ViewModel_Dunnage_AdminParts : ViewModel_Shared_Base
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_DunnageAdminWorkflow _adminWorkflow;
private readonly IService_Pagination _paginationService;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
⋮----
private async Task LoadPartsAsync()
⋮----
var typesResult = await _dunnageService.GetAllTypesAsync();
⋮----
AvailableTypes.Clear();
⋮----
AvailableTypes.Add(type);
⋮----
var partsResult = await _dunnageService.GetAllPartsAsync();
⋮----
await _errorHandler.HandleDaoErrorAsync(partsResult, "LoadPartsAsync", true);
⋮----
_paginationService.SetSource(_allParts);
⋮----
await _logger.LogInfoAsync($"Loaded {TotalRecords} dunnage parts", "PartManagement");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private async Task FilterByTypeAsync()
⋮----
var result = await _dunnageService.GetPartsByTypeAsync(SelectedFilterType.Id);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "FilterByTypeAsync", true);
⋮----
private async Task SearchPartsAsync()
⋮----
if (string.IsNullOrWhiteSpace(SearchText))
⋮----
var result = await _dunnageService.SearchPartsAsync(SearchText);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "SearchPartsAsync", true);
⋮----
private async Task ClearFiltersAsync()
⋮----
private void NextPage()
⋮----
if (_paginationService.NextPage())
⋮----
private void PreviousPage()
⋮----
if (_paginationService.PreviousPage())
⋮----
private void FirstPage()
⋮----
if (_paginationService.FirstPage())
⋮----
private void LastPage()
⋮----
if (_paginationService.LastPage())
⋮----
private void LoadPage()
⋮----
Parts.Clear();
⋮----
Parts.Add(item);
⋮----
private void OnPageChanged(object? sender, EventArgs e)
⋮----
private void UpdateNavigationButtons()
⋮----
private async Task ShowDeleteConfirmationAsync()
⋮----
var countResult = await _dunnageService.GetTransactionCountByPartAsync(SelectedPart.PartId);
⋮----
await _errorHandler.HandleDaoErrorAsync(countResult, "GetTransactionCountByPartAsync", true);
⋮----
var warningDialog = new ContentDialog
⋮----
XamlRoot = _windowService.GetXamlRoot()
⋮----
await warningDialog.ShowAsync();
⋮----
var confirmDialog = new ContentDialog
⋮----
var result = await confirmDialog.ShowAsync();
⋮----
private async Task DeletePartAsync()
⋮----
var result = await _dunnageService.DeletePartAsync(SelectedPart.PartId);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "DeletePartAsync", true);
⋮----
await _logger.LogInfoAsync($"Deleted part: {SelectedPart.PartId}", "PartManagement");
⋮----
private async Task ReturnToAdminHubAsync()
⋮----
await _adminWorkflow.NavigateToHubAsync();
⋮----
partial void OnSelectedPartChanged(Model_DunnagePart? value)
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs

```csharp
public partial class ViewModel_Dunnage_DetailsEntry : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Help _helpService;
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_Dispatcher _dispatcher;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
_dispatcher.TryEnqueue(async () =>
⋮----
public async Task LoadSpecsForSelectedPartAsync()
⋮----
_logger.LogError("No type selected in workflow session", null, "DetailsEntry");
SpecInputs.Clear();
⋮----
_logger.LogInfo($"Loading specs for type ID: {selectedTypeId}", "DetailsEntry");
var specsResult = await _dunnageService.GetSpecsForTypeAsync(selectedTypeId);
⋮----
_logger.LogWarning($"No specs found for type {selectedTypeId}: {specsResult.ErrorMessage}", "DetailsEntry");
⋮----
_logger.LogInfo($"Loaded {specs.Count} specs from database", "DetailsEntry");
⋮----
if (selectedPart != null && !string.IsNullOrWhiteSpace(selectedPart.SpecValues))
⋮----
_logger.LogInfo($"Loaded spec values from part: {selectedPart.SpecValues}", "DetailsEntry");
⋮----
_logger.LogError($"Failed to parse part spec values: {ex.Message}", ex, "DetailsEntry");
⋮----
TextSpecs.Clear();
NumberSpecs.Clear();
BooleanSpecs.Clear();
⋮----
var specType = specValueDict.ContainsKey("type") ? specValueDict["type"]?.ToString()?.ToLowerInvariant() ?? "text" : "text";
⋮----
if (!string.IsNullOrWhiteSpace(defaultValue))
⋮----
typedValue = double.Parse(defaultValue);
⋮----
typedValue = bool.Parse(defaultValue);
⋮----
_logger.LogWarning($"Failed to convert value '{defaultValue}' to type '{specType}': {ex.Message}", "DetailsEntry");
⋮----
var input = new Model_SpecInput
⋮----
Unit = specValueDict.ContainsKey("unit") ? specValueDict["unit"]?.ToString() : null,
IsRequired = specValueDict.ContainsKey("required") && bool.Parse(specValueDict["required"]?.ToString() ?? "false"),
⋮----
SpecInputs.Add(input);
var typeFromDb = specValueDict.ContainsKey("type") ? specValueDict["type"]?.ToString() ?? "null" : "not found";
_logger.LogInfo($"Processing spec: {spec.SpecKey}, Type from DB: {typeFromDb}, Normalized: {specType}", "DetailsEntry");
⋮----
BooleanSpecs.Add(input);
_logger.LogInfo($"Added {spec.SpecKey} to BooleanSpecs", "DetailsEntry");
⋮----
NumberSpecs.Add(input);
_logger.LogInfo($"Added {spec.SpecKey} to NumberSpecs", "DetailsEntry");
⋮----
TextSpecs.Add(input);
_logger.LogInfo($"Added {spec.SpecKey} to TextSpecs", "DetailsEntry");
⋮----
_logger.LogInfo($"Created {SpecInputs.Count} spec input controls (Text: {TextSpecs.Count}, Number: {NumberSpecs.Count}, Boolean: {BooleanSpecs.Count})", "DetailsEntry");
⋮----
var isInventoried = await _dunnageService.IsPartInventoriedAsync(selectedPart.PartId);
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private Dictionary<string, object> ParseSpecValue(string specValue)
⋮----
if (string.IsNullOrWhiteSpace(specValue))
⋮----
_logger.LogError($"Failed to parse spec value JSON: {ex.Message}", ex, "DetailsEntry");
⋮----
partial void OnPoNumberChanged(string value)
⋮----
if (string.IsNullOrWhiteSpace(value))
⋮----
private void UpdateInventoryMessage()
⋮----
private bool ValidateInputs()
⋮----
foreach (var spec in SpecInputs.Where(s => s.IsRequired))
⋮----
if (spec.Value == null || string.IsNullOrWhiteSpace(spec.Value.ToString()))
⋮----
private void GoBack()
⋮----
_logger.LogInfo("Navigating back to Quantity Entry", "DetailsEntry");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
⋮----
private async Task GoNextAsync()
⋮----
var specValues = SpecInputs.ToDictionary(
⋮----
_logger.LogInfo("Details saved, navigating to Review", "DetailsEntry");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.Review);
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_ManualEntryViewModel.cs

```csharp
public partial class ViewModel_Dunnage_ManualEntry : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_DunnageCSVWriter _csvWriter;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
public async Task InitializeAsync()
⋮----
var typesResult = await _dunnageService.GetAllTypesAsync();
⋮----
AvailableTypes.Clear();
⋮----
AvailableTypes.Add(type);
⋮----
var partsResult = await _dunnageService.GetAllPartsAsync();
⋮----
AvailableParts.Clear();
⋮----
AvailableParts.Add(part);
⋮----
_logger.LogInfo("Manual Entry initialized", "ManualEntry");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private void AddRow()
⋮----
var newLoad = new Model_DunnageLoad
⋮----
Loads.Add(newLoad);
⋮----
_logger.LogInfo($"Added new row, total: {Loads.Count}", "ManualEntry");
⋮----
private async Task AddMultipleRowsAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null", null, "ManualEntry");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error, null, true);
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogInfo($"Added {count} rows via dialog, total: {Loads.Count}", "ManualEntry");
⋮----
private void RemoveRow()
⋮----
Loads.Remove(SelectedLoad);
SelectedLoad = Loads.LastOrDefault();
⋮----
_logger.LogInfo($"Removed row, total: {Loads.Count}", "ManualEntry");
⋮----
private void FillBlankSpaces()
⋮----
var lastLoad = Loads.LastOrDefault();
⋮----
if (string.IsNullOrWhiteSpace(load.PoNumber) && !string.IsNullOrWhiteSpace(lastLoad.PoNumber))
⋮----
if (string.IsNullOrWhiteSpace(load.Location) && !string.IsNullOrWhiteSpace(lastLoad.Location))
⋮----
_logger.LogInfo("Fill blank spaces executed", "ManualEntry");
⋮----
private void SortForPrinting()
⋮----
.OrderBy(l => l.PartId)
.ThenBy(l => l.PoNumber)
.ThenBy(l => l.TypeName)
.ToList();
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
_logger.LogInfo("Sort for printing executed", "ManualEntry");
⋮----
private async Task AutoFillAsync()
⋮----
if (string.IsNullOrWhiteSpace(SelectedLoad.PartId))
⋮----
var partResult = await _dunnageService.GetPartByIdAsync(SelectedLoad.PartId);
⋮----
var type = AvailableTypes.FirstOrDefault(t => t.Id == part.TypeId);
⋮----
_logger.LogInfo($"Auto-filled {part.SpecValuesDict.Count} spec values for Part ID: {SelectedLoad.PartId}", "ManualEntry");
⋮----
if (string.IsNullOrWhiteSpace(SelectedLoad.InventoryMethod))
⋮----
SelectedLoad.InventoryMethod = string.IsNullOrWhiteSpace(SelectedLoad.PoNumber) ? "Adjust In" : "Receive In";
⋮----
_logger.LogInfo($"Auto-fill completed for Part ID: {SelectedLoad.PartId}", "ManualEntry");
⋮----
private async Task SaveToHistoryAsync()
⋮----
var result = await _dunnageService.SaveLoadsAsync(Loads.ToList());
⋮----
_logger.LogInfo($"Saved {Loads.Count} loads to history", "ManualEntry");
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "SaveToHistoryAsync", true);
⋮----
private async Task SaveAllAsync()
⋮----
if (string.IsNullOrWhiteSpace(load.TypeName) || string.IsNullOrWhiteSpace(load.PartId))
⋮----
var saveResult = await _dunnageService.SaveLoadsAsync(Loads.ToList());
⋮----
await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(Loads.ToList());
⋮----
_logger.LogInfo($"Saved {Loads.Count} loads", "ManualEntry");
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
var result = await dialog.ShowAsync().AsTask();
⋮----
_logger.LogInfo("User confirmed return to mode selection, clearing loads", "ManualEntry");
⋮----
_workflowService.ClearSession();
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
⋮----
_logger.LogError($"Failed to return to mode selection: {ex.Message}", ex, "ManualEntry");
await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex, true);
⋮----
_logger.LogInfo("User cancelled return to mode selection", "ManualEntry");
⋮----
private async Task LoadSpecColumnsAsync()
⋮----
var specKeys = await _dunnageService.GetAllSpecKeysAsync();
⋮----
_logger.LogInfo($"Loaded {SpecColumnHeaders.Count} dynamic spec columns", "ManualEntry");
⋮----
_logger.LogError($"Failed to load spec columns: {ex.Message}", ex, "ManualEntry");
⋮----
public async Task OnPartIdChangedAsync(Model_DunnageLoad load)
⋮----
if (load == null || string.IsNullOrWhiteSpace(load.PartId))
⋮----
var partResult = await _dunnageService.GetPartByIdAsync(load.PartId);
⋮----
_logger.LogInfo($"Auto-populated data for Part ID: {load.PartId}", "ManualEntry");
⋮----
_logger.LogError($"Error auto-populating part data: {ex.Message}", ex, "ManualEntry");
⋮----
private void UpdateCanSave()
⋮----
CanSave = Loads.Any(l => !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId));
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_PartSelectionViewModel.cs

```csharp
public partial class ViewModel_Dunnage_PartSelection : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_MySQL_Dunnage _dunnageService; private readonly IService_Help _helpService; private readonly IService_Dispatcher _dispatcher;
⋮----
_logger.LogInfo("PartSelection: ViewModel constructed and subscribed to StepChanged", "PartSelection");
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
_logger.LogInfo($"PartSelection: Workflow step changed to {_workflowService.CurrentStep}", "PartSelection");
⋮----
_logger.LogInfo("PartSelection: Step is PartSelection, calling InitializeAsync via dispatcher", "PartSelection");
_dispatcher.TryEnqueue(async () =>
⋮----
if (!string.IsNullOrEmpty(SelectedTypeIcon) && Enum.TryParse<MaterialIconKind>(SelectedTypeIcon, true, out var kind))
⋮----
public async Task InitializeAsync()
⋮----
_logger.LogInfo("PartSelection: InitializeAsync called", "PartSelection");
⋮----
_logger.LogInfo("PartSelection: InitializeAsync returning because IsBusy is true", "PartSelection");
⋮----
_logger.LogInfo($"PartSelection: SelectedTypeId={SelectedTypeId}, SelectedTypeName={SelectedTypeName}, SelectedTypeIcon={SelectedTypeIcon}", "PartSelection");
⋮----
_logger.LogInfo($"PartSelection: {StatusMessage}", "PartSelection");
⋮----
_logger.LogError($"PartSelection: Failed to initialize: {ex.Message}", ex, "PartSelection");
await _errorHandler.HandleErrorAsync(
⋮----
private async Task LoadPartsAsync()
⋮----
_logger.LogInfo($"PartSelection: LoadPartsAsync called with SelectedTypeId={SelectedTypeId}", "PartSelection");
⋮----
_logger.LogInfo("PartSelection: SelectedTypeId is 0, returning from LoadPartsAsync", "PartSelection");
⋮----
_logger.LogInfo($"PartSelection: Calling _dunnageService.GetPartsByTypeAsync({SelectedTypeId})", "PartSelection");
var result = await _dunnageService.GetPartsByTypeAsync(SelectedTypeId);
⋮----
AvailableParts.Clear();
⋮----
AvailableParts.Add(part);
⋮----
_logger.LogInfo($"PartSelection: Successfully loaded {AvailableParts.Count} parts", "PartSelection");
⋮----
_logger.LogWarning($"PartSelection: Failed to load parts: {result.ErrorMessage}", "PartSelection");
await _errorHandler.HandleDaoErrorAsync(
⋮----
_logger.LogError($"PartSelection: Error in LoadPartsAsync: {ex.Message}", ex, "PartSelection");
⋮----
partial void OnSelectedPartChanged(Model_DunnagePart? oldValue, Model_DunnagePart? newValue)
⋮----
_logger.LogInfo($"Part selected via ComboBox: {newValue.PartId}", "PartSelection");
⋮----
_logger.LogInfo($"Updated workflow session with selected part: {newValue.PartId}", "PartSelection");
⋮----
SelectPartCommand.NotifyCanExecuteChanged();
⋮----
private async Task CheckInventoryStatusAsync(Model_DunnagePart part)
⋮----
var isInventoried = await _dunnageService.IsPartInventoriedAsync(part.PartId);
⋮----
_logger.LogInfo($"Part {part.PartId} is inventoried - showing notification", "PartSelection");
⋮----
_logger.LogError($"Error checking inventory status: {ex.Message}", ex, "PartSelection");
⋮----
private void UpdateInventoryMessage()
⋮----
private void GoBack()
⋮----
_logger.LogInfo("Returning to Type Selection", "PartSelection");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
⋮----
private async Task SelectPartAsync()
⋮----
_logger.LogInfo($"Selected part: {SelectedPart.PartId}", "PartSelection");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
⋮----
private async Task QuickAddPartAsync()
⋮----
_logger.LogInfo($"Quick Add Part requested for type {SelectedTypeName}", "PartSelection");
var specsResult = await _dunnageService.GetSpecsForTypeAsync(SelectedTypeId);
⋮----
_logger.LogInfo("Cannot show dialog: XamlRoot is null", "PartSelection");
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogInfo($"Adding new part: {partId} for type {SelectedTypeName}", "PartSelection");
var newPart = new Model_DunnagePart
⋮----
var insertResult = await _dunnageService.InsertPartAsync(newPart);
⋮----
_logger.LogInfo($"Successfully added part: {partId}", "PartSelection");
⋮----
var addedPart = AvailableParts.FirstOrDefault(p => p.PartId == partId);
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/Views/View_Dunnage_AdminInventoryView.xaml.cs

```csharp
public sealed partial class View_Dunnage_AdminInventoryView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
protected override async void OnNavigatedTo(NavigationEventArgs e)
⋮----
base.OnNavigatedTo(e);
App.GetService<IService_LoggingUtility>().LogInfo("Admin Inventory View loaded", "AdminInventoryView");
await ViewModel.InitializeAsync();
```

## File: Module_Dunnage/Views/View_Dunnage_AdminTypesView.xaml.cs

```csharp
public sealed partial class View_Dunnage_AdminTypesView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
App.GetService<IService_LoggingUtility>().LogInfo("Admin Types View loaded", "AdminTypesView");
await ViewModel.LoadTypesCommand.ExecuteAsync(null);
```

## File: Module_Dunnage/Views/View_Dunnage_Control_IconPickerControl.xaml.cs

```csharp
public sealed partial class View_Dunnage_Control_IconPickerControl : UserControl
⋮----
public static readonly DependencyProperty SelectedIconProperty =
DependencyProperty.Register(
⋮----
new PropertyMetadata(MaterialIconKind.PackageVariantClosed, OnSelectedIconChanged));
public static readonly DependencyProperty RecentlyUsedIconsProperty =
⋮----
new PropertyMetadata(default(ObservableCollection<Model_IconDefinition>)));
⋮----
_allIcons = Helper_MaterialIcons.GetAllIcons();
_filteredIcons = new ObservableCollection<MaterialIconKind>(_allIcons.Take(200));
⋮----
private static void OnSelectedIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
⋮----
var recent = control.RecentlyUsedIcons.FirstOrDefault(x => x.Kind == kind);
⋮----
private void OnRecentIconSelectionChanged(object sender, SelectionChangedEventArgs e)
⋮----
private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
⋮----
_filteredIcons.Clear();
var matches = Helper_MaterialIcons.SearchIcons(searchText).Take(200);
⋮----
_filteredIcons.Add(icon);
```

## File: Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml.cs

```csharp
public sealed partial class View_Dunnage_QuickAddPartDialog : ContentDialog
⋮----
private void GenerateSpecFields()
⋮----
if (string.Equals(spec.SpecKey, "Width", System.StringComparison.OrdinalIgnoreCase) ||
string.Equals(spec.SpecKey, "Height", System.StringComparison.OrdinalIgnoreCase) ||
string.Equals(spec.SpecKey, "Depth", System.StringComparison.OrdinalIgnoreCase))
⋮----
def = new SpecDefinition { DataType = "Text" };
⋮----
var stackPanel = new StackPanel { Spacing = 4 };
⋮----
if (!string.IsNullOrEmpty(def.Unit))
⋮----
var label = new TextBlock
⋮----
stackPanel.Children.Add(label);
⋮----
if (string.Equals(def.DataType, "Number", System.StringComparison.OrdinalIgnoreCase))
⋮----
var numberBox = new NumberBox
⋮----
else if (string.Equals(def.DataType, "Boolean", System.StringComparison.OrdinalIgnoreCase))
⋮----
var checkBox = new CheckBox
⋮----
var textBox = new TextBox
⋮----
PlaceholderText = $"Enter {spec.SpecKey.ToLower()}",
⋮----
stackPanel.Children.Add(inputControl);
DynamicSpecsPanel.Children.Add(stackPanel);
⋮----
private void OnDimensionChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
⋮----
private void UpdatePartId()
⋮----
parts.Add(TypeName);
⋮----
if (kvp.Value is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
⋮----
textSpecs.Add(tb.Text.Trim());
⋮----
if (!double.IsNaN(WidthNumberBox.Value) && WidthNumberBox.Value > 0)
⋮----
numbers.Add(WidthNumberBox.Value);
⋮----
if (!double.IsNaN(HeightNumberBox.Value) && HeightNumberBox.Value > 0)
⋮----
numbers.Add(HeightNumberBox.Value);
⋮----
if (!double.IsNaN(DepthNumberBox.Value) && DepthNumberBox.Value > 0)
⋮----
numbers.Add(DepthNumberBox.Value);
⋮----
if (kvp.Value is NumberBox nb && !double.IsNaN(nb.Value) && nb.Value > 0)
⋮----
numbers.Add(nb.Value);
⋮----
var formattedNumbers = numbers.Select(n =>
n == Math.Floor(n) ? ((int)n).ToString() : n.ToString("0.##")
⋮----
parts.Add($"({string.Join("x", formattedNumbers)})");
⋮----
// 4. Boolean specs (only if true, abbreviated if >2 words)
⋮----
var words = specName.Split(new[] { ' ', '_' }, System.StringSplitOptions.RemoveEmptyEntries);
⋮----
// Abbreviate: use first letter of each word
boolSpecs.Add(string.Join("", words.Select(w => char.ToUpper(w[0]))));
⋮----
// Use full name
boolSpecs.Add(specName);
⋮----
parts.Add(string.Join(", ", boolSpecs));
⋮----
// Add text specs if any
⋮----
// Insert text specs after type name
parts.Insert(1, string.Join(", ", textSpecs));
⋮----
PartIdTextBox.Text = string.Join(" - ", parts);
⋮----
private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
// Set Part ID
PartId = PartIdTextBox.Text.Trim();
// Build spec values dictionary
⋮----
// Add dynamic specs
⋮----
specValues[kvp.Key] = tb.Text.Trim();
⋮----
else if (kvp.Value is NumberBox nb && !double.IsNaN(nb.Value))
⋮----
// Add dimensions if provided
⋮----
if (!string.IsNullOrWhiteSpace(NotesTextBox.Text))
⋮----
specValues["Notes"] = NotesTextBox.Text.Trim();
⋮----
SpecValuesJson = JsonSerializer.Serialize(specValues);
```

## File: Module_Dunnage/Views/View_Dunnage_TypeSelectionView.xaml

```xml
<UserControl x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_dunnage_typeselectionView"
             x:Name="RootUserControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
             xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
             xmlns:materialIcons="using:Material.Icons.WinUI3"
             xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
             mc:Ignorable="d"
             Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_IconCodeToGlyph x:Key="IconCodeToGlyphConverter" />
    </UserControl.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0"
                    Spacing="8"
                    Margin="0,0,0,32">
            <TextBlock Text="Select Dunnage Type"
                       Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="{x:Bind ViewModel.GetTip('Dunnage.TypeSelection'), Mode=OneWay}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       Style="{StaticResource BodyTextBlockStyle}" />
        </StackPanel>

        <!-- 3x3 Type Grid -->
        <GridView Grid.Row="1"
                  ItemsSource="{x:Bind ViewModel.DisplayedTypes, Mode=OneWay}"
                  SelectedItem="{x:Bind ViewModel.SelectedType, Mode=TwoWay}"
                  SelectionMode="None"
                  IsItemClickEnabled="True"
                  ItemClick="TypeCard_ItemClick"
                  MaxWidth="960"
                  HorizontalAlignment="Center">
            <GridView.ItemsPanel>
                <ItemsPanelTemplate>
                    <ItemsWrapGrid Orientation="Horizontal"
                                   MaximumRowsOrColumns="3" />
                </ItemsPanelTemplate>
            </GridView.ItemsPanel>
            <GridView.ItemTemplate>
                <DataTemplate x:DataType="models:Model_DunnageType">
                    <Border Width="280"
                            Height="180"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            Padding="20">
                        <Border.ContextFlyout>
                            <MenuFlyout>
                                <MenuFlyoutItem Text="Edit"
                                                Icon="Edit"
                                                Command="{Binding ElementName=RootUserControl, Path=ViewModel.EditTypeCommand}"
                                                CommandParameter="{x:Bind}" />
                                <MenuFlyoutItem Text="Delete"
                                                Icon="Delete"
                                                Command="{Binding ElementName=RootUserControl, Path=ViewModel.DeleteTypeCommand}"
                                                CommandParameter="{x:Bind}" />
                            </MenuFlyout>
                        </Border.ContextFlyout>
                        <Grid>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto" />
                                <RowDefinition Height="*" />
                                <RowDefinition Height="Auto" />
                            </Grid.RowDefinitions>

                            <!-- Icon -->
                            <materialIcons:MaterialIcon Grid.Row="0"
                                                        Kind="{x:Bind IconKind, Mode=OneWay}"
                                                        Width="48"
                                                        Height="48"
                                                        Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                                                        HorizontalAlignment="Center"
                                                        Margin="0,0,0,12" />

                            <!-- Type Name -->
                            <TextBlock Grid.Row="1"
                                       Text="{x:Bind TypeName}"
                                       Style="{StaticResource SubtitleTextBlockStyle}"
                                       HorizontalAlignment="Center"
                                       VerticalAlignment="Center"
                                       TextAlignment="Center"
                                       TextWrapping="Wrap" />
                        </Grid>
                    </Border>
                </DataTemplate>
            </GridView.ItemTemplate>
        </GridView>

        <!-- Pagination Controls -->
        <StackPanel Grid.Row="2"
                    Orientation="Horizontal"
                    HorizontalAlignment="Center"
                    Spacing="16"
                    Margin="0,32,0,0">
            <!-- First Page -->
            <Button Command="{x:Bind ViewModel.FirstPageCommand}"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FirstPage'), Mode=OneWay}">
                <FontIcon Glyph="&#xE892;"
                          FontSize="16" />
            </Button>

            <!-- Previous Page -->
            <Button Command="{x:Bind ViewModel.PreviousPageCommand}"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.PreviousPage'), Mode=OneWay}">
                <FontIcon Glyph="&#xE76B;"
                          FontSize="16" />
            </Button>

            <!-- Page Info -->
            <Border Background="{ThemeResource LayerFillColorDefaultBrush}"
                    Padding="16,8"
                    CornerRadius="4"
                    MinWidth="120">
                <TextBlock Text="{x:Bind ViewModel.PageInfo, Mode=OneWay}"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center" />
            </Border>

            <!-- Next Page -->
            <Button Command="{x:Bind ViewModel.NextPageCommand}"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.NextPage'), Mode=OneWay}">
                <FontIcon Glyph="&#xE76C;"
                          FontSize="16" />
            </Button>

            <!-- Last Page -->
            <Button Command="{x:Bind ViewModel.LastPageCommand}"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LastPage'), Mode=OneWay}">
                <FontIcon Glyph="&#xE893;"
                          FontSize="16" />
            </Button>

            <!-- Quick Add Button -->
            <Button Command="{x:Bind ViewModel.QuickAddTypeCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    Margin="32,0,0,0">
                <StackPanel Orientation="Horizontal"
                            Spacing="8">
                    <FontIcon Glyph="&#xE710;"
                              FontSize="16" />
                    <TextBlock Text="Quick Add Type" />
                </StackPanel>
            </Button>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_TypeSelectionView.xaml.cs

```csharp
public sealed partial class View_dunnage_typeselectionView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void OnLoaded(object sender, RoutedEventArgs e)
⋮----
System.Diagnostics.Debug.WriteLine("dunnage_typeselectionView: OnLoaded called");
await ViewModel.InitializeAsync();
⋮----
private async void TypeCard_ItemClick(object sender, ItemClickEventArgs e)
⋮----
await ViewModel.SelectTypeCommand.ExecuteAsync(type);
```

## File: Module_Receiving/Contracts/IService_MySQL_PackagePreferences.cs

```csharp
public interface IService_MySQL_PackagePreferences
⋮----
public Task<Model_PackageTypePreference?> GetPreferenceAsync(string partID);
public Task SavePreferenceAsync(Model_PackageTypePreference preference);
public Task<bool> DeletePreferenceAsync(string partID);
```

## File: Module_Receiving/Contracts/IService_MySQL_Receiving.cs

```csharp
public interface IService_MySQL_Receiving
⋮----
public Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads);
public Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate);
public Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate);
public Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads);
public Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads);
public Task<bool> TestConnectionAsync();
```

## File: Module_Receiving/Contracts/IService_MySQL_ReceivingLine.cs

```csharp
public interface IService_MySQL_ReceivingLine
⋮----
public Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line);
```

## File: Module_Receiving/Contracts/IService_ReceivingValidation.cs

```csharp
public interface IService_ReceivingValidation
⋮----
public Model_ReceivingValidationResult ValidatePONumber(string poNumber);
public Model_ReceivingValidationResult ValidatePartID(string partID);
public Model_ReceivingValidationResult ValidateNumberOfLoads(int numLoads);
public Model_ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity);
public Model_ReceivingValidationResult ValidatePackageCount(int packagesPerLoad);
public Model_ReceivingValidationResult ValidateHeatLotNumber(string heatLotNumber);
public Task<Model_ReceivingValidationResult> ValidateAgainstPOQuantityAsync(
⋮----
public Task<Model_ReceivingValidationResult> CheckSameDayReceivingAsync(
⋮----
public Model_ReceivingValidationResult ValidateReceivingLoad(Model_ReceivingLoad load);
public Model_ReceivingValidationResult ValidateSession(List<Model_ReceivingLoad> loads);
public Task<Model_ReceivingValidationResult> ValidatePartExistsInVisualAsync(string partID);
```

## File: Module_Receiving/Contracts/IService_ReceivingWorkflow.cs

```csharp
public interface IService_ReceivingWorkflow
⋮----
public void RaiseStatusMessage(string message);
public event EventHandler StepChanged;
⋮----
public Task<bool> StartWorkflowAsync();
public Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync();
public Model_ReceivingWorkflowStepResult GoToPreviousStep();
public Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step);
public Task AddCurrentPartToSessionAsync();
public Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null);
public void ClearUIInputs();
public Task<Model_SaveResult> SaveToCSVOnlyAsync();
public Task<Model_SaveResult> SaveToDatabaseOnlyAsync();
public Task ResetWorkflowAsync();
public Task<Model_CSVDeleteResult> ResetCSVFilesAsync();
public Task PersistSessionAsync();
```

## File: Module_Receiving/Data/Dao_PackageTypePreference.cs

```csharp
public class Dao_PackageTypePreference
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<Model_UserPreference>> GetByUserAsync(string username)
⋮----
reader => new Model_UserPreference
⋮----
Username = reader["username"].ToString() ?? string.Empty,
PreferredPackageType = reader["preferred_package_type"].ToString() ?? string.Empty,
Workstation = reader["workstation"].ToString() ?? string.Empty,
LastUpdated = Convert.ToDateTime(reader["last_modified"])
⋮----
public async Task<Model_Dao_Result> UpsertAsync(Model_UserPreference preference)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
return Model_Dao_Result_Factory.Failure(
⋮----
public async Task<Model_Dao_Result<Model_PackageTypePreference?>> GetPreferenceAsync(string partID)
⋮----
reader => new Model_PackageTypePreference
⋮----
PreferenceID = Convert.ToInt32(reader["PreferenceID"]),
PartID = reader["PartID"].ToString() ?? string.Empty,
PackageTypeName = reader["PackageTypeName"].ToString() ?? string.Empty,
CustomTypeName = reader["CustomTypeName"] == DBNull.Value ? null : reader["CustomTypeName"].ToString(),
LastModified = Convert.ToDateTime(reader["LastModified"])
⋮----
return Model_Dao_Result_Factory.Success(result.Data);
⋮----
public async Task<Model_Dao_Result> SavePreferenceAsync(Model_PackageTypePreference preference)
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
return Model_Dao_Result_Factory.Success();
⋮----
return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
⋮----
public async Task<Model_Dao_Result<bool>> DeletePreferenceAsync(string partID)
⋮----
return Model_Dao_Result_Factory.Success(true);
```

## File: Module_Receiving/Data/Dao_ReceivingLoad.cs

```csharp
public class Dao_ReceivingLoad
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
private string? CleanPONumber(string? poNumber)
⋮----
if (string.IsNullOrEmpty(poNumber))
⋮----
return poNumber.Replace("PO-", "", StringComparison.OrdinalIgnoreCase).Trim();
⋮----
public async Task<Model_Dao_Result<int>> SaveLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
await using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();
⋮----
{ "LoadID", load.LoadID.ToString() },
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
⋮----
throw new InvalidOperationException(result.ErrorMessage, result.Exception);
⋮----
await transaction.CommitAsync();
⋮----
await transaction.RollbackAsync();
⋮----
public async Task<Model_Dao_Result<int>> UpdateLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
public async Task<Model_Dao_Result<int>> DeleteLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
{ "p_LoadID", load.LoadID.ToString() }
⋮----
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetHistoryAsync(string partID, DateTime startDate, DateTime endDate)
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
⋮----
loads.Add(MapRowToLoad(row));
⋮----
return Model_Dao_Result_Factory.Success(loads);
⋮----
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync(DateTime startDate, DateTime endDate)
⋮----
private Model_ReceivingLoad MapRowToLoad(DataRow row)
⋮----
return new Model_ReceivingLoad
⋮----
LoadID = Guid.Parse(row["LoadID"]?.ToString() ?? Guid.Empty.ToString()),
⋮----
PoNumber = row["PONumber"] == DBNull.Value ? null : row["PONumber"].ToString(),
⋮----
LoadNumber = Convert.ToInt32(row["LoadNumber"]),
WeightQuantity = Convert.ToDecimal(row["WeightQuantity"]),
⋮----
PackagesPerLoad = Convert.ToInt32(row["PackagesPerLoad"]),
⋮----
WeightPerPackage = Convert.ToDecimal(row["WeightPerPackage"]),
IsNonPOItem = Convert.ToBoolean(row["IsNonPOItem"]),
ReceivedDate = Convert.ToDateTime(row["ReceivedDate"])
```

## File: Module_Receiving/Models/Model_ReceivingLine.cs

```csharp
public class Model_ReceivingLine
```

## File: Module_Receiving/Models/Model_ReceivingLoad.cs

```csharp
public partial class Model_ReceivingLoad : ObservableObject
⋮----
private Guid _loadID = Guid.NewGuid();
⋮----
private Enum_PackageType _packageType = Enum_PackageType.Skid;
⋮----
private DateTime _receivedDate = DateTime.Now;
⋮----
partial void OnPartIDChanged(string value)
⋮----
if (string.IsNullOrWhiteSpace(value))
⋮----
var upperValue = value.ToUpperInvariant();
if (upperValue.Contains("MMC"))
⋮----
else if (upperValue.Contains("MMF"))
⋮----
System.Diagnostics.Debug.WriteLine($"[Model_ReceivingLoad] OnPartIDChanged error: {ex.Message}");
⋮----
partial void OnPackageTypeChanged(Enum_PackageType value)
⋮----
PackageTypeName = value.ToString();
⋮----
partial void OnPackageTypeNameChanged(string value)
⋮----
partial void OnWeightQuantityChanged(decimal value)
⋮----
partial void OnPackagesPerLoadChanged(int value)
⋮----
private void CalculateWeightPerPackage()
⋮----
WeightPerPackage = Math.Round(WeightQuantity / PackagesPerLoad, 0);
⋮----
string.IsNullOrEmpty(PoNumber) ? "N/A" : PoNumber;
```

## File: Module_Receiving/Models/Model_ReceivingSession.cs

```csharp
public class Model_ReceivingSession
⋮----
public Guid SessionID { get; set; } = Guid.NewGuid();
⋮----
Loads?.Select(l => l.PartID).Distinct().ToList() ?? new List<string>();
```

## File: Module_Receiving/Models/Model_ReceivingValidationResult.cs

```csharp
public class Model_ReceivingValidationResult
⋮----
public static Model_ReceivingValidationResult Success() => new() { IsValid = true };
public static Model_ReceivingValidationResult Error(string message) => new()
⋮----
public static Model_ReceivingValidationResult Warning(string message) => new()
```

## File: Module_Receiving/Models/Model_ReceivingWorkflowStepResult.cs

```csharp
public class Model_ReceivingWorkflowStepResult
⋮----
public static Model_ReceivingWorkflowStepResult SuccessResult(Enum_ReceivingWorkflowStep newStep, string message = "") => new()
⋮----
public static Model_ReceivingWorkflowStepResult ErrorResult(List<string> errors) => new()
```

## File: Module_Receiving/Services/Service_CSVWriter.cs

```csharp
public class Service_CSVWriter : IService_CSVWriter
⋮----
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_LoggingUtility _logger;
⋮----
_sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var appDir = Path.Combine(appDataPath, "MTM_Receiving_Application");
if (!Directory.Exists(appDir))
⋮----
Directory.CreateDirectory(appDir);
⋮----
_localCSVPath = Path.Combine(appDir, "ReceivingData.csv");
⋮----
private string GetNetworkCSVPathInternal()
⋮----
_logger.LogInfo("Resolving network CSV path...");
⋮----
var userDir = Path.Combine(networkBase, userName);
_logger.LogInfo($"Checking network directory: {userDir}");
if (!Directory.Exists(userDir))
⋮----
_logger.LogInfo($"Creating network directory: {userDir}");
Directory.CreateDirectory(userDir);
⋮----
_logger.LogWarning($"Failed to create network directory: {ex.Message}");
⋮----
return Path.Combine(userDir, "ReceivingData.csv");
⋮----
_logger.LogError($"Error resolving network path: {ex.Message}");
⋮----
public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_ReceivingLoad> loads)
⋮----
_logger.LogInfo($"Starting WriteToCSVAsync for {loads?.Count ?? 0} loads.");
⋮----
throw new ArgumentException("Loads list cannot be null or empty", nameof(loads));
⋮----
var result = new Model_CSVWriteResult { RecordsWritten = loads.Count };
⋮----
_logger.LogInfo($"Writing to local CSV: {_localCSVPath}");
⋮----
_logger.LogInfo("Local CSV write successful.");
⋮----
_logger.LogError($"Local CSV write failed: {ex.Message}");
⋮----
throw new InvalidOperationException("Failed to write local CSV file", ex);
⋮----
_logger.LogInfo("Attempting network CSV write...");
var networkPath = await Task.Run(() => GetNetworkCSVPathInternal());
_logger.LogInfo($"Network path resolved: {networkPath}");
⋮----
_logger.LogInfo("Network CSV write successful.");
⋮----
_logger.LogWarning($"Network CSV write failed: {ex.Message}");
⋮----
public async Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads, bool append = true)
⋮----
_logger.LogInfo($"WriteToFileAsync called for: {filePath}, Append: {append}");
await Task.Run(async () =>
⋮----
bool isNewFile = !File.Exists(filePath) || !append;
_logger.LogInfo($"File exists check for {filePath}: {!isNewFile}");
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
⋮----
await using var stream = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.Read);
await using var writer = new StreamWriter(stream);
await using var csv = new CsvWriter(writer, config);
⋮----
csv.NextRecord();
⋮----
csv.WriteRecord(load);
⋮----
await writer.FlushAsync();
_logger.LogInfo($"Successfully wrote to {filePath}");
⋮----
_logger.LogError($"Error writing to file {filePath}: {ex.Message}");
⋮----
public async Task<List<Model_ReceivingLoad>> ReadFromCSVAsync(string filePath)
⋮----
_logger.LogInfo($"ReadFromCSVAsync called for: {filePath}");
return await Task.Run(() =>
⋮----
if (!File.Exists(filePath))
⋮----
throw new FileNotFoundException($"CSV file not found: {filePath}");
⋮----
using var reader = new StreamReader(filePath);
using var csv = new CsvReader(reader, config);
⋮----
_logger.LogInfo($"Successfully read {loads.Count} records from {filePath}");
⋮----
_logger.LogError($"Error reading from file {filePath}: {ex.Message}");
⋮----
public async Task<Model_CSVDeleteResult> ClearCSVFilesAsync()
⋮----
var result = new Model_CSVDeleteResult();
if (File.Exists(_localCSVPath))
⋮----
await Task.Run(() =>
⋮----
using var writer = new StreamWriter(_localCSVPath);
using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
⋮----
if (File.Exists(networkPath))
⋮----
using var writer = new StreamWriter(networkPath);
⋮----
public async Task<Model_CSVExistenceResult> CheckCSVFilesExistAsync()
⋮----
var result = new Model_CSVExistenceResult();
⋮----
result.LocalExists = File.Exists(_localCSVPath);
⋮----
result.NetworkExists = File.Exists(networkPath);
⋮----
public string GetLocalCSVPath() => _localCSVPath;
public string GetNetworkCSVPath() => GetNetworkCSVPathInternal();
```

## File: Module_Receiving/Services/Service_MySQL_PackagePreferences.cs

```csharp
public class Service_MySQL_PackagePreferences : IService_MySQL_PackagePreferences
⋮----
private readonly Dao_PackageTypePreference _dao;
⋮----
_dao = dao ?? throw new ArgumentNullException(nameof(dao));
⋮----
_dao = new Dao_PackageTypePreference(connectionString);
⋮----
public async Task<Model_PackageTypePreference?> GetPreferenceAsync(string partID)
⋮----
if (string.IsNullOrWhiteSpace(partID))
⋮----
throw new ArgumentException("Part ID cannot be null or empty", nameof(partID));
⋮----
var result = await _dao.GetPreferenceAsync(partID);
⋮----
public async Task SavePreferenceAsync(Model_PackageTypePreference preference)
⋮----
throw new ArgumentNullException(nameof(preference));
⋮----
var result = await _dao.SavePreferenceAsync(preference);
⋮----
throw new Exception(result.ErrorMessage);
⋮----
public async Task<bool> DeletePreferenceAsync(string partID)
⋮----
var result = await _dao.DeletePreferenceAsync(partID);
```

## File: Module_Receiving/Services/Service_MySQL_Receiving.cs

```csharp
public class Service_MySQL_Receiving : IService_MySQL_Receiving
⋮----
private readonly Dao_ReceivingLoad _receivingLoadDao;
private readonly IService_LoggingUtility _logger;
⋮----
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
_receivingLoadDao = new Dao_ReceivingLoad(connectionString);
⋮----
public async Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
_logger.LogInfo($"Saving {loads.Count} loads to database.");
var result = await _receivingLoadDao.SaveLoadsAsync(loads);
⋮----
_logger.LogInfo($"Successfully saved {result.Data} loads.");
⋮----
_logger.LogError($"Failed to save loads: {result.ErrorMessage}", result.Exception);
throw new InvalidOperationException(result.ErrorMessage, result.Exception);
⋮----
public async Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
_logger.LogInfo($"Updating {loads.Count} loads in database.");
var result = await _receivingLoadDao.UpdateLoadsAsync(loads);
⋮----
_logger.LogInfo($"Successfully updated {result.Data} loads.");
⋮----
_logger.LogError($"Failed to update loads: {result.ErrorMessage}", result.Exception);
⋮----
public async Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
_logger.LogInfo($"Deleting {loads.Count} loads from database.");
var result = await _receivingLoadDao.DeleteLoadsAsync(loads);
⋮----
_logger.LogInfo($"Successfully deleted {result.Data} loads.");
⋮----
_logger.LogError($"Failed to delete loads: {result.ErrorMessage}", result.Exception);
⋮----
public async Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate)
⋮----
var result = await _receivingLoadDao.GetHistoryAsync(partID, startDate, endDate);
⋮----
_logger.LogError($"Failed to get receiving history: {result.ErrorMessage}", result.Exception);
⋮----
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate)
⋮----
_logger.LogInfo($"Retrieving all receiving loads from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
var result = await _receivingLoadDao.GetAllAsync(startDate, endDate);
⋮----
_logger.LogInfo($"Retrieved {result.Data?.Count ?? 0} receiving loads from database");
⋮----
_logger.LogError($"Failed to retrieve receiving loads: {result.ErrorMessage}", result.Exception);
⋮----
public async Task<bool> TestConnectionAsync()
```

## File: Module_Receiving/Services/Service_MySQL_ReceivingLine.cs

```csharp
public class Service_MySQL_ReceivingLine : IService_MySQL_ReceivingLine
⋮----
private readonly Dao_ReceivingLine _receivingLineDao;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
⋮----
public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
⋮----
var result = await _receivingLineDao.InsertReceivingLineAsync(line);
⋮----
_logger.LogError(
⋮----
_logger.LogInfo(
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
return Model_Dao_Result_Factory.Failure(
```

## File: Module_Receiving/Services/Service_ReceivingWorkflow.cs

```csharp
public class Service_ReceivingWorkflow : IService_ReceivingWorkflow
⋮----
private readonly IService_SessionManager _sessionManager;
private readonly IService_CSVWriter _csvWriter;
private readonly IService_MySQL_Receiving _mysqlReceiving;
private readonly IService_ReceivingValidation _validation;
private readonly IService_LoggingUtility _logger;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
public void RaiseStatusMessage(string message)
⋮----
private Enum_ReceivingWorkflowStep _currentStep = Enum_ReceivingWorkflowStep.ModeSelection;
⋮----
_logger.LogInfo($"Changing step from {_currentStep} to {value}");
⋮----
_logger.LogInfo($"Step changed to {value} (event fired)");
⋮----
_sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
_csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
_mysqlReceiving = mysqlReceiving ?? throw new ArgumentNullException(nameof(mysqlReceiving));
_validation = validation ?? throw new ArgumentNullException(nameof(validation));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_viewModelRegistry = viewModelRegistry ?? throw new ArgumentNullException(nameof(viewModelRegistry));
⋮----
public async Task<bool> StartWorkflowAsync()
⋮----
_logger.LogInfo("Starting receiving workflow.");
var existingSession = await _sessionManager.LoadSessionAsync();
⋮----
_logger.LogInfo("Restoring existing session.");
⋮----
CurrentSession = new Model_ReceivingSession();
⋮----
if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultReceivingMode))
⋮----
switch (currentUser.DefaultReceivingMode.ToLower())
⋮----
_logger.LogInfo("Starting in Guided mode (default)");
⋮----
_logger.LogInfo("Starting in Manual Entry mode (default)");
⋮----
_logger.LogInfo("Starting in Edit mode (default)");
⋮----
_logger.LogInfo("Invalid default mode, showing mode selection");
⋮----
_logger.LogInfo("No default mode set, showing mode selection");
⋮----
public async Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync()
⋮----
return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep);
⋮----
if (string.IsNullOrEmpty(CurrentPONumber) && !IsNonPOItem)
⋮----
validationErrors.Add("PO Number is required.");
return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
⋮----
validationErrors.Add("Part selection is required.");
⋮----
validationErrors.Add("Number of loads must be at least 1.");
⋮----
var result = _validation.ValidateWeightQuantity(load.WeightQuantity);
⋮----
validationErrors.Add($"Load {load.LoadNumber}: {result.Message}");
⋮----
var result = _validation.ValidateHeatLotNumber(load.HeatLotNumber);
⋮----
var result = _validation.ValidatePackageCount(load.PackagesPerLoad);
⋮----
if (string.IsNullOrWhiteSpace(load.PackageTypeName))
⋮----
validationErrors.Add($"Load {load.LoadNumber}: Package Type is required.");
⋮----
_logger.LogInfo("Transitioning from Review to Saving...");
⋮----
validationErrors.Add($"Cannot advance from step {CurrentStep}");
⋮----
_logger.LogInfo("Persisting session...");
⋮----
_logger.LogInfo("Session persisted.");
return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Advanced to {CurrentStep}");
⋮----
private void GenerateLoads()
⋮----
CurrentSession.Loads.Remove(load);
⋮----
_currentBatchLoads.Clear();
⋮----
var load = new Model_ReceivingLoad
⋮----
CurrentSession.Loads.Add(load);
_currentBatchLoads.Add(load);
⋮----
public Model_ReceivingWorkflowStepResult GoToPreviousStep()
⋮----
return Model_ReceivingWorkflowStepResult.ErrorResult(new List<string> { $"Cannot go back from step {CurrentStep}" });
⋮----
return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Returned to {CurrentStep}");
⋮----
public Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step)
⋮----
return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Navigated to {CurrentStep}");
⋮----
public async Task AddCurrentPartToSessionAsync()
⋮----
public void ClearUIInputs()
⋮----
_viewModelRegistry.ClearAllInputs();
⋮----
public async Task<Model_SaveResult> SaveToCSVOnlyAsync()
⋮----
var result = new Model_SaveResult();
var validation = _validation.ValidateSession(CurrentSession.Loads);
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(CurrentSession.Loads);
⋮----
result.LocalCSVPath = _csvWriter.GetLocalCSVPath();
result.NetworkCSVPath = _csvWriter.GetNetworkCSVPath();
⋮----
result.Errors.Add($"Local CSV write failed: {csvResult.LocalError}");
⋮----
result.Warnings.Add($"Network CSV write failed: {csvResult.NetworkError}");
⋮----
result.Errors.Add($"CSV save failed: {ex.Message}");
_logger.LogError("CSV save failed", ex);
⋮----
public async Task<Model_SaveResult> SaveToDatabaseOnlyAsync()
⋮----
int savedCount = await _mysqlReceiving.SaveReceivingLoadsAsync(CurrentSession.Loads);
⋮----
result.Errors.Add($"Database save failed: {ex.Message}");
_logger.LogError("Database save failed", ex);
⋮----
public async Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null)
⋮----
_logger.LogInfo("Starting session save.");
⋮----
_logger.LogInfo("Validating session before save...");
⋮----
_logger.LogWarning($"Session validation failed: {string.Join(", ", validation.Errors)}");
⋮----
_logger.LogInfo("Reporting progress: Saving to local CSV...");
⋮----
result.Errors.AddRange(csvResult.Errors);
result.Warnings.AddRange(csvResult.Warnings);
_logger.LogInfo("Reporting progress: Saving to database...");
⋮----
result.Errors.AddRange(dbResult.Errors);
⋮----
_logger.LogInfo("Reporting progress: Finalizing...");
⋮----
_logger.LogInfo("Save completed successfully. Clearing session.");
await _sessionManager.ClearSessionAsync();
CurrentSession.Loads.Clear();
await _csvWriter.ClearCSVFilesAsync();
⋮----
_logger.LogWarning($"Save completed with errors. Success: {result.Success}");
⋮----
_logger.LogError("Unexpected error during save session", ex);
⋮----
result.Errors.Add($"Unexpected error: {ex.Message}");
⋮----
public async Task ResetWorkflowAsync()
⋮----
public async Task<Model_CSVDeleteResult> ResetCSVFilesAsync()
⋮----
_logger.LogInfo("Resetting CSV files requested.");
return await _csvWriter.ClearCSVFilesAsync();
⋮----
public async Task PersistSessionAsync()
⋮----
await _sessionManager.SaveSessionAsync(CurrentSession);
```

## File: Module_Receiving/Services/Service_SessionManager.cs

```csharp
public class Service_SessionManager : IService_SessionManager
⋮----
private readonly IService_LoggingUtility _logger;
⋮----
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var appFolder = Path.Combine(appDataPath, "MTM_Receiving_Application");
if (!Directory.Exists(appFolder))
⋮----
Directory.CreateDirectory(appFolder);
⋮----
_sessionPath = Path.Combine(appFolder, "session.json");
⋮----
public async Task SaveSessionAsync(Model_ReceivingSession session)
⋮----
_logger.LogInfo("SaveSessionAsync started.");
⋮----
throw new ArgumentNullException(nameof(session));
⋮----
var options = new JsonSerializerOptions
⋮----
_logger.LogInfo("Serializing session...");
var json = JsonSerializer.Serialize(session, options);
_logger.LogInfo($"Writing session to file: {_sessionPath}");
await File.WriteAllTextAsync(_sessionPath, json);
_logger.LogInfo("Session saved successfully.");
⋮----
_logger.LogError($"Failed to save session to file: {ex.Message}", ex);
throw new InvalidOperationException("Failed to save session to file", ex);
⋮----
public async Task<Model_ReceivingSession?> LoadSessionAsync()
⋮----
if (!File.Exists(_sessionPath))
⋮----
var json = await File.ReadAllTextAsync(_sessionPath);
⋮----
File.Delete(_sessionPath);
⋮----
throw new InvalidOperationException("Failed to load session from file", ex);
⋮----
public async Task<bool> ClearSessionAsync()
⋮----
await Task.Run(() => File.Delete(_sessionPath));
⋮----
public bool SessionExists()
⋮----
return File.Exists(_sessionPath);
⋮----
public string GetSessionFilePath()
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs

```csharp
public partial class ViewModel_Receiving_EditMode : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_MySQL_Receiving _mysqlService;
private readonly IService_CSVWriter _csvWriter;
private readonly IService_Pagination _paginationService;
private readonly IService_Help _helpService;
⋮----
private Enum_DataSourceType _currentDataSource = Enum_DataSourceType.Memory;
⋮----
private DateTimeOffset _filterStartDate = DateTimeOffset.Now.AddDays(-7);
⋮----
private DateTimeOffset _filterEndDate = DateTimeOffset.Now;
⋮----
private string _thisMonthButtonText = DateTime.Now.ToString("MMMM");
⋮----
private readonly IService_Window _windowService;
⋮----
_logger.LogInfo("Edit Mode initialized");
⋮----
private static string GetQuarterText(DateTime date)
⋮----
private void OnPageChanged(object? sender, EventArgs e)
⋮----
private void UpdatePagedDisplay()
⋮----
Loads.Clear();
⋮----
Loads.Add(item);
⋮----
private void NotifyPaginationCommands()
⋮----
NextPageCommand.NotifyCanExecuteChanged();
PreviousPageCommand.NotifyCanExecuteChanged();
FirstPageCommand.NotifyCanExecuteChanged();
LastPageCommand.NotifyCanExecuteChanged();
GoToPageCommand.NotifyCanExecuteChanged();
⋮----
private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
⋮----
private void Load_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
⋮----
RemoveRowCommand.NotifyCanExecuteChanged();
⋮----
private void NotifyCommands()
⋮----
SaveCommand.NotifyCanExecuteChanged();
⋮----
SelectAllCommand.NotifyCanExecuteChanged();
⋮----
partial void OnFilterStartDateChanged(DateTimeOffset value) => ApplyDateFilter();
partial void OnFilterEndDateChanged(DateTimeOffset value) => ApplyDateFilter();
private void ApplyDateFilter()
⋮----
private void FilterAndPaginate()
⋮----
var end = FilterEndDate.Date.AddDays(1).AddTicks(-1);
⋮----
_filteredLoads = _allLoads.Where(l => l.ReceivedDate >= start && l.ReceivedDate <= end).ToList();
⋮----
_paginationService.SetSource(_filteredLoads);
⋮----
private async Task SetFilterLastWeekAsync()
⋮----
FilterStartDate = DateTime.Today.AddDays(-7);
⋮----
private async Task SetFilterTodayAsync()
⋮----
private async Task SetFilterThisWeekAsync()
⋮----
var start = today.AddDays(-(int)today.DayOfWeek);
var end = start.AddDays(6);
⋮----
private async Task SetFilterThisMonthAsync()
⋮----
FilterStartDate = new DateTime(today.Year, today.Month, 1);
FilterEndDate = FilterStartDate.AddMonths(1).AddDays(-1);
⋮----
private async Task SetFilterThisQuarterAsync()
⋮----
FilterStartDate = new DateTime(today.Year, 3 * quarter - 2, 1);
FilterEndDate = FilterStartDate.AddMonths(3).AddDays(-1);
⋮----
private async Task SetFilterShowAllAsync()
⋮----
FilterStartDate = DateTime.Today.AddYears(-1);
⋮----
private void PreviousPage() => _paginationService.PreviousPage();
⋮----
private void NextPage() => _paginationService.NextPage();
⋮----
private void FirstPage() => _paginationService.FirstPage();
⋮----
private void LastPage() => _paginationService.LastPage();
⋮----
private void GoToPage() => _paginationService.GoToPage(GotoPageNumber);
private bool CanGoNext() => _paginationService.HasNextPage;
private bool CanGoPrevious() => _paginationService.HasPreviousPage;
⋮----
private async Task LoadFromCurrentMemoryAsync()
⋮----
_logger.LogInfo("Loading data from current memory");
⋮----
_deletedLoads.Clear();
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
_allLoads.Clear();
⋮----
_allLoads.Add(load);
⋮----
_logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from current memory");
⋮----
FilterStartDate = DateTimeOffset.Now.AddYears(-1);
⋮----
_logger.LogError($"Failed to load from current memory: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to load data from current session", Enum_ErrorSeverity.Error, ex);
⋮----
private async Task LoadFromCurrentLabelsAsync()
⋮----
_logger.LogInfo("User initiated Current Labels (CSV) load");
⋮----
await _errorHandler.ShowErrorDialogAsync(
⋮----
_logger.LogError($"Failed to load from labels: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to load data from label file", Enum_ErrorSeverity.Error, ex);
⋮----
private async Task<bool> TryLoadFromDefaultCsvAsync()
⋮----
string localPath = _csvWriter.GetLocalCSVPath();
if (File.Exists(localPath))
⋮----
_logger.LogInfo($"Attempting to load from local CSV: {localPath}");
var loadedData = await _csvWriter.ReadFromCSVAsync(localPath);
⋮----
_workflowService.CurrentSession.Loads.Add(load);
⋮----
_logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from local labels");
⋮----
_logger.LogWarning($"Failed to load from local labels: {ex.Message}");
⋮----
string networkPath = _csvWriter.GetNetworkCSVPath();
if (File.Exists(networkPath))
⋮----
_logger.LogInfo($"Attempting to load from network labels: {networkPath}");
var loadedData = await _csvWriter.ReadFromCSVAsync(networkPath);
⋮----
_logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from network labels");
⋮----
_logger.LogWarning($"Failed to load from network labels: {ex.Message}");
⋮----
private async Task LoadFromHistoryAsync()
⋮----
_logger.LogInfo("User initiated history load");
⋮----
_logger.LogInfo($"Loading receiving history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
var result = await _mysqlService.GetAllReceivingLoadsAsync(startDate, endDate);
⋮----
_logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from history");
⋮----
_logger.LogError($"Failed to load from history: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to load data from history", Enum_ErrorSeverity.Error, ex);
⋮----
private void SelectAll()
⋮----
bool anyUnselected = Loads.Any(l => !l.IsSelected);
⋮----
private bool CanSelectAll() => Loads.Count > 0;
⋮----
private void RemoveRow()
⋮----
var selectedLoads = Loads.Where(l => l.IsSelected).ToList();
⋮----
_logger.LogInfo($"Removing {selectedLoads.Count} selected loads");
⋮----
_deletedLoads.Add(load);
_workflowService.CurrentSession.Loads.Remove(load);
_allLoads.Remove(load);
_filteredLoads.Remove(load);
Loads.Remove(load);
⋮----
_logger.LogInfo($"Removing load {SelectedLoad.LoadNumber} (Part ID: {SelectedLoad.PartID})");
_deletedLoads.Add(SelectedLoad);
_workflowService.CurrentSession.Loads.Remove(SelectedLoad);
_allLoads.Remove(SelectedLoad);
_filteredLoads.Remove(SelectedLoad);
Loads.Remove(SelectedLoad);
⋮----
_logger.LogWarning("RemoveRow called with no selected load(s)");
⋮----
private bool CanRemoveRow() => Loads.Any(l => l.IsSelected);
⋮----
private async Task SaveAsync()
⋮----
_logger.LogInfo($"Validating and saving {_filteredLoads.Count} loads from edit mode");
⋮----
if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
⋮----
var errorMessage = string.Join("\n", validationErrors);
_logger.LogWarning($"Edit mode validation failed: {validationErrors.Count} errors");
⋮----
await _workflowService.AdvanceToNextStepAsync();
⋮----
if (string.IsNullOrEmpty(_currentCsvPath))
⋮----
await _errorHandler.HandleErrorAsync("No label file path available for saving.", Enum_ErrorSeverity.Error);
⋮----
_logger.LogInfo($"Overwriting label file: {_currentCsvPath}");
await _csvWriter.WriteToFileAsync(_currentCsvPath, _allLoads, append: false);
⋮----
await _errorHandler.ShowErrorDialogAsync("Success", "Label file updated successfully.", Enum_ErrorSeverity.Info);
⋮----
_logger.LogInfo("Updating history records");
⋮----
_logger.LogInfo($"Deleting {_deletedLoads.Count} removed records");
deleted = await _mysqlService.DeleteReceivingLoadsAsync(_deletedLoads);
⋮----
updated = await _mysqlService.UpdateReceivingLoadsAsync(_filteredLoads);
⋮----
await _errorHandler.ShowErrorDialogAsync("Success", $"History updated successfully.\n{updated} records updated.\n{deleted} records deleted.", Enum_ErrorSeverity.Info);
⋮----
_logger.LogInfo($"Edit mode save completed successfully for source: {CurrentDataSource}");
⋮----
_logger.LogError($"Failed to save edit mode data: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to save receiving data", Enum_ErrorSeverity.Critical, ex);
⋮----
private bool CanSave()
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
⋮----
var result = await dialog.ShowAsync().AsTask();
⋮----
_logger.LogInfo("User confirmed return to mode selection, resetting workflow");
await _workflowService.ResetWorkflowAsync();
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
⋮----
_logger.LogError($"Failed to reset workflow: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex);
⋮----
_logger.LogInfo("User cancelled return to mode selection");
⋮----
private System.Collections.Generic.List<string> ValidateLoads(IEnumerable<Model_ReceivingLoad> loadsToValidate)
⋮----
if (!loadsToValidate.Any() && _deletedLoads.Count == 0)
⋮----
errors.Add("No loads to save");
⋮----
if (string.IsNullOrWhiteSpace(load.PartID))
⋮----
errors.Add($"Load #{load.LoadNumber}: Part ID is required");
⋮----
errors.Add($"Load #{load.LoadNumber}: Weight/Quantity must be greater than zero");
⋮----
errors.Add($"Load #{load.LoadNumber}: Packages per load must be greater than zero");
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.EditMode");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs

```csharp
public partial class ViewModel_Receiving_ManualEntry : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_MySQL_Receiving _mysqlService;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
private async Task AutoFillAsync()
⋮----
_logger.LogInfo("Starting Auto-Fill Blank Spaces operation");
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.PartID))
⋮----
if (!string.IsNullOrWhiteSpace(prev.PartID))
⋮----
if (!string.IsNullOrWhiteSpace(currentLoad.PartID))
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.PoNumber) && !string.IsNullOrWhiteSpace(sourceLoad.PoNumber))
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.HeatLotNumber) && !string.IsNullOrWhiteSpace(sourceLoad.HeatLotNumber))
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.PackageTypeName) && !string.IsNullOrWhiteSpace(sourceLoad.PackageTypeName))
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.UserId))
⋮----
if (i > 0 && !string.IsNullOrWhiteSpace(Loads[i - 1].UserId))
⋮----
else if (Loads.Count > 0 && !string.IsNullOrWhiteSpace(Loads[0].UserId))
⋮----
_logger.LogInfo($"Auto-Fill Blank Spaces completed. Updated {filledCount} fields across {Loads.Count} rows.");
⋮----
_logger.LogError($"Auto-fill failed: {ex.Message}");
await _errorHandler.HandleErrorAsync("Auto-fill failed", Enum_ErrorSeverity.Error, ex);
⋮----
private void AddRow()
⋮----
private async Task AddMultipleRowsAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
var result = await dialog.ShowAsync();
⋮----
if (int.TryParse(inputTextBox.Text, out int count) && count > 0 && count <= 50)
⋮----
_logger.LogInfo($"Adding {count} new rows to manual entry grid");
⋮----
_logger.LogWarning($"Invalid row count entered: {inputTextBox.Text}");
await _errorHandler.HandleErrorAsync("Please enter a valid number between 1 and 50.", Enum_ErrorSeverity.Warning);
⋮----
public void AddNewLoad()
⋮----
var newLoad = new Model_ReceivingLoad
⋮----
LoadID = System.Guid.NewGuid(),
⋮----
Loads.Add(newLoad);
_workflowService.CurrentSession.Loads.Add(newLoad);
⋮----
private void RemoveRow()
⋮----
_logger.LogInfo($"Removing load {SelectedLoad.LoadNumber} (Part ID: {SelectedLoad.PartID})");
_workflowService.CurrentSession.Loads.Remove(SelectedLoad);
Loads.Remove(SelectedLoad);
⋮----
_logger.LogWarning("RemoveRow called with no selected load");
⋮----
private async Task SaveAsync()
⋮----
_logger.LogInfo($"Saving {Loads.Count} loads from manual entry");
⋮----
if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
⋮----
await _workflowService.AdvanceToNextStepAsync();
⋮----
_logger.LogError($"Failed to save manual entry data: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to save receiving data", Enum_ErrorSeverity.Critical, ex);
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
⋮----
var result = await dialog.ShowAsync().AsTask();
⋮----
_logger.LogInfo("User confirmed return to mode selection, resetting workflow");
await _workflowService.ResetWorkflowAsync();
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
⋮----
_logger.LogError($"Failed to reset workflow: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex);
⋮----
_logger.LogInfo("User cancelled return to mode selection");
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.ManualEntry");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_PackageType.cs

```csharp
public partial class ViewModel_Receiving_PackageType : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_MySQL_PackagePreferences _preferencesService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_Help _helpService;
⋮----
private void OnStepChanged(object? sender, System.EventArgs e)
⋮----
public async Task OnNavigatedToAsync()
⋮----
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
private async Task LoadPreferencesAsync()
⋮----
if (string.IsNullOrEmpty(partId))
⋮----
var preference = await _preferencesService.GetPreferenceAsync(partId);
⋮----
if (PackageTypes.Contains(preference.PackageTypeName))
⋮----
if (partID.StartsWith("MMC", StringComparison.OrdinalIgnoreCase))
⋮----
else if (partID.StartsWith("MMF", StringComparison.OrdinalIgnoreCase))
⋮----
partial void OnSelectedPackageTypeChanged(string value)
⋮----
SavePreferenceAsync().ConfigureAwait(false);
⋮----
partial void OnCustomPackageTypeNameChanged(string value)
⋮----
partial void OnIsSaveAsDefaultChanged(bool value)
⋮----
DeletePreferenceAsync().ConfigureAwait(false);
⋮----
private void UpdateLoadsPackageType()
⋮----
private async Task SavePreferenceAsync()
⋮----
_workflowService.RaiseStatusMessage("Part ID too long (max 50 chars).");
⋮----
if (string.IsNullOrWhiteSpace(typeName))
⋮----
_workflowService.RaiseStatusMessage("Please enter a package type name.");
⋮----
_workflowService.RaiseStatusMessage("Package type name too long (max 50 chars).");
⋮----
if (!_regex.IsMatch(typeName))
⋮----
_workflowService.RaiseStatusMessage("Invalid characters in package type name.");
⋮----
var preference = new Model_PackageTypePreference
⋮----
await _preferencesService.SavePreferenceAsync(preference);
_workflowService.RaiseStatusMessage("Preference saved.");
⋮----
await _errorHandler.HandleErrorAsync(msg, Enum_ErrorSeverity.Warning, ex);
⋮----
private async Task DeletePreferenceAsync()
⋮----
await _preferencesService.DeletePreferenceAsync(partId);
_workflowService.RaiseStatusMessage("Preference deleted.");
⋮----
await _errorHandler.HandleErrorAsync("Failed to delete preference", Enum_ErrorSeverity.Warning, ex);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.PackageType");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs

```csharp
public sealed partial class View_Receiving_ManualEntry : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this, AddRowButton);
⋮----
private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
⋮----
Debug.WriteLine("[ManualEntryView] Loads_CollectionChanged: New row added");
ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[ManualEntryView] Loads_CollectionChanged: Selecting new item LoadNumber={newItem.LoadNumber}");
⋮----
ManualEntryDataGrid.ScrollIntoView(newItem, ManualEntryDataGrid.Columns.FirstOrDefault());
_ = Task.Run(async () =>
⋮----
await Task.Delay(100);
⋮----
private void ManualEntryDataGrid_CurrentCellChanged(object? sender, EventArgs e)
⋮----
grid?.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[ManualEntryView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
grid.BeginEdit();
⋮----
private void ManualEntryDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
⋮----
Debug.WriteLine($"[ManualEntryView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
⋮----
private void ManualEntryDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
⋮----
Debug.WriteLine($"[ManualEntryView] Tapped: OriginalSource={e.OriginalSource}");
⋮----
Debug.WriteLine("[ManualEntryView] Tapped: Grid empty, triggering AddRow command.");
if (ViewModel.AddRowCommand.CanExecute(null))
⋮----
ViewModel.AddRowCommand.Execute(null);
⋮----
Debug.WriteLine("[ManualEntryView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
⋮----
Debug.WriteLine("[ManualEntryView] Tapped: Cell clicked. Enqueuing BeginEdit.");
grid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[ManualEntryView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
⋮----
private void SelectFirstEditableCell(DataGrid grid)
⋮----
Debug.WriteLine("[ManualEntryView] SelectFirstEditableCell: Starting");
⋮----
var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
⋮----
Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
⋮----
grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
⋮----
Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
⋮----
Debug.WriteLine("[ManualEntryView] SelectFirstEditableCell: No editable column found.");
⋮----
Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
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

## File: Module_Routing/Services/IRoutingInforVisualService.cs

```csharp
public interface IRoutingInforVisualService
⋮----
public Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber);
public Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPoLinesAsync(string poNumber);
public Task<Model_Dao_Result<Model_InforVisualPOLine>> GetPoLineAsync(string poNumber, string lineNumber);
public Task<Model_Dao_Result<bool>> CheckConnectionAsync();
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

```xml
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

## File: Module_Settings/Interfaces/IService_SettingsWorkflow.cs

```csharp
public interface IService_SettingsWorkflow
⋮----
public void GoToStep(Enum_SettingsWorkflowStep step);
public void GoBack();
public void Reset();
```

## File: Module_Settings/Models/Model_ConnectionTestResult.cs

```csharp
public partial class Model_ConnectionTestResult : ObservableObject
```

## File: Module_Settings/Models/Model_DaoTestResult.cs

```csharp
public partial class Model_DaoTestResult : ObservableObject
```

## File: Module_Settings/Models/Model_SettingValue.cs

```csharp
public class Model_SettingValue
⋮----
public string AsString() => RawValue ?? string.Empty;
public int AsInt() => int.TryParse(RawValue, out var val) ? val : 0;
public bool AsBool() => bool.TryParse(RawValue, out var val) && val;
public double AsDouble() => double.TryParse(RawValue, out var val) ? val : 0.0;
public T? AsJson<T>()
⋮----
return string.IsNullOrWhiteSpace(RawValue)
⋮----
public object? AsTyped()
```

## File: Module_Settings/Models/Model_StoredProcedureTestResult.cs

```csharp
public partial class Model_StoredProcedureTestResult : ObservableObject
```

## File: Module_Settings/Models/Model_TableTestResult.cs

```csharp
public partial class Model_TableTestResult : ObservableObject
```

## File: Module_Settings/Services/Service_UserPreferences.cs

```csharp
public class Service_UserPreferences : IService_UserPreferences
⋮----
private readonly Dao_User _userDao;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
⋮----
public async Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username)
⋮----
if (string.IsNullOrWhiteSpace(normalizedUsername))
⋮----
var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
⋮----
_logger.LogError(
⋮----
var preference = new Model_UserPreference
⋮----
DefaultReceivingMode = string.IsNullOrWhiteSpace(userResult.Data.DefaultReceivingMode)
⋮----
DefaultDunnageMode = string.IsNullOrWhiteSpace(userResult.Data.DefaultDunnageMode)
⋮----
return Model_Dao_Result_Factory.Success(preference);
⋮----
_logger.LogError($"Error getting user preference for {username}", ex, "UserPreferences");
⋮----
public async Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode)
⋮----
return Model_Dao_Result_Factory.Success();
⋮----
_logger.LogWarning($"Default mode update skipped: user not found ({normalizedUsername})");
⋮----
var normalizedMode = string.IsNullOrWhiteSpace(defaultMode)
⋮----
: defaultMode.Trim().ToLowerInvariant();
var result = await _userDao.UpdateDefaultModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
⋮----
_logger.LogWarning($"Default mode update failed (non-blocking): {result.ErrorMessage}");
⋮----
_logger.LogError($"Error updating default mode for {username} (non-blocking)", ex, "UserPreferences");
⋮----
public async Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string defaultMode)
⋮----
_logger.LogWarning($"Receiving default update skipped: user not found ({normalizedUsername})");
⋮----
var result = await _userDao.UpdateDefaultReceivingModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
⋮----
_logger.LogWarning($"Receiving default update failed (non-blocking): {result.ErrorMessage}");
⋮----
_logger.LogError($"Error updating receiving default for {username} (non-blocking)", ex, "UserPreferences");
⋮----
public async Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string defaultMode)
⋮----
_logger.LogWarning($"Dunnage default update skipped: user not found ({normalizedUsername})");
⋮----
var result = await _userDao.UpdateDefaultDunnageModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
⋮----
_logger.LogWarning($"Dunnage default update failed (non-blocking): {result.ErrorMessage}");
⋮----
_logger.LogError($"Error updating dunnage default for {username} (non-blocking)", ex, "UserPreferences");
```

## File: Module_Settings/Views/View_Settings_DatabaseTest.xaml.cs

```csharp
public sealed partial class View_Settings_DatabaseTest : Page
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Constructor started");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Attempting to get ViewModel from DI");
⋮----
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] ViewModel obtained: {ViewModel != null}");
⋮----
throw new InvalidOperationException("Failed to obtain ViewModel from DI - returned null");
⋮----
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Failed to create ViewModel: {ex.Message}");
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Exception type: {ex.GetType().FullName}");
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Stack trace: {ex.StackTrace}");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Calling InitializeComponent()");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] InitializeComponent() completed");
⋮----
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] InitializeComponent() FAILED: {ex.Message}");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Setting DataContext");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Attempting to get Focus service");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Attaching focus service");
focusService.AttachFocusOnVisibility(this);
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Focus service is null");
⋮----
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Focus service failed (non-critical): {ex.Message}");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Attaching Loaded event handler");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Constructor completed");
⋮----
private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] OnLoaded event fired");
⋮----
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Executing RunAllTestsCommand");
await ViewModel.RunAllTestsCommand.ExecuteAsync(null);
System.Diagnostics.Debug.WriteLine("[DatabaseTestView] RunAllTestsCommand completed");
⋮----
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Cannot run tests - ViewModel: {ViewModel != null}, Command: {ViewModel?.RunAllTestsCommand != null}");
⋮----
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Error running database tests: {ex.Message}");
⋮----
private void OnSchemaTabClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
private void OnStoredProceduresTabClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
private void OnDaosTabClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
private void OnLogsTabClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
private void UpdateTabVisibility()
⋮----
System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Selected tab: {ViewModel.SelectedTab}");
```

## File: Module_Shared/ViewModels/ViewModel_Shared_MainWindow.cs

```csharp
public partial class ViewModel_Shared_MainWindow : ViewModel_Shared_Base
⋮----
private readonly IService_UserSessionManager _sessionManager;
⋮----
private void OnNotificationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
⋮----
private void UpdateUserDisplay(Model_User user)
```

## File: Module_Volvo/Contracts/IService_Volvo.cs

```csharp
public interface IService_Volvo
⋮----
public Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
⋮----
public Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(int shipmentId);
public Task<string> FormatEmailTextAsync(
⋮----
public Task<Model_VolvoEmailData> FormatEmailDataAsync(
⋮----
public string FormatEmailAsHtml(Model_VolvoEmailData emailData);
public Task<Model_Dao_Result> ValidateShipmentAsync(
⋮----
public Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
⋮----
public Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync();
public Task<Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>> GetPendingShipmentWithLinesAsync();
public Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber);
public Task<Model_Dao_Result<List<Model_VolvoPart>>> GetActivePartsAsync();
public Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
⋮----
public Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetShipmentLinesAsync(int shipmentId);
public Task<Model_Dao_Result> UpdateShipmentAsync(
⋮----
public Task<Model_Dao_Result<string>> ExportHistoryToCsvAsync(
```

## File: Module_Volvo/Contracts/IService_VolvoMasterData.cs

```csharp
public interface IService_VolvoMasterData
⋮----
public Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllPartsAsync(bool includeInactive = false);
public Task<Model_Dao_Result<Model_VolvoPart?>> GetPartByNumberAsync(string partNumber);
public Task<Model_Dao_Result> AddPartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null);
public Task<Model_Dao_Result> UpdatePartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null);
public Task<Model_Dao_Result> DeactivatePartAsync(string partNumber);
public Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetComponentsAsync(string partNumber);
public Task<Model_Dao_Result<(int New, int Updated, int Unchanged)>> ImportCsvAsync(string csvFilePath);
public Task<Model_Dao_Result<string>> ExportCsvAsync(string csvFilePath, bool includeInactive = false);
```

## File: Module_Volvo/Interfaces/IDao_VolvoPart.cs

```csharp
public interface IDao_VolvoPart
⋮----
Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllAsync(bool includeInactive = false);
Task<Model_Dao_Result<Model_VolvoPart>> GetByPartNumberAsync(string partNumber);
Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoPart part);
Task<Model_Dao_Result> UpdateAsync(Model_VolvoPart part);
Task<Model_Dao_Result> DeactivateAsync(string partNumber);
```

## File: Module_Volvo/Interfaces/IDao_VolvoShipment.cs

```csharp
public interface IDao_VolvoShipment
⋮----
Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetAllAsync(bool includeArchived = false);
Task<Model_Dao_Result<Model_VolvoShipment>> GetByIdAsync(int shipmentId);
Task<Model_Dao_Result<Model_VolvoShipment>> GetPendingAsync();
Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoShipment shipment);
Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipment shipment);
Task<Model_Dao_Result> ArchiveAsync(int shipmentId);
```

## File: Module_Volvo/Interfaces/IDao_VolvoShipmentLine.cs

```csharp
public interface IDao_VolvoShipmentLine
⋮----
Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetByShipmentIdAsync(int shipmentId);
Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoShipmentLine line);
Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipmentLine line);
Task<Model_Dao_Result> DeleteAsync(int lineId);
```

## File: Module_Volvo/Models/Model_EmailRecipient.cs

```csharp
public class Model_EmailRecipient
⋮----
public string ToOutlookFormat()
```

## File: Module_Volvo/Models/Model_VolvoEmailData.cs

```csharp
public class Model_VolvoEmailData
⋮----
public class DiscrepancyLineItem
```

## File: Module_Volvo/Models/Model_VolvoPartComponent.cs

```csharp
public class Model_VolvoPartComponent
```

## File: Module_Volvo/Models/Model_VolvoShipment.cs

```csharp
public class Model_VolvoShipment
```

## File: Module_Volvo/Models/VolvoShipmentStatus.cs

```csharp
public static class VolvoShipmentStatus
```

## File: Module_Volvo/Services/IService_VolvoAuthorization.cs

```csharp
public interface IService_VolvoAuthorization
⋮----
Task<Model_Dao_Result> CanManageShipmentsAsync();
Task<Model_Dao_Result> CanManageMasterDataAsync();
Task<Model_Dao_Result> CanCompleteShipmentsAsync();
Task<Model_Dao_Result> CanGenerateLabelsAsync();
```

## File: Module_Volvo/Views/VolvoPartAddEditDialog.xaml

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.VolvoPartAddEditDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="Add/Edit Volvo Part"
    PrimaryButtonText="Save"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="OnSaveClicked"
    Width="500">

    <StackPanel Spacing="16" Padding="8">
        <!-- Part Number -->
        <TextBox x:Name="PartNumberTextBox"
                 Header="Part Number"
                 PlaceholderText="e.g., V-EMB-500"
                 MaxLength="50"/>

        <!-- Quantity Per Skid -->
        <NumberBox x:Name="QuantityPerSkidNumberBox"
                   Header="Quantity Per Skid"
                   PlaceholderText="e.g., 88"
                   Minimum="0"
                   Maximum="10000"
                   SpinButtonPlacementMode="Inline"
                   Value="0"/>

        <!-- Historical Integrity Warning (Edit Mode Only) -->
        <InfoBar x:Name="EditModeWarning"
                 Severity="Warning"
                 IsOpen="False"
                 IsClosable="False"
                 Message="Changes to quantity will NOT affect past shipments. Historical piece counts are preserved."/>

        <!-- Component Section (Simplified - Future Enhancement) -->
        <Expander Header="Components (Advanced)" IsExpanded="False">
            <StackPanel Spacing="8" Padding="8">
                <TextBlock Text="Component management will be available in a future update." 
                          Style="{StaticResource CaptionTextBlockStyle}"
                          Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                <TextBlock Text="For now, use CSV import to define components." 
                          Style="{StaticResource CaptionTextBlockStyle}"
                          Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
            </StackPanel>
        </Expander>
    </StackPanel>
</ContentDialog>
```

## File: MTM_Receiving_Application.Tests/Fixtures/DatabaseFixture.cs

```csharp
public sealed class DatabaseFixture : IDisposable
⋮----
public void Dispose()
```

## File: MTM_Receiving_Application.Tests/Fixtures/ServiceCollectionFixture.cs

```csharp
public sealed class ServiceCollectionFixture : IDisposable
⋮----
public void Dispose()
```

## File: MTM_Receiving_Application.Tests/Helpers/TestHelper.cs

```csharp
public static class TestHelper
⋮----
public static string NewGuidString() => Guid.NewGuid().ToString("N");
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_BooleanToVisibility_Tests.cs

```csharp
public class Converter_BooleanToVisibility_Tests
⋮----
private readonly Converter_BooleanToVisibility _sut;
⋮----
public void Convert_WithBooleanValue_ReturnsExpectedVisibility(bool input, string? parameter, Visibility expected)
⋮----
var result = _sut.Convert(input, typeof(Visibility), parameter, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithNullValue_ReturnsCollapsed()
⋮----
var result = _sut.Convert(null, typeof(Visibility), null, "en-US");
result.Should().Be(Visibility.Collapsed);
⋮----
public void Convert_WithNonBooleanValue_ReturnsCollapsed(object input)
⋮----
var result = _sut.Convert(input, typeof(Visibility), null, "en-US");
⋮----
public void Convert_WithUnknownParameter_IgnoresParameter()
⋮----
var result = _sut.Convert(true, typeof(Visibility), "SomeOtherParam", "en-US");
result.Should().Be(Visibility.Visible);
⋮----
public void ConvertBack_ThrowsNotImplementedException()
⋮----
Action act = () => _sut.ConvertBack(Visibility.Visible, typeof(bool), null, "en-US");
act.Should().Throw<NotImplementedException>();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_BoolToString_Tests.cs

```csharp
public class Converter_BoolToString_Tests
⋮----
private readonly Converter_BoolToString _sut;
⋮----
public void Convert_WithBooleanAndValidParameter_ReturnsCorrectString(bool input, string parameter, string expected)
⋮----
var result = _sut.Convert(input, typeof(string), parameter, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithNullValue_ReturnsEmptyString()
⋮----
var result = _sut.Convert(null, typeof(string), "Yes|No", "en-US");
result.Should().Be(string.Empty);
⋮----
public void Convert_WithNullParameter_ReturnsBooleanToString(bool input, object? parameter)
⋮----
result.Should().Be(input.ToString());
⋮----
public void Convert_WithInvalidParameter_ReturnsBooleanToString(bool input, string parameter)
⋮----
public void Convert_WithNonBooleanValue_ReturnsValueToString(object input, string parameter)
⋮----
public void Convert_WithMoreThanTwoParts_ReturnsOriginalBooleanString(bool input, string parameter)
⋮----
public void ConvertBack_ThrowsNotImplementedException()
⋮----
Action act = () => _sut.ConvertBack("Yes", typeof(bool), "Yes|No", "en-US");
act.Should().Throw<NotImplementedException>();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_DecimalToString_Tests.cs

```csharp
public class Converter_DecimalToString_Tests
⋮----
private readonly Converter_DecimalToString _sut;
⋮----
public void Convert_WithDecimalValue_ReturnsExpectedString(decimal input, string expected)
⋮----
var result = _sut.Convert(input, typeof(string), null, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithVeryPreciseDecimal_ReturnsPreciseString()
⋮----
result.Should().BeOfType<string>();
⋮----
resultString.Should().NotBeNullOrEmpty();
resultString.Should().StartWith("0.12345678901234");
⋮----
public void Convert_WithZero_ReturnsEmptyString()
⋮----
var result = _sut.Convert(0m, typeof(string), null, "en-US");
result.Should().Be(string.Empty);
⋮----
public void Convert_WithNonDecimalValue_ReturnsEmptyString(object? input)
⋮----
public void ConvertBack_WithValidString_ReturnsDecimal(string input, decimal expected)
⋮----
var result = _sut.ConvertBack(input, typeof(decimal), null, "en-US");
⋮----
public void ConvertBack_WithEmptyOrWhitespaceString_ReturnsZero(string? input)
⋮----
// Act
⋮----
result.Should().Be(0m);
⋮----
public void ConvertBack_WithInvalidString_ReturnsZero(string input)
⋮----
public void ConvertBack_WithNonStringValue_ReturnsZero()
⋮----
var result = _sut.ConvertBack(42, typeof(decimal), null, "en-US");
⋮----
public void Convert_RoundTrip_PreservesValue()
⋮----
var stringResult = _sut.Convert(originalValue, typeof(string), null, "en-US");
var decimalResult = _sut.ConvertBack(stringResult, typeof(decimal), null, "en-US");
decimalResult.Should().Be(originalValue);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_DoubleToDecimal_Tests.cs

```csharp
public class Converter_DoubleToDecimal_Tests
⋮----
private readonly Converter_DoubleToDecimal _sut;
⋮----
public void Convert_WithDecimalValue_ReturnsDouble(decimal input, double expected)
⋮----
var result = _sut.Convert(input, typeof(double), null, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithNonDecimalValue_ReturnsZeroDouble(object? input)
⋮----
result.Should().Be(0.0);
⋮----
public void ConvertBack_WithDoubleValue_ReturnsDecimal(double input, decimal expected)
⋮----
var result = _sut.ConvertBack(input, typeof(decimal), null, "en-US");
⋮----
public void ConvertBack_WithNonDoubleValue_ReturnsZeroDecimal(object? input)
⋮----
result.Should().Be(0m);
⋮----
public void Convert_RoundTrip_PreservesValue()
⋮----
var doubleResult = _sut.Convert(originalValue, typeof(double), null, "en-US");
var decimalResult = _sut.ConvertBack(doubleResult, typeof(decimal), null, "en-US");
decimalResult.Should().Be(originalValue);
⋮----
public void Convert_WithLargeDecimalValue_HandlesWithinDoubleRange()
⋮----
result.Should().BeOfType<double>();
((double)result).Should().BeApproximately(79228162514264.0, 0.1);
⋮----
public void ConvertBack_WithLargeDoubleValue_HandlesWithinDecimalRange()
⋮----
result.Should().BeOfType<decimal>();
((decimal)result).Should().BeApproximately(79228162514264m, 1m);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_EmptyStringToVisibility_Tests.cs

```csharp
public class Converter_EmptyStringToVisibility_Tests
⋮----
private readonly Converter_EmptyStringToVisibility _sut;
⋮----
public void Convert_WithStringValue_ReturnsExpectedVisibility(string? input, object? parameter, Visibility expected)
⋮----
// Act
var result = _sut.Convert(input, typeof(Visibility), parameter, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithInverseParameter_ReturnsInvertedVisibility(string? input, string parameter, Visibility expected)
⋮----
public void Convert_WithNonStringValue_ReturnsCollapsed(object input)
⋮----
var result = _sut.Convert(input, typeof(Visibility), null, "en-US");
result.Should().Be(Visibility.Collapsed);
⋮----
public void Convert_WithUnknownParameter_IgnoresParameter()
⋮----
var result = _sut.Convert("Hello", typeof(Visibility), "SomeOtherParam", "en-US");
result.Should().Be(Visibility.Visible);
⋮----
public void ConvertBack_ThrowsNotImplementedException()
⋮----
Action act = () => _sut.ConvertBack(Visibility.Visible, typeof(string), null, "en-US");
act.Should().Throw<NotImplementedException>();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_EnumToVisibility_Tests.cs

```csharp
public class Converter_EnumToVisibility_Tests
⋮----
private readonly Converter_EnumToVisibility _sut;
⋮----
public void Convert_WithEnumValue_ReturnsExpectedVisibility(TestEnum input, string parameter, Visibility expected)
⋮----
var result = _sut.Convert(input, typeof(Visibility), parameter, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithNullValue_ReturnsCollapsed()
⋮----
var result = _sut.Convert(null, typeof(Visibility), "First", "en-US");
result.Should().Be(Visibility.Collapsed);
⋮----
public void Convert_WithNullParameter_ReturnsCollapsed()
⋮----
var result = _sut.Convert(TestEnum.First, typeof(Visibility), null, "en-US");
⋮----
public void Convert_WithBothNull_ReturnsCollapsed()
⋮----
var result = _sut.Convert(null, typeof(Visibility), null, "en-US");
⋮----
public void Convert_WithStringValue_ComparesCorrectly(string input, string parameter, Visibility expected)
⋮----
public void Convert_WithNumericValue_ComparesAsString(int input, string parameter, Visibility expected)
⋮----
public void ConvertBack_ThrowsNotImplementedException()
⋮----
Action act = () => _sut.ConvertBack(Visibility.Visible, typeof(TestEnum), "First", "en-US");
act.Should().Throw<NotImplementedException>();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_IconCodeToGlyph_Tests.cs

```csharp
public class Converter_IconCodeToGlyph_Tests
⋮----
private readonly Converter_IconCodeToGlyph _sut;
⋮----
public void Convert_WithHtmlEntityFormat_ReturnsGlyphCharacter(string input, string expected)
⋮----
var result = _sut.Convert(input, typeof(string), null, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithRawHexFormat_ReturnsGlyphCharacter(string input, string expected)
⋮----
public void Convert_WithNullOrWhitespace_ReturnsDefaultIcon(string? input)
⋮----
// Act
⋮----
result.Should().Be("\uE7B8");
⋮----
public void Convert_WithInvalidHexValue_ReturnsInputAsIs(string input)
⋮----
result.Should().Be(input);
⋮----
public void Convert_WithNonFourCharacterHex_ReturnsInputAsIs(string input)
⋮----
public void Convert_WithSingleCharacterString_ReturnsInputAsIs()
⋮----
public void Convert_WithNonStringValue_ReturnsDefaultIcon(object input)
⋮----
public void ConvertBack_ThrowsNotImplementedException()
⋮----
Action act = () => _sut.ConvertBack("\uE7B8", typeof(string), null, "en-US");
act.Should().Throw<NotImplementedException>();
⋮----
public void Convert_WithLowercaseHex_HandlesCorrectly(string input)
⋮----
var lowercaseInput = input.ToLower();
var result = _sut.Convert(lowercaseInput, typeof(string), null, "en-US");
result.Should().NotBeNull();
result.Should().BeOfType<string>();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_IntToVisibility_Tests.cs

```csharp
public class Converter_IntToVisibility_Tests
⋮----
private readonly Converter_IntToVisibility _sut;
⋮----
public void Convert_WithNoParameter_ShowsWhenValueGreaterThanZero(int input, Visibility expected)
⋮----
var result = _sut.Convert(input, typeof(Visibility), null, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithInverseParameter_ShowsWhenValueIsZero(int input, Visibility expected)
⋮----
var result = _sut.Convert(input, typeof(Visibility), "Inverse", "en-US");
⋮----
public void Convert_WithInverseParameterCaseInsensitive_WorksCorrectly(int input, Visibility expected)
⋮----
var result = _sut.Convert(input, typeof(Visibility), "inverse", "en-US");
⋮----
public void Convert_WithNumericStringParameter_ShowsWhenValueMatchesParameter(int input, string parameter, Visibility expected)
⋮----
var result = _sut.Convert(input, typeof(Visibility), parameter, "en-US");
⋮----
public void Convert_WithDirectIntParameter_ShowsWhenValueMatchesParameter(int input, int parameter, Visibility expected)
⋮----
public void Convert_WithNullValue_ReturnsCollapsed()
⋮----
var result = _sut.Convert(null, typeof(Visibility), null, "en-US");
result.Should().Be(Visibility.Collapsed);
⋮----
public void Convert_WithNonNumericStringParameter_FallsBackToGreaterThanZero(string parameter)
⋮----
var result = _sut.Convert(5, typeof(Visibility), parameter, "en-US");
result.Should().Be(Visibility.Visible);
⋮----
public void Convert_WithNonIntegerValue_TreatsAsZero(object input)
⋮----
public void ConvertBack_ThrowsNotImplementedException()
⋮----
Action act = () => _sut.ConvertBack(Visibility.Visible, typeof(int), null, "en-US");
act.Should().Throw<NotImplementedException>();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_LoadNumberToOneBased_Tests.cs

```csharp
public class Converter_LoadNumberToOneBased_Tests
⋮----
private readonly Converter_LoadNumberToOneBased _sut;
⋮----
public void Convert_WithIntegerValue_ReturnsOneBasedValue(int input, int expected)
⋮----
var result = _sut.Convert(input, typeof(int), null, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithNonIntegerValue_ReturnsValueAsIs(object? input)
⋮----
result.Should().Be(input);
⋮----
public void ConvertBack_WithValidStringValue_ReturnsZeroBasedValue(string input, int expected)
⋮----
var result = _sut.ConvertBack(input, typeof(int), null, "en-US");
⋮----
public void ConvertBack_WithInvalidString_ReturnsValueAsIs(string input)
⋮----
public void ConvertBack_WithNonStringValue_ReturnsValueAsIs(object? input)
⋮----
public void Convert_RoundTrip_PreservesValue(int originalValue)
⋮----
var oneBasedResult = _sut.Convert(originalValue, typeof(int), null, "en-US");
var zeroBasedResult = _sut.ConvertBack(oneBasedResult.ToString(), typeof(int), null, "en-US");
zeroBasedResult.Should().Be(originalValue);
⋮----
public void Convert_WithNegativeValue_ReturnsNegativeOneBased()
⋮----
var result = _sut.Convert(-5, typeof(int), null, "en-US");
result.Should().Be(-4);
⋮----
public void ConvertBack_WithZeroString_ReturnsNegativeOne()
⋮----
var result = _sut.ConvertBack("0", typeof(int), null, "en-US");
result.Should().Be(-1);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_StringFormat_Tests.cs

```csharp
public class Converter_StringFormat_Tests
⋮----
private readonly Converter_StringFormat _sut;
⋮----
public void Convert_WithValidFormatParameter_ReturnsFormattedString(object input, string format, string expected)
⋮----
var result = _sut.Convert(input, typeof(string), format, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithNullParameter_ReturnsValueAsIs(object input)
⋮----
var result = _sut.Convert(input, typeof(string), null, "en-US");
result.Should().Be(input);
⋮----
public void Convert_WithNonStringParameter_ReturnsValueAsIs(object input)
⋮----
var result = _sut.Convert(input, typeof(string), 123, "en-US");
⋮----
public void Convert_WithNullValue_FormatsNullAsString()
⋮----
var result = _sut.Convert(null, typeof(string), "Value: {0}", "en-US");
result.Should().Be("Value: ");
⋮----
public void Convert_WithNumericFormatting_ReturnsFormattedNumber(object input, string format, string expected)
⋮----
public void Convert_WithEmptyFormatString_ReturnsEmptyString()
⋮----
var result = _sut.Convert("Hello", typeof(string), "", "en-US");
result.Should().Be("");
⋮----
public void ConvertBack_ThrowsNotImplementedException()
⋮----
// Act
Action act = () => _sut.ConvertBack("Formatted String", typeof(object), "{0}", "en-US");
act.Should().Throw<NotImplementedException>();
⋮----
public void Convert_WithComplexFormatString_HandlesCorrectly(object input, string format, string expected)
```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/NullableDoubleToDoubleConverter_Tests.cs

```csharp
public class NullableDoubleToDoubleConverter_Tests
⋮----
private readonly NullableDoubleToDoubleConverter _sut;
⋮----
public void Convert_WithDoubleValue_ReturnsDoubleValue(double input, double expected)
⋮----
var result = _sut.Convert(input, typeof(double), null, "en-US");
result.Should().Be(expected);
⋮----
public void Convert_WithNonDoubleValue_ReturnsZero(object? input)
⋮----
result.Should().Be(0.0);
⋮----
public void ConvertBack_WithDoubleValue_ReturnsNullableDouble(double input)
⋮----
var result = _sut.ConvertBack(input, typeof(double?), null, "en-US");
result.Should().Be((double?)input);
⋮----
public void ConvertBack_WithNonDoubleValue_ReturnsNull(object? input)
⋮----
result.Should().BeNull();
⋮----
public void Convert_RoundTrip_PreservesValue(double originalValue)
⋮----
var doubleResult = _sut.Convert(originalValue, typeof(double), null, "en-US");
var nullableResult = _sut.ConvertBack(doubleResult, typeof(double?), null, "en-US");
nullableResult.Should().Be(originalValue);
⋮----
public void Convert_WithNaN_ReturnsNaN()
⋮----
var result = _sut.Convert(double.NaN, typeof(double), null, "en-US");
result.Should().Be(double.NaN);
⋮----
public void Convert_WithPositiveInfinity_ReturnsPositiveInfinity()
⋮----
var result = _sut.Convert(double.PositiveInfinity, typeof(double), null, "en-US");
result.Should().Be(double.PositiveInfinity);
⋮----
public void Convert_WithNegativeInfinity_ReturnsNegativeInfinity()
⋮----
var result = _sut.Convert(double.NegativeInfinity, typeof(double), null, "en-US");
result.Should().Be(double.NegativeInfinity);
⋮----
public void ConvertBack_WithNaN_ReturnsNullableNaN()
⋮----
var result = _sut.ConvertBack(double.NaN, typeof(double?), null, "en-US");
result.Should().Be((double?)double.NaN);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Defaults/InforVisualDefaults_Tests.cs

```csharp
public class InforVisualDefaults_Tests
⋮----
public void DefaultSiteId_ShouldNotBeNullOrWhiteSpace()
⋮----
InforVisualDefaults.DefaultSiteId.Should().NotBeNullOrWhiteSpace();
⋮----
public void DefaultUom_ShouldNotBeNullOrWhiteSpace()
⋮----
InforVisualDefaults.DefaultUom.Should().NotBeNullOrWhiteSpace();
⋮----
public void DefaultPartStatus_ShouldNotBeNullOrWhiteSpace()
⋮----
InforVisualDefaults.DefaultPartStatus.Should().NotBeNullOrWhiteSpace();
⋮----
public void Model_InforVisualConnection_DefaultsShouldMatch()
⋮----
var model = new Model_InforVisualConnection();
model.Server.Should().Be(InforVisualDefaults.DefaultServer);
model.Database.Should().Be(InforVisualDefaults.DefaultDatabase);
model.SiteId.Should().Be(InforVisualDefaults.DefaultSiteId);
⋮----
public void Model_InforVisualPart_DefaultsShouldMatch()
⋮----
var model = new Model_InforVisualPart();
model.PrimaryUom.Should().Be(InforVisualDefaults.DefaultUom);
model.DefaultSite.Should().Be(InforVisualDefaults.DefaultSiteId);
model.PartStatus.Should().Be(InforVisualDefaults.DefaultPartStatus);
⋮----
public void Model_InforVisualPO_DefaultsShouldMatch()
⋮----
var model = new Model_InforVisualPO();
model.UnitOfMeasure.Should().Be(InforVisualDefaults.DefaultUom);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Defaults/WorkstationDefaults_Tests.cs

```csharp
public class WorkstationDefaults_Tests
⋮----
public void SharedTerminalWorkstationType_ShouldNotBeNullOrWhiteSpace()
⋮----
WorkstationDefaults.SharedTerminalWorkstationType.Should().NotBeNullOrWhiteSpace();
⋮----
public void PersonalWorkstationWorkstationType_ShouldNotBeNullOrWhiteSpace()
⋮----
WorkstationDefaults.PersonalWorkstationWorkstationType.Should().NotBeNullOrWhiteSpace();
⋮----
public void SharedTerminalTimeoutMinutes_ShouldBePositive()
⋮----
WorkstationDefaults.SharedTerminalTimeoutMinutes.Should().BeGreaterThan(0);
⋮----
public void PersonalWorkstationTimeoutMinutes_ShouldBePositive()
⋮----
WorkstationDefaults.PersonalWorkstationTimeoutMinutes.Should().BeGreaterThan(0);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Helpers/Database/Helper_Database_StoredProcedure_Tests.cs

```csharp
public class Helper_Database_StoredProcedure_Tests
⋮----
public void ValidateParameters_NullParameters_ReturnsTrue()
⋮----
var result = Helper_Database_StoredProcedure.ValidateParameters(null);
result.Should().BeTrue("null parameters list is considered valid (no parameters to validate)");
⋮----
public void ValidateParameters_ValidInputParameters_ReturnsTrue()
⋮----
new MySqlParameter("@p1", "value"),
new MySqlParameter("@p2", 123)
⋮----
var result = Helper_Database_StoredProcedure.ValidateParameters(parameters);
result.Should().BeTrue();
⋮----
public void ValidateParameters_NullInputParameterContent_ReturnsFalse()
⋮----
var parameter = new MySqlParameter("@p1", MySqlDbType.VarChar);
⋮----
result.Should().BeFalse("input parameters should not be null (use DBNull.Value instead)");
⋮----
public void ValidateParameters_DBNullInputParameter_ReturnsTrue()
⋮----
new MySqlParameter("@p1", System.DBNull.Value)
⋮----
result.Should().BeTrue("DBNull.Value is a valid value for parameters");
```

## File: MTM_Receiving_Application.Tests/Module_Core/Helpers/Database/Helper_Database_Variables_Tests.cs

```csharp
public class Helper_Database_Variables_Tests
⋮----
public void GetConnectionString_Production_ReturnsProductionString()
⋮----
var connectionString = Helper_Database_Variables.GetConnectionString(true);
connectionString.Should().Contain("Database=mtm_receiving_application");
connectionString.Should().NotContain("_test");
⋮----
public void GetConnectionString_Test_ReturnsTestString()
⋮----
var connectionString = Helper_Database_Variables.GetConnectionString(false);
connectionString.Should().Contain("Database=mtm_receiving_application_test");
⋮----
public void GetInforVisualConnectionString_ReturnsReadOnlyIntent()
⋮----
var connectionString = Helper_Database_Variables.GetInforVisualConnectionString();
connectionString.Should().Contain("ApplicationIntent=ReadOnly");
```

## File: MTM_Receiving_Application.Tests/Module_Core/Models/Systems/Model_AuthenticationResult_Tests.cs

```csharp
public class Model_AuthenticationResult_Tests
⋮----
public void SuccessResult_SetsSuccessTrue()
⋮----
var user = new Model_User();
var result = Model_AuthenticationResult.SuccessResult(user);
result.Success.Should().BeTrue();
⋮----
public void SuccessResult_SetsUser()
⋮----
result.User.Should().BeSameAs(user);
⋮----
public void SuccessResult_SetsErrorMessageEmpty()
⋮----
result.ErrorMessage.Should().BeEmpty();
⋮----
public void SuccessResult_WithNullUser_Throws()
⋮----
var act = static () => Model_AuthenticationResult.SuccessResult(null!);
act.Should().Throw<ArgumentNullException>();
⋮----
public void ErrorResult_SetsSuccessFalse()
⋮----
var result = Model_AuthenticationResult.ErrorResult("bad");
result.Success.Should().BeFalse();
⋮----
public void ErrorResult_SetsUserNull()
⋮----
result.User.Should().BeNull();
⋮----
public void ErrorResult_SetsErrorMessage()
⋮----
result.ErrorMessage.Should().Be("bad");
⋮----
public void ErrorResult_WithNullMessage_Throws()
⋮----
var act = static () => Model_AuthenticationResult.ErrorResult(null!);
act.Should().Throw<ArgumentException>();
⋮----
public void ErrorResult_WithWhitespaceMessage_Throws()
⋮----
var act = static () => Model_AuthenticationResult.ErrorResult("   ");
```

## File: MTM_Receiving_Application.Tests/Module_Core/Models/Systems/Model_User_Tests.cs

```csharp
public class Model_User_Tests
⋮----
public void Constructor_Default_SetsCreatedAndModifiedToNow()
⋮----
var user = new Model_User();
⋮----
user.CreatedDate.Should().BeOnOrAfter(before);
user.CreatedDate.Should().BeOnOrBefore(after);
user.ModifiedDate.Should().BeOnOrAfter(before);
user.ModifiedDate.Should().BeOnOrBefore(after);
⋮----
public void HasErpAccess_ReturnsFalseWhenVisualUsernameMissing()
⋮----
var user = new Model_User { VisualUsername = null };
user.HasErpAccess.Should().BeFalse();
⋮----
public void HasErpAccess_ReturnsFalseWhenVisualUsernameWhitespace()
⋮----
var user = new Model_User { VisualUsername = "   " };
⋮----
public void HasErpAccess_ReturnsTrueWhenVisualUsernameProvided()
⋮----
var user = new Model_User { VisualUsername = "user" };
user.HasErpAccess.Should().BeTrue();
⋮----
public void DisplayName_FormatsEmployeeNumberAndName()
⋮----
var user = new Model_User
⋮----
user.DisplayName.Should().Be("Jane Doe (Emp #123)");
```

## File: MTM_Receiving_Application.Tests/Module_Core/Models/Systems/Model_ValidationResult_Tests.cs

```csharp
public class Model_ValidationResult_Tests
⋮----
public void Valid_SetsIsValidTrue()
⋮----
var result = Model_ValidationResult.Valid();
result.IsValid.Should().BeTrue();
⋮----
public void Valid_SetsErrorMessageEmpty()
⋮----
result.ErrorMessage.Should().BeEmpty();
⋮----
public void Invalid_SetsIsValidFalse()
⋮----
var result = Model_ValidationResult.Invalid("bad");
result.IsValid.Should().BeFalse();
⋮----
public void Invalid_SetsErrorMessage()
⋮----
result.ErrorMessage.Should().Be("bad");
⋮----
public void Invalid_WithNullMessage_NormalizesMessage()
⋮----
var result = Model_ValidationResult.Invalid(null);
result.ErrorMessage.Should().Be("Validation failed.");
⋮----
public void Invalid_WithWhitespaceMessage_NormalizesMessage()
⋮----
var result = Model_ValidationResult.Invalid("   ");
```

## File: MTM_Receiving_Application.Tests/Module_Core/Services/Authentication/Service_Authentication_Tests.cs

```csharp
public class Service_Authentication_Tests
⋮----
private readonly Service_Authentication _sut;
⋮----
_sut = new Service_Authentication(_mockDaoUser.Object, _mockErrorHandler.Object);
⋮----
public async Task AuthenticateByWindowsUsernameAsync_EmptyUsername_ReturnsError_Async()
⋮----
var result = await _sut.AuthenticateByWindowsUsernameAsync("");
// Assert
result.Success.Should().BeFalse();
result.ErrorMessage.Should().Contain("required");
⋮----
public async Task AuthenticateByWindowsUsernameAsync_UserNotFound_ReturnsError_Async()
⋮----
_mockDaoUser.Setup(x => x.GetUserByWindowsUsernameAsync(It.IsAny<string>()))
.ReturnsAsync(Model_Dao_Result_Factory.Failure<Model_User>("User not found"));
var result = await _sut.AuthenticateByWindowsUsernameAsync("domain\\user");
⋮----
result.ErrorMessage.Should().Contain("not found");
⋮----
public async Task AuthenticateByWindowsUsernameAsync_ValidUser_ReturnsSuccess_Async()
⋮----
var user = new Model_User { FullName = "Test User", WindowsUsername = "domain\\user" };
_mockDaoUser.Setup(x => x.GetUserByWindowsUsernameAsync("domain\\user"))
.ReturnsAsync(Model_Dao_Result_Factory.Success(user));
_mockDaoUser.Setup(x => x.LogUserActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
.ReturnsAsync(Model_Dao_Result_Factory.Success(true));
⋮----
result.Success.Should().BeTrue();
result.User.Should().BeEquivalentTo(user);
_mockDaoUser.Verify(x => x.LogUserActivityAsync("login_success", "domain\\user", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
⋮----
public async Task AuthenticateByPinAsync_InvalidInput_ReturnsError_Async(string username, string pin)
⋮----
var result = await _sut.AuthenticateByPinAsync(username, pin);
⋮----
public async Task AuthenticateByPinAsync_ValidCredentials_ReturnsSuccess_Async()
⋮----
_mockDaoUser.Setup(x => x.ValidateUserPinAsync("user", "1234"))
⋮----
var result = await _sut.AuthenticateByPinAsync("user", "1234");
⋮----
result.User.Should().Be(user);
⋮----
public async Task CreateNewUserAsync_InvalidData_ReturnsError_Async()
⋮----
var user = new Model_User { WindowsUsername = "" }; // Missing fields
// Act
var result = await _sut.CreateNewUserAsync(user, "admin");
⋮----
public async Task CreateNewUserAsync_ValidData_ReturnsSuccess_Async()
⋮----
var user = new Model_User
⋮----
_mockDaoUser.Setup(x => x.CreateNewUserAsync(user, "admin"))
.ReturnsAsync(Model_Dao_Result_Factory.Success<int>(101));
⋮----
result.EmployeeNumber.Should().Be(101);
⋮----
public async Task ValidatePinAsync_ValidatesCorrectly_Async(string pin, bool expectedValid)
⋮----
var result = await _sut.ValidatePinAsync(pin);
⋮----
result.IsValid.Should().Be(expectedValid);
⋮----
public async Task DetectWorkstationTypeAsync_SharedTerminal_ReturnsType_Async()
⋮----
// Arrange
⋮----
_mockDaoUser.Setup(x => x.GetSharedTerminalNamesAsync())
.ReturnsAsync(Model_Dao_Result_Factory.Success(new List<string> { "SHARED-PC" }));
var result = await _sut.DetectWorkstationTypeAsync(computerName);
result.WorkstationType.Should().Be("shared_terminal");
result.ComputerName.Should().Be(computerName);
⋮----
public async Task DetectWorkstationTypeAsync_PersonalWorkstation_ReturnsType_Async()
⋮----
result.WorkstationType.Should().Be("personal_workstation");
```

## File: MTM_Receiving_Application.Tests/Module_Core/Services/Authentication/Service_UserSessionManager_Tests.cs

```csharp
public class Service_UserSessionManager_Tests
⋮----
private readonly Service_UserSessionManager _sut;
⋮----
_mockDispatcher.Setup(x => x.CreateTimer()).Returns(_mockTimer.Object);
_sut = new Service_UserSessionManager(_mockDaoUser.Object, _mockDispatcher.Object);
⋮----
public void CreateSession_NullUser_ThrowsArgumentNullException()
⋮----
Action act = () => _sut.CreateSession(null!, new Model_WorkstationConfig("PC"), "Windows");
act.Should().Throw<ArgumentNullException>().WithParameterName("user");
⋮----
public void CreateSession_ValidData_SetsCurrentSession()
⋮----
var user = new Model_User { WindowsUsername = "test" };
var config = new Model_WorkstationConfig("PC") { WorkstationType = "personal_workstation" };
var session = _sut.CreateSession(user, config, "Windows");
session.Should().NotBeNull();
session.User.Should().Be(user);
session.WorkstationName.Should().Be("PC");
session.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(30));
_sut.CurrentSession.Should().Be(session);
⋮----
public void StartTimeoutMonitoring_NoSession_ThrowsInvalidOperationException()
⋮----
Action act = () => _sut.StartTimeoutMonitoring();
act.Should().Throw<InvalidOperationException>();
⋮----
public void StartTimeoutMonitoring_WithSession_StartsTimer()
⋮----
_sut.CreateSession(new Model_User(), new Model_WorkstationConfig("PC"), "Windows");
_sut.StartTimeoutMonitoring();
_mockDispatcher.Verify(x => x.CreateTimer(), Times.Once);
_mockTimer.Verify(x => x.Start(), Times.Once);
_mockTimer.VerifySet(x => x.Interval = It.IsAny<TimeSpan>());
_mockTimer.VerifySet(x => x.IsRepeating = true);
⋮----
public void StopTimeoutMonitoring_StopsTimer()
⋮----
_sut.StopTimeoutMonitoring();
_mockTimer.Verify(x => x.Stop(), Times.Once);
⋮----
public async Task EndSessionAsync_LogsAndClearsSession_Async()
⋮----
_sut.CreateSession(user, new Model_WorkstationConfig("PC"), "Windows");
⋮----
_mockDaoUser.Setup(x => x.LogUserActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
.ReturnsAsync(Model_Dao_Result_Factory.Success(true));
await _sut.EndSessionAsync("Logout");
_sut.CurrentSession.Should().BeNull();
⋮----
_mockDaoUser.Verify(x => x.LogUserActivityAsync("Logout", "test", "PC", It.IsNotNull<string>()), Times.Once);
⋮----
public void UpdateLastActivity_UpdatesTimestamp()
⋮----
_sut.UpdateLastActivity();
_sut.CurrentSession.LastActivityTimestamp.Should().BeOnOrAfter(initialTime);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Services/Database/Service_ErrorHandler_Tests.cs

```csharp
public class Service_ErrorHandler_Tests
⋮----
private readonly Service_ErrorHandler _sut;
⋮----
_sut = new Service_ErrorHandler(_mockLogger.Object, _mockWindowService.Object);
⋮----
public async Task LogErrorAsync_InfoSeverity_CallsLogInfo_Async()
⋮----
await _sut.LogErrorAsync("Test Info", Enum_ErrorSeverity.Info);
_mockLogger.Verify(x => x.LogInfo("Test Info", It.IsAny<string?>()), Times.Once);
⋮----
public async Task LogErrorAsync_ErrorSeverity_CallsLogError_Async()
⋮----
var ex = new Exception("Test Ex");
await _sut.LogErrorAsync("Test Error", Enum_ErrorSeverity.Error, ex);
_mockLogger.Verify(x => x.LogError("Test Error", ex, It.IsAny<string?>()), Times.Once);
⋮----
public async Task HandleErrorAsync_ShowDialogFalse_OnlyLogs_Async()
⋮----
await _sut.HandleErrorAsync("Test", Enum_ErrorSeverity.Warning, null, false);
_mockLogger.Verify(x => x.LogWarning("Test", It.IsAny<string?>()), Times.Once);
_mockWindowService.Verify(x => x.GetXamlRoot(), Times.Never);
⋮----
public async Task HandleErrorAsync_ShowDialogTrue_LogsAndChecksWindow_Async()
⋮----
_mockWindowService.Setup(x => x.GetXamlRoot()).Returns((XamlRoot?)null);
await _sut.HandleErrorAsync("Test", Enum_ErrorSeverity.Error, null, true);
_mockLogger.Verify(x => x.LogError("Test", null, It.IsAny<string?>()), Times.Once);
_mockWindowService.Verify(x => x.GetXamlRoot(), Times.Once);
_mockLogger.Verify(x => x.LogWarning("Cannot show dialog - XamlRoot is null", It.IsAny<string?>()), Times.Once);
⋮----
public async Task HandleDaoErrorAsync_SuccessResult_DoesNothing_Async()
⋮----
var result = Model_Dao_Result_Factory.Success();
await _sut.HandleDaoErrorAsync(result, "TestOp");
_mockLogger.Verify(x => x.LogInfo(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
_mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception?>(), It.IsAny<string?>()), Times.Never);
⋮----
public async Task HandleDaoErrorAsync_FailureResult_LogsError_Async()
⋮----
var result = Model_Dao_Result_Factory.Failure("DB Fail", new Exception("SQL Error"));
⋮----
await _sut.HandleDaoErrorAsync(result, "TestOp", true);
_mockLogger.Verify(x => x.LogError(It.Is<string>(s => s.Contains("DB Fail") && s.Contains("TestOp")), It.IsAny<Exception>(), It.IsAny<string?>()), Times.Once);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Services/Help/Service_Help_Tests.cs

```csharp
public class Service_Help_Tests
⋮----
private readonly Service_Help _sut;
⋮----
_sut = new Service_Help(_mockWindow.Object, _mockLogger.Object, _mockDispatcher.Object);
⋮----
public void GetHelpContent_ExistingKey_ReturnsContent()
⋮----
var content = _sut.GetHelpContent("Dunnage.ModeSelection");
content.Should().NotBeNull();
content?.Title.Should().Be("Select Entry Mode");
content?.Key.Should().Be("Dunnage.ModeSelection");
⋮----
public void GetHelpContent_NonExistentKey_ReturnsNull()
⋮----
var content = _sut.GetHelpContent("NonExistentKey");
content.Should().BeNull();
⋮----
public void GetHelpByCategory_ReturnsFilteredList()
⋮----
var list = _sut.GetHelpByCategory("Dunnage Workflow");
list.Should().NotBeEmpty();
list.All(x => x.Category == "Dunnage Workflow").Should().BeTrue();
⋮----
public void SearchHelp_ReturnsMatchingItems()
⋮----
var results = _sut.SearchHelp("dunnage");
results.Should().NotBeEmpty();
results.Should().Contain(x => x.Title.ToLower().Contains("dunnage") || x.Content.ToLower().Contains("dunnage"));
⋮----
public void SearchHelp_EmptyTerm_ReturnsEmptyList()
⋮----
var results = _sut.SearchHelp("");
// Assert
results.Should().BeEmpty();
⋮----
public void TipRetreival_ReturnsContent()
⋮----
// Act
var tip = _sut.GetTip("Dunnage.QuantityEntry");
tip.Should().NotBeNullOrEmpty();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Services/UI/Service_ViewModelRegistry_Tests.cs

```csharp
public class Service_ViewModelRegistry_Tests
⋮----
private class TestViewModel : IResettableViewModel
⋮----
public void ResetToDefaults() => ResetCalled = true;
⋮----
private class AnotherViewModel { }
⋮----
public void Register_and_GetViewModels_WorksCorrectly()
⋮----
var sut = new Service_ViewModelRegistry();
var vm1 = new TestViewModel();
var vm2 = new AnotherViewModel();
sut.Register(vm1);
sut.Register(vm2);
var retrieved = sut.GetViewModels<TestViewModel>().ToList();
retrieved.Should().HaveCount(1);
retrieved.First().Should().Be(vm1);
⋮----
public void ClearAllInputs_CallsResetOnResettableVMs()
⋮----
sut.ClearAllInputs();
vm1.ResetCalled.Should().BeTrue();
⋮----
public void Cleanup_RemovesDeadReferences()
⋮----
var vm = new TestViewModel();
sut.Register(vm);
⋮----
GC.Collect();
GC.WaitForPendingFinalizers();
var list = sut.GetViewModels<TestViewModel>().ToList();
list.Should().BeEmpty();
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_Application_Variables_Tests.cs

```csharp
public class Model_Application_Variables_Tests
⋮----
public void Constructor_Defaults_SetsSensibleValues()
⋮----
var config = new Model_Application_Variables();
config.ApplicationName.Should().NotBeEmpty();
config.Version.Should().MatchRegex(@"\d+\.\d+\.\d+");
config.ConnectionString.Should().BeEmpty();
config.EnvironmentType.Should().Be("Development");
⋮----
public void LogDirectory_Default_IsInAppData()
⋮----
var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
config.LogDirectory.Should().StartWith(appData);
config.LogDirectory.Should().Contain("MTM_Receiving_Application");
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_CSVDeleteResult_Tests.cs

```csharp
public class Model_CSVDeleteResult_Tests
⋮----
public void Properties_SetAndGet_WorksCorrectly()
⋮----
var result = new Model_CSVDeleteResult();
⋮----
result.LocalDeleted.Should().BeTrue();
result.NetworkDeleted.Should().BeFalse();
result.LocalError.Should().BeNull();
result.NetworkError.Should().Be("Access Denied");
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_CSVExistenceResult_Tests.cs

```csharp
public class Model_CSVExistenceResult_Tests
⋮----
public void Properties_SetAndGet_Works()
⋮----
var result = new Model_CSVExistenceResult();
⋮----
result.LocalExists.Should().BeTrue();
result.NetworkExists.Should().BeTrue();
result.NetworkAccessible.Should().BeTrue();
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_CSVWriteResult_Tests.cs

```csharp
public class Model_CSVWriteResult_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var result = new Model_CSVWriteResult();
result.LocalSuccess.Should().BeFalse();
result.NetworkSuccess.Should().BeFalse();
result.ErrorMessage.Should().BeEmpty();
⋮----
public void IsFullSuccess_ReturnsTrueOnlyIfBothSucceed()
⋮----
var result = new Model_CSVWriteResult
⋮----
result.IsFullSuccess.Should().BeTrue();
result.IsPartialSuccess.Should().BeFalse();
result.IsFailure.Should().BeFalse();
⋮----
public void IsPartialSuccess_ReturnsTrueOnlyIfLocalSucceedsButNetworkFails()
⋮----
result.IsFullSuccess.Should().BeFalse();
result.IsPartialSuccess.Should().BeTrue();
⋮----
public void IsFailure_ReturnsTrueIfLocalFails()
⋮----
result.IsFailure.Should().BeTrue();
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_InforVisualPart_Tests.cs

```csharp
public class Model_InforVisualPart_Tests
⋮----
public void DisplayText_FormatsCorrectly()
⋮----
var part = new Model_InforVisualPart
⋮----
text.Should().Be("PART-01 - A Widget (Line 001)");
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var part = new Model_InforVisualPart();
part.UnitOfMeasure.Should().Be("EA");
part.QtyOrdered.Should().Be(0);
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_InforVisualPO_Tests.cs

```csharp
public class Model_InforVisualPO_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var po = new Model_InforVisualPO();
po.Parts.Should().NotBeNull().And.BeEmpty();
po.HasParts.Should().BeFalse();
po.PONumber.Should().BeEmpty();
⋮----
public void HasParts_ReturnsTrueOnlyWhenPartsExist()
⋮----
po.Parts.Add(new Model_InforVisualPart());
po.HasParts.Should().BeTrue();
⋮----
public void StatusDescription_MapsCodesCorrectly(string code, string expected)
⋮----
var po = new Model_InforVisualPO { Status = code };
⋮----
desc.Should().Be(expected);
⋮----
public void IsClosed_ReturnsTrueForClosedOrCancelled(string status, bool expected)
⋮----
var po = new Model_InforVisualPO { Status = status };
⋮----
isClosed.Should().Be(expected);
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_PackageTypePreference_Tests.cs

```csharp
public class Model_PackageTypePreference_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var model = new Model_PackageTypePreference();
model.PartID.Should().BeEmpty();
model.PackageTypeName.Should().BeEmpty();
model.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_ReceivingLine_Tests.cs

```csharp
public class Model_ReceivingLine_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var line = new Model_ReceivingLine();
line.Date.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
line.LabelNumber.Should().Be(1);
line.TotalLabels.Should().Be(1);
line.VendorName.Should().Be("Unknown");
line.PartID.Should().BeEmpty();
⋮----
public void LabelText_ReturnsFormattedString()
⋮----
var line = new Model_ReceivingLine
⋮----
text.Should().Be("2 / 5");
⋮----
public void LabelText_UpdatesWhenPropertiesChange()
⋮----
line.LabelText.Should().Be("3 / 10");
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_ReceivingLoad_Tests.cs

```csharp
public class Model_ReceivingLoad_Tests
⋮----
private Model_ReceivingLoad _sut;
⋮----
_sut = new Model_ReceivingLoad();
⋮----
public void Constructor_Initialization_SetsDefaults()
⋮----
var model = new Model_ReceivingLoad();
model.LoadID.Should().NotBeEmpty();
model.PartID.Should().BeEmpty();
model.HeatLotNumber.Should().BeEmpty();
model.ReceivedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
model.PackageType.Should().Be(Enum_PackageType.Skid);
model.PackageTypeName.Should().Be("Skid");
model.PackagesPerLoad.Should().Be(0);
⋮----
public void PartID_WhenChanged_SetsPackagesPerLoadToOneIfZero()
⋮----
_sut.PackagesPerLoad.Should().Be(1);
⋮----
public void PartID_WhenChangedToMMC_SetsPackageTypeToCoil()
⋮----
_sut.PackageType.Should().Be(Enum_PackageType.Coil);
⋮----
public void PartID_WhenChangedToMMF_SetsPackageTypeToSheet()
⋮----
_sut.PackageType.Should().Be(Enum_PackageType.Sheet);
⋮----
public void PartID_WhenSetToEmptyOrNull_DoesNotChangePackageType(string partId)
⋮----
// Arrange
⋮----
// Act
⋮----
// Assert
_sut.PackageType.Should().Be(Enum_PackageType.Skid);
_sut.PackagesPerLoad.Should().Be(0);
⋮----
// ====================================================================
// Property Logic: PackageType / PackageTypeName
⋮----
public void PackageType_WhenChanged_UpdatesPackageTypeName()
⋮----
_sut.PackageTypeName.Should().Be("Box");
⋮----
public void PackageTypeName_WhenChangedToValidEnum_UpdatesPackageType()
⋮----
_sut.PackageType.Should().Be(Enum_PackageType.Box);
⋮----
public void PackageTypeName_WhenChangedToInvalidEnum_DoesNotUpdatePackageType()
⋮----
public void CalculateWeightPerPackage_UpdatesUsingWeightAndPackages(decimal weight, int packages, decimal expected)
⋮----
_sut.WeightPerPackage.Should().Be(expected);
⋮----
public void CalculateWeightPerPackage_WhenPackagesIsZero_SetResultToZero()
⋮----
_sut.WeightPerPackage.Should().Be(0);
⋮----
public void WeightPerPackageDisplay_FormattedCorrectly()
⋮----
display.Should().Be("50 lbs per Box");
⋮----
public void PONumberDisplay_WhenNullOrEmpty_ReturnsNA()
⋮----
display.Should().Be("N/A");
⋮----
public void PONumberDisplay_WhenValid_ReturnsPONumber()
⋮----
display.Should().Be("12345");
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_ReceivingSession_Tests.cs

```csharp
public class Model_ReceivingSession_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var session = new Model_ReceivingSession();
session.SessionID.Should().NotBeEmpty();
session.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
session.Loads.Should().NotBeNull().And.BeEmpty();
session.TotalLoadsCount.Should().Be(0);
session.TotalWeightQuantity.Should().Be(0);
session.HasLoads.Should().BeFalse();
⋮----
public void TotalWeightQuantity_CalculatesSumCorrectly()
⋮----
session.Loads.Add(new Model_ReceivingLoad { WeightQuantity = 100 });
session.Loads.Add(new Model_ReceivingLoad { WeightQuantity = 250 });
⋮----
total.Should().Be(350);
⋮----
public void UniqueParts_ReturnsDistinctList()
⋮----
session.Loads.Add(new Model_ReceivingLoad { PartID = "PART-A" });
⋮----
session.Loads.Add(new Model_ReceivingLoad { PartID = "PART-B" });
⋮----
parts.Should().HaveCount(2);
parts.Should().Contain("PART-A");
parts.Should().Contain("PART-B");
⋮----
public void HasLoads_ReturnsTrueWhenLoadsExist()
⋮----
session.Loads.Add(new Model_ReceivingLoad());
⋮----
hasLoads.Should().BeTrue();
⋮----
public void TransientProperties_HandleNullLoadsForSafety()
⋮----
var session = new Model_ReceivingSession
⋮----
session.UniqueParts.Should().NotBeNull().And.BeEmpty();
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_ReceivingValidationResult_Tests.cs

```csharp
public class Model_ReceivingValidationResult_Tests
⋮----
public void Constructor_Defaults_AreSafe()
⋮----
var result = new Model_ReceivingValidationResult();
result.IsValid.Should().BeFalse();
result.Severity.Should().Be(Enum_ValidationSeverity.Error);
result.Message.Should().BeEmpty();
result.Errors.Should().NotBeNull().And.BeEmpty();
⋮----
public void Success_Factory_ReturnsValidObject()
⋮----
var result = Model_ReceivingValidationResult.Success();
result.IsValid.Should().BeTrue();
⋮----
result.Errors.Should().BeEmpty();
⋮----
public void Error_Factory_ReturnsErrorObject()
⋮----
var result = Model_ReceivingValidationResult.Error(msg);
⋮----
result.Message.Should().Be(msg);
result.Errors.Should().Contain(msg);
⋮----
public void Warning_Factory_ReturnsWarningObject()
⋮----
var result = Model_ReceivingValidationResult.Warning(msg);
result.IsValid.Should().BeTrue("Warnings should not block validity");
result.Severity.Should().Be(Enum_ValidationSeverity.Warning);
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_ReceivingWorkflowStepResult_Tests.cs

```csharp
public class Model_ReceivingWorkflowStepResult_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var result = new Model_ReceivingWorkflowStepResult();
result.Success.Should().BeFalse();
result.Message.Should().BeEmpty();
result.ValidationErrors.Should().NotBeNull().And.BeEmpty();
⋮----
public void SuccessResult_Factory_CreatesSuccessObject()
⋮----
var result = Model_ReceivingWorkflowStepResult.SuccessResult(step, message);
result.Success.Should().BeTrue();
result.NewStep.Should().Be(step);
result.Message.Should().Be(message);
⋮----
public void ErrorResult_Factory_CreatesErrorObject()
⋮----
var result = Model_ReceivingWorkflowStepResult.ErrorResult(errors);
⋮----
result.ValidationErrors.Should().BeEquivalentTo(errors);
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_SaveResult_Tests.cs

```csharp
public class Model_SaveResult_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var result = new Model_SaveResult();
result.Success.Should().BeFalse();
result.LoadsSaved.Should().Be(0);
result.LocalCSVSuccess.Should().BeFalse();
result.NetworkCSVSuccess.Should().BeFalse();
result.DatabaseSuccess.Should().BeFalse();
result.Errors.Should().BeEmpty();
result.Warnings.Should().BeEmpty();
⋮----
public void IsFullSuccess_WhenAllTrue_ReturnsTrue()
⋮----
var result = new Model_SaveResult
⋮----
result.IsFullSuccess.Should().BeTrue();
result.IsPartialSuccess.Should().BeFalse();
⋮----
public void IsFullSuccess_WhenAnyFalse_ReturnsFalse(bool local, bool network, bool db)
⋮----
result.IsFullSuccess.Should().BeFalse();
⋮----
public void IsPartialSuccess_WhenMixedSuccessMatchesCriteria_ReturnsTrue(bool local, bool network, bool db)
⋮----
result.IsPartialSuccess.Should().BeTrue();
⋮----
public void IsPartialSuccess_WhenFullSuccess_ReturnsFalse()
⋮----
public void IsPartialSuccess_WhenNoSuccess_ReturnsFalse()
⋮----
public void LegacyProperties_MapToNewProperties()
⋮----
result.LoadsSaved.Should().Be(10);
result.Success.Should().BeTrue();
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_UserPreference_Tests.cs

```csharp
public class Model_UserPreference_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var prefs = new Model_UserPreference();
prefs.Username.Should().BeEmpty();
prefs.PreferredPackageType.Should().Be("Package");
prefs.Workstation.Should().BeEmpty();
⋮----
public void Properties_NotifyChanges()
⋮----
propertyChanged.Should().BeTrue();
prefs.Username.Should().Be("NewUser");
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Models/Model_WorkflowStepResult_Tests.cs

```csharp
public class Model_WorkflowStepResult_Tests
⋮----
public void Constructor_Defaults_AreCorrect()
⋮----
var model = new Model_WorkflowStepResult();
model.IsSuccess.Should().BeFalse();
model.ErrorMessage.Should().BeEmpty();
model.TargetStep.Should().BeNull();
⋮----
public void Properties_SetAndGet_WorksCorrectly()
⋮----
model.IsSuccess.Should().BeTrue();
model.ErrorMessage.Should().Be("Operation successful");
model.TargetStep.Should().Be(step);
```

## File: Module_Core/Contracts/Services/IService_Authentication.cs

```csharp
public interface IService_Authentication
⋮----
public Task<Model_AuthenticationResult> AuthenticateByWindowsUsernameAsync(
⋮----
public Task<Model_AuthenticationResult> AuthenticateByPinAsync(
⋮----
public Task<Model_CreateUserResult> CreateNewUserAsync(
⋮----
public Task<Model_ValidationResult> ValidatePinAsync(string pin, int? excludeEmployeeNumber = null);
public Task<Model_WorkstationConfig> DetectWorkstationTypeAsync(string? computerName = null);
public Task<List<string>> GetActiveDepartmentsAsync();
public Task LogUserActivityAsync(string eventType, string username, string workstationName, string details);
```

## File: Module_Core/Converters/Converter_EnumToVisibility.cs

```csharp
public class Converter_EnumToVisibility : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
var enumValue = value.ToString();
var targetValue = parameter.ToString();
return string.Equals(enumValue, targetValue, StringComparison.OrdinalIgnoreCase)
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Core/Converters/Converter_IntToVisibility.cs

```csharp
public class Converter_IntToVisibility : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
if (paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
⋮----
else if (int.TryParse(paramStr, out int paramInt))
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
throw new NotImplementedException();
```

## File: Module_Core/Converters/NullableDoubleToDoubleConverter.cs

```csharp
public class NullableDoubleToDoubleConverter : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
```

## File: Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs

```csharp
public static class Helper_Database_StoredProcedure
⋮----
public static async Task<Model_Dao_Result> ExecuteAsync(
⋮----
var result = new Model_Dao_Result();
var stopwatch = Stopwatch.StartNew();
⋮----
await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();
await using var command = new MySqlCommand(procedureName, connection)
⋮----
command.Parameters.AddRange(parameters);
⋮----
var affectedRows = await command.ExecuteNonQueryAsync();
⋮----
stopwatch.Stop();
⋮----
await Task.Delay(RetryDelaysMs[attempt - 1]);
⋮----
return Model_Dao_Result_Factory.Failure($"Stored procedure '{procedureName}' failed: {ex.Message}", ex);
⋮----
return Model_Dao_Result_Factory.Failure($"Stored procedure '{procedureName}' failed after {MaxRetries} attempts");
⋮----
public static bool ValidateParameters(MySqlParameter[]? parameters)
⋮----
public static async Task<Model_Dao_Result> ExecuteNonQueryAsync(
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] ExecuteNonQueryAsync: {procedureName} (Attempt {attempt}/{MaxRetries})");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] Executing: {procedureName}");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB]   {param.ParameterName} = {valueDisplay} ({param.Value?.GetType().Name ?? "DBNull"})");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] Success: {affectedRows} rows affected in {stopwatch.ElapsedMilliseconds}ms");
⋮----
public static async Task<Model_Dao_Result<T>> ExecuteSingleAsync<T>(
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] ExecuteSingleAsync: {procedureName} (Attempt {attempt}/{MaxRetries})");
⋮----
await using var reader = await command.ExecuteReaderAsync();
if (await reader.ReadAsync())
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] Success: 1 record found in {stopwatch.ElapsedMilliseconds}ms");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] No record found");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] ERROR in ExecuteSingleAsync: {procedureName}");
System.Diagnostics.Debug.WriteLine($"[DB] Error Type: {ex.GetType().Name}");
System.Diagnostics.Debug.WriteLine($"[DB] Error Message: {ex.Message}");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] MySQL Error {sqlEx.Number}: {errorDesc}");
System.Diagnostics.Debug.WriteLine($"[DB] SQL State: {sqlEx.SqlState}");
⋮----
public static async Task<Model_Dao_Result<List<T>>> ExecuteListAsync<T>(
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] ExecuteListAsync: {procedureName} (Attempt {attempt}/{MaxRetries})");
⋮----
while (await reader.ReadAsync())
⋮----
list.Add(mapper(reader));
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] Success: {list.Count} records retrieved in {stopwatch.ElapsedMilliseconds}ms");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] ERROR in ExecuteListAsync: {procedureName}");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] MySQL Error Code: {sqlEx.Number}");
⋮----
public static async Task<Model_Dao_Result<DataTable>> ExecuteDataTableAsync(
⋮----
var dataTable = new DataTable();
dataTable.Load(reader);
⋮----
public static async Task<Model_Dao_Result> ExecuteInTransactionAsync(
⋮----
await using var command = new MySqlCommand(procedureName, connection, transaction)
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] Executing SP: {procedureName}");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB]   Parameter: {param.ParameterName} = {valueDisplay} (Type: {param.MySqlDbType})");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] MySQL ERROR {sqlEx.Number}: {errorDesc}");
System.Diagnostics.Debug.WriteLine($"[DB] Error Message: {sqlEx.Message}");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] Procedure: {procedureName}");
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] ERROR: {ex.GetType().Name}: {ex.Message}");
⋮----
private static void AddParameters(MySqlCommand command, Dictionary<string, object>? parameters)
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] Adding {parameters.Count} parameters:");
⋮----
string paramName = param.Key.StartsWith("p_") ? "@" + param.Key : "@p_" + param.Key;
string cleanName = param.Key.TrimStart('@');
string finalName = cleanName.StartsWith("p_") ? "@" + cleanName : "@p_" + cleanName;
⋮----
param.Value.ToString();
System.Diagnostics.Debug.WriteLine($"[DB]   {param.Key} → {finalName} = {valueDisplay} ({param.Value?.GetType().Name ?? "null"})");
command.Parameters.AddWithValue(finalName, param.Value ?? DBNull.Value);
⋮----
System.Diagnostics.Debug.WriteLine($"[DB] No parameters provided");
⋮----
private static string GetMySqlErrorDescription(int errorCode)
⋮----
private static bool IsTransientError(MySqlException ex)
```

## File: Module_Core/Models/Systems/Model_User.cs

```csharp
public class Model_User
⋮----
public bool HasErpAccess => !string.IsNullOrWhiteSpace(VisualUsername);
```

## File: Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs

```csharp
public class Service_OnStartup_AppLifecycle : IService_OnStartup_AppLifecycle
⋮----
private readonly IServiceProvider _serviceProvider;
private readonly IService_Authentication _authService;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_CSVWriter _csvWriter;
⋮----
public async Task StartAsync()
⋮----
_splashScreen.Activate();
⋮----
await Task.Delay(100);
⋮----
await Task.Delay(300);
⋮----
var userCheckResult = await _authService.AuthenticateByWindowsUsernameAsync(windowsUser);
⋮----
// If user doesn't exist, create account first (before workstation detection)
⋮----
// Show New User Setup Dialog as child of splash screen
⋮----
newUserViewModel.CreatedBy = windowsUser; // Self-creation (physical presence is authorization)
var newUserDialog = new View_Shared_NewUserSetupDialog(newUserViewModel);
// Set splash screen as parent
⋮----
// Show dialog and wait for result
var dialogResult = await newUserDialog.ShowAsync();
// Check result
⋮----
// Account created successfully - re-authenticate to get full user data
⋮----
userCheckResult = await _authService.AuthenticateByWindowsUsernameAsync(windowsUser);
⋮----
// Resilience: if the DB lookup doesn't immediately return the user (e.g. mismatch or timing),
// continue startup with the known data instead of shutting down.
authenticatedUser = new Model_User
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
// User cancelled account creation - close app properly
⋮----
// Close splash screen
⋮----
_splashScreen.Close();
⋮----
// Close main window if it exists
⋮----
// Exit application properly
Application.Current.Exit();
⋮----
// User exists - use existing user account
⋮----
// Ensure first-run never proceeds with missing defaults.
// Database schema may be mid-migration; apply safe app-level defaults.
⋮----
// 4. Detect Workstation (50%)
⋮----
var workstationConfig = await _authService.DetectWorkstationTypeAsync();
// Debug logging
System.Diagnostics.Debug.WriteLine($"Workstation: {workstationConfig.ComputerName}");
System.Diagnostics.Debug.WriteLine($"Type: {workstationConfig.WorkstationType}");
System.Diagnostics.Debug.WriteLine($"Is Shared: {workstationConfig.IsSharedTerminal}");
System.Diagnostics.Debug.WriteLine($"Is Personal: {workstationConfig.IsPersonalWorkstation}");
// 5. Determine authentication method based on workstation type (55-80%)
⋮----
// Personal workstation - use Windows auto-login
⋮----
var loginDialog = new View_Shared_SharedTerminalLoginDialog(loginViewModel);
⋮----
var dialogResult = await loginDialog.ShowAsync();
⋮----
Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().UnregisterKey();
System.Environment.Exit(0);
⋮----
_sessionManager.CreateSession(authenticatedUser, workstationConfig, authMethod);
_sessionManager.StartTimeoutMonitoring();
⋮----
mainWin.DispatcherQueue.TryEnqueue(() => mainWin.UpdateUserDisplay());
⋮----
await Task.Delay(500);
⋮----
await _errorHandler.HandleErrorAsync("Startup failed", Models.Enums.Enum_ErrorSeverity.Critical, ex);
⋮----
System.Environment.Exit(1);
⋮----
private void UpdateSplash(double percentage, string message)
⋮----
_splashScreen?.ViewModel.UpdateProgress(percentage, message);
⋮----
private void SetSplashIndeterminate(string message)
⋮----
_splashScreen?.ViewModel.SetIndeterminate(message);
⋮----
private static void ApplySafeUserDefaults(Model_User? user)
⋮----
if (string.IsNullOrWhiteSpace(user.DefaultReceivingMode))
⋮----
if (string.IsNullOrWhiteSpace(user.DefaultDunnageMode))
```

## File: Module_Dunnage/Data/Dao_DunnageCustomField.cs

```csharp
public class Dao_DunnageCustomField
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(int typeId, Model_CustomFieldDefinition field, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_type_id", typeId),
new MySqlParameter("@p_field_name", field.FieldName),
new MySqlParameter("@p_field_type", field.FieldType),
new MySqlParameter("@p_display_order", field.DisplayOrder),
new MySqlParameter("@p_is_required", field.IsRequired),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetByTypeAsync(int typeId)
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(int fieldId)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
private Model_CustomFieldDefinition MapFromReader(IDataReader reader)
⋮----
return new Model_CustomFieldDefinition
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
FieldName = reader.GetString(reader.GetOrdinal("field_name")),
FieldType = reader.GetString(reader.GetOrdinal("field_type")),
DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
IsRequired = reader.GetBoolean(reader.GetOrdinal("is_required"))
```

## File: Module_Dunnage/Data/Dao_DunnageLoad.cs

```csharp
public class Dao_DunnageLoad
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllAsync()
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
⋮----
public virtual async Task<Model_Dao_Result<Model_DunnageLoad>> GetByIdAsync(Guid loadUuid)
⋮----
{ "load_uuid", loadUuid.ToString() }
⋮----
public virtual async Task<Model_Dao_Result> InsertAsync(Guid loadUuid, string partId, decimal quantity, string user)
⋮----
{ "load_uuid", loadUuid.ToString() },
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> InsertBatchAsync(List<Model_DunnageLoad> loads, string user)
⋮----
var uniquePartIds = loads.Select(l => l.PartId).Distinct().ToList();
var daoPart = new Dao_DunnagePart(_connectionString);
⋮----
var partResult = await daoPart.GetByIdAsync(partId);
⋮----
invalidParts.Add(partId);
⋮----
return new Model_Dao_Result
⋮----
ErrorMessage = $"Cannot save loads: The following Part ID(s) have not been registered in the system: {string.Join(", ", invalidParts)}. " +
⋮----
loadData.Add(new
⋮----
load_uuid = load.LoadUuid.ToString(),
⋮----
string json = JsonSerializer.Serialize(loadData);
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
if (result.ErrorMessage.Contains("FK_dunnage_history_part_id") ||
result.ErrorMessage.Contains("foreign key constraint"))
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(Guid loadUuid, decimal quantity, string user)
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(Guid loadUuid)
⋮----
private Model_DunnageLoad MapFromReader(IDataReader reader)
⋮----
return new Model_DunnageLoad
⋮----
LoadUuid = (Guid)reader[reader.GetOrdinal("load_uuid")],
PartId = reader.GetString(reader.GetOrdinal("part_id")),
Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
ReceivedDate = reader.GetDateTime(reader.GetOrdinal("received_date")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Data/Dao_DunnageType.cs

```csharp
public class Dao_DunnageType
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllAsync()
⋮----
Console.WriteLine($"[Dao_DunnageType] GetAllAsync called");
Console.WriteLine($"[Dao_DunnageType] Connection string: {_connectionString}");
⋮----
Console.WriteLine($"[Dao_DunnageType] Result - IsSuccess: {result.IsSuccess}, Data Count: {result.Data?.Count ?? 0}, Error: {result.ErrorMessage}");
⋮----
public virtual async Task<Model_Dao_Result<Model_DunnageType>> GetByIdAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(string typeName, string icon, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_type_name", typeName),
new MySqlParameter("@p_icon", icon),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string typeName, string icon, string user)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result<int>> CountPartsAsync(int typeId)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("part_count")),
⋮----
public virtual async Task<Model_Dao_Result<int>> CountTransactionsAsync(int typeId)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("transaction_count")),
⋮----
public virtual async Task<Model_Dao_Result<bool>> CheckDuplicateNameAsync(string typeName, int? excludeId = null)
⋮----
var pExists = new MySqlParameter("@p_exists", MySqlDbType.Bit)
⋮----
new MySqlParameter("@p_exclude_id", excludeId.HasValue ? (object)excludeId.Value : DBNull.Value),
⋮----
return Model_Dao_Result_Factory.Success<bool>(Convert.ToBoolean(pExists.Value));
⋮----
private Model_DunnageType MapFromReader(IDataReader reader)
⋮----
return new Model_DunnageType
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
TypeName = reader.GetString(reader.GetOrdinal("type_name")),
Icon = reader.IsDBNull(reader.GetOrdinal("icon")) ? "Help" : reader.GetString(reader.GetOrdinal("icon")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Data/Dao_InventoriedDunnage.cs

```csharp
public class Dao_InventoriedDunnage
⋮----
public virtual async Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllAsync()
⋮----
public virtual async Task<Model_Dao_Result<bool>> CheckAsync(string partId)
⋮----
reader => reader.GetBoolean(reader.GetOrdinal("requires_inventory")),
⋮----
public virtual async Task<Model_Dao_Result<Model_InventoriedDunnage>> GetByPartAsync(string partId)
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(string partId, string inventoryMethod, string notes, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_part_id", partId),
new MySqlParameter("@p_inventory_method", inventoryMethod),
new MySqlParameter("@p_notes", notes),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string inventoryMethod, string notes, string user)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
private Model_InventoriedDunnage MapFromReader(IDataReader reader)
⋮----
return new Model_InventoriedDunnage
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
PartId = reader.GetString(reader.GetOrdinal("part_id")),
InventoryMethod = reader.IsDBNull(reader.GetOrdinal("inventory_method")) ? null : reader.GetString(reader.GetOrdinal("inventory_method")),
Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Services/Service_DunnageAdminWorkflow.cs

```csharp
public class Service_DunnageAdminWorkflow : IService_DunnageAdminWorkflow
⋮----
private Enum_DunnageAdminSection _currentSection = Enum_DunnageAdminSection.Hub;
⋮----
public async Task NavigateToSectionAsync(Enum_DunnageAdminSection section)
⋮----
public async Task NavigateToHubAsync()
⋮----
public Task<bool> CanNavigateAwayAsync()
⋮----
return Task.FromResult(true);
⋮----
return Task.FromResult(false);
⋮----
public void MarkDirty()
⋮----
public void MarkClean()
```

## File: Module_Dunnage/Services/Service_DunnageCSVWriter.cs

```csharp
public class Service_DunnageCSVWriter : IService_DunnageCSVWriter
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
⋮----
private CsvConfiguration GetRfc4180Configuration()
⋮----
return new CsvConfiguration(CultureInfo.InvariantCulture)
⋮----
ShouldQuote = args => !string.IsNullOrEmpty(args.Field) &&
(args.Field.Contains(",") ||
args.Field.Contains("\"") ||
args.Field.Contains("\n") ||
args.Field.Contains("\r")),
⋮----
public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads)
⋮----
public async Task<Model_CSVWriteResult> WriteToCsvAsync(List<Model_DunnageLoad> loads, string typeName)
⋮----
await _logger.LogWarningAsync("WriteToCsvAsync called with no loads to export");
return new Model_CSVWriteResult { ErrorMessage = "No loads to export." };
⋮----
await _logger.LogInfoAsync($"Starting CSV export for {loads.Count} loads of type '{typeName}'");
var specKeys = await _dunnageService.GetAllSpecKeysAsync();
specKeys.Sort();
⋮----
dynamic record = new ExpandoObject();
⋮----
if (load.Specs != null && load.Specs.TryGetValue(key, out object? value))
⋮----
dict[key] = ""; // FR-043: Empty string if missing
⋮----
records.Add(record);
⋮----
// Write to local path
⋮----
await _logger.LogInfoAsync($"Successfully wrote {records.Count} records to local CSV: {localPath}");
⋮----
// Ensure directory exists for network path
var networkDir = Path.GetDirectoryName(networkPath);
if (!string.IsNullOrEmpty(networkDir) && !Directory.Exists(networkDir))
⋮----
Directory.CreateDirectory(networkDir);
⋮----
await _logger.LogInfoAsync($"Successfully wrote {records.Count} records to network CSV: {networkPath}");
⋮----
await _logger.LogErrorAsync($"Failed to write to network path '{networkPath}': {ex.Message}");
// FR-044: Network failure graceful handling
⋮----
return new Model_CSVWriteResult
⋮----
await _logger.LogErrorAsync($"CSV export failed for type '{typeName}': {ex.Message}");
await _errorHandler.HandleErrorAsync($"Export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
return new Model_CSVWriteResult { ErrorMessage = $"Export failed: {ex.Message}" };
⋮----
/// <summary>
/// Write dunnage loads to CSV with dynamic columns for all spec keys
/// Used for Manual Entry and Edit Mode exports (all types in one file)
/// </summary>
/// <param name="loads"></param>
public async Task<Model_CSVWriteResult> WriteDynamicCsvAsync(
⋮----
await _logger.LogWarningAsync("WriteDynamicCsvAsync called with no loads to export");
⋮----
await _logger.LogInfoAsync($"Starting dynamic CSV export for {loads.Count} loads");
var specKeys = allSpecKeys ?? await _dunnageService.GetAllSpecKeysAsync();
⋮----
dict[key] = ""; // Blank cell for specs not applicable to this load's type
⋮----
// Generate filename if not provided
⋮----
await _logger.LogWarningAsync("Network path not available for CSV export");
⋮----
await _errorHandler.HandleErrorAsync($"Dynamic CSV export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
⋮----
public async Task<Model_CSVWriteResult> ExportSelectedLoadsAsync(
⋮----
return new Model_CSVWriteResult { ErrorMessage = "No loads selected for export." };
⋮----
specKeys = await _dunnageService.GetAllSpecKeysAsync();
⋮----
.Where(l => l.TypeId.HasValue)
.Select(l => l.TypeId!.Value)
.Distinct()
.ToList();
⋮----
var specs = await _dunnageService.GetSpecsForTypeAsync(typeId);
⋮----
var keys = specs.Data.Select(s => s.SpecKey).Distinct();
specKeys.AddRange(keys);
⋮----
specKeys = specKeys.Distinct().ToList();
⋮----
await _errorHandler.HandleErrorAsync($"Selected loads export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
⋮----
public async Task<bool> IsNetworkPathAvailableAsync(int timeout = 3)
⋮----
return await Task.Run(() =>
⋮----
return Directory.Exists(networkRoot);
⋮----
public string GetLocalCsvPath(string filename)
⋮----
var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var folder = Path.Combine(appData, "MTM_Receiving_Application");
if (!Directory.Exists(folder))
⋮----
Directory.CreateDirectory(folder);
⋮----
return Path.Combine(folder, filename);
⋮----
public string GetNetworkCsvPath(string filename)
⋮----
private async Task WriteCsvFileAsync(string path, IEnumerable<dynamic> records)
⋮----
var encoding = new UTF8Encoding(true);
await using (var writer = new StreamWriter(path, false, encoding))
await using (var csv = new CsvWriter(writer, GetRfc4180Configuration()))
⋮----
await csv.WriteRecordsAsync(records);
⋮----
private string FormatDateTime(DateTime value)
⋮----
return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
⋮----
public async Task<Model_CSVDeleteResult> ClearCSVFilesAsync(string? filenamePattern = null)
⋮----
var result = new Model_CSVDeleteResult();
⋮----
await _logger.LogInfoAsync($"Starting CSV file clearing with pattern: {filenamePattern ?? "all"}");
// Clear local files
⋮----
var localFolder = Path.GetDirectoryName(GetLocalCsvPath("dummy.csv"));
if (Directory.Exists(localFolder))
⋮----
var filesToClear = Directory.GetFiles(localFolder, filenamePattern ?? "*.csv");
⋮----
await using var writer = new StreamWriter(file, false, new UTF8Encoding(true));
await using var csv = new CsvWriter(writer, GetRfc4180Configuration());
⋮----
csv.NextRecord();
await _logger.LogInfoAsync($"Cleared local CSV file: {file}");
⋮----
await _logger.LogInfoAsync("Local CSV directory does not exist - no files to clear");
⋮----
errors.Add(result.LocalError);
await _logger.LogErrorAsync($"Failed to clear local CSV files: {ex.Message}");
⋮----
var networkFolder = Path.GetDirectoryName(GetNetworkCsvPath("dummy.csv"));
if (Directory.Exists(networkFolder))
⋮----
var filesToClear = Directory.GetFiles(networkFolder, filenamePattern ?? "*.csv");
⋮----
await _logger.LogInfoAsync($"Cleared network CSV file: {file}");
⋮----
await _logger.LogInfoAsync("Network CSV directory does not exist - no files to clear");
⋮----
errors.Add("Network path not available");
await _logger.LogWarningAsync("Network path not available for CSV clearing");
⋮----
errors.Add(result.NetworkError);
await _logger.LogErrorAsync($"Failed to clear network CSV files: {ex.Message}");
⋮----
await _logger.LogInfoAsync($"CSV clearing completed. Local: {result.LocalDeleted}, Network: {result.NetworkDeleted}");
⋮----
await _errorHandler.HandleErrorAsync($"CSV clearing failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
```

## File: Module_Dunnage/Services/Service_DunnageWorkflow.cs

```csharp
public class Service_DunnageWorkflow : IService_DunnageWorkflow
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_DunnageCSVWriter _csvWriter;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
public Task<bool> StartWorkflowAsync()
⋮----
if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultDunnageMode))
⋮----
switch (currentUser.DefaultDunnageMode.ToLower())
⋮----
return Task.FromResult(true);
⋮----
public Task<Model_WorkflowStepResult> AdvanceToNextStepAsync()
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Please select a dunnage type." });
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Please select a part." });
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Quantity must be greater than zero." });
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Already at Review step. Use Save to finish." });
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = true, TargetStep = CurrentStep });
⋮----
_errorHandler.HandleErrorAsync("Error advancing step", Enum_ErrorSeverity.Error, ex, true);
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = ex.Message });
⋮----
public void GoToStep(Enum_DunnageWorkflowStep step)
⋮----
public async Task<Model_SaveResult> SaveToCSVOnlyAsync()
⋮----
loads.Add(load);
⋮----
return new Model_SaveResult { IsSuccess = false, ErrorMessage = "No data to save." };
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(loads);
return new Model_SaveResult
⋮----
await _errorHandler.HandleErrorAsync("Error saving CSV", Enum_ErrorSeverity.Error, ex, true);
return new Model_SaveResult { IsSuccess = false, ErrorMessage = ex.Message };
⋮----
public async Task<Model_SaveResult> SaveToDatabaseOnlyAsync()
⋮----
var dbResult = await _dunnageService.SaveLoadsAsync(loads);
⋮----
await _errorHandler.HandleErrorAsync("Error saving to database", Enum_ErrorSeverity.Error, ex, true);
⋮----
public async Task<Model_SaveResult> SaveSessionAsync()
⋮----
await _errorHandler.HandleErrorAsync("Error saving session", Enum_ErrorSeverity.Error, ex, true);
⋮----
public void ClearSession()
⋮----
CurrentSession = new Model_DunnageSession();
_viewModelRegistry.ClearAllInputs();
⋮----
public async Task<Model_CSVDeleteResult> ResetCSVFilesAsync()
⋮----
return await _csvWriter.ClearCSVFilesAsync();
⋮----
public void AddCurrentLoadToSession()
⋮----
var location = string.IsNullOrWhiteSpace(CurrentSession.Location)
⋮----
var poNumber = string.IsNullOrWhiteSpace(CurrentSession.PONumber)
⋮----
var load = new Model_DunnageLoad
⋮----
LoadUuid = Guid.NewGuid(),
⋮----
CurrentSession.Loads.Add(load);
_logger.LogInfo($"Added load to session: Part {load.PartId}, Qty {load.Quantity}", "DunnageWorkflow");
⋮----
_errorHandler.HandleErrorAsync("Error adding load to session", Enum_ErrorSeverity.Medium, ex, false);
⋮----
private Model_DunnageLoad CreateLoadFromCurrentSession()
⋮----
return new Model_DunnageLoad
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AddTypeDialogViewModel.cs

```csharp
public partial class ViewModel_Dunnage_AddTypeDialog : ViewModel_Shared_Base
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly DispatcherQueue _dispatcherQueue;
⋮----
private MaterialIconKind _selectedIcon = MaterialIconKind.PackageVariantClosed;
⋮----
_dispatcherQueue = DispatcherQueue.GetForCurrentThread();
_validationTimer = _dispatcherQueue.CreateTimer();
_validationTimer.Interval = TimeSpan.FromMilliseconds(300);
⋮----
private async Task SaveTypeAsync()
⋮----
var typeResult = await _dunnageService.InsertTypeAsync(TypeName, SelectedIcon.ToString());
⋮----
await _errorHandler.ShowUserErrorAsync(typeResult.ErrorMessage, "Save Failed", nameof(SaveTypeAsync));
⋮----
var fieldResult = await _dunnageService.InsertCustomFieldAsync(typeId, field);
⋮----
await _errorHandler.ShowUserErrorAsync($"Failed to save field '{field.FieldName}': {fieldResult.ErrorMessage}",
⋮----
await _dunnageService.UpsertUserPreferenceAsync($"RecentIcon_{SelectedIcon}", DateTime.Now.ToString("O"));
⋮----
_errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium,
⋮----
private void AddField()
⋮----
if (string.IsNullOrWhiteSpace(FieldName) || CustomFields.Count >= 25)
⋮----
var field = new Model_CustomFieldDefinition
⋮----
CustomFields.Add(field);
⋮----
private void EditField(Model_CustomFieldDefinition field)
⋮----
private void DeleteField(Model_CustomFieldDefinition field)
⋮----
CustomFields.Remove(field);
⋮----
partial void OnTypeNameChanged(string value)
⋮----
partial void OnFieldNameChanged(string value)
⋮----
private void OnValidationTimerTick(object? sender, object e)
⋮----
private async void ValidateTypeName()
⋮----
if (string.IsNullOrWhiteSpace(TypeName))
⋮----
var result = await _dunnageService.CheckDuplicateTypeNameAsync(TypeName);
⋮----
DuplicateTypeId = result.Data.ToString();
⋮----
private void ValidateFieldName()
⋮----
if (string.IsNullOrWhiteSpace(FieldName))
⋮----
if (FieldName.Any(c => "<>{}[]|\\".Contains(c)))
⋮----
// Check for duplicate field name
if (CustomFields.Any(f => f.FieldName.Equals(FieldName, StringComparison.OrdinalIgnoreCase) && f != EditingField))
⋮----
private void UpdateCanSave()
⋮----
CanSave = !string.IsNullOrWhiteSpace(TypeName) &&
string.IsNullOrEmpty(TypeNameError) &&
⋮----
private async Task LoadRecentlyUsedIconsAsync()
⋮----
var result = await _dunnageService.GetRecentlyUsedIconsAsync(6);
⋮----
RecentlyUsedIcons.Clear();
⋮----
RecentlyUsedIcons.Add(icon);
⋮----
await _logger.LogErrorAsync($"Failed to load recently used icons: {ex.Message}");
⋮----
public void OnDragItemsCompleted()
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminMainViewModel.cs

```csharp
public partial class ViewModel_Dunnage_AdminMain : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageAdminWorkflow _adminWorkflow;
⋮----
private async Task NavigateToManageTypesAsync()
⋮----
await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Types);
await _logger.LogInfoAsync("Navigated to Type Management", "AdminMain");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private async Task NavigateToManageSpecsAsync()
⋮----
await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Specs);
await _logger.LogInfoAsync("Navigated to Spec Management", "AdminMain");
⋮----
private async Task NavigateToManagePartsAsync()
⋮----
await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Parts);
await _logger.LogInfoAsync("Navigated to Part Management", "AdminMain");
⋮----
private async Task NavigateToInventoriedListAsync()
⋮----
await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.InventoriedList);
await _logger.LogInfoAsync("Navigated to Inventoried List", "AdminMain");
⋮----
private async Task ReturnToMainNavigationAsync()
⋮----
await _adminWorkflow.NavigateToHubAsync();
await _logger.LogInfoAsync("Returned to main navigation hub", "AdminMain");
⋮----
private void OnSectionChanged(object? sender, Enum_DunnageAdminSection section)
⋮----
private void OnStatusMessageRaised(object? sender, string message)
⋮----
private void UpdateVisibility(Enum_DunnageAdminSection section)
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminTypesViewModel.cs

```csharp
public partial class ViewModel_Dunnage_AdminTypes : ViewModel_Shared_Base
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_DunnageAdminWorkflow _adminWorkflow;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
private async Task LoadTypesAsync()
⋮----
var result = await _dunnageService.GetAllTypesAsync();
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "LoadTypesAsync", true);
⋮----
Types.Clear();
⋮----
Types.Add(type);
⋮----
await _logger.LogInfoAsync($"Loaded {Types.Count} dunnage types", "TypeManagement");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private async Task ShowAddTypeAsync()
⋮----
XamlRoot = _windowService.GetXamlRoot()
⋮----
var result = await dialog.ShowAsync();
⋮----
await _logger.LogInfoAsync("New dunnage type added via Add Type Dialog", "TypeManagement");
⋮----
private async Task ShowEditTypeAsync()
⋮----
var editedType = new Model_DunnageType
⋮----
var dialog = new ContentDialog
⋮----
var stackPanel = new StackPanel { Spacing = 12 };
var typeNameBox = new TextBox
⋮----
var iconSelectorButton = new Button
⋮----
Padding = new Thickness(16, 12, 16, 12)
⋮----
var iconButtonPanel = new StackPanel
⋮----
var iconTextPanel = new StackPanel
⋮----
var iconLabel = new TextBlock
⋮----
var iconName = new TextBlock
⋮----
iconTextPanel.Children.Add(iconLabel);
iconTextPanel.Children.Add(iconName);
iconButtonPanel.Children.Add(iconDisplay);
iconButtonPanel.Children.Add(iconTextPanel);
⋮----
iconSelector.SetInitialSelection(editedType.IconKind);
iconSelector.Activate();
var selectedIcon = await iconSelector.WaitForSelectionAsync();
⋮----
editedType.Icon = selectedIcon.Value.ToString();
⋮----
iconName.Text = selectedIcon.Value.ToString();
⋮----
var iconHeader = new TextBlock
⋮----
Margin = new Thickness(0, 8, 0, 0),
⋮----
stackPanel.Children.Add(typeNameBox);
stackPanel.Children.Add(iconHeader);
stackPanel.Children.Add(iconSelectorButton);
⋮----
editedType.DunnageType = typeNameBox.Text.Trim();
if (string.IsNullOrWhiteSpace(editedType.DunnageType))
⋮----
await _errorHandler.ShowUserErrorAsync("Type name is required", "Validation Error", "ShowEditTypeAsync");
⋮----
var updateResult = await _dunnageService.UpdateTypeAsync(editedType);
⋮----
await _errorHandler.HandleDaoErrorAsync(updateResult, "UpdateTypeAsync", true);
⋮----
await _logger.LogInfoAsync($"Updated type: {editedType.DunnageType}", "TypeManagement");
⋮----
private async Task ShowDeleteConfirmationAsync()
⋮----
var partCountResult = await _dunnageService.GetPartCountByTypeAsync(SelectedType.Id);
var transactionCountResult = await _dunnageService.GetTransactionCountByTypeAsync(SelectedType.Id);
⋮----
await _errorHandler.HandleDaoErrorAsync(
⋮----
var warningDialog = new ContentDialog
⋮----
await warningDialog.ShowAsync();
⋮----
var confirmDialog = new ContentDialog
⋮----
stackPanel.Children.Add(new TextBlock { Text = impactMessage, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
var confirmBox = new TextBox
⋮----
stackPanel.Children.Add(confirmBox);
⋮----
var result = await confirmDialog.ShowAsync();
⋮----
private async Task DeleteTypeAsync()
⋮----
var result = await _dunnageService.DeleteTypeAsync(SelectedType.Id);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "DeleteTypeAsync", true);
⋮----
await _logger.LogInfoAsync($"Deleted type: {SelectedType.DunnageType}", "TypeManagement");
⋮----
private async Task ReturnToAdminHubAsync()
⋮----
_logger.LogInfo("Returning to Settings Mode Selection from Admin Types");
⋮----
settingsWorkflow.GoBack();
⋮----
partial void OnSelectedTypeChanged(Model_DunnageType? value)
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_EditModeViewModel.cs

```csharp
public partial class ViewModel_Dunnage_EditMode : ViewModel_Shared_Base
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_Pagination _paginationService;
private readonly IService_DunnageCSVWriter _csvWriter;
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
FromDate = DateTimeOffset.Now.AddDays(-7);
⋮----
public string LastWeekButtonText => $"Last Week ({DateTime.Now.Date.AddDays(-7):MMM d} - {DateTime.Now.Date:MMM d})";
⋮----
var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
⋮----
startOfWeek = startOfWeek.AddDays(-7);
⋮----
var quarterStart = new DateTime(today.Year, startMonth, 1);
⋮----
private async Task LoadFromCurrentMemoryAsync()
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
_logger.LogInfo("No unsaved loads found in current session", "EditMode");
⋮----
_allLoads = _workflowService.CurrentSession.Loads.ToList();
⋮----
_paginationService.SetSource(_allLoads);
⋮----
_logger.LogInfo($"Loaded {TotalRecords} loads from current session", "EditMode");
⋮----
private async Task LoadFromCurrentLabelsAsync()
⋮----
var localPath = System.IO.Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
⋮----
if (!System.IO.File.Exists(localPath))
⋮----
_logger.LogWarning($"CSV file not found at {localPath}", "EditMode");
⋮----
csv.Read();
csv.ReadHeader();
while (csv.Read())
⋮----
var load = new Model_DunnageLoad
⋮----
ReceivedDate = DateTime.TryParse(csv.GetField<string>("Date"), out var date) ? date : DateTime.Now
⋮----
loadsList.Add(load);
⋮----
_logger.LogWarning($"Failed to parse line {lineNumber}: {rowEx.Message}", "EditMode");
⋮----
_logger.LogInfo($"Loaded {TotalRecords} loads from CSV file", "EditMode");
⋮----
private async Task LoadFromHistoryAsync()
⋮----
var startDate = FromDate?.DateTime ?? DateTime.Now.AddDays(-7);
⋮----
var result = await _dunnageService.GetLoadsByDateRangeAsync(startDate, endDate);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "LoadFromHistoryAsync", true);
⋮----
_logger.LogInfo($"Loaded {TotalRecords} historical loads from {startDate:d} to {endDate:d}", "EditMode");
⋮----
private async Task SetFilterLastWeekAsync()
⋮----
FromDate = DateTime.Now.Date.AddDays(-7);
⋮----
private async Task SetFilterTodayAsync()
⋮----
private async Task SetFilterThisWeekAsync()
⋮----
private async Task SetFilterThisMonthAsync()
⋮----
FromDate = new DateTime(today.Year, today.Month, 1);
⋮----
private async Task SetFilterThisQuarterAsync()
⋮----
FromDate = new DateTime(today.Year, startMonth, 1);
⋮----
private async Task SetFilterShowAllAsync()
⋮----
FromDate = DateTime.Now.Date.AddYears(-1);
⋮----
private void SetDateRangeToday()
⋮----
private void SetDateRangeLastWeek()
⋮----
private void SetDateRangeLastMonth()
⋮----
FromDate = DateTime.Now.Date.AddMonths(-1);
⋮----
private void FirstPage()
⋮----
private void PreviousPage()
⋮----
private void NextPage()
⋮----
private void LastPage()
⋮----
private void LoadPage(int pageNumber)
⋮----
_paginationService.GoToPage(pageNumber);
⋮----
FilteredLoads.Clear();
⋮----
FilteredLoads.Add(load);
⋮----
_logger.LogInfo($"Loaded page {CurrentPage} of {TotalPages}", "EditMode");
⋮----
private void SelectAll()
⋮----
SelectedLoads.Clear();
⋮----
SelectedLoads.Add(load);
⋮----
_logger.LogInfo($"Selected all {FilteredLoads.Count} loads on page", "EditMode");
⋮----
private void RemoveSelectedRows()
⋮----
var loadsToRemove = SelectedLoads.ToList();
⋮----
FilteredLoads.Remove(load);
_allLoads.Remove(load);
⋮----
_logger.LogInfo($"Removed {loadsToRemove.Count} loads", "EditMode");
⋮----
private async Task SaveAllAsync()
⋮----
if (string.IsNullOrWhiteSpace(load.TypeName) || string.IsNullOrWhiteSpace(load.PartId))
⋮----
var saveResult = await _dunnageService.SaveLoadsAsync(_allLoads);
⋮----
await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(_allLoads);
⋮----
_logger.LogInfo($"Saved {_allLoads.Count} loads", "EditMode");
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null", null, "EditMode");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error, null, true);
⋮----
var result = await dialog.ShowAsync().AsTask();
⋮----
_logger.LogInfo("User confirmed return to mode selection, clearing data", "EditMode");
⋮----
_allLoads.Clear();
_workflowService.ClearSession();
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
⋮----
_logger.LogError($"Failed to return to mode selection: {ex.Message}", ex, "EditMode");
await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex, true);
⋮----
_logger.LogInfo("User cancelled return to mode selection", "EditMode");
⋮----
private void OnPageChanged(object? sender, EventArgs e)
⋮----
_logger.LogInfo($"Page changed to {CurrentPage} of {TotalPages}", "EditMode");
⋮----
private void UpdateCanSave()
⋮----
CanSave = _allLoads.Any(l => !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId));
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_QuantityEntryViewModel.cs

```csharp
public partial class ViewModel_Dunnage_QuantityEntry : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Dispatcher _dispatcher;
private readonly IService_Help _helpService;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
_dispatcher.TryEnqueue(LoadContextData);
⋮----
if (!string.IsNullOrEmpty(SelectedTypeIcon) && Enum.TryParse<MaterialIconKind>(SelectedTypeIcon, true, out var kind))
⋮----
public void LoadContextData()
⋮----
_logger.LogInfo($"Initialized workflow session quantity to {Quantity}", "QuantityEntry");
⋮----
_logger.LogInfo($"Loaded context: Type={SelectedTypeName}, Part={SelectedPartName}, Quantity={_workflowService.CurrentSession.Quantity}", "QuantityEntry");
⋮----
_logger.LogError($"Error loading context data: {ex.Message}", ex, "QuantityEntry");
⋮----
partial void OnQuantityChanged(int value)
⋮----
_logger.LogInfo($"Quantity changed to {value}, updated workflow session", "QuantityEntry");
⋮----
GoNextCommand.NotifyCanExecuteChanged();
⋮----
private void ValidateQuantity()
⋮----
private void GoBack()
⋮----
_logger.LogInfo("Navigating back to Part Selection", "QuantityEntry");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
⋮----
private async Task GoNextAsync()
⋮----
_logger.LogInfo($"Quantity set to {Quantity}", "QuantityEntry");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_TypeSelectionViewModel.cs

```csharp
public partial class ViewModel_dunnage_typeselection : ViewModel_Shared_Base, IResettableViewModel
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_Pagination _paginationService;
private readonly IService_Help _helpService;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
_viewModelRegistry.Register(this);
⋮----
public void ResetToDefaults()
⋮----
public async Task InitializeAsync()
⋮----
_logger.LogInfo("TypeSelection: InitializeAsync called", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo("TypeSelection: Already busy, returning", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo("TypeSelection: Starting to load types", "ViewModel_dunnage_typeselection");
⋮----
_errorHandler.HandleErrorAsync(
⋮----
).Wait();
⋮----
private async Task LoadTypesAsync()
⋮----
_logger.LogInfo("TypeSelection: LoadTypesAsync called", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo("TypeSelection: Calling service.GetAllTypesAsync()", "ViewModel_dunnage_typeselection");
var result = await _dunnageService.GetAllTypesAsync();
_logger.LogInfo($"TypeSelection: Service returned - IsSuccess: {result.IsSuccess}, Data null: {result.Data == null}, Count: {result.Data?.Count ?? 0}", "ViewModel_dunnage_typeselection");
⋮----
_paginationService.SetSource(result.Data);
_logger.LogInfo($"TypeSelection: Pagination configured with PageSize=9, TotalItems={result.Data.Count}", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo($"TypeSelection: Successfully loaded {result.Data.Count} dunnage types with {TotalPages} pages, DisplayedTypes.Count={DisplayedTypes.Count}", "ViewModel_dunnage_typeselection");
⋮----
await _errorHandler.HandleDaoErrorAsync(
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private void NextPage()
⋮----
_paginationService.NextPage();
⋮----
private void PreviousPage()
⋮----
_paginationService.PreviousPage();
⋮----
private void FirstPage()
⋮----
_paginationService.FirstPage();
⋮----
private void LastPage()
⋮----
_paginationService.LastPage();
⋮----
private async Task SelectTypeAsync(Model_DunnageType? type)
⋮----
_logger.LogInfo($"TypeSelection: Selected dunnage type: {type.TypeName} (ID: {type.Id})", "TypeSelection");
⋮----
_logger.LogInfo($"TypeSelection: Session updated - SelectedTypeId={_workflowService.CurrentSession.SelectedTypeId}", "TypeSelection");
_logger.LogInfo("TypeSelection: Navigating to PartSelection step", "TypeSelection");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
⋮----
_logger.LogError($"TypeSelection: Error selecting type: {ex.Message}", ex, "TypeSelection");
⋮----
private async Task QuickAddTypeAsync()
⋮----
_logger.LogInfo("Quick Add Type requested", "TypeSelection");
⋮----
_logger.LogInfo("Cannot show dialog: XamlRoot is null", "TypeSelection");
⋮----
var result = await dialog.ShowAsync();
⋮----
var iconName = dialog.SelectedIconKind.ToString();
_logger.LogInfo($"Adding new type: {typeName} with icon {iconName}", "TypeSelection");
var newType = new Model_DunnageType
⋮----
var insertResult = await _dunnageService.InsertTypeAsync(newType);
⋮----
_logger.LogInfo($"Successfully added type: {typeName}", "TypeSelection");
⋮----
var specDef = new SpecDefinition
⋮----
var specModel = new Model_DunnageSpec
⋮----
SpecValue = JsonSerializer.Serialize(specDef)
⋮----
await _dunnageService.InsertSpecAsync(specModel);
⋮----
private async Task EditTypeAsync(Model_DunnageType type)
⋮----
_logger.LogInfo($"Edit Type requested for {type.TypeName}", "TypeSelection");
⋮----
var specsResult = await _dunnageService.GetSpecsForTypeAsync(type.Id);
⋮----
existingSpecsDict[s.SpecKey] = new SpecDefinition();
⋮----
dialog.InitializeForEdit(type.TypeName, type.Icon, existingSpecsDict);
⋮----
var newIcon = dialog.SelectedIconKind.ToString();
⋮----
var updateResult = await _dunnageService.UpdateTypeAsync(type);
⋮----
await _errorHandler.HandleDaoErrorAsync(updateResult, nameof(EditTypeAsync), true);
⋮----
var newSpecNames = newSpecs.Select(s => s.Name).ToList();
var removedSpecKeys = existingSpecsDict.Keys.Except(newSpecNames).ToList();
⋮----
await _dunnageService.DeleteSpecAsync(specToDelete.Id);
⋮----
var json = JsonSerializer.Serialize(specDef);
if (existingSpecsDict.ContainsKey(specItem.Name))
⋮----
await _dunnageService.UpdateSpecAsync(existingModel);
⋮----
_logger.LogInfo($"Successfully updated type: {type.TypeName}", "TypeSelection");
⋮----
await _errorHandler.HandleErrorAsync("Error updating dunnage type", Enum_ErrorSeverity.Error, ex, true);
⋮----
private async Task DeleteTypeAsync(Model_DunnageType type)
⋮----
_logger.LogInfo($"Delete Type requested for {type.TypeName}", "TypeSelection");
⋮----
var deleteResult = await _dunnageService.DeleteTypeAsync(type.Id);
⋮----
_logger.LogInfo($"Successfully deleted type: {type.TypeName}", "TypeSelection");
⋮----
await _errorHandler.HandleDaoErrorAsync(deleteResult, nameof(DeleteTypeAsync), true);
⋮----
await _errorHandler.HandleErrorAsync("Error deleting dunnage type", Enum_ErrorSeverity.Error, ex, true);
⋮----
private void OnPageChanged(object? sender, EventArgs e)
⋮----
private void UpdatePaginationProperties()
⋮----
NextPageCommand.NotifyCanExecuteChanged();
PreviousPageCommand.NotifyCanExecuteChanged();
⋮----
private void UpdatePageDisplay()
⋮----
_logger.LogInfo($"TypeSelection: UpdatePageDisplay - Got {currentItems.Count()} items from pagination service", "ViewModel_dunnage_typeselection");
DisplayedTypes.Clear();
⋮----
DisplayedTypes.Add(type);
_logger.LogInfo($"TypeSelection: Added type to DisplayedTypes - ID: {type.Id}, Name: {type.TypeName}", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo($"TypeSelection: DisplayedTypes.Count after update: {DisplayedTypes.Count}", "ViewModel_dunnage_typeselection");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml

```xml
<Page x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_WorkflowView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
      xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
      xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
      mc:Ignorable="d">

    <Page.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter" />
    </Page.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Content Area -->
        <Grid Grid.Row="1"
              Margin="16,12,16,16">
            <!-- Mode Selection View -->
            <views:View_Dunnage_ModeSelectionView Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <!-- Wizard Steps -->
            <views:View_dunnage_typeselectionView Visibility="{x:Bind ViewModel.IsTypeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_PartSelectionView Visibility="{x:Bind ViewModel.IsPartSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_QuantityEntryView Visibility="{x:Bind ViewModel.IsQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_DetailsEntryView Visibility="{x:Bind ViewModel.IsDetailsEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_ReviewView Visibility="{x:Bind ViewModel.IsReviewVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <!-- Bulk Operation Views -->
            <views:View_Dunnage_ManualEntryView Visibility="{x:Bind ViewModel.IsManualEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_EditModeView Visibility="{x:Bind ViewModel.IsEditModeVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
        </Grid>

        <!-- Navigation Buttons -->
        <Grid Grid.Row="2"
              Margin="24">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>

            <!-- Left-aligned buttons -->
            <StackPanel Grid.Column="0"
                        Orientation="Horizontal"
                        HorizontalAlignment="Left"
                        Spacing="12">
                <!-- Mode Selection Button (visible in wizard steps) -->
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsTypeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsPartSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsDetailsEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsManualEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsEditModeVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
            </StackPanel>

            <!-- Center help button -->
            <Button Grid.Column="1"
                    HorizontalAlignment="Center"
                    Style="{StaticResource AccentButtonStyle}"
                    Padding="12,8"
                    Click="HelpButton_Click"
                    ToolTipService.ToolTip="Click for help about the current step">
                <StackPanel Orientation="Horizontal"
                            Spacing="8">
                    <FontIcon Glyph="&#xE946;"
                              FontSize="16" />
                    <TextBlock Text="Help" />
                </StackPanel>
            </Button>

            <!-- Right-aligned navigation buttons (wizard flow) -->
            <StackPanel Grid.Column="2"
                        Orientation="Horizontal"
                        HorizontalAlignment="Right"
                        Spacing="12">
                <!-- Back Button (not visible on first wizard step) -->
                <Button Content="Back"
                        Visibility="{x:Bind ViewModel.IsPartSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnBackClick"
                        ToolTipService.ToolTip="Go back to previous step (Ctrl+Left)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Left"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Back"
                        Visibility="{x:Bind ViewModel.IsQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnBackClick"
                        ToolTipService.ToolTip="Go back to previous step (Ctrl+Left)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Left"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Back"
                        Visibility="{x:Bind ViewModel.IsDetailsEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnBackClick"
                        ToolTipService.ToolTip="Go back to previous step (Ctrl+Left)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Left"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>

                <!-- Next Button (wizard flow) -->
                <Button Content="Next"
                        Style="{StaticResource AccentButtonStyle}"
                        Visibility="{x:Bind ViewModel.IsTypeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnNextClick"
                        ToolTipService.ToolTip="Proceed to next step (Ctrl+Right)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Right"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Next"
                        Style="{StaticResource AccentButtonStyle}"
                        Visibility="{x:Bind ViewModel.IsPartSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnNextClick"
                        ToolTipService.ToolTip="Proceed to next step (Ctrl+Right)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Right"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Next"
                        Style="{StaticResource AccentButtonStyle}"
                        Visibility="{x:Bind ViewModel.IsQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnNextClick"
                        ToolTipService.ToolTip="Proceed to next step (Ctrl+Right)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Right"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Save &amp; Review"
                        Style="{StaticResource AccentButtonStyle}"
                        Visibility="{x:Bind ViewModel.IsDetailsEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnSaveAndReviewClick"
                        ToolTipService.ToolTip="Save entry and proceed to review (Ctrl+Right)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Right"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Receiving/Data/Dao_ReceivingLine.cs

```csharp
public class Dao_ReceivingLine
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
⋮----
new MySqlParameter("@p_Quantity", line.Quantity),
new MySqlParameter("@p_PartID", line.PartID ?? string.Empty),
new MySqlParameter("@p_PONumber", line.PONumber),
new MySqlParameter("@p_EmployeeNumber", line.EmployeeNumber),
new MySqlParameter("@p_Heat", line.Heat ?? string.Empty),
new MySqlParameter("@p_Date", line.Date),
new MySqlParameter("@p_InitialLocation", line.InitialLocation ?? string.Empty),
new MySqlParameter("@p_CoilsOnSkid", (object?)line.CoilsOnSkid ?? DBNull.Value),
new MySqlParameter("@p_VendorName", line.VendorName ?? "Unknown"),
new MySqlParameter("@p_PartDescription", line.PartDescription ?? string.Empty),
new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
⋮----
if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
⋮----
return new Model_Dao_Result
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
```

## File: Module_Receiving/Services/Service_ReceivingValidation.cs

```csharp
public class Service_ReceivingValidation : IService_ReceivingValidation
⋮----
private readonly IService_InforVisual _inforVisualService;
private static readonly Regex _regex = new Regex(@"^(PO-)?\d{1,6}$", RegexOptions.IgnoreCase);
⋮----
_inforVisualService = inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
⋮----
public Model_ReceivingValidationResult ValidatePONumber(string poNumber)
⋮----
if (string.IsNullOrWhiteSpace(poNumber))
⋮----
return Model_ReceivingValidationResult.Error("PO number is required");
⋮----
if (!_regex.IsMatch(poNumber))
⋮----
return Model_ReceivingValidationResult.Error("PO number must be numeric (up to 6 digits) or in PO-###### format");
⋮----
return Model_ReceivingValidationResult.Success();
⋮----
public Model_ReceivingValidationResult ValidatePartID(string partID)
⋮----
if (string.IsNullOrWhiteSpace(partID))
⋮----
return Model_ReceivingValidationResult.Error("Part ID is required");
⋮----
return Model_ReceivingValidationResult.Error("Part ID cannot exceed 50 characters");
⋮----
public Model_ReceivingValidationResult ValidateNumberOfLoads(int numLoads)
⋮----
return Model_ReceivingValidationResult.Error("Number of loads must be at least 1");
⋮----
return Model_ReceivingValidationResult.Error("Number of loads cannot exceed 99");
⋮----
public Model_ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity)
⋮----
return Model_ReceivingValidationResult.Error("Weight/Quantity must be greater than 0");
⋮----
public Model_ReceivingValidationResult ValidatePackageCount(int packagesPerLoad)
⋮----
return Model_ReceivingValidationResult.Error("Package count must be greater than 0");
⋮----
public Model_ReceivingValidationResult ValidateHeatLotNumber(string heatLotNumber)
⋮----
if (string.IsNullOrWhiteSpace(heatLotNumber))
⋮----
return Model_ReceivingValidationResult.Error("Heat/Lot number is required");
⋮----
return Model_ReceivingValidationResult.Error("Heat/Lot number cannot exceed 50 characters");
⋮----
public Task<Model_ReceivingValidationResult> ValidateAgainstPOQuantityAsync(decimal totalQuantity, decimal orderedQuantity, string partID)
⋮----
return Task.FromResult(Model_ReceivingValidationResult.Warning(
⋮----
return Task.FromResult(Model_ReceivingValidationResult.Success());
⋮----
public async Task<Model_ReceivingValidationResult> CheckSameDayReceivingAsync(string poNumber, string partID, decimal userEnteredQuantity)
⋮----
var result = await _inforVisualService.GetSameDayReceivingQuantityAsync(poNumber, partID, DateTime.Today);
⋮----
return Model_ReceivingValidationResult.Warning(
⋮----
public Model_ReceivingValidationResult ValidateReceivingLoad(Model_ReceivingLoad load)
⋮----
if (string.IsNullOrWhiteSpace(load.PartID))
⋮----
errors.Add("Part ID is required");
⋮----
if (string.IsNullOrWhiteSpace(load.PartType))
⋮----
errors.Add("Part Type is required");
⋮----
errors.Add("Load number must be at least 1");
⋮----
errors.Add($"Load {load.LoadNumber}: Weight/Quantity must be greater than 0");
⋮----
if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
⋮----
errors.Add($"Load {load.LoadNumber}: Heat/Lot number is required");
⋮----
errors.Add($"Load {load.LoadNumber}: Package count must be greater than 0");
⋮----
if (string.IsNullOrWhiteSpace(load.PackageTypeName))
⋮----
errors.Add($"Load {load.LoadNumber}: Package type is required");
⋮----
var result = Model_ReceivingValidationResult.Error(string.Join("; ", errors));
⋮----
public Model_ReceivingValidationResult ValidateSession(List<Model_ReceivingLoad> loads)
⋮----
return Model_ReceivingValidationResult.Error("Session must contain at least one load");
⋮----
allErrors.AddRange(loadValidation.Errors);
⋮----
var result = Model_ReceivingValidationResult.Error($"{allErrors.Count} validation error(s) found");
⋮----
public async Task<Model_ReceivingValidationResult> ValidatePartExistsInVisualAsync(string partID)
⋮----
var result = await _inforVisualService.GetPartByIDAsync(partID);
⋮----
return Model_ReceivingValidationResult.Error($"Error validating part: {result.ErrorMessage}");
⋮----
return Model_ReceivingValidationResult.Error($"Part ID {partID} not found in Infor Visual database");
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_HeatLot.cs

```csharp
public partial class ViewModel_Receiving_HeatLot : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_Help _helpService;
⋮----
private void OnStepChanged(object? sender, System.EventArgs e)
⋮----
public Task OnNavigatedToAsync()
⋮----
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
private void AutoFill()
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.HeatLotNumber) && !string.IsNullOrWhiteSpace(prevLoad.HeatLotNumber))
⋮----
private Task ValidateAndContinueAsync()
⋮----
private void PrepareHeatLotFields()
⋮----
if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.HeatLot");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs

```csharp
public partial class ViewModel_Receiving_LoadEntry : ViewModel_Shared_Base, IResettableViewModel
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_Help _helpService;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
_viewModelRegistry.Register(this);
⋮----
public void ResetToDefaults()
⋮----
private void OnStepChanged(object? sender, System.EventArgs e)
⋮----
private async Task CreateLoadsAsync()
⋮----
var validationResult = _validationService.ValidateNumberOfLoads(NumberOfLoads);
⋮----
await _errorHandler.HandleErrorAsync(validationResult.Message, Enum_ErrorSeverity.Warning);
⋮----
partial void OnNumberOfLoadsChanged(int value)
⋮----
public void UpdatePartInfo(string partId, string description)
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.LoadEntry");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs

```csharp
public partial class ViewModel_Receiving_ModeSelection : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_UserPreferences _userPreferencesService;
private readonly IService_Help _helpService;
private readonly IService_Window _windowService;
⋮----
private void LoadDefaultMode()
⋮----
private async Task SelectGuidedModeAsync()
⋮----
_logger.LogInfo("User selected Guided Mode.");
⋮----
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
⋮----
private async Task SelectManualModeAsync()
⋮----
_logger.LogInfo("User selected Manual Mode.");
⋮----
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry);
⋮----
private async Task SelectEditModeAsync()
⋮----
_logger.LogInfo("User selected Edit Mode.");
⋮----
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.EditMode);
⋮----
private bool HasUnsavedData()
⋮----
if (!string.IsNullOrEmpty(_workflowService.CurrentSession.PoNumber) ||
⋮----
private async Task<bool> ConfirmModeChangeAsync()
⋮----
_logger.LogInfo("No unsaved data detected, skipping confirmation dialog");
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogWarning("XamlRoot is null, proceeding without confirmation");
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogError($"Error showing confirmation dialog: {ex.Message}", ex);
⋮----
private void ClearWorkflowData()
⋮----
_logger.LogInfo("Workflow data and UI inputs cleared for mode change");
⋮----
_logger.LogError($"Error clearing workflow data: {ex.Message}", ex);
⋮----
private void ClearAllUIInputs()
⋮----
_logger.LogInfo("UI inputs cleared across all Receiving ViewModels");
⋮----
_logger.LogError($"Error clearing UI inputs: {ex.Message}", ex);
⋮----
private async Task SetGuidedAsDefaultAsync(bool isChecked)
⋮----
var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, newMode ?? "");
⋮----
// Update in-memory user object
⋮----
// Update UI state
⋮----
_logger.LogInfo($"Default mode set to: {newMode ?? "none"}");
⋮----
await _errorHandler.ShowErrorDialogAsync("Save Error", result.ErrorMessage, Enum_ErrorSeverity.Error);
⋮----
await _errorHandler.HandleErrorAsync($"Failed to set default mode: {ex.Message}",
⋮----
private async Task SetManualAsDefaultAsync(bool isChecked)
⋮----
private async Task SetEditAsDefaultAsync(bool isChecked)
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.ModeSelection");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs

```csharp
public partial class ViewModel_Receiving_Review : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_Help _helpService;
private readonly IService_Window _windowService;
⋮----
private void OnStepChanged(object? sender, System.EventArgs e)
⋮----
public async Task OnNavigatedToAsync()
⋮----
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
private void PreviousEntry()
⋮----
private void NextEntry()
⋮----
private void SwitchToTableView()
⋮----
private void SwitchToSingleView()
⋮----
private async Task AddAnotherPartAsync()
⋮----
_logger.LogInfo("User requested to add another part/PO");
⋮----
_logger.LogInfo("User cancelled add another part/PO");
⋮----
var saveResult = await _workflowService.SaveToCSVOnlyAsync();
⋮----
await _errorHandler.HandleErrorAsync(
$"Failed to save CSV backup: {string.Join(", ", saveResult.Errors)}",
⋮----
await _workflowService.AddCurrentPartToSessionAsync();
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
_logger.LogInfo("Navigated to PO Entry for new part, workflow data cleared");
⋮----
_logger.LogError($"Error in AddAnotherPartAsync: {ex.Message}", ex);
⋮----
private async Task<bool> ConfirmAddAnotherAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogWarning("XamlRoot is null, proceeding without confirmation");
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogError($"Error showing confirmation dialog: {ex.Message}", ex);
⋮----
private void ClearTransientWorkflowData()
⋮----
_workflowService.ClearUIInputs();
_logger.LogInfo("Transient workflow data and UI inputs cleared for new entry");
⋮----
_logger.LogError($"Error clearing transient workflow data: {ex.Message}", ex);
⋮----
private async Task SaveAsync()
⋮----
await _workflowService.AdvanceToNextStepAsync();
⋮----
public void HandleCascadingUpdate(Model_ReceivingLoad changedLoad, string propertyName)
⋮----
var index = Loads.IndexOf(changedLoad);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.Review");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_WeightQuantity.cs

```csharp
public partial class ViewModel_Receiving_WeightQuantity : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_InforVisual _inforVisualService;
private readonly IService_Help _helpService;
⋮----
private void OnStepChanged(object? sender, EventArgs e)
⋮----
public async Task OnNavigatedToAsync()
⋮----
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
private void UpdatePOQuantityInfo()
⋮----
private async Task CheckSameDayReceivingAsync()
⋮----
if (string.IsNullOrEmpty(poNumber) || string.IsNullOrEmpty(partId))
⋮----
var result = await _inforVisualService.GetSameDayReceivingQuantityAsync(poNumber, partId, DateTime.Today);
⋮----
await _errorHandler.LogErrorAsync("Failed to check same-day receiving", Enum_ErrorSeverity.Warning, ex);
⋮----
private async Task ValidateAndContinueAsync()
⋮----
var result = _validationService.ValidateWeightQuantity(load.WeightQuantity);
⋮----
await _errorHandler.HandleErrorAsync($"Load {load.LoadNumber}: {result.Message}", Enum_ErrorSeverity.Warning);
⋮----
var totalWeight = Loads.Sum(l => l.WeightQuantity);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.WeightQuantity");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs

```csharp
public partial class ViewModel_Receiving_Workflow : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_Help _helpService;
⋮----
private readonly IService_Dispatcher _dispatcherService;
private readonly IService_Window _windowService;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
_logger.LogInfo("StepChanged event received in ViewModel. Updating visibility.");
⋮----
_logger.LogInfo("Step is Saving. Enqueuing PerformSaveAsync via Dispatcher.");
_dispatcherService.TryEnqueue(async () =>
⋮----
HelpContent = Helper_WorkflowHelpContentGenerator.GenerateHelpContent(_workflowService.CurrentStep);
_logger.LogInfo($"Visibility updated. Current Step: {_workflowService.CurrentStep}, Title: {CurrentStepTitle}");
⋮----
private async Task NextStepAsync()
⋮----
_logger.LogInfo("NextStepAsync command triggered.");
var result = await _workflowService.AdvanceToNextStepAsync();
_logger.LogInfo($"AdvanceToNextStepAsync returned. Success: {result.Success}, Step: {_workflowService.CurrentStep}");
⋮----
await _errorHandler.HandleErrorAsync(
string.Join("\n", result.ValidationErrors),
⋮----
_logger.LogError($"Error in NextStepAsync: {ex.Message}", ex);
await _errorHandler.HandleErrorAsync($"An error occurred: {ex.Message}", Enum_ErrorSeverity.Error);
⋮----
private async Task PerformSaveAsync()
⋮----
_logger.LogInfo("PerformSaveAsync called but already saving. Ignoring.");
⋮----
_logger.LogInfo("PerformSaveAsync started.");
⋮----
_logger.LogInfo($"Save progress message: {msg}");
⋮----
_logger.LogInfo($"Save progress percent: {pct}");
⋮----
_logger.LogInfo("Calling _workflowService.SaveSessionAsync...");
LastSaveResult = await _workflowService.SaveSessionAsync(messageProgress, percentProgress);
_logger.LogInfo($"SaveSessionAsync returned. Success: {LastSaveResult.Success}");
await _workflowService.AdvanceToNextStepAsync();
⋮----
_logger.LogError($"Error in PerformSaveAsync: {ex.Message}", ex);
await _errorHandler.HandleErrorAsync($"Save failed: {ex.Message}", Enum_ErrorSeverity.Error);
⋮----
private async Task StartNewEntryAsync()
⋮----
await _workflowService.ResetWorkflowAsync();
⋮----
private async Task ResetCSVAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
var saveResult = await _workflowService.SaveToDatabaseOnlyAsync();
⋮----
var warnDialog = new ContentDialog
⋮----
Content = $"Failed to save to database: {string.Join(", ", saveResult.Errors)}\n\nDo you want to proceed with deleting CSV files anyway?",
⋮----
var warnResult = await warnDialog.ShowAsync();
⋮----
var deleteResult = await _workflowService.ResetCSVFilesAsync();
⋮----
private void PreviousStep()
⋮----
var result = _workflowService.GoToPreviousStep();
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.Workflow");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/Views/View_Receiving_Workflow.xaml.cs

```csharp
public sealed partial class View_Receiving_Workflow : Page
⋮----
this.InitializeComponent();
⋮----
private async void HelpButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
⋮----
protected override void OnNavigatedTo(NavigationEventArgs e)
⋮----
base.OnNavigatedTo(e);
⋮----
if (!string.IsNullOrEmpty(defaultMode))
⋮----
workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
⋮----
workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry);
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

## File: Module_Routing/Views/RoutingModeSelectionView.xaml

```xml
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

## File: Module_Routing/Views/RoutingWizardContainerView.xaml

```xml
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

## File: Module_Routing/Views/RoutingWizardStep2View.xaml

```xml
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

## File: Module_Settings/Data/Dao_PackageTypeMappings.cs

```csharp
public class Dao_PackageTypeMappings
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_PackageTypeMapping>>> GetAllAsync()
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result<string>> GetByPrefixAsync(string partPrefix)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
reader => reader.IsDBNull(reader.GetOrdinal("package_type"))
⋮----
: reader.GetString(reader.GetOrdinal("package_type")),
⋮----
public async Task<Model_Dao_Result<int>> InsertAsync(Model_PackageTypeMapping mapping, int createdBy)
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
reader => reader.GetInt32(reader.GetOrdinal("id")),
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_PackageTypeMapping mapping)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
private static Model_PackageTypeMapping MapFromReader(IDataReader reader)
⋮----
return new Model_PackageTypeMapping
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
PartPrefix = reader.GetString(reader.GetOrdinal("part_prefix")),
PackageType = reader.GetString(reader.GetOrdinal("package_type")),
IsDefault = reader.GetBoolean(reader.GetOrdinal("is_default")),
DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32(reader.GetOrdinal("created_by"))
```

## File: Module_Settings/Data/Dao_ScheduledReport.cs

```csharp
public class Dao_ScheduledReport
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_ScheduledReport>>> GetAllAsync()
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result<Model_ScheduledReport>> GetByIdAsync(int id)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result<List<Model_ScheduledReport>>> GetDueAsync()
⋮----
public async Task<Model_Dao_Result<List<Model_ScheduledReport>>> GetActiveAsync()
⋮----
public async Task<Model_Dao_Result<int>> InsertAsync(Model_ScheduledReport report, int createdBy)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("id")),
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_ScheduledReport report)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> UpdateLastRunAsync(int id, DateTime lastRunDate, DateTime nextRunDate)
⋮----
public async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
public async Task<Model_Dao_Result> ToggleActiveAsync(int id, bool isActive)
⋮----
private static Model_ScheduledReport MapFromReader(IDataReader reader)
⋮----
return new Model_ScheduledReport
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
ReportType = reader.GetString(reader.GetOrdinal("report_type")),
Schedule = reader.GetString(reader.GetOrdinal("schedule")),
EmailRecipients = reader.IsDBNull(reader.GetOrdinal("email_recipients")) ? null : reader.GetString(reader.GetOrdinal("email_recipients")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
NextRunDate = reader.IsDBNull(reader.GetOrdinal("next_run_date")) ? null : reader.GetDateTime(reader.GetOrdinal("next_run_date")),
LastRunDate = reader.IsDBNull(reader.GetOrdinal("last_run_date")) ? null : reader.GetDateTime(reader.GetOrdinal("last_run_date")),
CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32(reader.GetOrdinal("created_by"))
```

## File: Module_Settings/Data/Dao_SystemSettings.cs

```csharp
public class Dao_SystemSettings
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_SystemSetting>>> GetAllAsync()
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result<List<Model_SystemSetting>>> GetByCategoryAsync(string category)
⋮----
public async Task<Model_Dao_Result<Model_SystemSetting>> GetByKeyAsync(string category, string settingKey)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result> UpdateValueAsync(
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> ResetToDefaultAsync(
⋮----
public async Task<Model_Dao_Result> SetLockedAsync(
⋮----
private static Model_SystemSetting MapFromReader(IDataReader reader)
⋮----
return new Model_SystemSetting
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
Category = reader.GetString(reader.GetOrdinal("category")),
SubCategory = reader.IsDBNull(reader.GetOrdinal("sub_category")) ? null : reader.GetString(reader.GetOrdinal("sub_category")),
SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
SettingName = reader.GetString(reader.GetOrdinal("setting_name")),
Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
SettingValue = reader.IsDBNull(reader.GetOrdinal("setting_value")) ? null : reader.GetString(reader.GetOrdinal("setting_value")),
DefaultValue = reader.IsDBNull(reader.GetOrdinal("default_value")) ? null : reader.GetString(reader.GetOrdinal("default_value")),
DataType = reader.GetString(reader.GetOrdinal("data_type")),
Scope = reader.GetString(reader.GetOrdinal("scope")),
PermissionLevel = reader.GetString(reader.GetOrdinal("permission_level")),
IsLocked = reader.GetBoolean(reader.GetOrdinal("is_locked")),
IsSensitive = reader.GetBoolean(reader.GetOrdinal("is_sensitive")),
ValidationRules = reader.IsDBNull(reader.GetOrdinal("validation_rules")) ? null : reader.GetString(reader.GetOrdinal("validation_rules")),
UiControlType = reader.GetString(reader.GetOrdinal("ui_control_type")),
UiOrder = reader.GetInt32(reader.GetOrdinal("ui_order")),
CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
UpdatedBy = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetInt32(reader.GetOrdinal("updated_by"))
```

## File: Module_Settings/Data/Dao_UserSettings.cs

```csharp
public class Dao_UserSettings
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<Model_SettingValue>> GetAsync(int userId, string category, string settingKey)
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result<List<Model_UserSetting>>> GetAllForUserAsync(int userId)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result> SetAsync(int userId, int settingId, string value)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> ResetAsync(int userId, int settingId)
⋮----
public async Task<Model_Dao_Result<int>> ResetAllAsync(int userId, int changedBy)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("reset_count")),
⋮----
private static Model_SettingValue MapToSettingValue(IDataReader reader)
⋮----
return new Model_SettingValue
⋮----
RawValue = reader.IsDBNull(reader.GetOrdinal("effective_value"))
⋮----
: reader.GetString(reader.GetOrdinal("effective_value")),
DataType = reader.GetString(reader.GetOrdinal("data_type"))
⋮----
private static Model_UserSetting MapFromReader(IDataReader reader)
⋮----
return new Model_UserSetting
⋮----
Id = reader.GetInt32(reader.GetOrdinal("setting_id")),
⋮----
SettingId = reader.GetInt32(reader.GetOrdinal("setting_id")),
SettingValue = reader.IsDBNull(reader.GetOrdinal("user_override"))
⋮----
: reader.GetString(reader.GetOrdinal("user_override")),
SystemSetting = new Model_SystemSetting
⋮----
Category = reader.GetString(reader.GetOrdinal("category")),
SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
SettingName = reader.GetString(reader.GetOrdinal("setting_name")),
SettingValue = reader.IsDBNull(reader.GetOrdinal("system_default"))
⋮----
: reader.GetString(reader.GetOrdinal("system_default")),
DataType = reader.GetString(reader.GetOrdinal("data_type")),
UiControlType = reader.GetString(reader.GetOrdinal("ui_control_type"))
```

## File: Module_Settings/Models/Model_PackageType.cs

```csharp
public partial class Model_PackageType : ObservableObject
⋮----
private DateTime _createdAt;
⋮----
private DateTime _updatedAt;
```

## File: Module_Settings/Models/Model_PackageTypeMapping.cs

```csharp
public partial class Model_PackageTypeMapping : ObservableObject
⋮----
private DateTime _createdAt;
⋮----
private DateTime _updatedAt;
```

## File: Module_Settings/Models/Model_RoutingRule.cs

```csharp
public partial class Model_RoutingRule : ObservableObject
⋮----
private DateTime _createdAt;
⋮----
private DateTime _updatedAt;
```

## File: Module_Settings/Models/Model_ScheduledReport.cs

```csharp
public partial class Model_ScheduledReport : ObservableObject
⋮----
private DateTime _createdAt;
⋮----
private DateTime _updatedAt;
```

## File: Module_Settings/Models/Model_SettingsAuditLog.cs

```csharp
public partial class Model_SettingsAuditLog : ObservableObject
⋮----
private DateTime _changedAt;
```

## File: Module_Settings/Models/Model_SystemSetting.cs

```csharp
public partial class Model_SystemSetting : ObservableObject
⋮----
private DateTime _createdAt;
⋮----
private DateTime _updatedAt;
```

## File: Module_Settings/Models/Model_UserSetting.cs

```csharp
public partial class Model_UserSetting : ObservableObject
⋮----
private DateTime _createdAt;
⋮----
private DateTime _updatedAt;
```

## File: Module_Settings/Views/View_Settings_DatabaseTest.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Page x:Class="MTM_Receiving_Application.Module_Settings.Views.View_Settings_DatabaseTest"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      xmlns:icons="using:Material.Icons.WinUI3"
      xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.ViewModels"
      xmlns:models="using:MTM_Receiving_Application.Module_Settings.Models"
      mc:Ignorable="d"
      Background="{ThemeResource LayerFillColorDefaultBrush}">

    <Grid Background="{ThemeResource LayerFillColorDefaultBrush}">
        <ScrollViewer VerticalScrollBarVisibility="Auto">
            <StackPanel Spacing="20"
                        Margin="40,20,40,40">

                <!-- Header -->
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>

                    <StackPanel Grid.Column="0"
                                Spacing="4">
                        <TextBlock Text="Settings Database Test"
                                   Style="{StaticResource TitleTextBlockStyle}" />
                        <TextBlock Text="Development Tool - Schema &amp; Stored Procedure Validation"
                                   Style="{StaticResource CaptionTextBlockStyle}"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                    </StackPanel>

                    <Button Grid.Column="1"
                            Content="Refresh All Tests"
                            Command="{Binding RunAllTestsCommand}"
                            Style="{StaticResource AccentButtonStyle}"
                            Height="36"
                            Padding="20,0"
                            VerticalAlignment="Center">
                        <Button.KeyboardAccelerators>
                            <KeyboardAccelerator Key="F5" />
                        </Button.KeyboardAccelerators>
                    </Button>
                </Grid>

                <!-- Connection Status Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20">
                    <StackPanel Spacing="12">
                        <TextBlock Text="Database Connection"
                                   Style="{StaticResource SubtitleTextBlockStyle}" />

                        <!-- Success Status (use visible when connected) -->
                        <StackPanel Orientation="Horizontal"
                                    Spacing="12">
                            <Border Width="20"
                                    Height="20"
                                    CornerRadius="10"
                                    Background="#107c10">
                                <icons:MaterialIcon Kind="Check"
                                                    Width="14"
                                                    Height="14"
                                                    Foreground="White" />
                            </Border>
                            <TextBlock Text="{x:Bind ViewModel.ConnectionStatus, Mode=OneWay}"
                                       Foreground="#107c10"
                                       FontWeight="SemiBold" />
                        </StackPanel>

                        <TextBlock Text="Connection details"
                                   Style="{StaticResource CaptionTextBlockStyle}"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                    </StackPanel>
                </Border>

                <!-- Summary Cards Row -->
                <Grid ColumnSpacing="20">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>

                    <!-- Tables Card -->
                    <Border Grid.Column="0"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            Padding="20"
                            MinHeight="120">
                        <StackPanel Spacing="8">
                            <TextBlock Text="Tables"
                                       Style="{StaticResource BodyTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <StackPanel Orientation="Horizontal"
                                        Spacing="4">
                                <TextBlock Text="{x:Bind ViewModel.TablesValidated, Mode=OneWay}"
                                           FontSize="36"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10" />
                                <TextBlock Text="/"
                                           FontSize="28"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10"
                                           VerticalAlignment="Center" />
                                <TextBlock Text="{x:Bind ViewModel.TotalTables, Mode=OneWay}"
                                           FontSize="28"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10"
                                           VerticalAlignment="Center" />
                            </StackPanel>
                            <TextBlock Text="All tables validated"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <TextBlock Text="✓ Indexes OK"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="#107c10" />
                        </StackPanel>
                    </Border>

                    <!-- Stored Procedures Card -->
                    <Border Grid.Column="1"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            Padding="20"
                            MinHeight="120">
                        <StackPanel Spacing="8">
                            <TextBlock Text="Stored Procedures"
                                       Style="{StaticResource BodyTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <StackPanel Orientation="Horizontal"
                                        Spacing="4">
                                <TextBlock Text="{x:Bind ViewModel.StoredProceduresTested, Mode=OneWay}"
                                           FontSize="36"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10" />
                                <TextBlock Text="/"
                                           FontSize="28"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10"
                                           VerticalAlignment="Center" />
                                <TextBlock Text="{x:Bind ViewModel.TotalStoredProcedures, Mode=OneWay}"
                                           FontSize="28"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10"
                                           VerticalAlignment="Center" />
                            </StackPanel>
                            <TextBlock Text="All procedures tested"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <TextBlock Text="✓ No errors"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="#107c10" />
                        </StackPanel>
                    </Border>

                    <!-- Data Seeding Card -->
                    <Border Grid.Column="2"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            Padding="20"
                            MinHeight="120">
                        <StackPanel Spacing="8">
                            <TextBlock Text="Data Seeding"
                                       Style="{StaticResource BodyTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <TextBlock Text="{x:Bind ViewModel.SettingsSeeded, Mode=OneWay}"
                                       FontSize="36"
                                       FontWeight="SemiBold"
                                       Foreground="#107c10" />
                            <TextBlock Text="Settings inserted"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <TextBlock Text="✓ All categories"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="#107c10" />
                        </StackPanel>
                    </Border>

                    <!-- DAOs Card -->
                    <Border Grid.Column="3"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            Padding="20"
                            MinHeight="120">
                        <StackPanel Spacing="8">
                            <TextBlock Text="DAO Tests"
                                       Style="{StaticResource BodyTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <StackPanel Orientation="Horizontal"
                                        Spacing="4">
                                <TextBlock Text="{x:Bind ViewModel.DaosTested, Mode=OneWay}"
                                           FontSize="36"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10" />
                                <TextBlock Text="/"
                                           FontSize="28"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10"
                                           VerticalAlignment="Center" />
                                <TextBlock Text="{x:Bind ViewModel.TotalDaos, Mode=OneWay}"
                                           FontSize="28"
                                           FontWeight="SemiBold"
                                           Foreground="#107c10"
                                           VerticalAlignment="Center" />
                            </StackPanel>
                            <TextBlock Text="All DAOs operational"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <TextBlock Text="✓ CRUD verified"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="#107c10" />
                        </StackPanel>
                    </Border>
                </Grid>

                <!-- Test Results Tabs -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto" />
                            <RowDefinition Height="*" />
                        </Grid.RowDefinitions>

                        <!-- Tab Header -->
                        <Border Grid.Row="0"
                                BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                BorderThickness="0,0,0,1">
                            <StackPanel Orientation="Horizontal"
                                        Spacing="0"
                                        Height="48">
                                <Button Content="Schema Tests"
                                        x:Name="SchemaTabButton"
                                        MinWidth="150"
                                        Height="48"
                                        Background="Transparent"
                                        BorderThickness="0"
                                        CornerRadius="0"
                                        Click="OnSchemaTabClick" />
                                <Button Content="SP Tests"
                                        x:Name="SpTabButton"
                                        MinWidth="150"
                                        Height="48"
                                        Background="Transparent"
                                        BorderThickness="0"
                                        CornerRadius="0"
                                        Click="OnStoredProceduresTabClick" />
                                <Button Content="DAO Tests"
                                        x:Name="DaoTabButton"
                                        MinWidth="150"
                                        Height="48"
                                        Background="Transparent"
                                        BorderThickness="0"
                                        CornerRadius="0"
                                        Click="OnDaosTabClick" />
                                <Button Content="Logs"
                                        x:Name="LogsTabButton"
                                        MinWidth="150"
                                        Height="48"
                                        Background="Transparent"
                                        BorderThickness="0"
                                        CornerRadius="0"
                                        Click="OnLogsTabClick" />
                            </StackPanel>
                        </Border>

                        <!-- Tab Content -->
                        <ScrollViewer Grid.Row="1"
                                      MaxHeight="400"
                                      Padding="20">
                            <Grid>
                                <!-- Schema Tab -->
                                <StackPanel x:Name="SchemaTabContent"
                                            Spacing="16">
                                    <TextBlock Text="Table Validation Results"
                                               Style="{StaticResource SubtitleTextBlockStyle}" />

                                    <!-- Table Result Items -->
                                    <ItemsControl ItemsSource="{x:Bind ViewModel.TableResults, Mode=OneWay}">
                                        <ItemsControl.ItemTemplate>
                                            <DataTemplate x:DataType="models:Model_TableTestResult">
                                                <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                        BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                        BorderThickness="0,0,0,1"
                                                        Padding="12,8"
                                                        Margin="0,0,0,12">
                                                    <Grid>
                                                        <Grid.ColumnDefinitions>
                                                            <ColumnDefinition Width="Auto" />
                                                            <ColumnDefinition Width="*" />
                                                            <ColumnDefinition Width="Auto" />
                                                        </Grid.ColumnDefinitions>

                                                        <StackPanel Grid.Column="0"
                                                                    Orientation="Horizontal"
                                                                    Spacing="12"
                                                                    VerticalAlignment="Center">
                                                            <Border Width="20"
                                                                    Height="20"
                                                                    CornerRadius="10"
                                                                    Background="#107c10">
                                                                <icons:MaterialIcon Kind="Check"
                                                                                    Width="14"
                                                                                    Height="14"
                                                                                    Foreground="White" />
                                                            </Border>
                                                            <StackPanel Spacing="2">
                                                                <TextBlock Text="{x:Bind TableName}"
                                                                           FontWeight="SemiBold" />
                                                                <TextBlock Text="{x:Bind Details}"
                                                                           Style="{StaticResource CaptionTextBlockStyle}"
                                                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                                                            </StackPanel>
                                                        </StackPanel>

                                                        <Border Grid.Column="2"
                                                                BorderBrush="#107c10"
                                                                BorderThickness="1"
                                                                CornerRadius="4"
                                                                Padding="12,4">
                                                            <TextBlock Text="{x:Bind StatusText}"
                                                                       Style="{StaticResource CaptionTextBlockStyle}"
                                                                       Foreground="#107c10" />
                                                        </Border>
                                                    </Grid>
                                                </Border>
                                            </DataTemplate>
                                        </ItemsControl.ItemTemplate>
                                    </ItemsControl>

                                    <!-- Legacy hardcoded items removed -->
                                    <StackPanel Spacing="12"
                                                Visibility="Collapsed">

                                        <!-- Table 1: settings_universal -->
                                        <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                BorderThickness="0,0,0,1"
                                                Padding="12,8">
                                            <Grid>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width="Auto" />
                                                    <ColumnDefinition Width="*" />
                                                    <ColumnDefinition Width="Auto" />
                                                </Grid.ColumnDefinitions>

                                                <StackPanel Grid.Column="0"
                                                            Orientation="Horizontal"
                                                            Spacing="12"
                                                            VerticalAlignment="Center">
                                                    <Border Width="20"
                                                            Height="20"
                                                            CornerRadius="10"
                                                            Background="#107c10">
                                                        <icons:MaterialIcon Kind="Check"
                                                                            Width="14"
                                                                            Height="14"
                                                                            Foreground="White" />
                                                    </Border>
                                                    <StackPanel Spacing="2">
                                                        <TextBlock Text="settings_universal"
                                                                   FontWeight="SemiBold" />
                                                        <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
                                                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                                                            <Run Text="79 rows | 18 columns | 3 indexes" />
                                                        </TextBlock>
                                                    </StackPanel>
                                                </StackPanel>

                                                <Border Grid.Column="2"
                                                        BorderBrush="#107c10"
                                                        BorderThickness="1"
                                                        CornerRadius="4"
                                                        Padding="12,4">
                                                    <TextBlock Text="Validated"
                                                               Style="{StaticResource CaptionTextBlockStyle}"
                                                               Foreground="#107c10" />
                                                </Border>
                                            </Grid>
                                        </Border>

                                        <!-- Table 2: settings_personal -->
                                        <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                BorderThickness="0,0,0,1"
                                                Padding="12,8">
                                            <Grid>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width="Auto" />
                                                    <ColumnDefinition Width="*" />
                                                    <ColumnDefinition Width="Auto" />
                                                </Grid.ColumnDefinitions>

                                                <StackPanel Grid.Column="0"
                                                            Orientation="Horizontal"
                                                            Spacing="12"
                                                            VerticalAlignment="Center">
                                                    <Border Width="20"
                                                            Height="20"
                                                            CornerRadius="10"
                                                            Background="#107c10">
                                                        <icons:MaterialIcon Kind="Check"
                                                                            Width="14"
                                                                            Height="14"
                                                                            Foreground="White" />
                                                    </Border>
                                                    <StackPanel Spacing="2">
                                                        <TextBlock Text="settings_personal"
                                                                   FontWeight="SemiBold" />
                                                        <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
                                                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                                                            <Run Text="0 rows | 5 columns | 1 unique constraint" />
                                                        </TextBlock>
                                                    </StackPanel>
                                                </StackPanel>

                                                <Border Grid.Column="2"
                                                        BorderBrush="#107c10"
                                                        BorderThickness="1"
                                                        CornerRadius="4"
                                                        Padding="12,4">
                                                    <TextBlock Text="Validated"
                                                               Style="{StaticResource CaptionTextBlockStyle}"
                                                               Foreground="#107c10" />
                                                </Border>
                                            </Grid>
                                        </Border>

                                        <!-- Table 3: dunnage_types -->
                                        <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                BorderThickness="0,0,0,1"
                                                Padding="12,8">
                                            <Grid>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width="Auto" />
                                                    <ColumnDefinition Width="*" />
                                                    <ColumnDefinition Width="Auto" />
                                                </Grid.ColumnDefinitions>

                                                <StackPanel Grid.Column="0"
                                                            Orientation="Horizontal"
                                                            Spacing="12"
                                                            VerticalAlignment="Center">
                                                    <Border Width="20"
                                                            Height="20"
                                                            CornerRadius="10"
                                                            Background="#107c10">
                                                        <icons:MaterialIcon Kind="Check"
                                                                            Width="14"
                                                                            Height="14"
                                                                            Foreground="White" />
                                                    </Border>
                                                    <StackPanel Spacing="2">
                                                        <TextBlock Text="dunnage_types"
                                                                   FontWeight="SemiBold" />
                                                        <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
                                                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                                                            <Run Text="5 rows | 5 columns | 2 unique constraints" />
                                                        </TextBlock>
                                                    </StackPanel>
                                                </StackPanel>

                                                <Border Grid.Column="2"
                                                        BorderBrush="#107c10"
                                                        BorderThickness="1"
                                                        CornerRadius="4"
                                                        Padding="12,4">
                                                    <TextBlock Text="Validated"
                                                               Style="{StaticResource CaptionTextBlockStyle}"
                                                               Foreground="#107c10" />
                                                </Border>
                                            </Grid>
                                        </Border>

                                        <!-- Table 4: routing_home_locations -->
                                        <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                BorderThickness="0,0,0,1"
                                                Padding="12,8">
                                            <Grid>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width="Auto" />
                                                    <ColumnDefinition Width="*" />
                                                    <ColumnDefinition Width="Auto" />
                                                </Grid.ColumnDefinitions>

                                                <StackPanel Grid.Column="0"
                                                            Orientation="Horizontal"
                                                            Spacing="12"
                                                            VerticalAlignment="Center">
                                                    <Border Width="20"
                                                            Height="20"
                                                            CornerRadius="10"
                                                            Background="#107c10">
                                                        <icons:MaterialIcon Kind="Check"
                                                                            Width="14"
                                                                            Height="14"
                                                                            Foreground="White" />
                                                    </Border>
                                                    <StackPanel Spacing="2">
                                                        <TextBlock Text="routing_home_locations"
                                                                   FontWeight="SemiBold" />
                                                        <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
                                                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                                                            <Run Text="3 rows | 7 columns | 1 index on priority" />
                                                        </TextBlock>
                                                    </StackPanel>
                                                </StackPanel>

                                                <Border Grid.Column="2"
                                                        BorderBrush="#107c10"
                                                        BorderThickness="1"
                                                        CornerRadius="4"
                                                        Padding="12,4">
                                                    <TextBlock Text="Validated"
                                                               Style="{StaticResource CaptionTextBlockStyle}"
                                                               Foreground="#107c10" />
                                                </Border>
                                            </Grid>
                                        </Border>

                                        <!-- Table 5: reporting_scheduled_reports -->
                                        <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                BorderThickness="0,0,0,1"
                                                Padding="12,8">
                                            <Grid>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width="Auto" />
                                                    <ColumnDefinition Width="*" />
                                                    <ColumnDefinition Width="Auto" />
                                                </Grid.ColumnDefinitions>

                                                <StackPanel Grid.Column="0"
                                                            Orientation="Horizontal"
                                                            Spacing="12"
                                                            VerticalAlignment="Center">
                                                    <Border Width="20"
                                                            Height="20"
                                                            CornerRadius="10"
                                                            Background="#107c10">
                                                        <icons:MaterialIcon Kind="Check"
                                                                            Width="14"
                                                                            Height="14"
                                                                            Foreground="White" />
                                                    </Border>
                                                    <StackPanel Spacing="2">
                                                        <TextBlock Text="reporting_scheduled_reports"
                                                                   FontWeight="SemiBold" />
                                                        <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
                                                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                                                            <Run Text="2 rows | 8 columns | 2 indexes" />
                                                        </TextBlock>
                                                    </StackPanel>
                                                </StackPanel>

                                                <Border Grid.Column="2"
                                                        BorderBrush="#107c10"
                                                        BorderThickness="1"
                                                        CornerRadius="4"
                                                        Padding="12,4">
                                                    <TextBlock Text="Validated"
                                                               Style="{StaticResource CaptionTextBlockStyle}"
                                                               Foreground="#107c10" />
                                                </Border>
                                            </Grid>
                                        </Border>

                                    </StackPanel>
                                    <!-- End of collapsed hardcoded items -->

                                    <!-- Show All Tables Button -->
                                    <Button Content="Show All Tables (7)"
                                            HorizontalAlignment="Left"
                                            Margin="0,8,0,0"
                                            Background="Transparent"
                                            BorderBrush="{ThemeResource ControlStrokeColorDefaultBrush}"
                                            BorderThickness="1" />
                                </StackPanel>

                                <!-- Stored Procedures Tab -->
                                <StackPanel x:Name="StoredProceduresTabContent"
                                            Spacing="16"
                                            Visibility="Collapsed">
                                    <TextBlock Text="Stored Procedure Results"
                                               Style="{StaticResource SubtitleTextBlockStyle}" />

                                    <ItemsControl ItemsSource="{x:Bind ViewModel.StoredProcedureResults, Mode=OneWay}">
                                        <ItemsControl.ItemTemplate>
                                            <DataTemplate x:DataType="models:Model_StoredProcedureTestResult">
                                                <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                        BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                        BorderThickness="0,0,0,1"
                                                        Padding="12,8"
                                                        Margin="0,0,0,12">
                                                    <Grid>
                                                        <Grid.ColumnDefinitions>
                                                            <ColumnDefinition Width="*" />
                                                            <ColumnDefinition Width="Auto" />
                                                        </Grid.ColumnDefinitions>

                                                        <StackPanel Spacing="2">
                                                            <TextBlock Text="{x:Bind ProcedureName}"
                                                                       FontWeight="SemiBold" />
                                                            <TextBlock Text="{x:Bind TestDetails}"
                                                                       Style="{StaticResource CaptionTextBlockStyle}"
                                                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                                                        </StackPanel>

                                                        <TextBlock Grid.Column="1"
                                                                   Text="{x:Bind ExecutionTimeMs}"
                                                                   Style="{StaticResource CaptionTextBlockStyle}"
                                                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                                                   VerticalAlignment="Center" />
                                                    </Grid>
                                                </Border>
                                            </DataTemplate>
                                        </ItemsControl.ItemTemplate>
                                    </ItemsControl>
                                </StackPanel>

                                <!-- DAOs Tab -->
                                <StackPanel x:Name="DaosTabContent"
                                            Spacing="16"
                                            Visibility="Collapsed">
                                    <TextBlock Text="DAO Results"
                                               Style="{StaticResource SubtitleTextBlockStyle}" />

                                    <ItemsControl ItemsSource="{x:Bind ViewModel.DaoResults, Mode=OneWay}">
                                        <ItemsControl.ItemTemplate>
                                            <DataTemplate x:DataType="models:Model_DaoTestResult">
                                                <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                        BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                        BorderThickness="0,0,0,1"
                                                        Padding="12,8"
                                                        Margin="0,0,0,12">
                                                    <Grid>
                                                        <Grid.ColumnDefinitions>
                                                            <ColumnDefinition Width="*" />
                                                            <ColumnDefinition Width="Auto" />
                                                        </Grid.ColumnDefinitions>

                                                        <StackPanel Spacing="2">
                                                            <TextBlock Text="{x:Bind DaoName}"
                                                                       FontWeight="SemiBold" />
                                                            <TextBlock Text="{x:Bind StatusText}"
                                                                       Style="{StaticResource CaptionTextBlockStyle}"
                                                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                                                        </StackPanel>

                                                        <TextBlock Grid.Column="1"
                                                                   Text="{x:Bind OperationsTested}"
                                                                   Style="{StaticResource CaptionTextBlockStyle}"
                                                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                                                   VerticalAlignment="Center" />
                                                    </Grid>
                                                </Border>
                                            </DataTemplate>
                                        </ItemsControl.ItemTemplate>
                                    </ItemsControl>
                                </StackPanel>

                                <!-- Logs Tab -->
                                <StackPanel x:Name="LogsTabContent"
                                            Spacing="16"
                                            Visibility="Collapsed">
                                    <StackPanel Orientation="Horizontal"
                                                Spacing="12">
                                        <TextBlock Text="Logs"
                                                   Style="{StaticResource SubtitleTextBlockStyle}" />
                                        <Button Content="Load Audit Log"
                                                Command="{x:Bind ViewModel.LoadAuditLogCommand}" />
                                    </StackPanel>

                                    <TextBlock Text="Runtime Log Messages"
                                               Style="{StaticResource BodyTextBlockStyle}" />
                                    <ItemsControl ItemsSource="{x:Bind ViewModel.LogMessages, Mode=OneWay}">
                                        <ItemsControl.ItemTemplate>
                                            <DataTemplate x:DataType="x:String">
                                                <TextBlock Text="{x:Bind}"
                                                           Style="{StaticResource CaptionTextBlockStyle}"
                                                           TextWrapping="WrapWholeWords" />
                                            </DataTemplate>
                                        </ItemsControl.ItemTemplate>
                                    </ItemsControl>

                                    <TextBlock Text="Settings Audit Log (Sample)"
                                               Style="{StaticResource BodyTextBlockStyle}"
                                               Margin="0,12,0,0" />
                                    <ItemsControl ItemsSource="{x:Bind ViewModel.AuditLogEntries, Mode=OneWay}">
                                        <ItemsControl.ItemTemplate>
                                            <DataTemplate x:DataType="models:Model_SettingsAuditLog">
                                                <Border Background="{ThemeResource SubtleFillColorTransparentBrush}"
                                                        BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}"
                                                        BorderThickness="0,0,0,1"
                                                        Padding="12,8"
                                                        Margin="0,0,0,12">
                                                    <StackPanel Spacing="2">
                                                        <TextBlock Text="{x:Bind SettingName}"
                                                                   FontWeight="SemiBold" />
                                                        <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
                                                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                                                            <Run Text="{x:Bind ChangeType}" />
                                                            <Run Text="  " />
                                                            <Run Text="{x:Bind ChangedAt}" />
                                                        </TextBlock>
                                                    </StackPanel>
                                                </Border>
                                            </DataTemplate>
                                        </ItemsControl.ItemTemplate>
                                    </ItemsControl>
                                </StackPanel>
                            </Grid>
                        </ScrollViewer>
                    </Grid>
                </Border>

                <!-- Footer -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*" />
                            <ColumnDefinition Width="Auto" />
                        </Grid.ColumnDefinitions>

                        <StackPanel Grid.Column="0"
                                    Spacing="4">
                            <TextBlock Text="Last run: Not yet executed"
                                       Style="{StaticResource CaptionTextBlockStyle}"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            <StackPanel Orientation="Horizontal"
                                        Spacing="4">
                                <TextBlock Text="Total test duration:"
                                           Style="{StaticResource CaptionTextBlockStyle}"
                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                                <TextBlock Text="{x:Bind ViewModel.TotalTestDurationMs, Mode=OneWay}"
                                           Style="{StaticResource CaptionTextBlockStyle}"
                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                                <TextBlock Text="ms"
                                           Style="{StaticResource CaptionTextBlockStyle}"
                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                            </StackPanel>
                        </StackPanel>

                        <StackPanel Grid.Column="1"
                                    Orientation="Horizontal"
                                    Spacing="12">
                            <Button Content="Export Results"
                                    Command="{x:Bind ViewModel.ExportResultsCommand}"
                                    Height="32"
                                    Padding="16,0"
                                    Background="Transparent"
                                    BorderBrush="{ThemeResource ControlStrokeColorDefaultBrush}"
                                    BorderThickness="1" />
                            <Button Content="Close"
                                    Height="32"
                                    Padding="16,0"
                                    Background="Transparent"
                                    BorderBrush="{ThemeResource ControlStrokeColorDefaultBrush}"
                                    BorderThickness="1" />
                        </StackPanel>
                    </Grid>
                </Border>

            </StackPanel>
        </ScrollViewer>
    </Grid>
</Page>
```

## File: Module_Volvo/Models/Model_VolvoPart.cs

```csharp
public class Model_VolvoPart
```

## File: Module_Volvo/Models/Model_VolvoSetting.cs

```csharp
public partial class Model_VolvoSetting : ObservableObject
⋮----
private DateTime _modifiedDate = DateTime.Now;
```

## File: Module_Volvo/Services/Service_VolvoAuthorization.cs

```csharp
public class Service_VolvoAuthorization : IService_VolvoAuthorization
⋮----
private readonly IService_LoggingUtility _logger;
⋮----
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result> CanManageShipmentsAsync()
⋮----
await _logger.LogInfoAsync("Authorization check: CanManageShipments");
return new Model_Dao_Result
⋮----
await _logger.LogErrorAsync($"Error checking shipment management authorization: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> CanManageMasterDataAsync()
⋮----
await _logger.LogInfoAsync("Authorization check: CanManageMasterData");
⋮----
await _logger.LogErrorAsync($"Error checking master data authorization: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> CanCompleteShipmentsAsync()
⋮----
await _logger.LogInfoAsync("Authorization check: CanCompleteShipments");
⋮----
await _logger.LogErrorAsync($"Error checking shipment completion authorization: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> CanGenerateLabelsAsync()
⋮----
await _logger.LogInfoAsync("Authorization check: CanGenerateLabels");
⋮----
await _logger.LogErrorAsync($"Error checking label generation authorization: {ex.Message}", ex);
```

## File: Module_Volvo/Views/View_Volvo_History.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Page
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.View_Volvo_History"
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
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="12">
            <Button 
                Command="{x:Bind ViewModel.GoBackCommand}"
                VerticalAlignment="Center"
                ToolTipService.ToolTip="Back to Shipment Entry">
                <SymbolIcon Symbol="Back"/>
            </Button>
            <TextBlock 
                Text="Volvo Shipment History"
                Style="{StaticResource TitleTextBlockStyle}"
                VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Filters -->
        <StackPanel Grid.Row="1" Orientation="Horizontal" Spacing="16">
            <CalendarDatePicker
                Header="Start Date"
                Date="{x:Bind ViewModel.StartDate, Mode=TwoWay}"
                Width="200"/>
            
            <CalendarDatePicker
                Header="End Date"
                Date="{x:Bind ViewModel.EndDate, Mode=TwoWay}"
                Width="200"/>
            
            <ComboBox
                Header="Status"
                ItemsSource="{x:Bind ViewModel.StatusOptions}"
                SelectedItem="{x:Bind ViewModel.StatusFilter, Mode=TwoWay}"
                Width="150"/>
            
            <Button
                Content="Filter"
                Command="{x:Bind ViewModel.FilterCommand}"
                Style="{StaticResource AccentButtonStyle}"
                VerticalAlignment="Bottom"/>
            
            <Button
                Content="Export CSV"
                Command="{x:Bind ViewModel.ExportCommand}"
                VerticalAlignment="Bottom"/>
        </StackPanel>

        <!-- DataGrid -->
        <controls:DataGrid
            Grid.Row="2"
            ItemsSource="{x:Bind ViewModel.History, Mode=OneWay}"
            SelectedItem="{x:Bind ViewModel.SelectedShipment, Mode=TwoWay}"
            AutoGenerateColumns="False"
            IsReadOnly="True"
            CanUserReorderColumns="False"
            GridLinesVisibility="Horizontal">
            
            <controls:DataGrid.Columns>
                <controls:DataGridTextColumn 
                    Header="Shipment #" 
                    Binding="{Binding ShipmentNumber}"
                    Width="100"/>
                
                <controls:DataGridTextColumn 
                    Header="Date" 
                    Binding="{Binding ShipmentDate}"
                    Width="120"/>
                
                <controls:DataGridTextColumn 
                    Header="PO Number" 
                    Binding="{Binding PONumber}"
                    Width="120"/>
                
                <controls:DataGridTextColumn 
                    Header="Receiver" 
                    Binding="{Binding ReceiverNumber}"
                    Width="120"/>
                
                <controls:DataGridTextColumn 
                    Header="Status" 
                    Binding="{Binding Status}"
                    Width="120"/>
                
                <controls:DataGridTextColumn 
                    Header="Notes" 
                    Binding="{Binding Notes}"
                    Width="*"/>
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
            <Button
                Content="View Details"
                Command="{x:Bind ViewModel.ViewDetailCommand}"
                Margin="12,0,0,0"/>
            <Button
                Content="Edit"
                Command="{x:Bind ViewModel.EditCommand}"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Volvo/Views/View_Volvo_Settings.xaml

```xml
<Page
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.View_Volvo_Settings"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:models="using:MTM_Receiving_Application.Module_Volvo.Models"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    mc:Ignorable="d"
    Loaded="OnPageLoaded">

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
            Text="Volvo Dunnage Parts Master Data"
            Style="{StaticResource TitleTextBlockStyle}"/>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right">
            <AppBarButton 
                Icon="Add" 
                Label="Add Part" 
                Command="{x:Bind ViewModel.AddPartCommand, Mode=OneWay}"/>
            <AppBarButton 
                Icon="Edit" 
                Label="Edit Part" 
                Command="{x:Bind ViewModel.EditPartCommand, Mode=OneWay}"/>
            <AppBarButton 
                Icon="Delete" 
                Label="Deactivate" 
                Command="{x:Bind ViewModel.DeactivatePartCommand, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="PreviewLink" 
                Label="View Components" 
                Command="{x:Bind ViewModel.ViewComponentsCommand, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Import" 
                Label="Import CSV" 
                Command="{x:Bind ViewModel.ImportCsvCommand, Mode=OneWay}"/>
            <AppBarButton 
                Icon="Save" 
                Label="Export CSV" 
                Command="{x:Bind ViewModel.ExportCsvCommand, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Refresh" 
                Label="Refresh" 
                Command="{x:Bind ViewModel.RefreshCommand, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarToggleButton 
                Icon="View" 
                Label="Show Inactive"
                IsChecked="{x:Bind ViewModel.ShowInactive, Mode=TwoWay}"/>
        </CommandBar>

        <!-- Data Grid -->
        <Grid Grid.Row="2">
            <controls:DataGrid
                ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
                SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                AutoGenerateColumns="False"
                IsReadOnly="True"
                GridLinesVisibility="All"
                HeadersVisibility="Column"
                AlternatingRowBackground="{ThemeResource SystemListLowColor}"
                SelectionMode="Single">
                <controls:DataGrid.Columns>
                    <controls:DataGridTextColumn Header="Part Number" Binding="{Binding PartNumber}" Width="*"/>
                    <controls:DataGridTextColumn Header="Qty/Skid" Binding="{Binding QuantityPerSkid}" Width="100"/>
                    <controls:DataGridTemplateColumn Header="Active" Width="80">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoPart">
                                <TextBlock 
                                    Text="{x:Bind IsActive}" 
                                    Foreground="{x:Bind IsActive, Converter={StaticResource BoolToColorConverter}, ConverterParameter='Green|Gray'}"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                </controls:DataGrid.Columns>
            </controls:DataGrid>
        </Grid>

        <!-- Status Bar -->
        <Grid Grid.Row="3" ColumnSpacing="12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <ProgressRing 
                Grid.Column="0"
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"
                Visibility="{x:Bind ViewModel.IsBusy, Mode=OneWay}"/>

            <TextBlock 
                Grid.Column="1"
                Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                VerticalAlignment="Center"/>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml.cs

```csharp
public sealed partial class View_Volvo_ShipmentEntry : Page
⋮----
private async void OnLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.InitializeAsync();
⋮----
private void OnPartSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
⋮----
ViewModel.UpdatePartSuggestions(sender.Text);
⋮----
private void OnPartSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
⋮----
ViewModel.OnPartSuggestionChosen(selectedPart);
```

## File: Module_Volvo/Views/VolvoPartAddEditDialog.xaml.cs

```csharp
public sealed partial class VolvoPartAddEditDialog : ContentDialog
⋮----
public void InitializeForAdd()
⋮----
public void InitializeForEdit(Model_VolvoPart part)
⋮----
private void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
if (string.IsNullOrWhiteSpace(PartNumberTextBox.Text))
⋮----
Part = new Model_VolvoPart
⋮----
PartNumber = PartNumberTextBox.Text.Trim().ToUpperInvariant(),
```

## File: Module_Volvo/Views/VolvoShipmentEditDialog.xaml.cs

```csharp
public sealed partial class VolvoShipmentEditDialog : ContentDialog
⋮----
public void LoadShipment(Model_VolvoShipment shipment, ObservableCollection<Model_VolvoShipmentLine> lines, ObservableCollection<Model_VolvoPart> availableParts)
⋮----
ShipmentNumberBox.Text = shipment.ShipmentNumber.ToString();
⋮----
Lines.Clear();
⋮----
Lines.Add(line);
⋮----
public Model_VolvoShipment GetUpdatedShipment()
⋮----
Shipment.PONumber = string.IsNullOrWhiteSpace(PONumberBox.Text) ? null : PONumberBox.Text;
Shipment.ReceiverNumber = string.IsNullOrWhiteSpace(ReceiverNumberBox.Text) ? null : ReceiverNumberBox.Text;
Shipment.Notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? null : NotesBox.Text;
⋮----
public ObservableCollection<Model_VolvoShipmentLine> GetUpdatedLines()
⋮----
private void AddNewLine()
⋮----
Lines.Add(new Model_VolvoShipmentLine
⋮----
private void RemoveSelectedLine()
⋮----
Lines.Remove(selectedLine);
```

## File: MTM_Receiving_Application.Tests/GlobalUsings.cs

```csharp

```

## File: MTM_Receiving_Application.Tests/Module_Core/Converters/Converter_DecimalToInt_Tests.cs

```csharp
public class Converter_DecimalToInt_Tests
⋮----
private readonly Converter_DecimalToInt _sut;
⋮----
public void Convert_WithDecimalValue_ReturnsFormattedIntegerString(decimal input, string expected)
⋮----
object result = _sut.Convert(input, typeof(string), default(object)!, "en-US");
_ = result.Should().Be(expected);
⋮----
public void Convert_WithDoubleValue_ReturnsFormattedIntegerString(double input, string expected)
⋮----
public void Convert_WithFloatValue_ReturnsFormattedIntegerString(float input, string expected)
⋮----
public void Convert_WithNullValue_ReturnsEmptyString()
⋮----
object result = _sut.Convert(null!, typeof(string), default(object)!, "en-US");
_ = result.Should().Be(string.Empty);
⋮----
public void Convert_WithNonNumericValue_ReturnsValueToString(object input)
⋮----
_ = result.Should().Be(input.ToString());
⋮----
public void ConvertBack_WithValidString_ReturnsDecimal(string input, decimal expected)
⋮----
object result = _sut.ConvertBack(input, typeof(decimal), default(object)!, "en-US");
⋮----
public void ConvertBack_WithInvalidString_ReturnsZero(string? input)
⋮----
object result = _sut.ConvertBack(input!, typeof(decimal), default(object)!, "en-US");
_ = result.Should().Be(0m);
⋮----
public void ConvertBack_WithNonStringValue_ReturnsZero()
⋮----
object result = _sut.ConvertBack(42, typeof(decimal), default(object)!, "en-US");
```

## File: MTM_Receiving_Application.Tests/Module_Core/Models/Core/Model_Dao_Result_Factory_Tests.cs

```csharp
public class Model_Dao_Result_Factory_Tests
⋮----
public void Failure_CreatesFailedResult()
⋮----
var result = Model_Dao_Result_Factory.Failure("Error!");
result.Success.Should().BeFalse();
result.ErrorMessage.Should().Be("Error!");
result.Severity.Should().Be(Enum_ErrorSeverity.Error);
⋮----
public void Failure_WithNullMessage_UsesDefaultMessage()
⋮----
var result = Model_Dao_Result_Factory.Failure(null);
⋮----
result.ErrorMessage.Should().Be("Operation failed.");
⋮----
public void Failure_WithWhitespaceMessage_UsesDefaultMessage()
⋮----
var result = Model_Dao_Result_Factory.Failure("   ");
⋮----
public void Failure_WithException_CapturesException()
⋮----
var ex = new InvalidOperationException("Test ex");
var result = Model_Dao_Result_Factory.Failure("Error!", ex);
⋮----
result.Exception.Should().Be(ex);
⋮----
public void Success_CreatesSuccessResult()
⋮----
var result = Model_Dao_Result_Factory.Success(5);
result.Success.Should().BeTrue();
result.AffectedRows.Should().Be(5);
result.ErrorMessage.Should().BeEmpty();
⋮----
public void Success_WithNegativeAffectedRows_ClampsToZero()
⋮----
var result = Model_Dao_Result_Factory.Success(-1);
⋮----
result.AffectedRows.Should().Be(0);
⋮----
public void FailureGeneric_CreatesFailedResult()
⋮----
result.ErrorMessage.Should().Be("Generic Error!");
result.Data.Should().BeNull();
⋮----
public void FailureGeneric_WithNullMessage_UsesDefaultMessage()
⋮----
public void SuccessGeneric_CreatesSuccessResultWithData()
⋮----
var result = Model_Dao_Result_Factory.Success("Test Data", 1);
⋮----
result.Data.Should().Be("Test Data");
result.AffectedRows.Should().Be(1);
⋮----
public void SuccessGeneric_WithNegativeAffectedRows_ClampsToZero()
⋮----
var result = Model_Dao_Result_Factory.Success("Test Data", -1);
```

## File: MTM_Receiving_Application.Tests/Module_Core/Models/Systems/Model_UserSession_Tests.cs

```csharp
public class Model_UserSession_Tests
⋮----
public void DefaultConstructor_SetsTimestamps()
⋮----
var session = new Model_UserSession();
⋮----
session.LoginTimestamp.Should().BeOnOrAfter(before);
session.LoginTimestamp.Should().BeOnOrBefore(after);
session.LastActivityTimestamp.Should().BeOnOrAfter(before);
session.LastActivityTimestamp.Should().BeOnOrBefore(after);
⋮----
public void UpdateLastActivity_UpdatesTimestamp()
⋮----
session.LastActivityTimestamp = DateTime.Now.AddMinutes(-1);
session.UpdateLastActivity();
session.LastActivityTimestamp.Should().BeAfter(DateTime.Now.AddSeconds(-5));
⋮----
public void TimeSinceLastActivity_ReturnsCorrectDifference()
⋮----
session.LastActivityTimestamp = DateTime.Now.AddMinutes(-5);
⋮----
elapsed.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(2));
⋮----
public void IsTimedOut_ReturnsTrueWhenExpired()
⋮----
var session = new Model_UserSession
⋮----
TimeoutDuration = TimeSpan.FromMinutes(10),
LastActivityTimestamp = DateTime.Now.AddMinutes(-11)
⋮----
isTimedOut.Should().BeTrue();
⋮----
public void IsTimedOut_ReturnsFalseWhenActive()
⋮----
LastActivityTimestamp = DateTime.Now.AddMinutes(-5)
⋮----
isTimedOut.Should().BeFalse();
⋮----
public void HasErpAccess_ReturnsFalseIfUserIsNull()
⋮----
var session = new Model_UserSession { User = null };
⋮----
hasAccess.Should().BeFalse();
⋮----
public void HasErpAccess_ReturnsFalseIfUserHasNoVisualUsername()
⋮----
User = new Model_User { VisualUsername = null }
⋮----
public void HasErpAccess_ReturnsTrueIfUserHasVisualUsername()
⋮----
User = new Model_User { VisualUsername = "user" }
⋮----
hasAccess.Should().BeTrue();
⋮----
public void Constructor_WithUser_SetsUser()
⋮----
var user = new Model_User();
var session = new Model_UserSession(user);
session.User.Should().BeSameAs(user);
⋮----
public void Constructor_WithNullUser_Throws()
⋮----
var act = static () => new Model_UserSession(null!);
act.Should().Throw<ArgumentNullException>();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Models/Systems/Model_WorkstationConfig_Tests.cs

```csharp
public class Model_WorkstationConfig_Tests
⋮----
public void Constructor_Default_UsesMachineName()
⋮----
var config = new Model_WorkstationConfig();
config.ComputerName.Should().Be(Environment.MachineName);
⋮----
public void Constructor_WithParameter_SetsComputerName()
⋮----
var config = new Model_WorkstationConfig("TEST-PC");
config.ComputerName.Should().Be("TEST-PC");
⋮----
public void IsSharedTerminal_ReturnsTrueForSharedType()
⋮----
var config = new Model_WorkstationConfig { WorkstationType = WorkstationDefaults.SharedTerminalWorkstationType };
config.IsSharedTerminal.Should().BeTrue();
⋮----
public void IsSharedTerminal_ReturnsFalseForSharedType()
⋮----
config.IsPersonalWorkstation.Should().BeFalse();
⋮----
public void IsPersonalWorkstation_ReturnsTrueForPersonalType()
⋮----
var config = new Model_WorkstationConfig { WorkstationType = WorkstationDefaults.PersonalWorkstationWorkstationType };
config.IsPersonalWorkstation.Should().BeTrue();
⋮----
public void IsPersonalWorkstation_ReturnsFalseForPersonalType()
⋮----
config.IsSharedTerminal.Should().BeFalse();
⋮----
public void TimeoutDuration_SharedTerminal_ReturnsSharedTimeoutMinutes()
⋮----
config.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(WorkstationDefaults.SharedTerminalTimeoutMinutes));
⋮----
public void TimeoutDuration_PersonalWorkstation_ReturnsPersonalTimeoutMinutes()
⋮----
config.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(WorkstationDefaults.PersonalWorkstationTimeoutMinutes));
⋮----
public void TimeoutDuration_UnknownType_ReturnsPersonalTimeoutMinutesDefault()
⋮----
var config = new Model_WorkstationConfig { WorkstationType = "unknown" };
```

## File: Module_Core/Models/Systems/Model_WorkstationConfig.cs

```csharp
public class Model_WorkstationConfig
⋮----
? TimeSpan.FromMinutes(WorkstationDefaults.SharedTerminalTimeoutMinutes)
: TimeSpan.FromMinutes(WorkstationDefaults.PersonalWorkstationTimeoutMinutes);
```

## File: Module_Core/Services/Authentication/Service_Authentication.cs

```csharp
public class Service_Authentication : IService_Authentication
⋮----
private readonly Dao_User _daoUser;
private readonly IService_ErrorHandler _errorHandler;
private static readonly Regex _regex = new Regex(@"^\d{4}$");
⋮----
_daoUser = daoUser ?? throw new ArgumentNullException(nameof(daoUser));
_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
⋮----
public async Task<Model_AuthenticationResult> AuthenticateByWindowsUsernameAsync(
⋮----
if (string.IsNullOrWhiteSpace(windowsUsername))
⋮----
return Model_AuthenticationResult.ErrorResult("Windows username is required");
⋮----
var result = await _daoUser.GetUserByWindowsUsernameAsync(windowsUsername);
⋮----
return Model_AuthenticationResult.ErrorResult(result.ErrorMessage);
⋮----
return Model_AuthenticationResult.ErrorResult("User data is invalid");
⋮----
return Model_AuthenticationResult.SuccessResult(result.Data);
⋮----
await _errorHandler.HandleErrorAsync("Authentication by Windows username failed", Enum_ErrorSeverity.Error, ex, false);
⋮----
return Model_AuthenticationResult.ErrorResult($"Authentication error: {ex.Message}");
⋮----
public async Task<Model_AuthenticationResult> AuthenticateByPinAsync(
⋮----
if (string.IsNullOrWhiteSpace(username))
⋮----
return Model_AuthenticationResult.ErrorResult("Username is required");
⋮----
if (string.IsNullOrWhiteSpace(pin))
⋮----
return Model_AuthenticationResult.ErrorResult("PIN is required");
⋮----
if (pin.Length != 4 || !pin.All(char.IsDigit))
⋮----
return Model_AuthenticationResult.ErrorResult("PIN must be exactly 4 numeric digits");
⋮----
var result = await _daoUser.ValidateUserPinAsync(username, pin);
⋮----
return Model_AuthenticationResult.ErrorResult("Invalid username or PIN");
⋮----
await _errorHandler.HandleErrorAsync("Authentication by PIN failed", Enum_ErrorSeverity.Error, ex, false);
⋮----
public async Task<Model_CreateUserResult> CreateNewUserAsync(
⋮----
if (string.IsNullOrWhiteSpace(user.WindowsUsername))
⋮----
return Model_CreateUserResult.ErrorResult("Windows username is required");
⋮----
if (string.IsNullOrWhiteSpace(user.FullName))
⋮----
return Model_CreateUserResult.ErrorResult("Full name is required");
⋮----
if (string.IsNullOrWhiteSpace(user.Department))
⋮----
return Model_CreateUserResult.ErrorResult("Department is required");
⋮----
if (string.IsNullOrWhiteSpace(user.Shift))
⋮----
return Model_CreateUserResult.ErrorResult("Shift is required");
⋮----
return Model_CreateUserResult.ErrorResult(pinValidation.ErrorMessage);
⋮----
var result = await _daoUser.CreateNewUserAsync(user, createdBy);
⋮----
return Model_CreateUserResult.ErrorResult(result.ErrorMessage);
⋮----
return Model_CreateUserResult.SuccessResult(result.Data);
⋮----
await _errorHandler.HandleErrorAsync("Create new user failed", Enum_ErrorSeverity.Error, ex, false);
⋮----
return Model_CreateUserResult.ErrorResult($"User creation error: {ex.Message}");
⋮----
public async Task<Model_ValidationResult> ValidatePinAsync(string pin, int? excludeEmployeeNumber = null)
⋮----
return Model_ValidationResult.Invalid("PIN is required");
⋮----
return Model_ValidationResult.Invalid("PIN must be exactly 4 digits");
⋮----
if (!_regex.IsMatch(pin))
⋮----
return Model_ValidationResult.Invalid("PIN must contain only numeric digits");
⋮----
return Model_ValidationResult.Valid();
⋮----
await _errorHandler.HandleErrorAsync("PIN validation failed", Enum_ErrorSeverity.Error, ex, false);
⋮----
return Model_ValidationResult.Invalid($"Validation error: {ex.Message}");
⋮----
public async Task<Model_WorkstationConfig> DetectWorkstationTypeAsync(string? computerName = null)
⋮----
computerName = computerName.Trim();
var sharedTerminalsResult = await _daoUser.GetSharedTerminalNamesAsync();
var config = new Model_WorkstationConfig(computerName);
⋮----
var isShared = sharedTerminalsResult.Data.Any(name =>
string.Equals(name?.Trim(), computerName, StringComparison.OrdinalIgnoreCase));
⋮----
await _daoUser.UpsertWorkstationConfigAsync(
⋮----
await _errorHandler.HandleErrorAsync("Workstation type detection failed", Enum_ErrorSeverity.Warning, ex, false);
⋮----
return new Model_WorkstationConfig(computerName ?? Environment.MachineName)
⋮----
public async Task<List<string>> GetActiveDepartmentsAsync()
⋮----
var result = await _daoUser.GetActiveDepartmentsAsync();
⋮----
await _errorHandler.HandleErrorAsync("Get active departments failed", Enum_ErrorSeverity.Warning, ex, false);
⋮----
public async Task LogUserActivityAsync(string eventType, string username, string workstationName, string details)
⋮----
await _daoUser.LogUserActivityAsync(eventType, username, workstationName, details);
⋮----
await _errorHandler.HandleErrorAsync("Activity logging failed", Enum_ErrorSeverity.Info, ex, false);
⋮----
System.Diagnostics.Debug.WriteLine($"Failed to log activity: {ex.Message}");
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminInventoryViewModel.cs

```csharp
public partial class ViewModel_Dunnage_AdminInventory : ViewModel_Shared_Base
⋮----
private readonly Dao_InventoriedDunnage _daoInventory;
private readonly Dao_DunnagePart _daoPart;
private readonly IService_DunnageAdminWorkflow _adminWorkflow;
private readonly IService_Window _windowService;
⋮----
_daoInventory = daoInventory ?? throw new ArgumentNullException(nameof(daoInventory));
_daoPart = daoPart ?? throw new ArgumentNullException(nameof(daoPart));
_adminWorkflow = adminWorkflow ?? throw new ArgumentNullException(nameof(adminWorkflow));
_windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
⋮----
public async Task InitializeAsync()
⋮----
private async Task LoadInventoriedPartsAsync()
⋮----
var result = await _daoInventory.GetAllAsync();
⋮----
InventoriedParts.Clear();
⋮----
InventoriedParts.Add(part);
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private async Task ShowAddToListAsync()
⋮----
var result = await dialog.ShowAsync();
⋮----
private async Task ShowEditEntryAsync()
⋮----
var dialog = new ContentDialog
⋮----
XamlRoot = _windowService.GetXamlRoot()
⋮----
var stackPanel = new StackPanel { Spacing = 12 };
stackPanel.Children.Add(new TextBlock { Text = "Part ID (read-only)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
var partIdBox = new TextBox
⋮----
stackPanel.Children.Add(partIdBox);
stackPanel.Children.Add(new TextBlock { Text = "Inventory Method", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
var methodCombo = new ComboBox
⋮----
stackPanel.Children.Add(methodCombo);
stackPanel.Children.Add(new TextBlock { Text = "Notes (optional)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
var notesBox = new TextBox
⋮----
stackPanel.Children.Add(notesBox);
⋮----
var dialogResult = await dialog.ShowAsync();
⋮----
var updateResult = await _daoInventory.UpdateAsync(
⋮----
private async Task ShowRemoveConfirmationAsync()
⋮----
private async Task RemoveFromListAsync()
⋮----
var result = await _daoInventory.DeleteAsync(SelectedInventoriedPart.Id);
⋮----
private async Task BackToHubAsync()
⋮----
_logger.LogInfo("Returning to Settings Mode Selection from Admin Inventory");
⋮----
settingsWorkflow.GoBack();
⋮----
partial void OnSelectedInventoriedPartChanged(Model_InventoriedDunnage? value)
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs

```csharp
public partial class ViewModel_Dunnage_ModeSelection : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Help _helpService;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_UserPreferences _userPreferencesService;
private readonly IService_Window _windowService;
⋮----
private void LoadDefaultMode()
⋮----
private async Task SelectGuidedModeAsync()
⋮----
_logger.LogInfo("User selected Guided Wizard mode");
⋮----
_workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
⋮----
private async Task SelectManualModeAsync()
⋮----
_logger.LogInfo("User selected Manual Entry mode");
⋮----
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ManualEntry);
⋮----
private async Task SelectEditModeAsync()
⋮----
_logger.LogInfo("User selected Edit Mode");
⋮----
_workflowService.GoToStep(Enum_DunnageWorkflowStep.EditMode);
⋮----
private bool HasUnsavedData()
⋮----
!string.IsNullOrEmpty(_workflowService.CurrentSession.PONumber) ||
!string.IsNullOrEmpty(_workflowService.CurrentSession.Location))
⋮----
private async Task<bool> ConfirmModeChangeAsync()
⋮----
_logger.LogInfo("No unsaved data detected, skipping confirmation dialog");
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogWarning("XamlRoot is null, proceeding without confirmation");
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogError($"Error showing confirmation dialog: {ex.Message}", ex);
⋮----
private void ClearWorkflowData()
⋮----
_logger.LogInfo("Dunnage workflow data and UI inputs cleared for mode change");
⋮----
_logger.LogError($"Error clearing dunnage workflow data: {ex.Message}", ex);
⋮----
private void ClearAllUIInputs()
⋮----
_logger.LogInfo("UI inputs cleared across all Dunnage ViewModels");
⋮----
_logger.LogError($"Error clearing UI inputs: {ex.Message}", ex);
⋮----
private async Task SetGuidedAsDefaultAsync(bool isChecked)
⋮----
var result = await _userPreferencesService.UpdateDefaultDunnageModeAsync(currentUser.WindowsUsername, newMode ?? "");
⋮----
// Update in-memory user object
⋮----
// Update UI state
⋮----
_logger.LogInfo($"Dunnage default mode set to: {newMode ?? "none"}");
⋮----
await _errorHandler.ShowErrorDialogAsync("Save Error", result.ErrorMessage, Enum_ErrorSeverity.Error);
⋮----
await _errorHandler.HandleErrorAsync($"Failed to set default dunnage mode: {ex.Message}",
⋮----
private async Task SetManualAsDefaultAsync(bool isChecked)
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Dunnage.ModeSelection");
⋮----
private async Task SetEditAsDefaultAsync(bool isChecked)
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs

```csharp
public partial class ViewModel_Dunnage_Review : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_MySQL_Dunnage _dunnageService; private readonly IService_Help _helpService; private readonly IService_DunnageCSVWriter _csvWriter;
private readonly IService_Window _windowService;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
public void LoadSessionLoads()
⋮----
SessionLoads.Clear();
⋮----
SessionLoads.Add(load);
⋮----
_logger.LogInfo($"Loaded {LoadCount} loads for review", "Review");
⋮----
_logger.LogError($"Error loading session loads: {ex.Message}", ex, "Review");
⋮----
private void UpdateNavigationButtons()
⋮----
private void SwitchToSingleView()
⋮----
_logger.LogInfo("Switched to Single View", "Review");
⋮----
private void SwitchToTableView()
⋮----
_logger.LogInfo("Switched to Table View", "Review");
⋮----
private void PreviousEntry()
⋮----
private void NextEntry()
⋮----
private async Task AddAnotherAsync()
⋮----
_logger.LogInfo("User requested to add another load", "Review");
⋮----
_logger.LogInfo("User cancelled add another load", "Review");
⋮----
var saveResult = await _workflowService.SaveToCSVOnlyAsync();
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
_workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
_logger.LogInfo("Navigated to Type Selection for new load, workflow data cleared", "Review");
⋮----
_logger.LogError($"Error in AddAnotherAsync: {ex.Message}", ex);
⋮----
private async Task<bool> ConfirmAddAnotherAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogWarning("XamlRoot is null, proceeding without confirmation", "Review");
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogError($"Error showing confirmation dialog: {ex.Message}", ex, "Review");
⋮----
private void ClearTransientWorkflowData()
⋮----
_logger.LogInfo("Transient workflow data and UI inputs cleared for new entry", "Review");
⋮----
_logger.LogError($"Error clearing transient workflow data: {ex.Message}", ex, "Review");
⋮----
private void ClearUIInputsForNewEntry()
⋮----
_logger.LogInfo("UI inputs cleared for new entry (loads preserved)", "Review");
⋮----
_logger.LogError($"Error clearing UI inputs: {ex.Message}", ex, "Review");
⋮----
private async Task SaveAllAsync()
⋮----
await _logger.LogInfoAsync($"Starting SaveAllAsync: {LoadCount} loads to save");
var saveResult = await _dunnageService.SaveLoadsAsync(SessionLoads.ToList());
⋮----
await _logger.LogErrorAsync($"Failed to save {LoadCount} loads: {saveResult.ErrorMessage}");
await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
⋮----
await _logger.LogInfoAsync($"Successfully saved {LoadCount} loads to database");
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(SessionLoads.ToList());
⋮----
await _logger.LogWarningAsync($"CSV export failed for {LoadCount} loads: {csvResult.ErrorMessage}");
⋮----
await _logger.LogInfoAsync($"Successfully exported {LoadCount} loads to CSV");
⋮----
await _logger.LogInfoAsync($"Completed SaveAllAsync: {LoadCount} loads processed successfully");
⋮----
await _logger.LogErrorAsync($"Exception in SaveAllAsync: {ex.Message}");
⋮----
private void StartNewEntry()
⋮----
_workflowService.ClearSession();
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Dunnage.Review");
⋮----
private void Cancel()
⋮----
_logger.LogInfo("Cancelling review, clearing session", "Review");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs

```csharp
public partial class ViewModel_Dunnage_WorkFlowViewModel : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Window _windowService;
⋮----
_currentLine = new Model_DunnageLine();
⋮----
private async Task InitializeWorkflowAsync()
⋮----
await _workflowService.StartWorkflowAsync();
⋮----
_errorHandler.HandleException(
⋮----
private Model_DunnageLine _currentLine;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
private async Task ResetCSVAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
⋮----
var result = await dialog.ShowAsync();
⋮----
var saveResult = await _workflowService.SaveToDatabaseOnlyAsync();
⋮----
var warnResult = await warnDialog.ShowAsync();
⋮----
var deleteResult = await _workflowService.ResetCSVFilesAsync();
⋮----
private void ReturnToModeSelection()
⋮----
_workflowService.ClearSession();
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
⋮----
private async Task AddLineAsync()
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs

```csharp
public partial class ViewModel_Receiving_POEntry : ViewModel_Shared_Base, IResettableViewModel
⋮----
private readonly IService_InforVisual _inforVisualService;
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_Help _helpService;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
_viewModelRegistry.Register(this);
⋮----
public void ResetToDefaults()
⋮----
Parts.Clear();
⋮----
private void PoTextBoxLostFocus()
⋮----
if (string.IsNullOrWhiteSpace(PoNumber))
⋮----
string value = PoNumber.Trim().ToUpper();
if (value.StartsWith("PO-", StringComparison.OrdinalIgnoreCase))
⋮----
string numberPart = value.Substring(3);
if (numberPart.All(char.IsDigit) && numberPart.Length <= 6)
⋮----
PoNumber = $"PO-{numberPart.PadLeft(6, '0')}";
⋮----
else if (value.All(char.IsDigit) && value.Length <= 6)
⋮----
PoNumber = $"PO-{value.PadLeft(6, '0')}";
⋮----
private async Task LoadPOAsync()
⋮----
await _errorHandler.HandleErrorAsync("Please enter a PO number.", Enum_ErrorSeverity.Warning);
⋮----
var result = await _inforVisualService.GetPOWithPartsAsync(PoNumber);
⋮----
var remainingQtyResult = await _inforVisualService.GetRemainingQuantityAsync(PoNumber, part.PartID);
⋮----
Parts.Add(part);
⋮----
_workflowService.RaiseStatusMessage($"Purchase Order {PoNumber} loaded with {Parts.Count} parts.");
⋮----
var errorMessage = !string.IsNullOrWhiteSpace(result.ErrorMessage)
⋮----
await _errorHandler.HandleErrorAsync(errorMessage, Enum_ErrorSeverity.Error);
⋮----
private void ToggleNonPO()
⋮----
private async Task LookupPartAsync()
⋮----
if (string.IsNullOrWhiteSpace(PartID))
⋮----
await _errorHandler.HandleErrorAsync("Please enter a Part ID.", Enum_ErrorSeverity.Warning);
⋮----
var result = await _inforVisualService.GetPartByIDAsync(PartID);
⋮----
Parts.Add(result.Data);
_workflowService.RaiseStatusMessage($"Part {PartID} found.");
⋮----
await _errorHandler.HandleErrorAsync(result.ErrorMessage ?? "Part not found.", Enum_ErrorSeverity.Error);
⋮----
partial void OnPoNumberChanged(string value)
⋮----
if (string.IsNullOrWhiteSpace(value))
⋮----
string validatedPO = value.Trim();
⋮----
if (validatedPO.StartsWith("po-", StringComparison.OrdinalIgnoreCase))
⋮----
string numberPart = validatedPO.Substring(3);
if (numberPart.All(char.IsDigit))
⋮----
validatedPO = $"PO-{numberPart.PadLeft(6, '0')}";
⋮----
else if (validatedPO.All(char.IsDigit))
⋮----
validatedPO = $"PO-{validatedPO.PadLeft(6, '0')}";
⋮----
partial void OnPartIDChanged(string value)
⋮----
var upperPart = value.Trim().ToUpper();
if (upperPart.StartsWith("MMC"))
⋮----
else if (upperPart.StartsWith("MMF"))
⋮----
partial void OnSelectedPartChanged(Model_InforVisualPart? value)
⋮----
if (value != null && !string.IsNullOrWhiteSpace(value.PartID))
⋮----
var upperPart = value.PartID.Trim().ToUpper();
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.POEntry");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
⋮----
private async void InitializeAsync()
⋮----
await _logger.LogInfoAsync($"[MOCK DATA MODE] Auto-filling PO number: {defaultPO}");
⋮----
_workflowService.RaiseStatusMessage($"[MOCK DATA] Auto-filled PO: {defaultPO}");
await Task.Delay(500);
⋮----
await _logger.LogErrorAsync($"Error during initialization: {ex.Message}", ex);
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

## File: Module_Routing/Views/RoutingEditModeView.xaml

```xml
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

## File: Module_Settings/Data/Dao_PackageType.cs

```csharp
public class Dao_PackageType
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_PackageType>>> GetAllAsync()
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result<Model_PackageType>> GetByIdAsync(int id)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result<int>> InsertAsync(Model_PackageType packageType, int createdBy)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("id")),
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_PackageType packageType)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
public async Task<Model_Dao_Result<int>> GetUsageCountAsync(int id)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("usage_count")),
⋮----
private static Model_PackageType MapFromReader(IDataReader reader)
⋮----
return new Model_PackageType
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
Name = reader.GetString(reader.GetOrdinal("name")),
Code = reader.GetString(reader.GetOrdinal("code")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32(reader.GetOrdinal("created_by"))
```

## File: Module_Settings/Data/Dao_RoutingRule.cs

```csharp
public class Dao_RoutingRule
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_RoutingRule>>> GetAllAsync()
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result<Model_RoutingRule>> GetByIdAsync(int id)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result<int>> InsertAsync(Model_RoutingRule rule, int createdBy)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("id")),
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_RoutingRule rule)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
public async Task<Model_Dao_Result<Model_RoutingRule>> FindMatchAsync(string matchType, string value)
⋮----
public async Task<Model_Dao_Result<Model_RoutingRule>> GetByPartNumberAsync(string partNumber)
⋮----
private static Model_RoutingRule MapFromReader(IDataReader reader)
⋮----
return new Model_RoutingRule
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
MatchType = reader.GetString(reader.GetOrdinal("match_type")),
Pattern = reader.GetString(reader.GetOrdinal("pattern")),
DestinationLocation = reader.GetString(reader.GetOrdinal("destination_location")),
Priority = reader.GetInt32(reader.GetOrdinal("priority")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32(reader.GetOrdinal("created_by"))
```

## File: Module_Settings/Data/Dao_SettingsAuditLog.cs

```csharp
public class Dao_SettingsAuditLog
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_SettingsAuditLog>>> GetAsync(int settingId, int limit)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result<List<Model_SettingsAuditLog>>> GetBySettingAsync(int settingId, int limit)
⋮----
public async Task<Model_Dao_Result<List<Model_SettingsAuditLog>>> GetByUserAsync(int changedBy, int limit)
⋮----
private static Model_SettingsAuditLog MapFromReader(IDataReader reader)
⋮----
return new Model_SettingsAuditLog
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
SettingId = reader.GetInt32(reader.GetOrdinal("setting_id")),
Category = reader.GetString(reader.GetOrdinal("category")),
SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
SettingName = reader.GetString(reader.GetOrdinal("setting_name")),
UserSettingId = reader.IsDBNull(reader.GetOrdinal("user_setting_id"))
⋮----
: reader.GetInt32(reader.GetOrdinal("user_setting_id")),
OldValue = reader.IsDBNull(reader.GetOrdinal("old_value"))
⋮----
: reader.GetString(reader.GetOrdinal("old_value")),
NewValue = reader.IsDBNull(reader.GetOrdinal("new_value"))
⋮----
: reader.GetString(reader.GetOrdinal("new_value")),
ChangeType = reader.GetString(reader.GetOrdinal("change_type")),
ChangedBy = reader.GetInt32(reader.GetOrdinal("changed_by")),
ChangedAt = reader.GetDateTime(reader.GetOrdinal("changed_at")),
IpAddress = reader.IsDBNull(reader.GetOrdinal("ip_address"))
⋮----
: reader.GetString(reader.GetOrdinal("ip_address")),
WorkstationName = reader.IsDBNull(reader.GetOrdinal("workstation_name"))
⋮----
: reader.GetString(reader.GetOrdinal("workstation_name"))
```

## File: Module_Volvo/Data/Dao_VolvoPartComponent.cs

```csharp
public class Dao_VolvoPartComponent
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetByParentPartAsync(string parentPartNumber)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result> InsertAsync(Model_VolvoPartComponent component)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> DeleteByParentPartAsync(string parentPartNumber)
⋮----
public async Task<Model_Dao_Result<Dictionary<string, List<Model_VolvoPartComponent>>>> GetComponentsByParentPartsAsync(
⋮----
private static Model_VolvoPartComponent MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoPartComponent
⋮----
ComponentPartNumber = reader.GetString(reader.GetOrdinal("component_part_number")),
Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
ComponentQuantityPerSkid = reader.GetInt32(reader.GetOrdinal("component_quantity_per_skid"))
```

## File: Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs

```csharp
public partial class ViewModel_Volvo_Settings : ViewModel_Shared_Base
⋮----
private readonly IService_VolvoMasterData _masterDataService;
⋮----
_masterDataService = masterDataService ?? throw new ArgumentNullException(nameof(masterDataService));
⋮----
private async Task RefreshAsync()
⋮----
var result = await _masterDataService.GetAllPartsAsync(ShowInactive);
⋮----
Parts.Clear();
foreach (var part in result.Data.OrderBy(p => p.PartNumber))
⋮----
Parts.Add(part);
⋮----
ActivePartsCount = Parts.Count(p => p.IsActive);
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private async Task AddPartAsync()
⋮----
dialog.InitializeForAdd();
⋮----
dialog.XamlRoot = windowService.GetXamlRoot();
⋮----
var result = await dialog.ShowAsync();
⋮----
var saveResult = await _masterDataService.AddPartAsync(dialog.Part, new System.Collections.Generic.List<Model_VolvoPartComponent>());
⋮----
private async Task EditPartAsync()
⋮----
dialog.InitializeForEdit(SelectedPart);
⋮----
var saveResult = await _masterDataService.UpdatePartAsync(dialog.Part, new System.Collections.Generic.List<Model_VolvoPartComponent>());
⋮----
private bool CanEditPart() => SelectedPart != null && !IsBusy;
⋮----
private async Task DeactivatePartAsync()
⋮----
var dialog = new ContentDialog
⋮----
var deactivateResult = await _masterDataService.DeactivatePartAsync(SelectedPart.PartNumber);
⋮----
private bool CanDeactivatePart() => SelectedPart?.IsActive == true && !IsBusy;
⋮----
private async Task ViewComponentsAsync()
⋮----
var result = await _masterDataService.GetComponentsAsync(SelectedPart.PartNumber);
⋮----
? string.Join("\n", result.Data.Select(c => $"• {c.ComponentPartNumber} (Qty: {c.Quantity})"))
⋮----
await dialog.ShowAsync();
⋮----
private bool CanViewComponents() => SelectedPart != null && !IsBusy;
⋮----
private async Task ImportCsvAsync()
⋮----
var picker = new FileOpenPicker();
picker.FileTypeFilter.Add(".csv");
⋮----
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
var file = await picker.PickSingleFileAsync();
⋮----
var csvContent = await FileIO.ReadTextAsync(file);
var result = await _masterDataService.ImportCsvAsync(csvContent);
⋮----
private async Task ExportCsvAsync()
⋮----
var picker = new FileSavePicker();
picker.FileTypeChoices.Add("CSV File", new[] { ".csv" });
⋮----
var file = await picker.PickSaveFileAsync();
⋮----
var result = await _masterDataService.ExportCsvAsync(file.Path, ShowInactive);
⋮----
await FileIO.WriteTextAsync(file, result.Data);
⋮----
partial void OnShowInactiveChanged(bool value)
⋮----
partial void OnSelectedPartChanged(Model_VolvoPart? value)
⋮----
EditPartCommand.NotifyCanExecuteChanged();
DeactivatePartCommand.NotifyCanExecuteChanged();
ViewComponentsCommand.NotifyCanExecuteChanged();
```

## File: MTM_Receiving_Application.Tests/Module_Core/Data/Authentication/Dao_User_Tests.cs

```csharp
public class Dao_User_Tests
⋮----
Helper_Database_Variables.GetConnectionString(useProduction: false);
⋮----
public void Constructor_ValidConnectionString_CreatesInstance()
⋮----
var dao = new Dao_User(TestConnectionString);
dao.Should().NotBeNull();
⋮----
public void Constructor_NullConnectionString_ThrowsArgumentNullException()
⋮----
Action act = () => new Dao_User(null!);
act.Should().Throw<ArgumentNullException>().WithMessage("*connectionString*");
⋮----
public void Constructor_EmptyConnectionString_DoesNotThrow()
⋮----
Action act = () => new Dao_User(string.Empty);
act.Should().NotThrow();
⋮----
public async Task GetUserByWindowsUsernameAsync_ValidUsername_CallsStoredProcedure_Async()
⋮----
var result = await dao.GetUserByWindowsUsernameAsync(username);
result.Should().NotBeNull();
⋮----
public async Task GetUserByWindowsUsernameAsync_VariousUsernames_AcceptsInput_Async(
⋮----
public async Task ValidateUserPinAsync_ValidCredentials_CallsStoredProcedure_Async()
⋮----
var result = await dao.ValidateUserPinAsync(username, pin);
⋮----
public async Task ValidateUserPinAsync_VariousInputs_AcceptsAllCombinations_Async(
⋮----
// Arrange
⋮----
// Act
⋮----
// Assert
⋮----
// ====================================================================
// CreateNewUserAsync Tests
⋮----
public async Task CreateNewUserAsync_ValidUser_ReturnsEmployeeNumber_Async()
⋮----
var result = await dao.CreateNewUserAsync(user, createdBy);
⋮----
public async Task CreateNewUserAsync_NullVisualCredentials_HandlesGracefully_Async()
⋮----
public async Task CreateNewUserAsync_WithVisualCredentials_PassesParameters_Async()
⋮----
public async Task IsWindowsUsernameUniqueAsync_WithoutExclusion_ChecksAllUsers_Async()
⋮----
var result = await dao.IsWindowsUsernameUniqueAsync(username);
⋮----
public async Task IsWindowsUsernameUniqueAsync_WithExclusion_ExcludesSpecificEmployee_Async()
⋮----
var result = await dao.IsWindowsUsernameUniqueAsync(username, excludeEmployeeNumber);
⋮----
public async Task IsWindowsUsernameUniqueAsync_VariousScenarios_HandlesAllCases_Async(
⋮----
var result = await dao.IsWindowsUsernameUniqueAsync(username, excludeId);
⋮----
// LogUserActivityAsync Tests
⋮----
public async Task LogUserActivityAsync_ValidData_LogsActivity_Async()
⋮----
var result = await dao.LogUserActivityAsync(eventType, username, workstation, details);
⋮----
public async Task LogUserActivityAsync_DifferentEventTypes_LogsCorrectly_Async(
⋮----
public async Task LogUserActivityAsync_NullParameters_HandlesGracefully_Async()
⋮----
var result = await dao.LogUserActivityAsync("event", null!, null!, null!);
⋮----
public async Task GetSharedTerminalNamesAsync_NoParameters_ReturnsTerminalList_Async()
⋮----
var result = await dao.GetSharedTerminalNamesAsync();
⋮----
public async Task UpsertWorkstationConfigAsync_ValidData_InsertsOrUpdates_Async()
⋮----
var result = await dao.UpsertWorkstationConfigAsync(
⋮----
public async Task UpsertWorkstationConfigAsync_NullDescription_HandlesGracefully_Async()
⋮----
var result = await dao.UpsertWorkstationConfigAsync("WS-01", "dedicated", true, null);
⋮----
public async Task UpsertWorkstationConfigAsync_VariousConfigurations_HandlesAll_Async(
⋮----
var result = await dao.UpsertWorkstationConfigAsync(name, type, active, description);
⋮----
public async Task GetActiveDepartmentsAsync_NoParameters_ReturnsDepartmentList_Async()
⋮----
var result = await dao.GetActiveDepartmentsAsync();
⋮----
public async Task UpdateDefaultModeAsync_ValidData_UpdatesMode_Async()
⋮----
var result = await dao.UpdateDefaultModeAsync(userId, defaultMode);
⋮----
public async Task UpdateDefaultModeAsync_NullMode_HandlesGracefully_Async()
⋮----
var result = await dao.UpdateDefaultModeAsync(userId, null);
⋮----
public async Task UpdateDefaultModeAsync_VariousInputs_HandlesAll_Async(int userId, string? mode)
⋮----
var result = await dao.UpdateDefaultModeAsync(userId, mode);
⋮----
public async Task UpdateDefaultReceivingModeAsync_ValidData_UpdatesMode_Async()
⋮----
var result = await dao.UpdateDefaultReceivingModeAsync(userId, defaultMode);
⋮----
public async Task UpdateDefaultReceivingModeAsync_DifferentModes_HandlesAll_Async(
⋮----
var result = await dao.UpdateDefaultReceivingModeAsync(userId, mode);
⋮----
public async Task UpdateDefaultDunnageModeAsync_ValidData_UpdatesMode_Async()
⋮----
var result = await dao.UpdateDefaultDunnageModeAsync(userId, defaultMode);
⋮----
public async Task UpdateDefaultDunnageModeAsync_VariousModes_HandlesAll_Async(
⋮----
var result = await dao.UpdateDefaultDunnageModeAsync(userId, mode);
⋮----
public async Task GetUserByWindowsUsernameAsync_EmptyString_DoesNotThrow_Async()
⋮----
Func<Task> act = async () => await dao.GetUserByWindowsUsernameAsync(string.Empty);
await act.Should().NotThrowAsync();
⋮----
public async Task ValidateUserPinAsync_SpecialCharactersInUsername_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.ValidateUserPinAsync(username, pin);
⋮----
public async Task CreateNewUserAsync_AllFieldsPopulated_PassesAllParameters_Async()
⋮----
var user = new Model_User
⋮----
Func<Task> act = async () => await dao.CreateNewUserAsync(user, createdBy);
⋮----
public async Task IsWindowsUsernameUniqueAsync_VeryLongUsername_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.IsWindowsUsernameUniqueAsync(longUsername);
⋮----
public async Task LogUserActivityAsync_LongDetails_HandlesGracefully_Async()
⋮----
await dao.LogUserActivityAsync("test_event", "user", "workstation", longDetails);
⋮----
public async Task UpsertWorkstationConfigAsync_VeryLongDescription_HandlesGracefully_Async()
⋮----
await dao.UpsertWorkstationConfigAsync("WS-01", "shared", true, longDescription);
⋮----
public async Task UpdateDefaultModeAsync_BoundaryUserIds_HandlesAll_Async(int userId)
⋮----
Func<Task> act = async () => await dao.UpdateDefaultModeAsync(userId, "mode");
⋮----
public async Task IsWindowsUsernameUniqueAsync_BoundaryExclusionValues_HandlesAll_Async(
⋮----
Func<Task> act = async () => await dao.IsWindowsUsernameUniqueAsync("user", excludeId);
⋮----
private static Model_User CreateValidTestUser()
⋮----
return new Model_User
```

## File: MTM_Receiving_Application.Tests/Module_Dunnage/Data/Dao_DunnageLoad_Tests.cs

```csharp
public class Dao_DunnageLoad_Tests
⋮----
private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);
⋮----
public void Constructor_ValidConnectionString_CreatesInstance()
⋮----
var dao = new Dao_DunnageLoad(TestConnectionString);
dao.Should().NotBeNull();
⋮----
public void Constructor_NullConnectionString_DoesNotThrow()
⋮----
Action act = () => new Dao_DunnageLoad(null!);
act.Should().NotThrow();
⋮----
public async Task GetAllAsync_NoParameters_CallsStoredProcedure_Async()
⋮----
var result = await dao.GetAllAsync();
result.Should().NotBeNull();
⋮----
public async Task GetByDateRangeAsync_ValidDateRange_CallsStoredProcedure_Async()
⋮----
var startDate = DateTime.Now.AddDays(-30);
⋮----
var result = await dao.GetByDateRangeAsync(startDate, endDate);
⋮----
public async Task GetByDateRangeAsync_StartDateAfterEndDate_HandlesGracefully_Async()
⋮----
var endDate = DateTime.Now.AddDays(-30);
⋮----
public async Task GetByDateRangeAsync_SameDates_HandlesGracefully_Async()
⋮----
var result = await dao.GetByDateRangeAsync(date, date);
⋮----
public async Task GetByDateRangeAsync_VeryOldDates_HandlesGracefully_Async()
⋮----
var startDate = new DateTime(1900, 1, 1);
var endDate = new DateTime(1900, 12, 31);
Func<Task> act = async () => await dao.GetByDateRangeAsync(startDate, endDate);
await act.Should().NotThrowAsync();
⋮----
public async Task GetByDateRangeAsync_FutureDates_HandlesGracefully_Async()
⋮----
var startDate = DateTime.Now.AddYears(1);
var endDate = DateTime.Now.AddYears(2);
⋮----
public async Task GetByDateRangeAsync_VeryLargeRange_HandlesGracefully_Async()
⋮----
public async Task GetByIdAsync_ValidGuid_CallsStoredProcedure_Async()
⋮----
var loadUuid = Guid.NewGuid();
var result = await dao.GetByIdAsync(loadUuid);
⋮----
public async Task GetByIdAsync_EmptyGuid_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.GetByIdAsync(loadUuid);
⋮----
public async Task GetByIdAsync_DifferentGuids_HandlesAll_Async()
⋮----
var guid = Guid.NewGuid();
Func<Task> act = async () => await dao.GetByIdAsync(guid);
⋮----
public async Task InsertAsync_ValidParameters_CallsStoredProcedure_Async()
⋮----
var result = await dao.InsertAsync(loadUuid, partId, quantity, user);
⋮----
public async Task InsertAsync_DifferentParts_HandlesAll_Async(string partId, decimal quantity)
⋮----
public async Task InsertAsync_DifferentQuantities_HandlesAll_Async(decimal quantity)
⋮----
public async Task InsertAsync_DifferentUsers_HandlesAll_Async(string user)
⋮----
// Arrange
⋮----
public async Task InsertBatchAsync_ValidSingleLoad_CallsStoredProcedure_Async()
⋮----
var result = await dao.InsertBatchAsync(loads, user);
⋮----
public async Task InsertBatchAsync_MultipleLoads_CallsStoredProcedure_Async()
⋮----
public async Task InsertBatchAsync_VariousBatchSizes_HandlesAll_Async(int batchSize)
⋮----
loads.Add(CreateValidDunnageLoad());
⋮----
public async Task UpdateAsync_ValidParameters_CallsStoredProcedure_Async()
⋮----
var result = await dao.UpdateAsync(loadUuid, quantity, user);
⋮----
public async Task UpdateAsync_DifferentQuantities_HandlesAll_Async(decimal quantity)
⋮----
public async Task UpdateAsync_EmptyGuid_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.UpdateAsync(loadUuid, quantity, user);
⋮----
public async Task DeleteAsync_ValidGuid_CallsStoredProcedure_Async()
⋮----
var result = await dao.DeleteAsync(loadUuid);
⋮----
public async Task DeleteAsync_EmptyGuid_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.DeleteAsync(loadUuid);
⋮----
public async Task DeleteAsync_MultipleDeletes_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.DeleteAsync(guid);
⋮----
public async Task InsertAsync_VeryLongPartId_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.InsertAsync(loadUuid, partId, quantity, user);
⋮----
public async Task InsertAsync_VeryLongUsername_HandlesGracefully_Async()
⋮----
public async Task InsertAsync_SpecialCharactersInPartId_HandlesGracefully_Async()
⋮----
public async Task InsertAsync_NegativeQuantity_HandlesGracefully_Async()
⋮----
public async Task InsertBatchAsync_EmptyList_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.InsertBatchAsync(loads, user);
⋮----
public async Task InsertBatchAsync_DuplicatePartIds_HandlesGracefully_Async()
⋮----
public async Task UpdateAsync_VeryLargeQuantity_HandlesGracefully_Async()
⋮----
public async Task UpdateAsync_VerySmallQuantity_HandlesGracefully_Async()
⋮----
private static Model_DunnageLoad CreateValidDunnageLoad()
⋮----
return new Model_DunnageLoad
⋮----
LoadUuid = Guid.NewGuid(),
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Data/Dao_ReceivingLine_Tests.cs

```csharp
public class Dao_ReceivingLine_Tests
⋮----
private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);
⋮----
public void Constructor_ValidConnectionString_CreatesInstance()
⋮----
var dao = new Dao_ReceivingLine(TestConnectionString);
dao.Should().NotBeNull();
⋮----
public void Constructor_NullConnectionString_ThrowsArgumentNullException()
⋮----
Action act = () => new Dao_ReceivingLine(null!);
act.Should().Throw<ArgumentNullException>()
.WithParameterName("connectionString");
⋮----
public void Constructor_EmptyConnectionString_DoesNotThrow()
⋮----
Action act = () => new Dao_ReceivingLine(string.Empty);
act.Should().NotThrow();
⋮----
public async Task InsertReceivingLineAsync_InvalidConnectionString_ReturnsFailureResult_Async()
⋮----
var dao = new Dao_ReceivingLine("Server=invalid;Database=nonexistent;");
⋮----
var result = await dao.InsertReceivingLineAsync(line);
result.Should().NotBeNull();
result.Success.Should().BeFalse("DAO must return failure result instead of throwing exception");
result.ErrorMessage.Should().NotBeNullOrEmpty();
result.Severity.Should().Be(Enum_ErrorSeverity.Error);
⋮----
public async Task InsertReceivingLineAsync_NullPartID_ReturnsResult_Async()
⋮----
var dao = new Dao_ReceivingLine("Server=invalid;");
⋮----
public async Task InsertReceivingLineAsync_NullHeat_ReturnsResult_Async()
⋮----
public async Task InsertReceivingLineAsync_NullInitialLocation_ReturnsResult_Async()
⋮----
public async Task InsertReceivingLineAsync_NullCoilsOnSkid_ReturnsResult_Async()
⋮----
public async Task InsertReceivingLineAsync_NullVendorName_ReturnsResult_Async()
⋮----
public async Task InsertReceivingLineAsync_NullPartDescription_ReturnsResult_Async()
⋮----
public async Task InsertReceivingLineAsync_DifferentQuantities_HandlesAllValues_Async(int quantity)
⋮----
public async Task InsertReceivingLineAsync_DifferentPONumbers_HandlesAllValues_Async(int poNumber)
⋮----
public async Task InsertReceivingLineAsync_AllFieldsPopulated_DoesNotThrow_Async()
⋮----
var line = new Model_ReceivingLine
⋮----
Func<Task> act = async () => await dao.InsertReceivingLineAsync(line);
await act.Should().NotThrowAsync();
⋮----
public async Task InsertReceivingLineAsync_MinimalFields_DoesNotThrow_Async()
⋮----
public async Task InsertReceivingLineAsync_VeryLongPartID_DoesNotThrow_Async()
⋮----
public async Task InsertReceivingLineAsync_VeryLongHeat_DoesNotThrow_Async()
⋮----
public async Task InsertReceivingLineAsync_VeryLongPartDescription_DoesNotThrow_Async()
⋮----
public async Task InsertReceivingLineAsync_FutureDate_DoesNotThrow_Async()
⋮----
line.Date = DateTime.Now.AddYears(1);
⋮----
public async Task InsertReceivingLineAsync_PastDate_DoesNotThrow_Async()
⋮----
line.Date = DateTime.Now.AddYears(-10);
⋮----
public async Task InsertReceivingLineAsync_BoundaryCoilsOnSkid_DoesNotThrow_Async(int? coilsOnSkid)
⋮----
public async Task InsertReceivingLineAsync_BoundaryEmployeeNumbers_DoesNotThrow_Async(int employeeNumber)
⋮----
public async Task InsertReceivingLineAsync_SpecialCharactersInFields_DoesNotThrow_Async()
⋮----
public async Task InsertReceivingLineAsync_DatabaseException_ReturnsFailureNotThrow_Async()
⋮----
var dao = new Dao_ReceivingLine(string.Empty);
⋮----
await act.Should().NotThrowAsync("DAO must return failure result instead of throwing exception");
⋮----
result.Success.Should().BeFalse();
⋮----
private static Model_ReceivingLine CreateValidReceivingLine()
⋮----
return new Model_ReceivingLine
```

## File: MTM_Receiving_Application.Tests/Module_Receiving/Data/Dao_ReceivingLoad_Tests.cs

```csharp
public class Dao_ReceivingLoad_Tests
⋮----
private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);
⋮----
public void Constructor_ValidConnectionString_CreatesInstance()
⋮----
var dao = new Dao_ReceivingLoad(TestConnectionString);
dao.Should().NotBeNull();
⋮----
public void Constructor_NullConnectionString_ThrowsArgumentNullException()
⋮----
Action act = () => new Dao_ReceivingLoad(null!);
act.Should().Throw<ArgumentNullException>()
.WithMessage("*connectionString*");
⋮----
public async Task SaveLoadsAsync_NullList_ReturnsFailure_Async()
⋮----
var result = await dao.SaveLoadsAsync(null!);
result.Should().NotBeNull();
result.Success.Should().BeFalse();
result.ErrorMessage.Should().Contain("null or empty");
⋮----
public async Task SaveLoadsAsync_EmptyList_ReturnsFailure_Async()
⋮----
var result = await dao.SaveLoadsAsync(loads);
⋮----
public async Task SaveLoadsAsync_ValidSingleLoad_CallsStoredProcedure_Async()
⋮----
public async Task SaveLoadsAsync_MultipleValidLoads_CallsStoredProcedure_Async()
⋮----
public async Task SaveLoadsAsync_NullPONumber_HandlesGracefully_Async()
⋮----
public async Task SaveLoadsAsync_CleansPONumberPrefix_HandlesAll_Async(string poNumber)
⋮----
public async Task SaveLoadsAsync_NonPOItem_HandlesGracefully_Async()
⋮----
public async Task UpdateLoadsAsync_NullList_ReturnsFailure_Async()
⋮----
var result = await dao.UpdateLoadsAsync(null!);
⋮----
public async Task UpdateLoadsAsync_EmptyList_ReturnsFailure_Async()
⋮----
var result = await dao.UpdateLoadsAsync(loads);
⋮----
public async Task UpdateLoadsAsync_ValidSingleLoad_CallsStoredProcedure_Async()
⋮----
public async Task UpdateLoadsAsync_MultipleLoads_CallsStoredProcedure_Async()
⋮----
public async Task DeleteLoadsAsync_NullList_ReturnsSuccess_Async()
⋮----
var result = await dao.DeleteLoadsAsync(null!);
⋮----
result.Success.Should().BeTrue();
result.Data.Should().Be(0);
⋮----
public async Task DeleteLoadsAsync_EmptyList_ReturnsSuccess_Async()
⋮----
var result = await dao.DeleteLoadsAsync(loads);
⋮----
public async Task DeleteLoadsAsync_ValidSingleLoad_CallsStoredProcedure_Async()
⋮----
public async Task DeleteLoadsAsync_MultipleLoads_CallsStoredProcedure_Async()
⋮----
public async Task GetHistoryAsync_ValidParameters_CallsStoredProcedure_Async()
⋮----
var startDate = DateTime.Now.AddDays(-30);
⋮----
var result = await dao.GetHistoryAsync(partID, startDate, endDate);
⋮----
public async Task GetHistoryAsync_DifferentPartIDs_HandlesAll_Async(string partID)
⋮----
public async Task GetHistoryAsync_StartDateAfterEndDate_HandlesGracefully_Async()
⋮----
var endDate = DateTime.Now.AddDays(-30);
⋮----
public async Task GetHistoryAsync_SameDateRange_HandlesGracefully_Async()
⋮----
var result = await dao.GetHistoryAsync(partID, date, date);
⋮----
public async Task GetAllAsync_ValidDateRange_CallsStoredProcedure_Async()
⋮----
var result = await dao.GetAllAsync(startDate, endDate);
⋮----
public async Task GetAllAsync_WideRange_HandlesGracefully_Async()
⋮----
var startDate = DateTime.Now.AddYears(-10);
var endDate = DateTime.Now.AddYears(1);
⋮----
public async Task SaveLoadsAsync_DifferentWeights_HandlesAll_Async(decimal weight)
⋮----
public async Task SaveLoadsAsync_DifferentPackageCounts_HandlesAll_Async(int packageCount)
⋮----
public async Task SaveLoadsAsync_VeryLongPartID_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.SaveLoadsAsync(loads);
await act.Should().NotThrowAsync();
⋮----
public async Task SaveLoadsAsync_VeryLongHeatNumber_HandlesGracefully_Async()
⋮----
public async Task SaveLoadsAsync_SpecialCharactersInFields_HandlesGracefully_Async()
⋮----
public async Task SaveLoadsAsync_VariousBatchSizes_HandlesAll_Async(int batchSize)
⋮----
loads.Add(CreateValidLoad());
⋮----
public async Task GetHistoryAsync_VeryOldDates_HandlesGracefully_Async()
⋮----
var startDate = new DateTime(1900, 1, 1);
var endDate = new DateTime(1900, 12, 31);
Func<Task> act = async () => await dao.GetHistoryAsync("PART-001", startDate, endDate);
⋮----
public async Task GetAllAsync_FutureDates_HandlesGracefully_Async()
⋮----
var startDate = DateTime.Now.AddYears(1);
var endDate = DateTime.Now.AddYears(2);
Func<Task> act = async () => await dao.GetAllAsync(startDate, endDate);
⋮----
private static Model_ReceivingLoad CreateValidLoad()
⋮----
return new Model_ReceivingLoad
⋮----
LoadID = Guid.NewGuid(),
```

## File: MTM_Receiving_Application.Tests/Module_Routing/Data/Dao_RoutingLabel_Tests.cs

```csharp
public class Dao_RoutingLabel_Tests
⋮----
private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);
⋮----
public void Constructor_ValidConnectionString_CreatesInstance()
⋮----
var dao = new Dao_RoutingLabel(TestConnectionString);
dao.Should().NotBeNull();
⋮----
public void Constructor_NullConnectionString_ThrowsArgumentNullException()
⋮----
Action act = () => new Dao_RoutingLabel(null!);
act.Should().Throw<ArgumentNullException>()
.WithMessage("*connectionString*");
⋮----
public async Task InsertLabelAsync_ValidLabel_CallsStoredProcedure_Async()
⋮----
var result = await dao.InsertLabelAsync(label);
result.Should().NotBeNull();
⋮----
public async Task InsertLabelAsync_NullOtherReasonId_HandlesGracefully_Async()
⋮----
public async Task InsertLabelAsync_WithOtherReasonId_HandlesGracefully_Async()
⋮----
public async Task InsertLabelAsync_DifferentQuantities_HandlesAll_Async(int quantity)
⋮----
public async Task InsertLabelAsync_DifferentPONumbers_HandlesAll_Async(string poNumber, string lineNumber)
⋮----
public async Task InsertLabelAsync_DifferentCreatedBy_HandlesAll_Async(int createdBy)
⋮----
public async Task UpdateLabelAsync_ValidLabel_CallsStoredProcedure_Async()
⋮----
var result = await dao.UpdateLabelAsync(label);
⋮----
public async Task UpdateLabelAsync_UpdatedQuantity_CallsStoredProcedure_Async()
⋮----
public async Task UpdateLabelAsync_NullOtherReasonId_HandlesGracefully_Async()
⋮----
public async Task UpdateLabelAsync_DifferentIds_HandlesAll_Async(int id)
⋮----
public async Task GetLabelByIdAsync_ValidId_CallsStoredProcedure_Async()
⋮----
var result = await dao.GetLabelByIdAsync(labelId);
⋮----
public async Task GetLabelByIdAsync_DifferentIds_HandlesAll_Async(int labelId)
⋮----
public async Task GetLabelByIdAsync_NegativeId_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.GetLabelByIdAsync(labelId);
await act.Should().NotThrowAsync();
⋮----
public async Task GetAllLabelsAsync_DefaultParameters_CallsStoredProcedure_Async()
⋮----
var result = await dao.GetAllLabelsAsync();
⋮----
public async Task GetAllLabelsAsync_DifferentPagination_HandlesAll_Async(int limit, int offset)
⋮----
var result = await dao.GetAllLabelsAsync(limit, offset);
⋮----
public async Task GetAllLabelsAsync_ZeroLimit_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.GetAllLabelsAsync(0, 0);
⋮----
public async Task GetAllLabelsAsync_NegativeOffset_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.GetAllLabelsAsync(100, -10);
⋮----
public async Task DeleteLabelAsync_ValidId_CallsStoredProcedure_Async()
⋮----
var result = await dao.DeleteLabelAsync(labelId);
⋮----
public async Task DeleteLabelAsync_DifferentIds_HandlesAll_Async(int labelId)
⋮----
public async Task MarkLabelExportedAsync_ValidId_CallsStoredProcedure_Async()
⋮----
var result = await dao.MarkLabelExportedAsync(labelId);
⋮----
public async Task MarkLabelExportedAsync_DifferentIds_HandlesAll_Async(int labelId)
⋮----
public async Task CheckDuplicateLabelAsync_ValidParameters_CallsStoredProcedure_Async()
⋮----
var result = await dao.CheckDuplicateLabelAsync(poNumber, lineNumber, recipientId, hoursWindow);
⋮----
public async Task CheckDuplicateLabelAsync_DifferentTimeWindows_HandlesAll_Async(int hoursWindow)
⋮----
public async Task CheckDuplicateLabelAsync_ZeroHoursWindow_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.CheckDuplicateLabelAsync(poNumber, lineNumber, recipientId, 0);
⋮----
public async Task CheckDuplicateLabelAsync_DifferentPONumbers_HandlesAll_Async(string poNumber, string lineNumber)
⋮----
var result = await dao.CheckDuplicateLabelAsync(poNumber, lineNumber, recipientId, 24);
⋮----
public async Task CheckDuplicateAsync_ValidParameters_CallsCheckDuplicate_Async()
⋮----
var createdWithinDate = DateTime.Now.AddHours(-12);
var result = await dao.CheckDuplicateAsync(poNumber, lineNumber, recipientId, createdWithinDate);
⋮----
public async Task CheckDuplicateAsync_DefaultDateTime_UsesDefault24Hours_Async()
⋮----
var result = await dao.CheckDuplicateAsync(poNumber, lineNumber, recipientId, default);
⋮----
public async Task CheckDuplicateAsync_FutureDate_HandlesGracefully_Async()
⋮----
var futureDate = DateTime.Now.AddDays(1);
Func<Task> act = async () => await dao.CheckDuplicateAsync(poNumber, lineNumber, recipientId, futureDate);
⋮----
public async Task CheckDuplicateAsync_OldDate_CapsAtOneWeek_Async()
⋮----
var oldDate = DateTime.Now.AddDays(-30);
Func<Task> act = async () => await dao.CheckDuplicateAsync(poNumber, lineNumber, recipientId, oldDate);
⋮----
public async Task MarkExportedAsync_EmptyList_HandlesGracefully_Async()
⋮----
var result = await dao.MarkExportedAsync(labelIds);
⋮----
public async Task MarkExportedAsync_SingleId_CallsMarkExported_Async()
⋮----
public async Task MarkExportedAsync_MultipleIds_CallsMarkExportedForEach_Async()
⋮----
public async Task InsertLabelAsync_VeryLongDescription_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.InsertLabelAsync(label);
⋮----
public async Task InsertLabelAsync_SpecialCharactersInFields_HandlesGracefully_Async()
⋮----
public async Task InsertLabelAsync_ZeroQuantity_HandlesGracefully_Async()
⋮----
public async Task InsertLabelAsync_NegativeQuantity_HandlesGracefully_Async()
⋮----
public async Task InsertLabelAsync_BoundaryRecipientIds_HandlesAll_Async(int recipientId)
⋮----
public async Task GetAllLabelsAsync_VeryLargeLimit_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.GetAllLabelsAsync(int.MaxValue, 0);
⋮----
public async Task CheckDuplicateLabelAsync_VeryLargeTimeWindow_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.CheckDuplicateLabelAsync("PO-123", "001", 10, 99999);
⋮----
public async Task CheckDuplicateLabelAsync_NegativeTimeWindow_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.CheckDuplicateLabelAsync("PO-123", "001", 10, -24);
⋮----
private static Model_RoutingLabel CreateValidRoutingLabel()
⋮----
return new Model_RoutingLabel
```

## File: MTM_Receiving_Application.Tests/Module_Settings/Data/Dao_PackageType_Tests.cs

```csharp
public class Dao_PackageType_Tests
⋮----
private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);
⋮----
public void Constructor_ValidConnectionString_CreatesInstance()
⋮----
var dao = new Dao_PackageType(TestConnectionString);
dao.Should().NotBeNull();
⋮----
public void Constructor_NullConnectionString_ThrowsArgumentNullException()
⋮----
Action act = () => new Dao_PackageType(null!);
act.Should().Throw<ArgumentNullException>()
.WithMessage("*connectionString*");
⋮----
public async Task GetAllAsync_NoParameters_CallsStoredProcedure_Async()
⋮----
var result = await dao.GetAllAsync();
result.Should().NotBeNull();
⋮----
public async Task GetByIdAsync_ValidId_CallsStoredProcedure_Async()
⋮----
var result = await dao.GetByIdAsync(id);
⋮----
public async Task GetByIdAsync_DifferentIds_HandlesAll_Async(int id)
⋮----
public async Task GetByIdAsync_NegativeId_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.GetByIdAsync(id);
await act.Should().NotThrowAsync();
⋮----
public async Task InsertAsync_ValidPackageType_CallsStoredProcedure_Async()
⋮----
var result = await dao.InsertAsync(packageType, createdBy);
⋮----
public async Task InsertAsync_MinimalData_CallsStoredProcedure_Async()
⋮----
var packageType = new Model_PackageType
⋮----
public async Task InsertAsync_DifferentTypes_HandlesAll_Async(string name, string code)
⋮----
public async Task InsertAsync_DifferentCreatedBy_HandlesAll_Async(int createdBy)
⋮----
public async Task UpdateAsync_ValidPackageType_CallsStoredProcedure_Async()
⋮----
var result = await dao.UpdateAsync(packageType);
⋮----
public async Task UpdateAsync_AllFieldsChanged_CallsStoredProcedure_Async()
⋮----
public async Task UpdateAsync_DifferentIds_HandlesAll_Async(int id)
⋮----
public async Task DeleteAsync_ValidId_CallsStoredProcedure_Async()
⋮----
var result = await dao.DeleteAsync(id);
⋮----
public async Task DeleteAsync_DifferentIds_HandlesAll_Async(int id)
⋮----
public async Task DeleteAsync_NegativeId_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.DeleteAsync(id);
⋮----
public async Task GetUsageCountAsync_ValidId_CallsStoredProcedure_Async()
⋮----
var result = await dao.GetUsageCountAsync(id);
⋮----
public async Task GetUsageCountAsync_DifferentIds_HandlesAll_Async(int id)
⋮----
public async Task InsertAsync_VeryLongName_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.InsertAsync(packageType, createdBy);
⋮----
public async Task InsertAsync_VeryLongCode_HandlesGracefully_Async()
⋮----
public async Task InsertAsync_SpecialCharactersInName_HandlesGracefully_Async()
⋮----
public async Task InsertAsync_EmptyStrings_HandlesGracefully_Async()
⋮----
public async Task UpdateAsync_VeryLongName_HandlesGracefully_Async()
⋮----
Func<Task> act = async () => await dao.UpdateAsync(packageType);
⋮----
public async Task UpdateAsync_SpecialCharactersInCode_HandlesGracefully_Async()
⋮----
public async Task GetUsageCountAsync_BoundaryValues_HandlesAll_Async(int id)
⋮----
Func<Task> act = async () => await dao.GetUsageCountAsync(id);
⋮----
public async Task InsertAsync_NegativeCreatedBy_HandlesGracefully_Async()
⋮----
public async Task UpdateAsync_WithIdZero_HandlesGracefully_Async()
⋮----
private static Model_PackageType CreateValidPackageType()
⋮----
return new Model_PackageType
```

## File: MTM_Receiving_Application.Tests/UnitTest1.cs

```csharp
public class UnitTest1
⋮----
public void Test_BasicMath_ShouldPass()
⋮----
result.Should().Be(expected);
⋮----
public void Test_StringOperation_ShouldPass()
⋮----
var result = testString.ToUpper();
result.Should().Be("HELLO, WORLD!");
```

## File: Module_Core/Helpers/Database/Helper_Database_Variables.cs

```csharp
public static class Helper_Database_Variables
⋮----
public static string GetConnectionString(bool useProduction = true)
⋮----
public static string GetInforVisualConnectionString()
```

## File: Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml.cs

```csharp
public sealed partial class View_Dunnage_WorkflowView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
private async void HelpButton_Click(object sender, RoutedEventArgs e)
⋮----
private async System.Threading.Tasks.Task ShowHelpForCurrentStepAsync()
⋮----
await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
⋮----
private async void OnNextClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
var result = await workflowService.AdvanceToNextStepAsync();
⋮----
var dialog = new ContentDialog
⋮----
var dialogResult = await dialog.ShowAsync();
⋮----
private async void OnSaveAndReviewClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
var confirmDialog = new ContentDialog
⋮----
var confirmResult = await confirmDialog.ShowAsync();
⋮----
var errorDialog = new ContentDialog
⋮----
await errorDialog.ShowAsync();
⋮----
private void OnBackClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
⋮----
workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
⋮----
workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
⋮----
workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
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

## File: Module_Routing/Views/RoutingWizardStep3View.xaml

```xml
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

## File: Module_Volvo/Data/Dao_VolvoPart.cs

```csharp
public class Dao_VolvoPart
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllAsync(bool includeInactive = false)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result<Model_VolvoPart>> GetByIdAsync(string partNumber)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result> InsertAsync(Model_VolvoPart part)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoPart part)
⋮----
public async Task<Model_Dao_Result> DeactivateAsync(string partNumber)
⋮----
var checkResult = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result<Dictionary<string, Model_VolvoPart>>> GetPartsByNumbersAsync(List<string> partNumbers)
⋮----
private static Model_VolvoPart MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoPart
⋮----
PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Volvo/Data/Dao_VolvoSettings.cs

```csharp
public class Dao_VolvoSettings
⋮----
public async Task<Model_Dao_Result<Model_VolvoSetting>> GetSettingAsync(string settingKey)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoSetting>>> GetAllSettingsAsync(string? category = null)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result> UpsertSettingAsync(string settingKey, string settingValue, string modifiedBy)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> ResetSettingAsync(string settingKey, string modifiedBy)
⋮----
private static Model_VolvoSetting MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoSetting
⋮----
SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
SettingType = reader.GetString(reader.GetOrdinal("setting_type")),
Category = reader.GetString(reader.GetOrdinal("category")),
Description = reader.IsDBNull(reader.GetOrdinal("description"))
⋮----
: reader.GetString(reader.GetOrdinal("description")),
DefaultValue = reader.GetString(reader.GetOrdinal("default_value")),
MinValue = reader.IsDBNull(reader.GetOrdinal("min_value"))
⋮----
: reader.GetInt32(reader.GetOrdinal("min_value")),
MaxValue = reader.IsDBNull(reader.GetOrdinal("max_value"))
⋮----
: reader.GetInt32(reader.GetOrdinal("max_value")),
ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by"))
⋮----
: reader.GetString(reader.GetOrdinal("modified_by"))
```

## File: Module_Volvo/Data/Dao_VolvoShipmentLine.cs

```csharp
public class Dao_VolvoShipmentLine
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result> InsertAsync(Model_VolvoShipmentLine line)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetByShipmentIdAsync(int shipmentId)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipmentLine line)
⋮----
public async Task<Model_Dao_Result> DeleteAsync(int lineId)
⋮----
private static Model_VolvoShipmentLine MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoShipmentLine
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
ShipmentId = reader.GetInt32(reader.GetOrdinal("shipment_id")),
PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
ReceivedSkidCount = reader.GetInt32(reader.GetOrdinal("received_skid_count")),
CalculatedPieceCount = reader.GetInt32(reader.GetOrdinal("calculated_piece_count")),
HasDiscrepancy = reader.GetBoolean(reader.GetOrdinal("has_discrepancy")),
ExpectedSkidCount = reader.IsDBNull(reader.GetOrdinal("expected_skid_count"))
⋮----
: reader.GetInt32(reader.GetOrdinal("expected_skid_count")),
DiscrepancyNote = reader.IsDBNull(reader.GetOrdinal("discrepancy_note"))
⋮----
: reader.GetString(reader.GetOrdinal("discrepancy_note"))
```

## File: Module_Volvo/Services/Service_VolvoMasterData.cs

```csharp
public class Service_VolvoMasterData : IService_VolvoMasterData
⋮----
private readonly Dao_VolvoPart _daoPart;
private readonly Dao_VolvoPartComponent _daoComponent;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
⋮----
_daoPart = daoPart ?? throw new ArgumentNullException(nameof(daoPart));
_daoComponent = daoComponent ?? throw new ArgumentNullException(nameof(daoComponent));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllPartsAsync(bool includeInactive)
⋮----
await _logger.LogInfoAsync($"Getting all Volvo parts (includeInactive={includeInactive})");
var result = await _daoPart.GetAllAsync(includeInactive);
⋮----
await _logger.LogErrorAsync($"Failed to get parts: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Error getting all parts: {ex.Message}", ex);
await _errorHandler.HandleErrorAsync(
⋮----
public async Task<Model_Dao_Result<Model_VolvoPart?>> GetPartByNumberAsync(string partNumber)
⋮----
await _logger.LogInfoAsync($"Getting part by number: {partNumber}");
var result = await _daoPart.GetByIdAsync(partNumber);
⋮----
await _logger.LogErrorAsync($"Failed to get part {partNumber}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Error getting part {partNumber}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> AddPartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null)
⋮----
await _logger.LogInfoAsync($"Adding new part: {part.PartNumber}");
if (string.IsNullOrWhiteSpace(part.PartNumber))
⋮----
return Model_Dao_Result_Factory.Failure("Part number is required");
⋮----
return Model_Dao_Result_Factory.Failure("Quantity per skid must be non-negative");
⋮----
var insertResult = await _daoPart.InsertAsync(part);
⋮----
await _logger.LogErrorAsync($"Failed to insert part {part.PartNumber}: {insertResult.ErrorMessage}");
return Model_Dao_Result_Factory.Failure(insertResult.ErrorMessage);
⋮----
var componentResult = await _daoComponent.InsertAsync(component);
⋮----
await _logger.LogErrorAsync($"Failed to insert component {component.ComponentPartNumber}: {componentResult.ErrorMessage}");
return Model_Dao_Result_Factory.Failure($"Failed to save component {component.ComponentPartNumber}");
⋮----
await _logger.LogInfoAsync($"Successfully added part {part.PartNumber} with {components?.Count ?? 0} components");
return Model_Dao_Result_Factory.Success("Part added successfully");
⋮----
await _logger.LogErrorAsync($"Error adding part {part.PartNumber}: {ex.Message}", ex);
⋮----
return Model_Dao_Result_Factory.Failure($"Error adding part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpdatePartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null)
⋮----
await _logger.LogInfoAsync($"Updating part: {part.PartNumber}");
⋮----
var updateResult = await _daoPart.UpdateAsync(part);
⋮----
await _logger.LogErrorAsync($"Failed to update part {part.PartNumber}: {updateResult.ErrorMessage}");
return Model_Dao_Result_Factory.Failure(updateResult.ErrorMessage);
⋮----
var deleteResult = await _daoComponent.DeleteByParentPartAsync(part.PartNumber);
⋮----
await _logger.LogErrorAsync($"Failed to delete components for {part.PartNumber}: {deleteResult.ErrorMessage}");
return Model_Dao_Result_Factory.Failure($"Failed to update components: {deleteResult.ErrorMessage}");
⋮----
var insertResult = await _daoComponent.InsertAsync(component);
⋮----
await _logger.LogErrorAsync($"Failed to insert component {component.ComponentPartNumber}: {insertResult.ErrorMessage}");
⋮----
await _daoComponent.DeleteByParentPartAsync(part.PartNumber);
⋮----
await _logger.LogInfoAsync($"Successfully updated part {part.PartNumber} with {components?.Count ?? 0} components");
return Model_Dao_Result_Factory.Success("Part updated successfully");
⋮----
await _logger.LogErrorAsync($"Error updating part {part.PartNumber}: {ex.Message}", ex);
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeactivatePartAsync(string partNumber)
⋮----
await _logger.LogInfoAsync($"Deactivating part: {partNumber}");
var result = await _daoPart.DeactivateAsync(partNumber);
⋮----
await _logger.LogErrorAsync($"Failed to deactivate part {partNumber}: {result.ErrorMessage}");
⋮----
await _logger.LogInfoAsync($"Successfully deactivated part {partNumber}");
⋮----
await _logger.LogErrorAsync($"Error deactivating part {partNumber}: {ex.Message}", ex);
return Model_Dao_Result_Factory.Failure($"Error deactivating part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetComponentsAsync(string partNumber)
⋮----
await _logger.LogInfoAsync($"Getting components for part: {partNumber}");
var result = await _daoComponent.GetByParentPartAsync(partNumber);
⋮----
await _logger.LogErrorAsync($"Failed to get components for {partNumber}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Error getting components for {partNumber}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<(int New, int Updated, int Unchanged)>> ImportCsvAsync(string csvFilePath)
⋮----
await _logger.LogInfoAsync("Starting CSV import");
if (string.IsNullOrWhiteSpace(csvFilePath))
⋮----
var lines = csvFilePath.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
⋮----
if (!header.Contains("PartNumber") || !header.Contains("QuantityPerSkid"))
⋮----
errors.Add($"Line {i + 1}: Invalid format - expected at least 3 fields");
⋮----
var partNumber = fields[0].Trim();
var quantityStr = fields[1].Trim();
var componentsStr = fields.Length > 2 ? fields[2].Trim() : "";
if (!int.TryParse(quantityStr, out int quantity))
⋮----
errors.Add($"Line {i + 1}: Invalid quantity '{quantityStr}'");
⋮----
var part = new Model_VolvoPart
⋮----
// Parse components (format: "PART1:QTY1;PART2:QTY2")
⋮----
if (!string.IsNullOrWhiteSpace(componentsStr))
⋮----
var componentPairs = componentsStr.Split(';');
⋮----
var parts = pair.Split(':');
⋮----
if (int.TryParse(parts[1], out int compQty))
⋮----
components.Add(new Model_VolvoPartComponent
⋮----
ComponentPartNumber = parts[0].Trim(),
⋮----
var existing = await _daoPart.GetByIdAsync(partNumber);
⋮----
Model_Dao_Result saveResult;
⋮----
errors.Add($"Line {i + 1}: {saveResult.ErrorMessage}");
⋮----
errors.Add($"Line {i + 1}: {ex.Message}");
⋮----
await _logger.LogInfoAsync(summary);
⋮----
summary += "\nErrors:\n" + string.Join("\n", errors);
⋮----
return Model_Dao_Result_Factory.Success((newCount, updatedCount, errorCount));
⋮----
await _logger.LogErrorAsync($"Error importing CSV: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<string>> ExportCsvAsync(string csvFilePath, bool includeInactive = false)
⋮----
await _logger.LogInfoAsync($"Exporting parts to CSV (includeInactive={includeInactive})");
⋮----
var csv = new StringBuilder();
csv.AppendLine("PartNumber,QuantityPerSkid,Components");
⋮----
componentsStr = string.Join(";", componentsResult.Data.Select(c => $"{c.ComponentPartNumber}:{c.Quantity}"));
⋮----
csv.AppendLine($"{EscapeCsvField(part.PartNumber ?? string.Empty)},{part.QuantityPerSkid},{EscapeCsvField(componentsStr)}");
⋮----
await _logger.LogInfoAsync($"Export complete: {partsResult.Data?.Count ?? 0} parts");
return Model_Dao_Result_Factory.Success(csv.ToString());
⋮----
await _logger.LogErrorAsync($"Error exporting CSV: {ex.Message}", ex);
⋮----
private string[] ParseCsvLine(string line)
⋮----
var currentField = new StringBuilder();
⋮----
fields.Add(currentField.ToString());
currentField.Clear();
⋮----
currentField.Append(c);
⋮----
return fields.ToArray();
⋮----
private string EscapeCsvField(string field)
⋮----
if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
⋮----
return "\"" + field.Replace("\"", "\"\"") + "\"";
```

## File: Module_Volvo/ViewModels/ViewModel_Volvo_History.cs

```csharp
public partial class ViewModel_Volvo_History : ViewModel_Shared_Base
⋮----
private readonly IService_Volvo _volvoService;
⋮----
private DateTimeOffset? _startDate = DateTimeOffset.Now.AddDays(-30);
⋮----
_volvoService = volvoService ?? throw new ArgumentNullException(nameof(volvoService));
⋮----
private void GoBack()
⋮----
var contentFrame = mainWindow.GetContentFrame();
⋮----
contentFrame.GoBack();
⋮----
private async Task FilterAsync()
⋮----
var result = await _volvoService.GetHistoryAsync(
StartDate?.DateTime ?? DateTime.Now.AddDays(-30),
⋮----
History.Clear();
⋮----
History.Add(shipment);
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private async Task ViewDetailAsync()
⋮----
await _logger.LogInfoAsync($"Loading details for shipment ID: {SelectedShipment.Id}");
var result = await _volvoService.GetShipmentLinesAsync(SelectedShipment.Id);
⋮----
details.AppendLine($"Shipment #{SelectedShipment.ShipmentNumber}");
details.AppendLine($"Date: {SelectedShipment.ShipmentDate:d}");
details.AppendLine($"PO Number: {SelectedShipment.PONumber ?? "N/A"}");
details.AppendLine($"Receiver: {SelectedShipment.ReceiverNumber ?? "N/A"}");
details.AppendLine($"Status: {SelectedShipment.Status}");
details.AppendLine();
details.AppendLine($"Parts ({result.Data.Count}):");
⋮----
details.AppendLine($"  • {line.PartNumber}: {line.ReceivedSkidCount} skids ({line.CalculatedPieceCount} pieces)");
⋮----
details.AppendLine($"    ⚠ Discrepancy: Expected {line.ExpectedSkidCount} skids");
⋮----
if (!string.IsNullOrWhiteSpace(SelectedShipment.Notes))
⋮----
details.AppendLine($"Notes: {SelectedShipment.Notes}");
⋮----
var dialog = new ContentDialog
⋮----
Content = new ScrollViewer
⋮----
Content = new TextBlock
⋮----
Text = details.ToString(),
⋮----
await dialog.ShowAsync();
⋮----
await _logger.LogInfoAsync($"Successfully loaded {result.Data.Count} lines");
⋮----
private bool CanViewDetail() => SelectedShipment != null && !IsBusy;
⋮----
private async Task EditAsync()
⋮----
var linesResult = await _volvoService.GetShipmentLinesAsync(SelectedShipment.Id);
⋮----
dialog.LoadShipment(SelectedShipment, linesCollection, availableParts);
var result = await dialog.ShowAsync();
⋮----
var updatedShipment = dialog.GetUpdatedShipment();
var updatedLines = dialog.GetUpdatedLines();
var updateResult = await _volvoService.UpdateShipmentAsync(
⋮----
private bool CanEdit() => SelectedShipment != null && !IsBusy;
⋮----
private async Task ExportAsync()
⋮----
var result = await _volvoService.ExportHistoryToCsvAsync(
⋮----
if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
⋮----
partial void OnSelectedShipmentChanged(Model_VolvoShipment? value)
⋮----
ViewDetailCommand.NotifyCanExecuteChanged();
EditCommand.NotifyCanExecuteChanged();
```

## File: Module_Volvo/Views/VolvoShipmentEditDialog.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.VolvoShipmentEditDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:models="using:MTM_Receiving_Application.Module_Volvo.Models"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    Title="Edit Shipment"
    PrimaryButtonText="Save Changes"
    CloseButtonText="Cancel"
    DefaultButton="Primary">
    
    <ContentDialog.Resources>
        <x:Double x:Key="ContentDialogMaxWidth">1200</x:Double>
        <x:Double x:Key="ContentDialogMinWidth">1200</x:Double>
        <converters:NullableDoubleToDoubleConverter x:Key="NullableDoubleToDoubleConverter"/>
    </ContentDialog.Resources>

    <ScrollViewer MaxHeight="600">
        <StackPanel Spacing="16" Padding="24">
            
            <!-- Shipment Header -->
            <StackPanel Spacing="8">
                <TextBlock Text="Shipment Details" Style="{StaticResource SubtitleTextBlockStyle}"/>
                
                <Grid ColumnSpacing="12" RowSpacing="8">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>
                    
                    <!-- Shipment Date -->
                    <CalendarDatePicker
                        Grid.Row="0" Grid.Column="0"
                        Header="Shipment Date"
                        x:Name="ShipmentDatePicker"
                        HorizontalAlignment="Stretch" />
                    
                    <!-- Shipment Number (Read-only) -->
                    <TextBox
                        Grid.Row="0" Grid.Column="1"
                        Header="Shipment Number"
                        x:Name="ShipmentNumberBox"
                        IsReadOnly="True"
                        HorizontalAlignment="Stretch" />
                    
                    <!-- PO Number -->
                    <TextBox
                        Grid.Row="1" Grid.Column="0"
                        Header="PO Number"
                        PlaceholderText="Optional"
                        x:Name="PONumberBox"
                        HorizontalAlignment="Stretch" />
                    
                    <!-- Receiver Number -->
                    <TextBox
                        Grid.Row="1" Grid.Column="1"
                        Header="Receiver Number"
                        PlaceholderText="Optional"
                        x:Name="ReceiverNumberBox"
                        HorizontalAlignment="Stretch" />
                    
                    <!-- Notes -->
                    <TextBox
                        Grid.Row="2" Grid.Column="0" Grid.ColumnSpan="2"
                        Header="Notes"
                        PlaceholderText="Optional notes"
                        x:Name="NotesBox"
                        TextWrapping="Wrap"
                        AcceptsReturn="True"
                        Height="80"
                        HorizontalAlignment="Stretch" />
                </Grid>
            </StackPanel>
            
            <!-- Parts DataGrid -->
            <StackPanel Spacing="8">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <TextBlock Text="Parts" Style="{StaticResource SubtitleTextBlockStyle}"/>
                    <Button x:Name="AddPartButton" Content="Add Part" Style="{StaticResource AccentButtonStyle}" Height="32"/>
                    <Button x:Name="RemovePartButton" Content="Remove Part" Height="32"/>
                </StackPanel>
                
                <controls:DataGrid
                    x:Name="PartsDataGrid"
                    Height="300"
                    AutoGenerateColumns="False"
                    CanUserReorderColumns="False"
                    CanUserSortColumns="False"
                    SelectionMode="Single"
                    GridLinesVisibility="All"
                    HorizontalScrollBarVisibility="Auto">
                    
                    <controls:DataGrid.Columns>
                        <!-- Part Number -->
                        <controls:DataGridTextColumn
                            Header="Part Number"
                            Binding="{Binding PartNumber}"
                            Width="150"/>
                        
                        <!-- Received Skids -->
                        <controls:DataGridTemplateColumn Header="Received Skids" Width="120">
                            <controls:DataGridTemplateColumn.CellTemplate>
                                <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                    <NumberBox
                                        Value="{x:Bind ReceivedSkidCount, Mode=TwoWay}"
                                        Minimum="0"
                                        SpinButtonPlacementMode="Compact"
                                        HorizontalAlignment="Stretch" />
                                </DataTemplate>
                            </controls:DataGridTemplateColumn.CellTemplate>
                        </controls:DataGridTemplateColumn>
                        
                        <!-- Calculated Pieces (Read-only) -->
                        <controls:DataGridTextColumn
                            Header="Calculated Pieces"
                            Binding="{Binding CalculatedPieceCount}"
                            IsReadOnly="True"
                            Width="120"/>
                        
                        <!-- Has Discrepancy -->
                        <controls:DataGridCheckBoxColumn
                            Header="Discrepancy?"
                            Binding="{Binding HasDiscrepancy, Mode=TwoWay}"
                            Width="100"/>
                        
                        <!-- Expected Skids -->
                        <controls:DataGridTemplateColumn Header="Expected Skids" Width="120">
                            <controls:DataGridTemplateColumn.CellTemplate>
                                <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                    <NumberBox
                                        Value="{Binding ExpectedSkidCount, Mode=TwoWay, Converter={StaticResource NullableDoubleToDoubleConverter}}"
                                        Minimum="0"
                                        SpinButtonPlacementMode="Compact"
                                        HorizontalAlignment="Stretch"
                                        IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay}" />
                                </DataTemplate>
                            </controls:DataGridTemplateColumn.CellTemplate>
                        </controls:DataGridTemplateColumn>
                        
                        <!-- Expected Pieces Column -->
                        <controls:DataGridTextColumn Header="Expected Pieces"
                                                      Width="120"
                                                      Binding="{Binding ExpectedPieceCount, Mode=OneWay}"
                                                      IsReadOnly="True"/>
                        
                        <!-- Difference Column -->
                        <controls:DataGridTextColumn Header="Difference"
                                                      Width="100"
                                                      Binding="{Binding PieceDifference, Mode=OneWay}"
                                                      IsReadOnly="True"/>
                        
                        <!-- Discrepancy Note -->
                        <controls:DataGridTemplateColumn Header="Discrepancy Note" Width="*" MinWidth="200">
                            <controls:DataGridTemplateColumn.CellTemplate>
                                <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                    <TextBox
                                        Text="{x:Bind DiscrepancyNote, Mode=TwoWay}"
                                        IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay}"
                                        PlaceholderText="Enter note"
                                        HorizontalAlignment="Stretch" />
                                </DataTemplate>
                            </controls:DataGridTemplateColumn.CellTemplate>
                        </controls:DataGridTemplateColumn>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>
            </StackPanel>
            
        </StackPanel>
    </ScrollViewer>
</ContentDialog>
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

```xml
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

## File: Module_Volvo/Data/Dao_VolvoShipment.cs

```csharp
public class Dao_VolvoShipment
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> InsertAsync(Model_VolvoShipment shipment)
⋮----
await using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync();
await using var command = new MySqlCommand("sp_volvo_shipment_insert", connection)
⋮----
command.Parameters.AddWithValue("@p_shipment_date", shipment.ShipmentDate);
command.Parameters.AddWithValue("@p_employee_number", shipment.EmployeeNumber);
command.Parameters.AddWithValue("@p_notes", shipment.Notes ?? (object)DBNull.Value);
var newIdParam = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
var shipmentNumberParam = new MySqlParameter("@p_shipment_number", MySqlDbType.Int32)
⋮----
command.Parameters.Add(newIdParam);
command.Parameters.Add(shipmentNumberParam);
await command.ExecuteNonQueryAsync();
var shipmentId = Convert.ToInt32(newIdParam.Value);
var shipmentNumber = Convert.ToInt32(shipmentNumberParam.Value);
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipment shipment)
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
return new Model_Dao_Result
⋮----
public async Task<Model_Dao_Result> CompleteAsync(int shipmentId, string poNumber, string receiverNumber)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> DeleteAsync(int shipmentId)
⋮----
public async Task<Model_Dao_Result<Model_VolvoShipment>> GetPendingAsync()
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result<Model_VolvoShipment?>> GetByIdAsync(int shipmentId)
⋮----
await using var command = new MySqlCommand
⋮----
command.Parameters.AddWithValue("@p_id", shipmentId);
await using var reader = await command.ExecuteReaderAsync();
if (await reader.ReadAsync())
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
private static Model_VolvoShipment MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoShipment
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
ShipmentDate = reader.GetDateTime(reader.GetOrdinal("shipment_date")),
ShipmentNumber = reader.GetInt32(reader.GetOrdinal("shipment_number")),
PONumber = reader.IsDBNull(reader.GetOrdinal("po_number"))
⋮----
: reader.GetString(reader.GetOrdinal("po_number")),
ReceiverNumber = reader.IsDBNull(reader.GetOrdinal("receiver_number"))
⋮----
: reader.GetString(reader.GetOrdinal("receiver_number")),
EmployeeNumber = reader.GetString(reader.GetOrdinal("employee_number")),
Notes = reader.IsDBNull(reader.GetOrdinal("notes"))
⋮----
: reader.GetString(reader.GetOrdinal("notes")),
Status = reader.GetString(reader.GetOrdinal("status")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date")),
IsArchived = reader.GetBoolean(reader.GetOrdinal("is_archived"))
```

## File: Module_Volvo/Models/Model_VolvoShipmentLine.cs

```csharp
public partial class Model_VolvoShipmentLine : ObservableObject
⋮----
partial void OnReceivedSkidCountChanged(int value)
⋮----
partial void OnQuantityPerSkidChanged(int value)
⋮----
partial void OnExpectedSkidCountChanged(double? value)
⋮----
partial void OnHasDiscrepancyChanged(bool value)
```

## File: MainWindow.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window
    x:Class="MTM_Receiving_Application.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:MTM_Receiving_Application"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:Name="MainWindowRoot"
    Title="MTM_Receiving_Application">

    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>

    <Grid>
        <!-- Custom Title Bar Area (when ExtendsContentIntoTitleBar = true) -->
        <Grid x:Name="AppTitleBar" 
              Height="48" 
              VerticalAlignment="Top" 
              Canvas.ZIndex="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <!-- App Icon and Title -->
            <StackPanel Orientation="Horizontal" Spacing="12" Margin="16,0,0,0">
                <Image Source="/Assets/app-icon.ico" 
                       Width="16" 
                       Height="16" 
                       VerticalAlignment="Center"/>
                <TextBlock Text="MTM Receiving Application" 
                           VerticalAlignment="Center"
                           Style="{StaticResource CaptionTextBlockStyle}"/>
            </StackPanel>
        </Grid>

        <NavigationView x:Name="NavView"
                        PaneDisplayMode="Left"
                        IsSettingsVisible="True"
                        SelectionChanged="NavView_SelectionChanged">
            <NavigationView.PaneHeader>
                <TextBlock Text="MTM Receiving" 
                           Style="{StaticResource TitleTextBlockStyle}"
                           Margin="12,0,0,0"/>
            </NavigationView.PaneHeader>
            
            <NavigationView.Header>
                <StackPanel>
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        
                        <TextBlock x:Name="PageTitleTextBlock"
                                   Text="Dashboard" 
                                   Style="{StaticResource TitleTextBlockStyle}"
                                   VerticalAlignment="Center"
                                   Margin="16,0"/>
                                   
                        <StackPanel Grid.Column="1" 
                                    Orientation="Horizontal" 
                                    Spacing="12" 
                                    Margin="0,0,24,0"
                                    VerticalAlignment="Center">
                            <PersonPicture x:Name="UserPicture"
                                           Height="32"
                                           Width="32"/>
                            <TextBlock x:Name="UserDisplayTextBlock"
                                       Text="Not Logged In"
                                       VerticalAlignment="Center"
                                       Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        </StackPanel>
                    </Grid>
                    <InfoBar IsOpen="{x:Bind ViewModel.NotificationService.IsStatusOpen, Mode=TwoWay}"
                             Severity="{x:Bind ViewModel.WinUIStatusSeverity, Mode=OneWay}"
                             Message="{x:Bind ViewModel.NotificationService.StatusMessage, Mode=OneWay}"
                             IsClosable="True"
                             HorizontalAlignment="Center"
                             VerticalAlignment="Center"
                             Margin="0,20,0,20"/>
                </StackPanel>
            </NavigationView.Header>

            <NavigationView.MenuItems>
                <NavigationViewItem Content="Receiving Labels" Tag="ReceivingWorkflowView" Margin="0,0,0,4">
                    <NavigationViewItem.Icon>
                        <FontIcon Glyph="&#xE74C;" />
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
                <NavigationViewItem Content="Dunnage Labels" Tag="DunnageLabelPage" Margin="0,0,0,4">
                    <NavigationViewItem.Icon>
                        <FontIcon Glyph="&#xE7B8;" />
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
                <NavigationViewItem Content="UPS / FedEx Labels" Tag="RoutingLabelPage" Margin="0,0,0,4">
                    <NavigationViewItem.Icon>
                        <FontIcon Glyph="&#xE724;" />
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
                <NavigationViewItem Content="Volvo Dunnage Requisition" Tag="VolvoShipmentEntry" Margin="0,0,0,4">
                    <NavigationViewItem.Icon>
                        <FontIcon Glyph="&#xE7C1;" />
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
                <NavigationViewItem Content="End of Day Reports" Tag="ReportingMainPage" Margin="0,0,0,4">
                    <NavigationViewItem.Icon>
                        <FontIcon Glyph="&#xE9F9;" />
                    </NavigationViewItem.Icon>
                </NavigationViewItem>

                <!-- Development Tools (visible in debug builds only) -->
                <NavigationViewItem x:Name="DatabaseTestMenuItem" 
                                    Content="Settings DB Test" 
                                    Tag="DatabaseTestView" 
                                    Margin="0,0,0,4"
                                    Visibility="Collapsed">
                    <NavigationViewItem.Icon>
                        <FontIcon Glyph="&#xE8AB;" />
                    </NavigationViewItem.Icon>
                </NavigationViewItem>

            </NavigationView.MenuItems>

            <Frame x:Name="ContentFrame" />
        </NavigationView>
    </Grid>
</Window>
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

## File: Module_Settings/ViewModels/ViewModel_Settings_DatabaseTest.cs

```csharp
public partial class ViewModel_Settings_DatabaseTest : ViewModel_Shared_Base
⋮----
private readonly Dao_SystemSettings _daoSystemSettings;
private readonly Dao_UserSettings _daoUserSettings;
private readonly Dao_PackageType _daoPackageType;
private readonly Dao_PackageTypeMappings _daoPackageTypeMappings;
private readonly Dao_RoutingRule _daoRoutingRule;
private readonly Dao_ScheduledReport _daoScheduledReport;
private readonly Dao_SettingsAuditLog _daoSettingsAuditLog;
⋮----
private DateTime _lastRunTime = DateTime.Now;
⋮----
private async Task RunAllTestsAsync()
⋮----
var stopwatch = Stopwatch.StartNew();
LogMessages.Clear();
⋮----
await _logger.LogInfoAsync("Database test suite started", "ViewModel_Settings_DatabaseTest");
⋮----
stopwatch.Stop();
⋮----
await _logger.LogInfoAsync($"Database tests completed successfully in {TotalTestDurationMs}ms", "ViewModel_Settings_DatabaseTest");
⋮----
await _logger.LogErrorAsync($"Database test execution failed: {ex.Message}", ex, "ViewModel_Settings_DatabaseTest");
_errorHandler.HandleException(
⋮----
private async Task ExportResultsAsync()
⋮----
var exportRoot = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
⋮----
Directory.CreateDirectory(exportRoot);
var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
var jsonPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}.json");
var logPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}.log.txt");
var tablesCsvPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}_tables.csv");
var procsCsvPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}_stored_procedures.csv");
var daosCsvPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}_daos.csv");
⋮----
var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
⋮----
return "\"" + s.Replace("\"", "\"\"") + "\"";
⋮----
.Select(t => new { t.TableName, t.IsValid, t.StatusText, t.Details })
.ToList(),
⋮----
.Select(p => new { p.ProcedureName, p.IsValid, p.ExecutionTimeMs, p.TestDetails })
⋮----
.Select(d => new { d.DaoName, d.IsValid, d.StatusText, d.OperationsTested })
⋮----
.Select(a => new
⋮----
Logs = (LogMessages ?? new ObservableCollection<string>()).ToList()
⋮----
var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
⋮----
await File.WriteAllTextAsync(jsonPath, json, Encoding.UTF8);
await File.WriteAllTextAsync(logPath, string.Join(Environment.NewLine, report.Logs), Encoding.UTF8);
var tablesCsv = new StringBuilder();
tablesCsv.AppendLine("TableName,IsValid,StatusText,Details");
⋮----
tablesCsv.AppendLine($"{EscapeCsv(t.TableName)},{t.IsValid},{EscapeCsv(t.StatusText)},{EscapeCsv(t.Details)}");
⋮----
await File.WriteAllTextAsync(tablesCsvPath, tablesCsv.ToString(), Encoding.UTF8);
var procsCsv = new StringBuilder();
procsCsv.AppendLine("ProcedureName,IsValid,ExecutionTimeMs,TestDetails");
⋮----
procsCsv.AppendLine($"{EscapeCsv(p.ProcedureName)},{p.IsValid},{p.ExecutionTimeMs},{EscapeCsv(p.TestDetails)}");
⋮----
await File.WriteAllTextAsync(procsCsvPath, procsCsv.ToString(), Encoding.UTF8);
var daosCsv = new StringBuilder();
daosCsv.AppendLine("DaoName,IsValid,StatusText,OperationsTested");
⋮----
daosCsv.AppendLine($"{EscapeCsv(d.DaoName)},{d.IsValid},{EscapeCsv(d.StatusText)},{EscapeCsv(d.OperationsTested)}");
⋮----
await File.WriteAllTextAsync(daosCsvPath, daosCsv.ToString(), Encoding.UTF8);
⋮----
await _logger.LogInfoAsync($"Settings DB Test export completed: {jsonPath}", "ViewModel_Settings_DatabaseTest");
⋮----
_errorHandler.HandleException(ex, Enum_ErrorSeverity.Low, nameof(ExportResultsAsync), nameof(ViewModel_Settings_DatabaseTest));
⋮----
private async Task LoadAuditLogAsync()
⋮----
AuditLogEntries.Clear();
var settingsResult = await _daoSystemSettings.GetAllAsync();
⋮----
var auditResult = await _daoSettingsAuditLog.GetAsync(settingId, 50);
⋮----
AuditLogEntries.Add(entry);
⋮----
_errorHandler.HandleException(ex, Enum_ErrorSeverity.Low, nameof(LoadAuditLogAsync), nameof(ViewModel_Settings_DatabaseTest));
⋮----
private async Task TestConnectionAsync()
⋮----
await _logger.LogInfoAsync("Testing database connection", "ViewModel_Settings_DatabaseTest");
⋮----
await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
await connection.OpenAsync();
⋮----
await _logger.LogInfoAsync($"Database connection successful in {ConnectionTimeMs}ms - MySQL {ServerVersion}", "ViewModel_Settings_DatabaseTest");
⋮----
await _logger.LogErrorAsync($"Database connection failed: {ex.Message}", ex, "ViewModel_Settings_DatabaseTest");
⋮----
private async Task TestTablesAsync()
⋮----
TableResults.Clear();
⋮----
await using var command = new MySqlCommand(query, connection);
var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
⋮----
TableResults.Add(new Model_TableTestResult
⋮----
private async Task TestStoredProceduresAsync()
⋮----
StoredProcedureResults.Clear();
⋮----
var sw = Stopwatch.StartNew();
⋮----
sw.Stop();
StoredProcedureResults.Add(new Model_StoredProcedureTestResult
⋮----
private async Task TestDaosAsync()
⋮----
DaoResults.Clear();
⋮----
var result = await _daoSystemSettings.GetAllAsync();
⋮----
var result = await _daoUserSettings.GetAllForUserAsync(1);
⋮----
var result = await _daoPackageType.GetAllAsync();
⋮----
var result = await _daoRoutingRule.GetAllAsync();
⋮----
var result = await _daoScheduledReport.GetAllAsync();
⋮----
var result = await _daoPackageTypeMappings.GetAllAsync();
⋮----
var auditResult = await _daoSettingsAuditLog.GetAsync(settingsResult.Data[0].Id, 10);
⋮----
private async Task<bool> TestDaoAsync(string daoName, Func<Task<bool>> testFunc)
⋮----
DaoResults.Add(new Model_DaoTestResult
⋮----
private void AddLog(string message)
⋮----
var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
LogMessages.Add($"[{timestamp}] {message}");
```

## File: Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Page
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.View_Volvo_ShipmentEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:views="using:MTM_Receiving_Application.Module_Volvo.Views"
    xmlns:models="using:MTM_Receiving_Application.Module_Volvo.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    Loaded="OnLoaded">

    <Page.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:NullableDoubleToDoubleConverter x:Key="NullableDoubleToDoubleConverter"/>
    </Page.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Success Message -->
        <InfoBar Grid.Row="0"
                 IsOpen="{x:Bind ViewModel.IsSuccessMessageVisible, Mode=OneWay}"
                 Severity="Success"
                 Message="{x:Bind ViewModel.SuccessMessage, Mode=OneWay}"
                 IsClosable="True"/>

        <!-- Header Section -->
        <StackPanel Grid.Row="1" Spacing="16">
            <!-- Shipment Info Row -->
            <Grid ColumnSpacing="16">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="2*"/>
                </Grid.ColumnDefinitions>

                <CalendarDatePicker Grid.Column="0"
                                    Header="Shipment Date"
                                    Date="{x:Bind ViewModel.ShipmentDate, Mode=TwoWay}"
                                    HorizontalAlignment="Stretch"/>

                <NumberBox Grid.Column="1"
                           Header="Shipment Number"
                           Value="{x:Bind ViewModel.ShipmentNumber, Mode=TwoWay}"
                           Minimum="1"
                           SpinButtonPlacementMode="Inline"
                           HorizontalAlignment="Stretch"/>

                <TextBox Grid.Column="2"
                         Header="Notes (optional)"
                         Text="{x:Bind ViewModel.Notes, Mode=TwoWay}"
                         PlaceholderText="Enter any additional notes"/>
            </Grid>

            <!-- Part Entry Row -->
            <Grid ColumnSpacing="16">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="2*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <AutoSuggestBox Grid.Column="0"
                                Header="Part Number"
                                ItemsSource="{x:Bind ViewModel.SuggestedParts, Mode=OneWay}"
                                Text="{x:Bind ViewModel.PartSearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                DisplayMemberPath="PartNumber"
                                TextMemberPath="PartNumber"
                                PlaceholderText="Type to search for a part number"
                                TextChanged="OnPartSearchTextChanged"
                                SuggestionChosen="OnPartSuggestionChosen"
                                HorizontalAlignment="Stretch"/>

                <NumberBox Grid.Column="1"
                           Header="Received Skids"
                           Value="{x:Bind ViewModel.ReceivedSkidsToAdd, Mode=TwoWay}"
                           Minimum="1"
                           Maximum="99"
                           SpinButtonPlacementMode="Inline"
                           HorizontalAlignment="Stretch"/>

                <Button Grid.Column="2"
                        Content="Add Part"
                        Command="{x:Bind ViewModel.AddPartCommand}"
                        VerticalAlignment="Bottom"
                        Style="{StaticResource AccentButtonStyle}"
                        Margin="0,0,0,4"/>
            </Grid>
        </StackPanel>

        <!-- Parts Entry DataGrid -->
        <Grid Grid.Row="2">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <TextBlock Grid.Row="0" 
                       Text="Parts Received" 
                       Style="{StaticResource SubtitleTextBlockStyle}"
                       Margin="0,0,0,8"/>

            <controls:DataGrid Grid.Row="1"
                               ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
                               SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                               AutoGenerateColumns="False"
                               CanUserSortColumns="False"
                               CanUserReorderColumns="False"
                               GridLinesVisibility="All">
                <controls:DataGrid.Columns>
                    <controls:DataGridTextColumn Header="Part Number"
                                                  Width="200"
                                                  Binding="{Binding PartNumber, Mode=OneWay}"
                                                  IsReadOnly="True"/>

                    <controls:DataGridTemplateColumn Header="Received Skids"
                                                      Width="150">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <NumberBox Value="{x:Bind ReceivedSkidCount, Mode=TwoWay}"
                                           Minimum="1"
                                           Maximum="99"
                                           SpinButtonPlacementMode="Inline"
                                           HorizontalAlignment="Stretch"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTextColumn Header="Calculated Pieces"
                                                  Width="150"
                                                  Binding="{Binding CalculatedPieceCount, Mode=OneWay}"
                                                  IsReadOnly="True"/>

                    <controls:DataGridCheckBoxColumn Header="Discrepancy"
                                                      Width="120"
                                                      Binding="{Binding HasDiscrepancy, Mode=TwoWay}"/>

                    <controls:DataGridTemplateColumn Header="Expected Skids"
                                                      Width="150">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <NumberBox Value="{Binding ExpectedSkidCount, Mode=TwoWay, Converter={StaticResource NullableDoubleToDoubleConverter}}"
                                           Minimum="0"
                                           SpinButtonPlacementMode="Inline"
                                           HorizontalAlignment="Stretch"
                                           IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay}"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTextColumn Header="Expected Pieces"
                                                  Width="130"
                                                  Binding="{Binding ExpectedPieceCount, Mode=OneWay}"
                                                  IsReadOnly="True"/>

                    <controls:DataGridTextColumn Header="Difference"
                                                  Width="100"
                                                  Binding="{Binding PieceDifference, Mode=OneWay}"
                                                  IsReadOnly="True"/>

                    <controls:DataGridTemplateColumn Header="Discrepancy Note"
                                                      Width="*">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBox Text="{x:Bind DiscrepancyNote, Mode=TwoWay}"
                                         IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay}"
                                         PlaceholderText="Explain discrepancy"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                </controls:DataGrid.Columns>
            </controls:DataGrid>
        </Grid>

        <!-- Action Buttons -->
        <CommandBar Grid.Row="3" DefaultLabelPosition="Right">
            <AppBarButton Icon="Delete" Label="Remove Part" Command="{x:Bind ViewModel.RemovePartCommand}"/>
            <AppBarSeparator/>
            <AppBarButton Label="Generate Labels" Command="{x:Bind ViewModel.GenerateLabelsCommand}">
                <AppBarButton.Icon>
                    <FontIcon Glyph="&#xE8B7;"/>
                </AppBarButton.Icon>
            </AppBarButton>
            <AppBarButton Icon="Mail" Label="Preview Email" Command="{x:Bind ViewModel.PreviewEmailCommand}"/>
            <AppBarSeparator/>
            <AppBarButton Icon="Save" Label="Save as Pending" Command="{x:Bind ViewModel.SaveAsPendingCommand}"/>
            <AppBarButton Icon="Accept" 
                          Label="Complete Shipment" 
                          Command="{x:Bind ViewModel.CompleteShipmentCommand}"
                          Visibility="{x:Bind ViewModel.HasPendingShipment, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <AppBarSeparator/>
            <AppBarButton Icon="Calendar" Label="View History" Command="{x:Bind ViewModel.ViewHistoryCommand}"/>
        </CommandBar>

        <!-- Status Bar -->
        <Grid Grid.Row="3" Margin="0,8,0,0" Visibility="Collapsed">
            <StackPanel Orientation="Horizontal" Spacing="8">
                <ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}" 
                              Width="20" 
                              Height="20"
                              Visibility="{x:Bind ViewModel.IsBusy, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" 
                           VerticalAlignment="Center"/>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Core/Data/Authentication/Dao_User.cs

```csharp
public class Dao_User
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public virtual async Task<Model_Dao_Result<Model_User>> GetUserByWindowsUsernameAsync(string windowsUsername)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public virtual async Task<Model_Dao_Result<Model_User>> ValidateUserPinAsync(string username, string pin)
⋮----
public virtual async Task<Model_Dao_Result<int>> CreateNewUserAsync(Model_User user, string createdBy)
⋮----
await using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync();
await using var command = new MySqlCommand("sp_Auth_User_Create", connection)
⋮----
command.Parameters.AddWithValue("@p_employee_number", user.EmployeeNumber);
command.Parameters.AddWithValue("@p_windows_username", user.WindowsUsername);
command.Parameters.AddWithValue("@p_full_name", user.FullName);
command.Parameters.AddWithValue("@p_pin", user.Pin);
command.Parameters.AddWithValue("@p_department", user.Department);
command.Parameters.AddWithValue("@p_shift", user.Shift);
command.Parameters.AddWithValue("@p_created_by", createdBy);
command.Parameters.AddWithValue("@p_visual_username", user.VisualUsername ?? (object)DBNull.Value);
command.Parameters.AddWithValue("@p_visual_password", user.VisualPassword ?? (object)DBNull.Value);
var errorMessageParam = new MySqlParameter("@p_error_message", MySqlDbType.VarChar, 500)
⋮----
command.Parameters.Add(errorMessageParam);
await command.ExecuteNonQueryAsync();
⋮----
if (!string.IsNullOrEmpty(errorMessage))
⋮----
await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result<bool>> IsWindowsUsernameUniqueAsync(string username, int? excludeEmployeeNumber = null)
⋮----
await using var command = new MySqlCommand(query, connection);
command.Parameters.AddWithValue("@username", username);
⋮----
command.Parameters.AddWithValue("@excludeId", excludeEmployeeNumber.Value);
⋮----
var count = Convert.ToInt32(await command.ExecuteScalarAsync());
⋮----
public virtual async Task<Model_Dao_Result<bool>> LogUserActivityAsync(
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result<List<string>>> GetSharedTerminalNamesAsync()
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
reader => reader.GetString(reader.GetOrdinal("workstation_name"))
⋮----
public virtual async Task<Model_Dao_Result> UpsertWorkstationConfigAsync(
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result<List<string>>> GetActiveDepartmentsAsync()
⋮----
reader => reader.GetString(reader.GetOrdinal("department_name"))
⋮----
private Model_User MapReaderToUser(IDataReader reader)
⋮----
return new Model_User
⋮----
EmployeeNumber = reader.GetInt32(reader.GetOrdinal("employee_number")),
WindowsUsername = reader.GetString(reader.GetOrdinal("windows_username")),
FullName = reader.GetString(reader.GetOrdinal("full_name")),
Pin = reader.GetString(reader.GetOrdinal("pin")),
Department = reader.GetString(reader.GetOrdinal("department")),
Shift = reader.GetString(reader.GetOrdinal("shift")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
VisualUsername = reader.IsDBNull(reader.GetOrdinal("visual_username"))
⋮----
: reader.GetString(reader.GetOrdinal("visual_username")),
VisualPassword = reader.IsDBNull(reader.GetOrdinal("visual_password"))
⋮----
: reader.GetString(reader.GetOrdinal("visual_password")),
⋮----
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by"))
⋮----
: reader.GetString(reader.GetOrdinal("created_by")),
ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date"))
⋮----
public virtual async Task<Model_Dao_Result> UpdateDefaultModeAsync(int userId, string? defaultMode)
⋮----
public virtual async Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(int userId, string? defaultMode)
⋮----
public virtual async Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(int userId, string? defaultMode)
⋮----
private static string? TryGetDefaultReceivingMode(IDataReader reader)
⋮----
var ordinal = reader.GetOrdinal("default_receiving_mode");
return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
⋮----
private static string? TryGetDefaultDunnageMode(IDataReader reader)
⋮----
var ordinal = reader.GetOrdinal("default_dunnage_mode");
```

## File: Module_Volvo/Services/Service_Volvo.cs

```csharp
public class Service_Volvo : IService_Volvo
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
private readonly IService_LoggingUtility _logger;
private readonly IService_VolvoAuthorization _authService;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
_lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
_componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
⋮----
public async Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
⋮----
await _logger.LogInfoAsync("Calculating component explosion for shipment lines");
⋮----
var partResult = await _partDao.GetByIdAsync(line.PartNumber);
⋮----
if (aggregatedPieces.ContainsKey(line.PartNumber))
⋮----
var componentsResult = await _componentDao.GetByParentPartAsync(line.PartNumber);
⋮----
await _logger.LogWarningAsync(
⋮----
if (aggregatedPieces.ContainsKey(component.ComponentPartNumber))
⋮----
await _logger.LogInfoAsync($"Component explosion complete: {aggregatedPieces.Count} unique parts");
⋮----
await _logger.LogErrorAsync($"Error calculating component explosion: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(int shipmentId)
⋮----
var authResult = await _authService.CanGenerateLabelsAsync();
⋮----
await _logger.LogInfoAsync($"Generating label CSV for shipment {shipmentId}");
var shipmentResult = await _shipmentDao.GetByIdAsync(shipmentId);
⋮----
var linesResult = await _lineDao.GetByShipmentIdAsync(shipmentId);
⋮----
string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string csvDirectory = Path.Combine(appDataPath, "MTM_Receiving_Application", "Volvo", "Labels");
Directory.CreateDirectory(csvDirectory);
⋮----
string filePath = Path.Combine(csvDirectory, fileName);
var csvContent = new StringBuilder();
csvContent.AppendLine("Material,Quantity,Employee,Date,Time,Receiver,Notes");
string dateFormatted = shipment.ShipmentDate.ToString("MM/dd/yyyy");
string timeFormatted = DateTime.Now.ToString("HH:mm:ss");
foreach (var kvp in aggregatedPieces.OrderBy(x => x.Key))
⋮----
csvContent.AppendLine($"{kvp.Key},{kvp.Value},{shipment.EmployeeNumber},{dateFormatted},{timeFormatted},,");
⋮----
await File.WriteAllTextAsync(filePath, csvContent.ToString());
await _logger.LogInfoAsync($"Label CSV generated: {filePath}");
⋮----
await _logger.LogErrorAsync($"Error generating label CSV: {ex.Message}", ex);
⋮----
public async Task<string> FormatEmailTextAsync(
⋮----
var emailText = new StringBuilder();
emailText.AppendLine($"Subject: PO Requisition - Volvo Dunnage - {shipment.ShipmentDate:MM/dd/yyyy} Shipment #{shipment.ShipmentNumber}");
emailText.AppendLine();
emailText.AppendLine("Good morning,");
⋮----
emailText.AppendLine($"Please create a PO for the following Volvo dunnage received on {shipment.ShipmentDate:MM/dd/yyyy}:");
⋮----
var discrepancies = lines.Where(l => l.HasDiscrepancy).ToList();
⋮----
emailText.AppendLine("**DISCREPANCIES NOTED**");
⋮----
emailText.AppendLine("Part Number\tPacklist Qty (pcs)\tReceived Qty (pcs)\tDifference (pcs)\tNote");
emailText.AppendLine(new string('-', 80));
⋮----
string diffStr = difference > 0 ? $"+{difference}" : difference.ToString();
emailText.AppendLine($"{line.PartNumber}\t{expectedPieces}\t{receivedPieces}\t{diffStr}\t{line.DiscrepancyNote ?? ""}");
⋮----
emailText.AppendLine("Requested Lines:");
⋮----
emailText.AppendLine("Part Number\tQuantity (pcs)");
emailText.AppendLine(new string('-', 40));
foreach (var kvp in requestedLines.OrderBy(x => x.Key))
⋮----
emailText.AppendLine($"{kvp.Key}\t{kvp.Value}");
⋮----
if (!string.IsNullOrWhiteSpace(shipment.Notes))
⋮----
emailText.AppendLine("Additional Notes:");
emailText.AppendLine(shipment.Notes);
⋮----
await _logger.LogInfoAsync("Email text formatted");
return emailText.ToString();
⋮----
public async Task<Model_VolvoEmailData> FormatEmailDataAsync(
⋮----
var emailData = new Model_VolvoEmailData
⋮----
AdditionalNotes = string.IsNullOrWhiteSpace(shipment.Notes) ? null : shipment.Notes,
⋮----
emailData.Discrepancies.Add(new Model_VolvoEmailData.DiscrepancyLineItem
⋮----
await _logger.LogInfoAsync("Email data formatted");
⋮----
public string FormatEmailAsHtml(Model_VolvoEmailData emailData)
⋮----
var html = new StringBuilder();
html.AppendLine("<html>");
html.AppendLine("<body style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>");
html.AppendLine($"<p>{emailData.Greeting}</p>");
html.AppendLine($"<p>{emailData.Message}</p>");
⋮----
html.AppendLine("<p><strong>**DISCREPANCIES NOTED**</strong></p>");
html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; font-size: 10pt;'>");
html.AppendLine("<thead>");
html.AppendLine("<tr style='background-color: #D9D9D9; font-weight: bold;'>");
html.AppendLine("<th>Part Number</th>");
html.AppendLine("<th>Packlist Qty</th>");
html.AppendLine("<th>Received Qty</th>");
html.AppendLine("<th>Difference</th>");
html.AppendLine("<th>Note</th>");
html.AppendLine("</tr>");
html.AppendLine("</thead>");
html.AppendLine("<tbody>");
⋮----
string diffStr = disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
html.AppendLine("<tr>");
html.AppendLine($"<td>{disc.PartNumber}</td>");
html.AppendLine($"<td>{disc.PacklistQty}</td>");
html.AppendLine($"<td>{disc.ReceivedQty}</td>");
html.AppendLine($"<td>{diffStr}</td>");
html.AppendLine($"<td>{disc.Note}</td>");
⋮----
html.AppendLine("</tbody>");
html.AppendLine("</table>");
html.AppendLine("<br/>");
⋮----
html.AppendLine("<p><strong>Requested Lines:</strong></p>");
⋮----
html.AppendLine("<th>Quantity (pcs)</th>");
⋮----
foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
⋮----
html.AppendLine($"<td>{kvp.Key}</td>");
html.AppendLine($"<td>{kvp.Value}</td>");
⋮----
if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
⋮----
html.AppendLine("<p><strong>Additional Notes:</strong></p>");
html.AppendLine($"<p>{emailData.AdditionalNotes}</p>");
⋮----
html.AppendLine($"<p>{emailData.Signature.Replace("\\n", "<br/>")}</p>");
html.AppendLine("</body>");
html.AppendLine("</html>");
return html.ToString();
⋮----
/// <summary>
/// Validates shipment data before save
/// Centralized validation logic for data integrity
/// </summary>
/// <param name="shipment"></param>
public async Task<Model_Dao_Result> ValidateShipmentAsync(
⋮----
return Model_Dao_Result_Factory.Failure("At least one part line is required");
⋮----
if (string.IsNullOrWhiteSpace(line.PartNumber))
⋮----
return Model_Dao_Result_Factory.Failure("All parts must have a part number");
⋮----
return Model_Dao_Result_Factory.Failure(
⋮----
if (shipment.ShipmentDate > DateTime.Now.AddDays(1))
⋮----
return Model_Dao_Result_Factory.Failure("Shipment date cannot be in the future");
⋮----
await _logger.LogInfoAsync("Shipment validation passed");
return Model_Dao_Result_Factory.Success();
⋮----
public async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
⋮----
var authResult = await _authService.CanManageShipmentsAsync();
⋮----
await _logger.LogInfoAsync("Saving Volvo shipment as pending PO");
var existingPendingResult = await _shipmentDao.GetPendingAsync();
⋮----
await _logger.LogInfoAsync($"Deleting existing pending shipment #{existingPendingResult.Data.ShipmentNumber}");
var deleteResult = await _shipmentDao.DeleteAsync(existingPendingResult.Data.Id);
⋮----
var insertResult = await _shipmentDao.InsertAsync(shipment);
⋮----
await _logger.LogInfoAsync($"Starting line insertion transaction for shipment {shipmentId}, {lines.Count} lines to insert");
await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();
⋮----
await _logger.LogInfoAsync($"Inserting line {lineIndex}/{lines.Count}: " +
⋮----
$"Note={(!string.IsNullOrEmpty(line.DiscrepancyNote) ? "PROVIDED" : "NULL")}");
⋮----
await _logger.LogInfoAsync($"Parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
var lineResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
⋮----
await transaction.RollbackAsync();
await _logger.LogErrorAsync($"Failed to insert line {lineIndex} for part {line.PartNumber}");
await _logger.LogErrorAsync($"Error: {lineResult.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception Type: {lineResult.Exception.GetType().Name}");
await _logger.LogErrorAsync($"Exception Message: {lineResult.Exception.Message}");
⋮----
await _logger.LogErrorAsync($"Inner Exception: {lineResult.Exception.InnerException.Message}");
⋮----
await _logger.LogInfoAsync($"Line {lineIndex} inserted successfully");
⋮----
await transaction.CommitAsync();
await _logger.LogInfoAsync($"Transaction committed: Shipment {shipmentId} saved with {lines.Count} lines");
⋮----
await _logger.LogErrorAsync($"Transaction failed, rolled back: {ex.Message}", ex);
⋮----
await _logger.LogErrorAsync($"Error saving shipment: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync()
⋮----
var result = await _shipmentDao.GetPendingAsync();
⋮----
public async Task<Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>> GetPendingShipmentWithLinesAsync()
⋮----
var shipmentResult = await _shipmentDao.GetPendingAsync();
⋮----
var linesResult = await _lineDao.GetByShipmentIdAsync(shipmentResult.Data.Id);
⋮----
await _logger.LogErrorAsync($"Error getting pending shipment with lines: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber)
⋮----
var authResult = await _authService.CanCompleteShipmentsAsync();
⋮----
return new Model_Dao_Result
⋮----
await _logger.LogInfoAsync($"Completing shipment {shipmentId} with PO={poNumber}, Receiver={receiverNumber}");
if (string.IsNullOrWhiteSpace(poNumber))
⋮----
if (string.IsNullOrWhiteSpace(receiverNumber))
⋮----
var result = await _shipmentDao.CompleteAsync(shipmentId, poNumber, receiverNumber);
⋮----
await _logger.LogInfoAsync($"Shipment {shipmentId} completed successfully");
⋮----
await _logger.LogErrorAsync($"Error completing shipment: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetActivePartsAsync()
⋮----
return await _partDao.GetAllAsync(includeInactive: false);
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
⋮----
return await _shipmentDao.GetHistoryAsync(startDate, endDate, status);
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetShipmentLinesAsync(int shipmentId)
⋮----
return await _lineDao.GetByShipmentIdAsync(shipmentId);
⋮----
public async Task<Model_Dao_Result> UpdateShipmentAsync(
⋮----
var shipmentResult = await _shipmentDao.UpdateAsync(shipment);
⋮----
var existingLines = await _lineDao.GetByShipmentIdAsync(shipment.Id);
⋮----
await _lineDao.DeleteAsync(line.Id);
⋮----
var lineResult = await _lineDao.InsertAsync(line);
⋮----
await _logger.LogErrorAsync(
⋮----
if (shipment.Status == "completed" && !string.IsNullOrEmpty(shipment.PONumber))
⋮----
return Model_Dao_Result_Factory.Success("Shipment updated successfully");
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating shipment: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<string>> ExportHistoryToCsvAsync(
⋮----
var csv = new StringBuilder();
csv.AppendLine("ShipmentNumber,Date,PONumber,ReceiverNumber,Status,EmployeeNumber,Notes");
⋮----
csv.AppendLine($"{shipment.ShipmentNumber}," +
⋮----
return Model_Dao_Result_Factory.Success(csv.ToString());
⋮----
private string EscapeCsv(string? value)
⋮----
if (string.IsNullOrEmpty(value))
⋮----
if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
⋮----
return $"\"{value.Replace("\"", "\"\"")}\"";
```

## File: Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs

```csharp
public partial class ViewModel_Volvo_ShipmentEntry : ViewModel_Shared_Base
⋮----
private readonly IService_Volvo _volvoService;
private readonly IService_Window _windowService;
private readonly IService_UserSessionManager _sessionManager;
⋮----
public async Task InitializeAsync()
⋮----
var partsResult = await _volvoService.GetActivePartsAsync();
⋮----
AvailableParts.Clear();
⋮----
AvailableParts.Add(part);
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
var pendingResult = await _volvoService.GetPendingShipmentAsync();
⋮----
await _logger.LogErrorAsync($"Error initializing Volvo shipment entry: {ex.Message}", ex);
⋮----
private async Task LoadPendingShipmentAsync()
⋮----
ShipmentDate = new DateTimeOffset(shipment.ShipmentDate);
⋮----
var linesResult = await _volvoService.GetShipmentLinesAsync(shipment.Id);
⋮----
Parts.Clear();
⋮----
Parts.Add(line);
⋮----
public void UpdatePartSuggestions(string queryText)
⋮----
if (string.IsNullOrWhiteSpace(queryText))
⋮----
SuggestedParts.Clear();
⋮----
.Where(p => p.PartNumber.Contains(queryText, StringComparison.OrdinalIgnoreCase))
.Take(20)
.ToList();
⋮----
SuggestedParts.Add(part);
⋮----
var exactMatch = _allParts.FirstOrDefault(p =>
p.PartNumber.Equals(queryText, StringComparison.OrdinalIgnoreCase));
⋮----
public void OnPartSuggestionChosen(Model_VolvoPart? chosenPart)
⋮----
partial void OnPartSearchTextChanged(string value)
⋮----
private async void AddPart()
⋮----
if (Parts.Any(p => p.PartNumber.Equals(SelectedPartToAdd.PartNumber, StringComparison.OrdinalIgnoreCase)))
⋮----
var newLine = new Model_VolvoShipmentLine
⋮----
Parts.Add(newLine);
await _logger.LogInfoAsync($"User added part {SelectedPartToAdd.PartNumber}, {ReceivedSkidsToAdd} skids ({calculatedPieces} pcs)");
⋮----
private bool CanAddPart()
⋮----
private async void RemovePart()
⋮----
await _logger.LogInfoAsync($"User removed part {SelectedPart.PartNumber} from shipment");
Parts.Remove(SelectedPart);
⋮----
private async Task GenerateLabelsAsync()
⋮----
var csvResult = await _volvoService.GenerateLabelCsvAsync(pendingResult.Data.Id);
⋮----
await _logger.LogErrorAsync($"Error generating labels: {ex.Message}", ex);
⋮----
private async Task PreviewEmailAsync()
⋮----
var pendingResult = await _volvoService.GetPendingShipmentWithLinesAsync();
⋮----
var explosionResult = await _volvoService.CalculateComponentExplosionAsync(lines);
⋮----
await _logger.LogInfoAsync($"Component explosion successful: {requestedLines.Count} parts");
⋮----
await _logger.LogWarningAsync($"Component explosion failed for email preview: {explosionResult.ErrorMessage}");
⋮----
await _logger.LogInfoAsync($"Email preview - RequestedLines count: {requestedLines.Count}");
var emailData = await _volvoService.FormatEmailDataAsync(shipment, lines, requestedLines);
await _logger.LogInfoAsync($"Email data formatted - RequestedLines count: {emailData.RequestedLines.Count}");
⋮----
await _logger.LogErrorAsync($"Error previewing email: {ex.Message}", ex);
⋮----
private async Task ShowEmailPreviewDialogAsync(Model_VolvoEmailData emailData)
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
var settingsDao = new Data.Dao_VolvoSettings(Module_Core.Helpers.Database.Helper_Database_Variables.GetConnectionString());
var toResult = await settingsDao.GetSettingAsync("email_to_recipients");
var ccResult = await settingsDao.GetSettingAsync("email_cc_recipients");
⋮----
var dialog = new ContentDialog
⋮----
var mainStack = new StackPanel { Spacing = 12, Margin = new Microsoft.UI.Xaml.Thickness(0) };
var toPanel = new Grid { ColumnSpacing = 8 };
toPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
toPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
var toBox = new TextBox
⋮----
toBox.SetValue(Grid.ColumnProperty, 0);
var toCopyButton = new Button
⋮----
toCopyButton.SetValue(Grid.ColumnProperty, 1);
Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(toCopyButton, "Copy To Recipients");
⋮----
var dataPackage = new DataPackage();
dataPackage.SetText(toBox.Text);
Clipboard.SetContent(dataPackage);
⋮----
toPanel.Children.Add(toBox);
toPanel.Children.Add(toCopyButton);
mainStack.Children.Add(toPanel);
var ccPanel = new Grid { ColumnSpacing = 8 };
ccPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
ccPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
var ccBox = new TextBox
⋮----
ccBox.SetValue(Grid.ColumnProperty, 0);
var ccCopyButton = new Button
⋮----
ccCopyButton.SetValue(Grid.ColumnProperty, 1);
Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(ccCopyButton, "Copy CC Recipients");
⋮----
dataPackage.SetText(ccBox.Text);
⋮----
ccPanel.Children.Add(ccBox);
ccPanel.Children.Add(ccCopyButton);
mainStack.Children.Add(ccPanel);
var subjectPanel = new Grid { ColumnSpacing = 8 };
subjectPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
subjectPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
var subjectBox = new TextBox
⋮----
subjectBox.SetValue(Grid.ColumnProperty, 0);
var subjectCopyButton = new Button
⋮----
subjectCopyButton.SetValue(Grid.ColumnProperty, 1);
Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(subjectCopyButton, "Copy Subject");
⋮----
dataPackage.SetText(subjectBox.Text);
⋮----
subjectPanel.Children.Add(subjectBox);
subjectPanel.Children.Add(subjectCopyButton);
mainStack.Children.Add(subjectPanel);
⋮----
var discHeader = new TextBlock
⋮----
mainStack.Children.Add(discHeader);
var discBox = new TextBox
⋮----
var discText = new StringBuilder();
discText.AppendLine("Part Number\tPacklist Qty (pcs)\tReceived Qty (pcs)\tDifference (pcs)\tNote");
discText.AppendLine(new string('-', 80));
⋮----
string diffStr = disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
discText.AppendLine($"{disc.PartNumber}\t{disc.PacklistQty}\t{disc.ReceivedQty}\t{diffStr}\t{disc.Note}");
⋮----
discBox.Text = discText.ToString();
mainStack.Children.Add(discBox);
⋮----
var reqHeader = new TextBlock
⋮----
mainStack.Children.Add(reqHeader);
var reqBox = new TextBox
⋮----
var reqText = new StringBuilder();
reqText.AppendLine("Part Number\tQuantity (pcs)");
reqText.AppendLine(new string('-', 40));
foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
⋮----
reqText.AppendLine($"{kvp.Key}\t{kvp.Value}");
⋮----
reqBox.Text = reqText.ToString();
mainStack.Children.Add(reqBox);
if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
⋮----
var notesBox = new TextBox
⋮----
mainStack.Children.Add(notesBox);
⋮----
var scrollViewer = new ScrollViewer
⋮----
var result = await dialog.ShowAsync();
⋮----
var htmlContent = _volvoService.FormatEmailAsHtml(emailData);
⋮----
dataPackage.SetHtmlFormat(htmlContent);
⋮----
dataPackage.SetText(plainText);
⋮----
private string BuildPlainTextEmail(Model_VolvoEmailData emailData)
⋮----
var text = new StringBuilder();
text.AppendLine($"Subject: {emailData.Subject}");
text.AppendLine();
text.AppendLine(emailData.Greeting);
⋮----
text.AppendLine(emailData.Message);
⋮----
text.AppendLine("**DISCREPANCIES NOTED**");
⋮----
text.AppendLine("Part Number\tPacklist Qty\tReceived Qty\tDifference\tNote");
text.AppendLine(new string('-', 80));
⋮----
text.AppendLine($"{disc.PartNumber}\t{disc.PacklistQty}\t{disc.ReceivedQty}\t{diffStr}\t{disc.Note}");
⋮----
text.AppendLine("Requested Lines:");
⋮----
text.AppendLine("Part Number\tQuantity (pcs)");
text.AppendLine(new string('-', 40));
⋮----
text.AppendLine($"{kvp.Key}\t{kvp.Value}");
⋮----
text.AppendLine("Additional Notes:");
text.AppendLine(emailData.AdditionalNotes);
⋮----
text.AppendLine(emailData.Signature);
return text.ToString();
⋮----
private void ViewHistory()
⋮----
_logger.LogInfo("Navigating to Volvo Shipment History");
⋮----
var contentFrame = mainWindow.GetContentFrame();
⋮----
private async Task SaveAsPendingAsync()
⋮----
await _logger.LogErrorAsync($"Error saving shipment: {ex.Message}", ex);
⋮----
private async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentInternalAsync()
⋮----
var shipment = new Model_VolvoShipment
⋮----
EmployeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber.ToString() ?? "0"
⋮----
var saveResult = await _volvoService.SaveShipmentAsync(shipment, Parts.ToList());
if (!saveResult.IsSuccess && saveResult.ErrorMessage.StartsWith("PENDING_EXISTS"))
⋮----
var parts = saveResult.ErrorMessage.Split('|');
⋮----
await _logger.LogInfoAsync($"User confirmed overwrite of pending shipment #{existingShipmentNumber}");
return await _volvoService.SaveShipmentAsync(shipment, Parts.ToList(), overwriteExisting: true);
⋮----
await _logger.LogInfoAsync("User canceled shipment overwrite");
⋮----
private async Task CompleteShipmentAsync()
⋮----
await _logger.LogErrorAsync($"Error completing shipment: {ex.Message}", ex);
⋮----
private async Task ShowCompletionDialogAsync(Model_VolvoShipment shipment)
⋮----
var stackPanel = new StackPanel { Spacing = 12 };
var poTextBox = new TextBox
⋮----
var receiverTextBox = new TextBox
⋮----
stackPanel.Children.Add(poTextBox);
stackPanel.Children.Add(receiverTextBox);
⋮----
if (string.IsNullOrWhiteSpace(poTextBox.Text) || string.IsNullOrWhiteSpace(receiverTextBox.Text))
⋮----
var completeResult = await _volvoService.CompleteShipmentAsync(
⋮----
poTextBox.Text.Trim(),
receiverTextBox.Text.Trim());
⋮----
private void StartNewEntry()
⋮----
private bool ValidateShipment()
⋮----
_errorHandler.HandleErrorAsync(
⋮----
true).ConfigureAwait(false);
⋮----
if (string.IsNullOrWhiteSpace(part.PartNumber))
⋮----
private string FormatRecipientsFromJson(string? jsonValue, string fallbackValue)
⋮----
if (string.IsNullOrWhiteSpace(jsonValue))
⋮----
return string.Join("; ", recipients.Select(r => r.ToOutlookFormat()));
⋮----
_logger.LogErrorAsync($"Error parsing email recipients JSON: {ex.Message}", ex).ConfigureAwait(false);
⋮----
private void ValidateSaveEligibility()
⋮----
CanSave = Parts.Count > 0 && Parts.All(p => !string.IsNullOrWhiteSpace(p.PartNumber) && p.ReceivedSkidCount > 0);
⋮----
partial void OnPartsChanged(ObservableCollection<Model_VolvoShipmentLine> value)
⋮----
partial void OnSelectedPartToAddChanged(Model_VolvoPart? value)
⋮----
AddPartCommand.NotifyCanExecuteChanged();
⋮----
partial void OnReceivedSkidsToAddChanged(int value)
⋮----
private void ClearShipmentForm()
```

## File: MainWindow.xaml.cs

```csharp
namespace MTM_Receiving_Application
⋮----
public sealed partial class MainWindow : Window
⋮----
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_LoggingUtility _logger;
⋮----
public Frame GetContentFrame() => ContentFrame;
⋮----
AppWindow.Resize(new Windows.Graphics.SizeInt32(1450, 900));
⋮----
rootElement.PointerMoved += (s, e) => _sessionManager.UpdateLastActivity();
rootElement.KeyDown += (s, e) => _sessionManager.UpdateLastActivity();
⋮----
private void MainWindow_Closed(object sender, WindowEventArgs args)
⋮----
Application.Current.Exit();
⋮----
private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
⋮----
_sessionManager.UpdateLastActivity();
⋮----
ContentFrame.Navigate(typeof(Module_Receiving.Views.View_Receiving_Workflow));
⋮----
private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
⋮----
ContentFrame.Navigate(typeof(Module_Dunnage.Views.View_Dunnage_WorkflowView));
⋮----
ContentFrame.Navigate(typeof(Module_Routing.Views.RoutingModeSelectionView));
⋮----
ContentFrame.Navigate(typeof(Module_Volvo.Views.View_Volvo_ShipmentEntry));
⋮----
ContentFrame.Navigate(typeof(Module_Volvo.Views.View_Volvo_History));
⋮----
ContentFrame.Navigate(typeof(Module_Reporting.Views.View_Reporting_Main));
⋮----
_logger.LogInfo("Navigating to Settings Database Test view", "MainWindow");
⋮----
_logger.LogError("ContentFrame is null - cannot navigate", null, "MainWindow");
throw new InvalidOperationException("ContentFrame is null");
⋮----
_logger.LogInfo($"ContentFrame state: IsLoaded={ContentFrame.IsLoaded}, BackStackDepth={ContentFrame.BackStackDepth}", "MainWindow");
⋮----
_logger.LogInfo($"View type: {viewType.FullName}", "MainWindow");
⋮----
_logger.LogInfo("Calling ContentFrame.Navigate()", "MainWindow");
var navigated = ContentFrame.Navigate(viewType);
_logger.LogInfo($"Navigation result: {navigated}", "MainWindow");
⋮----
_logger.LogError("Navigation returned false", null, "MainWindow");
⋮----
_logger.LogInfo("Successfully navigated to Settings Database Test view", "MainWindow");
⋮----
_logger.LogError($"Failed to navigate to Settings Database Test view: {ex.Message}", ex, "MainWindow");
⋮----
errorDetails.AppendLine("Failed to load Settings Database Test view:");
errorDetails.AppendLine();
errorDetails.AppendLine($"Error Type: {ex.GetType().FullName}");
errorDetails.AppendLine($"Message: {ex.Message}");
⋮----
errorDetails.AppendLine("Stack Trace:");
errorDetails.AppendLine(ex.StackTrace ?? "No stack trace available");
⋮----
errorDetails.AppendLine("Inner Exception:");
errorDetails.AppendLine($"Type: {ex.InnerException.GetType().FullName}");
errorDetails.AppendLine($"Message: {ex.InnerException.Message}");
errorDetails.AppendLine($"Stack: {ex.InnerException.StackTrace ?? "No stack trace"}");
⋮----
// Show detailed error to user in scrollable view
var scrollViewer = new ScrollViewer
⋮----
Content = new TextBlock
⋮----
Text = errorDetails.ToString(),
⋮----
var errorDialog = new ContentDialog
⋮----
_ = errorDialog.ShowAsync();
⋮----
private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
⋮----
DispatcherQueue.TryEnqueue(() =>
⋮----
public void UpdateUserDisplay()
⋮----
private void CenterWindow()
⋮----
AppWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
⋮----
private void ConfigureTitleBar()
⋮----
var transparentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
⋮----
var foregroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
⋮----
private void SetTitleBarDragRegion()
⋮----
AppWindow.TitleBar.SetDragRectangles(new[] { dragRect });
```

## File: App.xaml.cs

```csharp
public partial class App : Application
⋮----
private readonly IHost _host;
⋮----
Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.Enrich.WithMachineName()
.Enrich.WithThreadId()
.Enrich.WithProperty("Application", "MTM_Receiving_Application")
.Enrich.WithProperty("Environment", "Production")
.WriteTo.File(
⋮----
.CreateLogger();
_host = Host.CreateDefaultBuilder()
.ConfigureServices((context, services) =>
⋮----
services.AddMediatR(cfg =>
⋮----
cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
⋮----
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
⋮----
var mySqlConnectionString = Helper_Database_Variables.GetConnectionString();
var inforVisualConnectionString = Helper_Database_Variables.GetInforVisualConnectionString();
services.AddSingleton(sp => new Dao_User(mySqlConnectionString));
services.AddSingleton(sp => new Dao_ReceivingLoad(mySqlConnectionString));
services.AddSingleton(sp => new Dao_ReceivingLine(mySqlConnectionString));
services.AddSingleton(sp => new Dao_PackageTypePreference(mySqlConnectionString));
services.AddSingleton(sp => new Dao_DunnageLoad(mySqlConnectionString));
services.AddSingleton(sp => new Dao_DunnageType(mySqlConnectionString));
services.AddSingleton(sp => new Dao_DunnagePart(mySqlConnectionString));
services.AddSingleton(sp => new Dao_DunnageSpec(mySqlConnectionString));
services.AddSingleton(sp => new Dao_InventoriedDunnage(mySqlConnectionString));
services.AddSingleton(sp => new Dao_DunnageCustomField(mySqlConnectionString));
services.AddSingleton(sp => new Dao_DunnageUserPreference(mySqlConnectionString));
services.AddSingleton(sp => new Dao_RoutingLabel(mySqlConnectionString));
services.AddSingleton(sp => new Dao_RoutingRecipient(mySqlConnectionString));
services.AddSingleton(sp => new Dao_RoutingOtherReason(mySqlConnectionString));
services.AddSingleton(sp => new Dao_RoutingUsageTracking(mySqlConnectionString));
services.AddSingleton(sp => new Dao_RoutingUserPreference(mySqlConnectionString));
services.AddSingleton(sp => new Dao_RoutingLabelHistory(mySqlConnectionString));
services.AddSingleton(sp => new MTM_Receiving_Application.Module_Routing.Data.Dao_InforVisualPO(inforVisualConnectionString));
services.AddSingleton(sp => new Dao_VolvoShipment(mySqlConnectionString));
services.AddSingleton(sp => new Dao_VolvoShipmentLine(mySqlConnectionString));
services.AddSingleton(sp => new Dao_VolvoPart(mySqlConnectionString));
services.AddSingleton(sp => new Dao_VolvoPartComponent(mySqlConnectionString));
services.AddSingleton(sp => new Module_Settings.Data.Dao_SystemSettings(mySqlConnectionString));
services.AddSingleton(sp => new Module_Settings.Data.Dao_UserSettings(mySqlConnectionString));
services.AddSingleton(sp => new Module_Settings.Data.Dao_PackageType(mySqlConnectionString));
services.AddSingleton(sp => new Module_Settings.Data.Dao_PackageTypeMappings(mySqlConnectionString));
services.AddSingleton(sp => new Module_Settings.Data.Dao_RoutingRule(mySqlConnectionString));
services.AddSingleton(sp => new Module_Settings.Data.Dao_ScheduledReport(mySqlConnectionString));
services.AddSingleton(sp => new Module_Settings.Data.Dao_SettingsAuditLog(mySqlConnectionString));
⋮----
return new RoutingService(daoLabel, daoHistory, inforVisualService, usageTrackingService, recipientService, logger, configuration);
⋮----
return new RoutingInforVisualService(daoInforVisual, logger, useMockData);
⋮----
return new RoutingRecipientService(daoRecipient, logger);
⋮----
return new RoutingUsageTrackingService(daoUsageTracking, logger);
⋮----
return new RoutingUserPreferenceService(daoUserPreference, logger);
⋮----
services.AddSingleton(sp =>
⋮----
return new Dao_InforVisualPart(inforVisualConnectionString, logger);
⋮----
var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
return new Service_Dispatcher(dispatcherQueue);
⋮----
return new Service_Authentication(daoUser, errorHandler);
⋮----
return new Service_UserSessionManager(daoUser, dispatcherService);
⋮----
return new Dao_InforVisualConnection(
Helper_Database_Variables.GetInforVisualConnectionString(), logger);
⋮----
return new Service_InforVisualConnect(dao, useMockData, logger);
⋮----
services.AddSingleton<IService_MySQL_Receiving>(sp => { var logger = sp.GetRequiredService<IService_LoggingUtility>(); return new Service_MySQL_Receiving(Helper_Database_Variables.GetConnectionString(), logger); });
⋮----
services.AddSingleton<IService_MySQL_PackagePreferences>(sp => new Service_MySQL_PackagePreferences(Helper_Database_Variables.GetConnectionString()));
services.AddSingleton<IService_SessionManager>(sp => { var logger = sp.GetRequiredService<IService_LoggingUtility>(); return new Service_SessionManager(logger); });
services.AddSingleton<IService_CSVWriter>(sp => { var sessionManager = sp.GetRequiredService<IService_UserSessionManager>(); var logger = sp.GetRequiredService<IService_LoggingUtility>(); return new Service_CSVWriter(sessionManager, logger); });
⋮----
return new Service_VolvoAuthorization(logger);
⋮----
return new Service_Volvo(shipmentDao, lineDao, partDao, componentDao, logger, authService);
⋮----
return new Service_VolvoMasterData(partDao, componentDao, logger, errorHandler);
⋮----
services.AddSingleton(sp => new Dao_Reporting(mySqlConnectionString));
⋮----
return new Service_Reporting(dao, logger);
⋮----
.Build();
⋮----
protected override async void OnLaunched(LaunchActivatedEventArgs args)
⋮----
await startupService.StartAsync();
⋮----
private void OnSessionTimedOut(object? sender, Model_SessionTimedOutEventArgs e)
⋮----
private async void OnMainWindowClosed(object sender, WindowEventArgs args)
⋮----
await sessionManager.EndSessionAsync("manual_close");
⋮----
public static T GetService<T>() where T : class
⋮----
?? throw new InvalidOperationException($"Service {typeof(T)} not found");
```

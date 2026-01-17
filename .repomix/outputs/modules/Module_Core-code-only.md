# User Provided Header
MTM Receiving Application - Module_Core Code-Only Export

# Files

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

## File: Module_Core/Contracts/Services/IService_Focus.cs
```csharp
public interface IService_Focus
⋮----
public void SetFocus(Control control);
public void SetFocusFirstInput(DependencyObject container);
public void AttachFocusOnVisibility(FrameworkElement view, Control? targetControl = null);
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

## File: Module_Core/Contracts/ViewModels/IResettableViewModel.cs
```csharp
public interface IResettableViewModel
⋮----
public void ResetToDefaults();
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

## File: Module_Core/Models/Reporting/Model_ReportRow.cs
```csharp
public class Model_ReportRow
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
```
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

## File: Module_Core/Converters/Converter_IntToString.cs
```csharp
public class Converter_IntToString : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
return intValue.ToString();
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
if (value is string stringValue && int.TryParse(stringValue, out int result))
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

## File: Module_Core/Converters/Converter_InverseBool.cs
```csharp
public class Converter_InverseBool : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
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

## File: Module_Core/Converters/Converter_NullableDoubleToString.cs
```csharp
public class Converter_NullableDoubleToString : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
return doubleValue.ToString("F2");
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
⋮----
if (double.TryParse(stringValue, out double result))
```

## File: Module_Core/Converters/Converter_NullableIntToString.cs
```csharp
public class Converter_NullableIntToString : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
return intValue.ToString();
⋮----
public object ConvertBack(object? value, Type targetType, object? parameter, string language)
⋮----
if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
⋮----
if (int.TryParse(stringValue, out int result))
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

## File: Module_Core/Converters/NullableDoubleToDoubleConverter.cs
```csharp
public class NullableDoubleToDoubleConverter : IValueConverter
⋮----
public object Convert(object? value, Type targetType, object? parameter, string language)
⋮----
public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
```

## File: Module_Core/Defaults/InforVisualDefaults.cs
```csharp
public static class InforVisualDefaults
```

## File: Module_Core/Defaults/WorkstationDefaults.cs
```csharp
public static class WorkstationDefaults
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

## File: Module_Core/Helpers/UI/WindowHelper_WindowSizeAndStartupLocation.cs
```csharp
public static class WindowHelper_WindowSizeAndStartupLocation
⋮----
public static void SetWindowSize(Window window, int width, int height)
⋮----
window.SetWindowSize(width, height);
window.CenterOnScreen();
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

## File: Module_Core/Models/Systems/Model_User.cs
```csharp
public class Model_User
⋮----
public bool HasErpAccess => !string.IsNullOrWhiteSpace(VisualUsername);
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
await settingsFacade.InitializeDefaultsAsync(authenticatedUser.EmployeeNumber);
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

## File: Module_Core/Helpers/Database/Helper_Database_Variables.cs
```csharp
public static class Helper_Database_Variables
⋮----
public static string GetConnectionString(bool useProduction = true)
⋮----
public static string GetInforVisualConnectionString()
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

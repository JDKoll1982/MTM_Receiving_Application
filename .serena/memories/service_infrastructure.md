# Service Infrastructure Patterns

## Service Registration in App.xaml.cs

### Registration Types

**Singleton Services** (Single instance for application lifetime):

- Infrastructure services: `IService_ErrorHandler`, `IService_LoggingUtility`, `IService_Window`
- Workflow services: `IService_ReceivingWorkflow`, `IService_DunnageWorkflow`, `IService_DunnageAdminWorkflow`, `IService_SettingsWorkflow`
- Session management: `IService_SessionManager`, `IService_UserSessionManager`, `IService_Authentication`
- Navigation: `IService_Navigation`
- Data services: `IService_InforVisual`, `IService_MySQL_Receiving`, `IService_MySQL_PackagePreferences`
- All DAOs (registered with connection string factory pattern)

**Transient Services** (New instance per request):

- All ViewModels (every ViewModel is Transient)
- Some data services: `IService_MySQL_ReceivingLine`, `IService_MySQL_Dunnage`, `IService_DunnageCSVWriter`
- Pagination: `IService_Pagination`
- User preferences: `IService_UserPreferences`
- Startup services: `IService_OnStartup_AppLifecycle`
- All dialog Views

### DAO Registration Pattern

All DAOs are registered as Singletons with connection string from `Helper_Database_Variables`:

```csharp
services.AddSingleton(sp => new Dao_User(mySqlConnectionString));
services.AddSingleton(sp => new Dao_DunnageType(mySqlConnectionString));
```

### Service with Dependencies Pattern

```csharp
services.AddSingleton<IService_SessionManager>(sp => {
    var logger = sp.GetRequiredService<IService_LoggingUtility>();
    return new Service_SessionManager(logger);
});
```

## Service Interface Patterns

### Standard Service Methods

Services typically provide:

- Async operations (all methods suffixed with `Async`)
- Return `Task` for void operations
- Return `Task<Model_Dao_Result>` or `Task<Model_Dao_Result<T>>` for data operations
- Accept cancellation tokens where appropriate

### Workflow Services Pattern

Workflow services manage:

- Current workflow step (enum)
- Session state (current selection, entered data)
- Step navigation (`GoToStep`, `GoNext`, `GoBack`)
- Step validation
- Session completion/cancellation
- Events for step changes (`StepChanged` event)

### Error Handler Service Pattern

`IService_ErrorHandler` provides:

- `HandleErrorAsync()` - Logs and optionally shows dialog
- `LogErrorAsync()` - Logs only
- `ShowErrorDialogAsync()` - Shows user dialog
- `HandleDaoErrorAsync()` - Handles DAO result errors
- Compatibility aliases for older code

## Service Consumption Patterns

### In ViewModels

```csharp
public partial class MyViewModel : Shared_BaseViewModel
{
    private readonly IMyService _myService;
    
    public MyViewModel(
        IMyService myService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _myService = myService;
    }
}
```

### In Code-Behind (Views/Dialogs)

```csharp
public MyDialog()
{
    ViewModel = App.GetService<MyDialogViewModel>();
    InitializeComponent();
}

private var service = App.GetService<IMyService>();
```

### Service Retrieval

`App.GetService<T>()` static method retrieves any registered service.

## BaseViewModel Pattern

All ViewModels inherit from `Shared_BaseViewModel` which provides:

- `_errorHandler` - protected readonly IService_ErrorHandler
- `_logger` - protected readonly IService_LoggingUtility
- `IsBusy` - Observable boolean for loading states
- `StatusMessage` - Observable string for status bar
- `Title` - Observable string for page title

Constructor must call `base(errorHandler, logger)`.

## Help Service (IMPLEMENTED ✅)

**Registration**: Singleton in App.xaml.cs
**Interface**: `Contracts/Services/IService_Help.cs`
**Implementation**: `Services/Help/Service_Help.cs`
**Dialog View**: `Views/Shared/Shared_HelpDialog.xaml` (Transient)
**Dialog ViewModel**: `ViewModels/Shared/Shared_HelpDialogViewModel.cs` (Transient)

**Dependencies**:

- `IService_Window` - For XamlRoot when displaying dialogs
- `IService_LoggingUtility` - For logging help display events
- `IService_Dispatcher` - For UI thread dispatching

**Key Methods**:

- `ShowHelpAsync(string helpKey)` - Display help dialog
- `ShowContextualHelpAsync(Enum_DunnageWorkflowStep step)` - Workflow-specific help
- `ShowContextualHelpAsync(Enum_ReceivingWorkflowStep step)` - Workflow-specific help
- `GetHelpContent(string key)` - Retrieve help content object
- `GetHelpByCategory(string category)` - Get all help in category
- `SearchHelp(string searchTerm)` - Full-text search
- `IsDismissedAsync(string helpKey)` - Check if tip dismissed
- `SetDismissedAsync(string helpKey, bool isDismissed)` - Mark tip as dismissed

**Legacy Methods** (backward compatibility):

- `GetTooltip(string elementName)` - Returns tooltip text
- `GetPlaceholder(string fieldName)` - Returns placeholder text
- `GetTip(string viewName)` - Returns tip text
- `GetInfoBarMessage(string messageKey)` - Returns InfoBar message text
- `GetDunnageWorkflowHelp(Enum_DunnageWorkflowStep step)` - Returns workflow help text
- `GetReceivingWorkflowHelp(Enum_ReceivingWorkflowStep step)` - Returns workflow help text

**Content Organization**:

- All content loaded at service initialization into in-memory dictionary cache
- Hierarchical key structure (e.g., "Dunnage.PartSelection", "Tooltip.Button.Save")
- Categories: "Dunnage Workflow", "Receiving Workflow", "Admin"
- 100+ help entries covering tooltips, placeholders, tips, and contextual help

**Usage in ViewModels**:

```csharp
public partial class MyViewModel : BaseViewModel
{
    private readonly IService_Help _helpService;
    
    public MyViewModel(
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _helpService = helpService;
    }
    
    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowContextualHelpAsync(CurrentStep);
    }
    
    public string GetTooltip(string name) => _helpService.GetTooltip(name);
    public string GetPlaceholder(string name) => _helpService.GetPlaceholder(name);
}
```

**Usage in XAML**:

```xml
<!-- Direct binding for static content -->
<TextBox PlaceholderText="{x:Bind ViewModel.HelpService.GetPlaceholder('Field.PONumber'), Mode=OneTime}"/>

<!-- Command for contextual help -->
<Button Command="{x:Bind ViewModel.ShowHelpCommand}" Content="Help"/>
```

**Features**:

- In-memory content cache (O(1) lookups)
- Related topics navigation
- Copy content to clipboard
- Dismissible tips with session tracking
- Material Design icons
- Severity-based styling (Info, Warning, Critical)
- Full-text search capability

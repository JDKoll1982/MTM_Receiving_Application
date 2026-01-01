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

## Help Service Requirements

For the new Help Service:
1. **Should be Singleton** - Single instance manages all help content
2. **Should inject**: `IService_Window` (for XamlRoot), `IService_LoggingUtility` (for logging)
3. **Must be registered** in App.xaml.cs ConfigureServices before ViewModels
4. **Interface location**: `Contracts/Services/IService_Help.cs`
5. **Implementation location**: `Services/Help/Service_Help.cs` (new folder)
6. **Should provide**: 
   - Content retrieval methods
   - Dialog display method (ShowHelpAsync)
   - Content management (in-memory or from resource files)
7. **ViewModels should inject** IService_Help in constructor
8. **Dialog should be registered** as Transient in Views registration section

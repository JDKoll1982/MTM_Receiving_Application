---
description: Guidelines for dependency injection configuration in the MTM Receiving Application
applyTo: 'App.xaml.cs'
---

# Dependency Injection Guidelines

## Purpose

This file provides guidelines for configuring and using dependency injection in the WinUI 3 application.

## Setup Location

All DI configuration is in `App.xaml.cs`

## Service Lifetimes

### Singleton

- **One instance for the entire application**
- Use for: Services, logging, configuration, MainWindow

```csharp
services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
services.AddSingleton<ILoggingService, LoggingUtility>();
services.AddSingleton<MainWindow>();
```

### Transient

- **New instance every time it's requested**
- Use for: ViewModels, Views, short-lived operations

```csharp
services.AddTransient<ReceivingLabelViewModel>();
services.AddTransient<ReceivingLabelPage>();
```

### Scoped

- **Not typically used in WinUI 3** (no web request scope)
- Avoid unless you have a specific scoped lifetime need

## Current Configuration

```csharp
_host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        // Services (Singleton - one instance app-wide)
        services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
        services.AddSingleton<ILoggingService, LoggingUtility>();

        // ViewModels (Transient - new instance per request)
        services.AddTransient<ReceivingLabelViewModel>();
        services.AddTransient<DunnageLabelViewModel>();
        services.AddTransient<RoutingLabelViewModel>();

        // Views (Transient - new instance per request)
        services.AddTransient<ReceivingLabelPage>();
        services.AddTransient<DunnageLabelPage>();
        services.AddTransient<RoutingLabelPage>();

        // Main Window (Singleton - only one main window)
        services.AddSingleton<MainWindow>();
    })
    .Build();
```

## Adding New Services

### Step 1: Create the Interface

```csharp
// In Contracts/Services/IMyService.cs
public interface IMyService
{
    Task<string> DoSomethingAsync();
}
```

### Step 2: Create the Implementation

```csharp
// In Services/MyService.cs
public class MyService : IMyService
{
    private readonly ILoggingService _logger;

    public MyService(ILoggingService logger)
    {
        _logger = logger;
    }

    public async Task<string> DoSomethingAsync()
    {
        // Implementation
        return await Task.FromResult("Done");
    }
}
```

### Step 3: Register in DI Container

```csharp
// In App.xaml.cs ConfigureServices
services.AddSingleton<IMyService, MyService>();
```

## Adding New ViewModels/Views

### Step 1: Create ViewModel

```csharp
// ViewModels/MyFeature/MyViewModel.cs
public partial class MyViewModel : BaseViewModel
{
    public MyViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger,
        IMyService myService)  // Inject additional services
        : base(errorHandler, logger)
    {
        _myService = myService;
    }
}
```

### Step 2: Create View

```csharp
// Views/MyFeature/MyPage.xaml.cs
public sealed partial class MyPage : Page
{
    public MyViewModel ViewModel { get; }

    public MyPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<MyViewModel>();
        DataContext = ViewModel;
    }
}
```

### Step 3: Register Both

```csharp
// In App.xaml.cs ConfigureServices
services.AddTransient<MyViewModel>();
services.AddTransient<MyPage>();
```

## Retrieving Services

### From App (Static Method)

```csharp
var viewModel = App.GetService<MyViewModel>();
var service = App.GetService<IMyService>();
```

### Via Constructor Injection (Preferred)

```csharp
public class MyViewModel : BaseViewModel
{
    private readonly IMyService _myService;

    public MyViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger,
        IMyService myService)  // Automatically injected
        : base(errorHandler, logger)
    {
        _myService = myService;
    }
}
```

## Service Dependencies

Services can depend on other services:

```csharp
public class DatabaseService : IDatabaseService
{
    private readonly ILoggingService _logger;
    private readonly IService_ErrorHandler _errorHandler;

    public DatabaseService(
        ILoggingService logger,
        IService_ErrorHandler errorHandler)
    {
        _logger = logger;
        _errorHandler = errorHandler;
    }
}
```

## Circular Dependency Prevention

❌ **Avoid circular dependencies:**

```csharp
// BAD: A depends on B, B depends on A
public class ServiceA
{
    public ServiceA(IServiceB serviceB) { }
}

public class ServiceB
{
    public ServiceB(IServiceA serviceA) { }
}
```

✅ **Solution: Extract shared logic to a third service:**

```csharp
public class ServiceA
{
    public ServiceA(ISharedService shared) { }
}

public class ServiceB
{
    public ServiceB(ISharedService shared) { }
}
```

## Registration Order

Order doesn't matter for dependencies (DI resolves automatically), but group logically:

```csharp
.ConfigureServices((context, services) =>
{
    // 1. Core Services (logging, error handling, config)
    services.AddSingleton<ILoggingService, LoggingUtility>();
    services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();

    // 2. Business Services (database, API, etc.)
    services.AddSingleton<IDatabaseService, DatabaseService>();

    // 3. ViewModels
    services.AddTransient<MyViewModel>();

    // 4. Views
    services.AddTransient<MyPage>();

    // 5. Windows (last)
    services.AddSingleton<MainWindow>();
})
```

## Testing with DI

For unit tests, mock the dependencies:

```csharp
[TestMethod]
public void TestViewModel()
{
    // Arrange
    var mockLogger = new Mock<ILoggingService>();
    var mockErrorHandler = new Mock<IService_ErrorHandler>();

    var viewModel = new MyViewModel(
        mockErrorHandler.Object,
        mockLogger.Object);

    // Act & Assert
    // Test viewModel behavior
}
```

## Common Patterns

### Service with Configuration

```csharp
public class DatabaseService
{
    private readonly string _connectionString;
    private readonly ILoggingService _logger;

    public DatabaseService(
        IConfiguration configuration,
        ILoggingService logger)
    {
        _connectionString = configuration["ConnectionString"];
        _logger = logger;
    }
}
```

### Factory Pattern

```csharp
public interface IViewModelFactory
{
    T Create<T>() where T : BaseViewModel;
}

public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create<T>() where T : BaseViewModel
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
```

## Things to Avoid

❌ Don't use `new` to create ViewModels or Services
❌ Don't store IServiceProvider in ViewModels
❌ Don't create circular dependencies
❌ Don't forget to register new services
❌ Don't use Scoped lifetime in WinUI 3 (no scope context)
❌ Don't register the same interface twice (last registration wins)

## When to Add a New Service

Add a new service when:

- ✅ Logic is shared across multiple ViewModels
- ✅ Logic involves external resources (database, API, files)
- ✅ Logic is complex and needs isolation for testing
- ✅ Logic has state that should be shared

Don't create a service when:

- ❌ Logic is specific to one ViewModel
- ❌ Logic is simple property manipulation
- ❌ You're just trying to reduce ViewModel size

## Service Interface Benefits

Always use interfaces for services:

- ✅ Enables mocking in unit tests
- ✅ Allows swapping implementations
- ✅ Enforces contract
- ✅ Improves testability

```csharp
// Interface
public interface IDatabaseService
{
    Task<Model_Dao_Result<T>> SaveAsync<T>(T entity);
}

// Implementation
public class DatabaseService : IDatabaseService
{
    public async Task<Model_Dao_Result<T>> SaveAsync<T>(T entity)
    {
        // Implementation
    }
}

// Registration
services.AddSingleton<IDatabaseService, DatabaseService>();

// Usage in ViewModel
public MyViewModel(IDatabaseService database)
{
    _database = database;
}
```

## Debugging DI Issues

If service not found:

1. Check service is registered in `App.xaml.cs`
2. Check using correct lifetime (Singleton/Transient)
3. Check all constructor dependencies are registered
4. Check for circular dependencies
5. Check spelling of interface/implementation names

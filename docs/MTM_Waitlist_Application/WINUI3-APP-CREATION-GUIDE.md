# WinUI3 Application Creation Guide - MTM Waitlist Application

**Based on**: MTM Receiving Application Architecture  
**Target**: MTM Waitlist Application  
**Framework**: WinUI 3 (.NET 8)  
**Architecture**: MVVM with CQRS/MediatR  
**Created**: January 21, 2026

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Scaffolding](#project-scaffolding)
4. [Infrastructure Setup](#infrastructure-setup)
5. [Module Creation](#module-creation)
6. [Database Setup](#database-setup)
7. [Authentication & Privileges](#authentication--privileges)
8. [Workflow Implementation](#workflow-implementation)
9. [Testing Strategy](#testing-strategy)
10. [Build & Deployment](#build--deployment)
11. [Development Workflow](#development-workflow)
12. [Troubleshooting](#troubleshooting)

---

## Overview

This guide provides step-by-step instructions for creating the **MTM Waitlist Application**, a role-based production task management system using the proven patterns from MTM Receiving Application.

### Application Purpose

The MTM Waitlist Application manages production floor tasks across multiple roles:

- **Operators**: Create task requests (material, setup, quality, production)
- **Material Handlers**: Fulfill material handling requests by zone
- **Setup Technicians**: Handle die-related issues
- **Quality Control**: Manage quality inspections and issues
- **Production/Operator Leads**: Oversee operations with analytics
- **Plant Manager**: Full visibility across all operations

### Key Architecture Principles

✅ **Strict MVVM** - View → ViewModel → Service → DAO → Database  
✅ **Multi-Module** - Each role/feature is a separate module  
✅ **MySQL with Stored Procedures** - No raw SQL in C# code  
✅ **Infor Visual READ-ONLY** - Work order validation only  
✅ **CQRS Pattern** - Command/Query separation with MediatR  
✅ **Privilege-Based Access** - Role + privilege system  
✅ **Instance-Based DAOs** - No static classes  
✅ **x:Bind in XAML** - Compile-time binding only

---

## Prerequisites

### Development Environment

- **OS**: Windows 10 (1809+) or Windows 11
- **IDE**: Visual Studio 2022 (17.8+)
  - Workloads: .NET Desktop Development, UWP Development
- **SDK**: .NET 8 SDK (8.0.100+)
- **Windows App SDK**: 1.6+ (via NuGet)

### Database Requirements

- **MySQL Server 8.0+**
  - Database: `mtm_waitlist_application`
  - Character Set: utf8mb4
  - Connection: `172.16.1.104:3306`
- **SQL Server** (Infor Visual)
  - Server: `VISUAL`
  - Database: `MTMFG`
  - Connection: **READ-ONLY** (`ApplicationIntent=ReadOnly`)

### Development Tools

```powershell
# Install required tools
winget install Git.Git
winget install Microsoft.VisualStudio.2022.Community
winget install Oracle.MySQL

# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8
```

---

## Project Scaffolding

### Step 1: Create Solution Structure

```powershell
# Create solution directory
mkdir MTM_Waitlist_Application
cd MTM_Waitlist_Application

# Create .NET 8 WinUI 3 project
dotnet new install Microsoft.WindowsAppSDK.Templates
dotnet new wasdk -n MTM_Waitlist_Application -f net8.0-windows10.0.22621.0

# Create solution file
dotnet new sln -n MTM_Waitlist_Application
dotnet sln add MTM_Waitlist_Application.csproj

# Create test project
dotnet new xunit -n MTM_Waitlist_Application.Tests -f net8.0
dotnet sln add MTM_Waitlist_Application.Tests/MTM_Waitlist_Application.Tests.csproj
```

### Step 2: Create Folder Structure

```powershell
# Core infrastructure
mkdir Infrastructure/Configuration
mkdir Infrastructure/DependencyInjection
mkdir Infrastructure/Logging

# Module structure (replicate for each module)
$modules = @(
    "Module_Core",
    "Module_Login", 
    "Module_Operator",
    "Module_SetupTechnician",
    "Module_MaterialHandler",
    "Module_Quality",
    "Module_ProductionLead",
    "Module_PlantManager",
    "Module_WaitList",
    "Module_Analytics",
    "Module_Settings",
    "Module_Shared"
)

foreach ($module in $modules) {
    mkdir "$module/Views"
    mkdir "$module/ViewModels"
    mkdir "$module/Services"
    mkdir "$module/Data"
    mkdir "$module/Models"
    mkdir "$module/Contracts/Services"
    mkdir "$module/Contracts/Repositories"
}

# Additional folders
mkdir Assets/Fonts
mkdir Assets/Icons
mkdir Assets/Images
mkdir Database/Schemas
mkdir Database/StoredProcedures
mkdir Database/TestData
mkdir Scripts
mkdir docs
mkdir .github/instructions
mkdir .specify/templates
mkdir .serena/memories
```

### Step 3: Configure Project File

**File**: `MTM_Waitlist_Application.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.22621.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <RootNamespace>MTM_Waitlist_Application</RootNamespace>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <Platforms>x64;ARM64</Platforms>
    <RuntimeIdentifiers>win-x64;win-arm64</RuntimeIdentifiers>
    <UseWinUI>true</UseWinUI>
    <EnableMsixTooling>true</EnableMsixTooling>
    <Nullable>enable</Nullable>
    <WindowsPackageType>None</WindowsPackageType>
    <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
    
    <!-- Performance Optimizations -->
    <BuildInParallel>true</BuildInParallel>
    <MaxCpuCount>0</MaxCpuCount>
    <IncrementalBuild>true</IncrementalBuild>
    <Deterministic>true</Deterministic>
    <ProduceReferenceAssembly>false</ProduceReferenceAssembly>
  </PropertyGroup>

  <ItemGroup>
    <!-- Windows App SDK -->
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.250124002" />
    
    <!-- MVVM Toolkit -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
    
    <!-- CQRS Pattern -->
    <PackageReference Include="MediatR" Version="12.4.1" />
    <PackageReference Include="FluentValidation" Version="11.11.0" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
    
    <!-- Dependency Injection -->
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog" Version="4.1.0" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="8.0.4" />
    
    <!-- Database -->
    <PackageReference Include="MySql.Data" Version="9.1.0" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
    
    <!-- Testing -->
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
  </ItemGroup>

  <ItemGroup>
    <None Include="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="appsettings.*.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <DependentUpon>appsettings.json</DependentUpon>
    </None>
  </ItemGroup>

  <ItemGroup>
    <PRIResource Remove=".github\**" />
    <Compile Remove="MTM_Waitlist_Application.Tests\**" />
    <EmbeddedResource Remove="MTM_Waitlist_Application.Tests\**" />
    <None Remove="MTM_Waitlist_Application.Tests\**" />
    <Page Remove="MTM_Waitlist_Application.Tests\**" />
  </ItemGroup>
</Project>
```

### Step 4: Create Directory.Build.props

**File**: `Directory.Build.props`

```xml
<Project>
  <PropertyGroup>
    <!-- Default to x64 platform for WinUI 3 projects -->
    <Platform Condition="'$(Platform)' == ''">x64</Platform>
  </PropertyGroup>
</Project>
```

### Step 5: Create Configuration Files

**File**: `appsettings.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/waitlist-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "AppSettings": {
    "ApplicationName": "MTM Waitlist Application",
    "Version": "1.0.0",
    "Environment": "Development",
    "EnableDiagnostics": true
  },
  "ConnectionStrings": {
    "MySql": "Server=172.16.1.104;Port=3306;Database=mtm_waitlist_application;Uid=root;Pwd=root;CharSet=utf8mb4;",
    "InforVisual": "Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;"
  },
  "InforVisual": {
    "UseMockData": false,
    "ConnectionTimeout": 30
  },
  "Session": {
    "PersonalWorkstationTimeoutMinutes": 30,
    "SharedTerminalTimeoutMinutes": 15,
    "ActivityCheckIntervalSeconds": 60
  },
  "Privileges": {
    "EnableRoleBasedAccess": true,
    "LogAccessAttempts": true
  }
}
```

**File**: `appsettings.Development.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  },
  "AppSettings": {
    "Environment": "Development",
    "EnableDiagnostics": true
  },
  "InforVisual": {
    "UseMockData": false
  }
}
```

---

## Infrastructure Setup

### Step 1: Create App.xaml.cs with DI

**File**: `App.xaml.cs`

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Waitlist_Application.Infrastructure.DependencyInjection;
using MTM_Waitlist_Application.Infrastructure.Logging;
using Serilog;

namespace MTM_Waitlist_Application;

public partial class App : Application
{
    private readonly IHost _host;

    public static Window? MainWindow { get; internal set; }

    public App()
    {
        InitializeComponent();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) => 
                SerilogConfiguration.Configure(configuration, context.Configuration))
            .ConfigureServices((context, services) =>
            {
                // Register all services via extension methods
                services.AddCoreServices(context.Configuration);
                services.AddCqrsInfrastructure();
                services.AddModuleServices(context.Configuration);
            })
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var startupService = _host.Services
            .GetRequiredService<IService_OnStartup_AppLifecycle>();
        await startupService.StartAsync();
    }

    /// <summary>
    /// Service Locator - Use sparingly, prefer constructor injection
    /// </summary>
    public static T GetService<T>() where T : class
    {
        if (Current is not App app)
            throw new InvalidOperationException("Application instance not available");

        return app._host.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
    }
}
```

### Step 2: Create Serilog Configuration

**File**: `Infrastructure/Logging/SerilogConfiguration.cs`

```csharp
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MTM_Waitlist_Application.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static void Configure(LoggerConfiguration configuration, IConfiguration appConfiguration)
    {
        configuration.ReadFrom.Configuration(appConfiguration);
    }
}
```

### Step 3: Create Core Service Extensions

**File**: `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`

```csharp
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTM_Waitlist_Application.Module_Core.Contracts.Services;
using MTM_Waitlist_Application.Module_Core.Services;

namespace MTM_Waitlist_Application.Infrastructure.DependencyInjection;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Error Handling & Logging
        services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
        
        // UI Services
        services.AddSingleton<IService_Notification, Service_Notification>();
        services.AddSingleton<IService_Window, Service_Window>();
        services.AddSingleton<IService_Dispatcher>(_ =>
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue
                .GetForCurrentThread();
            return new Service_Dispatcher(dispatcherQueue);
        });

        // Navigation
        services.AddSingleton<IService_Navigation, Service_Navigation>();

        // Authentication & Privileges
        RegisterAuthenticationServices(services, configuration);

        // Infor Visual Integration
        RegisterInforVisualServices(services, configuration);

        return services;
    }

    private static void RegisterAuthenticationServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs
        services.AddSingleton(_ => new Dao_User(connectionString));
        services.AddSingleton(_ => new Dao_UserPrivilege(connectionString));
        services.AddSingleton(_ => new Dao_Role(connectionString));

        // Services
        services.AddSingleton<IService_Authentication, Service_Authentication>();
        services.AddSingleton<IService_PrivilegeManager, Service_PrivilegeManager>();
        services.AddSingleton<IService_UserSessionManager, Service_UserSessionManager>();
    }

    private static void RegisterInforVisualServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InforVisual")
            ?? throw new InvalidOperationException("InforVisual connection string not found");

        services.AddSingleton(sp => 
            new Dao_InforVisualWorkOrder(connectionString));
        services.AddSingleton<IService_InforVisual, Service_InforVisual>();
    }
}
```

### Step 4: Create CQRS Infrastructure Extensions

**File**: `Infrastructure/DependencyInjection/CqrsInfrastructureExtensions.cs`

```csharp
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MTM_Waitlist_Application.Module_Core.Behaviors;

namespace MTM_Waitlist_Application.Infrastructure.DependencyInjection;

public static class CqrsInfrastructureExtensions
{
    public static IServiceCollection AddCqrsInfrastructure(
        this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(App).Assembly);
            
            // Add pipeline behaviors (order matters: Logging → Validation → Audit)
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuditBehavior<,>));
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(App).Assembly);

        return services;
    }
}
```

### Step 5: Create Module Services Extensions

**File**: `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

```csharp
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Waitlist_Application.Infrastructure.DependencyInjection;

public static class ModuleServicesExtensions
{
    public static IServiceCollection AddModuleServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLoginModule(configuration);
        services.AddOperatorModule(configuration);
        services.AddMaterialHandlerModule(configuration);
        services.AddSetupTechnicianModule(configuration);
        services.AddQualityModule(configuration);
        services.AddProductionLeadModule(configuration);
        services.AddPlantManagerModule(configuration);
        services.AddWaitListModule(configuration);
        services.AddAnalyticsModule(configuration);
        services.AddSettingsModule(configuration);
        services.AddSharedModule(configuration);

        return services;
    }

    private static IServiceCollection AddLoginModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Login module services, DAOs, ViewModels
        // See Module Creation section for details
        return services;
    }

    // Add methods for each module...
}
```

---

## Module Creation

### Module Template Pattern

Each module follows this structure:

```
Module_<Name>/
├── Contracts/
│   └── Services/
│       └── IService_<Feature>.cs
├── Data/
│   └── Dao_<Entity>.cs
├── Models/
│   ├── Model_<Entity>.cs
│   └── Queries/
│       └── Get<Entity>Query.cs
│   └── Commands/
│       └── Create<Entity>Command.cs
├── Services/
│   └── Service_<Feature>.cs
├── ViewModels/
│   └── ViewModel_<Module>_<View>.cs
└── Views/
    └── View_<Module>_<View>.xaml
```

### Example: Operator Module

#### Step 1: Create Models

**File**: `Module_Operator/Models/Model_TaskRequest.cs`

```csharp
namespace MTM_Waitlist_Application.Module_Operator.Models;

public class Model_TaskRequest
{
    public int Id { get; set; }
    public int OperatorId { get; set; }
    public string RequestCategory { get; set; } = string.Empty; // "Material Handler", "Setup Tech", etc.
    public string RequestType { get; set; } = string.Empty;
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal"; // "Normal", "Urgent", "Critical"
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed", "Cancelled"
}
```

#### Step 2: Create CQRS Commands/Queries

**File**: `Module_Operator/Models/Commands/CreateTaskRequestCommand.cs`

```csharp
using MediatR;
using MTM_Waitlist_Application.Module_Core.Models;

namespace MTM_Waitlist_Application.Module_Operator.Models.Commands;

public record CreateTaskRequestCommand(
    int OperatorId,
    string RequestCategory,
    string RequestType,
    string WorkOrderNumber,
    string PartNumber,
    string Description,
    string Zone,
    string Priority
) : IRequest<Model_Dao_Result<int>>;
```

**File**: `Module_Operator/Models/Commands/CreateTaskRequestCommandHandler.cs`

```csharp
using MediatR;
using MTM_Waitlist_Application.Module_Core.Models;
using MTM_Waitlist_Application.Module_Operator.Data;

namespace MTM_Waitlist_Application.Module_Operator.Models.Commands;

public class CreateTaskRequestCommandHandler 
    : IRequestHandler<CreateTaskRequestCommand, Model_Dao_Result<int>>
{
    private readonly Dao_TaskRequest _dao;

    public CreateTaskRequestCommandHandler(Dao_TaskRequest dao)
    {
        _dao = dao;
    }

    public async Task<Model_Dao_Result<int>> Handle(
        CreateTaskRequestCommand request, 
        CancellationToken cancellationToken)
    {
        var taskRequest = new Model_TaskRequest
        {
            OperatorId = request.OperatorId,
            RequestCategory = request.RequestCategory,
            RequestType = request.RequestType,
            WorkOrderNumber = request.WorkOrderNumber,
            PartNumber = request.PartNumber,
            Description = request.Description,
            Zone = request.Zone,
            Priority = request.Priority,
            CreatedAt = DateTime.Now,
            Status = "Pending"
        };

        return await _dao.InsertAsync(taskRequest);
    }
}
```

#### Step 3: Create Validator

**File**: `Module_Operator/Models/Commands/CreateTaskRequestCommandValidator.cs`

```csharp
using FluentValidation;

namespace MTM_Waitlist_Application.Module_Operator.Models.Commands;

public class CreateTaskRequestCommandValidator 
    : AbstractValidator<CreateTaskRequestCommand>
{
    public CreateTaskRequestCommandValidator()
    {
        RuleFor(x => x.RequestCategory)
            .NotEmpty()
            .WithMessage("Request category is required");

        RuleFor(x => x.RequestType)
            .NotEmpty()
            .WithMessage("Request type is required");

        RuleFor(x => x.OperatorId)
            .GreaterThan(0)
            .WithMessage("Valid operator ID is required");

        RuleFor(x => x.Zone)
            .NotEmpty()
            .When(x => x.RequestCategory == "Material Handler")
            .WithMessage("Zone is required for material handling requests");
    }
}
```

#### Step 4: Create DAO

**File**: `Module_Operator/Data/Dao_TaskRequest.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Waitlist_Application.Module_Core.Helpers.Database;
using MTM_Waitlist_Application.Module_Core.Models;
using MTM_Waitlist_Application.Module_Operator.Models;
using MySql.Data.MySqlClient;

namespace MTM_Waitlist_Application.Module_Operator.Data;

public class Dao_TaskRequest
{
    private readonly string _connectionString;

    public Dao_TaskRequest(string connectionString)
    {
        _connectionString = connectionString 
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Model_Dao_Result<int>> InsertAsync(Model_TaskRequest request)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "operator_id", request.OperatorId },
                { "request_category", request.RequestCategory },
                { "request_type", request.RequestType },
                { "work_order_number", request.WorkOrderNumber },
                { "part_number", request.PartNumber },
                { "description", request.Description },
                { "zone", request.Zone },
                { "priority", request.Priority }
            };

            return await Helper_Database_StoredProcedure
                .ExecuteScalarAsync<int>(
                    _connectionString,
                    "sp_TaskRequest_Insert",
                    parameters);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<int>.Failure(
                $"Error inserting task request: {ex.Message}",
                ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_TaskRequest>>> GetByOperatorAsync(
        int operatorId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "operator_id", operatorId }
            };

            return await Helper_Database_StoredProcedure
                .ExecuteStoredProcedureAsync<Model_TaskRequest>(
                    _connectionString,
                    "sp_TaskRequest_GetByOperator",
                    parameters);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<List<Model_TaskRequest>>.Failure(
                $"Error retrieving task requests: {ex.Message}",
                ex);
        }
    }
}
```

#### Step 5: Create ViewModel

**File**: `Module_Operator/ViewModels/ViewModel_Operator_CreateRequest.cs`

```csharp
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Waitlist_Application.Module_Core.Contracts.Services;
using MTM_Waitlist_Application.Module_Shared.ViewModels;
using MTM_Waitlist_Application.Module_Operator.Models;
using MTM_Waitlist_Application.Module_Operator.Models.Commands;

namespace MTM_Waitlist_Application.Module_Operator.ViewModels;

public partial class ViewModel_Operator_CreateRequest : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private string _selectedCategory = string.Empty;

    [ObservableProperty]
    private string _selectedRequestType = string.Empty;

    [ObservableProperty]
    private string _workOrderNumber = string.Empty;

    [ObservableProperty]
    private string _partNumber = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _selectedZone = string.Empty;

    [ObservableProperty]
    private string _selectedPriority = "Normal";

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private ObservableCollection<string> _requestTypes = new();

    [ObservableProperty]
    private ObservableCollection<string> _zones = new();

    public ViewModel_Operator_CreateRequest(
        IMediator mediator,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler)
        : base(errorHandler)
    {
        _mediator = mediator;
        _sessionManager = sessionManager;

        InitializeCollections();
    }

    private void InitializeCollections()
    {
        Categories = new ObservableCollection<string>
        {
            "Material Handler",
            "Setup Technician",
            "Quality Control",
            "Production Lead"
        };

        Zones = new ObservableCollection<string>
        {
            "Zone 1", "Zone 2", "Zone 3", "Zone 4", "Zone 5"
        };
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        LoadRequestTypesForCategory(value);
    }

    private void LoadRequestTypesForCategory(string category)
    {
        RequestTypes.Clear();

        switch (category)
        {
            case "Material Handler":
                RequestTypes = new ObservableCollection<string>
                {
                    "Coils", "Flatstock", "Component Parts", 
                    "Dunnage", "Dies", "Scrap", "NCM"
                };
                break;
            case "Setup Technician":
                RequestTypes = new ObservableCollection<string>
                {
                    "Die Protection Alarm", "Die Stuck", 
                    "Die Misalignment", "Die Damage"
                };
                break;
            case "Quality Control":
                RequestTypes = new ObservableCollection<string>
                {
                    "Inspection Request", "Quality Question", "Other"
                };
                break;
            case "Production Lead":
                RequestTypes = new ObservableCollection<string>
                {
                    "Production Question", "Safety/Injury Report", "Other"
                };
                break;
        }
    }

    [RelayCommand]
    private async Task SubmitRequestAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Submitting request...";

            var user = _sessionManager.GetCurrentUser();
            if (user == null)
            {
                _errorHandler.ShowUserError(
                    "No active user session",
                    "Authentication Error",
                    nameof(SubmitRequestAsync));
                return;
            }

            var command = new CreateTaskRequestCommand(
                user.UserId,
                SelectedCategory,
                SelectedRequestType,
                WorkOrderNumber,
                PartNumber,
                Description,
                SelectedZone,
                SelectedPriority
            );

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                StatusMessage = "Request submitted successfully";
                ClearForm();
                // TODO: Navigate back or show confirmation
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage,
                    "Submission Error",
                    nameof(SubmitRequestAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SubmitRequestAsync),
                nameof(ViewModel_Operator_CreateRequest));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearForm()
    {
        SelectedCategory = string.Empty;
        SelectedRequestType = string.Empty;
        WorkOrderNumber = string.Empty;
        PartNumber = string.Empty;
        Description = string.Empty;
        SelectedZone = string.Empty;
        SelectedPriority = "Normal";
    }
}
```

#### Step 6: Create View

**File**: `Module_Operator/Views/View_Operator_CreateRequest.xaml`

```xml
<Page
    x:Class="MTM_Waitlist_Application.Module_Operator.Views.View_Operator_CreateRequest"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Waitlist_Application.Module_Operator.ViewModels">

    <Grid Padding="20">
        <StackPanel Spacing="16" MaxWidth="600">
            <TextBlock 
                Text="Create Task Request" 
                Style="{StaticResource TitleTextBlockStyle}"/>

            <ComboBox
                Header="Request Category"
                ItemsSource="{x:Bind ViewModel.Categories, Mode=OneWay}"
                SelectedItem="{x:Bind ViewModel.SelectedCategory, Mode=TwoWay}"
                PlaceholderText="Select category..."
                HorizontalAlignment="Stretch"/>

            <ComboBox
                Header="Request Type"
                ItemsSource="{x:Bind ViewModel.RequestTypes, Mode=OneWay}"
                SelectedItem="{x:Bind ViewModel.SelectedRequestType, Mode=TwoWay}"
                PlaceholderText="Select request type..."
                HorizontalAlignment="Stretch"
                IsEnabled="{x:Bind ViewModel.Categories.Count, Mode=OneWay, Converter={StaticResource GreaterThanZeroConverter}}"/>

            <TextBox
                Header="Work Order Number"
                Text="{x:Bind ViewModel.WorkOrderNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="Enter work order number..."/>

            <TextBox
                Header="Part Number"
                Text="{x:Bind ViewModel.PartNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="Enter part number..."/>

            <TextBox
                Header="Description"
                Text="{x:Bind ViewModel.Description, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="Enter additional details..."
                AcceptsReturn="True"
                Height="100"/>

            <ComboBox
                Header="Zone"
                ItemsSource="{x:Bind ViewModel.Zones, Mode=OneWay}"
                SelectedItem="{x:Bind ViewModel.SelectedZone, Mode=TwoWay}"
                PlaceholderText="Select zone..."
                HorizontalAlignment="Stretch"
                Visibility="{x:Bind ViewModel.SelectedCategory, Mode=OneWay, Converter={StaticResource MaterialHandlerVisibilityConverter}}"/>

            <ComboBox
                Header="Priority"
                SelectedItem="{x:Bind ViewModel.SelectedPriority, Mode=TwoWay}"
                HorizontalAlignment="Stretch">
                <x:String>Normal</x:String>
                <x:String>Urgent</x:String>
                <x:String>Critical</x:String>
            </ComboBox>

            <StackPanel Orientation="Horizontal" Spacing="8">
                <Button
                    Content="Submit Request"
                    Command="{x:Bind ViewModel.SubmitRequestCommand}"
                    IsEnabled="{x:Bind ViewModel.IsBusy, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}"
                    Style="{StaticResource AccentButtonStyle}"/>
                <Button
                    Content="Cancel"
                    Command="{x:Bind ViewModel.CancelCommand}"/>
            </StackPanel>

            <TextBlock 
                Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                Foreground="{ThemeResource SystemAccentColor}"/>
        </StackPanel>
    </Grid>
</Page>
```

**File**: `Module_Operator/Views/View_Operator_CreateRequest.xaml.cs`

```csharp
using Microsoft.UI.Xaml.Controls;
using MTM_Waitlist_Application.Module_Operator.ViewModels;

namespace MTM_Waitlist_Application.Module_Operator.Views;

public sealed partial class View_Operator_CreateRequest : Page
{
    public ViewModel_Operator_CreateRequest ViewModel { get; }

    public View_Operator_CreateRequest()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<ViewModel_Operator_CreateRequest>();
        this.DataContext = ViewModel;
    }
}
```

#### Step 7: Register Module in DI

**File**: `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

```csharp
private static IServiceCollection AddOperatorModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("MySql")
        ?? throw new InvalidOperationException("MySql connection string not found");

    // DAOs
    services.AddSingleton(_ => new Dao_TaskRequest(connectionString));

    // Command/Query Handlers (MediatR discovers these automatically)

    // ViewModels
    services.AddTransient<ViewModel_Operator_CreateRequest>();
    services.AddTransient<ViewModel_Operator_Dashboard>();
    services.AddTransient<ViewModel_Operator_MyRequests>();

    // Views
    services.AddTransient<View_Operator_CreateRequest>();
    services.AddTransient<View_Operator_Dashboard>();
    services.AddTransient<View_Operator_MyRequests>();

    return services;
}
```

---

## Database Setup

### Step 1: Create Database Schema

**File**: `Database/Schemas/01_create_database.sql`

```sql
-- Create database
CREATE DATABASE IF NOT EXISTS mtm_waitlist_application
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE mtm_waitlist_application;
```

**File**: `Database/Schemas/02_create_users_tables.sql`

```sql
USE mtm_waitlist_application;

-- Users table
CREATE TABLE IF NOT EXISTS users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    badge_number VARCHAR(20) NOT NULL UNIQUE,
    windows_username VARCHAR(100) UNIQUE,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    pin_hash VARCHAR(255),
    is_active TINYINT(1) DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_badge_number (badge_number),
    INDEX idx_windows_username (windows_username),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Roles table
CREATE TABLE IF NOT EXISTS roles (
    role_id INT AUTO_INCREMENT PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    description TEXT,
    is_active TINYINT(1) DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_role_name (role_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Privileges table
CREATE TABLE IF NOT EXISTS privileges (
    privilege_id INT AUTO_INCREMENT PRIMARY KEY,
    privilege_name VARCHAR(100) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    description TEXT,
    module_name VARCHAR(50) NOT NULL,
    is_active TINYINT(1) DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_privilege_name (privilege_name),
    INDEX idx_module (module_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- User-Role mapping
CREATE TABLE IF NOT EXISTS user_roles (
    user_role_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    role_id INT NOT NULL,
    assigned_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    assigned_by INT,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES roles(role_id) ON DELETE CASCADE,
    FOREIGN KEY (assigned_by) REFERENCES users(user_id) ON DELETE SET NULL,
    UNIQUE KEY unique_user_role (user_id, role_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Role-Privilege mapping
CREATE TABLE IF NOT EXISTS role_privileges (
    role_privilege_id INT AUTO_INCREMENT PRIMARY KEY,
    role_id INT NOT NULL,
    privilege_id INT NOT NULL,
    granted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (role_id) REFERENCES roles(role_id) ON DELETE CASCADE,
    FOREIGN KEY (privilege_id) REFERENCES privileges(privilege_id) ON DELETE CASCADE,
    UNIQUE KEY unique_role_privilege (role_id, privilege_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

**File**: `Database/Schemas/03_create_task_request_tables.sql`

```sql
USE mtm_waitlist_application;

-- Task categories
CREATE TABLE IF NOT EXISTS task_categories (
    category_id INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(50) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    fulfiller_role VARCHAR(50) NOT NULL,
    is_active TINYINT(1) DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_category_name (category_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Request types
CREATE TABLE IF NOT EXISTS request_types (
    request_type_id INT AUTO_INCREMENT PRIMARY KEY,
    category_id INT NOT NULL,
    type_name VARCHAR(100) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    requires_work_order TINYINT(1) DEFAULT 0,
    requires_part_number TINYINT(1) DEFAULT 0,
    requires_zone TINYINT(1) DEFAULT 0,
    default_priority VARCHAR(20) DEFAULT 'Normal',
    estimated_minutes INT DEFAULT 15,
    is_active TINYINT(1) DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES task_categories(category_id) ON DELETE CASCADE,
    UNIQUE KEY unique_type (category_id, type_name),
    INDEX idx_type_name (type_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Task requests (main waitlist table)
CREATE TABLE IF NOT EXISTS task_requests (
    request_id INT AUTO_INCREMENT PRIMARY KEY,
    category_id INT NOT NULL,
    request_type_id INT NOT NULL,
    created_by_user_id INT NOT NULL,
    assigned_to_user_id INT,
    work_order_number VARCHAR(50),
    part_number VARCHAR(50),
    description TEXT,
    zone VARCHAR(20),
    priority VARCHAR(20) DEFAULT 'Normal',
    status VARCHAR(20) DEFAULT 'Pending',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    started_at DATETIME,
    completed_at DATETIME,
    cancelled_at DATETIME,
    cancelled_by_user_id INT,
    cancellation_reason TEXT,
    estimated_duration_minutes INT,
    actual_duration_minutes INT,
    notes TEXT,
    FOREIGN KEY (category_id) REFERENCES task_categories(category_id),
    FOREIGN KEY (request_type_id) REFERENCES request_types(request_type_id),
    FOREIGN KEY (created_by_user_id) REFERENCES users(user_id),
    FOREIGN KEY (assigned_to_user_id) REFERENCES users(user_id) ON DELETE SET NULL,
    FOREIGN KEY (cancelled_by_user_id) REFERENCES users(user_id) ON DELETE SET NULL,
    INDEX idx_status (status),
    INDEX idx_created_by (created_by_user_id),
    INDEX idx_assigned_to (assigned_to_user_id),
    INDEX idx_category (category_id),
    INDEX idx_zone (zone),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Task history/audit trail
CREATE TABLE IF NOT EXISTS task_request_history (
    history_id INT AUTO_INCREMENT PRIMARY KEY,
    request_id INT NOT NULL,
    changed_by_user_id INT NOT NULL,
    field_name VARCHAR(50) NOT NULL,
    old_value TEXT,
    new_value TEXT,
    changed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (request_id) REFERENCES task_requests(request_id) ON DELETE CASCADE,
    FOREIGN KEY (changed_by_user_id) REFERENCES users(user_id),
    INDEX idx_request (request_id),
    INDEX idx_changed_at (changed_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### Step 2: Seed Reference Data

**File**: `Database/TestData/01_seed_roles_privileges.sql`

```sql
USE mtm_waitlist_application;

-- Roles
INSERT INTO roles (role_name, display_name, description) VALUES
('Operator', 'Press Operator', 'Floor operator who creates task requests'),
('MaterialHandler', 'Material Handler', 'Fulfills material handling requests'),
('MaterialHandlerLead', 'Material Handler Lead', 'Oversees material handling team'),
('SetupTechnician', 'Setup Technician', 'Handles die-related setup tasks'),
('Quality', 'Quality Control', 'Handles quality inspections and issues'),
('ProductionLead', 'Production Lead', 'Oversees production operations'),
('OperatorLead', 'Operator Lead', 'Lead operator with oversight capabilities'),
('PlantManager', 'Plant Manager', 'Full visibility and control across all operations')
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name);

-- Privileges
INSERT INTO privileges (privilege_name, display_name, description, module_name) VALUES
('CREATE_TASK_REQUEST', 'Create Task Request', 'Can create new task requests', 'Operator'),
('VIEW_OWN_REQUESTS', 'View Own Requests', 'Can view own created requests', 'Operator'),
('VIEW_MATERIAL_HANDLER_QUEUE', 'View MH Queue', 'Can view material handler queue', 'MaterialHandler'),
('FULFILL_MATERIAL_TASKS', 'Fulfill Material Tasks', 'Can complete material handling tasks', 'MaterialHandler'),
('VIEW_SETUP_TECH_QUEUE', 'View Setup Queue', 'Can view setup technician queue', 'SetupTechnician'),
('FULFILL_SETUP_TASKS', 'Fulfill Setup Tasks', 'Can complete setup technician tasks', 'SetupTechnician'),
('VIEW_QUALITY_QUEUE', 'View Quality Queue', 'Can view quality control queue', 'Quality'),
('FULFILL_QUALITY_TASKS', 'Fulfill Quality Tasks', 'Can complete quality tasks', 'Quality'),
('VIEW_PRODUCTION_LEAD_QUEUE', 'View PL Queue', 'Can view production lead queue', 'ProductionLead'),
('VIEW_ANALYTICS', 'View Analytics', 'Can view analytics dashboards', 'Analytics'),
('MANAGE_USERS', 'Manage Users', 'Can manage user accounts', 'Admin'),
('MANAGE_SETTINGS', 'Manage Settings', 'Can modify application settings', 'Settings')
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name);

-- Assign privileges to roles
INSERT INTO role_privileges (role_id, privilege_id)
SELECT r.role_id, p.privilege_id FROM roles r, privileges p
WHERE r.role_name = 'Operator' AND p.privilege_name IN ('CREATE_TASK_REQUEST', 'VIEW_OWN_REQUESTS')
ON DUPLICATE KEY UPDATE role_id=VALUES(role_id);

INSERT INTO role_privileges (role_id, privilege_id)
SELECT r.role_id, p.privilege_id FROM roles r, privileges p
WHERE r.role_name = 'MaterialHandler' AND p.privilege_name IN ('VIEW_MATERIAL_HANDLER_QUEUE', 'FULFILL_MATERIAL_TASKS')
ON DUPLICATE KEY UPDATE role_id=VALUES(role_id);

INSERT INTO role_privileges (role_id, privilege_id)
SELECT r.role_id, p.privilege_id FROM roles r, privileges p
WHERE r.role_name = 'MaterialHandlerLead' AND p.privilege_name IN ('VIEW_MATERIAL_HANDLER_QUEUE', 'FULFILL_MATERIAL_TASKS', 'VIEW_QUALITY_QUEUE', 'VIEW_ANALYTICS')
ON DUPLICATE KEY UPDATE role_id=VALUES(role_id);

-- Continue for other roles...
```

**File**: `Database/TestData/02_seed_categories_request_types.sql`

```sql
USE mtm_waitlist_application;

-- Task Categories
INSERT INTO task_categories (category_name, display_name, fulfiller_role) VALUES
('MaterialHandler', 'Material Handler', 'MaterialHandler'),
('SetupTechnician', 'Setup Technician', 'SetupTechnician'),
('QualityControl', 'Quality Control', 'Quality'),
('ProductionLead', 'Production Lead', 'ProductionLead')
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name);

-- Material Handler Request Types
INSERT INTO request_types (category_id, type_name, display_name, requires_work_order, requires_part_number, requires_zone, estimated_minutes)
SELECT c.category_id, 'Coils', 'Coils', 1, 1, 1, 20 FROM task_categories c WHERE c.category_name = 'MaterialHandler'
UNION ALL
SELECT c.category_id, 'Flatstock', 'Flatstock', 1, 1, 1, 15 FROM task_categories c WHERE c.category_name = 'MaterialHandler'
UNION ALL
SELECT c.category_id, 'ComponentParts', 'Component Parts', 1, 1, 1, 15 FROM task_categories c WHERE c.category_name = 'MaterialHandler'
UNION ALL
SELECT c.category_id, 'Dunnage', 'Dunnage', 1, 0, 1, 10 FROM task_categories c WHERE c.category_name = 'MaterialHandler'
UNION ALL
SELECT c.category_id, 'Dies', 'Dies', 1, 0, 1, 30 FROM task_categories c WHERE c.category_name = 'MaterialHandler'
UNION ALL
SELECT c.category_id, 'Scrap', 'Scrap', 1, 0, 1, 15 FROM task_categories c WHERE c.category_name = 'MaterialHandler'
UNION ALL
SELECT c.category_id, 'NCM', 'Non-Conforming Material', 1, 1, 1, 20 FROM task_categories c WHERE c.category_name = 'MaterialHandler'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name);

-- Setup Technician Request Types
INSERT INTO request_types (category_id, type_name, display_name, requires_work_order, requires_part_number, requires_zone, estimated_minutes)
SELECT c.category_id, 'DieProtectionAlarm', 'Die Protection Alarm', 1, 0, 0, 10 FROM task_categories c WHERE c.category_name = 'SetupTechnician'
UNION ALL
SELECT c.category_id, 'DieStuck', 'Die Stuck', 1, 0, 0, 30 FROM task_categories c WHERE c.category_name = 'SetupTechnician'
UNION ALL
SELECT c.category_id, 'DieMisalignment', 'Die Misalignment', 1, 0, 0, 20 FROM task_categories c WHERE c.category_name = 'SetupTechnician'
UNION ALL
SELECT c.category_id, 'DieDamage', 'Die Damage', 1, 0, 0, 45 FROM task_categories c WHERE c.category_name = 'SetupTechnician'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name);

-- Quality Control Request Types
INSERT INTO request_types (category_id, type_name, display_name, requires_work_order, requires_part_number, requires_zone, estimated_minutes)
SELECT c.category_id, 'InspectionRequest', 'Inspection Request', 1, 1, 0, 15 FROM task_categories c WHERE c.category_name = 'QualityControl'
UNION ALL
SELECT c.category_id, 'QualityQuestion', 'Quality Question', 1, 1, 0, 10 FROM task_categories c WHERE c.category_name = 'QualityControl'
UNION ALL
SELECT c.category_id, 'OtherQC', 'Other QC Request', 0, 0, 0, 15 FROM task_categories c WHERE c.category_name = 'QualityControl'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name);

-- Production Lead Request Types
INSERT INTO request_types (category_id, type_name, display_name, requires_work_order, requires_part_number, requires_zone, estimated_minutes)
SELECT c.category_id, 'ProductionQuestion', 'Production Question', 0, 0, 0, 10 FROM task_categories c WHERE c.category_name = 'ProductionLead'
UNION ALL
SELECT c.category_id, 'SafetyIncident', 'Safety/Injury Report', 0, 0, 0, 30 FROM task_categories c WHERE c.category_name = 'ProductionLead'
UNION ALL
SELECT c.category_id, 'OtherPL', 'Other PL Request', 0, 0, 0, 15 FROM task_categories c WHERE c.category_name = 'ProductionLead'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name);
```

### Step 3: Create Stored Procedures

**File**: `Database/StoredProcedures/Authentication/sp_User_GetByBadge.sql`

```sql
USE mtm_waitlist_application;

DROP PROCEDURE IF EXISTS sp_User_GetByBadge;

DELIMITER $$

CREATE PROCEDURE sp_User_GetByBadge(
    IN p_badge_number VARCHAR(20)
)
BEGIN
    SELECT 
        u.user_id,
        u.badge_number,
        u.windows_username,
        u.first_name,
        u.last_name,
        u.pin_hash,
        u.is_active,
        u.created_at,
        u.updated_at
    FROM users u
    WHERE u.badge_number = p_badge_number
        AND u.is_active = 1;
END$$

DELIMITER ;
```

**File**: `Database/StoredProcedures/Authentication/sp_UserPrivileges_GetByUserId.sql`

```sql
USE mtm_waitlist_application;

DROP PROCEDURE IF EXISTS sp_UserPrivileges_GetByUserId;

DELIMITER $$

CREATE PROCEDURE sp_UserPrivileges_GetByUserId(
    IN p_user_id INT
)
BEGIN
    SELECT DISTINCT
        p.privilege_id,
        p.privilege_name,
        p.display_name,
        p.description,
        p.module_name,
        r.role_name,
        r.display_name AS role_display_name
    FROM user_roles ur
    INNER JOIN roles r ON ur.role_id = r.role_id
    INNER JOIN role_privileges rp ON r.role_id = rp.role_id
    INNER JOIN privileges p ON rp.privilege_id = p.privilege_id
    WHERE ur.user_id = p_user_id
        AND r.is_active = 1
        AND p.is_active = 1
    ORDER BY p.module_name, p.privilege_name;
END$$

DELIMITER ;
```

**File**: `Database/StoredProcedures/TaskRequests/sp_TaskRequest_Insert.sql`

```sql
USE mtm_waitlist_application;

DROP PROCEDURE IF EXISTS sp_TaskRequest_Insert;

DELIMITER $$

CREATE PROCEDURE sp_TaskRequest_Insert(
    IN p_category_id INT,
    IN p_request_type_id INT,
    IN p_created_by_user_id INT,
    IN p_work_order_number VARCHAR(50),
    IN p_part_number VARCHAR(50),
    IN p_description TEXT,
    IN p_zone VARCHAR(20),
    IN p_priority VARCHAR(20),
    OUT p_request_id INT
)
BEGIN
    DECLARE v_estimated_duration INT;
    
    -- Get estimated duration from request type
    SELECT estimated_minutes INTO v_estimated_duration
    FROM request_types
    WHERE request_type_id = p_request_type_id;
    
    INSERT INTO task_requests (
        category_id,
        request_type_id,
        created_by_user_id,
        work_order_number,
        part_number,
        description,
        zone,
        priority,
        status,
        estimated_duration_minutes,
        created_at
    ) VALUES (
        p_category_id,
        p_request_type_id,
        p_created_by_user_id,
        p_work_order_number,
        p_part_number,
        p_description,
        p_zone,
        COALESCE(p_priority, 'Normal'),
        'Pending',
        v_estimated_duration,
        NOW()
    );
    
    SET p_request_id = LAST_INSERT_ID();
    
    SELECT p_request_id AS request_id;
END$$

DELIMITER ;
```

**File**: `Database/StoredProcedures/TaskRequests/sp_TaskRequest_GetByCategory.sql`

```sql
USE mtm_waitlist_application;

DROP PROCEDURE IF EXISTS sp_TaskRequest_GetByCategory;

DELIMITER $$

CREATE PROCEDURE sp_TaskRequest_GetByCategory(
    IN p_category_name VARCHAR(50),
    IN p_status VARCHAR(20)
)
BEGIN
    SELECT 
        tr.request_id,
        tr.category_id,
        tc.category_name,
        tc.display_name AS category_display_name,
        tr.request_type_id,
        rt.type_name,
        rt.display_name AS request_type_display_name,
        tr.created_by_user_id,
        CONCAT(u_created.first_name, ' ', u_created.last_name) AS created_by_name,
        tr.assigned_to_user_id,
        CONCAT(u_assigned.first_name, ' ', u_assigned.last_name) AS assigned_to_name,
        tr.work_order_number,
        tr.part_number,
        tr.description,
        tr.zone,
        tr.priority,
        tr.status,
        tr.created_at,
        tr.started_at,
        tr.completed_at,
        tr.estimated_duration_minutes,
        tr.actual_duration_minutes,
        TIMESTAMPDIFF(MINUTE, tr.created_at, NOW()) AS wait_time_minutes
    FROM task_requests tr
    INNER JOIN task_categories tc ON tr.category_id = tc.category_id
    INNER JOIN request_types rt ON tr.request_type_id = rt.request_type_id
    INNER JOIN users u_created ON tr.created_by_user_id = u_created.user_id
    LEFT JOIN users u_assigned ON tr.assigned_to_user_id = u_assigned.user_id
    WHERE tc.category_name = p_category_name
        AND (p_status IS NULL OR tr.status = p_status)
    ORDER BY 
        CASE tr.priority 
            WHEN 'Critical' THEN 1
            WHEN 'Urgent' THEN 2
            WHEN 'Normal' THEN 3
            ELSE 4
        END,
        tr.created_at ASC;
END$$

DELIMITER ;
```

### Step 4: Deploy Database

```powershell
# Deploy schemas
$schemas = Get-ChildItem "Database/Schemas" -Filter "*.sql" | Sort-Object Name
foreach ($schema in $schemas) {
    Write-Host "Deploying $($schema.Name)..." -ForegroundColor Cyan
    mysql -h 172.16.1.104 -P 3306 -u root -p mtm_waitlist_application < $schema.FullName
}

# Deploy test data
$testData = Get-ChildItem "Database/TestData" -Filter "*.sql" | Sort-Object Name
foreach ($data in $testData) {
    Write-Host "Loading $($data.Name)..." -ForegroundColor Cyan
    mysql -h 172.16.1.104 -P 3306 -u root -p mtm_waitlist_application < $data.FullName
}

# Deploy stored procedures
$procedures = Get-ChildItem "Database/StoredProcedures" -Filter "*.sql" -Recurse | Sort-Object Name
foreach ($proc in $procedures) {
    Write-Host "Deploying $($proc.Name)..." -ForegroundColor Cyan
    mysql -h 172.16.1.104 -P 3306 -u root -p mtm_waitlist_application < $proc.FullName
}

Write-Host "`nDatabase deployment complete!" -ForegroundColor Green
```

---

## Authentication & Privileges

### Privilege System Architecture

The privilege system extends the basic authentication with fine-grained access control:

```
User → UserRoles → Roles → RolePrivileges → Privileges
```

### Step 1: Create Privilege Service

**File**: `Module_Core/Contracts/Services/IService_PrivilegeManager.cs`

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Waitlist_Application.Module_Core.Models;

namespace MTM_Waitlist_Application.Module_Core.Contracts.Services;

public interface IService_PrivilegeManager
{
    Task<bool> UserHasPrivilegeAsync(int userId, string privilegeName);
    Task<List<string>> GetUserPrivilegesAsync(int userId);
    Task<bool> UserHasRoleAsync(int userId, string roleName);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task<bool> CanAccessModuleAsync(int userId, string moduleName);
}
```

**File**: `Module_Core/Services/Service_PrivilegeManager.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Waitlist_Application.Module_Core.Contracts.Services;
using MTM_Waitlist_Application.Module_Core.Data.Authentication;

namespace MTM_Waitlist_Application.Module_Core.Services;

public class Service_PrivilegeManager : IService_PrivilegeManager
{
    private readonly Dao_UserPrivilege _privilegeDao;
    private readonly Dictionary<int, List<string>> _userPrivilegeCache = new();
    private readonly Dictionary<int, DateTime> _cacheExpiry = new();
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(15);

    public Service_PrivilegeManager(Dao_UserPrivilege privilegeDao)
    {
        _privilegeDao = privilegeDao;
    }

    public async Task<bool> UserHasPrivilegeAsync(int userId, string privilegeName)
    {
        var privileges = await GetUserPrivilegesAsync(userId);
        return privileges.Contains(privilegeName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<string>> GetUserPrivilegesAsync(int userId)
    {
        // Check cache
        if (_userPrivilegeCache.TryGetValue(userId, out var cachedPrivileges))
        {
            if (_cacheExpiry[userId] > DateTime.Now)
            {
                return cachedPrivileges;
            }
        }

        // Fetch from database
        var result = await _privilegeDao.GetUserPrivilegesAsync(userId);
        if (result.IsSuccess && result.Data != null)
        {
            var privilegeNames = result.Data
                .Select(p => p.PrivilegeName)
                .ToList();

            // Update cache
            _userPrivilegeCache[userId] = privilegeNames;
            _cacheExpiry[userId] = DateTime.Now.Add(_cacheLifetime);

            return privilegeNames;
        }

        return new List<string>();
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName)
    {
        var roles = await GetUserRolesAsync(userId);
        return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        var result = await _privilegeDao.GetUserPrivilegesAsync(userId);
        if (result.IsSuccess && result.Data != null)
        {
            return result.Data
                .Select(p => p.RoleName)
                .Distinct()
                .ToList();
        }

        return new List<string>();
    }

    public async Task<bool> CanAccessModuleAsync(int userId, string moduleName)
    {
        var privileges = await GetUserPrivilegesAsync(userId);
        return privileges.Any(p => p.StartsWith(moduleName, 
            StringComparison.OrdinalIgnoreCase));
    }

    public void ClearCache(int userId)
    {
        _userPrivilegeCache.Remove(userId);
        _cacheExpiry.Remove(userId);
    }

    public void ClearAllCaches()
    {
        _userPrivilegeCache.Clear();
        _cacheExpiry.Clear();
    }
}
```

### Step 2: Create Privilege Attribute

**File**: `Module_Core/Attributes/RequirePrivilegeAttribute.cs`

```csharp
using System;

namespace MTM_Waitlist_Application.Module_Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePrivilegeAttribute : Attribute
{
    public string PrivilegeName { get; }
    public bool RequireAll { get; set; } = false;

    public RequirePrivilegeAttribute(string privilegeName)
    {
        PrivilegeName = privilegeName 
            ?? throw new ArgumentNullException(nameof(privilegeName));
    }
}
```

### Step 3: Use Privileges in ViewModels

```csharp
[RequirePrivilege("CREATE_TASK_REQUEST")]
public partial class ViewModel_Operator_CreateRequest : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_PrivilegeManager _privilegeManager;
    private readonly IService_UserSessionManager _sessionManager;

    public ViewModel_Operator_CreateRequest(
        IMediator mediator,
        IService_PrivilegeManager privilegeManager,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler)
        : base(errorHandler)
    {
        _mediator = mediator;
        _privilegeManager = privilegeManager;
        _sessionManager = sessionManager;
    }

    public async Task<bool> CheckAccessAsync()
    {
        var user = _sessionManager.GetCurrentUser();
        if (user == null) return false;

        return await _privilegeManager.UserHasPrivilegeAsync(
            user.UserId, 
            "CREATE_TASK_REQUEST");
    }

    [RelayCommand]
    private async Task SubmitRequestAsync()
    {
        // Check privilege
        if (!await CheckAccessAsync())
        {
            _errorHandler.ShowUserError(
                "You do not have permission to create task requests",
                "Access Denied",
                nameof(SubmitRequestAsync));
            return;
        }

        // Proceed with submit...
    }
}
```

---

## Workflow Implementation

### Workflow Pattern

Workflows use a state machine pattern with step enumeration:

```csharp
public enum Enum_OperatorWorkflowStep
{
    CategorySelection,
    RequestTypeSelection,
    WorkOrderEntry,
    PartNumberEntry,
    DetailsEntry,
    Review,
    Complete
}
```

### Workflow Service

**File**: `Module_Operator/Contracts/IService_OperatorWorkflow.cs`

```csharp
using System;
using System.Threading.Tasks;
using MTM_Waitlist_Application.Module_Operator.Models;

namespace MTM_Waitlist_Application.Module_Operator.Contracts;

public interface IService_OperatorWorkflow
{
    event EventHandler<Enum_OperatorWorkflowStep>? StepChanged;
    
    Enum_OperatorWorkflowStep CurrentStep { get; }
    Model_TaskRequest CurrentRequest { get; }
    
    void GoToStep(Enum_OperatorWorkflowStep step);
    void GoNext();
    void GoBack();
    bool CanGoNext();
    bool CanGoBack();
    
    void SetCategory(string category);
    void SetRequestType(string requestType);
    void SetWorkOrder(string workOrderNumber);
    void SetPartNumber(string partNumber);
    void SetDetails(string description, string zone, string priority);
    
    Task<bool> ValidateCurrentStepAsync();
    Task<bool> SubmitRequestAsync();
    void CancelWorkflow();
}
```

**File**: `Module_Operator/Services/Service_OperatorWorkflow.cs`

```csharp
using System;
using System.Threading.Tasks;
using MediatR;
using MTM_Waitlist_Application.Module_Operator.Contracts;
using MTM_Waitlist_Application.Module_Operator.Models;
using MTM_Waitlist_Application.Module_Operator.Models.Commands;

namespace MTM_Waitlist_Application.Module_Operator.Services;

public class Service_OperatorWorkflow : IService_OperatorWorkflow
{
    private readonly IMediator _mediator;
    private Enum_OperatorWorkflowStep _currentStep;
    private readonly Model_TaskRequest _currentRequest;

    public event EventHandler<Enum_OperatorWorkflowStep>? StepChanged;

    public Enum_OperatorWorkflowStep CurrentStep
    {
        get => _currentStep;
        private set
        {
            if (_currentStep != value)
            {
                _currentStep = value;
                StepChanged?.Invoke(this, _currentStep);
            }
        }
    }

    public Model_TaskRequest CurrentRequest => _currentRequest;

    public Service_OperatorWorkflow(IMediator mediator)
    {
        _mediator = mediator;
        _currentRequest = new Model_TaskRequest();
        _currentStep = Enum_OperatorWorkflowStep.CategorySelection;
    }

    public void GoToStep(Enum_OperatorWorkflowStep step)
    {
        CurrentStep = step;
    }

    public void GoNext()
    {
        if (!CanGoNext()) return;

        CurrentStep = CurrentStep switch
        {
            Enum_OperatorWorkflowStep.CategorySelection => 
                Enum_OperatorWorkflowStep.RequestTypeSelection,
            Enum_OperatorWorkflowStep.RequestTypeSelection => 
                Enum_OperatorWorkflowStep.WorkOrderEntry,
            Enum_OperatorWorkflowStep.WorkOrderEntry => 
                Enum_OperatorWorkflowStep.PartNumberEntry,
            Enum_OperatorWorkflowStep.PartNumberEntry => 
                Enum_OperatorWorkflowStep.DetailsEntry,
            Enum_OperatorWorkflowStep.DetailsEntry => 
                Enum_OperatorWorkflowStep.Review,
            _ => CurrentStep
        };
    }

    public void GoBack()
    {
        if (!CanGoBack()) return;

        CurrentStep = CurrentStep switch
        {
            Enum_OperatorWorkflowStep.RequestTypeSelection => 
                Enum_OperatorWorkflowStep.CategorySelection,
            Enum_OperatorWorkflowStep.WorkOrderEntry => 
                Enum_OperatorWorkflowStep.RequestTypeSelection,
            Enum_OperatorWorkflowStep.PartNumberEntry => 
                Enum_OperatorWorkflowStep.WorkOrderEntry,
            Enum_OperatorWorkflowStep.DetailsEntry => 
                Enum_OperatorWorkflowStep.PartNumberEntry,
            Enum_OperatorWorkflowStep.Review => 
                Enum_OperatorWorkflowStep.DetailsEntry,
            _ => CurrentStep
        };
    }

    public bool CanGoNext()
    {
        return CurrentStep != Enum_OperatorWorkflowStep.Review &&
               CurrentStep != Enum_OperatorWorkflowStep.Complete;
    }

    public bool CanGoBack()
    {
        return CurrentStep != Enum_OperatorWorkflowStep.CategorySelection &&
               CurrentStep != Enum_OperatorWorkflowStep.Complete;
    }

    public void SetCategory(string category)
    {
        _currentRequest.RequestCategory = category;
    }

    public void SetRequestType(string requestType)
    {
        _currentRequest.RequestType = requestType;
    }

    public void SetWorkOrder(string workOrderNumber)
    {
        _currentRequest.WorkOrderNumber = workOrderNumber;
    }

    public void SetPartNumber(string partNumber)
    {
        _currentRequest.PartNumber = partNumber;
    }

    public void SetDetails(string description, string zone, string priority)
    {
        _currentRequest.Description = description;
        _currentRequest.Zone = zone;
        _currentRequest.Priority = priority;
    }

    public async Task<bool> ValidateCurrentStepAsync()
    {
        return CurrentStep switch
        {
            Enum_OperatorWorkflowStep.CategorySelection => 
                !string.IsNullOrWhiteSpace(_currentRequest.RequestCategory),
            Enum_OperatorWorkflowStep.RequestTypeSelection => 
                !string.IsNullOrWhiteSpace(_currentRequest.RequestType),
            Enum_OperatorWorkflowStep.WorkOrderEntry => 
                !string.IsNullOrWhiteSpace(_currentRequest.WorkOrderNumber),
            _ => await Task.FromResult(true)
        };
    }

    public async Task<bool> SubmitRequestAsync()
    {
        var command = new CreateTaskRequestCommand(
            _currentRequest.OperatorId,
            _currentRequest.RequestCategory,
            _currentRequest.RequestType,
            _currentRequest.WorkOrderNumber,
            _currentRequest.PartNumber,
            _currentRequest.Description,
            _currentRequest.Zone,
            _currentRequest.Priority
        );

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            CurrentStep = Enum_OperatorWorkflowStep.Complete;
            return true;
        }

        return false;
    }

    public void CancelWorkflow()
    {
        CurrentStep = Enum_OperatorWorkflowStep.CategorySelection;
        // Reset current request
    }
}
```

---

## Testing Strategy

### Unit Testing Pattern

**File**: `MTM_Waitlist_Application.Tests/Module_Operator/ViewModels/ViewModel_Operator_CreateRequestTests.cs`

```csharp
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using MTM_Waitlist_Application.Module_Core.Contracts.Services;
using MTM_Waitlist_Application.Module_Core.Models;
using MTM_Waitlist_Application.Module_Operator.Models;
using MTM_Waitlist_Application.Module_Operator.Models.Commands;
using MTM_Waitlist_Application.Module_Operator.ViewModels;
using Xunit;

namespace MTM_Waitlist_Application.Tests.Module_Operator.ViewModels;

public class ViewModel_Operator_CreateRequestTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IService_UserSessionManager> _mockSessionManager;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly ViewModel_Operator_CreateRequest _viewModel;

    public ViewModel_Operator_CreateRequestTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockSessionManager = new Mock<IService_UserSessionManager>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();

        _viewModel = new ViewModel_Operator_CreateRequest(
            _mockMediator.Object,
            _mockSessionManager.Object,
            _mockErrorHandler.Object);
    }

    [Fact]
    public async Task SubmitRequestAsync_WithValidData_ShouldCallMediator()
    {
        // Arrange
        var user = new Model_User { UserId = 1, BadgeNumber = "12345" };
        _mockSessionManager.Setup(x => x.GetCurrentUser()).Returns(user);
        
        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateTaskRequestCommand>(), default))
            .ReturnsAsync(Model_Dao_Result<int>.Success(1));

        _viewModel.SelectedCategory = "Material Handler";
        _viewModel.SelectedRequestType = "Coils";
        _viewModel.WorkOrderNumber = "WO12345";

        // Act
        await _viewModel.SubmitRequestCommand.ExecuteAsync(null);

        // Assert
        _mockMediator.Verify(
            x => x.Send(
                It.Is<CreateTaskRequestCommand>(c => 
                    c.RequestCategory == "Material Handler" &&
                    c.RequestType == "Coils"),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SubmitRequestAsync_WithNoUser_ShouldShowError()
    {
        // Arrange
        _mockSessionManager.Setup(x => x.GetCurrentUser()).Returns((Model_User?)null);

        // Act
        await _viewModel.SubmitRequestCommand.ExecuteAsync(null);

        // Assert
        _mockErrorHandler.Verify(
            x => x.ShowUserError(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void OnSelectedCategoryChanged_ShouldLoadRequestTypes()
    {
        // Arrange & Act
        _viewModel.SelectedCategory = "Material Handler";

        // Assert
        _viewModel.RequestTypes.Should().NotBeEmpty();
        _viewModel.RequestTypes.Should().Contain("Coils");
    }
}
```

### Integration Testing Pattern

**File**: `MTM_Waitlist_Application.Tests/Module_Operator/Data/Dao_TaskRequestIntegrationTests.cs`

```csharp
using System;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Waitlist_Application.Module_Core.Helpers.Database;
using MTM_Waitlist_Application.Module_Operator.Data;
using MTM_Waitlist_Application.Module_Operator.Models;
using Xunit;

namespace MTM_Waitlist_Application.Tests.Module_Operator.Data;

[Collection("Database")]
public class Dao_TaskRequestIntegrationTests : IAsyncLifetime
{
    private readonly Dao_TaskRequest _dao;
    private int _testRequestId;

    public Dao_TaskRequestIntegrationTests()
    {
        var connectionString = Helper_Database_Variables.GetConnectionString();
        _dao = new Dao_TaskRequest(connectionString);
    }

    public async Task InitializeAsync()
    {
        var request = new Model_TaskRequest
        {
            OperatorId = 1,
            RequestCategory = "Material Handler",
            RequestType = "Coils",
            WorkOrderNumber = "TEST-WO-001",
            PartNumber = "TEST-PART-001",
            Description = "Test request",
            Zone = "Zone 1",
            Priority = "Normal"
        };

        var result = await _dao.InsertAsync(request);
        _testRequestId = result.Data;
    }

    public async Task DisposeAsync()
    {
        await _dao.DeleteAsync(_testRequestId);
    }

    [Fact]
    public async Task InsertAsync_WithValidRequest_ShouldReturnId()
    {
        // Arrange
        var request = new Model_TaskRequest
        {
            OperatorId = 1,
            RequestCategory = "Quality Control",
            RequestType = "Inspection Request",
            WorkOrderNumber = "WO-INTEGRATION-TEST",
            PartNumber = "PART-INT-001",
            Priority = "Normal"
        };

        // Act
        var result = await _dao.InsertAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);

        // Cleanup
        await _dao.DeleteAsync(result.Data);
    }

    [Fact]
    public async Task GetByOperatorAsync_WithExistingRequests_ShouldReturnList()
    {
        // Arrange & Act
        var result = await _dao.GetByOperatorAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Contain(r => r.Id == _testRequestId);
    }
}
```

---

## Build & Deployment

### Build Scripts

**File**: `Scripts/Build-Application.ps1`

```powershell
<#
.SYNOPSIS
    Builds the MTM Waitlist Application
.DESCRIPTION
    Builds the application in Debug or Release configuration
#>
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("x64", "ARM64")]
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"

Write-Host "Building MTM Waitlist Application..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow

# Clean previous build
if (Test-Path "bin") {
    Remove-Item "bin" -Recurse -Force
}
if (Test-Path "obj") {
    Remove-Item "obj" -Recurse -Force
}

# Restore NuGet packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Cyan
dotnet restore MTM_Waitlist_Application.slnx

# Build solution
Write-Host "`nBuilding solution..." -ForegroundColor Cyan
dotnet build MTM_Waitlist_Application.slnx `
    -c $Configuration `
    /p:Platform=$Platform `
    /p:GenerateFullPaths=true `
    /consoleloggerparameters:NoSummary

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBuild FAILED!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
```

### Deployment Script

**File**: `Scripts/Deploy-Application.ps1`

```powershell
<#
.SYNOPSIS
    Deploys the MTM Waitlist Application
.DESCRIPTION
    Publishes the application and creates deployment package
#>
param(
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".\publish",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("x64", "ARM64")]
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"

Write-Host "Publishing MTM Waitlist Application..." -ForegroundColor Cyan

# Clean output directory
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}

# Publish application
dotnet publish MTM_Waitlist_Application.slnx `
    -c Release `
    /p:Platform=$Platform `
    /p:PublishReadyToRun=false `
    -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nPublish FAILED!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nApplication published to: $OutputPath" -ForegroundColor Green
```

---

## Development Workflow

### Daily Development Process

1. **Start Development Session**
   ```powershell
   # Pull latest changes
   git pull origin main
   
   # Restore packages
   dotnet restore
   
   # Run database migrations if any
   .\Scripts\Deploy-Database.ps1
   ```

2. **Create Feature Branch**
   ```powershell
   git checkout -b feature/operator-favorites
   ```

3. **Implement Feature**
   - Create models
   - Create CQRS commands/queries with validators
   - Create DAO with stored procedures
   - Create service
   - Create ViewModel
   - Create View
   - Register in DI

4. **Test Feature**
   ```powershell
   # Run tests
   dotnet test
   
   # Build and run
   dotnet build
   dotnet run
   ```

5. **Commit and Push**
   ```powershell
   git add .
   git commit -m "feat(operator): Add favorites functionality"
   git push origin feature/operator-favorites
   ```

### Code Review Checklist

- [ ] All ViewModels are `partial` classes inheriting from `ViewModel_Shared_Base`
- [ ] XAML uses `x:Bind` (never `Binding`)
- [ ] No ViewModel→DAO direct calls (must go through Service/Mediator)
- [ ] DAOs are instance-based (no static)
- [ ] MySQL uses stored procedures only
- [ ] No writes to Infor Visual (READ-ONLY)
- [ ] Async methods end with `Async`
- [ ] Proper error handling with `IService_ErrorHandler`
- [ ] All DI registrations are correct
- [ ] Tests pass
- [ ] No compiler warnings

---

## Troubleshooting

### Common Issues

#### Issue: XAML Binding Errors

**Symptom**: Blank UI, binding failures in Output window

**Solution**:
```xml
<!-- ❌ WRONG -->
<TextBox Text="{Binding MyProperty}"/>

<!-- ✅ CORRECT -->
<TextBox Text="{x:Bind ViewModel.MyProperty, Mode=TwoWay}"/>
```

#### Issue: ViewModel Not Found

**Symptom**: Service not registered exception

**Solution**: Check DI registration in `ModuleServicesExtensions.cs`:
```csharp
services.AddTransient<ViewModel_Operator_CreateRequest>();
```

#### Issue: MySQL Connection Failed

**Symptom**: Cannot connect to MySQL database

**Solution**: Verify connection string in `appsettings.json` and ensure MySQL service is running:
```powershell
Get-Service MySQL80
Start-Service MySQL80
```

#### Issue: Stored Procedure Not Found

**Symptom**: `Unknown procedure` error

**Solution**: Deploy stored procedure:
```powershell
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_waitlist_application < Database/StoredProcedures/[module]/sp_name.sql
```

#### Issue: DAO Throws Exception

**Symptom**: Unhandled exception from DAO

**Fix**: DAOs must NEVER throw - return failure result:
```csharp
// ❌ WRONG
public async Task<Model_Dao_Result> SaveAsync()
{
    throw new Exception("Error"); // NO!
}

// ✅ CORRECT
public async Task<Model_Dao_Result> SaveAsync()
{
    try {
        // ...
    }
    catch (Exception ex) {
        return Model_Dao_Result.Failure($"Error: {ex.Message}", ex);
    }
}
```

### Debug Commands

```powershell
# Check build errors
dotnet build MTM_Waitlist_Application.slnx 2>&1 | Select-String "error"

# Verbose XAML errors (Visual Studio)
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
& "$vs\Common7\IDE\devenv.com" MTM_Waitlist_Application.slnx /Rebuild "Debug|x64"

# Test database connection
mysql -h 172.16.1.104 -P 3306 -u root -p -e "USE mtm_waitlist_application; SHOW TABLES;"

# Check DI registrations
# Add logging in App.xaml.cs ConfigureServices
```

---

## Next Steps

1. **Review Clarification Files**: See individual `CLARIFICATION-*.md` files for detailed questions
2. **Set Up Development Environment**: Install prerequisites and tools
3. **Create Project Structure**: Run scaffolding scripts
4. **Implement Module_Core**: Start with foundation services
5. **Implement Module_Login**: Add authentication and privilege system
6. **Implement Feature Modules**: Operator, Material Handler, etc.
7. **Create Database**: Deploy schemas and stored procedures
8. **Test Integration**: End-to-end testing
9. **Deploy**: Package and deploy to production

---

## References

- [MTM Receiving Application Documentation](../README.md)
- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [MVVM Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [MySQL Documentation](https://dev.mysql.com/doc/)

---

**Document Version**: 1.0.0  
**Last Updated**: January 21, 2026  
**Author**: Development Team  
**Status**: Complete - Ready for Implementation

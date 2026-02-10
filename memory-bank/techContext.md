# Technical Context

**Last Updated:** 2025-01-19

## Technology Stack

### Core Framework
- **.NET 10** - Latest LTS release
- **C# 12** - Modern language features
- **WinUI 3** - Windows App SDK 1.8.260101001
- **Windows 10.0.22621.0** - Target platform

### UI & MVVM
- **CommunityToolkit.Mvvm 8.4.0** - MVVM helpers ([ObservableProperty], [RelayCommand])
- **CommunityToolkit.WinUI.UI.Controls 7.1.2** - Additional WinUI controls
- **CommunityToolkit.WinUI.Animations 8.2.251219** - Animation helpers
- **Material.Icons.WinUI3 2.4.1** - Material Design icons

### CQRS & Validation
- **MediatR 14.0.0** - CQRS pattern implementation
- **FluentValidation 12.1.1** - Command/query validation
- **FluentValidation.DependencyInjectionExtensions 12.1.1** - DI integration

### Database
- **MySql.Data 9.6.0** - MySQL connector (READ/WRITE operations)
- **Microsoft.Data.SqlClient 6.1.4** - SQL Server connector (READ ONLY)

### Logging & Observability
- **Serilog 4.3.0** - Structured logging
- **Serilog.Sinks.File 7.0.0** - File sink
- **Serilog.Extensions.Logging 10.0.0** - Microsoft.Extensions.Logging integration
- **Serilog.Extensions.Hosting 10.0.0** - Hosting integration
- **Serilog.Settings.Configuration 10.0.0** - Configuration support
- **Serilog.Enrichers.Thread 4.0.0** - Thread enrichment
- **Serilog.Enrichers.Environment 3.0.1** - Environment enrichment
- **OpenTelemetry 1.15.0** - Distributed tracing
- **OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.0** - OTLP exporter
- **OpenTelemetry.Extensions.Hosting 1.15.0** - Hosting integration

### Utilities
- **Mapster 7.4.0** - Object mapping
- **CsvHelper 33.1.0** - CSV file handling
- **Ardalis.GuardClauses 5.0.0** - Defensive programming

### Testing Stack
- **xUnit 2.9.3** - Test framework
- **xunit.runner.visualstudio 3.1.5** - Visual Studio integration
- **Microsoft.NET.Test.Sdk 18.0.1** - Test SDK
- **FluentAssertions 8.8.0** - Readable test assertions
- **Moq 4.20.72** - Mocking framework
- **Verify.Xunit 31.10.0** - Snapshot testing
- **Bogus 35.6.5** - Test data generation
- **FsCheck.Xunit 3.3.2** - Property-based testing
- **Xunit.SkippableFact 1.5.61** - Conditional test execution
- **coverlet.collector 6.0.4** - Code coverage

### Development Tools
- **Roslynator.Analyzers 4.15.0** - Code analysis
- **.editorconfig** - Code formatting standards

## Development Setup

### Prerequisites
1. Visual Studio 2022 17.12+ or Rider 2024.3+
2. .NET 10 SDK
3. Windows App SDK 1.8+
4. MySQL 8.0+ (for database access)
5. SQL Server (for Infor Visual integration)

### Build Configuration

**Debug:**
- Analyzers enabled during build
- XAML debugging information included
- No compiler optimizations
- Full PDB generation

**Release:**
- Full analysis enabled
- XML documentation required
- Compiler optimizations enabled
- Deterministic builds

### Project Structure
```
MTM_Receiving_Application/
├── Module_Core/              # Core infrastructure
│   ├── Behaviors/            # MediatR pipeline behaviors
│   ├── Data/                 # DAOs and helpers
│   ├── Models/               # Shared models
│   └── Services/             # Core services
├── Module_Shared/            # Shared components
├── Module_Receiving/         # Receiving workflows
├── Module_Volvo/             # Volvo integration
├── Module_Dunnage/           # Dunnage management
├── Module_Reporting/         # Reporting
├── Module_Settings/          # Settings UI
├── Database/                 # Database scripts
│   ├── MySQL/                # MySQL stored procedures
│   └── InforVisualScripts/   # SQL Server queries (read-only)
└── MTM_Receiving_Application.Tests/  # Test project
    └── Module_Core/
        └── Behaviors/        # Behavior tests
```

## Dependencies & Constraints

### Database Constraints
- **MySQL** - All operations MUST use stored procedures
- **SQL Server (Infor Visual)** - READ ONLY, direct queries allowed
- Connection strings managed by `Helper_Database_Variables`
- Connection pooling managed by framework

### Architecture Constraints
- **No Service Locator pattern** - Use constructor injection only
- **No static DAOs** - All must be instance-based
- **No exceptions from DAOs** - Return `Model_Dao_Result` instead
- **No direct DAO access from ViewModels** - Must go through Services
- **No runtime binding** - Use `x:Bind` compile-time binding only

### Testing Constraints
- **MediatR 14.0 Delegate Signature** - `RequestHandlerDelegate<TResponse>` requires `CancellationToken` parameter
- **Moq Strong-Named Assemblies** - Test types must be `public` for proxy generation
- **Integration Tests** - Require real database connection
- **Test Data** - Must use TEST- prefix for easy cleanup

## Performance Considerations
- DAOs are registered as Singletons (stateless, reusable)
- ViewModels are Transient (new instance per navigation)
- Views are Transient
- Connection pooling enabled for database access
- Parallel build enabled (`BuildInParallel=true`)
- Incremental builds enabled

## Security Considerations
- Connection strings in appsettings.json (not in code)
- SQL injection prevention via stored procedures
- Input validation via FluentValidation
- Audit logging for all operations (AuditBehavior)
- Read-only access to Infor Visual (no data modification)

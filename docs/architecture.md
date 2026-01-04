# MTM Receiving Application - Architecture Document

## Executive Summary

The MTM Receiving Application is a WinUI 3 desktop application built for manufacturing receiving operations. It provides workflows for receiving materials against purchase orders, managing returnable packaging (dunnage), and generating labels for incoming shipments.

**Project Type**: Desktop Application (Windows-only)  
**Architecture**: Modular Monolith with Strict MVVM  
**Primary Language**: C# (.NET 8)  
**UI Framework**: WinUI 3 (Windows App SDK)  
**Database Strategy**: Dual-database (MySQL for app data, SQL Server for ERP integration)

## Technology Stack

### Core Framework
- **.NET 8**: Modern cross-platform framework (Windows-targeted)
- **WinUI 3**: Native Windows UI framework (Windows App SDK 1.8+)
- **C# 12**: Modern language features with nullable reference types enabled

### MVVM Framework
- **CommunityToolkit.Mvvm 8.2.2**: Source generators for MVVM patterns
  - `[ObservableProperty]` for bindable properties
  - `[RelayCommand]` for command methods
  - Replaces manual INotifyPropertyChanged implementation

### Dependency Injection
- **Microsoft.Extensions.DependencyInjection 8.0**: Built-in .NET DI container
- **Microsoft.Extensions.Hosting 8.0**: Application lifetime management
- All services registered in `App.xaml.cs`

### Database Access
- **MySql.Data 9.4.0**: MySQL ADO.NET provider
  - Server: 172.16.1.104:3306
  - Database: `mtm_receiving_application`
  - Access: Full READ/WRITE
  - All operations via stored procedures
- **Microsoft.Data.SqlClient 5.2.2**: SQL Server provider
  - Server: VISUAL
  - Database: MTMFG
  - Access: **READ ONLY** (`ApplicationIntent=ReadOnly`)
  - Used for PO and Part lookups from Infor Visual ERP

### UI Components
- **CommunityToolkit.WinUI.UI.Controls 7.1.2**: Extended WinUI controls
- **Material.Icons.WinUI3 2.4.1**: Material Design icon library
- **Custom Controls**: Touch-optimized inputs for manufacturing terminals

### Utilities
- **CsvHelper 33.0.1**: CSV generation for LabelView label printing
- **OpenTelemetry 1.14.0**: Observability and tracing (future use)

### Code Quality
- **Roslynator.Analyzers 4.15.0**: Static code analysis
- **Nullable Reference Types**: Enabled project-wide for null safety

## Architecture Pattern

### Modular Monolith

The application is organized into feature-based modules, each with a complete MVVM stack:

```
Module_Core/        # Infrastructure and shared services
Module_Receiving/   # Purchase order receiving workflows
Module_Dunnage/     # Returnable packaging management
Module_Shared/      # Shared UI components (login, splash, help)
Module_Settings/    # User preferences and configuration
Module_Routing/     # Future: Routing and logistics (placeholder)
```

Each module follows the same structure:
- **Data/**: DAO classes for database access
- **Models/**: Domain models (C# classes matching database schemas)
- **Services/**: Business logic services
- **ViewModels/**: MVVM ViewModels (presentation logic)
- **Views/**: XAML UI (declarative markup only)
- **Interfaces/**: Service contracts for dependency injection
- **Enums/**: Module-specific enumerations

### MVVM Architecture

**Strict separation of concerns:**

1. **Model**: Plain C# classes with `[ObservableProperty]` attributes
   - Represent database entities
   - No business logic
   - Observable for UI binding

2. **View**: XAML markup only
   - No code-behind logic (except DI injection)
   - Uses `x:Bind` for compile-time binding
   - Binds to ViewModel properties and commands

3. **ViewModel**: Presentation logic
   - Inherits from `BaseViewModel`
   - Marked as `partial` for source generators
   - Uses `[ObservableProperty]` for bindable state
   - Uses `[RelayCommand]` for user actions
   - Injects services via constructor
   - Manages workflow state and validation

### Dependency Injection

All components are registered in `App.xaml.cs`:

- **Singletons**: DAOs, infrastructure services (logging, error handling, navigation)
- **Transient**: ViewModels, business services (new instance per request)
- **Instance-based DAOs**: All DAOs are concrete classes (not static) for testability

Example DI registration:
```csharp
// Core services (Singleton)
services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
services.AddSingleton<IService_LoggingUtility, Service_LoggingUtility>();

// DAOs (Singleton with connection string)
services.AddSingleton(sp => new Dao_ReceivingLoad(mySqlConnectionString));
services.AddSingleton(sp => new Dao_InforVisualPO(inforVisualConnectionString, logger));

// Business services (Transient)
services.AddTransient<IService_ReceivingWorkflow, Service_ReceivingWorkflow>();

// ViewModels (Transient)
services.AddTransient<ViewModel_Receiving_POEntry>();
```

## Data Architecture

### Dual-Database Strategy

**MySQL (mtm_receiving_application)**: Operational data store
- **Purpose**: Application transactional data
- **Access**: Full READ/WRITE
- **Operations**: Via stored procedures only
- **Location**: 172.16.1.104:3306

**SQL Server (Infor Visual ERP - MTMFG)**: Source of truth
- **Purpose**: PO and Part master data
- **Access**: **READ ONLY** (`ApplicationIntent=ReadOnly`)
- **Operations**: Direct SQL queries (SELECT only)
- **Server**: VISUAL

### MySQL Schema

**Authentication**:
- `users`: User credentials, PINs, Windows username mapping
- `departments`: Department definitions

**Receiving**:
- `receiving_loads`: Header-level transactions (PO, carrier, packing slip)
- `receiving_lines`: Line-item details (weight, quantity, heat/lot, package type)
- `package_type_preferences`: User-defined package configurations

**Dunnage**:
- `dunnage_types`: Packaging categories (bins, racks, pallets, etc.)
- `dunnage_parts`: Specific part numbers with home locations
- `dunnage_specs`: Specification templates
- `inventoried_dunnage`: Current inventory levels
- `dunnage_loads`: Transaction log (incoming, outgoing, transfer, count)
- `dunnage_custom_fields`: Extensible metadata

### Data Access Layer (DAO Pattern)

**Mandatory Rules**:
1. All MySQL operations use stored procedures (no raw SQL in C#)
2. All DAOs are instance-based (not static)
3. All methods return `Model_Dao_Result` or `Model_Dao_Result<T>`
4. Never throw exceptions - return failure results
5. SQL Server queries are READ-ONLY (validation only)

Example DAO method:
```csharp
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetLoadsByDateAsync(DateTime date)
{
    try
    {
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("p_date", MySqlDbType.Date) { Value = date }
        };
        
        var result = await Helper_Database_StoredProcedure
            .ExecuteStoredProcedureAsync("sp_receiving_loads_get_by_date", parameters);
        
        if (!result.IsSuccess)
        {
            return Model_Dao_Result<List<Model_ReceivingLoad>>.Failure(result.ErrorMessage);
        }
        
        var loads = new List<Model_ReceivingLoad>();
        foreach (DataRow row in result.Data.Rows)
        {
            loads.Add(MapFromDataRow(row));
        }
        
        return Model_Dao_Result<List<Model_ReceivingLoad>>.Success(loads);
    }
    catch (Exception ex)
    {
        return Model_Dao_Result<List<Model_ReceivingLoad>>.Failure($"Error: {ex.Message}");
    }
}
```

## Component Architecture

### Application Shell

**MainWindow**: Root application window
- Hosts main navigation frame
- Managed by `ViewModel_Shared_MainWindow`

**Main Navigation Pages**:
- `Main_ReceivingLabelPage`: Entry to receiving workflow
- `Main_DunnageLabelPage`: Entry to dunnage workflow
- `Main_CarrierDeliveryLabelPage`: Entry to carrier label workflow

### Module_Core: Infrastructure

**Services**:
- `IService_ErrorHandler`: Centralized error handling and user notifications
- `IService_LoggingUtility`: Application-wide logging
- `IService_Notification`: Toast notifications
- `IService_Focus`: Focus management for touch UI
- `IService_Authentication`: User authentication
- `IService_UserSessionManager`: Session state management
- `IService_Navigation`: Navigation between views
- `IService_Window`: Window management utilities

**Data Access**:
- `Dao_User`: User authentication and preferences
- `Dao_InforVisualPO`: Purchase order lookups (READ-ONLY)
- `Dao_InforVisualPart`: Part master data (READ-ONLY)
- `Helper_Database_Variables`: Connection string management
- `Helper_Database_StoredProcedure`: Stored procedure execution helper

### Module_Receiving: Purchase Order Receiving

**Workflow**: Linear wizard with 8 steps
1. Mode Selection (Standard/Manual/Edit)
2. PO Entry & Validation (against Infor Visual)
3. Load Entry (Carrier, packing slip)
4. Weight & Quantity
5. Heat/Lot Numbers
6. Package Type Selection
7. Review & Confirm
8. Label Generation (CSV for LabelView)

**Services**:
- `IService_ReceivingWorkflow`: Orchestrates workflow steps
- `IService_ReceivingValidation`: PO and data validation
- `IService_MySQL_Receiving`: Receiving load operations
- `IService_MySQL_ReceivingLine`: Line item operations
- `IService_CSVWriter`: Generates CSV files for label printing

**ViewModels**: 10 ViewModels (one per workflow step + alternates)

### Module_Dunnage: Returnable Packaging Management

**User Workflow**: Linear wizard with 6 steps
1. Mode Selection (Incoming/Outgoing/Transfer/Count)
2. Type Selection (Bins/Racks/Pallets/etc.)
3. Part Selection (Specific part number)
4. Quantity Entry (Large touch targets)
5. Details Entry (Custom fields)
6. Review & Commit

**Admin Dashboard**: CRUD operations
- Manage Dunnage Types (add/edit/delete types with icons)
- Manage Parts (add/edit/delete part numbers with home locations)
- View Inventory (current counts, transaction history)

**Services**:
- `IService_DunnageWorkflow`: User workflow orchestration
- `IService_DunnageAdminWorkflow`: Admin operations
- `IService_MySQL_Dunnage`: Dunnage data operations
- `IService_DunnageCSVWriter`: Label generation

**ViewModels**: 20+ ViewModels (workflow steps + admin views + dialogs)

### Module_Shared: Reusable UI Components

**Components**:
- `View_Shared_SplashScreenWindow`: Application splash screen
- `View_Shared_SharedTerminalLoginDialog`: PIN-based authentication for shared terminals
- `View_Shared_NewUserSetupDialog`: First-time user configuration
- `View_Shared_HelpDialog`: Context-sensitive help system
- `View_Shared_IconSelectorWindow`: Material Design icon picker

### Module_Settings: User Preferences

**Configuration**:
- Default workflow modes (Receiving/Dunnage)
- User-specific preferences
- Application settings

## Workflow Architecture

### State Management

Each workflow module uses a session model to track state:
- `Model_ReceivingSession`: Receiving workflow state
- `Model_DunnageSession`: Dunnage workflow state

Session services manage:
- Current step in wizard
- Accumulated data across steps
- Validation state
- Navigation history

### Workflow Navigation

**Pattern**: Container View + Step Views

Each module has a workflow container (`View_Receiving_Workflow`, `View_Dunnage_WorkflowView`) that:
- Hosts step views in a Frame
- Manages navigation between steps
- Displays progress indicators
- Provides Back/Next/Cancel buttons

Step views are loaded dynamically based on workflow state.

### Validation Strategy

**Multi-layered validation**:
1. **UI-level**: Real-time input validation in ViewModels
2. **Business-level**: Service-layer validation before database operations
3. **Database-level**: Stored procedure constraints and foreign keys
4. **ERP-level**: PO validation against Infor Visual (READ-ONLY)

Example validation flow:
```
User enters PO → ViewModel validates format → Service calls Dao_InforVisualPO → 
Query Infor Visual → Return validation result → Update UI state
```

## Integration Points

### Infor Visual ERP Integration

**Purpose**: Validate purchase orders and retrieve part details

**Architecture**:
- **Connection**: SQL Server with `ApplicationIntent=ReadOnly`
- **DAOs**: `Dao_InforVisualPO`, `Dao_InforVisualPart`
- **Service**: `IService_InforVisual` (supports mock data for development)
- **Tables**: `po_detail`, `part`, `vendor`
- **Site Filter**: All queries include `site_ref = '002'` (MTM warehouse)

**Mock Data Mode**:
- Set `UseInforVisualMockData: true` in `appsettings.json`
- Enables development without live ERP connection
- Returns predefined sample data

### Label Printing Integration

**LabelView 2022**: CSV-driven label printing

**Process**:
1. User completes workflow (Receiving or Dunnage)
2. Application generates CSV file with label data
3. CSV saved to network location monitored by LabelView
4. LabelView automatically prints labels

**CSV Services**:
- `IService_CSVWriter`: Receiving labels
- `IService_DunnageCSVWriter`: Dunnage labels

**CSV Fields** (Receiving):
- PO number, Part number, Quantity, Weight
- Carrier, Packing slip, Date received
- User, Department, Package type

## Testing Strategy

### Unit Testing
- **Framework**: xUnit
- **Focus**: ViewModel logic, Service layer, Data transformations
- **Mocking**: Service interfaces mocked for ViewModel tests
- **Coverage**: Business logic, validation rules, state management

### Integration Testing
- **Database**: Tests against live MySQL test database
- **Stored Procedures**: Validated via integration tests
- **ERP Integration**: Uses mock data (no live ERP dependency)

### Manual Testing
- **Target Hardware**: Touchscreen terminals (large button UI)
- **User Workflows**: End-to-end receiving and dunnage scenarios
- **Label Printing**: Verify CSV generation and LabelView integration

## Deployment Architecture

### Target Environment
- **OS**: Windows 10/11 (build 17763+)
- **Hardware**: Manufacturing floor touchscreen terminals
- **Network**: Access to MySQL server and Infor Visual ERP

### Deployment Package
- **Format**: Self-contained .NET 8 application
- **Dependencies**: Windows App SDK runtime (bundled via `WindowsAppSDKSelfContained`)
- **Configuration**: `appsettings.json` for environment-specific settings

### Installation
1. Copy application folder to terminal
2. Configure `appsettings.json` with database connections
3. Deploy MySQL schemas and stored procedures
4. Configure LabelView CSV monitoring

### Updates
- Manual deployment (no auto-update mechanism)
- Database migrations run via SQL scripts
- Versioned schema changes in `Database/Migrations/`

## Security Considerations

### Authentication
- Windows username auto-detection
- PIN-based authentication for shared terminals
- User permissions stored in MySQL `users` table

### Database Security
- **MySQL**: User credentials in `appsettings.json` (deploy-time configuration)
- **SQL Server**: Read-only account for ERP access
- **Connection Strings**: Not hardcoded, loaded from configuration

### Data Protection
- No encryption at rest (relies on network/database security)
- SQL injection prevented via stored procedures and parameterized queries
- Input validation at multiple layers

## Performance Considerations

### Database Optimization
- **Stored Procedures**: Pre-compiled queries for MySQL
- **Indexes**: Applied to frequently queried columns
- **Connection Pooling**: Enabled by ADO.NET providers

### UI Responsiveness
- **Async/Await**: All database operations are asynchronous
- **UI Thread Management**: `IService_Dispatcher` for thread-safe UI updates
- **Large Touch Targets**: Optimized for touchscreen interaction

### Build Optimizations
- **ReadyToRun**: Disabled (causes COM interop issues with WinUI 3)
- **Trimming**: Disabled (breaks XAML binding and JSON serialization)
- **Debug Symbols**: Embedded for crash analysis

## Extensibility

### Adding New Modules
1. Create `Module_{Name}/` folder with standard structure
2. Implement DAOs, Services, ViewModels, Views
3. Register services in `App.xaml.cs`
4. Add navigation menu entry

### Adding New Workflows
1. Define session model (`Model_{Feature}Session`)
2. Create workflow service implementing step orchestration
3. Build step ViewModels and Views
4. Create workflow container view

### Custom Fields (Dunnage)
- Extensible metadata via `dunnage_custom_fields` table
- `Model_CustomFieldDefinition` supports dynamic field definitions
- UI renders fields based on field type (text, number, date, etc.)

## Future Considerations

### Planned Features (from specs/)
- **Routing Module**: Warehouse routing and logistics
- **Volvo Module**: Volvo-specific receiving workflows
- **Reporting Module**: Analytics and reporting dashboard

### Technical Improvements
- CI/CD pipeline (GitHub Actions)
- Automated database migrations
- Application auto-update mechanism
- Enhanced error logging and telemetry (OpenTelemetry integration)
- Offline mode with sync capabilities

## Summary

The MTM Receiving Application is a well-architected WinUI 3 desktop application with:
- **Strict MVVM separation**: Clean architecture with testable components
- **Modular design**: Feature-based modules with clear boundaries
- **Robust data access**: Dual-database strategy with stored procedures
- **Enterprise integration**: Read-only ERP integration for PO validation
- **Manufacturing-optimized UI**: Touch-friendly workflows for shop floor terminals

The architecture supports extensibility, maintainability, and scalability for future manufacturing workflow requirements.

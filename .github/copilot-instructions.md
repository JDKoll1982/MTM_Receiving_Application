# MTM Receiving Application - GitHub Copilot Instructions

## Project Overview

WinUI 3 desktop application for manufacturing receiving operations, built with . NET 8, strict MVVM architecture, and dual-database integration (MySQL for application data, SQL Server for read-only ERP integration).

**Repository**:  JDKoll1982/MTM_Receiving_Application  
**Primary Language**: C# (88.2%)  
**Framework**: WinUI 3 / Windows App SDK  
**Architecture**: MVVM with Dependency Injection

---

## 🔴 CRITICAL CONSTRAINTS

### Database Access Rules
1. **MySQL (`mtm_receiving_application`)**: Full CRUD operations allowed
   - ALL operations MUST use stored procedures via `Helper_Database_StoredProcedure`
   - NO raw SQL in C# code
   - Connection: `Helper_Database_Variables.GetConnectionString()`

2. **SQL Server (Infor Visual ERP)**: **STRICTLY READ ONLY**
   - Database: `MTMFG`, Server: `VISUAL`, Warehouse: `002`
   - SELECT queries only
   - Must include `ApplicationIntent=ReadOnly` in connection string
   - Use for:  PO validation, part lookups, material master data
   - ❌ NO INSERT/UPDATE/DELETE operations allowed

### Architecture Rules (Non-Negotiable)
- **MVVM separation**: ViewModels contain ALL business logic, Views are XAML-only
- **Dependency Injection**: All services registered in `App.xaml.cs`, constructor injection only
- **DAO Pattern**: All database operations return `Model_Dao_Result` or `Model_Dao_Result<T>`
- **Error Handling**: Use `IService_ErrorHandler` for all exceptions and user-facing errors

---

## Code Generation Standards

### ViewModel Pattern

**File Location**: `ViewModels/[Feature]/[Entity]ViewModel.cs`

**Required Structure**:
```csharp
using CommunityToolkit.Mvvm. ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application. Contracts.Services;
using MTM_Receiving_Application.Models;
using System.Collections.ObjectModel;

namespace MTM_Receiving_Application. ViewModels.[Feature];

/// <summary>
/// ViewModel for [feature description]
/// </summary>
public partial class [Entity]ViewModel : BaseViewModel
{
    #region Dependencies
    private readonly I[Service] _service;
    #endregion

    #region Observable Properties
    [ObservableProperty]
    private ObservableCollection<[Model]> _items = new();

    [ObservableProperty]
    private [Model]? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;
    #endregion

    #region Constructor
    public [Entity]ViewModel(
        I[Service] service,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _service = service;
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Loading...";

            var result = await _service. GetDataAsync();
            
            if (result.IsSuccess)
            {
                Items.Clear();
                foreach (var item in result.Data)
                {
                    Items.Add(item);
                }
                StatusMessage = $"Loaded {Items.Count} items";
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage,
                    "Load Failed",
                    nameof(LoadDataAsync)
                );
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadDataAsync),
                nameof([Entity]ViewModel)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion
}
```

**Mandatory ViewModel Rules**:
- MUST be `partial` class
- MUST inherit from `BaseViewModel`
- Use `[ObservableProperty]` for all bindable properties (generates backing fields)
- Use `[RelayCommand]` for all command methods (generates ICommand properties)
- Constructor MUST call `base(errorHandler, logger)`
- Always check `IsBusy` at start of async commands
- Always wrap operations in try/catch/finally with `IsBusy` flag
- Use `StatusMessage` property (inherited) for user feedback

### View Pattern

**File Location**: `Views/[Feature]/[Entity]Page.xaml`

**Required Structure**:
```xml
<Page
    x:Class="MTM_Receiving_Application.Views.[Feature].[Entity]Page"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.ViewModels.[Feature]"
    xmlns: d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats. org/markup-compatibility/2006"
    mc:Ignorable="d"
    Loaded="OnPageLoaded">

    <Page.Resources>
        <!-- Styles and templates -->
    </Page.Resources>

    <Grid Padding="16" RowSpacing="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock 
            Grid.Row="0"
            Text="{x:Bind ViewModel.Title, Mode=OneWay}"
            Style="{StaticResource TitleTextBlockStyle}"/>

        <!-- Content -->
        <ListView
            Grid.Row="1"
            ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}"
            SelectedItem="{x: Bind ViewModel.SelectedItem, Mode=TwoWay}">
            <!-- ItemTemplate -->
        </ListView>

        <!-- Status Bar -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" Spacing="8">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
        </StackPanel>
    </Grid>
</Page>
```

**Code-Behind** (`[Entity]Page.xaml.cs`):
```csharp
using Microsoft.UI.Xaml. Controls;
using MTM_Receiving_Application.ViewModels.[Feature];

namespace MTM_Receiving_Application.Views.[Feature];

public sealed partial class [Entity]Page :  Page
{
    public [Entity]ViewModel ViewModel { get; }

    public [Entity]Page()
    {
        ViewModel = App.GetService<[Entity]ViewModel>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
```

**Mandatory View Rules**:
- Use `x: Bind` (compile-time binding) instead of `Binding` (runtime)
- NO business logic in code-behind (only UI setup and event wiring)
- ViewModel injected via `App.GetService<T>()`
- Use `Mode=OneWay` for read-only data, `Mode=TwoWay` for user input
- Bind to `IsBusy` and `StatusMessage` for loading states

### Model Pattern

**File Location**: `Models/[Feature]/Model_[Entity].cs`

**Required Structure**:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel. DataAnnotations;

namespace MTM_Receiving_Application.Models.[Feature];

/// <summary>
/// Represents [entity description]
/// Database Table: [table_name]
/// </summary>
public partial class Model_[Entity] : ObservableObject
{
    [ObservableProperty]
    [property: Key]
    private int _id;

    [ObservableProperty]
    [property:  Required]
    [property: MaxLength(50)]
    private string _name = string.Empty;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private bool _isActive = true;

    // Navigation properties
    [ObservableProperty]
    private ObservableCollection<Model_RelatedEntity> _relatedItems = new();
}
```

**Mandatory Model Rules**:
- MUST be `partial` class
- MUST inherit from `ObservableObject`
- Use `[ObservableProperty]` for all properties
- Use data annotations (`[Key]`, `[Required]`, `[MaxLength]`) matching database schema
- Include XML doc comment with database table name
- Initialize collections and strings to avoid null references

### DAO (Data Access Object) Pattern

**File Location**: `Data/[Feature]/Dao_[Entity].cs`

**Required Structure**:
```csharp
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Models.[Feature];
using MySql.Data.MySqlClient;
using System.Data;

namespace MTM_Receiving_Application.Data.[Feature];

/// <summary>
/// Data Access Object for [entity] operations
/// </summary>
public class Dao_[Entity]
{
    #region Get Operations
    /// <summary>
    /// Retrieves all [entities] from database
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_[Entity]>>> GetAllAsync()
    {
        try
        {
            var parameters = new List<MySqlParameter>();
            
            var result = await Helper_Database_StoredProcedure
                .ExecuteStoredProcedureAsync(
                    "sp_[Entity]_GetAll",
                    parameters
                );

            if (! result.IsSuccess)
            {
                return new Model_Dao_Result<List<Model_[Entity]>>
                {
                    IsSuccess = false,
                    ErrorMessage = result.ErrorMessage
                };
            }

            var entities = new List<Model_[Entity]>();
            
            foreach (DataRow row in result.Data. Rows)
            {
                entities.Add(MapFromDataRow(row));
            }

            return new Model_Dao_Result<List<Model_[Entity]>>
            {
                IsSuccess = true,
                Data = entities
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<List<Model_[Entity]>>
            {
                IsSuccess = false,
                ErrorMessage = $"Error retrieving [entities]: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Retrieves [entity] by ID
    /// </summary>
    public async Task<Model_Dao_Result<Model_[Entity]>> GetByIdAsync(int id)
    {
        try
        {
            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("p_Id", MySqlDbType.Int32) { Value = id }
            };

            var result = await Helper_Database_StoredProcedure
                .ExecuteStoredProcedureAsync(
                    "sp_[Entity]_GetById",
                    parameters
                );

            if (!result.IsSuccess)
            {
                return new Model_Dao_Result<Model_[Entity]>
                {
                    IsSuccess = false,
                    ErrorMessage = result. ErrorMessage
                };
            }

            if (result.Data.Rows. Count == 0)
            {
                return new Model_Dao_Result<Model_[Entity]>
                {
                    IsSuccess = false,
                    ErrorMessage = "[Entity] not found"
                };
            }

            return new Model_Dao_Result<Model_[Entity]>
            {
                IsSuccess = true,
                Data = MapFromDataRow(result. Data.Rows[0])
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<Model_[Entity]>
            {
                IsSuccess = false,
                ErrorMessage = $"Error retrieving [entity]: {ex.Message}"
            };
        }
    }
    #endregion

    #region Insert/Update Operations
    /// <summary>
    /// Inserts new [entity] record
    /// </summary>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_[Entity] entity)
    {
        try
        {
            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("p_Name", MySqlDbType.VarChar) { Value = entity.Name },
                new MySqlParameter("p_IsActive", MySqlDbType. Bit) { Value = entity.IsActive }
            };

            var result = await Helper_Database_StoredProcedure
                . ExecuteStoredProcedureAsync(
                    "sp_[Entity]_Insert",
                    parameters
                );

            if (!result.IsSuccess)
            {
                return new Model_Dao_Result<int>
                {
                    IsSuccess = false,
                    ErrorMessage = result.ErrorMessage
                };
            }

            // Assume stored procedure returns new ID in first row, first column
            int newId = Convert.ToInt32(result.Data.Rows[0][0]);

            return new Model_Dao_Result<int>
            {
                IsSuccess = true,
                Data = newId
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<int>
            {
                IsSuccess = false,
                ErrorMessage = $"Error inserting [entity]:  {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Updates existing [entity] record
    /// </summary>
    public async Task<Model_Dao_Result> UpdateAsync(Model_[Entity] entity)
    {
        try
        {
            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("p_Id", MySqlDbType.Int32) { Value = entity.Id },
                new MySqlParameter("p_Name", MySqlDbType.VarChar) { Value = entity.Name },
                new MySqlParameter("p_IsActive", MySqlDbType. Bit) { Value = entity.IsActive }
            };

            var result = await Helper_Database_StoredProcedure
                .ExecuteStoredProcedureAsync(
                    "sp_[Entity]_Update",
                    parameters
                );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                IsSuccess = false,
                ErrorMessage = $"Error updating [entity]: {ex. Message}"
            };
        }
    }
    #endregion

    #region Delete Operations
    /// <summary>
    /// Deletes [entity] by ID
    /// </summary>
    public async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        try
        {
            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("p_Id", MySqlDbType.Int32) { Value = id }
            };

            var result = await Helper_Database_StoredProcedure
                .ExecuteStoredProcedureAsync(
                    "sp_[Entity]_Delete",
                    parameters
                );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                IsSuccess = false,
                ErrorMessage = $"Error deleting [entity]: {ex.Message}"
            };
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Maps DataRow to Model_[Entity]
    /// </summary>
    private Model_[Entity] MapFromDataRow(DataRow row)
    {
        return new Model_[Entity]
        {
            Id = Convert.ToInt32(row["Id"]),
            Name = row["Name"].ToString() ?? string.Empty,
            CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
            IsActive = Convert.ToBoolean(row["IsActive"])
        };
    }
    #endregion
}
```

**Mandatory DAO Rules**:
- Instance-based class (NOT static)
- ALL methods MUST be async
- ALL methods return `Model_Dao_Result` or `Model_Dao_Result<T>`
- NEVER throw exceptions - return failure results
- Use `Helper_Database_StoredProcedure. ExecuteStoredProcedureAsync()` exclusively
- Parameter naming:  `p_ParameterName` convention
- Include XML doc comments for all public methods
- Must be registered as singleton in `App.xaml.cs` DI container

### Service Pattern

**Interface Location**: `Contracts/Services/I[Service].cs`

```csharp
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Models.[Feature];

namespace MTM_Receiving_Application.Contracts. Services;

/// <summary>
/// Service interface for [entity] business logic
/// </summary>
public interface I[Service]
{
    Task<Model_Dao_Result<List<Model_[Entity]>>> GetAllAsync();
    Task<Model_Dao_Result<Model_[Entity]>> GetByIdAsync(int id);
    Task<Model_Dao_Result<int>> SaveAsync(Model_[Entity] entity);
    Task<Model_Dao_Result> DeleteAsync(int id);
}
```

**Implementation Location**: `Services/[Feature]/[Service].cs`

```csharp
using MTM_Receiving_Application. Contracts.Services;
using MTM_Receiving_Application.Data.[Feature];
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Models.[Feature];

namespace MTM_Receiving_Application.Services.[Feature];

/// <summary>
/// Business logic service for [entity] operations
/// </summary>
public class [Service] : I[Service]
{
    private readonly Dao_[Entity] _dao;
    private readonly ILoggingService _logger;

    public [Service](
        Dao_[Entity] dao,
        ILoggingService logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<List<Model_[Entity]>>> GetAllAsync()
    {
        await _logger.LogInformationAsync("Retrieving all [entities]");
        return await _dao.GetAllAsync();
    }

    public async Task<Model_Dao_Result<Model_[Entity]>> GetByIdAsync(int id)
    {
        await _logger.LogInformationAsync($"Retrieving [entity] with ID: {id}");
        return await _dao.GetByIdAsync(id);
    }

    public async Task<Model_Dao_Result<int>> SaveAsync(Model_[Entity] entity)
    {
        // Validation logic
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            return new Model_Dao_Result<int>
            {
                IsSuccess = false,
                ErrorMessage = "Name is required"
            };
        }

        // Insert or Update logic
        if (entity.Id == 0)
        {
            await _logger.LogInformationAsync($"Inserting new [entity]:  {entity.Name}");
            return await _dao.InsertAsync(entity);
        }
        else
        {
            await _logger.LogInformationAsync($"Updating [entity] ID {entity.Id}");
            var updateResult = await _dao.UpdateAsync(entity);
            return new Model_Dao_Result<int>
            {
                IsSuccess = updateResult.IsSuccess,
                ErrorMessage = updateResult.ErrorMessage,
                Data = entity.Id
            };
        }
    }

    public async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        await _logger.LogInformationAsync($"Deleting [entity] ID: {id}");
        return await _dao.DeleteAsync(id);
    }
}
```

**Mandatory Service Rules**: 
- Inject DAO instances (not interfaces) via constructor
- Implement business validation before calling DAOs
- Log all operations via `ILoggingService`
- Return `Model_Dao_Result` types consistently
- Register with DI container (singleton or transient as appropriate)

---

## Dependency Injection Setup

**File**:  `App.xaml.cs`

When adding new services/ViewModels, register in `ConfigureServices()`:

```csharp
private static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // Singletons (shared instances)
    services.AddSingleton<ILoggingService, LoggingService>();
    services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
    services.AddSingleton<Dao_[Entity]>();
    services.AddSingleton<I[Service], [Service]>();

    // Transient (new instance each request)
    services.AddTransient<[Entity]ViewModel>();

    return services.BuildServiceProvider();
}
```

**Rules**:
- **Singleton**:  DAOs, infrastructure services, shared state
- **Transient**: ViewModels (new instance per navigation)
- Always register interfaces with implementations (except DAOs - register concrete class)

---

## Error Handling Standards

### In ViewModels
```csharp
try
{
    IsBusy = true;
    var result = await _service. OperationAsync();
    
    if (result.IsSuccess)
    {
        // Success path
        StatusMessage = "Operation completed";
    }
    else
    {
        // User-friendly error
        _errorHandler.ShowUserError(
            result.ErrorMessage,
            "Operation Failed",
            nameof(OperationCommand)
        );
    }
}
catch (Exception ex)
{
    // Unexpected errors
    _errorHandler.HandleException(
        ex,
        Enum_ErrorSeverity.High, // Low/Medium/High/Critical
        nameof(OperationCommand),
        nameof([Entity]ViewModel)
    );
}
finally
{
    IsBusy = false;
}
```

### Error Severity Levels
- **Low**: Non-critical, informational only
- **Medium**: Feature degradation, user can continue
- **High**: Feature failure, user blocked from operation
- **Critical**: Application instability, data integrity risk

---

## Database Query Patterns

### MySQL Stored Procedure Call (Application Database)
```csharp
var parameters = new List<MySqlParameter>
{
    new MySqlParameter("p_Id", MySqlDbType.Int32) { Value = id },
    new MySqlParameter("p_Name", MySqlDbType.VarChar) { Value = name }
};

var result = await Helper_Database_StoredProcedure
    .ExecuteStoredProcedureAsync("sp_Entity_Operation", parameters);

if (!result.IsSuccess)
{
    return new Model_Dao_Result
    {
        IsSuccess = false,
        ErrorMessage = result. ErrorMessage
    };
}
```

### SQL Server Read-Only Query (Infor Visual ERP)
```csharp
var connectionString = "Server=VISUAL;Database=MTMFG;Integrated Security=true;ApplicationIntent=ReadOnly;TrustServerCertificate=True";

using (var connection = new SqlConnection(connectionString))
{
    await connection.OpenAsync();
    
    var query = @"
        SELECT po_num, po_line, part_id, qty_ordered
        FROM po_detail
        WHERE po_num = @PoNumber
            AND site_ref = '002'";
    
    using (var command = new SqlCommand(query, connection))
    {
        command. Parameters.AddWithValue("@PoNumber", poNumber);
        
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                // Map to model
            }
        }
    }
}
```

**Important**: Always include warehouse/site filter (`site_ref = '002'`) for Visual queries.

---

## Common Code Patterns

### Loading List Data
```csharp
[RelayCommand]
private async Task LoadItemsAsync()
{
    if (IsBusy) return;

    try
    {
        IsBusy = true;
        StatusMessage = "Loading items...";

        var result = await _service.GetAllAsync();

        if (result.IsSuccess)
        {
            Items. Clear();
            foreach (var item in result.Data)
            {
                Items.Add(item);
            }
            StatusMessage = $"Loaded {Items. Count} items";
        }
        else
        {
            _errorHandler.ShowUserError(result.ErrorMessage, "Load Failed", nameof(LoadItemsAsync));
            StatusMessage = "Load failed";
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(LoadItemsAsync), nameof(MyViewModel));
        StatusMessage = "Error loading items";
    }
    finally
    {
        IsBusy = false;
    }
}
```

### Saving Data
```csharp
[RelayCommand]
private async Task SaveItemAsync()
{
    if (IsBusy) return;

    try
    {
        IsBusy = true;
        StatusMessage = "Saving... ";

        var result = await _service.SaveAsync(SelectedItem);

        if (result. IsSuccess)
        {
            StatusMessage = "Saved successfully";
            await LoadItemsAsync(); // Refresh list
        }
        else
        {
            _errorHandler. ShowUserError(result.ErrorMessage, "Save Failed", nameof(SaveItemAsync));
            StatusMessage = "Save failed";
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity. High, nameof(SaveItemAsync), nameof(MyViewModel));
        StatusMessage = "Error saving";
    }
    finally
    {
        IsBusy = false;
    }
}
```

### Command Validation
```csharp
[RelayCommand(CanExecute = nameof(CanSaveItem))]
private async Task SaveItemAsync()
{
    // Save logic
}

private bool CanSaveItem()
{
    return ! IsBusy 
        && SelectedItem != null 
        && !string.IsNullOrWhiteSpace(SelectedItem.Name);
}

// Call this whenever validation state changes
partial void OnSelectedItemChanged(Model_Item?  value)
{
    SaveItemCommand.NotifyCanExecuteChanged();
}
```

---

## Navigation Pattern

**Navigation Service**: `INavigationService`

```csharp
// In ViewModel
private readonly INavigationService _navigationService;

[RelayCommand]
private void NavigateToDetail()
{
    _navigationService. NavigateTo(
        typeof([Entity]DetailViewModel).FullName,
        SelectedItem
    );
}
```

**Receiving Navigation Parameter** (in target ViewModel):
```csharp
public void Initialize(object parameter)
{
    if (parameter is Model_[Entity] entity)
    {
        SelectedItem = entity;
        await LoadDetailsAsync();
    }
}
```

---

## Testing Considerations

When generating code, consider:
1. **Testability**: Services should have clear interfaces for mocking
2. **Async/Await**: All database operations are async
3. **Null Safety**: Initialize collections, use null-conditional operators
4. **Thread Safety**: UI updates must occur on UI thread (use `DispatcherQueue` if needed)

---

## File Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| ViewModel | `[Entity]ViewModel.cs` | `ReceivingViewModel.cs` |
| View | `[Entity]Page.xaml` | `ReceivingPage.xaml` |
| Model | `Model_[Entity].cs` | `Model_PurchaseOrder.cs` |
| DAO | `Dao_[Entity].cs` | `Dao_PurchaseOrder.cs` |
| Service Interface | `I[Service].cs` | `IReceivingService.cs` |
| Service Implementation | `[Service].cs` | `ReceivingService.cs` |
| Helper | `Helper_[Category]_[Name].cs` | `Helper_Database_StoredProcedure. cs` |

---

## Common Pitfalls to Avoid

❌ **Don't**:
- Use `Binding` in XAML (use `x:Bind` instead)
- Put business logic in code-behind files
- Use static service access (always use DI)
- Write raw SQL in C# for MySQL operations
- Write to SQL Server database (read-only!)
- Throw exceptions from DAOs (return error results)
- Forget to register new services in `App.xaml.cs`
- Make ViewModels without inheriting from `BaseViewModel`
- Forget `partial` keyword on ViewModels/Models
- Use synchronous database calls

✅ **Do**:
- Use `x:Bind` with explicit `Mode` in XAML
- Keep Views as thin XAML shells
- Inject dependencies via constructors
- Call stored procedures for all MySQL operations
- Return `Model_Dao_Result` from all DAOs
- Set `IsBusy` flag around async operations
- Log operations via `ILoggingService`
- Handle errors via `IService_ErrorHandler`
- Use `ObservableCollection<T>` for data-bound lists
- Initialize properties to avoid null reference exceptions

---

## Code Review Checklist

Before submitting code, verify: 
- [ ] ViewModel inherits from `BaseViewModel` and is `partial`
- [ ] All async methods use try/catch with `IsBusy` flag
- [ ] XAML uses `x:Bind` instead of `Binding`
- [ ] No business logic in View code-behind
- [ ] DAOs return `Model_Dao_Result` types
- [ ] DAOs use stored procedures exclusively (for MySQL)
- [ ] SQL Server queries include `ApplicationIntent=ReadOnly`
- [ ] New services registered in DI container
- [ ] Error handling uses `IService_ErrorHandler`
- [ ] Logging uses `ILoggingService`
- [ ] Properties initialized (no null references)
- [ ] XML doc comments on public members
- [ ] Consistent naming conventions followed

---

## Technology Stack Reference

**Core Framework**:
- . NET 8
- WinUI 3 / Windows App SDK 1.5+
- C# 12

**NuGet Packages**:
- `CommunityToolkit.Mvvm` (8.x) - MVVM helpers
- `Microsoft.Extensions.DependencyInjection` - DI container
- `MySql.Data` - MySQL database access
- `Microsoft.Data.SqlClient` - SQL Server access

**Databases**:
- MySQL 8.x (Application data - `mtm_receiving_application`)
- SQL Server 2019+ (Infor Visual ERP - `MTMFG` - READ ONLY)

---

## Getting Started with New Features

1. **Define Model**:  Create `Model_[Entity].cs` matching database schema
2. **Create DAO**: Build `Dao_[Entity].cs` with CRUD operations
3. **Build Service**:  Implement `I[Service]` interface with business logic
4. **Implement ViewModel**: Create `[Entity]ViewModel` inheriting from `BaseViewModel`
5. **Design View**: Build XAML page with `x:Bind` to ViewModel
6. **Register DI**: Add services/ViewModels to `App.xaml.cs`
7. **Add Navigation**: Wire up navigation in appropriate location
8. **Test Integration**: Verify end-to-end functionality

---

## Support Resources

**Key Helper Classes**:
- `Helper_Database_StoredProcedure` - MySQL stored procedure execution
- `Helper_Database_Variables` - Connection string management
- `BaseViewModel` - ViewModel base class with IsBusy, StatusMessage, logging
- `Model_Dao_Result` - Standard database operation result wrapper

**Core Services**:
- `IService_ErrorHandler` - Centralized error handling and user notifications
- `ILoggingService` - Application logging
- `INavigationService` - Page navigation

---

*This document represents the architectural standards for the MTM Receiving Application. When generating code, always follow these patterns to maintain consistency and reliability.*
# MTM Waitlist Application 2.0 - Copilot Instructions

## Architecture Overview

**Hybrid WPF + WinForms Application** (.NET 8, targeting Windows)
- **WPF**: Main UI shell using MVVM pattern (Views/, ViewModels/, Windows/)
- **WinForms**: Legacy modal dialogs for data entry (WinForms/New Job Setup/, WinForms/Add/, etc.)
- **Database**: MySQL 5.7 via MySql.Data (connection via `ServerSettings` user settings)

### Key Architectural Patterns

1. **MVVM in WPF Layer**
   - ViewModels inherit from `Core/ViewModel.cs` (implements INotifyPropertyChanged)
   - Navigation via `Services/NavigationService.cs` with DI factory pattern
   - Shared state via Stores pattern: `SelectedWaitlistTaskStore` for cross-component communication

2. **WinForms Modal Dialogs**
   - Opened from WPF using `ShowDialog()` (e.g., `NewJobSetup`, `AddNewBox`)
   - Direct MySQL access using DataAdapter/DataTable pattern
   - Property-based return values (set properties before dialog closes)

3. **Database Access Patterns**
   - **WPF Layer**: Static methods in `Core/Database Classes/SQLCommands.cs`
   - **WinForms Layer**: Instance methods in `Core/Database Classes/FormsSQLCommands.cs`
   - Connection string from `ServerSettings.Default.addressSetting` and `portSetting`
   - Database name: hardcoded in queries (varies by context: `application_database`, `waitlist_*`)

## Critical Conventions

### Error Handling (MANDATORY)
**Every try/catch block must use `ErrorHandler.ShowError()`**:
```csharp
try 
{
    // code
}
catch (Exception ex)
{
    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
}
```
- `ErrorHandler.ShowError()` displays error and **shuts down application** (by design)
- Pass method name via `MethodBase.GetCurrentMethod()!.Name`
- All methods in this codebase follow this pattern without exception

### Encapsulation Comments
Files marked `// Encapsulation completed. (2024/07/30)` at top are considered finalized

### Database Patterns
- **No ORM**: Direct MySqlConnection/MySqlCommand usage throughout
- **Column order matters**: `waitlist_active` table uses ordinal reads (`reader.GetString(0)`, etc.)
- **Auto-increment reset**: `ResetIDsOnStartup` runs on app startup for specific tables
- See `Database/README.md` for schema deployment order

### Dependency Injection (App.xaml.cs only)
- Services registered in `App()` constructor using Microsoft.Extensions.DependencyInjection
- ViewModels are singletons with factory pattern: `Func<Type, ViewModel>`
- WinForms dialogs are **not** in DI container (instantiated directly)

## Common Operations

### Opening WinForms Dialog from WPF
```csharp
var dialog = new NewJobSetup(workCenter); // Pass required data via constructor
if (dialog.ShowDialog() == DialogResult.OK)
{
    // Access dialog properties for return values
    var result = dialog.SomeProperty;
}
```

### Database Query Pattern (WPF)
```csharp
var connectionString = SqlCommands.GetConnectionString(null, databaseName, null, null);
using (var connection = new MySqlConnection(connectionString))
{
    var command = new MySqlCommand(query, connection);
    connection.Open();
    // Execute query
}
```

### Navigation in MVVM
```csharp
// Inject INavigationService in ViewModel constructor
_navigationService.NavigateTo<WaitlistViewModel>();
```

## File Organization

- `Core/`: Base classes (ViewModel, ErrorHandler, Observable Object, Database Classes)
- `ViewModels/`: WPF ViewModels (named `*ViewModel.cs`)
- `Views/`: WPF UserControls (XAML + code-behind)
- `Windows/`: WPF Windows (Main Window, UserLogin)
- `WinForms/`: Legacy forms organized by function (New Job Setup, Add dialogs, JobDetails, New Request)
- `Models/`: Domain models (e.g., `WaitlistTask.cs`)
- `Stores/`: Shared state containers
- `Services/`: Application services (NavigationService)
- `Database/`: SQL scripts and deployment tools (MySQL 5.7)

## Development Workflow

### Building
Standard `dotnet build` - No special build scripts required

### Testing Database
PowerShell scripts in `Database/01-Deploy/` for deployment (see Database/README.md)

### Common Gotchas

1. **ServerSettings**: User-scoped settings in `ServerSettings.settings` - defaults to localhost:3306
2. **AutoCompleteComboBox**: Custom WPF ComboBox with auto-complete (in root directory)
3. **Mixed UI Frameworks**: Don't add WPF features to WinForms dialogs or vice versa - keep them separate
4. **Database Names**: Multiple databases referenced (`application_database`, `waitlist_active`, `waitlist_history`, etc.)
5. **Partial Classes**: WinForms use .Designer.cs pattern - only modify main .cs files

## Key Dependencies
- **MySql.Data 8.3.0**: MySQL connector
- **MahApps.Metro.IconPacks 4.11.0**: Icon library for WPF
- **Microsoft.Extensions.DependencyInjection 8.0.0**: DI container
- **OpenXmlPowerTools 4.5.3.2**: Document processing (likely for reports)
- **FluentAssertions 6.12.0**: Testing library

## Security Notes
- Database credentials managed via `ServerSettings` (user-configurable)
- User authentication: `waitlist_users` table with PIN-based login
- Admin privileges checked via `UserType` column

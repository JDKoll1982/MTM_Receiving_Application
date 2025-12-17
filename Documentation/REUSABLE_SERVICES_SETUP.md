# Setup Instructions: Reusable Services for New WinUI 3 Multi-Database Project

This guide shows how to integrate reusable services from MTM WIP Application into a new WinUI 3 project with MySQL and SQL Server support.

---

## Prerequisites

- .NET 8.0 SDK
- Windows 10 version 1809 or later (for WinUI 3)
- MySQL Server (5.7+ or 8.0+) for application database
- SQL Server (any version) for Infor Visual or other enterprise databases (optional)
- Visual Studio 2022 (version 17.8+) with:
  - "Windows application development" workload
  - "Windows App SDK C# Templates" installed
  - ".NET desktop development" workload

---

## Step 1: Create New WinUI 3 Project

### 1.1 Create Project in Visual Studio

1. Open Visual Studio 2022
2. Create New Project → "Blank App, Packaged (WinUI 3 in Desktop)"
3. Set .NET version to .NET 8.0
4. Choose "Windows" as target platform

### 1.2 Project Structure

```
{YourProjectName}/
├── Services/
│   ├── Logging/
│   ├── ErrorHandling/
│   ├── Database/
│   │   ├── MySQL/         # MySQL-specific helpers
│   │   └── SqlServer/     # SQL Server-specific helpers (Infor Visual)
│   ├── UI/
│   ├── Validators/
│   └── Startup/
├── Helpers/
│   ├── MySQL/             # MySQL database helpers
│   └── SqlServer/         # SQL Server database helpers
├── Models/
├── Data/
│   ├── MySQL/             # MySQL DAOs
│   └── SqlServer/         # SQL Server DAOs (Infor Visual)
├── Views/                 # WinUI 3 Pages (instead of Forms)
├── ViewModels/            # MVVM pattern (recommended for WinUI 3)
└── Controls/              # WinUI 3 UserControls
```

---

## Step 2: Install Required NuGet Packages

```powershell
# WinUI 3 SDK (should be included in project template)
dotnet add package Microsoft.WindowsAppSDK --version 1.6.250124002
dotnet add package Microsoft.Windows.SDK.BuildTools --version 10.0.26100.54

# Database dependencies
dotnet add package MySql.Data --version 9.4.0                    # MySQL for app database
dotnet add package Microsoft.Data.SqlClient --version 5.2.2      # SQL Server for Infor Visual

# Dependency Injection
dotnet add package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add package Microsoft.Extensions.Logging --version 8.0.0
dotnet add package Microsoft.Extensions.Hosting --version 8.0.0

# MVVM Toolkit (recommended for WinUI 3)
dotnet add package CommunityToolkit.Mvvm --version 8.3.2

# WebView2 (if using Help System)
dotnet add package Microsoft.Web.WebView2 --version 1.0.2792.45
```

---

## Step 3: Copy Core Services

### 3.1 Logging Service

**Copy Files**:
- `Services/Logging/ILoggingService.cs`
- `Services/Logging/Service_Logging.cs`
- `Services/Logging/Service_LoggingUtility.cs`

**Update Namespaces** (Find & Replace):
```csharp
// OLD
namespace MTM_WIP_Application_Winforms.Services.Logging;

// NEW
namespace {YourProjectNamespace}.Services.Logging;
```

**Dependencies to Copy**:
- `Helpers/Helper_LogPath.cs` → Update `GetLogFilePathAsync()` to use your log directory

**Configuration Changes**:
```csharp
// In Helper_LogPath.cs, update the base log directory
// OLD
string baseDirectory = @"X:\MH_RESOURCE\Material_Handler\MTM WIP App\Logs";

// NEW
string baseDirectory = @"{YourLogDirectory}";
// OR use fallback:
string baseDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "{YourApplicationName}",
    "Logs");
```

---

### 3.2 Error Handler Service

**Copy Files**:
- `Services/ErrorHandling/IService_ErrorHandler.cs`
- `Services/ErrorHandling/Service_ErrorHandler.cs`

**Update Namespaces**:
```csharp
// Find & Replace
MTM_WIP_Application_Winforms → {YourProjectNamespace}
```

**Dependencies to Copy**:
- `Models/Enum_ErrorSeverity.cs` → Error severity levels

**WinUI 3 Changes Required**:
- **Replace WinForms MessageBox with WinUI 3 ContentDialog**:
  - Remove `using MTM_WIP_Application_Winforms.Forms.ErrorDialog;`
  - Create new `ErrorContentDialog.cs` using WinUI 3 ContentDialog
  - Update `Service_ErrorHandler` to show ContentDialog instead of MessageBox

**Example WinUI 3 Error Dialog**:
```csharp
public sealed partial class ErrorContentDialog : ContentDialog
{
    public ErrorContentDialog(Exception ex, string callerName)
    {
        this.XamlRoot = App.MainWindow.Content.XamlRoot;
        this.Title = "Application Error";
        this.Content = $"Error in {callerName}:\n\n{ex.Message}";
        this.PrimaryButtonText = "Retry";
        this.CloseButtonText = "Cancel";
    }
}

// Usage in Service_ErrorHandler:
var dialog = new ErrorContentDialog(ex, callerName);
var result = await dialog.ShowAsync();
return result == ContentDialogResult.Primary; // Retry clicked
```

---

### 3.3 Database Helpers (MySQL + SQL Server)

**Copy Files for MySQL (App Database)**:
- `Helpers/Helper_Database_StoredProcedure.cs` → `Helpers/MySQL/Helper_MySQL_StoredProcedure.cs`
- `Helpers/Helper_Database_Variables.cs` → `Helpers/MySQL/Helper_MySQL_Variables.cs`
- `Helpers/Helper_StoredProcedureProgress.cs` (optional, for long-running operations)

**Create Files for SQL Server (Infor Visual Database)**:
- `Helpers/SqlServer/Helper_SqlServer_StoredProcedure.cs` (adapt from MySQL version)
- `Helpers/SqlServer/Helper_SqlServer_Variables.cs` (adapt from MySQL version)

**SQL Server Adaptation Notes**:
1. Replace `MySqlConnection` with `SqlConnection`
2. Replace `MySqlCommand` with `SqlCommand`
3. Replace `MySqlParameter` with `SqlParameter`
4. Parameter prefix detection: Query `sys.parameters` instead of `INFORMATION_SCHEMA.PARAMETERS`
5. Connection string format: `Server={server};Database={database};User Id={uid};Password={password};TrustServerCertificate=true;`

**Update Namespaces**:
```csharp
// Find & Replace
MTM_WIP_Application_Winforms.Helpers → {YourProjectNamespace}.Helpers
```

**Configuration Changes in `Helper_Database_Variables.cs`**:

```csharp
// BEFORE
public static string GetConnectionString(string? server, string? database, string? uid, string? password)
{
    server ??= Model_Shared_Users.WipServerAddress;  // MTM-specific
    database ??= Model_Shared_Users.Database;         // MTM-specific
    uid ??= "root";
    password ??= "root";
    
    return $"Server={server};Database={database};Uid={uid};Pwd={password};Pooling=false;";
}

// AFTER
public static string GetConnectionString(string? server, string? database, string? uid, string? password)
{
    server ??= "{YOUR_DEFAULT_SERVER}";     // e.g., "172.16.1.104"
    database ??= "{YOUR_DATABASE_NAME}";    // e.g., "my_application_db"
    uid ??= "{YOUR_DB_USERNAME}";           // e.g., "root"
    password ??= "{YOUR_DB_PASSWORD}";      // e.g., "password"
    
    return $"Server={server};Database={database};Uid={uid};Pwd={password};Pooling=false;";
}
```

**Dependencies to Copy**:
- `Models/Model_Dao_Result.cs` → Standard DAO return type
- `Models/Model_Application_Variables.cs` → Application configuration (modify for your app)

---

### 3.4 Model_Dao_Result Pattern

**Copy Files**:
- `Models/Model_Dao_Result.cs`

**Update Namespaces**:
```csharp
// Find & Replace
MTM_WIP_Application_Winforms.Models → {YourProjectNamespace}.Models
```

**Usage Example**:
```csharp
// In your DAOs
public async Task<Model_Dao_Result<DataTable>> GetAllUsersAsync()
{
    return await Helper_Database_StoredProcedure
        .ExecuteDataTableWithStatusAsync(
            Model_Application_Variables.ConnectionString,
            "sp_users_GetAll",
            null);
}

// In your forms/services
var result = await _userDao.GetAllUsersAsync();
if (!result.IsSuccess)
{
    _errorHandler.ShowUserError(result.ErrorMessage);
    return;
}

var users = result.Data; // Safe to access
```

---

### 3.5 Application Variables

**Copy Files**:
- `Models/Model_Application_Variables.cs`

**Update for Your Application**:
```csharp
public static class Model_Application_Variables
{
    // Connection String
    public static string ConnectionString { get; set; } = 
        Helper_Database_Variables.GetConnectionString(
            server: "{YOUR_SERVER}",
            database: "{YOUR_DATABASE}",
            uid: "{YOUR_USERNAME}",
            password: "{YOUR_PASSWORD}");
    
    // Current User
    public static string User { get; set; } = Environment.UserName;
    
    // Command Timeout (seconds)
    public static int CommandTimeoutSeconds { get; set; } = 30;
    
    // Service Provider (for DI)
    public static IServiceProvider? ServiceProvider { get; set; }
    
    // Application Version
    public static string AppVersion { get; set; } = "1.0.0";
}
```

---

## Step 4: Setup Dependency Injection

**Copy Files**:
- `Services/Startup/Service_OnStartup_DependencyInjection.cs`

**Update for Your Services**:
```csharp
public static class Service_OnStartup_DependencyInjection
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Core Services (Singletons)
        services.AddSingleton<ILoggingService, Service_Logging>();
        services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
        
        // Add your application-specific services here
        // services.AddTransient<IYourDao, YourDao>();
        // services.AddTransient<IYourService, YourService>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Set static instances for backward compatibility
        Service_Logging.Instance = serviceProvider.GetRequiredService<ILoggingService>();
        Service_ErrorHandler.Instance = serviceProvider.GetRequiredService<IService_ErrorHandler>();
        
        // Store in application variables
        Model_Application_Variables.ServiceProvider = serviceProvider;
        
        return serviceProvider;
    }
}
```

**In Program.cs or Application Startup**:
```csharp
// Initialize DI container
var serviceProvider = Service_OnStartup_DependencyInjection.ConfigureServices();

// Initialize logging
await Service_Logging.Instance.InitializeAsync();

// Log startup
Service_Logging.Instance.Log("Application started");

// Run your application
Application.Run(new MainForm());
```

---

## Step 5: Database Setup

### 5.1 Create Stored Procedures with Standard Pattern

All stored procedures should follow this pattern for compatibility with `Helper_Database_StoredProcedure`:

```sql
DELIMITER $$

CREATE PROCEDURE `{sp_name}`(
    IN p_InputParam1 VARCHAR(100),
    IN p_InputParam2 INT,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1
            p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = -1;
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Your logic here
    
    SET p_Status = 0;
    SET p_ErrorMsg = 'Success';
    COMMIT;
END$$

DELIMITER ;
```

**Parameter Naming Conventions**:
- **Input**: `p_ParameterName`, `in_ParameterName`, or no prefix
- **Output**: `p_Status` (INT), `p_ErrorMsg` (VARCHAR)
- The helper automatically detects which prefix your stored procedures use

---

### 5.2 Create Required Database Tables (Optional)

For Email Notification Service:
```sql
CREATE TABLE `app_email_notification_config` (
    `ID` INT AUTO_INCREMENT PRIMARY KEY,
    `NotificationType` VARCHAR(50) NOT NULL,
    `Recipient` VARCHAR(255) NOT NULL,
    `IsEnabled` TINYINT(1) DEFAULT 1
);
```

For Feedback Manager Service:
```sql
CREATE TABLE `app_user_feedback` (
    `FeedbackID` INT AUTO_INCREMENT PRIMARY KEY,
    `UserID` INT NOT NULL,
    `FeedbackType` VARCHAR(50),
    `Title` VARCHAR(255),
    `Description` MEDIUMTEXT,
    `Status` VARCHAR(50) DEFAULT 'New',
    `SubmissionDateTime` DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE `app_tracking_number_sequence` (
    `ID` INT AUTO_INCREMENT PRIMARY KEY,
    `SequenceType` VARCHAR(50) UNIQUE NOT NULL,
    `CurrentValue` INT DEFAULT 1
);
```

---

## Step 6: Testing the Integration

### 6.1 Test Logging Service

```csharp
// In your form or service
Service_Logging.Instance.Log("Test message");
Service_Logging.Instance.Log(Enum_LogLevel.Warning, "MyComponent", "Warning message");

try
{
    throw new InvalidOperationException("Test error");
}
catch (Exception ex)
{
    Service_Logging.Instance.LogApplicationError(ex, "Testing error logging");
}
```

**Verify**: Check log files in your configured log directory.

---

### 6.2 Test Error Handler Service

```csharp
try
{
    throw new Exception("Simulated error");
}
catch (Exception ex)
{
    Service_ErrorHandler.Instance.HandleException(
        ex,
        Enum_ErrorSeverity.Medium,
        retryAction: null,
        contextData: null,
        callerName: nameof(MyMethod),
        controlName: this.Name);
}
```

**Verify**: Error dialog should appear with formatted error message.

---

### 6.3 Test Database Helper

```csharp
// Test stored procedure execution
var parameters = new Dictionary<string, object>
{
    { "UserId", 1 }
};

var result = await Helper_Database_StoredProcedure
    .ExecuteDataTableWithStatusAsync(
        Model_Application_Variables.ConnectionString,
        "sp_users_GetById",
        parameters);

if (result.IsSuccess)
{
    Console.WriteLine($"Rows returned: {result.Data.Rows.Count}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

**Verify**: Stored procedure executes and returns expected data.

---

## Step 7: Remove MTM-Specific References

### Find & Replace Operations:

1. **Namespaces**:
   ```
   MTM_WIP_Application_Winforms → {YourProjectNamespace}
   ```

2. **Log Directories**:
   ```
   X:\MH_RESOURCE\Material_Handler\MTM WIP App\Logs → {YourLogPath}
   ```

3. **Database Connection Defaults**:
   ```
   Model_Shared_Users.WipServerAddress → {YourServerAddress}
   Model_Shared_Users.Database → {YourDatabaseName}
   ```

4. **Application-Specific Variables**:
   - Remove any `Model_Shared_Users` references
   - Replace with your own configuration model

---

## Step 8: Strip MTM-Specific Logic from Services

After copying the service files, you need to remove MTM-specific business logic and references. Here's what to look for and remove:

### 8.1 Service_Logging.cs

**Remove/Replace**:
```csharp
// REMOVE: MTM-specific model references
using MTM_WIP_Application_Winforms.Models.Entities; // If present

// FIND: References to Model_Shared_Users
var server = new MySqlConnectionStringBuilder(Model_Application_Variables.ConnectionString).Server;
var userName = Model_Application_Variables.User;

// KEEP AS-IS: This is fine, uses standard Model_Application_Variables
```

**Check `Helper_LogPath.cs`**:
```csharp
// BEFORE (MTM-specific)
string baseDirectory = @"X:\MH_RESOURCE\Material_Handler\MTM WIP App\Logs";
if (!Directory.Exists(baseDirectory))
{
    // MTM network share logic
}

// AFTER (Generic)
string baseDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "{YourApplicationName}",
    "Logs");

// Or use a configurable path
string baseDirectory = Model_Application_Variables.LogDirectory ?? 
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "{YourApplicationName}", "Logs");
```

---

### 8.2 Service_ErrorHandler.cs

**Remove/Replace**:
```csharp
// FIND: Form-specific imports that may not exist in your project
using MTM_WIP_Application_Winforms.Forms.ErrorDialog;
using MTM_WIP_Application_Winforms.Forms.MainForm;

// If you don't have these forms, replace EnhancedErrorDialog with MessageBox:

// BEFORE (MTM-specific)
using var errorDialog = new EnhancedErrorDialog(ex, callerName, controlName, severity, retryAction, contextData);
var result = errorDialog.ShowDialog();

// AFTER (Generic fallback)
string message = $"Error in {callerName}:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
var result = MessageBox.Show(message, "Application Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
return result == DialogResult.Retry;
```

**Remove MTM-Specific Error Handling**:
```csharp
// FIND: Application-specific error recovery logic
private void HandleConnectionRecovery()
{
    // May reference MTM-specific connection recovery logic
}

// SIMPLIFY or REMOVE if not applicable to your app
```

---

### 8.3 Helper_Database_Variables.cs

**Remove/Replace**:
```csharp
// BEFORE (MTM-specific)
public static string GetConnectionString(string? server, string? database, string? uid, string? password)
{
    server ??= Model_Shared_Users.WipServerAddress;  // REMOVE THIS
    database ??= Model_Shared_Users.Database;         // REMOVE THIS
    uid ??= "root";
    password ??= "root";
    
    return $"Server={server};Database={database};Uid={uid};Pwd={password};Pooling=false;";
}

// AFTER (Generic)
public static string GetConnectionString(string? server, string? database, string? uid, string? password)
{
    // Read from config file or environment variables
    server ??= ConfigurationManager.AppSettings["DatabaseServer"] ?? "172.16.1.104";
    database ??= ConfigurationManager.AppSettings["DatabaseName"] ?? "{YourDatabaseName}";
    uid ??= ConfigurationManager.AppSettings["DatabaseUser"] ?? "root";
    password ??= ConfigurationManager.AppSettings["DatabasePassword"] ?? "password";
    
    return $"Server={server};Database={database};Uid={uid};Pwd={password};Pooling=false;";
}
```

**Remove Model_Shared_Users References**:
```csharp
// FIND all references to Model_Shared_Users and replace with:
// - Model_Application_Variables (for app-wide settings)
// - ConfigurationManager.AppSettings (for config file values)
// - Environment variables
// - Hard-coded defaults for your application
```

---

### 8.4 Model_Application_Variables.cs

**Remove/Replace**:
```csharp
// REMOVE: MTM-specific models and references
using MTM_WIP_Application_Winforms.Models.Entities; // If present
using MTM_WIP_Application_Winforms.Models.Shared;   // If present

// REMOVE: MTM-specific properties
public static Model_Shared_Users? SharedUsers { get; set; }  // MTM-specific
public static string WipServerAddress { get; set; }          // MTM-specific
public static bool InforVisualConnected { get; set; }        // MTM-specific

// KEEP: Generic application variables
public static string ConnectionString { get; set; }
public static string User { get; set; }
public static int CommandTimeoutSeconds { get; set; }
public static IServiceProvider? ServiceProvider { get; set; }
public static string AppVersion { get; set; }

// ADD: Your application-specific variables
public static string DatabaseServer { get; set; } = "172.16.1.104";
public static string DatabaseName { get; set; } = "{YourDatabaseName}";
public static string LogDirectory { get; set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "{YourApplicationName}", "Logs");
```

---

### 8.5 Helper_Database_StoredProcedure.cs

**Check for MTM-Specific Logic**:
```csharp
// SEARCH for any references to:
// - Model_Shared_Users
// - MTM-specific stored procedure names
// - MTM-specific error messages

// This file should be mostly generic, but verify:
// 1. No hard-coded stored procedure names
// 2. No MTM-specific error handling
// 3. All Model_Application_Variables references are for generic properties

// KEEP AS-IS if no MTM-specific logic found
```

---

### 8.6 Service_FeedbackManager.cs (Optional Service)

**Remove/Replace**:
```csharp
// FIND: MTM-specific feedback types
var validTypes = new[] { "Bug", "Feature", "Question", "Enhancement" }; // Generic - OK

// FIND: MTM-specific stored procedures
"md_feedback_GetAll" // Keep, but verify your database has this SP

// REMOVE: Any MTM-specific business logic
// Example: Auto-assignment to specific developers
// Example: Integration with MTM systems
```

---

### 8.7 Service_EmailNotification.cs (Optional Service)

**Remove/Replace**:
```csharp
// FIND: MTM-specific email addresses or domains
const string IT_SUPPORT_EMAIL = "itsupport@mtm.com"; // CHANGE THIS

// REPLACE WITH:
string IT_SUPPORT_EMAIL = ConfigurationManager.AppSettings["ITSupportEmail"] ?? "{your-support@email.com}";

// REMOVE: Any MTM-specific notification logic
```

---

### 8.8 Verification Checklist

After stripping MTM-specific logic, verify:

- [ ] No `using MTM_WIP_Application_Winforms.Models.Entities;` statements
- [ ] No `using MTM_WIP_Application_Winforms.Models.Shared;` statements
- [ ] No references to `Model_Shared_Users`
- [ ] No hard-coded paths to `X:\MH_RESOURCE\`
- [ ] No hard-coded MTM server addresses or database names
- [ ] All email addresses updated to your organization
- [ ] All stored procedure names verified against your database
- [ ] All configuration values use `ConfigurationManager` or `Model_Application_Variables`
- [ ] Build succeeds with no compilation errors
- [ ] No runtime errors related to missing MTM-specific models

---

### 8.9 Search & Replace Script

Use this PowerShell script to find remaining MTM-specific references:

```powershell
# Search for MTM-specific patterns
$projectPath = "C:\Path\To\Your\Project"

# Find Model_Shared_Users references
Get-ChildItem -Path $projectPath -Recurse -Filter *.cs | 
    Select-String -Pattern "Model_Shared_Users" | 
    Format-Table Path, LineNumber, Line -AutoSize

# Find MTM namespace references
Get-ChildItem -Path $projectPath -Recurse -Filter *.cs | 
    Select-String -Pattern "MTM_WIP_Application_Winforms" | 
    Format-Table Path, LineNumber, Line -AutoSize

# Find hard-coded paths
Get-ChildItem -Path $projectPath -Recurse -Filter *.cs | 
    Select-String -Pattern "X:\\MH_RESOURCE" | 
    Format-Table Path, LineNumber, Line -AutoSize

# Find MTM-specific email addresses
Get-ChildItem -Path $projectPath -Recurse -Filter *.cs | 
    Select-String -Pattern "@mtm\.com" | 
    Format-Table Path, LineNumber, Line -AutoSize
```

---

## Step 9: Optional Services

### Email Notification Service
```csharp
// Copy: Services/Service_EmailNotification.cs
// Setup: Create app_email_notification_config table
// Register: services.AddSingleton<IService_EmailNotification, Service_EmailNotification>();
```

### Feedback Manager Service
```csharp
// Copy: Services/Service_FeedbackManager.cs
// Setup: Create app_user_feedback, app_tracking_number_sequence tables
// Register: services.AddTransient<IService_FeedbackManager, Service_FeedbackManager>();
```

### DataGridView Helper Service
```csharp
// Copy: Services/UI/Service_DataGridView.cs
// No dependencies, no registration needed
// Use: Service_DataGridView.AutoResizeColumns(myDataGridView);
```

---

## Step 10: Build & Test

```powershell
# Build project
dotnet build {YourProject}.csproj

# Run project
dotnet run --project {YourProject}.csproj
```

**Verification Checklist**:
- [ ] Application starts without errors
- [ ] Logging writes to configured directory
- [ ] Error handler displays errors correctly
- [ ] Database operations execute successfully
- [ ] Model_Dao_Result pattern works as expected
- [ ] No MTM-specific references remain in code
- [ ] All configuration values point to your systems
- [ ] All hard-coded paths updated

---

## Troubleshooting

### Issue: "Logging service not initialized"
**Solution**: Ensure `Service_OnStartup_DependencyInjection.ConfigureServices()` is called before any logging operations.

### Issue: "Connection string invalid"
**Solution**: Verify `Helper_Database_Variables.GetConnectionString()` returns valid MySQL connection string with `Pooling=false`.

### Issue: "Stored procedure parameter mismatch"
**Solution**: Ensure stored procedure parameter names match the `Dictionary<string, object>` keys in your code (without prefix).

### Issue: "Model_Dao_Result.Data is null"
**Solution**: Always check `result.IsSuccess` before accessing `result.Data`.

---

## Next Steps

1. Create your DAO classes using `Model_Dao_Result` pattern
2. Create your business logic services
3. Register services in DI container
4. Build your forms and inject services via constructor
5. Test with real data

---

## Support Files Reference

**Essential Files to Copy** (minimum viable setup):
```
Services/
  Logging/ILoggingService.cs
  Logging/Service_Logging.cs
  ErrorHandling/IService_ErrorHandler.cs
  ErrorHandling/Service_ErrorHandler.cs
  Startup/Service_OnStartup_DependencyInjection.cs
Helpers/
  Helper_Database_StoredProcedure.cs
  Helper_Database_Variables.cs
  Helper_LogPath.cs
Models/
  Model_Dao_Result.cs
  Model_Application_Variables.cs
  Enum_ErrorSeverity.cs
  Enum_LogLevel.cs
```

**Total**: ~12 core files + optional services as needed

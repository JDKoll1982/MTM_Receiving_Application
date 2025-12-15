# Reusable Services for MySQL Projects

This document lists all reusable services from the MTM WIP Application that can be copied to new MySQL-based projects.

---

## Core Services (Essential)

### 1. Logging Service
- **Files**:
  - `Services/Logging/ILoggingService.cs`
  - `Services/Logging/Service_Logging.cs`
  - `Services/Logging/Service_LoggingUtility.cs` (static wrapper for backward compatibility)
- **Dependencies**: `Helper_LogPath`, `Model_Application_Variables`
- **Description**: CSV-based logging with separate files for normal logs, database errors, and application errors. Includes async initialization and background queue processing.

### 2. Error Handler Service
- **Files**:
  - `Services/ErrorHandling/IService_ErrorHandler.cs`
  - `Services/ErrorHandling/Service_ErrorHandler.cs`
- **Dependencies**: `ILoggingService`, WinUI 3 ContentDialog for error display
- **Description**: Centralized error handling with severity levels, retry logic, error frequency tracking, and user-friendly error dialogs.
- **WinUI 3 Note**: Replace WinForms MessageBox/EnhancedErrorDialog with WinUI 3 ContentDialog.

### 3. Database Helper (Multi-Database)
- **Files**:
  - `Helpers/Helper_Database_StoredProcedure.cs` (MySQL helper)
  - `Helpers/Helper_Database_Variables.cs` (MySQL connection strings)
  - `Helpers/Helper_SqlServer_StoredProcedure.cs` (SQL Server helper - create from MySQL version)
  - `Helpers/Helper_SqlServer_Variables.cs` (SQL Server connection strings - create from MySQL version)
  - `Helpers/Helper_StoredProcedureProgress.cs`
- **Dependencies**: `MySql.Data` (MySQL), `Microsoft.Data.SqlClient` (SQL Server), `Model_Dao_Result`, `Model_Application_Variables`
- **Description**: Centralized stored procedure execution for both MySQL and SQL Server with:
  - Automatic parameter prefix detection (INFORMATION_SCHEMA cache for MySQL, sys.parameters for SQL Server)
  - Model_Dao_Result pattern for consistent error handling
  - Transient error retry logic with exponential backoff
  - Performance monitoring
  - Support for DataTable, DataSet, Scalar, NonQuery operations
- **Multi-Database Note**: MySQL for app database, SQL Server for Infor Visual database. Use factory pattern or separate helpers.

---

## Database Support Services

### 4. Connection Recovery Manager
- **Files**:
  - `Services/Database/Service_ConnectionRecoveryManager.cs`
  - `Services/Database/IConnectionRecoveryView.cs`
- **Dependencies**: `ILoggingService`, `Helper_Database_Variables`
- **Description**: Handles database connection recovery with retry logic and user notifications.

---

## Utility Services

### 5. Email Notification Service
- **Files**:
  - `Services/Service_EmailNotification.cs`
- **Dependencies**: Database table `app_email_notification_config`
- **Description**: Manages email notification settings and recipient lists from database configuration.

### 6. Feedback Manager Service
- **Files**:
  - `Services/Service_FeedbackManager.cs`
- **Dependencies**: Database tables `app_user_feedback`, `app_user_feedback_comments`, `app_tracking_number_sequence`
- **Description**: Manages user feedback submissions, comments, status updates, and tracking number generation.

---

## UI Services (WinUI 3 - Requires Adaptation)

### 7. DataGrid Service (WinUI 3)
- **Files**:
  - `Services/UI/Service_DataGrid.cs` (adapt from Service_DataGridView.cs)
- **Dependencies**: None (WinUI 3 DataGrid)
- **Description**: Common DataGrid operations: sorting, filtering, column auto-sizing, cell styling.
- **WinUI 3 Note**: Adapt from WinForms DataGridView to WinUI 3 DataGrid control. API differences exist.

### 8. Summary Panel Service (WinUI 3)
- **Files**:
  - `Services/UI/Service_SummaryPanel.cs` (adapt for WinUI 3)
- **Dependencies**: None (WinUI 3 Grid/StackPanel)
- **Description**: Creates summary panels with statistics cards (count, total, average, etc.).
- **WinUI 3 Note**: Use WinUI 3 Grid, StackPanel, and Card controls instead of WinForms Panel.

### 9. Suggestion Filter Service (WinUI 3)
- **Files**:
  - `Services/UI/Service_SuggestionFilter.cs` (adapt for WinUI 3)
- **Dependencies**: None
- **Description**: Real-time suggestion/autocomplete filtering for ComboBox and ListView.
- **WinUI 3 Note**: Adapt from WinForms ComboBox/ListBox to WinUI 3 ComboBox/ListView with AutoSuggestBox.

---

## Input Validation Services

### 10. Input Validator Service
- **Files**:
  - `Services/Validators/Service_InputValidator.cs`
  - `Services/Interfaces/IValidator.cs`
- **Dependencies**: None
- **Description**: Common input validation patterns (email, phone, numeric, date ranges, required fields).

---

## Help System Services (Optional)

### 11. Help System
- **Files**:
  - `Services/Help/IHelpSystem.cs`
  - `Services/Help/Service_HelpSystem.cs`
  - `Services/Help/Service_HelpContentLoader.cs`
  - `Services/Help/Service_HelpTemplateEngine.cs`
- **Dependencies**: JSON help files, HTML templates, WebView2 (compatible with WinUI 3)
- **Description**: JSON-driven help system with search, navigation, and HTML template rendering.
- **WinUI 3 Note**: WebView2 control works in WinUI 3. Minimal changes needed.

---

## Developer Tools Services (Optional)

### 12. Developer Tools Service
- **Files**:
  - `Services/DeveloperTools/IService_DeveloperTools.cs`
  - `Services/DeveloperTools/Service_DeveloperTools.cs`
- **Dependencies**: `ILoggingService`, `IDao_DeveloperTools`, `IService_FeedbackManager`
- **Description**: Developer diagnostics: log statistics, error groupings, system health monitoring.

---

## Supporting Models (Required for Services)

### Core Models
- **Files**:
  - `Models/Model_Dao_Result.cs` - Standard DAO return type
  - `Models/Model_Application_Variables.cs` - Application-wide variables and configuration
  - `Models/Enum_ErrorSeverity.cs` - Error severity levels (Low, Medium, High, Critical, Fatal)
  - `Models/Enum_LogLevel.cs` - Log levels (Debug, Information, Warning, Error, Critical)

---

## Startup Configuration

### 13. Dependency Injection Setup
- **Files**:
  - `Services/Startup/Service_OnStartup_DependencyInjection.cs`
- **Description**: DI container registration for all services. Configure services as singletons or transient.

---

## Summary

**Essential Services** (Copy these first):
1. ✅ Logging Service (ILoggingService, Service_Logging)
2. ✅ Error Handler Service (IService_ErrorHandler, Service_ErrorHandler)
3. ✅ Database Helper (Helper_Database_StoredProcedure)
4. ✅ Model_Dao_Result pattern
5. ✅ Model_Application_Variables

**Optional Services** (Add as needed):
- Email Notification Service (if sending emails)
- Feedback Manager Service (if user feedback feature)
- DataGridView/UI Services (WinForms helpers)
- Help System (if in-app help needed)
- Developer Tools (diagnostics and monitoring)

**Total Files**: ~30-40 files for core services, ~60-80 files for full suite

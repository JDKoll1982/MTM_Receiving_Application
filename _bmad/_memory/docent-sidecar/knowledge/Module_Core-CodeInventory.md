---
module_name: Module_Core
section: 4
title: Code Inventory
generated: 2026-01-08
scope: "Inventory for Module_Core; companion to Module_Core.md"
---

## Module_Core - Code Inventory

This file contains the detail inventory for Section 4 of the AM workflow.

## Views

| View | Location | Notes |
|---|---|---|
| Main_ReceivingLabelPage | Module_Core/Views/Main/Main_ReceivingLabelPage.xaml | Landing page (not fully reviewed here) |
| Main_DunnageLabelPage | Module_Core/Views/Main/Main_DunnageLabelPage.xaml | Placeholder; points to Dunnage workflow view |
| Main_CarrierDeliveryLabelPage | Module_Core/Views/Main/Main_CarrierDeliveryLabelPage.xaml | Placeholder |

## ViewModels

| ViewModel | Location | Observable Properties | Commands (RelayCommand) | Key Dependencies |
|---|---|---:|---:|---|
| Main_ReceivingLabelViewModel | Module_Core/ViewModels/Main/Main_ReceivingLabelViewModel.cs | 3+ | 4+ | IService_MySQL_ReceivingLine, IService_ErrorHandler, IService_LoggingUtility |
| Main_DunnageLabelViewModel | Module_Core/ViewModels/Main/Main_DunnageLabelViewModel.cs | 10+ | 3+ | (Dunnage + logging + error handling dependencies) |
| Main_CarrierDeliveryLabelViewModel | Module_Core/ViewModels/Main/Main_CarrierDeliveryLabelViewModel.cs | 1+ | 1+ | (Receiving/Carrier label service dependencies) |

## Service Interfaces (Contracts)

Service contracts live in `Module_Core/Contracts/Services/`.

### Authentication & Session

- `IService_Authentication`
  - `Task<Model_AuthenticationResult> AuthenticateByWindowsUsernameAsync(string windowsUsername, IProgress<string>? progress = null);`
  - `Task<Model_AuthenticationResult> AuthenticateByPinAsync(string username, string pin, IProgress<string>? progress = null);`
  - `Task<Model_CreateUserResult> CreateNewUserAsync(Model_User user, string createdBy, IProgress<string>? progress = null);`
  - `Task<Model_ValidationResult> ValidatePinAsync(string pin, int? excludeEmployeeNumber = null);`
  - `Task<Model_WorkstationConfig> DetectWorkstationTypeAsync(string? computerName = null);`
  - `Task<List<string>> GetActiveDepartmentsAsync();`
  - `Task LogUserActivityAsync(string eventType, string username, string workstationName, string details);`

- `IService_UserSessionManager`
  - `Model_UserSession? CurrentSession { get; }`
  - `Model_UserSession CreateSession(Model_User user, Model_WorkstationConfig workstationConfig, string authenticationMethod);`
  - `void UpdateLastActivity();`
  - `void StartTimeoutMonitoring();`
  - `void StopTimeoutMonitoring();`
  - `bool IsSessionTimedOut();`
  - `Task EndSessionAsync(string reason);`
  - `event EventHandler<Model_SessionTimedOutEventArgs> SessionTimedOut;`

### Infor Visual (Read-Only)

- `IService_InforVisual`
  - `Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber);`
  - `Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID);`
  - `Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date);`
  - `Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(string poNumber, string partID);`
  - `Task<bool> TestConnectionAsync();`

### Cross-cutting

- `IService_LoggingUtility` (sync + async logging plus retention helpers)
- `IService_ErrorHandler`
  - `Task HandleErrorAsync(string errorMessage, Enum_ErrorSeverity severity, Exception? exception = null, bool showDialog = true);`
  - `Task LogErrorAsync(string errorMessage, Enum_ErrorSeverity severity, Exception? exception = null);`
  - `Task ShowErrorDialogAsync(string title, string message, Enum_ErrorSeverity severity);`
  - `Task HandleDaoErrorAsync(Model_Dao_Result result, string operationName, bool showDialog = true);`
  - `Task ShowUserErrorAsync(string message, string title, string method);`
  - `void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className);`

### MySQL - Receiving / Dunnage / Preferences

- `IService_MySQL_Receiving`
  - `Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads);`
  - `Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate);`
  - `Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate);`
  - `Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads);`
  - `Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads);`
  - `Task<bool> TestConnectionAsync();`

- `IService_MySQL_PackagePreferences`
  - `Task<Model_PackageTypePreference?> GetPreferenceAsync(string partID);`
  - `Task SavePreferenceAsync(Model_PackageTypePreference preference);`
  - `Task<bool> DeletePreferenceAsync(string partID);`

- `IService_MySQL_ReceivingLine`
  - `Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line);`

- `IService_MySQL_Dunnage` (high-level inventory)
  - Types, specs, parts, loads, inventory, custom fields, and user preferences operations.
  - See `Module_Core/Contracts/Services/IService_MySQL_Dunnage.cs` for the full method list.

## Service Implementations (selected)

| Service | Location | Notes |
|---|---|---|
| Service_Authentication | Module_Core/Services/Authentication/Service_Authentication.cs | Orchestrates auth and audit trail |
| Service_UserSessionManager | Module_Core/Services/Authentication/Service_UserSessionManager.cs | Timer-based session timeout |
| Service_InforVisualConnect | Module_Core/Services/Database/Service_InforVisualConnect.cs | Converts DAO PO lines to Receiving model |
| Service_ErrorHandler | Module_Core/Services/Database/Service_ErrorHandler.cs | Logs + ContentDialog display |
| Service_MySQL_Dunnage | Module_Core/Services/Database/Service_MySQL_Dunnage.cs | Large orchestration surface over Dunnage DAOs |
| Service_MySQL_Receiving | Module_Core/Services/Database/Service_MySQL_Receiving.cs | Uses Dao_ReceivingLoad; throws on failure |
| Service_MySQL_PackagePreferences | Module_Core/Services/Database/Service_MySQL_PackagePreferences.cs | Uses Dao_PackageTypePreference; throws on failure |
| Service_Help | Module_Core/Services/Help/Service_Help.cs | Help key routing and dismissal tracking |

## DAOs (Infor Visual)

| DAO | Location | Key Methods |
|---|---|---|
| Dao_InforVisualConnection | Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs | TestConnectionAsync, GetPOWithPartsAsync, ValidatePoNumberAsync, GetPartByNumberAsync |
| Dao_InforVisualPO | Module_Core/Data/InforVisual/Dao_InforVisualPO.cs | GetByPoNumberAsync, ValidatePoNumberAsync |
| Dao_InforVisualPart | Module_Core/Data/InforVisual/Dao_InforVisualPart.cs | GetByPartNumberAsync, SearchPartsByDescriptionAsync |

## Converters

Converters live in `Module_Core/Converters/`.

- Count: 12
- Typical responsibilities: enum-to-visibility, boolean inversion, formatting for display.

## Helpers

Helpers live in `Module_Core/Helpers/`.

- Notable: `Helper_SqlQueryLoader` loads SQL Server query files from embedded resources or file system.

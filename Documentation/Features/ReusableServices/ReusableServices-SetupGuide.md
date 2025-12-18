# Reusable Services - Setup Guide

## Prerequisites

- .NET 8.0 SDK
- Windows 10 version 1809 or later (for WinUI 3)
- MySQL Server (5.7+ or 8.0+)
- Visual Studio 2022 (17.8+)

## Quick Setup Steps

### 1. Create New WinUI 3 Project

Create "Blank App, Packaged (WinUI 3 in Desktop)" project with .NET 8.0

### 2. Install NuGet Packages

```powershell
dotnet add package Microsoft.WindowsAppSDK --version 1.6.250124002
dotnet add package MySql.Data --version 9.4.0
dotnet add package Microsoft.Data.SqlClient --version 5.2.2
dotnet add package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add package CommunityToolkit.Mvvm --version 8.3.2
```

### 3. Copy Core Services

**Essential Files**:
```
Services/
  Logging/ILoggingService.cs
  Logging/Service_Logging.cs
  ErrorHandling/IService_ErrorHandler.cs
  ErrorHandling/Service_ErrorHandler.cs
Helpers/
  Helper_Database_StoredProcedure.cs
  Helper_Database_Variables.cs
Models/
  Model_Dao_Result.cs
  Model_Application_Variables.cs
  Enum_ErrorSeverity.cs
```

### 4. Update Namespaces

Find & Replace: `MTM_WIP_Application_Winforms` → `{YourProjectNamespace}`

### 5. Configure Database Connection

Update `Helper_Database_Variables.cs`:
```csharp
public static string GetConnectionString(...)
{
    server ??= "{YOUR_SERVER}";
    database ??= "{YOUR_DATABASE}";
    uid ??= "{YOUR_USERNAME}";
    password ??= "{YOUR_PASSWORD}";
    return $"Server={server};Database={database};Uid={uid};Pwd={password};Pooling=false;";
}
```

### 6. Setup Dependency Injection

```csharp
services.AddSingleton<ILoggingService, Service_Logging>();
services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
```

### 7. Test Services

```csharp
Service_Logging.Instance.Log("Test message");
await _errorHandler.HandleErrorAsync("Test error", Enum_ErrorSeverity.Low, null, false);
```

## WinUI 3 Adaptations

Replace WinForms MessageBox with WinUI 3 ContentDialog:

```csharp
var dialog = new ContentDialog
{
    Title = "Error",
    Content = errorMessage,
    CloseButtonText = "OK",
    XamlRoot = App.MainWindow.Content.XamlRoot
};
await dialog.ShowAsync();
```

---

**Last Updated**: December 2025  
**Version**: 1.0.0

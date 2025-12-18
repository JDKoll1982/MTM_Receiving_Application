# Reusable Services - Copilot Guide

## Essential Services to Copy

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

## Usage Examples

```csharp
// Logging
_logger.LogInfo("Operation complete");
_logger.LogError("Error occurred", exception: ex);

// Error Handling
await _errorHandler.HandleErrorAsync("Error message", 
    Enum_ErrorSeverity.Error, ex, showDialog: true);

// Database
var result = await Helper_Database_StoredProcedure
    .ExecuteDataTableWithStatusAsync(connectionString, "sp_GetData", parameters);
```

---

**Last Updated**: December 2025  
**Version**: 1.0.0

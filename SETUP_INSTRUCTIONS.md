# MTM Receiving Application - Setup Instructions

**⚠️ DEPRECATED - Use New Setup Structure**

This file has been superseded by a comprehensive two-phase setup system:

## 📖 **READ THIS FIRST**: [SETUP_GUIDE.md](SETUP_GUIDE.md)

The new setup system is organized into:

1. **[SETUP_PHASE_1_INFRASTRUCTURE.md](SETUP_PHASE_1_INFRASTRUCTURE.md)** - Core infrastructure (database, services, helpers)
   - **Duration**: 2-3 hours
   - **Must complete before Phase 2**

2. **[SETUP_PHASE_2_MVVM_FEATURES.md](SETUP_PHASE_2_MVVM_FEATURES.md)** - MVVM features (ViewModels, Views)
   - **Duration**: 4-6 hours per feature
   - **Integrates with speckit workflow**

3. **[.github/instructions/]((.github/instructions/))** - GitHub Copilot instruction files
   - `database-layer.instructions.md` - DAO patterns
   - `mvvm-pattern.instructions.md` - MVVM patterns
   - More to be created

---

## Why the Change?

- **Better Organization**: Separate infrastructure from feature development
- **Speckit Integration**: Feature-driven development with `/speckit.specify`
- **Phase Gating**: Ensures infrastructure complete before building features
- **Clearer Workflow**: Step-by-step with verification checklists

---

## Quick Migration Guide

If you were following the old setup instructions:

**Old Approach**:
- Copy all templates → Build everything at once → Hope it works

**New Approach**:
1. **Phase 1**: Build solid foundation (database, services)
2. **Verify**: Check all Phase 1 items complete
3. **Phase 2**: Build features iteratively with speckit
4. **Repeat**: Use speckit for each new feature

---

## Legacy Content (For Reference Only)

The following content is preserved for historical reference but should NOT be followed. Use the new phased approach instead.

---

## Template Files to Copy (LEGACY - See Phase 1 Doc Instead)

The following files from `MTM_WIP_Application_WinForms` should be copied to this project as `.txt` reference files:

### Core Models (Copy to `Models/Receiving/` folder)
1. **Model_Dao_Result.cs** → `_TEMPLATE_Model_Dao_Result.txt`
   - Source: `Models/Core/Model_Dao_Result.cs`
   - Destination: `Models/Receiving/_TEMPLATE_Model_Dao_Result.txt`
   - Generic and non-generic result wrappers for DAO operations
   
2. **Model_Application_Variables.cs** → `_TEMPLATE_Model_Application_Variables.txt`
   - Source: `Models/Core/Model_Application_Variables.cs`
   - Destination: `Models/Receiving/_TEMPLATE_Model_Application_Variables.txt`
   - Application-wide configuration variables

### Enums (Copy to `Models/Enums/` folder)
3. **Enum_ErrorSeverity.cs** → `_TEMPLATE_Enum_ErrorSeverity.txt`
   - Source: `Models/Enums/Enum_ErrorSeverity.cs`
   - Error severity levels for error handling

4. **Enum_DatabaseEnum_ErrorSeverity.cs** → `_TEMPLATE_Enum_DatabaseEnum_ErrorSeverity.txt`
   - Source: `Models/Enums/Enum_DatabaseEnum_ErrorSeverity.cs`
   - Database-specific error severity levels

### Database Helpers (Copy to `Helpers/Database/` folder)
5. **Helper_Database_StoredProcedure.cs** → `_TEMPLATE_Helper_Database_StoredProcedure.txt`
   - Source: `Helpers/Helper_Database_StoredProcedure.cs`
   - Core database helper for stored procedure execution

6. **Helper_Database_Variables.cs** → `_TEMPLATE_Helper_Database_Variables.txt`
   - Source: `Helpers/Helper_Database_Variables.cs`
   - Database connection string management

7. **Helper_StoredProcedureProgress.cs** → `_TEMPLATE_Helper_StoredProcedureProgress.txt`
   - Source: `Helpers/Helper_StoredProcedureProgress.cs`
   - Progress feedback for long-running SP operations

### Validation Helpers (Copy to `Helpers/Validation/` folder)
8. **Helper_ValidatedTextBox.cs** → `_TEMPLATE_Helper_ValidatedTextBox.txt`
   - Source: `Helpers/Helper_ValidatedTextBox.cs`
   - Text box validation helper

### Formatting Helpers (Copy to `Helpers/Formatting/` folder)
9. **Helper_ExportManager.cs** → `_TEMPLATE_Helper_ExportManager.txt`
   - Source: `Helpers/Helper_ExportManager.cs`
   - CSV export functionalityDatabase/` folder)
10. **Service_ErrorHandler.cs** → `_TEMPLATE_Service_ErrorHandler.txt`
    - Source: `Services/ErrorHandling/Service_ErrorHandler.cs`
    - Destination: `Services/Database/_TEMPLATE_Service_ErrorHandler.txt`
    - Centralized error handling service

11. **LoggingUtility.cs** → `_TEMPLATE_LoggingUtility.txt`
    - Source: `Services/Logging/LoggingUtility.cs`
    - Destination: `Services/Database/_TEMPLATE_LoggingUtility.txt`
    - Structured CSV logging

12. **Service_DebugTracer.cs** → `_TEMPLATE_Service_DebugTracer.txt`
    - Source: `Services/Debugging/Service_DebugTracer.cs`
    - Destination: `Services/Database/_TEMPLATE_Service_DebugTracer.txt
12. **Service_DebugTracer.cs** → `_TEMPLATE_Service_DebugTracer.txt`
    - Source: `Services/Debugging/Service_DebugTracer.cs`
    - Development debugging/tracing

### Contracts (Copy to `Contracts/Services/` folder)
13. **IService_ErrorHandler.cs** → `_TEMPLATE_IService_ErrorHandler.txt`
    - Source: `Services/ErrorHandling/IService_ErrorHandler.cs`
    - Error handler interface

14. **ILoggingService.cs** → `_TEMPLATE_ILoggingService.txt`
    - Source: `Services/Logging/ILogginReceiving/` folder)
15. **Dao_User.cs** → `_TEMPLATE_Dao_User.txt`
    - Source: `Data/Dao_User.cs`
    - Destination: `Data/Receiving/_TEMPLATE_Dao_User.txt`
    - Example DAO for user operations

16. **Dao_Inventory.cs** → `_TEMPLATE_Dao_Inventory.txt`
    - Source: `Data/Dao_Inventory.cs`
    - Destination: `Data/Receiving/_TEMPLATE_Dao_Inventory.txts

16. **Dao_Inventory.cs** → `_TEMPLATE_Dao_Inventory.txt`
    - Source: `Data/Dao_Inventory.cs`
    - Example DAO for inventory operations

---

## Manual Copy Commands

Run these PowerShell commands from the MTM_WIP_Application_WinForms directory:

```powershell
$source = "C:\Users\johnk\source\repos\MTM_WIP_Application_WinForms"
$dest =  - Core templates go in Receiving subfolder (avoid root clutter)
Copy-Item "$source\Models\Core\Model_Dao_Result.cs" "$dest\Models\Receiving\_TEMPLATE_Model_Dao_Result.txt" -Force
Copy-Item "$source\Models\Core\Model_Application_Variables.cs" "$dest\Models\Receiving\_TEMPLATE_Model_Application_Variables.txt" -Force

# Enums - Already have proper subfolder
Copy-Item "$source\Models\Enums\Enum_ErrorSeverity.cs" "$dest\Models\Enums\_TEMPLATE_Enum_ErrorSeverity.txt" -Force
Copy-Item "$source\Models\Enums\Enum_DatabaseEnum_ErrorSeverity.cs" "$dest\Models\Enums\_TEMPLATE_Enum_DatabaseEnum_ErrorSeverity.txt" -Force

# Helpers - Already have proper subfolders
Copy-Item "$source\Helpers\Helper_Database_StoredProcedure.cs" "$dest\Helpers\Database\_TEMPLATE_Helper_Database_StoredProcedure.txt" -Force
Copy-Item "$source\Helpers\Helper_Database_Variables.cs" "$dest\Helpers\Database\_TEMPLATE_Helper_Database_Variables.txt" -Force
Copy-Item "$source\Helpers\Helper_StoredProcedureProgress.cs" "$dest\Helpers\Database\_TEMPLATE_Helper_StoredProcedureProgress.txt" -Force
Copy-Item "$source\Helpers\Helper_ValidatedTextBox.cs" "$dest\Helpers\Validation\_TEMPLATE_Helper_ValidatedTextBox.txt" -Force
Copy-Item "$source\Helpers\Helper_ExportManager.cs" "$dest\Helpers\Formatting\_TEMPLATE_Helper_ExportManager.txt" -Force

# Services - Place in Database subfolder (most are database-related)
Copy-Item "$source\Services\ErrorHandling\Service_ErrorHandler.cs" "$dest\Services\Database\_TEMPLATE_Service_ErrorHandler.txt" -Force
Copy-Item "$source\Services\Logging\LoggingUtility.cs" "$dest\Services\Database\_TEMPLATE_LoggingUtility.txt" -Force
Copy-Item "$source\Services\Debugging\Service_DebugTracer.cs" "$dest\Services\Database\_TEMPLATE_Service_DebugTracer.txt" -Force

# Contracts - Already have proper subfolder
Copy-Item "$source\Services\ErrorHandling\IService_ErrorHandler.cs" "$dest\Contracts\Services\_TEMPLATE_IService_ErrorHandler.txt" -Force
Copy-Item "$source\Services\Logging\ILoggingService.cs" "$dest\Contracts\Services\_TEMPLATE_ILoggingService.txt" -Force

# Data Examples - Place in Receiving subfolder
Copy-Item "$source\Data\Dao_User.cs" "$dest\Data\Receiving\_TEMPLATE_Dao_User.txt" -Force
Copy-Item "$source\Data\Dao_Inventory.cs" "$dest\Data\Receiving\_TEMPLATE_Dao_Inventory.txt" -Force

Write-Host "All template files copied to proper subfolders!" -ForegroundColor Green
Write-Host "No files left in root of main directories" -ForegroundColor Cyaory.txt" -Force

Write-Host "All template files copied successfully!" -ForegroundColor Green
```

---

## Next Steps After Copying Templates

1. **Review Templates**: Read through each .txt template to understand patterns
2. **Create New Classes**: Use templates as guides for new receiving-specific classes
3. **Adapt for WinUI 3**: Convert WinForms patterns to WinUI 3/MVVM patterns
4. **Update Namespaces**: Change from `MTM_WIP_Application_Winforms` to `MTM_Receiving_Application`
5. **Remove WinForms Dependencies**: Replace with WinUI 3 equivalents (e.g., ContentDialog instead of MessageBox)

---

## Key Patterns to Follow

### DAO Pattern
- All database methods return `Model_Dao_Result<T>` or `Model_Dao_Result`
- Use `Helper_Database_StoredProcedure` for all database access
- Never use `MySqlConnection` directly in DAOs
- All database operations are async

### Error Handling
- Use `Service_ErrorHandler` for all error display
- Never use `MessageBox.Show` directly
- Log errors with `LoggingUtility`

### MVVM Pattern (New for WinUI 3)
- **ViewModels**: Inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- **Commands**: Use `RelayCommand` / `AsyncRelayCommand`
- **Services**: Inject via constructor (Dependency Injection)
- **Views**: XAML pages with data binding

---

## Folder Structure Created

```
MTM_Receiving_Application/
├── Models/
│   ├── Receiving/
│   ├── Labels/
│   ├── Enums/
│   └── Lookup/
├── ViewModels/
│   ├── Receiving/
│   ├── Labels/
│   └── Shared/
├── Views/
│   ├── Receiving/
│   ├── Labels/
│   └── Shared/
├── Services/
│   ├── Database/
│   ├── Export/
│   ├── InforVisual/
│   └── LabelView/
├── Helpers/
│   ├── Database/
│   ├── Formatting/
│   └── Validation/
├── Data/
│   ├── Receiving/
│   ├── Labels/
│   └── Lookup/
├── Contracts/
│   ├── Services/
│   └── Data/
├── Assets/
│   ├── Icons/
│   └── Images/
└── Database/
    ├── StoredProcedures/
    │   ├── Receiving/
    │   └── Labels/
    └── Schemas/
```

---

**Last Updated**: December 15, 2025  
**Project**: MTM Receiving Label Application  
**Platform**: WinUI 3 (.NET 8.0)  
**Pattern**: MVVM with Dependency Injection

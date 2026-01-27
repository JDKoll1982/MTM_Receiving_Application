## **AI Agent Instructions - CodeForge**

**Elite AI development assistant for .NET/WinUI 3 enterprise applications.**

### Core Rules
1. **Work in batches** (5-10 files) - Don't stop after 1-2 files
2. **Review before creating** - Read 2+ similar files FIRST
3. **Standard C# casing** - `PoNumber` properties, `PONumber` in class/method names
4. **Complete logical units** - Finish entire sections before switching
5. **Update task files** - Mark progress after each batch

### Required Reading
- `.github/copilot-instructions.md` - Architecture & naming
- `.github/instructions/csharp.instructions.md` - C# best practices
- `.github/instructions/dotnet-architecture-good-practices.instructions.md` - .NET patterns
- `.github/instructions/memory-bank.instructions.md` - Memory management
- `.github/instructions/serena-tools.instructions.md` - Serena tooling
- `.github/taming-copilot.instructions.md` - Interaction patterns

---

## ⚠️ CRITICAL LESSONS FROM SYSTEMATIC ERRORS

### Session History: 86+ Compilation Errors Fixed (2025-01-XX)

**Root Cause:** Created 6 ViewModels and 8 XAML files WITHOUT reviewing existing code patterns first.

**Systematic Errors Found:**
1. Wrong base class constructor (missing IService_Notification parameter)
2. Wrong query names (ValidatePONumberQuery → should be QueryRequest_Receiving_Shared_Validate_PONumber)
3. Wrong entity names (Model_Receiving_Entity_PartType → should be Model_Receiving_TableEntitys_PartType)
4. Wrong error handler API (ShowUserError → should be ShowUserErrorAsync)
5. Wrong enum (Enum_Shared_Severity_ErrorSeverity → should be Enum_ErrorSeverity)
6. Missing using directives (System, System.Text.Json, etc.)
7. XAML x:DataType on UserControl (not supported - remove it)
8. Duplicate commands created with wrong names
9. Po/PO casing inconsistency (fixed: use standard PascalCase for properties)

**Result:** 20+ files had to be deleted, recreated, or heavily modified.

---

## 🔴 GOLDEN RULES - MANDATORY COMPLIANCE

### Rule #1: PO Naming Standard (UPDATED - Accepts Standard PascalCase)

**PO-related identifiers follow standard C# PascalCase conventions:**

```csharp
// ✅ CORRECT - Standard PascalCase (CommunityToolkit.Mvvm compatible)
[ObservableProperty]
private string _poNumber;        // Generates: PoNumber ✅
[ObservableProperty]
private bool _isPoValid;         // Generates: IsPoValid ✅
[ObservableProperty]
private bool _isNonPo;           // Generates: IsNonPo ✅
public string PoNumber { get; }  // ✅
partial void OnPoNumberChanged(string value)  // ✅

// ✅ ALSO ACCEPTABLE - All caps for constants, method names, commands
public const string PO_PREFIX = "PO-";  // ✅ (constant)
ValidatePONumberCommand                  // ✅ (command suffix)
GetPODetailsAsync()                      // ✅ (method name)
```

**Rationale:** CommunityToolkit.Mvvm source generator uses strict PascalCase rules. 
- `_poNumber` → generates `PoNumber` (standard C# casing)
- Attempting `_pONumber` to get `PONumber` breaks source generator patterns
- This is the industry-standard approach used by Microsoft and .NET libraries

**Applies to:**
- C# ObservableProperty fields: `_poNumber` → `PoNumber`
- Generated properties: `PoNumber`, `IsPoValid`, `IsNonPo`
- XAML bindings: `{x:Bind ViewModel.PoNumber}` ✅
- SQL parameters: `@p_PONumber` or `@p_PoNumber` (both acceptable)

**Still Required - All Caps PO:**
- Constants: `PO_PREFIX`, `PO_NUMBER_MAX_LENGTH`
- Command suffixes: `ValidatePONumberCommand`, `SearchPOCommand`
- Method names: `GetPODetailsAsync()`, `ValidatePONumberAsync()`
- Class names: `View_Receiving_Wizard_Display_PONumberEntry` ✅

**Summary:** `PoNumber` properties are CORRECT, `PONumber` commands/methods are CORRECT.

---

### Rule #2: MANDATORY Code Review Workflow

**BEFORE creating ANY file, complete this checklist:**

0. ️✅ **Do not Assume that task files and docuemntation are always up to date**
    - Always verify against ACTUAL existing code files in the codebase.
    - Always read COMPLETE implementations, not just signatures or snippets.
    - Always verify ALL dependencies exist (queries, commands, enums, DTOs, services, etc.)
    - Always document patterns found for future reference.
    - Always follow VERIFIED patterns ONLY when creating new files.
    - Expect 10-50 compilation errors PER FILE if you skip this workflow.

1. ✅ **Find & Read 2+ similar existing files**
   - Same file type (ViewModel, View, DAO, etc.)
   - Same module (Module_Receiving, Module_Dunnage, etc.)
   - Read COMPLETE implementation (not just signature)

2. ✅ **Document patterns found:**
```markdown
- Namespace: MTM_Receiving_Application.Module_Receiving.ViewModels.Hub (EXACT)
- Base class: ViewModel_Shared_Base(errorHandler, logger, notificationService) ← 3 params
- Using directives: System, System.Linq, System.Threading.Tasks, CommunityToolkit.Mvvm.ComponentModel, MediatR
- Property pattern: _poNumber → PoNumber (ObservableProperty, standard PascalCase)
- Commands: QueryRequest_Receiving_Shared_Validate_PONumber (FULL name, not simplified)
- Error API: await _errorHandler.ShowUserErrorAsync(...) (ASYNC)
```

3. ✅ **Verify ALL dependencies exist:**
   ```markdown
   ✅ Base class constructor signature verified
   ✅ Query/Command files exist (use file_search)
   ✅ Enum values verified (read enum file)
   ✅ DTO properties verified (read DTO file)
   ✅ Service interfaces verified (read interface file)
   ```

4. ✅ **Create file using VERIFIED patterns only**

**If you skip this workflow → Expect 10-50 compilation errors PER FILE.**

---

### Rule #3: XAML View Patterns (EXACT)

**UserControl Root Element:**
```xaml
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_Wizard_Display_PONumberEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    Loaded="OnLoaded">
    <!-- ❌ DO NOT ADD: x:DataType (not supported on UserControl) -->
    <!-- ❌ DO NOT ADD: xmlns:viewmodels (namespace incorrect - ViewModels are in subfolders) -->
```

**Code-Behind Pattern (DI Constructor):**
```csharp
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Hub; // ← Include subfolder!

namespace MTM_Receiving_Application.Module_Receiving.Views;

public sealed partial class View_Receiving_Hub_Display_ModeSelection : UserControl
{
    public ViewModel_Receiving_Hub_Display_ModeSelection ViewModel { get; }

    // ✅ DI Constructor (preferred)
    public View_Receiving_Hub_Display_ModeSelection(ViewModel_Receiving_Hub_Display_ModeSelection viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    // ✅ Parameterless constructor (for XAML instantiation - GetService is still used)
    public View_Receiving_Hub_Display_ModeSelection()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Hub_Display_ModeSelection>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
```

**Note:** App.GetService is still used in parameterless constructors despite being "legacy" - check Dunnage views for confirmation.

---

### Rule #4: ObservableProperty Naming (Standard PascalCase)

```csharp
// ✅ CORRECT - CommunityToolkit.Mvvm source generator standard
[ObservableProperty]
private string _poNumber;      // Generates: PoNumber ✅

[ObservableProperty]
private bool _isPoValid;       // Generates: IsPoValid ✅

[ObservableProperty]
private bool _isNonPo;         // Generates: IsNonPo ✅

// Partial methods MUST match generated property names:
partial void OnPoNumberChanged(string value)   // Match PoNumber property
partial void OnIsPoValidChanged(bool value)    // Match IsPoValid property
partial void OnIsNonPoChanged(bool value)      // Match IsNonPo property
```

**CRITICAL RULE for CommunityToolkit.Mvvm Source Generator:**
- Follow standard PascalCase: `_poNumber` → `PoNumber`
- Source generator converts `_fieldName` to `FieldName` (capitalize first letter only)
- For multi-word acronyms at start: `_poNumber` → `PoNumber` (not `PONumber`)
- For acronyms mid-word: `_isPoValid` → `IsPoValid` (standard camelCase rules)

---

### Rule #5: Constructor Signatures (EXACT)

**ViewModel_Shared_Base requires 3 parameters (not 2):**
```csharp
// ❌ WRONG - Missing IService_Notification
public ViewModel_Receiving_XXX(
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger)
    : base(errorHandler, logger)  // ← COMPILATION ERROR

// ✅ CORRECT - All 3 parameters
public ViewModel_Receiving_XXX(
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Notification notificationService)
    : base(errorHandler, logger, notificationService)
```

---

### Rule #6: Query/Command Naming (FULL PATTERN ONLY)

```csharp
// ❌ WRONG - Simplified names DON'T exist
new ValidatePONumberQuery()
new GetPartDetailsQuery()
new BulkCopyFieldsCommand()

// ✅ CORRECT - Use FULL 5-part pattern
new QueryRequest_Receiving_Shared_Validate_PONumber()
new QueryRequest_Receiving_Shared_Get_PartDetails()
new CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads()
```

**Pattern:** `{Type}Request_{Module}_{Mode}_{Action}_{EntityName}`

---

### Rule #7: Error Handler API (ASYNC ONLY)

```csharp
// ❌ WRONG - Sync methods don't exist
_errorHandler.ShowUserError(...);
Enum_Shared_Severity_ErrorSeverity.Medium  // Wrong enum
Enum_ErrorSeverity.High                     // Doesn't exist

// ✅ CORRECT - Async methods, correct enum
await _errorHandler.ShowUserErrorAsync(...);
await _errorHandler.ShowErrorDialogAsync(title, message, Enum_ErrorSeverity.Medium);
_errorHandler.HandleException(ex, Enum_ErrorSeverity.Critical, method, className);

// Valid severity values: Info, Low, Warning, Medium, Error, Critical, Fatal
```

---

## 📚 Quick Reference

**Critical Using Directives:**
```csharp
using System.Text.Json;  // For JSON serialization - LoadDetailsJson
```

**Critical Patterns:**
- **JSON:** `JsonSerializer.Serialize(Loads.ToList())` and `Deserialize<List<DTO>>(...)`
- **GUID Conversion:** `Guid.TryParse(SessionId, out var guid)` and `.ToString()`
- **Reference Data:** Access `refDataResult.Data.PartTypes` (not `refDataResult.Data` directly)

**Entity Naming:**
- Entities (DB tables): `Model_Receiving_TableEntitys_XXX`
- DTOs: `Model_Receiving_DataTransferObjects_XXX`

**CQRS Naming:**
- Commands: `CommandRequest_Receiving_{Mode}_{Action}_{Entity}`
- Queries: `QueryRequest_Receiving_{Mode}_{Action}_{Entity}`
- Handlers: `CommandHandler_` or `QueryHandler_` + same pattern

**Common Using Directives (C#):**
```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;  // For JSON serialization
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Receiving.ViewModels.{Subfolder};  // ← Include subfolder!
using MTM_Receiving_Application.Module_Shared.ViewModels;
```

**Common xmlns (XAML):**
```xaml
xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
xmlns:dto="using:MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects"
<!-- DO NOT USE: xmlns:viewmodels - ViewModels are in subfolders, won't resolve -->
<!-- DO NOT USE: x:DataType on UserControl - not supported -->
```

---

## ✅ Verification Checklist Before Creating Files

- [ ] Found and read 2+ similar existing files
- [ ] Documented namespace (include subfolders!)
- [ ] Verified base class constructor signature
- [ ] Checked all referenced classes exist (file_search)
- [ ] Verified enum values (read enum file)
- [ ] Confirmed query/command names (FULL pattern)
- [ ] Added ALL required using statements
- [ ] Applied standard C# PascalCase (PoNumber properties, PONumber class/method names)
- [ ] Reviewed XAML binding patterns (x:Bind, no x:DataType on UserControl)
- [ ] Added both DI constructor AND parameterless constructor to views

**If ANY checkbox is unchecked → DO NOT CREATE FILE YET.**

---

## 📊 Current Progress Tracking

**Last Updated:** 2025-01-23 | **Branch:** `copilot/follow-instructions-in-agent-files`

### Current Status: ✅ HUB VIEWS COMPLETE - Ready for Build Verification

**Completed This Session:**
- ✅ Updated Golden Rules to accept standard PascalCase (Rule #1 & #4)
- ✅ Deleted 5 problematic XAML files from previous session
- ✅ Created View_Receiving_Hub_Display_ModeSelection (XAML + code-behind)
- ✅ Created View_Receiving_Hub_Orchestration_MainWorkflow (XAML + code-behind)
- ✅ Verified patterns from existing Dunnage views
- ✅ Documented correct naming conventions

**Hub Views Created (2/2 - 100%):**
1. View_Receiving_Hub_Display_ModeSelection.xaml ✅
2. View_Receiving_Hub_Display_ModeSelection.xaml.cs ✅
3. View_Receiving_Hub_Orchestration_MainWorkflow.xaml ✅
4. View_Receiving_Hub_Orchestration_MainWorkflow.xaml.cs ✅

**Next Immediate Tasks:**
1. **Test build** - Verify Hub views compile successfully
2. **Register views in DI** (App.xaml.cs) - Add both Hub views
3. **Create Wizard views** (Step 1, Step 2, Step 3)
4. **Create Wizard orchestration** view
5. **Final integration testing**

---

**See Also:**
- `.github/copilot-instructions.md` - Complete architecture rules
- `Module_Receiving/COMPILATION_FIXES_2025-01-XX.md` - Detailed error analysis (18 error types)
- `Module_Receiving/FINAL_AUDIT_REPORT_2025-01-XX.md` - Component inventory
- `Module_Receiving/SESSION_SUMMARY_2025-01-XX.md` - Current session details
- `Fix-PoToUpperCase.ps1` - Automated Po→PO fix script

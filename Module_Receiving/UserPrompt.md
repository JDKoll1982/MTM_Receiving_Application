## **AI Agent Instructions - CodeForge**

MANDATORY - THIS SECTION MUST BE UPDATED AFTER EACH WORK SESSION

**📅 LAST SESSION:** 2025-01-30 | **WORK COMPLETED:** Documentation & Planning Phase

**✅ THIS SESSION DELIVERABLES:**
1. 📋 Created `COMPLETE_TASK_INVENTORY.md` - All 351 tasks with file references & line numbers
2. 📊 Updated `tasks_master.md` - Added comprehensive task inventory reference
3. 📝 Created `Module_Receiving_User_Approved_Implementation_Plan.md` - Full user selections
4. 🔗 Created `Module_Receiving_Comparison_Task_Cross_Reference.md` - Task mappings
5. 📝 Updated `Module_Receiving_vs_Old_Module_Receiving_Comparison.md` - Business logic
6. ✅ Created `md-file-structure-rules.instructions.md` - Markdown standards
7. 📝 Created `Module_Receiving_Feature_Cherry_Pick.md` - User selection board
8. 📄 Created `TASK_MASTER_UPDATE_SUMMARY.md` - Complete summary
9. ✅ Fixed markdown formatting in all files (proper checkbox syntax)
10. 📝 Updated `UserPrompt.md` - Added comprehensive implementation guide

**🔴 DOCUMENTATION COMPLETE - READY FOR IMPLEMENTATION**

---

**🎯 NEXT MISSION:** Begin Quality Hold Management Implementation (P0 CRITICAL)

**📊 CURRENT PROJECT STATUS:** 131/351 tasks complete (37%)
- ✅ **Phase 1:** Foundation - 100% (20/20 tasks)
- ✅ **Phase 2:** Database & DAOs - 91% (64/70 tasks) - 6 DAO tests + 8 Quality Hold tasks
- ✅ **Phase 3:** CQRS Handlers - 81% (34/42 tasks) - 6 validator tests + 8 user-approved
- 🟡 **Phase 4:** Wizard ViewModels - 20% (13/64 tasks) - 51 remaining + user-approved
- ⏳ **Phase 5:** Views & UI - 0% (0/62 tasks) - All wizard views pending
- ⏳ **Phase 6:** Integration - 0% (0/30 tasks) - DI, navigation, testing
- ⏳ **Phase 7:** Settings - 0% (0/35 tasks) - Settings infrastructure
- ⏳ **Phase 8:** Testing - 0% (0/28 tasks) - Unit/integration tests

**🔴 IMMEDIATE NEXT STEPS:** Quality Hold P0 Implementation (18 total tasks)
1. **Phase 2 Database** (Tasks 2.63-2.70) - 8 tasks, ~10.5 hours
2. **Phase 3 CQRS** (Tasks 3.41-3.46) - 6 tasks, ~8 hours
3. **Phase 4 Services** (Tasks 4.31-4.33) - 3 tasks, ~6 hours
4. **Phase 6 Integration** (Task 6.21) - 1 task, ~1 hour
5. **Phase 8 Testing** (Task 8.27) - 1 task, ~3 hours

**📋 COMPLETE TASK LIST:** `Module_Receiving/COMPLETE_TASK_INVENTORY.md` - All 351 tasks with file references

**📚 KEY DOCUMENTS:**
- `Module_Receiving_User_Approved_Implementation_Plan.md` (UA) - User selections & requirements
- `Module_Receiving_vs_Old_Module_Receiving_Comparison.md` (CMP) - Business logic details
- `Module_Receiving_Comparison_Task_Cross_Reference.md` (XR) - Task mappings

**⚡ START HERE - CSV Export CQRS:**
```
Phase 3 CQRS Tasks 3.47-3.48 (2 tasks, ~1.5 hours)

Create in Module_Receiving/:
1. Requests/Commands/CommandRequest_Receiving_Shared_Export_TransactionToCSV.cs
   - Properties: TransactionId, ExportPath, IncludeNetworkCopy, NetworkPath
   
2. Models/Results/Model_Receiving_Result_CSVExport.cs
   - Properties: LocalFilePath, NetworkFilePath, RowCount, IsSuccess, ErrorMessage

Reference: UA:Lines 437-487, CMP:Section 6

Key Requirements:
- Blocking mode only (export during save)
- User-configurable export path
- Optional network copy
- Return detailed export results
```

**⚠️ USER REQUIREMENTS (MANDATORY):**
- Configurable part patterns (NOT hardcoded MMFSR/MMCSR)
- Two-step acknowledgment dialogs ("Acknowledgment 1 of 2", "Acknowledgment 2 of 2")
- Hard block on save (cannot save without full acknowledgment)
- Full audit trail (all acknowledgment fields tracked)

**📋 IMPLEMENTATION SEQUENCE:**
1. 🔴 **P0 Quality Hold** → Database (Phase 2) → CQRS (Phase 3) → Services (Phase 4) → UI (Phase 5-6)
2. 🟡 **P1 CSV Export** → Single path, blocking mode, user-configurable via settings
3. 🟢 **P2 UX Enhancements** → Auto-defaults, session mgmt, validation, user prefs
4. 📺 **Phase 5 Views** → All wizard XAML views (62 tasks, blocks user testing)

**✅ BEFORE CREATING FILES:**
- Review 2+ existing similar files FIRST
- Verify dependencies exist (SPs, entities, enums)
- Check naming patterns in existing code
- Document patterns found before creating

**📝 AFTER EACH BATCH (5-10 files):**
- Update COMPLETE_TASK_INVENTORY.md (mark ✅)
- Test compilation (ensure no errors)
- Update phase percentages in tasks_master.md
- Commit with clear message

---

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

**Last Updated:** 2025-01-30 | **Branch:** `copilot/follow-instructions-in-agent-files`

### Current Status: ✅ DOCUMENTATION COMPLETE - Ready for Quality Hold Implementation

**Completed This Session (2025-01-30):**
- ✅ Created comprehensive task inventory (COMPLETE_TASK_INVENTORY.md) - All 351 tasks
- ✅ Integrated 100 user-approved features with cross-references
- ✅ Updated tasks_master.md with complete task tracking
- ✅ Created markdown formatting standards (md-file-structure-rules.instructions.md)
- ✅ Fixed markdown formatting in all comparison documents
- ✅ Created complete implementation roadmap with file references
- ✅ Updated UserPrompt.md with comprehensive guide
- ✅ Documented all user requirements with UPDATE notes

**Project Statistics:**
- **Total Tasks:** 351 (251 original + 100 user-approved)
- **Completed:** 149/351 (42%)
- **Remaining:** 202/351 (58%)
- **Estimated Effort:** ~440-460 hours total

**Phase Breakdown:**
- Phase 1: Foundation - ✅ 100% (20/20) ✅ COMPLETE
- Phase 2: Database & DAOs - ✅ 100% (70/70) ✅ COMPLETE
- Phase 3: CQRS Handlers - ✅ 95% (40/42) - 2 validator tests remaining
- Phase 4: Wizard ViewModels - 🟡 25% (16/64) - Quality Hold complete
- Phase 5: Views & UI - ⏳ 0% (0/62) - Wizard views pending
- Phase 6: Integration - ⏳ 3% (1/30) - Quality Hold DI complete
- Phase 7: Settings - ⏳ 0% (0/35) - Settings infrastructure
- Phase 8: Testing - ⏳ 0% (0/28) - Quality Hold E2E next

**🔴 Next Immediate Tasks: Quality Hold P0 Implementation (18 tasks, ~28.5 hours)**

1. **Phase 2 Database** (Tasks 2.63-2.70, ~10.5 hours)
   - Create tbl_Receiving_QualityHold table
   - Create 3 stored procedures (Insert, UpdateFinal, SelectByLine)
   - Create Model_Receiving_Entity_QualityHold entity
   - Create Dao_Receiving_Repository_QualityHold DAO
   - Add QualityHold fields to ReceivingLoad entity
   - Add WeightPerPackage column to Line table

2. **Phase 3 CQRS** (Tasks 3.41-3.46, ~8 hours)
   - Create QualityHold command + handler
   - Create QualityHold query + handler
   - Create RestrictedPart validator (configurable patterns)
   - Enhance SaveTransaction validator (hard blocking)

3. **Phase 4 Services** (Tasks 4.31-4.33, ~6 hours)
   - Create Service_Receiving_QualityHoldDetection (two-step dialogs)
   - Enhance PartSelection ViewModel (first warning)
   - Enhance SaveOperation ViewModel (final warning + blocking)

4. **Phase 6 Integration** (Task 6.21, ~1 hour)
   - Register Quality Hold service and DAO in DI

5. **Phase 8 Testing** (Task 8.27, ~3 hours)
   - Create Quality Hold E2E integration test

---

**📚 Key Reference Documents:**

**Task Management:**
- `Module_Receiving/COMPLETE_TASK_INVENTORY.md` - All 351 tasks with file references
- `Module_Receiving/tasks_master.md` - Master task list summary
- `Module_Receiving/TASK_MASTER_UPDATE_SUMMARY.md` - Complete update summary

**Implementation Guides:**
- `Module_Receiving_User_Approved_Implementation_Plan.md` (UA) - User selections with UPDATE notes
- `Module_Receiving_vs_Old_Module_Receiving_Comparison.md` (CMP) - Business logic comparison
- `Module_Receiving_Comparison_Task_Cross_Reference.md` (XR) - Task cross-reference

**Standards:**
- `.github/copilot-instructions.md` - Complete architecture rules
- `.github/instructions/md-file-structure-rules.instructions.md` - Markdown standards
- `.github/instructions/csharp.instructions.md` - C# best practices
- `.github/instructions/sql-sp-generation.instructions.md` - SQL stored procedure patterns

**Feature Selection:**
- `Module_Receiving_Feature_Cherry_Pick.md` - User feature selections with checkboxes
- `MARKDOWN_FORMATTING_FIX_SUMMARY.md` - Formatting fix summary

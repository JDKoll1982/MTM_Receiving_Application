# Service Locator Elimination - Continuation Prompt

**Context for GitHub Copilot Chat**

Use this prompt to continue the Service Locator elimination refactoring work in Visual Studio.

---

## What We've Completed

We are systematically eliminating the Service Locator anti-pattern (`App.GetService<T>()`) from all View classes in favor of constructor injection, following proper MVVM and dependency injection principles.

### ✅ Phase 1 Complete: Module_Settings Views (29 files)

**Successfully refactored all Module_Settings.* views to use constructor injection:**

- Module_Settings.Routing: 5 views
- Module_Settings.Reporting: 3 views
- Module_Settings.Dunnage: 5 views
- Module_Settings.Receiving: 3 views
- Module_Settings.Volvo: 3 views
- Module_Settings.DeveloperTools: 5 views
- Module_Settings.Core: 5 views

**Total eliminated:** 29 Service Locator calls  
**Build status:** ✅ 0 errors, 0 warnings  
**Progress:** 31% complete (29 of 95 total calls eliminated)

### Pattern Applied (Successfully)

All 29 views were converted from:

```csharp
// BEFORE (Service Locator - Anti-pattern)
public View_Settings_MyView()
{
    ViewModel = App.GetService<ViewModel_Settings_MyView>();
    InitializeComponent();
    DataContext = ViewModel;
}
```

To:

```csharp
// AFTER (Constructor Injection - Proper DI)
using System;

public View_Settings_MyView(ViewModel_Settings_MyView viewModel)
{
    ArgumentNullException.ThrowIfNull(viewModel);
    ViewModel = viewModel;
    InitializeComponent();
    DataContext = ViewModel;
}
```

### DI Registration

All views registered in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`:

```csharp
private static void RegisterSettingsViews(IServiceCollection services)
{
    // Routing Settings Views
    services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_FileIO>();
    services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_UiUx>();
    // ... (29 total registrations)
}
```

---

## What Still Needs to Be Done

**Remaining:** 66 Service Locator calls across multiple modules

### Phase 1 Remaining: Simple Views (~10 files)

**Module_Routing** (6 views - ViewModel only):
- RoutingModeSelectionView
- RoutingWizardStep1View
- RoutingWizardStep2View
- RoutingWizardStep3View
- RoutingManualEntryView
- RoutingEditModeView

**Module_Reporting** (1 view - ViewModel only):
- View_Reporting_Main

**Module_Volvo** (3 views - ViewModel only):
- View_Volvo_History
- View_Volvo_Settings
- View_Volvo_ShipmentEntry

### Phase 2: Moderate Complexity (~32 files)

Views with 2-3 dependencies (ViewModel + services):

**Module_Receiving** (11 views):
- Example: `View_Receiving_POEntry` has ViewModel + IService_Focus
- Example: `View_Receiving_WeightQuantity` has ViewModel + IService_Focus
- Pattern: Most need ViewModel + 1-2 service parameters

**Module_Dunnage** (18 views):
- Example: `View_Dunnage_PartSelectionView` has ViewModel + IService_Focus
- Example: `View_Dunnage_EditModeView` has ViewModel + IService_Focus
- Pattern: Mostly ViewModel + IService_Focus, some with 2-3 services

**Module_Shared** (3 views):
- View_Shared_HelpDialog
- View_Shared_SharedTerminalLoginDialog (has IService_Focus)
- View_Shared_IconSelectorWindow (has IService_LoggingUtility)

### Phase 3: High Complexity (~5 files)

Views with 4+ dependencies or complex patterns:

- **View_Receiving_Workflow** - 4+ services (ViewModel, IService_ReceivingWorkflow, IService_Help, IService_UserSessionManager)
- **View_Dunnage_AdminTypesView** - 3 services (ViewModel, IService_Focus, IService_LoggingUtility)
- **View_Dunnage_WorkflowView** - Complex initialization with method-level service calls
- **View_Dunnage_Dialog_AddToInventoriedListDialog** - 3+ dependencies including DAOs

---

## Next Step: Module_Routing (6 views)

**Recommended action:** Continue with Module_Routing views - all follow the same simple ViewModel-only pattern.

### Step-by-Step Instructions

1. **Read these instruction files first:**
   - `#file:.github/copilot-instructions.md` - MVVM/DI architecture requirements
   - `#file:.github/tasks/service-locator-elimination-progress.md` - Detailed refactoring templates

2. **For each of the 6 Routing views:**

   a. **Add `using System;` to the top of the file**
   
   b. **Convert constructor:**
   ```csharp
   // Find this pattern:
   public RoutingModeSelectionView()
   {
       ViewModel = App.GetService<RoutingModeSelectionViewModel>();
       InitializeComponent();
       DataContext = ViewModel;
   }
   
   // Replace with:
   public RoutingModeSelectionView(RoutingModeSelectionViewModel viewModel)
   {
       ArgumentNullException.ThrowIfNull(viewModel);
       ViewModel = viewModel;
       InitializeComponent();
       DataContext = ViewModel;
   }
   ```

3. **Register views in DI container:**

   In `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`, find the `AddRoutingModule()` method and add view registrations:

   ```csharp
   private static IServiceCollection AddRoutingModule(...)
   {
       // ... existing ViewModel and Service registrations ...
       
       // Routing Views (add at end of method)
       services.AddTransient<Module_Routing.Views.RoutingModeSelectionView>();
       services.AddTransient<Module_Routing.Views.RoutingWizardStep1View>();
       services.AddTransient<Module_Routing.Views.RoutingWizardStep2View>();
       services.AddTransient<Module_Routing.Views.RoutingWizardStep3View>();
       services.AddTransient<Module_Routing.Views.RoutingManualEntryView>();
       services.AddTransient<Module_Routing.Views.RoutingEditModeView>();
       
       return services;
   }
   ```

4. **Verify build:**
   ```bash
   dotnet build
   ```
   Should succeed with 0 errors, 0 warnings.

5. **Verify remaining calls:**
   ```bash
   grep -r "App\.GetService" Module_Routing/Views/*.xaml.cs
   ```
   Should return 0 matches after completion.

---

## Phase 2 Pattern (For Future Reference)

When moving to Phase 2 (moderate complexity), the pattern extends to multiple parameters:

```csharp
// Example: View with ViewModel + IService_Focus
public View_Receiving_POEntry(
    ViewModel_Receiving_POEntry viewModel,
    IService_Focus focusService)
{
    ArgumentNullException.ThrowIfNull(viewModel);
    ArgumentNullException.ThrowIfNull(focusService);
    
    ViewModel = viewModel;
    _focusService = focusService;
    
    InitializeComponent();
    DataContext = ViewModel;
    
    _focusService.AttachFocusOnVisibility(this);
}
```

**DI Registration:** Services are already registered, so just register the View:
```csharp
services.AddTransient<Module_Receiving.Views.View_Receiving_POEntry>();
```

---

## Key Architecture Rules (From Project Constitution)

**MUST follow these principles:**

1. ✅ **All ViewModels inherit from `ViewModel_Shared_Base`** and are `partial` classes
2. ✅ **All XAML bindings use `x:Bind`** (compile-time), never runtime `Binding`
3. ✅ **ViewModels NEVER call DAOs directly** - must go through Service layer
4. ✅ **All async methods end with `Async` suffix**
5. ✅ **Use `ArgumentNullException.ThrowIfNull()` for null checks** (C# 12 pattern)
6. ✅ **Views registered as Transient** in DI container
7. ✅ **No business logic in View code-behind** - only initialization and event wiring

---

## Required Instruction Files to Read

Before continuing, ensure you've read:

1. **`#file:.github/copilot-instructions.md`**
   - Core MVVM architecture requirements
   - DI registration patterns
   - View/ViewModel separation rules

2. **`#file:.github/tasks/service-locator-elimination-progress.md`**
   - Complete refactoring templates
   - File-by-file tracking
   - Validation procedures

3. **`#file:csharp.instructions.md`**
   - C# coding standards
   - Naming conventions
   - Code quality rules

4. **`#file:taming-copilot.instructions.md`**
   - Surgical code editing approach
   - Minimal necessary changes

---

## Validation Checklist

After completing Module_Routing refactoring:

- [ ] All 6 Routing views have constructor injection
- [ ] All 6 views have `using System;` at top
- [ ] All 6 views use `ArgumentNullException.ThrowIfNull()`
- [ ] All 6 views registered in `AddRoutingModule()` method
- [ ] Build succeeds: `dotnet build` → 0 errors, 0 warnings
- [ ] No remaining Service Locator calls in Routing Views
- [ ] Update progress tracking: Mark Phase 1 Routing complete

---

## Success Metrics

**Current Progress:**
- ✅ 29 views refactored (31%)
- 🔄 66 views remaining (69%)

**After Module_Routing:**
- ✅ 35 views refactored (37%)
- 🔄 60 views remaining (63%)

**Final Goal:**
- ✅ All 95 Service Locator calls eliminated
- ✅ Build: 0 errors, 0 warnings
- ✅ All tests passing
- ✅ Clean MVVM architecture throughout

---

## Quick Command Reference

```bash
# Build project
dotnet build

# Search for remaining Service Locator calls
grep -r "App\.GetService" --include="*.xaml.cs" Module_Routing/

# Count remaining calls
grep -r "App\.GetService" --include="*.xaml.cs" | wc -l

# Run tests
dotnet test
```

---

## Contact/Resources

- **Progress Tracking:** `.github/tasks/service-locator-elimination-progress.md`
- **Task Status:** `.github/tasks/service-locator-elimination-tasks.md`
- **Build Status:** Last verified 2025-01-21 - ✅ SUCCESS

---

**Ready to continue? Start with Module_Routing (6 views) using the pattern above!**

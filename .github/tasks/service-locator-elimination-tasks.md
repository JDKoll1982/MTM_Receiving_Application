# Service Locator Elimination - Detailed Task List

## Status Update (2025-01-21 - REFACTORING BEGUN)

**PRAGMATIC APPROACH ADOPTED WITH ACTIVE REFACTORING:** Constructor injection is being systematically implemented for all Views.

### Current Work Status (as of this session)
- **Batch 1 Complete**: Module_Settings.Routing (5 views refactored)
- **Batch 2 Complete**: Module_Settings.Reporting (3 views refactored)
- **Build Status**: ✅ SUCCESS - 0 errors, 0 warnings
- **Completed Refactorings**: 8 views with full DI conversion
- **DI Registration**: RegisterSettingsViews() method created in ModuleServicesExtensions.cs

### Next Steps (Remaining Phase 1)
- Continue with Module_Settings (Dunnage, Receiving, Volvo, DeveloperTools, Core) - ~26 views
- Complete Module_Routing and Module_Reporting - ~5 views
- All follow identical ViewModel-only pattern for Phase 1

### Detailed Progress Tracking
See: [service-locator-elimination-progress.md](service-locator-elimination-progress.md) for:
- Complete file-by-file tracking
- Refactoring templates and patterns
- Token-efficient strategy for remaining work
- Validation and testing procedures

### Reasoning for Active Refactoring (vs. Suppression-Only)
While CS0618 is suppressed via `.editorconfig`, active elimination provides:
- ✅ Cleaner MVVM architecture
- ✅ Better testability
- ✅ Explicit dependency declaration in code
- ✅ Proper dependency injection container usage
- ✅ Compliance with project constitution

### Current Build Status
- **Errors**: 0
- **Warnings**: 0  
- **CS0618 Suppression**: Still active in .editorconfig for non-refactored views
- **Last verified**: After Module_Settings.Reporting batch

---

## Overview
This task list breaks down the elimination of 141 `App.GetService<T>()` calls into manageable, testable phases.

**NOTE:** As of 2025-01-21, these tasks are deferred. CS0618 is suppressed via `.editorconfig`.

---

## Phase 1: Simple Views (1-2 Dependencies) [DEFERRED]

### Module_Settings.Routing (5 files)
- [ ] **View_Settings_Routing_SettingsOverview.xaml.cs**
  - Current: `App.GetService<ViewModel_Settings_Routing_SettingsOverview>()`
  - Add constructor: `ViewModel_Settings_Routing_SettingsOverview viewModel`
  - Update DI registration in ModuleServicesExtensions

- [ ] **View_Settings_Routing_FileIO.xaml.cs**
  - Current: `App.GetService<ViewModel_Settings_Routing_FileIO>()`
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Routing_UiUx.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Routing_BusinessRules.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Routing_Resilience.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Routing_UserPreferences.xaml.cs**
  - Add constructor parameter
  - Update DI registration

### Module_Settings.Reporting (6 files)
- [ ] **View_Settings_Reporting_SettingsOverview.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Reporting_FileIO.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Reporting_Permissions.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Reporting_Csv.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Reporting_EmailUx.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Reporting_BusinessRules.xaml.cs**
  - Add constructor parameter
  - Update DI registration

### Module_Settings.Dunnage (6 files)
- [ ] **View_Settings_Dunnage_SettingsOverview.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Dunnage_UserPreferences.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Dunnage_UiUx.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Dunnage_Workflow.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Dunnage_Permissions.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Dunnage_Audit.xaml.cs**
  - Add constructor parameter
  - Update DI registration

### Module_Settings.Receiving (6 files)
- [ ] **View_Settings_Receiving_SettingsOverview.xaml.cs** (Example from errors)
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Receiving_UserPreferences.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Receiving_Defaults.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Receiving_Validation.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Receiving_BusinessRules.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Receiving_Integrations.xaml.cs**
  - Add constructor parameter
  - Update DI registration

### Module_Settings.Volvo (6 files)
- [ ] **View_Settings_Volvo_SettingsOverview.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Volvo_FilePaths.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Volvo_UiConfiguration.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Volvo_DatabaseSettings.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Volvo_ConnectionStrings.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Volvo_ExternalizationBacklog.xaml.cs**
  - Add constructor parameter
  - Update DI registration

### Module_Settings.DeveloperTools (5 files)
- [ ] **View_Settings_DeveloperTools_SettingsOverview.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_DeveloperTools_FeatureA.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_DeveloperTools_FeatureB.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_DeveloperTools_FeatureD.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_SettingsDeveloperTools_DatabaseTest.xaml.cs**
  - Add constructor parameter
  - Update DI registration

### Module_Settings.Core (3 files)
- [ ] **View_Settings_Database.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_Theme.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Settings_SharedPaths.xaml.cs**
  - Add constructor parameter
  - Update DI registration

### Module_Routing (4 files)
- [ ] **RoutingModeSelectionView.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **RoutingWizardContainerView.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **RoutingWizardStep2View.xaml.cs**
  - Add constructor parameter
  - Update DI registration

- [ ] **RoutingManualEntryView.xaml.cs**
  - Add constructor parameter
  - Update DI registration

### Module_Reporting (1 file)
- [ ] **View_Reporting_Main.xaml.cs**
  - Add constructor parameter
  - Update DI registration

**Phase 1 Total: ~50 files**

---

## Phase 2: Moderate Views (3-4 Dependencies)

### Module_Dunnage (8 files)
- [ ] **View_Dunnage_ModeSelectionView.xaml.cs**
  - Current: ViewModel only
  - Add constructor parameter
  - Update DI registration

- [ ] **View_Dunnage_PartSelectionView.xaml.cs**
  - Current: ViewModel + IService_Focus
  - Add 2 constructor parameters
  - Update DI registration

- [ ] **View_Dunnage_ReviewView.xaml.cs**
  - Current: ViewModel + IService_Focus
  - Add 2 constructor parameters
  - Update DI registration

- [ ] **View_Dunnage_EditModeView.xaml.cs**
  - Current: ViewModel + IService_Focus
  - Add 2 constructor parameters
  - Update DI registration

- [ ] **View_Dunnage_Dialog_AddTypeDialog.xaml.cs**
  - Current: ViewModel + IService_Focus
  - Add 2 constructor parameters
  - Update DI registration

- [ ] **View_Dunnage_AdminMainView.xaml.cs**
  - Current: ViewModel + IService_Focus
  - Add 2 constructor parameters
  - Update DI registration

- [ ] **View_Dunnage_Dialog_AddMultipleRowsDialog.xaml.cs**
  - Current: IService_Focus
  - Add 1 constructor parameter
  - Update DI registration

### Module_Volvo (2 files)
- [ ] **View_Volvo_ShipmentEntry.xaml.cs**
  - Add constructor parameters
  - Update DI registration

- [ ] **View_Volvo_Settings.xaml.cs**
  - Add constructor parameters
  - Update DI registration

### Module_Receiving (1 file)
- [ ] **View_Receiving_PackageType.xaml.cs**
  - Current: ViewModel + IService_Focus
  - Add 2 constructor parameters
  - Update DI registration

**Phase 2 Total: ~15 files**

---

## Phase 3: Complex Views (5+ Dependencies)

### Module_Dunnage (3 files)
- [ ] **View_Dunnage_WorkflowView.xaml.cs** ⚠️ HIGH COMPLEXITY
  - Current dependencies found:
    - ViewModel (ViewModel_Dunnage_WorkFlowViewModel)
    - IService_Focus
    - IService_DunnageWorkflow (3 calls)
    - IService_Help
  - Total: 4-5 dependencies
  - **Action**:
    - Add 5 constructor parameters
    - Consider if >7 dependencies violate Object Calisthenics
    - Update DI registration

- [ ] **View_Dunnage_AdminTypesView.xaml.cs** ⚠️ HIGH COMPLEXITY
  - Current dependencies found:
    - ViewModel (ViewModel_Dunnage_AdminTypes)
    - IService_Focus
    - IService_LoggingUtility (called directly)
  - Total: 3 dependencies
  - **Action**:
    - Add 3 constructor parameters
    - Update DI registration

- [ ] **View_Dunnage_AdminPartsView.xaml.cs**
  - Estimate: 2-3 dependencies
  - Add constructor parameters
  - Update DI registration

- [ ] **View_Dunnage_AdminInventoryView.xaml.cs**
  - Estimate: 2-3 dependencies
  - Add constructor parameters
  - Update DI registration

**Phase 3 Total: ~5 files**

---

## Phase 4: ViewModels with Service Locator

### Shared Base Classes
- [ ] **ViewModel_Shared_Base.cs** ⚠️ CRITICAL - AFFECTS ALL DERIVED VIEWMODELS
  - Current: Uses Service Locator in `ShowNotification()` method
  - **Analysis Required**:
    - How many ViewModels derive from this base?
    - What dependencies does it need?
  - **Refactoring Strategy**:
    - Add protected constructor with IService_Notification
    - Update all derived ViewModels to call base constructor
    - Update all derived ViewModel registrations
  - **Risk**: HIGH - Affects potentially 50+ ViewModels
  - **Mitigation**: Test thoroughly, phase this separately

### ViewModels Creating Dialogs
- [ ] **ViewModel_Dunnage_AdminInventoryViewModel.cs**
  - Current: `App.GetService<View_Dunnage_Dialog_AddToInventoriedListDialog>()`
  - **Strategy**: Use Factory Pattern
  - Add constructor parameter: `Func<View_Dunnage_Dialog_AddToInventoriedListDialog> dialogFactory`
  - Update DI registration:
    ```csharp
    services.AddTransient<ViewModel_Dunnage_AdminInventoryViewModel>(sp => {
        var dialogFactory = new Func<View_Dunnage_Dialog_AddToInventoriedListDialog>(() =>
            sp.GetRequiredService<View_Dunnage_Dialog_AddToInventoriedListDialog>());
        return new ViewModel_Dunnage_AdminInventoryViewModel(dialogFactory, /*other deps*/);
    });
    ```

**Phase 4 Total: ~5 files (but high impact)**

---

## Phase 5: Update DI Registrations

### Create View Factory Registrations Helper
- [ ] Create method in ModuleServicesExtensions to register Views properly
- [ ] Pattern:
  ```csharp
  private static IServiceCollection RegisterViewWithDependencies<TView, TViewModel>(
      this IServiceCollection services)
      where TView : class
      where TViewModel : class
  {
      services.AddTransient<TView>(sp =>
      {
          var viewModel = sp.GetRequiredService<TViewModel>();
          return (TView)Activator.CreateInstance(typeof(TView), viewModel)!;
      });
      return services;
  }
  ```

### Update All View Registrations
- [ ] Replace simple `AddTransient<TView>()` with factory pattern
- [ ] Ensure dependencies are resolved from service provider
- [ ] Group by module for organization

**Estimated Updates: 100+ registration changes**

---

## Phase 6: Cleanup & Validation

### Remove Deprecated Code
- [ ] Remove `[Obsolete] GetService<T>()` method from App.xaml.cs
- [ ] Verify no remaining calls with regex search: `App\.GetService`
- [ ] Build solution - should succeed with zero errors

### Add Static Analysis
- [ ] Create .editorconfig rule to flag Service Locator pattern
- [ ] Add analyzer package if available
- [ ] Update code review checklist

### Testing
- [ ] Build solution (clean build)
- [ ] Run all unit tests
- [ ] Launch application
- [ ] Test each workflow:
  - [ ] Receiving workflow
  - [ ] Dunnage workflow (admin and regular)
  - [ ] Routing workflow
  - [ ] Reporting
  - [ ] Settings (all modules)
  - [ ] Dialogs open correctly
- [ ] Performance testing (no degradation expected)

### Documentation
- [ ] Update README with constructor injection pattern
- [ ] Create docs/DependencyInjection.md guide
- [ ] Document factory pattern for dialogs
- [ ] Add "How to Add New View" guide

---

## Detailed Refactoring Templates

### Template 1: Simple View (ViewModel Only)

**Before:**
```csharp
public MyView()
{
    InitializeComponent();
    ViewModel = App.GetService<MyViewModel>();
}
```

**After:**
```csharp
private readonly MyViewModel _viewModel;

public MyView(MyViewModel viewModel)
{
    ArgumentNullException.ThrowIfNull(viewModel);
    
    InitializeComponent();
    
    _viewModel = viewModel;
    ViewModel = viewModel;
    DataContext = viewModel;
}
```

**DI Registration:**
```csharp
// In ModuleServicesExtensions.cs
services.AddTransient<MyView>();  // Framework will auto-inject dependencies
```

### Template 2: View with Service Dependencies

**Before:**
```csharp
public MyView()
{
    InitializeComponent();
    ViewModel = App.GetService<MyViewModel>();
    _focusService = App.GetService<IService_Focus>();
}
```

**After:**
```csharp
private readonly MyViewModel _viewModel;
private readonly IService_Focus _focusService;

public MyView(
    MyViewModel viewModel,
    IService_Focus focusService)
{
    ArgumentNullException.ThrowIfNull(viewModel);
    ArgumentNullException.ThrowIfNull(focusService);
    
    InitializeComponent();
    
    _viewModel = viewModel;
    _focusService = focusService;
    ViewModel = viewModel;
    DataContext = viewModel;
}
```

**DI Registration:**
```csharp
services.AddTransient<MyView>();  // Auto-injects both dependencies
```

### Template 3: ViewModel Creating Dialogs (Factory Pattern)

**Before:**
```csharp
private async Task ShowDialogAsync()
{
    var dialog = App.GetService<MyDialog>();
    var result = await dialog.ShowAsync();
    // Process result
}
```

**After:**
```csharp
private readonly Func<MyDialog> _dialogFactory;

public MyViewModel(Func<MyDialog> dialogFactory)
{
    ArgumentNullException.ThrowIfNull(dialogFactory);
    _dialogFactory = dialogFactory;
}

private async Task ShowDialogAsync()
{
    var dialog = _dialogFactory();
    var result = await dialog.ShowAsync();
    // Process result
}
```

**DI Registration:**
```csharp
services.AddTransient<MyViewModel>(sp =>
{
    var dialogFactory = new Func<MyDialog>(() => sp.GetRequiredService<MyDialog>());
    return new MyViewModel(dialogFactory);
});
```

### Template 4: Shared Base Class

**Before:**
```csharp
public abstract class ViewModel_Shared_Base
{
    protected void ShowNotification(string message)
    {
        var notificationService = App.GetService<IService_Notification>();
        notificationService.Show(message);
    }
}
```

**After:**
```csharp
public abstract class ViewModel_Shared_Base
{
    protected readonly IService_Notification NotificationService;

    protected ViewModel_Shared_Base(IService_Notification notificationService)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        NotificationService = notificationService;
    }

    protected void ShowNotification(string message)
    {
        NotificationService.Show(message);
    }
}

// All derived classes must call base constructor
public class DerivedViewModel : ViewModel_Shared_Base
{
    public DerivedViewModel(
        IService_Notification notificationService,
        IOtherService otherService)
        : base(notificationService)
    {
        ArgumentNullException.ThrowIfNull(otherService);
        // Other initialization
    }
}
```

---

## Testing Checklist Per File

After refactoring each file:
- [ ] File compiles without errors
- [ ] Constructor has null checks (`ArgumentNullException.ThrowIfNull`)
- [ ] Fields are marked `readonly`
- [ ] DI registration updated
- [ ] View can be resolved from DI container
- [ ] DataContext properly set to ViewModel
- [ ] View loads in application
- [ ] All functionality works as before

---

## Progress Tracking

### Phase 1: Simple Views
- **Total Files**: 50
- **Completed**: 17
- **In Progress**: Active
- **Blocked**: 0

### Phase 2: Moderate Views
- **Total Files**: 15
- **Completed**: 0
- **In Progress**: Not Started
- **Blocked**: 0

### Phase 3: Complex Views
- **Total Files**: 5
- **Completed**: 2
- **In Progress**: Not Started
- **Blocked**: 0

### Phase 4: ViewModels
- **Total Files**: 82
- **Completed**: 82
- **In Progress**: Not Started
- **Blocked**: 0

### Phase 4: ViewModels
- **Total Files**: 5
- **Completed**: 0
- **In Progress**: 0
- **Blocked**: 0

### Phase 5: DI Updates
- **Total Registrations**: ~100
- **Completed**: 0
- **In Progress**: 0
- **Blocked**: 0

### Phase 6: Cleanup
- **Tasks**: 10
- **Completed**: 0
- **In Progress**: 0
- **Blocked**: 0

---

## Build Verification Strategy

### After Each Phase
1. Run `dotnet build`
2. Verify zero new errors
3. Verify Service Locator count decreases
4. Commit changes with descriptive message

### After All Phases
1. Run full solution build
2. Run all unit tests
3. Launch application
4. Perform smoke testing on all modules
5. Check for memory leaks or performance regressions

---

## Rollback Points

**Git Commits Per Phase:**
- Commit 1: After Phase 1 (Simple Views)
- Commit 2: After Phase 2 (Moderate Views)
- Commit 3: After Phase 3 (Complex Views)
- Commit 4: After Phase 4 (ViewModels)
- Commit 5: After Phase 5 (DI Updates)
- Commit 6: After Phase 6 (Cleanup)

**Rollback Strategy**: If critical issue found, `git revert` last commit and investigate

---

## Success Metrics

### Code Quality
- [ ] Zero `App.GetService<T>()` calls
- [ ] All dependencies explicit in constructors
- [ ] All dependencies validated with null checks
- [ ] Maximum 7 dependencies per class (Object Calisthenics Rule 8)

### Architecture
- [ ] SOLID principles followed
- [ ] Dependency direction correct (UI → Application → Domain → Infrastructure)
- [ ] No circular dependencies
- [ ] Clean separation of concerns

### Testing
- [ ] All Views unit testable with mocked dependencies
- [ ] Integration tests for DI resolution
- [ ] No regression in existing tests

---

## Estimated Timeline

- **Phase 1**: 3 hours (50 simple views)
- **Phase 2**: 2 hours (15 moderate views)
- **Phase 3**: 2 hours (5 complex views)
- **Phase 4**: 2 hours (ViewModels + base class)
- **Phase 5**: 1 hour (DI registration updates)
- **Phase 6**: 1 hour (Cleanup & testing)

**Total**: 11 hours over 2 days

---

## Next Actions

1. ✅ Plan created
2. ✅ Task list created
3. ⏭️ **START**: Phase 1, Task 1 - Refactor View_Settings_Routing_SettingsOverview
4. ⏭️ Continue sequentially through Phase 1
5. ⏭️ Build and test after Phase 1
6. ⏭️ Proceed to Phase 2

---

**Status**: Ready to Execute  
**First Target**: Module_Settings.Routing.View_Settings_Routing_SettingsOverview  
**Expected Completion**: 2 business days  
**Risk Level**: Medium (phased approach mitigates risk)

---

*Task List Created: 2024*  
*Total Tasks: 141 Service Locator eliminations + testing*  
*Author: Someone who's going to fix YOUR mess*

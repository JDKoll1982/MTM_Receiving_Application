# Service Locator Elimination - Progress Tracking

**Status as of January 21, 2026**

## Completed Work (Option A Baseline + Option B Start)

### ✅ Phase 1 Batch 1: Module_Settings.Routing (8 files completed)
- [x] View_Settings_Routing_FileIO - Constructor injection implemented
- [x] View_Settings_Routing_UiUx - Constructor injection implemented
- [x] View_Settings_Routing_BusinessRules - Constructor injection implemented
- [x] View_Settings_Routing_Resilience - Constructor injection implemented
- [x] View_Settings_Routing_UserPreferences - Constructor injection implemented
- [x] View_Settings_Routing_SettingsOverview - Already refactored (verified)
- [x] View_Settings_Routing_FileIO - Already refactored (verified)
- [x] DI Registration - Added to RegisterSettingsViews() in ModuleServicesExtensions.cs

### ✅ Phase 1 Batch 2: Module_Settings.Reporting (6 files completed)
- [x] View_Settings_Reporting_Csv - Constructor injection implemented
- [x] View_Settings_Reporting_BusinessRules - Constructor injection implemented
- [x] View_Settings_Reporting_EmailUx - Constructor injection implemented
- [x] View_Settings_Reporting_SettingsOverview - Already refactored (verified)
- [x] View_Settings_Reporting_FileIO - Already refactored (verified)  
- [x] View_Settings_Reporting_Permissions - Already refactored (verified)
- [x] DI Registration - Added to RegisterSettingsViews()

### Build Status
- **Current**: ✅ SUCCESS - 0 errors, 0 warnings
- **Last verified**: After completing Reporting module refactoring

## Remaining Work

### Phase 1 (Simple Views - ViewModel Only)

#### Module_Settings.Dunnage (6 files)
All follow ViewModel-only pattern. Check current status:
```bash
grep -n "App.GetService" Module_Settings.Dunnage/Views/View_Settings_*.xaml.cs
```
Expected files:
- View_Settings_Dunnage_SettingsOverview
- View_Settings_Dunnage_UserPreferences
- View_Settings_Dunnage_UiUx
- View_Settings_Dunnage_Workflow
- View_Settings_Dunnage_Permissions
- View_Settings_Dunnage_Audit

**Refactoring pattern**: Same as Routing/Reporting - add constructor parameter, add `using System;`, add null check

#### Module_Settings.Receiving (6 files)
Expected files:
- View_Settings_Receiving_SettingsOverview
- View_Settings_Receiving_UserPreferences
- View_Settings_Receiving_Defaults
- View_Settings_Receiving_Validation
- View_Settings_Receiving_BusinessRules
- View_Settings_Receiving_Integrations

#### Module_Settings.Volvo (6 files)
Expected files:
- View_Settings_Volvo_SettingsOverview
- View_Settings_Volvo_FilePaths
- View_Settings_Volvo_UiConfiguration
- View_Settings_Volvo_DatabaseSettings
- View_Settings_Volvo_ConnectionStrings
- View_Settings_Volvo_ExternalizationBacklog

#### Module_Settings.DeveloperTools (5 files)
Expected files:
- View_Settings_DeveloperTools_SettingsOverview
- View_Settings_DeveloperTools_FeatureA
- View_Settings_DeveloperTools_FeatureB
- View_Settings_DeveloperTools_FeatureC
- View_SettingsDeveloperTools_DatabaseTest

#### Module_Settings.Core (3 files)
Expected files:
- View_Settings_Database
- View_Settings_Theme
- View_Settings_SharedPaths
- View_Settings_Users
- View_Settings_Logging
- View_Settings_System

#### Module_Routing (4 files)
Note: NOT Module_Settings.Routing, but Module_Routing. Uses different ViewModel naming.
Expected files:
- RoutingModeSelectionView
- RoutingWizardContainerView / RoutingWizardStep1View
- RoutingWizardStep2View
- RoutingWizardStep3View
- RoutingManualEntryView
- RoutingEditModeView

#### Module_Reporting (1 file)
- View_Reporting_Main

### Phase 1 Summary
- **Total simple views remaining**: ~30 files
- **All follow identical pattern**: Constructor parameter + ArgumentNullException check
- **DI Registration**: Add to RegisterSettingsViews() or appropriate module method

### Phase 2 (Moderate Views - 3-4 Dependencies)
Requires multiple service dependencies in constructor:

#### Module_Dunnage (8 files) - Examples:
- View_Dunnage_ModeSelectionView (ViewModel only - check if really needs Phase 2)
- View_Dunnage_PartSelectionView (ViewModel + IService_Focus)
- View_Dunnage_ReviewView
- View_Dunnage_EditModeView
- View_Dunnage_Dialog_AddTypeDialog
- View_Dunnage_AdminMainView
- View_Dunnage_Dialog_AddMultipleRowsDialog
- View_Dunnage_AdminTypesView
- View_Dunnage_AdminPartsView
- View_Dunnage_AdminInventoryView

#### Module_Volvo (2 files)
- View_Volvo_ShipmentEntry (multiple dependencies)
- View_Volvo_Settings (multiple dependencies)

#### Module_Receiving (1+ files)
- View_Receiving_PackageType (ViewModel + IService_Focus)

### Phase 3 (Complex Views - 5+ Dependencies)
Requires investigation and potential ViewFactory pattern:

#### High-complexity views:
- View_Receiving_Workflow (multiple services)
- View_Dunnage_WorkflowView (multiple services)
- View_Dunnage_AdminTypesView (ViewModel + 2 services)

---

## Refactoring Template - Phase 1

### Step 1: Identify File
```csharp
// BEFORE
public View_MyView()
{
    ViewModel = App.GetService<ViewModel_MyView>();
    InitializeComponent();
    DataContext = ViewModel;
}

// AFTER
using System;  // <- ADD THIS

public View_MyView(ViewModel_MyView viewModel)
{
    ArgumentNullException.ThrowIfNull(viewModel);  // <- ADD THIS
    ViewModel = viewModel;
    InitializeComponent();
    DataContext = ViewModel;
}
```

### Step 2: Register in DI
Add to appropriate RegisterSettingsViews() or module registration method:
```csharp
services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_SettingsOverview>();
```

### Step 3: Verify Build
```bash
dotnet build
```

---

## Refactoring Template - Phase 2 (Multiple Services)

### Example: View with ViewModel + IService_Focus
```csharp
using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_PartSelectionView : UserControl
{
    public ViewModel_Dunnage_PartSelection ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_PartSelectionView(
        ViewModel_Dunnage_PartSelection viewModel,
        IService_Focus focusService)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(focusService);
        
        ViewModel = viewModel;
        _focusService = focusService;
        this.InitializeComponent();
        
        _focusService.AttachFocusOnVisibility(this);
    }
}
```

### Register Multiple Dependencies:
```csharp
services.AddTransient<Module_Dunnage.Views.View_Dunnage_PartSelectionView>();
```
(IService_Focus is already registered in core services)

---

## Key Files to Modify

1. **ModuleServicesExtensions.cs** - Central DI registration file
   - RegisterSettingsViews() - For all Settings.* modules
   - AddDunnageModule() - For Module_Dunnage views
   - AddReceivingModule() - For Module_Receiving views
   - AddRoutingModule() - For Module_Routing views
   - AddReportingModule() - For Module_Reporting views

2. **Individual .xaml.cs files** - Add constructor parameters
   - See list above organized by module

---

## Validation Strategy

### After each batch:
```bash
# Build
dotnet build

# Check for remaining App.GetService calls
grep -r "App\.GetService" *.xaml.cs

# Run tests
dotnet test
```

### Final verification:
```bash
# Should return 0 matches in Views (suppressed in .editorconfig for others)
grep -r "App\.GetService.*xaml.cs" --include="*.xaml.cs"
```

---

## Recommended Completion Order

1. **Phase 1 Batch 3**: Module_Settings remaining (Dunnage, Receiving, Volvo, DeveloperTools) - ~26 views
2. **Phase 1 Batch 4**: Module_Settings.Core (3 views)
3. **Phase 1 Batch 5**: Module_Routing (4-6 views)
4. **Phase 1 Batch 6**: Module_Reporting view (1 view)
5. **Phase 1 Verification**: Build check, remaining App.GetService scan
6. **Phase 2**: Module_Dunnage complex views
7. **Phase 3**: Highest-complexity views or ViewFactory refactoring

---

## Token-Efficient References

For future work, reference these instruction files:
- #file:copilot-instructions.md - MVVM/DI patterns
- #file:serena-tools.instructions.md - Efficient multi-file refactoring  
- #file:taming-copilot.instructions.md - Surgical code editing
- #file:csharp.instructions.md - C# standards

---

## Known Issues / Edge Cases

1. **Null checking**: Use `ArgumentNullException.ThrowIfNull()` per C# 12/project standards
2. **System namespace**: Ensure `using System;` is added to all refactored files
3. **View naming patterns**: Some modules use different naming (e.g., RoutingXxxView vs View_Routing_Xxx)
4. **DialogViewModels**: Views used as dialogs may need special handling in Phase 2/3
5. **DependencyProperty pattern**: Some views use DependencyProperty for ViewModel - verify pattern consistency

---

## Success Criteria

- ✅ Build succeeds with 0 errors, 0 warnings
- ✅ No `App.GetService<T>()` calls in Views (outside suppressed .xaml.cs)
- ✅ All Views use constructor injection for dependencies
- ✅ All Views registered in ModuleServicesExtensions
- ✅ All unit tests pass
- ✅ Application launches and navigates without errors

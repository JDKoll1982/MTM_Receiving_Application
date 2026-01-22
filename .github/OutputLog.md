# Build Issue Analysis & Action Plan

**Timestamp:** 5:59 AM  
**Configuration:** Debug x64  
**Target:** .NET 8 (WinUI 3)

## 🔴 Critical Architectural Violations (CS0618)

**Issue:** Usage of obsolete `App.GetService<T>()` Service Locator pattern.
**Action:** Refactor to use Constructor Injection. The dependency injection container should provide these services.

### 🧩 ViewModels (Refactoring Priority: High)
These classes are logic carriers and should strictly follow DI.
- `Module_Volvo\ViewModels\ViewModel_Volvo_Settings.cs`
- `Module_Receiving\ViewModels\ViewModel_Receiving_ModeSelection.cs`
- `Module_Dunnage\ViewModels\ViewModel_Dunnage_ModeSelectionViewModel.cs`
- `Module_Dunnage\ViewModels\ViewModel_Dunnage_ReviewViewModel.cs`

### ⚙️ Services (Refactoring Priority: High)
Services should inject dependencies via constructor.
- `Module_Receiving\Services\Service_ReceivingWorkflow.cs`
- `Module_Core\Services\Help\Service_Help.cs`

### 🖼️ Views/Code-Behind (Refactoring Priority: Medium)
Views might be forcing service location due to poor XAML instantiation. Ensure Views are resolved via DI or ViewModels are properly bound.
**Module: Receiving**
- `Module_Receiving\Views\View_Receiving_Workflow.xaml.cs`
- `Module_Receiving\Views\View_Receiving_WeightQuantity.xaml.cs`
- `Module_Receiving\Views\View_Receiving_Review.xaml.cs`
- `Module_Receiving\Views\View_Receiving_POEntry.xaml.cs`
- `Module_Receiving\Views\View_Receiving_PackageType.xaml.cs`
- `Module_Receiving\Views\View_Receiving_ModeSelection.xaml.cs`
- `Module_Receiving\Views\View_Receiving_ManualEntry.xaml.cs`
- `Module_Receiving\Views\View_Receiving_LoadEntry.xaml.cs`
- `Module_Receiving\Views\View_Receiving_HeatLot.xaml.cs`
- `Module_Receiving\Views\View_Receiving_EditMode.xaml.cs`

**Module: Routing**
- `Module_Routing\Views\RoutingWizardStep1View.xaml.cs`
- `Module_Routing\Views\RoutingWizardStep2View.xaml.cs`
- `Module_Routing\Views\RoutingWizardStep3View.xaml.cs`
- `Module_Routing\Views\RoutingModeSelectionView.xaml.cs`
- `Module_Routing\Views\RoutingManualEntryView.xaml.cs`
- `Module_Routing\Views\RoutingEditModeView.xaml.cs`

**Module: Dunnage**
- `Module_Dunnage\Views\View_Dunnage_WorkflowView.xaml.cs`
- `Module_Dunnage\Views\View_Dunnage_TypeSelectionView.xaml.cs`
- `Module_Dunnage\Views\View_Dunnage_ReviewView.xaml.cs`
- `Module_Dunnage\Views\View_Dunnage_QuantityEntryView.xaml.cs`
- `Module_Dunnage\Views\View_Dunnage_PartSelectionView.xaml.cs`
- `Module_Dunnage\Views\View_Dunnage_ModeSelectionView.xaml.cs`
- `Module_Dunnage\Views\View_Dunnage_ManualEntryView.xaml.cs`
- `Module_Dunnage\Views\View_Dunnage_EditModeView.xaml.cs`
- `Module_Dunnage\Views\View_Dunnage_DetailsEntryView.xaml.cs`
- `Module_Dunnage\Views\*` (Admin Views & Dialogs)

**Module: Settings, Shared, Reporting, Volvo**
- `Module_Settings.*\Views\*` (Multiple files)
- `Module_Shared\Views\*` (Dialogs & IconSelector)
- `Module_Reporting\Views\View_Reporting_Main.xaml.cs`
- `Module_Volvo\Views\*`

---

## ⚠️ Code Quality Warnings (CS0109)

**Issue:** Unnecessary `new` keyword on member declarations (member does not hide an inherited member).
**Action:** Remove the `new` keyword.

**Affected Files:**
- `Module_Settings.Dunnage\ViewModels\ViewModel_Settings_Dunnage_NavigationHub.cs`
- `Module_Settings.Reporting\ViewModels\ViewModel_Settings_Reporting_NavigationHub.cs`
- `Module_Settings.DeveloperTools\ViewModels\ViewModel_Settings_DeveloperTools_NavigationHub.cs`
- `Module_Settings.Routing\ViewModels\ViewModel_Settings_Routing_NavigationHub.cs`
- `Module_Settings.Volvo\ViewModels\ViewModel_Settings_Volvo_NavigationHub.cs`
- `Module_Settings.Receiving\ViewModels\ViewModel_Settings_Receiving_NavigationHub.cs`

**Members:** `Save()`, `Reset()`, `Cancel()`, `Back()`, `Next()` (and Async variants).

---

## 🧹 Static Analysis Suggestions (Roslynator)

**Issue:** Code improvement suggestions.
**Action:** Apply recommended fixes.

| Code | File | Issue |
|------|------|-------|
| **RCS1080** | `Module_Core\Behaviors\ValidationBehavior.cs` | Use `Count` instead of `Any()` on collection. |
| **RCS1146** | `Module_Volvo\ViewModels\ViewModel_Volvo_History.cs` | Use conditional access (`?.`). |
| **RCS1146** | `Module_Routing\Services\RoutingService.cs` | Use conditional access (`?.`). |
| **RCS1146** | `Module_Settings.Core\Views\View_Settings_CoreWindow.xaml.cs` | Use conditional access (`?.`). |
| **RCS1163** | `Infrastructure\DependencyInjection\ModuleServicesExtensions.cs` | Unused parameter `configuration`. |
| **RCS1163** | `Module_Volvo\ViewModels\ViewModel_Volvo_ShipmentEntry.cs` | Unused parameters `s`, `e` (Event Handlers likely). |
| **RCS1163** | `Module_Routing\ViewModels\RoutingEditModeViewModel.cs` | Unused parameters `oldLabel`, `newLabel`. |

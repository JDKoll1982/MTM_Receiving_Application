---
workflow: VD
module: Module_Settings
validation_date: 2026-01-09
scope:
  - Module_Settings (primary)
  - Cross-module settings boundary scan
  - Safety and MVVM/DI compliance review
status: needs_followup
---

# Validation Report: Module_Settings (VD)

**Validation Date:** 2026-01-09

This VD run focuses on:

- Unimplemented Settings experiences reachable from the Settings hub
- Settings implemented outside `Module_Settings` (boundary violations)
- Unsafe / inconsistent patterns in settings-related code (DI, service locator, hardcoded defaults)

---

## Overall Status

- **Module_Settings hub UX:** ✅ functional navigation shell, ❌ multiple placeholder categories, ❌ one admin deep-link not wired
- **Settings boundary ("settings must live in Module_Settings"):** ❌ violated by multiple modules
- **Safety/quality:** ⚠️ several MVVM/DI deviations in settings-related code paths

---

## ❌ Boundary Violations (Settings Implemented Outside Module_Settings)

### Volvo Settings live in Module_Volvo

- Settings view + viewmodel exist outside Module_Settings:
  - [Module_Volvo/Views/View_Volvo_Settings.xaml.cs](Module_Volvo/Views/View_Volvo_Settings.xaml.cs)
  - [Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs](Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs)
- Settings storage (MySQL via stored procedures) lives outside Module_Settings:
  - [Module_Volvo/Data/Dao_VolvoSettings.cs](Module_Volvo/Data/Dao_VolvoSettings.cs)
  - [Module_Volvo/Models/Model_VolvoSetting.cs](Module_Volvo/Models/Model_VolvoSetting.cs)

**Impact:** Settings hub embeds Volvo settings UI by directly referencing Volvo view in the Settings workflow page.

**Recommendation:** Move Volvo settings UI + persistence into `Module_Settings` (or wrap behind a `Module_Settings` facade service + view that delegates to Volvo but keeps settings surface within Module_Settings).

### User preference model + persistence are split across modules

- `Module_Settings.Service_UserPreferences` returns `Module_Receiving.Models.Model_UserPreference`:
  - [Module_Settings/Services/Service_UserPreferences.cs](Module_Settings/Services/Service_UserPreferences.cs)
  - [Module_Receiving/Models/Model_UserPreference.cs](Module_Receiving/Models/Model_UserPreference.cs)

- Receiving has a preference DAO (package type preference) that overlaps with “settings/user preferences”:
  - [Module_Receiving/Data/Dao_PackageTypePreference.cs](Module_Receiving/Data/Dao_PackageTypePreference.cs)

- Routing persists user preferences inside Module_Routing:
  - [Module_Routing/Services/RoutingUserPreferenceService.cs](Module_Routing/Services/RoutingUserPreferenceService.cs)

**Impact:** “Settings/preferences” are fragmented across modules, making defaults and persistence inconsistent.

**Recommendation:** Centralize preference models + persistence in `Module_Settings` and have other modules depend on it.

---

## ❌ Unimplemented / Incomplete Settings in Module_Settings

### Settings categories are placeholders

- Steps exist but show placeholder UI:
  - Receiving Settings (`ReceivingSettings`)
  - Shipping Settings (`ShippingSettings`)
  - Administrative Settings (`AdministrativeSettings`)

**Where:** Placeholder views are displayed in [Module_Settings/Views/View_Settings_Workflow.xaml](Module_Settings/Views/View_Settings_Workflow.xaml).

**Impact:** Users can navigate to categories that look “implemented” from the hub, but only see generic placeholder content.

**Recommendation:** Either (a) implement category-specific views+VMs in Module_Settings, or (b) explicitly label cards as “Coming soon” and set placeholder category text via `SetCategory(...)`.

### DunnageInventory admin deep-link is not wired

- `Enum_SettingsWorkflowStep.DunnageInventory` is handled as a no-op:
  - [Module_Settings/Views/View_Settings_Workflow.xaml.cs](Module_Settings/Views/View_Settings_Workflow.xaml.cs)

**Impact:** Users can choose Dunnage Inventory from the Settings hub, but AdminFrame will not navigate.

**Recommendation:** Add the missing navigation to the intended inventory admin page.

---

## ⚠️ Safety / Architecture Issues (Settings-Related)

### Service locator usage in non-Module_Settings ViewModels

- Dunnage admin VMs use `App.GetService<IService_SettingsWorkflow>()` to navigate back:
  - [Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminTypesViewModel.cs](Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminTypesViewModel.cs)
  - [Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminInventoryViewModel.cs](Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminInventoryViewModel.cs)

**Why unsafe:** Service locator makes testing and dependency tracking harder; also violates the “settings concerns stay inside Module_Settings” boundary.

**Recommendation:** Inject a navigation abstraction or callback into these VMs (or have Module_Settings host the admin views/VMs directly).

### Direct DAO instantiation inside a ViewModel

- Volvo shipment entry VM directly `new`s a settings DAO and pulls connection string inline:
  - [Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs](Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs)

**Why unsafe:** Bypasses DI, complicates testing, hard-codes persistence path, and makes settings access non-auditable.

**Recommendation:** Inject a settings service/DAO via constructor and register in DI.

### Hardcoded “defaults” and placeholder persistence

- `Service_UserPreferences` returns constructed defaults and update methods are `Task.Delay` placeholders:
  - [Module_Settings/Services/Service_UserPreferences.cs](Module_Settings/Services/Service_UserPreferences.cs)

**Why unsafe:** Default values may not match real persisted state; update methods silently succeed but do not persist.

**Recommendation:** Implement backing storage (MySQL stored procedures) or clearly mark methods as not supported and return failure results.

---

## Suggested Remediation Order

1. Wire `DunnageInventory` navigation in Settings workflow.
2. Decide and enforce the settings boundary:
   - Either migrate Volvo settings + preference persistence into Module_Settings,
   - Or formally declare exceptions and document them.
3. Remove service locator and direct `new Dao_*` usage in settings-related flows.
4. Implement actual persistence for `IService_UserPreferences` or return explicit failure for unimplemented updates.

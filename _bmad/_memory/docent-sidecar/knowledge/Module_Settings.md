---
module_name: Module_Settings
module_path: Module_Settings/
last_analyzed: 2026-01-09
last_validated: 2026-01-09
analyst: Docent v1.0.0
documentation_scope: full-module-analysis
component_counts:
  views_xaml: 4
  viewmodels: 4
  services_implementations: 2
  service_interfaces: 2
  daos: 0
  models: 0
  converters: 0
key_workflows:
  - Settings hub navigation (ModeSelection -> category)
  - Dunnage Settings (Types/Inventory admin deep links)
  - User preferences retrieval (used by other modules for default mode selection)
integration_points:
  upstream_dependencies:
    - Module_Core (navigation, focus service, error handling, converters)
    - Module_Shared (ViewModel_Shared_Base)
    - Material.Icons.WinUI3
  downstream_dependents:
    - MainWindow navigation shell (Settings entry)
    - Module_Receiving (uses IService_UserPreferences for defaults)
    - Module_Dunnage (uses IService_UserPreferences for defaults)
notes:
  status: "Settings hub is functional; several categories are placeholders. DunnageInventory navigation is not wired yet. UserPreferences persistence is stubbed."
---

# Module_Settings - Module Documentation

## Table of Contents

1. [Module Overview](#module-overview)
2. [Mermaid Workflow Diagrams](#mermaid-workflow-diagrams)
3. [User Interaction Lifecycle](#user-interaction-lifecycle)
4. [Code Inventory](#code-inventory)
5. [Database Schema Details](#database-schema-details)
6. [Module Dependencies & Integration](#module-dependencies--integration)
7. [Common Patterns & Code Examples](#common-patterns--code-examples)

---

## Module Overview

### Purpose

Module_Settings provides the application Settings “hub” UI and a small workflow coordinator for navigating between settings categories.

It currently includes:

- A **Mode Selection** hub that routes users to settings categories (Receiving, Dunnage, UPS/FedEx, Volvo, Admin).
- A **Dunnage Settings** sub-menu that deep-links into the Dunnage Admin pages (Types/Inventory).
- A **Placeholder** view for categories not implemented yet.
- A **User Preferences** service used by other modules to retrieve defaults (currently returns constructed defaults; persistence is stubbed).

### Primary Entry Point

- `View_Settings_Workflow` is navigated to when Settings is selected in the app shell.

### Core State Machine

- Settings navigation is represented by `Enum_SettingsWorkflowStep`.
- `Service_SettingsWorkflow` publishes `StepChanged` events.
- `ViewModel_Settings_Workflow` translates step changes into:
  - `CurrentStep` (controls which UserControl is visible)
  - `CurrentStepTitle` (MainWindow header)
  - `IsAdminPageVisible` (controls the AdminFrame visibility)

Additionally, the shell view (`View_Settings_Workflow.xaml.cs`) also subscribes to `StepChanged` to perform frame navigation when the workflow enters admin steps.

### Architecture Compliance (Highlights)

✅ Strong points

- XAML views use `x:Bind` for commands and state.
- ViewModels are `partial` and use CommunityToolkit.Mvvm attributes.
- Workflow is centralized via a small coordinator service (`IService_SettingsWorkflow`).

⚠️ Deviations / gaps to track

- `Service_UserPreferences` update methods are placeholders (`Task.Delay`) and `GetLatestUserPreferenceAsync` returns constructed defaults rather than persisted preferences.
- `View_Settings_Workflow` displays `View_Settings_Placeholder` for multiple categories but does not call `ViewModel_Settings_Placeholder.SetCategory(...)`, so the placeholder content remains the default "Settings" / "under development" message.
- `View_Settings_Workflow.xaml.cs` navigates AdminFrame for `DunnageTypes` but `DunnageInventory` is currently a TODO (no Frame navigation wired yet).

### Validation Findings (VD)

A targeted VD run was performed to identify unimplemented settings, unsafe settings patterns, and cross-module “settings boundary” violations.

- Validation report: [_bmad/_memory/docent-sidecar/validation/VD-Module_Settings-2026-01-09.md](_bmad/_memory/docent-sidecar/validation/VD-Module_Settings-2026-01-09.md)

Key findings summarized:

- Settings implementations exist outside `Module_Settings` (notably Volvo settings + user preference persistence), which violates the desired module boundary.
- Settings-related code paths include DI/service-locator deviations (direct `new Dao_*`, `App.GetService` inside non-settings ViewModels).

---

## Mermaid Workflow Diagrams

### Workflow: Settings Hub Navigation (ModeSelection -> Category)

```mermaid
flowchart TD
  subgraph UI[UI Layer]
    BtnRec[Receiving card button]
    BtnDun[Dunnage card button]
    BtnShip[UPS/FedEx card button]
    BtnVolvo[Volvo card button]
    BtnAdmin[Admin card button]
    ShellXaml[View_Settings_Workflow.xaml]
  end

  subgraph VM[ViewModel Layer]
    VMMode[ViewModel_Settings_ModeSelection]
    CmdRec[SelectReceivingSettingsCommand]
    CmdDun[SelectDunnageSettingsCommand]
    CmdShip[SelectShippingSettingsCommand]
    CmdVolvo[SelectVolvoSettingsCommand]
    CmdAdmin[SelectAdministrativeSettingsCommand]
    VMShell[ViewModel_Settings_Workflow]
    PropStep[CurrentStep: Enum_SettingsWorkflowStep]
    PropTitle[CurrentStepTitle: string]
    PropAdminVisible[IsAdminPageVisible: bool]
  end

  subgraph SVC[Service Layer]
    WF[IService_SettingsWorkflow]
    WFImpl[Service_SettingsWorkflow]
    EvStep[StepChanged event]
  end

  BtnRec -->|x:Bind Command| CmdRec
  BtnDun -->|x:Bind Command| CmdDun
  BtnShip -->|x:Bind Command| CmdShip
  BtnVolvo -->|x:Bind Command| CmdVolvo
  BtnAdmin -->|x:Bind Command| CmdAdmin

  CmdRec -->|GoToStep(ReceivingSettings)| WF
  CmdDun -->|GoToStep(DunnageSettings)| WF
  CmdShip -->|GoToStep(ShippingSettings)| WF
  CmdVolvo -->|GoToStep(VolvoSettings)| WF
  CmdAdmin -->|GoToStep(AdministrativeSettings)| WF

  WFImpl -.->|implements| WF
  WF -->|raises| EvStep
  EvStep -->|subscribed| VMShell

  VMShell -->|updates| PropStep
  VMShell -->|updates| PropTitle
  VMShell -->|updates| PropAdminVisible

  PropStep -.->|Converter_EnumToVisibility| ShellXaml
  PropAdminVisible -.->|Visibility| ShellXaml
```

### Workflow: Dunnage Settings -> AdminFrame (Types / Inventory)

```mermaid
flowchart TD
  subgraph UI[UI Layer]
    BtnTypes[Types card button]
    BtnInv[Inventory card button]
    AdminFrame[AdminFrame]
  end

  subgraph VM[ViewModel Layer]
    VMDun[ViewModel_Settings_DunnageMode]
    CmdTypes[SelectDunnageTypesCommand]
    CmdInv[SelectDunnageInventoryCommand]
    VMShell[ViewModel_Settings_Workflow]
    PropAdminVisible[IsAdminPageVisible: bool]
  end

  subgraph SVC[Service Layer]
    WF[IService_SettingsWorkflow]
    EvStep[StepChanged event]
  end

  subgraph NAV[Navigation]
    Nav[IService_Navigation]
    ShellCodeBehind[View_Settings_Workflow.xaml.cs]
  end

  BtnTypes -->|x:Bind Command| CmdTypes
  BtnInv -->|x:Bind Command| CmdInv

  CmdTypes -->|GoToStep(DunnageTypes)| WF
  CmdInv -->|GoToStep(DunnageInventory)| WF

  WF -->|raises| EvStep
  EvStep --> VMShell
  EvStep --> ShellCodeBehind

  VMShell -->|sets true for DunnageTypes/DunnageInventory| PropAdminVisible
  PropAdminVisible -.->|Visibility| AdminFrame

  ShellCodeBehind -->|NavigateTo(AdminFrame, Dunnage Admin Types)| Nav
  ShellCodeBehind -.->|TODO: no navigation yet for DunnageInventory| Nav
```

---

## User Interaction Lifecycle

### Workflow 1: Navigate to a Settings Category

**Forward Flow (User Action → UI state):**

1. User opens Settings from the app shell.
2. `MainWindow` navigates to `View_Settings_Workflow`.
3. The workflow starts at `ModeSelection` (default).
4. User selects a settings card (Receiving/Dunnage/Shipping/Volvo/Admin).
5. Mode selection ViewModel calls `IService_SettingsWorkflow.GoToStep(step)`.
6. Shell ViewModel updates `CurrentStep` and `CurrentStepTitle`.
7. Shell XAML shows the corresponding category view.

**Return Flow (UI updates):**

- `CurrentStepTitle` updates are observed by `MainWindow` to keep the header in sync.

**Observed limitations:**

- The category placeholder view uses `ViewModel_Settings_Placeholder` defaults unless another component calls `SetCategory(...)`. No category-specific placeholder content is assigned in the workflow shell today.

### Workflow 2: Dunnage Settings -> Admin Pages

**Forward Flow:**

1. User selects “Dunnage” from Mode Selection.
2. User selects “Types” or “Inventory”.
3. The Settings Workflow page routes AdminFrame navigation to the Dunnage Admin page.

**Observed limitation:**

- Only `DunnageTypes` is currently navigated via `IService_Navigation.NavigateTo(AdminFrame, typeof(Module_Dunnage.Views.View_Dunnage_AdminTypesView))`. `DunnageInventory` is not yet wired in `View_Settings_Workflow.xaml.cs`.

---

## Code Inventory

Detailed component inventory is maintained in the companion document:

- `_bmad/_memory/docent-sidecar/knowledge/Module_Settings-CodeInventory.md`

### Enums

- `Enum_SettingsWorkflowStep`
  - `ModeSelection`, `ReceivingSettings`, `DunnageSettings`, `ShippingSettings`, `VolvoSettings`, `AdministrativeSettings`, `DunnageTypes`, `DunnageInventory`

### Services

- `IService_SettingsWorkflow` / `Service_SettingsWorkflow`
  - `GoToStep(Enum_SettingsWorkflowStep step)`
  - `GoBack()` (currently returns to ModeSelection)
  - `Reset()`
  - Event: `StepChanged`

- `IService_UserPreferences` / `Service_UserPreferences`
  - `GetLatestUserPreferenceAsync(string username)`
  - `UpdateDefaultModeAsync(string username, string defaultMode)`
  - `UpdateDefaultReceivingModeAsync(string username, string defaultMode)`
  - `UpdateDefaultDunnageModeAsync(string username, string defaultMode)`

### ViewModels

- `ViewModel_Settings_Workflow`
  - Observable: `CurrentStep : Enum_SettingsWorkflowStep`, `CurrentStepTitle : string`, `IsAdminPageVisible : bool`

- `ViewModel_Settings_ModeSelection`
  - Commands: `SelectReceivingSettings`, `SelectDunnageSettings`, `SelectShippingSettings`, `SelectVolvoSettings`, `SelectAdministrativeSettings`

- `ViewModel_Settings_DunnageMode`
  - Commands: `Back`, `SelectDunnageTypes`, `SelectDunnageInventory`

- `ViewModel_Settings_Placeholder`
  - Observable: `CategoryTitle : string`, `PlaceholderMessage : string`
  - Methods: `SetCategory(string title, string message)`
  - Commands: `Back`

### Views

- `View_Settings_Workflow`
  - Hosts the category UserControls and an `AdminFrame` for deep-linked admin pages.

- `View_Settings_ModeSelection`
  - Card-style hub (Receiving, Dunnage, UPS/FedEx, Volvo, Admin).

- `View_Settings_DunnageMode`
  - Card-style sub-hub (Types, Inventory) + Back.

- `View_Settings_Placeholder`
  - “Coming soon” placeholder + Back.

---

## Database Schema Details

Module_Settings does not define its own DAOs or stored procedures.

However, `Service_UserPreferences` currently depends on:

- `Dao_User.GetUserByWindowsUsernameAsync(...)` (Module_Core)
  - Stored procedure: `sp_GetUserByWindowsUsername`

### Notes / gaps

- `Service_UserPreferences.GetLatestUserPreferenceAsync(...)` returns a constructed `Model_UserPreference` with example defaults rather than reading persisted preference values.
- The three `UpdateDefault*Async` methods are placeholders and do not persist.

---

## Module Dependencies & Integration

### Upstream Dependencies (what Module_Settings uses)

- **Module_Core**
  - `IService_ErrorHandler`, `IService_LoggingUtility`
  - `IService_Navigation` (Frame navigation)
  - `IService_Focus` (focus handling when controls become visible)
  - `Converter_EnumToVisibility`

- **Module_Shared**
  - `ViewModel_Shared_Base` base class

- **External**
  - `Material.Icons.WinUI3`

### Downstream Dependents (what uses Module_Settings)

- **MainWindow** routes Settings navigation to `View_Settings_Workflow` and binds the window title/header to `CurrentStepTitle`.

### Cross-module integration points

- **Module_Dunnage**
  - Settings deep-links to Dunnage admin pages via the `AdminFrame`.

- **Module_Volvo**
  - Settings workflow hosts `View_Volvo_Settings` when step = `VolvoSettings`.

- **Module_Receiving / Module_Dunnage**
  - Both depend on `IService_UserPreferences` for default mode preferences.

---

## Common Patterns & Code Examples

### Pattern: Event-driven workflow coordinator

- Coordinator service raises `StepChanged`.
- Shell ViewModel maps step → UI state (`CurrentStep`, `CurrentStepTitle`, `IsAdminPageVisible`).

### Pattern: Settings shell is a “visibility router”

- Each category view is loaded once and shown/hidden by `Converter_EnumToVisibility`.
- Admin pages are hosted in a `Frame` and navigated to by `IService_Navigation`.

### Pattern: Focus management

- Each UserControl attaches `IService_Focus.AttachFocusOnVisibility(this)` to ensure keyboard focus behaves predictably as views are shown/hidden.

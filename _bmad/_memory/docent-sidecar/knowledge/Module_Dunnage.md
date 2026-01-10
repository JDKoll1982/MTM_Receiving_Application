---
module_name: Module_Dunnage
module_path: Module_Dunnage/
last_analyzed: 2026-01-09
last_validated: 2026-01-10
analyst: Docent v1.0.0
documentation_scope: full-module-analysis
component_counts:
  views_xaml: 19
  viewmodels: 14
  services_implementations: 3
  daos: 8
  models: 12
key_workflows:
  - Dunnage label entry (Guided wizard)
  - Dunnage label entry (Manual entry)
  - Dunnage label entry (Edit mode)
  - Dunnage Admin (Hub -> Types/Parts/Specs/Inventoried)
integration_points:
  upstream_dependencies:
    - Module_Core (contracts, services, converters)
    - Module_Shared (ViewModel base)
    - Module_Settings (user preferences)
    - CsvHelper
  downstream_dependents:
    - App navigation shell (Dunnage workflow view)
notes:
  token_limit_strategy: "Split detail sections into companion docs"
  companion_docs:
    - _bmad/_memory/docent-sidecar/knowledge/Module_Dunnage-CodeInventory.md
    - _bmad/_memory/docent-sidecar/knowledge/Module_Dunnage-Database.md
---

# Module_Dunnage - Module Documentation

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

Module_Dunnage implements dunnage label entry workflows plus a Dunnage Admin area for maintaining the dunnage master data.

- Label entry: Guided wizard (Type → Part → Quantity → Details → Review/Save)
- Label entry: Manual entry (bulk grid)
- Label entry: Edit mode (modify existing loads)
- Admin: 4-section management hub (Types, Parts, Specs, Inventoried List)

### Primary Entry Point

- `View_Dunnage_WorkflowView.xaml` hosts all step views and shows/hides them via boolean visibility flags owned by `Main_DunnageLabelViewModel`.

### Core State Machine

- `Service_DunnageWorkflow` is the step coordinator.
- Current step is `Enum_DunnageWorkflowStep`.
- Step changes are published via `StepChanged` and observed by the shell ViewModel to switch visibility.

### Notable Behaviors

- **Default mode auto-skip:** on workflow start, if the current user has a default dunnage mode (`guided` / `manual` / `edit`), the workflow service navigates directly to that step.
- **Save pipeline order:** `Service_DunnageWorkflow.SaveSessionAsync` saves to DB first, then exports CSV.
- **CSV export:** `Service_DunnageCSVWriter` writes RFC 4180 CSV (UTF-8, CRLF) to local path, then best-effort to network path.
- **Add Another (from Review):** saves a CSV-only backup, clears transient selections, then returns to Type Selection while preserving reviewed loads.

### Architecture Compliance (Highlights)

✅ Strong points

- Workflow state and validation gates are centralized in `Service_DunnageWorkflow`.
- CSV export implementation is explicit about formatting and network failure handling.

⚠️ Deviations detected (worth tracking)

- `View_Dunnage_WorkflowView.xaml.cs` contains navigation logic (Back/Next/Save&Review) and uses service locator (`App.GetService<T>()`).
- `ViewModel_Dunnage_ModeSelection` and `ViewModel_Dunnage_Review` use service locator to reach other ViewModels to clear inputs.

---

## Mermaid Workflow Diagrams

### Workflow 1: Dunnage Label Entry (Shell → Workflow Service → Save)

```mermaid
flowchart LR
  U[User] --> W[View Dunnage Workflow]
  W --> VMw[Main Dunnage Label ViewModel]
  VMw --> Svc[Service Dunnage Workflow]

  %% Reduce fan-out intersections
  VMw --> VC[Visibility controller]

  subgraph Guided[Guided wizard steps]
    direction TB
    VC --> VType[View Type Selection] --> VMType[VM Type Selection]
    VC --> VPart[View Part Selection] --> VMPart[VM Part Selection]
    VC --> VQty[View Quantity Entry] --> VMQty[VM Quantity Entry]
    VC --> VDet[View Details Entry] --> VMDet[VM Details Entry]
    VC --> VRev[View Review] --> VMRev[VM Review]
  end

  subgraph Other[Other modes]
    direction TB
    VC --> VMode[View Mode Selection] --> VMMode[VM Mode Selection]
    VC --> VMan[View Manual Entry] --> VMMan[VM Manual Entry]
    VC --> VEdit[View Edit Mode] --> VMEdit[VM Edit Mode]
  end

  %% StepChanged event (collapsed)
  Svc -. StepChanged .-> Note[Shell updates visibility; step VMs refresh when active]

  subgraph Persist[Persistence]
    direction TB
    Svc -->|Save DB| My[Dunnage MySQL service]
    My --> DaoLoad[Dao Dunnage Load]
    DaoLoad --> DB[(MySQL)]

    Svc -->|Export CSV| Csv[Service Dunnage CSV Writer]
    Csv --> L[(Local CSV)]
    Csv --> N[(Network CSV)]
  end
```

### Workflow 2: Dunnage Admin Navigation (Section Workflow)

```mermaid
flowchart LR
  U[User] --> AdminView[Dunnage Admin Main View]
  AdminView --> AdminVM[Dunnage Admin Main VM]
  AdminVM --> AdminSvc[Service Dunnage Admin Workflow]

  AdminSvc --> Hub[Hub]
  AdminSvc --> Types[Types]
  AdminSvc --> Parts[Parts]
  AdminSvc --> Specs[Specs]
  AdminSvc --> Inv[Inventoried List]

  AdminSvc -. SectionChanged .-> AdminVM
  AdminVM --> Nav[Switch visible admin view]
```

---

## User Interaction Lifecycle

### A) Guided Wizard

1. Workflow starts in Mode Selection, unless the user has a default dunnage mode (auto-skip).
2. Guided path steps:
   - Type Selection
   - Part Selection
   - Quantity Entry
   - Details Entry (PO number, location, specs)
   - Review (single view or table view; Save; Add Another)
3. Save uses `Service_DunnageWorkflow.SaveSessionAsync`:
   - Save to DB
   - Export CSV (local + best-effort network)

### B) Manual Entry

- Manual Entry uses a bulk grid to add/edit loads.
- Save typically delegates to workflow save/CSV export methods.

### C) Edit Mode

- Edit Mode focuses on loading existing loads, filtering/selecting, editing, and persisting updates.

### D) Admin Workflow

- Admin uses `Service_DunnageAdminWorkflow` to navigate between the Hub and four management sections.
- Admin workflow blocks navigation when unsaved changes exist (`IsDirty`).

---

## Code Inventory

See: [_bmad/_memory/docent-sidecar/knowledge/Module_Dunnage-CodeInventory.md](../docent-sidecar/knowledge/Module_Dunnage-CodeInventory.md)

---

## Database Schema Details

See: [_bmad/_memory/docent-sidecar/knowledge/Module_Dunnage-Database.md](../docent-sidecar/knowledge/Module_Dunnage-Database.md)

---

## Module Dependencies & Integration

### Key Dependencies

- MySQL stored-procedure access via `Helper_Database_StoredProcedure`.
- User identity + default mode via `IService_UserSessionManager`.
- User preferences updates via `IService_UserPreferences`.

---

## Common Patterns & Code Examples

### Visibility Switching

- `Main_DunnageLabelViewModel` hides all views then enables the active step flag when `Service_DunnageWorkflow.StepChanged` fires.

### CSV Export Formatting

- CSV is RFC 4180 (CRLF line endings, quoting when needed).
- Network export is best-effort; local export is the success anchor.

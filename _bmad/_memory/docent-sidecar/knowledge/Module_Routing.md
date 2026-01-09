---
module_name: Module_Routing
module_path: Module_Routing/
last_analyzed: 2026-01-09
last_validated: 2026-01-09
analyst: Docent v1.0.0
documentation_scope: full-module-analysis
component_counts:
  views_xaml_pages: 7
  viewmodels: 7
  services_implementations: 5
  services_interfaces: 5
  daos: 7
  dao_interfaces: 4
  models: 7
  converters: 3
  constants: 2
  enums: 2
key_workflows:
  - Mode selection (Wizard / Manual / Edit) with persisted default
  - Wizard label creation (Step1 PO/OTHER → Step2 Recipient → Step3 Review/Confirm)
  - Manual entry batch creation
  - Edit mode: search, edit, history, reprint (CSV)
  - CSV export with retry + network/local fallback
integration_points:
  upstream_dependencies:
    - Module_Core (result models, error handling, logging, navigation)
    - Module_Shared (ViewModel_Shared_Base)
    - MySQL (routing tables via stored procedures)
    - SQL Server (Infor Visual MTMFG, read-only)
    - CommunityToolkit.Mvvm
notes:
  companion_docs:
    - _bmad/_memory/docent-sidecar/knowledge/Module_Routing-CodeInventory.md
    - _bmad/_memory/docent-sidecar/knowledge/Module_Routing-Database.md
---

# Module_Routing - Module Documentation

## Module Overview

Module_Routing enables receiving personnel to create and manage internal routing labels for packages/materials.

The module supports three user modes:

- Wizard (primary): guided 3-step flow with PO validation and recipient selection
- Manual Entry: batch entry of multiple labels
- Edit Mode: search/edit/reprint labels and review history

Routing labels are written to MySQL and exported to a CSV file for printing/integration.

## Mermaid Workflow Diagrams

### Mode Selection → Mode Page

```mermaid
flowchart LR
  U[User] --> MS[Routing Mode Selection]
  MS -->|Wizard| WZ[Wizard Container]
  MS -->|Manual Entry| ME[Manual Entry]
  MS -->|Edit Mode| EM[Edit Mode]

  MS --> Pref[(User preference)]
  Pref -->|default mode| MS
```

### Wizard Flow (Step 1 → 2 → 3 → Create)

```mermaid
flowchart TD
  S1[Step 1: PO/Line or OTHER] --> S2[Step 2: Recipient]
  S2 --> S3[Step 3: Review]
  S3 -->|CreateLabel| RS[RoutingService]
  RS --> MyDao[MySQL DAOs]
  RS -->|Background| CSV[CSV Export]
  RS -->|Background| UT[Usage Tracking]

  S3 -->|Edit PO| S1
  S3 -->|Edit Recipient| S2

  subgraph InforVisual[Infor Visual - Read Only]
    IV[SQL Server DAO]
  end
  S1 -->|Validate PO / Load lines| IV
```

## User Interaction Lifecycle

### Wizard Mode

1. Step 1 validates PO number and fetches PO lines (Infor Visual read-only).
2. User selects a PO line.
3. Step 2 loads recipients and Quick Add suggestions.
4. Step 3 presents a review summary.
5. Create label:
   - Inserts label into MySQL
   - Starts background tasks:
     - CSV append (network with retry, local fallback)
     - Usage tracking increment

### Manual Entry Mode

- User enters multiple rows in a grid.
- Optional PO validation can prefill description/line for single-line POs.
- Save All iterates labels and creates each label through the service.

### Edit Mode

- Loads labels (bounded by a limit).
- User searches by PO/recipient/description.
- User can:
  - Save edits (updates label and logs history)
  - Reprint (CSV export only)

## Code Inventory

See: [_bmad/_memory/docent-sidecar/knowledge/Module_Routing-CodeInventory.md](../docent-sidecar/knowledge/Module_Routing-CodeInventory.md)

## Database Schema Details

See: [_bmad/_memory/docent-sidecar/knowledge/Module_Routing-Database.md](../docent-sidecar/knowledge/Module_Routing-Database.md)

## Module Dependencies & Integration

- **MySQL (application DB)**
  - All CRUD operations for routing tables are performed through stored procedures.

- **SQL Server (Infor Visual, READ ONLY)**
  - Used for PO validation and line retrieval.
  - Queries enforce `SITE_REF = '002'`.
  - DAO documentation expects the connection string to include `ApplicationIntent=ReadOnly`.

- **Navigation**
  - Mode selection uses `IService_Navigation` to navigate within a provided Frame.

## Known Deviations & Notes

- Wizard Step 1 “OTHER workflow” reasons are not currently loaded via service (placeholder behavior), even though a DAO and stored procedure exist for `routing_other_reasons`.
- Infor Visual integration uses graceful degradation: if the SQL Server connection check fails, PO validation returns success (true) to avoid blocking label creation.
- CSV export is serialized with a static semaphore to prevent concurrent writes; network path is attempted first with retries, then a local fallback is used.

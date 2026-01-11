---
module_name: Module_Volvo
component: code-inventory
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Volvo - Code Inventory

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Volvo.md](../docent-sidecar/knowledge/Module_Volvo.md)

## Pages (Views)

| View | DataContext | Notes |
|------|------------|-------|
| View_Volvo_ShipmentEntry | ViewModel_Volvo_ShipmentEntry | Shipment creation; parts grid; Generate Labels, Preview Email, Save Pending, Complete Shipment, View History. |
| View_Volvo_History | ViewModel_Volvo_History | Filter + view details + edit + export CSV. |
| View_Volvo_Settings | ViewModel_Volvo_Settings | Parts master data CRUD + import/export. |

## Dialogs

| Dialog | Purpose |
|--------|---------|
| VolvoPartAddEditDialog | Add/edit part in master data |
| VolvoShipmentEditDialog | Edit an existing shipment + its lines |

## ViewModels

| ViewModel | Responsibilities |
|----------|------------------|
| ViewModel_Volvo_ShipmentEntry | Load parts catalog + pending shipment; add/remove parts; validate and save; generate labels; preview email; complete shipment; navigate to history. |
| ViewModel_Volvo_History | Filter history; view shipment details; edit shipment; export history to CSV; navigate back. |
| ViewModel_Volvo_Settings | Load parts catalog; add/edit/deactivate parts; view components; import/export CSV. |

## Services

| Service | Responsibilities |
|--------|------------------|
| Service_Volvo | Shipment lifecycle + lines; component explosion; LabelView CSV generation; email formatting; history retrieval; update/complete operations; authorization checks. |
| Service_VolvoMasterData | Parts master CRUD and CSV import/export; components maintenance. |
| Service_VolvoAuthorization | Authorization checks (currently placeholder: allow-all + logging). |

## Data Access (DAOs)

| DAO | Primary Entity |
|-----|----------------|
| Dao_VolvoShipment | volvo_label_data |
| Dao_VolvoShipmentLine | volvo_line_data |
| Dao_VolvoPart | volvo_masterdata |
| Dao_VolvoPartComponent | volvo_part_components |
| Dao_VolvoSettings | settings_module_volvo |

## Models

- Model_VolvoShipment
- Model_VolvoShipmentLine
- Model_VolvoPart
- Model_VolvoPartComponent
- Model_VolvoSetting
- Model_VolvoEmailData
- Model_EmailRecipient
- VolvoShipmentStatus

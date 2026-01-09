---
module_name: Module_Dunnage
component: code-inventory
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Dunnage - Code Inventory

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Dunnage.md](../docent-sidecar/knowledge/Module_Dunnage.md)

## Primary Views (Label Entry)

| View | DataContext / Owner | Notes |
|------|----------------------|-------|
| View_Dunnage_WorkflowView | Main_DunnageLabelViewModel (Module_Core) | Hosts all step views, provides Help + Back/Next in code-behind. |
| View_Dunnage_ModeSelectionView | ViewModel_Dunnage_ModeSelection | Choose Guided/Manual/Edit; set default mode. |
| View_Dunnage_TypeSelectionView | ViewModel_Dunnage_TypeSelection | Select type; likely drives available specs + part list. |
| View_Dunnage_PartSelectionView | ViewModel_Dunnage_PartSelection | Select part; may support quick-add. |
| View_Dunnage_QuantityEntryView | ViewModel_Dunnage_QuantityEntry | Enter quantity. |
| View_Dunnage_DetailsEntryView | ViewModel_Dunnage_DetailsEntry | PO, location, spec inputs. |
| View_Dunnage_ReviewView | ViewModel_Dunnage_Review | Review loads; Save; Add Another; single/table toggle. |
| View_Dunnage_ManualEntryView | ViewModel_Dunnage_ManualEntry | Bulk grid entry. |
| View_Dunnage_EditModeView | ViewModel_Dunnage_EditMode | Load/edit existing loads. |

## Admin Views

| View | DataContext | Notes |
|------|------------|-------|
| View_Dunnage_AdminMainView | ViewModel_Dunnage_AdminMain | Admin hub (section navigation). |
| View_Dunnage_AdminTypesView | ViewModel_Dunnage_AdminTypes | Manage dunnage types. |
| View_Dunnage_AdminPartsView | ViewModel_Dunnage_AdminParts | Manage part master. |
| View_Dunnage_AdminInventoryView | ViewModel_Dunnage_AdminInventory | Manage inventoried list. |

## Dialogs / Controls

- View_Dunnage_Control_IconPickerControl
- View_Dunnage_Dialog_AddMultipleRowsDialog
- View_Dunnage_Dialog_AddToInventoriedListDialog
- View_Dunnage_Dialog_Dunnage_AddTypeDialog
- View_Dunnage_QuickAddPartDialog
- View_Dunnage_QuickAddTypeDialog

## ViewModels (Responsibilities)

| ViewModel | Responsibilities |
|----------|------------------|
| Main_DunnageLabelViewModel (Module_Core) | Shell VM: visibility switching + Reset CSV + ReturnToModeSelection. |
| ViewModel_Dunnage_ModeSelection | Pick mode; set default mode; confirm mode change if unsaved data exists. |
| ViewModel_Dunnage_TypeSelection | Select type; resettable; loads types/specs for selected type. |
| ViewModel_Dunnage_PartSelection | Select part; filters/search; likely type-scoped list. |
| ViewModel_Dunnage_QuantityEntry | Quantity input validation and session sync. |
| ViewModel_Dunnage_DetailsEntry | PO, location, and dynamic spec input collection. |
| ViewModel_Dunnage_Review | Displays session loads; supports Add Another (CSV-only backup), view switching, and Save. |
| ViewModel_Dunnage_ManualEntry | Bulk edit/add/remove; export selected; save. |
| ViewModel_Dunnage_EditMode | Load/edit existing loads; export and persistence. |
| ViewModel_Dunnage_AdminMain | Admin section navigation, uses admin workflow service. |
| ViewModel_Dunnage_AdminTypes | CRUD for types + icon picking; transaction/part counts. |
| ViewModel_Dunnage_AdminParts | CRUD for parts; search; transaction counts. |
| ViewModel_Dunnage_AdminInventory | Manage inventoried dunnage list (add/update/delete/check). |
| ViewModel_Dunnage_AddTypeDialog | Supports adding new type via dialog. |

## Services

| Service | Key Behaviors |
|--------|---------------|
| Service_DunnageWorkflow | Step state machine; manages in-memory session; SaveSession (DB then CSV); Reset CSV. |
| Service_DunnageCSVWriter | RFC 4180 CSV writer; dynamic spec columns; local + best-effort network export. |
| Service_DunnageAdminWorkflow | Admin section navigation + dirty-state gating. |

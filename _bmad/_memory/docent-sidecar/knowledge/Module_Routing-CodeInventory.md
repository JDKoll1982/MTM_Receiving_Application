---
module_name: Module_Routing
component: code-inventory
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Routing - Code Inventory

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Routing.md](../docent-sidecar/knowledge/Module_Routing.md)

## Views (XAML pages)

| View | ViewModel | Purpose |
|------|-----------|---------|
| RoutingModeSelectionView | RoutingModeSelectionViewModel | Select Wizard/Manual/Edit and set default mode |
| RoutingWizardContainerView | RoutingWizardContainerViewModel | Hosts the 3-step wizard flow and shared state |
| RoutingWizardStep1View | RoutingWizardStep1ViewModel | PO validation + line selection (or OTHER) |
| RoutingWizardStep2View | RoutingWizardStep2ViewModel | Recipient selection + Quick Add |
| RoutingWizardStep3View | RoutingWizardStep3ViewModel | Review + confirm + duplicate warning |
| RoutingManualEntryView | RoutingManualEntryViewModel | Batch label creation grid |
| RoutingEditModeView | RoutingEditModeViewModel | Search/edit/reprint labels |

## ViewModels

| ViewModel | Responsibilities |
|----------|------------------|
| RoutingModeSelectionViewModel | Loads/saves user preference; navigates to selected mode |
| RoutingWizardContainerViewModel | Shared wizard state; step navigation; builds and creates label |
| RoutingWizardStep1ViewModel | PO validation + PO lines; toggles OTHER mode (placeholder load) |
| RoutingWizardStep2ViewModel | Loads recipients; Quick Add; filtering/search |
| RoutingWizardStep3ViewModel | Review summary; duplicate confirmation; triggers create |
| RoutingManualEntryViewModel | Bulk entry + per-row PO validation + Save All |
| RoutingEditModeViewModel | Loads labels and recipients; searching; edit and reprint |

## Services

| Service | Interface | Responsibilities |
|--------|-----------|------------------|
| RoutingService | IRoutingService | Label CRUD, duplicate checks, CSV export, history logging orchestration |
| RoutingRecipientService | IRoutingRecipientService | Recipient retrieval and Quick Add logic |
| RoutingInforVisualService | IRoutingInforVisualService | Read-only Infor Visual PO validation + lines; graceful degradation |
| RoutingUsageTrackingService | IRoutingUsageTrackingService | Usage increment (analytics) |
| RoutingUserPreferenceService | IRoutingUserPreferenceService | Default-mode and validation preference persistence |

## DAOs

| DAO | Database | Notes |
|-----|----------|-------|
| Dao_RoutingLabel | MySQL | Uses stored procedures for insert/update/get/delete/mark-exported/duplicate-check |
| Dao_RoutingLabelHistory | MySQL | Stored procedures for history insert and retrieval |
| Dao_RoutingRecipient | MySQL | Active recipients + top-by-usage (Quick Add) |
| Dao_RoutingOtherReason | MySQL | Active “OTHER reasons” list (wizard currently not wired through service) |
| Dao_RoutingUsageTracking | MySQL | Usage increment for employee-recipient pair |
| Dao_RoutingUserPreference | MySQL | Default mode + validation toggle |
| Dao_InforVisualPO | SQL Server (READ ONLY) | Parameterized SELECTs with `SITE_REF = '002'` |

## Other Components

- Converters: IntToFontWeightConverter, IntToProgressBrushConverter, NullToBooleanConverter
- Constants: Constant_Routing, Constant_RoutingConfiguration
- Enums: Enum_RoutingMode, Enum_Routing_WorkflowStep

---
module_name: Module_Receiving
component: code-inventory
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Receiving - Code Inventory

This file is a companion to the main module doc:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Receiving.md](../docent-sidecar/knowledge/Module_Receiving.md)

## Views and Primary Bindings

| View | Type | Primary ViewModel | Primary Commands / Bindings |
|------|------|-------------------|-----------------------------|
| View_Receiving_Workflow | Page | ViewModel_Receiving_Workflow | NextStepCommand, PreviousStepCommand, ReturnToModeSelectionCommand, ResetCSVCommand, StartNewEntryCommand, visibility flags per step |
| View_Receiving_ModeSelection | UserControl | ViewModel_Receiving_ModeSelection | SelectGuidedModeCommand, SelectManualModeCommand, SelectEditModeCommand, SetGuidedAsDefaultCommand, SetManualAsDefaultCommand, SetEditAsDefaultCommand |
| View_Receiving_POEntry | UserControl | ViewModel_Receiving_POEntry | LoadPOCommand, ToggleNonPOCommand, LookupPartCommand, PoNumber/PartID TwoWay, SelectedPart TwoWay |
| View_Receiving_LoadEntry | UserControl | ViewModel_Receiving_LoadEntry | NumberOfLoads TwoWay |
| View_Receiving_WeightQuantity | UserControl | ViewModel_Receiving_WeightQuantity | Loads list edits (WeightQuantity), warning banner via HasWarning/WarningMessage |
| View_Receiving_HeatLot | UserControl | ViewModel_Receiving_HeatLot | AutoFillCommand, Loads list edits (HeatLotNumber) |
| View_Receiving_PackageType | UserControl | ViewModel_Receiving_PackageType | SelectedPackageType TwoWay, CustomPackageTypeName TwoWay, IsSaveAsDefault TwoWay |
| View_Receiving_Review | UserControl | ViewModel_Receiving_Review | PreviousEntryCommand, NextEntryCommand, SwitchToTableViewCommand, SwitchToSingleViewCommand, SaveCommand, AddAnotherPartCommand |
| View_Receiving_ManualEntry | UserControl | ViewModel_Receiving_ManualEntry | AddRowCommand, AddMultipleRowsCommand, RemoveRowCommand, AutoFillCommand, SaveCommand |
| View_Receiving_EditMode | UserControl | ViewModel_Receiving_EditMode | LoadFromCurrentMemoryCommand, LoadFromCurrentLabelsCommand, LoadFromHistoryCommand, SelectAllCommand, RemoveRowCommand, SaveCommand, paging + date filters |

## ViewModels and Responsibilities

| ViewModel | Responsibilities | Key Dependencies |
|----------|-------------------|------------------|
| ViewModel_Receiving_Workflow | Shell VM. Tracks current step, toggles sub-view visibility, dispatches save, exposes reset and navigation commands. | IService_ReceivingWorkflow, IService_Dispatcher, IService_Window, IService_Help |
| ViewModel_Receiving_ModeSelection | Selects Guided/Manual/Edit workflows; updates default-mode preferences; protects against losing unsaved work. | IService_ReceivingWorkflow, IService_UserSessionManager, IService_UserPreferences, IService_Window, IService_Help |
| ViewModel_Receiving_POEntry | Formats/validates PO entry, loads PO+parts from Infor Visual, supports Non-PO part lookup; syncs PO + SelectedPart into workflow state. | IService_InforVisual, IService_ReceivingWorkflow, IService_ViewModelRegistry, IService_Help |
| ViewModel_Receiving_LoadEntry | Captures number-of-loads and syncs into workflow service. | IService_ReceivingWorkflow, IService_ReceivingValidation |
| ViewModel_Receiving_WeightQuantity | Refreshes current loads from session; validates per-load quantity; warns if over PO qty or received same-day. | IService_ReceivingWorkflow, IService_InforVisual, IService_ReceivingValidation |
| ViewModel_Receiving_HeatLot | Refreshes loads and supports auto-fill down; sets blanks to “Nothing Entered”. | IService_ReceivingWorkflow, IService_ReceivingValidation |
| ViewModel_Receiving_PackageType | Applies one package type to all loads; can persist per-part default package type preference. | IService_ReceivingWorkflow, IService_MySQL_PackagePreferences, IService_ReceivingValidation |
| ViewModel_Receiving_Review | Displays loads single-by-single or as table; supports “Add Another Part”; triggers save. | IService_ReceivingWorkflow, IService_ReceivingValidation, IService_Window |
| ViewModel_Receiving_ManualEntry | Bulk editable grid with row add/remove and autofill; triggers save step. | IService_ReceivingWorkflow, IService_MySQL_Receiving, IService_Window |
| ViewModel_Receiving_EditMode | Loads and edits existing loads from memory/CSV/history; supports paging and filtering; saves updates and deletes. | IService_ReceivingWorkflow, IService_MySQL_Receiving, IService_CSVWriter, IService_Pagination |

## Services and Key Behaviors

| Service | Key Behaviors |
|--------|---------------|
| Service_ReceivingWorkflow | State machine for step transitions + validation gates; persists session; orchestrates save to CSV and DB; exposes reset + CSV reset helpers. |
| Service_ReceivingValidation | Stateless validation rules for PO, part, load counts, heat/lot, etc.; calls Infor Visual for same-day receiving checks. |
| Service_CSVWriter | Writes receiving loads to local CSV and attempts network CSV; clears files on request; network failures are non-fatal. |
| Service_SessionManager | Saves/loads session JSON under %APPDATA% and handles corrupted session file cleanup. |
| Service_MySQL_ReceivingLine | Thin wrapper around Dao_ReceivingLine with logging + error handling. |

## Notable Implementation Notes

- Workflow step viewmodels refresh their `Loads` collections when their step becomes active (via `IService_ReceivingWorkflow.StepChanged`).
- DataGrid columns use runtime `Binding` (expected with CommunityToolkit DataGrid columns), while outer ViewModel bindings use `x:Bind`.
- `View_Receiving_Workflow.xaml.cs` performs default-mode auto-skip in `OnNavigatedTo` (small amount of view code-behind logic).

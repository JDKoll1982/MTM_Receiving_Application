# Contracts: Persistent Shell Action Bar

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md) | **Research**: [research.md](../research.md)
**Created**: 2026-11-19

These are the interfaces a consumer or a test codes against. They fix the shell's contract surface
and the per-page action matrix, not an implementation sketch.

- [shell-action-provider.md](shell-action-provider.md): the core shell contract the bar resolves
  from the active page, the snapshot it returns, and the single default-mode method it exposes.
- [per-page-action-matrix.md](per-page-action-matrix.md): the per-page UI contract listing exactly
  which actions each Receiving page offers and which verb occupies the Back or Next slot.
- [welded-parts-service.md](welded-parts-service.md): the welded part list's one interface, the rules the
  service owns, and the three existing procedures the DAO calls. This is the whole of US8's surface.

## Pinned Identifiers

Every string below is copied from the specification's Verbatim Constraints. None may be renamed,
re-cased, pluralised, or paraphrased.

| What | Exact value | Where this contract uses it |
| --- | --- | --- |
| Action labels | `Guided`, `Manual`, `Edit`, `Overlay`, `Back`, `Next`, `Clear Label Data`, `Help`, `Save & Finish`, `Save to Database`, `Start New Entry` | action slots and the per-page matrix |
| Default-checkbox tooltip text | `make Guided my default`, `make Manual my default`, `make Edit my default` | default-checkbox tooltips |
| Labels that must no longer appear | `Mode Selection`, `Add another Part/PO` | forbidden labels |
| Page names | `Enter PO Number`, `Select Part`, `Enter Load Information`, `Enter Weight & Quantity`, `Enter Heat & Lot`, `Enter Package Type`, `Manual Entry`, `Edit Mode`, `Review & Save`, `Result` | per-page matrix |
| Stored default mode values | `guided`, `manual`, `edit` | `SetDefaultMode` argument domain |
| Shell elements that stay | `ContentFrame`, `HeaderBarBorder`, `HeaderBackButton`, `HeaderStepBar`, `UserMenuButton`, `PrimaryLabelButtonsPanel` | shell wiring |
| Resolve-and-track pattern | `IViewModel_HeaderTitleProvider`, `IViewModel_HeaderStepBarProvider`, `MainWindow.UpdateHeader`, `MainWindow.TrackHeaderStepBar` | shell resolution |
| Header back capability | `IService_HeaderBackNavigation` | stays wired and hidden |
| Navigation row to delete | `ReceivingWorkflowNavigationGrid` in `Module_Receiving/Views/View_Receiving_Workflow.xaml` | deleted surface |
| Mode-selection page to delete | `Module_Receiving/Views/View_Receiving_ModeSelection.xaml`, `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`, `ModeSelection`, `ReturnToModeSelectionCommand` | deleted surface |
| Actions the strip invokes | `PreviousStepCommand`, `NextStepCommand`, `ResetLabelDataAsync`, `CanClearLabelData`, `ShowHelpAsync` | provider implementation |
| Services the work reuses | `IService_UserPreferences.UpdateDefaultReceivingModeAsync`, `IService_ReceivingWorkflow.IsPoPartListLoaded` | default persistence and Next gating |
| Overlay window | `View_Receiving_LabelOverlayWindow` | overlay trigger |
| Settings surface | `ReceivingSettingsKeys`, `IService_ReceivingSettings` | every label and tooltip |
| Registration location | `Infrastructure/DependencyInjection`, never `App.xaml.cs` | dependency injection |
| Keyboard shortcuts | `Ctrl+Left`, `Ctrl+Right` | Back and Next parity |
| Badge that marks the stored default | `Default` | mode group badge |
| Action colours | `ShellSuccessBrush`, `ShellDangerBrush` | save and back colouring |
| Shared welded list | `settings_weldedcoils`, `sp_Settings_WeldedCoils_GetAll`, `sp_Settings_WeldedCoils_Insert`, `sp_Settings_WeldedCoils_Delete` | welded parts service |
| Settings card that must match its step title | `Welded Part Numbers` | hub card resolution |

# Module_Receiving — UI Conditional Guards & Workflow Step Activation

Last Updated: 2026-03-21

This document captures the confirmed workflow shell behaviour, step activation rules,
and blocking conditions for the Receiving module.

---

## Workflow Shell Step-To-View Mapping

The workflow shell (`View_Receiving_Workflow.xaml`) hosts one step view at a time.
The active screen is selected by the `Enum_ReceivingWorkflowStep` state and a matching visibility property on `ViewModel_Receiving_Workflow`.

| Workflow Step | Visibility Property | Hosted View |
|---|---|---|
| `ModeSelection` | `IsModeSelectionVisible` | `View_Receiving_ModeSelection.xaml` |
| `ManualEntry` | `IsManualEntryVisible` | `View_Receiving_ManualEntry.xaml` |
| `EditMode` | `IsEditModeVisible` | `View_Receiving_EditMode.xaml` |
| `POEntry` | `IsPOEntryVisible` | `View_Receiving_POEntry.xaml` |
| `PartSelection` | `IsPartSelectionVisible` | PO-driven part selection state inside the guided flow |
| `LoadEntry` | `IsLoadEntryVisible` | `View_Receiving_LoadEntry.xaml` |
| `WeightQuantityEntry` | `IsWeightQuantityEntryVisible` | `View_Receiving_WeightQuantity.xaml` |
| `HeatLotEntry` | `IsHeatLotEntryVisible` | `View_Receiving_HeatLot.xaml` |
| `PackageTypeEntry` | `IsPackageTypeEntryVisible` | `View_Receiving_PackageType.xaml` |
| `Review` | `IsReviewVisible` | `View_Receiving_Review.xaml` |
| `Saving` | `IsSavingVisible` | Saving state inside workflow shell |
| `Complete` | `IsCompleteVisible` | Completion state inside workflow shell |

---

## Startup Mode Bypass (Priority Order)

The ModeSelection screen can be skipped entirely. The first matching condition wins.

| Priority | Source | Condition | Result |
|---|---|---|---|
| 1 | Session manager | Existing session with unsaved loads found on startup | Jumps to **Review**; ModeSelection never shown |
| 2 | User profile | `User.DefaultReceivingMode = "guided"` | Jumps to **POEntry** |
| 2 | User profile | `User.DefaultReceivingMode = "manual"` | Jumps to **ManualEntry** |
| 2 | User profile | `User.DefaultReceivingMode = "edit"` | Jumps to **EditMode** |
| 3 | Settings | `BusinessRules.RememberLastMode` (bool, per-user) `= true` | Reads `Defaults.DefaultReceivingMode` for last-used mode; applies that mode |
| 3 | Settings | `BusinessRules.DefaultModeOnStartup` (string, per-user) `= "guided"` | Jumps to **POEntry** |
| 3 | Settings | `BusinessRules.DefaultModeOnStartup` (string, per-user) `= "manual"` | Jumps to **ManualEntry** |
| 4 | Fallback | No user default, no configured setting | **ModeSelection** shown |

**Source:** `Service_ReceivingWorkflow.cs` — startup / `InitializeAsync` block

---

## Step Visibility Properties

All steps are hidden by default. `OnWorkflowStepChanged` sets exactly one to `true` on each transition.

| Workflow Step (Enum) | ViewModel Property | Default |
|---|---|---|
| `ModeSelection` | `IsModeSelectionVisible` | `false` |
| `ManualEntry` | `IsManualEntryVisible` | `false` |
| `EditMode` | `IsEditModeVisible` | `false` |
| `POEntry` | `IsPOEntryVisible` | `false` |
| `PartSelection` | `IsPartSelectionVisible` | `false` |
| `LoadEntry` | `IsLoadEntryVisible` | `false` |
| `WeightQuantityEntry` | `IsWeightQuantityEntryVisible` | `false` |
| `HeatLotEntry` | `IsHeatLotEntryVisible` | `false` |
| `PackageTypeEntry` | `IsPackageTypeEntryVisible` | `false` |
| `Review` | `IsReviewVisible` | `false` |
| `Saving` | `IsSavingVisible` | `false` |
| `Complete` | `IsCompleteVisible` | `false` |

**Source:** `ViewModel_Receiving_Workflow.cs` — `OnWorkflowStepChanged` handler

---

## Core User Journeys

The analysis confirmed three main entry paths into Receiving:

1. Guided flow: `ModeSelection` → `POEntry` → optional `PartSelection` → `LoadEntry` → `WeightQuantityEntry` → `HeatLotEntry` → `PackageTypeEntry` → `Review` → `Saving` → `Complete`
2. Manual flow: `ModeSelection` → `ManualEntry` → `Saving` → `Complete`
3. Edit flow: `ModeSelection` or post-save edit → `EditMode` → `Saving` or return to workflow review context

The `PartSelection` step is dynamic and only appears when a PO lookup returns more than one matching part.

---

## Step Exit Guards

Each step blocks advancement until all conditions are met. Guards run in `AdvanceToNextStepAsync`.

| From Step | Exit Blocked When | Error Returned |
|---|---|---|
| `POEntry` | PO Number is blank **and** `IsNonPOItem = false` | `"PO Number is required."` |
| `POEntry` | `CurrentPart == null` (no part resolved) | `"Part selection is required."` |
| `LoadEntry` | `NumberOfLoads < 1` | `"Number of loads must be at least 1."` |
| `LoadEntry` | Location is blank **and** `Defaults.DefaultLocation` setting is also blank | Location validation failure |
| `LoadEntry` | Location fails `ValidateLocationAsync` | Message from validation service |
| `WeightQuantityEntry` | Any load has invalid weight or quantity | `"Load {n}: {message}"` per failing load |
| `HeatLotEntry` | Any load's heat/lot entry exceeds max character length | `"Load {n}: {message}"` per failing load |
| `PackageTypeEntry` | Any load has invalid package count | `"Load {n}: {message}"` per failing load |
| `PackageTypeEntry` | Any load has no package type selected | `"Load {n}: Package Type is required."` |
| `ManualEntry` | Any load has `IsQualityHoldRequired = true` and `IsQualityHoldAcknowledged = false` | `"Quality hold acknowledgment required for {n} load(s) before proceeding."` |
| `EditMode` | — | Always allowed |
| `Review` | — | Always allowed |

**Note — HeatLotEntry auto-fill:** Any load with a blank heat/lot field is automatically populated with `"Nothing Entered"` before length validation runs. This is not a block — it is a silent default.

**Source:** `Service_ReceivingWorkflow.cs` — `AdvanceToNextStepAsync`

---

## Dynamic PartSelection Step Insertion

`PartSelection` is not part of the fixed advance chain. It is injected mid-flow from `POEntry` only under specific conditions.

| Condition During PO Lookup | Outcome |
|---|---|
| PO lookup returns **more than 1** matching part | `GoToStep(PartSelection)` called; user must pick a part before `LoadEntry` |
| PO lookup returns **exactly 1** part | Part is auto-selected; `PartSelection` step skipped entirely |
| Item is flagged as Non-PO (`IsNonPOItem = true`) | `PartSelection` step skipped entirely |

**Source:** `ViewModel_Receiving_POEntry.cs` — PO lookup handler

---

## Step-Level Screen Behaviour Confirmed From XAML And ViewModels

| Screen | Key User Actions | Notable Gate Or State |
|---|---|---|
| Mode Selection | Choose Guided, Manual, or Edit | Can show confirmation dialog before clearing in-progress work |
| PO Entry | Enter PO, load PO, switch to Non-PO, look up part | Blocks guided flow if no PO or no part is resolved |
| Load Entry | Enter number of loads and optional location | Location can be defaulted from settings; invalid location blocks advance |
| Weight / Quantity | Enter per-load quantity; review warnings | Same-day receiving and over-receive logic can warn before advance |
| Heat / Lot | Enter per-load heat/lot values or auto-fill | Blank values are normalized to `Nothing Entered` before final validation |
| Package Type | Pick package type, custom name, package counts | Missing type or invalid package counts block advance |
| Review | Switch between single-entry and table view; save or add another | Save path is always allowed from Review |
| Manual Entry | Work in grid, add/remove rows, auto-fill, save | Quality hold acknowledgments can block save |
| Edit Mode | Search, paginate, edit existing loads, save changes | Entry source can be memory, current labels, or history |

---

## Complete Step — Settings-Controlled UI

| ViewModel Property | Setting Key | Behaviour When `true` | Behaviour When `false` |
|---|---|---|---|
| `CanEditAfterSave` | `BusinessRules.AllowEditAfterSave` | Edit button visible on Complete screen | Edit button hidden via `BooleanToVisibilityConverter` |

**XAML binding:** `Visibility="{x:Bind ViewModel.CanEditAfterSave, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"`

**Source:** `ViewModel_Receiving_Workflow.cs` — `LoadSettingsAsync`, `View_Receiving_Workflow.xaml`

---

## Universal Busy Guard

All commands in this module guard against concurrent execution using `IsBusy`.

| Pattern | Applied To |
|---|---|
| `if (IsBusy) return;` at command entry | All async commands |
| `CanExecute = nameof(CanXxx)` where `CanXxx` includes `&& !IsBusy` | Primary action commands |
| `IsBusy = true` in `try` / `IsBusy = false` in `finally` | All async commands |

---

## Related Documents

- `Settings-Feature-Flags.md` — how runtime settings change the receiving flow
- `Architecture-Inventory.md` — screens, ViewModels, services, DAOs, models, and enum dependencies

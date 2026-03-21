# Module_Receiving — UI Conditional Guards & Workflow Step Activation

Last Updated: 2026-03-21

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

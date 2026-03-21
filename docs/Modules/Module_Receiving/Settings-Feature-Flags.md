# Module_Receiving — Settings Feature Flags & Behaviour Impact

Last Updated: 2026-03-21

This document maps every `ReceivingSettingsKeys` setting to the exact runtime behaviour it
controls. Settings are consumed by `Module_Settings_Receiving` (storage/defaults) and read
at runtime inside `Module_Receiving`. Code is the source of truth.

---

## Summary

`Module_Settings.Receiving` does configure `Module_Receiving` behaviour without a code change.
The code scan confirmed three categories of runtime impact:

1. Validation rules used while the workflow runs
2. Business-rule toggles that change step flow or UI state
3. Defaults and padding rules that pre-fill or transform user input

The same scan also confirmed that some keys are defined but are not yet actively consumed by the receiving runtime.

---

## Group: Validation (`Receiving.Validation.*`)

These keys are consumed by `Service_ReceivingValidation` and by the workflow service when it decides whether the user can advance.

| Setting Key                          | Type | Default  | Behaviour Impact                                                                   |
| ------------------------------------ | ---- | -------- | ---------------------------------------------------------------------------------- |
| `Validation.RequirePoNumber`         | bool | `false`  | When enabled, guided PO entry treats a blank PO number as an error.                |
| `Validation.RequireQuantity`         | bool | `true`   | Makes quantity a required value during receiving validation.                       |
| `Validation.RequireHeatLot`          | bool | `false`  | Treats blank heat/lot values as invalid instead of optional.                       |
| `Validation.AllowNegativeQuantity`   | bool | `false`  | Allows negative quantity inputs when enabled; otherwise quantity must be positive. |
| `Validation.ValidatePoExists`        | bool | `true`   | Enables real PO lookup validation against Infor Visual.                            |
| `Validation.ValidatePartExists`      | bool | `true`   | Enables real part validation against Infor Visual.                                 |
| `Validation.WarnOnQuantityExceedsPo` | bool | `true`   | Controls whether over-receive becomes a warning condition.                         |
| `Validation.WarnOnSameDayReceiving`  | bool | `true`   | Controls whether same-day receiving checks produce a warning.                      |
| `Validation.MinLoadCount`            | int  | `1`      | Lower bound for allowed load count.                                                |
| `Validation.MaxLoadCount`            | int  | `99`     | Upper bound for allowed load count.                                                |
| `Validation.MinQuantity`             | int  | `0`      | Lower bound for allowed quantity.                                                  |
| `Validation.MaxQuantity`             | int  | `999999` | Upper bound for allowed quantity.                                                  |

**Confirmed consumers:** `Service_ReceivingValidation.cs`, `Service_ReceivingWorkflow.cs`

---

## Group: Business Rules (`Receiving.BusinessRules.*`)

These keys affect startup routing, review behaviour, command confirmation, and guided-step UI state.

| Setting Key                              | Type   | Default         | Behaviour Impact                                                                                 |
| ---------------------------------------- | ------ | --------------- | ------------------------------------------------------------------------------------------------ |
| `BusinessRules.AutoSaveEnabled`          | bool   | `false`         | Defined in settings, but no active receiving runtime consumer was confirmed in the current scan. |
| `BusinessRules.AutoSaveIntervalSeconds`  | int    | `300`           | Defined in settings, but no active timer consumer was confirmed in the current scan.             |
| `BusinessRules.DefaultModeOnStartup`     | string | `ModeSelection` | Can bypass Mode Selection and start the workflow in a configured mode.                           |
| `BusinessRules.RememberLastMode`         | bool   | `true`          | Allows the workflow startup logic to reuse the last selected receiving mode.                     |
| `BusinessRules.ConfirmModeChange`        | bool   | `true`          | Controls whether mode switching asks for confirmation before clearing in-progress work.          |
| `BusinessRules.AutoFillHeatLotEnabled`   | bool   | `true`          | Controls whether Heat/Lot auto-fill is offered when that step opens.                             |
| `BusinessRules.SavePackageTypeAsDefault` | bool   | `false`         | Controls whether the package-type step shows the save-as-default option.                         |
| `BusinessRules.ShowReviewTableByDefault` | bool   | `false`         | Controls whether the Review screen opens in table view by default.                               |
| `BusinessRules.AllowEditAfterSave`       | bool   | `true`          | Controls whether the Complete screen exposes the post-save Edit action.                          |

**Confirmed consumers:** `Service_ReceivingWorkflow.cs`, `ViewModel_Receiving_Workflow.cs`, `ViewModel_Receiving_Review.cs`, `ViewModel_Receiving_HeatLot.cs`, `ViewModel_Receiving_PackageType.cs`

---

## Group: Defaults (`Receiving.Defaults.*`)

These keys are used to pre-fill or restore values that shape the next receiving session.

| Setting Key                     | Type   | Default                        | Behaviour Impact                                                                               |
| ------------------------------- | ------ | ------------------------------ | ---------------------------------------------------------------------------------------------- |
| `Defaults.DefaultReceivingMode` | string | `Guided`                       | Default fallback mode value used by settings and startup logic when mode preferences are read. |
| `Defaults.DefaultLocation`      | string | `RECV`                         | Default location used when the guided flow needs a location fallback.                          |
| `Defaults.XlsSaveLocation`      | string | Not confirmed in defaults file | Key exists, but no active consumer was confirmed in the current receiving runtime scan.        |

---

## Group: Part Number Padding (`Receiving.PartNumberPadding.*`)

These keys are consumed by the Manual Entry view path and affect how part numbers are transformed during entry.

| Setting Key                   | Type   | Default                          | Behaviour Impact                                             |
| ----------------------------- | ------ | -------------------------------- | ------------------------------------------------------------ |
| `PartNumberPadding.Enabled`   | bool   | `true`                           | Enables prefix-based padding rules in Manual Entry.          |
| `PartNumberPadding.RulesJson` | string | Two default rules (`MMC`, `MMF`) | Stores the JSON rule list used to pad matching part numbers. |

**Confirmed consumer:** `View_Receiving_ManualEntry.xaml.cs`

---

## Group: UI Text, Workflow Text, Messages, and Accessibility

The module defines a large text surface under these groups:

- `Receiving.UiText.*`
- `Receiving.Workflow.*`
- `Receiving.Messages.*`
- `Receiving.Accessibility.*`

These keys primarily control labels, prompts, status strings, step titles, and accessibility text.
They do not change core business rules by themselves, but they do change what the user sees at runtime.

**Confirmed consumers:** workflow and step ViewModels such as `ViewModel_Receiving_Workflow`, `ViewModel_Receiving_ModeSelection`, `ViewModel_Receiving_POEntry`, `ViewModel_Receiving_LoadEntry`, `ViewModel_Receiving_WeightQuantity`, `ViewModel_Receiving_HeatLot`, `ViewModel_Receiving_PackageType`, and `ViewModel_Receiving_Review`

---

## Group: Integrations (`Receiving.Integrations.*`)

The current analysis matched your earlier clarification: these keys exist, but no active Receiving runtime consumer was confirmed during the code scan.

| Setting Key                            | Type | Default |
| -------------------------------------- | ---- | ------- |
| `Integrations.ErpSyncEnabled`          | bool | `true`  |
| `Integrations.AutoPullPoDataEnabled`   | bool | `true`  |
| `Integrations.AutoPullPartDataEnabled` | bool | `true`  |
| `Integrations.SyncToInforVisual`       | bool | `false` |
| `Integrations.RetryFailedSyncs`        | bool | `true`  |
| `Integrations.ErpConnectionTimeout`    | int  | `30`    |
| `Integrations.MaxSyncRetries`          | int  | `3`     |

Treat these as placeholders until a consumer is added.

---

## Setting Group → Confirmed Consumer Map

| Group                                                     | Consumer                                                                                          | Read Timing                               |
| --------------------------------------------------------- | ------------------------------------------------------------------------------------------------- | ----------------------------------------- |
| `Validation.*`                                            | `Service_ReceivingValidation`                                                                     | During field and workflow validation      |
| `BusinessRules.*`                                         | `Service_ReceivingWorkflow`, multiple Receiving ViewModels                                        | Startup or step entry                     |
| `Defaults.*`                                              | `Service_ReceivingWorkflow`, `ViewModel_Receiving_ModeSelection`, `ViewModel_Receiving_LoadEntry` | Startup and step prefill                  |
| `PartNumberPadding.*`                                     | `View_Receiving_ManualEntry.xaml.cs`                                                              | Manual Entry view load / input processing |
| `UiText.*`, `Workflow.*`, `Messages.*`, `Accessibility.*` | Receiving step ViewModels                                                                         | ViewModel text-load phase                 |
| `Integrations.*`                                          | None confirmed                                                                                    | Not yet wired                             |

---

## Settings That Create Conditional Workflows

The following settings change the workflow path — not just UI appearance:

| Setting                                  | Condition                            | Workflow Effect                                                                                 |
| ---------------------------------------- | ------------------------------------ | ----------------------------------------------------------------------------------------------- |
| `BusinessRules.DefaultModeOnStartup`     | = `"guided"` / `"manual"` / `"edit"` | ModeSelection screen skipped; workflow starts at that step                                      |
| `BusinessRules.RememberLastMode`         | = `true`                             | Last-used mode persisted; `Defaults.DefaultReceivingMode` drives startup step                   |
| `BusinessRules.AllowEditAfterSave`       | = `false`                            | Edit button removed from Complete screen; completed sessions cannot be reopened                 |
| `BusinessRules.ConfirmModeChange`        | = `false`                            | No confirmation dialog on mid-session mode switch; transition is instant                        |
| `BusinessRules.AutoFillHeatLotEnabled`   | = `false`                            | Auto-fill button not shown on HeatLot step; each load must have its heat/lot typed individually |
| `BusinessRules.SavePackageTypeAsDefault` | = `false`                            | "Save as default" checkbox not shown on PackageType step                                        |
| `BusinessRules.ShowReviewTableByDefault` | = `false`                            | Review step opens in single-card view instead of table view                                     |
| `Validation.RequirePoNumber`             | = `false`                            | PO Number field becomes optional; blank is accepted                                             |
| `Validation.RequireHeatLot`              | = `true`                             | HeatLot step blocks advancement if all loads have blank heat/lot values                         |
| `Validation.AllowNegativeQuantity`       | = `true`                             | Negative weight/quantity entries accepted; only exact zero is rejected                          |
| `Validation.WarnOnQuantityExceedsPo`     | = `false`                            | Over-receive silently accepted; Infor Visual over-receive warning suppressed                    |
| `Validation.WarnOnSameDayReceiving`      | = `false`                            | Same-day duplicate receive silently accepted                                                    |
| `Validation.ValidatePoExists`            | = `false`                            | PO number not looked up in Infor Visual; any string accepted                                    |
| `Defaults.DefaultLocation`               | non-empty                            | Location field pre-populated; LoadEntry step advancement unblocked without user input           |
| `PartNumberPadding.Enabled`              | = `true`                             | Part IDs in Manual Entry grid are transformed by padding rules before being stored              |

---

## Source Files

| File                                                          | Role                                       |
| ------------------------------------------------------------- | ------------------------------------------ |
| `Module_Receiving/Settings/ReceivingSettingsKeys.cs`          | All setting key string constants           |
| `Module_Receiving/Settings/ReceivingSettingsDefaults.cs`      | Default values (string dict + typed dicts) |
| `Module_Receiving/Services/Service_ReceivingValidation.cs`    | Validation group consumers                 |
| `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` | BusinessRules/UiText consumers             |
| `Module_Receiving/Services/Service_ReceivingWorkflow.cs`      | BusinessRules/Defaults consumers (startup) |
| `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs`   | PartNumberPadding consumer                 |

---

## Notes On Accuracy Corrections Applied In This Update

This file was aligned to the current code analysis and defaults scan.
Notable corrections include:

- `RequirePoNumber` default corrected to `false`
- `AutoSaveIntervalSeconds` default corrected to `300`
- `SavePackageTypeAsDefault` default corrected to `false`
- `ShowReviewTableByDefault` default corrected to `false`
- `DefaultReceivingMode` default corrected to `Guided`
- `DefaultLocation` default corrected to `RECV`
- `AutoSave*` and `Integrations.*` sections clarified where no active consumer was confirmed

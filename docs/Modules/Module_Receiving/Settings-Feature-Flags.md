# Module_Receiving — Settings Feature Flags & Behaviour Impact

Last Updated: 2026-03-21

This document maps every `ReceivingSettingsKeys` setting to the exact runtime behaviour it
controls. Settings are consumed by `Module_Settings_Receiving` (storage/defaults) and read
at runtime inside `Module_Receiving`. Code is the source of truth.

---

## Summary

`Module_Settings_Receiving` **does** configure behaviour in `Module_Receiving` without a code
change. Settings are read at startup (via `LoadSettingsAsync`) and at the point of use
(validation calls, padding rules load). Changing a setting in the Settings UI immediately
affects the next receiving workflow session; no restart required.

---

## Group: Validation (`Receiving.Validation.*`)

Settings consumed directly by **`Service_ReceivingValidation`**.

| Setting Key | Type | Default | When `true` / non-zero | When `false` / zero |
|---|---|---|---|---|
| `Validation.RequirePoNumber` | bool | `true` | PO Number field is mandatory; blank value returns error `"PO number is required"` | Blank PO Number is silently accepted |
| `Validation.RequireQuantity` | bool | `true` | Quantity field is mandatory | Blank quantity allowed |
| `Validation.RequireHeatLot` | bool | `false` | Heat/Lot field is mandatory; blank returns error `"Heat/Lot number is required"` | Heat/Lot is optional; blank auto-filled with `"Nothing Entered"` |
| `Validation.AllowNegativeQuantity` | bool | `false` | Negative and zero quantities allowed; only `0` exactly is rejected | Only positive quantities accepted; `<= 0` returns error |
| `Validation.ValidatePoExists` | bool | `true` | PO number is looked up in Infor Visual; not-found returns error `"PO not found"` | PO lookup skipped; any typed PO number is accepted |
| `Validation.ValidatePartExists` | bool | `true` | Part ID is validated against Infor Visual; not-found returns error `"Part not found"` | Part lookup skipped |
| `Validation.WarnOnQuantityExceedsPo` | bool | `true` | Warning dialog shown when total qty > PO ordered qty | No warning; user can over-receive without prompt |
| `Validation.WarnOnSameDayReceiving` | bool | `true` | Warning shown if the same part was already received today (Infor Visual query) | Same-day check skipped |
| `Validation.MinLoadCount` | int | `1` | Minimum number of loads enforced; below-min shows `"Number of loads must be at least {n}"` | N/A |
| `Validation.MaxLoadCount` | int | `99` | Maximum number of loads enforced; above-max shows `"Number of loads cannot exceed {n}"` | N/A |
| `Validation.MinQuantity` | int | `0` | Lower bound for weight/quantity range check | N/A |
| `Validation.MaxQuantity` | int | `999999` | Upper bound for weight/quantity range check | N/A |

**Consuming code:** `Service_ReceivingValidation.cs` — `ValidatePONumber`, `ValidateNumberOfLoads`,
`ValidateWeightQuantity`, `ValidateHeatLotNumber`, `ValidateAgainstPOQuantityAsync`,
`CheckSameDayReceivingAsync`

---

## Group: Business Rules (`Receiving.BusinessRules.*`)

Settings consumed by **`ViewModel_Receiving_Workflow`** (loaded in `LoadSettingsAsync`) and
**`Service_ReceivingWorkflow`** (startup logic).

| Setting Key | Type | Default | Behaviour |
|---|---|---|---|
| `BusinessRules.AutoSaveEnabled` | bool | `false` | When `true`: auto-save timer is activated; session is periodically saved to local XLS. When `false`: user must save manually. |
| `BusinessRules.AutoSaveIntervalSeconds` | int | `60` | Interval in seconds between auto-save writes. Only relevant when `AutoSaveEnabled = true`. |
| `BusinessRules.DefaultModeOnStartup` | string | `""` | If set to `"guided"`, `"manual"`, or `"edit"`: the ModeSelection screen is bypassed and the workflow begins at that step. Empty string: ModeSelection is shown normally. |
| `BusinessRules.RememberLastMode` | bool | `false` | When `true`: the last-used mode is stored between sessions. On next launch, `Defaults.DefaultReceivingMode` is read and that mode is applied as if it were `DefaultModeOnStartup`. |
| `BusinessRules.ConfirmModeChange` | bool | `true` | When `true`: a confirmation dialog is shown before switching between Guided/Manual/Edit modes mid-session. When `false`: mode switch is immediate. |
| `BusinessRules.AutoFillHeatLotEnabled` | bool | `true` | When `true`: the Auto-Fill button is shown on the HeatLot step, allowing the user to copy the first load's value to all loads. When `false`: button is hidden. |
| `BusinessRules.SavePackageTypeAsDefault` | bool | `true` | When `true`: the "Save as default" checkbox is shown on the PackageType step; saving stores the selection to `Defaults.DefaultReceivingMode`. When `false`: checkbox is hidden. |
| `BusinessRules.ShowReviewTableByDefault` | bool | `true` | When `true`: Review step opens in table view. When `false`: opens in single-entry card view. |
| `BusinessRules.AllowEditAfterSave` | bool | `true` | When `true`: an Edit button is visible on the Complete screen (XAML-bound via `BooleanToVisibilityConverter` on `CanEditAfterSave`). When `false`: button is hidden; user cannot return to edit a completed session. |

**Consuming code:** `ViewModel_Receiving_Workflow.cs` — `LoadSettingsAsync` (lines 246–248
for `AllowEditAfterSave`); `Service_ReceivingWorkflow.cs` — startup/mode-select logic.

---

## Group: Defaults (`Receiving.Defaults.*`)

Settings used to **pre-populate fields** or determine the starting state of the workflow.

| Setting Key | Type | Default | Behaviour |
|---|---|---|---|
| `Defaults.DefaultReceivingMode` | string | `""` | Stores the last-used mode when `BusinessRules.RememberLastMode = true`. Read on startup to determine which step to start at. |
| `Defaults.DefaultLocation` | string | `""` | Pre-fills the Location field on the LoadEntry step. If blank and no location is entered, the step cannot be advanced (exit guard fires). |
| `Defaults.XlsSaveLocation` | string | `""` | Path where the local XLS file is written on save. Displayed on the Complete screen as the "Local XLS" path label. **Currently defined but not yet consumed by any ViewModel or Service** — behaviour pending implementation. |

---

## Group: Part Number Padding (`Receiving.PartNumberPadding.*`)

Settings consumed by **`View_Receiving_ManualEntry`** (code-behind, loaded in `LoadPaddingSettingsAsync`).

| Setting Key | Type | Default | Behaviour |
|---|---|---|---|
| `PartNumberPadding.Enabled` | bool | `false` | When `true`: padding rules are applied to each Part ID typed in the Manual Entry grid. When `false`: Part IDs are used as-is. |
| `PartNumberPadding.RulesJson` | string | `"[]"` | JSON array of `Model_PartNumberPrefixRule` objects. Each rule specifies a prefix match and the zero-padding length to apply. Rules are applied at the time of part number entry in the ManualEntry grid. Deserialization failure silently falls back to no-padding. |

**Consuming code:** `View_Receiving_ManualEntry.xaml.cs` — `LoadPaddingSettingsAsync()`, called on
`Loaded` event.

---

## Group: UI Text (`Receiving.UiText.*`)

These settings **do not control workflow behaviour**. They are localised string values that
replace hard-coded labels in ViewModels and Views. Changing them in settings updates the
displayed text only — no logic changes.

**Examples:** Button labels (`WorkflowHelp`, `WorkflowNext`, `WorkflowBack`), column headers,
step titles, confirmation dialog strings, completion screen labels.

**Consuming code:** `ViewModel_Receiving_Workflow.cs` — `LoadSettingsAsync` (reads all UiText
keys into matching `string` properties on the ViewModel, which XAML binds to via `x:Bind`).

---

## Group: Integrations (`Receiving.Integrations.*`)

**Status: Defined in `ReceivingSettingsKeys` and `ReceivingSettingsDefaults`. Not yet consumed
by any ViewModel or Service.** These are placeholders for future ERP sync behaviour.

| Setting Key | Type | Default | Intended Behaviour (not yet active) |
|---|---|---|---|
| `Integrations.ErpSyncEnabled` | bool | `true` | Enable/disable Infor Visual sync on save |
| `Integrations.AutoPullPoDataEnabled` | bool | `true` | Auto-fetch PO data from Visual on PO number entry |
| `Integrations.AutoPullPartDataEnabled` | bool | `true` | Auto-fetch part data from Visual on part ID entry |
| `Integrations.SyncToInforVisual` | bool | `false` | Push received quantities back to Visual |
| `Integrations.RetryFailedSyncs` | bool | `true` | Retry failed Visual sync on next save attempt |
| `Integrations.ErpConnectionTimeout` | int | `30` | Seconds before Infor Visual connection attempt times out |
| `Integrations.MaxSyncRetries` | int | `3` | Max number of retry attempts for failed syncs |

---

## Setting Group → Consuming Component Map

| Group | Consumer | Read At |
|---|---|---|
| `Validation.*` | `Service_ReceivingValidation` | At the moment each field is validated (synchronous `.GetAwaiter().GetResult()` call) |
| `BusinessRules.*` | `ViewModel_Receiving_Workflow`, `Service_ReceivingWorkflow` | On startup / `LoadSettingsAsync` |
| `Defaults.*` | `Service_ReceivingWorkflow` (mode), `ViewModel_Receiving_Workflow` (location), `View_Receiving_Workflow.xaml` (XLS path label) | On startup / step entry |
| `PartNumberPadding.*` | `View_Receiving_ManualEntry` (code-behind) | On `Loaded` event of ManualEntry view |
| `UiText.*` | `ViewModel_Receiving_Workflow` | On startup / `LoadSettingsAsync` |
| `Integrations.*` | *(none yet)* | N/A — not yet wired |

---

## Settings That Create Conditional Workflows

The following settings change the workflow path — not just UI appearance:

| Setting | Condition | Workflow Effect |
|---|---|---|
| `BusinessRules.DefaultModeOnStartup` | = `"guided"` / `"manual"` / `"edit"` | ModeSelection screen skipped; workflow starts at that step |
| `BusinessRules.RememberLastMode` | = `true` | Last-used mode persisted; `Defaults.DefaultReceivingMode` drives startup step |
| `BusinessRules.AllowEditAfterSave` | = `false` | Edit button removed from Complete screen; completed sessions cannot be reopened |
| `BusinessRules.ConfirmModeChange` | = `false` | No confirmation dialog on mid-session mode switch; transition is instant |
| `BusinessRules.AutoFillHeatLotEnabled` | = `false` | Auto-fill button not shown on HeatLot step; each load must have its heat/lot typed individually |
| `BusinessRules.SavePackageTypeAsDefault` | = `false` | "Save as default" checkbox not shown on PackageType step |
| `BusinessRules.ShowReviewTableByDefault` | = `false` | Review step opens in single-card view instead of table view |
| `Validation.RequirePoNumber` | = `false` | PO Number field becomes optional; blank is accepted |
| `Validation.RequireHeatLot` | = `true` | HeatLot step blocks advancement if all loads have blank heat/lot values |
| `Validation.AllowNegativeQuantity` | = `true` | Negative weight/quantity entries accepted; only exact zero is rejected |
| `Validation.WarnOnQuantityExceedsPo` | = `false` | Over-receive silently accepted; Infor Visual over-receive warning suppressed |
| `Validation.WarnOnSameDayReceiving` | = `false` | Same-day duplicate receive silently accepted |
| `Validation.ValidatePoExists` | = `false` | PO number not looked up in Infor Visual; any string accepted |
| `Defaults.DefaultLocation` | non-empty | Location field pre-populated; LoadEntry step advancement unblocked without user input |
| `PartNumberPadding.Enabled` | = `true` | Part IDs in Manual Entry grid are transformed by padding rules before being stored |

---

## Source Files

| File | Role |
|---|---|
| `Module_Receiving/Settings/ReceivingSettingsKeys.cs` | All setting key string constants |
| `Module_Receiving/Settings/ReceivingSettingsDefaults.cs` | Default values (string dict + typed dicts) |
| `Module_Receiving/Services/Service_ReceivingValidation.cs` | Validation group consumers |
| `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` | BusinessRules/UiText consumers |
| `Module_Receiving/Services/Service_ReceivingWorkflow.cs` | BusinessRules/Defaults consumers (startup) |
| `Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs` | PartNumberPadding consumer |

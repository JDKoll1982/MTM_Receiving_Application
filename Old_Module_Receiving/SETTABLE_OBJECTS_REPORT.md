# Module Receiving — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Receiving`.

## Settable objects

## Dev/Test

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `AppSettings:UseInforVisualMockData` | boolean | System | ToggleSwitch | If enabled, module uses mock PO/part data instead of querying Infor Visual. | Developer/QA only | `appsettings.json`; used in `ViewModel_Receiving_POEntry` | Keep enabled only in non-production; add environment guardrails. |
| `AppSettings:DefaultMockPONumber` | string | System | TextBox | Default PO number used when mock mode is enabled (fallback currently `PO-066868`). | Developer/QA only | `appsettings.json`; hardcoded fallback in viewmodels | Eliminate hardcoded fallback; enforce config presence in mock mode. |

## UI/UX

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Receiving:MockAutoLoadDelayMs` | int | System | NumberBox | Delay before auto-loading mock data (currently 500ms). | Developer | `ViewModel_Receiving_POEntry.InitializeAsync` | Prefer event-based UI sequencing; if delay remains, externalize. |
| `Receiving:ManualEntryGridSelectionDelayMs` | int | System | NumberBox | Delay to allow grid selection/render (currently 100ms). | Developer | `View_Receiving_ManualEntry.xaml.cs` | Prefer awaited UI state; externalize if different devices need tuning. |

## User Preferences

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Receiving:DefaultReceivingMode` | string | User | RadioButtons (Guided/Manual/Edit) | Default workflow mode selection (`guided`, `manual`, `edit`). | User (self) | Stored on user/session model; compared in `ViewModel_Receiving_ModeSelection` | Prefer enum-backed values and centralized validation/normalization. |

## Business Rules

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Receiving:PartPrefixToPackageTypeMapping` | string | System | DataGrid editor (Prefix → PackageType) | Maps part prefixes to package type labels (e.g., `MCC`→`Coils`, `MMF`→`Sheets`, else `Skids`). | Admin (Operations) | `ViewModel_Receiving_POEntry` | Move mapping to a DB-driven table or configuration so operations can adjust without code changes. |
| `Receiving:PackageTypeLabels` | string | System | TextBox / ComboBox | Package type labels used in UI/reporting (`Coils`, `Sheets`, `Skids`). | Admin (Operations) | `ViewModel_Receiving_POEntry` | Externalize to config/DB to support renames/localization. |

## ERP Integration

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `InforVisual:DefaultUom` | string | System | TextBox | Default unit of measure (`EA`) used when ERP data is missing. | Developer | `Module_Receiving/Models/Model_InforVisualPart` | Prefer reading from ERP; default only when truly necessary. |

## Hardcoded values that should not be hardcoded (high priority)

- Part-prefix package type mapping (`MCC`/`MMF` → labels) should be data-driven.
- Mock-mode PO fallback (`PO-066868`) should be config-only.

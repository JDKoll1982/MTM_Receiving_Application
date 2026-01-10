# Module Routing — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Routing`.

## Settable objects

## File I/O

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `RoutingModule:CsvExportPath` | string | System | TextBox + FolderPicker | Network CSV export directory (UNC path). | Admin (Operations) / IT | `appsettings.json` | Use per-environment configuration; validate access permissions at startup. |
| `RoutingModule:LocalCsvExportPath` | string | System | TextBox + FolderPicker | Local fallback CSV export directory (supports `%APPDATA%`). | Admin (Operations) / IT | `appsettings.json` | Ensure environment variable expansion and directory creation are consistent. |
| `Routing:ResetCsvFilePaths` | string | System | TextBox + FolderPicker | Paths used when resetting CSV files. | Admin (Operations) / IT | `RoutingService.ResetCsvFileAsync` | Ensure it uses the same canonical config keys as export. |
| `RoutingModule:CsvExportPath:Network` | string | System | No UI (developer migration) | Alternate key shape used by code for network export path. | Developer | `Constant_RoutingConfiguration.CsvExportPathNetwork` | Align configuration schema: either update keys or update code to one canonical layout. |
| `RoutingModule:CsvExportPath:Local` | string | System | No UI (developer migration) | Alternate key shape used by code for local export path. | Developer | `Constant_RoutingConfiguration.CsvExportPathLocal` | Align configuration schema: either update keys or update code to one canonical layout. |

## UI/UX

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `RoutingModule:DefaultMode` | string | System | ComboBox | Default mode (`WIZARD`, `MANUAL`, `EDIT`). | Admin (Operations) | `appsettings.json` | Standardize casing/enum usage; avoid string comparisons. |
| `RoutingModule:PersonalizationThreshold` | int | System | NumberBox | Threshold for UI personalization behaviors (currently 20). | Admin (Operations) | `appsettings.json` | Document behavior; ensure limits and validation. |
| `RoutingModule:QuickAddButtonCount` | int | System | NumberBox | Quick-add recipient button count (currently 5). | Admin (Operations) | `appsettings.json` | Externalize and validate range to prevent layout issues. |
| `RoutingModule:HistoryPageSize` | int | System | NumberBox | Default history page size (currently 100). | Admin (Operations) | `appsettings.json` | Externalize; tune for large datasets. |

## Business Rules

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `RoutingModule:EnableValidation` | boolean | System | ToggleSwitch | Default validation toggle for Infor Visual checks. | Admin (Operations) | `appsettings.json` + user preferences model | Allow admin-defined default; user preference may override. |
| `Routing:DefaultDuplicateDetectionHours` | int | System | NumberBox | Default duplicate label detection window (currently 24 hours). | Admin (Operations) | `Constant_RoutingConfiguration.DefaultDuplicateDetectionHours` + DAO hardcode | Externalize and enforce consistent bounds. |
| `Routing:DuplicateDetectionMaxHours` | int | System | NumberBox | Maximum cap for duplicate detection window (currently 168 hours). | Admin (Operations) | `Dao_RoutingLabel.CheckDuplicateAsync` | Externalize; document rationale. |
| `Routing:DuplicateDetectionMinHours` | int | System | NumberBox | Minimum hours for duplicate detection window (currently 0). | Admin (Operations) | `Dao_RoutingLabel.CheckDuplicateAsync` | Externalize; document rationale. |

## Resilience

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `RoutingModule:CsvRetryAttempts` | int | System | NumberBox | Retry attempts for CSV writes (currently 3). | Developer/IT | `appsettings.json` | Standardize with `Constant_RoutingConfiguration` key names (see note below). |
| `RoutingModule:CsvRetryDelayMs` | int | System | NumberBox | Retry delay for CSV writes (currently 500ms). | Developer/IT | `appsettings.json` | Standardize with `Constant_RoutingConfiguration` key names (see note below). |
| `RoutingModule:CsvRetry:MaxAttempts` | int | System | No UI (developer migration) | Alternate key shape used by code for retry attempts. | Developer | `Constant_RoutingConfiguration.CsvRetryMaxAttempts` | Align configuration schema. |
| `RoutingModule:CsvRetry:DelayMs` | int | System | No UI (developer migration) | Alternate key shape used by code for retry delay. | Developer | `Constant_RoutingConfiguration.CsvRetryDelayMs` | Align configuration schema. |

## User Preferences

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `RoutingUserPreference:DefaultMode` | string | User | RadioButtons (Wizard/Manual/Edit) | Default mode stored for a user (currently defaults to `WIZARD`). | User (self) | `Model_RoutingUserPreference.DefaultMode` | Prefer enum + validation; allow admin default via config. |
| `RoutingUserPreference:EnableValidation` | boolean | User | ToggleSwitch | User-specific validation toggle (defaults true). | User (self) | `Model_RoutingUserPreference.EnableValidation` | Prefer admin default + user override. |

## ERP Integration

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `InforVisual:SiteId` | string | System | TextBox | Warehouse/site filter used in Visual queries (currently hardcoded `002`). | IT/DevOps only | `Dao_InforVisualPO` and other Visual integrations | Centralize as single config and remove scattered literals. |

## Dev/Test

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `AppSettings:UseInforVisualMockData` | boolean | System | ToggleSwitch | Controls mock data behavior in wizard initialization. | Developer/QA only | `RoutingWizardStep1ViewModel` | Keep disabled in production. |
| `AppSettings:DefaultMockPONumber` | string | System | TextBox | Default PO for mock mode (fallback hardcoded `PO-066868`). | Developer/QA only | `RoutingWizardStep1ViewModel` | Eliminate hardcoded fallback; require config in mock mode. |
| `Routing:MockAutoValidateDelayMs` | int | System | NumberBox | Delay before auto-validating mock PO (currently 500ms). | Developer | `RoutingWizardStep1ViewModel.InitializeAsync` | Prefer event-based sequencing; externalize if needed. |

## Notes

- `RoutingService` currently reads configuration using *two different key layouts* (flat keys in `appsettings.json` vs nested keys in `Constant_RoutingConfiguration`). Consolidating these will reduce config drift.

## Hardcoded values that should not be hardcoded (high priority)

- Infor Visual warehouse/site `"002"` should be a single config value.
- Duplicate detection window defaults and caps should be configurable.
- Remove hardcoded mock fallback `PO-066868`.

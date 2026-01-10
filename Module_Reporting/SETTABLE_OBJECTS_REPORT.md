# Module Reporting — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Reporting`.

## Settable objects

## File I/O

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Reporting:ExportFolderPath` | string | System | TextBox + FolderPicker | Default export folder (currently `%APPDATA%\\MTM_Receiving_Application\\Reports`). | Admin (Operations) / IT | `Service_Reporting.ExportToCSVAsync` | Move to configuration (appsettings or DB) so locations can be redirected. |
| `Reporting:FileNameTemplate` | string | System | TextBox | Export file naming pattern (currently `EoD_{moduleName}_{timestamp}.csv`). | Admin (Operations) | `Service_Reporting.ExportToCSVAsync` | Externalize if naming conventions differ by department. |
| `Reporting:TimestampFormat` | string | System | TextBox | Timestamp format used in filenames (currently `yyyyMMdd_HHmmss`). | Admin (Operations) | `Service_Reporting.ExportToCSVAsync` | Externalize if downstream systems require different format. |

## CSV

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Reporting:CsvDateFormat` | string | System | TextBox | Date format written to CSV (currently `yyyy-MM-dd`). | Admin (Operations) | `Service_Reporting.ExportToCSVAsync` | Externalize if spreadsheets/import pipelines require different format. |
| `Reporting:CsvHeadersByModule` | string | System | DataGrid editor (Module → Header/Columns) | Hardcoded CSV headers and column ordering per module (`receiving`, `dunnage`, `routing`, `volvo`). | Admin (Operations) | `Service_Reporting.ExportToCSVAsync` | Consider module-specific templates/config or reuse a shared CSV exporter service. |

## Email / UX

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Reporting:EmailTableStyles` | string | System | TextBox (multiline) | HTML styles/colors used for email formatting (e.g., header background `#f0f0f0`). | Admin (Operations) | `Service_Reporting.FormatForEmailAsync` | Externalize into templates or allow theme override; keep safe allowlist for HTML. |

## Business Rules

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Reporting:PONormalizationPrefix` | string | System | TextBox | PO normalization behavior uses `PO-` prefixing and padding rules. | Admin (Operations) | `Service_Reporting` | Externalize if PO formats vary; document rules. |

## Maintainability (Developer-only)

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Reporting:SupportedModuleNames` | string | System | No UI (internal constants) | Module name strings used in switches (`receiving`, `dunnage`, `routing`, `volvo`). | Developer | `Service_Reporting` | Prefer enums/central constants to avoid drift across app. |

## Hardcoded values that should not be hardcoded (high priority)

- Export path and naming convention should be configurable.
- CSV headers/orderings should be configurable per module if reporting consumers change.

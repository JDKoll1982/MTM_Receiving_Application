# Module Reporting — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Reporting`.

## Settable objects

## File I/O

Spreadsheet export workflow has been removed from Reporting. No CSV export file path, filename template, or CSV formatting settable objects are currently active in `Module_Reporting`.

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

- Keep email table styling and module labels externally configurable when reporting customization is required.

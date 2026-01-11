# Module Volvo — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Volvo`.

## Settable objects

## Email (DB-driven)

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `settings_module_volvo.email_to_recipients` | string | System | DataGrid editor (Name + Email) | JSON array of primary email recipients (`name` + `email`). | Admin (Volvo) / IT | `Database/Migrations/010_settings_module_volvo_initial_data.sql`; table `settings_module_volvo` | Keep as DB-driven setting; restrict who can change; validate JSON shape. |
| `settings_module_volvo.email_cc_recipients` | string | System | DataGrid editor (Name + Email) | JSON array of CC email recipients (`name` + `email`). | Admin (Volvo) / IT | `Database/Migrations/010_settings_module_volvo_initial_data.sql`; table `settings_module_volvo` | Keep DB-driven; restrict and validate. |
| `settings_module_volvo.email_subject_template` | string | System | TextBox | Email subject template with variables: `{Date}`, `{Number}`, `{EmployeeNumber}`. | Admin (Volvo) | `Database/Migrations/010_settings_module_volvo_initial_data.sql`; table `settings_module_volvo` | Validate allowed tokens; consider preview UI. |
| `settings_module_volvo.email_greeting` | string | System | TextBox | Email greeting text. | Admin (Volvo) | `Database/Migrations/010_settings_module_volvo_initial_data.sql`; table `settings_module_volvo` | Keep DB-driven; allow customization per shift/time if needed. |

## UI/UX

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Volvo:HistoryStatusOptions` | string | System | ComboBox | Status filter options list (`All`, `Pending PO`, `Completed`). | Admin (Volvo) | `ViewModel_Volvo_History` | If these values align to backend statuses, define centrally and avoid string drift. |

## Hardcoded values that should not be hardcoded (high priority)

- Email recipients and templates should remain **data-driven** (they already are via `settings_module_volvo`), and should not appear in code.

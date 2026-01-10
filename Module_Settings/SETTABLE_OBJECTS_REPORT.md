# Module Settings — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Settings`.

## Settable objects

## User Preferences

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `UserPreferences:DefaultMode` | string | User | ComboBox | Default top-level module selection (placeholder currently uses `Receiving`). | User (self) / Admin default | `Service_UserPreferences.GetLatestUserPreferenceAsync` (placeholder mapping) | Replace placeholder defaults with DB-driven user preference model. |
| `UserPreferences:DefaultReceivingMode` | string | User | RadioButtons (Guided/Manual/Edit) | Default receiving workflow mode (placeholder uses `Guided`). | User (self) / Admin default | `Service_UserPreferences.GetLatestUserPreferenceAsync` | Replace placeholder defaults with stored preference; normalize/case. |
| `UserPreferences:DefaultDunnageMode` | string | User | RadioButtons (Guided/Manual/Edit) | Default dunnage workflow mode (placeholder uses `Types`). | User (self) / Admin default | `Service_UserPreferences.GetLatestUserPreferenceAsync` | Replace placeholder defaults with stored preference; define allowed values. |

## Hardcoded values that should not be hardcoded (high priority)

- The defaults in `Service_UserPreferences` are explicitly marked as placeholders; they should come from persisted preferences (DB) and/or admin-configured defaults.

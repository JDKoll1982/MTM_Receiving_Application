# Module Dunnage — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Dunnage`.

## Settable objects

## User Preferences

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Dunnage:DefaultDunnageMode` | string | User | RadioButtons (Guided/Manual/Edit) | Default workflow mode (`guided`, `manual`, `edit`). | User (self) | Compared in `ViewModel_Dunnage_ModeSelectionViewModel` via session user | Prefer enum-backed values; centralize allowed values and normalization. |

## UI/UX

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Dunnage:ManualEntryGridSelectionDelayMs` | int | System | NumberBox | Delay to allow grid selection/render (currently 100ms). | Developer | `View_Dunnage_ManualEntryView.xaml.cs` | Prefer awaited UI state; if delay remains, externalize for device tuning. |

## Hardcoded values that should not be hardcoded (high priority)

- User preference mode discriminator strings (`guided`/`manual`/`edit`) should be centralized (ideally enum + persisted string mapping).

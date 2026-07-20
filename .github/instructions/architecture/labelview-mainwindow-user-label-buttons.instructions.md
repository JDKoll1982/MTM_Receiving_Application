---
applyTo: "MainWindow.xaml,MainWindow.xaml.cs,Module_Settings.*/*.{cs,xaml},Module_*/Services/*UserLabelSettings*.cs,Module_Settings.Core/Data/Dao_SettingsCoreUser.cs,Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/sp_SettingsCore.sql"
description: "MainWindow footer label-button behavior: user-scoped label path persistence, launch flow, edge cases, and extension rules."
---

# MainWindow Label Buttons

## Purpose

Keep the footer label-button feature consistent while adding future user-defined buttons.

## Current Buttons

- MainWindow footer has 4 fixed buttons: Receiving, Mini-Receiving, Dunnage, Volvo.
- Click handlers are in MainWindow.xaml.cs and all route through OpenLabelAsync.

## Persistence Model

- Label paths used by MainWindow are user-scoped settings in MySQL settings_personal.
- Read/write stack:
  - MainWindow -> IService_*UserLabelSettings -> Dao_SettingsCoreUser
  - DAO uses stored procedures:
    - sp_SettingsCore_User_GetByKey
    - sp_SettingsCore_User_Upsert
- Upsert key is (user_id, category, setting_key).

## User Label Keys

- Receiving:
  - category: Receiving.UserLabels
  - keys: ReceivingLabelPath, MiniReceivingLabelPath
- Dunnage:
  - category: Dunnage.UserLabels
  - key: DunnageLabelPath
- Volvo:
  - category: Volvo.UserLabels
  - key: VolvoLabelPath

## Launch Workflow

1. Validate LabelView executable via IService_LabelViewLauncher.ResolveExecutablePathAsync.
2. Get user label path from the module user-label service.
3. Validate label path exists and has .lbl extension.
4. Launch through Windows shell via Service_LabelViewLauncher.LaunchLabelAsync.
5. Show success status.

## Edge Cases

- No active user session:
  - Get returns empty string.
  - Save is skipped.
  - Service logs warning.
- Missing or invalid LabelView executable:
  - Redirect to View_Settings_LabelViewExecutable.
- Missing/invalid .lbl path:
  - Redirect to module-specific label-path settings page.
- Launch failure after validation:
  - Error handler is invoked with DAO result.
- Empty path is allowed in settings pages; runtime open then redirects to settings.

## Important Behavior Split

- MainWindow footer buttons use user-scoped keys (Receiving.UserLabels, Dunnage.UserLabels, Volvo.UserLabels).
- Some workflow-level open-label commands still use system-scoped keys (Receiving.Labels.*, Dunnage.Labels.*, Volvo.Labels.*) through IService_*Settings.
- Do not assume both surfaces read the same storage keys.

## Rules For Adding Dynamic Buttons

- Keep launch path centralized in MainWindow OpenLabelAsync or an extracted shared launcher facade.
- Persist per-user button config in settings_personal (user scope), not system scope.
- Store only normalized absolute file path strings; validate on save and before launch.
- Keep per-button metadata minimal: id, display name, icon key, category, setting_key.
- Reuse existing module label settings pages or add a unified label-button settings page.
- Never bypass services from UI; keep View -> service -> DAO flow.
- Use stored procedures only for MySQL settings writes.

## File Anchors

- MainWindow launch handlers: MainWindow.xaml.cs
- Label launcher + validation: Module_Core/Services/Service_LabelViewLauncher.cs
- User label services:
  - Module_Receiving/Services/Service_ReceivingUserLabelSettings.cs
  - Module_Dunnage/Services/Service_DunnageUserLabelSettings.cs
  - Module_Volvo/Services/Service_VolvoUserLabelSettings.cs
- DAO + SQL procedures:
  - Module_Settings.Core/Data/Dao_SettingsCoreUser.cs
  - Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/sp_SettingsCore.sql
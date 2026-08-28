<!-- 
[DOC-META-START]
- File Name: plan-mainWindowButtonRevamp.prompt.md
- Description: Plan to replace the fixed 4-footer label buttons with a user-managed, data-driven button list stored per user in settings_personal, with an expanded LabelView settings page and MainWindow overflow flyout behavior.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 1: ## Plan: MainWindow Dynamic Label Buttons
- Critical Notes: Single-release delivery; keep launch validation via Service_LabelViewLauncher and route all label launches through MainWindow OpenLabelAsync without duplicate logic.
[DOC-META-END]
-->

## Plan: MainWindow Dynamic Label Buttons

Replace the fixed 4-footer buttons with a user-managed, data-driven button list stored per user in settings_personal, managed from an expanded LabelView settings page, and shown in MainWindow with overflow flyout behavior. Preserve current launch validation via Service_LabelViewLauncher and remove module-specific label-path pages in one release.

**Steps**
1. Phase 1 - Settings Model and Contracts
2. Define a new user-scoped setting for button config JSON under a single category/key in Settings.Core metadata registry (scope user, type string/json, default empty list). *blocks 2-6*
3. Add strongly typed model(s) for button config (id, displayName, tooltip, iconKey via MaterialIconKind name, accent preset enum, moduleTag, labelPath, sortOrder, isEnabled). *depends on 2*
4. Add a dedicated service for user button config CRUD + reorder + duplicate + enable/disable using Dao_SettingsCoreUser Upsert/GetByKey. *depends on 2-3*
5. Keep executable path storage unchanged in existing system-scoped CoreSettingsKeys.LabelView.ExecutablePath.

6. Phase 2 - Settings UI Consolidation
7. Extend View_Settings_LabelViewExecutable surface with a new section for custom buttons (list + add/edit/delete/duplicate/enable/disable + open folder + test/open actions). *depends on 4*
8. Reuse View_Dunnage_Control_IconPickerControl for icon selection in the add/edit workflow. *parallel with 7 once models exist*
9. Implement prompt-to-fix flow for invalid label paths (picker/dialog), with validation on save and on click. *depends on 7*
10. Remove/retire module-specific label-path pages from navigation hubs and settings headers, and remove their DI viewmodel/view registrations. *depends on 7-9*

11. Phase 3 - MainWindow Data-Driven Footer
12. Replace hardcoded footer button stack with an items-based rendering of user buttons plus overflow flyout/menu behavior.
13. Default click: open label. Secondary actions via context menu: open folder, edit, delete, duplicate, enable/disable. *depends on 12*
14. Route all label launches through existing MainWindow OpenLabelAsync + Service_LabelViewLauncher (no duplicate launch logic). *depends on 12*
15. Keep category/module tag for organization/filtering only (no hard requirement for path restrictions).

16. Phase 4 - Workflow and Cleanup
17. Update workflow-level references that currently redirect to module-specific label pages so they redirect to the unified LabelView settings page section when needed.
18. Remove obsolete interfaces/services/viewmodels for per-module user label path pages if no longer referenced.
19. Update docs/instruction references that mention the 4 fixed footer buttons or module-specific label-path settings pages.

20. Phase 5 - Verification
21. Add focused tests for serialization/deserialization, CRUD actions, ordering, and enable/disable behavior for button config service.
22. Add UI interaction checks for overflow behavior and context actions.
23. Validate edge cases: no active user session, missing LV.exe, invalid/missing .lbl path, deleted network file, disabled button click behavior.

**Relevant files**
- Main shell/footer behavior:
  - MainWindow.xaml
  - MainWindow.xaml.cs
- Launch/validation reuse:
  - Module_Core/Services/Service_LabelViewLauncher.cs
  - Module_Core/Contracts/Services/IService_LabelViewLauncher.cs
- Existing per-module user label services to retire/replace:
  - Module_Receiving/Services/Service_ReceivingUserLabelSettings.cs
  - Module_Dunnage/Services/Service_DunnageUserLabelSettings.cs
  - Module_Volvo/Services/Service_VolvoUserLabelSettings.cs
  - Module_Receiving/Contracts/IService_ReceivingUserLabelSettings.cs
  - Module_Dunnage/Contracts/IService_DunnageUserLabelSettings.cs
  - Module_Volvo/Contracts/IService_VolvoUserLabelSettings.cs
- Settings core persistence + metadata:
  - Module_Settings.Core/Data/Dao_SettingsCoreUser.cs
  - Module_Settings.Core/Services/GetSettingQueryHandler.cs
  - Module_Settings.Core/Services/SetSettingCommandHandler.cs
  - Module_Settings.Core/Services/Service_SettingsCoreFacade.cs
  - Module_Settings.Core/Models/Model_SettingsDefinition.cs
  - Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/sp_SettingsCore.sql
- Unified settings UI target:
  - Module_Settings.Core/Views/View_Settings_LabelViewExecutable.xaml
  - Module_Settings.Core/ViewModels/ViewModel_Settings_LabelViewExecutable.cs
- Icon picker reuse:
  - Module_Dunnage/Views/View_Dunnage_Control_IconPickerControl.xaml
  - Module_Dunnage/Views/View_Dunnage_Control_IconPickerControl.xaml.cs
- Settings navigation and removable pages:
  - Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs
  - Infrastructure/DependencyInjection/ModuleServicesExtensions.cs
  - Module_Settings.Receiving/Views/View_Settings_Receiving_LabelPaths.xaml
  - Module_Settings.Dunnage/Views/View_Settings_Dunnage_LabelPaths.xaml
  - Module_Settings.Volvo/Views/View_Settings_Volvo_LabelPaths.xaml
  - Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_CategoryHub.cs
  - Module_Settings.Dunnage/ViewModels/ViewModel_Settings_Dunnage_CategoryHub.cs
  - Module_Settings.Volvo/ViewModels/ViewModel_Settings_Volvo_NavigationHub.cs

**Verification**
1. Unit tests: button-config service CRUD, ordering, duplicate, enable/disable, and JSON validation.
2. Unit tests: invalid icon key/accent fallback handling.
3. Manual: from settings page add/edit/delete several buttons, verify persistence across app restart and user switch.
4. Manual: click open on valid/invalid paths; confirm prompt-to-fix flow and path picker behavior.
5. Manual: verify overflow flyout/menu when many buttons exist and context actions work.
6. Focused build/test on changed slices and impacted settings/navigation tests.

**Decisions**
- Unlimited custom buttons per user.
- Visibility is private per user.
- Stored fields include display name, icon key, tooltip, sort order, accent preset, module tag, path, enabled state.
- Actions include open label, open folder, edit, delete, duplicate, enable/disable.
- Validation on save and on click.
- Invalid path/executable handling uses prompt-to-fix behavior.
- Footer uses overflow flyout/menu pattern.
- Existing 4 buttons become part of a single data-driven list.
- No migration from old keys; start fresh.
- Single-release delivery.
- Keep current LabelView executable settings page and add custom-button management section there.
- Use View_Dunnage_Control_IconPickerControl for icon selection.
- Accent model uses enum preset (Default/Blue/Green/Gold/Red).
- Path policy: allow any existing local/UNC .lbl path.

**Further Considerations**
1. Seed behavior recommendation: first run for a user pre-populates 4 default buttons in-memory and writes on first save, even with no migration.
2. Deletion strategy recommendation: soft-retire module label-path pages from navigation first, then remove dead files in follow-up cleanup PR if you prefer lower risk.
3. Overflow UX recommendation: show top N pinned by sort order, move the rest to More flyout with same context menu actions.

## Task: Comprehensive Module Analysis & CopilotForms Synchronization

### Objective

Conduct a complete code analysis of `Module_Receiving` and `Module_Settings_Receiving` using Serena semantic tools, then update the CopilotForms configuration to reflect the actual codebase state.

### Phase 1: Code Analysis (Read-Only)

Using Serena tools, systematically extract and document:

**For Module_Receiving:**

1. All ViewModel classes and their public properties/commands
2. All Service interfaces and methods
3. All DAO classes and SQL operations
4. All Model/DTO structures
5. All XAML Views and their binding targets
6. Complete user workflow paths (entry point → exit point)
7. All validation rules and error scenarios
8. All data dependencies between layers

**For Module_Settings_Receiving:**

1. Settings ViewModels and configurable properties
2. Settings storage mechanisms and defaults
3. UI element types and their settings bindings
4. Validation constraints for each setting
5. Dependencies on Module_Receiving or other modules
6. Admin/user permission levels per setting

**For Each Workflow:**

- Identify all user actions (clicks, inputs, selections)
- Map data flow from UI → ViewModel → Service → DAO → Database
- Document all possible states and transitions
- List all error conditions and recovery paths
- Identify optional vs. mandatory steps
- Include error recovery and edge case workflows (not just happy path)

### Phase 2: Data Inventory

Create structured inventory of all data:

- Field names, types, nullable status, max lengths
- Validation rules (range, format, dependencies)
- Default values and initialization logic
- Master data sources (hardcoded lists, database lookups)
- Calculation formulas or computed properties

### Phase 3: CopilotForms Update

Update CopilotForms configuration at `docs/CopilotForms/data/copilot-forms.config.json` to include:

- **Forms Section:** Exact field list matching ViewModels, with correct types and validation
- **Fields Section:** All ViewModel properties with binding paths, types, constraints
- **Validation Section:** All rules discovered in source code (code is source of truth)
- **Workflows Section:** All user journeys including error recovery and edge cases
- **Enums Section:** All `Enum_*` types connected to both modules via Module_Core and Module_Reporting dependencies
- **Models Section:** All `Model_*` structures with field definitions

### Phase 4: UI Documentation (Noob-Friendly Format)

For each workflow, create HTML documentation matching CopilotForms UI format in `docs/CopilotForms/forms/`:

Each workflow document must include:

- What the user sees (screen names, button labels, field labels, visual hierarchy)
- What the user can do (clickable actions, input fields, optional vs. mandatory steps)
- What happens next (success state, confirmation messages, error messages)
- What data is required before proceeding
- Error recovery steps and alternative paths
- Visual grouping and control relationships

Format: HTML files rendered in CopilotForms UI (matching existing style in `docs/CopilotForms/assets/copilot-forms.css`)

### Execution Approach

**Step 1: Activate Serena**

```
Activate project: MTM_Receiving_Application
```

**Step 2: Read Architecture Memories**

```
read_memory("architectural_patterns")
read_memory("mvvm_guide")
read_memory("forbidden_practices")
read_memory("dao_best_practices")
read_memory("project_overview")
```

**Step 3: Module_Receiving Exploration**

```
get_symbols_overview of Module_Receiving/ViewModels/
find_symbol "ViewModel_Receiving_*" with depth=1 for each class
find_symbol "IService_*" in Module_Receiving/Contracts/Services/
get_symbols_overview of Module_Receiving/Data/
get_symbols_overview of Module_Receiving/Views/
find_symbol "Model_Receiving_*" in Module_Receiving/Models/
```

**Step 4: Module_Settings_Receiving Exploration**

```
(Repeat Step 3 pattern for settings module)
Also identify:
   - Admin-level vs. end-user settings
   - System configuration vs. user preferences
   - Default values and initialization
   - Permission/role-based access constraints
```

**Step 5: Module_Core and Module_Reporting Dependencies**

```
Trace all Enum_* types referenced by Module_Receiving and Module_Settings_Receiving
Document complete enum value sets and usage contexts
Map shared Services and Models from Module_Core
```

**Step 6: Workflow Mapping**

```
For each ViewModel found:
    - find_referencing_symbols to identify callers and command triggers
    - Read XAML bindings to understand UI mapping
    - Trace command execution path to service → DAO → database
    - Document state transitions and conditions
    - Map error handlers and recovery paths
    - Identify all user decision points and branches
```

**Step 7: Validation Rules Extraction**

```
For each ViewModel property and Service method:
    - Extract validation attributes and rules
    - Document constraints discovered in code
    - Map field-level vs. cross-field validations
    - Identify async validation (database lookups, checks)
    - Document conditional validations based on state
```

**Step 8: CopilotForms JSON Update**

```
Update docs/CopilotForms/data/copilot-forms.config.json with:
   - All discovered forms, fields, and validations
   - All workflows with decision points and error paths
   - Complete enum definitions with all values
   - All models with complete field definitions
   - Cross-references between related forms
```

**Step 9: UI Documentation Generation**

```
For each workflow, create HTML file in docs/CopilotForms/forms/:
   - File naming: <workflow-name>.html
   - Include visual mockup or description of screens involved
   - List all user actions and their outcomes
   - Show error messages and recovery steps
   - Document data requirements and validation feedback
   - Match CopilotForms CSS styling and layout
```

**Step 10: Validation Report**

```
Generate comparison report identifying:
   - Discrepancies between code and existing CopilotForms
   - Missing form definitions
   - Missing workflow documentation
   - Enum value mismatches
   - Validation rule differences (code vs. config)
```

### Deliverables

1. **copilot-forms.config.json** — Updated JSON with 100% of discovered forms, fields, validations, workflows, and enums
2. **Workflow HTML Documents** — Individual HTML files for each workflow in CopilotForms format
3. **Enum Reference Document** — All `Enum_*` types with complete value sets and usage
4. **Data Inventory CSV/JSON** — All fields, types, constraints, defaults, sources, validation rules
5. **Validation Discrepancy Report** — What differs between discovered code and current CopilotForms
6. **Assumptions Log** — All assumptions documented with rationale and confirmation status

### Success Criteria

- [ ] All ViewModels in both modules documented with complete property/command lists
- [ ] All user workflows traced from UI entry point through all branches (happy path, error paths, edge cases)
- [ ] CopilotForms config contains 100% of discovered fields with correct types, validation rules, and constraints
- [ ] Workflow documentation readable and usable by non-developer (HTML, visual, step-by-step)
- [ ] All `Enum_*` types from Module_Core and Module_Reporting dependencies documented with complete value sets
- [ ] Zero discrepancies between discovered code and CopilotForms definitions
- [ ] All validation rules in config match code-based validation (code is source of truth)
- [ ] Error recovery paths and edge cases documented in all workflows
- [ ] All assumptions confirmed by user before proceeding with implementation

---

## Clarifications Applied

1. ✅ **CopilotForms Location & Format** — Confirmed at `docs/CopilotForms/data/copilot-forms.config.json` (JSON format)
2. ✅ **Scope of "All Workflows"** — Include ALL scenarios: happy path, error recovery, edge cases, admin workflows
3. ✅ **Settings Module Scope** — Include both end-user settings AND admin-level configuration; document permission levels
4. ✅ **Data Sensitivity** — All fields safe to document (locally hosted, no PII exclusions needed)
5. ✅ **Validation Rules Source** — Code is source of truth; CopilotForms must match code behavior
6. ✅ **Enum Documentation** — Extract ALL `Enum_*` types connected to both modules via Module_Core and Module_Reporting
7. ✅ **UI Format for "Noob Mode"** — HTML files matching CopilotForms UI (CSS-styled, visual, step-by-step)

---

8. **Settings Persistence & Storage Mechanism** ✅ RESOLVED (user confirmation, 2026-03-21)
   - **Finding: Settings are persisted to the MySQL database — not JSON, INI, Windows Registry, or appSettings.json (user confirmed).**
   - **Key tables**: `settings_universal` (system-wide), `settings_personal` (per-user)
   - **Reset/restore**: `sp_Settings_System_ResetToDefault`, `sp_Settings_User_Reset`, `sp_Settings_User_ResetAll`
   - **Schemas/SPs**: `Database/Schemas/24_Table_settings_personal.sql`, `27_Table_settings_universal.sql`, `Database/StoredProcedures/Settings/sp_Settings_*`
   - *Supporting database file listing (user-provided evidence):*
     Database\00-Test
     Database\00-Test\templates
     Database\00-Test\templates\01-report-template.md
     Database\00-Test\templates\02-report-template.md
     Database\00-Test\templates\hardcode-report-template.md
     Database\00-Test\templates\sp-report-template.md
     Database\00-Test\templates\sp-report-unused-template.md
     Database\00-Test\templates\usage-report-template.md
     Database\00-Test\01-Generate-SP-TestData.ps1
     Database\00-Test\02-Test-StoredProcedures.ps1
     Database\00-Test\03-Analyze-SP-Usage.ps1
     Database\00-Test\04-Test-Receiving-LabelData-ClearToHistory.sql
     Database\00-Test\05-Test-Receiving-LabelData-Insert-FieldCoverage.sql
     Database\00-Test\06-Test-Dunnage-LabelData-InsertAndVerify.sql
     Database\00-Test\07-Test-Dunnage-LabelData-ClearToHistory.sql
     Database\00-Test\README.md
     Database\01-Deploy
     Database\01-Deploy\Deploy-Database-GUI-MAMP.ps1
     Database\01-Deploy\Deploy-Database-GUI-MySQL.ps1
     Database\01-Deploy\Deploy-Database-GUI-Workbench.ps1
     Database\01-Deploy\New-PC-Setup-Guide-Framework-Dependent.md
     Database\01-Deploy\New-PC-Setup-Guide.md
     Database\01-Deploy\Publish-App-GUI.ps1
     Database\01-Deploy\PublishAppScript.md
     Database\InforVisualScripts
     Database\InforVisualScripts\PowerShell_Scripts
     Database\InforVisualScripts\PowerShell_Scripts\Debug-POLinks.ps1
     Database\InforVisualScripts\PowerShell_Scripts\Search-POLineSpecs-GUI.ps1
     Database\InforVisualScripts\PowerShell_Scripts\Search-POLineSpecs.ps1
     Database\InforVisualScripts\PowerShell_Scripts\Test-POLineQuery.sql
     Database\InforVisualScripts\Queries
     Database\InforVisualScripts\Queries\01_GetPOWithParts.sql
     Database\InforVisualScripts\Queries\02_ValidatePONumber.sql
     Database\InforVisualScripts\Queries\03_GetPartByNumber.sql
     Database\InforVisualScripts\Queries\04_SearchPartsByDescription.sql
     Database\InforVisualScripts\Queries\05_GetOutsideServiceHistoryByPart.sql
     Database\InforVisualScripts\Queries\06_FuzzySearchPartsByID.sql
     Database\InforVisualScripts\Queries\07_FuzzySearchVendorsByName.sql
     Database\InforVisualScripts\Queries\08_GetOutsideServiceHistoryByVendor.sql
     Database\InforVisualScripts\Queries\09_GetDistinctPartsByVendor.sql
     Database\InforVisualScripts\Queries\10_GetOutsideServiceByVendorAndPart.sql
     Database\InforVisualScripts\Queries\11_FuzzySearchLocationsByWarehouse.sql
     Database\InforVisualScripts\README.md
     Database\InforVisualTest
     Database\InforVisualTest\01_GetPOWithParts.sql
     Database\InforVisualTest\02_ValidatePONumber.sql
     Database\InforVisualTest\03_GetPartByNumber.sql
     Database\InforVisualTest\04_SearchPartsByDescription.sql
     Database\InforVisualTest\README.md
     Database\OtherInformation
     Database\OtherInformation\Volvo Dunnage - Data Sheet.csv
     Database\Schemas
     Database\Schemas\00_Table_Bulk_Inventory_Transactions.sql
     Database\Schemas\00_Table_Settings_Core_Schema.sql
     Database\Schemas\01_Table_auth_departments.sql
     Database\Schemas\02_Table_auth_users.sql
     Database\Schemas\03_Table_auth_workstation_config.sql
     Database\Schemas\04_Table_dunnage_types.sql
     Database\Schemas\05_Table_dunnage_parts.sql
     Database\Schemas\06_Table_dunnage_history.sql
     Database\Schemas\07_Table_dunnage_requires_inventory.sql
     Database\Schemas\08_Table_dunnage_specs.sql
     Database\Schemas\09_Table_dunnage_custom_fields.sql
     Database\Schemas\09_Table_dunnage_non_po_entries.sql
     Database\Schemas\10_Table_receiving_history.sql
     Database\Schemas\11_Table_receiving_label_data.sql
     Database\Schemas\12_Table_receiving_package_type_mapping.sql
     Database\Schemas\13_Table_receiving_package_types.sql
     Database\Schemas\13_Table_Receiving_Quality_Holds_Schema.sql
     Database\Schemas\14_Table_reporting_scheduled_reports.sql
     Database\Schemas\21_Table_settings_activity.sql
     Database\Schemas\22_Table_settings_dunnage_personal.sql
     Database\Schemas\23_Table_settings_module_volvo.sql
     Database\Schemas\24_Table_settings_personal.sql
     Database\Schemas\25_Table_settings_personal_activity_log.sql
     Database\Schemas\27_Table_settings_universal.sql
     Database\Schemas\28_Table_volvo_label_data.sql
     Database\Schemas\29_Table_volvo_line_data.sql
     Database\Schemas\30_Table_volvo_masterdata.sql
     Database\Schemas\31_Table_dunnage_label_data.sql
     Database\Schemas\31_Table_volvo_label_history.sql
     Database\Schemas\31_Table_volvo_line_history.sql
     Database\Schemas\31_Table_volvo_part_components.sql
     Database\Schemas\32_Triggers_Module_Volvo.sql
     Database\Schemas\33_View_dunnage_history.sql
     Database\Schemas\34_View_receiving_history.sql
     Database\Schemas\36_View_volvo_history.sql
     Database\Schemas\37_View_volvo_label_data_history.sql
     Database\Schemas\38_Migration_receiving_label_queue_history_alignment.sql
     Database\Schemas\39_Migration_receiving_label_data_load_id_unique.sql
     Database\Schemas\41_Migration_dunnage_history_parity.sql
     Database\Schemas\42_Migration_volvo_line_location.sql
     Database\Schemas\99_Seed_SettingsCore_Roles.sql
     Database\Schemas\TABLE_NAME_MAPPING.md
     Database\Scripts
     Database\Scripts\Import-ReceivingHistory.ps1
     Database\Scripts\Migrate-AddIsNonPOItem.sql
     Database\Scripts\Migrate-ExpandReceivingLabelData.sql
     Database\Scripts\Preview-AllChanges.ps1
     Database\Scripts\Preview-SPRenames.ps1
     Database\Scripts\receiving_label_history_reconciliation.sql
     Database\Scripts\Rename-StoredProcedures.ps1
     Database\Scripts\Reorganize-StoredProcedures.ps1
     Database\Scripts\sp-rename-log.csv
     Database\Scripts\sp-reorganize-log.csv
     Database\Scripts\Standardize-AllStoredProcedures.ps1
     Database\StoredProcedures
     Database\StoredProcedures\Auth
     Database\StoredProcedures\Auth\sp_Auth_User_Deactivate.sql
     Database\StoredProcedures\Auth\sp_Auth_User_GetAll.sql
     Database\StoredProcedures\Auth\sp_Auth_User_Update.sql
     Database\StoredProcedures\Auth\sp_Auth_User_UpdateVisualCredentials.sql
     Database\StoredProcedures\Authentication
     Database\StoredProcedures\Authentication\sp_Auth_Department_GetAll.sql
     Database\StoredProcedures\Authentication\sp_Auth_Terminal_GetShared.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_Create.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_GetByWindowsUsername.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_GetDefaultMode.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_SeedDefaultModes.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_UpdateDefaultDunnageMode.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_UpdateDefaultMode.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_UpdateDefaultReceivingMode.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_Upsert.sql
     Database\StoredProcedures\Authentication\sp_Auth_User_ValidatePin.sql
     Database\StoredProcedures\Authentication\sp_Auth_Workstation_Upsert.sql
     Database\StoredProcedures\BulkInventory
     Database\StoredProcedures\BulkInventory\sp_BulkInventory_Transaction_DeleteById.sql
     Database\StoredProcedures\BulkInventory\sp_BulkInventory_Transaction_GetByUser.sql
     Database\StoredProcedures\BulkInventory\sp_BulkInventory_Transaction_Insert.sql
     Database\StoredProcedures\BulkInventory\sp_BulkInventory_Transaction_Update.sql
     Database\StoredProcedures\BulkInventory\sp_BulkInventory_Transaction_UpdateStatus.sql
     Database\StoredProcedures\Dunnage
     Database\StoredProcedures\Dunnage\sp_Dunnage_CustomFields_GetByType.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_CustomFields_Insert.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Inventory_Check.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Inventory_Delete.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Inventory_GetAll.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Inventory_GetByPart.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Inventory_Insert.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Inventory_Update.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_LabelData_ClearToHistory.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_LabelData_GetAll.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_LabelData_Insert.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_LabelData_InsertBatch.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_loads_delete.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Loads_GetAll.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Loads_GetByDateRange.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Loads_GetById.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_loads_update.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_NonPO_Delete.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_NonPO_GetAll.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_NonPO_Upsert.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Parts_CountTransactions.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_parts_delete.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Parts_GetAll.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Parts_GetById.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Parts_GetByType.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Parts_GetTransactionCount.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_parts_insert.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_parts_search.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_parts_update.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Specs_CountPartsUsingSpec.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Specs_DeleteById.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Specs_DeleteByType.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Specs_GetAll.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Specs_GetAllKeys.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Specs_GetById.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Specs_GetByType.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_specs_insert.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_specs_update.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Types_CheckDuplicate.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Types_CountParts.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Types_CountTransactions.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_types_delete.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Types_GetAll.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Types_GetById.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Types_GetPartCount.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Types_GetTransactionCount.sql
     Database\StoredProcedures\Dunnage\sp_Dunnage_Types_GetUsageCount.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_types_insert.sql
     Database\StoredProcedures\Dunnage\sp_dunnage_types_update.sql
     Database\StoredProcedures\Receiving
     Database\StoredProcedures\Receiving\sp_Receiving_History_Get.sql
     Database\StoredProcedures\Receiving\sp_Receiving_History_Import.sql
     Database\StoredProcedures\Receiving\sp_Receiving_LabelData_ClearToHistory.sql
     Database\StoredProcedures\Receiving\sp_Receiving_LabelData_GetAll.sql
     Database\StoredProcedures\Receiving\sp_Receiving_LabelData_Insert.sql
     Database\StoredProcedures\Receiving\sp_Receiving_LabelData_Update.sql
     Database\StoredProcedures\Receiving\sp_Receiving_Line_Insert.sql
     Database\StoredProcedures\Receiving\sp_Receiving_Load_Delete.sql
     Database\StoredProcedures\Receiving\sp_Receiving_Load_GetAll.sql
     Database\StoredProcedures\Receiving\sp_Receiving_Load_Insert.sql
     Database\StoredProcedures\Receiving\sp_Receiving_Load_Update.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypeMappings_Delete.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypeMappings_GetAll.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypeMappings_GetByPrefix.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypeMappings_Insert.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypeMappings_Update.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypePreference_Delete.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypePreference_Get.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypePreference_Save.sql
     Database\StoredProcedures\Receiving\sp_Receiving_PackageTypes_Delete.sql
     Database\StoredProcedures\Receiving\sp_Receiving_QualityHolds_GetByLoadID.sql
     Database\StoredProcedures\Receiving\sp_Receiving_QualityHolds_Insert.sql
     Database\StoredProcedures\Receiving\sp_Receiving_QualityHolds_Update.sql
     Database\StoredProcedures\Reporting
     Database\StoredProcedures\Reporting\sp_Reporting_Availability_GetByDateRange.sql
     Database\StoredProcedures\Reporting\sp_Reporting_DunnageHistory_GetByDateRange.sql
     Database\StoredProcedures\Reporting\sp_Reporting_ReceivingHistory_GetByDateRange.sql
     Database\StoredProcedures\Reporting\sp_Reporting_VolvoHistory_GetByDateRange.sql
     Database\StoredProcedures\Settings
     Database\StoredProcedures\Settings\sp_Auth_Activity_Log.sql
     Database\StoredProcedures\Settings\sp_Dunnage_UserPreferences_GetRecentIcons.sql
     Database\StoredProcedures\Settings\sp_Dunnage_UserPreferences_Upsert.sql
     Database\StoredProcedures\Settings\sp_Settings_AuditLog_Get.sql
     Database\StoredProcedures\Settings\sp_Settings_AuditLog_GetBySetting.sql
     Database\StoredProcedures\Settings\sp_Settings_AuditLog_GetByUser.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_Delete.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_GetActive.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_GetAll.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_GetById.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_GetDue.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_Insert.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_ToggleActive.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_Update.sql
     Database\StoredProcedures\Settings\sp_Settings_ScheduledReport_UpdateLastRun.sql
     Database\StoredProcedures\Settings\sp_Settings_System_GetAll.sql
     Database\StoredProcedures\Settings\sp_Settings_System_GetByCategory.sql
     Database\StoredProcedures\Settings\sp_Settings_System_GetByKey.sql
     Database\StoredProcedures\Settings\sp_Settings_System_ResetToDefault.sql
     Database\StoredProcedures\Settings\sp_Settings_System_SetLocked.sql
     Database\StoredProcedures\Settings\sp_Settings_System_UpdateValue.sql
     Database\StoredProcedures\Settings\sp_Settings_User_Get.sql
     Database\StoredProcedures\Settings\sp_Settings_User_GetAllForUser.sql
     Database\StoredProcedures\Settings\sp_Settings_User_Reset.sql
     Database\StoredProcedures\Settings\sp_Settings_User_ResetAll.sql
     Database\StoredProcedures\Settings\sp_Settings_User_Set.sql
     Database\StoredProcedures\Settings\sp_SettingsCore.sql
     Database\StoredProcedures\Settings\sp_Volvo_Settings_Get.sql
     Database\StoredProcedures\Settings\sp_Volvo_Settings_GetAll.sql
     Database\StoredProcedures\Settings\sp_Volvo_Settings_Reset.sql
     Database\StoredProcedures\Settings\sp_Volvo_Settings_Upsert.sql
     Database\StoredProcedures\Volvo
     Database\StoredProcedures\Volvo\sp_Volvo_LabelData_ClearToHistory.sql
     Database\StoredProcedures\Volvo\sp_volvo_part_check_references.sql
     Database\StoredProcedures\Volvo\sp_Volvo_PartComponent_DeleteByParent.sql
     Database\StoredProcedures\Volvo\sp_Volvo_PartComponent_Get.sql
     Database\StoredProcedures\Volvo\sp_Volvo_PartComponent_Insert.sql
     Database\StoredProcedures\Volvo\sp_Volvo_PartMaster_GetAll.sql
     Database\StoredProcedures\Volvo\sp_Volvo_PartMaster_GetById.sql
     Database\StoredProcedures\Volvo\sp_Volvo_PartMaster_Insert.sql
     Database\StoredProcedures\Volvo\sp_Volvo_PartMaster_SetActive.sql
     Database\StoredProcedures\Volvo\sp_Volvo_PartMaster_Update.sql
     Database\StoredProcedures\Volvo\sp_volvo_shipment_complete.sql
     Database\StoredProcedures\Volvo\sp_volvo_shipment_delete.sql
     Database\StoredProcedures\Volvo\sp_Volvo_Shipment_GetHistory.sql
     Database\StoredProcedures\Volvo\sp_Volvo_Shipment_GetPending.sql
     Database\StoredProcedures\Volvo\sp_volvo_shipment_insert.sql
     Database\StoredProcedures\Volvo\sp_volvo_shipment_update.sql
     Database\StoredProcedures\Volvo\sp_Volvo_ShipmentLine_Delete.sql
     Database\StoredProcedures\Volvo\sp_Volvo_ShipmentLine_GetByShipment.sql
     Database\StoredProcedures\Volvo\sp_Volvo_ShipmentLine_Insert.sql
     Database\StoredProcedures\Volvo\sp_Volvo_ShipmentLine_Update.sql
     Database\TestData
     Database\TestData\01_volvo_seed_data.sql
     Database\TestData\02_Migrate-ExpandReceivingLabelData.sql
     Database\TestData\02_Migrate-SeedDunnageMasterData.sql
     Database\Troubleshoot_MySQLErrors.md
     Database\UseToValidate_App_To_SP_Workflow.md


9. **Role-Based Access Control in Settings** ✅ RESOLVED (Serena, 2026-03-21)
   - **Finding: The assumption is partially correct — but the role model is more granular than Admin/User/Viewer.**
   - **Four roles exist** (defined in `Enum_SettingsPermissionLevel`): `User (0)`, `Supervisor (1)`, `Admin (2)`, `Developer (3)`
   - **Permission hierarchy is additive** (enforced in `SetSettingCommandHandler.HasPermissionAsync`):
     - `User` → any authenticated user can change it (no role check)
     - `Supervisor` → requires `Supervisor`, `Admin`, or `Developer` role
     - `Admin` → requires `Admin` or `Developer` role
     - `Developer` → requires `Developer` role only
   - **Enforcement is server-side only** (write-time, via `SetSettingCommandHandler`). There is currently **no UI-level IsEnabled/IsReadOnly gating** in any Settings ViewModel — the UI does not reflect whether the current user can write a setting.
   - **Current manifest state** (`Module_Settings.Core/Defaults/settings.manifest.json`, ~200 settings):
     - `Developer` level: **1 setting** — `System:Core.Database.StoredProcedureMaxRetries` (max DB retries for stored procedures)
     - `Supervisor` level: **0 settings**
     - `Admin` level: **0 settings**
     - `User` level: **all remaining settings** (~199)
   - **Impact for CopilotForms**: No separate workflow documentation per role is needed at this time — 99.5% of settings are unrestricted. A single note in the Developer Tools form for `StoredProcedureMaxRetries` is sufficient. If UI-level permission indicators are desired in the future, they would require a new `CanEdit` property surfaced from the manifest + current user session.
   - **Source files**: `Module_Settings.Core/Enums/Enum_SettingsPermissionLevel.cs`, `Module_Settings.Core/Services/SetSettingCommandHandler.cs` (lines 167–206), `Module_Settings.Core/Defaults/settings.manifest.json`

10. **Module Interdependencies & Feature Flags** ✅ RESOLVED (Serena + code analysis, 2026-03-21)
    - **Finding: The assumption is CORRECT — `Module_Settings_Receiving` fully configures runtime behaviour in `Module_Receiving` without any code change.**
    - **6 setting groups found** (`Validation`, `BusinessRules`, `Defaults`, `PartNumberPadding`, `UiText`, `Integrations`), totalling **32 distinct setting keys** across 4 consumers.
    - **15 settings directly alter workflow paths** (not just appearance):
      - `BusinessRules.DefaultModeOnStartup` / `RememberLastMode` → can bypass ModeSelection screen entirely
      - `BusinessRules.AllowEditAfterSave = false` → removes Edit button from Complete screen permanently
      - `BusinessRules.ConfirmModeChange = false` → eliminates confirmation dialog on mode switch
      - `BusinessRules.AutoFillHeatLotEnabled = false` → removes auto-fill button from HeatLot step
      - `BusinessRules.SavePackageTypeAsDefault = false` → removes "Save as default" checkbox from PackageType step
      - `BusinessRules.ShowReviewTableByDefault = false` → opens Review in single-card view rather than table
      - `Validation.RequirePoNumber = false` → makes PO number optional (no exit guard on POEntry)
      - `Validation.RequireHeatLot = true` → makes HeatLot mandatory (blocks step advancement)
      - `Validation.AllowNegativeQuantity = true` → allows negative weight/quantity
      - `Validation.WarnOnQuantityExceedsPo = false` → suppresses over-receive warning dialog
      - `Validation.WarnOnSameDayReceiving = false` → suppresses same-day duplicate warning
      - `Validation.ValidatePoExists = false` → disables Infor Visual PO lookup entirely
      - `Defaults.DefaultLocation` (non-empty) → pre-fills Location; unblocks LoadEntry exit guard
      - `PartNumberPadding.Enabled = true` → transforms Part IDs in ManualEntry grid via JSON rules
    - **`Integrations.*` group (7 keys): defined but not yet consumed** — no ViewModel or Service reads them; they are placeholders for future ERP sync behaviour.
    - **Consuming components**: `Service_ReceivingValidation` (Validation group), `ViewModel_Receiving_Workflow` + `Service_ReceivingWorkflow` (BusinessRules/Defaults/UiText), `View_Receiving_ManualEntry` code-behind (PartNumberPadding).
    - **Full mapping table:** `docs/Modules/Module_Receiving/Settings-Feature-Flags.md`

11. **Error Message Localization** ✅ RESOLVED (user confirmation, 2026-03-21)
    - **Finding: Error messages are English only — no localization or multi-language support exists in the codebase.**
    - **Documentation approach**: Document error messages as-is (in English). For AI agent output, reference both the human-readable message and the constant name (e.g., `ReceivingSettingsKeys.Messages.*`) so agents can locate the source in code.

12. **Form/Workflow Activation Conditions** ✅ RESOLVED (Serena + code analysis, 2026-03-21)
    - **Finding: The assumption is INCORRECT — forms are NOT always available. Significant conditional gating exists at every level in every module.**

    ***

    ### RECEIVING MODULE

    #### Step Visibility (ViewModel_Receiving_Workflow)

    All steps are hidden on render. Only one step is visible at a time, controlled by `Enum_ReceivingWorkflowStep`. Each step has its own `IsXxxVisible` bool property:
    `ModeSelection` → `ManualEntry` → `EditMode` → `POEntry` → `PartSelection` → `LoadEntry` → `WeightQuantityEntry` → `HeatLotEntry` → `PackageTypeEntry` → `Review` → `Saving` → `Complete`

    #### Startup Mode Bypass (ModeSelection May Be Skipped Entirely)

    Three levels, applied in priority order — any of these bypass the ModeSelection screen:
    1. **Saved session restore**: If a session with unsaved loads already exists on startup → jumps directly to `Review` (ModeSelection never shown)
    2. **User-level default mode** (`User.DefaultReceivingMode` profile field): If set to `"guided"`, `"manual"`, or `"edit"` → jumps directly to that step (ModeSelection never shown)
    3. **Settings-based default mode**: `BusinessRules.RememberLastMode` (bool, per-user) + `BusinessRules.DefaultModeOnStartup` (string, per-user); if remembered or configured mode resolves to guided/manual → jumps to that step
    4. **Fallback**: ModeSelection is shown

    #### Step Exit Guards (`Service_ReceivingWorkflow.AdvanceToNextStepAsync`)

    Each step blocks advancement until conditions are met:
    - **POEntry → LoadEntry**: Blocked if PO Number is blank AND `IsNonPOItem = false`; blocked if no part has been selected
    - **LoadEntry → WeightQuantityEntry**: Blocked if `NumberOfLoads < 1`; blocked if Location is blank AND no `Defaults.DefaultLocation` setting is configured; blocked if location fails validation via `ValidateLocationAsync`
    - **WeightQuantityEntry → HeatLotEntry**: Blocked if any load has invalid weight or quantity
    - **HeatLotEntry → PackageTypeEntry**: Auto-fills blank entries with `"Nothing Entered"`; blocked if any entry exceeds max character length
    - **PackageTypeEntry → Review**: Blocked if any load has invalid package count or missing package type
    - **ManualEntry → Saving**: Blocked if any load has `IsQualityHoldRequired = true` AND `IsQualityHoldAcknowledged = false` (user must acknowledge quality hold dialog before proceeding)
    - **EditMode → Saving**: Always allowed (no exit guard)
    - **Review → Saving**: Always allowed

    #### PartSelection Step (Dynamic — Mid-POEntry)
    - Shown only when a PO lookup returns more than one matching part
    - Not part of the normal `AdvanceToNextStepAsync` chain; triggered directly by the POEntry lookup logic via `GoToStep(PartSelection)`

    #### CanEditAfterSave (Complete Step)
    - Controlled by the `BusinessRules.AllowEditAfterSave` setting (bool), read on workflow init
    - When `false`: the Edit button on the Complete screen is hidden/disabled
    - When `true`: the Edit button is available to re-open the load in edit mode

    ***

    ### DUNNAGE MODULE

    #### Pagination Buttons (TypeSelection step)
    - **Next page button** (`GoNextPageCommand`): Enabled only when `pagination.HasNextPage = true`
    - **Previous page button** (`GoPreviousPageCommand`): Enabled only when `pagination.HasPreviousPage = true`

    #### Part Selection Guards
    - **Select Part / Proceed**: `CanExecute` gated by `IsPartSelected` (`SelectedPart != null`) — disabled when no part is highlighted
    - **Edit Part**: `SelectedPart != null && !IsBusy`

    #### Quantity Entry Guard
    - **GoNext**: `CanExecute = nameof(IsValid)` where `IsValid = Quantity > 0` — zero or blank quantity blocks advancement; validation message displayed inline

    ***

    ### VOLVO MODULE

    #### History View (`ViewModel_Volvo_History`)
    - **View Detail button**: `CanViewDetail()` → `SelectedShipment != null && !IsBusy`
    - **Edit button**: `CanEdit()` → `SelectedShipment != null && !IsBusy`

    #### Settings / Part Master View (`ViewModel_Volvo_Settings`)
    - **Edit Part button**: `CanEditPart()` → `SelectedPart != null && !IsBusy`

    ***

    ### REPORTING MODULE (`ViewModel_Reporting_Main`)
    - **Generate Reports button**: `CanGenerateReports()` → `!IsBusy && GetSelectedModules().Any()` — at least one report module checkbox must be selected
    - **Copy Email Format button**: `CanCopyEmail()` → `IncludedPreviewModuleCards.Count > 0 && !IsBusy` — reports must have been previewed/generated first; disabled until generation run completes

    ***

    ### UNIVERSAL PATTERN (All Modules)
    - **While any operation is loading** (`IsBusy = true`): All commands guard with `if (IsBusy) return;` at minimum; primary action commands additionally declare `CanExecute = nameof(CanXxx)` which always includes `&& !IsBusy`
    - There is no status-field-based locking (e.g., no "disabled if load status = Shipped" flag) in the current codebase — the Receiving workflow is reset/complete after save and the user starts fresh; Volvo shipments can always be re-edited from History regardless of status
    - Source files: `Service_ReceivingWorkflow.cs`, `ViewModel_Receiving_Workflow.cs`, `ViewModel_Dunnage_TypeSelectionViewModel.cs`, `ViewModel_Dunnage_PartSelectionViewModel.cs`, `ViewModel_Dunnage_QuantityEntryViewModel.cs`, `ViewModel_Volvo_History.cs`, `ViewModel_Volvo_Settings.cs`, `ViewModel_Reporting_Main.cs`

13. **Data Relationships and Master Data** ✅ RESOLVED (user confirmation, 2026-03-21)
    - **Finding: Lookup sources should be documented in AI agent output (CopilotForm output), NOT in user-facing UI.**
    - For each master data field (part number, vendor, location, status), the source (MySQL table, Infor Visual query, hardcoded enum) is included in the CopilotForm output delivered to AI agents after form completion. End users do not see this metadata.

14. **Async Operations and Progress Indication** ✅ RESOLVED (user confirmation, 2026-03-21)
    - **Finding: Yes — all long-running operations show progress indicators.**
    - Pattern: `IsBusy = true` + `StatusMessage` update → operation → `IsBusy = false`. Spinner is bound to `IsBusy` via `BooleanToVisibilityConverter` on all views. No timing estimates are shown. Workflow documentation should include the spinner/status-message step for every async path.

15. **Configuration Defaults and Migration** ✅ RESOLVED (user confirmation, 2026-03-21)
    - **Finding: App is in beta — no migration/upgrade path is required at this time.**
    - All defaults live in `ReceivingSettingsDefaults.cs` and `Module_Settings.Core/Defaults/settings.manifest.json` (source of truth). If defaults change, CopilotForms will be updated at that time. No versioned migration strategy is needed currently.

16. **Testing/Sandbox Mode in Settings** ✅ RESOLVED (user confirmation, 2026-03-21)
    - **Finding: Mock data / ERP toggle flags exist. The `Integrations.*` group (7 keys) in `ReceivingSettingsKeys` / `ReceivingSettingsDefaults` are the ERP on/off switches (e.g., `ErpSyncEnabled`, `AutoPullPoDataEnabled`, `SyncToInforVisual`).**
    - **Status**: All `Integrations.*` keys are defined but not yet consumed by any ViewModel or Service — placeholders for future implementation.
    - **Access level**: When implemented, these will be developer-only flags (requiring `Developer` permission from `Enum_SettingsPermissionLevel`). User-accessible dry-run for label printing is not yet implemented.

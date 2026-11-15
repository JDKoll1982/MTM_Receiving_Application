# Database Object Catalog

Last Updated: 2026-11-15

Generated from `Database/Database_Deployment/Sql_Files` by `SqlObjectCatalogTools.ps1`.
Every object listed here is deployed idempotently by `MTM_Database_Deployment`; no migration layer is required.

## Summary

| Category | Objects |
| --- | --- |
| schema_file | 51 |
| seed_file | 6 |
| stored_procedure_file | 216 |
| trigger_file | 1 |
| view_file | 5 |
| **Total** | **279** |

## Tables

| Object | File | Parameters | Writes | Codebase refs |
| --- | --- | --- | --- | --- |
| `auth_users` | `Schemas/02_Table_auth_users.sql` | 0 | 0 | 0 |
| `auth_workstation_config` | `Schemas/03_Table_auth_workstation_config.sql` | 0 | 0 | 0 |
| `departments` | `Schemas/01_Table_auth_departments.sql` | 0 | 0 | 0 |
| `dunnage_custom_field_choices` | `Schemas/10_Table_dunnage_custom_field_choices.sql` | 0 | 0 | 0 |
| `dunnage_custom_fields` | `Schemas/09_Table_dunnage_custom_fields.sql` | 0 | 0 | 0 |
| `dunnage_history` | `Schemas/06_Table_dunnage_history.sql` | 0 | 0 | 0 |
| `dunnage_label_data` | `Schemas/31_Table_dunnage_label_data.sql` | 0 | 0 | 0 |
| `dunnage_no_label_parts` | `Schemas/52_Table_dunnage_no_label_parts.sql` | 0 | 0 | 0 |
| `dunnage_non_po_entries` | `Schemas/09_Table_dunnage_non_po_entries.sql` | 0 | 0 | 0 |
| `dunnage_non_po_part_defaults` | `Schemas/41_Table_dunnage_non_po_part_defaults.sql` | 0 | 0 | 0 |
| `dunnage_parts` | `Schemas/05_Table_dunnage_parts.sql` | 0 | 0 | 0 |
| `dunnage_quantity_types` | `Schemas/40_Table_dunnage_quantity_types.sql` | 0 | 0 | 0 |
| `dunnage_requires_inventory` | `Schemas/07_Table_dunnage_requires_inventory.sql` | 0 | 0 | 0 |
| `dunnage_types` | `Schemas/04_Table_dunnage_types.sql` | 0 | 0 | 0 |
| `lot_marking_part_scope` | `Schemas/51_Table_lot_marking_part_scope.sql` | 0 | 0 | 0 |
| `lot_markings` | `Schemas/50_Table_lot_markings.sql` | 0 | 0 | 0 |
| `outside_service_request` | `Schemas/32_Table_outside_service_request.sql` | 0 | 0 | 0 |
| `outside_service_request_line` | `Schemas/33_Table_outside_service_request_line.sql` | 0 | 0 | 0 |
| `outside_service_request_package` | `Schemas/34_Table_outside_service_request_package.sql` | 0 | 0 | 0 |
| `receiving_history` | `Schemas/10_Table_receiving_history.sql` | 0 | 0 | 0 |
| `receiving_label_data` | `Schemas/11_Table_receiving_label_data.sql` | 0 | 0 | 0 |
| `receiving_non_po_entries` | `Schemas/42_Table_receiving_non_po_entries.sql` | 0 | 0 | 0 |
| `receiving_non_po_part_defaults` | `Schemas/43_Table_receiving_non_po_part_defaults.sql` | 0 | 0 | 0 |
| `receiving_package_type_mapping` | `Schemas/12_Table_receiving_package_type_mapping.sql` | 0 | 0 | 0 |
| `receiving_package_types` | `Schemas/13_Table_receiving_package_types.sql` | 0 | 0 | 0 |
| `receiving_quality_holds` | `Schemas/13_Table_Receiving_Quality_Holds_Schema.sql` | 0 | 0 | 0 |
| `receiving_scanner_item` | `Schemas/46_Table_receiving_scanner_item.sql` | 0 | 0 | 0 |
| `receiving_scanner_profile` | `Schemas/47_Table_receiving_scanner_profile.sql` | 0 | 0 | 0 |
| `receiving_scanner_session` | `Schemas/45_Table_receiving_scanner_session.sql` | 0 | 0 | 0 |
| `receiving_vendor_variables` | `Schemas/44_Table_receiving_vendor_variables.sql` | 0 | 0 | 0 |
| `reporting_scheduled_reports` | `Schemas/14_Table_reporting_scheduled_reports.sql` | 0 | 0 | 0 |
| `settings_activity` | `Schemas/21_Table_settings_activity.sql` | 0 | 0 | 0 |
| `settings_dunnage_personal` | `Schemas/22_Table_settings_dunnage_personal.sql` | 0 | 0 | 0 |
| `settings_module_volvo` | `Schemas/23_Table_settings_module_volvo.sql` | 0 | 0 | 0 |
| `settings_personal` | `Schemas/24_Table_settings_personal.sql` | 0 | 0 | 0 |
| `settings_personal_activity_log` | `Schemas/25_Table_settings_personal_activity_log.sql` | 0 | 0 | 0 |
| `settings_reporting_recipients` | `Schemas/39_Table_settings_reporting_recipients.sql` | 0 | 0 | 0 |
| `settings_roles` | `Schemas/20_Table_settings_roles.sql` | 0 | 0 | 0 |
| `settings_universal` | `Schemas/27_Table_settings_universal.sql` | 0 | 0 | 0 |
| `settings_user_roles` | `Schemas/19_Table_settings_user_roles.sql` | 0 | 0 | 0 |
| `settings_volvo_recipents` | `Schemas/38_Table_settings_volvo_recipents.sql` | 0 | 0 | 0 |
| `settings_weldedcoils` | `Schemas/49_Table_settings_weldedcoils.sql` | 0 | 0 | 0 |
| `software_version` | `Schemas/35_Table_software_version.sql` | 0 | 0 | 0 |
| `volvo_generated_label_data` | `Schemas/36_Table_volvo_generated_label_data.sql` | 0 | 0 | 0 |
| `volvo_generated_label_history` | `Schemas/37_Table_volvo_generated_label_history.sql` | 0 | 0 | 0 |
| `volvo_label_data` | `Schemas/28_Table_volvo_label_data.sql` | 0 | 0 | 0 |
| `volvo_label_history` | `Schemas/31_Table_volvo_label_history.sql` | 0 | 0 | 0 |
| `volvo_line_data` | `Schemas/29_Table_volvo_line_data.sql` | 0 | 0 | 0 |
| `volvo_line_history` | `Schemas/31_Table_volvo_line_history.sql` | 0 | 0 | 0 |
| `volvo_masterdata` | `Schemas/30_Table_volvo_masterdata.sql` | 0 | 0 | 0 |
| `volvo_part_components` | `Schemas/31_Table_volvo_part_components.sql` | 0 | 0 | 0 |

## Views

| Object | File | Parameters | Writes | Codebase refs |
| --- | --- | --- | --- | --- |
| `view_dunnage_history` | `Views/01_View_dunnage_history.sql` | 0 | 0 | 0 |
| `view_dunnage_label_data` | `Views/05_View_dunnage_label_data.sql` | 0 | 0 | 0 |
| `view_receiving_history` | `Views/02_View_receiving_history.sql` | 0 | 0 | 0 |
| `view_volvo_history` | `Views/03_View_volvo_history.sql` | 0 | 0 | 0 |
| `view_volvo_label_data_history` | `Views/04_View_volvo_label_data_history.sql` | 0 | 0 | 0 |

## Stored Procedures

| Object | File | Parameters | Writes | Codebase refs |
| --- | --- | --- | --- | --- |
| `sp_Auth_Activity_Log` | `StoredProcedures/Settings/sp_Auth_Activity_Log.sql` | 4 | 2 | 1 |
| `sp_Auth_Department_GetAll` | `StoredProcedures/Authentication/sp_Auth_Department_GetAll.sql` | 0 | 0 | 1 |
| `sp_Auth_Terminal_GetShared` | `StoredProcedures/Authentication/sp_Auth_Terminal_GetShared.sql` | 0 | 0 | 1 |
| `sp_Auth_User_Create` | `StoredProcedures/Authentication/sp_Auth_User_Create.sql` | 10 | 2 | 1 |
| `sp_Auth_User_Deactivate` | `StoredProcedures/Auth/sp_Auth_User_Deactivate.sql` | 3 | 2 | 1 |
| `sp_Auth_User_GetAll` | `StoredProcedures/Auth/sp_Auth_User_GetAll.sql` | 0 | 0 | 1 |
| `sp_Auth_User_GetByWindowsUsername` | `StoredProcedures/Authentication/sp_Auth_User_GetByWindowsUsername.sql` | 1 | 0 | 1 |
| `sp_Auth_User_GetDefaultMode` | `StoredProcedures/Authentication/sp_Auth_User_GetDefaultMode.sql` | 1 | 0 | 0 |
| `sp_Auth_User_IsWindowsUsernameUnique` | `StoredProcedures/Authentication/sp_Auth_User_IsWindowsUsernameUnique.sql` | 2 | 0 | 1 |
| `sp_Auth_User_SeedDefaultModes` | `StoredProcedures/Authentication/sp_Auth_User_SeedDefaultModes.sql` | 1 | 2 | 1 |
| `sp_Auth_User_Update` | `StoredProcedures/Auth/sp_Auth_User_Update.sql` | 10 | 2 | 1 |
| `sp_Auth_User_UpdateDefaultDunnageMode` | `StoredProcedures/Authentication/sp_Auth_User_UpdateDefaultDunnageMode.sql` | 2 | 2 | 1 |
| `sp_Auth_User_UpdateDefaultMode` | `StoredProcedures/Authentication/sp_Auth_User_UpdateDefaultMode.sql` | 2 | 2 | 1 |
| `sp_Auth_User_UpdateDefaultReceivingMode` | `StoredProcedures/Authentication/sp_Auth_User_UpdateDefaultReceivingMode.sql` | 2 | 2 | 1 |
| `sp_Auth_User_UpdateVisualCredentials` | `StoredProcedures/Auth/sp_Auth_User_UpdateVisualCredentials.sql` | 5 | 2 | 1 |
| `sp_Auth_User_Upsert` | `StoredProcedures/Authentication/sp_Auth_User_Upsert.sql` | 10 | 2 | 0 |
| `sp_Auth_User_ValidatePin` | `StoredProcedures/Authentication/sp_Auth_User_ValidatePin.sql` | 2 | 0 | 1 |
| `sp_Auth_Workstation_Upsert` | `StoredProcedures/Authentication/sp_Auth_Workstation_Upsert.sql` | 4 | 2 | 1 |
| `sp_Dunnage_CustomFieldChoices_DeleteByField` | `StoredProcedures/Dunnage/sp_Dunnage_CustomFieldChoices_DeleteByField.sql` | 1 | 1 | 1 |
| `sp_Dunnage_CustomFieldChoices_GetByField` | `StoredProcedures/Dunnage/sp_Dunnage_CustomFieldChoices_GetByField.sql` | 1 | 0 | 0 |
| `sp_Dunnage_CustomFieldChoices_Insert` | `StoredProcedures/Dunnage/sp_Dunnage_CustomFieldChoices_Insert.sql` | 3 | 2 | 1 |
| `sp_Dunnage_CustomFields_Delete` | `StoredProcedures/Dunnage/sp_Dunnage_CustomFields_Delete.sql` | 1 | 1 | 1 |
| `sp_Dunnage_CustomFields_GetByType` | `StoredProcedures/Dunnage/sp_Dunnage_CustomFields_GetByType.sql` | 1 | 0 | 0 |
| `sp_Dunnage_CustomFields_Insert` | `StoredProcedures/Dunnage/sp_Dunnage_CustomFields_Insert.sql` | 14 | 2 | 1 |
| `sp_Dunnage_CustomFields_Update` | `StoredProcedures/Dunnage/sp_Dunnage_CustomFields_Update.sql` | 10 | 2 | 1 |
| `sp_Dunnage_History_Insert` | `StoredProcedures/Dunnage/sp_Dunnage_History_Insert.sql` | 25 | 2 | 0 |
| `sp_Dunnage_Inventory_Check` | `StoredProcedures/Dunnage/sp_Dunnage_Inventory_Check.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Inventory_Delete` | `StoredProcedures/Dunnage/sp_Dunnage_Inventory_Delete.sql` | 1 | 1 | 1 |
| `sp_Dunnage_Inventory_GetAll` | `StoredProcedures/Dunnage/sp_Dunnage_Inventory_GetAll.sql` | 0 | 0 | 0 |
| `sp_Dunnage_Inventory_GetByPart` | `StoredProcedures/Dunnage/sp_Dunnage_Inventory_GetByPart.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Inventory_Insert` | `StoredProcedures/Dunnage/sp_Dunnage_Inventory_Insert.sql` | 5 | 2 | 1 |
| `sp_Dunnage_Inventory_Update` | `StoredProcedures/Dunnage/sp_Dunnage_Inventory_Update.sql` | 5 | 2 | 1 |
| `sp_Dunnage_LabelData_ClearToHistory` | `StoredProcedures/Dunnage/sp_Dunnage_LabelData_ClearToHistory.sql` | 7 | 2 | 1 |
| `sp_Dunnage_LabelData_Delete` | `StoredProcedures/Dunnage/sp_Dunnage_LabelData_Delete.sql` | 1 | 1 | 1 |
| `sp_Dunnage_LabelData_GetAll` | `StoredProcedures/Dunnage/sp_Dunnage_LabelData_GetAll.sql` | 0 | 0 | 0 |
| `sp_Dunnage_LabelData_Insert` | `StoredProcedures/Dunnage/sp_Dunnage_LabelData_Insert.sql` | 25 | 2 | 0 |
| `sp_Dunnage_LabelData_InsertFromHistory` | `StoredProcedures/Dunnage/sp_Dunnage_LabelData_InsertFromHistory.sql` | 3 | 2 | 1 |
| `sp_Dunnage_LabelData_Update` | `StoredProcedures/Dunnage/sp_Dunnage_LabelData_Update.sql` | 25 | 2 | 1 |
| `sp_Dunnage_LabelHistory_GetForReprint` | `StoredProcedures/Dunnage/sp_Dunnage_LabelHistory_GetForReprint.sql` | 4 | 0 | 1 |
| `sp_Dunnage_Loads_Delete` | `StoredProcedures/Dunnage/sp_dunnage_loads_delete.sql` | 1 | 1 | 1 |
| `sp_Dunnage_Loads_GetAll` | `StoredProcedures/Dunnage/sp_Dunnage_Loads_GetAll.sql` | 0 | 0 | 0 |
| `sp_Dunnage_Loads_GetByDateRange` | `StoredProcedures/Dunnage/sp_Dunnage_Loads_GetByDateRange.sql` | 2 | 0 | 0 |
| `sp_Dunnage_Loads_GetById` | `StoredProcedures/Dunnage/sp_Dunnage_Loads_GetById.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Loads_Update` | `StoredProcedures/Dunnage/sp_dunnage_loads_update.sql` | 22 | 2 | 1 |
| `sp_Dunnage_NoLabelParts_GetAll` | `StoredProcedures/Dunnage/sp_Dunnage_NoLabelParts_GetAll.sql` | 0 | 0 | 0 |
| `sp_Dunnage_NoLabelParts_Set` | `StoredProcedures/Dunnage/sp_Dunnage_NoLabelParts_Set.sql` | 3 | 2 | 1 |
| `sp_Dunnage_NonPO_Delete` | `StoredProcedures/Dunnage/sp_Dunnage_NonPO_Delete.sql` | 1 | 1 | 1 |
| `sp_Dunnage_NonPO_GetAll` | `StoredProcedures/Dunnage/sp_Dunnage_NonPO_GetAll.sql` | 0 | 0 | 0 |
| `sp_Dunnage_NonPO_PartDefault_GetByPartId` | `StoredProcedures/Dunnage/sp_Dunnage_NonPO_PartDefault_GetByPartId.sql` | 1 | 0 | 0 |
| `sp_Dunnage_NonPO_PartDefault_Upsert` | `StoredProcedures/Dunnage/sp_Dunnage_NonPO_PartDefault_Upsert.sql` | 3 | 2 | 1 |
| `sp_Dunnage_NonPO_Upsert` | `StoredProcedures/Dunnage/sp_Dunnage_NonPO_Upsert.sql` | 2 | 2 | 1 |
| `sp_Dunnage_Parts_ChangeType` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_ChangeType.sql` | 13 | 2 | 1 |
| `sp_Dunnage_Parts_CountTransactions` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_CountTransactions.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Parts_Delete` | `StoredProcedures/Dunnage/sp_dunnage_parts_delete.sql` | 1 | 1 | 1 |
| `sp_Dunnage_Parts_GetAll` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_GetAll.sql` | 0 | 0 | 0 |
| `sp_Dunnage_Parts_GetById` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_GetById.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Parts_GetByType` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_GetByType.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Parts_GetDeleteImpact` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_GetDeleteImpact.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Parts_GetTransactionCount` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_GetTransactionCount.sql` | 2 | 0 | 0 |
| `sp_Dunnage_Parts_Insert` | `StoredProcedures/Dunnage/sp_dunnage_parts_insert.sql` | 17 | 2 | 1 |
| `sp_Dunnage_Parts_InsertWithInventory` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_InsertWithInventory.sql` | 19 | 2 | 1 |
| `sp_Dunnage_Parts_Search` | `StoredProcedures/Dunnage/sp_dunnage_parts_search.sql` | 2 | 0 | 0 |
| `sp_Dunnage_Parts_Update` | `StoredProcedures/Dunnage/sp_dunnage_parts_update.sql` | 16 | 2 | 1 |
| `sp_Dunnage_Parts_UpdateWithReferences` | `StoredProcedures/Dunnage/sp_Dunnage_Parts_UpdateWithReferences.sql` | 19 | 2 | 1 |
| `sp_Dunnage_QuantityTypes_GetAll` | `StoredProcedures/Dunnage/sp_Dunnage_QuantityTypes_GetAll.sql` | 0 | 0 | 0 |
| `sp_Dunnage_QuantityTypes_InsertIfMissing` | `StoredProcedures/Dunnage/sp_Dunnage_QuantityTypes_InsertIfMissing.sql` | 2 | 0 | 1 |
| `sp_Dunnage_Types_CheckDuplicate` | `StoredProcedures/Dunnage/sp_Dunnage_Types_CheckDuplicate.sql` | 3 | 0 | 1 |
| `sp_Dunnage_Types_CountParts` | `StoredProcedures/Dunnage/sp_Dunnage_Types_CountParts.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Types_CountTransactions` | `StoredProcedures/Dunnage/sp_Dunnage_Types_CountTransactions.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Types_Delete` | `StoredProcedures/Dunnage/sp_dunnage_types_delete.sql` | 4 | 1 | 1 |
| `sp_Dunnage_Types_GetAll` | `StoredProcedures/Dunnage/sp_Dunnage_Types_GetAll.sql` | 0 | 0 | 0 |
| `sp_Dunnage_Types_GetById` | `StoredProcedures/Dunnage/sp_Dunnage_Types_GetById.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Types_GetDeleteImpact` | `StoredProcedures/Dunnage/sp_Dunnage_Types_GetDeleteImpact.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Types_GetPartCount` | `StoredProcedures/Dunnage/sp_Dunnage_Types_GetPartCount.sql` | 2 | 0 | 0 |
| `sp_Dunnage_Types_GetTransactionCount` | `StoredProcedures/Dunnage/sp_Dunnage_Types_GetTransactionCount.sql` | 2 | 0 | 0 |
| `sp_Dunnage_Types_GetUsageCount` | `StoredProcedures/Dunnage/sp_Dunnage_Types_GetUsageCount.sql` | 1 | 0 | 0 |
| `sp_Dunnage_Types_Insert` | `StoredProcedures/Dunnage/sp_dunnage_types_insert.sql` | 5 | 2 | 1 |
| `sp_Dunnage_Types_Update` | `StoredProcedures/Dunnage/sp_dunnage_types_update.sql` | 5 | 2 | 1 |
| `sp_Dunnage_UserPreferences_GetRecentIcons` | `StoredProcedures/Settings/sp_Dunnage_UserPreferences_GetRecentIcons.sql` | 2 | 0 | 0 |
| `sp_Dunnage_UserPreferences_Upsert` | `StoredProcedures/Settings/sp_Dunnage_UserPreferences_Upsert.sql` | 3 | 2 | 1 |
| `sp_LotMarkingPartScope_Delete` | `StoredProcedures/Settings/sp_LotMarkingPartScope_Delete.sql` | 1 | 1 | 1 |
| `sp_LotMarkingPartScope_GetAll` | `StoredProcedures/Settings/sp_LotMarkingPartScope_GetAll.sql` | 0 | 0 | 0 |
| `sp_LotMarkingPartScope_Search` | `StoredProcedures/Settings/sp_LotMarkingPartScope_Search.sql` | 1 | 0 | 0 |
| `sp_LotMarkingPartScope_SetEnabled` | `StoredProcedures/Settings/sp_LotMarkingPartScope_SetEnabled.sql` | 2 | 2 | 1 |
| `sp_LotMarkingPartScope_Upsert` | `StoredProcedures/Settings/sp_LotMarkingPartScope_Upsert.sql` | 2 | 2 | 0 |
| `sp_LotMarkings_Delete` | `StoredProcedures/Settings/sp_LotMarkings_Delete.sql` | 1 | 1 | 0 |
| `sp_LotMarkings_GetAll` | `StoredProcedures/Settings/sp_LotMarkings_GetAll.sql` | 0 | 0 | 0 |
| `sp_LotMarkings_GetByHeat` | `StoredProcedures/Settings/sp_LotMarkings_GetByHeat.sql` | 1 | 0 | 0 |
| `sp_LotMarkings_Insert` | `StoredProcedures/Settings/sp_LotMarkings_Insert.sql` | 9 | 2 | 0 |
| `sp_LotMarkings_Resolve` | `StoredProcedures/Settings/sp_LotMarkings_Resolve.sql` | 2 | 0 | 0 |
| `sp_LotMarkings_SetLocked` | `StoredProcedures/Settings/sp_LotMarkings_SetLocked.sql` | 3 | 2 | 0 |
| `sp_LotMarkings_UpdateMarking` | `StoredProcedures/Settings/sp_LotMarkings_UpdateMarking.sql` | 6 | 2 | 0 |
| `sp_Receiving_HeatLotKeys_GetAll` | `StoredProcedures/Receiving/sp_Receiving_HeatLotKeys_GetAll.sql` | 2 | 0 | 0 |
| `sp_Receiving_History_Get` | `StoredProcedures/Receiving/sp_Receiving_History_Get.sql` | 3 | 0 | 2 |
| `sp_Receiving_History_GetForReprint` | `StoredProcedures/Receiving/sp_Receiving_History_GetForReprint.sql` | 4 | 0 | 1 |
| `sp_Receiving_History_Import` | `StoredProcedures/Receiving/sp_Receiving_History_Import.sql` | 12 | 2 | 0 |
| `sp_Receiving_LabelData_ClearToHistory` | `StoredProcedures/Receiving/sp_Receiving_LabelData_ClearToHistory.sql` | 7 | 2 | 2 |
| `sp_Receiving_LabelData_Delete` | `StoredProcedures/Receiving/sp_Receiving_LabelData_Delete.sql` | 2 | 1 | 2 |
| `sp_Receiving_LabelData_GetAll` | `StoredProcedures/Receiving/sp_Receiving_LabelData_GetAll.sql` | 0 | 0 | 1 |
| `sp_Receiving_LabelData_Insert` | `StoredProcedures/Receiving/sp_Receiving_LabelData_Insert.sql` | 35 | 0 | 2 |
| `sp_Receiving_LabelData_InsertFromHistory` | `StoredProcedures/Receiving/sp_Receiving_LabelData_InsertFromHistory.sql` | 3 | 2 | 1 |
| `sp_Receiving_LabelData_SetMarking` | `StoredProcedures/Receiving/sp_Receiving_LabelData_SetMarking.sql` | 3 | 2 | 1 |
| `sp_Receiving_LabelData_Update` | `StoredProcedures/Receiving/sp_Receiving_LabelData_Update.sql` | 36 | 2 | 1 |
| `sp_Receiving_Line_Insert` | `StoredProcedures/Receiving/sp_Receiving_Line_Insert.sql` | 12 | 0 | 1 |
| `sp_Receiving_Load_Delete` | `StoredProcedures/Receiving/sp_Receiving_Load_Delete.sql` | 2 | 1 | 1 |
| `sp_Receiving_Load_GetAll` | `StoredProcedures/Receiving/sp_Receiving_Load_GetAll.sql` | 2 | 0 | 1 |
| `sp_Receiving_Load_Insert` | `StoredProcedures/Receiving/sp_Receiving_Load_Insert.sql` | 12 | 2 | 0 |
| `sp_Receiving_Load_Update` | `StoredProcedures/Receiving/sp_Receiving_Load_Update.sql` | 15 | 2 | 1 |
| `sp_Receiving_LotMarking_ClearRows` | `StoredProcedures/Receiving/sp_Receiving_LotMarking_ClearRows.sql` | 2 | 2 | 0 |
| `sp_Receiving_LotMarking_RealignRows` | `StoredProcedures/Receiving/sp_Receiving_LotMarking_RealignRows.sql` | 4 | 2 | 0 |
| `sp_Receiving_NonPO_Delete` | `StoredProcedures/Receiving/sp_Receiving_NonPO_Delete.sql` | 1 | 1 | 1 |
| `sp_Receiving_NonPO_GetAll` | `StoredProcedures/Receiving/sp_Receiving_NonPO_GetAll.sql` | 0 | 0 | 0 |
| `sp_Receiving_NonPO_PartDefault_GetByPartId` | `StoredProcedures/Receiving/sp_Receiving_NonPO_PartDefault_GetByPartId.sql` | 1 | 0 | 0 |
| `sp_Receiving_NonPO_PartDefault_Upsert` | `StoredProcedures/Receiving/sp_Receiving_NonPO_PartDefault_Upsert.sql` | 3 | 2 | 1 |
| `sp_Receiving_NonPO_Upsert` | `StoredProcedures/Receiving/sp_Receiving_NonPO_Upsert.sql` | 2 | 2 | 1 |
| `sp_Receiving_PackageTypeMappings_Delete` | `StoredProcedures/Receiving/sp_Receiving_PackageTypeMappings_Delete.sql` | 1 | 2 | 0 |
| `sp_Receiving_PackageTypeMappings_GetAll` | `StoredProcedures/Receiving/sp_Receiving_PackageTypeMappings_GetAll.sql` | 1 | 0 | 0 |
| `sp_Receiving_PackageTypeMappings_GetByPrefix` | `StoredProcedures/Receiving/sp_Receiving_PackageTypeMappings_GetByPrefix.sql` | 1 | 0 | 0 |
| `sp_Receiving_PackageTypeMappings_Insert` | `StoredProcedures/Receiving/sp_Receiving_PackageTypeMappings_Insert.sql` | 5 | 2 | 0 |
| `sp_Receiving_PackageTypeMappings_Update` | `StoredProcedures/Receiving/sp_Receiving_PackageTypeMappings_Update.sql` | 6 | 2 | 0 |
| `sp_Receiving_PackageTypePreference_Delete` | `StoredProcedures/Receiving/sp_Receiving_PackageTypePreference_Delete.sql` | 1 | 1 | 1 |
| `sp_Receiving_PackageTypePreference_Get` | `StoredProcedures/Receiving/sp_Receiving_PackageTypePreference_Get.sql` | 1 | 0 | 0 |
| `sp_Receiving_PackageTypePreference_Save` | `StoredProcedures/Receiving/sp_Receiving_PackageTypePreference_Save.sql` | 4 | 2 | 1 |
| `sp_Receiving_PackageTypes_Delete` | `StoredProcedures/Receiving/sp_Receiving_PackageTypes_Delete.sql` | 1 | 1 | 0 |
| `sp_Receiving_QualityHolds_GetByLoadID` | `StoredProcedures/Receiving/sp_Receiving_QualityHolds_GetByLoadID.sql` | 3 | 0 | 1 |
| `sp_Receiving_QualityHolds_Insert` | `StoredProcedures/Receiving/sp_Receiving_QualityHolds_Insert.sql` | 8 | 2 | 1 |
| `sp_Receiving_QualityHolds_Update` | `StoredProcedures/Receiving/sp_Receiving_QualityHolds_Update.sql` | 5 | 2 | 1 |
| `sp_Receiving_ScannerItem_DeleteBySession` | `StoredProcedures/Scanner/sp_Receiving_ScannerItem_DeleteBySession.sql` | 1 | 1 | 1 |
| `sp_Receiving_ScannerItem_GetBySession` | `StoredProcedures/Scanner/sp_Receiving_ScannerItem_GetBySession.sql` | 1 | 0 | 1 |
| `sp_Receiving_ScannerItem_Upsert` | `StoredProcedures/Scanner/sp_Receiving_ScannerItem_Upsert.sql` | 14 | 2 | 1 |
| `sp_Receiving_ScannerProfile_Delete` | `StoredProcedures/Scanner/sp_Receiving_ScannerProfile_Delete.sql` | 2 | 1 | 1 |
| `sp_Receiving_ScannerProfile_GetByUser` | `StoredProcedures/Scanner/sp_Receiving_ScannerProfile_GetByUser.sql` | 1 | 0 | 1 |
| `sp_Receiving_ScannerProfile_SetDefault` | `StoredProcedures/Scanner/sp_Receiving_ScannerProfile_SetDefault.sql` | 2 | 2 | 1 |
| `sp_Receiving_ScannerProfile_Upsert` | `StoredProcedures/Scanner/sp_Receiving_ScannerProfile_Upsert.sql` | 19 | 2 | 1 |
| `sp_Receiving_ScannerSession_GetByUser` | `StoredProcedures/Scanner/sp_Receiving_ScannerSession_GetByUser.sql` | 1 | 0 | 1 |
| `sp_Receiving_ScannerSession_Upsert` | `StoredProcedures/Scanner/sp_Receiving_ScannerSession_Upsert.sql` | 7 | 2 | 1 |
| `sp_Receiving_UnmarkedLotMarkings_Apply` | `StoredProcedures/Receiving/sp_Receiving_UnmarkedLotMarkings_Apply.sql` | 5 | 2 | 0 |
| `sp_Receiving_UnmarkedLotMarkings_Preview` | `StoredProcedures/Receiving/sp_Receiving_UnmarkedLotMarkings_Preview.sql` | 1 | 0 | 0 |
| `sp_receiving_vendor_variables_delete` | `StoredProcedures/Receiving/sp_receiving_vendor_variables_delete.sql` | 1 | 1 | 1 |
| `sp_receiving_vendor_variables_get_all` | `StoredProcedures/Receiving/sp_receiving_vendor_variables_get_all.sql` | 0 | 0 | 1 |
| `sp_receiving_vendor_variables_get_by_vendor` | `StoredProcedures/Receiving/sp_receiving_vendor_variables_get_by_vendor.sql` | 1 | 0 | 1 |
| `sp_receiving_vendor_variables_insert` | `StoredProcedures/Receiving/sp_receiving_vendor_variables_insert.sql` | 2 | 2 | 1 |
| `sp_receiving_vendor_variables_update` | `StoredProcedures/Receiving/sp_receiving_vendor_variables_update.sql` | 2 | 2 | 1 |
| `sp_Reconciliation_DayActivity_GetRange` | `StoredProcedures/Reconciliation/sp_Reconciliation_DayActivity_GetRange.sql` | 2 | 0 | 0 |
| `sp_Reconciliation_ReceivingLocation_Update` | `StoredProcedures/Reconciliation/sp_Reconciliation_ReceivingLocation_Update.sql` | 5 | 2 | 0 |
| `sp_Reconciliation_ReceivingRows_GetDay` | `StoredProcedures/Reconciliation/sp_Reconciliation_ReceivingRows_GetDay.sql` | 1 | 0 | 0 |
| `sp_Reporting_Availability_GetByDateRange` | `StoredProcedures/Reporting/sp_Reporting_Availability_GetByDateRange.sql` | 2 | 0 | 1 |
| `sp_Reporting_DunnageHistory_GetByDateRange` | `StoredProcedures/Reporting/sp_Reporting_DunnageHistory_GetByDateRange.sql` | 2 | 0 | 1 |
| `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay` | `StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay.sql` | 2 | 0 | 0 |
| `sp_Reporting_ReceivingAnalyticsApp_GetCoverageLines` | `StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetCoverageLines.sql` | 2 | 0 | 0 |
| `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount` | `StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount.sql` | 2 | 0 | 0 |
| `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay` | `StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay.sql` | 2 | 0 | 0 |
| `sp_Reporting_ReceivingAnalyticsApp_GetReceivedPartNumbers` | `StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetReceivedPartNumbers.sql` | 2 | 0 | 0 |
| `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals` | `StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals.sql` | 2 | 0 | 0 |
| `sp_Reporting_ReceivingHistory_GetByDateRange` | `StoredProcedures/Reporting/sp_Reporting_ReceivingHistory_GetByDateRange.sql` | 2 | 0 | 1 |
| `sp_Reporting_VolvoHistory_GetByDateRange` | `StoredProcedures/Reporting/sp_Reporting_VolvoHistory_GetByDateRange.sql` | 2 | 0 | 1 |
| `sp_Settings_ReportingRecipients_Delete` | `StoredProcedures/Settings/sp_Settings_ReportingRecipients_Delete.sql` | 1 | 1 | 1 |
| `sp_Settings_ReportingRecipients_GetAll` | `StoredProcedures/Settings/sp_Settings_ReportingRecipients_GetAll.sql` | 0 | 0 | 1 |
| `sp_Settings_ReportingRecipients_Insert` | `StoredProcedures/Settings/sp_Settings_ReportingRecipients_Insert.sql` | 4 | 2 | 1 |
| `sp_Settings_ReportingRecipients_Update` | `StoredProcedures/Settings/sp_Settings_ReportingRecipients_Update.sql` | 5 | 2 | 1 |
| `sp_Settings_ScheduledReport_Delete` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_Delete.sql` | 1 | 2 | 0 |
| `sp_Settings_ScheduledReport_GetActive` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_GetActive.sql` | 0 | 0 | 0 |
| `sp_Settings_ScheduledReport_GetAll` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_GetAll.sql` | 0 | 0 | 0 |
| `sp_Settings_ScheduledReport_GetById` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_GetById.sql` | 1 | 0 | 0 |
| `sp_Settings_ScheduledReport_GetDue` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_GetDue.sql` | 0 | 0 | 0 |
| `sp_Settings_ScheduledReport_Insert` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_Insert.sql` | 5 | 2 | 0 |
| `sp_Settings_ScheduledReport_ToggleActive` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_ToggleActive.sql` | 2 | 2 | 0 |
| `sp_Settings_ScheduledReport_Update` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_Update.sql` | 5 | 2 | 0 |
| `sp_Settings_ScheduledReport_UpdateLastRun` | `StoredProcedures/Settings/sp_Settings_ScheduledReport_UpdateLastRun.sql` | 4 | 2 | 0 |
| `sp_Settings_VolvoRecipients_Delete` | `StoredProcedures/Settings/sp_Settings_VolvoRecipients_Delete.sql` | 1 | 1 | 1 |
| `sp_Settings_VolvoRecipients_GetAll` | `StoredProcedures/Settings/sp_Settings_VolvoRecipients_GetAll.sql` | 0 | 0 | 1 |
| `sp_Settings_VolvoRecipients_Insert` | `StoredProcedures/Settings/sp_Settings_VolvoRecipients_Insert.sql` | 4 | 2 | 1 |
| `sp_Settings_VolvoRecipients_Update` | `StoredProcedures/Settings/sp_Settings_VolvoRecipients_Update.sql` | 5 | 2 | 1 |
| `sp_Settings_WeldedCoils_Delete` | `StoredProcedures/Settings/sp_Settings_WeldedCoils_Delete.sql` | 1 | 1 | 1 |
| `sp_Settings_WeldedCoils_GetAll` | `StoredProcedures/Settings/sp_Settings_WeldedCoils_GetAll.sql` | 0 | 0 | 0 |
| `sp_Settings_WeldedCoils_Insert` | `StoredProcedures/Settings/sp_Settings_WeldedCoils_Insert.sql` | 2 | 2 | 1 |
| `sp_Settings_WeldedCoils_SetActive` | `StoredProcedures/Settings/sp_Settings_WeldedCoils_SetActive.sql` | 2 | 2 | 0 |
| `sp_Settings_WeldedCoils_Update` | `StoredProcedures/Settings/sp_Settings_WeldedCoils_Update.sql` | 2 | 2 | 1 |
| `sp_SettingsCore_System_GetByKey` | `StoredProcedures/Settings/sp_SettingsCore.sql` | 2 | 2 | 1 |
| `sp_SoftwareVersion_GetCurrent` | `StoredProcedures/Settings/sp_SoftwareVersion_GetCurrent.sql` | 0 | 0 | 1 |
| `sp_SoftwareVersion_Upsert` | `StoredProcedures/Settings/sp_SoftwareVersion_Upsert.sql` | 2 | 2 | 1 |
| `sp_Volvo_GeneratedLabelData_ClearToHistory` | `StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_ClearToHistory.sql` | 7 | 2 | 1 |
| `sp_Volvo_GeneratedLabelData_DeleteByShipment` | `StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_DeleteByShipment.sql` | 1 | 1 | 2 |
| `sp_Volvo_GeneratedLabelData_GetAll` | `StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_GetAll.sql` | 0 | 0 | 1 |
| `sp_Volvo_GeneratedLabelData_Insert` | `StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_Insert.sql` | 9 | 2 | 1 |
| `sp_Volvo_GeneratedLabelData_InsertFromHistory` | `StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_InsertFromHistory.sql` | 3 | 2 | 1 |
| `sp_Volvo_GeneratedLabelHistory_GetForReprint` | `StoredProcedures/Volvo/sp_Volvo_GeneratedLabelHistory_GetForReprint.sql` | 4 | 0 | 1 |
| `sp_Volvo_LabelData_ClearToHistory` | `StoredProcedures/Volvo/sp_Volvo_LabelData_ClearToHistory.sql` | 6 | 2 | 1 |
| `sp_volvo_part_check_references` | `StoredProcedures/Volvo/sp_volvo_part_check_references.sql` | 2 | 0 | 1 |
| `sp_Volvo_PartComponent_DeleteByParent` | `StoredProcedures/Volvo/sp_Volvo_PartComponent_DeleteByParent.sql` | 1 | 1 | 1 |
| `sp_Volvo_PartComponent_Get` | `StoredProcedures/Volvo/sp_Volvo_PartComponent_Get.sql` | 1 | 0 | 1 |
| `sp_Volvo_PartComponent_Insert` | `StoredProcedures/Volvo/sp_Volvo_PartComponent_Insert.sql` | 3 | 2 | 1 |
| `sp_Volvo_PartMaster_GetAll` | `StoredProcedures/Volvo/sp_Volvo_PartMaster_GetAll.sql` | 1 | 0 | 1 |
| `sp_Volvo_PartMaster_GetById` | `StoredProcedures/Volvo/sp_Volvo_PartMaster_GetById.sql` | 1 | 0 | 1 |
| `sp_Volvo_PartMaster_Insert` | `StoredProcedures/Volvo/sp_Volvo_PartMaster_Insert.sql` | 3 | 2 | 1 |
| `sp_Volvo_PartMaster_SetActive` | `StoredProcedures/Volvo/sp_Volvo_PartMaster_SetActive.sql` | 2 | 2 | 2 |
| `sp_Volvo_PartMaster_Update` | `StoredProcedures/Volvo/sp_Volvo_PartMaster_Update.sql` | 2 | 2 | 1 |
| `sp_Volvo_Settings_Get` | `StoredProcedures/Settings/sp_Volvo_Settings_Get.sql` | 1 | 0 | 1 |
| `sp_Volvo_Settings_GetAll` | `StoredProcedures/Settings/sp_Volvo_Settings_GetAll.sql` | 1 | 0 | 1 |
| `sp_Volvo_Settings_Reset` | `StoredProcedures/Settings/sp_Volvo_Settings_Reset.sql` | 2 | 2 | 1 |
| `sp_Volvo_Settings_Upsert` | `StoredProcedures/Settings/sp_Volvo_Settings_Upsert.sql` | 3 | 2 | 1 |
| `sp_Volvo_Shipment_Complete` | `StoredProcedures/Volvo/sp_volvo_shipment_complete.sql` | 4 | 2 | 1 |
| `sp_Volvo_Shipment_Delete` | `StoredProcedures/Volvo/sp_volvo_shipment_delete.sql` | 1 | 1 | 1 |
| `sp_Volvo_Shipment_GetById` | `StoredProcedures/Volvo/sp_Volvo_Shipment_GetById.sql` | 1 | 0 | 1 |
| `sp_Volvo_Shipment_GetHistory` | `StoredProcedures/Volvo/sp_Volvo_Shipment_GetHistory.sql` | 4 | 0 | 1 |
| `sp_Volvo_Shipment_GetNextShipmentNumber` | `StoredProcedures/Volvo/sp_Volvo_Shipment_GetNextShipmentNumber.sql` | 0 | 0 | 1 |
| `sp_Volvo_Shipment_GetPending` | `StoredProcedures/Volvo/sp_Volvo_Shipment_GetPending.sql` | 0 | 0 | 1 |
| `sp_Volvo_Shipment_Insert` | `StoredProcedures/Volvo/sp_volvo_shipment_insert.sql` | 5 | 2 | 1 |
| `sp_Volvo_Shipment_Update` | `StoredProcedures/Volvo/sp_volvo_shipment_update.sql` | 2 | 2 | 1 |
| `sp_Volvo_ShipmentHistory_Delete` | `StoredProcedures/Volvo/sp_Volvo_ShipmentHistory_Delete.sql` | 1 | 1 | 1 |
| `sp_Volvo_ShipmentHistory_GetById` | `StoredProcedures/Volvo/sp_Volvo_ShipmentHistory_GetById.sql` | 1 | 0 | 1 |
| `sp_Volvo_ShipmentLine_Delete` | `StoredProcedures/Volvo/sp_Volvo_ShipmentLine_Delete.sql` | 1 | 1 | 1 |
| `sp_Volvo_ShipmentLine_GetByShipment` | `StoredProcedures/Volvo/sp_Volvo_ShipmentLine_GetByShipment.sql` | 1 | 0 | 1 |
| `sp_Volvo_ShipmentLine_Insert` | `StoredProcedures/Volvo/sp_Volvo_ShipmentLine_Insert.sql` | 10 | 2 | 2 |
| `sp_Volvo_ShipmentLine_Update` | `StoredProcedures/Volvo/sp_Volvo_ShipmentLine_Update.sql` | 10 | 2 | 1 |
| `sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId` | `StoredProcedures/Volvo/sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId.sql` | 1 | 0 | 1 |

## Triggers

| Object | File | Parameters | Writes | Codebase refs |
| --- | --- | --- | --- | --- |
| `01_Triggers_Module_Volvo` | `Triggers/01_Triggers_Module_Volvo.sql` | 0 | 0 | 0 |

## Seed Data Scripts

| Object | File | Parameters | Writes | Codebase refs |
| --- | --- | --- | --- | --- |
| `01_seed_volvo_data` | `SeedData/01_seed_volvo_data.sql` | 0 | 2 | 0 |
| `02_seed_settingscore_roles` | `SeedData/02_seed_settingscore_roles.sql` | 0 | 0 | 0 |
| `03_seed_dunnage_data` | `SeedData/03_seed_dunnage_data.sql` | 0 | 0 | 0 |
| `04_seed_auth_user_bootstrap_accounts` | `SeedData/04_seed_auth_user_bootstrap_accounts.sql` | 0 | 2 | 0 |
| `05_seed_software_version` | `SeedData/05_seed_software_version.sql` | 0 | 2 | 0 |
| `06_seed_module_email_recipients` | `SeedData/06_seed_module_email_recipients.sql` | 0 | 2 | 0 |

## Object dependencies

| Object | References |
| --- | --- |
| `` | dunnage_non_po_entries, dunnage_parts, dunnage_quantity_types, dunnage_requires_inventory, dunnage_types |
| `` | volvo_label_data |
| `` | auth_users, settings_roles, settings_user_roles |
| `sp_Auth_Department_GetAll` | departments |
| `sp_Auth_Terminal_GetShared` | auth_workstation_config |
| `sp_Auth_User_Create` | auth_users |
| `sp_Auth_User_Deactivate` | auth_users |
| `sp_Auth_User_GetAll` | auth_users |
| `sp_Auth_User_GetByWindowsUsername` | auth_users |
| `sp_Auth_User_GetDefaultMode` | auth_users |
| `sp_Auth_User_IsWindowsUsernameUnique` | auth_users |
| `sp_Auth_User_SeedDefaultModes` | __system__:COLUMNS |
| `sp_Auth_User_Update` | auth_users |
| `sp_Auth_User_UpdateVisualCredentials` | auth_users |
| `sp_Auth_User_Upsert` | auth_users |
| `sp_Auth_User_ValidatePin` | auth_users |
| `sp_Dunnage_CustomFieldChoices_DeleteByField` | dunnage_custom_field_choices |
| `sp_Dunnage_CustomFieldChoices_GetByField` | dunnage_custom_field_choices |
| `sp_Dunnage_CustomFields_Delete` | dunnage_custom_fields |
| `sp_Dunnage_CustomFields_GetByType` | dunnage_custom_fields |
| `sp_Dunnage_CustomFields_Insert` | dunnage_custom_fields |
| `sp_Dunnage_CustomFields_Update` | dunnage_custom_fields |
| `sp_Dunnage_Inventory_Check` | dunnage_requires_inventory |
| `sp_Dunnage_Inventory_Delete` | dunnage_requires_inventory |
| `sp_Dunnage_Inventory_GetAll` | dunnage_parts, dunnage_requires_inventory, dunnage_types |
| `sp_Dunnage_Inventory_GetByPart` | dunnage_parts, dunnage_requires_inventory, dunnage_types |
| `sp_Dunnage_LabelData_ClearToHistory` | dunnage_label_data |
| `sp_Dunnage_LabelData_Delete` | dunnage_label_data |
| `sp_Dunnage_LabelData_GetAll` | dunnage_label_data |
| `sp_Dunnage_LabelData_InsertFromHistory` | dunnage_history, dunnage_label_data |
| `sp_Dunnage_LabelHistory_GetForReprint` | dunnage_history, dunnage_label_data |
| `sp_Dunnage_Loads_Delete` | dunnage_history |
| `sp_Dunnage_Loads_GetAll` | dunnage_history, dunnage_parts, dunnage_types |
| `sp_Dunnage_Loads_GetByDateRange` | dunnage_history, dunnage_parts, dunnage_types |
| `sp_Dunnage_Loads_GetById` | dunnage_history, dunnage_parts, dunnage_types |
| `sp_Dunnage_NoLabelParts_GetAll` | dunnage_no_label_parts |
| `sp_Dunnage_NoLabelParts_Set` | dunnage_no_label_parts |
| `sp_Dunnage_NonPO_Delete` | dunnage_non_po_entries |
| `sp_Dunnage_NonPO_GetAll` | dunnage_non_po_entries |
| `sp_Dunnage_NonPO_PartDefault_GetByPartId` | dunnage_non_po_part_defaults |
| `sp_Dunnage_Parts_ChangeType` | dunnage_parts, dunnage_types |
| `sp_Dunnage_Parts_CountTransactions` | dunnage_history |
| `sp_Dunnage_Parts_Delete` | dunnage_history, dunnage_label_data, dunnage_no_label_parts, dunnage_non_po_part_defaults, dunnage_parts, dunnage_requires_inventory |
| `sp_Dunnage_Parts_GetAll` | dunnage_parts, dunnage_types |
| `sp_Dunnage_Parts_GetById` | dunnage_parts, dunnage_types |
| `sp_Dunnage_Parts_GetByType` | dunnage_parts, dunnage_types |
| `sp_Dunnage_Parts_GetDeleteImpact` | dunnage_history, dunnage_label_data, dunnage_requires_inventory |
| `sp_Dunnage_Parts_GetTransactionCount` | dunnage_history |
| `sp_Dunnage_Parts_Search` | dunnage_parts, dunnage_types |
| `sp_Dunnage_Parts_UpdateWithReferences` | dunnage_parts, dunnage_requires_inventory |
| `sp_Dunnage_QuantityTypes_GetAll` | dunnage_quantity_types |
| `sp_Dunnage_Types_CheckDuplicate` | dunnage_types |
| `sp_Dunnage_Types_CountParts` | dunnage_parts |
| `sp_Dunnage_Types_CountTransactions` | dunnage_history, dunnage_parts |
| `sp_Dunnage_Types_Delete` | dunnage_history, dunnage_label_data, dunnage_non_po_part_defaults, dunnage_parts, dunnage_types |
| `sp_Dunnage_Types_GetAll` | dunnage_types |
| `sp_Dunnage_Types_GetById` | dunnage_types |
| `sp_Dunnage_Types_GetDeleteImpact` | dunnage_history, dunnage_label_data, dunnage_parts |
| `sp_Dunnage_Types_GetPartCount` | dunnage_parts |
| `sp_Dunnage_Types_GetTransactionCount` | dunnage_history, dunnage_parts |
| `sp_Dunnage_Types_GetUsageCount` | dunnage_types, receiving_package_type_mapping |
| `sp_Dunnage_Types_Insert` | dunnage_types |
| `sp_Dunnage_Types_Update` | dunnage_types |
| `sp_Dunnage_UserPreferences_GetRecentIcons` | settings_dunnage_personal |
| `sp_LotMarkingPartScope_Delete` | lot_marking_part_scope |
| `sp_LotMarkingPartScope_GetAll` | lot_marking_part_scope |
| `sp_LotMarkingPartScope_Search` | lot_marking_part_scope |
| `sp_LotMarkingPartScope_SetEnabled` | lot_marking_part_scope |
| `sp_LotMarkingPartScope_Upsert` | lot_marking_part_scope |
| `sp_LotMarkings_Delete` | lot_markings |
| `sp_LotMarkings_GetAll` | lot_markings |
| `sp_LotMarkings_GetByHeat` | lot_markings |
| `sp_LotMarkings_Insert` | lot_markings |
| `sp_LotMarkings_Resolve` | lot_markings |
| `sp_LotMarkings_SetLocked` | lot_markings |
| `sp_LotMarkings_UpdateMarking` | lot_markings |
| `sp_Receiving_HeatLotKeys_GetAll` | receiving_history, receiving_label_data |
| `sp_Receiving_History_Get` | receiving_history |
| `sp_Receiving_History_GetForReprint` | receiving_history, receiving_label_data |
| `sp_Receiving_LabelData_ClearToHistory` | receiving_label_data |
| `sp_Receiving_LabelData_Delete` | receiving_label_data |
| `sp_Receiving_LabelData_GetAll` | receiving_label_data |
| `sp_Receiving_LabelData_InsertFromHistory` | receiving_history, receiving_label_data |
| `sp_Receiving_LabelData_SetMarking` | receiving_label_data |
| `sp_Receiving_Load_Delete` | receiving_history |
| `sp_Receiving_Load_GetAll` | receiving_history |
| `sp_Receiving_NonPO_Delete` | receiving_non_po_entries |
| `sp_Receiving_NonPO_GetAll` | receiving_non_po_entries |
| `sp_Receiving_NonPO_PartDefault_GetByPartId` | receiving_non_po_part_defaults |
| `sp_Receiving_PackageTypeMappings_Delete` | receiving_package_type_mapping |
| `sp_Receiving_PackageTypeMappings_GetAll` | receiving_package_type_mapping |
| `sp_Receiving_PackageTypeMappings_GetByPrefix` | receiving_package_type_mapping |
| `sp_Receiving_PackageTypeMappings_Update` | receiving_package_type_mapping |
| `sp_Receiving_PackageTypePreference_Delete` | receiving_package_types |
| `sp_Receiving_PackageTypePreference_Get` | receiving_package_types |
| `sp_Receiving_PackageTypes_Delete` | receiving_package_types |
| `sp_Receiving_QualityHolds_GetByLoadID` | receiving_quality_holds |
| `sp_Receiving_ScannerItem_DeleteBySession` | receiving_scanner_item |
| `sp_Receiving_ScannerItem_GetBySession` | receiving_scanner_item |
| `sp_Receiving_ScannerProfile_Delete` | receiving_scanner_profile |
| `sp_Receiving_ScannerProfile_GetByUser` | receiving_scanner_profile |
| `sp_Receiving_ScannerSession_GetByUser` | receiving_scanner_session |
| `sp_Receiving_UnmarkedLotMarkings_Apply` | lot_marking_part_scope |
| `sp_Receiving_UnmarkedLotMarkings_Preview` | receiving_history, receiving_label_data |
| `sp_receiving_vendor_variables_delete` | receiving_vendor_variables |
| `sp_receiving_vendor_variables_get_all` | receiving_vendor_variables |
| `sp_receiving_vendor_variables_get_by_vendor` | receiving_vendor_variables |
| `sp_Reconciliation_DayActivity_GetRange` | receiving_history, receiving_label_data |
| `sp_Reconciliation_ReceivingRows_GetDay` | receiving_history, receiving_label_data |
| `sp_Reporting_Availability_GetByDateRange` | view_dunnage_history, view_receiving_history, view_volvo_history |
| `sp_Reporting_DunnageHistory_GetByDateRange` | view_dunnage_history |
| `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay` | receiving_history |
| `sp_Reporting_ReceivingAnalyticsApp_GetCoverageLines` | receiving_history |
| `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount` | receiving_history |
| `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay` | receiving_history |
| `sp_Reporting_ReceivingAnalyticsApp_GetReceivedPartNumbers` | receiving_history |
| `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals` | receiving_history |
| `sp_Reporting_ReceivingHistory_GetByDateRange` | view_receiving_history |
| `sp_Reporting_VolvoHistory_GetByDateRange` | view_volvo_history |
| `sp_Settings_ReportingRecipients_Delete` | settings_reporting_recipients |
| `sp_Settings_ReportingRecipients_GetAll` | settings_reporting_recipients |
| `sp_Settings_ScheduledReport_GetActive` | reporting_scheduled_reports |
| `sp_Settings_ScheduledReport_GetAll` | reporting_scheduled_reports |
| `sp_Settings_ScheduledReport_GetById` | reporting_scheduled_reports |
| `sp_Settings_ScheduledReport_GetDue` | reporting_scheduled_reports |
| `sp_Settings_VolvoRecipients_Delete` | settings_volvo_recipents |
| `sp_Settings_VolvoRecipients_GetAll` | settings_volvo_recipents |
| `sp_Settings_WeldedCoils_Delete` | settings_weldedcoils |
| `sp_Settings_WeldedCoils_GetAll` | settings_weldedcoils |
| `sp_Settings_WeldedCoils_Insert` | settings_weldedcoils |
| `sp_Settings_WeldedCoils_Update` | settings_weldedcoils |
| `sp_SettingsCore_System_GetByKey` | __system__:routines, __system__:tables, settings_activity, settings_personal, settings_roles, settings_universal, settings_user_roles |
| `sp_SoftwareVersion_GetCurrent` | software_version |
| `sp_Volvo_GeneratedLabelData_ClearToHistory` | volvo_generated_label_data |
| `sp_Volvo_GeneratedLabelData_DeleteByShipment` | volvo_generated_label_data |
| `sp_Volvo_GeneratedLabelData_GetAll` | volvo_generated_label_data |
| `sp_Volvo_GeneratedLabelData_InsertFromHistory` | volvo_generated_label_data, volvo_generated_label_history |
| `sp_Volvo_GeneratedLabelHistory_GetForReprint` | volvo_generated_label_data, volvo_generated_label_history |
| `sp_Volvo_LabelData_ClearToHistory` | volvo_label_data, volvo_label_history, volvo_line_data |
| `sp_volvo_part_check_references` | volvo_label_data, volvo_line_data |
| `sp_Volvo_PartComponent_DeleteByParent` | volvo_part_components |
| `sp_Volvo_PartComponent_Get` | volvo_masterdata, volvo_part_components |
| `sp_Volvo_PartMaster_GetAll` | volvo_masterdata |
| `sp_Volvo_PartMaster_GetById` | volvo_masterdata |
| `sp_Volvo_Settings_Get` | settings_module_volvo |
| `sp_Volvo_Settings_GetAll` | settings_module_volvo |
| `sp_Volvo_Settings_Upsert` | settings_module_volvo |
| `sp_Volvo_Shipment_Complete` | volvo_label_data, volvo_line_data |
| `sp_Volvo_Shipment_Delete` | volvo_label_data |
| `sp_Volvo_Shipment_GetById` | volvo_label_data |
| `sp_Volvo_Shipment_GetHistory` | volvo_label_data, volvo_label_history, volvo_line_data, volvo_line_history |
| `sp_Volvo_Shipment_GetNextShipmentNumber` | volvo_label_data |
| `sp_Volvo_Shipment_GetPending` | volvo_label_data |
| `sp_Volvo_Shipment_Insert` | volvo_label_data |
| `sp_Volvo_ShipmentHistory_Delete` | volvo_label_history |
| `sp_Volvo_ShipmentHistory_GetById` | volvo_label_history |
| `sp_Volvo_ShipmentLine_Delete` | volvo_line_data |
| `sp_Volvo_ShipmentLine_GetByShipment` | volvo_line_data |
| `sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId` | volvo_line_history |
| `view_dunnage_history` | auth_users, dunnage_history, dunnage_parts, dunnage_types |
| `view_dunnage_label_data` | auth_users, dunnage_custom_fields, dunnage_label_data, dunnage_parts, dunnage_quantity_types, dunnage_types |
| `view_receiving_history` | receiving_history, receiving_label_data |
| `view_volvo_history` | volvo_label_data, volvo_label_history, volvo_line_data, volvo_line_history |
| `view_volvo_label_data_history` | volvo_label_data, volvo_label_history, volvo_line_data, volvo_line_history |

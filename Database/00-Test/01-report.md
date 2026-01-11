# Stored Procedure Analysis Report

**Generated:** 2026-01-11 11:56:07
**Database:** mtm_receiving_application
**Server:** localhost:3306

## Summary

- **Total Stored Procedures:** 153
- **Execution Groups:** 52
- **Categories:** 22

## Parameter Statistics

| Type | Count |
|------|-------|
| Total Parameters | 362 |
| IN Parameters | 362 |
| OUT Parameters | 0 |
| INOUT Parameters | 0 |
| SPs without Parameters | 19 |

## Stored Procedures by Category

### Departments (1 procedures)

- **sp_GetDepartments** - No parameters

### DunnageLoads (7 procedures)

- **sp_dunnage_loads_insert** - IN: 4
- **sp_dunnage_loads_insert_batch** - IN: 2
- **sp_dunnage_loads_update** - IN: 3
- **sp_dunnage_loads_get_all** - No parameters
- **sp_dunnage_loads_get_by_date_range** - IN: 2
- **sp_dunnage_loads_get_by_id** - IN: 4
- **sp_dunnage_loads_delete** - IN: 4

### DunnageParts (9 procedures)

- **sp_dunnage_parts_insert** - IN: 5
- **sp_dunnage_parts_update** - IN: 4
- **sp_dunnage_parts_count_transactions** - IN: 4
- **sp_dunnage_parts_get_all** - No parameters
- **sp_dunnage_parts_get_by_id** - IN: 4
- **sp_dunnage_parts_get_by_type** - IN: 4
- **sp_dunnage_parts_get_transaction_count** - IN: 4
- **sp_dunnage_parts_search** - IN: 2
- **sp_dunnage_parts_delete** - IN: 4

### DunnageSpecs (9 procedures)

- **sp_dunnage_specs_insert** - IN: 4
- **sp_dunnage_specs_update** - IN: 3
- **sp_dunnage_specs_count_parts_using_spec** - IN: 2
- **sp_dunnage_specs_get_all** - No parameters
- **sp_dunnage_specs_get_all_keys** - No parameters
- **sp_dunnage_specs_get_by_id** - IN: 4
- **sp_dunnage_specs_get_by_type** - IN: 4
- **sp_dunnage_specs_delete_by_id** - IN: 4
- **sp_dunnage_specs_delete_by_type** - IN: 4

### DunnageTypes (10 procedures)

- **sp_dunnage_types_insert** - IN: 3
- **sp_dunnage_types_update** - IN: 4
- **sp_dunnage_types_check_duplicate** - IN: 2
- **sp_dunnage_types_count_parts** - IN: 4
- **sp_dunnage_types_count_transactions** - IN: 4
- **sp_dunnage_types_get_all** - No parameters
- **sp_dunnage_types_get_by_id** - IN: 4
- **sp_dunnage_types_get_part_count** - IN: 4
- **sp_dunnage_types_get_transaction_count** - IN: 4
- **sp_dunnage_types_delete** - IN: 4

### InventoriedDunnage (6 procedures)

- **sp_inventoried_dunnage_insert** - IN: 4
- **sp_inventoried_dunnage_update** - IN: 4
- **sp_inventoried_dunnage_check** - IN: 4
- **sp_inventoried_dunnage_get_all** - No parameters
- **sp_inventoried_dunnage_get_by_part** - IN: 4
- **sp_inventoried_dunnage_delete** - IN: 4

### Other (26 procedures)

- **carrier_delivery_label_Insert** - IN: 7
- **dunnage_line_Insert** - IN: 7
- **sp_CreateNewUser** - IN: 9
- **sp_custom_fields_insert** - IN: 8
- **sp_LogUserActivity** - IN: 4
- **sp_routing_usage_increment** - IN: 2
- **sp_RoutingRule_Insert** - IN: 5
- **sp_RoutingRule_Update** - IN: 5
- **sp_seed_user_default_modes** - IN: 4
- **sp_update_user_default_dunnage_mode** - IN: 2
- **sp_update_user_default_mode** - IN: 2
- **sp_update_user_default_receiving_mode** - IN: 2
- **sp_UpsertUser** - IN: 10
- **sp_ValidateUserPin** - IN: 2
- **sp_custom_fields_get_by_type** - IN: 4
- **sp_get_user_default_mode** - IN: 4
- **sp_GetReceivingHistory** - IN: 3
- **sp_GetSharedTerminalNames** - No parameters
- **sp_GetUserByWindowsUsername** - IN: 4
- **sp_routing_label_get_history** - IN: 2
- **sp_RoutingRule_FindMatch** - IN: 2
- **sp_RoutingRule_GetAll** - No parameters
- **sp_RoutingRule_GetById** - IN: 4
- **sp_RoutingRule_GetByPartNumber** - IN: 4
- **sp_volvo_part_check_references** - IN: 4
- **sp_RoutingRule_Delete** - IN: 4

### PackageTypes (14 procedures)

- **sp_GetPackageTypePreference** - IN: 4
- **sp_PackageType_GetAll** - No parameters
- **sp_PackageType_GetById** - IN: 4
- **sp_PackageType_Insert** - IN: 3
- **sp_PackageType_Update** - IN: 3
- **sp_PackageType_UsageCount** - IN: 4
- **sp_PackageTypeMappings_GetAll** - No parameters
- **sp_PackageTypeMappings_GetByPrefix** - IN: 4
- **sp_PackageTypeMappings_Insert** - IN: 5
- **sp_PackageTypeMappings_Update** - IN: 4
- **sp_SavePackageTypePreference** - IN: 4
- **sp_DeletePackageTypePreference** - IN: 4
- **sp_PackageType_Delete** - IN: 4
- **sp_PackageTypeMappings_Delete** - IN: 4

### Preferences (2 procedures)

- **sp_routing_user_preference_save** - IN: 3
- **sp_routing_user_preference_get** - IN: 4

### ReceivingLines (1 procedures)

- **receiving_line_Insert** - IN: 10

### ReceivingLoads (4 procedures)

- **sp_InsertReceivingLoad** - IN: 13
- **sp_UpdateReceivingLoad** - IN: 13
- **sp_GetAllReceivingLoads** - IN: 2
- **sp_DeleteReceivingLoad** - IN: 4

### Reports (9 procedures)

- **sp_ScheduledReport_Insert** - IN: 5
- **sp_ScheduledReport_ToggleActive** - IN: 2
- **sp_ScheduledReport_Update** - IN: 5
- **sp_ScheduledReport_UpdateLastRun** - IN: 3
- **sp_ScheduledReport_GetActive** - No parameters
- **sp_ScheduledReport_GetAll** - No parameters
- **sp_ScheduledReport_GetById** - IN: 4
- **sp_ScheduledReport_GetDue** - No parameters
- **sp_ScheduledReport_Delete** - IN: 4

### RoutingHistory (1 procedures)

- **sp_routing_label_history_insert** - IN: 5

### RoutingLabels (9 procedures)

- **sp_routing_label_archive** - IN: 4
- **sp_routing_label_insert** - IN: 8
- **sp_routing_label_mark_exported** - IN: 4
- **sp_routing_label_update** - IN: 7
- **sp_routing_label_check_duplicate** - IN: 4
- **sp_routing_label_get_all** - IN: 2
- **sp_routing_label_get_by_id** - IN: 4
- **sp_routing_label_get_today** - IN: 4
- **sp_routing_label_delete** - IN: 4

### RoutingReasons (1 procedures)

- **sp_routing_other_reason_get_all_active** - No parameters

### RoutingRecipients (6 procedures)

- **sp_routing_recipient_insert** - IN: 2
- **sp_routing_recipient_update** - IN: 4
- **sp_routing_recipient_get_all** - No parameters
- **sp_routing_recipient_get_all_active** - No parameters
- **sp_routing_recipient_get_by_name** - IN: 4
- **sp_routing_recipient_get_top_by_usage** - IN: 2

### Settings (13 procedures)

- **sp_SystemSettings_ResetToDefault** - IN: 4
- **sp_SystemSettings_SetLocked** - IN: 5
- **sp_SystemSettings_UpdateValue** - IN: 5
- **sp_volvo_settings_reset** - IN: 2
- **sp_volvo_settings_upsert** - IN: 3
- **sp_SettingsAuditLog_Get** - IN: 2
- **sp_SettingsAuditLog_GetBySetting** - IN: 2
- **sp_SettingsAuditLog_GetByUser** - IN: 2
- **sp_SystemSettings_GetAll** - No parameters
- **sp_SystemSettings_GetByCategory** - IN: 4
- **sp_SystemSettings_GetByKey** - IN: 2
- **sp_volvo_settings_get** - IN: 4
- **sp_volvo_settings_get_all** - IN: 4

### Users (7 procedures)

- **sp_user_preferences_upsert** - IN: 3
- **sp_UserSettings_Reset** - IN: 2
- **sp_UserSettings_ResetAll** - IN: 2
- **sp_UserSettings_Set** - IN: 3
- **sp_user_preferences_get_recent_icons** - IN: 2
- **sp_UserSettings_Get** - IN: 3
- **sp_UserSettings_GetAllForUser** - IN: 4

### VolvoComponents (3 procedures)

- **sp_volvo_part_component_insert** - IN: 3
- **sp_volvo_part_component_get** - IN: 4
- **sp_volvo_part_component_delete_by_parent** - IN: 4

### VolvoParts (5 procedures)

- **sp_volvo_part_master_insert** - IN: 3
- **sp_volvo_part_master_set_active** - IN: 2
- **sp_volvo_part_master_update** - IN: 2
- **sp_volvo_part_master_get_all** - IN: 4
- **sp_volvo_part_master_get_by_id** - IN: 4

### VolvoShipmentLines (4 procedures)

- **sp_volvo_shipment_line_insert** - IN: 8
- **sp_volvo_shipment_line_update** - IN: 6
- **sp_volvo_shipment_line_get_by_shipment** - IN: 4
- **sp_volvo_shipment_line_delete** - IN: 4

### VolvoShipments (6 procedures)

- **sp_volvo_shipment_complete** - IN: 3
- **sp_volvo_shipment_insert** - IN: 3
- **sp_volvo_shipment_update** - IN: 2
- **sp_volvo_shipment_get_pending** - No parameters
- **sp_volvo_shipment_history_get** - IN: 3
- **sp_volvo_shipment_delete** - IN: 4

## Execution Order Groups

| Order | Category | Count | Stored Procedures |
|-------|----------|-------|-------------------|
| 10 | Users | 4 | sp_user_preferences_upsert, sp_UserSettings_Reset, sp_UserSettings_ResetAll, sp_UserSettings_Set |
| 25 | DunnageTypes | 2 | sp_dunnage_types_insert, sp_dunnage_types_update |
| 30 | DunnageSpecs | 2 | sp_dunnage_specs_insert, sp_dunnage_specs_update |
| 35 | DunnageParts | 2 | sp_dunnage_parts_insert, sp_dunnage_parts_update |
| 40 | VolvoParts | 3 | sp_volvo_part_master_insert, sp_volvo_part_master_set_active, sp_volvo_part_master_update |
| 45 | RoutingRecipients | 2 | sp_routing_recipient_insert, sp_routing_recipient_update |
| 100 | ReceivingLoads | 2 | sp_InsertReceivingLoad, sp_UpdateReceivingLoad |
| 120 | ReceivingLines | 1 | receiving_line_Insert |
| 130 | DunnageLoads | 3 | sp_dunnage_loads_insert, sp_dunnage_loads_insert_batch, sp_dunnage_loads_update |
| 140 | InventoriedDunnage | 2 | sp_inventoried_dunnage_insert, sp_inventoried_dunnage_update |
| 150 | RoutingLabels | 4 | sp_routing_label_archive, sp_routing_label_insert, sp_routing_label_mark_exported, sp_routing_lab... |
| 160 | RoutingHistory | 1 | sp_routing_label_history_insert |
| 170 | VolvoShipments | 3 | sp_volvo_shipment_complete, sp_volvo_shipment_insert, sp_volvo_shipment_update |
| 180 | VolvoShipmentLines | 2 | sp_volvo_shipment_line_insert, sp_volvo_shipment_line_update |
| 190 | VolvoComponents | 1 | sp_volvo_part_component_insert |
| 200 | Preferences | 1 | sp_routing_user_preference_save |
| 210 | Settings | 5 | sp_SystemSettings_ResetToDefault, sp_SystemSettings_SetLocked, sp_SystemSettings_UpdateValue, sp_... |
| 220 | Reports | 4 | sp_ScheduledReport_Insert, sp_ScheduledReport_ToggleActive, sp_ScheduledReport_Update, sp_Schedul... |
| 999 | Other | 14 | carrier_delivery_label_Insert, dunnage_line_Insert, sp_CreateNewUser, sp_custom_fields_insert, sp... |
| 1010 | Users | 3 | sp_user_preferences_get_recent_icons, sp_UserSettings_Get, sp_UserSettings_GetAllForUser |
| 1015 | Departments | 1 | sp_GetDepartments |
| 1020 | PackageTypes | 11 | sp_GetPackageTypePreference, sp_PackageType_GetAll, sp_PackageType_GetById, sp_PackageType_Insert... |
| 1025 | DunnageTypes | 7 | sp_dunnage_types_check_duplicate, sp_dunnage_types_count_parts, sp_dunnage_types_count_transactio... |
| 1030 | DunnageSpecs | 5 | sp_dunnage_specs_count_parts_using_spec, sp_dunnage_specs_get_all, sp_dunnage_specs_get_all_keys,... |
| 1035 | DunnageParts | 6 | sp_dunnage_parts_count_transactions, sp_dunnage_parts_get_all, sp_dunnage_parts_get_by_id, sp_dun... |
| 1040 | VolvoParts | 2 | sp_volvo_part_master_get_all, sp_volvo_part_master_get_by_id |
| 1045 | RoutingRecipients | 4 | sp_routing_recipient_get_all, sp_routing_recipient_get_all_active, sp_routing_recipient_get_by_na... |
| 1050 | RoutingReasons | 1 | sp_routing_other_reason_get_all_active |
| 1100 | ReceivingLoads | 1 | sp_GetAllReceivingLoads |
| 1130 | DunnageLoads | 3 | sp_dunnage_loads_get_all, sp_dunnage_loads_get_by_date_range, sp_dunnage_loads_get_by_id |
| 1140 | InventoriedDunnage | 3 | sp_inventoried_dunnage_check, sp_inventoried_dunnage_get_all, sp_inventoried_dunnage_get_by_part |
| 1150 | RoutingLabels | 4 | sp_routing_label_check_duplicate, sp_routing_label_get_all, sp_routing_label_get_by_id, sp_routin... |
| 1170 | VolvoShipments | 2 | sp_volvo_shipment_get_pending, sp_volvo_shipment_history_get |
| 1180 | VolvoShipmentLines | 1 | sp_volvo_shipment_line_get_by_shipment |
| 1190 | VolvoComponents | 1 | sp_volvo_part_component_get |
| 1200 | Preferences | 1 | sp_routing_user_preference_get |
| 1210 | Settings | 8 | sp_SettingsAuditLog_Get, sp_SettingsAuditLog_GetBySetting, sp_SettingsAuditLog_GetByUser, sp_Syst... |
| 1220 | Reports | 4 | sp_ScheduledReport_GetActive, sp_ScheduledReport_GetAll, sp_ScheduledReport_GetById, sp_Scheduled... |
| 1999 | Other | 11 | sp_custom_fields_get_by_type, sp_get_user_default_mode, sp_GetReceivingHistory, sp_GetSharedTermi... |
| 2025 | DunnageTypes | 1 | sp_dunnage_types_delete |
| 2030 | DunnageSpecs | 2 | sp_dunnage_specs_delete_by_id, sp_dunnage_specs_delete_by_type |
| 2035 | DunnageParts | 1 | sp_dunnage_parts_delete |
| 2100 | ReceivingLoads | 1 | sp_DeleteReceivingLoad |
| 2130 | DunnageLoads | 1 | sp_dunnage_loads_delete |
| 2140 | InventoriedDunnage | 1 | sp_inventoried_dunnage_delete |
| 2150 | RoutingLabels | 1 | sp_routing_label_delete |
| 2170 | VolvoShipments | 1 | sp_volvo_shipment_delete |
| 2180 | VolvoShipmentLines | 1 | sp_volvo_shipment_line_delete |
| 2190 | VolvoComponents | 1 | sp_volvo_part_component_delete_by_parent |
| 2220 | Reports | 1 | sp_ScheduledReport_Delete |
| 2999 | Other | 1 | sp_RoutingRule_Delete |
| 3020 | PackageTypes | 3 | sp_DeletePackageTypePreference, sp_PackageType_Delete, sp_PackageTypeMappings_Delete |

## Notes

- **Execution Order:** Lower numbers execute first (10-999)
- **Read Operations:** Offset by +1000 (can run anytime)
- **Delete Operations:** Offset by +2000 (run last)
- **Mock Data:** Review `01-mock-data.json` to customize parameter values
- **Dependencies:** Use `-UseExecutionOrder` flag when testing to avoid FK constraint errors

## Complex Stored Procedures (8+ parameters)

| SP Name | Total Params | IN | OUT | INOUT |
|---------|--------------|-----|-----|-------|
| sp_InsertReceivingLoad | 13 | 13 | 0 | 0 |
| sp_UpdateReceivingLoad | 13 | 13 | 0 | 0 |
| receiving_line_Insert | 10 | 10 | 0 | 0 |
| sp_UpsertUser | 10 | 10 | 0 | 0 |
| sp_CreateNewUser | 9 | 9 | 0 | 0 |
| sp_custom_fields_insert | 8 | 8 | 0 | 0 |
| sp_volvo_shipment_line_insert | 8 | 8 | 0 | 0 |
| sp_routing_label_insert | 8 | 8 | 0 | 0 |


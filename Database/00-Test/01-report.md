# Stored Procedure Analysis Report

**Generated:** 2026-01-11 16:28:50
**Database:** mtm_receiving_application
**Server:** localhost:3306

## Summary

- **Total Stored Procedures:** 153
- **Execution Groups:** 52
- **Categories:** 22

## Parameter Statistics

| Type                   | Count            |
| ---------------------- | ---------------- |
| Total Parameters       | 407 |
| IN Parameters          | 359    |
| OUT Parameters         | 48   |
| INOUT Parameters       | 0 |
| SPs without Parameters | 18    |

## Stored Procedures by Category

### Departments (1 procedures)

- **sp_GetDepartments** - No parameters

### DunnageLoads (7 procedures)

- **sp_dunnage_history_insert** - IN: 4
- **sp_dunnage_history_insert_batch** - IN: 2
- **sp_dunnage_history_update** - IN: 3
- **sp_dunnage_history_get_all** - No parameters
- **sp_dunnage_history_get_by_date_range** - IN: 2
- **sp_dunnage_history_get_by_id** - IN: 4
- **sp_dunnage_history_delete** - IN: 4

### DunnageParts (9 procedures)

- **sp_dunnage_parts_insert** - IN: 5 OUT: 4
- **sp_dunnage_parts_update** - IN: 4
- **sp_dunnage_parts_count_transactions** - IN: 4
- **sp_dunnage_parts_get_all** - No parameters
- **sp_dunnage_parts_get_by_id** - IN: 4
- **sp_dunnage_parts_get_by_type** - IN: 4
- **sp_dunnage_parts_get_transaction_count** - IN: 4 OUT: 4
- **sp_dunnage_parts_search** - IN: 2
- **sp_dunnage_parts_delete** - IN: 4

### DunnageSpecs (9 procedures)

- **sp_dunnage_specs_insert** - IN: 4 OUT: 4
- **sp_dunnage_specs_update** - IN: 3
- **sp_dunnage_specs_count_parts_using_spec** - IN: 2
- **sp_dunnage_specs_get_all** - No parameters
- **sp_dunnage_specs_get_all_keys** - No parameters
- **sp_dunnage_specs_get_by_id** - IN: 4
- **sp_dunnage_specs_get_by_type** - IN: 4
- **sp_dunnage_specs_delete_by_id** - IN: 4
- **sp_dunnage_specs_delete_by_type** - IN: 4

### DunnageTypes (10 procedures)

- **sp_dunnage_types_insert** - IN: 3 OUT: 4
- **sp_dunnage_types_update** - IN: 4
- **sp_dunnage_types_check_duplicate** - IN: 2 OUT: 4
- **sp_dunnage_types_count_parts** - IN: 4
- **sp_dunnage_types_count_transactions** - IN: 4
- **sp_dunnage_types_get_all** - No parameters
- **sp_dunnage_types_get_by_id** - IN: 4
- **sp_dunnage_types_get_part_count** - IN: 4 OUT: 4
- **sp_dunnage_types_get_transaction_count** - IN: 4 OUT: 4
- **sp_dunnage_types_delete** - IN: 4

### InventoriedDunnage (6 procedures)

- **sp_dunnage_requires_inventory_insert** - IN: 4 OUT: 4
- **sp_dunnage_requires_inventory_update** - IN: 4
- **sp_dunnage_requires_inventory_check** - IN: 4
- **sp_dunnage_requires_inventory_get_all** - No parameters
- **sp_dunnage_requires_inventory_get_by_part** - IN: 4
- **sp_dunnage_requires_inventory_delete** - IN: 4

### Other (26 procedures)

- **dunnage_line_Insert** - IN: 7 OUT: 2
- **sp_CreateNewUser** - IN: 9 OUT: 4
- **sp_custom_fields_insert** - IN: 8 OUT: 3
- **sp_LogUserActivity** - IN: 4
- **sp_routing_usage_increment** - IN: 2 OUT: 2
- **sp_RoutingRule_Insert** - IN: 5
- **sp_RoutingRule_Update** - IN: 5
- **sp_seed_user_default_modes** - IN: 4
- **sp_update_user_default_dunnage_mode** - IN: 2
- **sp_update_user_default_mode** - IN: 2
- **sp_update_user_default_receiving_mode** - IN: 2
- **sp_UpsertUser** - IN: 10
- **sp_UpsertWorkstationConfig** - IN: 4
- **sp_ValidateUserPin** - IN: 2
- **sp_dunnage_custom_fields_get_by_type** - IN: 4
- **sp_get_user_default_mode** - IN: 4
- **sp_GetReceivingHistory** - IN: 3
- **sp_GetSharedTerminalNames** - No parameters
- **sp_GetUserByWindowsUsername** - IN: 4
- **sp_routing_label_get_history** - IN: 2
- **sp_RoutingRule_FindMatch** - IN: 2
- **sp_RoutingRule_GetAll** - No parameters
- **sp_RoutingRule_GetById** - IN: 4
- **sp_RoutingRule_GetByPartNumber** - IN: 4
- **sp_volvo_part_check_references** - IN: 4 OUT: 4
- **sp_RoutingRule_Delete** - IN: 4

### PackageTypes (14 procedures)

- **sp_GetPackageTypePreference** - IN: 4
- **sp_PackageType_GetAll** - No parameters
- **sp_PackageType_GetById** - IN: 4
- **sp_PackageType_Insert** - IN: 3
- **sp_PackageType_Update** - IN: 3
- **sp_PackageType_UsageCount** - IN: 4
- **sp_Receiving_PackageTypeMappings_GetAll** - No parameters
- **sp_Receiving_PackageTypeMappings_GetByPrefix** - IN: 4
- **sp_Receiving_PackageTypeMappings_Insert** - IN: 5
- **sp_Receiving_PackageTypeMappings_Update** - IN: 4
- **sp_SavePackageTypePreference** - IN: 4
- **sp_DeletePackageTypePreference** - IN: 4
- **sp_PackageType_Delete** - IN: 4
- **sp_Receiving_PackageTypeMappings_Delete** - IN: 4

### Preferences (2 procedures)

- **sp_routing_user_preference_save** - IN: 3 OUT: 2
- **sp_routing_user_preference_get** - IN: 4 OUT: 2

### ReceivingLines (1 procedures)

- **receiving_line_Insert** - IN: 10 OUT: 2

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

- **sp_routing_history_insert** - IN: 5 OUT: 2

### RoutingLabels (9 procedures)

- **sp_routing_label_archive** - IN: 4 OUT: 2
- **sp_routing_label_insert** - IN: 8 OUT: 2
- **sp_routing_label_mark_exported** - IN: 4 OUT: 2
- **sp_routing_label_update** - IN: 7 OUT: 4
- **sp_routing_label_check_duplicate** - IN: 4 OUT: 2
- **sp_routing_label_get_all** - IN: 2 OUT: 2
- **sp_routing_label_get_by_id** - IN: 4 OUT: 2
- **sp_routing_label_get_today** - IN: 4
- **sp_routing_label_delete** - IN: 4 OUT: 4

### RoutingReasons (1 procedures)

- **sp_routing_other_reason_get_all_active** - OUT: 2

### RoutingRecipients (6 procedures)

- **sp_routing_recipient_insert** - IN: 2 OUT: 2
- **sp_routing_recipient_update** - IN: 4 OUT: 4
- **sp_routing_recipient_get_all** - No parameters
- **sp_routing_recipient_get_all_active** - No parameters
- **sp_routing_recipient_get_by_name** - IN: 4
- **sp_routing_recipient_get_top_by_usage** - IN: 2

### Settings (13 procedures)

- **sp_settings_module_volvo_reset** - IN: 2
- **sp_settings_module_volvo_upsert** - IN: 3
- **sp_SystemSettings_ResetToDefault** - IN: 4
- **sp_SystemSettings_SetLocked** - IN: 5
- **sp_SystemSettings_UpdateValue** - IN: 5
- **sp_settings_module_volvo_get** - IN: 4
- **sp_settings_module_volvo_get_all** - IN: 4
- **sp_SettingsAuditLog_Get** - IN: 2
- **sp_SettingsAuditLog_GetBySetting** - IN: 2
- **sp_SettingsAuditLog_GetByUser** - IN: 2
- **sp_SystemSettings_GetAll** - No parameters
- **sp_SystemSettings_GetByCategory** - IN: 4
- **sp_SystemSettings_GetByKey** - IN: 2

### Users (7 procedures)

- **sp_user_preferences_upsert** - IN: 3 OUT: 2
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
- **sp_volvo_shipment_insert** - IN: 3 OUT: 2
- **sp_volvo_shipment_update** - IN: 2
- **sp_volvo_shipment_get_pending** - No parameters
- **sp_volvo_shipment_history_get** - IN: 3
- **sp_volvo_shipment_delete** - IN: 4


## Execution Order Groups

| Order | Category | Count | Stored Procedures |
| ----- | -------- | ----- | ----------------- |
| 10 | Users | 4 | sp_user_preferences_upsert, sp_UserSettings_Reset, sp_UserSettings_ResetAll, sp_UserSettings_Set |
| 25 | DunnageTypes | 2 | sp_dunnage_types_insert, sp_dunnage_types_update |
| 30 | DunnageSpecs | 2 | sp_dunnage_specs_insert, sp_dunnage_specs_update |
| 35 | DunnageParts | 2 | sp_dunnage_parts_insert, sp_dunnage_parts_update |
| 40 | VolvoParts | 3 | sp_volvo_part_master_insert, sp_volvo_part_master_set_active, sp_volvo_part_master_update |
| 45 | RoutingRecipients | 2 | sp_routing_recipient_insert, sp_routing_recipient_update |
| 100 | ReceivingLoads | 2 | sp_InsertReceivingLoad, sp_UpdateReceivingLoad |
| 120 | ReceivingLines | 1 | receiving_line_Insert |
| 130 | DunnageLoads | 3 | sp_dunnage_history_insert, sp_dunnage_history_insert_batch, sp_dunnage_history_update |
| 140 | InventoriedDunnage | 2 | sp_dunnage_requires_inventory_insert, sp_dunnage_requires_inventory_update |
| 150 | RoutingLabels | 4 | sp_routing_label_archive, sp_routing_label_insert, sp_routing_label_mark_exported, sp_routing_lab... |
| 160 | RoutingHistory | 1 | sp_routing_history_insert |
| 170 | VolvoShipments | 3 | sp_volvo_shipment_complete, sp_volvo_shipment_insert, sp_volvo_shipment_update |
| 180 | VolvoShipmentLines | 2 | sp_volvo_shipment_line_insert, sp_volvo_shipment_line_update |
| 190 | VolvoComponents | 1 | sp_volvo_part_component_insert |
| 200 | Preferences | 1 | sp_routing_user_preference_save |
| 210 | Settings | 5 | sp_settings_module_volvo_reset, sp_settings_module_volvo_upsert, sp_SystemSettings_ResetToDefault... |
| 220 | Reports | 4 | sp_ScheduledReport_Insert, sp_ScheduledReport_ToggleActive, sp_ScheduledReport_Update, sp_Schedul... |
| 999 | Other | 14 | dunnage_line_Insert, sp_CreateNewUser, sp_custom_fields_insert, sp_LogUserActivity, sp_routing_us... |
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
| 1130 | DunnageLoads | 3 | sp_dunnage_history_get_all, sp_dunnage_history_get_by_date_range, sp_dunnage_history_get_by_id |
| 1140 | InventoriedDunnage | 3 | sp_dunnage_requires_inventory_check, sp_dunnage_requires_inventory_get_all, sp_dunnage_requires_i... |
| 1150 | RoutingLabels | 4 | sp_routing_label_check_duplicate, sp_routing_label_get_all, sp_routing_label_get_by_id, sp_routin... |
| 1170 | VolvoShipments | 2 | sp_volvo_shipment_get_pending, sp_volvo_shipment_history_get |
| 1180 | VolvoShipmentLines | 1 | sp_volvo_shipment_line_get_by_shipment |
| 1190 | VolvoComponents | 1 | sp_volvo_part_component_get |
| 1200 | Preferences | 1 | sp_routing_user_preference_get |
| 1210 | Settings | 8 | sp_settings_module_volvo_get, sp_settings_module_volvo_get_all, sp_SettingsAuditLog_Get, sp_Setti... |
| 1220 | Reports | 4 | sp_ScheduledReport_GetActive, sp_ScheduledReport_GetAll, sp_ScheduledReport_GetById, sp_Scheduled... |
| 1999 | Other | 11 | sp_dunnage_custom_fields_get_by_type, sp_get_user_default_mode, sp_GetReceivingHistory, sp_GetSharedTermi... |
| 2025 | DunnageTypes | 1 | sp_dunnage_types_delete |
| 2030 | DunnageSpecs | 2 | sp_dunnage_specs_delete_by_id, sp_dunnage_specs_delete_by_type |
| 2035 | DunnageParts | 1 | sp_dunnage_parts_delete |
| 2100 | ReceivingLoads | 1 | sp_DeleteReceivingLoad |
| 2130 | DunnageLoads | 1 | sp_dunnage_history_delete |
| 2140 | InventoriedDunnage | 1 | sp_dunnage_requires_inventory_delete |
| 2150 | RoutingLabels | 1 | sp_routing_label_delete |
| 2170 | VolvoShipments | 1 | sp_volvo_shipment_delete |
| 2180 | VolvoShipmentLines | 1 | sp_volvo_shipment_line_delete |
| 2190 | VolvoComponents | 1 | sp_volvo_part_component_delete_by_parent |
| 2220 | Reports | 1 | sp_ScheduledReport_Delete |
| 2999 | Other | 1 | sp_RoutingRule_Delete |
| 3020 | PackageTypes | 3 | sp_DeletePackageTypePreference, sp_PackageType_Delete, sp_Receiving_PackageTypeMappings_Delete |

## Notes

- **Execution Order:** Lower numbers execute first (10-999)
- **Read Operations:** Offset by +1000 (can run anytime)
- **Delete Operations:** Offset by +2000 (run last)
- **Mock Data:** Review `01-mock-data.json` to customize parameter values
- **Dependencies:** Use `-UseExecutionOrder` flag when testing to avoid FK constraint errors

## Complex Stored Procedures (8+ parameters)

| SP Name | Total Params | IN | OUT | INOUT |
|---------|--------------|-----|-----|-------|
| sp_UpdateReceivingLoad | 13 | 13 | 0 | 0 |
| sp_InsertReceivingLoad | 13 | 13 | 0 | 0 |
| receiving_line_Insert | 12 | 10 | 2 | 0 |
| sp_custom_fields_insert | 11 | 8 | 3 | 0 |
| sp_routing_label_insert | 10 | 8 | 2 | 0 |
| sp_UpsertUser | 10 | 10 | 0 | 0 |
| sp_CreateNewUser | 10 | 9 | 4 | 0 |
| dunnage_line_Insert | 9 | 7 | 2 | 0 |
| sp_volvo_shipment_line_insert | 8 | 8 | 0 | 0 |
| sp_routing_label_update | 8 | 7 | 4 | 0 |

## 🔧 Fix Checklist

### Stored Procedures Requiring Updates

| Fixed | SP Name | Current Params | Expected Params | Missing Params | File Path | DAO Class | Issue Type | Priority | Fix Notes |
|-------|---------|----------------|-----------------|----------------|-----------|-----------|------------|----------|-----------|
| [ ] | dunnage_line_Insert | 7 IN | 9 total | 2 OUT/INOUT | Database/StoredProcedures/dunnage_line_Insert.sql | Dao_Dunnage Line | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | receiving_line_Insert | 10 IN | 12 total | 2 OUT/INOUT | Database/StoredProcedures/receiving_line_Insert.sql | Dao_Receiving Line | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_CreateNewUser | 9 IN | 10 total | 4 OUT/INOUT | Database/StoredProcedures/sp_CreateNewUser.sql |  | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_custom_fields_insert | 8 IN | 11 total | 3 OUT/INOUT | Database/StoredProcedures/sp_custom_fields_insert.sql | Dao_Custom Fields | OUT Parameters | High | Update DAO to handle 3 OUT params |
| [ ] | sp_dunnage_parts_get_transaction_count | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_dunnage_parts_get_transaction_count.sql | Dao_Dunnage Parts | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_dunnage_parts_insert | 5 IN | 6 total | 4 OUT/INOUT | Database/StoredProcedures/sp_dunnage_parts_insert.sql | Dao_Dunnage Parts | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_dunnage_requires_inventory_insert | 4 IN | 5 total | 4 OUT/INOUT | Database/StoredProcedures/sp_dunnage_requires_inventory_insert.sql | Dao_Dunnage Requires Inventory | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_dunnage_specs_insert | 4 IN | 5 total | 4 OUT/INOUT | Database/StoredProcedures/sp_dunnage_specs_insert.sql | Dao_Dunnage Specs | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_dunnage_types_check_duplicate | 2 IN | 3 total | 4 OUT/INOUT | Database/StoredProcedures/sp_dunnage_types_check_duplicate.sql | Dao_Dunnage Types | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_dunnage_types_get_part_count | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_dunnage_types_get_part_count.sql | Dao_Dunnage Types | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_dunnage_types_get_transaction_count | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_dunnage_types_get_transaction_count.sql | Dao_Dunnage Types | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_dunnage_types_insert | 3 IN | 4 total | 4 OUT/INOUT | Database/StoredProcedures/sp_dunnage_types_insert.sql | Dao_Dunnage Types | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_routing_history_insert | 5 IN | 7 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_history_insert.sql | Dao_Routing History | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_archive | 4 IN | 3 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_archive.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_check_duplicate | 4 IN | 6 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_check_duplicate.sql | Dao_Routing Label | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_delete | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_routing_label_delete.sql | Dao_Routing Label | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_routing_label_get_all | 2 IN | 4 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_get_all.sql | Dao_Routing Label | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_get_by_id | 4 IN | 3 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_get_by_id.sql | Dao_Routing Label | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_insert | 8 IN | 10 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_insert.sql | Dao_Routing Label | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_mark_exported | 4 IN | 3 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_mark_exported.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_update | 7 IN | 8 total | 4 OUT/INOUT | Database/StoredProcedures/sp_routing_label_update.sql | Dao_Routing Label | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_routing_other_reason_get_all_active | 0 IN | 2 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_other_reason_get_all_active.sql | Dao_Routing Other Reason | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_recipient_insert | 2 IN | 4 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_recipient_insert.sql | Dao_Routing Recipient | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_recipient_update | 4 IN | 5 total | 4 OUT/INOUT | Database/StoredProcedures/sp_routing_recipient_update.sql | Dao_Routing Recipient | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_routing_usage_increment | 2 IN | 4 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_usage_increment.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_user_preference_get | 4 IN | 3 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_user_preference_get.sql | Dao_Routing User Preference | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_user_preference_save | 3 IN | 5 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_user_preference_save.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_user_preferences_upsert | 3 IN | 5 total | 2 OUT/INOUT | Database/StoredProcedures/sp_user_preferences_upsert.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_volvo_part_check_references | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_volvo_part_check_references.sql | Dao_Volvo Part | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_volvo_shipment_insert | 3 IN | 5 total | 2 OUT/INOUT | Database/StoredProcedures/sp_volvo_shipment_insert.sql | Dao_Volvo Shipment | OUT Parameters | Medium | Update DAO to handle 2 OUT params |

### DAO Files Requiring Updates

| Fixed | DAO Class | File Path | Related SPs | Param Mismatches | Methods to Update | Issue Summary | Priority |
|-------|-----------|-----------|-------------|------------------|-------------------|---------------|----------|
| [ ] | Dao_Custom Fields | **/Dao_Custom Fields.cs (search required) | sp_custom_fields_insert | 3 | Insert*Async | 3 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Dunnage Line | **/Dao_Dunnage Line.cs (search required) | dunnage_line_Insert | 2 | Insert*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Dunnage Parts | **/Dao_Dunnage Parts.cs (search required) | sp_dunnage_parts_get_transaction_count, sp_dunnage_parts_... | 8 | Get*Async, Insert*Async | 8 OUT/INOUT params across 2 SP(s) | High |
| [ ] | Dao_Dunnage Requires Inventory | **/Dao_Dunnage Requires Inventory.cs (search required) | sp_dunnage_requires_inventory_insert | 4 | Insert*Async | 4 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Dunnage Specs | **/Dao_Dunnage Specs.cs (search required) | sp_dunnage_specs_insert | 4 | Insert*Async | 4 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Dunnage Types | **/Dao_Dunnage Types.cs (search required) | sp_dunnage_types_check_duplicate, sp_dunnage_types_get_pa... | 16 | Get*Async, Insert*Async | 16 OUT/INOUT params across 4 SP(s) | High |
| [ ] | Dao_Receiving Line | **/Dao_Receiving Line.cs (search required) | receiving_line_Insert | 2 | Insert*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Routing History | **/Dao_Routing History.cs (search required) | sp_routing_history_insert | 2 | Insert*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Routing Label | **/Dao_Routing Label.cs (search required) | sp_routing_label_check_duplicate, sp_routing_label_delete... | 16 | Delete*Async, Get*Async, Insert*Async, Update*Async | 16 OUT/INOUT params across 6 SP(s) | High |
| [ ] | Dao_Routing Other Reason | **/Dao_Routing Other Reason.cs (search required) | sp_routing_other_reason_get_all_active | 2 | Get*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Routing Recipient | **/Dao_Routing Recipient.cs (search required) | sp_routing_recipient_insert, sp_routing_recipient_update | 6 | Insert*Async, Update*Async | 6 OUT/INOUT params across 2 SP(s) | High |
| [ ] | Dao_Routing User Preference | **/Dao_Routing User Preference.cs (search required) | sp_routing_user_preference_get | 2 | Get*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Volvo Part | **/Dao_Volvo Part.cs (search required) | sp_volvo_part_check_references | 4 |  | 4 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Volvo Shipment | **/Dao_Volvo Shipment.cs (search required) | sp_volvo_shipment_insert | 2 | Insert*Async | 2 OUT/INOUT params across 1 SP(s) | Low |

### Database Schema Issues

| Fixed | SP Name | Missing Column/Table | Schema File | Table Name | Expected Column | Current Status | SQL Fix Required | Priority | Fix Notes |
|-------|---------|----------------------|-------------|------------|-----------------|----------------|------------------|----------|-----------|
| [ ] | sp_dunnage_specs_get_all_keys |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_dunnage_type_Delete |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_dunnage_type_GetAll |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_dunnage_type_GetById |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_dunnage_type_Insert |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_dunnage_type_Update |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_dunnage_type_UsageCount |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_routing_label_get_history |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_routing_label_get_today |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_routing_recipient_get_all |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_routing_recipient_get_by_name |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_update_user_default_dunnage_mode |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_update_user_default_mode |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |
| [ ] | sp_update_user_default_receiving_mode |  | Database/Schemas/* (search for table) |  |  | Missing in DB | CREATE TABLE | Critical | Review SP definition and update schema or SP code |

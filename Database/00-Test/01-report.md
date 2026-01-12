# Stored Procedure Analysis Report

**Generated:** 2026-01-11 18:38:48
**Database:** mtm_receiving_application
**Server:** localhost:3306

## Summary

- **Total Stored Procedures:** 149
- **Execution Groups:** 33
- **Categories:** 14

## Parameter Statistics

| Type                   | Count            |
| ---------------------- | ---------------- |
| Total Parameters       | 407 |
| IN Parameters          | 357    |
| OUT Parameters         | 50   |
| INOUT Parameters       | 0 |
| SPs without Parameters | 16    |

## Stored Procedures by Category

### Departments (1 procedures)

- **sp_Auth_Department_GetAll** - No parameters

### DunnageParts (9 procedures)

- **sp_Dunnage_Parts_Insert** - IN: 5 OUT: 4
- **sp_Dunnage_Parts_Update** - IN: 4
- **sp_Dunnage_Parts_CountTransactions** - IN: 4
- **sp_Dunnage_Parts_GetAll** - No parameters
- **sp_Dunnage_Parts_GetById** - IN: 4
- **sp_Dunnage_Parts_GetByType** - IN: 4
- **sp_Dunnage_Parts_GetTransactionCount** - IN: 4 OUT: 4
- **sp_Dunnage_Parts_Search** - IN: 2
- **sp_Dunnage_Parts_Delete** - IN: 4

### DunnageSpecs (9 procedures)

- **sp_Dunnage_Specs_Insert** - IN: 4 OUT: 4
- **sp_Dunnage_Specs_Update** - IN: 3
- **sp_Dunnage_Specs_CountPartsUsingSpec** - IN: 2
- **sp_Dunnage_Specs_GetAll** - No parameters
- **sp_Dunnage_Specs_GetAllKeys** - No parameters
- **sp_Dunnage_Specs_GetById** - IN: 4
- **sp_Dunnage_Specs_GetByType** - IN: 4
- **sp_Dunnage_Specs_DeleteById** - IN: 4
- **sp_Dunnage_Specs_DeleteByType** - IN: 4

### DunnageTypes (11 procedures)

- **sp_Dunnage_Types_Insert** - IN: 3
- **sp_Dunnage_Types_Update** - IN: 4
- **sp_Dunnage_Types_CheckDuplicate** - IN: 2 OUT: 4
- **sp_Dunnage_Types_CountParts** - IN: 4
- **sp_Dunnage_Types_CountTransactions** - IN: 4
- **sp_Dunnage_Types_GetAll** - No parameters
- **sp_Dunnage_Types_GetById** - IN: 4
- **sp_Dunnage_Types_GetPartCount** - IN: 4 OUT: 4
- **sp_Dunnage_Types_GetTransactionCount** - IN: 4 OUT: 4
- **sp_Dunnage_Types_GetUsageCount** - IN: 4
- **sp_Dunnage_Types_Delete** - IN: 2 OUT: 2

### Other (45 procedures)

- **sp_Auth_Activity_Log** - IN: 4
- **sp_Auth_User_Create** - IN: 9 OUT: 4
- **sp_Auth_User_SeedDefaultModes** - IN: 4
- **sp_Auth_User_UpdateDefaultDunnageMode** - IN: 2
- **sp_Auth_User_UpdateDefaultMode** - IN: 2
- **sp_Auth_User_UpdateDefaultReceivingMode** - IN: 2
- **sp_Auth_User_Upsert** - IN: 10
- **sp_Auth_User_ValidatePin** - IN: 2
- **sp_Auth_Workstation_Upsert** - IN: 4
- **sp_Dunnage_CustomFields_Insert** - IN: 8 OUT: 3
- **sp_Dunnage_Inventory_Insert** - IN: 4 OUT: 4
- **sp_Dunnage_Inventory_Update** - IN: 4
- **sp_Dunnage_Line_Insert** - IN: 7 OUT: 2
- **sp_Dunnage_Loads_Insert** - IN: 4
- **sp_Dunnage_Loads_InsertBatch** - IN: 2
- **sp_Dunnage_Loads_Update** - IN: 3
- **sp_routing_label_history_insert** - IN: 5 OUT: 2
- **sp_routing_usage_increment** - IN: 2 OUT: 2
- **sp_Volvo_PartComponent_Insert** - IN: 3
- **sp_Volvo_PartMaster_Insert** - IN: 3
- **sp_Volvo_PartMaster_SetActive** - IN: 2
- **sp_Volvo_PartMaster_Update** - IN: 2
- **sp_Volvo_ShipmentLine_Insert** - IN: 8
- **sp_Volvo_ShipmentLine_Update** - IN: 6
- **sp_Auth_Terminal_GetShared** - No parameters
- **sp_Auth_User_GetByWindowsUsername** - IN: 4
- **sp_Auth_User_GetDefaultMode** - IN: 4
- **sp_Dunnage_CustomFields_GetByType** - IN: 4
- **sp_Dunnage_Inventory_Check** - IN: 4
- **sp_Dunnage_Inventory_GetAll** - No parameters
- **sp_Dunnage_Inventory_GetByPart** - IN: 4
- **sp_Dunnage_Loads_GetAll** - No parameters
- **sp_Dunnage_Loads_GetByDateRange** - IN: 2
- **sp_Dunnage_Loads_GetById** - IN: 4
- **sp_Receiving_History_Get** - IN: 3
- **sp_routing_label_get_history** - IN: 2
- **sp_volvo_part_check_references** - IN: 4 OUT: 4
- **sp_Volvo_PartComponent_Get** - IN: 4
- **sp_Volvo_PartMaster_GetAll** - IN: 4
- **sp_Volvo_PartMaster_GetById** - IN: 4
- **sp_Volvo_ShipmentLine_GetByShipment** - IN: 4
- **sp_Dunnage_Inventory_Delete** - IN: 4
- **sp_Dunnage_Loads_Delete** - IN: 4
- **sp_Volvo_PartComponent_DeleteByParent** - IN: 4
- **sp_Volvo_ShipmentLine_Delete** - IN: 4

### PackageTypes (9 procedures)

- **sp_Receiving_PackageTypeMappings_GetAll** - IN: 4
- **sp_Receiving_PackageTypeMappings_GetByPrefix** - IN: 4
- **sp_Receiving_PackageTypeMappings_Insert** - IN: 5
- **sp_Receiving_PackageTypeMappings_Update** - IN: 6
- **sp_Receiving_PackageTypePreference_Get** - IN: 4
- **sp_Receiving_PackageTypePreference_Save** - IN: 4
- **sp_Receiving_PackageTypeMappings_Delete** - IN: 4
- **sp_Receiving_PackageTypePreference_Delete** - IN: 4
- **sp_Receiving_PackageTypes_Delete** - IN: 4

### Preferences (4 procedures)

- **sp_Dunnage_UserPreferences_Upsert** - IN: 3 OUT: 2
- **sp_routing_user_preference_save** - IN: 3 OUT: 2
- **sp_Dunnage_UserPreferences_GetRecentIcons** - IN: 2
- **sp_routing_user_preference_get** - IN: 4 OUT: 2

### ReceivingLines (1 procedures)

- **sp_Receiving_Line_Insert** - IN: 10 OUT: 2

### ReceivingLoads (4 procedures)

- **sp_Receiving_Load_Insert** - IN: 13
- **sp_Receiving_Load_Update** - IN: 13
- **sp_Receiving_Load_GetAll** - IN: 2
- **sp_Receiving_Load_Delete** - IN: 4

### RoutingLabels (9 procedures)

- **sp_routing_label_archive** - IN: 4 OUT: 2
- **sp_routing_label_insert** - IN: 7 OUT: 2
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

- **sp_routing_recipient_insert** - IN: 3 OUT: 2
- **sp_routing_recipient_update** - IN: 5 OUT: 4
- **sp_routing_recipient_get_all** - No parameters
- **sp_routing_recipient_get_all_active** - No parameters
- **sp_routing_recipient_get_by_name** - IN: 4
- **sp_routing_recipient_get_top_by_usage** - IN: 2

### Settings (34 procedures)

- **sp_Settings_RoutingRule_Insert** - IN: 5
- **sp_Settings_RoutingRule_Update** - IN: 5
- **sp_Settings_ScheduledReport_Insert** - IN: 5
- **sp_Settings_ScheduledReport_ToggleActive** - IN: 2
- **sp_Settings_ScheduledReport_Update** - IN: 5
- **sp_Settings_ScheduledReport_UpdateLastRun** - IN: 3 OUT: 4
- **sp_Settings_System_ResetToDefault** - IN: 4
- **sp_Settings_System_SetLocked** - IN: 5
- **sp_Settings_System_UpdateValue** - IN: 5
- **sp_Settings_User_Reset** - IN: 2
- **sp_Settings_User_ResetAll** - IN: 2
- **sp_Settings_User_Set** - IN: 3
- **sp_Volvo_Settings_Reset** - IN: 2
- **sp_Volvo_Settings_Upsert** - IN: 3
- **sp_Settings_AuditLog_Get** - IN: 2
- **sp_Settings_AuditLog_GetBySetting** - IN: 2
- **sp_Settings_AuditLog_GetByUser** - IN: 2
- **sp_Settings_RoutingRule_FindMatch** - IN: 2
- **sp_Settings_RoutingRule_GetAll** - No parameters
- **sp_Settings_RoutingRule_GetById** - IN: 4
- **sp_Settings_RoutingRule_GetByPartNumber** - IN: 4
- **sp_Settings_ScheduledReport_GetActive** - No parameters
- **sp_Settings_ScheduledReport_GetAll** - No parameters
- **sp_Settings_ScheduledReport_GetById** - IN: 4
- **sp_Settings_ScheduledReport_GetDue** - No parameters
- **sp_Settings_System_GetAll** - No parameters
- **sp_Settings_System_GetByCategory** - IN: 4
- **sp_Settings_System_GetByKey** - IN: 2
- **sp_Settings_User_Get** - IN: 3
- **sp_Settings_User_GetAllForUser** - IN: 4
- **sp_Volvo_Settings_Get** - IN: 4
- **sp_Volvo_Settings_GetAll** - IN: 4
- **sp_Settings_RoutingRule_Delete** - IN: 4
- **sp_Settings_ScheduledReport_Delete** - IN: 4

### VolvoShipments (6 procedures)

- **sp_Volvo_Shipment_Complete** - IN: 3
- **sp_Volvo_Shipment_Insert** - IN: 3 OUT: 2
- **sp_Volvo_Shipment_Update** - IN: 2
- **sp_Volvo_Shipment_GetHistory** - IN: 3
- **sp_Volvo_Shipment_GetPending** - No parameters
- **sp_Volvo_Shipment_Delete** - IN: 4


## Execution Order Groups

| Order | Category | Count | Stored Procedures |
| ----- | -------- | ----- | ----------------- |
| 25 | DunnageTypes | 2 | sp_Dunnage_Types_Insert, sp_Dunnage_Types_Update |
| 30 | DunnageSpecs | 2 | sp_Dunnage_Specs_Insert, sp_Dunnage_Specs_Update |
| 35 | DunnageParts | 2 | sp_Dunnage_Parts_Insert, sp_Dunnage_Parts_Update |
| 45 | RoutingRecipients | 2 | sp_routing_recipient_insert, sp_routing_recipient_update |
| 100 | ReceivingLoads | 2 | sp_Receiving_Load_Insert, sp_Receiving_Load_Update |
| 120 | ReceivingLines | 1 | sp_Receiving_Line_Insert |
| 150 | RoutingLabels | 4 | sp_routing_label_archive, sp_routing_label_insert, sp_routing_label_mark_exported, sp_routing_lab... |
| 170 | VolvoShipments | 3 | sp_Volvo_Shipment_Complete, sp_Volvo_Shipment_Insert, sp_Volvo_Shipment_Update |
| 200 | Preferences | 2 | sp_Dunnage_UserPreferences_Upsert, sp_routing_user_preference_save |
| 210 | Settings | 14 | sp_Settings_RoutingRule_Insert, sp_Settings_RoutingRule_Update, sp_Settings_ScheduledReport_Inser... |
| 999 | Other | 24 | sp_Auth_Activity_Log, sp_Auth_User_Create, sp_Auth_User_SeedDefaultModes, sp_Auth_User_UpdateDefa... |
| 1015 | Departments | 1 | sp_Auth_Department_GetAll |
| 1020 | PackageTypes | 6 | sp_Receiving_PackageTypeMappings_GetAll, sp_Receiving_PackageTypeMappings_GetByPrefix, sp_Receivi... |
| 1025 | DunnageTypes | 8 | sp_Dunnage_Types_CheckDuplicate, sp_Dunnage_Types_CountParts, sp_Dunnage_Types_CountTransactions,... |
| 1030 | DunnageSpecs | 5 | sp_Dunnage_Specs_CountPartsUsingSpec, sp_Dunnage_Specs_GetAll, sp_Dunnage_Specs_GetAllKeys, sp_Du... |
| 1035 | DunnageParts | 6 | sp_Dunnage_Parts_CountTransactions, sp_Dunnage_Parts_GetAll, sp_Dunnage_Parts_GetById, sp_Dunnage... |
| 1045 | RoutingRecipients | 4 | sp_routing_recipient_get_all, sp_routing_recipient_get_all_active, sp_routing_recipient_get_by_na... |
| 1050 | RoutingReasons | 1 | sp_routing_other_reason_get_all_active |
| 1100 | ReceivingLoads | 1 | sp_Receiving_Load_GetAll |
| 1150 | RoutingLabels | 4 | sp_routing_label_check_duplicate, sp_routing_label_get_all, sp_routing_label_get_by_id, sp_routin... |
| 1170 | VolvoShipments | 2 | sp_Volvo_Shipment_GetHistory, sp_Volvo_Shipment_GetPending |
| 1200 | Preferences | 2 | sp_Dunnage_UserPreferences_GetRecentIcons, sp_routing_user_preference_get |
| 1210 | Settings | 18 | sp_Settings_AuditLog_Get, sp_Settings_AuditLog_GetBySetting, sp_Settings_AuditLog_GetByUser, sp_S... |
| 1999 | Other | 17 | sp_Auth_Terminal_GetShared, sp_Auth_User_GetByWindowsUsername, sp_Auth_User_GetDefaultMode, sp_Du... |
| 2025 | DunnageTypes | 1 | sp_Dunnage_Types_Delete |
| 2030 | DunnageSpecs | 2 | sp_Dunnage_Specs_DeleteById, sp_Dunnage_Specs_DeleteByType |
| 2035 | DunnageParts | 1 | sp_Dunnage_Parts_Delete |
| 2100 | ReceivingLoads | 1 | sp_Receiving_Load_Delete |
| 2150 | RoutingLabels | 1 | sp_routing_label_delete |
| 2170 | VolvoShipments | 1 | sp_Volvo_Shipment_Delete |
| 2210 | Settings | 2 | sp_Settings_RoutingRule_Delete, sp_Settings_ScheduledReport_Delete |
| 2999 | Other | 4 | sp_Dunnage_Inventory_Delete, sp_Dunnage_Loads_Delete, sp_Volvo_PartComponent_DeleteByParent, sp_V... |
| 3020 | PackageTypes | 3 | sp_Receiving_PackageTypeMappings_Delete, sp_Receiving_PackageTypePreference_Delete, sp_Receiving_... |

## Notes

- **Execution Order:** Lower numbers execute first (10-999)
- **Read Operations:** Offset by +1000 (can run anytime)
- **Delete Operations:** Offset by +2000 (run last)
- **Mock Data:** Review `01-mock-data.json` to customize parameter values
- **Dependencies:** Use `-UseExecutionOrder` flag when testing to avoid FK constraint errors

## Complex Stored Procedures (8+ parameters)

| SP Name | Total Params | IN | OUT | INOUT |
|---------|--------------|-----|-----|-------|
| sp_Receiving_Load_Update | 13 | 13 | 0 | 0 |
| sp_Receiving_Load_Insert | 13 | 13 | 0 | 0 |
| sp_Receiving_Line_Insert | 12 | 10 | 2 | 0 |
| sp_Dunnage_CustomFields_Insert | 11 | 8 | 3 | 0 |
| sp_Auth_User_Create | 10 | 9 | 4 | 0 |
| sp_Auth_User_Upsert | 10 | 10 | 0 | 0 |
| sp_routing_label_insert | 9 | 7 | 2 | 0 |
| sp_Dunnage_Line_Insert | 9 | 7 | 2 | 0 |
| sp_routing_label_update | 8 | 7 | 4 | 0 |
| sp_Volvo_ShipmentLine_Insert | 8 | 8 | 0 | 0 |

## 🔧 Fix Checklist

### Stored Procedures Requiring Updates

| Fixed | SP Name | Current Params | Expected Params | Missing Params | File Path | DAO Class | Issue Type | Priority | Fix Notes |
|-------|---------|----------------|-----------------|----------------|-----------|-----------|------------|----------|-----------|
| [ ] | sp_Auth_User_Create | 9 IN | 10 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Auth_User_Create.sql |  | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Dunnage_CustomFields_Insert | 8 IN | 11 total | 3 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_CustomFields_Insert.sql | Dao_Dunnage Customfields | OUT Parameters | High | Update DAO to handle 3 OUT params |
| [ ] | sp_Dunnage_Inventory_Insert | 4 IN | 5 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Inventory_Insert.sql | Dao_Dunnage Inventory | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Dunnage_Line_Insert | 7 IN | 9 total | 2 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Line_Insert.sql | Dao_Dunnage Line | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_Dunnage_Parts_GetTransactionCount | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Parts_GetTransactionCount.sql | Dao_Dunnage Parts | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Dunnage_Parts_Insert | 5 IN | 6 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Parts_Insert.sql | Dao_Dunnage Parts | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Dunnage_Specs_Insert | 4 IN | 5 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Specs_Insert.sql | Dao_Dunnage Specs | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Dunnage_Types_CheckDuplicate | 2 IN | 3 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Types_CheckDuplicate.sql | Dao_Dunnage Types | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Dunnage_Types_Delete | 2 IN | 4 total | 2 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Types_Delete.sql | Dao_Dunnage Types | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_Dunnage_Types_GetPartCount | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Types_GetPartCount.sql | Dao_Dunnage Types | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Dunnage_Types_GetTransactionCount | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_Types_GetTransactionCount.sql | Dao_Dunnage Types | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Dunnage_UserPreferences_Upsert | 3 IN | 5 total | 2 OUT/INOUT | Database/StoredProcedures/sp_Dunnage_UserPreferences_Upsert.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_Receiving_Line_Insert | 10 IN | 12 total | 2 OUT/INOUT | Database/StoredProcedures/sp_Receiving_Line_Insert.sql | Dao_Receiving Line | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_archive | 4 IN | 3 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_archive.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_check_duplicate | 4 IN | 6 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_check_duplicate.sql | Dao_Routing Label | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_delete | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_routing_label_delete.sql | Dao_Routing Label | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_routing_label_get_all | 2 IN | 4 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_get_all.sql | Dao_Routing Label | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_get_by_id | 4 IN | 3 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_get_by_id.sql | Dao_Routing Label | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_history_insert | 5 IN | 7 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_history_insert.sql | Dao_Routing Label History | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_insert | 7 IN | 9 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_insert.sql | Dao_Routing Label | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_mark_exported | 4 IN | 3 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_label_mark_exported.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_label_update | 7 IN | 8 total | 4 OUT/INOUT | Database/StoredProcedures/sp_routing_label_update.sql | Dao_Routing Label | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_routing_other_reason_get_all_active | 0 IN | 2 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_other_reason_get_all_active.sql | Dao_Routing Other Reason | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_recipient_insert | 3 IN | 5 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_recipient_insert.sql | Dao_Routing Recipient | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_recipient_update | 5 IN | 6 total | 4 OUT/INOUT | Database/StoredProcedures/sp_routing_recipient_update.sql | Dao_Routing Recipient | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_routing_usage_increment | 2 IN | 4 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_usage_increment.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_user_preference_get | 4 IN | 3 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_user_preference_get.sql | Dao_Routing User Preference | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_routing_user_preference_save | 3 IN | 5 total | 2 OUT/INOUT | Database/StoredProcedures/sp_routing_user_preference_save.sql |  | OUT Parameters | Medium | Update DAO to handle 2 OUT params |
| [ ] | sp_Settings_ScheduledReport_UpdateLastRun | 3 IN | 4 total | 4 OUT/INOUT | Database/StoredProcedures/sp_Settings_ScheduledReport_UpdateLastRun.sql | Dao_Settings Scheduledreport | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_volvo_part_check_references | 4 IN | 2 total | 4 OUT/INOUT | Database/StoredProcedures/sp_volvo_part_check_references.sql | Dao_Volvo Part | OUT Parameters | High | Update DAO to handle 4 OUT params |
| [ ] | sp_Volvo_Shipment_Insert | 3 IN | 5 total | 2 OUT/INOUT | Database/StoredProcedures/sp_Volvo_Shipment_Insert.sql | Dao_Volvo Shipment | OUT Parameters | Medium | Update DAO to handle 2 OUT params |

### DAO Files Requiring Updates

| Fixed | DAO Class | File Path | Related SPs | Param Mismatches | Methods to Update | Issue Summary | Priority |
|-------|-----------|-----------|-------------|------------------|-------------------|---------------|----------|
| [ ] | Dao_Dunnage Customfields | **/Dao_Dunnage Customfields.cs (search required) | sp_Dunnage_CustomFields_Insert | 3 | Insert*Async | 3 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Dunnage Inventory | **/Dao_Dunnage Inventory.cs (search required) | sp_Dunnage_Inventory_Insert | 4 | Insert*Async | 4 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Dunnage Line | **/Dao_Dunnage Line.cs (search required) | sp_Dunnage_Line_Insert | 2 | Insert*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Dunnage Parts | **/Dao_Dunnage Parts.cs (search required) | sp_Dunnage_Parts_GetTransactionCount, sp_Dunnage_Parts_In... | 8 | Get*Async, Insert*Async | 8 OUT/INOUT params across 2 SP(s) | High |
| [ ] | Dao_Dunnage Specs | **/Dao_Dunnage Specs.cs (search required) | sp_Dunnage_Specs_Insert | 4 | Insert*Async | 4 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Dunnage Types | **/Dao_Dunnage Types.cs (search required) | sp_Dunnage_Types_CheckDuplicate, sp_Dunnage_Types_Delete,... | 14 | Delete*Async, Get*Async | 14 OUT/INOUT params across 4 SP(s) | High |
| [ ] | Dao_Receiving Line | **/Dao_Receiving Line.cs (search required) | sp_Receiving_Line_Insert | 2 | Insert*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Routing Label | **/Dao_Routing Label.cs (search required) | sp_routing_label_check_duplicate, sp_routing_label_delete... | 16 | Delete*Async, Get*Async, Insert*Async, Update*Async | 16 OUT/INOUT params across 6 SP(s) | High |
| [ ] | Dao_Routing Label History | **/Dao_Routing Label History.cs (search required) | sp_routing_label_history_insert | 2 | Insert*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Routing Other Reason | **/Dao_Routing Other Reason.cs (search required) | sp_routing_other_reason_get_all_active | 2 | Get*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Routing Recipient | **/Dao_Routing Recipient.cs (search required) | sp_routing_recipient_insert, sp_routing_recipient_update | 6 | Insert*Async, Update*Async | 6 OUT/INOUT params across 2 SP(s) | High |
| [ ] | Dao_Routing User Preference | **/Dao_Routing User Preference.cs (search required) | sp_routing_user_preference_get | 2 | Get*Async | 2 OUT/INOUT params across 1 SP(s) | Low |
| [ ] | Dao_Settings Scheduledreport | **/Dao_Settings Scheduledreport.cs (search required) | sp_Settings_ScheduledReport_UpdateLastRun | 4 | Update*Async | 4 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Volvo Part | **/Dao_Volvo Part.cs (search required) | sp_volvo_part_check_references | 4 |  | 4 OUT/INOUT params across 1 SP(s) | Medium |
| [ ] | Dao_Volvo Shipment | **/Dao_Volvo Shipment.cs (search required) | sp_Volvo_Shipment_Insert | 2 | Insert*Async | 2 OUT/INOUT params across 1 SP(s) | Low |

### Database Schema Issues

| Fixed | SP Name | Missing Column/Table | Schema File | Table Name | Expected Column | Current Status | SQL Fix Required | Priority | Fix Notes |
|-------|---------|----------------------|-------------|------------|-----------------|----------------|------------------|----------|-----------|
| - | *No schema issues detected - run 02-Test-StoredProcedures.ps1 first* | - | - | - | - | - | - | - | - |


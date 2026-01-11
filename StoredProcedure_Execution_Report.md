# Stored Procedure Execution Test (MAMP)
**Generated:** 01/11/2026 11:49:48
**Server:** localhost:3306

## Summary
- **Total:** 153
- **Passed:** 91
- **Schema Broken:** 5

## Critical Failures (Schema Broken)
| SP Name | Error Code | Message | IN Params | OUT Params |
|---|---|---|---|---|
| **sp_dunnage_specs_get_all_keys** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'DunnageSpecs' in 'field list'" | 0 | 0 |
| **sp_routing_label_get_history** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'label_number' in 'field list'" | 2 | 0 |
| **sp_routing_label_get_today** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'label_number' in 'field list'" | 1 | 0 |
| **sp_routing_recipient_get_all** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'default_department' in 'field list'" | 0 | 0 |
| **sp_routing_recipient_get_by_name** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'default_department' in 'field list'" | 1 | 0 |

## Other Failures (Constraints/Runtime)
| SP Name | Category | Message | IN Params | OUT Params |
|---|---|---|---|---|
| carrier_delivery_label_Insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.carrier_delivery_label_Insert; expected 9, got 7" | 7 | 2 |
| dunnage_line_Insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.dunnage_line_Insert; expected 9, got 7" | 7 | 2 |
| receiving_line_Insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.receiving_line_Insert; expected 12, got 10" | 10 | 2 |
| sp_CreateNewUser | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_CreateNewUser; expected 10, got 9" | 9 | 1 |
| sp_custom_fields_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_custom_fields_insert; expected 11, got 9" | 8 | 3 |
| sp_dunnage_loads_get_by_date_range | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_loads_get_by_date_range; expected 2, got 4" | 2 | 0 |
| sp_dunnage_loads_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot add or update a child row: a foreign key constraint fails (`mtm_receiving_application`.`dunnage_loads`, CONSTRAINT `FK_dunnage_loads_part_id` FOREIGN KEY (`part_id`) REFERENCES `dunnage_parts` (`part_id`))" | 4 | 0 |
| sp_dunnage_parts_get_transaction_count | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_parts_get_transaction_count; expected 2, got 1" | 1 | 1 |
| sp_dunnage_parts_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_parts_insert; expected 6, got 5" | 5 | 1 |
| sp_dunnage_specs_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_specs_insert; expected 5, got 4" | 4 | 1 |
| sp_dunnage_types_check_duplicate | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_types_check_duplicate; expected 3, got 2" | 2 | 1 |
| sp_dunnage_types_delete | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot delete or update a parent row: a foreign key constraint fails (`mtm_receiving_application`.`dunnage_parts`, CONSTRAINT `FK_dunnage_parts_type_id` FOREIGN KEY (`type_id`) REFERENCES `dunnage_types` (`id`))" | 1 | 0 |
| sp_dunnage_types_get_part_count | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_types_get_part_count; expected 2, got 1" | 1 | 1 |
| sp_dunnage_types_get_transaction_count | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_types_get_transaction_count; expected 2, got 1" | 1 | 1 |
| sp_dunnage_types_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_types_insert; expected 4, got 3" | 3 | 1 |
| sp_GetReceivingHistory | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_GetReceivingHistory; expected 3, got 5" | 3 | 0 |
| sp_InsertReceivingLoad | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_InsertReceivingLoad; expected 13, got 14" | 13 | 0 |
| sp_inventoried_dunnage_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_inventoried_dunnage_insert; expected 5, got 4" | 4 | 1 |
| sp_PackageTypeMappings_Insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_PackageTypeMappings_Insert; expected 5, got 6" | 5 | 0 |
| sp_PackageTypeMappings_Update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_PackageTypeMappings_Update; expected 4, got 5" | 4 | 0 |
| sp_PackageType_Insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Package type name already exists" | 3 | 0 |
| sp_PackageType_Update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Package type name already exists" | 3 | 0 |
| sp_RoutingRule_Insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Data truncated for column 'match_type' at row 1" | 5 | 0 |
| sp_RoutingRule_Update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Data truncated for column 'match_type' at row 1" | 5 | 0 |
| sp_routing_label_archive | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_archive; expected 3, got 1" | 1 | 2 |
| sp_routing_label_check_duplicate | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_check_duplicate; expected 6, got 4" | 4 | 2 |
| sp_routing_label_delete | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_delete; expected 2, got 1" | 1 | 1 |
| sp_routing_label_get_all | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_get_all; expected 4, got 2" | 2 | 2 |
| sp_routing_label_get_by_id | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_get_by_id; expected 3, got 1" | 1 | 2 |
| sp_routing_label_history_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_history_insert; expected 7, got 5" | 5 | 2 |
| sp_routing_label_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_insert; expected 10, got 8" | 8 | 2 |
| sp_routing_label_mark_exported | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_mark_exported; expected 3, got 1" | 1 | 2 |
| sp_routing_label_update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_update; expected 8, got 7" | 7 | 1 |
| sp_routing_other_reason_get_all_active | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_other_reason_get_all_active; expected 2, got 0" | 0 | 2 |
| sp_routing_recipient_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_recipient_insert; expected 4, got 2" | 2 | 2 |
| sp_routing_recipient_update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "OUT or INOUT argument 5 for routine mtm_receiving_application.sp_routing_recipient_update is not a variable or NEW pseudo-variable in BEFORE trigger" | 4 | 1 |
| sp_routing_usage_increment | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_usage_increment; expected 4, got 2" | 2 | 2 |
| sp_routing_user_preference_get | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_user_preference_get; expected 3, got 1" | 1 | 2 |
| sp_routing_user_preference_save | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_user_preference_save; expected 5, got 4" | 3 | 2 |
| sp_SavePackageTypePreference | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_SavePackageTypePreference; expected 4, got 5" | 4 | 0 |
| sp_ScheduledReport_Insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_ScheduledReport_Insert; expected 5, got 6" | 5 | 0 |
| sp_ScheduledReport_ToggleActive | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_ScheduledReport_ToggleActive; expected 2, got 3" | 2 | 0 |
| sp_ScheduledReport_Update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_ScheduledReport_Update; expected 5, got 6" | 5 | 0 |
| sp_ScheduledReport_UpdateLastRun | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_ScheduledReport_UpdateLastRun; expected 3, got 5" | 3 | 0 |
| sp_SystemSettings_SetLocked | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_SystemSettings_SetLocked; expected 5, got 6" | 5 | 0 |
| sp_UpdateReceivingLoad | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_UpdateReceivingLoad; expected 13, got 14" | 13 | 0 |
| sp_UpsertUser | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_UpsertUser; expected 10, got 11" | 10 | 0 |
| sp_user_preferences_get_recent_icons | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Illegal mix of collations (utf8mb4_general_ci,IMPLICIT) and (utf8mb4_unicode_ci,IMPLICIT) for operation '='" | 2 | 0 |
| sp_user_preferences_upsert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_user_preferences_upsert; expected 5, got 3" | 3 | 2 |
| sp_volvo_part_check_references | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_check_references; expected 2, got 1" | 1 | 1 |
| sp_volvo_part_component_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot add or update a child row: a foreign key constraint fails (`mtm_receiving_application`.`volvo_part_components`, CONSTRAINT `volvo_part_components_ibfk_1` FOREIGN KEY (`parent_part_number`) REFERENCES `volvo_parts_master` (`part_number`) ON DELETE CASC)" | 3 | 0 |
| sp_volvo_part_master_get_all | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_master_get_all; expected 1, got 2" | 1 | 0 |
| sp_volvo_part_master_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_master_insert; expected 3, got 4" | 3 | 0 |
| sp_volvo_part_master_set_active | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_master_set_active; expected 2, got 3" | 2 | 0 |
| sp_volvo_shipment_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_shipment_insert; expected 5, got 3" | 3 | 2 |
| sp_volvo_shipment_line_insert | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_shipment_line_insert; expected 8, got 9" | 8 | 0 |
| sp_volvo_shipment_line_update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_shipment_line_update; expected 6, got 7" | 6 | 0 |

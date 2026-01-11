# Stored Procedure Execution Test Report

**Generated:** 2026-01-11 13:34:36
**Database:** mtm_receiving_application
**Server:** localhost:3306
**Execution Mode:** Alphabetical

## Summary

| Metric                     | Count              | Percentage           |
| -------------------------- | ------------------ | -------------------- |
| **Total Tests**            | 153    | 100%                 |
| **Passed**                 | 91         | 59.5%    |
| **Failed**                 | 62         | 40.5%       |
| Schema Broken              | 5  | 3.3%     |
| Runtime Errors             | 57 | 37.3%    |
| Constraint Violations      | 0    | 0% |
| Business Logic Validations | 0   | 0%      |

## Results by Category

| Category | Total | Passed | Failed | Success Rate |
|----------|-------|--------|--------|--------------|
| Departments | 1 | 1 | 0 | 100% |
| DunnageLoads | 7 | 5 | 2 | 71.4% |
| DunnageParts | 9 | 7 | 2 | 77.8% |
| DunnageSpecs | 9 | 7 | 2 | 77.8% |
| DunnageTypes | 10 | 5 | 5 | 50% |
| InventoriedDunnage | 6 | 5 | 1 | 83.3% |
| Other | 26 | 15 | 11 | 57.7% |
| PackageTypes | 14 | 9 | 5 | 64.3% |
| Preferences | 2 | 0 | 2 | 0% |
| ReceivingLines | 1 | 0 | 1 | 0% |
| ReceivingLoads | 4 | 2 | 2 | 50% |
| Reports | 9 | 5 | 4 | 55.6% |
| RoutingHistory | 1 | 0 | 1 | 0% |
| RoutingLabels | 9 | 0 | 9 | 0% |
| RoutingReasons | 1 | 0 | 1 | 0% |
| RoutingRecipients | 6 | 2 | 4 | 33.3% |
| Settings | 13 | 12 | 1 | 92.3% |
| Users | 7 | 5 | 2 | 71.4% |
| VolvoComponents | 3 | 2 | 1 | 66.7% |
| VolvoParts | 5 | 2 | 3 | 40% |
| VolvoShipmentLines | 4 | 2 | 2 | 50% |
| VolvoShipments | 6 | 5 | 1 | 83.3% |


## 🔴 Critical Failures (Schema Broken)

These stored procedures reference columns or tables that don't exist in the database.

| SP Name | Error Code | Message | IN Params | OUT Params |
| ------- | ---------- | ------- | --------- | ---------- |
| **sp_dunnage_specs_get_all_keys** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'DunnageSpecs' in 'field list'" | 0 | 0 |
| **sp_routing_label_get_history** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'label_number' in 'field list'" | 2 | 0 |
| **sp_routing_label_get_today** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'label_number' in 'field list'" | 1 | 0 |
| **sp_routing_recipient_get_all** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'default_department' in 'field list'" | 0 | 0 |
| **sp_routing_recipient_get_by_name** | 1054 | Exception calling "ExecuteNonQuery" with "0" argument(s): "Unknown column 'default_department' in 'field list'" | 1 | 0 |

## ⚠️ Parameter Mismatches

These stored procedures have parameter count mismatches between the database definition and mock data.

| SP Name | Message | IN Params | OUT Params |
|---------|---------|-----------|------------|
| carrier_delivery_label_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.carrier_delivery_label_Insert; expected 9, got 7" | 7 | 2 |
| dunnage_line_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.dunnage_line_Insert; expected 9, got 7" | 7 | 2 |
| receiving_line_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.receiving_line_Insert; expected 12, got 10" | 10 | 2 |
| sp_CreateNewUser | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_CreateNewUser; expected 10, got 9" | 9 | 1 |
| sp_custom_fields_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_custom_fields_insert; expected 11, got 9" | 8 | 3 |
| sp_dunnage_loads_get_by_date_range | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_loads_get_by_date_range; expected 2, got 4" | 2 | 0 |
| sp_dunnage_parts_get_transaction_count | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_parts_get_transaction_count; expected 2, got 1" | 1 | 1 |
| sp_dunnage_parts_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_parts_insert; expected 6, got 5" | 5 | 1 |
| sp_dunnage_specs_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_specs_insert; expected 5, got 4" | 4 | 1 |
| sp_dunnage_types_check_duplicate | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_types_check_duplicate; expected 3, got 2" | 2 | 1 |
| sp_dunnage_types_get_part_count | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_types_get_part_count; expected 2, got 1" | 1 | 1 |
| sp_dunnage_types_get_transaction_count | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_types_get_transaction_count; expected 2, got 1" | 1 | 1 |
| sp_dunnage_types_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_dunnage_types_insert; expected 4, got 3" | 3 | 1 |
| sp_GetReceivingHistory | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_GetReceivingHistory; expected 3, got 5" | 3 | 0 |
| sp_InsertReceivingLoad | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_InsertReceivingLoad; expected 13, got 14" | 13 | 0 |
| sp_inventoried_dunnage_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_inventoried_dunnage_insert; expected 5, got 4" | 4 | 1 |
| sp_PackageTypeMappings_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_PackageTypeMappings_Insert; expected 5, got 6" | 5 | 0 |
| sp_PackageTypeMappings_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_PackageTypeMappings_Update; expected 4, got 5" | 4 | 0 |
| sp_routing_label_archive | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_archive; expected 3, got 1" | 1 | 2 |
| sp_routing_label_check_duplicate | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_check_duplicate; expected 6, got 4" | 4 | 2 |
| sp_routing_label_delete | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_delete; expected 2, got 1" | 1 | 1 |
| sp_routing_label_get_all | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_get_all; expected 4, got 2" | 2 | 2 |
| sp_routing_label_get_by_id | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_get_by_id; expected 3, got 1" | 1 | 2 |
| sp_routing_label_history_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_history_insert; expected 7, got 5" | 5 | 2 |
| sp_routing_label_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_insert; expected 10, got 8" | 8 | 2 |
| sp_routing_label_mark_exported | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_mark_exported; expected 3, got 1" | 1 | 2 |
| sp_routing_label_update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_update; expected 8, got 7" | 7 | 1 |
| sp_routing_other_reason_get_all_active | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_other_reason_get_all_active; expected 2, got 0" | 0 | 2 |
| sp_routing_recipient_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_recipient_insert; expected 4, got 2" | 2 | 2 |
| sp_routing_usage_increment | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_usage_increment; expected 4, got 2" | 2 | 2 |
| sp_routing_user_preference_get | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_user_preference_get; expected 3, got 1" | 1 | 2 |
| sp_routing_user_preference_save | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_user_preference_save; expected 5, got 4" | 3 | 2 |
| sp_SavePackageTypePreference | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_SavePackageTypePreference; expected 4, got 5" | 4 | 0 |
| sp_ScheduledReport_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_ScheduledReport_Insert; expected 5, got 6" | 5 | 0 |
| sp_ScheduledReport_ToggleActive | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_ScheduledReport_ToggleActive; expected 2, got 3" | 2 | 0 |
| sp_ScheduledReport_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_ScheduledReport_Update; expected 5, got 6" | 5 | 0 |
| sp_ScheduledReport_UpdateLastRun | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_ScheduledReport_UpdateLastRun; expected 3, got 5" | 3 | 0 |
| sp_SystemSettings_SetLocked | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_SystemSettings_SetLocked; expected 5, got 6" | 5 | 0 |
| sp_UpdateReceivingLoad | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_UpdateReceivingLoad; expected 13, got 14" | 13 | 0 |
| sp_UpsertUser | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_UpsertUser; expected 10, got 11" | 10 | 0 |
| sp_user_preferences_upsert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_user_preferences_upsert; expected 5, got 3" | 3 | 2 |
| sp_volvo_part_check_references | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_check_references; expected 2, got 1" | 1 | 1 |
| sp_volvo_part_master_get_all | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_master_get_all; expected 1, got 2" | 1 | 0 |
| sp_volvo_part_master_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_master_insert; expected 3, got 4" | 3 | 0 |
| sp_volvo_part_master_set_active | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_master_set_active; expected 2, got 3" | 2 | 0 |
| sp_volvo_shipment_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_shipment_insert; expected 5, got 3" | 3 | 2 |
| sp_volvo_shipment_line_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_shipment_line_insert; expected 8, got 9" | 8 | 0 |
| sp_volvo_shipment_line_update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_shipment_line_update; expected 6, got 7" | 6 | 0 |

## 🔗 Constraint Violations

These stored procedures failed due to foreign key constraints (missing prerequisite data).

| SP Name | Message |
|---------|---------|
| sp_dunnage_loads_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot add or update a child row: a foreign key constraint fails (`mtm_receiving_application`.`dunnage_loads`, CONSTRAINT `FK_dunnage_loads_part_id` FOREIGN KEY (`part_id`) REFERENCES `dunnage_parts` (`part_id`))" |
| sp_dunnage_types_delete | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot delete or update a parent row: a foreign key constraint fails (`mtm_receiving_application`.`dunnage_parts`, CONSTRAINT `FK_dunnage_parts_type_id` FOREIGN KEY (`type_id`) REFERENCES `dunnage_types` (`id`))" |
| sp_volvo_part_component_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot add or update a child row: a foreign key constraint fails (`mtm_receiving_application`.`volvo_part_components`, CONSTRAINT `volvo_part_components_ibfk_1` FOREIGN KEY (`parent_part_number`) REFERENCES `volvo_parts_master` (`part_number`) ON DELETE CASC)" |

**Recommendation:** Run with `-UseExecutionOrder` flag or add prerequisite test data.

## 📋 Data Validation Errors

These stored procedures failed due to data validation issues.

| SP Name | Message |
|---------|---------|
| sp_PackageType_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Package type name already exists" |
| sp_PackageType_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Package type name already exists" |
| sp_RoutingRule_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Data truncated for column 'match_type' at row 1" |
| sp_RoutingRule_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Data truncated for column 'match_type' at row 1" |
| sp_user_preferences_get_recent_icons | Exception calling "ExecuteNonQuery" with "0" argument(s): "Illegal mix of collations (utf8mb4_general_ci,IMPLICIT) and (utf8mb4_unicode_ci,IMPLICIT) for operation '='" |

**Recommendation:** Review mock data values in `01-mock-data.json`.

## 🔧 Other Runtime Errors

| SP Name | Category | Message |
|---------|----------|---------|
| sp_routing_recipient_update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "OUT or INOUT argument 5 for routine mtm_receiving_application.sp_routing_recipient_update is not a variable or NEW pseudo-variable in BEFORE trigger" |

## 💡 Recommendations

### Critical: Fix Schema Issues
- 5 stored procedure(s) reference non-existent columns or tables
- Review and update stored procedure SQL or database schema

### High Priority: Fix Parameter Mismatches
- 48 stored procedure(s) have parameter count issues
- Regenerate mock data: `pwsh -File .\Database\00-Test\01-Generate-SP-TestData.ps1`

### Improve Test Data
- 3 stored procedure(s) failed due to missing FK references
- Use dependency-aware execution: `pwsh -File .\Database\00-Test\02-Test-StoredProcedures.ps1 -UseExecutionOrder`
- Or add prerequisite test data to the database

### Needs Attention
- Only 59.5% success rate
- Review error categories above and prioritize fixes


## Test Configuration

- **Mock Data File:** ✓ Loaded
- **Execution Order File:** ✓ Loaded
- **Using Execution Order:** No

---

**Report Generated By:** 02-Test-StoredProcedures.ps1

**Next Steps:**
1. Fix critical schema issues first (red flags above)
2. Regenerate mock data if parameter mismatches exist
3. Run with `-UseExecutionOrder` flag to reduce FK constraint errors
4. Review and customize mock data values in `01-mock-data.json`


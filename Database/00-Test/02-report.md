# Stored Procedure Execution Test Report

**Generated:** 2026-01-11 17:36:34
**Database:** mtm_receiving_application
**Server:** localhost:3306
**Execution Mode:** Alphabetical

## Summary

| Metric                     | Count              | Percentage           |
| -------------------------- | ------------------ | -------------------- |
| **Total Tests**            | 149    | 100%                 |
| **Passed**                 | 89         | 59.7%    |
| **Failed**                 | 60         | 40.3%       |
| Schema Broken              | 0  | 0%     |
| Runtime Errors             | 60 | 40.3%    |
| Constraint Violations      | 0    | 0% |
| Business Logic Validations | 0   | 0%      |

## Results by Category

| Category | Total | Passed | Failed | Success Rate |
|----------|-------|--------|--------|--------------|
| Departments | 1 | 1 | 0 | 100% |
| DunnageParts | 9 | 7 | 2 | 77.8% |
| DunnageSpecs | 9 | 8 | 1 | 88.9% |
| DunnageTypes | 11 | 6 | 5 | 54.5% |
| Other | 45 | 27 | 18 | 60% |
| PackageTypes | 9 | 5 | 4 | 55.6% |
| Preferences | 4 | 0 | 4 | 0% |
| ReceivingLines | 1 | 0 | 1 | 0% |
| ReceivingLoads | 4 | 2 | 2 | 50% |
| RoutingLabels | 9 | 1 | 8 | 11.1% |
| RoutingReasons | 1 | 0 | 1 | 0% |
| RoutingRecipients | 6 | 3 | 3 | 50% |
| Settings | 34 | 24 | 10 | 70.6% |
| VolvoShipments | 6 | 5 | 1 | 83.3% |


## 🔴 Critical Failures (Schema Broken)

These stored procedures reference columns or tables that don't exist in the database.

| SP Name | Error Code | Message | IN Params | OUT Params |
| ------- | ---------- | ------- | --------- | ---------- |
| - | - | ✓ No schema errors | - | - |

## ⚠️ Parameter Mismatches

These stored procedures have parameter count mismatches between the database definition and mock data.

| SP Name | Message | IN Params | OUT Params |
|---------|---------|-----------|------------|
| sp_Auth_User_Create | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Auth_User_Create; expected 10, got 9" | 9 | 1 |
| sp_Auth_User_Upsert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Auth_User_Upsert; expected 10, got 11" | 10 | 0 |
| sp_Auth_Workstation_Upsert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Auth_Workstation_Upsert; expected 4, got 5" | 4 | 0 |
| sp_Dunnage_CustomFields_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_CustomFields_Insert; expected 11, got 9" | 8 | 3 |
| sp_Dunnage_Inventory_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Inventory_Insert; expected 5, got 4" | 4 | 1 |
| sp_Dunnage_Line_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Line_Insert; expected 9, got 7" | 7 | 2 |
| sp_Dunnage_Loads_GetByDateRange | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Loads_GetByDateRange; expected 2, got 4" | 2 | 0 |
| sp_Dunnage_Parts_GetTransactionCount | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Parts_GetTransactionCount; expected 2, got 1" | 1 | 1 |
| sp_Dunnage_Parts_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Parts_Insert; expected 6, got 5" | 5 | 1 |
| sp_Dunnage_Specs_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Specs_Insert; expected 5, got 4" | 4 | 1 |
| sp_Dunnage_Types_CheckDuplicate | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Types_CheckDuplicate; expected 3, got 2" | 2 | 1 |
| sp_Dunnage_Types_Delete | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Types_Delete; expected 4, got 2" | 2 | 2 |
| sp_Dunnage_Types_GetPartCount | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Types_GetPartCount; expected 2, got 1" | 1 | 1 |
| sp_Dunnage_Types_GetTransactionCount | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_Types_GetTransactionCount; expected 2, got 1" | 1 | 1 |
| sp_Dunnage_UserPreferences_Upsert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Dunnage_UserPreferences_Upsert; expected 5, got 3" | 3 | 2 |
| sp_Receiving_History_Get | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Receiving_History_Get; expected 3, got 5" | 3 | 0 |
| sp_Receiving_Load_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Receiving_Load_Insert; expected 13, got 14" | 13 | 0 |
| sp_Receiving_Load_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Receiving_Load_Update; expected 13, got 14" | 13 | 0 |
| sp_Receiving_PackageTypeMappings_GetAll | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Receiving_PackageTypeMappings_GetAll; expected 1, got 2" | 1 | 0 |
| sp_Receiving_PackageTypeMappings_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Receiving_PackageTypeMappings_Insert; expected 5, got 6" | 5 | 0 |
| sp_Receiving_PackageTypeMappings_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Receiving_PackageTypeMappings_Update; expected 6, got 8" | 6 | 0 |
| sp_Receiving_PackageTypePreference_Save | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Receiving_PackageTypePreference_Save; expected 4, got 5" | 4 | 0 |
| sp_routing_label_archive | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_archive; expected 3, got 1" | 1 | 2 |
| sp_routing_label_check_duplicate | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_check_duplicate; expected 6, got 4" | 4 | 2 |
| sp_routing_label_delete | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_delete; expected 2, got 1" | 1 | 1 |
| sp_routing_label_get_all | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_get_all; expected 4, got 2" | 2 | 2 |
| sp_routing_label_get_by_id | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_get_by_id; expected 3, got 1" | 1 | 2 |
| sp_routing_label_history_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_history_insert; expected 7, got 5" | 5 | 2 |
| sp_routing_label_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_insert; expected 9, got 7" | 7 | 2 |
| sp_routing_label_mark_exported | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_mark_exported; expected 3, got 1" | 1 | 2 |
| sp_routing_label_update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_label_update; expected 8, got 7" | 7 | 1 |
| sp_routing_other_reason_get_all_active | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_other_reason_get_all_active; expected 2, got 0" | 0 | 2 |
| sp_routing_recipient_insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_recipient_insert; expected 5, got 3" | 3 | 2 |
| sp_routing_usage_increment | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_usage_increment; expected 4, got 2" | 2 | 2 |
| sp_routing_user_preference_get | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_user_preference_get; expected 3, got 1" | 1 | 2 |
| sp_routing_user_preference_save | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_routing_user_preference_save; expected 5, got 4" | 3 | 2 |
| sp_Settings_ScheduledReport_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Settings_ScheduledReport_Insert; expected 5, got 6" | 5 | 0 |
| sp_Settings_ScheduledReport_ToggleActive | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Settings_ScheduledReport_ToggleActive; expected 2, got 3" | 2 | 0 |
| sp_Settings_ScheduledReport_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Settings_ScheduledReport_Update; expected 5, got 6" | 5 | 0 |
| sp_Settings_ScheduledReport_UpdateLastRun | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Settings_ScheduledReport_UpdateLastRun; expected 4, got 5" | 3 | 1 |
| sp_Settings_System_SetLocked | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Settings_System_SetLocked; expected 5, got 6" | 5 | 0 |
| sp_sp_Receiving_Line_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_sp_Receiving_Line_Insert; expected 12, got 10" | 10 | 2 |
| sp_Volvo_PartMaster_GetAll | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Volvo_PartMaster_GetAll; expected 1, got 2" | 1 | 0 |
| sp_Volvo_PartMaster_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Volvo_PartMaster_Insert; expected 3, got 4" | 3 | 0 |
| sp_Volvo_PartMaster_SetActive | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Volvo_PartMaster_SetActive; expected 2, got 3" | 2 | 0 |
| sp_volvo_part_check_references | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_volvo_part_check_references; expected 2, got 1" | 1 | 1 |
| sp_Volvo_ShipmentLine_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Volvo_ShipmentLine_Insert; expected 8, got 9" | 8 | 0 |
| sp_Volvo_ShipmentLine_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Volvo_ShipmentLine_Update; expected 6, got 7" | 6 | 0 |
| sp_Volvo_Shipment_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Incorrect number of arguments for PROCEDURE mtm_receiving_application.sp_Volvo_Shipment_Insert; expected 5, got 3" | 3 | 2 |

## 🔗 Constraint Violations

These stored procedures failed due to foreign key constraints (missing prerequisite data).

| SP Name | Message |
|---------|---------|
| sp_Dunnage_Loads_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot add or update a child row: a foreign key constraint fails (`mtm_receiving_application`.`dunnage_history`, CONSTRAINT `FK_dunnage_history_part_id` FOREIGN KEY (`part_id`) REFERENCES `dunnage_parts` (`part_id`))" |
| sp_Settings_User_Set | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot add or update a child row: a foreign key constraint fails (`mtm_receiving_application`.`settings_personal`, CONSTRAINT `settings_personal_ibfk_1` FOREIGN KEY (`setting_id`) REFERENCES `settings_universal` (`id`) ON DELETE CASCADE)" |
| sp_Volvo_PartComponent_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Cannot add or update a child row: a foreign key constraint fails (`mtm_receiving_application`.`volvo_part_components`, CONSTRAINT `fk_volvo_part_components_parent` FOREIGN KEY (`parent_part_number`) REFERENCES `volvo_masterdata` (`part_number`) ON DELETE CAS)" |

**Recommendation:** Run with `-UseExecutionOrder` flag or add prerequisite test data.

## 📋 Data Validation Errors

These stored procedures failed due to data validation issues.

| SP Name | Message |
|---------|---------|
| sp_Dunnage_Types_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Dunnage type name already exists" |
| sp_Dunnage_UserPreferences_GetRecentIcons | Exception calling "ExecuteNonQuery" with "0" argument(s): "Illegal mix of collations (utf8mb4_general_ci,IMPLICIT) and (utf8mb4_unicode_ci,IMPLICIT) for operation '='" |
| sp_routing_recipient_get_by_name | Exception calling "ExecuteNonQuery" with "0" argument(s): "Illegal mix of collations (utf8mb4_general_ci,IMPLICIT) and (utf8mb4_unicode_ci,IMPLICIT) for operation '='" |
| sp_Settings_RoutingRule_Insert | Exception calling "ExecuteNonQuery" with "0" argument(s): "Data truncated for column 'match_type' at row 1" |
| sp_Settings_RoutingRule_Update | Exception calling "ExecuteNonQuery" with "0" argument(s): "Data truncated for column 'match_type' at row 1" |

**Recommendation:** Review mock data values in `01-mock-data.json`.

## 🔧 Other Runtime Errors

| SP Name | Category | Message |
|---------|----------|---------|
| sp_routing_recipient_update | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "OUT or INOUT argument 6 for routine mtm_receiving_application.sp_routing_recipient_update is not a variable or NEW pseudo-variable in BEFORE trigger" |
| sp_Settings_System_ResetToDefault | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Setting not found" |
| sp_Settings_System_UpdateValue | RuntimeError | Exception calling "ExecuteNonQuery" with "0" argument(s): "Setting not found" |

## 💡 Recommendations

### High Priority: Fix Parameter Mismatches
- 49 stored procedure(s) have parameter count issues
- Regenerate mock data: `pwsh -File .\Database\00-Test\01-Generate-SP-TestData.ps1`

### Improve Test Data
- 3 stored procedure(s) failed due to missing FK references
- Use dependency-aware execution: `pwsh -File .\Database\00-Test\02-Test-StoredProcedures.ps1 -UseExecutionOrder`
- Or add prerequisite test data to the database

### Needs Attention
- Only 59.7% success rate
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


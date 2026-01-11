# Stored Procedure Usage Analysis

**Generated:** 2026-01-11 13:34:59
**Database:** mtm_receiving_application
**Total Stored Procedures:** 153

## Summary

| Metric | Count | Percentage |
|--------|-------|------------|
| **Used Stored Procedures** | 141 | 92.2% |
| **Unused Stored Procedures** | 12 | 7.8% |
| **Total Usage Locations** | 198 | - |

## Stored Procedures in Use

| SP Name | File | Class | Method | Line | Has Callers | Caller Count |
|---------|------|-------|--------|------|-------------|--------------|
| [`dunnage_line_Insert`](sp-reports/Settings/IN/dunnage_line_Insert.md) | Module_Dunnage\Data\Dao_DunnageLine.cs | Dao_DunnageLine | `InsertDunnageLineAsync` | 67 | ✓ Yes | 1 |
| [`receiving_line_Insert`](sp-reports/Receiving/IN/receiving_line_Insert.md) | Module_Receiving\Data\Dao_ReceivingLine.cs | Dao_ReceivingLine | `InsertReceivingLineAsync` | 64 | ✓ Yes | 3 |
| [`sp_CreateNewUser`](sp-reports/Authentication/IN/sp_CreateNewUser.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `CreateNewUserAsync` | 94 | ✓ Yes | 4 |
| [`sp_CreateNewUser`](sp-reports/Authentication/IN/sp_CreateNewUser.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `CreateNewUserAsync` | 127 | ✓ Yes | 4 |
| [`sp_custom_fields_get_by_type`](sp-reports/Dunnage/IN/sp_custom_fields_get_by_type.md) | Module_Dunnage\Data\Dao_DunnageCustomField.cs | Dao_DunnageCustomField | `GetByTypeAsync` | 66 | ✓ Yes | 4 |
| [`sp_custom_fields_insert`](sp-reports/Dunnage/IN/sp_custom_fields_insert.md) | Module_Dunnage\Data\Dao_DunnageCustomField.cs | Dao_DunnageCustomField | `InsertAsync` | 40 | ✓ Yes | 21 |
| [`sp_DeletePackageTypePreference`](sp-reports/Other/IN/sp_DeletePackageTypePreference.md) | Module_Receiving\Data\Dao_PackageTypePreference.cs | Dao_PackageTypePreference | `DeletePreferenceAsync` | 162 | ✓ Yes | 4 |
| [`sp_DeleteReceivingLoad`](sp-reports/Receiving/IN/sp_DeleteReceivingLoad.md) | Module_Receiving\Data\Dao_ReceivingLoad.cs | Dao_ReceivingLoad | `DeleteLoadsAsync` | 174 | ✓ Yes | 2 |
| [`sp_dunnage_loads_delete`](sp-reports/Dunnage/IN/sp_dunnage_loads_delete.md) | Module_Dunnage\Data\Dao_DunnageLoad.cs | Dao_DunnageLoad | `DeleteAsync` | 176 | ✓ Yes | 15 |
| [`sp_dunnage_loads_get_all`](sp-reports/Dunnage/IN/sp_dunnage_loads_get_all.md) | Module_Dunnage\Data\Dao_DunnageLoad.cs | Dao_DunnageLoad | `GetAllAsync` | 29 | ✓ Yes | 21 |
| [`sp_dunnage_loads_get_by_date_range`](sp-reports/Dunnage/IN/sp_dunnage_loads_get_by_date_range.md) | Module_Dunnage\Data\Dao_DunnageLoad.cs | Dao_DunnageLoad | `GetByDateRangeAsync` | 44 | ✓ Yes | 2 |
| [`sp_dunnage_loads_get_by_id`](sp-reports/Dunnage/IN/sp_dunnage_loads_get_by_id.md) | Module_Dunnage\Data\Dao_DunnageLoad.cs | Dao_DunnageLoad | `GetByIdAsync` | 59 | ✓ Yes | 13 |
| [`sp_dunnage_loads_insert`](sp-reports/Dunnage/IN/sp_dunnage_loads_insert.md) | Module_Dunnage\Data\Dao_DunnageLoad.cs | Dao_DunnageLoad | `InsertAsync` | 77 | ✓ Yes | 21 |
| [`sp_dunnage_loads_insert`](sp-reports/Dunnage/IN/sp_dunnage_loads_insert.md) | Module_Dunnage\Data\Dao_DunnageLoad.cs | Dao_DunnageLoad | `InsertBatchAsync` | 132 | ✓ Yes | 2 |
| [`sp_dunnage_loads_insert_batch`](sp-reports/Dunnage/IN/sp_dunnage_loads_insert_batch.md) | Module_Dunnage\Data\Dao_DunnageLoad.cs | Dao_DunnageLoad | `InsertBatchAsync` | 132 | ✓ Yes | 2 |
| [`sp_dunnage_loads_update`](sp-reports/Dunnage/IN/sp_dunnage_loads_update.md) | Module_Dunnage\Data\Dao_DunnageLoad.cs | Dao_DunnageLoad | `UpdateAsync` | 162 | ✓ Yes | 19 |
| [`sp_dunnage_parts_count_transactions`](sp-reports/Dunnage/IN/sp_dunnage_parts_count_transactions.md) | Module_Dunnage\Data\Dao_DunnagePart.cs | Dao_DunnagePart | `CountTransactionsAsync` | 140 | ✓ Yes | 3 |
| [`sp_dunnage_parts_delete`](sp-reports/Dunnage/IN/sp_dunnage_parts_delete.md) | Module_Dunnage\Data\Dao_DunnagePart.cs | Dao_DunnagePart | `DeleteAsync` | 126 | ✓ Yes | 15 |
| [`sp_dunnage_parts_get_all`](sp-reports/Dunnage/IN/sp_dunnage_parts_get_all.md) | Module_Dunnage\Data\Dao_DunnagePart.cs | Dao_DunnagePart | `GetAllAsync` | 26 | ✓ Yes | 21 |
| [`sp_dunnage_parts_get_by_id`](sp-reports/Dunnage/IN/sp_dunnage_parts_get_by_id.md) | Module_Dunnage\Data\Dao_DunnagePart.cs | Dao_DunnagePart | `GetByIdAsync` | 59 | ✓ Yes | 13 |
| [`sp_dunnage_parts_get_by_type`](sp-reports/Dunnage/IN/sp_dunnage_parts_get_by_type.md) | Module_Dunnage\Data\Dao_DunnagePart.cs | Dao_DunnagePart | `GetByTypeAsync` | 41 | ✓ Yes | 4 |
| [`sp_dunnage_parts_insert`](sp-reports/Dunnage/IN/sp_dunnage_parts_insert.md) | Module_Dunnage\Data\Dao_DunnagePart.cs | Dao_DunnagePart | `InsertAsync` | 83 | ✓ Yes | 21 |
| [`sp_dunnage_parts_search`](sp-reports/Dunnage/IN/sp_dunnage_parts_search.md) | Module_Dunnage\Data\Dao_DunnagePart.cs | Dao_DunnagePart | `SearchAsync` | 156 | ✓ Yes | 1 |
| [`sp_dunnage_parts_update`](sp-reports/Dunnage/IN/sp_dunnage_parts_update.md) | Module_Dunnage\Data\Dao_DunnagePart.cs | Dao_DunnagePart | `UpdateAsync` | 112 | ✓ Yes | 19 |
| [`sp_dunnage_specs_count_parts_using_spec`](sp-reports/Dunnage/IN/sp_dunnage_specs_count_parts_using_spec.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `CountPartsUsingSpecAsync` | 149 | ✓ Yes | 2 |
| [`sp_dunnage_specs_delete_by_id`](sp-reports/Dunnage/IN/sp_dunnage_specs_delete_by_id.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `DeleteByIdAsync` | 120 | ✓ Yes | 2 |
| [`sp_dunnage_specs_delete_by_type`](sp-reports/Dunnage/IN/sp_dunnage_specs_delete_by_type.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `DeleteByTypeAsync` | 134 | ✓ Yes | 1 |
| [`sp_dunnage_specs_get_all`](sp-reports/Dunnage/IN/sp_dunnage_specs_get_all.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `GetAllAsync` | 41 | ✓ Yes | 21 |
| [`sp_dunnage_specs_get_all`](sp-reports/Dunnage/IN/sp_dunnage_specs_get_all.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `GetAllSpecKeysAsync` | 163 | ✓ Yes | 5 |
| [`sp_dunnage_specs_get_all_keys`](sp-reports/Dunnage/IN/sp_dunnage_specs_get_all_keys.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `GetAllSpecKeysAsync` | 163 | ✓ Yes | 5 |
| [`sp_dunnage_specs_get_by_id`](sp-reports/Dunnage/IN/sp_dunnage_specs_get_by_id.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `GetByIdAsync` | 55 | ✓ Yes | 13 |
| [`sp_dunnage_specs_get_by_type`](sp-reports/Dunnage/IN/sp_dunnage_specs_get_by_type.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `GetByTypeAsync` | 31 | ✓ Yes | 4 |
| [`sp_dunnage_specs_insert`](sp-reports/Dunnage/IN/sp_dunnage_specs_insert.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `InsertAsync` | 78 | ✓ Yes | 21 |
| [`sp_dunnage_specs_update`](sp-reports/Dunnage/IN/sp_dunnage_specs_update.md) | Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `UpdateAsync` | 106 | ✓ Yes | 19 |
| [`sp_dunnage_types_check_duplicate`](sp-reports/Dunnage/IN/sp_dunnage_types_check_duplicate.md) | Module_Dunnage\Data\Dao_DunnageType.cs | Dao_DunnageType | `CheckDuplicateNameAsync` | 168 | ✓ Yes | 2 |
| [`sp_dunnage_types_count_parts`](sp-reports/Dunnage/IN/sp_dunnage_types_count_parts.md) | Module_Dunnage\Data\Dao_DunnageType.cs | Dao_DunnageType | `CountPartsAsync` | 126 | ✓ Yes | 2 |
| [`sp_dunnage_types_count_transactions`](sp-reports/Dunnage/IN/sp_dunnage_types_count_transactions.md) | Module_Dunnage\Data\Dao_DunnageType.cs | Dao_DunnageType | `CountTransactionsAsync` | 141 | ✓ Yes | 3 |
| [`sp_dunnage_types_delete`](sp-reports/Dunnage/IN/sp_dunnage_types_delete.md) | Module_Dunnage\Data\Dao_DunnageType.cs | Dao_DunnageType | `DeleteAsync` | 112 | ✓ Yes | 15 |
| [`sp_dunnage_types_get_all`](sp-reports/Dunnage/IN/sp_dunnage_types_get_all.md) | Module_Dunnage\Data\Dao_DunnageType.cs | Dao_DunnageType | `GetAllAsync` | 29 | ✓ Yes | 21 |
| [`sp_dunnage_types_get_by_id`](sp-reports/Dunnage/IN/sp_dunnage_types_get_by_id.md) | Module_Dunnage\Data\Dao_DunnageType.cs | Dao_DunnageType | `GetByIdAsync` | 47 | ✓ Yes | 13 |
| [`sp_dunnage_types_insert`](sp-reports/Dunnage/IN/sp_dunnage_types_insert.md) | Module_Dunnage\Data\Dao_DunnageType.cs | Dao_DunnageType | `InsertAsync` | 69 | ✓ Yes | 21 |
| [`sp_dunnage_types_update`](sp-reports/Dunnage/IN/sp_dunnage_types_update.md) | Module_Dunnage\Data\Dao_DunnageType.cs | Dao_DunnageType | `UpdateAsync` | 98 | ✓ Yes | 19 |
| [`sp_GetAllReceivingLoads`](sp-reports/Authentication/IN/sp_GetAllReceivingLoads.md) | Module_Receiving\Data\Dao_ReceivingLoad.cs | Dao_ReceivingLoad | `GetAllAsync` | 243 | ✓ Yes | 21 |
| [`sp_GetDepartments`](sp-reports/Authentication/IN/sp_GetDepartments.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `GetActiveDepartmentsAsync` | 294 | ✓ Yes | 4 |
| [`sp_GetPackageTypePreference`](sp-reports/Authentication/IN/sp_GetPackageTypePreference.md) | Module_Receiving\Data\Dao_PackageTypePreference.cs | Dao_PackageTypePreference | `GetPreferenceAsync` | 93 | ✓ Yes | 4 |
| [`sp_GetReceivingHistory`](sp-reports/Authentication/IN/sp_GetReceivingHistory.md) | Module_Receiving\Data\Dao_ReceivingLoad.cs | Dao_ReceivingLoad | `GetHistoryAsync` | 210 | ✓ Yes | 6 |
| [`sp_GetSharedTerminalNames`](sp-reports/Authentication/IN/sp_GetSharedTerminalNames.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `GetSharedTerminalNamesAsync` | 256 | ✓ Yes | 2 |
| [`sp_GetUserByWindowsUsername`](sp-reports/Authentication/IN/sp_GetUserByWindowsUsername.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `GetUserByWindowsUsernameAsync` | 49 | ✓ Yes | 3 |
| [`sp_InsertReceivingLoad`](sp-reports/Receiving/IN/sp_InsertReceivingLoad.md) | Module_Receiving\Data\Dao_ReceivingLoad.cs | Dao_ReceivingLoad | `SaveLoadsAsync` | 68 | ✓ Yes | 8 |
| [`sp_inventoried_dunnage_check`](sp-reports/Dunnage/IN/sp_inventoried_dunnage_check.md) | Module_Dunnage\Data\Dao_InventoriedDunnage.cs | Dao_InventoriedDunnage | `CheckAsync` | 40 | ✓ Yes | 2 |
| [`sp_inventoried_dunnage_delete`](sp-reports/Dunnage/IN/sp_inventoried_dunnage_delete.md) | Module_Dunnage\Data\Dao_InventoriedDunnage.cs | Dao_InventoriedDunnage | `DeleteAsync` | 121 | ✓ Yes | 15 |
| [`sp_inventoried_dunnage_get_all`](sp-reports/Dunnage/IN/sp_inventoried_dunnage_get_all.md) | Module_Dunnage\Data\Dao_InventoriedDunnage.cs | Dao_InventoriedDunnage | `GetAllAsync` | 26 | ✓ Yes | 21 |
| [`sp_inventoried_dunnage_get_by_part`](sp-reports/Dunnage/IN/sp_inventoried_dunnage_get_by_part.md) | Module_Dunnage\Data\Dao_InventoriedDunnage.cs | Dao_InventoriedDunnage | `GetByPartAsync` | 55 | ✓ Yes | 3 |
| [`sp_inventoried_dunnage_insert`](sp-reports/Dunnage/IN/sp_inventoried_dunnage_insert.md) | Module_Dunnage\Data\Dao_InventoriedDunnage.cs | Dao_InventoriedDunnage | `InsertAsync` | 78 | ✓ Yes | 21 |
| [`sp_inventoried_dunnage_update`](sp-reports/Dunnage/IN/sp_inventoried_dunnage_update.md) | Module_Dunnage\Data\Dao_InventoriedDunnage.cs | Dao_InventoriedDunnage | `UpdateAsync` | 107 | ✓ Yes | 19 |
| [`sp_LogUserActivity`](sp-reports/Authentication/IN/sp_LogUserActivity.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `LogUserActivityAsync` | 230 | ✓ Yes | 4 |
| [`sp_PackageType_Delete`](sp-reports/Settings/IN/sp_PackageType_Delete.md) | Module_Settings\Data\Dao_PackageType.cs | Dao_PackageType | `DeleteAsync` | 110 | ✓ Yes | 15 |
| [`sp_PackageType_Delete`](sp-reports/Settings/IN/sp_PackageType_Delete.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 520 | ✓ Yes | 1 |
| [`sp_PackageType_GetAll`](sp-reports/Settings/IN/sp_PackageType_GetAll.md) | Module_Settings\Data\Dao_PackageType.cs | Dao_PackageType | `GetAllAsync` | 31 | ✓ Yes | 21 |
| [`sp_PackageType_GetAll`](sp-reports/Settings/IN/sp_PackageType_GetAll.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 516 | ✓ Yes | 1 |
| [`sp_PackageType_GetById`](sp-reports/Settings/IN/sp_PackageType_GetById.md) | Module_Settings\Data\Dao_PackageType.cs | Dao_PackageType | `GetByIdAsync` | 49 | ✓ Yes | 13 |
| [`sp_PackageType_GetById`](sp-reports/Settings/IN/sp_PackageType_GetById.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 517 | ✓ Yes | 1 |
| [`sp_PackageType_Insert`](sp-reports/Settings/IN/sp_PackageType_Insert.md) | Module_Settings\Data\Dao_PackageType.cs | Dao_PackageType | `InsertAsync` | 71 | ✓ Yes | 21 |
| [`sp_PackageType_Insert`](sp-reports/Settings/IN/sp_PackageType_Insert.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 518 | ✓ Yes | 1 |
| [`sp_PackageType_Update`](sp-reports/Settings/IN/sp_PackageType_Update.md) | Module_Settings\Data\Dao_PackageType.cs | Dao_PackageType | `UpdateAsync` | 92 | ✓ Yes | 19 |
| [`sp_PackageType_Update`](sp-reports/Settings/IN/sp_PackageType_Update.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 519 | ✓ Yes | 1 |
| [`sp_PackageType_UsageCount`](sp-reports/Settings/IN/sp_PackageType_UsageCount.md) | Module_Settings\Data\Dao_PackageType.cs | Dao_PackageType | `GetUsageCountAsync` | 128 | ✓ Yes | 3 |
| [`sp_PackageType_UsageCount`](sp-reports/Settings/IN/sp_PackageType_UsageCount.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 521 | ✓ Yes | 1 |
| [`sp_PackageTypeMappings_Delete`](sp-reports/Settings/IN/sp_PackageTypeMappings_Delete.md) | Module_Settings\Data\Dao_PackageTypeMappings.cs | Dao_PackageTypeMappings | `DeleteAsync` | 117 | ✓ Yes | 15 |
| [`sp_PackageTypeMappings_Delete`](sp-reports/Settings/IN/sp_PackageTypeMappings_Delete.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 527 | ✓ Yes | 1 |
| [`sp_PackageTypeMappings_GetAll`](sp-reports/Settings/IN/sp_PackageTypeMappings_GetAll.md) | Module_Settings\Data\Dao_PackageTypeMappings.cs | Dao_PackageTypeMappings | `GetAllAsync` | 31 | ✓ Yes | 21 |
| [`sp_PackageTypeMappings_GetAll`](sp-reports/Settings/IN/sp_PackageTypeMappings_GetAll.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 523 | ✓ Yes | 1 |
| [`sp_PackageTypeMappings_GetByPrefix`](sp-reports/Settings/IN/sp_PackageTypeMappings_GetByPrefix.md) | Module_Settings\Data\Dao_PackageTypeMappings.cs | Dao_PackageTypeMappings | `GetByPrefixAsync` | 49 | ✓ Yes | 1 |
| [`sp_PackageTypeMappings_GetByPrefix`](sp-reports/Settings/IN/sp_PackageTypeMappings_GetByPrefix.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 524 | ✓ Yes | 1 |
| [`sp_PackageTypeMappings_Insert`](sp-reports/Settings/IN/sp_PackageTypeMappings_Insert.md) | Module_Settings\Data\Dao_PackageTypeMappings.cs | Dao_PackageTypeMappings | `InsertAsync` | 75 | ✓ Yes | 21 |
| [`sp_PackageTypeMappings_Insert`](sp-reports/Settings/IN/sp_PackageTypeMappings_Insert.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 525 | ✓ Yes | 1 |
| [`sp_PackageTypeMappings_Update`](sp-reports/Settings/IN/sp_PackageTypeMappings_Update.md) | Module_Settings\Data\Dao_PackageTypeMappings.cs | Dao_PackageTypeMappings | `UpdateAsync` | 99 | ✓ Yes | 19 |
| [`sp_PackageTypeMappings_Update`](sp-reports/Settings/IN/sp_PackageTypeMappings_Update.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 526 | ✓ Yes | 1 |
| [`sp_routing_label_check_duplicate`](sp-reports/Routing/IN/sp_routing_label_check_duplicate.md) | Module_Routing\Data\Dao_RoutingLabel.cs | Dao_RoutingLabel | `CheckDuplicateLabelAsync` | 265 | ✓ Yes | 5 |
| [`sp_routing_label_delete`](sp-reports/Routing/IN/sp_routing_label_delete.md) | Module_Routing\Data\Dao_RoutingLabel.cs | Dao_RoutingLabel | `DeleteLabelAsync` | 205 | ✓ Yes | 2 |
| [`sp_routing_label_get_all`](sp-reports/Routing/IN/sp_routing_label_get_all.md) | Module_Routing\Data\Dao_RoutingLabel.cs | Dao_RoutingLabel | `GetAllLabelsAsync` | 178 | ✓ Yes | 5 |
| [`sp_routing_label_get_by_id`](sp-reports/Routing/IN/sp_routing_label_get_by_id.md) | Module_Routing\Data\Dao_RoutingLabel.cs | Dao_RoutingLabel | `GetLabelByIdAsync` | 150 | ✓ Yes | 4 |
| [`sp_routing_label_history_insert`](sp-reports/Routing/IN/sp_routing_label_history_insert.md) | Module_Routing\Data\Dao_RoutingLabelHistory.cs | Dao_RoutingLabelHistory | `InsertHistoryAsync` | 44 | ✓ Yes | 2 |
| [`sp_routing_label_insert`](sp-reports/Routing/IN/sp_routing_label_insert.md) | Module_Routing\Data\Dao_RoutingLabel.cs | Dao_RoutingLabel | `InsertLabelAsync` | 52 | ✓ Yes | 3 |
| [`sp_routing_label_mark_exported`](sp-reports/Routing/IN/sp_routing_label_mark_exported.md) | Module_Routing\Data\Dao_RoutingLabel.cs | Dao_RoutingLabel | `MarkLabelExportedAsync` | 232 | ✓ Yes | 2 |
| [`sp_routing_label_update`](sp-reports/Routing/IN/sp_routing_label_update.md) | Module_Routing\Data\Dao_RoutingLabel.cs | Dao_RoutingLabel | `UpdateLabelAsync` | 124 | ✓ Yes | 5 |
| [`sp_routing_other_reason_get_all_active`](sp-reports/Routing/OUT/sp_routing_other_reason_get_all_active.md) | Module_Routing\Data\Dao_RoutingOtherReason.cs | Dao_RoutingOtherReason | `GetAllActiveReasonsAsync` | 32 | ✓ Yes | 1 |
| [`sp_routing_recipient_get_all`](sp-reports/Routing/IN/sp_routing_recipient_get_all.md) | Module_Routing\Data\Dao_RoutingRecipient.cs | Dao_RoutingRecipient | `GetAllActiveRecipientsAsync` | 32 | ✓ Yes | 3 |
| [`sp_routing_recipient_get_all_active`](sp-reports/Routing/IN/sp_routing_recipient_get_all_active.md) | Module_Routing\Data\Dao_RoutingRecipient.cs | Dao_RoutingRecipient | `GetAllActiveRecipientsAsync` | 32 | ✓ Yes | 3 |
| [`sp_routing_recipient_get_top_by_usage`](sp-reports/Routing/IN/sp_routing_recipient_get_top_by_usage.md) | Module_Routing\Data\Dao_RoutingRecipient.cs | Dao_RoutingRecipient | `GetTopRecipientsByUsageAsync` | 59 | ✓ Yes | 3 |
| [`sp_routing_usage_increment`](sp-reports/Routing/IN/sp_routing_usage_increment.md) | Module_Routing\Data\Dao_RoutingUsageTracking.cs | Dao_RoutingUsageTracking | `IncrementUsageAsync` | 39 | ✓ Yes | 3 |
| [`sp_routing_user_preference_get`](sp-reports/Routing/IN/sp_routing_user_preference_get.md) | Module_Routing\Data\Dao_RoutingUserPreference.cs | Dao_RoutingUserPreference | `GetUserPreferenceAsync` | 39 | ✓ Yes | 4 |
| [`sp_routing_user_preference_save`](sp-reports/Routing/IN/sp_routing_user_preference_save.md) | Module_Routing\Data\Dao_RoutingUserPreference.cs | Dao_RoutingUserPreference | `SaveUserPreferenceAsync` | 68 | ✓ Yes | 4 |
| [`sp_RoutingRule_Delete`](sp-reports/Routing/IN/sp_RoutingRule_Delete.md) | Module_Settings\Data\Dao_RoutingRule.cs | Dao_RoutingRule | `DeleteAsync` | 114 | ✓ Yes | 15 |
| [`sp_RoutingRule_Delete`](sp-reports/Routing/IN/sp_RoutingRule_Delete.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 535 | ✓ Yes | 1 |
| [`sp_RoutingRule_FindMatch`](sp-reports/Routing/IN/sp_RoutingRule_FindMatch.md) | Module_Settings\Data\Dao_RoutingRule.cs | Dao_RoutingRule | `FindMatchAsync` | 134 | ✓ Yes | 1 |
| [`sp_RoutingRule_FindMatch`](sp-reports/Routing/IN/sp_RoutingRule_FindMatch.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 531 | ✓ Yes | 1 |
| [`sp_RoutingRule_GetAll`](sp-reports/Routing/IN/sp_RoutingRule_GetAll.md) | Module_Settings\Data\Dao_RoutingRule.cs | Dao_RoutingRule | `GetAllAsync` | 31 | ✓ Yes | 21 |
| [`sp_RoutingRule_GetAll`](sp-reports/Routing/IN/sp_RoutingRule_GetAll.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 529 | ✓ Yes | 1 |
| [`sp_RoutingRule_GetById`](sp-reports/Routing/IN/sp_RoutingRule_GetById.md) | Module_Settings\Data\Dao_RoutingRule.cs | Dao_RoutingRule | `GetByIdAsync` | 49 | ✓ Yes | 13 |
| [`sp_RoutingRule_GetById`](sp-reports/Routing/IN/sp_RoutingRule_GetById.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 530 | ✓ Yes | 1 |
| [`sp_RoutingRule_GetByPartNumber`](sp-reports/Routing/IN/sp_RoutingRule_GetByPartNumber.md) | Module_Settings\Data\Dao_RoutingRule.cs | Dao_RoutingRule | `GetByPartNumberAsync` | 152 | ✓ Yes | 3 |
| [`sp_RoutingRule_GetByPartNumber`](sp-reports/Routing/IN/sp_RoutingRule_GetByPartNumber.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 532 | ✓ Yes | 1 |
| [`sp_RoutingRule_Insert`](sp-reports/Routing/IN/sp_RoutingRule_Insert.md) | Module_Settings\Data\Dao_RoutingRule.cs | Dao_RoutingRule | `InsertAsync` | 73 | ✓ Yes | 21 |
| [`sp_RoutingRule_Insert`](sp-reports/Routing/IN/sp_RoutingRule_Insert.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 533 | ✓ Yes | 1 |
| [`sp_RoutingRule_Update`](sp-reports/Routing/IN/sp_RoutingRule_Update.md) | Module_Settings\Data\Dao_RoutingRule.cs | Dao_RoutingRule | `UpdateAsync` | 96 | ✓ Yes | 19 |
| [`sp_RoutingRule_Update`](sp-reports/Routing/IN/sp_RoutingRule_Update.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 534 | ✓ Yes | 1 |
| [`sp_SavePackageTypePreference`](sp-reports/Other/IN/sp_SavePackageTypePreference.md) | Module_Receiving\Data\Dao_PackageTypePreference.cs | Dao_PackageTypePreference | `SavePreferenceAsync` | 133 | ✓ Yes | 5 |
| [`sp_ScheduledReport_Delete`](sp-reports/Settings/IN/sp_ScheduledReport_Delete.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `DeleteAsync` | 160 | ✓ Yes | 15 |
| [`sp_ScheduledReport_Delete`](sp-reports/Settings/IN/sp_ScheduledReport_Delete.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 545 | ✓ Yes | 1 |
| [`sp_ScheduledReport_GetActive`](sp-reports/Settings/IN/sp_ScheduledReport_GetActive.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `GetActiveAsync` | 74 | ✓ Yes | 1 |
| [`sp_ScheduledReport_GetActive`](sp-reports/Settings/IN/sp_ScheduledReport_GetActive.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 538 | ✓ Yes | 1 |
| [`sp_ScheduledReport_GetAll`](sp-reports/Settings/IN/sp_ScheduledReport_GetAll.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `GetAllAsync` | 31 | ✓ Yes | 21 |
| [`sp_ScheduledReport_GetAll`](sp-reports/Settings/IN/sp_ScheduledReport_GetAll.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 537 | ✓ Yes | 1 |
| [`sp_ScheduledReport_GetById`](sp-reports/Settings/IN/sp_ScheduledReport_GetById.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `GetByIdAsync` | 49 | ✓ Yes | 13 |
| [`sp_ScheduledReport_GetById`](sp-reports/Settings/IN/sp_ScheduledReport_GetById.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 539 | ✓ Yes | 1 |
| [`sp_ScheduledReport_GetDue`](sp-reports/Settings/IN/sp_ScheduledReport_GetDue.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `GetDueAsync` | 62 | ✓ Yes | 1 |
| [`sp_ScheduledReport_GetDue`](sp-reports/Settings/IN/sp_ScheduledReport_GetDue.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 540 | ✓ Yes | 1 |
| [`sp_ScheduledReport_Insert`](sp-reports/Settings/IN/sp_ScheduledReport_Insert.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `InsertAsync` | 97 | ✓ Yes | 21 |
| [`sp_ScheduledReport_Insert`](sp-reports/Settings/IN/sp_ScheduledReport_Insert.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 541 | ✓ Yes | 1 |
| [`sp_ScheduledReport_ToggleActive`](sp-reports/Settings/IN/sp_ScheduledReport_ToggleActive.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `ToggleActiveAsync` | 180 | ✓ Yes | 1 |
| [`sp_ScheduledReport_ToggleActive`](sp-reports/Settings/IN/sp_ScheduledReport_ToggleActive.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 544 | ✓ Yes | 1 |
| [`sp_ScheduledReport_Update`](sp-reports/Settings/IN/sp_ScheduledReport_Update.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `UpdateAsync` | 120 | ✓ Yes | 19 |
| [`sp_ScheduledReport_Update`](sp-reports/Settings/IN/sp_ScheduledReport_Update.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `UpdateLastRunAsync` | 142 | ✓ Yes | 1 |
| [`sp_ScheduledReport_Update`](sp-reports/Settings/IN/sp_ScheduledReport_Update.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 542 | ✓ Yes | 1 |
| [`sp_ScheduledReport_Update`](sp-reports/Settings/IN/sp_ScheduledReport_Update.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 543 | ✓ Yes | 1 |
| [`sp_ScheduledReport_UpdateLastRun`](sp-reports/Settings/IN/sp_ScheduledReport_UpdateLastRun.md) | Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `UpdateLastRunAsync` | 142 | ✓ Yes | 1 |
| [`sp_ScheduledReport_UpdateLastRun`](sp-reports/Settings/IN/sp_ScheduledReport_UpdateLastRun.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 543 | ✓ Yes | 1 |
| [`sp_seed_user_default_modes`](sp-reports/Authentication/IN/sp_seed_user_default_modes.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `CreateNewUserAsync` | 127 | ✓ Yes | 4 |
| [`sp_seed_user_default_modes`](sp-reports/Authentication/IN/sp_seed_user_default_modes.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `CreateNewUserAsync` | 133 | ✓ Yes | 4 |
| [`sp_SettingsAuditLog_Get`](sp-reports/Settings/IN/sp_SettingsAuditLog_Get.md) | Module_Settings\Data\Dao_SettingsAuditLog.cs | Dao_SettingsAuditLog | `GetAsync` | 39 | ✓ Yes | 3 |
| [`sp_SettingsAuditLog_Get`](sp-reports/Settings/IN/sp_SettingsAuditLog_Get.md) | Module_Settings\Data\Dao_SettingsAuditLog.cs | Dao_SettingsAuditLog | `GetBySettingAsync` | 58 | ✓ Yes | 1 |
| [`sp_SettingsAuditLog_Get`](sp-reports/Settings/IN/sp_SettingsAuditLog_Get.md) | Module_Settings\Data\Dao_SettingsAuditLog.cs | Dao_SettingsAuditLog | `GetByUserAsync` | 77 | ✓ Yes | 2 |
| [`sp_SettingsAuditLog_Get`](sp-reports/Settings/IN/sp_SettingsAuditLog_Get.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 547 | ✓ Yes | 1 |
| [`sp_SettingsAuditLog_Get`](sp-reports/Settings/IN/sp_SettingsAuditLog_Get.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 548 | ✓ Yes | 1 |
| [`sp_SettingsAuditLog_Get`](sp-reports/Settings/IN/sp_SettingsAuditLog_Get.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 549 | ✓ Yes | 1 |
| [`sp_SettingsAuditLog_GetBySetting`](sp-reports/Settings/IN/sp_SettingsAuditLog_GetBySetting.md) | Module_Settings\Data\Dao_SettingsAuditLog.cs | Dao_SettingsAuditLog | `GetBySettingAsync` | 58 | ✓ Yes | 1 |
| [`sp_SettingsAuditLog_GetBySetting`](sp-reports/Settings/IN/sp_SettingsAuditLog_GetBySetting.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 548 | ✓ Yes | 1 |
| [`sp_SettingsAuditLog_GetByUser`](sp-reports/Settings/IN/sp_SettingsAuditLog_GetByUser.md) | Module_Settings\Data\Dao_SettingsAuditLog.cs | Dao_SettingsAuditLog | `GetByUserAsync` | 77 | ✓ Yes | 2 |
| [`sp_SettingsAuditLog_GetByUser`](sp-reports/Settings/IN/sp_SettingsAuditLog_GetByUser.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 549 | ✓ Yes | 1 |
| [`sp_SystemSettings_GetAll`](sp-reports/Settings/IN/sp_SystemSettings_GetAll.md) | Module_Settings\Data\Dao_SystemSettings.cs | Dao_SystemSettings | `GetAllAsync` | 31 | ✓ Yes | 21 |
| [`sp_SystemSettings_GetAll`](sp-reports/Settings/IN/sp_SystemSettings_GetAll.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 501 | ✓ Yes | 1 |
| [`sp_SystemSettings_GetByCategory`](sp-reports/Settings/IN/sp_SystemSettings_GetByCategory.md) | Module_Settings\Data\Dao_SystemSettings.cs | Dao_SystemSettings | `GetByCategoryAsync` | 49 | ✓ Yes | 1 |
| [`sp_SystemSettings_GetByCategory`](sp-reports/Settings/IN/sp_SystemSettings_GetByCategory.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 502 | ✓ Yes | 1 |
| [`sp_SystemSettings_GetByKey`](sp-reports/Settings/IN/sp_SystemSettings_GetByKey.md) | Module_Settings\Data\Dao_SystemSettings.cs | Dao_SystemSettings | `GetByKeyAsync` | 70 | ✓ Yes | 1 |
| [`sp_SystemSettings_GetByKey`](sp-reports/Settings/IN/sp_SystemSettings_GetByKey.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 503 | ✓ Yes | 1 |
| [`sp_SystemSettings_ResetToDefault`](sp-reports/Settings/IN/sp_SystemSettings_ResetToDefault.md) | Module_Settings\Data\Dao_SystemSettings.cs | Dao_SystemSettings | `ResetToDefaultAsync` | 130 | ✓ Yes | 1 |
| [`sp_SystemSettings_ResetToDefault`](sp-reports/Settings/IN/sp_SystemSettings_ResetToDefault.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 506 | ✓ Yes | 1 |
| [`sp_SystemSettings_SetLocked`](sp-reports/Settings/IN/sp_SystemSettings_SetLocked.md) | Module_Settings\Data\Dao_SystemSettings.cs | Dao_SystemSettings | `SetLockedAsync` | 161 | ✓ Yes | 1 |
| [`sp_SystemSettings_SetLocked`](sp-reports/Settings/IN/sp_SystemSettings_SetLocked.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 507 | ✓ Yes | 1 |
| [`sp_SystemSettings_UpdateValue`](sp-reports/Settings/IN/sp_SystemSettings_UpdateValue.md) | Module_Settings\Data\Dao_SystemSettings.cs | Dao_SystemSettings | `UpdateValueAsync` | 102 | ✓ Yes | 1 |
| [`sp_SystemSettings_UpdateValue`](sp-reports/Settings/IN/sp_SystemSettings_UpdateValue.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 504 | ✓ Yes | 1 |
| [`sp_update_user_default_dunnage_mode`](sp-reports/Authentication/IN/sp_update_user_default_dunnage_mode.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `UpdateDefaultDunnageModeAsync` | 389 | ✓ Yes | 4 |
| [`sp_update_user_default_mode`](sp-reports/Authentication/IN/sp_update_user_default_mode.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `UpdateDefaultModeAsync` | 349 | ✓ Yes | 3 |
| [`sp_update_user_default_receiving_mode`](sp-reports/Authentication/IN/sp_update_user_default_receiving_mode.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `UpdateDefaultReceivingModeAsync` | 369 | ✓ Yes | 4 |
| [`sp_UpdateReceivingLoad`](sp-reports/Receiving/IN/sp_UpdateReceivingLoad.md) | Module_Receiving\Data\Dao_ReceivingLoad.cs | Dao_ReceivingLoad | `UpdateLoadsAsync` | 127 | ✓ Yes | 2 |
| [`sp_user_preferences_get_recent_icons`](sp-reports/Dunnage/IN/sp_user_preferences_get_recent_icons.md) | Module_Dunnage\Data\Dao_DunnageUserPreference.cs | Dao_DunnageUserPreference | `GetRecentlyUsedIconsAsync` | 47 | ✓ Yes | 4 |
| [`sp_user_preferences_upsert`](sp-reports/Dunnage/IN/sp_user_preferences_upsert.md) | Module_Dunnage\Data\Dao_DunnageUserPreference.cs | Dao_DunnageUserPreference | `UpsertAsync` | 32 | ✓ Yes | 3 |
| [`sp_UserSettings_Get`](sp-reports/Settings/IN/sp_UserSettings_Get.md) | Module_Settings\Data\Dao_UserSettings.cs | Dao_UserSettings | `GetAsync` | 41 | ✓ Yes | 3 |
| [`sp_UserSettings_Get`](sp-reports/Settings/IN/sp_UserSettings_Get.md) | Module_Settings\Data\Dao_UserSettings.cs | Dao_UserSettings | `GetAllForUserAsync` | 62 | ✓ Yes | 2 |
| [`sp_UserSettings_Get`](sp-reports/Settings/IN/sp_UserSettings_Get.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 509 | ✓ Yes | 1 |
| [`sp_UserSettings_Get`](sp-reports/Settings/IN/sp_UserSettings_Get.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 512 | ✓ Yes | 1 |
| [`sp_UserSettings_GetAllForUser`](sp-reports/Settings/IN/sp_UserSettings_GetAllForUser.md) | Module_Settings\Data\Dao_UserSettings.cs | Dao_UserSettings | `GetAllForUserAsync` | 62 | ✓ Yes | 2 |
| [`sp_UserSettings_GetAllForUser`](sp-reports/Settings/IN/sp_UserSettings_GetAllForUser.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 512 | ✓ Yes | 1 |
| [`sp_UserSettings_Reset`](sp-reports/Settings/IN/sp_UserSettings_Reset.md) | Module_Settings\Data\Dao_UserSettings.cs | Dao_UserSettings | `ResetAsync` | 105 | ✓ Yes | 1 |
| [`sp_UserSettings_Reset`](sp-reports/Settings/IN/sp_UserSettings_Reset.md) | Module_Settings\Data\Dao_UserSettings.cs | Dao_UserSettings | `ResetAllAsync` | 126 | ✓ Yes | 1 |
| [`sp_UserSettings_Reset`](sp-reports/Settings/IN/sp_UserSettings_Reset.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 513 | ✓ Yes | 1 |
| [`sp_UserSettings_Reset`](sp-reports/Settings/IN/sp_UserSettings_Reset.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 514 | ✓ Yes | 1 |
| [`sp_UserSettings_ResetAll`](sp-reports/Settings/IN/sp_UserSettings_ResetAll.md) | Module_Settings\Data\Dao_UserSettings.cs | Dao_UserSettings | `ResetAllAsync` | 126 | ✓ Yes | 1 |
| [`sp_UserSettings_ResetAll`](sp-reports/Settings/IN/sp_UserSettings_ResetAll.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 514 | ✓ Yes | 1 |
| [`sp_UserSettings_Set`](sp-reports/Settings/IN/sp_UserSettings_Set.md) | Module_Settings\Data\Dao_UserSettings.cs | Dao_UserSettings | `SetAsync` | 85 | ✓ Yes | 1 |
| [`sp_UserSettings_Set`](sp-reports/Settings/IN/sp_UserSettings_Set.md) | Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 510 | ✓ Yes | 1 |
| [`sp_ValidateUserPin`](sp-reports/Authentication/IN/sp_ValidateUserPin.md) | Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `ValidateUserPinAsync` | 71 | ✓ Yes | 2 |
| [`sp_volvo_part_check_references`](sp-reports/Volvo/IN/sp_volvo_part_check_references.md) | Module_Volvo\Data\Dao_VolvoPart.cs | Dao_VolvoPart | `DeactivateAsync` | 118 | ✓ Yes | 3 |
| [`sp_volvo_part_component_delete_by_parent`](sp-reports/Volvo/IN/sp_volvo_part_component_delete_by_parent.md) | Module_Volvo\Data\Dao_VolvoPartComponent.cs | Dao_VolvoPartComponent | `DeleteByParentPartAsync` | 77 | ✓ Yes | 2 |
| [`sp_volvo_part_component_get`](sp-reports/Volvo/IN/sp_volvo_part_component_get.md) | Module_Volvo\Data\Dao_VolvoPartComponent.cs | Dao_VolvoPartComponent | `GetByParentPartAsync` | 38 | ✓ Yes | 3 |
| [`sp_volvo_part_component_insert`](sp-reports/Volvo/IN/sp_volvo_part_component_insert.md) | Module_Volvo\Data\Dao_VolvoPartComponent.cs | Dao_VolvoPartComponent | `InsertAsync` | 59 | ✓ Yes | 21 |
| [`sp_volvo_part_master_get_all`](sp-reports/Volvo/IN/sp_volvo_part_master_get_all.md) | Module_Volvo\Data\Dao_VolvoPart.cs | Dao_VolvoPart | `GetAllAsync` | 38 | ✓ Yes | 21 |
| [`sp_volvo_part_master_get_by_id`](sp-reports/Volvo/IN/sp_volvo_part_master_get_by_id.md) | Module_Volvo\Data\Dao_VolvoPart.cs | Dao_VolvoPart | `GetByIdAsync` | 57 | ✓ Yes | 13 |
| [`sp_volvo_part_master_insert`](sp-reports/Volvo/IN/sp_volvo_part_master_insert.md) | Module_Volvo\Data\Dao_VolvoPart.cs | Dao_VolvoPart | `InsertAsync` | 78 | ✓ Yes | 21 |
| [`sp_volvo_part_master_set_active`](sp-reports/Volvo/IN/sp_volvo_part_master_set_active.md) | Module_Volvo\Data\Dao_VolvoPart.cs | Dao_VolvoPart | `DeactivateAsync` | 135 | ✓ Yes | 3 |
| [`sp_volvo_part_master_update`](sp-reports/Volvo/IN/sp_volvo_part_master_update.md) | Module_Volvo\Data\Dao_VolvoPart.cs | Dao_VolvoPart | `UpdateAsync` | 97 | ✓ Yes | 19 |
| [`sp_volvo_settings_get`](sp-reports/Settings/IN/sp_volvo_settings_get.md) | Module_Volvo\Data\Dao_VolvoSettings.cs | Dao_VolvoSettings | `GetSettingAsync` | 35 | ✓ Yes | 2 |
| [`sp_volvo_settings_get`](sp-reports/Settings/IN/sp_volvo_settings_get.md) | Module_Volvo\Data\Dao_VolvoSettings.cs | Dao_VolvoSettings | `GetAllSettingsAsync` | 54 | ✓ Yes | 1 |
| [`sp_volvo_settings_get_all`](sp-reports/Settings/IN/sp_volvo_settings_get_all.md) | Module_Volvo\Data\Dao_VolvoSettings.cs | Dao_VolvoSettings | `GetAllSettingsAsync` | 54 | ✓ Yes | 1 |
| [`sp_volvo_settings_reset`](sp-reports/Settings/IN/sp_volvo_settings_reset.md) | Module_Volvo\Data\Dao_VolvoSettings.cs | Dao_VolvoSettings | `ResetSettingAsync` | 97 | ✓ Yes | 1 |
| [`sp_volvo_settings_upsert`](sp-reports/Settings/IN/sp_volvo_settings_upsert.md) | Module_Volvo\Data\Dao_VolvoSettings.cs | Dao_VolvoSettings | `UpsertSettingAsync` | 77 | ✓ Yes | 1 |
| [`sp_volvo_shipment_complete`](sp-reports/Volvo/IN/sp_volvo_shipment_complete.md) | Module_Volvo\Data\Dao_VolvoShipment.cs | Dao_VolvoShipment | `CompleteAsync` | 137 | ✓ Yes | 2 |
| [`sp_volvo_shipment_delete`](sp-reports/Volvo/IN/sp_volvo_shipment_delete.md) | Module_Volvo\Data\Dao_VolvoShipment.cs | Dao_VolvoShipment | `DeleteAsync` | 156 | ✓ Yes | 15 |
| [`sp_volvo_shipment_get_pending`](sp-reports/Volvo/IN/sp_volvo_shipment_get_pending.md) | Module_Volvo\Data\Dao_VolvoShipment.cs | Dao_VolvoShipment | `GetPendingAsync` | 169 | ✓ Yes | 3 |
| [`sp_volvo_shipment_history_get`](sp-reports/Volvo/IN/sp_volvo_shipment_history_get.md) | Module_Volvo\Data\Dao_VolvoShipment.cs | Dao_VolvoShipment | `GetHistoryAsync` | 253 | ✓ Yes | 6 |
| [`sp_volvo_shipment_insert`](sp-reports/Volvo/IN/sp_volvo_shipment_insert.md) | Module_Volvo\Data\Dao_VolvoShipment.cs | Dao_VolvoShipment | `InsertAsync` | 38 | ✓ Yes | 21 |
| [`sp_volvo_shipment_line_delete`](sp-reports/Volvo/IN/sp_volvo_shipment_line_delete.md) | Module_Volvo\Data\Dao_VolvoShipmentLine.cs | Dao_VolvoShipmentLine | `DeleteAsync` | 105 | ✓ Yes | 15 |
| [`sp_volvo_shipment_line_get_by_shipment`](sp-reports/Volvo/IN/sp_volvo_shipment_line_get_by_shipment.md) | Module_Volvo\Data\Dao_VolvoShipmentLine.cs | Dao_VolvoShipmentLine | `GetByShipmentIdAsync` | 63 | ✓ Yes | 3 |
| [`sp_volvo_shipment_line_insert`](sp-reports/Volvo/IN/sp_volvo_shipment_line_insert.md) | Module_Volvo\Data\Dao_VolvoShipmentLine.cs | Dao_VolvoShipmentLine | `InsertAsync` | 45 | ✓ Yes | 21 |
| [`sp_volvo_shipment_line_insert`](sp-reports/Volvo/IN/sp_volvo_shipment_line_insert.md) | Module_Volvo\Services\Service_Volvo.cs | Service_Volvo | `SaveShipmentAsync` | 685 | ✓ Yes | 3 |
| [`sp_volvo_shipment_line_update`](sp-reports/Volvo/IN/sp_volvo_shipment_line_update.md) | Module_Volvo\Data\Dao_VolvoShipmentLine.cs | Dao_VolvoShipmentLine | `UpdateAsync` | 87 | ✓ Yes | 19 |
| [`sp_volvo_shipment_update`](sp-reports/Volvo/IN/sp_volvo_shipment_update.md) | Module_Volvo\Data\Dao_VolvoShipment.cs | Dao_VolvoShipment | `UpdateAsync` | 102 | ✓ Yes | 19 |

## ⚠️ Unused Stored Procedures

| SP Name | Category | Recommendation |
|---------|----------|----------------|
| [`carrier_delivery_label_Insert`](sp-reports/Settings/IN/carrier_delivery_label_Insert.md) | Settings | Review for removal |
| [`sp_dunnage_parts_get_transaction_count`](sp-reports/Dunnage/IN/sp_dunnage_parts_get_transaction_count.md) | Dunnage | Check if used externally |
| [`sp_dunnage_types_get_part_count`](sp-reports/Dunnage/IN/sp_dunnage_types_get_part_count.md) | Dunnage | Check if used externally |
| [`sp_dunnage_types_get_transaction_count`](sp-reports/Dunnage/IN/sp_dunnage_types_get_transaction_count.md) | Dunnage | Check if used externally |
| [`sp_get_user_default_mode`](sp-reports/Authentication/IN/sp_get_user_default_mode.md) | Authentication | Check if used externally |
| [`sp_routing_label_archive`](sp-reports/Routing/IN/sp_routing_label_archive.md) | Routing | Review for removal |
| [`sp_routing_label_get_history`](sp-reports/Routing/IN/sp_routing_label_get_history.md) | Routing | Check if used externally |
| [`sp_routing_label_get_today`](sp-reports/Routing/IN/sp_routing_label_get_today.md) | Routing | Check if used externally |
| [`sp_routing_recipient_get_by_name`](sp-reports/Routing/IN/sp_routing_recipient_get_by_name.md) | Routing | Check if used externally |
| [`sp_routing_recipient_insert`](sp-reports/Routing/IN/sp_routing_recipient_insert.md) | Routing | Review for removal |
| [`sp_routing_recipient_update`](sp-reports/Routing/IN/sp_routing_recipient_update.md) | Routing | Review for removal |
| [`sp_UpsertUser`](sp-reports/Authentication/IN/sp_UpsertUser.md) | Authentication | Review for removal |

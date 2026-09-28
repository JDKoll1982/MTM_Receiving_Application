# Stored Procedure Test Checklist

Progress: 170/170 stored procedures covered

Update rule: mark completed procedures every 5 stored-procedure tests finished.

## Auth

- [x] sp_Auth_User_Deactivate
- [x] sp_Auth_User_GetAll
- [x] sp_Auth_User_Update
- [x] sp_Auth_User_UpdateVisualCredentials

## Authentication

- [x] sp_Auth_Department_GetAll
- [x] sp_Auth_Terminal_GetShared
- [x] sp_Auth_User_Create
- [x] sp_Auth_User_GetByWindowsUsername
- [x] sp_Auth_User_GetDefaultMode
- [x] sp_Auth_User_IsWindowsUsernameUnique
- [x] sp_Auth_User_SeedDefaultModes
- [x] sp_Auth_User_UpdateDefaultDunnageMode
- [x] sp_Auth_User_UpdateDefaultMode
- [x] sp_Auth_User_UpdateDefaultReceivingMode
- [x] sp_Auth_User_Upsert
- [x] sp_Auth_User_ValidatePin
- [x] sp_Auth_Workstation_Upsert

## Dunnage

- [x] sp_Dunnage_CustomFields_Delete
- [x] sp_Dunnage_CustomFields_GetByType
- [x] sp_Dunnage_CustomFields_Insert
- [x] sp_Dunnage_CustomFields_Update
- [x] sp_Dunnage_Inventory_Check
- [x] sp_Dunnage_Inventory_Delete
- [x] sp_Dunnage_Inventory_GetAll
- [x] sp_Dunnage_Inventory_GetByPart
- [x] sp_Dunnage_Inventory_Insert
- [x] sp_Dunnage_Inventory_Update
- [x] sp_Dunnage_LabelData_ClearToHistory
- [x] sp_Dunnage_LabelData_Delete
- [x] sp_Dunnage_LabelData_GetAll
- [x] sp_Dunnage_LabelData_Insert
- [x] sp_Dunnage_LabelData_InsertBatch
- [x] sp_Dunnage_LabelData_Update
- [x] sp_dunnage_loads_delete
- [x] sp_Dunnage_Loads_GetAll
- [x] sp_Dunnage_Loads_GetByDateRange
- [x] sp_Dunnage_Loads_GetById
- [x] sp_dunnage_loads_update
- [x] sp_Dunnage_NonPO_Delete
- [x] sp_Dunnage_NonPO_GetAll
- [x] sp_Dunnage_NonPO_PartDefault_GetByPartId
- [x] sp_Dunnage_NonPO_PartDefault_Upsert
- [x] sp_Dunnage_NonPO_Upsert
- [x] sp_Dunnage_Parts_CountTransactions
- [x] sp_dunnage_parts_delete
- [x] sp_Dunnage_Parts_GetAll
- [x] sp_Dunnage_Parts_GetById
- [x] sp_Dunnage_Parts_GetByType
- [x] sp_Dunnage_Parts_GetTransactionCount
- [x] sp_dunnage_parts_insert
- [x] sp_Dunnage_Parts_InsertWithInventory
- [x] sp_dunnage_parts_search
- [x] sp_dunnage_parts_update
- [x] sp_Dunnage_Parts_UpdateWithReferences
- [x] sp_Dunnage_QuantityTypes_GetAll
- [x] sp_Dunnage_QuantityTypes_InsertIfMissing
- [x] sp_Dunnage_Specs_CountPartsUsingSpec
- [x] sp_Dunnage_Specs_DeleteById
- [x] sp_Dunnage_Specs_DeleteByType
- [x] sp_Dunnage_Specs_GetAll
- [x] sp_Dunnage_Specs_GetAllKeys
- [x] sp_Dunnage_Specs_GetById
- [x] sp_Dunnage_Specs_GetByType
- [x] sp_dunnage_specs_insert
- [x] sp_dunnage_specs_update
- [x] sp_Dunnage_Types_CheckDuplicate
- [x] sp_Dunnage_Types_CountParts
- [x] sp_Dunnage_Types_CountTransactions
- [x] sp_dunnage_types_delete
- [x] sp_Dunnage_Types_GetAll
- [x] sp_Dunnage_Types_GetById
- [x] sp_Dunnage_Types_GetPartCount
- [x] sp_Dunnage_Types_GetTransactionCount
- [x] sp_Dunnage_Types_GetUsageCount
- [x] sp_dunnage_types_insert
- [x] sp_dunnage_types_update

## Receiving

- [x] sp_Receiving_History_Get
- [x] sp_Receiving_History_Import
- [x] sp_Receiving_LabelData_ClearToHistory
- [x] sp_Receiving_LabelData_Delete
- [x] sp_Receiving_LabelData_GetAll
- [x] sp_Receiving_LabelData_Insert
- [x] sp_Receiving_LabelData_InsertFromHistory
- [x] sp_Receiving_LabelData_Update
- [x] sp_Receiving_Line_Insert
- [x] sp_Receiving_Load_Delete
- [x] sp_Receiving_Load_GetAll
- [x] sp_Receiving_Load_Insert
- [x] sp_Receiving_Load_Update
- [x] sp_Receiving_NonPO_Delete
- [x] sp_Receiving_NonPO_GetAll
- [x] sp_Receiving_NonPO_PartDefault_GetByPartId
- [x] sp_Receiving_NonPO_PartDefault_Upsert
- [x] sp_Receiving_NonPO_Upsert
- [x] sp_Receiving_PackageTypeMappings_Delete
- [x] sp_Receiving_PackageTypeMappings_GetAll
- [x] sp_Receiving_PackageTypeMappings_GetByPrefix
- [x] sp_Receiving_PackageTypeMappings_Insert
- [x] sp_Receiving_PackageTypeMappings_Update
- [x] sp_Receiving_PackageTypePreference_Delete
- [x] sp_Receiving_PackageTypePreference_Get
- [x] sp_Receiving_PackageTypePreference_Save
- [x] sp_Receiving_PackageTypes_Delete
- [x] sp_Receiving_QualityHolds_GetByLoadID
- [x] sp_Receiving_QualityHolds_Insert
- [x] sp_Receiving_QualityHolds_Update

## Reporting

- [x] sp_Reporting_Availability_GetByDateRange
- [x] sp_Reporting_DunnageHistory_GetByDateRange
- [x] sp_Reporting_ReceivingHistory_GetByDateRange
- [x] sp_Reporting_VolvoHistory_GetByDateRange

## Settings

- [x] sp_Auth_Activity_Log
- [x] sp_Dunnage_UserPreferences_GetRecentIcons
- [x] sp_Dunnage_UserPreferences_Upsert
- [x] sp_Settings_ReportingRecipients_Delete
- [x] sp_Settings_ReportingRecipients_GetAll
- [x] sp_Settings_ReportingRecipients_Insert
- [x] sp_Settings_ReportingRecipients_Update
- [x] sp_Settings_ScheduledReport_Delete
- [x] sp_Settings_ScheduledReport_GetActive
- [x] sp_Settings_ScheduledReport_GetAll
- [x] sp_Settings_ScheduledReport_GetById
- [x] sp_Settings_ScheduledReport_GetDue
- [x] sp_Settings_ScheduledReport_Insert
- [x] sp_Settings_ScheduledReport_ToggleActive
- [x] sp_Settings_ScheduledReport_Update
- [x] sp_Settings_ScheduledReport_UpdateLastRun
- [x] sp_Settings_VolvoRecipients_Delete
- [x] sp_Settings_VolvoRecipients_GetAll
- [x] sp_Settings_VolvoRecipients_Insert
- [x] sp_Settings_VolvoRecipients_Update
- [x] sp_SettingsCore
- [x] sp_SoftwareVersion_GetCurrent
- [x] sp_SoftwareVersion_Upsert
- [x] sp_Volvo_Settings_Get
- [x] sp_Volvo_Settings_GetAll
- [x] sp_Volvo_Settings_Reset
- [x] sp_Volvo_Settings_Upsert

## Volvo

- [x] sp_Volvo_GeneratedLabelData_ClearToHistory
- [x] sp_Volvo_GeneratedLabelData_DeleteByShipment
- [x] sp_Volvo_GeneratedLabelData_GetAll
- [x] sp_Volvo_GeneratedLabelData_Insert
- [x] sp_Volvo_LabelData_ClearToHistory
- [x] sp_volvo_part_check_references
- [x] sp_Volvo_PartComponent_DeleteByParent
- [x] sp_Volvo_PartComponent_Get
- [x] sp_Volvo_PartComponent_Insert
- [x] sp_Volvo_PartMaster_GetAll
- [x] sp_Volvo_PartMaster_GetById
- [x] sp_Volvo_PartMaster_Insert
- [x] sp_Volvo_PartMaster_SetActive
- [x] sp_Volvo_PartMaster_Update
- [x] sp_volvo_shipment_complete
- [x] sp_volvo_shipment_delete
- [x] sp_Volvo_Shipment_GetById
- [x] sp_Volvo_Shipment_GetHistory
- [x] sp_Volvo_Shipment_GetNextShipmentNumber
- [x] sp_Volvo_Shipment_GetPending
- [x] sp_volvo_shipment_insert
- [x] sp_volvo_shipment_update
- [x] sp_Volvo_ShipmentHistory_Delete
- [x] sp_Volvo_ShipmentHistory_GetById
- [x] sp_Volvo_ShipmentLine_Delete
- [x] sp_Volvo_ShipmentLine_GetByShipment
- [x] sp_Volvo_ShipmentLine_Insert
- [x] sp_Volvo_ShipmentLine_Update
- [x] sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId
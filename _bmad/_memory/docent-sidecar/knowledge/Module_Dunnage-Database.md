---
module_name: Module_Dunnage
component: database-mapping
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Dunnage - Database Schema Details

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Dunnage.md](../docent-sidecar/knowledge/Module_Dunnage.md)

## MySQL Stored Procedures (Observed in Code)

All procedures below are referenced by Module_Dunnage DAOs via `Helper_Database_StoredProcedure`.

### Dunnage Loads

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageLoad | sp_Dunnage_Loads_GetAll | Database/StoredProcedures/Dunnage/sp_Dunnage_Loads_GetAll.sql |
| Dao_DunnageLoad | sp_Dunnage_Loads_GetByDateRange | Database/StoredProcedures/Dunnage/sp_Dunnage_Loads_GetByDateRange.sql |
| Dao_DunnageLoad | sp_Dunnage_Loads_GetById | Database/StoredProcedures/Dunnage/sp_Dunnage_Loads_GetById.sql |
| Dao_DunnageLoad | sp_Dunnage_Loads_Insert | Database/StoredProcedures/Dunnage/sp_Dunnage_Loads_Insert.sql |
| Dao_DunnageLoad | sp_Dunnage_Loads_InsertBatch | Database/StoredProcedures/Dunnage/sp_Dunnage_Loads_InsertBatch.sql |
| Dao_DunnageLoad | sp_Dunnage_Loads_Update | Database/StoredProcedures/Dunnage/sp_Dunnage_Loads_Update.sql |
| Dao_DunnageLoad | sp_Dunnage_Loads_Delete | Database/StoredProcedures/Dunnage/sp_Dunnage_Loads_Delete.sql |

### Dunnage Types

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageType | sp_Dunnage_Types_GetAll | Database/StoredProcedures/Dunnage/sp_Dunnage_Types_GetAll.sql |
| Dao_DunnageType | sp_Dunnage_Types_GetById | Database/StoredProcedures/Dunnage/sp_Dunnage_Types_GetById.sql |
| Dao_DunnageType | sp_dunnage_types_insert | Database/StoredProcedures/Dunnage/sp_dunnage_types_insert.sql |
| Dao_DunnageType | sp_dunnage_types_update | Database/StoredProcedures/Dunnage/sp_dunnage_types_update.sql |
| Dao_DunnageType | sp_dunnage_types_delete | Database/StoredProcedures/Dunnage/sp_dunnage_types_delete.sql |
| Dao_DunnageType | sp_Dunnage_Types_CountParts | Database/StoredProcedures/Dunnage/sp_Dunnage_Types_CountParts.sql |
| Dao_DunnageType | sp_Dunnage_Types_CountTransactions | Database/StoredProcedures/Dunnage/sp_Dunnage_Types_CountTransactions.sql |
| Dao_DunnageType | sp_Dunnage_Types_CheckDuplicate | Database/StoredProcedures/Dunnage/sp_Dunnage_Types_CheckDuplicate.sql |

### Dunnage Parts

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnagePart | sp_Dunnage_Parts_GetAll | Database/StoredProcedures/Dunnage/sp_Dunnage_Parts_GetAll.sql |
| Dao_DunnagePart | sp_Dunnage_Parts_GetByType | Database/StoredProcedures/Dunnage/sp_Dunnage_Parts_GetByType.sql |
| Dao_DunnagePart | sp_Dunnage_Parts_GetById | Database/StoredProcedures/Dunnage/sp_Dunnage_Parts_GetById.sql |
| Dao_DunnagePart | sp_dunnage_parts_insert | Database/StoredProcedures/Dunnage/sp_dunnage_parts_insert.sql |
| Dao_DunnagePart | sp_dunnage_parts_update | Database/StoredProcedures/Dunnage/sp_dunnage_parts_update.sql |
| Dao_DunnagePart | sp_dunnage_parts_delete | Database/StoredProcedures/Dunnage/sp_dunnage_parts_delete.sql |
| Dao_DunnagePart | sp_Dunnage_Parts_CountTransactions | Database/StoredProcedures/Dunnage/sp_Dunnage_Parts_CountTransactions.sql |
| Dao_DunnagePart | sp_dunnage_parts_search | Database/StoredProcedures/Dunnage/sp_dunnage_parts_search.sql |

### Dunnage Specs

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageSpec | sp_Dunnage_Specs_GetByType | Database/StoredProcedures/Dunnage/sp_Dunnage_Specs_GetByType.sql |
| Dao_DunnageSpec | sp_Dunnage_Specs_GetAll | Database/StoredProcedures/Dunnage/sp_Dunnage_Specs_GetAll.sql |
| Dao_DunnageSpec | sp_Dunnage_Specs_GetById | Database/StoredProcedures/Dunnage/sp_Dunnage_Specs_GetById.sql |
| Dao_DunnageSpec | sp_dunnage_specs_insert | Database/StoredProcedures/Dunnage/sp_dunnage_specs_insert.sql |
| Dao_DunnageSpec | sp_dunnage_specs_update | Database/StoredProcedures/Dunnage/sp_dunnage_specs_update.sql |
| Dao_DunnageSpec | sp_Dunnage_Specs_DeleteById | Database/StoredProcedures/Dunnage/sp_Dunnage_Specs_DeleteById.sql |
| Dao_DunnageSpec | sp_Dunnage_Specs_DeleteByType | Database/StoredProcedures/Dunnage/sp_Dunnage_Specs_DeleteByType.sql |
| Dao_DunnageSpec | sp_Dunnage_Specs_CountPartsUsingSpec | Database/StoredProcedures/Dunnage/sp_Dunnage_Specs_CountPartsUsingSpec.sql |
| Dao_DunnageSpec | sp_Dunnage_Specs_GetAllKeys | Database/StoredProcedures/Dunnage/sp_Dunnage_Specs_GetAllKeys.sql |

### Custom Fields

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageCustomField | sp_Dunnage_CustomFields_Insert | Database/StoredProcedures/Dunnage/sp_Dunnage_CustomFields_Insert.sql |
| Dao_DunnageCustomField | sp_Dunnage_CustomFields_GetByType | Database/StoredProcedures/Dunnage/sp_Dunnage_CustomFields_GetByType.sql |
| Dao_DunnageCustomField | sp_custom_fields_delete | (missing in repo) |

### Inventoried Dunnage

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_InventoriedDunnage | sp_Dunnage_Inventory_GetAll | Database/StoredProcedures/Dunnage/sp_Dunnage_Inventory_GetAll.sql |
| Dao_InventoriedDunnage | sp_Dunnage_Inventory_Check | Database/StoredProcedures/Dunnage/sp_Dunnage_Inventory_Check.sql |
| Dao_InventoriedDunnage | sp_Dunnage_Inventory_GetByPart | Database/StoredProcedures/Dunnage/sp_Dunnage_Inventory_GetByPart.sql |
| Dao_InventoriedDunnage | sp_Dunnage_Inventory_Insert | Database/StoredProcedures/Dunnage/sp_Dunnage_Inventory_Insert.sql |
| Dao_InventoriedDunnage | sp_Dunnage_Inventory_Update | Database/StoredProcedures/Dunnage/sp_Dunnage_Inventory_Update.sql |
| Dao_InventoriedDunnage | sp_Dunnage_Inventory_Delete | Database/StoredProcedures/Dunnage/sp_Dunnage_Inventory_Delete.sql |

### User Preferences

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageUserPreference | sp_Dunnage_UserPreferences_Upsert | Database/StoredProcedures/Dunnage/sp_Dunnage_UserPreferences_Upsert.sql |
| Dao_DunnageUserPreference | sp_Dunnage_UserPreferences_GetRecentIcons | Database/StoredProcedures/Dunnage/sp_Dunnage_UserPreferences_GetRecentIcons.sql |

## Notes / Gaps

- `sp_custom_fields_delete` is referenced in `Dao_DunnageCustomField` but no corresponding SQL script was found under `Database/StoredProcedures/` at the time of analysis.
- Module_Dunnage itself does not perform Infor Visual (SQL Server) queries; it is MySQL + CSV focused.

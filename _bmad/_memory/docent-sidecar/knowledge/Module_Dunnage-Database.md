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
| Dao_DunnageLoad | sp_dunnage_history_get_all | Database/StoredProcedures/Dunnage/sp_dunnage_history_get_all.sql |
| Dao_DunnageLoad | sp_dunnage_history_get_by_date_range | Database/StoredProcedures/Dunnage/sp_dunnage_history_get_by_date_range.sql |
| Dao_DunnageLoad | sp_dunnage_history_get_by_id | Database/StoredProcedures/Dunnage/sp_dunnage_history_get_by_id.sql |
| Dao_DunnageLoad | sp_dunnage_history_insert | Database/StoredProcedures/Dunnage/sp_dunnage_history_insert.sql |
| Dao_DunnageLoad | sp_dunnage_history_insert_batch | Database/StoredProcedures/Dunnage/sp_dunnage_history_insert_batch.sql |
| Dao_DunnageLoad | sp_dunnage_history_update | Database/StoredProcedures/Dunnage/sp_dunnage_history_update.sql |
| Dao_DunnageLoad | sp_dunnage_history_delete | Database/StoredProcedures/Dunnage/sp_dunnage_history_delete.sql |

### Dunnage Types

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageType | sp_dunnage_types_get_all | Database/StoredProcedures/Dunnage/sp_dunnage_types_get_all.sql |
| Dao_DunnageType | sp_dunnage_types_get_by_id | Database/StoredProcedures/Dunnage/sp_dunnage_types_get_by_id.sql |
| Dao_DunnageType | sp_dunnage_types_insert | Database/StoredProcedures/Dunnage/sp_dunnage_types_insert.sql |
| Dao_DunnageType | sp_dunnage_types_update | Database/StoredProcedures/Dunnage/sp_dunnage_types_update.sql |
| Dao_DunnageType | sp_dunnage_types_delete | Database/StoredProcedures/Dunnage/sp_dunnage_types_delete.sql |
| Dao_DunnageType | sp_dunnage_types_count_parts | Database/StoredProcedures/Dunnage/sp_dunnage_types_count_parts.sql |
| Dao_DunnageType | sp_dunnage_types_count_transactions | Database/StoredProcedures/Dunnage/sp_dunnage_types_count_transactions.sql |
| Dao_DunnageType | sp_dunnage_types_check_duplicate | Database/StoredProcedures/Dunnage/sp_dunnage_types_check_duplicate.sql |

### Dunnage Parts

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnagePart | sp_dunnage_parts_get_all | Database/StoredProcedures/Dunnage/sp_dunnage_parts_get_all.sql |
| Dao_DunnagePart | sp_dunnage_parts_get_by_type | Database/StoredProcedures/Dunnage/sp_dunnage_parts_get_by_type.sql |
| Dao_DunnagePart | sp_dunnage_parts_get_by_id | Database/StoredProcedures/Dunnage/sp_dunnage_parts_get_by_id.sql |
| Dao_DunnagePart | sp_dunnage_parts_insert | Database/StoredProcedures/Dunnage/sp_dunnage_parts_insert.sql |
| Dao_DunnagePart | sp_dunnage_parts_update | Database/StoredProcedures/Dunnage/sp_dunnage_parts_update.sql |
| Dao_DunnagePart | sp_dunnage_parts_delete | Database/StoredProcedures/Dunnage/sp_dunnage_parts_delete.sql |
| Dao_DunnagePart | sp_dunnage_parts_count_transactions | Database/StoredProcedures/Dunnage/sp_dunnage_parts_count_transactions.sql |
| Dao_DunnagePart | sp_dunnage_parts_search | Database/StoredProcedures/Dunnage/sp_dunnage_parts_search.sql |

### Dunnage Specs

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageSpec | sp_dunnage_specs_get_by_type | Database/StoredProcedures/Dunnage/sp_dunnage_specs_get_by_type.sql |
| Dao_DunnageSpec | sp_dunnage_specs_get_all | Database/StoredProcedures/Dunnage/sp_dunnage_specs_get_all.sql |
| Dao_DunnageSpec | sp_dunnage_specs_get_by_id | Database/StoredProcedures/Dunnage/sp_dunnage_specs_get_by_id.sql |
| Dao_DunnageSpec | sp_dunnage_specs_insert | Database/StoredProcedures/Dunnage/sp_dunnage_specs_insert.sql |
| Dao_DunnageSpec | sp_dunnage_specs_update | Database/StoredProcedures/Dunnage/sp_dunnage_specs_update.sql |
| Dao_DunnageSpec | sp_dunnage_specs_delete_by_id | Database/StoredProcedures/Dunnage/sp_dunnage_specs_delete_by_id.sql |
| Dao_DunnageSpec | sp_dunnage_specs_delete_by_type | Database/StoredProcedures/Dunnage/sp_dunnage_specs_delete_by_type.sql |
| Dao_DunnageSpec | sp_dunnage_specs_count_parts_using_spec | Database/StoredProcedures/Dunnage/sp_dunnage_specs_count_parts_using_spec.sql |
| Dao_DunnageSpec | sp_dunnage_specs_get_all_keys | Database/StoredProcedures/Dunnage/sp_dunnage_specs_get_all_keys.sql |

### Custom Fields

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageCustomField | sp_custom_fields_insert | Database/StoredProcedures/Dunnage/sp_custom_fields_insert.sql |
| Dao_DunnageCustomField | sp_dunnage_custom_fields_get_by_type | Database/StoredProcedures/Dunnage/sp_dunnage_custom_fields_get_by_type.sql |
| Dao_DunnageCustomField | sp_custom_fields_delete | (missing in repo) |

### Inventoried Dunnage

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_InventoriedDunnage | sp_dunnage_requires_inventory_get_all | Database/StoredProcedures/Dunnage/sp_dunnage_requires_inventory_get_all.sql |
| Dao_InventoriedDunnage | sp_dunnage_requires_inventory_check | Database/StoredProcedures/Dunnage/sp_dunnage_requires_inventory_check.sql |
| Dao_InventoriedDunnage | sp_dunnage_requires_inventory_get_by_part | Database/StoredProcedures/Dunnage/sp_dunnage_requires_inventory_get_by_part.sql |
| Dao_InventoriedDunnage | sp_dunnage_requires_inventory_insert | Database/StoredProcedures/Dunnage/sp_dunnage_requires_inventory_insert.sql |
| Dao_InventoriedDunnage | sp_dunnage_requires_inventory_update | Database/StoredProcedures/Dunnage/sp_dunnage_requires_inventory_update.sql |
| Dao_InventoriedDunnage | sp_dunnage_requires_inventory_delete | Database/StoredProcedures/Dunnage/sp_dunnage_requires_inventory_delete.sql |

### User Preferences

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_DunnageUserPreference | sp_user_preferences_upsert | Database/StoredProcedures/Dunnage/sp_user_preferences_upsert.sql |
| Dao_DunnageUserPreference | sp_user_preferences_get_recent_icons | Database/StoredProcedures/Dunnage/sp_user_preferences_get_recent_icons.sql |

## Notes / Gaps

- `sp_custom_fields_delete` is referenced in `Dao_DunnageCustomField` but no corresponding SQL script was found under `Database/StoredProcedures/` at the time of analysis.
- Module_Dunnage itself does not perform Infor Visual (SQL Server) queries; it is MySQL + CSV focused.

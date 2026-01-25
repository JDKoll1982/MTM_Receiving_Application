# Database Table Name Mapping Reference

This file documents the mapping between old table names (from Wizard schema files) and new standardized table names (from individual schema files).

Generated: 2026-01-11

## Table Name Changes

| Module | Old Table Name | New Table Name | Notes |
|--------|---------------|----------------|-------|
| **Authentication** | | | |
| Auth | `departments` | `departments` | No change |
| Auth | `users` | `auth_users` | Renamed for clarity |
| Auth | `workstation_config` | `auth_workstation_config` | Renamed for clarity |
| Auth | `user_activity_log` | `settings_personal_activity_log` | Moved to settings module |
| **Receiving** | | | |
| Receiving | `label_table_receiving` | `receiving_label_data` | Standardized naming |
| Receiving | `label_table_parcel` | `receiving_label_data` | Wizard into receiving_label_data |
| Receiving | `receiving_loads` | `receiving_history` | Renamed for clarity |
| Receiving | `package_type_preferences` | `receiving_package_type_mapping` | Renamed for clarity |
| Receiving | N/A | `receiving_package_types` | New table (from settings) |
| **Dunnage** | | | |
| Dunnage | `dunnage_types` | `dunnage_types` | No change |
| Dunnage | `dunnage_specs` | `dunnage_specs` | No change |
| Dunnage | `dunnage_parts` | `dunnage_parts` | No change |
| Dunnage | `dunnage_loads` | `dunnage_history` | Renamed for clarity |
| Dunnage | `inventoried_dunnage` | `dunnage_requires_inventory` | Renamed for clarity |
| Dunnage | `custom_field_definitions` | `dunnage_custom_fields` | Renamed for clarity |
| Dunnage | `user_preferences` | `settings_dunnage_personal` | Moved to settings module |
| **Routing** | | | |
| Routing | `routing_recipients` | `routing_recipients` | No change |
| Routing | `routing_other_reasons` | `routing_po_alternatives` | Renamed for clarity |
| Routing | `routing_labels` | `routing_label_data` | Standardized naming |
| Routing | `routing_usage_tracking` | `routing_recipient_tracker` | Renamed for clarity |
| Routing | `routing_user_preferences` | `settings_routing_personal` | Moved to settings module |
| Routing | `routing_label_history` | `routing_history` | Renamed for clarity |
| Routing | N/A | `routing_home_locations` | New table |
| **Volvo** | | | |
| Volvo | `volvo_parts_master` | `volvo_masterdata` | Renamed for clarity |
| Volvo | `volvo_shipments` | `volvo_label_data` | Standardized naming |
| Volvo | `volvo_shipment_lines` | `volvo_line_data` | Standardized naming |
| Volvo | `volvo_part_components` | `volvo_part_components` | No change |
| Volvo | `volvo_settings` | `settings_module_volvo` | Moved to settings module |
| **Settings** | | | |
| Settings | `system_settings` | `settings_universal` | Renamed for clarity |
| Settings | `user_settings` | `settings_personal` | Renamed for clarity |
| Settings | `settings_audit_log` | `settings_activity` | Renamed for clarity |
| Settings | `package_type_mappings` | `receiving_package_type_mapping` | Moved to receiving module |
| Settings | `package_types` | `receiving_package_types` | Moved to receiving module |
| Settings | `routing_rules` | `routing_home_locations` | Moved to routing module |
| Settings | `scheduled_reports` | `reporting_scheduled_reports` | Moved to reporting module |
| **Reporting** | | | |
| Reporting | N/A | `reporting_scheduled_reports` | New table (from settings) |

## View Name Changes

| Old View Name | New View Name | Notes |
|--------------|---------------|-------|
| `vw_receiving_history` | `view_receiving_history` | Standardized prefix |
| `vw_dunnage_history` | `view_dunnage_history` | Standardized prefix |
| `vw_routing_history` | `view_routing_history` | Standardized prefix |
| `vw_volvo_history` | `view_volvo_history` | Standardized prefix |
| `vw_volvo_shipments_history` | `view_volvo_label_data_history` | Standardized prefix and naming |

## Summary of Naming Conventions

### Prefixes

- **Old Convention**: Mixed (`label_table_`, `vw_`, no prefix)
- **New Convention**: Consistent module-based naming

### Table Naming Pattern

- **Format**: `{module}_{purpose}` or `{module}_{entity}_{qualifier}`
- **Examples**:
  - `auth_users` (module: auth, entity: users)
  - `receiving_label_data` (module: receiving, purpose: label data)
  - `settings_routing_personal` (module: settings, scope: routing, level: personal)

### View Naming Pattern

- **Old**: `vw_{module}_{purpose}`
- **New**: `view_{module}_{purpose}`

### Key Changes

1. **Module-first naming**: All tables now start with their module name
2. **Consistent suffixes**:
   - `_data` for transactional tables
   - `_history` for archival/transaction log tables
   - `_personal` for user-specific settings
   - `_universal` for system-wide settings
3. **Removed prefixes**: Eliminated `label_table_` prefix
4. **Standardized views**: All views use `view_` prefix instead of `vw_`

## Migration Notes

⚠️ **Important**: Any stored procedures, application code, or queries referencing the old table names must be updated to use the new names.

### Files to Check

- `Database/StoredProcedures/**/*.sql`
- `Module_*/Data/Dao_*.cs`
- `Module_*/Models/Model_*.cs`
- Any direct SQL queries in the application

### Verification Query

```sql
-- Check for references to old table names in stored procedures
SELECT ROUTINE_NAME, ROUTINE_DEFINITION
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'mtm_receiving_application'
AND (
    ROUTINE_DEFINITION LIKE '%label_table_%' OR
    ROUTINE_DEFINITION LIKE '%vw_%' OR
    ROUTINE_DEFINITION LIKE '%users%' AND ROUTINE_DEFINITION NOT LIKE '%auth_users%'
);
```

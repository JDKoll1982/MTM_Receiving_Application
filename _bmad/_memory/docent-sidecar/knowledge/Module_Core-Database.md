---
module_name: Module_Core
section: 5
title: Database Schema Details
generated: 2026-01-08
scope: "DB mapping for Module_Core; companion to Module_Core.md"
---

## Module_Core - Database Schema Details

This file contains the detail DB mapping for Section 5 of the AM workflow.

## Databases in scope

### MySQL (mtm_receiving_application)

- Intended pattern: stored-procedure-only access via helper execution wrappers.
- Module_Core participates primarily in **Authentication** and some orchestration services that call DAOs.

### SQL Server (Infor Visual: Server=VISUAL, Database=MTMFG)

- Read-only query access via query files under `Database/InforVisualScripts/Queries/` loaded by `Helper_SqlQueryLoader`.

## MySQL: Authentication Stored Procedures

These stored procedures are used by `Dao_User` (auth / session) and `Service_Authentication`.

- `sp_GetUserByWindowsUsername(p_windows_username)`
  - Table: `users`
  - Returns: user record fields including `employee_number`, `windows_username`, `full_name`, `pin`, `department`, `shift`, `default_receiving_mode`, `default_dunnage_mode`, etc.

- `sp_ValidateUserPin(p_username, p_pin)`
  - Table: `users`
  - Accepts either `windows_username` or `full_name` as the username selector.

- `sp_CreateNewUser(p_employee_number, p_windows_username, p_full_name, p_pin, p_department, p_shift, p_created_by, p_visual_username, p_visual_password, OUT p_error_message)`
  - Table: `users`
  - Performs validation and inserts a new user within a transaction.

- `sp_LogUserActivity(p_event_type, p_username, p_workstation_name, p_details)`
  - Table: `user_activity_log`
  - Inserts audit entries and returns `ROW_COUNT()`.

- `sp_GetSharedTerminalNames()`
  - Purpose: enumerates shared terminal workstation names (used by `DetectWorkstationTypeAsync`).

- `sp_GetDepartments()`
  - Purpose: provides department list for UI dropdowns.

## MySQL: Receiving Stored Procedures (used by Core-facing services)

- `sp_InsertReceivingLoad`
- `sp_UpdateReceivingLoad`
- `sp_DeleteReceivingLoad`
- `sp_GetAllReceivingLoads`
- `sp_GetReceivingHistory`

Package preferences:

- `sp_GetPackageTypePreference`
- `sp_SavePackageTypePreference`
- `sp_DeletePackageTypePreference`

Receiving label insert:

- `receiving_line_Insert.sql` (stored procedure script name; implementation used via `IService_MySQL_ReceivingLine`)

## MySQL: Dunnage Stored Procedures (orchestrated by Service_MySQL_Dunnage)

Representative set (see `Database/StoredProcedures/Dunnage/`):

- Types: `sp_dunnage_types_get_all`, `sp_dunnage_types_insert`, `sp_dunnage_types_update`, `sp_dunnage_types_delete`, `sp_dunnage_types_check_duplicate`
- Specs: `sp_dunnage_specs_get_by_type`, `sp_dunnage_specs_insert`, `sp_dunnage_specs_update`, `sp_dunnage_specs_delete_by_id`, `sp_dunnage_specs_delete_by_type`
- Parts: `sp_dunnage_parts_get_all`, `sp_dunnage_parts_get_by_id`, `sp_dunnage_parts_get_by_type`, `sp_dunnage_parts_insert`, `sp_dunnage_parts_update`, `sp_dunnage_parts_delete`, `sp_dunnage_parts_search`
- Loads: `sp_dunnage_loads_insert`, `sp_dunnage_loads_insert_batch`, `sp_dunnage_loads_get_all`, `sp_dunnage_loads_get_by_id`, `sp_dunnage_loads_get_by_date_range`, `sp_dunnage_loads_update`, `sp_dunnage_loads_delete`
- Inventory: `sp_inventoried_dunnage_check`, `sp_inventoried_dunnage_get_by_part`, `sp_inventoried_dunnage_get_all`, `sp_inventoried_dunnage_insert`, `sp_inventoried_dunnage_update`, `sp_inventoried_dunnage_delete`
- Custom fields: `sp_custom_fields_get_by_type`, `sp_custom_fields_insert`
- User prefs: `sp_user_preferences_upsert`, `sp_user_preferences_get_recent_icons`

## SQL Server (Infor Visual): Query Files

Query files are executed via `Dao_InforVisualConnection`/`Dao_InforVisualPO`/`Dao_InforVisualPart`.

- `01_GetPOWithParts.sql`
  - Primary tables: `dbo.PURCHASE_ORDER`, `dbo.PURC_ORDER_LINE`, `dbo.PART`, `dbo.VENDOR`
  - Inputs: `@PoNumber`
  - Outputs: PoNumber, PoLine, PartNumber, quantities, vendor, status, site

- `02_ValidatePONumber.sql`
  - Table: `dbo.PURCHASE_ORDER`
  - Input: `@PoNumber`
  - Output: COUNT

- `03_GetPartByNumber.sql`
  - Tables: `dbo.PART`, `dbo.PART_SITE`
  - Input: `@PartNumber`

- `04_SearchPartsByDescription.sql`
  - Tables: `dbo.PART`, `dbo.PART_SITE`
  - Inputs: `@SearchTerm`, `@MaxResults`

## DB Constraints and Risks

- **Infor Visual site filter:** query files include commented site filters (e.g., `-- AND po.SITE_ID = '002'`). If the application depends on site 002-only behavior, enforce the filter.
- **MySQL stored-procedure-only constraint:** auth stored procedures exist for uniqueness checks, but `Dao_User` still performs some uniqueness checks via raw SQL (risk of standards drift).

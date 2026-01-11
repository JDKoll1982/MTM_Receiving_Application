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

- `sp_Auth_User_GetByWindowsUsername(p_windows_username)`
  - Table: `users`
  - Returns: user record fields including `employee_number`, `windows_username`, `full_name`, `pin`, `department`, `shift`, `default_receiving_mode`, `default_dunnage_mode`, etc.

- `sp_Auth_User_ValidatePin(p_username, p_pin)`
  - Table: `users`
  - Accepts either `windows_username` or `full_name` as the username selector.

- `sp_Auth_User_Create(p_employee_number, p_windows_username, p_full_name, p_pin, p_department, p_shift, p_created_by, p_visual_username, p_visual_password, OUT p_error_message)`
  - Table: `users`
  - Performs validation and inserts a new user within a transaction.

- `sp_Auth_Activity_Log(p_event_type, p_username, p_workstation_name, p_details)`
  - Table: `settings_personal_activity_log`
  - Inserts audit entries and returns `ROW_COUNT()`.

- `sp_Auth_Terminal_GetShared()`
  - Purpose: enumerates shared terminal workstation names (used by `DetectWorkstationTypeAsync`).

- `sp_Auth_Department_GetAll()`
  - Purpose: provides department list for UI dropdowns.

## MySQL: Receiving Stored Procedures (used by Core-facing services)

- `sp_Receiving_Load_Insert`
- `sp_Receiving_Load_Update`
- `sp_Receiving_Load_Delete`
- `sp_Receiving_Load_GetAll`
- `sp_Receiving_History_Get`

Package preferences:

- `sp_Receiving_PackageTypePreference_Get`
- `sp_Receiving_PackageTypePreference_Save`
- `sp_Receiving_PackageTypePreference_Delete`

Receiving label insert:

- `sp_Receiving_Line_Insert.sql` (stored procedure script name; implementation used via `IService_MySQL_ReceivingLine`)

## MySQL: Dunnage Stored Procedures (orchestrated by Service_MySQL_Dunnage)

Representative set (see `Database/StoredProcedures/Dunnage/`):

- Types: `sp_Dunnage_Types_GetAll`, `sp_dunnage_types_insert`, `sp_dunnage_types_update`, `sp_dunnage_types_delete`, `sp_Dunnage_Types_CheckDuplicate`
- Specs: `sp_Dunnage_Specs_GetByType`, `sp_dunnage_specs_insert`, `sp_dunnage_specs_update`, `sp_Dunnage_Specs_DeleteById`, `sp_Dunnage_Specs_DeleteByType`
- Parts: `sp_Dunnage_Parts_GetAll`, `sp_Dunnage_Parts_GetById`, `sp_Dunnage_Parts_GetByType`, `sp_dunnage_parts_insert`, `sp_dunnage_parts_update`, `sp_dunnage_parts_delete`, `sp_dunnage_parts_search`
- Loads: `sp_Dunnage_Loads_Insert`, `sp_Dunnage_Loads_InsertBatch`, `sp_Dunnage_Loads_GetAll`, `sp_Dunnage_Loads_GetById`, `sp_Dunnage_Loads_GetByDateRange`, `sp_Dunnage_Loads_Update`, `sp_Dunnage_Loads_Delete`
- Inventory: `sp_Dunnage_Inventory_Check`, `sp_Dunnage_Inventory_GetByPart`, `sp_Dunnage_Inventory_GetAll`, `sp_Dunnage_Inventory_Insert`, `sp_Dunnage_Inventory_Update`, `sp_Dunnage_Inventory_Delete`
- Custom fields: `sp_Dunnage_CustomFields_GetByType`, `sp_Dunnage_CustomFields_Insert`
- User prefs: `sp_Dunnage_UserPreferences_Upsert`, `sp_Dunnage_UserPreferences_GetRecentIcons`

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

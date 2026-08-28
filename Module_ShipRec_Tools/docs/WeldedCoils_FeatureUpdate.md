# Welded Coils Tool — Feature Update

- **Last Updated:** 2026-08-28
- **Module:** Ship/Rec Tools (`Module_ShipRec_Tools`)
- **Table:** `settings_weldedcoils` (MySQL, `mtm_receiving_application` schema)

## Overview

The Welded Coils tool maintains the list of coils that our material handlers need to have
the inner diameter welded before the coil goes on the press cradle. Previously this table
had no stored procedures and no app-side access. This feature adds a simple CRUD tool to
the Ship/Rec Tools module so the list can be maintained without touching the database
directly.

## Table Schema

The live table (verified 2026-08-28) has three columns:

| Column    | Type          | Notes                                      |
| --------- | ------------- | ------------------------------------------ |
| `id`      | INT(11) PK AI | Auto-incrementing unique identifier        |
| `partid`  | VARCHAR(11)   | Part number (NOT NULL)                     |
| `isActive`| TINYINT(1)    | 1 = requires welding, 0 = inactive (default 1) |

The live table has no unique key on `partid`; duplicate part numbers are blocked at the
stored-procedure layer instead of changing the existing schema.

## Stored Procedures

New procedures live under
`Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/`:

| Procedure                                | Purpose                                  |
| ---------------------------------------- | ---------------------------------------- |
| `sp_Settings_WeldedCoils_GetAll`         | List all coils (active first, then part) |
| `sp_Settings_WeldedCoils_Insert`         | Add a part (defaults active)             |
| `sp_Settings_WeldedCoils_Update`         | Rename a part on an existing row         |
| `sp_Settings_WeldedCoils_SetActive`      | Flip a row active / inactive             |
| `sp_Settings_WeldedCoils_Delete`         | Remove a row                             |

All writes go through these procedures; there is no raw MySQL SQL in C#.

## App Layer

New files in `Module_ShipRec_Tools`:

- `Models/Model_Tool_WeldedCoil.cs` — observable row model (`Id`, `PartId`, `IsActive`)
  with computed `ToggleLabel` ("Activate"/"Deactivate") and `StatusLabel` ("Active"/"Inactive").
- `Data/Dao_Tool_WeldedCoil.cs` — DAO calling the stored procedures (instance-based,
  injected connection string, returns `Model_Dao_Result`).
- `Contracts/IService_Tool_WeldedCoils.cs` / `Services/Service_Tool_WeldedCoils.cs` —
  service facade: normalizes part numbers, validates against the Infor Visual part
  master (read-only), enforces the 11-character limit, and logs/normalizes errors.
- `ViewModels/ViewModel_Tool_WeldedCoils.cs` — add / search / active-inactive toggle /
  delete-selected logic with re-entrancy and row-identity guards.
- `Views/View_Tool_WeldedCoils.xaml` (+ code-behind) — tool page: search bar, Add and
  Delete Selected buttons, and a read-only ListView (Part, Status, Activate/Deactivate
  button per row).

Wiring:

- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` — registers the DAO,
  service, ViewModel, and View; threads the MySQL connection string into the ShipRec
  module registration and injects `IService_InforVisual` into the service.
- `Services/Service_ShipRecTools_Navigation.cs` — registers the "Welded Coils" tool card.
- `ViewModels/ViewModel_ShipRecTools_Main.cs` and `Views/View_ShipRecTools_Main.*` — host
  the new tool page alongside the other tools.

## How To Use

1. Open **Ship/Rec Tools** → select **Welded Coils**.
2. Use the **search** box at the top to filter the list by part number.
3. Type a part number in the **Part #** box and click **Add Coil** (or press Enter).
4. Click a row to select it, then click **Delete Selected** (next to Add) to remove it.
5. Use each row's **Activate / Deactivate** button to flip whether the coil still needs
   welding (the button label and Status column update to match).

Rows are read-only after they are added — there are no editable text boxes in the list.
Part numbers are stored trimmed and uppercase, and are validated against the Infor Visual
part master before add — a part that does not exist in the ERP is rejected with a clear
message. Part numbers over 11 characters are rejected. Adding a part number that already
exists is blocked (in the ViewModel and at the stored-procedure layer).

## Known Data Condition

At first deploy the test table already contained 32 rows where every part number appeared
twice (e.g. `MMC0000262` at ids 3 and 18). The tool lists all rows as-is (so users can
remove the extras) and blocks new duplicates; it does not auto-dedupe existing data.

## Deployment

The schema and procedure files are the source of truth. To enable the tool against a
new database, run the following files in order (test first, then production):

1. `Database/Database_Deployment/Sql_Files/Schemas/49_Table_settings_weldedcoils.sql`
   (idempotent `CREATE TABLE IF NOT EXISTS`; uses `utf8mb4_unicode_ci` so it deploys on
   MySQL 5.7).
2. The five `sp_Settings_WeldedCoils_*` files under
   `Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/`.

**Status (2026-08-28):** deployed and verified on `mtm_receiving_application_test` at
`172.16.1.104:3306` — table created, all five procedures present, and insert / duplicate
rejection / update / set-active / delete verified end-to-end. The live `mtm_receiving_application`
database still needs the same deployment before the tool is used in production.

## Tests

- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_WeldedCoilsTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_WeldedCoilsTests.cs`

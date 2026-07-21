# Receiving Scanner Feature Database Requirements

Last Updated: 2026-07-21

This folder defines the database requirements for the Receiving Scanner feature.

## MySQL And ERP Guardrails

- Target database engine: MySQL 5.7.
- All application writes must go through stored procedures.
- Keep DAO result-model patterns aligned with existing Receiving module conventions.
- Infor Visual SQL Server remains read-only and must not be modified by this feature.
- Target scanner workflow currently points at `VMINVENT.exe` with an `Inventory Transfers` child screen in the Gupta/Centura client.
- Profile persistence should carry both from and to warehouse default variables, currently defaulting to `002`.

## UI Mockup Alignment Inputs

Database behavior in this folder supports the UI flows represented by:

- docs/features/receiving/scanner-feature/mockups/01-scanner-workbench.png
- docs/features/receiving/scanner-feature/mockups/02-scanner-history.png
- docs/features/receiving/scanner-feature/mockups/03-scanner-settings.png
- docs/features/receiving/scanner-feature/mockups/04-modal-manage-batch-rows.png
- docs/features/receiving/scanner-feature/mockups/05-modal-save-draft.png
- docs/features/receiving/scanner-feature/mockups/06-modal-load-draft.png
- docs/features/receiving/scanner-feature/mockups/07-modal-configure-hotkeys.png

## Structure

- `Schemas/`: Table requirements (one file per table).
- `StoredProcedures/Receiving/`: Stored procedure requirements (one file per logic unit).
- `Functions/`: SQL function requirements (one file per function).
- `Views/`: Read-model requirements (one file per view).
- `Triggers/`: Trigger requirements (one file per trigger).
- `Migrations/`: Migration entry points.

## Naming And Pattern Alignment

This pack follows the existing repository patterns in `Database/Database_Deployment/Sql_Files`:

- `USE mtm_receiving_application;`
- `DROP ... IF EXISTS` before recreate
- `DELIMITER $$` for procedures/functions/triggers
- snake_case table names and `sp_Receiving_*` routine naming

## Recommended Execution Order

1. Run `Schemas/*.sql` in numeric order.
2. Run `Functions/*.sql`.
3. Run `Views/*.sql`.
4. Run `Triggers/*.sql`.
5. Run `StoredProcedures/Receiving/*.sql`.
6. Optionally run `Migrations/*.sql` for environment bootstrap.

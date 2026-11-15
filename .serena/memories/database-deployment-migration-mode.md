<!-- 
[DOC-META-START]
- File Name: database-deployment-migration-mode.md
- Description: Notes on the idempotent MySQL object deployment model, the WinUI deployment tool, and the retired migration layer.
- Last Updated: 2026-11-15
- Quick TOC:
  - Line 18-25: # Database Deployment (Idempotent Object Model)
  - Line 26-33: ## Supported tools
  - Line 34-46: ## Object tree contract
  - Line 47-56: ## Seed idempotency
  - Line 57-65: ## Server selection
  - Line 66-74: ## Known constraints
- Critical Notes: Migrations are archived; every SQL object is re-runnable and no script may contain USE, DROP TABLE, or non-idempotent DDL.
[DOC-META-END]
-->

# Database Deployment (Idempotent Object Model)

- Deployment no longer uses a migration layer. Every object under `Database/Database_Deployment/Sql_Files` is written so it can be re-applied to a live database without data loss.
- The migration folder was retired to `Sql_Files/_Archive/Migrations`. Archived scripts are never deployed and are excluded by both tools.
- Migration-only columns, indexes, and constraints were folded into the owning table scripts (`dunnage_label_data.is_reprint`, `receiving_label_data.part_skid_sequence` / `total`, `volvo_generated_label_data.is_reprint`).
- `dunnage_specs` is retired with no schema file; the seed reference to it was removed.

## Supported tools

- `MTM_Database_Deployment/` (WinUI 3, .NET 10) is the supported deployment app. It builds a preview from a scratch schema, then applies an update in place or a confirmed fresh rebuild.
- `Database/Database_Deployment/Deploy-Database-GUI-Workbench.ps1` is legacy. It deploys the same tree for scripted/no-UI runs and is kept for reference only.
- Module spec for porting the app to another repo: `docs/database-deployment-module-spec.md`.

## Object tree contract

- `Schemas/` - `CREATE TABLE IF NOT EXISTS` only. No `DROP TABLE`, no `TRUNCATE`, no data-changing `ALTER`.
- `Views/` - `CREATE OR REPLACE VIEW` only.
- `StoredProcedures/`, `Triggers/` - `DROP ... IF EXISTS` then `CREATE`.
- `SeedData/` - re-runnable; second run inserts zero rows.
- No script in the tree contains `USE <database>;`. The target database is always a parameter, so the same tree deploys to dev, test, or production.
- Views live in `Views/`, never in `Schemas/`, so a view can never reference a column that a later-deployed object introduces.

## Seed idempotency

- `03_seed_dunnage_data.sql` resolves parent ids from natural keys (`type_name`, `FieldName`) immediately before use. Do not trust the insert procedures' `OUT` id: on a repeat run they skip the insert and leave the variable at its previous value, which silently attaches child rows to the wrong parent.
- The `dunnage_requires_inventory` block is a set-based `INSERT ... SELECT ... WHERE NOT EXISTS` because `sp_Dunnage_Inventory_Insert` has no duplicate guard and would double the rows on every run.
- `dunnage_types` and `dunnage_parts` still raise benign duplicate errors on a repeat run (the insert procedures guard with `SIGNAL` / unique keys). The deployment engine classifies those as already applied and aggregates them.

## Server selection

- Both tools probe the preferred server (`172.16.1.104`) with a 2 second TCP check, then fall back to `127.0.0.1`.
- If neither answers the run stops immediately with a message naming both endpoints.
- Connection timeouts are 3-5 seconds so a lost server fails fast instead of blocking for ~21 seconds per script.

## Known constraints

- MySQL only. The Infor Visual SQL Server database stays read-only and is never touched by deployment tooling.
- Column renames are seen as a drop plus an add, so a rename must be deliberate and confirmed.
- Table options (engine, charset, comments) are not compared; only columns, indexes, and foreign keys are.
- Verified 2026-11-15 with the deployment harness: 12 of 12 checks pass - empty-DB preview, first apply (51 tables / 5 views / 232 routines / 2 triggers), converged second preview with zero table work, data preservation, stale-column restore, extra-column removal, and fresh rebuild.
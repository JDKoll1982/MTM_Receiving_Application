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

## Generated DDL gotchas

- `information_schema.COLUMNS.COLUMN_DEFAULT` returns a string default as the bare literal. Emitting it raw produces `DEFAULT Quantity`, which MySQL rejects with a syntax error near the value. `Model_ColumnDefinition.ToColumnDefinitionSql` quotes character-type defaults, leaves numeric ones alone, and leaves expressions (`CURRENT_TIMESTAMP`, anything with `(`, `b'0'`, `0x..`) and `EXTRA=DEFAULT_GENERATED` unquoted.
- A database-only `NOT NULL` column with no default silently breaks every insert that does not name it. Inside a procedure with an `EXIT HANDLER FOR SQLEXCEPTION` the insert fails without surfacing an error, so the symptom is empty tables and confusing downstream `Column 'X' cannot be null` failures rather than the real cause. The comparer therefore flags such a column as `IsBlocking`, the plan summary names it, and a confirmed removal runs it before the seeds.
- Live-DB symptom seen 2026-11-15 on `mtm_receiving_application`: `dunnage_custom_fields.DatabaseColumnName varchar(64) NOT NULL` (no default, not in the file tree) left all 38 custom fields uninserted, which then failed all 20 choice inserts. After the fixes the same deploy reports one aggregated failure instead of 99, and converges in one pass once removal is confirmed.
- If a schema change fails, the engine stops before views, procedures, triggers, and seeds. Continuing previously produced a cascade (3 bad `ALTER`s produced 99 reported failures).
## Operator UI

- `MTM_Database_Deployment` is a three-step wizard (Connect -> Check -> Save), not a control panel. Every label is plain language; SQL detail is folded behind a "Technical details - for IT" expander.
- Connect proves sign-in with a real `SELECT 1`, so "Connected" never lies: 1049 (unknown database) is reported separately from 1045 (bad credentials) and from an unreachable host.
- DevWinUI supplies `StepBar`, `InfoCard`, `SelectorBarSegmented`, and `ProgressButton`. `ms-appx:///DevWinUI/Themes/Generic.xaml` has to be merged in `App.xaml` because `StepBar` reads its templates from `Application.Current.Resources` in its constructor.
- Do not use a converter for visibility in this window: its binding root is a `Window`, which is not a framework element, so x:Bind cannot build a converter lookup root. Use `x:Bind ui:Show.If(flag)` instead.
- `ProgressButton` is a `ToggleButton` and does not refuse a second click, so `IsEnabled` must be bound to a `CanSave` flag.
- Verified 2026-11-15 by driving the running app: connect (fallback to 127.0.0.1), check (251 changes on the live DB, 279 on an empty DB), save against a scratch database (51 tables / 232 routines, "All done - the database is up to date"), and the destructive mode keeping the save button disabled until the confirmation box is ticked.- The target database is chosen from a radio list built from `AvailableDatabases` in `deployment.settings.json` (`mtm_receiving_application` and `mtm_receiving_application_test`). Changing it discards the plan and returns to step 1. `ResolveAvailableDatabases()` inserts the configured name if the list does not contain it, so a custom name is never silently swapped.
- Database sign-in lives in a person-icon flyout in the header: user name, password, `Check sign-in`, and the result. The button text doubles as the status, because clicking `Check sign-in` dismisses the flyout.
- `Model_DatabaseOption` must keep `ToString()` returning `Display`; without it the `RadioButtons` items are announced to a screen reader as the type name.
- Never leave a test database name or password in `MTM_Database_Deployment/bin/**/deployment.settings.json`. The app writes saved settings there and a build will not overwrite a newer copy, so a stale scratch name survives and the next preview targets a database that no longer exists.
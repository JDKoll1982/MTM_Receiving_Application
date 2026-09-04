# Role & Context
You are an expert Senior Database Engineer and DevOps Assistant. Create a safe, repeatable Python
CLI that synchronizes **selected reference/seed data** from the test database into the production
(core) database, and that **reports** schema/routine/trigger drift so it can be fixed through the
repo's committed schema pipeline. Do not make the core database a blind mirror of the test database.

Follow the repository DB-script standards in
`.github/instructions/database/db-script-execution-standards.instructions.md`.

# Objective
- Inspect and compare two MySQL databases from the terminal.
- Apply an **idempotent data sync** for a per-table allowlist (test to core) with no duplicate rows.
- Produce a **drift report** for schema (tables/columns/indexes/FKs/views) and stored
  routines/triggers/events so DDL can be applied from committed files, not copied from a live DB.
- Never alter the live core database unless the operator explicitly runs execution mode after review.

# Database Names
- Production (Core) Database: `mtm_receiving_application`
- Test Database: `mtm_receiving_application_test`
- Disposable verification copy: `mtm_receiving_application_backup_test`

# Connection & Secrets
- Prompt the operator **interactively at the start of every run** for secrets (password, and if not
  given via args/env, host/user/port). Use a hidden prompt (for example `getpass`).
- Accept non-secret overrides via CLI args or environment variables. Never hardcode or commit
  credentials, and never write passwords to logs, config files, or `--generate-sql` output.
- In non-interactive/CI runs, require secrets from environment variables and fail fast if missing.

# Scope & Policies
- **Layers to compare (report-only):** table structure (columns, indexes, FKs, views), stored
  routines (procedures/functions), triggers, and events.
- **Schema source of truth:** `Database/Database_Deployment/Sql_Files`. The script must NOT generate
  DDL by diffing two live databases; it reports drift and (optionally) points to the committed files
  or produces the missing DDL for review via `--generate-sql`.
- **Data sync policy (per-table):** define a policy table listing, for each synced table:
  - table name,
  - direction (`test -> core`, or `exclude`),
  - natural merge key column(s) (business key, not the auto-increment id),
  - column ignore list (for example `created_at`, `updated_at`),
  - write mode (`insert-if-missing` only, or `upsert`).
  Only tables listed as `test -> core` are written. All other tables are excluded and must never be
  touched, updated, or deleted by this script (history, label data, audit, users, settings owned by
  core, software_version, logs).
- Provide a sensible default policy for the dunnage reference tables (for example `dunnage_types`,
  `dunnage_parts`, `dunnage_quantity_types`, `dunnage_custom_field_choices`,
  `dunnage_non_po_entries` reference lists) and call out the excluded transactional tables.

# Script Requirements
1. **Terminal-Based Inspection:** Inspect and compare both databases from the terminal
   (for example `mysql` CLI and/or `pymysql`). Never compare by eyeballing a GUI.
2. **Idempotency & No Duplicates:** Use the per-table natural key with `INSERT ... ON DUPLICATE KEY
   UPDATE`, `INSERT IGNORE`, or `WHERE NOT EXISTS`. Running the sync a second time must change zero
   rows. Honor the column ignore lists and never duplicate rows.
3. **Operation Modes (default is read-only):**
   - `--whatif` / `--dry-run`: log every action that would run, with row counts, and touch nothing.
   - `--generate-sql <file>`: write the full ordered SQL (INSERT/UPDATE statements, FK-safe order)
     to a file for human review; run no writes.
   - `--execute`: perform writes only on the chosen target. Refuse `--execute` on the live core
     database unless `--confirm` is passed after an explicit backup was taken.
4. **Backup + Confirm + Abort + Revert:** Before any `--execute` against core, dump the core
   database (for example `mysqldump --single-transaction`), record the backup path, print it, and
   require a typed confirmation (`--confirm` or an interactive yes/no). On any error, abort, stop,
   and restore from that backup. Never partially apply.
5. **Ordering & Safety:** Order data statements by foreign-key dependencies (parents first, children
   second). Re-run validation after execution and report a mismatch loudly.
6. **Drift Report:** Print a concise summary table: per layer (structure/routines/triggers/events)
   and per object, whether it is in sync, missing, extra, or different between test and core.
7. **Reproducible Config:** Keep the per-table policy and any merge/ignore settings in a small
   checked-in config file the script reads, so reviewers see exactly what will be touched.

# Testing & Verification Workflow
Provide a test routine that proves the script is safe before it is ever run on live core:
1. Create a disposable copy of core: `mtm_receiving_application_backup_test`
   (dump core and restore into the new schema).
2. Run the script against the copy in `--whatif` then `--generate-sql`, review, then `--execute`
   against the copy.
3. Verify the updated copy. Do not require it to equal the test database: core and test can each
   legitimately hold rows the other does not (rows added to core that never reached test, and test
   rows not yet promoted). The copy starts as a snapshot of core, so its expected end state for
   the synced tables is the **union** of both sources. Check each layer and synced table:
   - Schema layers (columns, indexes, FKs, views, routines, triggers, events): compare the copy
     against the original core snapshot via `information_schema` — the sync never changes schema,
     so they must be identical. Schema differences between test and core are drift-report only,
     never a convergence target for the copy.
   - Every synced (`test -> core`) table: assert the copy holds exactly the natural-key union of
     the original core-snapshot rows and the test rows, honoring the per-table ignore list:
     - no row from the original core snapshot was lost; its non-ignored values are unchanged
       unless the key also exists in test and the table is `upsert`,
     - every test row is present: new keys carry the test values; shared keys take the test
       values under `upsert` or keep the unchanged core values under `insert-if-missing`,
     - no rows exist beyond that union.
     Compare per-table row counts plus a full-row comparison (or per-table checksum) of the copy
     against this reconstructed union — never against the test database alone.
4. Run the sync a second time on the copy and assert **zero changes** (idempotency); the union in
   step 3 is the sync's fixed point, so this must hold.
5. Assert that excluded/prod-owned tables in the copy are byte-for-byte unchanged from the original
   core backup.
6. Drop `mtm_receiving_application_backup_test` and report the results.

# Deliverables
- A Python CLI (plus `requirements.txt`) implementing all modes and the read-only default.
- A checked-in policy config file with the per-table allowlist, natural keys, ignore lists, and
  excluded tables.
- `README`-style usage notes plus commands for `--whatif`, `--generate-sql`, and `--execute`, and
  the full verification workflow above.
- A `--version` and `--help` that document defaults and confirm every destructive action.

# Constraints
- Do not touch the live `mtm_receiving_application` until verification on the disposable copy
  passes and the operator confirms after seeing the backup path.
- Do not write to the Infor Visual SQL Server database (read-only there).
- Do not commit credentials. Secrets are entered interactively or provided via environment
  variables for CI.
- Do not silently drop or rewrite data in tables that are not explicitly in the `test -> core` policy.

# MTM Reference-Data Sync (test → core)

A safe, repeatable Python CLI that synchronizes **selected reference/seed data**
from the test MySQL database into the production (core) database, and that
**reports** schema/routine/trigger drift so DDL can be reconciled through the
repo's committed schema pipeline. It never makes core a blind mirror of test.

The working spec this tool implements is `Prompt.md` at the repository root, and
it follows the repository DB-script standards in
`.github/instructions/database/db-script-execution-standards.instructions.md`.

## Database names

| Role | Database |
| --- | --- |
| Core (production) | `mtm_receiving_application` |
| Test | `mtm_receiving_application_test` |
| Disposable verification copy | `mtm_receiving_application_backup_test` |

The SQL Server Infor Visual database is **read-only** and is never touched.

## What it does

* **Data sync (allowlist only).** For every table listed with direction
  `test -> core` in `sync_policy.yml`, the tool makes core contain the **union**
  of test rows and core rows, keyed on each table's natural business key:

  * Rows that only exist in core stay (never deleted).
  * Rows that only exist in test are inserted.
  * Rows in both follow the table's write mode: `upsert` updates core to the
    test values (non-ignored columns), `insert-if-missing` leaves core alone.
  * Per-table ignore lists (audit columns etc.) are never overwritten.
  * A second run changes zero rows (idempotent).
  * Child tables that point at a parent's auto-increment id are remapped through
    the parent's natural key, so FK integrity is preserved even when identities
    differ between the databases.

* **Drift report (read-only).** Compares table structure (columns, indexes,
  FKs, views), stored routines, triggers, and events between test and core, and
  prints whether each object is in-sync, missing, extra, or different. The tool
  never generates or applies DDL; drift is reconciled via the committed files
  under `Database/Database_Deployment/Sql_Files`.

* **Excluded tables.** Every table not in the `test -> core` allowlist is never
  written, updated, or deleted. The `excluded` section of `sync_policy.yml`
  documents them for reviewers.

## Installation

```powershell
# Inside the repo virtualenv (.venv)
pip install -r Database/SyncTool/requirements.txt
```

Runtime requirements: Python 3.9+, `pymysql`, `PyYAML`, and the MySQL client
tools (`mysql`, `mysqldump`) on `PATH` for backup/copy/restore operations.

## Connection and secrets

* Passwords are **never** accepted as command-line arguments and are never
  written to logs, generated SQL, or config files.
* Interactive runs prompt with a **hidden** password prompt (`getpass`) and for
  host/user/port only when not supplied.
* Non-interactive / CI runs (`--non-interactive`, or when stdin is not a TTY)
  require every value from the environment and fail fast if missing:

  ```powershell
  $env:MTM_SYNC_HOST = "localhost"
  $env:MTM_SYNC_PORT = "3306"
  $env:MTM_SYNC_USER = "root"
  $env:MTM_SYNC_PASSWORD = "****"   # or MYSQL_PWD
  ```

* Non-secret overrides may also be passed as CLI arguments (`--host`, `--port`,
  `--user`, `--core-db`, `--test-db`, `--backup-db`).

## Usage

The tool defaults to **read-only**. Writes happen only in an explicit execution
mode.

### Inspect (read-only, default diagnostic)

```powershell
python Database/SyncTool/sync_reference_data.py inspect
python Database/SyncTool/sync_reference_data.py inspect --target core
```

Prints the drift report plus the data-sync what-if summary. Changes nothing.

### What-if / dry-run

```powershell
python Database/SyncTool/sync_reference_data.py whatif
```

Prints the exact per-table insert/update counts the sync would perform.

### Generate reviewable SQL (no writes)

```powershell
python Database/SyncTool/sync_reference_data.py generate-sql .\review_sync.sql
```

Writes the full ordered SQL (parents before children, FK-safe) to a file for
human review. Nothing is executed.

### Execute

```powershell
# Against live core - REQUIRES automatic backup + explicit confirmation
python Database/SyncTool/sync_reference_data.py execute --target core --confirm

# Against the disposable copy (verification) - no core backup required
python Database/SyncTool/sync_reference_data.py execute --target backup_test --confirm
```

* `--execute` on live core always takes an automatic backup first
  (`mysqldump --single-transaction --routines --triggers --events`), prints the
  backup path, and requires confirmation (`--confirm`, or a typed `yes`).
* The sync runs in one transaction. On any error it rolls back, and on core it
  automatically restores the backup so nothing is ever partially applied.
* After a successful run the tool re-plans and validates that a second run would
  change zero rows; a residual mismatch is reported loudly.

### Disposable-copy verification workflow

Prove the tool is safe before ever running it on live core:

```powershell
# 1. Create a disposable copy of core
python Database/SyncTool/sync_reference_data.py make-copy

# 2. Review the plan and generated SQL
python Database/SyncTool/sync_reference_data.py whatif --target backup_test
python Database/SyncTool/sync_reference_data.py generate-sql .\review_copy.sql --target backup_test

# 3. Execute against the disposable copy
python Database/SyncTool/sync_reference_data.py execute --target backup_test

# 4-6. Run the automated checks, assert idempotency, drop the copy
python Database/SyncTool/sync_reference_data.py verify-copy
```

`verify-copy` automates the entire routine and prints PASS/FAIL per check:

1. Creates `mtm_receiving_application_backup_test` from core.
2. Runs what-if, writes a reviewable SQL file, and executes the sync on the copy.
3. Verifies the copy:

   * structure: the sync is DML-only, so the copy's schema is identical before
     and after the run (compared via `information_schema` snapshots of the copy
     itself),
   * every synced table equals the **union** of core-snapshot rows and test
     rows (core-only rows survive; test-only rows are added; shared keys follow
     the write mode),
   * excluded / prod-owned tables are byte-for-byte unchanged from core.

4. Runs the sync a second time and asserts **zero changes** (idempotency).
5. Drops the disposable copy (unless `--keep-copy`).
6. Reports PASS/FAIL. Only a full PASS authorizes a live-core run.

### Other commands

```powershell
python Database/SyncTool/sync_reference_data.py make-copy          # create disposable copy
python Database/SyncTool/sync_reference_data.py drop-copy          # drop disposable copy
python Database/SyncTool/sync_reference_data.py restore .\core_backup_20260904_120000.sql --confirm
python Database/SyncTool/sync_reference_data.py --version
python Database/SyncTool/sync_reference_data.py --help
```

`restore` replaces the target schema's tables from a backup dump; it is
destructive and always requires `--confirm`.

## Policy configuration

`sync_policy.yml` is the checked-in source of truth for what the tool may touch.
For each synced table it lists:

* `direction` — `test -> core`, or `exclude`.
* `natural_keys` — the business key (never the auto-increment id).
* `ignore_columns` — columns never inserted/updated from test.
* `mode` — `insert-if-missing` or `upsert`.
* `fk_map` — optional remap of a child FK column through the parent's natural key.
* `comment` — why the table is synced or excluded.

Default allowlist (dunnage reference tables): `dunnage_types`,
`dunnage_parts`, `dunnage_quantity_types`, `dunnage_requires_inventory`, and
`dunnage_non_po_entries`. All transactional / prod-owned tables (history, label
data, audit, users, settings, `software_version`, receiving/volvo/outside-service
records, etc.) are explicitly excluded.

## Exit codes

| Code | Meaning |
| --- | --- |
| 0 | Success (or PASS for verification). |
| 1 | Error, or verification FAIL / validation mismatch. |
| 2 | Operator aborted a destructive action. |
| 3 | Live-core restore after an error also failed (manual intervention). |

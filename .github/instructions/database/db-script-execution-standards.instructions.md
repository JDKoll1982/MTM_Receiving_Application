---
applyTo: "**/*.sql, **/*.py, **/*.sh, **/*.ps1, **/*.md"
description: >
  Required conventions for any DB-related script or tool in this repo (data sync, migrations,
  comparisons, seed updates): interactive secrets, read-only defaults, what-if / generate-SQL
  modes, automatic backup, confirmation, abort-and-restore, idempotency, and verify-on-a-copy.
---

<!-- 
[DOC-META-START]
- File Name: db-script-execution-standards.instructions.md
- Description: Required conventions for any DB-related script or tool in this repo - secrets handling, read-only defaults, safety modes, backup/confirm/abort, idempotency, and copy-based verification.
- Last Updated: 2026-09-04
- Quick TOC:
  - Line 12-17: # DB Script Execution Standards
  - Line 18-21: ## Purpose
  - Line 22-38: ## Connections And Secrets
  - Line 39-48: ## Database Targets
  - Line 49-67: ## Mandatory Safety Workflow
  - Line 68-79: ## Idempotency And Deduplication
  - Line 80-89: ## Verification On A Copy
  - Line 90-94: ## Related Guidance
- Critical Notes: Never commit credentials. Default every DB script to read-only; require explicit confirmed execution for any write against core.
[DOC-META-END]
-->

# DB Script Execution Standards

## Purpose

Define the non-negotiable conventions every database-related script or tool in this repository must
follow, including data-sync utilities, migrations, seed refreshes, schema comparisons, and
maintenance tooling. The working spec that applies these rules to the test-to-core sync is
`Prompt.md` at the repository root.

## Connections And Secrets

- Prompt the operator **interactively at the start of every run** for secrets (password; host, user,
  and port when not supplied by arguments or environment).
- Use a hidden input prompt for passwords. Never echo secrets to the terminal or logs.
- Accept non-secret overrides (database names, host, port, modes) via CLI arguments or environment
  variables.
- **Never hardcode or commit credentials.** Do not write passwords into config files, generated SQL,
  logs, or source control.
- For non-interactive / CI execution, require secrets from environment variables and fail fast when
  they are missing. A script must not guess or use empty credentials.

## Database Targets

- MySQL application databases:
  - Core (production): `mtm_receiving_application`
  - Test: `mtm_receiving_application_test`
  - Disposable verification copy: `mtm_receiving_application_backup_test`
- Infor Visual (SQL Server) is **read-only** in this project. Never issue writes against it from any
  script.
- Prefer validating writes against the test schema first; keep the source-of-truth SQL under
  `Database/` reconciled with anything a script performs.

## Mandatory Safety Workflow

Every script that can modify a database must:

- Default to **read-only**; writes only happen in an explicit execution mode.
- Implement `--whatif` / `--dry-run` that logs intended actions and affected row counts and changes
  nothing.
- Implement `--generate-sql <file>` that writes the ordered, reviewable SQL without executing it.
- Take an automatic backup (for example `mysqldump --single-transaction`) before `--execute` against
  core, print the backup path, and require a typed confirmation before applying.
- Abort on the first error, stop, and restore from the backup taken at the start of the run. Never
  leave a partially applied change.
- Order statements by foreign-key dependencies (parents before children) and validate results after
  execution.

## Idempotency And Deduplication

- Every write must be idempotent. Use natural business keys with `INSERT ... ON DUPLICATE KEY
  UPDATE`, `INSERT IGNORE`, or `WHERE NOT EXISTS`.
- Do not rely on auto-increment identity values to detect duplicates between databases; they can
  differ.
- Define a per-table policy that lists the merge key and any columns to ignore (for example
  `created_at`, `updated_at`). Tables not in the sync allowlist must never be modified by a generic
  sync script.

## Verification On A Copy

Before any run against live core, prove the script on a disposable copy:

1. Create `mtm_receiving_application_backup_test` from the core database.
2. Run `--whatif`, review `--generate-sql`, then `--execute` against the copy.
3. Verify the copy. The copy is a snapshot of core plus the synced test rows, so it must NOT equal
   the test database alone when core and test legitimately hold different rows:
   - structure: the sync is DML-only; compare the copy's `information_schema` snapshot before and
     after the run (they must be identical),
   - every synced table matches the UNION of the original core-snapshot rows and the test rows,
     keyed on the natural business key and honoring per-table ignore lists and write modes,
   - excluded / prod-owned tables are byte-for-byte unchanged from the original core backup
     (compare row counts and per-table checksums).
4. Run the script a second time and assert zero changes (idempotency).
5. Assert excluded/prod-owned tables are unchanged from the original core backup.
6. Drop the disposable copy and report results.

## Related Guidance

- Working spec for the test-to-core sync: `Prompt.md`
- Query authoring rules: `.github/instructions/database/infor-visual-query-authoring.instructions.md`
- Infor Visual database reference: `.github/instructions/database/infor-visual-database-reference.instructions.md`
- SQL source-of-truth and non-Markdown config rules:
  `.github/instructions/tooling/non-markdown-config-files.instructions.md`

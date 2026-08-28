<!-- 
[DOC-META-START]
- File Name: database-deployment-migration-mode.md
- Description: Notes on database deployment migration mode.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-17: # Database Deployment Migration Mode
- Critical Notes: None
[DOC-META-END]
-->

# Database Deployment Migration Mode

- `Database/Database_Deployment/Deploy-Database-GUI-Workbench.ps1` now exposes both full deployment and non-destructive migration modes.
- Full deployment still drops and recreates the database before replaying `Sql_Files`.
- Migration mode skips database recreation, runs schema/migration/view/procedure/trigger/seed stages in place, and tolerates expected schema-already-exists / duplicate-seed conditions.
- Current `Sql_Files/Schemas` is only partially idempotent, so migration safety depends on the script's error filtering rather than on every schema file using `IF NOT EXISTS`.

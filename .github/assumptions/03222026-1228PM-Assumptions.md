# Assumptions For SQL Object Metadata Validation

Created: 2026-03-22 12:28 PM

1. Assumption: The parseable metadata will be a standardized SQL comment block placed near the top of every stored procedure and view file, using a markdown-style table plus fixed marker lines such as `METADATA_START` and `METADATA_END`.
   Why needed: You requested a commented table the deploy script can parse, but the exact comment syntax and parser contract were not specified.
   Potential impact if wrong: If you wanted JSON, YAML, or a different comment shape, I could annotate 147 files in a format you do not want to maintain.
   Alternative interpretations considered: JSON inside comments, key/value comment lines, external manifest files, or a markdown table without explicit markers.

Instead create a single json file that houses this information on all sql files this way the script has 1 source of truth

2. Assumption: The first implementation pass should validate structural alignment only, not full SQL semantic correctness.
   Proposed checks: referenced table exists, referenced column exists, referenced value source/parameter is declared, view column aliases map to known source columns, and metadata rows that do not match the SQL body are reported.
   Why needed: “validate that logic against the table(s) its reading looking for mismatches” can mean anything from simple schema checks to full SQL parser-level validation.
   Potential impact if wrong: A too-shallow validator may miss logic bugs; a too-deep validator will significantly increase scope, false positives, and maintenance burden.
   Alternative interpretations considered: full SQL AST parsing, regex-only validation, or table/column existence checks only.

Full Database alignment, and full sql semantic correctness is required out of this script, it can be broken down into different ps1 files if it simplifies things

3. Assumption: Migration helper procedures under `Database/Schemas/*Migration*.sql` are out of scope for the metadata-table requirement unless they create deployable views or business stored procedures.
   Why needed: The repo contains deploy-time helper procedures in schema migration files that are not business APIs, and annotating all of them the same way may add noise.
   Potential impact if wrong: The report might omit migration helper objects you expected to be covered.
   Alternative interpretations considered: annotate every `CREATE PROCEDURE` in the repository, including migration helpers and test procedures.

For migration data, if it is updating a table already generated earlier, then update the schema file and remove the migration, same thing with sps and views.
The following are migration files, valadate each against the required schema and/or sps
Database\TestData\02_Migrate-ExpandReceivingLabelData.sql
Database\TestData\04-Migrate-AddIsNonPOItem.sql
Database\TestData\05-Migrate-ExpandReceivingLabelData.sql
Database\TestData\06-receiving_label_history_reconciliation.sql

4. Assumption: Legacy database test procedures are out of scope for the initial metadata annotation pass.
   Why needed: Your request names stored procedures and views generally, but test harness procedures are validation assets rather than deployable app database APIs.
   Potential impact if wrong: You may expect those files to carry the same metadata standard from day one.
   Alternative interpretations considered: annotate all test procedures too, or include them only in report generation.

The following are not test files, these seed data into the database, move to a new SeedData folder and update ALL deploy scripts in Database\Deploy to respect it
Database\TestData\01_seed_volvo_data.sql
Database\TestData\03_seed_dunnage_data.sql

When both migration and seed files have been delt with remove the TestData folder and make sure the deploy scripts no longer address it

5. Assumption: The deploy script should never fail deployment solely because metadata validation fails; it should always emit a report file and continue unless the SQL object itself fails to deploy.
   Why needed: You explicitly asked to allow failures to pass but generate a report file.
   Potential impact if wrong: If you wanted optional strict mode, the first version would be too permissive.
   Alternative interpretations considered: warning-only mode by default with a future strict toggle, or fail the deployment on severe mismatches only.

This is fine, as we will use the report to fix failures and redeploy the database

6. Assumption: The report file should be a timestamped markdown file under `Database/Deploy/outputs/` with a machine-readable JSON companion if needed later.
   Why needed: You asked for a report file but did not specify format or location.
   Potential impact if wrong: The output may land somewhere inconvenient for your workflow.
   Alternative interpretations considered: plain text only, JSON only, or writing to the existing GUI status panel without a persisted file.

Use that location

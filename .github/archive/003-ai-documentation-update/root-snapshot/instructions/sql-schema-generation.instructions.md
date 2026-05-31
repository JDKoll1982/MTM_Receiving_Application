---
applyTo: "Database/Schemas/**/*.sql"
---

# SQL Schema Development

## Scope

- use this instruction file for table, index, constraint, trigger, and view schema files under `Database/Schemas/`
- prefer schema-only changes in these files; put operational test scripts and ad hoc reconciliation queries elsewhere
- preserve existing object names when modifying existing schema files unless the change explicitly requires a rename

## Database Schema Generation

- prefer singular names for new tables
- prefer singular names for new columns
- every new table should have a primary key column named `id` unless the existing design for that area already uses a different established key pattern
- every new table should have a `created_at` column for creation timestamp tracking unless the file is intentionally extending a legacy table that already uses a different timestamp convention
- every new table should have an `updated_at` column for last-update tracking unless the file is intentionally extending a legacy table that already uses a different timestamp convention
- do not rename existing production objects solely to enforce naming style; follow the established schema contract for that object

## Database Schema Design

- every table should have an explicit primary key constraint
- every foreign key constraint should have an explicit name
- define foreign key constraints consistently and close to the related column definitions when practical
- foreign keys should reference the parent table primary key unless there is a documented reason to reference a different unique key
- use `ON DELETE CASCADE` and `ON UPDATE CASCADE` only when that behavior is intentionally correct for the data lifecycle; do not apply cascade rules automatically to all relationships
- add indexes for columns that are expected to be common join, filter, or lookup targets

## SQL Coding Style

- use uppercase for SQL keywords such as `CREATE`, `ALTER`, `PRIMARY KEY`, `FOREIGN KEY`, `REFERENCES`, and `INDEX`
- keep indentation consistent across column lists, constraint definitions, and multi-line expressions
- break long statements into multiple lines for readability
- include comments only where they explain non-obvious design choices or migration intent
- keep one logical schema change per clearly labeled section when a file performs multiple related updates

## Table Definition Structure

- define columns in a stable, readable order: primary key, business columns, foreign keys, audit columns
- specify nullability explicitly for each column
- specify default values explicitly when the application depends on them
- prefer precise data types over oversized generic types
- use consistent naming for audit columns and status flags across related tables

## Migration and Alter Safety

- write schema changes so they are understandable to someone reviewing deployment risk before execution
- if a file alters an existing table, comment the intent of the migration near the change
- avoid destructive operations unless they are explicitly required by the task
- when adding columns or indexes to existing tables, guard the change appropriately if the deployment pattern expects repeatable scripts
- keep data backfill logic separate from pure schema definition when practical

## Views and Triggers

- use explicit column lists in views; never use `SELECT *`
- qualify columns with aliases when a view joins multiple tables
- keep trigger logic minimal and focused on the exact schema-side behavior required
- comment any trigger behavior that could surprise application developers or affect data auditing

## Security and Reliability

- do not embed credentials or environment-specific secrets in schema files
- avoid dynamic SQL in schema scripts unless there is no simpler alternative
- make constraint and index names deterministic so deployments are easier to review and troubleshoot
- prefer additive, low-risk schema evolution where possible

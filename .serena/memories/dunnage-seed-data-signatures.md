<!-- 
[DOC-META-START]
- File Name: dunnage-seed-data-signatures.md
- Description: What 03_seed_dunnage_data.sql seeds, how it stays re-runnable, and the live-database capture it is generated from.
- Last Updated: 2026-11-16
- Quick TOC:
  - Line 14-18: # Dunnage Seed Data (03_seed_dunnage_data.sql)
  - Line 19-30: ## What it seeds
  - Line 31-40: ## How it stays re-runnable
  - Line 41-52: ## Captured from live, and what live holds that the seed cannot
  - Line 53-58: ## Regenerating it
  - Line 59-64: ## Validation
- Critical Notes: The file is generated from a live capture; never hand-edit rows, regenerate instead.
[DOC-META-END]
-->

# Dunnage Seed Data (03_seed_dunnage_data.sql)

`Database/Database_Deployment/Sql_Files/SeedData/03_seed_dunnage_data.sql` is the dunnage master-data
seed. As of 2026-11-16 it is a capture of the live database, not a hand-written list.

## What it seeds

| Step | Table | Live rows | Seeded |
| --- | --- | --- | --- |
| 1 | dunnage_types | 24 | 24 |
| 2 | dunnage_custom_fields | 76 | 44 (32 unreachable, see below) |
| 3 | dunnage_custom_field_choices | 37 | 21 (16 belong to unreachable fields) |
| 4 | dunnage_parts (incl. udc1..udc10) | 158 | 158 |
| 5 | dunnage_requires_inventory | 28 | 18 (10 point at missing parts) |
| 6 | dunnage_non_po_entries | 14 | 14 |
| 7 | dunnage_no_label_parts | 8 | 8 |
| 8 | dunnage_non_po_part_defaults | 57 | 49 (8 point at missing parts) |

`dunnage_quantity_types` is seeded by schema file `40_Table_dunnage_quantity_types.sql`, which now
carries all 20 live values with `INSERT IGNORE`.

## How it stays re-runnable

- One existence-guarded `INSERT ... SELECT` per table, resolved by natural key: `type_name`,
  `part_id`, `(DunnageTypeID, FieldName)`, `(CustomFieldID, Choice)`, `value`.
- No stored-procedure calls. The insert procedures either SIGNAL on a duplicate
  (`sp_Dunnage_Types_Insert`) or rely on duplicate-key errors, and `sp_Dunnage_NonPO_Upsert`
  increments `use_count` on every replay; the guarded inserts avoid all three.
- Rows that hang off a part join `dunnage_parts`, so a run that has no such part skips them instead
  of failing the foreign key.
- A replay inserts 0 rows, touches no timestamp, and leaves `use_count` alone. Verified by
  `DunnageSeedIdempotencyIntegrationTests` and by a scratch-schema replay that compares every
  seeded table against live column-for-column.

## Captured from live, and what live holds that the seed cannot

- **32 dunnage_custom_fields** (plus their 16 choices) point at `DunnageTypeID` values that have no
  `dunnage_types` row: 4, 25, 26, 27, 28, 29, 30, 31, 34, 35, 37. The app cannot reach them - its
  type list comes from `dunnage_types` - and a seed cannot create a child without its parent. They
  are named in the file header so the omission is not read as a capture error.
- **10 dunnage_requires_inventory** and **8 dunnage_non_po_part_defaults** rows name a `part_id`
  that is not in `dunnage_parts`. Seeding them would break the foreign key on a fresh install.
- `dunnage_history` and `dunnage_label_data` are transactional and never seeded.

## Regenerating it

The session helpers `files/dump_dunnage_live.py` (live -> `files/dunnage_live_seed_data.json`) and
`files/generate_dunnage_seed.py` (JSON -> the SQL file) reproduce it. Re-run both after live
reference data changes; do not hand-edit the rows.

## Validation

`files/verify_dunnage_seed.py` builds a scratch schema from `Sql_Files/Schemas`, runs the seed
twice, and compares the business columns of all eight tables with live. It must print `MATCH` for
every table with identical row counts across the two runs, then drop the scratch schema.

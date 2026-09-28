<!-- 
[DOC-META-START]
- File Name: dunnage-seed-data-signatures.md
- Description: Seed data signatures for 03_seed_dunnage_data.sql.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-21: # Dunnage Seed Data (03_seed_dunnage_data.sql)
- Critical Notes: None
[DOC-META-END]
-->

# Dunnage Seed Data (03_seed_dunnage_data.sql)

- `Database/Database_Deployment/Sql_Files/SeedData/03_seed_dunnage_data.sql` is the dunnage master-data seed.
- Seeded tables: dunnage_types (13), dunnage_parts (~70), dunnage_specs (per-type templates derived from part spec_values), dunnage_requires_inventory (returnable dunnage types 10-13, method 'Adjust In'), dunnage_non_po_entries (6 common reasons). dunnage_quantity_types is seeded by schema file `40_Table_dunnage_quantity_types.sql` (Weight, Gallons, Boxes, Sheets, Bags, Pieces).
- CRITICAL: seed CALL signatures must match the current stored procedures:
  - `sp_Dunnage_Types_Insert(type_name, icon, image_path, user, OUT)` — 5 params.
  - `sp_Dunnage_Parts_Insert(part_id, type_id, spec_values, image_path, quantity_type, home_location, user, OUT)` — 8 params.
  - Older seed wrote 4-arg types / 6-arg parts calls (locations in the image_path slot) — that was broken and left dunnage tables empty.
- Validation pattern: copy seed, replace final COMMIT with ROLLBACK, run via `mysql ... < file` (cmd /c, since pwsh lacks `<`), expect EXIT=0 and zeroed counts (rollback) with dunnage_quantity_types=6.
- `dunnage_custom_fields` and `dunnage_non_po_part_defaults` are user-generated, intentionally NOT seeded.

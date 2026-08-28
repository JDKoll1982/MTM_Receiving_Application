<!-- 
[DOC-META-START]
- File Name: infor-visual-csv-source-of-truth.md
- Description: Notes on the Infor Visual CSV schema files as source of truth.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-16: # Infor Visual CSV Source of Truth
- Critical Notes: None
[DOC-META-END]
-->

# Infor Visual CSV Source of Truth

- For all Infor Visual-related table/query discovery, use docs/development/InforVisual/DatabaseCSVFiles/*.csv as the primary schema source of truth.
- Key files: MTMFG_Schema_Tables.csv, ColumnDetails.csv, FKs.csv, PKs.csv, Views.csv, plus related constraints/index/row-count exports.
- Validate table ownership, FK join paths, PK/composite-key lookups, actual views vs V_-prefixed base tables, and row-count/index concerns from those CSVs before changing Infor Visual query logic.

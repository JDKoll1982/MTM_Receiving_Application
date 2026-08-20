# Infor Visual CSV Source of Truth

- For all Infor Visual-related table/query discovery, use docs/development/InforVisual/DatabaseCSVFiles/*.csv as the primary schema source of truth.
- Key files: MTMFG_Schema_Tables.csv, ColumnDetails.csv, FKs.csv, PKs.csv, Views.csv, plus related constraints/index/row-count exports.
- Validate table ownership, FK join paths, PK/composite-key lookups, actual views vs V_-prefixed base tables, and row-count/index concerns from those CSVs before changing Infor Visual query logic.

# Dunnage Label Data — LabelView 2022 MySQL View

One MySQL view houses every field you need on a dunnage label: the whole
`dunnage_label_data` row plus the linked data from `dunnage_parts`,
`dunnage_types`, `auth_users`, `dunnage_quantity_types`, and the UDC display
names from `dunnage_custom_fields`. A single LabelView 2022 Table Lookup
against the view returns all of it for the row being printed.

Built from the UDC logic in `Module_Dunnage` and the schemas under
`Database/Database_Deployment/Sql_Files/`. The MySQL ODBC connection format is
verified against Microsoft Learn.

## The View

Defined in
`Database/Database_Deployment/Sql_Files/Schemas/48_View_dunnage_label_data.sql`.
The deploy tooling (`Deploy-Database-GUI-Workbench.ps1`) applies every `.sql`
in `Sql_Files/Schemas` in name order, so the view is created automatically.

The view returns, for every active queue row:

- All `dunnage_label_data` columns, with their original names (`load_uuid`,
  `part_id`, `quantity`, `udc1..udc10`, ...) so existing label bindings stay
  stable.
- Part data (`dunnage_parts`): `part_home_location`, `part_image_path`,
  `part_quantity_type`, `part_udc1..10` (current master values), and audit
  columns.
- Type data (`dunnage_types`): `type_name_live`, `type_icon_live`,
  `type_image_path`.
- Employee data (`auth_users`): `employee_full_name`, `employee_department`,
  `employee_shift`, `employee_active`.
- Quantity-type data (`dunnage_quantity_types`): `qty_type_created_by`,
  `qty_type_created_date`.
- UDC display names (`dunnage_custom_fields`): `udc1_name..udc10_name`.

All joins are `LEFT JOIN`, so a row still returns even if a related record is
missing (for example a non-provisioned user).

## LabelView Table Lookup

LabelView's Table Lookup SQL does **not** use `?` placeholders. Reference the
When Printed variable directly with `APPLICATION.DOCUMENT.[<variable>]` — the
same pattern as `Where p.ID = APPLICATION.DOCUMENT.[part_id]`.

1. Create a **When Printed** variable named `LoadUuid` (the workflow session
   GUID the MTM app passes for the label being printed).
2. Add a Table Lookup against the MySQL ODBC data source with this SQL:

```sql
SELECT * FROM view_dunnage_label_data
WHERE load_uuid = APPLICATION.DOCUMENT.[LoadUuid]
```

3. Place the returned columns on the label (text / barcode / image).
4. The lookup re-runs per printed label, keyed by the current `LoadUuid`.

A Table Lookup shows the **first** matching record, so filter on the unique
`load_uuid` as shown.

## Reprints (dunnage_history)

The active queue moves to `dunnage_history` on Clear Label Data. To reprint,
create an equivalent view against `dunnage_history` (swap the `FROM`
`dunnage_label_data` for `dunnage_history`) and point the lookup at it.

## Data Source Setup (MySQL 5.7)

- MySQL server is `172.16.1.104:3306` (from `appsettings.json`).
- Database: `mtm_receiving_application` (production) or
  `mtm_receiving_application_test` (dev/test). Pick whichever DSN you configure.
- The DSN/connection string format used by the MySQL ODBC driver (Microsoft
  Learn, "Connect to a MySQL Data Source"):

```text
Driver={MySQL ODBC 8.0 Unicode Driver};Server=172.16.1.104;Port=3306;Database=mtm_receiving_application;Uid=<user>;Pwd=<password>;CharSet=utf8mb4;
```

Do not store real credentials in the label file — configure them in the DSN and
reference the DSN from LabelView.

### MySQL 5.7 Compatibility Constraints

The view uses only MySQL 5.7-supported constructs:

- No `WITH` (CTEs), no window functions, no `PIVOT`.
- UDC display names are resolved with static `LEFT JOIN`s to
  `dunnage_custom_fields` (one join per slot 1–10).
- Read-only `SELECT` view — safe for LabelView to query.

## UDC Logic (from Module_Dunnage)

- `dunnage_label_data.udc1..udc10` hold **snapshot values** captured at save
  time (`sp_Dunnage_LabelData_Insert`).
- The **display name** for each slot comes from `dunnage_custom_fields`:
  `DunnageTypeID = dunnage_label_data.dunnage_type_id` and `DisplayOrder` =
  slot number; `FieldName` is the name shown in the UI
  (`sp_Dunnage_CustomFields_GetByType`, `Helper_Dunnage_PartSpecs`).
- `dunnage_custom_field_choices` holds pick-list options per custom field.
- If a slot has no definition, the scripts fall back to the literal
  `UDC{n}` label.

## Schema and FK Map

Full `dunnage_label_data` reference (source:
`Database/Database_Deployment/Sql_Files/Schemas/31_Table_dunnage_label_data.sql`):

| Column | Type | Notes / FK target |
| --- | --- | --- |
| `id` | INT PK | Surrogate key |
| `load_uuid` | CHAR(36) | Workflow session GUID — primary print key |
| `part_id` | VARCHAR(50) | FK-ish to `dunnage_parts.part_id` (VARCHAR, UNIQUE) |
| `dunnage_type_id` | INT NULL | FK to `dunnage_types.id` (snapshot) |
| `dunnage_type_name` | VARCHAR(100) | Type name snapshot |
| `dunnage_type_icon` | VARCHAR(100) | Icon snapshot |
| `quantity` | DECIMAL(10,2) | Received quantity |
| `quantity_type` | VARCHAR(100) | FK-ish to `dunnage_quantity_types.quantity_type` |
| `po_number` | VARCHAR(50) | PO, NULL for non-PO |
| `received_date` | DATETIME | Receive timestamp |
| `user_id` | VARCHAR(100) | FK-ish to `auth_users.windows_username` |
| `employee_number` | INT NULL | FK-ish to `auth_users.employee_number` |
| `location` | VARCHAR(100) | Warehouse location |
| `label_number` | VARCHAR(50) | Secondary key (multi-label splits, reprint) |
| `part_skid_sequence` / `part_skid_total` | INT | Skid position / total |
| `udc1..udc10` | VARCHAR(255) | UDC value snapshots |
| `created_at` | TIMESTAMP | Queue insert time |

Related tables resolved by the scripts:

| Target table | Join key | Exposed as |
| --- | --- | --- |
| `dunnage_parts` | `part_id` | `part_home_location`, `part_image_path`, `part_quantity_type`, `part_udc1..10`, audit columns |
| `dunnage_types` | `dunnage_type_id` | `live_type_name`, `live_type_icon`, `type_image_path` |
| `auth_users` | `user_id` | `employee_full_name`, `employee_department`, `employee_shift`, `employee_active` |
| `dunnage_quantity_types` | `quantity_type` | `qty_type_created_by`, `qty_type_created_date` |
| `dunnage_custom_fields` | `dunnage_type_id` + `DisplayOrder` | `udc1_name..udc10_name` |

> The view works the same way against `dunnage_history` for reprints — it
> carries the same `udc1..udc10` plus the compatibility aliases
> `dunnage_type_id` / `dunnage_type_name` / `dunnage_type_icon` (source:
> `06_Table_dunnage_history.sql`). Swap the `FROM` table to build a history
> view.

---

## Script 1 — Table Lookup: full label data (row + linked)

Returns every `dunnage_label_data` column plus the linked data (part, type,
employee, quantity type, and UDC display names) in one Table Lookup against
the view.

```mermaid
flowchart TD
  S1_Start([Label designer opens the label]) --> S1_Var[Create When Printed variable LoadUuid]
  S1_Var --> S1_Add[Data Sources > Table Lookup > Add]
  S1_Add --> S1_Name[Name the lookup variable, for example DunnageLabel]
  S1_Name --> S1_DS[Select the MySQL ODBC data source]
  S1_DS --> S1_Tbl[Choose the view view_dunnage_label_data]
  S1_Tbl --> S1_Key[Set Key field = load_uuid]
  S1_Key --> S1_Val[Set Key value = LoadUuid]
  S1_Val --> S1_Sql[Enter a query that returns the row matching LoadUuid]
  S1_Sql --> S1_Test{Test returns the expected row?}
  S1_Test -->|No| S1_Fix[Review the key field, key value, and query]
  S1_Fix --> S1_Sql
  S1_Test -->|Yes| S1_Place[Place the returned columns on the label]
  S1_Place --> S1_Save[Save the label]
  S1_Save --> S1_Done([Full-data Table Lookup ready to print])
```

To look up by a different key instead of `load_uuid`, set the Key field to
`id` or `label_number` and bind the matching When Printed variable. For
reprints, point the lookup at an equivalent `dunnage_history` view.

## Supplemental linked-data lookups (add as extra Table Lookups)

Your existing row lookup already returns the `dunnage_label_data` row keyed on
`load_uuid`. Each script below returns the **linked data from one other
table** for that same row. Add one Table Lookup variable per script (all keyed
on the same When Printed `LoadUuid`), then place the returned columns on the
label as extra fields. Every script returns a single row.

## Script 2 — Table Lookup: part detail

Adds the part master data (`dunnage_parts`) for the printed row.

```mermaid
flowchart TD
  S2_Start([Label designer opens the label]) --> S2_Var[Create When Printed variable LoadUuid]
  S2_Var --> S2_Add[Data Sources > Table Lookup > Add]
  S2_Add --> S2_Name[Name the lookup variable, for example PartDetail]
  S2_Name --> S2_DS[Select the MySQL ODBC data source]
  S2_DS --> S2_Tbl[Choose the view view_dunnage_label_data]
  S2_Tbl --> S2_Key[Set Key field = load_uuid]
  S2_Key --> S2_Val[Set Key value = LoadUuid]
  S2_Val --> S2_Sql[Enter a query that returns the row matching LoadUuid]
  S2_Sql --> S2_Test{Test returns the expected row?}
  S2_Test -->|No| S2_Fix[Review the key field, key value, and query]
  S2_Fix --> S2_Sql
  S2_Test -->|Yes| S2_Place[Place the part_* columns on the label]
  S2_Place --> S2_Save[Save the label]
  S2_Save --> S2_Done([Part-detail Table Lookup ready to print])
```

## Script 3 — Table Lookup: type detail

Adds the dunnage type data (`dunnage_types`) for the printed row.

```mermaid
flowchart TD
  S3_Start([Label designer opens the label]) --> S3_Var[Create When Printed variable LoadUuid]
  S3_Var --> S3_Add[Data Sources > Table Lookup > Add]
  S3_Add --> S3_Name[Name the lookup variable, for example TypeDetail]
  S3_Name --> S3_DS[Select the MySQL ODBC data source]
  S3_DS --> S3_Tbl[Choose the view view_dunnage_label_data]
  S3_Tbl --> S3_Key[Set Key field = load_uuid]
  S3_Key --> S3_Val[Set Key value = LoadUuid]
  S3_Val --> S3_Sql[Enter a query that returns the row matching LoadUuid]
  S3_Sql --> S3_Test{Test returns the expected row?}
  S3_Test -->|No| S3_Fix[Review the key field, key value, and query]
  S3_Fix --> S3_Sql
  S3_Test -->|Yes| S3_Place[Place the type_* columns on the label]
  S3_Place --> S3_Save[Save the label]
  S3_Save --> S3_Done([Type-detail Table Lookup ready to print])
```

## Script 4 — Table Lookup: employee detail

Adds the user / employee data (`auth_users`) for the printed row.

```mermaid
flowchart TD
  S4_Start([Label designer opens the label]) --> S4_Var[Create When Printed variable LoadUuid]
  S4_Var --> S4_Add[Data Sources > Table Lookup > Add]
  S4_Add --> S4_Name[Name the lookup variable, for example EmployeeDetail]
  S4_Name --> S4_DS[Select the MySQL ODBC data source]
  S4_DS --> S4_Tbl[Choose the view view_dunnage_label_data]
  S4_Tbl --> S4_Key[Set Key field = load_uuid]
  S4_Key --> S4_Val[Set Key value = LoadUuid]
  S4_Val --> S4_Sql[Enter a query that returns the row matching LoadUuid]
  S4_Sql --> S4_Test{Test returns the expected row?}
  S4_Test -->|No| S4_Fix[Review the key field, key value, and query]
  S4_Fix --> S4_Sql
  S4_Test -->|Yes| S4_Place[Place the employee_* columns on the label]
  S4_Place --> S4_Save[Save the label]
  S4_Save --> S4_Done([Employee-detail Table Lookup ready to print])
```

## Script 5 — Table Lookup: quantity-type detail

Adds the reusable quantity-type data (`dunnage_quantity_types`) for the
printed row.

```mermaid
flowchart TD
  S5_Start([Label designer opens the label]) --> S5_Var[Create When Printed variable LoadUuid]
  S5_Var --> S5_Add[Data Sources > Table Lookup > Add]
  S5_Add --> S5_Name[Name the lookup variable, for example QuantityTypeDetail]
  S5_Name --> S5_DS[Select the MySQL ODBC data source]
  S5_DS --> S5_Tbl[Choose the view view_dunnage_label_data]
  S5_Tbl --> S5_Key[Set Key field = load_uuid]
  S5_Key --> S5_Val[Set Key value = LoadUuid]
  S5_Val --> S5_Sql[Enter a query that returns the row matching LoadUuid]
  S5_Sql --> S5_Test{Test returns the expected row?}
  S5_Test -->|No| S5_Fix[Review the key field, key value, and query]
  S5_Fix --> S5_Sql
  S5_Test -->|Yes| S5_Place[Place the qty_type_* columns on the label]
  S5_Place --> S5_Save[Save the label]
  S5_Save --> S5_Done([Quantity-type Table Lookup ready to print])
```

## Script 6 — Table Lookup: UDC display names

Adds the UDC display name (`FieldName`) for each slot so the label can caption
the `udc1..udc10` values already returned by the row lookup.

```mermaid
flowchart TD
  S6_Start([Label designer opens the label]) --> S6_Var[Create When Printed variable LoadUuid]
  S6_Var --> S6_Add[Data Sources > Table Lookup > Add]
  S6_Add --> S6_Name[Name the lookup variable, for example UdcNames]
  S6_Name --> S6_DS[Select the MySQL ODBC data source]
  S6_DS --> S6_Tbl[Choose the view view_dunnage_label_data]
  S6_Tbl --> S6_Key[Set Key field = load_uuid]
  S6_Key --> S6_Val[Set Key value = LoadUuid]
  S6_Val --> S6_Sql[Enter a query that returns the row matching LoadUuid]
  S6_Sql --> S6_Test{Test returns the expected row?}
  S6_Test -->|No| S6_Fix[Review the key field, key value, and query]
  S6_Fix --> S6_Sql
  S6_Test -->|Yes| S6_Place[Place the udcN_name columns as captions next to each udcN value]
  S6_Place --> S6_Save[Save the label]
  S6_Save --> S6_Done([UDC-name Table Lookup ready to print])
```

## Script 7 — Table Lookup: UDC digest

Adds a single combined string of the populated UDC slots (`FieldName: value`,
separated by `|`).

```mermaid
flowchart TD
  S7_Start([Label designer opens the label]) --> S7_Var[Create When Printed variable LoadUuid]
  S7_Var --> S7_Add[Data Sources > Table Lookup > Add]
  S7_Add --> S7_Name[Name the lookup variable, for example UdcDigest]
  S7_Name --> S7_DS[Select the MySQL ODBC data source]
  S7_DS --> S7_Tbl[Choose the view view_dunnage_label_data]
  S7_Tbl --> S7_Key[Set Key field = load_uuid]
  S7_Key --> S7_Val[Set Key value = LoadUuid]
  S7_Val --> S7_Sql[Enter a query that returns the row matching LoadUuid]
  S7_Sql --> S7_Test{Test returns the expected row?}
  S7_Test -->|No| S7_Fix[Review the key field, key value, and query]
  S7_Fix --> S7_Sql
  S7_Test -->|Yes| S7_Place[Place the udc_digest column on the label]
  S7_Place --> S7_Save[Save the label]
  S7_Save --> S7_Done([UDC-digest Table Lookup ready to print])
```

## Script 8 — Table Lookup: part master (by part_id)

Pulls the part master row directly by `part_id` for a part-master label that
is not tied to a specific load.

```mermaid
flowchart TD
  S8_Start([Label designer opens the label]) --> S8_Var[Create When Printed variable PartId]
  S8_Var --> S8_Add[Data Sources > Table Lookup > Add]
  S8_Add --> S8_Name[Name the lookup variable, for example PartMaster]
  S8_Name --> S8_DS[Select the MySQL ODBC data source]
  S8_DS --> S8_Tbl[Choose the table dunnage_parts with its type and UDC fields]
  S8_Tbl --> S8_Key[Set Key field = part_id]
  S8_Key --> S8_Val[Set Key value = PartId]
  S8_Val --> S8_Sql[Enter a query that returns the part matching PartId]
  S8_Sql --> S8_Test{Test returns the expected part?}
  S8_Test -->|No| S8_Fix[Review the key field, key value, and query]
  S8_Fix --> S8_Sql
  S8_Test -->|Yes| S8_Place[Place the part master, type, and UDC name columns on the label]
  S8_Place --> S8_Save[Save the label]
  S8_Save --> S8_Done([Part-master Table Lookup ready to print])
```

## Script 9 — Reference: UDC definitions and choices

Returns the custom-field definitions and pick-list choices for a type. A type
can have up to 10 fields, each with several choices, so this returns multiple
rows and is **not** a per-label Table Lookup — build it as a Database query
and use it for pick-lists or field headers.

```mermaid
flowchart TD
  S9_Start([Label designer opens the label]) --> S9_Add[Data Source > Database > Create/Edit query]
  S9_Add --> S9_DS[Select the MySQL ODBC data source]
  S9_DS --> S9_Tbl[Choose dunnage_custom_fields joined to dunnage_custom_field_choices]
  S9_Tbl --> S9_Fields[Select the definition and choice columns]
  S9_Fields --> S9_Test{Query returns the definitions and choices?}
  S9_Test -->|No| S9_Fix[Adjust the query and test again]
  S9_Fix --> S9_Test
  S9_Test -->|Yes| S9_Use[Use the result to build a pick-list or field headers]
  S9_Use --> S9_Done([Reference query ready])
```

## Multi-record queries (Database merge)

Scripts 10 and 11 return multiple rows on purpose. Do not use them as Table
Lookup variables — a lookup would silently use the first row. Use them with
the Database merge feature (one label per record) or external tooling.

## Script 10 — Database merge: all queue rows for one part

Returns every active queue row for a part (useful for multi-skid labels).
This is a **multi-record** Database merge — do not use it as a Table Lookup
variable, which would silently show only the first row.

```mermaid
flowchart TD
  S10_Start([Label designer opens the label]) --> S10_Add[Data Source > Database > Create/Edit query]
  S10_Add --> S10_DS[Select the MySQL ODBC data source]
  S10_DS --> S10_Tbl[Choose the view view_dunnage_label_data]
  S10_Tbl --> S10_Filter[Filter to the part using the When Printed PartId variable]
  S10_Filter --> S10_Fields[Select the columns to print]
  S10_Fields --> S10_Test{Query returns all rows for the part?}
  S10_Test -->|No| S10_Fix[Adjust the filter and test again]
  S10_Fix --> S10_Test
  S10_Test -->|Yes| S10_Place[Place the database variables on the label]
  S10_Place --> S10_Print[Print one label per queue row]
  S10_Print --> S10_Done([Database merge ready])
```

## Script 11 — Database merge: entire active queue

Returns every row in the active queue for batch printing. This is a
**multi-record** Database merge (one label per record) — do not use it as a
Table Lookup variable.

```mermaid
flowchart TD
  S11_Start([Label designer opens the label]) --> S11_Add[Data Source > Database > Create/Edit query]
  S11_Add --> S11_DS[Select the MySQL ODBC data source]
  S11_DS --> S11_Tbl[Choose the view view_dunnage_label_data]
  S11_Tbl --> S11_Fields[Select the columns to print]
  S11_Fields --> S11_Test{Query returns the active queue?}
  S11_Test -->|No| S11_Fix[Adjust the query and test again]
  S11_Fix --> S11_Test
  S11_Test -->|Yes| S11_Place[Place the database variables on the label]
  S11_Place --> S11_Print[Print one label per queue row]
  S11_Print --> S11_Done([Database merge ready])
```

## LabelView Binding Notes

- The recommended approach is one Table Lookup on the view
  `view_dunnage_label_data`, keyed on the **When Printed** variable
  `LoadUuid` against the view column `load_uuid`. The lookup re-runs per
  printed label whenever `LoadUuid` changes.
- LabelView's Table Lookup SQL does **not** use `?` placeholders — reference
  the When Printed variable directly, for example
  `APPLICATION.DOCUMENT.[LoadUuid]`.
- A Table Lookup shows the **first** matching record, so key on the unique
  `load_uuid` (or `id` / `label_number`).
- Result columns map to LabelView variables **by name**. `udc1..udc10` are
  the value fields; `udc1_name..udc10_name` are the captions; `part_*` /
  `type_*` / `employee_*` / `qty_type_*` are the linked fields.
- All joins are `LEFT JOIN`, so the label still prints if a related record is
  missing (e.g., a non-provisioned user).
- For reprints, point the lookup at an equivalent `dunnage_history` view.
- Scripts 10 and 11 are **Database merge** flows (print one label per
  record), not Table Lookups.

## See Also

- [View definition](Database/Database_Deployment/Sql_Files/Schemas/48_View_dunnage_label_data.sql)
- TEKLYNX: [Differences between Databases & Table Lookups (KA-02321)](https://teklynx.microsoftcrmportals.com/knowledgebase/article/KA-02321/en-us)
- TEKLYNX: [Creating a Table Lookup (KA-02506)](https://teklynx.microsoftcrmportals.com/knowledgebase/article/KA-02506/en-us)
- TEKLYNX: [Queries, Sorting, and Joining for Databases (KA-01839)](https://teklynx.microsoftcrmportals.com/knowledgebase/article/KA-01839/en-us)
- TEKLYNX: [LabelView 2022 tutorial (PDF)](https://www.teklynx.com/-/media/Files/Manuals-and-Guides/LABELVIEW/2022/LV2022_tutorial_en.pdf)
- [Module_Dunnage UDC helper](Module_Dunnage/Helpers/Helper_Dunnage_PartSpecs.cs)
- [dunnage_label_data schema](Database/Database_Deployment/Sql_Files/Schemas/31_Table_dunnage_label_data.sql)
- [dunnage_custom_fields schema](Database/Database_Deployment/Sql_Files/Schemas/09_Table_dunnage_custom_fields.sql)
- [dunnage_custom_field_choices schema](Database/Database_Deployment/Sql_Files/Schemas/10_Table_dunnage_custom_field_choices.sql)
- [LabelView reverse-engineering analysis](docs/development/LabelView/ReverseEngThis_Analysis.md)
- [ODBC data source configuration](docs/development/InforVisual/Interaction_Guides/UI_Interaction_Guide.md)
- Microsoft Learn: [Connect to a MySQL Data Source (ODBC driver connection string)](https://learn.microsoft.com/sql/integration-services/import-export-data/connect-to-a-mysql-data-source-sql-server-import-and-export-wizard)

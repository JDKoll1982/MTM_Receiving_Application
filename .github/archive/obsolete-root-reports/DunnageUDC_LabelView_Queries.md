# Dunnage UDC LabelView Queries

One LabelView Table Lookup per UDC slot. Each query returns the display name
(`udcN_name`) for that slot from `view_dunnage_label_data`, keyed on the When
Printed `load_uuid` variable — same pattern as your existing row lookup.

Create each as a separate Table Lookup variable (Data Sources → Table Lookup →
Add), set Key field `load_uuid` / Key value `load_uuid`, paste the query, and
place the returned `udcN_name` column as the caption next to the matching
`udcN` value field.

## UDC 1 — udc1_name

```sql
SELECT [view_dunnage_label_data].[udc1_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 2 — udc2_name

```sql
SELECT [view_dunnage_label_data].[udc2_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 3 — udc3_name

```sql
SELECT [view_dunnage_label_data].[udc3_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 4 — udc4_name

```sql
SELECT [view_dunnage_label_data].[udc4_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 5 — udc5_name

```sql
SELECT [view_dunnage_label_data].[udc5_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 6 — udc6_name

```sql
SELECT [view_dunnage_label_data].[udc6_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 7 — udc7_name

```sql
SELECT [view_dunnage_label_data].[udc7_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 8 — udc8_name

```sql
SELECT [view_dunnage_label_data].[udc8_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 9 — udc9_name

```sql
SELECT [view_dunnage_label_data].[udc9_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## UDC 10 — udc10_name

```sql
SELECT [view_dunnage_label_data].[udc10_name]
FROM [view_dunnage_label_data]
WHERE [view_dunnage_label_data].[load_uuid] = APPLICATION.DOCUMENT.[load_uuid]
```

## Notes

- The `APPLICATION.DOCUMENT.[load_uuid]` reference must match the When Printed
  variable name exactly (case-sensitive in LabelView). Use `LoadUuid` if that
  is the variable name on your label.
- The same pattern works for the value columns — swap `udcN_name` for `udcN`
  (e.g., `SELECT [view_dunnage_label_data].[udc1] ...`) or for the part
  master values (`part_udcN`).

## See Also

- [DunnageLabelData_LabelView_Queries.md](DunnageLabelData_LabelView_Queries.md)
- [View definition](Database/Database_Deployment/Sql_Files/Schemas/48_View_dunnage_label_data.sql)

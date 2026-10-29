# Data Model: Receiving Analytics App Data Tab

**Feature**: `specs/002-receiving-analytics-two-tab/spec.md`
**Created**: 2026-10-29
**Inputs**: `spec.md`, `plan.md`, `research.md`

## Scope

The Receiving Analytics page becomes two tabs. ERP Data is the existing page, wrapped and
otherwise unchanged. App Data is new and reports the application's own completed receiving work
from MySQL, then compares its coverage and category mix against Infor Visual.

This feature adds no table, view, function or trigger. Every App Data read of MySQL goes through
one of four new stored procedures, and `receiving_history` is read only and never altered. Infor
Visual stays read only through the existing DAO whose connection carries
`ApplicationIntent=ReadOnly`.

| Entity | Change | Storage |
| --- | --- | --- |
| Completed receipt | read only | `receiving_history` |
| App Data filter | new, client only | `Model_Tool_ReceivingAnalyticsAppFilter` |
| Activity row | new, query row | `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay` |
| On-time row | new, query row | `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay` |
| Supplier row | new, query row | `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals` |
| Missing-load-number count | new, query result | `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount` |
| Assembled dataset | new, client only | `Model_Tool_ReceivingAnalyticsAppDataset` |
| Comparison result | new, client only | `Model_Tool_ReceivingAnalyticsAppComparison` |
| Bucketed period point | new, derived | view model output |
| Supplier ranking entry | new, derived | view model output |
| Analysis target | new, setting | `ReceivingAnalytics.OnTimeTarget` |
| Category translation | new, value set | `Translation_ReceivingAnalyticsCategory` |

## Source table: `receiving_history`

App Data counts completed work only. `receiving_history` is the completed-work table, so reading
it alone satisfies FR-004. In-progress rows live in the working label data and are never joined or
unioned in.

All four procedures filter the bucket key with `p_start_date` inclusive and `p_end_date` exclusive.
The predicate names the two columns directly rather than wrapping the coalesce in `DATE()`, so no
row needs a conversion to be tested:

```text
(received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
OR
(received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
```

A receipt with both `received_date` and `transaction_date` counts once, on the received date. A
receipt with no `received_date` counts once, on `transaction_date`. A receipt is never counted
twice.

The columns the procedures read, and how:

| Column | Use |
| --- | --- |
| `received_date` | Preferred date and time for the bucket key. |
| `transaction_date` | Date-only fallback for the bucket key. |
| `quantity` | Rounded operator entry, second choice in `COALESCE(weight_quantity, quantity)`. |
| `weight_quantity` | Unrounded operator entry, first choice in the quantity total. |
| `unit_of_measure` | Source of every unit in a quantity label, passed through as stored with blank allowed. |
| `po_due_date` | Source of the early, on time and late classification. |
| `load_number` | Source of the missing-load-number disclosure. |
| `label_number` | Source of the label count. |
| `load_guid` | Fallback in the load identity expression. |
| `load_id` | First choice in the load identity expression. |
| `vendor_name` | Source of the supplier ranking. |
| `part_type` | Source of the app side of every category figure. |
| `id` | Final fallback in the load identity expression. |

The columns on `receiving_history` that this slice does not read are `po_number`,
`po_line_number`, `po_vendor`, `part_id`, `part_description`, `po_status` and `is_reprint`.

No `is_reprint` predicate is applied. `is_reprint` marks a row that was re-queued for a label
reprint, and the completion procedure skips the insert for re-queued rows, so a reprinted label
never produces a second history row. Filtering on the flag would delete real work from the totals.

## Shared read rules

### Bucket key

Every App Data time bucket follows `COALESCE(received_date, transaction_date)`, truncated to the
calendar date. Weekly buckets run Monday to Sunday. Monthly buckets follow the calendar month. A
bucket with no completed receipts is present on the axis and empty in the data, never skipped.

### Quantity and unit

A quantity total is `SUM(COALESCE(weight_quantity, quantity))` inside one selected unit.
`weight_quantity` is the number the operator typed and `quantity` is that same number rounded, so
they are read in that order and never compared against each other. FR-023 forbids presenting one as
a check on the other, and no panel does.

No total adds across units. The quantity panels gate on a selected unit. The selector is populated
from the units the range actually contains and opens on the unit carrying the most completed
receipts. Where two units tie, the panel opens on each and shows a separate line per unit, and the
selector shows no choice.

A blank `unit_of_measure` reads as `EA`. The conversion happens once in C#, never in SQL, so the
database and the screen cannot disagree on how blanks are read.

Every quantity total names its unit. A total is labelled `Quantity (` followed by the unit in
capitals followed by `)`, for example `Quantity (LB)`, `Quantity (EA)`, `Quantity (COILS)`.

### Load and label identity

A label is `COUNT(*)` over completed receipt rows. A load is:

```text
COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR)))
```

The two measures are defined differently so they can diverge, which is why FR-008 keeps them
separate. Today they agree because `label_number` is written from the same per-session counter as
`load_number` and `load_id` is a fresh GUID on every row, so one completed receipt row is one load
and one label at once. Counting `DISTINCT load_number` would merge unrelated loads across sessions
and badly undercount, so it is never done.

The number of receipts in range whose `load_number` is null is disclosed beside the two figures.
On those older rows the label number stands in as the load, and the disclosure is what stops a
reader mistaking today's agreement for a defect.

### Category translation

One static type, `Translation_ReceivingAnalyticsCategory`, owns the three app category names, the
five ERP category names, and the mapping in both directions. The app procedures pass `part_type`
through untranslated; normalisation happens only in that type, and the ERP tab's existing copies of
the ERP category literals fold onto it.

The app side sorts into `Standard`, `Coil` and `Sheet`. The ERP side sorts into `Parts`,
`MMC Coils`, `MMF Flat`, `Outside Service` and `Uninventoried`.

| App category | ERP category |
| --- | --- |
| `Standard` | `Parts` |
| `Coil` | `MMC Coils` |
| `Sheet` | `MMF Flat` |
| no app counterpart | `Outside Service` |
| no app counterpart | `Uninventoried` |

An app category is derived from the stored `part_type` by contains: `MMC` maps to `Coil`, `MMF`
maps to `Sheet`, anything else maps to `Standard`. This is the rule `Model_ReceivingLoad` already
applies, and the translation type is its single home. The ERP side keeps its own unchanged SQL
Server derivation, which matches `MMC` and `MMF` by starts with for `MMC Coils` and `MMF Flat`.

On the comparison mix panel both sides are normalised to the five ERP names, so the app side shows
`Parts`, `MMC Coils` and `MMF Flat` and a zero share for `Outside Service` and `Uninventoried`.
The disagreement therefore reads as a real difference between the systems, not as a page defect.

## Stored procedure result shapes

The four procedures each return the finest grain a panel needs, which is one row per received day
plus the category, unit or status the panel needs. Weekly and monthly bucketing happens in C#, so
one fetch serves all three periods.

### Activity row

`sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay` returns one row per received day, material
category and unit.

| Field | Type | Meaning |
| --- | --- | --- |
| `bucket_date` | `DATE` | The calendar date of `COALESCE(received_date, transaction_date)`. |
| `part_type` | `VARCHAR` | Stored `part_type`, passed through untranslated. |
| `unit_of_measure` | `VARCHAR` | Stored `unit_of_measure`, blank allowed. |
| `quantity_total` | `DECIMAL` | `SUM(COALESCE(weight_quantity, quantity))` for the day, category and unit. |
| `label_count` | `INT` | `COUNT(*)` for the day. |
| `load_count` | `INT` | `COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR)))` for the day. |

`label_count` and `load_count` are per-day figures and repeat on every category and unit row of the
same day. The assembler reads them once per day. The model the DAO maps into is
`Model_Tool_ReceivingAnalyticsAppActivityRow`.

### On-time row

`sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay` returns one row per received day and status.

| Field | Type | Meaning |
| --- | --- | --- |
| `bucket_date` | `DATE` | The calendar date of the bucket key. |
| `status` | `VARCHAR` | One of `Early`, `OnTime`, `Late` or `NoDueDate`. |
| `receipt_count` | `INT` | Completed receipts in that status on that day. |

The classification compares the bucket date against `po_due_date`. A receipt completed on its due
date counts as on time. `NoDueDate` rows carry receipts with a null `po_due_date`; they form the
separate remainder and are never counted as on time. The model the DAO maps into is
`Model_Tool_ReceivingAnalyticsAppOnTimeRow`.

### Supplier row

`sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals` returns one row per supplier and unit.

| Field | Type | Meaning |
| --- | --- | --- |
| `vendor_name` | `VARCHAR` | Supplier grouped case-insensitively with whitespace trimmed; blank preserved for `Unknown`. |
| `unit_of_measure` | `VARCHAR` | Stored unit, blank allowed. |
| `label_count` | `INT` | Labels for the supplier. |
| `load_count` | `INT` | Loads for the supplier. |
| `quantity_total` | `DECIMAL` | `SUM(COALESCE(weight_quantity, quantity))` for the supplier and unit. |

`label_count` and `load_count` are per-supplier figures and repeat on every unit row of the same
supplier; the assembler reads them once per supplier when ranking by labels or loads. The model the
DAO maps into is `Model_Tool_ReceivingAnalyticsAppSupplierRow`.

### Missing-load-number count

`sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount` returns one row with one column.

| Field | Type | Meaning |
| --- | --- | --- |
| `missing_load_number_count` | `INT` | `COUNT(*)` of rows in range with `load_number IS NULL`. |

This is the FR-035 disclosure figure.

## Client-side models

### App Data filter

`Model_Tool_ReceivingAnalyticsAppFilter` holds the tab's own filter state. It never shares state
with the ERP tab.

| Field | Type | Default |
| --- | --- | --- |
| `StartDate` | `DateTime` | Today minus 29 days, inclusive. |
| `EndDate` | `DateTime` | Today plus one day, exclusive, so the range covers the last 30 calendar days. |
| `Period` | enum | `Day`; also `Week` and `Month`. |
| `UnitOfMeasure` | `string?` | Dominant unit; null means no single unit, show one line per unit. Blank reads as `EA`. |
| `SupplierMeasure` | enum | `Labels`; also `Quantity` and `Loads`. |

### Assembled dataset

`Model_Tool_ReceivingAnalyticsAppDataset` holds the raw per-day rows the service fetched, so a
period or supplier-measure change re-buckets in memory instead of re-querying.

| Field | Type |
| --- | --- |
| `activity_rows` | read-only list of `Model_Tool_ReceivingAnalyticsAppActivityRow` |
| `on_time_rows` | read-only list of `Model_Tool_ReceivingAnalyticsAppOnTimeRow` |
| `supplier_rows` | read-only list of `Model_Tool_ReceivingAnalyticsAppSupplierRow` |
| `missing_load_number_count` | `int` |

The Empty state is derived from the dataset: the range is empty when the activity rows contain no
completed receipts.

### Comparison result

`Model_Tool_ReceivingAnalyticsAppComparison` carries both sides of the coverage and mix panels. It
is produced by a separate service method because its ERP half can fail on its own, and a failure
there must leave the other three panels working.

| Field | Type |
| --- | --- |
| `AppCoverageSeries` | list of `(date, count)` for the app side |
| `ErpCoverageSeries` | list of `(date, count)` for the ERP side |
| `AppCategorySplit` | shares over the five ERP names for the app side |
| `ErpCategorySplit` | shares over the five ERP names for the ERP side |
| `ErpAvailable` | `bool` |

Coverage counts one app label against one ERP receipt line. Both sides are grouped by received date
and by the app's normalised category, then the app's categories are translated to the ERP names. An
ERP row with no app equivalent, `Outside Service` or `Uninventoried`, counts toward the ERP line
only. Both sides are requested with identical date bounds, which is the only filter the ERP query
can express and therefore the whole of the filter.

### Bucketed period point

The view model builds one period point per bucket from the cached dataset. The activity and on-time
panels share the same points because they share the same period selector and the same per-day rows.

| Field | Type |
| --- | --- |
| `PeriodStart` | `DateTime` |
| `PeriodLabel` | `string` |
| `LoadCount` | `int` |
| `LabelCount` | `int` |
| `CategoryTotals` | per app category totals for the selected unit, keyed by `Standard`, `Coil`, `Sheet` |
| `EarlyCount` | `int` |
| `OnTimeCount` | `int` |
| `LateCount` | `int` |
| `NoDueDateCount` | `int` |
| `OnTimePercentage` | `decimal?`, null when the period has no receipts carrying a due date |

The on-time percentage is `(EarlyCount + OnTimeCount) / (EarlyCount + OnTimeCount + LateCount)`.
When the denominator is zero the percentage is absent, never zero and never one hundred.

### Supplier ranking entry

The view model builds one entry per ranked supplier plus one remainder entry.

| Field | Type |
| --- | --- |
| `VendorName` | `string`, `Unknown` when blank |
| `LabelCount` | `int` |
| `LoadCount` | `int` |
| `QuantityTotal` | `decimal`, for the selected unit |
| `Rank` | `int` |
| `CumulativeShare` | `decimal`, running share of the selected measure |
| `IsRemainder` | `bool` |

The ranking defaults to labels and can switch to quantity or loads. Quantity ranking sums within the
selected unit and names it. Suppliers beyond the leading few are grouped into one remainder entry,
so the cumulative share across the entries reaches 100 per cent.

## Key entities

- **Completed receipt**: one row in `receiving_history`, the single source for every App Data panel.
  It holds the material, the quantity with its unit, the supplier, the receipt date and time, the PO
  due date, and the load and label it belonged to.
- **Load**: the grouping of labels received together. Identified for counting by
  `COALESCE(load_id, load_guid, CAST(id AS CHAR))`. The human `load_number` is absent on older rows,
  which is what the missing-load-number disclosure measures.
- **Label**: one printed and applied label, counted as `COUNT(*)`. One label per completed receipt
  in this slice.
- **Material category**: how a receipt is sorted on each side. The app sorts into `Standard`, `Coil`
  and `Sheet` from `part_type`; Infor Visual sorts into `Parts`, `MMC Coils`, `MMF Flat`,
  `Outside Service` and `Uninventoried`.
- **Category translation**: `Translation_ReceivingAnalyticsCategory`, the one definition that maps
  an app category to an ERP category and back, including the two ERP categories with no app
  counterpart.
- **Supplier**: the vendor named on the receipt, `vendor_name` grouped case-insensitively and
  trimmed. A blank supplier is ranked as `Unknown`, not dropped.
- **Analysis target**: the business goal, currently the on-time percentage, with its value and
  whether it is set. Drives the red and green marking.

## Analysis target

The target is `ReceivingAnalytics.OnTimeTarget`, a decimal under the `ShipRecTools` settings
category, declared in `Module_Settings.Core/Defaults/settings.manifest.json` with a default of 95
and administrator permission. It is read through `IService_ShipRecToolsSettings`.

| Field | Type | Meaning |
| --- | --- | --- |
| `value` | `decimal?` | The pass mark, null or absent when no target is configured. |
| `is_set` | `bool` | False when the value is blank or missing. |

A period whose on-time percentage is at or above the target shows green. Below the target shows
red, with the target in force displayed beside it. Where no target is set the percentage is shown
plainly with no colour and no implied pass or fail.

## State transitions

The App Data tab has six states. Five form the query lifecycle; NoTarget is orthogonal and can hold
in any of them.

- **Idle**: the tab has not run a query and no panel carries figures.
- **Loading**: the filter was accepted; the four app reads and the ERP comparison are in flight;
  panels hold their previous figures behind a progress ring.
- **Loaded**: all four panels carry figures for the range.
- **Empty**: the range holds no completed receipts; the panels state that instead of drawing a flat
  chart of zeros, and the filter bar stays usable.
- **ErpUnavailable**: the three app panels are complete; the comparison panel reports that the ERP
  side could not be read.
- **NoTarget**: the on-time target is blank or missing, so the percentage is shown plainly.

Transitions:

| From | To | When |
| --- | --- | --- |
| Idle | Loading | The Load command runs with a valid filter. |
| Loading | Loaded | All reads succeed and the range has at least one completed receipt. |
| Loading | Empty | The app reads succeed and the range has no completed receipts. |
| Loading | ErpUnavailable | The app reads succeed and the ERP comparison fails. |
| Loaded, Empty, ErpUnavailable | Loading | The next Load command runs. |
| ErpUnavailable | Loaded or Empty | A later Load finds the ERP reachable. |

Period, unit and supplier-measure changes do not leave Loaded. They call `RenderFromLastData`,
which re-buckets and re-renders the cached rows without re-querying. Only a new Load re-queries.
NoTarget is entered when the settings read returns a blank or missing target and left when a target
is set; it affects only the on-time panel's presentation.

## Notes

This feature adds four stored procedures and nothing else to the database. No table, view, function
or trigger is created or altered, and `receiving_history` is read only and never written.

The comparison inherits two limits from the unchanged ERP query, documented here so the panels do
not silently adjust them. The ERP query counts one PO line anchored on its last received date, so a
line received on two days counts once, on the later day. It also excludes consignment and internal
orders. The comparison therefore states what it is: app labels against ERP receipt lines, on the
same dates, with the ERP counted as the ERP counts.

# Stored Procedures: Receiving Analytics App Data

Deployment path: `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/`

All four procedures read `receiving_history` only, take the same two date parameters, and follow the
repo's `DELIMITER $$` / `DROP PROCEDURE IF EXISTS` / `CREATE PROCEDURE` conventions.

## Shared conventions

| Item | Rule |
| --- | --- |
| Parameters | `IN p_start_date DATETIME, IN p_end_date DATETIME` |
| Range semantics | start inclusive with `>=`, end exclusive with `<` |
| Bucket key | `DATE(COALESCE(received_date, transaction_date))` |
| Range predicate | direct column comparison, never wrapped in `DATE()` or `COALESCE()` |
| Allowed columns | `quantity`, `weight_quantity`, `transaction_date`, `received_date`, `unit_of_measure`, `po_due_date`, `load_number`, `label_number`, `load_guid`, `load_id`, `vendor_name`, `po_vendor`, `part_type`, `id` |
| Load expression | `COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR)))` |
| Unit | `unit_of_measure` is passed through as stored, including blank; `EA` is applied in C#, not in SQL |
| Category | `part_type` is passed through untranslated; the three app category names are applied in C#, not in SQL |
| Reprints | `is_reprint` is deliberately not filtered |

The supplier source is `vendor_name` only, as pinned by the spec. `po_vendor` is in the allowed
column set but is not referenced.

`is_reprint` is deliberately not filtered because "`is_reprint` on a history row means that row was
re-queued for a label reprint, not that it is a duplicate" (R14). Filtering it would delete real work
from the totals.

## sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay

| Column | Type | Meaning |
| --- | --- | --- |
| `bucket_date` | `DATE` | `DATE(COALESCE(received_date, transaction_date))` |
| `part_type` | `VARCHAR(50)`, nullable | category as stored, untranslated |
| `unit_of_measure` | `VARCHAR(20)`, nullable | unit as stored, blank allowed |
| `load_count` | `BIGINT` | distinct loads for the day, repeated on every row of that day |
| `label_count` | `BIGINT` | labels for the day, repeated on every row of that day |
| `quantity_total` | `DECIMAL` | `SUM(COALESCE(weight_quantity, quantity))` for the day plus category plus unit |
| `receipt_count` | `BIGINT` | `COUNT(*)` for the day plus category plus unit: the app labels in that one group |

GROUP BY grain: one row per (`bucket_date`, `part_type`, `unit_of_measure`). `load_count` and
`label_count` are day totals joined in from a derived table, so they repeat on every row that shares
the day. A consumer must read them once per day, not sum them across category and unit rows.
`receipt_count` is the opposite: it belongs to the group, so summing it across a category's rows
gives that category's label count and comparing it with the ERP's receipt lines compares like with
like.

Ordering: `bucket_date` ascending, then `part_type` ascending, then `unit_of_measure` ascending.

```sql
DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay`$$

CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        DATE(COALESCE(rh.received_date, rh.transaction_date)) AS bucket_date,
        rh.part_type AS part_type,
        rh.unit_of_measure AS unit_of_measure,
        day_totals.load_count AS load_count,
        day_totals.label_count AS label_count,
        SUM(COALESCE(rh.weight_quantity, rh.quantity)) AS quantity_total,
        COUNT(*) AS receipt_count
    FROM receiving_history rh
    INNER JOIN (
        SELECT
            DATE(COALESCE(received_date, transaction_date)) AS bucket_date,
            COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR))) AS load_count,
            COUNT(*) AS label_count
        FROM receiving_history
        WHERE
            (
                (received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
                OR
                (received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
            )
        GROUP BY DATE(COALESCE(received_date, transaction_date))
    ) AS day_totals
        ON day_totals.bucket_date = DATE(COALESCE(rh.received_date, rh.transaction_date))
    WHERE
        (
            (rh.received_date IS NOT NULL AND rh.received_date >= p_start_date AND rh.received_date < p_end_date)
            OR
            (rh.received_date IS NULL AND rh.transaction_date >= p_start_date AND rh.transaction_date < p_end_date)
        )
    GROUP BY
        DATE(COALESCE(rh.received_date, rh.transaction_date)),
        rh.part_type,
        rh.unit_of_measure,
        day_totals.load_count,
        day_totals.label_count
    ORDER BY bucket_date ASC, rh.part_type ASC, rh.unit_of_measure ASC;
END $$

DELIMITER ;
```

## sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay

| Column | Type | Meaning |
| --- | --- | --- |
| `bucket_date` | `DATE` | `DATE(COALESCE(received_date, transaction_date))` |
| `status` | `VARCHAR`, one of `Early`, `OnTime`, `Late`, `NoDueDate` | on-time classification |
| `receipt_count` | `BIGINT` | `COUNT(*)` for the day plus status |

GROUP BY grain: one row per (`bucket_date`, `status`).

Classification:

| Status | Rule |
| --- | --- |
| `Early` | bucket date is before `po_due_date` |
| `OnTime` | bucket date equals `po_due_date` |
| `Late` | bucket date is after `po_due_date` |
| `NoDueDate` | `po_due_date` is null |

On or before the due date counts as on time, so the on-time percentage is
(`Early` + `OnTime`) divided by (`Early` + `OnTime` + `Late`). `NoDueDate` is a separate remainder
and is never counted as on time.

Ordering: `bucket_date` ascending, then status in the order `Early`, `OnTime`, `Late`, `NoDueDate`.

```sql
DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay`$$

CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        DATE(COALESCE(received_date, transaction_date)) AS bucket_date,
        CASE
            WHEN po_due_date IS NULL THEN 'NoDueDate'
            WHEN DATE(COALESCE(received_date, transaction_date)) < po_due_date THEN 'Early'
            WHEN DATE(COALESCE(received_date, transaction_date)) = po_due_date THEN 'OnTime'
            ELSE 'Late'
        END AS status,
        COUNT(*) AS receipt_count
    FROM receiving_history
    WHERE
        (
            (received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
            OR
            (received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
        )
    GROUP BY
        DATE(COALESCE(received_date, transaction_date)),
        CASE
            WHEN po_due_date IS NULL THEN 'NoDueDate'
            WHEN DATE(COALESCE(received_date, transaction_date)) < po_due_date THEN 'Early'
            WHEN DATE(COALESCE(received_date, transaction_date)) = po_due_date THEN 'OnTime'
            ELSE 'Late'
        END
    ORDER BY
        bucket_date ASC,
        FIELD(status, 'Early', 'OnTime', 'Late', 'NoDueDate');
END $$

DELIMITER ;
```

## sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals

| Column | Type | Meaning |
| --- | --- | --- |
| `vendor_name` | `VARCHAR` | `COALESCE(NULLIF(TRIM(vendor_name), ''), 'Unknown')` |
| `unit_of_measure` | `VARCHAR(20)`, nullable | unit as stored, blank allowed |
| `label_count` | `BIGINT` | `COUNT(*)` for the supplier plus unit |
| `load_count` | `BIGINT` | `COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR)))` for the supplier plus unit |
| `quantity_total` | `DECIMAL` | `SUM(COALESCE(weight_quantity, quantity))` for the supplier plus unit |

GROUP BY grain: one row per (`vendor_name`, `unit_of_measure`). A blank or null supplier is grouped
as `Unknown`, so no receipt is dropped from the ranking. The supplier is grouped case-insensitively
and with whitespace trimmed.

Ordering: `vendor_name` ascending, then `unit_of_measure` ascending.

```sql
DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals`$$

CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        COALESCE(NULLIF(TRIM(vendor_name), ''), 'Unknown') AS vendor_name,
        unit_of_measure AS unit_of_measure,
        COUNT(*) AS label_count,
        COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR))) AS load_count,
        SUM(COALESCE(weight_quantity, quantity)) AS quantity_total
    FROM receiving_history
    WHERE
        (
            (received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
            OR
            (received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
        )
    GROUP BY
        COALESCE(NULLIF(TRIM(vendor_name), ''), 'Unknown'),
        unit_of_measure
    ORDER BY vendor_name ASC, unit_of_measure ASC;
END $$

DELIMITER ;
```

## sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount

| Column | Type | Meaning |
| --- | --- | --- |
| `missing_load_number_count` | `BIGINT` | `COUNT(*)` of receipts in range with `load_number IS NULL` |

Returns a single scalar row. There is no grouping and no ordering.

```sql
DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount`$$

CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME
)
BEGIN
    SELECT
        COUNT(*) AS missing_load_number_count
    FROM receiving_history
    WHERE
        (
            (received_date IS NOT NULL AND received_date >= p_start_date AND received_date < p_end_date)
            OR
            (received_date IS NULL AND transaction_date >= p_start_date AND transaction_date < p_end_date)
        )
        AND load_number IS NULL;
END $$

DELIMITER ;
```

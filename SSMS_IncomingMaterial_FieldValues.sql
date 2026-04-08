-- ========================================
-- Query: SSMS Incoming Material Field Values
-- Description: Reproduces the same Incoming Material field values shown on the Material Availability Board.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
-- Run in SSMS.
-- Use either @LocationId OR @PartId.

SET NOCOUNT ON;

DECLARE @WarehouseCode      nvarchar(15) = '002';
DECLARE @LocationId         nvarchar(30) = 'RECV';
DECLARE @PartId             nvarchar(30) = NULL;
DECLARE @IncomingWindowDays int          = 30;   -- Use NULL for the same behavior as the app's "All" option.

DROP TABLE IF EXISTS #RequestedPartCatalog;
DROP TABLE IF EXISTS #SelectedDates;
DROP TABLE IF EXISTS #DistinctDateRows;
DROP TABLE IF EXISTS #FirstDisplayDate;
DROP TABLE IF EXISTS #RollupByPart;

;
WITH NormalizedParameters AS (
    SELECT
        UPPER(LTRIM(RTRIM(@WarehouseCode))) AS WarehouseCode,
        NULLIF(UPPER(LTRIM(RTRIM(@LocationId))), '') AS LocationId,
        NULLIF(UPPER(LTRIM(RTRIM(@PartId))), '') AS PartId
),
RequestedParts AS (
    SELECT np.PartId AS PartId
    FROM NormalizedParameters np
    WHERE np.PartId IS NOT NULL

    UNION

    SELECT DISTINCT cpl.ID AS PartId
    FROM dbo.CR_PART_LOCATION cpl
    CROSS JOIN NormalizedParameters np
    WHERE np.PartId IS NULL
      AND np.LocationId IS NOT NULL
      AND cpl.WAREHOUSE_ID = np.WarehouseCode
      AND cpl.LOCATION_ID = np.LocationId
      AND COALESCE(cpl.QTY, 0) > 0
)
SELECT
    rp.PartId,
    COALESCE(p.DESCRIPTION, '') AS PartDescription
INTO #RequestedPartCatalog
FROM RequestedParts rp
LEFT JOIN dbo.PART p
    ON p.ID = rp.PartId;

;
WITH NormalizedParameters AS (
    SELECT
        UPPER(LTRIM(RTRIM(@WarehouseCode))) AS WarehouseCode,
        NULLIF(UPPER(LTRIM(RTRIM(@LocationId))), '') AS LocationId,
        NULLIF(UPPER(LTRIM(RTRIM(@PartId))), '') AS PartId,
        @IncomingWindowDays AS IncomingWindowDays
),
RequestedParts AS (
    SELECT np.PartId AS PartId
    FROM NormalizedParameters np
    WHERE np.PartId IS NOT NULL

    UNION

    SELECT DISTINCT cpl.ID AS PartId
    FROM dbo.CR_PART_LOCATION cpl
    CROSS JOIN NormalizedParameters np
    WHERE np.PartId IS NULL
      AND np.LocationId IS NOT NULL
      AND cpl.WAREHOUSE_ID = np.WarehouseCode
      AND cpl.LOCATION_ID = np.LocationId
      AND COALESCE(cpl.QTY, 0) > 0
),
IncomingRows AS (
    SELECT
        rp.PartId AS PartId,
        COALESCE(p.DESCRIPTION, '') AS PartDescription,
        np.WarehouseCode AS WarehouseCode,
        po.ID AS PONumber,
        CONVERT(nvarchar(10), pol.LINE_NO) AS POLineNumber,
        COALESCE(v.NAME, '') AS VendorName,
        po.STATUS AS PoStatus,
        pol.ORDER_QTY AS OrderedQty,
        pol.TOTAL_RECEIVED_QTY AS ReceivedQty,
        (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) AS RemainingQty,
        pol.DESIRED_RECV_DATE AS LineDesiredReceiveDate,
        pol.PROMISE_DATE AS LinePromiseDate,
        pol.LAST_RECEIVED_DATE AS LineLastReceivedDate,
        po.PROMISE_DATE AS HeaderPromiseDate,
        po.DESIRED_RECV_DATE AS HeaderDesiredReceiveDate,
        COALESCE(po.FREE_ON_BOARD, '') AS FreeOnBoard,
        CASE
            WHEN RIGHT(RTRIM(po.ID), 1) = 'B' THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END AS IsBlanketOrder
    FROM RequestedParts rp
    CROSS JOIN NormalizedParameters np
    INNER JOIN dbo.PURC_ORDER_LINE pol
        ON pol.PART_ID = rp.PartId
    INNER JOIN dbo.PURCHASE_ORDER po
        ON po.ID = pol.PURC_ORDER_ID
    LEFT JOIN dbo.PART p
        ON p.ID = rp.PartId
    LEFT JOIN dbo.VENDOR v
        ON v.ID = po.VENDOR_ID
    WHERE ISNULL(po.STATUS, '') NOT IN ('C', 'X')
      AND ISNULL(pol.LINE_STATUS, '') NOT IN ('C', 'X')
      AND (
            pol.WAREHOUSE_ID = np.WarehouseCode
            OR (
                NULLIF(LTRIM(RTRIM(pol.WAREHOUSE_ID)), '') IS NULL
                AND po.WAREHOUSE_ID = np.WarehouseCode
            )
          )
      AND (
            (RIGHT(RTRIM(po.ID), 1) = 'B' AND pol.LAST_RECEIVED_DATE IS NOT NULL)
            OR (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) > 0
          )
),
SelectedDates AS (
    SELECT
        row.PartId,
        row.PartDescription,
        row.PONumber,
        row.POLineNumber,
        row.OrderedQty,
        row.ReceivedQty,
        row.RemainingQty,
        row.IsBlanketOrder,
        selected.SelectedDate,
        selected.DateLabel,
        selected.IsBlanket
    FROM IncomingRows row
    OUTER APPLY (
        SELECT TOP (1)
            candidate.SelectedDate,
            candidate.DateLabel,
            candidate.IsBlanket
        FROM (
            SELECT row.LinePromiseDate AS SelectedDate, 'Line promise' AS DateLabel, CAST(0 AS bit) AS IsBlanket, 0 AS Priority
            WHERE row.LinePromiseDate IS NOT NULL

            UNION ALL

            SELECT row.LineDesiredReceiveDate, 'Line desired', CAST(0 AS bit), 1
            WHERE row.LineDesiredReceiveDate IS NOT NULL

            UNION ALL

            SELECT row.HeaderPromiseDate, 'PO promise', CAST(0 AS bit), 2
            WHERE row.HeaderPromiseDate IS NOT NULL

            UNION ALL

            SELECT row.HeaderDesiredReceiveDate, 'PO desired', CAST(0 AS bit), 3
            WHERE row.HeaderDesiredReceiveDate IS NOT NULL

            UNION ALL

            SELECT row.LineLastReceivedDate, 'Last received on', CAST(1 AS bit), 4
            WHERE row.IsBlanketOrder = 1 AND row.LineLastReceivedDate IS NOT NULL
        ) candidate
        ORDER BY candidate.SelectedDate, candidate.Priority
    ) selected
)
SELECT
    PartId,
    PartDescription,
    PONumber,
    POLineNumber,
    OrderedQty,
    ReceivedQty,
    RemainingQty,
    DateLabel,
    CAST(SelectedDate AS date) AS DisplayDate,
    IsBlanketOrder,
    IsBlanket
INTO #SelectedDates
FROM SelectedDates;

;
WITH DistinctDateRows AS (
    SELECT DISTINCT
        row.PartId,
        row.PartDescription,
        row.DateLabel,
        row.DisplayDate,
        row.IsBlanket
    FROM #SelectedDates row
    WHERE row.DisplayDate IS NOT NULL
      AND (
            row.IsBlanketOrder = 1
            OR @IncomingWindowDays IS NULL
            OR (
                row.DisplayDate >= CAST(GETDATE() AS date)
                AND row.DisplayDate <= DATEADD(day, @IncomingWindowDays, CAST(GETDATE() AS date))
            )
          )
)
SELECT
    PartId,
    PartDescription,
    DateLabel,
    DisplayDate,
    IsBlanket
INTO #DistinctDateRows
FROM DistinctDateRows;

;
WITH FirstDisplayDate AS (
    SELECT
        PartId,
        DateLabel,
        DisplayDate,
        IsBlanket,
        ROW_NUMBER() OVER (
            PARTITION BY PartId
            ORDER BY IsBlanket, DisplayDate, DateLabel
        ) AS RowNumber
    FROM #DistinctDateRows
)
SELECT
    PartId,
    DateLabel,
    DisplayDate,
    IsBlanket
INTO #FirstDisplayDate
FROM FirstDisplayDate
WHERE RowNumber = 1;

;
WITH QualifyingRollupRows AS (
    SELECT
        row.PartId,
        row.PartDescription,
        row.PONumber,
        row.POLineNumber,
        row.OrderedQty,
        row.ReceivedQty,
        row.RemainingQty,
        ROW_NUMBER() OVER (
            PARTITION BY row.PartId, row.PONumber, row.POLineNumber
            ORDER BY row.PONumber, row.POLineNumber
        ) AS RowNumberWithinPOLine
    FROM #SelectedDates row
    WHERE row.IsBlanketOrder = 1
       OR row.DisplayDate IS NULL
       OR @IncomingWindowDays IS NULL
       OR (
            row.DisplayDate >= CAST(GETDATE() AS date)
            AND row.DisplayDate <= DATEADD(day, @IncomingWindowDays, CAST(GETDATE() AS date))
          )
),
RollupByPart AS (
    SELECT
        q.PartId,
        MAX(q.PartDescription) AS PartDescription,
        SUM(q.OrderedQty) AS OrderedQty,
        SUM(q.ReceivedQty) AS ReceivedQty,
        SUM(q.RemainingQty) AS RemainingQty,
        COUNT(DISTINCT q.PONumber) AS PurchaseOrderCount,
        COUNT(*) AS POLineCount
    FROM QualifyingRollupRows q
    WHERE q.RowNumberWithinPOLine = 1
    GROUP BY q.PartId
)
SELECT
    PartId,
    PartDescription,
    OrderedQty,
    ReceivedQty,
    RemainingQty,
    PurchaseOrderCount,
    POLineCount
INTO #RollupByPart
FROM RollupByPart;

SELECT
    rpc.PartId,
    COALESCE(r.PartDescription, rpc.PartDescription) AS PartDescription,
    CAST(ROUND(COALESCE(r.OrderedQty, 0), 0) AS int) AS OrderedQtyDisplay,
    CAST(ROUND(COALESCE(r.ReceivedQty, 0), 0) AS int) AS ReceivedQtyDisplay,
    CAST(ROUND(COALESCE(r.RemainingQty, 0), 0) AS int) AS RemainingQtyDisplay,
    CONCAT(COALESCE(r.PurchaseOrderCount, 0), ' PO(s)') AS PurchaseOrderCountSummary,
    CONCAT(COALESCE(r.POLineCount, 0), ' line(s)') AS POLineCountSummary,
    CONCAT(
        CASE
            WHEN COALESCE(r.OrderedQty, 0) <= 0 THEN 0
            ELSE CAST(ROUND((COALESCE(r.ReceivedQty, 0) / NULLIF(r.OrderedQty, 0)) * 100.0, 0) AS int)
        END,
        '% complete'
    ) AS PercentCompleteSummary,
    CASE
        WHEN f.DisplayDate IS NOT NULL THEN CONCAT('Next material date: ', f.DateLabel, ' ', CONVERT(varchar(10), f.DisplayDate, 101))
        WHEN COALESCE(r.POLineCount, 0) > 0 THEN 'Incoming material found, but no qualifying due dates are available.'
        WHEN @IncomingWindowDays IS NOT NULL THEN CONCAT('No inbound material due in the next ', @IncomingWindowDays, ' days.')
        ELSE 'No inbound material found.'
    END AS NextDateSummary
FROM #RequestedPartCatalog rpc
LEFT JOIN #RollupByPart r
    ON r.PartId = rpc.PartId
LEFT JOIN #FirstDisplayDate f
    ON f.PartId = rpc.PartId
ORDER BY rpc.PartId;

SELECT
    d.PartId,
    d.PartDescription,
    d.DateLabel,
    CONVERT(varchar(10), d.DisplayDate, 101) AS DisplayDate,
    CONCAT(d.DateLabel, ' ', CONVERT(varchar(10), d.DisplayDate, 101)) AS DisplayText
FROM #DistinctDateRows d
ORDER BY d.PartId, d.IsBlanket, d.DisplayDate, d.DateLabel;

DROP TABLE IF EXISTS #RollupByPart;
DROP TABLE IF EXISTS #FirstDisplayDate;
DROP TABLE IF EXISTS #DistinctDateRows;
DROP TABLE IF EXISTS #SelectedDates;
DROP TABLE IF EXISTS #RequestedPartCatalog;
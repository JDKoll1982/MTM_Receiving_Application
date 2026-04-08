-- ========================================
-- Query: SSMS Material Availability Board Field Values
-- Description: Reproduces the Material Availability Board header summary together with the
--              Incoming Material and Associated Parts section values for the same part set.
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
DECLARE @MaxResults         int          = 200;

DROP TABLE IF EXISTS #RequestedPartCatalog;
DROP TABLE IF EXISTS #SelectedDates;
DROP TABLE IF EXISTS #DistinctDateRows;
DROP TABLE IF EXISTS #FirstDisplayDate;
DROP TABLE IF EXISTS #RollupByPart;
DROP TABLE IF EXISTS #OrderedAssociatedParts;
DROP TABLE IF EXISTS #IncomingCardSummary;
DROP TABLE IF EXISTS #AssociatedCardSummary;

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
        po.ID AS PONumber,
        CONVERT(nvarchar(10), pol.LINE_NO) AS POLineNumber,
        pol.ORDER_QTY AS OrderedQty,
        pol.TOTAL_RECEIVED_QTY AS ReceivedQty,
        (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) AS RemainingQty,
        pol.DESIRED_RECV_DATE AS LineDesiredReceiveDate,
        pol.PROMISE_DATE AS LinePromiseDate,
        pol.LAST_RECEIVED_DATE AS LineLastReceivedDate,
        po.PROMISE_DATE AS HeaderPromiseDate,
        po.DESIRED_RECV_DATE AS HeaderDesiredReceiveDate,
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
        row.LineLastReceivedDate,
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
    CAST(LineLastReceivedDate AS date) AS LineLastReceivedDate,
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
        CAST(0 AS int) AS SortBucket
    FROM #SelectedDates row
    WHERE row.DisplayDate IS NOT NULL
      AND (
            row.IsBlanketOrder = 1
            OR (
                row.DisplayDate >= CAST(GETDATE() AS date)
                AND (
                    @IncomingWindowDays IS NULL
                    OR row.DisplayDate <= DATEADD(day, @IncomingWindowDays, CAST(GETDATE() AS date))
                )
            )
          )

    UNION

    SELECT DISTINCT
        row.PartId,
        row.PartDescription,
        'Last received in shipment' AS DateLabel,
        row.LineLastReceivedDate AS DisplayDate,
        CAST(1 AS int) AS SortBucket
    FROM #SelectedDates row
    WHERE row.IsBlanketOrder = 0
      AND row.LineLastReceivedDate IS NOT NULL
      AND (
            row.DisplayDate IS NULL
            OR row.DisplayDate < CAST(GETDATE() AS date)
            OR (
                @IncomingWindowDays IS NOT NULL
                AND row.DisplayDate > DATEADD(day, @IncomingWindowDays, CAST(GETDATE() AS date))
            )
          )
)
SELECT
    PartId,
    PartDescription,
    DateLabel,
    DisplayDate,
    SortBucket
INTO #DistinctDateRows
FROM DistinctDateRows;

;
WITH FirstDisplayDate AS (
    SELECT
        PartId,
        DateLabel,
        DisplayDate,
        SortBucket,
        ROW_NUMBER() OVER (
            PARTITION BY PartId
            ORDER BY
                SortBucket,
                CASE WHEN SortBucket = 0 THEN DisplayDate END,
                CASE WHEN SortBucket = 1 THEN DisplayDate END DESC,
                DateLabel
        ) AS RowNumber
    FROM #DistinctDateRows
)
SELECT
    PartId,
    DateLabel,
    DisplayDate,
    SortBucket
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
    rpc.PartDescription,
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
        WHEN f.DisplayDate IS NOT NULL AND f.SortBucket = 1 THEN CONCAT(f.DateLabel, ': ', CONVERT(varchar(10), f.DisplayDate, 101))
        WHEN f.DisplayDate IS NOT NULL THEN CONCAT('Next material date: ', f.DateLabel, ' ', CONVERT(varchar(10), f.DisplayDate, 101))
        WHEN COALESCE(r.POLineCount, 0) > 0 THEN 'Incoming material found, but no qualifying due dates are available.'
        WHEN @IncomingWindowDays IS NOT NULL THEN CONCAT('No inbound material due in the next ', @IncomingWindowDays, ' days.')
        ELSE 'No inbound material found.'
    END AS IncomingSummaryText,
    COALESCE(r.POLineCount, 0) AS POLineCount,
    COALESCE(r.PurchaseOrderCount, 0) AS PurchaseOrderCount,
    f.DisplayDate AS IncomingSortDate,
    CASE WHEN f.DisplayDate IS NOT NULL THEN 0 ELSE 2 END AS IncomingSortBucket
INTO #IncomingCardSummary
FROM #RequestedPartCatalog rpc
LEFT JOIN #RollupByPart r
    ON r.PartId = rpc.PartId
LEFT JOIN #FirstDisplayDate f
    ON f.PartId = rpc.PartId;

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
MatchingRequirements AS (
    SELECT
        r.PART_ID AS InputPartNumber,
        COALESCE(component.DESCRIPTION, '') AS InputPartDescription,
        wo.PART_ID AS AssociatedPartNumber,
        COALESCE(parent.DESCRIPTION, '') AS AssociatedPartDescription,
        r.WORKORDER_TYPE AS WorkOrderType,
        r.WORKORDER_BASE_ID AS WorkOrderBaseId,
        r.WORKORDER_LOT_ID AS WorkOrderLotId,
        r.WORKORDER_SPLIT_ID AS WorkOrderSplitId,
        r.WORKORDER_SUB_ID AS WorkOrderSubId,
        r.OPERATION_SEQ_NO AS OperationSeqNo,
        r.PIECE_NO AS RequirementPieceNo,
        ISNULL(r.STATUS, '') AS RequirementStatus,
        ISNULL(wo.STATUS, '') AS WorkOrderStatus,
        CAST(
            COALESCE(
                r.REQUIRED_DATE,
                r.DUE_DATE,
                wo.SCHED_START_DATE,
                wo.DESIRED_RLS_DATE,
                wo.DESIRED_WANT_DATE
            ) AS date
        ) AS NextDueToRunDate,
        CASE
            WHEN r.REQUIRED_DATE IS NOT NULL THEN 'REQUIREMENT.REQUIRED_DATE'
            WHEN r.DUE_DATE IS NOT NULL THEN 'REQUIREMENT.DUE_DATE'
            WHEN wo.SCHED_START_DATE IS NOT NULL THEN 'WORK_ORDER.SCHED_START_DATE'
            WHEN wo.DESIRED_RLS_DATE IS NOT NULL THEN 'WORK_ORDER.DESIRED_RLS_DATE'
            WHEN wo.DESIRED_WANT_DATE IS NOT NULL THEN 'WORK_ORDER.DESIRED_WANT_DATE'
            ELSE 'NO DATE FOUND'
        END AS NextDueDateSource,
        CASE
            WHEN COALESCE(
                r.REQUIRED_DATE,
                r.DUE_DATE,
                wo.SCHED_START_DATE,
                wo.DESIRED_RLS_DATE,
                wo.DESIRED_WANT_DATE
            ) >= CAST(GETDATE() AS date)
             AND ISNULL(r.STATUS, '') NOT IN ('C', 'X')
             AND ISNULL(wo.STATUS, '') NOT IN ('C', 'X')
                THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END AS IsFutureOrTodayRun
    FROM RequestedParts rp
    CROSS JOIN NormalizedParameters np
    INNER JOIN dbo.REQUIREMENT r
        ON r.PART_ID = rp.PartId
    INNER JOIN dbo.WORK_ORDER wo
        ON r.WORKORDER_TYPE     = wo.TYPE
       AND r.WORKORDER_BASE_ID  = wo.BASE_ID
       AND r.WORKORDER_LOT_ID   = wo.LOT_ID
       AND r.WORKORDER_SPLIT_ID = wo.SPLIT_ID
       AND r.WORKORDER_SUB_ID   = wo.SUB_ID
    LEFT JOIN dbo.PART component
        ON r.PART_ID = component.ID
    LEFT JOIN dbo.PART parent
        ON wo.PART_ID = parent.ID
        WHERE ISNULL(r.STATUS, '') <> 'X'
            AND ISNULL(wo.STATUS, '') <> 'X'
),
FilteredRuns AS (
    SELECT
        m.*
    FROM MatchingRequirements m
    CROSS JOIN NormalizedParameters np
    WHERE m.NextDueToRunDate IS NOT NULL
      AND (
            np.IncomingWindowDays IS NULL
                        OR m.IsFutureOrTodayRun = 0
            OR (
                m.IsFutureOrTodayRun = 1
                AND m.NextDueToRunDate <= DATEADD(day, np.IncomingWindowDays, CAST(GETDATE() AS date))
            )
          )
),
RankedAssociatedParts AS (
    SELECT
        f.InputPartNumber,
        f.InputPartDescription,
        f.AssociatedPartNumber,
        f.AssociatedPartDescription,
        f.NextDueToRunDate,
        f.IsFutureOrTodayRun,
        f.NextDueDateSource,
        f.WorkOrderType,
        f.WorkOrderBaseId,
        f.WorkOrderLotId,
        f.WorkOrderSplitId,
        f.WorkOrderSubId,
        f.OperationSeqNo,
        f.RequirementPieceNo,
        f.WorkOrderStatus,
        f.RequirementStatus,
        ROW_NUMBER() OVER (
            PARTITION BY f.InputPartNumber, f.AssociatedPartNumber
            ORDER BY
                CASE WHEN f.IsFutureOrTodayRun = 1 THEN 0 ELSE 1 END,
                CASE WHEN f.IsFutureOrTodayRun = 1 THEN f.NextDueToRunDate END,
                CASE WHEN f.IsFutureOrTodayRun = 0 THEN f.NextDueToRunDate END DESC,
                f.WorkOrderBaseId,
                f.OperationSeqNo,
                f.RequirementPieceNo
        ) AS RowNumberWithinAssociatedPart
    FROM FilteredRuns f
),
OrderedAssociatedParts AS (
    SELECT
        d.InputPartNumber,
        d.InputPartDescription,
        d.AssociatedPartNumber,
        d.AssociatedPartDescription,
        d.NextDueToRunDate,
        d.IsFutureOrTodayRun,
        d.NextDueDateSource,
        CONCAT(
            'WO ',
            LTRIM(RTRIM(COALESCE(d.WorkOrderType, ''))),
            ' ',
            COALESCE(d.WorkOrderBaseId, ''),
            '-',
            COALESCE(d.WorkOrderLotId, ''),
            '-',
            COALESCE(d.WorkOrderSplitId, ''),
            '-',
            COALESCE(d.WorkOrderSubId, '')
        ) AS WorkOrderDisplay,
        CASE WHEN d.IsFutureOrTodayRun = 1 THEN 'Next run' ELSE 'Latest known run' END AS RunTimingLabel,
        CONVERT(varchar(10), d.NextDueToRunDate, 101) AS NextRunDateDisplay,
        ROW_NUMBER() OVER (
            PARTITION BY d.InputPartNumber
            ORDER BY
                CASE WHEN d.IsFutureOrTodayRun = 1 THEN 0 ELSE 1 END,
                CASE WHEN d.IsFutureOrTodayRun = 1 THEN d.NextDueToRunDate END,
                CASE WHEN d.IsFutureOrTodayRun = 0 THEN d.NextDueToRunDate END DESC,
                d.AssociatedPartNumber
        ) AS RowNumberWithinInputPart
    FROM RankedAssociatedParts d
    WHERE d.RowNumberWithinAssociatedPart = 1
)
SELECT
    InputPartNumber,
    InputPartDescription,
    AssociatedPartNumber,
    AssociatedPartDescription,
    NextDueToRunDate,
    IsFutureOrTodayRun,
    NextDueDateSource,
    WorkOrderDisplay,
    RunTimingLabel,
    NextRunDateDisplay,
    RowNumberWithinInputPart
INTO #OrderedAssociatedParts
FROM OrderedAssociatedParts;

;
WITH AssociatedPartCounts AS (
    SELECT
        o.InputPartNumber,
        COUNT(*) AS AssociatedPartCount
    FROM #OrderedAssociatedParts o
    GROUP BY o.InputPartNumber
),
FirstAssociatedPartRows AS (
    SELECT
        o.InputPartNumber,
        o.AssociatedPartNumber,
        o.RunTimingLabel,
        o.NextRunDateDisplay,
        o.NextDueToRunDate,
        o.IsFutureOrTodayRun
    FROM #OrderedAssociatedParts o
    WHERE o.RowNumberWithinInputPart = 1
)
SELECT
    rpc.PartId,
    rpc.PartDescription,
    COALESCE(c.AssociatedPartCount, 0) AS AssociatedPartCount,
    CASE
        WHEN COALESCE(c.AssociatedPartCount, 0) = 0 AND @IncomingWindowDays IS NOT NULL THEN CONCAT('No associated part runs were found in the next ', @IncomingWindowDays, ' days.')
        WHEN COALESCE(c.AssociatedPartCount, 0) = 0 THEN 'No associated part runs were found.'
        ELSE CONCAT(
            f.RunTimingLabel,
            ': ',
            f.AssociatedPartNumber,
            ' ',
            f.NextRunDateDisplay
        )
    END AS AssociatedSummaryText,
    f.NextDueToRunDate AS AssociatedSortDate,
    CASE
        WHEN COALESCE(c.AssociatedPartCount, 0) = 0 THEN 2
        WHEN f.IsFutureOrTodayRun = 1 THEN 0
        ELSE 1
    END AS AssociatedSortBucket
INTO #AssociatedCardSummary
FROM #RequestedPartCatalog rpc
LEFT JOIN AssociatedPartCounts c
    ON c.InputPartNumber = rpc.PartId
LEFT JOIN FirstAssociatedPartRows f
    ON f.InputPartNumber = rpc.PartId;

SELECT
    rpc.PartId,
    rpc.PartDescription,
    CASE
        WHEN COALESCE(a.AssociatedPartCount, 0) > 0 THEN 'Associated Parts'
        ELSE 'Incoming Material'
    END AS HeaderSource,
    CASE
        WHEN COALESCE(a.AssociatedPartCount, 0) > 0 THEN a.AssociatedSummaryText
        ELSE i.IncomingSummaryText
    END AS HeaderSummaryText,
    i.IncomingSummaryText,
    i.OrderedQtyDisplay,
    i.ReceivedQtyDisplay,
    i.RemainingQtyDisplay,
    i.PurchaseOrderCountSummary,
    i.POLineCountSummary,
    i.PercentCompleteSummary,
    a.AssociatedSummaryText,
    COALESCE(a.AssociatedPartCount, 0) AS AssociatedPartCount,
    CASE
        WHEN COALESCE(a.AssociatedPartCount, 0) > 0 THEN a.AssociatedSortBucket
        ELSE i.IncomingSortBucket
    END AS HeaderSortBucket,
    CASE
        WHEN COALESCE(a.AssociatedPartCount, 0) > 0 THEN a.AssociatedSortDate
        ELSE i.IncomingSortDate
    END AS HeaderSortDate
FROM #RequestedPartCatalog rpc
LEFT JOIN #IncomingCardSummary i
    ON i.PartId = rpc.PartId
LEFT JOIN #AssociatedCardSummary a
    ON a.PartId = rpc.PartId
ORDER BY rpc.PartId;

SELECT
    d.PartId,
    d.PartDescription,
    d.DateLabel,
    CONVERT(varchar(10), d.DisplayDate, 101) AS DisplayDate,
    CONCAT(d.DateLabel, ' ', CONVERT(varchar(10), d.DisplayDate, 101)) AS DisplayText
FROM #DistinctDateRows d
ORDER BY
    d.PartId,
    d.SortBucket,
    CASE WHEN d.SortBucket = 0 THEN d.DisplayDate END,
    CASE WHEN d.SortBucket = 1 THEN d.DisplayDate END DESC,
    d.DateLabel;

SELECT TOP (@MaxResults)
    o.InputPartNumber AS PartId,
    o.InputPartDescription AS PartDescription,
    o.AssociatedPartNumber,
    o.AssociatedPartDescription,
    o.WorkOrderDisplay,
    o.RunTimingLabel,
    o.NextRunDateDisplay,
    o.NextDueDateSource,
    o.IsFutureOrTodayRun
FROM #OrderedAssociatedParts o
ORDER BY
    o.InputPartNumber,
    o.RowNumberWithinInputPart,
    o.AssociatedPartNumber;

DROP TABLE IF EXISTS #AssociatedCardSummary;
DROP TABLE IF EXISTS #IncomingCardSummary;
DROP TABLE IF EXISTS #OrderedAssociatedParts;
DROP TABLE IF EXISTS #RollupByPart;
DROP TABLE IF EXISTS #FirstDisplayDate;
DROP TABLE IF EXISTS #DistinctDateRows;
DROP TABLE IF EXISTS #SelectedDates;
DROP TABLE IF EXISTS #RequestedPartCatalog;
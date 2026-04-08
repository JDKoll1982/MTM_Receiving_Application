-- ========================================
-- Query: SSMS Associated Parts Field Values
-- Description: Reproduces the same Associated Parts field values shown on the Material Availability Board.
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
DROP TABLE IF EXISTS #OrderedAssociatedParts;

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
    WHERE (
            r.WAREHOUSE_ID = np.WarehouseCode
            OR wo.WAREHOUSE_ID = np.WarehouseCode
          )
      AND r.CLOSE_DATE IS NULL
      AND wo.CLOSE_DATE IS NULL
      AND ISNULL(r.STATUS, '') NOT IN ('C', 'X')
      AND ISNULL(wo.STATUS, '') NOT IN ('C', 'X')
),
FilteredRuns AS (
    SELECT
        m.*
    FROM MatchingRequirements m
    CROSS JOIN NormalizedParameters np
    WHERE m.NextDueToRunDate IS NOT NULL
      AND (
            np.IncomingWindowDays IS NULL
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

SELECT
    rpc.PartId,
    rpc.PartDescription,
    COUNT(o.AssociatedPartNumber) AS AssociatedPartCount,
    CASE
        WHEN COUNT(o.AssociatedPartNumber) = 0 AND @IncomingWindowDays IS NOT NULL THEN CONCAT('No associated part runs were found in the next ', @IncomingWindowDays, ' days.')
        WHEN COUNT(o.AssociatedPartNumber) = 0 THEN 'No associated part runs were found.'
        ELSE CONCAT(
            MAX(CASE WHEN o.RowNumberWithinInputPart = 1 THEN o.RunTimingLabel END),
            ': ',
            MAX(CASE WHEN o.RowNumberWithinInputPart = 1 THEN o.AssociatedPartNumber END),
            ' ',
            MAX(CASE WHEN o.RowNumberWithinInputPart = 1 THEN o.NextRunDateDisplay END)
        )
    END AS SummaryText
FROM #RequestedPartCatalog rpc
LEFT JOIN #OrderedAssociatedParts o
    ON o.InputPartNumber = rpc.PartId
GROUP BY rpc.PartId, rpc.PartDescription
ORDER BY rpc.PartId;

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

DROP TABLE IF EXISTS #OrderedAssociatedParts;
DROP TABLE IF EXISTS #RequestedPartCatalog;
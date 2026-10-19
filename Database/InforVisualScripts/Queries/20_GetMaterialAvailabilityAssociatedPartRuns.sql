-- ========================================
-- Query: Material Availability Associated Part Runs
-- Description: Returns the parent or associated work-order parts that consume the requested component part,
--              together with the scheduled run window and normalized run-state for that demand.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
-- Parameters:
--   @WarehouseCode  nvarchar  Warehouse scope, e.g. '002'
--   @LocationId     nvarchar  Optional exact warehouse location
--   @PartId         nvarchar  Optional exact component/material part ID
--   @MaxResults     int       Maximum rows to return

DECLARE @WarehouseCode nvarchar(15) = '002';
DECLARE @LocationId    nvarchar(30) = 'RECV';
DECLARE @PartId        nvarchar(30) = NULL;
DECLARE @MaxResults    int          = 200;

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
),
MatchingRequirements AS (
    SELECT
        r.PART_ID                           AS InputPartNumber,
        COALESCE(component.DESCRIPTION, '') AS InputPartDescription,
        r.PART_ID                           AS ComponentPartNumber,
        wo.PART_ID                          AS AssociatedPartNumber,
        COALESCE(parent.DESCRIPTION, '')    AS AssociatedPartDescription,
        r.WORKORDER_TYPE                    AS WorkOrderType,
        r.WORKORDER_BASE_ID                 AS WorkOrderBaseId,
        r.WORKORDER_LOT_ID                  AS WorkOrderLotId,
        r.WORKORDER_SPLIT_ID                AS WorkOrderSplitId,
        r.WORKORDER_SUB_ID                  AS WorkOrderSubId,
        wo.STATUS_EFF_DATE                  AS WorkOrderStatusEffectiveDate,
        wo.SITE_ID                          AS SiteId,
        r.OPERATION_SEQ_NO                  AS OperationSeqNo,
        r.PIECE_NO                          AS RequirementPieceNo,
        ISNULL(r.STATUS, '')                AS RequirementStatus,
        ISNULL(wo.STATUS, '')               AS WorkOrderStatus,
        r.REQUIRED_DATE                     AS RequiredDate,
        r.QTY_PER                           AS QtyPer,
        r.FIXED_QTY                         AS FixedQty,
        r.CALC_QTY                          AS CalcQty,
        r.ISSUED_QTY                        AS IssuedQty,
        r.ALLOCATED_QTY                     AS AllocatedQty,
        r.FULFILLED_QTY                     AS FulfilledQty,
        ISNULL(r.USAGE_UM, '')              AS UsageUm,
        r.SCRAP_PERCENT                     AS ScrapPercent,
        ISNULL(op.OPERATION_TYPE, '')       AS OperationType,
        ISNULL(op.RESOURCE_ID, '')          AS ResourceId,
        ISNULL(op.SERVICE_ID, '')           AS ServiceId,
        ISNULL(op.WAREHOUSE_ID, '')         AS OperationWarehouseId,
        op.RUN_QTY_PER_CYCLE                AS RunQtyPerCycle,
        op.SETUP_HRS                        AS SetupHours,
        op.RUN_HRS                          AS RunHours,
        COALESCE(op.SCHED_START_DATE, wo.SCHED_START_DATE) AS ScheduledStartDate,
        COALESCE(op.SCHED_FINISH_DATE, wo.SCHED_FINISH_DATE) AS ScheduledFinishDate,
        ISNULL(r.DIMENSIONS, '')            AS Dimensions,
        ISNULL(r.DIM_EXPRESSION, '')        AS DimensionExpression,
        r.LENGTH                            AS Length,
        r.WIDTH                             AS Width,
        r.HEIGHT                            AS Height,
        COALESCE(r.DRAWING_ID, op.DRAWING_ID, wo.DRAWING_ID, '') AS DrawingId,
        COALESCE(r.DRAWING_REV_NO, op.DRAWING_REV_NO, wo.DRAWING_REV_NO, '') AS DrawingRevision,
        -- Effective "due to run" date. Only ~7% of open requirement rows carry an operation or
        -- work-order schedule, so the requirement's own REQUIRED_DATE is the first realistic
        -- fallback; the work-order want date is the last resort.
        COALESCE(
            op.SCHED_START_DATE,
            wo.SCHED_START_DATE,
            r.REQUIRED_DATE,
            wo.DESIRED_WANT_DATE
        )                                   AS NextDueToRunDate,
        CASE
            WHEN op.SCHED_START_DATE IS NOT NULL THEN 'OPERATION.SCHED_START_DATE'
            WHEN wo.SCHED_START_DATE IS NOT NULL THEN 'WORK_ORDER.SCHED_START_DATE'
            WHEN r.REQUIRED_DATE IS NOT NULL THEN 'REQUIREMENT.REQUIRED_DATE'
            WHEN wo.DESIRED_WANT_DATE IS NOT NULL THEN 'WORK_ORDER.DESIRED_WANT_DATE'
            ELSE 'NO SCHEDULED START DATE'
        END                                 AS NextDueDateSource,
        -- Demand is open while neither the requirement nor the work order is closed or cancelled.
        CASE
            WHEN ISNULL(r.STATUS, '') NOT IN ('C', 'X')
             AND ISNULL(wo.STATUS, '') NOT IN ('C', 'X')
                THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END                                 AS IsOpenDemand,
        CASE
            WHEN COALESCE(
                op.SCHED_START_DATE,
                wo.SCHED_START_DATE
            ) >= CAST(GETDATE() AS date)
             AND ISNULL(r.STATUS, '') NOT IN ('C', 'X')
             AND ISNULL(wo.STATUS, '') NOT IN ('C', 'X')
                THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END                                 AS IsFutureOrTodayRun,
        CASE
            WHEN COALESCE(op.SCHED_START_DATE, wo.SCHED_START_DATE) <= CAST(GETDATE() AS date)
             AND COALESCE(op.SCHED_FINISH_DATE, wo.SCHED_FINISH_DATE) >= CAST(GETDATE() AS date)
             AND ISNULL(r.STATUS, '') NOT IN ('C', 'X')
             AND ISNULL(wo.STATUS, '') NOT IN ('C', 'X')
                THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END                                 AS IsCurrentlyRunning,
        -- "Past job" means the demand is closed, not merely dated in the past: an open,
        -- past-due requirement is still live demand and must not be reported as history.
        CASE
            WHEN ISNULL(r.STATUS, '') IN ('C', 'X')
             OR ISNULL(wo.STATUS, '') IN ('C', 'X')
                THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END                                 AS IsPastJob
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
    LEFT JOIN dbo.OPERATION op
        ON r.WORKORDER_TYPE     = op.WORKORDER_TYPE
       AND r.WORKORDER_BASE_ID  = op.WORKORDER_BASE_ID
       AND r.WORKORDER_LOT_ID   = op.WORKORDER_LOT_ID
       AND r.WORKORDER_SPLIT_ID = op.WORKORDER_SPLIT_ID
       AND r.WORKORDER_SUB_ID   = op.WORKORDER_SUB_ID
       AND r.OPERATION_SEQ_NO   = op.SEQUENCE_NO
    LEFT JOIN dbo.PART component
        ON r.PART_ID = component.ID
    LEFT JOIN dbo.PART parent
        ON wo.PART_ID = parent.ID
    WHERE ISNULL(r.STATUS, '') <> 'X'
      AND ISNULL(wo.STATUS, '') <> 'X'
      -- Quote work orders never represent real production demand.
      AND wo.TYPE <> 'Q'
      -- Unreleased master ('M') work orders are make-to-stock planning shells. 8025 of the 8866
      -- open requirement rows in MTMFG are these shells, 98.6% of them dated more than 90 days
      -- ago, and they otherwise outrank the real released ('W') work orders.
      AND NOT (wo.TYPE = 'M' AND ISNULL(wo.STATUS, '') = 'U')
),
-- A component part can have dozens of requirement rows per parent (one per piece, per operation,
-- per year of history). The board only ever shows one row per component/parent pair, so the best
-- row is chosen here. Without this the row budget is consumed by duplicates of the same demand and
-- parts that only have closed history come back with no row at all.
RankedRequirements AS (
    SELECT
        mr.*,
        ROW_NUMBER() OVER (
            PARTITION BY mr.InputPartNumber, mr.AssociatedPartNumber
            ORDER BY
                CASE
                    WHEN mr.IsCurrentlyRunning = 1 THEN 0
                    WHEN mr.IsPastJob = 1 THEN 5
                    WHEN mr.NextDueToRunDate IS NULL THEN 4
                    WHEN mr.NextDueToRunDate < CAST(GETDATE() AS date) THEN 1
                    ELSE 2
                END,
                CASE WHEN mr.IsPastJob = 0 THEN mr.NextDueToRunDate END,
                CASE WHEN mr.IsPastJob = 1
                     THEN COALESCE(mr.ScheduledFinishDate, mr.NextDueToRunDate)
                END DESC,
                mr.WorkOrderBaseId,
                mr.OperationSeqNo,
                mr.RequirementPieceNo
        ) AS DemandRank
    FROM MatchingRequirements mr
)
SELECT TOP (@MaxResults)
    InputPartNumber,
    InputPartDescription,
    ComponentPartNumber,
    AssociatedPartNumber,
    AssociatedPartDescription,
    NextDueToRunDate,
    IsFutureOrTodayRun,
    IsOpenDemand,
    NextDueDateSource,
    WorkOrderType,
    WorkOrderBaseId,
    WorkOrderLotId,
    WorkOrderSplitId,
    WorkOrderSubId,
    WorkOrderStatusEffectiveDate,
    SiteId,
    OperationSeqNo,
    RequirementPieceNo,
    WorkOrderStatus,
    RequirementStatus,
    ScheduledStartDate,
    ScheduledFinishDate,
    RequiredDate,
    QtyPer,
    FixedQty,
    CalcQty,
    IssuedQty,
    AllocatedQty,
    FulfilledQty,
    UsageUm,
    ScrapPercent,
    OperationType,
    ResourceId,
    ServiceId,
    OperationWarehouseId,
    RunQtyPerCycle,
    SetupHours,
    RunHours,
    Dimensions,
    DimensionExpression,
    Length,
    Width,
    Height,
    DrawingId,
    DrawingRevision,
    IsCurrentlyRunning,
    IsPastJob
FROM RankedRequirements
WHERE DemandRank = 1
ORDER BY
    CASE
        WHEN IsCurrentlyRunning = 1 THEN 0
        WHEN IsPastJob = 1 THEN 5
        WHEN NextDueToRunDate IS NULL THEN 4
        WHEN NextDueToRunDate < CAST(GETDATE() AS date) THEN 1
        WHEN IsFutureOrTodayRun = 1 THEN 2
        ELSE 3
    END,
    CASE WHEN IsCurrentlyRunning = 1 THEN ScheduledStartDate END DESC,
    CASE WHEN IsPastJob = 0 AND NextDueToRunDate IS NOT NULL THEN NextDueToRunDate END,
    CASE WHEN IsPastJob = 1 THEN COALESCE(ScheduledFinishDate, NextDueToRunDate) END DESC,
    AssociatedPartNumber,
    WorkOrderBaseId,
    OperationSeqNo,
    RequirementPieceNo;
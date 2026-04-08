-- ========================================
-- Query: Find Associated Part And Next Due Run
-- Description: Given a component/material part number, shows the parent or associated work-order part
--              and the earliest available run-related date for that demand.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Assumptions used in this script:
-- 1. The entered Part Number is a required component/material on a work order.
-- 2. The associated Part Number is the parent part on WORK_ORDER.PART_ID.
-- 3. "Next due to run" is approximated by the earliest non-null value in this order:
--      REQUIREMENT.REQUIRED_DATE
--      REQUIREMENT.DUE_DATE
--      WORK_ORDER.SCHED_START_DATE
--      WORK_ORDER.DESIRED_RLS_DATE
--      WORK_ORDER.DESIRED_WANT_DATE
-- 4. In the live data reviewed for MMC0000850, SITE_ID was 'MTM2' while WAREHOUSE_ID was '002'.
--    Do not assume SITE_ID and WAREHOUSE_ID are interchangeable.
-- 5. By default, this query keeps closed rows so you still get a useful association history
--    when no active or future run exists for the part.

DECLARE @PartNumber     nvarchar(30) = 'MMC0000658';
DECLARE @SiteId         nvarchar(15) = 'MTM2';
DECLARE @WarehouseId    nvarchar(15) = '002';
DECLARE @MaxResults     int          = 50;
DECLARE @IncludeClosed  bit          = 1;
DECLARE @OnlyFutureRuns bit          = 0;

WITH MatchingRequirements AS
(
    SELECT
        r.PART_ID                                            AS InputPartNumber,
        component.DESCRIPTION                                AS InputPartDescription,
        wo.PART_ID                                           AS AssociatedPartNumber,
        parent.DESCRIPTION                                   AS AssociatedPartDescription,
        r.WORKORDER_TYPE                                     AS WorkOrderType,
        r.WORKORDER_BASE_ID                                  AS WorkOrderBaseId,
        r.WORKORDER_LOT_ID                                   AS WorkOrderLotId,
        r.WORKORDER_SPLIT_ID                                 AS WorkOrderSplitId,
        r.WORKORDER_SUB_ID                                   AS WorkOrderSubId,
        r.OPERATION_SEQ_NO                                   AS OperationSeqNo,
        r.PIECE_NO                                           AS RequirementPieceNo,
        r.STATUS                                             AS RequirementStatus,
        wo.STATUS                                            AS WorkOrderStatus,
        r.REQUIRED_DATE                                      AS RequirementRequiredDate,
        r.DUE_DATE                                           AS RequirementDueDate,
        wo.SCHED_START_DATE                                  AS WorkOrderScheduledStartDate,
        wo.DESIRED_RLS_DATE                                  AS WorkOrderDesiredReleaseDate,
        wo.DESIRED_WANT_DATE                                 AS WorkOrderDesiredWantDate,
        COALESCE(
            r.REQUIRED_DATE,
            r.DUE_DATE,
            wo.SCHED_START_DATE,
            wo.DESIRED_RLS_DATE,
            wo.DESIRED_WANT_DATE
        )                                                    AS NextDueToRunDate,
        CASE
            WHEN r.REQUIRED_DATE IS NOT NULL THEN 'REQUIREMENT.REQUIRED_DATE'
            WHEN r.DUE_DATE IS NOT NULL THEN 'REQUIREMENT.DUE_DATE'
            WHEN wo.SCHED_START_DATE IS NOT NULL THEN 'WORK_ORDER.SCHED_START_DATE'
            WHEN wo.DESIRED_RLS_DATE IS NOT NULL THEN 'WORK_ORDER.DESIRED_RLS_DATE'
            WHEN wo.DESIRED_WANT_DATE IS NOT NULL THEN 'WORK_ORDER.DESIRED_WANT_DATE'
            ELSE 'NO DATE FOUND'
        END                                                  AS NextDueDateSource,
        r.QTY_PER                                            AS QtyPer,
        r.CALC_QTY                                           AS CalculatedRequirementQty,
        r.ISSUED_QTY                                         AS IssuedQty,
        r.ALLOCATED_QTY                                      AS AllocatedQty,
        r.FULFILLED_QTY                                      AS FulfilledQty,
        r.WAREHOUSE_ID                                       AS RequirementWarehouseId,
        r.LOCATION_ID                                        AS RequirementLocationId,
        wo.WAREHOUSE_ID                                      AS WorkOrderWarehouseId,
        r.SITE_ID                                            AS RequirementSiteId,
        wo.SITE_ID                                           AS WorkOrderSiteId,
        wo.DESIRED_QTY                                       AS WorkOrderDesiredQty,
        wo.RECEIVED_QTY                                      AS WorkOrderReceivedQty,
        wo.CLOSE_DATE                                        AS WorkOrderCloseDate,
        r.CLOSE_DATE                                         AS RequirementCloseDate,
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
        END                                                  AS IsFutureOrTodayRun
    FROM dbo.REQUIREMENT r
    INNER JOIN dbo.WORK_ORDER wo
        ON r.WORKORDER_TYPE      = wo.TYPE
       AND r.WORKORDER_BASE_ID   = wo.BASE_ID
       AND r.WORKORDER_LOT_ID    = wo.LOT_ID
       AND r.WORKORDER_SPLIT_ID  = wo.SPLIT_ID
       AND r.WORKORDER_SUB_ID    = wo.SUB_ID
    LEFT JOIN dbo.PART component
        ON r.PART_ID = component.ID
    LEFT JOIN dbo.PART parent
        ON wo.PART_ID = parent.ID
    WHERE r.PART_ID = @PartNumber
        AND (@SiteId IS NULL OR r.SITE_ID = @SiteId OR wo.SITE_ID = @SiteId)
        AND (
            @WarehouseId IS NULL
            OR r.WAREHOUSE_ID = @WarehouseId
            OR wo.WAREHOUSE_ID = @WarehouseId
            )
      AND (
            @IncludeClosed = 1
            OR (
                r.CLOSE_DATE IS NULL
                AND wo.CLOSE_DATE IS NULL
                AND ISNULL(r.STATUS, '') NOT IN ('C', 'X')
                AND ISNULL(wo.STATUS, '') NOT IN ('C', 'X')
            )
          )
            AND (
                        @OnlyFutureRuns = 0
                        OR COALESCE(
                                r.REQUIRED_DATE,
                                r.DUE_DATE,
                                wo.SCHED_START_DATE,
                                wo.DESIRED_RLS_DATE,
                                wo.DESIRED_WANT_DATE
                        ) >= CAST(GETDATE() AS date)
                    )
)
SELECT TOP (@MaxResults)
    InputPartNumber,
    InputPartDescription,
    AssociatedPartNumber,
    AssociatedPartDescription,
    NextDueToRunDate,
        IsFutureOrTodayRun,
    NextDueDateSource,
    RequirementRequiredDate,
    RequirementDueDate,
    WorkOrderScheduledStartDate,
    WorkOrderDesiredReleaseDate,
    WorkOrderDesiredWantDate,
    WorkOrderStatus,
    RequirementStatus,
        RequirementWarehouseId,
        WorkOrderWarehouseId,
        RequirementLocationId,
        RequirementSiteId,
        WorkOrderSiteId,
    WorkOrderType,
    WorkOrderBaseId,
    WorkOrderLotId,
    WorkOrderSplitId,
    WorkOrderSubId,
    OperationSeqNo,
    RequirementPieceNo,
    QtyPer,
    CalculatedRequirementQty,
    IssuedQty,
    AllocatedQty,
    FulfilledQty,
    WorkOrderDesiredQty,
    WorkOrderReceivedQty
FROM MatchingRequirements
ORDER BY
    CASE WHEN IsFutureOrTodayRun = 1 THEN 0 ELSE 1 END,
    CASE WHEN IsFutureOrTodayRun = 1 THEN NextDueToRunDate END,
    CASE WHEN IsFutureOrTodayRun = 0 THEN NextDueToRunDate END DESC,
    AssociatedPartNumber,
    WorkOrderBaseId,
    OperationSeqNo,
    RequirementPieceNo;

-- Expected Results:
-- - InputPartNumber: The part number you entered.
-- - AssociatedPartNumber: The parent/work-order part that consumes the input part.
-- - NextDueToRunDate: Best available run-related date for that associated part.
-- - IsFutureOrTodayRun: 1 when the chosen date is today or in the future; 0 when the row is historical fallback.
-- - NextDueDateSource: Which column supplied the date.
-- - RequirementWarehouseId / WorkOrderWarehouseId: Useful when site and warehouse differ in the source data.
-- - Work order key columns: The specific work order that created the requirement.
-- - Quantity columns: Current requirement and work-order progress context.
-- ========================================
-- Query: Material Availability Associated Part Runs
-- Description: Returns the parent or associated work-order parts that consume the requested component part,
--              together with the best available next due-to-run date for that demand.
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
        wo.PART_ID                          AS AssociatedPartNumber,
        COALESCE(parent.DESCRIPTION, '')    AS AssociatedPartDescription,
        r.WORKORDER_TYPE                    AS WorkOrderType,
        r.WORKORDER_BASE_ID                 AS WorkOrderBaseId,
        r.WORKORDER_LOT_ID                  AS WorkOrderLotId,
        r.WORKORDER_SPLIT_ID                AS WorkOrderSplitId,
        r.WORKORDER_SUB_ID                  AS WorkOrderSubId,
        r.OPERATION_SEQ_NO                  AS OperationSeqNo,
        r.PIECE_NO                          AS RequirementPieceNo,
        ISNULL(r.STATUS, '')                AS RequirementStatus,
        ISNULL(wo.STATUS, '')               AS WorkOrderStatus,
        COALESCE(
            r.REQUIRED_DATE,
            r.DUE_DATE,
            wo.SCHED_START_DATE,
            wo.DESIRED_RLS_DATE,
            wo.DESIRED_WANT_DATE
        )                                   AS NextDueToRunDate,
        CASE
            WHEN r.REQUIRED_DATE IS NOT NULL THEN 'REQUIREMENT.REQUIRED_DATE'
            WHEN r.DUE_DATE IS NOT NULL THEN 'REQUIREMENT.DUE_DATE'
            WHEN wo.SCHED_START_DATE IS NOT NULL THEN 'WORK_ORDER.SCHED_START_DATE'
            WHEN wo.DESIRED_RLS_DATE IS NOT NULL THEN 'WORK_ORDER.DESIRED_RLS_DATE'
            WHEN wo.DESIRED_WANT_DATE IS NOT NULL THEN 'WORK_ORDER.DESIRED_WANT_DATE'
            ELSE 'NO DATE FOUND'
        END                                 AS NextDueDateSource,
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
        END                                 AS IsFutureOrTodayRun
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
      AND ISNULL(r.STATUS, '') <> 'X'
      AND ISNULL(wo.STATUS, '') <> 'X'
)
SELECT TOP (@MaxResults)
    InputPartDescription,
    AssociatedPartNumber,
    AssociatedPartDescription,
    NextDueToRunDate,
    IsFutureOrTodayRun,
    NextDueDateSource,
    WorkOrderType,
    WorkOrderBaseId,
    WorkOrderLotId,
    WorkOrderSplitId,
    WorkOrderSubId,
    OperationSeqNo,
    RequirementPieceNo,
    WorkOrderStatus,
    RequirementStatus
FROM MatchingRequirements
ORDER BY
    CASE WHEN IsFutureOrTodayRun = 1 THEN 0 ELSE 1 END,
    CASE WHEN IsFutureOrTodayRun = 1 THEN NextDueToRunDate END,
    CASE WHEN IsFutureOrTodayRun = 0 THEN NextDueToRunDate END DESC,
    AssociatedPartNumber,
    WorkOrderBaseId,
    OperationSeqNo,
    RequirementPieceNo;
-- ========================================
-- Query: Troubleshoot Find Associated Part And Next Due Run
-- Description: Diagnostic script for cases where Find_AssociatedPart_NextDueRun.sql returns no rows.
--              Run in SSMS, then paste the Messages text and all result grids into chat.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.

SET NOCOUNT ON;

DECLARE @PartNumber nvarchar(30) = 'MMC0000658';
DECLARE @SiteId     nvarchar(15) = '002';
DECLARE @TopRows    int          = 25;

PRINT '=== Troubleshoot_Find_AssociatedPart_NextDueRun.sql ===';
PRINT 'PartNumber = ' + ISNULL(@PartNumber, '<NULL>');
PRINT 'SiteId     = ' + ISNULL(@SiteId, '<NULL>');
PRINT 'TopRows    = ' + CAST(@TopRows AS nvarchar(20));
PRINT 'Tip: In SSMS, switch to Results to Text if you want easier copy/paste into chat.';

-- 1) Confirm the part exists at all.
SELECT
    '01-PartExists'          AS SectionName,
    p.ID                     AS PartNumber,
    p.DESCRIPTION            AS PartDescription,
    p.STOCK_UM               AS StockUom,
    p.PRODUCT_CODE           AS ProductCode
FROM dbo.PART p
WHERE p.ID = @PartNumber;

-- 2) Show whether the part appears in active/live work-order requirements.
SELECT
    '02-RequirementHitCounts' AS SectionName,
    SUM(CASE WHEN r.PART_ID = @PartNumber THEN 1 ELSE 0 END) AS AnyRequirementRows,
    SUM(CASE WHEN r.PART_ID = @PartNumber AND r.SITE_ID = @SiteId THEN 1 ELSE 0 END) AS SiteScopedRequirementRows,
    SUM(CASE WHEN r.PART_ID = @PartNumber AND ISNULL(r.STATUS, '') NOT IN ('C', 'X') AND r.CLOSE_DATE IS NULL THEN 1 ELSE 0 END) AS OpenRequirementRows,
    SUM(CASE WHEN r.PART_ID = @PartNumber AND r.SITE_ID = @SiteId AND ISNULL(r.STATUS, '') NOT IN ('C', 'X') AND r.CLOSE_DATE IS NULL THEN 1 ELSE 0 END) AS OpenSiteScopedRequirementRows
FROM dbo.REQUIREMENT r
WHERE r.PART_ID = @PartNumber;

-- 3) Show raw requirement rows for the part so we can see site, status, and dates.
SELECT TOP (@TopRows)
    '03-RawRequirementRows' AS SectionName,
    r.PART_ID               AS RequiredPartNumber,
    r.STATUS                AS RequirementStatus,
    r.SITE_ID               AS RequirementSiteId,
    r.WAREHOUSE_ID          AS RequirementWarehouseId,
    r.LOCATION_ID           AS RequirementLocationId,
    r.WORKORDER_TYPE        AS WorkOrderType,
    r.WORKORDER_BASE_ID     AS WorkOrderBaseId,
    r.WORKORDER_LOT_ID      AS WorkOrderLotId,
    r.WORKORDER_SPLIT_ID    AS WorkOrderSplitId,
    r.WORKORDER_SUB_ID      AS WorkOrderSubId,
    r.OPERATION_SEQ_NO      AS OperationSeqNo,
    r.PIECE_NO              AS RequirementPieceNo,
    r.REQUIRED_DATE         AS RequirementRequiredDate,
    r.DUE_DATE              AS RequirementDueDate,
    r.CLOSE_DATE            AS RequirementCloseDate,
    r.QTY_PER               AS QtyPer,
    r.CALC_QTY              AS CalculatedRequirementQty,
    r.ISSUED_QTY            AS IssuedQty,
    r.ALLOCATED_QTY         AS AllocatedQty,
    r.FULFILLED_QTY         AS FulfilledQty
FROM dbo.REQUIREMENT r
WHERE r.PART_ID = @PartNumber
ORDER BY
    CASE WHEN r.REQUIRED_DATE IS NULL THEN 1 ELSE 0 END,
    r.REQUIRED_DATE,
    r.DUE_DATE,
    r.WORKORDER_BASE_ID,
    r.OPERATION_SEQ_NO,
    r.PIECE_NO;

-- 4) Show the joined work-order rows that the main script depends on.
SELECT TOP (@TopRows)
    '04-RequirementToWorkOrderJoin' AS SectionName,
    r.PART_ID                       AS InputPartNumber,
    component.DESCRIPTION           AS InputPartDescription,
    wo.PART_ID                      AS AssociatedPartNumber,
    parent.DESCRIPTION              AS AssociatedPartDescription,
    wo.STATUS                       AS WorkOrderStatus,
    r.STATUS                        AS RequirementStatus,
    r.SITE_ID                       AS RequirementSiteId,
    wo.SITE_ID                      AS WorkOrderSiteId,
    r.REQUIRED_DATE                 AS RequirementRequiredDate,
    r.DUE_DATE                      AS RequirementDueDate,
    wo.SCHED_START_DATE             AS WorkOrderScheduledStartDate,
    wo.DESIRED_RLS_DATE             AS WorkOrderDesiredReleaseDate,
    wo.DESIRED_WANT_DATE            AS WorkOrderDesiredWantDate,
    COALESCE(
        r.REQUIRED_DATE,
        r.DUE_DATE,
        wo.SCHED_START_DATE,
        wo.DESIRED_RLS_DATE,
        wo.DESIRED_WANT_DATE
    )                               AS NextDueToRunDate,
    wo.TYPE                         AS WorkOrderType,
    wo.BASE_ID                      AS WorkOrderBaseId,
    wo.LOT_ID                       AS WorkOrderLotId,
    wo.SPLIT_ID                     AS WorkOrderSplitId,
    wo.SUB_ID                       AS WorkOrderSubId,
    wo.CLOSE_DATE                   AS WorkOrderCloseDate,
    r.CLOSE_DATE                    AS RequirementCloseDate
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
ORDER BY
    CASE WHEN COALESCE(r.REQUIRED_DATE, r.DUE_DATE, wo.SCHED_START_DATE, wo.DESIRED_RLS_DATE, wo.DESIRED_WANT_DATE) IS NULL THEN 1 ELSE 0 END,
    COALESCE(r.REQUIRED_DATE, r.DUE_DATE, wo.SCHED_START_DATE, wo.DESIRED_RLS_DATE, wo.DESIRED_WANT_DATE),
    wo.PART_ID,
    wo.BASE_ID;

-- 5) Show whether the part is itself a direct work-order parent rather than a requirement/component.
SELECT TOP (@TopRows)
    '05-DirectWorkOrdersForInputPart' AS SectionName,
    wo.PART_ID                        AS WorkOrderPartNumber,
    parent.DESCRIPTION                AS WorkOrderPartDescription,
    wo.STATUS                         AS WorkOrderStatus,
    wo.SITE_ID                        AS WorkOrderSiteId,
    wo.WAREHOUSE_ID                   AS WorkOrderWarehouseId,
    wo.SCHED_START_DATE               AS WorkOrderScheduledStartDate,
    wo.DESIRED_RLS_DATE               AS WorkOrderDesiredReleaseDate,
    wo.DESIRED_WANT_DATE              AS WorkOrderDesiredWantDate,
    wo.TYPE                           AS WorkOrderType,
    wo.BASE_ID                        AS WorkOrderBaseId,
    wo.LOT_ID                         AS WorkOrderLotId,
    wo.SPLIT_ID                       AS WorkOrderSplitId,
    wo.SUB_ID                         AS WorkOrderSubId,
    wo.CLOSE_DATE                     AS WorkOrderCloseDate
FROM dbo.WORK_ORDER wo
LEFT JOIN dbo.PART parent
    ON wo.PART_ID = parent.ID
WHERE wo.PART_ID = @PartNumber
  AND (@SiteId IS NULL OR wo.SITE_ID = @SiteId)
ORDER BY
    CASE WHEN COALESCE(wo.SCHED_START_DATE, wo.DESIRED_RLS_DATE, wo.DESIRED_WANT_DATE) IS NULL THEN 1 ELSE 0 END,
    COALESCE(wo.SCHED_START_DATE, wo.DESIRED_RLS_DATE, wo.DESIRED_WANT_DATE),
    wo.BASE_ID;

-- 6) Check planning demand tables in case the relationship is planned but not yet released to WORK_ORDER/REQUIREMENT.
SELECT TOP (@TopRows)
    '06-PlannedMaterialRequirements' AS SectionName,
    pmr.REQUIRED_PART_ID             AS InputPartNumber,
    component.DESCRIPTION            AS InputPartDescription,
    pmr.PARENT_PART_ID               AS AssociatedPartNumber,
    parent.DESCRIPTION               AS AssociatedPartDescription,
    pmr.REQUIRED_DATE                AS NextDueToRunDate,
    pmr.REQUIRED_QTY                 AS RequiredQty,
    pmr.SITE_ID                      AS SiteId,
    pmr.WAREHOUSE_ID                 AS WarehouseId,
    pmr.PARENT_SEQ_NO                AS ParentSeqNo,
    pmr.REQ_NO                       AS RequirementNo
FROM dbo.PLANNED_MATL_REQ pmr
LEFT JOIN dbo.PART component
    ON pmr.REQUIRED_PART_ID = component.ID
LEFT JOIN dbo.PART parent
    ON pmr.PARENT_PART_ID = parent.ID
WHERE pmr.REQUIRED_PART_ID = @PartNumber
  AND (@SiteId IS NULL OR pmr.SITE_ID = @SiteId)
ORDER BY
    CASE WHEN pmr.REQUIRED_DATE IS NULL THEN 1 ELSE 0 END,
    pmr.REQUIRED_DATE,
    pmr.PARENT_PART_ID,
    pmr.PARENT_SEQ_NO,
    pmr.REQ_NO;

-- 7) Check planning BOM relationships in case the part is only associated through planning/BOM, not active work-order demand.
SELECT TOP (@TopRows)
    '07-PlanningBomRelationships' AS SectionName,
    pb.SUBORD_PART_ID             AS InputPartNumber,
    component.DESCRIPTION         AS InputPartDescription,
    pb.PARENT_PLN_PART_ID         AS AssociatedPartNumber,
    parent.DESCRIPTION            AS AssociatedPartDescription,
    pb.SUBORD_PLN_PART_ID         AS SubordinatePlanningPartId,
    pb.PERCENT_PER                AS PercentPer,
    pb.SITE_ID                    AS SiteId
FROM dbo.PLANNING_BOM pb
LEFT JOIN dbo.PART component
    ON pb.SUBORD_PART_ID = component.ID
LEFT JOIN dbo.PART parent
    ON pb.PARENT_PLN_PART_ID = parent.ID
WHERE pb.SUBORD_PART_ID = @PartNumber
  AND (@SiteId IS NULL OR pb.SITE_ID = @SiteId)
ORDER BY
    pb.PARENT_PLN_PART_ID,
    pb.SUBORD_PLN_PART_ID,
    pb.SUBORD_PART_ID;

-- 8) Check master schedule demand for the input part directly.
SELECT TOP (@TopRows)
    '08-MasterScheduleForInputPart' AS SectionName,
    ms.PART_ID                      AS ScheduledPartNumber,
    p.DESCRIPTION                   AS ScheduledPartDescription,
    ms.WANT_DATE                    AS WantDate,
    ms.ORDER_QTY                    AS OrderQty,
    ms.FIRMED                       AS Firmed,
    ms.WAREHOUSE_ID                 AS WarehouseId,
    ms.SITE_ID                      AS SiteId,
    ms.MASTER_SCHEDULE_ID           AS MasterScheduleId
FROM dbo.MASTER_SCHEDULE ms
LEFT JOIN dbo.PART p
    ON ms.PART_ID = p.ID
WHERE ms.PART_ID = @PartNumber
  AND (@SiteId IS NULL OR ms.SITE_ID = @SiteId)
ORDER BY
    ms.WANT_DATE,
    ms.MASTER_SCHEDULE_ID;

-- 9) Check planned orders for the input part directly.
SELECT TOP (@TopRows)
    '09-PlannedOrdersForInputPart' AS SectionName,
    po.PART_ID                     AS PlannedPartNumber,
    p.DESCRIPTION                  AS PlannedPartDescription,
    po.WANT_DATE                   AS WantDate,
    po.ORDER_QTY                   AS OrderQty,
    po.MAIN_PART_ID                AS MainPartId,
    mainPart.DESCRIPTION           AS MainPartDescription,
    po.SCHEDULE_ID                 AS ScheduleId,
    po.WAREHOUSE_ID                AS WarehouseId,
    po.SITE_ID                     AS SiteId,
    po.SEQ_NO                      AS SeqNo,
    po.MAIN_SEQ_NO                 AS MainSeqNo
FROM dbo.PLANNED_ORDER po
LEFT JOIN dbo.PART p
    ON po.PART_ID = p.ID
LEFT JOIN dbo.PART mainPart
    ON po.MAIN_PART_ID = mainPart.ID
WHERE po.PART_ID = @PartNumber
  AND (@SiteId IS NULL OR po.SITE_ID = @SiteId)
ORDER BY
    po.WANT_DATE,
    po.SEQ_NO;

PRINT '=== End of troubleshooting script ===';
PRINT 'Paste the output back into chat and I can tighten or replace the original query based on the actual result set.';
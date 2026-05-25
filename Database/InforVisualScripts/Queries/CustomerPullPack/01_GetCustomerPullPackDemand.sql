-- ========================================
-- Query: Customer Pull n' Pack Demand
-- Description: Returns one-customer pull-and-pack demand lines with due-date grouping context,
--              shipped/open quantity math, and finished-goods location rollup from Infor Visual.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
-- NOTE: `CUST_BOOK_DEL` is a very large table, so this query stays tightly filtered by customer,
--       site/warehouse, date range, and optional part/location parameters.
-- NOTE: This query returns the core demand projection. Detailed waitlist state remains an MTM-managed
--       overlay, and location-row expansion for the report surface is handled separately.
-- Parameters:
--   @CustomerId       nvarchar  Required Infor Visual customer ID.
--   @DateFrom         datetime  Inclusive lower bound for pull date.
--   @DateTo           datetime  Inclusive upper bound for pull date.
--   @SiteId           nvarchar  Site scope, defaults to '002'.
--   @WarehouseCode    nvarchar  Warehouse scope for CR_PART_LOCATION, defaults to '002'.
--   @PartId           nvarchar  Optional exact parent part filter.
--   @LocationId       nvarchar  Optional exact FG location filter.
--   @ShortagesOnly    bit       Optional shortage-only filter.
--   @LateOrdersOnly   bit       Optional late-order-only filter.
--   @UnpulledOnly     bit       Optional open-demand-only filter.
--   @MaxResults       int       Maximum number of rows to return.

DECLARE @CustomerId     nvarchar(15) = 'VOLVO';
DECLARE @DateFrom       datetime     = '2026-05-01';
DECLARE @DateTo         datetime     = '2026-05-31';
DECLARE @SiteId         nvarchar(15) = '002';
DECLARE @WarehouseCode  nvarchar(15) = '002';
DECLARE @PartId         nvarchar(30) = NULL;
DECLARE @LocationId     nvarchar(15) = NULL;
DECLARE @ShortagesOnly  bit          = 0;
DECLARE @LateOrdersOnly bit          = 0;
DECLARE @UnpulledOnly   bit          = 1;
DECLARE @MaxResults     int          = 500;

;WITH NormalizedParameters AS
(
    SELECT
        UPPER(LTRIM(RTRIM(@CustomerId)))    AS CustomerId,
        @DateFrom                           AS DateFrom,
        @DateTo                             AS DateTo,
        UPPER(LTRIM(RTRIM(@SiteId)))        AS SiteId,
        UPPER(LTRIM(RTRIM(@WarehouseCode))) AS WarehouseCode,
        NULLIF(UPPER(LTRIM(RTRIM(@PartId))), '')     AS PartId,
        NULLIF(UPPER(LTRIM(RTRIM(@LocationId))), '') AS LocationId,
        CAST(COALESCE(@ShortagesOnly, 0) AS bit)     AS ShortagesOnly,
        CAST(COALESCE(@LateOrdersOnly, 0) AS bit)    AS LateOrdersOnly,
        CAST(COALESCE(@UnpulledOnly, 1) AS bit)      AS UnpulledOnly,
        CASE
            WHEN COALESCE(@MaxResults, 0) <= 0 THEN 500
            ELSE @MaxResults
        END AS MaxResults
),
LocationRollup AS
(
    SELECT
        cpl.ID                              AS PartId,
        SUM(COALESCE(cpl.QTY, 0))           AS FgOnHandQuantity,
        COUNT(*)                            AS PositiveLocationCount
    FROM dbo.CR_PART_LOCATION cpl
    CROSS JOIN NormalizedParameters np
    WHERE cpl.WAREHOUSE_ID = np.WarehouseCode
      AND COALESCE(cpl.QTY, 0) > 0
    GROUP BY
        cpl.ID
),
PrimaryLocation AS
(
    SELECT
        cpl.ID                              AS PartId,
        cpl.LOCATION_ID                     AS FgLocationId,
        COALESCE(cpl.QTY, 0)                AS FgLocationQuantity,
        ROW_NUMBER() OVER
        (
            PARTITION BY cpl.ID
            ORDER BY COALESCE(cpl.QTY, 0) DESC, cpl.LOCATION_ID ASC
        ) AS LocationRank
    FROM dbo.CR_PART_LOCATION cpl
    CROSS JOIN NormalizedParameters np
    WHERE cpl.WAREHOUSE_ID = np.WarehouseCode
      AND COALESCE(cpl.QTY, 0) > 0
),
CandidateDemand AS
(
    SELECT
        CONCAT(
            co.ID,
            N'|',
            CONVERT(nvarchar(10), col.LINE_NO),
            N'|',
            CONVERT(nvarchar(10), cbd.DEL_SCHED_LINE_NO),
            N'|',
            CONVERT(nvarchar(10), CAST(cbd.BOOK_DATE AS date), 23)
        )                                                   AS SourceLineKey,
        co.CUSTOMER_ID                                      AS CustomerId,
        cust.NAME                                           AS CustomerName,
        co.ID                                               AS CustomerOrderId,
        col.LINE_NO                                         AS CustomerOrderLineNo,
        cbd.DEL_SCHED_LINE_NO                               AS DeliveryScheduleLineNo,
        col.PART_ID                                         AS ParentPartId,
        p.DESCRIPTION                                       AS ParentPartDescription,
        COALESCE(pl.FgLocationId, cbd.WAREHOUSE_ID, col.WAREHOUSE_ID, co.WAREHOUSE_ID, N'')
                                                            AS SourceLocationId,
        CAST(COALESCE(NULLIF(cbd.USER_ORDER_QTY, 0), cbd.ORDER_QTY, NULLIF(col.USER_ORDER_QTY, 0), col.ORDER_QTY, 0) AS decimal(20, 8))
                                                            AS ShipQuantity,
        CAST(COALESCE(cbd.USER_SHIPPED_QTY, cbd.SHIPPED_QTY, 0) AS decimal(20, 8))
                                                            AS ShippedQuantity,
        CAST(
            COALESCE(NULLIF(cbd.USER_ORDER_QTY, 0), cbd.ORDER_QTY, NULLIF(col.USER_ORDER_QTY, 0), col.ORDER_QTY, 0)
            - COALESCE(cbd.USER_SHIPPED_QTY, cbd.SHIPPED_QTY, 0)
            AS decimal(20, 8)
        )                                                   AS RemainingQuantity,
        COALESCE(cbd.DESIRED_SHIP_DATE, cbd.BOOK_DATE, col.PROMISE_DEL_DATE, col.PROMISE_DATE, co.PROMISE_DEL_DATE, co.PROMISE_DATE, co.DESIRED_SHIP_DATE, co.ORDER_DATE)
                                                            AS PullDate,
        COALESCE(lr.FgOnHandQuantity, 0)                    AS FgOnHandQuantity,
        COALESCE(pl.FgLocationId, N'')                      AS FgLocationId,
        COALESCE(lr.PositiveLocationCount, 0)               AS PositiveLocationCount
    FROM dbo.CUSTOMER_ORDER co
    INNER JOIN dbo.CUST_ORDER_LINE col
        ON col.CUST_ORDER_ID = co.ID
    INNER JOIN dbo.CUST_BOOK_DEL cbd
        ON cbd.CUST_ORDER_ID = col.CUST_ORDER_ID
       AND cbd.CUST_ORDER_LINE_NO = col.LINE_NO
    INNER JOIN dbo.CUSTOMER cust
        ON cust.ID = co.CUSTOMER_ID
    LEFT JOIN dbo.PART p
        ON p.ID = col.PART_ID
    LEFT JOIN LocationRollup lr
        ON lr.PartId = col.PART_ID
    LEFT JOIN PrimaryLocation pl
        ON pl.PartId = col.PART_ID
       AND pl.LocationRank = 1
    CROSS JOIN NormalizedParameters np
    WHERE co.CUSTOMER_ID = np.CustomerId
      AND co.SITE_ID = np.SiteId
      AND col.SITE_ID = np.SiteId
      AND col.PART_ID IS NOT NULL
      AND COALESCE(cbd.DESIRED_SHIP_DATE, cbd.BOOK_DATE, col.PROMISE_DEL_DATE, col.PROMISE_DATE, co.PROMISE_DEL_DATE, co.PROMISE_DATE, co.DESIRED_SHIP_DATE, co.ORDER_DATE) >= np.DateFrom
      AND COALESCE(cbd.DESIRED_SHIP_DATE, cbd.BOOK_DATE, col.PROMISE_DEL_DATE, col.PROMISE_DATE, co.PROMISE_DEL_DATE, co.PROMISE_DATE, co.DESIRED_SHIP_DATE, co.ORDER_DATE) < DATEADD(day, 1, np.DateTo)
      AND (np.PartId IS NULL OR col.PART_ID = np.PartId)
      AND (
            np.LocationId IS NULL
            OR EXISTS
            (
                SELECT 1
                FROM dbo.CR_PART_LOCATION cpl
                WHERE cpl.ID = col.PART_ID
                  AND cpl.WAREHOUSE_ID = np.WarehouseCode
                  AND cpl.LOCATION_ID = np.LocationId
                  AND COALESCE(cpl.QTY, 0) > 0
            )
          )
),
EnrichedDemand AS
(
    SELECT
        cd.SourceLineKey                                                      AS SourceLineKey,
        cd.CustomerId                                                         AS CustomerId,
        COALESCE(cd.CustomerName, N'')                                        AS CustomerName,
        cd.CustomerOrderId                                                    AS CustomerOrderId,
        cd.CustomerOrderLineNo                                                AS CustomerOrderLineNo,
        cd.DeliveryScheduleLineNo                                             AS DeliveryScheduleLineNo,
        cd.ParentPartId                                                       AS ParentPartId,
        COALESCE(cd.ParentPartDescription, N'')                               AS ParentPartDescription,
        cd.SourceLocationId                                                   AS SourceLocationId,
        cd.ShipQuantity                                                       AS ShipQuantity,
        cd.PullDate                                                           AS PullDate,
        SUM(cd.ShipQuantity) OVER (PARTITION BY cd.CustomerId, cd.ParentPartId)
                                                                            AS QtyToPack,
        cd.FgOnHandQuantity                                                   AS FgOnHandQuantity,
        cd.FgLocationId                                                       AS FgLocationId,
        cd.PositiveLocationCount                                              AS PositiveLocationCount,
        CONCAT(
            CONVERT(nvarchar(20), cd.PositiveLocationCount),
            N' FG locations / ',
            CONVERT(nvarchar(40), CAST(cd.FgOnHandQuantity AS decimal(20, 2))),
            N' on hand'
        )                                                                     AS SubPartAvailabilitySummary,
        CASE
            WHEN cd.PullDate < CAST(GETDATE() AS date) THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END                                                                   AS LateOrderFlag,
        CASE
            WHEN cd.FgOnHandQuantity < SUM(cd.ShipQuantity) OVER (PARTITION BY cd.CustomerId, cd.ParentPartId)
                THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END                                                                   AS ShortageFlag,
        CASE
            WHEN cd.RemainingQuantity <= 0 THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END                                                                   AS PulledFlag,
        cd.RemainingQuantity                                                  AS RemainingQuantity,
        cd.ShippedQuantity                                                    AS ShippedQuantity,
        CAST(0 AS bit)                                                        AS HasLinkedWaitlist,
        N''                                                                   AS LinkedWaitlistId,
        N''                                                                   AS LinkedWaitlistStatus
    FROM CandidateDemand cd
)
SELECT TOP (@MaxResults)
    ed.SourceLineKey                                                          AS SourceLineKey,
    ed.CustomerId                                                             AS CustomerId,
    ed.CustomerName                                                           AS CustomerName,
    ed.CustomerOrderId                                                        AS CustomerOrderId,
    ed.CustomerOrderLineNo                                                    AS CustomerOrderLineNo,
    ed.DeliveryScheduleLineNo                                                 AS DeliveryScheduleLineNo,
    ed.ParentPartId                                                           AS ParentPartId,
    ed.ParentPartDescription                                                  AS ParentPartDescription,
    ed.SourceLocationId                                                       AS SourceLocationId,
    ed.ShipQuantity                                                           AS ShipQuantity,
    ed.PullDate                                                               AS PullDate,
    ed.QtyToPack                                                              AS QtyToPack,
    ed.FgOnHandQuantity                                                       AS FgOnHandQuantity,
    ed.FgLocationId                                                           AS FgLocationId,
    ed.PositiveLocationCount                                                  AS PositiveLocationCount,
    ed.SubPartAvailabilitySummary                                             AS SubPartAvailabilitySummary,
    ed.LateOrderFlag                                                          AS LateOrderFlag,
    ed.ShortageFlag                                                           AS ShortageFlag,
    ed.PulledFlag                                                             AS PulledFlag,
    ed.RemainingQuantity                                                      AS RemainingQuantity,
    ed.ShippedQuantity                                                        AS ShippedQuantity,
    ed.HasLinkedWaitlist                                                      AS HasLinkedWaitlist,
    ed.LinkedWaitlistId                                                       AS LinkedWaitlistId,
    ed.LinkedWaitlistStatus                                                   AS LinkedWaitlistStatus
FROM EnrichedDemand ed
CROSS JOIN NormalizedParameters np
WHERE (np.UnpulledOnly = 0 OR ed.RemainingQuantity > 0)
  AND (
        np.LateOrdersOnly = 0
        OR ed.LateOrderFlag = 1
      )
  AND (
        np.ShortagesOnly = 0
        OR ed.ShortageFlag = 1
      )
ORDER BY
    ed.PullDate,
    ed.CustomerOrderId,
    ed.ParentPartId,
    ed.CustomerOrderLineNo,
    ed.DeliveryScheduleLineNo;
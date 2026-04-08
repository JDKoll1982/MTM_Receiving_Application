-- ========================================
-- Query: Material Availability Incoming Supply
-- Description: Returns active incoming PO-line rows for either one exact part or all parts currently found in a requested warehouse location.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
-- Parameters:
--   @WarehouseCode  nvarchar  Warehouse scope, e.g. '002'
--   @LocationId     nvarchar  Optional exact warehouse location
--   @PartId         nvarchar  Optional exact part ID

DECLARE @WarehouseCode nvarchar(15) = '002';
DECLARE @LocationId    nvarchar(30) = 'RECV';
DECLARE @PartId        nvarchar(30) = NULL;

;WITH NormalizedParameters AS (
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
        WHEN po.FREE_ON_BOARD LIKE '%BLANKET%' THEN CAST(1 AS bit)
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
WHERE po.STATUS IN ('O', 'P', 'R', 'F')
  AND (
        pol.WAREHOUSE_ID = np.WarehouseCode
        OR (
            NULLIF(LTRIM(RTRIM(pol.WAREHOUSE_ID)), '') IS NULL
            AND po.WAREHOUSE_ID = np.WarehouseCode
        )
      )
  AND (
        (po.FREE_ON_BOARD LIKE '%BLANKET%' AND pol.LAST_RECEIVED_DATE IS NOT NULL)
        OR (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) > 0
      )
ORDER BY
    rp.PartId,
    po.ID,
    pol.LINE_NO;
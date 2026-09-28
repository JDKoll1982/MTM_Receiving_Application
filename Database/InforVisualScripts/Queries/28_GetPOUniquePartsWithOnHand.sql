-- ========================================
-- Query: Get PO Unique Parts with On Hand
-- Description: Returns the PO header plus one row per UNIQUE part number on
--              the PO, with the total on-hand quantity and current location
--              (single location, 'Multiple Locations', or blank), scoped to
--              the PO's site.
-- Database: MTMFG (Infor Visual) -- READ ONLY
-- Server: VISUAL
-- Parameters:
--   @PoNumber  nvarchar  Purchase order number (e.g. '500001')
-- ========================================
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.

DECLARE @PoNumber nvarchar(50) = '500001';

;WITH POMerge AS (
    SELECT
        po.ID                   AS PoNumber,
        po.STATUS               AS PoStatus,
        po.VENDOR_ID            AS VendorCode,
        po.PROMISE_DATE         AS HeaderPromiseDate,
        po.DESIRED_RECV_DATE    AS HeaderDesiredRecvDate,
        po.FREE_ON_BOARD        AS FreeOnBoard,
        -- Inventory is tracked per WAREHOUSE in CR_PART_LOCATION. The PO header
        -- SITE_ID is a site code (e.g. 'MTM2') and is NOT the stock warehouse,
        -- so use the PO's receiving warehouse, defaulting to the app's '002'.
        COALESCE(NULLIF(po.WAREHOUSE_ID, ''), '002') AS WarehouseId
    FROM dbo.PURCHASE_ORDER po
    WHERE po.ID = @PoNumber
),
POUniqueParts AS (
    SELECT
        pol.PART_ID AS PartNumber,
        pm.WarehouseId,
        MIN(pol.LINE_NO) AS FirstLineNumber
    FROM dbo.PURC_ORDER_LINE pol
    INNER JOIN POMerge pm ON pm.PoNumber = pol.PURC_ORDER_ID
    WHERE NULLIF(LTRIM(RTRIM(pol.PART_ID)), '') IS NOT NULL
    GROUP BY pol.PART_ID, pm.WarehouseId
)
SELECT
    pm.PoNumber,
    pm.PoStatus,
    pm.VendorCode,
    v.NAME                       AS VendorName,
    pm.HeaderPromiseDate,
    pm.HeaderDesiredRecvDate,
    pm.FreeOnBoard,
    pup.PartNumber,
    pup.FirstLineNumber,
    COALESCE(p.DESCRIPTION, '')  AS PartDescription,
    CAST(ROUND(COALESCE(SUM(COALESCE(pl.QTY, 0)), 0), 2) AS decimal(18, 2)) AS OnHandQty,
    CASE
        WHEN COUNT(DISTINCT
                CASE WHEN pl.QTY > 0
                     AND NULLIF(LTRIM(RTRIM(pl.LOCATION_ID)), '') IS NOT NULL
                     THEN pl.LOCATION_ID END) = 1
            THEN MAX(CASE WHEN pl.QTY > 0
                          AND NULLIF(LTRIM(RTRIM(pl.LOCATION_ID)), '') IS NOT NULL
                          THEN pl.LOCATION_ID END)
        WHEN COUNT(DISTINCT
                CASE WHEN pl.QTY > 0
                     AND NULLIF(LTRIM(RTRIM(pl.LOCATION_ID)), '') IS NOT NULL
                     THEN pl.LOCATION_ID END) > 1
            THEN 'Multiple Locations'
        ELSE ''
    END AS Location
FROM POUniqueParts pup
INNER JOIN POMerge pm ON 1 = 1
INNER JOIN dbo.PART p ON p.ID = pup.PartNumber
LEFT JOIN dbo.VENDOR v ON v.ID = pm.VendorCode
LEFT JOIN dbo.CR_PART_LOCATION pl
    ON pl.ID = pup.PartNumber
   AND pl.WAREHOUSE_ID = pup.WarehouseId
GROUP BY
    pm.PoNumber,
    pm.PoStatus,
    pm.VendorCode,
    v.NAME,
    pm.HeaderPromiseDate,
    pm.HeaderDesiredRecvDate,
    pm.FreeOnBoard,
    pup.PartNumber,
    pup.FirstLineNumber,
    p.DESCRIPTION
ORDER BY pup.PartNumber;

-- Expected Results:
-- - PoNumber / PoStatus / VendorName / HeaderPromiseDate / HeaderDesiredRecvDate / FreeOnBoard: PO header
-- - PartNumber: Unique part ID on the PO
-- - PartDescription: Part description
-- - OnHandQty: Total on-hand across all locations in the PO's site
-- - Location: Single location when only one location holds stock, 'Multiple Locations' otherwise

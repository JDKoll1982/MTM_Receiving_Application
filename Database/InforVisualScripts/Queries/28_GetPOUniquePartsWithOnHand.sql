-- ========================================
-- Query: Get PO Unique Parts with On Hand
-- Description: Returns the PO header plus one row per UNIQUE part number on
--              the PO, with the total on-hand quantity and current location
--              (single location, 'Multiple Locations', or blank), scoped to
--              the PO's site.
-- Database: MTMFG (Infor Visual) -- READ ONLY
-- Server: VISUAL
-- IMPORTANT: this is T-SQL for Microsoft SQL Server. Do NOT run it in the MySQL client
-- (cweijan.vscode-mysql-client2) - "DECLARE @PoNumber" is SQL Server syntax and MySQL rejects it.
-- Run it in SSMS / the SQL Server extension, or let Dao_InforVisualConnection run it.
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
        MIN(pol.LINE_NO) AS FirstLineNumber,
        -- What is still owed on this part across every line of the PO. Several lines can carry
        -- the same part, so the lines are summed rather than read one at a time.
        CAST(ROUND(COALESCE(SUM(COALESCE(pol.ORDER_QTY, 0) - COALESCE(pol.TOTAL_RECEIVED_QTY, 0)), 0), 2) AS decimal(18, 2)) AS RemainingQty,
        -- When each part was last actually received, which is what the part page shows and sorts on.
        -- The ERP stamps this per line as receivers are posted, so it follows the real receipt rather
        -- than the PO's expected date. Several lines can carry the same part, so the latest wins.
        MAX(pol.LAST_RECEIVED_DATE) AS LastReceivedDate
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
    pup.RemainingQty,
    pup.LastReceivedDate,
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
    pup.RemainingQty,
    pup.LastReceivedDate,
    p.DESCRIPTION
ORDER BY pup.PartNumber;

-- Expected Results:
-- - PoNumber / PoStatus / VendorName / HeaderPromiseDate / HeaderDesiredRecvDate / FreeOnBoard: PO header
-- - PartNumber: Unique part ID on the PO
-- - PartDescription: Part description
-- - RemainingQty: Ordered minus received, summed across every line that carries this part
-- - ReceiveDate: Latest line expected receive date for this part (line desired, line promise, then
--   PO header), the same fallback chain query 01 uses
-- - LastReceivedDate: When this part was last actually received (max of the PO lines carrying it).
--   Null when no line on the PO has been received yet.
-- - OnHandQty: Total on-hand across all locations in the PO's site
-- - Location: Single location when only one location holds stock, 'Multiple Locations' otherwise

-- 33_GetNegativeOnHandLocations.sql
-- Finds every location holding a negative on-hand balance, shop-wide.
-- A location cannot physically hold a negative quantity, so each row here is stock that
-- physically exists somewhere but has already been consumed on the books. Use it to size the
-- total exposure once the named part is understood, and to spot the pattern repeating
-- across other parts.
--
-- Result sets, in order:
--   1  Every negative part/location row, worst first
--   2  Summary by location - where the negatives are concentrated
--
-- Analysis notes:
--   * Negatives concentrated in staging locations (S-...) or the work centre (WC) mean
--     consumption is being posted there without stock ever being transferred in.
--     Racks (V-...) going negative is rare and worth a separate look.
--   * Compare the total here against the reported whole-shop variance. If the negatives alone
--     exceed the reported variance, something elsewhere must be overstating the books.
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @WarehouseId  nvarchar  Warehouse to scan; leave empty to scan all warehouses

DECLARE @WarehouseId nvarchar(15) = '002';

PRINT '1 - NEGATIVE ON-HAND BY PART AND LOCATION';
SELECT
    cpl.WAREHOUSE_ID            AS WarehouseId,
    cpl.LOCATION_ID             AS LocationId,
    cpl.ID                      AS PartId,
    cpl.QTY                     AS OnHand,
    p.DESCRIPTION               AS Description,
    p.STOCK_UM                  AS StockUnit,
    cpl.LAST_COUNT_DATE         AS LastCountDate
FROM dbo.CR_PART_LOCATION cpl
LEFT JOIN dbo.PART p
     ON p.ID = cpl.ID
WHERE cpl.QTY < 0
  AND (cpl.WAREHOUSE_ID = @WarehouseId OR @WarehouseId = '')
ORDER BY cpl.QTY;

PRINT '2 - SUMMARY BY LOCATION';
SELECT
    cpl.WAREHOUSE_ID            AS WarehouseId,
    cpl.LOCATION_ID             AS LocationId,
    COUNT(*)                    AS PartCount,
    SUM(cpl.QTY)                AS TotalNegativeQty
FROM dbo.CR_PART_LOCATION cpl
WHERE cpl.QTY < 0
  AND (cpl.WAREHOUSE_ID = @WarehouseId OR @WarehouseId = '')
GROUP BY cpl.WAREHOUSE_ID, cpl.LOCATION_ID
ORDER BY SUM(cpl.QTY);

PRINT '3 - TOTAL EXPOSURE (sum of all negatives in scope)';
SELECT
    COUNT(*)                    AS NegativeRows,
    SUM(cpl.QTY)                AS TotalNegativeQty
FROM dbo.CR_PART_LOCATION cpl
WHERE cpl.QTY < 0
  AND (cpl.WAREHOUSE_ID = @WarehouseId OR @WarehouseId = '');

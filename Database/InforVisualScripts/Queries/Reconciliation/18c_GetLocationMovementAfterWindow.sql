-- 18c_GetLocationMovementAfterWindow.sql
-- Returns the balance input the source-residual check needs for one part: the
-- current on-hand at each location, and the signed net movement at that location
-- after the end of the strict validation window.
--
-- The residual is compared against an as-of balance, reconstructed as
--   live on-hand  minus  signed net movement after the window
-- because CR_PART_LOCATION is a live snapshot. For a same-day run the post-window
-- delta is empty and the as-of balance is exactly the live on-hand figure the spec
-- names; for a historical day the delta restores that day's balance (FR-023,
-- decision D-02, finding R-4).
--
-- Signed net movement is +QTY for an inbound leg (TYPE = 'I') and -QTY for an
-- outbound leg (TYPE = 'O'). Direction comes from TYPE, never from the sign of
-- the stored quantity (FR-030, R-DATA-09).
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes (R-SQL-07).
--
-- Parameters:
--   @PartId     nvarchar  Exact part ID
--   @AfterDate  datetime  Start of the first day after the strict window

DECLARE @PartId    nvarchar(50) = 'MMC0001000';
DECLARE @AfterDate datetime     = '2026-11-13T00:00:00';

;
WITH LocationKeys AS (
    SELECT
        pl.WAREHOUSE_ID AS WarehouseId,
        pl.LOCATION_ID AS LocationId
    FROM dbo.PART_LOCATION pl
    WHERE pl.PART_ID = @PartId
      AND NULLIF(LTRIM(RTRIM(pl.LOCATION_ID)), '') IS NOT NULL

    UNION

    SELECT
        it.WAREHOUSE_ID AS WarehouseId,
        it.LOCATION_ID AS LocationId
    FROM dbo.INVENTORY_TRANS it
    WHERE it.PART_ID = @PartId
      AND it.TRANSACTION_DATE >= @AfterDate
      AND NULLIF(LTRIM(RTRIM(it.LOCATION_ID)), '') IS NOT NULL
),
PostWindowMovement AS (
    SELECT
        it.WAREHOUSE_ID AS WarehouseId,
        it.LOCATION_ID AS LocationId,
        SUM(CASE WHEN it.TYPE = 'I' THEN it.QTY ELSE -it.QTY END) AS NetMovement
    FROM dbo.INVENTORY_TRANS it
    WHERE it.PART_ID = @PartId
      AND it.TRANSACTION_DATE >= @AfterDate
    GROUP BY it.WAREHOUSE_ID, it.LOCATION_ID
)
SELECT
    @PartId AS PartId,
    COALESCE(k.WarehouseId, '') AS WarehouseId,
    COALESCE(k.LocationId, '') AS LocationId,
    ISNULL(pl.QTY, 0) AS OnHand,
    ISNULL(pm.NetMovement, 0) AS MovementAfterWindow
FROM LocationKeys k
LEFT JOIN dbo.PART_LOCATION pl
    ON pl.PART_ID = @PartId
   AND pl.WAREHOUSE_ID = k.WarehouseId
   AND pl.LOCATION_ID = k.LocationId
LEFT JOIN PostWindowMovement pm
    ON pm.WarehouseId = k.WarehouseId
   AND pm.LocationId = k.LocationId
ORDER BY k.LocationId ASC;

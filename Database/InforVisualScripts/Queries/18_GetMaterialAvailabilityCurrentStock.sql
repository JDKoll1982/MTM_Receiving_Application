-- ========================================
-- Query: Material Availability Current Stock
-- Description: Returns current positive-quantity locations for either one exact part or all parts currently found in a requested warehouse location.
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
)
SELECT
    rp.PartId AS PartId,
    COALESCE(p.DESCRIPTION, '') AS PartDescription,
    cpl.WAREHOUSE_ID AS WarehouseCode,
    cpl.LOCATION_ID AS LocationId,
    COALESCE(cpl.QTY, 0) AS Quantity,
    COALESCE(cpl.COMMITTED_QTY, 0) AS CommittedQuantity
FROM RequestedParts rp
CROSS JOIN NormalizedParameters np
INNER JOIN dbo.PART p
    ON p.ID = rp.PartId
INNER JOIN dbo.CR_PART_LOCATION cpl
    ON cpl.ID = rp.PartId
   AND cpl.WAREHOUSE_ID = np.WarehouseCode
   AND (np.LocationId IS NULL OR cpl.LOCATION_ID = np.LocationId)
   AND NULLIF(LTRIM(RTRIM(cpl.LOCATION_ID)), '') IS NOT NULL
   AND COALESCE(cpl.QTY, 0) > 0
ORDER BY
    rp.PartId,
    CASE
        WHEN np.LocationId IS NOT NULL AND cpl.LOCATION_ID = np.LocationId THEN 0
        ELSE 1
    END,
    cpl.LOCATION_ID;
-- ========================================
-- Query: Locations In Range
-- Description: Returns every existing warehouse location whose ID falls between an inclusive
--              start and end location, scoped to one warehouse and ordered by location ID.
--              Used by the Scanner Workbench and the ShipRec Tools Material Availability Board
--              so a typed location range can be expanded into the real locations that exist in
--              Infor Visual before stock is queried. Only existing locations are returned, so a
--              range spanning gaps in the warehouse map never produces phantom locations.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- ========================================
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @LocationIdStart nvarchar  Inclusive lower bound location ID, e.g. 'V-A0-01'
--   @LocationIdEnd   nvarchar  Inclusive upper bound location ID, e.g. 'V-A0-05'
--   @WarehouseCode   nvarchar  Warehouse to restrict results, e.g. '002'
--   @MaxResults      int       Row cap so an accidentally huge range cannot flood the UI
--
-- Notes:
--   Location IDs are zero-padded (V-A0-01), so a lexicographic BETWEEN matches the physical
--   walking order of the warehouse racks and bins. Empty or NULL IDs fall outside any range
--   bound, so no extra filtering is needed on the indexed ID column.

DECLARE @LocationIdStart nvarchar(30) = 'V-A0-01';
DECLARE @LocationIdEnd   nvarchar(30) = 'V-A0-05';
DECLARE @WarehouseCode   nvarchar(10) = '002';
DECLARE @MaxResults      int          = 60;

SELECT TOP (@MaxResults)
    l.ID           AS LocationId,
    l.WAREHOUSE_ID AS WarehouseCode,
    COALESCE(l.DESCRIPTION, '') AS Description
FROM
    dbo.LOCATION l
WHERE
    l.WAREHOUSE_ID = @WarehouseCode
    AND l.ID BETWEEN @LocationIdStart AND @LocationIdEnd
ORDER BY
    l.ID;

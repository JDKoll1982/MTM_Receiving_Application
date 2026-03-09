-- 11_FuzzySearchLocationsByWarehouse.sql
-- Fuzzy search for warehouse locations whose ID contains the given term,
-- scoped to a specific warehouse code.
-- Returns up to @MaxResults rows ordered by location ID.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @Term           nvarchar  Wildcard-wrapped search term, e.g. '%A-01%'
--   @WarehouseCode  nvarchar  Warehouse code to restrict results, e.g. '002'
--   @MaxResults     int       Maximum rows to return (default: 50 in C# caller)

DECLARE @Term          nvarchar(60) = '%A-01%';
DECLARE @WarehouseCode nvarchar(10) = '002';
DECLARE @MaxResults    int          = 50;

SELECT TOP (@MaxResults)
    l.ID            AS LocationId,
    l.WAREHOUSE_ID  AS WarehouseCode,
    l.DESCRIPTION   AS Description
FROM
    dbo.LOCATION l
WHERE
    l.WAREHOUSE_ID = @WarehouseCode
    AND l.ID LIKE @Term
ORDER BY
    l.ID;

-- 13_ValidateLocationExists.sql
-- Validates whether a warehouse location exists in Infor Visual by exact
-- location ID and warehouse code.
-- Returns a single count value that the C# DAO maps to bool.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @LocationId     nvarchar  Exact location ID to validate, e.g. 'A-01'
--   @WarehouseCode  nvarchar  Warehouse code to match, e.g. '002'

DECLARE @LocationId    nvarchar(30) = 'A-01';
DECLARE @WarehouseCode nvarchar(10) = '002';

SELECT COUNT(*) AS LocationExists
FROM dbo.LOCATION
WHERE ID = @LocationId
  AND WAREHOUSE_ID = @WarehouseCode;
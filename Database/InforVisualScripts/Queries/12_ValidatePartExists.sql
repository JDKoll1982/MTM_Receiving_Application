-- 12_ValidatePartExists.sql
-- Validates whether a part exists in Infor Visual by exact part ID.
-- Returns a single count value that the C# DAO maps to bool.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PartId  nvarchar  Exact part ID to validate, e.g. '21-288'

DECLARE @PartId nvarchar(30) = '21-288';

SELECT COUNT(*) AS PartExists
FROM dbo.PART
WHERE ID = @PartId;
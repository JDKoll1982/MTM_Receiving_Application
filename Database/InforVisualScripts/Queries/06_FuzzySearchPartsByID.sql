-- 06_FuzzySearchPartsByID.sql
-- Fuzzy search for parts whose ID contains the given term.
-- Returns up to @MaxResults rows ordered by part ID.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @Term        nvarchar  Wildcard-wrapped search term, e.g. '%21-288%'
--   @MaxResults  int       Maximum rows to return (default: 50 in C# caller)

DECLARE @Term       nvarchar(60) = '%21-288%';
DECLARE @MaxResults int          = 50;

SELECT TOP (@MaxResults)
    p.ID          AS PartNumber,
    p.DESCRIPTION AS Description,
    p.STOCK_UM    AS PrimaryUom
FROM
    dbo.PART p
WHERE
    p.ID LIKE @Term
ORDER BY
    p.ID;

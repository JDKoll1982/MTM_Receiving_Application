-- 21_FuzzySearchCustomersByIdOrName.sql
-- Fuzzy search for customers whose ID or NAME contains the given term.
-- Returns up to @MaxResults rows ordered by customer ID.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @Term        nvarchar  Wildcard-wrapped search term, e.g. '%volvo%'
--   @MaxResults  int       Maximum rows to return (default: 50 in C# caller)

DECLARE @Term       nvarchar(60) = '%volvo%';
DECLARE @MaxResults int          = 50;

SELECT TOP (@MaxResults)
    c.ID                 AS CustomerID,
    COALESCE(c.NAME, '') AS CustomerName
FROM
    dbo.CUSTOMER c
WHERE
    c.ID LIKE @Term
    OR c.NAME LIKE @Term
ORDER BY
    c.ID;
-- 07_FuzzySearchVendorsByName.sql
-- Fuzzy search for vendors whose NAME contains the given term.
-- Returns up to @MaxResults rows ordered by vendor name.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @Term        nvarchar  Wildcard-wrapped search term, e.g. '%acme%'
--   @MaxResults  int       Maximum rows to return (default: 50 in C# caller)

DECLARE @Term       nvarchar(60) = '%acme%';
DECLARE @MaxResults int          = 50;

SELECT TOP (@MaxResults)
    v.ID    AS VendorID,
    v.NAME  AS VendorName,
    v.CITY  AS VendorCity,
    v.STATE AS VendorState
FROM
    dbo.VENDOR v
WHERE
    v.NAME LIKE @Term
ORDER BY
    v.NAME;

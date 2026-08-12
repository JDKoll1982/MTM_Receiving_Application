SET NOCOUNT ON;
DECLARE @SearchTerm NVARCHAR(200)='PUR AIR CYLINDER';
DECLARE @SearchLike NVARCHAR(260)='%' + @SearchTerm + '%';
DECLARE @NormalizedWildcard NVARCHAR(260)='%PUR%AIR%CYLINDER%';
DECLARE @FirstTokenLike NVARCHAR(260)='%PUR%';
DECLARE @MaxResults INT=2000;

WITH CandidateSpecs AS (
    SELECT
        plb.PURC_ORDER_ID AS PONumber,
        CAST(plb.PURC_ORDER_LINE_NO AS INT) AS POLineNumber,
        ISNULL(pol.PART_ID, '') AS PartId,
        ISNULL(po.VENDOR_ID, '') AS VendorId,
        CAST(plb.TYPE AS NVARCHAR(10)) AS BinaryType,
        CAST(CAST(plb.BITS AS VARBINARY(MAX)) AS NVARCHAR(MAX)) AS SpecText,
        UPPER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(CAST(CAST(plb.BITS AS VARBINARY(MAX)) AS NVARCHAR(MAX)), CHAR(13), ' '), CHAR(10), ' '), CHAR(9), ' ')))) AS NormalizedSpecText,
        UPPER(LTRIM(RTRIM(
            ISNULL(pol.VENDOR_PART_ID, '') + ' ' + ISNULL(pol.MFG_PART_ID, '') + ' ' + ISNULL(pol.PRODUCT_CODE, '') + ' ' + ISNULL(pol.COMMODITY_CODE, '') + ' ' +
            ISNULL(pol.USER_1, '') + ' ' + ISNULL(pol.USER_2, '') + ' ' + ISNULL(pol.USER_3, '') + ' ' + ISNULL(pol.USER_4, '') + ' ' + ISNULL(pol.USER_5, '') + ' ' +
            ISNULL(pol.USER_6, '') + ' ' + ISNULL(pol.USER_7, '') + ' ' + ISNULL(pol.USER_8, '') + ' ' + ISNULL(pol.USER_9, '') + ' ' + ISNULL(pol.USER_10, '')
        ))) AS SupplementalText
    FROM dbo.PURC_LINE_BINARY plb
    INNER JOIN dbo.PURC_ORDER_LINE pol
        ON pol.PURC_ORDER_ID = plb.PURC_ORDER_ID
        AND pol.LINE_NO = plb.PURC_ORDER_LINE_NO
    INNER JOIN dbo.PURCHASE_ORDER po
        ON po.ID = pol.PURC_ORDER_ID
    WHERE plb.BITS IS NOT NULL AND DATALENGTH(plb.BITS) > 0
), Ranked AS (
    SELECT TOP (@MaxResults)
        PONumber,
        POLineNumber,
        PartId,
        VendorId,
        BinaryType,
        SpecText,
        CASE
            WHEN NormalizedSpecText LIKE @SearchLike THEN 300
            WHEN NormalizedSpecText LIKE @NormalizedWildcard THEN 250
            WHEN SupplementalText LIKE @SearchLike THEN 200
            WHEN SupplementalText LIKE @NormalizedWildcard THEN 170
            WHEN NormalizedSpecText LIKE @FirstTokenLike THEN 100
            WHEN SupplementalText LIKE @FirstTokenLike THEN 80
            ELSE 0
        END AS SqlMatchWeight
    FROM CandidateSpecs
    WHERE
        NormalizedSpecText LIKE @SearchLike
        OR NormalizedSpecText LIKE @NormalizedWildcard
        OR SupplementalText LIKE @SearchLike
        OR SupplementalText LIKE @NormalizedWildcard
        OR NormalizedSpecText LIKE @FirstTokenLike
        OR SupplementalText LIKE @FirstTokenLike
    ORDER BY
        CASE
            WHEN NormalizedSpecText LIKE @SearchLike THEN 300
            WHEN NormalizedSpecText LIKE @NormalizedWildcard THEN 250
            WHEN SupplementalText LIKE @SearchLike THEN 200
            WHEN SupplementalText LIKE @NormalizedWildcard THEN 170
            WHEN NormalizedSpecText LIKE @FirstTokenLike THEN 100
            WHEN SupplementalText LIKE @FirstTokenLike THEN 80
            ELSE 0
        END DESC,
        PONumber DESC,
        POLineNumber,
        BinaryType
)
SELECT TOP 5 PONumber, POLineNumber, PartId, VendorId, BinaryType, SqlMatchWeight, LEFT(SpecText,120) AS SpecPreview
FROM Ranked
WHERE PONumber = 'PO-070945' AND POLineNumber = 1
ORDER BY SqlMatchWeight DESC;

SELECT TOP 5 PONumber, POLineNumber, PartId, VendorId, BinaryType, SqlMatchWeight
FROM Ranked
ORDER BY SqlMatchWeight DESC, PONumber DESC, POLineNumber;

/*
Exact-phrase PO line spec search.

Parameters expected from C#:
  @SearchTerm          NVARCHAR(200)
  @SearchLike          NVARCHAR(260)
  @NormalizedWildcard  NVARCHAR(260)
  @FirstTokenLike      NVARCHAR(260)
  @MaxResults          INT
*/

WITH CandidateSpecs AS (
    SELECT
        plb.PURC_ORDER_ID AS PONumber,
        CAST(plb.PURC_ORDER_LINE_NO AS INT) AS POLineNumber,
        ISNULL(pol.PART_ID, '') AS PartId,
        ISNULL(po.VENDOR_ID, '') AS VendorId,
        ISNULL(v.NAME, '') AS VendorName,
        ISNULL(pol.VENDOR_PART_ID, '') AS VendorPartId,
        ISNULL(pol.USER_ORDER_QTY, 0) AS QtyOrdered,
        ISNULL(pol.TOTAL_RECEIVED_QTY, 0) AS TotalQtyReceived,
        ISNULL(CAST(po.STATUS AS NVARCHAR(10)), '') AS PoStatus,
        CAST(plb.TYPE AS NVARCHAR(10)) AS BinaryType,
        CAST(plb.BITS AS VARBINARY(MAX)) AS BinaryBits,
        CAST(CAST(plb.BITS AS VARBINARY(MAX)) AS NVARCHAR(MAX)) AS SpecSearchText,
        ISNULL(pol.VENDOR_PART_ID, '') + ' '
            + ISNULL(pol.MFG_PART_ID, '') + ' '
            + ISNULL(pol.PRODUCT_CODE, '') + ' '
            + ISNULL(pol.COMMODITY_CODE, '') + ' '
            + ISNULL(pol.USER_1, '') + ' '
            + ISNULL(pol.USER_2, '') + ' '
            + ISNULL(pol.USER_3, '') + ' '
            + ISNULL(pol.USER_4, '') + ' '
            + ISNULL(pol.USER_5, '') + ' '
            + ISNULL(pol.USER_6, '') + ' '
            + ISNULL(pol.USER_7, '') + ' '
            + ISNULL(pol.USER_8, '') + ' '
            + ISNULL(pol.USER_9, '') + ' '
            + ISNULL(pol.USER_10, '') AS SupplementalText
    FROM dbo.PURC_LINE_BINARY plb
    INNER JOIN dbo.PURC_ORDER_LINE pol
        ON pol.PURC_ORDER_ID = plb.PURC_ORDER_ID
        AND pol.LINE_NO = plb.PURC_ORDER_LINE_NO
    INNER JOIN dbo.PURCHASE_ORDER po
        ON po.ID = pol.PURC_ORDER_ID
    LEFT JOIN dbo.VENDOR v
        ON v.ID = po.VENDOR_ID
    WHERE
        plb.BITS IS NOT NULL
        AND DATALENGTH(plb.BITS) > 0
)
SELECT TOP (@MaxResults)
    PONumber,
    POLineNumber,
    PartId,
    VendorId,
    VendorName,
    VendorPartId,
    QtyOrdered,
    TotalQtyReceived,
    PoStatus,
    BinaryType,
    BinaryBits,
    SupplementalText,
    CASE
        WHEN SpecSearchText LIKE @SearchLike THEN 320
        WHEN SupplementalText LIKE @SearchLike THEN 220
        ELSE 0
    END AS SqlMatchWeight
FROM CandidateSpecs
WHERE
    SpecSearchText LIKE @SearchLike
    OR SupplementalText LIKE @SearchLike
ORDER BY
    SqlMatchWeight DESC,
    PONumber DESC,
    POLineNumber,
    BinaryType;

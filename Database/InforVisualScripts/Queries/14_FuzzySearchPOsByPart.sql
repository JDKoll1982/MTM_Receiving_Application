-- 14_FuzzySearchPOsByPart.sql
-- Returns purchase orders that contain the specified part number.
-- Results include vendor and status so the picker can distinguish similar POs.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PartId      nvarchar  Exact part ID to search for
--   @MaxResults  int       Maximum rows to return (default: 50 in C# caller)

DECLARE @PartId     nvarchar(50) = 'MOCK-PART-001';
DECLARE @MaxResults int          = 50;

SELECT DISTINCT TOP (@MaxResults)
    po.ID AS PoNumber,
    v.NAME AS VendorName,
    po.STATUS AS PoStatus
FROM dbo.PURCHASE_ORDER po
INNER JOIN dbo.PURC_ORDER_LINE pol ON po.ID = pol.PURC_ORDER_ID
LEFT JOIN dbo.VENDOR v ON po.VENDOR_ID = v.ID
WHERE pol.PART_ID = @PartId
ORDER BY po.ID DESC;
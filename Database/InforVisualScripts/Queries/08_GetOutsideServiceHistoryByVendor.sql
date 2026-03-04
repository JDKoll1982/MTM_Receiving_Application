-- 08_GetOutsideServiceHistoryByVendor.sql
-- Retrieves outside service dispatch history for a specific vendor ID.
-- Mirrors 05_GetOutsideServiceHistoryByPart but filters by VENDOR.ID instead of PART_ID.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @VendorID  nvarchar  The vendor ID to search for (e.g. 'VENDOR-001')

DECLARE @VendorID nvarchar(12) = 'VENDOR-001';

SELECT DISTINCT
    v.ID             AS VendorID,
    v.NAME           AS VendorName,
    v.CITY           AS VendorCity,
    v.STATE          AS VendorState,
    sd.ID            AS DispatchID,
    sd.CREATE_DATE   AS DispatchDate,
    sdl.PART_ID      AS PartNumber,
    sdl.QTY          AS QuantitySent,
    sd.STATUS        AS DispatchStatus
FROM
    dbo.SERVICE_DISP_LINE sdl
    INNER JOIN dbo.SERVICE_DISPATCH sd  ON sdl.DISPATCH_ID = sd.ID
    INNER JOIN dbo.VENDOR            v  ON sd.VENDOR_ID    = v.ID
WHERE
    v.ID = @VendorID
ORDER BY
    sd.CREATE_DATE DESC;

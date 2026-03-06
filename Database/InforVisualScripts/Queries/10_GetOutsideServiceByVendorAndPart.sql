-- 10_GetOutsideServiceByVendorAndPart.sql
-- Retrieves outside service dispatch history filtered by both vendor ID and part number.
-- Called after the user selects a vendor and a specific part from the parts picker.
-- Mirrors 08_GetOutsideServiceHistoryByVendor with an additional PART_ID filter.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @VendorID   nvarchar  The vendor ID to filter by
--   @PartNumber nvarchar  The part ID to filter by in SERVICE_DISP_LINE.SERVICE_PART_ID

DECLARE @VendorID   nvarchar(12) = 'VENDOR-001';
DECLARE @PartNumber nvarchar(30) = 'PART-001';

SELECT DISTINCT
    v.ID             AS VendorID,
    v.NAME           AS VendorName,
    v.CITY           AS VendorCity,
    v.STATE          AS VendorState,
    sd.ID            AS DispatchID,
    sd.CREATE_DATE   AS DispatchDate,
    sdl.SERVICE_PART_ID AS PartNumber,
    sdl.DISPATCH_QTY    AS QuantitySent,
    CASE
        WHEN sdl.RECEIVED_QTY >= sdl.DISPATCH_QTY THEN 'Closed'
        ELSE 'Open'
    END              AS DispatchStatus
FROM
    dbo.SERVICE_DISP_LINE sdl
    INNER JOIN dbo.SERVICE_DISPATCH sd  ON sdl.DISPATCH_ID = sd.ID
    INNER JOIN dbo.VENDOR            v  ON sd.VENDOR_ID    = v.ID
WHERE
    sd.VENDOR_ID  = @VendorID
    AND sdl.SERVICE_PART_ID = @PartNumber
ORDER BY
    sd.CREATE_DATE DESC;

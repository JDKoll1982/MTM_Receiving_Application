-- 05_GetOutsideServiceHistoryByPart.sql
-- Retrieves outside service dispatch history for a specific part number.
-- Joins SERVICE_DISP_LINE → SERVICE_DISPATCH → VENDOR for full vendor context.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PartNumber  nvarchar  The part ID to search for in SERVICE_DISP_LINE.SERVICE_PART_ID

DECLARE @PartNumber nvarchar(30) = 'TEST-PART';

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
    sdl.SERVICE_PART_ID = @PartNumber
ORDER BY
    sd.CREATE_DATE DESC;

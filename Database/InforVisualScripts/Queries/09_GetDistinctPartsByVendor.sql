-- 09_GetDistinctPartsByVendor.sql
-- Returns all distinct part numbers a specific vendor has serviced,
-- along with dispatch count and the most recent dispatch date.
-- Used to populate the part selection picker after a vendor is confirmed.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @VendorID  nvarchar  The vendor ID to retrieve serviced parts for

DECLARE @VendorID nvarchar(12) = 'VENDOR-001';

SELECT
    sdl.PART_ID                     AS PartNumber,
    COUNT(sd.ID)                    AS DispatchCount,
    MAX(sd.CREATE_DATE)             AS LastDispatchDate
FROM
    dbo.SERVICE_DISP_LINE sdl
    INNER JOIN dbo.SERVICE_DISPATCH sd ON sdl.DISPATCH_ID = sd.ID
WHERE
    sd.VENDOR_ID = @VendorID
GROUP BY
    sdl.PART_ID
ORDER BY
    sdl.PART_ID;

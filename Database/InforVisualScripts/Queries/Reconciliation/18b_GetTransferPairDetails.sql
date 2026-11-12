-- 18b_GetTransferPairDetails.sql
-- Step 2 of the two-step transfer retrieval workflow.
-- Hydrates the full movement detail for the transfer pairs 18a discovered.
--
-- Each leg's role is labelled from TYPE, not from transaction id ordering:
-- src is the TYPE = 'O' leg and dst is the TYPE = 'I' leg (FR-030, decision D-04).
--
-- The candidate set is re-derived from the same TYPE-based join and the same
-- window parameters rather than passed in as an IN (...) list. Building that list
-- in application code would be dynamic SQL, which R-SQL-03 forbids, and it would
-- add an injection surface for no gain: the pair set for one part and one window
-- is small and cheap to recompute.
--
-- Rows are excluded when both legs name the same location, and when the movement
-- quantity is not positive.
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes (R-SQL-07).
--
-- Parameters:
--   @PartId        nvarchar  Exact part ID
--   @ReceivedDate  datetime  Receipt-date anchor
--   @WindowDays    int       Days either side of the anchor to search

DECLARE @PartId       nvarchar(50) = 'MMC0000650';
DECLARE @ReceivedDate datetime     = '2026-05-07T00:00:00';
DECLARE @WindowDays   int          = 2;

SELECT
    COALESCE(NULLIF(LTRIM(RTRIM(dst.PART_ID)), ''), src.PART_ID, @PartId)   AS PartId,
    COALESCE(NULLIF(LTRIM(RTRIM(dst.PURC_ORDER_ID)), ''),
             NULLIF(LTRIM(RTRIM(src.PURC_ORDER_ID)), ''), '')               AS PONumber,
    COALESCE(CONVERT(nvarchar(10), dst.PURC_ORDER_LINE_NO),
             CONVERT(nvarchar(10), src.PURC_ORDER_LINE_NO), '')             AS POLineNumber,
    COALESCE(src.WAREHOUSE_ID, '')                                          AS SourceWarehouseId,
    COALESCE(src.LOCATION_ID,  '')                                          AS SourceLocationId,
    COALESCE(dst.WAREHOUSE_ID, '')                                          AS DestinationWarehouseId,
    COALESCE(dst.LOCATION_ID,  '')                                          AS DestinationLocationId,
    COALESCE(dst.QTY, 0)                                                    AS TransferQuantity,
    COALESCE(dst.TRANSACTION_DATE, src.TRANSACTION_DATE)                    AS TransactionDate,
    -- TRANSACTION_DATE carries a date only in this database, so the UI would show
    -- a fabricated 00:00. CREATE_DATE is the real posting timestamp and is what the
    -- evidence pane shows; TRANSACTION_DATE stays for date-window arithmetic.
    COALESCE(dst.CREATE_DATE, src.CREATE_DATE,
             dst.TRANSACTION_DATE, src.TRANSACTION_DATE)                    AS MovementDateTime,
    COALESCE(NULLIF(LTRIM(RTRIM(dst.USER_ID)), ''),
             NULLIF(LTRIM(RTRIM(src.USER_ID)), ''), '')                     AS TransactionUserId,
    src.TRANSACTION_ID                                                      AS SourceTransactionId,
    dst.TRANSACTION_ID                                                      AS DestinationTransactionId
FROM dbo.INVENTORY_TRANS src
INNER JOIN dbo.INVENTORY_TRANS dst
    ON dst.TRANSACTION_ID = src.TRANSFER_TRANS_ID
WHERE src.PART_ID = @PartId
  AND src.TRANSFER_TRANS_ID IS NOT NULL
  AND src.TYPE = 'O'
  AND dst.TYPE = 'I'
  AND src.TRANSACTION_DATE >= DATEADD(day, -@WindowDays, CAST(@ReceivedDate AS date))
  AND src.TRANSACTION_DATE < DATEADD(day, @WindowDays + 1, CAST(@ReceivedDate AS date))
  AND NULLIF(LTRIM(RTRIM(src.LOCATION_ID)), '') IS NOT NULL
  AND NULLIF(LTRIM(RTRIM(dst.LOCATION_ID)), '') IS NOT NULL
  AND COALESCE(dst.QTY, 0) > 0
  AND NOT (
        COALESCE(src.WAREHOUSE_ID, '') = COALESCE(dst.WAREHOUSE_ID, '')
        AND COALESCE(src.LOCATION_ID, '') = COALESCE(dst.LOCATION_ID, '')
      )
ORDER BY
    COALESCE(dst.TRANSACTION_DATE, src.TRANSACTION_DATE) ASC,
    dst.TRANSACTION_ID ASC,
    src.TRANSACTION_ID ASC;

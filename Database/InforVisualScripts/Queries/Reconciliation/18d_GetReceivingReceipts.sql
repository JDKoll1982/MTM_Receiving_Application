-- 18d_GetReceivingReceipts.sql
-- Receipts that landed straight in a warehouse location, with no transfer pair.
--
-- A part received directly into its stock location never produces a transfer, so
-- the pair queries 18a and 18b return nothing for it and the group would otherwise
-- be reported as having no evidence at all. The receipt itself is the evidence: it
-- names the location the material actually reached.
--
-- CLASS = 'R' is the receiving class. A transfer leg carries CLASS = 'A' and an
-- issue to a work centre carries TYPE = 'O' with CLASS = 'I', so the CLASS filter
-- is what keeps those out; TRANSFER_TRANS_ID IS NULL keeps paired legs out as well.
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes (R-SQL-07).
--
-- Parameters:
--   @PartId        nvarchar  Exact part ID
--   @ReceivedDate  datetime  Receipt-date anchor
--   @WindowDays    int       Days either side of the anchor to search

DECLARE @PartId       nvarchar(50) = 'MMF0008050';
DECLARE @ReceivedDate datetime     = '2026-11-12T00:00:00';
DECLARE @WindowDays   int          = 2;

SELECT
    src.PART_ID                                             AS PartId,
    COALESCE(src.WAREHOUSE_ID, '')                          AS WarehouseId,
    COALESCE(src.LOCATION_ID, '')                           AS LocationId,
    COALESCE(src.QTY, 0)                                    AS ReceiptQuantity,
    COALESCE(NULLIF(LTRIM(RTRIM(src.PURC_ORDER_ID)), ''), '') AS PONumber,
    COALESCE(CONVERT(nvarchar(10), src.PURC_ORDER_LINE_NO), '') AS POLineNumber,
    -- TRANSACTION_DATE holds a date only in this database, so CREATE_DATE carries
    -- the real posting time the evidence pane shows.
    COALESCE(src.CREATE_DATE, src.TRANSACTION_DATE)         AS ReceiptDateTime,
    COALESCE(NULLIF(LTRIM(RTRIM(src.USER_ID)), ''), '')     AS ReceiptUserId,
    src.TRANSACTION_ID                                      AS TransactionId
FROM dbo.INVENTORY_TRANS src
WHERE src.PART_ID = @PartId
  AND src.TRANSFER_TRANS_ID IS NULL
  AND src.TYPE = 'I'
  AND src.CLASS = 'R'
  AND src.TRANSACTION_DATE >= DATEADD(day, -@WindowDays, CAST(@ReceivedDate AS date))
  AND src.TRANSACTION_DATE < DATEADD(day, @WindowDays + 1, CAST(@ReceivedDate AS date))
  AND NULLIF(LTRIM(RTRIM(src.LOCATION_ID)), '') IS NOT NULL
  AND COALESCE(src.QTY, 0) > 0
ORDER BY
    COALESCE(src.CREATE_DATE, src.TRANSACTION_DATE) ASC,
    src.TRANSACTION_ID ASC;

-- 17_GetReceivingLocationTransactionHistory.sql
-- Returns ordered transaction-history rows for one PO and part.
-- Used by receiving reconciliation smart-fix logic when location totals alone are not enough.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PoNumber      nvarchar  Canonical Infor Visual PO number, e.g. 'PO-066868'
--   @PartId        nvarchar  Exact part ID
--   @PoLineNumber  nvarchar  Optional PO line number
--   @ReceivedDate  datetime  Optional local receipt date anchor

DECLARE @PoNumber     nvarchar(30) = 'PO-066868';
DECLARE @PartId       nvarchar(50) = 'MMC-TEST';
DECLARE @PoLineNumber nvarchar(10) = '1';
DECLARE @ReceivedDate datetime     = '2026-04-05T00:00:00';

SELECT
    it.WAREHOUSE_ID                                  AS WarehouseId,
    it.LOCATION_ID                                   AS LocationId,
    it.QTY                                           AS Quantity,
    it.TRANSACTION_DATE                              AS TransactionDate,
    it.USER_ID                                       AS TransactionUserId,
    it.TRANSACTION_ID                                AS TransactionId,
    ISNULL(rl.WAREHOUSE_ID, '')                      AS ReceiptWarehouseId,
    ISNULL(rl.LOCATION_ID, '')                       AS ReceiptLocationId
FROM dbo.INVENTORY_TRANS it
LEFT JOIN dbo.RECEIVER_LINE rl
    ON rl.TRANSACTION_ID = it.TRANSACTION_ID
   AND rl.PURC_ORDER_ID = @PoNumber
WHERE it.PART_ID = @PartId
  AND it.PURC_ORDER_ID = @PoNumber
  AND (
        NULLIF(LTRIM(RTRIM(@PoLineNumber)), '') IS NULL
        OR it.PURC_ORDER_LINE_NO = TRY_CONVERT(smallint, @PoLineNumber)
      )
  AND NULLIF(LTRIM(RTRIM(it.LOCATION_ID)), '') IS NOT NULL
  AND (
        @ReceivedDate IS NULL
        OR CAST(it.TRANSACTION_DATE AS date)
            BETWEEN DATEADD(day, -2, CAST(@ReceivedDate AS date))
                AND DATEADD(day, 2, CAST(@ReceivedDate AS date))
      )
ORDER BY it.TRANSACTION_DATE DESC, it.TRANSACTION_ID DESC;
-- 18_GetReceivingLocationTransferMovements.sql
-- Returns resolved inventory transfer movements for one part around a receiving date.
-- Used by receiving reconciliation to allocate saved MTM rows against actual Visual destination moves.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PartId        nvarchar  Exact part ID
--   @ReceivedDate  datetime  Receipt-date anchor used to scope transfer evidence

DECLARE @PartId       nvarchar(50) = 'MMC0000650';
DECLARE @ReceivedDate datetime     = '2026-05-07T00:00:00';

;
WITH CandidateTransactions AS (
    SELECT
        it.TRANSACTION_ID,
        it.TRANSFER_TRANS_ID,
        it.PART_ID,
        it.PURC_ORDER_ID,
        it.PURC_ORDER_LINE_NO,
        it.WAREHOUSE_ID,
        it.LOCATION_ID,
        it.QTY,
        it.TRANSACTION_DATE,
        it.USER_ID
    FROM dbo.INVENTORY_TRANS it
    WHERE it.PART_ID = @PartId
      AND it.TRANSFER_TRANS_ID IS NOT NULL
      AND NULLIF(LTRIM(RTRIM(it.LOCATION_ID)), '') IS NOT NULL
      AND CAST(it.TRANSACTION_DATE AS date)
            BETWEEN DATEADD(day, -2, CAST(@ReceivedDate AS date))
                AND DATEADD(day, 2, CAST(@ReceivedDate AS date))
),
PairedTransfers AS (
    SELECT
        COALESCE(positive.PART_ID, negative.PART_ID, @PartId) AS PartId,
        COALESCE(NULLIF(LTRIM(RTRIM(positive.PURC_ORDER_ID)), ''), NULLIF(LTRIM(RTRIM(negative.PURC_ORDER_ID)), ''), '') AS PONumber,
        COALESCE(
            CONVERT(nvarchar(10), positive.PURC_ORDER_LINE_NO),
            CONVERT(nvarchar(10), negative.PURC_ORDER_LINE_NO),
            ''
        ) AS POLineNumber,
        COALESCE(negative.WAREHOUSE_ID, '') AS SourceWarehouseId,
        COALESCE(negative.LOCATION_ID, '') AS SourceLocationId,
        COALESCE(positive.WAREHOUSE_ID, '') AS DestinationWarehouseId,
        COALESCE(positive.LOCATION_ID, '') AS DestinationLocationId,
        COALESCE(positive.QTY, ABS(negative.QTY), 0) AS TransferQuantity,
        COALESCE(positive.TRANSACTION_DATE, negative.TRANSACTION_DATE) AS TransactionDate,
        COALESCE(NULLIF(LTRIM(RTRIM(positive.USER_ID)), ''), NULLIF(LTRIM(RTRIM(negative.USER_ID)), ''), '') AS TransactionUserId,
        negative.TRANSACTION_ID AS SourceTransactionId,
        positive.TRANSACTION_ID AS DestinationTransactionId,
        ROW_NUMBER() OVER (
            PARTITION BY
                CASE
                    WHEN negative.TRANSACTION_ID < positive.TRANSACTION_ID
                        THEN CONCAT(CONVERT(nvarchar(20), negative.TRANSACTION_ID), ':', CONVERT(nvarchar(20), positive.TRANSACTION_ID))
                    ELSE CONCAT(CONVERT(nvarchar(20), positive.TRANSACTION_ID), ':', CONVERT(nvarchar(20), negative.TRANSACTION_ID))
                END
            ORDER BY COALESCE(positive.TRANSACTION_DATE, negative.TRANSACTION_DATE) DESC,
                     positive.TRANSACTION_ID DESC,
                     negative.TRANSACTION_ID DESC
        ) AS PairRank
    FROM CandidateTransactions negative
    INNER JOIN dbo.INVENTORY_TRANS positive
        ON positive.PART_ID = negative.PART_ID
       AND (
            positive.TRANSACTION_ID = negative.TRANSFER_TRANS_ID
            OR positive.TRANSFER_TRANS_ID = negative.TRANSACTION_ID
       )
    WHERE negative.QTY < 0
      AND positive.QTY > 0
      AND NULLIF(LTRIM(RTRIM(positive.LOCATION_ID)), '') IS NOT NULL
)
SELECT
    PartId,
    PONumber,
    POLineNumber,
    SourceWarehouseId,
    SourceLocationId,
    DestinationWarehouseId,
    DestinationLocationId,
    TransferQuantity,
    TransactionDate,
    TransactionUserId,
    SourceTransactionId,
    DestinationTransactionId
FROM PairedTransfers
WHERE PairRank = 1
  AND TransferQuantity > 0
  AND NULLIF(LTRIM(RTRIM(SourceLocationId)), '') IS NOT NULL
  AND NULLIF(LTRIM(RTRIM(DestinationLocationId)), '') IS NOT NULL
ORDER BY TransactionDate ASC, DestinationTransactionId ASC, SourceTransactionId ASC;
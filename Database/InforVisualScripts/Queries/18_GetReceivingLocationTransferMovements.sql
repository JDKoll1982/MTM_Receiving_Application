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
WITH CandidatePairs AS (
    -- NOTE: Live INVENTORY_TRANS transfer pairs are recorded as reciprocal positive-quantity rows.
    -- The lower TRANSACTION_ID row is the source location and the higher TRANSACTION_ID row is the destination.
    SELECT
        CASE
            WHEN it.TRANSACTION_ID < it.TRANSFER_TRANS_ID THEN it.TRANSACTION_ID
            ELSE it.TRANSFER_TRANS_ID
        END AS SourceTransactionId,
        CASE
            WHEN it.TRANSACTION_ID < it.TRANSFER_TRANS_ID THEN it.TRANSFER_TRANS_ID
            ELSE it.TRANSACTION_ID
        END AS DestinationTransactionId
    FROM dbo.INVENTORY_TRANS it
    WHERE it.PART_ID = @PartId
      AND it.TRANSFER_TRANS_ID IS NOT NULL
      AND CAST(it.TRANSACTION_DATE AS date)
            BETWEEN DATEADD(day, -2, CAST(@ReceivedDate AS date))
                AND DATEADD(day, 2, CAST(@ReceivedDate AS date))
    GROUP BY
        CASE
            WHEN it.TRANSACTION_ID < it.TRANSFER_TRANS_ID THEN it.TRANSACTION_ID
            ELSE it.TRANSFER_TRANS_ID
        END,
        CASE
            WHEN it.TRANSACTION_ID < it.TRANSFER_TRANS_ID THEN it.TRANSFER_TRANS_ID
            ELSE it.TRANSACTION_ID
        END
),
PairedTransfers AS (
    SELECT
        COALESCE(dst.PART_ID, src.PART_ID, @PartId) AS PartId,
        COALESCE(NULLIF(LTRIM(RTRIM(dst.PURC_ORDER_ID)), ''), NULLIF(LTRIM(RTRIM(src.PURC_ORDER_ID)), ''), '') AS PONumber,
        COALESCE(
            CONVERT(nvarchar(10), dst.PURC_ORDER_LINE_NO),
            CONVERT(nvarchar(10), src.PURC_ORDER_LINE_NO),
            ''
        ) AS POLineNumber,
        COALESCE(src.WAREHOUSE_ID, '') AS SourceWarehouseId,
        COALESCE(src.LOCATION_ID, '') AS SourceLocationId,
        COALESCE(dst.WAREHOUSE_ID, '') AS DestinationWarehouseId,
        COALESCE(dst.LOCATION_ID, '') AS DestinationLocationId,
        CASE
            WHEN COALESCE(dst.QTY, 0) > 0 THEN COALESCE(dst.QTY, 0)
            ELSE ABS(COALESCE(src.QTY, 0))
        END AS TransferQuantity,
        COALESCE(dst.TRANSACTION_DATE, src.TRANSACTION_DATE) AS TransactionDate,
        COALESCE(NULLIF(LTRIM(RTRIM(dst.USER_ID)), ''), NULLIF(LTRIM(RTRIM(src.USER_ID)), ''), '') AS TransactionUserId,
        src.TRANSACTION_ID AS SourceTransactionId,
        dst.TRANSACTION_ID AS DestinationTransactionId
    FROM CandidatePairs p
    INNER JOIN dbo.INVENTORY_TRANS src
        ON src.TRANSACTION_ID = p.SourceTransactionId
    INNER JOIN dbo.INVENTORY_TRANS dst
        ON dst.TRANSACTION_ID = p.DestinationTransactionId
    WHERE COALESCE(src.PART_ID, dst.PART_ID) = @PartId
      AND NULLIF(LTRIM(RTRIM(src.LOCATION_ID)), '') IS NOT NULL
      AND NULLIF(LTRIM(RTRIM(dst.LOCATION_ID)), '') IS NOT NULL
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
WHERE TransferQuantity > 0
  AND NULLIF(LTRIM(RTRIM(SourceLocationId)), '') IS NOT NULL
  AND NULLIF(LTRIM(RTRIM(DestinationLocationId)), '') IS NOT NULL
ORDER BY TransactionDate ASC, DestinationTransactionId ASC, SourceTransactionId ASC;
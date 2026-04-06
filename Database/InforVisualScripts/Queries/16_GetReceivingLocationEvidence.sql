-- 16_GetReceivingLocationEvidence.sql
-- Returns current inventory locations plus per-location PO transaction evidence for one PO/part receipt.
-- Used by Receiving location reconciliation when MTM does not use TRACE in Infor Visual.
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PoNumber      nvarchar  Canonical Infor Visual PO number (for example PO-066868)
--   @PartId        nvarchar  Exact part ID
--   @PoLineNumber  nvarchar  Optional PO line number
--   @ReceivedDate  datetime  Optional local receipt date anchor

DECLARE @PoNumber     nvarchar(30) = 'PO-066868';
DECLARE @PartId       nvarchar(50) = 'MMC-TEST';
DECLARE @PoLineNumber nvarchar(10) = '1';
DECLARE @ReceivedDate datetime     = '2026-04-05T00:00:00';

WITH ReceiptMatches AS (
    SELECT
        r.RECEIVED_DATE AS ReceivedDate,
        rl.WAREHOUSE_ID AS WarehouseId,
        rl.LOCATION_ID AS LocationId,
        COALESCE(it.TRANSACTION_DATE, r.RECEIVED_DATE) AS EvidenceDate,
        ROW_NUMBER() OVER (
            ORDER BY COALESCE(it.TRANSACTION_DATE, r.RECEIVED_DATE) DESC,
                     ISNULL(rl.TRANSACTION_ID, 0) DESC,
                     rl.LINE_NO DESC
        ) AS ReceiptRank
    FROM dbo.RECEIVER_LINE rl
    INNER JOIN dbo.RECEIVER r
        ON r.ID = rl.RECEIVER_ID
    INNER JOIN dbo.PURC_ORDER_LINE pol
        ON pol.PURC_ORDER_ID = rl.PURC_ORDER_ID
       AND pol.LINE_NO = rl.PURC_ORDER_LINE_NO
    LEFT JOIN dbo.INVENTORY_TRANS it
        ON it.TRANSACTION_ID = rl.TRANSACTION_ID
       AND it.PART_ID = pol.PART_ID
    WHERE rl.PURC_ORDER_ID = @PoNumber
      AND pol.PART_ID = @PartId
      AND (
            NULLIF(LTRIM(RTRIM(@PoLineNumber)), '') IS NULL
            OR rl.PURC_ORDER_LINE_NO = TRY_CONVERT(smallint, @PoLineNumber)
          )
      AND (
            @ReceivedDate IS NULL
            OR CAST(r.RECEIVED_DATE AS date)
                BETWEEN DATEADD(day, -2, CAST(@ReceivedDate AS date))
                    AND DATEADD(day, 2, CAST(@ReceivedDate AS date))
          )
),
ReceiptSummary AS (
    SELECT
        COUNT(1) AS ReceiptCount,
        MIN(ReceivedDate) AS FirstReceivedDate,
        MAX(ReceivedDate) AS LastReceivedDate,
        MAX(CASE WHEN ReceiptRank = 1 THEN WarehouseId END) AS LatestReceiptWarehouseId,
        MAX(CASE WHEN ReceiptRank = 1 THEN LocationId END) AS LatestReceiptLocationId,
        MAX(CASE WHEN ReceiptRank = 1 THEN EvidenceDate END) AS LatestReceiptEvidenceDate
    FROM ReceiptMatches
),
PoTransactions AS (
    SELECT
        it.WAREHOUSE_ID AS WarehouseId,
        it.LOCATION_ID AS LocationId,
        it.QTY AS TransactionQuantity,
        it.TRANSACTION_DATE AS TransactionDate,
        it.USER_ID AS TransactionUserId,
        it.TRANSACTION_ID AS TransactionId,
        ROW_NUMBER() OVER (
            PARTITION BY it.WAREHOUSE_ID, it.LOCATION_ID
            ORDER BY it.TRANSACTION_DATE DESC, it.TRANSACTION_ID DESC
        ) AS LocationRank,
        ROW_NUMBER() OVER (
            ORDER BY it.TRANSACTION_DATE DESC, it.TRANSACTION_ID DESC
        ) AS OverallRank
    FROM dbo.INVENTORY_TRANS it
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
),
LatestPoTransaction AS (
    SELECT TOP (1)
        WarehouseId,
        LocationId,
        TransactionQuantity,
        TransactionDate,
        TransactionUserId,
        TransactionId
    FROM PoTransactions
    WHERE OverallRank = 1
),
LocationTransactionSummary AS (
    SELECT
        WarehouseId,
        LocationId,
        SUM(TransactionQuantity) AS MatchedTransactionQuantity,
        COUNT(1) AS MatchedTransactionCount,
        MAX(CASE WHEN LocationRank = 1 THEN TransactionDate END) AS MatchedTransactionDate,
        MAX(CASE WHEN LocationRank = 1 THEN TransactionUserId END) AS MatchedTransactionUserId,
        MAX(CASE WHEN LocationRank = 1 THEN TransactionId END) AS MatchedTransactionId
    FROM PoTransactions
    GROUP BY WarehouseId, LocationId
),
CurrentInventory AS (
    SELECT
        pl.WAREHOUSE_ID AS WarehouseId,
        pl.LOCATION_ID AS LocationId,
        pl.QTY AS CurrentQuantity
    FROM dbo.PART_LOCATION pl
    WHERE pl.PART_ID = @PartId
      AND pl.QTY > 0
      AND NULLIF(LTRIM(RTRIM(pl.LOCATION_ID)), '') IS NOT NULL
)
SELECT
    ISNULL(ci.WarehouseId, '') AS CurrentWarehouseId,
    ISNULL(ci.LocationId, '') AS CurrentLocationId,
    ISNULL(ci.CurrentQuantity, 0) AS CurrentQuantity,
    ISNULL(lts.MatchedTransactionQuantity, 0) AS MatchedTransactionQuantity,
    ISNULL(lts.MatchedTransactionCount, 0) AS MatchedTransactionCount,
    lts.MatchedTransactionDate,
    ISNULL(lts.MatchedTransactionUserId, '') AS MatchedTransactionUserId,
    lts.MatchedTransactionId,
    ISNULL(rs.ReceiptCount, 0) AS ReceiptCount,
    rs.FirstReceivedDate,
    rs.LastReceivedDate,
    ISNULL(rs.LatestReceiptWarehouseId, '') AS LatestReceiptWarehouseId,
    ISNULL(rs.LatestReceiptLocationId, '') AS LatestReceiptLocationId,
    rs.LatestReceiptEvidenceDate,
    ISNULL(lpt.WarehouseId, '') AS LatestTransactionWarehouseId,
    ISNULL(lpt.LocationId, '') AS LatestTransactionLocationId,
    ISNULL(lpt.TransactionQuantity, 0) AS LatestTransactionQuantity,
    lpt.TransactionDate AS LatestTransactionDate,
    ISNULL(lpt.TransactionUserId, '') AS LatestTransactionUserId,
    lpt.TransactionId AS LatestTransactionId
FROM ReceiptSummary rs
LEFT JOIN CurrentInventory ci
    ON 1 = 1
LEFT JOIN LocationTransactionSummary lts
    ON lts.WarehouseId = ci.WarehouseId
   AND lts.LocationId = ci.LocationId
LEFT JOIN LatestPoTransaction lpt
    ON 1 = 1;
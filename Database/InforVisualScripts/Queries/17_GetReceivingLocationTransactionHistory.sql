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

;
WITH ParameterValues AS (
    SELECT
        NULLIF(LTRIM(RTRIM(@PoLineNumber)), '') AS NormalizedPoLineNumber,
        CASE
            WHEN NULLIF(LTRIM(RTRIM(@PoLineNumber)), '') IS NOT NULL
                 AND NULLIF(LTRIM(RTRIM(@PoLineNumber)), '') NOT LIKE N'%[^0-9]%'
                 AND (
                        LEN(NULLIF(LTRIM(RTRIM(@PoLineNumber)), '')) < 5
                        OR (
                            LEN(NULLIF(LTRIM(RTRIM(@PoLineNumber)), '')) = 5
                            AND NULLIF(LTRIM(RTRIM(@PoLineNumber)), '') <= N'32767'
                        )
                    )
                THEN CONVERT(smallint, NULLIF(LTRIM(RTRIM(@PoLineNumber)), ''))
            ELSE NULL
        END AS PoLineNumberValue
),
FilteredTransactions AS (
    SELECT
        it.TRANSACTION_ID     AS TransactionId,
        it.TRANSFER_TRANS_ID  AS TransferTransactionId,
        it.WAREHOUSE_ID       AS WarehouseId,
        it.LOCATION_ID        AS LocationId,
        it.QTY                AS Quantity,
        it.TRANSACTION_DATE   AS TransactionDate,
        it.USER_ID            AS TransactionUserId,
        ISNULL(rl.WAREHOUSE_ID, '') AS ReceiptWarehouseId,
        ISNULL(rl.LOCATION_ID, '')  AS ReceiptLocationId
    FROM dbo.INVENTORY_TRANS it
    CROSS JOIN ParameterValues pv
    LEFT JOIN dbo.RECEIVER_LINE rl
        ON rl.TRANSACTION_ID = it.TRANSACTION_ID
       AND rl.PURC_ORDER_ID = @PoNumber
    WHERE it.PART_ID = @PartId
      AND it.PURC_ORDER_ID = @PoNumber
      AND (
            pv.NormalizedPoLineNumber IS NULL
            OR (pv.PoLineNumberValue IS NOT NULL AND it.PURC_ORDER_LINE_NO = pv.PoLineNumberValue)
          )
      AND NULLIF(LTRIM(RTRIM(it.LOCATION_ID)), '') IS NOT NULL
      AND (
            @ReceivedDate IS NULL
            OR CAST(it.TRANSACTION_DATE AS date)
                BETWEEN DATEADD(day, -2, CAST(@ReceivedDate AS date))
                    AND DATEADD(day, 2, CAST(@ReceivedDate AS date))
          )
),
ResolvedTransactions AS (
    SELECT
        ft.TransactionId,
        CASE
            WHEN ft.Quantity > 0 THEN ft.WarehouseId
            WHEN linked.QTY > 0 THEN linked.WAREHOUSE_ID
            ELSE ft.WarehouseId
        END AS WarehouseId,
        CASE
            WHEN ft.Quantity > 0 THEN ft.LocationId
            WHEN linked.QTY > 0 THEN linked.LOCATION_ID
            ELSE ft.LocationId
        END AS LocationId,
        CASE
            WHEN ft.Quantity > 0 THEN ft.Quantity
            WHEN linked.QTY > 0 THEN linked.QTY
            ELSE ft.Quantity
        END AS Quantity,
        CASE
            WHEN ft.Quantity > 0 THEN ft.TransactionDate
            WHEN linked.QTY > 0 THEN linked.TRANSACTION_DATE
            ELSE ft.TransactionDate
        END AS TransactionDate,
        CASE
            WHEN ft.Quantity > 0 THEN ft.TransactionUserId
            WHEN linked.QTY > 0 THEN linked.USER_ID
            ELSE ft.TransactionUserId
        END AS TransactionUserId,
        CASE
            WHEN ft.Quantity > 0 THEN ft.TransactionId
            WHEN linked.QTY > 0 THEN linked.TRANSACTION_ID
            ELSE ft.TransactionId
        END AS ResolvedTransactionId,
        ft.ReceiptWarehouseId,
        ft.ReceiptLocationId,
        ROW_NUMBER() OVER (
            PARTITION BY CASE
                WHEN ft.TransferTransactionId IS NOT NULL AND linked.TRANSACTION_ID IS NOT NULL
                    THEN CONCAT(
                        CONVERT(nvarchar(20), CASE WHEN ft.TransactionId < linked.TRANSACTION_ID THEN ft.TransactionId ELSE linked.TRANSACTION_ID END),
                        ':',
                        CONVERT(nvarchar(20), CASE WHEN ft.TransactionId < linked.TRANSACTION_ID THEN linked.TRANSACTION_ID ELSE ft.TransactionId END)
                    )
                ELSE CONVERT(nvarchar(20), ft.TransactionId)
            END
            ORDER BY CASE
                WHEN ft.Quantity > 0 THEN 0
                WHEN linked.QTY > 0 THEN 1
                ELSE 2
            END,
            CASE
                WHEN ft.Quantity > 0 THEN ft.TransactionDate
                WHEN linked.QTY > 0 THEN linked.TRANSACTION_DATE
                ELSE ft.TransactionDate
            END DESC,
            CASE
                WHEN ft.Quantity > 0 THEN ft.TransactionId
                WHEN linked.QTY > 0 THEN linked.TRANSACTION_ID
                ELSE ft.TransactionId
            END DESC
        ) AS PairRank
    FROM FilteredTransactions ft
    LEFT JOIN dbo.INVENTORY_TRANS linked
        ON linked.PART_ID = @PartId
       AND (
            linked.TRANSACTION_ID = ft.TransferTransactionId
            OR linked.TRANSFER_TRANS_ID = ft.TransactionId
       )
)
SELECT
    WarehouseId,
    LocationId,
    Quantity,
    TransactionDate,
    TransactionUserId,
    ResolvedTransactionId AS TransactionId,
    ReceiptWarehouseId,
    ReceiptLocationId
FROM ResolvedTransactions
WHERE PairRank = 1
  AND NULLIF(LTRIM(RTRIM(LocationId)), '') IS NOT NULL
ORDER BY TransactionDate DESC, TransactionId DESC;
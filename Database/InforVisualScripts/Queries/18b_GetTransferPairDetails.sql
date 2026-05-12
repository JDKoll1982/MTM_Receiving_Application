-- 18b_GetTransferPairDetails.sql
-- Step 2 of the two-step transfer retrieval workflow.
-- Hydrates full movement detail rows for the candidate source transaction IDs returned by 18a.
--
-- The IN (...) list below must contain the SourceTransactionId integers produced by 18a.
-- In C# production code the list is built dynamically:
--
--   WHERE src.TRANSACTION_ID IN (1,2,3,...)
--
-- A table-valued parameter and #temp table are intentionally avoided; the candidate set
-- for a single part-and-date reconciliation is expected to remain small.
--
-- For SSMS testing: replace the example values in the IN clause with actual IDs from 18a.
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters (C# passes these):
--   SourceTransactionId list  integers  Source TRANSACTION_ID values from 18a
--   @PartId                   nvarchar  Exact part ID (used as a COALESCE fallback)

DECLARE @PartId nvarchar(50) = 'MMC0000650';

SELECT
    COALESCE(dst.PART_ID, src.PART_ID, @PartId)                                          AS PartId,
    COALESCE(NULLIF(LTRIM(RTRIM(dst.PURC_ORDER_ID)), ''),
             NULLIF(LTRIM(RTRIM(src.PURC_ORDER_ID)), ''), '')                             AS PONumber,
    COALESCE(CONVERT(nvarchar(10), dst.PURC_ORDER_LINE_NO),
             CONVERT(nvarchar(10), src.PURC_ORDER_LINE_NO), '')                           AS POLineNumber,
    COALESCE(src.WAREHOUSE_ID, '')                                                        AS SourceWarehouseId,
    COALESCE(src.LOCATION_ID,  '')                                                        AS SourceLocationId,
    COALESCE(dst.WAREHOUSE_ID, '')                                                        AS DestinationWarehouseId,
    COALESCE(dst.LOCATION_ID,  '')                                                        AS DestinationLocationId,
    CASE
        WHEN COALESCE(dst.QTY, 0) > 0 THEN  COALESCE(dst.QTY, 0)
        ELSE                                 ABS(COALESCE(src.QTY, 0))
    END                                                                                   AS TransferQuantity,
    COALESCE(dst.TRANSACTION_DATE, src.TRANSACTION_DATE)                                  AS TransactionDate,
    COALESCE(NULLIF(LTRIM(RTRIM(dst.USER_ID)), ''),
             NULLIF(LTRIM(RTRIM(src.USER_ID)), ''), '')                                   AS TransactionUserId,
    src.TRANSACTION_ID                                                                    AS SourceTransactionId,
    dst.TRANSACTION_ID                                                                    AS DestinationTransactionId
FROM dbo.INVENTORY_TRANS src
INNER JOIN dbo.INVENTORY_TRANS dst
    ON dst.TRANSACTION_ID = src.TRANSFER_TRANS_ID
WHERE src.TRANSACTION_ID IN (1, 2, 3)   -- Replace with SourceTransactionId values from 18a
  AND COALESCE(src.PART_ID, dst.PART_ID) = @PartId
  AND NULLIF(LTRIM(RTRIM(src.LOCATION_ID)), '') IS NOT NULL
  AND NULLIF(LTRIM(RTRIM(dst.LOCATION_ID)), '') IS NOT NULL
  AND CASE
          WHEN COALESCE(dst.QTY, 0) > 0 THEN  COALESCE(dst.QTY, 0)
          ELSE                                 ABS(COALESCE(src.QTY, 0))
      END > 0
ORDER BY TransactionDate ASC, DestinationTransactionId ASC, SourceTransactionId ASC;

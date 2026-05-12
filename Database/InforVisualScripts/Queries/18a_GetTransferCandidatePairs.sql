-- 18a_GetTransferCandidatePairs.sql
-- Step 1 of the two-step transfer retrieval workflow.
-- Discovers candidate source/destination INVENTORY_TRANS ID pairs for one part around a receiving date.
--
-- The lower TRANSACTION_ID of each reciprocal pair is treated as the source movement;
-- the higher TRANSACTION_ID is treated as the destination movement.
-- This assumption is preserved from the original combined query (18_GetReceivingLocationTransferMovements.sql).
--
-- C# runtime workflow:
--   1. Run this query to get (SourceTransactionId, DestinationTransactionId) pairs.
--   2. Collect the SourceTransactionId integers.
--   3. Pass them to 18b_GetTransferPairDetails.sql via a dynamically built IN (...) clause.
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PartId        nvarchar  Exact part ID
--   @ReceivedDate  datetime  Receipt-date anchor; ±2 calendar days are searched

DECLARE @PartId       nvarchar(50) = 'MMC0000650';
DECLARE @ReceivedDate datetime     = '2026-05-07T00:00:00';

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
            AND DATEADD(day,  2, CAST(@ReceivedDate AS date))
GROUP BY
    CASE
        WHEN it.TRANSACTION_ID < it.TRANSFER_TRANS_ID THEN it.TRANSACTION_ID
        ELSE it.TRANSFER_TRANS_ID
    END,
    CASE
        WHEN it.TRANSACTION_ID < it.TRANSFER_TRANS_ID THEN it.TRANSFER_TRANS_ID
        ELSE it.TRANSACTION_ID
    END
ORDER BY SourceTransactionId ASC;

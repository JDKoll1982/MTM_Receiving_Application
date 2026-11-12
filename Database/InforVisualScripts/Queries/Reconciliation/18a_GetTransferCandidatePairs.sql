-- 18a_GetTransferCandidatePairs.sql
-- Step 1 of the two-step transfer retrieval workflow.
-- Discovers candidate source/destination INVENTORY_TRANS id pairs for one part
-- around a receiving date.
--
-- Direction comes from TYPE, never from transaction id ordering:
--   TYPE = 'O' is the leg that left, so it is the source.
--   TYPE = 'I' is the leg that arrived, so it is the destination.
-- The lower-id-is-source rule that this query replaced inverted source and
-- destination whenever the out leg carried the higher id, which would certify a
-- wrong residual (FR-030, R-DATA-09, decision D-04).
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes (R-SQL-07).
--
-- Parameters:
--   @PartId        nvarchar  Exact part ID
--   @ReceivedDate  datetime  Receipt-date anchor
--   @WindowDays    int       Days either side of the anchor to search (FR-014)

DECLARE @PartId       nvarchar(50) = 'MMC0000650';
DECLARE @ReceivedDate datetime     = '2026-05-07T00:00:00';
DECLARE @WindowDays   int          = 2;

SELECT
    src.TRANSACTION_ID AS SourceTransactionId,
    dst.TRANSACTION_ID AS DestinationTransactionId
FROM dbo.INVENTORY_TRANS src
INNER JOIN dbo.INVENTORY_TRANS dst
    ON dst.TRANSACTION_ID = src.TRANSFER_TRANS_ID
WHERE src.PART_ID = @PartId
  AND src.TRANSFER_TRANS_ID IS NOT NULL
  AND src.TYPE = 'O'
  AND dst.TYPE = 'I'
  AND src.TRANSACTION_DATE >= DATEADD(day, -@WindowDays, CAST(@ReceivedDate AS date))
  AND src.TRANSACTION_DATE < DATEADD(day, @WindowDays + 1, CAST(@ReceivedDate AS date))
ORDER BY src.TRANSACTION_ID ASC;

-- 28_ScannerTransferTransactionLookup.sql
-- Confirms whether an inventory transfer matching a scanner-emitted line was recorded in
-- Infor Visual after a given instant. Polled by the scanner workbench to auto-confirm a send
-- instead of asking the operator "Saved?".
--
-- A scanner line maps to an INVENTORY_TRANS row at the source (from) location or the
-- destination (to) location with the matching part and quantity. The query returns 1 as soon
-- as such a row is recorded after the emission time.
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PartId        nvarchar  Exact part ID
--   @FromWarehouse nvarchar  Source warehouse
--   @FromLocation  nvarchar  Source location
--   @ToWarehouse   nvarchar  Destination warehouse
--   @ToLocation    nvarchar  Destination location
--   @Quantity      decimal   Expected transfer quantity (absolute value is matched)
--   @AfterUtc      datetime  Only consider transactions recorded at/after this instant

DECLARE @PartId        nvarchar(50)  = 'MMC0000850';
DECLARE @FromWarehouse nvarchar(10)  = '002';
DECLARE @FromLocation  nvarchar(30)  = 'A01';
DECLARE @ToWarehouse   nvarchar(10)  = '002';
DECLARE @ToLocation    nvarchar(30)  = 'B01';
DECLARE @Quantity      decimal(19,4) = 1;
DECLARE @AfterUtc      datetime      = '2026-08-24T19:00:00';

SELECT
    CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.INVENTORY_TRANS it
        WHERE it.PART_ID = @PartId
          AND it.TYPE IN ('I', 'O')
          AND ABS(it.QTY) = ABS(@Quantity)
          AND COALESCE(it.CREATE_DATE, it.TRANSACTION_DATE) >= @AfterUtc
          AND (
                (it.WAREHOUSE_ID = @FromWarehouse AND it.LOCATION_ID = @FromLocation)
                OR (it.WAREHOUSE_ID = @ToWarehouse AND it.LOCATION_ID = @ToLocation)
              )
    ) THEN 1 ELSE 0 END AS TransferFound;

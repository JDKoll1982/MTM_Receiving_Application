-- 32_GetInventoryDiscrepancyPartProfile.sql
-- Discrepancy investigation profile for ONE part. Run the result sets in order.
-- Implements the mandatory checks listed in:
--   .github/instructions/database/infor-visual-inventory-discrepancy-investigation.instructions.md
--
-- Result sets, in order:
--    1  Part master - system on-hand, on order, demand, backflush flag
--    2  Current on-hand per location - every row, including zero and negative
--    3  Ledger self-check - In minus Out must equal the on-hand total in result set 2
--    4  Lifetime type/class profile - I/CLASS-A must equal O/CLASS-A (transfers net to zero)
--    5  Lifetime net by location - shows which locations are carrying the imbalance
--    6  Running-balance low point per location - finds consumption ahead of stock
--    7  Unpaired transfers - legs whose partner transaction is missing
--    8  Receipt audit - every physical receiver must carry a posted TRANSACTION_ID
--    9  Work-order consumption versus work order status
--   10  Labor clock-outs behind one work order (fill @WorkOrder from result set 9)
--   11  Purchase-order lines never received
--   12  Manual adjustments - CLASS A with no transfer partner and no WO/PO
--   13  Cycle-count setup, and whether the cycle-count feature is used at all
--
-- Analysis notes:
--   * The ledger is always internally consistent. If result set 3 equals result set 2,
--     the gap is a physical-activity mismatch, not a missing record.
--   * A negative balance in a location that is flagged DEF_BACKFLUSH_LOC or AUTO_ISSUE_LOC
--     means consumption was posted there without stock ever being transferred in.
--     That is the usual cause of "physical is higher than the system".
--   * Compare the flagged backflush location against result set 5 before concluding.
--
-- READ-ONLY query against Infor Visual (MTMFG) - no writes.
--
-- Parameters:
--   @PartId     nvarchar  Exact part ID (exact match; part IDs are not space-padded)
--   @WorkOrder  nvarchar  Work order for result set 10; leave empty to skip

DECLARE @PartId    nvarchar(30) = 'W99996-06002S';
DECLARE @WorkOrder nvarchar(30) = '';

PRINT '1 - PART MASTER';
SELECT
    p.ID                        AS PartId,
    p.DESCRIPTION               AS Description,
    p.STOCK_UM                  AS StockUnit,
    p.QTY_ON_HAND               AS SystemOnHand,
    p.QTY_AVAILABLE_ISS         AS AvailableToIssue,
    p.QTY_ON_ORDER              AS OnOrder,
    p.QTY_IN_DEMAND             AS InDemand,
    p.QTY_COMMITTED             AS Committed,
    p.AUTO_BACKFLUSH            AS AutoBackflush,
    p.INVENTORY_LOCKED          AS InventoryLocked,
    p.PLANNER_USER_ID           AS Planner,
    p.BUYER_USER_ID             AS Buyer,
    p.MFG_NAME                  AS Manufacturer,
    p.MFG_PART_ID               AS ManufacturerPart,
    p.STATUS                    AS PartStatus
FROM dbo.PART p
WHERE p.ID = @PartId;

PRINT '2 - CURRENT ON-HAND PER LOCATION';
SELECT
    cpl.WAREHOUSE_ID            AS WarehouseId,
    cpl.LOCATION_ID             AS LocationId,
    cpl.QTY                     AS OnHand,
    cpl.COMMITTED_QTY           AS CommittedQty,
    cpl.STATUS                  AS LocationStatus,
    cpl.TRANSIT                 AS InTransit,
    cpl.DEF_BACKFLUSH_LOC       AS IsBackflushLocation,
    cpl.AUTO_ISSUE_LOC          AS IsAutoIssueLocation,
    cpl.LOCKED                  AS IsLocked,
    cpl.LAST_COUNT_DATE         AS LastCountDate,
    cpl.DEF_INSPECT_LOC         AS IsInspectLocation
FROM dbo.CR_PART_LOCATION cpl
WHERE cpl.ID = @PartId
ORDER BY cpl.WAREHOUSE_ID, cpl.LOCATION_ID;

PRINT '3 - LEDGER SELF-CHECK (NetOnHand must equal the total from result set 2)';
SELECT
    COUNT(*)                                            AS TransactionCount,
    MIN(it.TRANSACTION_DATE)                            AS FirstTransaction,
    MAX(it.TRANSACTION_DATE)                            AS LastTransaction,
    SUM(CASE WHEN it.TYPE = 'I' THEN it.QTY ELSE 0 END)    AS SumIn,
    SUM(CASE WHEN it.TYPE = 'O' THEN it.QTY ELSE 0 END)    AS SumOut,
    SUM(CASE WHEN it.TYPE = 'I' THEN it.QTY ELSE -it.QTY END) AS NetOnHand
FROM dbo.INVENTORY_TRANS it
WHERE it.PART_ID = @PartId;

PRINT '4 - LIFETIME TYPE/CLASS PROFILE (I/A must equal O/A)';
SELECT
    it.TYPE                     AS MovementType,
    it.CLASS                    AS MovementClass,
    COUNT(*)                    AS TransactionCount,
    SUM(it.QTY)                 AS TotalQty,
    MIN(it.TRANSACTION_DATE)    AS FirstTransaction,
    MAX(it.TRANSACTION_DATE)    AS LastTransaction
FROM dbo.INVENTORY_TRANS it
WHERE it.PART_ID = @PartId
GROUP BY it.TYPE, it.CLASS
ORDER BY it.TYPE, it.CLASS;

PRINT '5 - LIFETIME NET BY LOCATION';
SELECT
    it.WAREHOUSE_ID             AS WarehouseId,
    it.LOCATION_ID              AS LocationId,
    SUM(CASE WHEN it.TYPE = 'I' THEN it.QTY ELSE 0 END)     AS QtyIn,
    SUM(CASE WHEN it.TYPE = 'O' THEN it.QTY ELSE 0 END)     AS QtyOut,
    SUM(CASE WHEN it.TYPE = 'I' THEN it.QTY ELSE -it.QTY END) AS NetQty,
    COUNT(*)                    AS TransactionCount
FROM dbo.INVENTORY_TRANS it
WHERE it.PART_ID = @PartId
GROUP BY it.WAREHOUSE_ID, it.LOCATION_ID
ORDER BY SUM(CASE WHEN it.TYPE = 'I' THEN it.QTY ELSE -it.QTY END);

PRINT '6 - RUNNING-BALANCE LOW POINT PER LOCATION';
;WITH Movements AS (
    SELECT
        it.TRANSACTION_ID,
        it.TRANSACTION_DATE,
        it.WAREHOUSE_ID,
        it.LOCATION_ID,
        CASE WHEN it.TYPE = 'I' THEN it.QTY ELSE -it.QTY END AS Delta
    FROM dbo.INVENTORY_TRANS it
    WHERE it.PART_ID = @PartId
),
RunningBalances AS (
    SELECT
        Movements.WAREHOUSE_ID,
        Movements.LOCATION_ID,
        Movements.TRANSACTION_DATE,
        SUM(Movements.Delta) OVER (
            PARTITION BY Movements.WAREHOUSE_ID, Movements.LOCATION_ID
            ORDER BY Movements.TRANSACTION_DATE, Movements.TRANSACTION_ID
        ) AS RunningBalance
    FROM Movements
)
SELECT
    RunningBalances.WAREHOUSE_ID    AS WarehouseId,
    RunningBalances.LOCATION_ID     AS LocationId,
    MIN(RunningBalances.RunningBalance) AS WorstBalance,
    MIN(CASE WHEN RunningBalances.RunningBalance < 0
             THEN RunningBalances.TRANSACTION_DATE END) AS FirstNegativeDate,
    MAX(CASE WHEN RunningBalances.RunningBalance < 0
             THEN RunningBalances.TRANSACTION_DATE END) AS LastNegativeDate
FROM RunningBalances
GROUP BY RunningBalances.WAREHOUSE_ID, RunningBalances.LOCATION_ID
ORDER BY MIN(RunningBalances.RunningBalance);

PRINT '7 - UNPAIRED TRANSFERS (expect no rows)';
SELECT
    it.TRANSACTION_ID           AS TransactionId,
    it.TRANSFER_TRANS_ID        AS TransferTransactionId,
    it.TYPE                     AS MovementType,
    it.CLASS                    AS MovementClass,
    it.QTY,
    it.WAREHOUSE_ID             AS WarehouseId,
    it.LOCATION_ID              AS LocationId,
    it.TRANSACTION_DATE         AS TransactionDate
FROM dbo.INVENTORY_TRANS it
WHERE it.PART_ID = @PartId
  AND it.TRANSFER_TRANS_ID IS NOT NULL
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.INVENTORY_TRANS partner
        WHERE partner.TRANSACTION_ID = it.TRANSFER_TRANS_ID
  );

PRINT '8 - RECEIPT AUDIT (PostedTransactionId must never be NULL)';
SELECT
    r.ID                        AS ReceiverId,
    r.RECEIVED_DATE             AS ReceivedDate,
    r.USER_ID                   AS ReceivedBy,
    rl.PURC_ORDER_ID            AS PurchaseOrder,
    rl.PURC_ORDER_LINE_NO       AS PurchaseOrderLine,
    rl.RECEIVED_QTY             AS ReceivedQty,
    rl.REJECTED_QTY             AS RejectedQty,
    rl.TRANSACTION_ID           AS PostedTransactionId,
    CASE WHEN rl.TRANSACTION_ID IS NULL THEN 'NOT POSTED' ELSE 'posted' END AS PostingStatus,
    rl.WAREHOUSE_ID             AS WarehouseId,
    rl.LOCATION_ID              AS LocationId
FROM dbo.RECEIVER r
JOIN dbo.RECEIVER_LINE rl
     ON rl.RECEIVER_ID = r.ID
JOIN dbo.PURC_ORDER_LINE pl
     ON  RTRIM(pl.PURC_ORDER_ID) = RTRIM(rl.PURC_ORDER_ID)
     AND pl.LINE_NO = rl.PURC_ORDER_LINE_NO
WHERE pl.PART_ID = @PartId
ORDER BY r.RECEIVED_DATE;

PRINT '9 - WORK-ORDER CONSUMPTION VERSUS STATUS';
;WITH WorkOrderUsage AS (
    SELECT
        it.WORKORDER_BASE_ID                            AS WorkOrder,
        SUM(CASE WHEN it.TYPE = 'O' THEN it.QTY ELSE 0 END) AS IssuedQty,
        SUM(CASE WHEN it.TYPE = 'I' THEN it.QTY ELSE 0 END) AS ReturnedQty,
        MIN(it.TRANSACTION_DATE)                        AS FirstIssueDate,
        MAX(it.TRANSACTION_DATE)                        AS LastIssueDate
    FROM dbo.INVENTORY_TRANS it
    WHERE it.PART_ID = @PartId
      AND it.WORKORDER_BASE_ID IS NOT NULL
    GROUP BY it.WORKORDER_BASE_ID
)
SELECT
    u.WorkOrder                 AS WorkOrder,
    wo.STATUS                   AS WorkOrderStatus,
    wo.PART_ID                  AS ManufacturedPart,
    wo.DESIRED_QTY              AS DesiredQty,
    wo.RECEIVED_QTY             AS ReceivedQty,
    wo.CLOSE_DATE               AS CloseDate,
    u.IssuedQty                 AS IssuedQty,
    u.ReturnedQty               AS ReturnedQty,
    u.IssuedQty - u.ReturnedQty AS NetWrittenOff,
    u.FirstIssueDate,
    u.LastIssueDate
FROM WorkOrderUsage u
LEFT JOIN dbo.WORK_ORDER wo ON wo.BASE_ID = u.WorkOrder
ORDER BY u.IssuedQty - u.ReturnedQty DESC;

PRINT '10 - LABOR CLOCK-OUTS (set @WorkOrder above to use this)';
SELECT
    lt.TRANSACTION_ID           AS LaborTransactionId,
    lt.WORKORDER_BASE_ID        AS WorkOrder,
    lt.OPERATION_SEQ_NO         AS OperationSeq,
    lt.EMPLOYEE_ID              AS EmployeeId,
    lt.RESOURCE_ID              AS ResourceId,
    lt.GOOD_QTY                 AS GoodQty,
    lt.BAD_QTY                  AS BadQty,
    lt.CLOCK_IN                 AS ClockIn,
    lt.CLOCK_OUT                AS ClockOut,
    lt.USER_ID                  AS CapturedBy
FROM dbo.LABOR_TICKET lt
WHERE lt.WORKORDER_BASE_ID = @WorkOrder
ORDER BY lt.CLOCK_IN;

PRINT '11 - PURCHASE-ORDER LINES NEVER RECEIVED';
SELECT
    pol.PURC_ORDER_ID           AS PurchaseOrder,
    pol.LINE_NO                 AS PurchaseOrderLineNo,
    pol.ORDER_QTY               AS OrderedQty,
    pol.TOTAL_RECEIVED_QTY      AS TotalReceivedQty,
    pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY AS OutstandingQty,
    pol.LINE_STATUS             AS LineStatus,
    pol.DESIRED_RECV_DATE       AS DesiredReceiveDate,
    pol.LAST_RECEIVED_DATE      AS LastReceivedDate
FROM dbo.PURC_ORDER_LINE pol
WHERE pol.PART_ID = @PartId
ORDER BY pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY DESC, pol.PURC_ORDER_ID DESC;

PRINT '12 - MANUAL ADJUSTMENTS (CLASS A, no transfer partner, no WO/PO)';
SELECT
    it.TRANSACTION_ID           AS TransactionId,
    it.TRANSACTION_DATE         AS TransactionDate,
    it.CREATE_DATE              AS CreatedDate,
    it.TYPE                     AS MovementType,
    it.QTY,
    it.WAREHOUSE_ID             AS WarehouseId,
    it.LOCATION_ID              AS LocationId,
    it.USER_ID                  AS EnteredBy,
    it.DESCRIPTION              AS Description,
    it.ADJ_REASON_ID            AS AdjustmentReason,
    it.ISSUE_REAS_ID            AS IssueReason
FROM dbo.INVENTORY_TRANS it
WHERE it.PART_ID = @PartId
  AND it.CLASS = 'A'
  AND it.TRANSFER_TRANS_ID IS NULL
ORDER BY it.TRANSACTION_DATE DESC;

PRINT '13a - IS THE CYCLE-COUNT FEATURE USED AT ALL';
SELECT
    (SELECT COUNT(*) FROM dbo.CYCLE_COUNT_PART)                             AS CycleCountPartRows,
    (SELECT COUNT(*) FROM dbo.CR_PART_LOCATION WHERE LAST_COUNT_DATE IS NOT NULL) AS LocationsEverCounted,
    (SELECT COUNT(*) FROM dbo.PHYSICAL_COUNT)                               AS PhysicalCountBatches,
    (SELECT MAX(START_DATE) FROM dbo.PHYSICAL_COUNT)                        AS LastPhysicalCountStart;

PRINT '13b - IS THIS PART SET UP FOR CYCLE COUNTING (expect a row)';
SELECT
    ccp.PART_ID                 AS PartId,
    ccp.WAREHOUSE_ID            AS WarehouseId,
    ccp.COUNT_FREQ              AS CountFrequency,
    ccp.LAST_COUNT_DATE         AS LastCountDate
FROM dbo.CYCLE_COUNT_PART ccp
WHERE ccp.PART_ID = @PartId;

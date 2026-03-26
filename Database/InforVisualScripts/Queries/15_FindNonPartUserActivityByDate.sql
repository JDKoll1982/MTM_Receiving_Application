-- ========================================
-- Query: Find Non-Part Receiver Activity By Date
-- Description: Returns RECEIVER line rows for a specific Infor Visual user on a target date.
--              This is focused on the RECEIVER table because non-inventoried receipts appear to be
--              recorded there by RECEIVER.USER_ID, while the specific received line is stored in
--              RECEIVER_LINE and must be used to pull the correct PO line spec.
-- Database: MTMFG (Infor Visual)
-- Server: VISUAL
-- READ ONLY: No writes. Safe to run in SQL Server Management Studio.
-- ========================================

DECLARE @TargetUserId nvarchar(20) = N'JKOLL';
DECLARE @TargetDate date = '2026-03-24';

IF OBJECT_ID(N'dbo.RECEIVER', N'U') IS NULL
BEGIN
    THROW 50000, 'RECEIVER table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID(N'dbo.RECEIVER_LINE', N'U') IS NULL
BEGIN
    THROW 50001, 'RECEIVER_LINE table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID(N'dbo.PURCHASE_ORDER', N'U') IS NULL
BEGIN
    THROW 50002, 'PURCHASE_ORDER table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID(N'dbo.VENDOR', N'U') IS NULL
BEGIN
    THROW 50003, 'VENDOR table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID(N'dbo.INVENTORY_TRANS', N'U') IS NULL
BEGIN
    THROW 50004, 'INVENTORY_TRANS table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID(N'dbo.PAYABLE_LINE', N'U') IS NULL
BEGIN
    THROW 50005, 'PAYABLE_LINE table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID(N'dbo.SERVICE_RECEIPT', N'U') IS NULL
BEGIN
    THROW 50006, 'SERVICE_RECEIPT table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID(N'dbo.DEMAND_SUPPLY_LINK', N'U') IS NULL
BEGIN
    THROW 50007, 'DEMAND_SUPPLY_LINK table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID(N'dbo.WORK_ORDER', N'U') IS NULL
BEGIN
    THROW 50008, 'WORK_ORDER table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF OBJECT_ID('tempdb..#ResolvedUserIds') IS NOT NULL
BEGIN
    DROP TABLE #ResolvedUserIds;
END;

CREATE TABLE #ResolvedUserIds
(
    UserId nvarchar(20) NOT NULL PRIMARY KEY
);

INSERT INTO #ResolvedUserIds (UserId)
SELECT LTRIM(RTRIM(@TargetUserId))
WHERE LTRIM(RTRIM(@TargetUserId)) <> N'';

IF OBJECT_ID(N'dbo.EMPLOYEE', N'U') IS NOT NULL
BEGIN
        INSERT INTO #ResolvedUserIds (UserId)
        SELECT DISTINCT resolved.ResolvedUserId
        FROM
        (
                SELECT LTRIM(RTRIM(e.USER_ID)) AS ResolvedUserId
                FROM dbo.EMPLOYEE AS e
                WHERE e.ID = @TargetUserId
                    AND e.USER_ID IS NOT NULL

                UNION

                SELECT LTRIM(RTRIM(e.USER_ID)) AS ResolvedUserId
                FROM dbo.EMPLOYEE AS e
                WHERE e.USER_ID = @TargetUserId
                    AND e.USER_ID IS NOT NULL
        ) AS resolved
        WHERE resolved.ResolvedUserId <> N''
            AND NOT EXISTS
            (
                    SELECT 1
                    FROM #ResolvedUserIds AS existing
                    WHERE existing.UserId = resolved.ResolvedUserId
            );
END;

IF OBJECT_ID('tempdb..#BaseRows') IS NOT NULL
BEGIN
    DROP TABLE #BaseRows;
END;

CREATE TABLE #BaseRows
(
    UserId nvarchar(20) NULL,
    MatchedDateTime datetime NULL,
    ReceiverId nvarchar(15) NULL,
    PurchaseOrderId nvarchar(15) NULL,
    PurchaseOrderLineNo smallint NULL,
    UserReceivedQty int NULL,
    SiteId nvarchar(15) NULL,
    VendorName nvarchar(255) NULL,
    CustomerOrderId nvarchar(15) NULL,
    CustomerOrderLineNo smallint NULL,
    WorkOrderType nchar(1) NULL,
    WorkOrderBaseId nvarchar(30) NULL,
    WorkOrderLotId nvarchar(3) NULL,
    WorkOrderSplitId nvarchar(3) NULL,
    WorkOrderSubId nvarchar(3) NULL,
    POLineSpecText nvarchar(max) NULL
);

INSERT INTO #BaseRows
(
    UserId,
    MatchedDateTime,
    ReceiverId,
    PurchaseOrderId,
    PurchaseOrderLineNo,
    UserReceivedQty,
    SiteId,
    VendorName,
    CustomerOrderId,
    CustomerOrderLineNo,
    WorkOrderType,
    WorkOrderBaseId,
    WorkOrderLotId,
    WorkOrderSplitId,
    WorkOrderSubId,
    POLineSpecText
)
SELECT
    r.USER_ID AS UserId,
    r.CREATE_DATE AS MatchedDateTime,
    r.ID AS ReceiverId,
    r.PURC_ORDER_ID AS PurchaseOrderId,
    rl.PURC_ORDER_LINE_NO AS PurchaseOrderLineNo,
    CAST(ROUND(rl.USER_RECEIVED_QTY, 0) AS int) AS UserReceivedQty,
    r.SITE_ID AS SiteId,
    v.NAME AS VendorName,
    COALESCE(payLine.CUST_ORDER_ID, itTxn.CUST_ORDER_ID, itServ.CUST_ORDER_ID) AS CustomerOrderId,
    COALESCE(itTxn.CUST_ORDER_LINE_NO, itServ.CUST_ORDER_LINE_NO) AS CustomerOrderLineNo,
    COALESCE(workOrderLink.WorkOrderType, serviceReceipt.WORKORDER_TYPE, payLine.WORKORDER_TYPE, itTxn.WORKORDER_TYPE, itServ.WORKORDER_TYPE) AS WorkOrderType,
    COALESCE(workOrderLink.WorkOrderBaseId, serviceReceipt.WORKORDER_BASE_ID, payLine.WORKORDER_BASE_ID, itTxn.WORKORDER_BASE_ID, itServ.WORKORDER_BASE_ID) AS WorkOrderBaseId,
    COALESCE(workOrderLink.WorkOrderLotId, serviceReceipt.WORKORDER_LOT_ID, payLine.WORKORDER_LOT_ID, itTxn.WORKORDER_LOT_ID, itServ.WORKORDER_LOT_ID) AS WorkOrderLotId,
    COALESCE(workOrderLink.WorkOrderSplitId, serviceReceipt.WORKORDER_SPLIT_ID, payLine.WORKORDER_SPLIT_ID, itTxn.WORKORDER_SPLIT_ID, itServ.WORKORDER_SPLIT_ID) AS WorkOrderSplitId,
    COALESCE(workOrderLink.WorkOrderSubId, serviceReceipt.WORKORDER_SUB_ID, payLine.WORKORDER_SUB_ID, itTxn.WORKORDER_SUB_ID, itServ.WORKORDER_SUB_ID) AS WorkOrderSubId,
    CASE
        WHEN OBJECT_ID(N'dbo.PURC_LINE_BINARY', N'U') IS NULL THEN CAST(NULL AS nvarchar(max))
        ELSE
        (
            SELECT STUFF
            (
                (
                    SELECT ' | ' + specs.SpecText
                    FROM
                    (
                        SELECT DISTINCT
                            CONVERT(nvarchar(max), CONVERT(varbinary(max), plb.BITS)) AS SpecText
                        FROM dbo.PURC_LINE_BINARY AS plb
                        WHERE plb.PURC_ORDER_ID = rl.PURC_ORDER_ID
                          AND plb.PURC_ORDER_LINE_NO = rl.PURC_ORDER_LINE_NO
                          AND plb.TYPE = 'D'
                    ) AS specs
                    FOR XML PATH(''), TYPE
                ).value('.', 'nvarchar(max)'),
                1,
                3,
                ''
            )
        )
    END AS POLineSpecText
FROM dbo.RECEIVER AS r
INNER JOIN dbo.RECEIVER_LINE AS rl
    ON rl.RECEIVER_ID = r.ID
LEFT JOIN dbo.PURCHASE_ORDER AS po
    ON po.ID = r.PURC_ORDER_ID
LEFT JOIN dbo.VENDOR AS v
    ON v.ID = po.VENDOR_ID
OUTER APPLY
(
    SELECT TOP (1)
        wo.TYPE AS WorkOrderType,
        wo.BASE_ID AS WorkOrderBaseId,
        wo.LOT_ID AS WorkOrderLotId,
        wo.SPLIT_ID AS WorkOrderSplitId,
        wo.SUB_ID AS WorkOrderSubId
    FROM dbo.DEMAND_SUPPLY_LINK AS dsl
    INNER JOIN dbo.WORK_ORDER AS wo
        ON wo.BASE_ID = dsl.DEMAND_BASE_ID
       AND wo.LOT_ID = ISNULL(dsl.DEMAND_LOT_ID, N'')
       AND wo.SPLIT_ID = ISNULL(dsl.DEMAND_SPLIT_ID, N'')
       AND wo.SUB_ID = ISNULL(dsl.DEMAND_SUB_ID, N'')
    WHERE dsl.SUPPLY_BASE_ID = rl.PURC_ORDER_ID
      AND dsl.SUPPLY_NO = rl.PURC_ORDER_LINE_NO
    ORDER BY dsl.CREATE_DATE DESC, dsl.ID DESC
) AS workOrderLink
OUTER APPLY
(
    SELECT TOP (1)
        pl.CUST_ORDER_ID,
        pl.WORKORDER_TYPE,
        pl.WORKORDER_BASE_ID,
        pl.WORKORDER_LOT_ID,
        pl.WORKORDER_SPLIT_ID,
        pl.WORKORDER_SUB_ID
    FROM dbo.PAYABLE_LINE AS pl
    WHERE
        (
            pl.RECEIVER_ID = rl.RECEIVER_ID
            AND pl.RECEIVER_LINE_NO = rl.LINE_NO
        )
        OR
        (
            pl.PURC_ORDER_ID = rl.PURC_ORDER_ID
            AND pl.PURC_ORDER_LINE_NO = rl.PURC_ORDER_LINE_NO
        )
    ORDER BY
        CASE
            WHEN pl.RECEIVER_ID = rl.RECEIVER_ID
             AND pl.RECEIVER_LINE_NO = rl.LINE_NO THEN 0
            ELSE 1
        END,
        pl.VOUCHER_ID DESC,
        pl.LINE_NO DESC
) AS payLine
OUTER APPLY
(
    SELECT TOP (1)
        sr.WORKORDER_TYPE,
        sr.WORKORDER_BASE_ID,
        sr.WORKORDER_LOT_ID,
        sr.WORKORDER_SPLIT_ID,
        sr.WORKORDER_SUB_ID
    FROM dbo.SERVICE_RECEIPT AS sr
    WHERE
        (
            rl.SERV_TRANS_ID IS NOT NULL
            AND sr.TRANSACTION_ID = rl.SERV_TRANS_ID
        )
        OR
        (
            sr.RECEIVER_ID = rl.RECEIVER_ID
            AND sr.RECEIVER_LINE_NO = rl.LINE_NO
        )
        OR
        (
            sr.PURC_ORDER_ID = rl.PURC_ORDER_ID
            AND sr.PURC_ORDER_LINE_NO = rl.PURC_ORDER_LINE_NO
        )
    ORDER BY
        CASE
            WHEN rl.SERV_TRANS_ID IS NOT NULL AND sr.TRANSACTION_ID = rl.SERV_TRANS_ID THEN 0
            WHEN sr.RECEIVER_ID = rl.RECEIVER_ID
             AND sr.RECEIVER_LINE_NO = rl.LINE_NO THEN 1
            ELSE 2
        END,
        sr.CREATE_DATE DESC,
        sr.TRANSACTION_ID DESC
) AS serviceReceipt
LEFT JOIN dbo.INVENTORY_TRANS AS itTxn
    ON itTxn.TRANSACTION_ID = rl.TRANSACTION_ID
LEFT JOIN dbo.INVENTORY_TRANS AS itServ
    ON itServ.TRANSACTION_ID = rl.SERV_TRANS_ID
WHERE EXISTS
(
    SELECT 1
    FROM #ResolvedUserIds AS resolved
    WHERE resolved.UserId = LTRIM(RTRIM(CONVERT(nvarchar(20), r.USER_ID)))
)
  AND r.RECEIVED_DATE >= @TargetDate
  AND r.RECEIVED_DATE < DATEADD(day, 1, @TargetDate);

SELECT
    UserId,
    CONVERT(varchar(10), MatchedDateTime, 23) AS MatchedDate,
    STUFF(RIGHT('0' + LTRIM(RIGHT(CONVERT(varchar(20), MatchedDateTime, 100), 7)), 7), 6, 0, ' ') AS MatchedTime,
    ReceiverId,
    PurchaseOrderId,
    PurchaseOrderLineNo,
    UserReceivedQty,
    SiteId,
    VendorName,
    POLineSpecText
FROM #BaseRows
WHERE CustomerOrderId IS NULL
  AND WorkOrderBaseId IS NULL
ORDER BY MatchedDateTime, ReceiverId, PurchaseOrderLineNo;

SELECT
    UserId,
    CONVERT(varchar(10), MatchedDateTime, 23) AS MatchedDate,
    STUFF(RIGHT('0' + LTRIM(RIGHT(CONVERT(varchar(20), MatchedDateTime, 100), 7)), 7), 6, 0, ' ') AS MatchedTime,
    ReceiverId,
    PurchaseOrderId,
    PurchaseOrderLineNo,
    UserReceivedQty,
    SiteId,
    VendorName,
    CustomerOrderId,
    CustomerOrderLineNo,
    WorkOrderType,
    WorkOrderBaseId,
    WorkOrderLotId,
    WorkOrderSplitId,
    WorkOrderSubId,
    POLineSpecText
FROM #BaseRows
WHERE CustomerOrderId IS NOT NULL
   OR WorkOrderBaseId IS NOT NULL
ORDER BY MatchedDateTime, ReceiverId, PurchaseOrderLineNo;

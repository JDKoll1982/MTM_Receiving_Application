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
DECLARE @ResolveUserSql nvarchar(max);
DECLARE @MainQuerySql nvarchar(max);
DECLARE @ReceiverObjectName nvarchar(258);
DECLARE @ReceiverLineObjectName nvarchar(258);
DECLARE @PurcLineBinaryObjectName nvarchar(258);
DECLARE @EmployeeObjectName nvarchar(258);
DECLARE @PurchaseOrderObjectName nvarchar(258);
DECLARE @VendorObjectName nvarchar(258);
DECLARE @InventoryTransObjectName nvarchar(258);

SELECT TOP (1)
    @ReceiverObjectName = QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE t.name = N'RECEIVER'
ORDER BY CASE WHEN s.name = N'dbo' THEN 0 ELSE 1 END, s.name;

SELECT TOP (1)
    @ReceiverLineObjectName = QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE t.name = N'RECEIVER_LINE'
ORDER BY CASE WHEN s.name = N'dbo' THEN 0 ELSE 1 END, s.name;

SELECT TOP (1)
    @PurcLineBinaryObjectName = QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE t.name = N'PURC_LINE_BINARY'
ORDER BY CASE WHEN s.name = N'dbo' THEN 0 ELSE 1 END, s.name;

SELECT TOP (1)
    @EmployeeObjectName = QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE t.name = N'EMPLOYEE'
ORDER BY CASE WHEN s.name = N'dbo' THEN 0 ELSE 1 END, s.name;

SELECT TOP (1)
    @PurchaseOrderObjectName = QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE t.name = N'PURCHASE_ORDER'
ORDER BY CASE WHEN s.name = N'dbo' THEN 0 ELSE 1 END, s.name;

SELECT TOP (1)
    @VendorObjectName = QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE t.name = N'VENDOR'
ORDER BY CASE WHEN s.name = N'dbo' THEN 0 ELSE 1 END, s.name;

SELECT TOP (1)
    @InventoryTransObjectName = QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
FROM sys.tables AS t
INNER JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE t.name = N'INVENTORY_TRANS'
ORDER BY CASE WHEN s.name = N'dbo' THEN 0 ELSE 1 END, s.name;

IF @ReceiverObjectName IS NULL
BEGIN
    THROW 50000, 'RECEIVER table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
END;

IF @ReceiverLineObjectName IS NULL
BEGIN
    THROW 50001, 'RECEIVER_LINE table was not found in the current database. Verify you are connected to the correct MTMFG database.', 1;
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

IF @EmployeeObjectName IS NOT NULL
BEGIN
    SET @ResolveUserSql = N'
    INSERT INTO #ResolvedUserIds (UserId)
    SELECT DISTINCT resolved.ResolvedUserId
    FROM
    (
        SELECT LTRIM(RTRIM(e.USER_ID)) AS ResolvedUserId
                FROM ' + @EmployeeObjectName + N' AS e
        WHERE e.ID = @TargetUserId
          AND e.USER_ID IS NOT NULL

        UNION

        SELECT LTRIM(RTRIM(e.USER_ID)) AS ResolvedUserId
                FROM ' + @EmployeeObjectName + N' AS e
        WHERE e.USER_ID = @TargetUserId
          AND e.USER_ID IS NOT NULL
    ) AS resolved
    WHERE resolved.ResolvedUserId <> N''''
      AND NOT EXISTS
      (
          SELECT 1
          FROM #ResolvedUserIds AS existing
          WHERE existing.UserId = resolved.ResolvedUserId
      );';

    EXEC sys.sp_executesql
        @ResolveUserSql,
        N'@TargetUserId nvarchar(20)',
        @TargetUserId = @TargetUserId;
END;

SET @MainQuerySql = N'
IF OBJECT_ID(''tempdb..#BaseRows'') IS NOT NULL
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
    ' + CASE
        WHEN @PurchaseOrderObjectName IS NOT NULL AND @VendorObjectName IS NOT NULL THEN N'v.NAME'
        ELSE N'CAST(NULL AS nvarchar(255))'
    END + N' AS VendorName,
    ' + CASE
        WHEN @InventoryTransObjectName IS NOT NULL THEN N'COALESCE(itTxn.CUST_ORDER_ID, itServ.CUST_ORDER_ID, itFallback.CUST_ORDER_ID)'
        ELSE N'CAST(NULL AS nvarchar(15))'
    END + N' AS CustomerOrderId,
    ' + CASE
        WHEN @InventoryTransObjectName IS NOT NULL THEN N'COALESCE(itTxn.CUST_ORDER_LINE_NO, itServ.CUST_ORDER_LINE_NO, itFallback.CUST_ORDER_LINE_NO)'
        ELSE N'CAST(NULL AS smallint)'
    END + N' AS CustomerOrderLineNo,
    ' + CASE
        WHEN @InventoryTransObjectName IS NOT NULL THEN N'COALESCE(itTxn.WORKORDER_TYPE, itServ.WORKORDER_TYPE, itFallback.WORKORDER_TYPE)'
        ELSE N'CAST(NULL AS nchar(1))'
    END + N' AS WorkOrderType,
    ' + CASE
        WHEN @InventoryTransObjectName IS NOT NULL THEN N'COALESCE(itTxn.WORKORDER_BASE_ID, itServ.WORKORDER_BASE_ID, itFallback.WORKORDER_BASE_ID)'
        ELSE N'CAST(NULL AS nvarchar(30))'
    END + N' AS WorkOrderBaseId,
    ' + CASE
        WHEN @InventoryTransObjectName IS NOT NULL THEN N'COALESCE(itTxn.WORKORDER_LOT_ID, itServ.WORKORDER_LOT_ID, itFallback.WORKORDER_LOT_ID)'
        ELSE N'CAST(NULL AS nvarchar(3))'
    END + N' AS WorkOrderLotId,
    ' + CASE
        WHEN @InventoryTransObjectName IS NOT NULL THEN N'COALESCE(itTxn.WORKORDER_SPLIT_ID, itServ.WORKORDER_SPLIT_ID, itFallback.WORKORDER_SPLIT_ID)'
        ELSE N'CAST(NULL AS nvarchar(3))'
    END + N' AS WorkOrderSplitId,
    ' + CASE
        WHEN @InventoryTransObjectName IS NOT NULL THEN N'COALESCE(itTxn.WORKORDER_SUB_ID, itServ.WORKORDER_SUB_ID, itFallback.WORKORDER_SUB_ID)'
        ELSE N'CAST(NULL AS nvarchar(3))'
    END + N' AS WorkOrderSubId,
    ' + CASE
        WHEN @PurcLineBinaryObjectName IS NULL THEN N'CAST(NULL AS nvarchar(max))'
        ELSE N'(
        SELECT STUFF
        (
            (
                SELECT '' | '' + specs.SpecText
                FROM
                (
                    SELECT DISTINCT
                        CONVERT(nvarchar(max), CONVERT(varbinary(max), plb.BITS)) AS SpecText
                    FROM ' + @PurcLineBinaryObjectName + N' AS plb
                    WHERE plb.PURC_ORDER_ID = rl.PURC_ORDER_ID
                      AND plb.PURC_ORDER_LINE_NO = rl.PURC_ORDER_LINE_NO
                      AND plb.TYPE = ''D''
                ) AS specs
                FOR XML PATH(''''), TYPE
            ).value(''.'', ''nvarchar(max)''),
            1,
            3,
            ''''
        )
    )'
    END + N' AS POLineSpecText
FROM ' + @ReceiverObjectName + N' AS r
INNER JOIN ' + @ReceiverLineObjectName + N' AS rl
    ON rl.RECEIVER_ID = r.ID
' + CASE
        WHEN @PurchaseOrderObjectName IS NOT NULL THEN N'LEFT JOIN ' + @PurchaseOrderObjectName + N' AS po
    ON po.ID = r.PURC_ORDER_ID
'
        ELSE N''
    END + CASE
        WHEN @PurchaseOrderObjectName IS NOT NULL AND @VendorObjectName IS NOT NULL THEN N'LEFT JOIN ' + @VendorObjectName + N' AS v
    ON v.ID = po.VENDOR_ID
'
        ELSE N''
    END + CASE
        WHEN @InventoryTransObjectName IS NOT NULL THEN N'LEFT JOIN ' + @InventoryTransObjectName + N' AS itTxn
    ON itTxn.TRANSACTION_ID = rl.TRANSACTION_ID
LEFT JOIN ' + @InventoryTransObjectName + N' AS itServ
    ON itServ.TRANSACTION_ID = rl.SERV_TRANS_ID
LEFT JOIN
(
    SELECT
        fallbackKey.PURC_ORDER_ID,
        fallbackKey.PURC_ORDER_LINE_NO,
        MAX(fallbackKey.TRANSACTION_ID) AS FallbackTransactionId
    FROM ' + @InventoryTransObjectName + N' AS fallbackKey
    WHERE fallbackKey.TRANSACTION_DATE >= @TargetDate
      AND fallbackKey.TRANSACTION_DATE < DATEADD(day, 1, @TargetDate)
      AND
      (
          fallbackKey.CUST_ORDER_ID IS NOT NULL
          OR fallbackKey.WORKORDER_BASE_ID IS NOT NULL
      )
    GROUP BY
        fallbackKey.PURC_ORDER_ID,
        fallbackKey.PURC_ORDER_LINE_NO
) AS fallbackPick
    ON fallbackPick.PURC_ORDER_ID = rl.PURC_ORDER_ID
   AND fallbackPick.PURC_ORDER_LINE_NO = rl.PURC_ORDER_LINE_NO
LEFT JOIN ' + @InventoryTransObjectName + N' AS itFallback
    ON itFallback.TRANSACTION_ID = fallbackPick.FallbackTransactionId
'
        ELSE N''
    END + N'WHERE EXISTS
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
    STUFF(RIGHT(''0'' + LTRIM(RIGHT(CONVERT(varchar(20), MatchedDateTime, 100), 7)), 7), 6, 0, '' '') AS MatchedTime,
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
    STUFF(RIGHT(''0'' + LTRIM(RIGHT(CONVERT(varchar(20), MatchedDateTime, 100), 7)), 7), 6, 0, '' '') AS MatchedTime,
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
ORDER BY MatchedDateTime, ReceiverId, PurchaseOrderLineNo;';

EXEC sys.sp_executesql
    @MainQuerySql,
    N'@TargetDate date',
    @TargetDate = @TargetDate;

/*
Receiving Analytics grid — per-PO-line receiving schedule rows within an optional
date window, with search, scope (parts/MMC coils/MMF flat/outside service),
delivery-state (open/partial/closed), and PO-state (on time/late) filters.

Read-only Infor Visual (MTMFG). Parameters expected from C#:

  @FromDate          DATETIME    -- inclusive lower bound on the due date; NULL = no bound
  @ToDate            DATETIME    -- inclusive upper bound on the due date; NULL = no bound
  @PartSearch        NVARCHAR(60)  -- '%term%' against PART_ID (optional)
  @PoSearch          NVARCHAR(15)  -- '%term%' against PURCHASE_ORDER.ID (optional)
  @SupplierSearch    NVARCHAR(60)  -- '%term%' against VENDOR.NAME (optional)
  @CarrierSearch     NVARCHAR(60)  -- '%term%' against RECEIVER.CARRIER_ID (optional)
  @SearchAll         BIT           -- 1 = search any non-empty term; 0 = no search filter
  @ScopeParts        BIT           -- 1 = show Parts lines (inclusion)
  @ScopeCoils        BIT           -- 1 = show MMC (coils) lines (inclusion)
  @ScopeFlat         BIT           -- 1 = show MMF (flat stock) lines (inclusion)
  @ScopeOutside      BIT           -- 1 = show Outside Service lines (inclusion)
  @ScopeUninv        BIT           -- 1 = show Uninventoried (no part) lines (inclusion)
  @ShowNearFilled    BIT           -- 1 = only show lines with recv/order % >= @NearFillPct
  @NearFillPct       INT           -- 1-99 threshold for the near-filled filter (default 90)
  @ShowOpen          BIT           -- 1 = include lines with no receipts yet
  @ShowPartial       BIT           -- 1 = include partially received lines
  @ShowClosed        BIT           -- 1 = include fully received lines
  @ShowOnTime        BIT           -- 1 = include lines due on/before today that are on time
  @ShowLate          BIT           -- 1 = include lines past due with remaining qty
  @Today             DATETIME      -- reference "now" date for on-time/late classification
  @MaxResults        INT           -- safety cap on returned rows
*/

WITH ReceivingLines AS (
    SELECT
        po.ID                                   AS PoNumber,
        ISNULL(v.NAME, '')                      AS VendorName,
        po.DESIRED_RECV_DATE                    AS PoDesiredDate,
        po.PROMISE_DATE                         AS PoPromiseDate,
        po.ORDER_DATE                           AS OrderDate,
        ISNULL(po.SHIP_VIA, '')                 AS Carrier,
        ISNULL(pol.PART_ID, '')                 AS PartNumber,
        pol.USER_ORDER_QTY                      AS OrderQty,
        pol.TOTAL_RECEIVED_QTY                  AS ReceivedQty,
        (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) AS RemainingQty,
        pol.DESIRED_RECV_DATE                   AS LineDesiredDate,
        pol.PROMISE_DATE                        AS LinePromiseDate,
        CAST(po.STATUS AS NVARCHAR(10))         AS PoStatus,
        CAST(pol.LINE_STATUS AS NVARCHAR(10))   AS LineStatus,
        ISNULL(r.USER_ID, '')                   AS ReceivedBy,
        CAST(COALESCE(
            pol.DESIRED_RECV_DATE, pol.PROMISE_DATE,
            po.DESIRED_RECV_DATE, po.PROMISE_DATE
        ) AS DATE)                              AS DueDate,
        CASE
            WHEN pol.SERVICE_ID IS NOT NULL THEN 'Outside Service'
            WHEN pol.PART_ID IS NULL OR p.DETAIL_ONLY = 'Y' THEN 'Uninventoried'
            WHEN pol.PART_ID LIKE 'MMC%' THEN 'MMC Coils'
            WHEN pol.PART_ID LIKE 'MMF%' THEN 'MMF Flat'
            ELSE 'Parts'
        END                                     AS Category,
        CASE
            WHEN pol.TOTAL_RECEIVED_QTY >= pol.ORDER_QTY
                 AND pol.ORDER_QTY > 0 THEN 'Closed'
            WHEN pol.TOTAL_RECEIVED_QTY > 0 THEN 'Partial'
            ELSE 'Open'
        END                                     AS DeliveryState,
        CASE
            WHEN pol.TOTAL_RECEIVED_QTY < pol.ORDER_QTY
                 AND CAST(COALESCE(
                     pol.DESIRED_RECV_DATE, pol.PROMISE_DATE,
                     po.DESIRED_RECV_DATE, po.PROMISE_DATE
                 ) AS DATE) < CAST(@Today AS DATE) THEN 'Late'
            ELSE 'OnTime'
        END                                     AS PoState
    FROM dbo.PURCHASE_ORDER po
    INNER JOIN dbo.PURC_ORDER_LINE pol ON po.ID = pol.PURC_ORDER_ID
    LEFT JOIN dbo.VENDOR v ON po.VENDOR_ID = v.ID
    LEFT JOIN dbo.PART p ON pol.PART_ID = p.ID
    OUTER APPLY (
        SELECT TOP 1 rl2.RECEIVER_ID
        FROM dbo.RECEIVER_LINE rl2
        WHERE rl2.PURC_ORDER_ID = pol.PURC_ORDER_ID
          AND rl2.PURC_ORDER_LINE_NO = pol.LINE_NO
        ORDER BY rl2.ROWID DESC
    ) latest_rl
    LEFT JOIN dbo.RECEIVER r ON r.ID = latest_rl.RECEIVER_ID
)
SELECT TOP (@MaxResults)
    PoNumber,
    VendorName,
    PoDesiredDate,
    PoPromiseDate,
    OrderDate,
    Carrier,
    PartNumber,
    OrderQty,
    ReceivedQty,
    RemainingQty,
    LineDesiredDate,
    LinePromiseDate,
    PoStatus,
    LineStatus,
    ReceivedBy,
    DueDate,
    Category,
    DeliveryState,
    PoState
FROM ReceivingLines
WHERE
    (@FromDate IS NULL OR DueDate >= CAST(@FromDate AS DATE))
    AND (@ToDate IS NULL OR DueDate <= CAST(@ToDate AS DATE))
    -- Search: when @SearchAll is on, any non-empty term must match (OR semantics).
    -- Empty terms never match (they are ignored), so a single typed term filters correctly.
    AND (
        @SearchAll = 0
        OR (ISNULL(@PartSearch, '') <> '' AND PartNumber LIKE @PartSearch)
        OR (ISNULL(@PoSearch, '') <> '' AND PoNumber LIKE @PoSearch)
        OR (ISNULL(@SupplierSearch, '') <> '' AND VendorName LIKE @SupplierSearch)
        OR (ISNULL(@CarrierSearch, '') <> '' AND EXISTS (
            SELECT 1
            FROM dbo.RECEIVER rc
            WHERE rc.PURC_ORDER_ID = PoNumber
              AND rc.CARRIER_ID LIKE @CarrierSearch
        ))
    )
    -- Scope: inclusion semantics — show ONLY the checked categories.
    AND (
        (@ScopeParts = 1 AND Category = 'Parts')
        OR (@ScopeCoils = 1 AND Category = 'MMC Coils')
        OR (@ScopeFlat = 1 AND Category = 'MMF Flat')
        OR (@ScopeOutside = 1 AND Category = 'Outside Service')
        OR (@ScopeUninv = 1 AND Category = 'Uninventoried')
    )
    -- Ignore lines with a zero order quantity.
    AND OrderQty > 0
    -- Show Partials Below (toggle): when checked, keep only PARTIAL lines whose
    -- received% is BELOW the threshold. When unchecked, only show rows with recv qty 0.
    AND (
        (
            @ShowNearFilled = 1
            AND DeliveryState = 'Partial'
            AND OrderQty > 0
            AND (ReceivedQty * 100.0 / NULLIF(OrderQty, 0)) < @NearFillPct
        )
        OR
        (
            @ShowNearFilled = 0
            AND ReceivedQty = 0
        )
    )
    AND (@ShowOpen = 1 OR DeliveryState <> 'Open')
    AND (@ShowClosed = 1 OR DeliveryState <> 'Closed')
    AND (@ShowOnTime = 1 OR PoState <> 'OnTime')
    AND (@ShowLate = 1 OR PoState <> 'Late')
ORDER BY DueDate, PoNumber, PartNumber;

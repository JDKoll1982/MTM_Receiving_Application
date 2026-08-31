/*
Receiving Analytics — FORECAST dataset (incoming items).

Counts open purchase-order lines due on each calendar date, grouped by category
(Parts / MMC Coils / MMF Flat / Outside Service / Uninventoried). Mirrors the WIP
app's forecast query: open lines (remaining qty > 0, not closed) where the due
date falls in range. Service lines anchor on Desired Recv Date; all other lines
anchor on Promise Date (with a fallback chain to the line/header dates).

Read-only Infor Visual (MTMFG). Parameters expected from C#:

  @FromDate     DATETIME   -- inclusive lower bound on due date; NULL = no bound
  @ToDate       DATETIME   -- inclusive upper bound on due date; NULL = no bound
  @ScopeParts   BIT        -- 1 = include regular Parts
  @ScopeCoils   BIT        -- 1 = include MMC (coils)
  @ScopeFlat    BIT        -- 1 = include MMF (flat stock)
  @ScopeOutside BIT        -- 1 = include Outside Service lines
  @ScopeUninv   BIT        -- 1 = include Uninventoried lines
  @MaxResults   INT        -- safety cap on returned rows
*/

WITH ForecastByDate AS (
    SELECT
        CASE
            WHEN pol.SERVICE_ID IS NOT NULL THEN COALESCE(
                pol.DESIRED_RECV_DATE,
                po.DESIRED_RECV_DATE,
                pol.PROMISE_DATE,
                po.PROMISE_DATE
            )
            ELSE COALESCE(
                pol.PROMISE_DATE,
                po.PROMISE_DATE,
                pol.DESIRED_RECV_DATE,
                po.DESIRED_RECV_DATE
            )
        END AS DueDate,
        CASE
            WHEN pol.SERVICE_ID IS NOT NULL THEN 'Outside Service'
            WHEN pol.PART_ID IS NULL OR p.DETAIL_ONLY = 'Y' THEN 'Uninventoried'
            WHEN pol.PART_ID LIKE 'MMC%' THEN 'MMC Coils'
            WHEN pol.PART_ID LIKE 'MMF%' THEN 'MMF Flat'
            ELSE 'Parts'
        END AS Category
    FROM dbo.PURC_ORDER_LINE pol
    INNER JOIN dbo.PURCHASE_ORDER po ON po.ID = pol.PURC_ORDER_ID
    LEFT JOIN dbo.PART p ON pol.PART_ID = p.ID
    WHERE (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) > 0
      AND pol.LINE_STATUS <> 'C'
      AND po.STATUS <> 'C'
      AND ISNULL(po.CONSIGNMENT, 'N') <> 'Y'
      AND ISNULL(po.INTERNAL_ORDER, 'N') <> 'Y'
),
ForecastGrouped AS (
    SELECT
        CAST(DueDate AS DATE) AS ActivityDate,
        Category,
        COUNT(*) AS LineCount
    FROM ForecastByDate
    WHERE DueDate IS NOT NULL
    GROUP BY CAST(DueDate AS DATE), Category
)
SELECT TOP (@MaxResults)
    ActivityDate,
    Category,
    LineCount
FROM ForecastGrouped
WHERE
    (@FromDate IS NULL OR ActivityDate >= CAST(@FromDate AS DATE))
    AND (@ToDate IS NULL OR ActivityDate <= CAST(@ToDate AS DATE))
    AND (@ScopeParts = 1 OR Category <> 'Parts')
    AND (@ScopeCoils = 1 OR Category <> 'MMC Coils')
    AND (@ScopeFlat = 1 OR Category <> 'MMF Flat')
    AND (@ScopeOutside = 1 OR Category <> 'Outside Service')
    AND (@ScopeUninv = 1 OR Category <> 'Uninventoried')
ORDER BY ActivityDate, Category;

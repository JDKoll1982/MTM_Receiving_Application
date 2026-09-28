/*
Receiving Analytics — HISTORY dataset (past received items).

Counts receiving lines whose receipt activity falls on each calendar date,
grouped by category (Parts / MMC Coils / MMF Flat / Outside Service /
Uninventoried). Mirrors the WIP app's history query: lines where
TOTAL_RECEIVED_QTY > 0 anchored on PURC_ORDER_LINE.LAST_RECEIVED_DATE.

Read-only Infor Visual (MTMFG). Parameters expected from C#:

  @FromDate     DATETIME   -- inclusive lower bound on received date; NULL = no bound
  @ToDate       DATETIME   -- inclusive upper bound on received date; NULL = no bound
  @ScopeParts   BIT        -- 1 = include regular Parts
  @ScopeCoils   BIT        -- 1 = include MMC (coils)
  @ScopeFlat    BIT        -- 1 = include MMF (flat stock)
  @ScopeOutside BIT        -- 1 = include Outside Service lines
  @ScopeUninv   BIT        -- 1 = include Uninventoried lines
  @MaxResults   INT        -- safety cap on returned rows
*/

WITH HistoryByDate AS (
    SELECT
        CAST(pol.LAST_RECEIVED_DATE AS DATE) AS ActivityDate,
        CASE
            WHEN pol.SERVICE_ID IS NOT NULL THEN 'Outside Service'
            WHEN pol.PART_ID IS NULL OR p.DETAIL_ONLY = 'Y' THEN 'Uninventoried'
            WHEN pol.PART_ID LIKE 'MMC%' THEN 'MMC Coils'
            WHEN pol.PART_ID LIKE 'MMF%' THEN 'MMF Flat'
            ELSE 'Parts'
        END AS Category,
        COUNT(*) AS LineCount
    FROM dbo.PURC_ORDER_LINE pol
    INNER JOIN dbo.PURCHASE_ORDER po ON po.ID = pol.PURC_ORDER_ID
    LEFT JOIN dbo.PART p ON pol.PART_ID = p.ID
    WHERE pol.TOTAL_RECEIVED_QTY > 0
      AND pol.LAST_RECEIVED_DATE IS NOT NULL
      AND ISNULL(po.CONSIGNMENT, 'N') <> 'Y'
      AND ISNULL(po.INTERNAL_ORDER, 'N') <> 'Y'
    GROUP BY
        CAST(pol.LAST_RECEIVED_DATE AS DATE),
        CASE
            WHEN pol.SERVICE_ID IS NOT NULL THEN 'Outside Service'
            WHEN pol.PART_ID IS NULL OR p.DETAIL_ONLY = 'Y' THEN 'Uninventoried'
            WHEN pol.PART_ID LIKE 'MMC%' THEN 'MMC Coils'
            WHEN pol.PART_ID LIKE 'MMF%' THEN 'MMF Flat'
            ELSE 'Parts'
        END
)
SELECT TOP (@MaxResults)
    ActivityDate,
    Category,
    LineCount
FROM HistoryByDate
WHERE
    (@FromDate IS NULL OR ActivityDate >= CAST(@FromDate AS DATE))
    AND (@ToDate IS NULL OR ActivityDate <= CAST(@ToDate AS DATE))
    AND (@ScopeParts = 1 OR Category <> 'Parts')
    AND (@ScopeCoils = 1 OR Category <> 'MMC Coils')
    AND (@ScopeFlat = 1 OR Category <> 'MMF Flat')
    AND (@ScopeOutside = 1 OR Category <> 'Outside Service')
    AND (@ScopeUninv = 1 OR Category <> 'Uninventoried')
ORDER BY ActivityDate, Category;

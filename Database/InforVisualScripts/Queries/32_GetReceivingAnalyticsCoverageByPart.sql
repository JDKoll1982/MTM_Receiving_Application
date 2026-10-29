/*
Receiving Analytics — COVERAGE dataset by part number.

Counts receiving lines per received date, category and part, so the App Data
coverage panel can compare Infor Visual against the receiving app on the same
material. The existing 30_GetReceivingAnalyticsHistory.sql aggregates at
date-and-category grain, which is enough for the ERP Data tab but cannot be
narrowed to a set of part numbers; this query keeps the part so the caller can.

Lines with no part number cannot match an app part number and are therefore
left out: they are Outside Service or Uninventoried material, which the
receiving app does not carry by part.

Read-only Infor Visual (MTMFG). Same receipt definition as the history query:
lines where TOTAL_RECEIVED_QTY > 0 anchored on PURC_ORDER_LINE.LAST_RECEIVED_DATE,
consignment and internal orders excluded.

Parameters expected from C#:

  @FromDate     DATETIME   -- inclusive lower bound on received date; NULL = no bound
  @ToDate       DATETIME   -- inclusive upper bound on received date; NULL = no bound
  @MaxResults   INT        -- safety cap on returned rows
*/

WITH LinesByPart AS (
    SELECT
        CAST(pol.LAST_RECEIVED_DATE AS DATE) AS ActivityDate,
        CASE
            WHEN pol.SERVICE_ID IS NOT NULL THEN 'Outside Service'
            WHEN pol.PART_ID IS NULL OR p.DETAIL_ONLY = 'Y' THEN 'Uninventoried'
            WHEN pol.PART_ID LIKE 'MMC%' THEN 'MMC Coils'
            WHEN pol.PART_ID LIKE 'MMF%' THEN 'MMF Flat'
            ELSE 'Parts'
        END AS Category,
        pol.PART_ID AS PartId,
        COUNT(*) AS LineCount
    FROM dbo.PURC_ORDER_LINE pol
    INNER JOIN dbo.PURCHASE_ORDER po ON po.ID = pol.PURC_ORDER_ID
    LEFT JOIN dbo.PART p ON pol.PART_ID = p.ID
    WHERE pol.TOTAL_RECEIVED_QTY > 0
      AND pol.LAST_RECEIVED_DATE IS NOT NULL
      AND pol.PART_ID IS NOT NULL
      AND LTRIM(RTRIM(pol.PART_ID)) <> ''
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
        END,
        pol.PART_ID
)
SELECT TOP (@MaxResults)
    ActivityDate,
    Category,
    PartId,
    LineCount
FROM LinesByPart
WHERE
    (@FromDate IS NULL OR ActivityDate >= CAST(@FromDate AS DATE))
    AND (@ToDate IS NULL OR ActivityDate <= CAST(@ToDate AS DATE))
ORDER BY ActivityDate, Category, PartId;

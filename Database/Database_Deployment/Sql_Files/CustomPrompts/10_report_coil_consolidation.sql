-- Report: Coil consolidation candidates (parts spread across multiple locations)
-- Description: Lists coil parts that have been received into more than one distinct
--              location. High location counts mean the part is fragmented across the
--              warehouse floor, which increases pick time and risk of losing track of
--              stock. These are the best targets for a consolidation effort.
-- Usage: Paste directly into phpMyAdmin or pipe into MAMP mysql client.
--        Edit the LIKE literals in the WHERE clause to change the coil filter.
--        Change HAVING location_count > 1 to > 2 to tighten the threshold.

SELECT
  part_id                                                           AS part_number,
  COALESCE(part_description, part_id)                               AS description,
  COUNT(DISTINCT initial_location)                                  AS location_count,
  GROUP_CONCAT(
    DISTINCT initial_location
    ORDER BY initial_location
    SEPARATOR ', '
  )                                                                 AS locations,
  SUM(COALESCE(packages_per_load, 1))                               AS total_skids,
  SUM(quantity)                                                     AS total_qty,
  MAX(transaction_date)                                             AS last_received
FROM receiving_history
WHERE (LOWER(part_id)          LIKE '%mmc%'
    OR LOWER(part_description) LIKE '%mmc%')
  AND initial_location IS NOT NULL
  AND TRIM(initial_location) <> ''
GROUP BY part_id, part_description
HAVING location_count > 1
ORDER BY location_count DESC, total_skids DESC
LIMIT 50;

-- End of file

-- Report: Coil parts distributed across locations
-- Description: Shows what coil parts are stored in each location, how many skids,
--              and total quantity. Run this first to get a full map of where every
--              coil lives in the warehouse.
-- Usage: Paste directly into phpMyAdmin or pipe into MAMP mysql client.
--        Edit the LIKE literals in the WHERE clause to change the coil filter.
--        Edit LIMIT to see more rows.

SELECT
  COALESCE(initial_location, '** NO LOCATION **')          AS location,
  part_id                                                   AS part_number,
  COALESCE(part_description, part_id)                       AS description,
  COUNT(*)                                                  AS receiving_events,
  SUM(COALESCE(packages_per_load, 1))                       AS total_skids,
  SUM(quantity)                                             AS total_qty,
  MAX(transaction_date)                                     AS last_received
FROM receiving_history
WHERE LOWER(part_id)          LIKE '%mmc%'
   OR LOWER(part_description) LIKE '%mmc%'
GROUP BY initial_location, part_id, part_description
ORDER BY location ASC, total_skids DESC
LIMIT 200;

-- End of file

-- Report: Top coil parts by average received quantity per event
-- Description: Returns parts whose average quantity per receiving event is highest.
--              Useful for spotting parts that arrive in large batches on average,
--              regardless of how often they are received.
-- Usage: Edit the LIKE literals in the WHERE clause, then run directly in
--        phpMyAdmin or pipe into the MAMP mysql client.

-- -----------------------------
-- Configurable reference (not used by the direct SELECT — edit literals below)
-- -----------------------------
-- Filter  : '%mmc%'   → change in both LIKE clauses in the WHERE
-- Limit   : 25        → change in the LIMIT clause
-- Database: mtm_receiving_application.receiving_history

-- -----------------------------
-- Report
-- -----------------------------
SELECT
  part_id                                                   AS part_number,
  COALESCE(part_description, part_id)                       AS description,
  COUNT(*)                                                  AS receiving_events,
  ROUND(AVG(quantity), 2)                                   AS avg_qty_per_event,
  ROUND(AVG(COALESCE(packages_per_load, 1)), 2)             AS avg_skids_per_event,
  SUM(quantity)                                             AS total_received_qty
FROM receiving_history
WHERE LOWER(part_id)          LIKE '%mmc%'
   OR LOWER(part_description) LIKE '%mmc%'
GROUP BY part_id, part_description
ORDER BY avg_qty_per_event DESC
LIMIT 25;

-- End of file

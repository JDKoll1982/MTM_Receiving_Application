-- Report: Location utilization by coil skid count
-- Description: Ranks every location by how many coil skids have been received there.
--              Use this to identify your highest-traffic coil areas and plan space
--              allocation or label signage.
-- Usage: Paste directly into phpMyAdmin or pipe into MAMP mysql client.
--        Edit the LIKE literals in the WHERE clause to change the coil filter.

SELECT
  COALESCE(initial_location, '** NO LOCATION **')          AS location,
  COUNT(DISTINCT part_id)                                   AS distinct_coil_parts,
  COUNT(*)                                                  AS receiving_events,
  SUM(COALESCE(packages_per_load, 1))                       AS total_skids,
  SUM(quantity)                                             AS total_qty,
  ROUND(AVG(COALESCE(packages_per_load, 1)), 2)             AS avg_skids_per_event,
  MAX(transaction_date)                                     AS last_activity
FROM receiving_history
WHERE LOWER(part_id)          LIKE '%mmc%'
   OR LOWER(part_description) LIKE '%mmc%'
GROUP BY initial_location
ORDER BY total_skids DESC
LIMIT 50;

-- End of file

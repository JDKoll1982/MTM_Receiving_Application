-- Report: Top coil parts by skid count
-- Description: Configurable query to list coil parts ordered by skid count (explicit or estimated).
-- Usage: Edit variables below as needed, then run in the MySQL client connected to the target server.

-- -----------------------------
-- Configurable variables
-- -----------------------------
SET @db_name = 'mtm_receiving_application_test';
SET @receiver_table = 'receiving_history';
SET @part_id_col = 'part_id';
SET @part_number_col = 'part_id';
SET @description_col = 'part_description';
SET @received_qty_col = 'quantity';
SET @skid_count_col = 'coils_on_skid';
SET @qty_per_skid_col = 'packages_per_load';
SET @part_filter = '%MMC%';
SET @row_limit = 25;

-- -----------------------------
-- Query: prefer explicit skid_count when present, otherwise estimate using quantity_per_skid
-- -----------------------------
-- Direct SELECT — phpMyAdmin-compatible literal filter.
-- BUG FIX 1: COALESCE(part_description, part_id) hides part_id whenever
--            part_description is non-NULL, causing zero matches for MMC parts
--            whose descriptions don't contain 'mmc'. Use OR to check both.
-- BUG FIX 2: CEIL(quantity/packages_per_load) = pieces-per-skid (density),
--            not the skid count. packages_per_load is the number of skids in
--            the receiving event; SUM(packages_per_load) is total skids received.
-- Edit the '%mmc%' literals below to change the filter (e.g., '%coil%').
SELECT
  part_id                                         AS part_number,
  COALESCE(part_description, part_id)             AS description,
  SUM(COALESCE(packages_per_load, 1))             AS total_skids_received,
  SUM(quantity)                                   AS total_received_qty
FROM receiving_history
WHERE LOWER(part_id)          LIKE '%mmc%'
   OR LOWER(part_description) LIKE '%mmc%'
GROUP BY part_id, part_description
ORDER BY total_skids_received DESC
LIMIT 25;

-- End of file

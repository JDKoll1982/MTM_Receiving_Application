-- Report: Top coil parts by received quantity
-- Description: Configurable query to list coil parts ordered by total received quantity.
-- Usage: Edit variables below as needed, then run in the MySQL client connected to the target server.

-- -----------------------------
-- Configurable variables
-- -----------------------------
SET @db_name = 'mtm_receiving_application_test';
-- Using `receiving_history` schema (see Schemas/10_Table_receiving_history.sql)
SET @receiver_table = 'receiving_history';
SET @part_id_col = 'part_id';                 -- varchar(50) in receiving_history
SET @part_number_col = 'part_id';
SET @description_col = 'part_description';
SET @received_qty_col = 'quantity';
SET @skid_count_col = 'coils_on_skid';       -- explicit coils per skid (may be NULL)
SET @qty_per_skid_col = 'packages_per_load';  -- fallback to estimate skids if coils_on_skid is NULL
-- Default literal filter for phpMyAdmin compatibility. Edit literal directly if needed.
SET @part_filter = '%MMC%';                  -- case-insensitive filter applied to description/part_id
SET @row_limit = 25;

-- -----------------------------
-- Direct SELECT — phpMyAdmin-compatible literal filter.
-- BUG FIX 1: COALESCE hides part_id when part_description is non-NULL.
--            Use OR so rows where part_id matches are included even when
--            part_description does not contain the search token.
-- BUG FIX 2: CEIL(quantity/packages_per_load) gives pieces-per-skid, not skid count.
--            packages_per_load is the number of packages/skids in the receiving event;
--            SUM(packages_per_load) is the correct total-skid aggregate.
-- Edit the '%mmc%' literals below to change the filter (e.g., '%coil%').
SELECT
  part_id                                         AS part_number,
  COALESCE(part_description, part_id)             AS description,
  SUM(quantity)                                   AS total_received_qty,
  SUM(COALESCE(packages_per_load, 1))             AS total_skids_received
FROM receiving_history
WHERE LOWER(part_id)          LIKE '%mmc%'
   OR LOWER(part_description) LIKE '%mmc%'
GROUP BY part_id, part_description
ORDER BY total_received_qty DESC
LIMIT 25;


-- Report: Top coil parts by skid count
-- Description: Configurable query to list coil parts ordered by skid count (explicit or estimated).
-- Usage: Edit variables below as needed, then run in the MySQL client connected to the target server.

-- -----------------------------
-- Configurable variables
-- -----------------------------
SET @db_name = 'mtm_receiving_application';
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
-- Direct SELECT using literal filter for phpMyAdmin compatibility. Edit literal below as needed.
SELECT
  part_id AS part_number,
  COALESCE(part_description, part_id) AS description,
  SUM(COALESCE(coils_on_skid, CEIL(quantity / NULLIF(packages_per_load,0)))) AS total_skids,
  SUM(quantity) AS total_qty
FROM receiving_history
WHERE LOWER(COALESCE(part_description, part_id)) LIKE '%mmc%'
GROUP BY part_id, part_description
ORDER BY total_skids DESC
LIMIT 25;

-- End of file

-- Stored Procedure: sp_LotMarkings_GetByHeat
-- Purpose:  Searches recorded markings by heat number. Used by the tool's search box.
-- Parameters:
--   p_search_text VARCHAR(255) - Partial heat number. Null or empty returns the most recent rows.
-- Returns:  The same column list as sp_LotMarkings_GetAll, capped at 100 rows.
-- Errors:   none.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkings_GetByHeat`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkings_GetByHeat`(IN p_search_text VARCHAR(255))
BEGIN
    SELECT
        `id`,
        `heat_number`,
        `heat_number_normalized`,
        `supplier_id`,
        `supplier_name`,
        `shape_code`,
        `color_code`,
        `marking_source`,
        `is_locked`,
        `first_seen_part_number`,
        `change_reason`,
        `resolved_by`,
        `resolved_at`,
        `notes`,
        `created_at`
    FROM `lot_markings`
    WHERE p_search_text IS NULL
       OR p_search_text = ''
       OR `heat_number` LIKE CONCAT('%', p_search_text, '%')
       OR `heat_number_normalized` LIKE CONCAT('%', UPPER(p_search_text), '%')
    ORDER BY `resolved_at` DESC, `heat_number` ASC
    LIMIT 100;
END $$
DELIMITER ;

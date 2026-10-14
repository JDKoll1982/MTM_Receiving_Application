-- Stored Procedure: sp_LotMarkingPartScope_Search
-- Purpose:  Searches the enabled-part list so the tool can resolve a typed term to a part
--           number before adding it. Caps the result so a broad term stays usable.
-- Parameters:
--   p_search_text VARCHAR(255) - Partial part number. Null or empty returns the first rows.
-- Returns:  id, part_id, is_enabled, added_by, added_at, capped at 100 rows.
-- Errors:   none.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkingPartScope_Search`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkingPartScope_Search`(IN p_search_text VARCHAR(255))
BEGIN
    SELECT
        `id`,
        `part_id`,
        `is_enabled`,
        `added_by`,
        `added_at`
    FROM `lot_marking_part_scope`
    WHERE p_search_text IS NULL
       OR p_search_text = ''
       OR `part_id` LIKE CONCAT('%', p_search_text, '%')
    ORDER BY `is_enabled` DESC, `part_id` ASC
    LIMIT 100;
END $$
DELIMITER ;

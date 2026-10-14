-- Stored Procedure: sp_LotMarkingPartScope_Upsert
-- Purpose:  Adds a part number to the enabled list, or puts back a part that was removed.
--           Safe to run twice: a repeated call re-enables the same row instead of failing.
-- Parameters:
--   p_part_id  VARCHAR(50) - Part number to enable.
--   p_added_by VARCHAR(50) - Acting user.
-- Returns:  part_scope_id - the id of the enabled row.
-- Errors:   SQLSTATE 45000 when the part number is empty.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkingPartScope_Upsert`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkingPartScope_Upsert`(
    IN p_part_id VARCHAR(50),
    IN p_added_by VARCHAR(50)
)
BEGIN
    IF p_part_id IS NULL OR TRIM(p_part_id) = '' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A part number is required.';
    END IF;

    INSERT INTO `lot_marking_part_scope` (`part_id`, `is_enabled`, `added_by`, `added_at`)
    VALUES (TRIM(p_part_id), 1, p_added_by, CURRENT_TIMESTAMP)
    ON DUPLICATE KEY UPDATE
        `is_enabled` = 1,
        `added_by` = p_added_by,
        `added_at` = CURRENT_TIMESTAMP;

    SELECT `id` AS part_scope_id
    FROM `lot_marking_part_scope`
    WHERE `part_id` = TRIM(p_part_id)
    LIMIT 1;
END $$
DELIMITER ;

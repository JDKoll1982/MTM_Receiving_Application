-- Stored Procedure: sp_LotMarkingPartScope_SetEnabled
-- Purpose:  Removes a part from the enabled list, or puts it back, without deleting the row.
--           Markings already recorded for the part keep their shape and color either way.
-- Parameters:
--   p_id         INT        - Id of the list row.
--   p_is_enabled TINYINT(1) - 0 to stop assigning markings, 1 to assign them again.
-- Returns:  rows_updated.
-- Errors:   SQLSTATE 45000 when the row does not exist.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkingPartScope_SetEnabled`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkingPartScope_SetEnabled`(
    IN p_id INT,
    IN p_is_enabled TINYINT(1)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM `lot_marking_part_scope`
    WHERE `id` = p_id;

    IF v_exists = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'That part is no longer on the list.';
    END IF;

    UPDATE `lot_marking_part_scope`
    SET `is_enabled` = IF(p_is_enabled = 1, 1, 0)
    WHERE `id` = p_id;

    SELECT ROW_COUNT() AS rows_updated;
END $$
DELIMITER ;

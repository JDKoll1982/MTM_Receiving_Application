-- Stored Procedure: sp_LotMarkings_SetLocked
-- Purpose:  Locks or unlocks a marking. A locked marking is returned exactly as it is and
--           is never recomputed or advanced to a free shape and color.
-- Parameters:
--   p_id          INT         - Id of the ledger row.
--   p_is_locked   TINYINT(1)  - 1 to protect the marking, 0 to allow normal behaviour.
--   p_resolved_by VARCHAR(50) - Acting user.
-- Returns:  rows_updated.
-- Errors:   SQLSTATE 45000 when the row does not exist.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkings_SetLocked`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkings_SetLocked`(
    IN p_id INT,
    IN p_is_locked TINYINT(1),
    IN p_resolved_by VARCHAR(50)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM `lot_markings`
    WHERE `id` = p_id;

    IF v_exists = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'That lot marking no longer exists.';
    END IF;

    UPDATE `lot_markings`
    SET
        `is_locked` = IF(p_is_locked = 1, 1, 0),
        `resolved_by` = p_resolved_by,
        `resolved_at` = CURRENT_TIMESTAMP
    WHERE `id` = p_id;

    SELECT ROW_COUNT() AS rows_updated;
END $$
DELIMITER ;

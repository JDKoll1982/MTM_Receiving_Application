-- Stored Procedure: sp_LotMarkings_UpdateMarking
-- Purpose:  Replaces a lot's shape and color by hand. A manual change must carry a reason,
--           and the acting user is recorded. Locking is not required to change a marking,
--           because a lock protects a marking from the system, not from a user.
-- Parameters:
--   p_id             INT          - Id of the ledger row.
--   p_shape_code     VARCHAR(32)  - New shape code.
--   p_color_code     VARCHAR(32)  - New color code.
--   p_marking_source VARCHAR(16)  - Auto, Manual or Import.
--   p_change_reason  VARCHAR(255) - Why the change was made. Required for a manual change.
--   p_resolved_by    VARCHAR(50)  - Acting user.
-- Returns:  rows_updated.
-- Errors:   SQLSTATE 45000 when the row does not exist, or when a manual change carries no reason.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkings_UpdateMarking`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkings_UpdateMarking`(
    IN p_id INT,
    IN p_shape_code VARCHAR(32),
    IN p_color_code VARCHAR(32),
    IN p_marking_source VARCHAR(16),
    IN p_change_reason VARCHAR(255),
    IN p_resolved_by VARCHAR(50)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_source VARCHAR(16);

    SELECT COUNT(*)
    INTO v_exists
    FROM `lot_markings`
    WHERE `id` = p_id;

    IF v_exists = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'That lot marking no longer exists.';
    END IF;

    SET v_source = COALESCE(NULLIF(p_marking_source, ''), 'Manual');

    IF v_source = 'Manual' AND (p_change_reason IS NULL OR TRIM(p_change_reason) = '') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A reason is required when a marking is changed by hand.';
    END IF;

    UPDATE `lot_markings`
    SET
        `shape_code` = p_shape_code,
        `color_code` = p_color_code,
        `marking_source` = v_source,
        `change_reason` = p_change_reason,
        `resolved_by` = p_resolved_by,
        `resolved_at` = CURRENT_TIMESTAMP
    WHERE `id` = p_id;

    SELECT ROW_COUNT() AS rows_updated;
END $$
DELIMITER ;

-- Stored Procedure: sp_LotMarkings_Delete
-- Purpose:  Removes one lot from the list, so that its shape and color become free for a later lot.
--           Used by Update All Lots for a lot whose lot number no longer appears on any receiving
--           row, which means it describes nothing.
-- Parameters:
--   p_id INT - The lot to remove.
-- Returns:  rows_deleted.
-- Errors:   SQLSTATE 45000 when the lot does not exist, or when its marking is protected.
-- Notes:    The lot is deleted rather than emptied. The one-row-per-lot-number-and-supplier rule
--           would otherwise stop that number from ever being recorded again, and a number that is
--           received again is meant to be treated as one that has never been seen.
--           A protected marking is refused here as well as in the service, so no path can remove one
--           by mistake. Protection means a person deliberately fixed that marking.
--           Deleting a lot does not touch any receiving row. The rows that used to belong to it have
--           already been given the marking of the number they now carry, which is what the caller
--           does first.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkings_Delete`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkings_Delete`(
    IN p_id INT
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_is_locked TINYINT(1) DEFAULT 0;

    SELECT COUNT(*), COALESCE(MAX(`is_locked`), 0)
    INTO v_exists, v_is_locked
    FROM `lot_markings`
    WHERE `id` = p_id;

    IF v_exists = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'That lot is no longer in the list.';
    END IF;

    IF v_is_locked = 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'That lot is protected, so it is never removed. Release it first if it really is gone.';
    END IF;

    DELETE FROM `lot_markings`
    WHERE `id` = p_id;

    SELECT ROW_COUNT() AS `rows_deleted`;
END $$
DELIMITER ;

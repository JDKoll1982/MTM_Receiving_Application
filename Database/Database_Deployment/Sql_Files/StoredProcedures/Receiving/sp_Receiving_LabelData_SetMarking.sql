-- Stored Procedure: sp_Receiving_LabelData_SetMarking
-- Purpose:  Writes a lot's marking codes onto an existing receiving queue row.
--           Additive by design: the existing insert and update procedures keep their current
--           signatures, so a build that predates this feature continues to call them unchanged
--           and simply never sets a marking.
--           The marking is resolved once by the application and copied here verbatim. Nothing
--           recomputes it, so a row keeps the marking the handler actually saw.
-- Parameters:
--   p_load_id          CHAR(36)    - Queue row to mark, matched on load_id.
--   p_heat_shape_code  VARCHAR(32) - Shape emoji, or null to clear the marking.
--   p_heat_color_code  VARCHAR(32) - Color code, or null to clear the marking.
-- Returns:  rows_updated.
-- Errors:   SQLSTATE 45000 when no queue row matches the supplied load id.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_SetMarking`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_LabelData_SetMarking`(
    IN p_load_id CHAR(36),
    IN p_heat_shape_code VARCHAR(32),
    IN p_heat_color_code VARCHAR(32)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM `receiving_label_data`
    WHERE `load_id` = p_load_id;

    IF v_exists = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'No queued receiving row matches that load id.';
    END IF;

    UPDATE `receiving_label_data`
    SET
        `heat_shape_code` = p_heat_shape_code,
        `heat_color_code` = p_heat_color_code
    WHERE `load_id` = p_load_id;

    SELECT ROW_COUNT() AS rows_updated;
END $$
DELIMITER ;

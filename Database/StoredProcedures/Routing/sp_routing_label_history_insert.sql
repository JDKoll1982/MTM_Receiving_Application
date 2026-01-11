-- ============================================================================
-- Stored Procedure: sp_routing_label_history_insert
-- Description: Insert a new routing label history entry for audit trail
-- Parameters:
--   @p_label_id: Foreign key to routing_label_data
--   @p_field_changed: Name of the field that was modified
--   @p_old_value: Previous value before change
--   @p_new_value: New value after change
--   @p_edited_by: User ID who made the change
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_label_history_insert`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_label_history_insert`(
    IN p_label_id INT,
    IN p_field_changed VARCHAR(100),
    IN p_old_value TEXT,
    IN p_new_value TEXT,
    IN p_edited_by INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    START TRANSACTION;

    IF p_label_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Label ID is required';
        ROLLBACK;
    ELSEIF p_field_changed IS NULL OR p_field_changed = '' THEN
        SET p_status = -1;
        SET p_error_msg = 'Field changed is required';
        ROLLBACK;
    ELSE
        INSERT INTO routing_history (
            label_id,
            field_changed,
            old_value,
            new_value,
            edited_by
        ) VALUES (
            p_label_id,
            p_field_changed,
            p_old_value,
            p_new_value,
            p_edited_by
        );

        SET p_status = 1;
        SET p_error_msg = 'History entry created';
        COMMIT;
    END IF;
END $$

DELIMITER ;

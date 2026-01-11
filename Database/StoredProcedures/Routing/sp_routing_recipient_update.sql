-- ============================================================================
-- Stored Procedure: sp_routing_recipient_update
-- Description: Update existing routing recipient
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_recipient_update;

DELIMITER $$

CREATE PROCEDURE sp_routing_recipient_update(
    IN p_id INT,
    IN p_name VARCHAR(100),
    IN p_location VARCHAR(100),
    IN p_department VARCHAR(100),
    IN p_is_active TINYINT(1),
    OUT p_error_message VARCHAR(500)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_name_duplicate INT DEFAULT 0;
    DECLARE v_err_msg VARCHAR(500) DEFAULT NULL;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        -- Capture diagnostic message into a local variable (MySQL 5.7 compatible)
        GET DIAGNOSTICS CONDITION 1
            v_err_msg = MESSAGE_TEXT;
        SET p_error_message = v_err_msg;
        ROLLBACK;
    END;

    -- Initialize output
    SET p_error_message = NULL;

    -- Start transaction
    START TRANSACTION;

    -- Validate ID exists
    SELECT COUNT(*) INTO v_exists
    FROM routing_recipients
    WHERE id = p_id;

    IF v_exists = 0 THEN
        SET p_error_message = 'Recipient ID not found';
        ROLLBACK;
    ELSEIF p_name IS NULL OR TRIM(p_name) = '' THEN
        SET p_error_message = 'Recipient name is required';
        ROLLBACK;
    ELSEIF p_location IS NULL OR TRIM(p_location) = '' THEN
        SET p_error_message = 'Location is required';
        ROLLBACK;
    ELSE
        -- Check if name already exists for a different recipient
        SELECT COUNT(*) INTO v_name_duplicate
        FROM routing_recipients
        WHERE name = p_name
            AND id != p_id;

        IF v_name_duplicate > 0 THEN
            SET p_error_message = 'Recipient name already exists';
            ROLLBACK;
        ELSE
            -- Update recipient
            UPDATE routing_recipients
            SET
                name = p_name,
                location = p_location,
                department = p_department,
                is_active = p_is_active
            WHERE id = p_id;

            -- Commit transaction
            COMMIT;
        END IF;
    END IF;
END$$

DELIMITER ;

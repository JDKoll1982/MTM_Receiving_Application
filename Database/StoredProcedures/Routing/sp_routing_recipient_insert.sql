-- ============================================================================
-- Stored Procedure: sp_routing_recipient_insert
-- Description: Insert new routing recipient
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_recipient_insert;

DELIMITER $$

CREATE PROCEDURE sp_routing_recipient_insert(
    IN p_name VARCHAR(100),
    IN p_default_department VARCHAR(100),
    OUT p_new_recipient_id INT,
    OUT p_error_message VARCHAR(500)
)
BEGIN
    DECLARE v_name_exists INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1
            p_error_message = MESSAGE_TEXT;
        SET p_new_recipient_id = NULL;
        ROLLBACK;
    END;
    
    -- Initialize outputs
    SET p_new_recipient_id = NULL;
    SET p_error_message = NULL;
    
    -- Start transaction
    START TRANSACTION;
    
    -- Validate name is provided
    IF p_name IS NULL OR TRIM(p_name) = '' THEN
        SET p_error_message = 'Recipient name is required';
        ROLLBACK;
        LEAVE;
    END IF;
    
    -- Check if name already exists
    SELECT COUNT(*) INTO v_name_exists
    FROM routing_recipients
    WHERE name = p_name;
    
    IF v_name_exists > 0 THEN
        SET p_error_message = 'Recipient name already exists';
        ROLLBACK;
        LEAVE;
    END IF;
    
    -- Insert recipient
    INSERT INTO routing_recipients (
        name,
        default_department,
        is_active
    ) VALUES (
        p_name,
        p_default_department,
        1  -- Default: active
    );
    
    -- Get newly created ID
    SET p_new_recipient_id = LAST_INSERT_ID();
    
    -- Commit transaction
    COMMIT;
END$$

DELIMITER ;

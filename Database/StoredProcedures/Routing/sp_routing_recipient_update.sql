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
    IN p_default_department VARCHAR(100),
    IN p_is_active TINYINT(1),
    OUT p_error_message VARCHAR(500)
)
proc: BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_name_duplicate INT DEFAULT 0;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1
            p_error_message = MESSAGE_TEXT;
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
        LEAVE proc;
    END IF;
    
    -- Validate name is provided
    IF p_name IS NULL OR TRIM(p_name) = '' THEN
        SET p_error_message = 'Recipient name is required';
        ROLLBACK;
        LEAVE proc;
    END IF;
    
    -- Check if name already exists for a different recipient
    SELECT COUNT(*) INTO v_name_duplicate
    FROM routing_recipients
    WHERE name = p_name
        AND id != p_id;
    
    IF v_name_duplicate > 0 THEN
        SET p_error_message = 'Recipient name already exists';
        ROLLBACK;
        LEAVE proc;
    END IF;
    
    -- Update recipient
    UPDATE routing_recipients
    SET 
        name = p_name,
        default_department = p_default_department,
        is_active = p_is_active
    WHERE id = p_id;
    
    -- Commit transaction
    COMMIT;
END proc$$

DELIMITER ;

-- ============================================================================
-- Stored Procedure: sp_routing_label_delete
-- Description: Delete routing label by ID
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_label_delete;

DELIMITER $$

CREATE PROCEDURE sp_routing_label_delete(
    IN p_id INT,
    OUT p_error_message VARCHAR(500)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;
    
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
    FROM routing_labels
    WHERE id = p_id;
    
    IF v_exists = 0 THEN
        SET p_error_message = 'Label ID not found';
        ROLLBACK;
        LEAVE;
    END IF;
    
    -- Delete label
    DELETE FROM routing_labels
    WHERE id = p_id;
    
    -- Commit transaction
    COMMIT;
END$$

DELIMITER ;

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
    -- Initialize output
    SET p_error_message = NULL;

    -- Attempt delete
    DELETE FROM routing_label_data
    WHERE id = p_id;

    -- If no rows were affected, report not found
    IF ROW_COUNT() = 0 THEN
        SET p_error_message = 'Label ID not found';
    END IF;
END$$

DELIMITER ;

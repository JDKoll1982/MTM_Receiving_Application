-- ============================================================================
-- Stored Procedure: sp_routing_recipient_get_by_name
-- Description: Get routing recipient by name for department lookup
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_recipient_get_by_name;

DELIMITER $$

CREATE PROCEDURE sp_routing_recipient_get_by_name(
    IN p_name VARCHAR(100)
)
BEGIN
    -- Return recipient with matching name (exact match)
    SELECT 
        id,
        name,
        default_department,
        is_active,
        created_date
    FROM routing_recipients
    WHERE name = p_name
    LIMIT 1;
END$$

DELIMITER ;

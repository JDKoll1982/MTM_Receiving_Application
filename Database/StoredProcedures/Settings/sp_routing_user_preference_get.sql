-- ============================================================================
-- Stored Procedure: sp_routing_user_preference_get
-- Description: Retrieve user preferences for routing module
-- Parameters:
--   @p_employee_number: Employee to retrieve preferences for
-- Returns: User preference record from settings_routing_personal
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_user_preference_get`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_user_preference_get`(
    IN p_employee_number INT
)
BEGIN
    SELECT
        id,
        employee_number,
        default_mode,
        enable_validation,
        updated_date
    FROM settings_routing_personal
    WHERE employee_number = p_employee_number;
END $$

DELIMITER ;

-- ============================================================================
-- Stored Procedure: sp_routing_recipient_get_by_name
-- Description: Get routing recipient by name for department lookup
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================
USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `mtm_receiving_application`.`sp_routing_recipient_get_by_name`;

DELIMITER $$

SET NAMES utf8mb4 COLLATE utf8mb4_general_ci$$

CREATE PROCEDURE `mtm_receiving_application`.`sp_routing_recipient_get_by_name`(
    IN p_name VARCHAR(100)
)
BEGIN
    -- Return recipient with matching name (exact match)
    SELECT
        `id`,
        `name`,
        `location`,
        `department`,
        `is_active`,
        `created_date`,
        `updated_date`
    FROM `mtm_receiving_application`.`routing_recipients`
    WHERE `name` = (CONVERT(p_name USING utf8mb4) COLLATE utf8mb4_general_ci)
    LIMIT 1;
END$$

DELIMITER ;

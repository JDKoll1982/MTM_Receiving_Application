-- ============================================================================
-- Stored Procedure: sp_routing_recipient_get_all
-- Description: Get all active routing recipients for dropdown population
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

-- Avoid changing connection default; qualify object names to be explicit and use identifier quoting for MySQL 5.7 compatibility
DROP PROCEDURE IF EXISTS `sp_routing_recipient_get_all`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_recipient_get_all`()
BEGIN
    -- Return all active recipients sorted by name
    SELECT
        `id`,
        `name`,
        `location`,
        `department`,
        `is_active`,
        `created_date`,
        `updated_date`
    FROM `routing_recipients`
    WHERE `is_active` = 1
    ORDER BY `name` ASC;
END$$

DELIMITER ;

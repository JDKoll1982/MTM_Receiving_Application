-- ============================================================================
-- Stored Procedure: sp_routing_recipient_get_all_active
-- Description: Get all active recipients sorted by name
-- Returns: Result set of active recipients with usage_count=0 placeholder
-- Feature: Routing Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_routing_recipient_get_all_active`;

DELIMITER $$

CREATE PROCEDURE `sp_routing_recipient_get_all_active`()
BEGIN
    SELECT
        `id`,
        `name`,
        `location`,
        `department`,
        `is_active`,
        `created_date`,
        `updated_date`,
        0 AS `usage_count`
    FROM `routing_recipients`
    WHERE `is_active` = 1
    ORDER BY `name`;
END $$

DELIMITER ;

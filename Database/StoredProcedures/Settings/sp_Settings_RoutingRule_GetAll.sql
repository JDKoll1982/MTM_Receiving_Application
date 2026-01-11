-- =============================================
-- Stored Procedure: sp_Settings_RoutingRule_GetAll
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All Routing Rules
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_RoutingRule_GetAll`$$
CREATE PROCEDURE `sp_Settings_RoutingRule_GetAll`()
BEGIN
    SELECT
        `id`,
        `match_type`,
        `pattern`,
        `destination_location`,
        `priority`,
        `is_active`,
        `created_at`,
        `updated_at`,
        `created_by`
    FROM `routing_home_locations`
    WHERE `is_active` = 1
    ORDER BY `priority` ASC, `created_at` DESC;
END$$

DELIMITER ;

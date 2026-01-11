-- =============================================
-- Stored Procedure: sp_Settings_RoutingRule_GetById
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Routing Rule by ID
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_RoutingRule_GetById`$$
CREATE PROCEDURE `sp_Settings_RoutingRule_GetById`(
    IN p_id INT
)
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
    WHERE `id` = p_id;
END$$

DELIMITER ;

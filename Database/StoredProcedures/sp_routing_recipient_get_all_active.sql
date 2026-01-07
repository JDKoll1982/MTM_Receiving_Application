DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_recipient_get_all_active` $$

-- =============================================
-- Procedure: sp_routing_recipient_get_all_active
-- Purpose: Get all active recipients sorted by name
-- Returns: Result set of active recipients
-- =============================================
CREATE PROCEDURE `sp_routing_recipient_get_all_active`()
BEGIN
    SELECT
        id,
        name,
        location,
        department,
        is_active,
        created_date,
        updated_date,
        0 AS usage_count  -- Placeholder for consistency with top_by_usage
    FROM routing_recipients
    WHERE is_active = 1
    ORDER BY name;
END $$

DELIMITER ;

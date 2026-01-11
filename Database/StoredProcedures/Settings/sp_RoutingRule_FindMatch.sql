-- =============================================
-- Stored Procedure: sp_RoutingRule_FindMatch
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Find Matching Routing Rule
-- =============================================
DROP PROCEDURE IF EXISTS `sp_RoutingRule_FindMatch`$$
CREATE PROCEDURE `sp_RoutingRule_FindMatch`(
    IN p_match_type VARCHAR(50),
    IN p_value VARCHAR(100)
)
BEGIN
    SELECT
        r.id,
        r.match_type,
        r.`pattern`,
        r.destination_location,
        r.priority
    FROM routing_home_locations AS r
    WHERE r.match_type = p_match_type
      AND r.is_active = 1
      AND (
          p_value LIKE REPLACE(REPLACE(r.`pattern`, '*', '%'), '?', '_')
          OR r.`pattern` = p_value
      )
    ORDER BY r.priority ASC
    LIMIT 1;
END$$

-- =============================================
-- SCHEDULED REPORTS CRUD OPERATIONS
-- =============================================



DELIMITER ;

-- =============================================
-- Stored Procedure: sp_RoutingRule_FindMatch
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Find Matching Routing Rule
-- =============================================
DROP PROCEDURE IF EXISTS sp_RoutingRule_FindMatch$$
CREATE PROCEDURE sp_RoutingRule_FindMatch(
    IN p_match_type VARCHAR(50),
    IN p_value VARCHAR(100)
)
BEGIN
    SELECT 
        id,
        match_type,
        pattern,
        destination_location,
        priority
    FROM routing_rules
    WHERE match_type = p_match_type
      AND is_active = TRUE
      AND (
          p_value LIKE REPLACE(REPLACE(pattern, '*', '%'), '?', '_')
          OR pattern = p_value
      )
    ORDER BY priority ASC
    LIMIT 1;
END$$

-- =============================================
-- SCHEDULED REPORTS CRUD OPERATIONS
-- =============================================



DELIMITER ;

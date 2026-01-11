-- =============================================
-- Stored Procedure: sp_Settings_RoutingRule_GetByPartNumber
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Find Matching Routing Rule (Part Number)
-- Wrapper for spec compatibility; uses routing_home_locations pattern matching.
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_RoutingRule_GetByPartNumber` $$
CREATE PROCEDURE `sp_Settings_RoutingRule_GetByPartNumber`(
    IN p_part_number VARCHAR(100)
)
BEGIN
    SELECT
        id,
        match_type,
        pattern,
        destination_location,
        priority
    FROM routing_home_locations
    WHERE match_type = 'Part Number'
      AND is_active = 1
      AND (
          p_part_number LIKE REPLACE(REPLACE(pattern, '*', '%'), '?', '_')
          OR pattern = p_part_number
      )
    ORDER BY priority ASC
    LIMIT 1;
END $$
DELIMITER ;

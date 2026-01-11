-- =============================================
-- Stored Procedure: sp_Settings_RoutingRule_Insert
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Insert Routing Rule
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_RoutingRule_Insert`$$
CREATE PROCEDURE `sp_Settings_RoutingRule_Insert`(
    IN p_match_type VARCHAR(50),
    IN p_pattern VARCHAR(100),
    IN p_destination_location VARCHAR(50),
    IN p_priority INT,
    IN p_created_by INT
)
BEGIN
    -- Check for duplicate active pattern (use numeric literal for boolean for MySQL 5.7)
    IF EXISTS (
        SELECT 1
        FROM `routing_home_locations`
        WHERE `match_type` = p_match_type
          AND `pattern` = p_pattern
          AND `is_active` = 1
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Routing rule with this match type and pattern already exists';
    END IF;

    -- Ensure priority has a sensible default if NULL is passed
    INSERT INTO `routing_home_locations`
        (`match_type`, `pattern`, `destination_location`, `priority`, `is_active`, `created_by`)
    VALUES
        (p_match_type, p_pattern, p_destination_location, COALESCE(p_priority, 50), 1, p_created_by);

    SELECT LAST_INSERT_ID() AS `id`;
END$$

DELIMITER ;

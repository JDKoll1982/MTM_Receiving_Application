-- =============================================
-- Stored Procedure: sp_RoutingRule_Update
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update Routing Rule
-- =============================================
DROP PROCEDURE IF EXISTS sp_RoutingRule_Update$$
CREATE PROCEDURE sp_RoutingRule_Update(
    IN p_id INT,
    IN p_match_type VARCHAR(50),
    IN p_pattern VARCHAR(100),
    IN p_destination_location VARCHAR(50),
    IN p_priority INT
)
BEGIN
    -- Check for duplicate pattern (excluding self)
    -- NOTE: the table has a UNIQUE index on (match_type, pattern) so we must check all rows,
    -- not only active ones, otherwise the UPDATE will fail with duplicate-key error.
    IF EXISTS (SELECT 1 FROM routing_home_locations WHERE match_type = p_match_type AND pattern = p_pattern AND id != p_id) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Routing rule with this match type and pattern already exists';
    END IF;

    UPDATE routing_home_locations
    SET match_type = p_match_type,
        pattern = p_pattern,
        destination_location = p_destination_location,
        priority = p_priority,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;

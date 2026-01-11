-- =============================================
-- Stored Procedure: sp_RoutingRule_Delete
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Delete Routing Rule
-- =============================================
DROP PROCEDURE IF EXISTS sp_RoutingRule_Delete$$
CREATE PROCEDURE sp_RoutingRule_Delete(
    IN p_id INT
)
BEGIN
    -- Soft delete
    UPDATE routing_home_locations
    SET is_active = FALSE,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;

-- Fixed: sp_Settings_RoutingRule_Delete
-- Reads from routing_home_locations (see 16_Table_routing_home_locations.sql)
DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_RoutingRule_Delete`$$
CREATE PROCEDURE `sp_Settings_RoutingRule_Delete`(
    IN p_id INT
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_affected INT DEFAULT 0;

    -- Verify the record exists
    SELECT COUNT(1) INTO v_exists
    FROM routing_home_locations
    WHERE id = p_id;

    IF v_exists = 0 THEN
        -- Not found
        SELECT 0 AS affected_rows, 'NotFound' AS status;
    ELSE
        -- Soft delete only if currently active
        UPDATE routing_home_locations
        SET is_active = 0,
            updated_at = CURRENT_TIMESTAMP
        WHERE id = p_id AND is_active = 1;

        SET v_affected = ROW_COUNT();

        SELECT v_affected AS affected_rows,
               CASE
                   WHEN v_affected = 1 THEN 'Deleted'
                   ELSE 'AlreadyInactive'
               END AS status;
    END IF;
END$$

DELIMITER ;

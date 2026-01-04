-- ============================================================================
-- Stored Procedure: sp_routing_label_get_today
-- Description: Get all non-archived routing labels for today's date
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_label_get_today;

DELIMITER $$

CREATE PROCEDURE sp_routing_label_get_today(
    IN p_today_date DATE
)
BEGIN
    -- Return all labels created today that haven't been archived yet
    SELECT 
        id,
        label_number,
        deliver_to,
        department,
        package_description,
        po_number,
        work_order,
        employee_number,
        created_date,
        created_at
    FROM routing_labels
    WHERE created_date = p_today_date
        AND is_archived = 0
    ORDER BY label_number ASC;
END$$

DELIMITER ;

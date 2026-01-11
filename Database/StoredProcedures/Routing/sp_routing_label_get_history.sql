-- ============================================================================
-- Stored Procedure: sp_routing_label_get_history
-- Description: Get archived routing labels filtered by date range
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_label_get_history;

DELIMITER $$

CREATE PROCEDURE sp_routing_label_get_history(
    IN p_start_date DATE,
    IN p_end_date DATE
)
BEGIN
    -- Return archived routing labels within date range
    -- Ordered by date (descending) and label number (ascending)
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
    FROM routing_label_data
    WHERE created_date BETWEEN p_start_date AND p_end_date
        AND is_archived = 1
    ORDER BY created_date DESC, label_number ASC;
END$$

DELIMITER ;

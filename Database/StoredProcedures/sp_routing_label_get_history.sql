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
        l.id,
        l.id AS label_number, -- Mapping ID to label_number as per schema limitation
        r.name AS deliver_to,
        r.department,
        l.description AS package_description,
        l.po_number,
        l.line_number AS work_order,
        CAST(l.created_by AS CHAR) AS employee_number,
        l.created_date,
        l.created_date AS created_at
    FROM routing_labels l
    LEFT JOIN routing_recipients r ON l.recipient_id = r.id
    WHERE l.created_date BETWEEN p_start_date AND p_end_date
        AND l.is_active = 0 -- Assuming is_active=0 means archived/history based on context
    ORDER BY l.created_date DESC, l.id ASC;
END$$

DELIMITER ;

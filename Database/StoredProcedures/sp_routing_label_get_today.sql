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
    WHERE l.created_date = p_today_date
        AND l.is_active = 1
    ORDER BY l.id ASC;
END$$

DELIMITER ;

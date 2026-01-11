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
    -- Return all labels created today that are active
    SELECT
        id,
        po_number,
        line_number,
        description,
        recipient_id,
        quantity,
        created_by,
        DATE(created_date) AS created_date,
        created_date AS created_datetime,
        other_reason_id,
        is_active,
        csv_exported,
        csv_export_date
    FROM routing_label_data
    WHERE DATE(created_date) = p_today_date
        AND is_active = 1
    ORDER BY id ASC;
END$$

DELIMITER ;

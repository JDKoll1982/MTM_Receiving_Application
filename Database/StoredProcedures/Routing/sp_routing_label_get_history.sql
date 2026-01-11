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
    -- Use created_date (DATETIME) range to include full days
    SELECT
        id,
        po_number,
        line_number,
        description,
        recipient_id,
        quantity,
        created_by,
        created_date,
        other_reason_id,
        is_active,
        csv_exported,
        csv_export_date
    FROM routing_label_data
    WHERE created_date >= CAST(p_start_date AS DATETIME)
        AND created_date < DATE_ADD(CAST(p_end_date AS DATETIME), INTERVAL 1 DAY)
        AND is_active = 0  -- Inactive labels are considered archived
    ORDER BY created_date DESC, id ASC;
END$$

DELIMITER ;

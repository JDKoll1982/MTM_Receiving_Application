-- =============================================
-- Stored Procedure: sp_ScheduledReport_ToggleActive
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Toggle Scheduled Report Active Status
-- =============================================
DROP PROCEDURE IF EXISTS sp_ScheduledReport_ToggleActive$$
CREATE PROCEDURE sp_ScheduledReport_ToggleActive(
    IN p_id INT,
    IN p_is_active BOOLEAN
)
BEGIN
    UPDATE reporting_scheduled_reports
    SET is_active = p_is_active,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$

DELIMITER ;


DELIMITER ;

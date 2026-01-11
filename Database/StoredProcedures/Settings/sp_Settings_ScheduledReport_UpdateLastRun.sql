-- =============================================
-- Stored Procedure: sp_Settings_ScheduledReport_UpdateLastRun
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update Last Run Date
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_ScheduledReport_UpdateLastRun`$$
CREATE PROCEDURE `sp_Settings_ScheduledReport_UpdateLastRun`(
    IN p_id INT,
    IN p_last_run_date DATETIME,
    IN p_next_run_date DATETIME,
    OUT p_affected_rows INT
)
BEGIN
    UPDATE reporting_scheduled_reports
    SET last_run_date = p_last_run_date,
        next_run_date = p_next_run_date,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    -- assign the number of affected rows to the OUT parameter
    SET p_affected_rows = ROW_COUNT();
END$$



DELIMITER ;

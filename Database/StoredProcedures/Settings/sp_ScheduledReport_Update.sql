-- =============================================
-- Stored Procedure: sp_ScheduledReport_Update
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update Scheduled Report
-- =============================================
DROP PROCEDURE IF EXISTS sp_ScheduledReport_Update$$
CREATE PROCEDURE sp_ScheduledReport_Update(
    IN p_id INT,
    IN p_report_type VARCHAR(50),
    IN p_schedule VARCHAR(100),
    IN p_email_recipients TEXT,
    IN p_next_run_date DATETIME
)
BEGIN
    UPDATE reporting_scheduled_reports
    SET report_type = p_report_type,
        schedule = p_schedule,
        email_recipients = p_email_recipients,
        next_run_date = p_next_run_date,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;

-- =============================================
-- Stored Procedure: sp_ScheduledReport_GetActive
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Active Scheduled Reports
-- Spec compatibility wrapper.
-- =============================================
DROP PROCEDURE IF EXISTS sp_ScheduledReport_GetActive$$
CREATE PROCEDURE sp_ScheduledReport_GetActive()
BEGIN
    SELECT
        id,
        report_type,
        schedule,
        email_recipients,
        next_run_date,
        last_run_date
    FROM reporting_scheduled_reports
    WHERE is_active = TRUE
      AND next_run_date IS NOT NULL
    ORDER BY next_run_date ASC;
END$$



DELIMITER ;

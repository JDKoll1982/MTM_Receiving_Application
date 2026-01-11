-- =============================================
-- Stored Procedure: sp_Settings_ScheduledReport_GetDue
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Scheduled Reports Due for Execution
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_ScheduledReport_GetDue`$$
CREATE PROCEDURE `sp_Settings_ScheduledReport_GetDue`()
BEGIN
    SELECT
        id,
        report_type,
        schedule,
        email_recipients,
        next_run_date,
        last_run_date
    FROM reporting_scheduled_reports
    WHERE is_active = 1
      AND next_run_date IS NOT NULL
      AND next_run_date <= NOW()
    ORDER BY next_run_date ASC;
END$$



DELIMITER ;

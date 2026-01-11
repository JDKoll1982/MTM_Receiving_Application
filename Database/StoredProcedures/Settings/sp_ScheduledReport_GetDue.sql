-- =============================================
-- Stored Procedure: sp_ScheduledReport_GetDue
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Scheduled Reports Due for Execution
-- =============================================
DROP PROCEDURE IF EXISTS sp_ScheduledReport_GetDue$$
CREATE PROCEDURE sp_ScheduledReport_GetDue()
BEGIN
    SELECT 
        id,
        report_type,
        schedule,
        email_recipients,
        next_run_date,
        last_run_date
    FROM scheduled_reports
    WHERE is_active = TRUE
      AND next_run_date IS NOT NULL
      AND next_run_date <= NOW()
    ORDER BY next_run_date ASC;
END$$



DELIMITER ;

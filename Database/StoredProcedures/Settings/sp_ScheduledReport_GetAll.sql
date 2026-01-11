-- =============================================
-- Stored Procedure: sp_ScheduledReport_GetAll
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All Scheduled Reports
-- =============================================
DROP PROCEDURE IF EXISTS sp_ScheduledReport_GetAll$$
CREATE PROCEDURE sp_ScheduledReport_GetAll()
BEGIN
    SELECT 
        id,
        report_type,
        schedule,
        email_recipients,
        is_active,
        next_run_date,
        last_run_date,
        created_at,
        updated_at,
        created_by
    FROM scheduled_reports
    WHERE is_active = TRUE
    ORDER BY next_run_date ASC, report_type;
END$$



DELIMITER ;

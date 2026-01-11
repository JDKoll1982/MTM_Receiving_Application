-- =============================================
-- Stored Procedure: sp_ScheduledReport_Insert
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Insert Scheduled Report
-- =============================================
DROP PROCEDURE IF EXISTS sp_ScheduledReport_Insert$$
CREATE PROCEDURE sp_ScheduledReport_Insert(
    IN p_report_type VARCHAR(50),
    IN p_schedule VARCHAR(100),
    IN p_email_recipients TEXT,
    IN p_next_run_date DATETIME,
    IN p_created_by INT
)
BEGIN
    INSERT INTO reporting_scheduled_reports (
        report_type,
        schedule,
        email_recipients,
        is_active,
        next_run_date,
        created_by
    ) VALUES (
        p_report_type,
        p_schedule,
        p_email_recipients,
        TRUE,
        p_next_run_date,
        p_created_by
    );

    SELECT LAST_INSERT_ID() AS id;
END$$



DELIMITER ;
